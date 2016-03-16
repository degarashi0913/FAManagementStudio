﻿using FAManagementStudio.Common;
using FAManagementStudio.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FAManagementStudio.ViewModels
{
    public class MainViewModel : BindableBase
    {
        public MainViewModel()
        {
            SetCommand();
        }
        private DbViewModel _db = new DbViewModel();
        private QueryInfo _queryInf = new QueryInfo();
        public string InputPath { get; set; }

        public ObservableCollection<QueryTabViewModel> Queries { get; } = new ObservableCollection<QueryTabViewModel> { new QueryTabViewModel("Query1", ""), QueryTabViewModel.GetNewInstance() };
        public int TagSelectedIndex { get; set; } = 0;
        public QueryTabViewModel TagSelectedValue { get; set; }

        public List<TableViewModel> Tables
        {
            get
            {
                return CurrentDatabase.Tables;
            }
        }

        public List<TriggerViewModel> Triggers { get { return _db.Triggers; } }

        public DbViewModel CurrentDatabase
        {
            get
            {
                return _db;
            }
            set
            {
                _db = value;
                RaisePropertyChanged(nameof(Tables));
                RaisePropertyChanged(nameof(CurrentDatabase));
                RaisePropertyChanged(nameof(Triggers));
            }
        }

        public object SelectedTableItem { get; set; }

        public ObservableCollection<DbViewModel> Databases { get; set; } = new ObservableCollection<DbViewModel>();

        public ObservableCollection<DataTable> Datasource { get; set; } = new ObservableCollection<DataTable>();

        #region CommandBind
        public ICommand CreateDatabase { get; private set; }
        public ICommand LoadDatabase { get; private set; }
        public ICommand ExecuteQuery { get; private set; }

        public ICommand DropFile { get; private set; }
        public ICommand DbListDropFile { get; private set; }

        public ICommand SetSqlTemplate { get; private set; }
        public ICommand ReloadDatabase { get; private set; }
        public ICommand ShutdownDatabase { get; private set; }
        public ICommand AddTab { get; private set; }
        public ICommand DeleteTabItem { get; private set; }

        public ICommand LoadHistry { get; private set; }
        public ICommand SaveHistry { get; private set; }

        private PathHistoryRepository _history = new PathHistoryRepository();
        public ObservableCollection<string> DataInput { get { return _history.History; } }

        public void SetCommand()
        {
            CreateDatabase = new RelayCommand(() =>
            {
                if (string.IsNullOrEmpty(this.InputPath)) return;
                if (File.Exists(this.InputPath)) return;

                var db = new DbViewModel();
                db.CreateDatabase(this.InputPath);
                db.LoadDatabase(this.InputPath);
                Databases.Add(db);
                _history.DataAdd(this.InputPath);
            });

            LoadDatabase = new RelayCommand(() =>
            {
                if (string.IsNullOrEmpty(this.InputPath)) return;
                if (!File.Exists(this.InputPath)) return;

                var db = new DbViewModel();
                db.LoadDatabase(this.InputPath);
                Databases.Add(db);
                _history.DataAdd(this.InputPath);
            });

            ExecuteQuery = new RelayCommand(async () =>
           {
               if (!CurrentDatabase.CanExecute()) return;
               Datasource.Clear();
               await TaskEx.Run(() =>
               {
                   _queryInf.ExecuteQuery(CurrentDatabase.ConnectionString, TagSelectedValue.Query);
                   Application.Current.Dispatcher.Invoke(new Action(() =>
                   {
                       foreach (var table in _queryInf.Result)
                       {
                           Datasource.Add(table);
                       }
                   }));
               });

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

            SetSqlTemplate = new RelayCommand<string>((string sqlKind) =>
            {
                TagSelectedValue.Query = CreateSqlSentence(SelectedTableItem, sqlKind);
                RaisePropertyChanged(nameof(Queries));
            });

            ReloadDatabase = new RelayCommand(() => { CurrentDatabase.ReloadDatabase(); });

            ShutdownDatabase = new RelayCommand(() => { Databases.Remove(CurrentDatabase); });

            AddTab = new RelayCommand(() =>
            {
                TagSelectedValue.Header = $"Query{Queries.Count}";
                Queries.Add(QueryTabViewModel.GetNewInstance());
                RaisePropertyChanged(nameof(Queries));
            });

            DeleteTabItem = new RelayCommand(() =>
            {
                var item = TagSelectedValue;
                var idx = Queries.IndexOf(item);
                TagSelectedIndex = idx - 1;
                RaisePropertyChanged(nameof(TagSelectedIndex));
                Queries.Remove(item);
            });

            LoadHistry = new RelayCommand(() => _history.LoadData(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)));

            SaveHistry = new RelayCommand(() => _history.SaveData(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)));
        }

        private string CreateSqlSentence(object treeitem, string sqlKind)
        {
            var table = treeitem as TableViewModel;
            string[] colums;
            string tableName;
            if (table == null)
            {
                var col = treeitem as ColumViewMoodel;
                tableName = Tables.Where(x => 0 < x.ChildItems.Count(c => c == col)).First().TableName;
                colums = new[] { col.ColumName };
            }
            else {
                colums = table.ChildItems.Select(x => x.ColumName).ToArray();
                tableName = table.TableName;
            }
            if (sqlKind == "select")
            {
                return CreateSelectStatement(tableName, colums);
            }
            else if (sqlKind == "insert")
            {
                return CreateInsertStatement(tableName, colums);
            }
            else if (sqlKind == "update")
            {
                return CreateUpdateStatement(tableName, colums);
            }
            else {
                return "";
            }
        }

        #endregion

        private string CreateSelectStatement(string tableName, string[] colums)
        {
            return $"select {string.Join(", ", colums)} from {tableName}";
        }

        private string CreateInsertStatement(string tableName, string[] colums)
        {
            var valuesStr = string.Join(", ", colums.Select(x => $"@{x.ToLower()}").ToArray());
            return $"insert into {tableName} ({string.Join(", ", colums)}) values ({valuesStr})";
        }

        private string CreateUpdateStatement(string tableName, string[] colums)
        {
            var setStr = string.Join(", ", colums.Select(x => $"{x} = @{x.ToLower()}").ToArray());
            return $"update {tableName} set {setStr}";
        }
    }
}
