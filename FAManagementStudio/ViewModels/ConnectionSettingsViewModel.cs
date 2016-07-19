using FAManagementStudio.Common;
using FAManagementStudio.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Input;

namespace FAManagementStudio.ViewModels
{
    public class ConnectionSettingsViewModel : ViewModelBase
    {
        public ConnectionSettingsViewModel() { }
        public ConnectionSettingsViewModel(DatabaseInfo inf)
        {
            _inf = inf;
            Init();
            OkCommand = new RelayCommand(() =>
            {
                var properties = _inf.Builder.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                foreach (var item in NotInputList.Where(x => x.Value != null).Union(InputedList))
                {
                    _inf.Builder[_synonyms[item.Name]] = item.Value;
                }
                MessengerInstance.Send(new MessageBase(this, "WindowClose"));
            });
        }
        private DatabaseInfo _inf;

        private void Init()
        {
            var properties = _inf.Builder.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(x => x.Name != "ContextConnection")
                .Select(x =>
                {
                    object value = null;
                    _inf.Builder.TryGetValue(_synonyms[x.Name], out value);
                    return new ConnectionSettingsDispModel(x.Name, x.PropertyType, value);
                })
                .OrderBy(x => x.Name);
            InputedList = properties.Where(x => x.Value != null).ToList();
            NotInputList = properties.Where(x => x.Value == null).ToList();
        }
        public List<ConnectionSettingsDispModel> InputedList { get; private set; }
        public List<ConnectionSettingsDispModel> NotInputList { get; private set; }

        public ICommand OkCommand { get; private set; }

        private readonly Dictionary<string, string> _synonyms = new Dictionary<string, string>
        {
            {"UserID","user id"},
            {"Password","password"},
            {"DataSource","data source"},
            {"Database","initial catalog"},
            {"Port","port number"},
            {"PacketSize","packet size"},
            {"Role","role name"},
            {"Dialect","dialect"},
            {"Charset","character set"},
            {"ConnectionTimeout","connection timeout"},
            {"Pooling","pooling"},
            {"ConnectionLifeTime","connection lifetime"},
            {"MinPoolSize","min pool size"},
            {"MaxPoolSize","max pool size"},
            {"FetchSize","fetch size"},
            {"ServerType","server type"},
            {"IsolationLevel","isolation level"},
            {"ReturnRecordsAffected","records affected"},
            {"Enlist","enlist"},
            {"ClientLibrary","client library"},
            {"DbCachePages","cache pages"},
            {"NoDatabaseTriggers","no db triggers"},
            {"NoGarbageCollect","no garbage collect"},
            {"ContextConnection"," contextconnection" }
        };
    }

    public class ConnectionSettingsDispModel
    {
        public string Name { get; set; }
        public Type PropertyType { get; set; }
        public object Value { get; set; }

        public ConnectionSettingsDispModel(string name, Type type, object obj)
        {
            Name = name;
            PropertyType = type;
            Value = obj;
        }
    }
}
