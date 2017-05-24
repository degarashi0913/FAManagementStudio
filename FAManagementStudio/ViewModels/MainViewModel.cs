using FAManagementStudio.Common;
using FAManagementStudio.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FAManagementStudio.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            SetCommand();
#if !DEBUG
            SetNewVersionStatus();
#endif
            SetQueryProject();
        }
        public string InputPath { get; set; }

        public ObservableCollection<QueryTabViewModel> Queries { get; } = new ObservableCollection<QueryTabViewModel> { new QueryTabViewModel("Query1", ""), QueryTabViewModel.GetNewInstance() };
        public int TagSelectedIndex { get; set; } = 0;
        public QueryTabViewModel TagSelectedValue { get; set; }
        private DbViewModel _currentDatabase;
        public DbViewModel CurrentDatabase
        {
            get { return _currentDatabase; }
            set
            {
                _currentDatabase = value;
                RaisePropertyChanged(nameof(CurrentDatabase));
            }
        }

        private Visibility _existNewVersion = Visibility.Collapsed;
        public Visibility ExistNewVersion
        {
            get
            {
                return _existNewVersion;
            }
            set
            {
                _existNewVersion = value;
                RaisePropertyChanged(nameof(ExistNewVersion));
            }
        }

        public object SelectedTableItem { get; set; }

        public ObservableCollection<DbViewModel> Databases { get; } = new ObservableCollection<DbViewModel>();

        public ObservableCollection<QueryResultViewModel> Datasource { get; } = new ObservableCollection<QueryResultViewModel> { new QueryResultViewModel("Result") };
        public int SelectedResultIndex { get; set; } = 0;

        #region CommandBind
        public ICommand CreateDatabase { get; private set; }
        public ICommand LoadDatabase { get; private set; }
        public ICommand ExecuteQuery { get; private set; }

        public ICommand DropFile { get; private set; }
        public ICommand DbListDropFile { get; private set; }

        public ICommand SetSqlTemplate { get; private set; }
        public ICommand ExecSqlTemplate { get; private set; }
        public ICommand SetSqlDataTemplate { get; private set; }
        public ICommand ExecLimitedSql { get; private set; }
        public ICommand ShutdownDatabase { get; private set; }
        public ICommand ChangeConfig { get; private set; }
        public ICommand ShowEntity { get; private set; }
        public ICommand AddTab { get; private set; }
        public ICommand DeleteTabItem { get; private set; }

        public ICommand LoadHistry { get; private set; }
        public ICommand SaveHistry { get; private set; }
        public ICommand OpenGitPage { get; private set; }

        public ICommand ReleasePinCommand { get; private set; }

        public ICommand ShowPathSettings { get; private set; }

        public ICommand ProjectItemOpen { get; private set; }
        public ICommand ProjectItemDrop { get; private set; }

        public ICommand PinedCommand { get; private set; }

        private PathHistoryRepository _history = new PathHistoryRepository();
        public ObservableCollection<string> DataInput { get { return _history.History; } }

        public void SetCommand()
        {
            CreateDatabase = new RelayCommand(async () =>
            {
                var vm = new NewDatabaseSettingsViewModel();
                MessengerInstance.Send(new MessageBase(vm, "NewDbSettingsWindowOpen"));

                if (string.IsNullOrEmpty(vm.Path)) return;

                var db = new DbViewModel();
                await db.CreateDatabase(vm.Path, vm.Type, vm.CharSet);
                Databases.Add(db);
                _history.DataAdd(vm.Path);
            });

            LoadDatabase = new RelayCommand<string>(async (path) =>
            {
                if (string.IsNullOrEmpty(path)) return;
                if (!File.Exists(path)) return;

                var dbInf = new DatabaseInfo(new FirebirdInfo(path));
                if (!dbInf.CanLoadDatabase) return;

                var db = new DbViewModel();
                Databases.Add(db);
                await db.LoadDatabase(dbInf);
                _history.DataAdd(path);
            });

            ExecuteQuery = new RelayCommand(async () =>
           {
               if (CurrentDatabase == null || !CurrentDatabase.CanExecute()) return;
               if (TagSelectedValue.IsNewResult && 0 < Datasource[0].Result.Count) Datasource[0].Pined = true;
               var QueryResult = Datasource[0];
               QueryResult.Result.Clear();
               await Task.Run(() =>
               {
                   var inf = new QueryInfo { ShowExecutePlan = TagSelectedValue.IsShowExecutionPlan };
                   QueryResult.GetExecuteResult(inf, CurrentDatabase.ConnectionString, TagSelectedValue.Query);
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

            SetSqlTemplate = new RelayCommand<SqlKind>((SqlKind sqlKind) =>
            {
                TagSelectedValue.Query = CreateSqlSentence(SelectedTableItem, sqlKind);
                RaisePropertyChanged(nameof(Queries));
            });

            ExecLimitedSql = new RelayCommand<string>((count) =>
            {
                TagSelectedValue.Query = CreateSqlSentence(SelectedTableItem, SqlKind.Select, int.Parse(count));
                RaisePropertyChanged(nameof(Queries));
                ExecuteQuery.Execute(null);
            });

            ExecSqlTemplate = new RelayCommand<SqlKind>((SqlKind sqlKind) =>
            {
                SetSqlTemplate.Execute(sqlKind);
                ExecuteQuery.Execute(null);
            });

            ShutdownDatabase = new RelayCommand(() => { Databases.Remove(CurrentDatabase); });

            ChangeConfig = new RelayCommand(() =>
            {
                if (CurrentDatabase == null) return;
                var vm = new ConnectionSettingsViewModel(CurrentDatabase.DbInfo);
                MessengerInstance.Send(new MessageBase(vm, "WindowOpen"));
            });

            ShowEntity = new RelayCommand(() =>
            {
                if (CurrentDatabase == null) return;
                var vm = new EntityRelationshipViewModel(CurrentDatabase);
                MessengerInstance.Send(new MessageBase(vm, "EintityWindowOpen"));
            });

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
            }, () => 0 < Datasource[0].Result.Count);

            ReleasePinCommand = new RelayCommand<QueryResultViewModel>((data) =>
            {
                Datasource.Remove(data);
            });

            SetSqlDataTemplate = new RelayCommand<string>((s) =>
            {
                var table = GetTreeViewTableName(CurrentDatabase, SelectedTableItem);
                var result = "";
                if (s == "table")
                {
                    result = table.GetDdl(CurrentDatabase);
                }
                else if (s == "insert")
                {
                    if (table is TableViewViewModel) return;
                    var colums = table.Colums.Select(x => x.ColumName).ToArray();
                    var escapedColumsStr = string.Join(", ", colums.Select(x => EscapeKeyWord(x)).ToArray());

                    var insertTemplate = $"insert into {table.TableName} ({escapedColumsStr})";

                    var qry = new QueryInfo();
                    var res = qry.ExecuteQuery(CurrentDatabase.ConnectionString, CreateSelectStatement(table.TableName, colums, 0)).First();

                    var sb = new StringBuilder();

                    foreach (DataRow row in res.View.Rows)
                    {
                        sb.Append(insertTemplate + " values(");
                        sb.Append(string.Join(", ", row.ItemArray
                                                        .Select(x =>
                                                                    {
                                                                        var type = x.GetType();
                                                                        if (type == typeof(DBNull)) return "Null";
                                                                        if (type == typeof(string)) return $"\'{x}\'";
                                                                        return $"{x}";
                                                                    })
                                                        .ToArray()));
                        sb.AppendLine(");");
                    }
                    result = sb.ToString();
                }

                var idx = Queries.IndexOf(TagSelectedValue);
                Queries[idx].Query = result;
                RaisePropertyChanged(nameof(Queries));
            });

            ShowPathSettings = new RelayCommand(() =>
            {
                var vm = new BasePathSettingsViewModel(QueryProjects);
                MessengerInstance.Send(new MessageBase(vm, "BasePathSettingsWindowOpen"));
            });

            ProjectItemOpen = new RelayCommand<object>((obj) =>
            {
                var item = obj as QueryProjectFileViewModel;
                if (item == null) return;
                var idx = Queries.IndexOf(TagSelectedValue);
                try
                {
                    Queries[idx].Header = Path.GetFileNameWithoutExtension(item.FullPath);
                    Queries[idx].Query = Queries[idx].FileLoad(item.FullPath, Encoding.UTF8);
                }
                catch (IOException) { }
            });
            ProjectItemDrop = new RelayCommand<string>((string path) =>
            {
                QueryProjects.Add(QueryProjectViewModel.GetData(path).First());
                AppSettingsManager.QueryProjectBasePaths.Add(path);
            });
        }

        private ITableViewModel GetTreeViewTableName(DbViewModel db, object treeitem)
        {
            var table = treeitem as ITableViewModel;

            if (table == null)
            {

                return db.Tables.Where(x => 0 < x.Colums.Count(c => c == (ColumViewMoodel)treeitem)).First();
            }
            return table;
        }

        private string CreateSqlSentence(object treeitem, SqlKind sqlKind, int limitCount = 0)
        {
            string[] colums;
            var col = treeitem as ColumViewMoodel;
            var table = GetTreeViewTableName(CurrentDatabase, treeitem);

            if (col == null)
            {
                colums = table.Colums.Select(x => x.ColumName).ToArray();
            }
            else
            {
                colums = new[] { col.ColumName };
            }

            if (sqlKind == SqlKind.Select)
            {
                return CreateSelectStatement(table.TableName, colums, limitCount);
            }
            else if (sqlKind == SqlKind.Insert)
            {
                return CreateInsertStatement(table.TableName, colums);
            }
            else if (sqlKind == SqlKind.Update)
            {
                return CreateUpdateStatement(table.TableName, colums);
            }
            else
            {
                return "";
            }
        }

        #endregion

        private string CreateSelectStatement(string tableName, string[] colums, int topCount)
        {
            var escapedColumsStr = string.Join(", ", colums.Select(x => EscapeKeyWord(x)).ToArray());
            var topSentence = 0 < topCount ? $" first({topCount})" : "";
            return $"select{topSentence} {escapedColumsStr} from {tableName}";
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

        private async void SetNewVersionStatus()
        {
            try
            {
                var latestVersion = "";
                var version = Assembly.GetExecutingAssembly().GetName().Version;
                var versionStr = $"{version.Major}.{version.Minor}.{version.Build}";
                if ((AppSettingsManager.StartTime - AppSettingsManager.PreviousActivation).Days < 1)
                {
                    latestVersion = string.IsNullOrEmpty(AppSettingsManager.Version) ? versionStr : AppSettingsManager.Version;
                }
                else
                {
                    latestVersion = await GetNewVirsion();
                    AppSettingsManager.Version = latestVersion;
                }

                if (latestVersion != versionStr)
                {
                    ExistNewVersion = Visibility.Visible;
                };
            }
            catch { }
        }

        private Task<string> GetNewVirsion()
        {
            return Task.Run<string>(() =>
            {
                var reqest = (HttpWebRequest)WebRequest.Create(@"https://github.com/degarashi0913/FAManagementStudio/releases/latest");
                reqest.UserAgent = "FAManagementStudio";
                reqest.Method = "GET";
                using (var stream = reqest.GetResponse().GetResponseStream())
                using (var readStream = new StreamReader(stream, Encoding.UTF8))
                {
                    var html = readStream.ReadToEnd();
                    var title = Regex.Match(html, @"\<title\>(?<title>.*)\<\/title\>").Groups["title"].Value;
                    return Regex.Match(title, @"FAManagementStudio-v(?<version>\d*\.\d*\.\d*)").Groups["version"].Value;
                }
            });
        }

        public ObservableCollection<IProjectNodeViewModel> QueryProjects { get; } = new ObservableCollection<IProjectNodeViewModel>();

        private async void SetQueryProject()
        {
            await Task.Run(() =>
            {
                foreach (var pItem in QueryProjectViewModel.GetData(AppSettingsManager.QueryProjectBasePaths.ToArray()))
                {
                    Application.Current.Dispatcher.Invoke(new Action(() => QueryProjects.Add(pItem)));
                }
            });
        }

        public FirebirdRecommender Recommender { get; } = new FirebirdRecommender();
    }
}
