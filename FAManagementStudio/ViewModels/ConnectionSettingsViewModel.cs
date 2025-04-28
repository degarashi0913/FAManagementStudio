using FAManagementStudio.Common;
using FAManagementStudio.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Windows.Input;

namespace FAManagementStudio.ViewModels
{
    public class ConnectionSettingsViewModel : ViewModelBase
    {
        /// <summary>
        /// Default constructor for design time data.
        /// </summary>
#pragma warning disable CS8618 
        public ConnectionSettingsViewModel()
#pragma warning restore CS8618 
        {
        }

        public ConnectionSettingsViewModel(DatabaseInfo inf)
        {
            _inf = inf;
            Init();
            OkCommand = new RelayCommand(() =>
            {
                var properties = _inf.Builder.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                foreach (var item in NotInputList.Where(x => x.Value != null).Union(InputtedList))
                {
                    _inf.Builder[_synonyms[item.Name]] = item.Value;
                }
                MessengerInstance.Send(new MessageBase(this, "WindowClose"));
            });
        }

        private readonly DatabaseInfo _inf;

        [MemberNotNull(nameof(InputtedList), nameof(NotInputList))]
        private void Init()
        {
            var properties = _inf.Builder.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(x => _synonyms.ContainsKey(x.Name) && x.Name != "ContextConnection")
                .Select(x =>
                {
                    object? value = null;
                    _inf.Builder.TryGetValue(_synonyms[x.Name], out value);
                    return new ConnectionSettingsDisplayModel(x.Name, x.PropertyType, value);
                })
                .OrderBy(x => x.Name)
                .ToLookup(x => x.Value is not null);

            InputtedList = [.. properties[true]];
            NotInputList = [.. properties[false]];
        }

        public List<ConnectionSettingsDisplayModel> InputtedList { get; private set; }


        public List<ConnectionSettingsDisplayModel> NotInputList { get; private set; }

        public ICommand OkCommand { get; private set; }

        private readonly Dictionary<string, string> _synonyms = new()
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
            {"ContextConnection"," contextconnection" },
            {"Compression","Compression" }
        };
    }

    public record ConnectionSettingsDisplayModel(string Name, Type PropertyType, object? Value);
}
