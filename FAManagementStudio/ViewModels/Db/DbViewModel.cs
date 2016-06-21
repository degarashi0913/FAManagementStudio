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

        public string ConnectionString { get { return _dbInfo.ConnectionString; } }
        public string Path { get { return _dbInfo.Path; } }

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
            }
            return true;
        }

        public void ReloadDatabase()
        {
            Tables.Clear();
            Triggers.Clear();
            var path = _dbInfo.Path;
            _dbInfo = new DatabaseInfo();
            LoadDatabase(path);
            CollectionViewSource.GetDefaultView(this.Tables).Refresh();
        }
    }
}