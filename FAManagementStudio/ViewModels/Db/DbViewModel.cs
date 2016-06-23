using FAManagementStudio.Common;
using FAManagementStudio.Models;
using FAManagementStudio.ViewModels;
using FirebirdSql.Data.FirebirdClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;

namespace FAManagementStudio.ViewModels
{
    public class DbViewModel : ViewModelBase
    {
        public DbViewModel()
        {
        }

        private DatabaseInfo _dbInfo = new DatabaseInfo();
        public DatabaseInfo DbInfo { get { return _dbInfo; } }

        public string DisplayDbName { get { return _dbInfo.Path.Substring(_dbInfo.Path.LastIndexOf('\\') + 1); } }

        public List<ITableViewModel> Tables { get; } = new List<ITableViewModel>();
        public List<TriggerViewModel> Triggers { get; } = new List<TriggerViewModel>();
        public List<IndexViewModel> Indexes { get; } = new List<IndexViewModel>();
        private List<DomainViewModel> _domains;
        public List<DomainViewModel> Domains
        {
            get
            {
                if (_domains == null) _domains = GetDomains();
                return _domains;
            }
        }

        private List<DomainViewModel> GetDomains()
        {
            var list = new List<DomainViewModel>();
            using (var con = new FbConnection(_dbInfo.ConnectionString))
            {
                con.Open();
                foreach (var item in _dbInfo.GetDomain(con))
                {
                    list.Add(new DomainViewModel(item));
                }
            }
            return list;
        }

        public string ConnectionString { get { return _dbInfo.ConnectionString; } }
        public string Path { get { return _dbInfo.Path; } }

        private AdditionalDbInfoControl _additionalInfo;
        public AdditionalDbInfoControl AdditionalInfo
        {
            get
            {
                if (_additionalInfo == null) _additionalInfo = new AdditionalDbInfoControl(this);
                return _additionalInfo;
            }
        }

        public bool CanExecute()
        {
            return !string.IsNullOrEmpty(_dbInfo.ConnectionString);
        }

        public void CreateDatabase(string path)
        {
            _dbInfo.Path = path;
            using (var con = new FbConnection(_dbInfo.ConnectionString))
            {
                _dbInfo.CreateDatabase(con);
            }
            LoadDatabase(path);
        }

        private bool CanLoadDatabase(string path)
        {
            var info = new FirebirdInfo();
            return info.IsTargetOdsDb(path);
        }

        public bool LoadDatabase(string path)
        {
            if (!CanLoadDatabase(path)) return false;
            _dbInfo.Path = path;
            using (var con = new FbConnection(_dbInfo.ConnectionString))
            {
                con.Open();
                foreach (var item in _dbInfo.GetTables(con))
                {
                    var vm = new TableViewModel(item.TableName);
                    foreach (var colums in item.GetColums(con))
                    {
                        vm.Colums.Add(new ColumViewMoodel(colums));
                    }
                    foreach (var trigger in item.GetTrigger(con))
                    {
                        vm.Triggers.Add(new TriggerViewModel(trigger));
                    }
                    foreach (var idx in item.GetIndex(con))
                    {
                        vm.Indexs.Add(new IndexViewModel(idx));
                    }
                    Tables.Add(vm);
                }
                Triggers.AddRange(Tables.SelectMany(x => ((TableViewModel)x).Triggers).ToArray());
                Indexes.AddRange(Tables.SelectMany(x => ((TableViewModel)x).Indexs).ToArray());
                foreach (var item in _dbInfo.GetViews(con))
                {
                    var vm = new TableViewViewModel(item.ViewName, item.Source);
                    foreach (var colums in item.GetColums(con))
                    {
                        vm.Colums.Add(new ColumViewMoodel(colums));
                    }
                    Tables.Add(vm);
                }
                RaisePropertyChanged(nameof(Tables));
                RaisePropertyChanged(nameof(Triggers));
                RaisePropertyChanged(nameof(Indexes));
            }
            return true;
        }

        public void ReloadDatabase()
        {
            Tables.Clear();
            Triggers.Clear();
            Indexes.Clear();
            _domains = null;
            var path = _dbInfo.Path;
            _dbInfo = new DatabaseInfo();
            LoadDatabase(path);
            _additionalInfo.RefrechData(this);
            CollectionViewSource.GetDefaultView(this.Tables).Refresh();
        }
    }
}