using FAManagementStudio.Common;
using FAManagementStudio.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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

        public ObservableCollection<QueryResultViewModel> Datasource { get; } = new ObservableCollection<QueryResultViewModel> { new QueryResultViewModel("Result") };
        public int SelectedResultIndex { get; set; } = 0;

        #region CommandBind
        public ICommand CreateDatabase { get; private set; }
        public ICommand LoadDatabase { get; private set; }
        public ICommand ExecuteQuery { get; private set; }

        public ICommand DropFile { get; private set; }
        public ICommand DbListDropFile { get; private set; }

        public ICommand SetSqlTemplate { get; private set; }
        public ICommand SetSqlDataTemplate { get; private set; }
        public ICommand ReloadDatabase { get; private set; }
        public ICommand ShutdownDatabase { get; private set; }
        public ICommand AddTab { get; private set; }
        public ICommand DeleteTabItem { get; private set; }

        public ICommand LoadHistry { get; private set; }
        public ICommand SaveHistry { get; private set; }
        public ICommand OpenGitPage { get; private set; }

        public ICommand PinCommand { get; private set; }

        public ICommand PinedCommand { get; private set; }

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

            LoadDatabase = new RelayCommand<string>(async (path) =>
            {
                if (string.IsNullOrEmpty(path)) return;
                if (!File.Exists(path)) return;
                var db = new DbViewModel();
                await TaskEx.Run(() =>
                {
                    db.LoadDatabase(path);
                });
                Databases.Add(db);
                _history.DataAdd(path);
            });

            ExecuteQuery = new RelayCommand(async () =>
           {
               if (CurrentDatabase == null || !CurrentDatabase.CanExecute()) return;
               var QueryResult = Datasource[0];
               QueryResult.Result.Clear();
               await TaskEx.Run(() =>
               {
                   QueryResult.GetExecuteResult(_queryInf, CurrentDatabase.ConnectionString, TagSelectedValue.Query);
               });

           });

            DropFile = new RelayCommand<string>((string path) =>
            {
                InputPath = path;
                RaisePropertyChanged(nameof(InputPath));
            });

            DbListDropFile = new RelayCommand<string>((string path) =>
            {
                LoadDatabase.Execute(path);
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

            OpenGitPage = new RelayCommand(() => System.Diagnostics.Process.Start("https://github.com/degarashi0913/FAManagementStudio"));

            PinedCommand = new RelayCommand(() =>
            {
                Datasource[0].Header = $"Pin{Datasource.Count}";
                Datasource.Insert(0, new QueryResultViewModel("Result"));
                SelectedResultIndex = 0;
                RaisePropertyChanged(nameof(SelectedResultIndex));
            });

            PinCommand = new RelayCommand<QueryResultViewModel>((data) =>
            {
                Datasource.Remove(data);
            });

            SetSqlDataTemplate = new RelayCommand<string>((s) =>
            {
                var table = GetTreeViewTableName(SelectedTableItem);
                var result = "";
                if (s == "table")
                {
                    result = table.GetDdl(CurrentDatabase);
                }
                else if (s == "insert")
                {
                    var colums = table.Colums.Select(x => x.ColumName).ToArray();
                    var escapedColumsStr = string.Join(", ", colums.Select(x => EscapeKeyWord(x)).ToArray());

                    var insertTemplate = $"insert into {table.TableName} ({escapedColumsStr})";

                    var qry = new QueryInfo();
                    var res = qry.ExecuteQuery(CurrentDatabase.ConnectionString, CreateSelectStatement(table.TableName, colums)).First();

                    var sb = new StringBuilder();

                    foreach (DataRow row in res.View.Rows)
                    {
                        sb.Append(insertTemplate + " values(");
                        sb.Append(string.Join(", ", row.ItemArray.Select(x => x.GetType() == typeof(string) ? $"\'{x}\'" : $"{x}").ToArray()));
                        sb.AppendLine(");");
                    }
                    result = sb.ToString();
                }

                var idx = Queries.IndexOf(TagSelectedValue);
                Queries[idx].Query = result;
                RaisePropertyChanged(nameof(Queries));
            });
        }

        private TableViewModel GetTreeViewTableName(object treeitem)
        {
            var table = treeitem as TableViewModel;

            if (table == null)
            {
                return Tables.Where(x => 0 < x.Colums.Count(c => c == (ColumViewMoodel)treeitem)).First();
            }
            return table;
        }

        private string CreateSqlSentence(object treeitem, string sqlKind)
        {
            string[] colums;
            var col = treeitem as ColumViewMoodel;
            var table = GetTreeViewTableName(treeitem);

            if (col == null)
            {
                colums = table.Colums.Select(x => x.ColumName).ToArray();
            }
            else
            {
                colums = new[] { col.ColumName };
            }

            if (sqlKind == "select")
            {
                return CreateSelectStatement(table.TableName, colums);
            }
            else if (sqlKind == "insert")
            {
                return CreateInsertStatement(table.TableName, colums);
            }
            else if (sqlKind == "update")
            {
                return CreateUpdateStatement(table.TableName, colums);
            }
            else
            {
                return "";
            }
        }

        #endregion

        private string CreateSelectStatement(string tableName, string[] colums)
        {
            var escapedColumsStr = string.Join(", ", colums.Select(x => EscapeKeyWord(x)).ToArray());
            return $"select {escapedColumsStr} from {tableName}";
        }

        private string CreateInsertStatement(string tableName, string[] colums)
        {
            var escapedColumsStr = string.Join(", ", colums.Select(x => EscapeKeyWord(x)).ToArray());
            var valuesStr = string.Join(", ", colums.Select(x => $"@{x.ToLower()}").ToArray());
            return $"insert into {tableName} ({escapedColumsStr}) values ({valuesStr})";
        }

        private string CreateUpdateStatement(string tableName, string[] colums)
        {
            var setStr = string.Join(", ", colums.Select(x => $"{EscapeKeyWord(x)} = @{x.ToLower()}").ToArray());
            return $"update {tableName} set {setStr}";
        }

        private readonly HashSet<string> _sqlKeyWord = new HashSet<string> { "index" };
        private string EscapeKeyWord(string colum)
        {
            return _sqlKeyWord.Contains(colum.ToLower()) ? $"'{colum}'" : colum;
        }
    }
}
