using FAManagementStudio.Common;
using FAManagementStudio.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Input;

namespace FAManagementStudio.ViewModels
{
    class MainViewModel : BindableBase
    {
        public MainViewModel()
        {
            SetCommand();
        }
        private DatabaseInfo _dbInf = new DatabaseInfo();
        private QueryInfo _queryInf = new QueryInfo();
        public string InputPath { get; set; }

        public string Query { get; set; }

        public ObservableCollection<TableInfo> Tables
        {
            get
            {
                return CurrentDatabase.Chiled;
            }
        }

        public DatabaseInfo CurrentDatabase
        {
            get
            {
                return _dbInf;
            }
            set
            {
                _dbInf = value;
                RaisePropertyChanged(nameof(Tables));
                RaisePropertyChanged(nameof(CurrentDatabase));
                RaisePropertyChanged(nameof(Triggers));
            }
        }

        public object SelectedTableItem { get; set; }

        public ObservableCollection<DatabaseInfo> Databases { get; set; } = new ObservableCollection<DatabaseInfo>();

        public ObservableCollection<DataView> Datasource { get { return _queryInf.Result; } }

        public ObservableCollection<TriggerInfo> Triggers { get { return _dbInf.Trrigers; } }

        #region CommandBind
        public ICommand CreateDatabase { get; private set; }
        public ICommand LoadDatabase { get; private set; }
        public ICommand ExecuteQuery { get; private set; }

        public ICommand DropFile { get; private set; }
        public ICommand DbListDropFile { get; private set; }

        public ICommand SetSelectSql { get; private set; }
        public ICommand ReloadDatabase { get; private set; }
        public ICommand ShutdownDatabase { get; private set; }

        private PathHistoryRepository _history = new PathHistoryRepository();
        public ObservableCollection<string> DataInput { get { return _history.History; } }

        public void SetCommand()
        {
            CreateDatabase = new RelayCommand(() =>
            {
                if (string.IsNullOrEmpty(this.InputPath)) return;
                if (File.Exists(this.InputPath)) return;

                var db = new DatabaseInfo();
                db.CreateDatabase(this.InputPath);
                db.LoadDatabase(this.InputPath);
                Databases.Add(db);
                _history.DataAdd(this.InputPath);
            });
            LoadDatabase = new RelayCommand(() =>
            {
                if (string.IsNullOrEmpty(this.InputPath)) return;
                if (!File.Exists(this.InputPath)) return;

                var db = new DatabaseInfo();
                db.LoadDatabase(this.InputPath);
                Databases.Add(db);
                _history.DataAdd(this.InputPath);
            });
            ExecuteQuery = new RelayCommand(() =>
            {
                if (string.IsNullOrEmpty(CurrentDatabase.ConnectionString)) return;
                _queryInf.ExecuteQuery(CurrentDatabase.ConnectionString, Query);
                RaisePropertyChanged(nameof(Datasource));
            });

            DropFile = new RelayCommand<string>((string path) =>
            {
                InputPath = path;
                RaisePropertyChanged(nameof(InputPath));
            });

            DbListDropFile = new RelayCommand<string>((string path) =>
            {
                InputPath = path;
                LoadDatabase.Execute(null);
            });

            SetSelectSql = new RelayCommand(() =>
            {
                Query = CreateSelectSentence(SelectedTableItem);
                RaisePropertyChanged(nameof(Query));
            });

            ReloadDatabase = new RelayCommand(() =>
            {
                CurrentDatabase.LoadDatabase(CurrentDatabase.Path);
            });
            ShutdownDatabase = new RelayCommand(() =>
            {
                Databases.Remove(CurrentDatabase);
            });
        }
        #endregion
        private string CreateSelectSentence(object treeitem)
        {
            var table = treeitem as TableInfo;
            if (table == null)
            {
                var col = treeitem as ColumInfo;
                return CreateFromColumName(Tables.ToList(), col);
            }
            else {
                return CreateFromTableName(table);
            }
        }

        private string CreateFromTableName(TableInfo table)
        {
            var colums = string.Join(", ", table.Chiled.Select(x => x.ColumName).ToArray());
            return $"select {colums} from {table.TableName}";
        }

        private string CreateFromColumName(List<TableInfo> tables, ColumInfo targetCol)
        {
            var table = tables.Where(x => 0 < x.Chiled.Count(col => col == targetCol)).First();
            return $"select {targetCol.ColumName} from {table.TableName}";
        }

        ~MainViewModel()
        {
            _history.SaveData();
        }
    }
}
