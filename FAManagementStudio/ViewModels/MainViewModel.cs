using FAManagementStudio.Common;
using FAManagementStudio.Foundation.Common;
using FAManagementStudio.Models;
using FAManagementStudio.Models.Db;
using FAManagementStudio.ViewModels.Commons;
using FAManagementStudio.ViewModels.Db;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FAManagementStudio.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            QueryProjects = new(_queryProjects);

#if !DEBUG
            _ = SetNewVersionStatusAsync();
#endif
            SetQueryProject();
        }

        public string InputPath { get; set; } = string.Empty;

        public ObservableCollection<QueryTabViewModel> Queries { get; } = [new QueryTabViewModel("Query1", ""), QueryTabViewModel.GetNewInstance()];
        public int TagSelectedIndex { get; set; } = 0;
        public QueryTabViewModel TagSelectedValue { get; set; } = default!; // 初期化後に画面側からセットされる
        private DbViewModel? _currentDatabase;
        public DbViewModel? CurrentDatabase
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

        public object? SelectedTableItem { get; set; }

        public ObservableCollection<DbViewModel> Databases { get; } = [];

        public ObservableCollection<QueryResultViewModel> Datasource { get; } = [new("Result")];
        public int SelectedResultIndex { get; set; } = 0;

        #region CommandBind
        private ICommand? _createDatabase;
        public ICommand CreateDatabase => _createDatabase ??= new RelayCommand(async () => await OnCreateDatabaseAsync());
        private ICommand? _loadDatabase;
        public ICommand LoadDatabase => _loadDatabase ??= new RelayCommand<string>(async path => await OnLoadDatabaseAsync(path));
        private ICommand? _executeQuery;
        public ICommand ExecuteQuery => _executeQuery ??= new RelayCommand(async () => await OnExecuteQueryAsync());

        private ICommand? _dropFile;
        public ICommand DropFile => _dropFile ??= new RelayCommand<string>(OnDropFile);
        private ICommand? _dbListDropFile;
        public ICommand DbListDropFile => _dbListDropFile ??= new RelayCommand<string>(OnDbListDropFile);

        private ICommand? _openFilePathDialog;
        public ICommand OpenFilePathDialog => _openFilePathDialog ??= new RelayCommand(OnOpenFilePathDialog);

        private ICommand? _setSqlTemplate;
        public ICommand SetSqlTemplate => _setSqlTemplate ??= new RelayCommand<SqlKind>(OnSetSqlTemplate);
        private ICommand? _execSqlTemplate;
        public ICommand ExecSqlTemplate => _execSqlTemplate ??= new RelayCommand<SqlKind>(OnExecSqlTemplate);
        private ICommand? _setSqlDataTemplate;
        public ICommand SetSqlDataTemplate => _setSqlDataTemplate ??= new RelayCommand<string>(OnSetSqlDataTemplate);
        private ICommand? _execLimitedSql;
        public ICommand ExecLimitedSql => _execLimitedSql ??= new RelayCommand<string>(OnExecLimitedSql);
        private ICommand? _shutdownDatabase;
        public ICommand ShutdownDatabase => _shutdownDatabase ??= new RelayCommand(OnShutdownDatabase);
        private ICommand? _changeConfig;
        public ICommand ChangeConfig => _changeConfig ??= new RelayCommand(OnChangeConfig);
        private ICommand? _showEntity;
        public ICommand ShowEntity => _showEntity ??= new RelayCommand(OnShowEntity);
        private ICommand? _addCommand;
        public ICommand AddTab => _addCommand ??= new RelayCommand(OnAddTab);
        private ICommand? _deleteTabItem;
        public ICommand DeleteTabItem => _deleteTabItem ??= new RelayCommand(OnDeleteTabItem);

        private ICommand? _loadHistory;
        public ICommand LoadHistory => _loadHistory ??= new RelayCommand(OnLoadHistory);
        private ICommand? _saveHistory;
        public ICommand SaveHistory => _saveHistory ??= new RelayCommand(OnSaveHistory);
        private ICommand? _openGitPage;
        public ICommand OpenGitPage => _openGitPage ??= new RelayCommand(OnOpenGitPage);

        private ICommand? _pinedCommand;
        public ICommand PinedCommand => _pinedCommand ??= new RelayCommand(OnPinedCommand, CanExecutePinedCommand);
        private ICommand? _releasePinCommand;
        public ICommand ReleasePinCommand => _releasePinCommand ??= new RelayCommand<QueryResultViewModel>(OnReleasePinCommand);

        private ICommand? _showPathSettings;
        public ICommand ShowPathSettings => _showPathSettings ??= new RelayCommand(OnShowPathSettings);

        private ICommand? _reloadFileView;
        public ICommand ReloadFileView => _reloadFileView ??= new RelayCommand(OnReloadFileView);

        private ICommand? _projectItemOpen;
        public ICommand ProjectItemOpen => _projectItemOpen ??= new RelayCommand<object>(OnProjectItemOpen);
        private ICommand? _projectItemDrop;
        public ICommand ProjectItemDrop => _projectItemDrop ??= new RelayCommand<string>(OnProjectItemDrop);

        private readonly PathHistoryRepository _history = new();
        public ObservableCollection<string> DataInput => _history.History;

        private readonly QueryBuilder _queryBuilder = new();

        private async Task OnCreateDatabaseAsync()
        {
            var vm = new NewDatabaseSettingsViewModel();
            MessengerInstance.Send(new MessageBase(vm, "NewDbSettingsWindowOpen"));

            if (string.IsNullOrEmpty(vm.Path)) return;

            var db = DbViewModel.CreateDatabase(vm.Path, vm.Type, vm.CharSet);
            Databases.Add(db);
            await db.LoadDatabase();
            _history.DataAdd(vm.Path);
        }

        private async Task OnLoadDatabaseAsync(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            if (!File.Exists(path)) return;

            var dbInf = new DatabaseInfo(new FirebirdInfo(path));
            if (!dbInf.CanLoadDatabase) return;

            var db = new DbViewModel(dbInf);
            Databases.Add(db);
            await db.LoadDatabase();
            _history.DataAdd(path);
        }

        private async Task OnExecuteQueryAsync()
        {
            if (CurrentDatabase == null || !CurrentDatabase.CanExecute()) return;
            if (TagSelectedValue.IsNewResult && 0 < Datasource[0].Result.Count) Datasource[0].Pined = true;
            var QueryResult = Datasource[0];
            QueryResult.Result.Clear();
            await Task.Run(() =>
            {
                var inf = new QueryInfo(TagSelectedValue.IsShowExecutionPlan);
                QueryResult.GetExecuteResult(inf, CurrentDatabase.ConnectionString, TagSelectedValue.Query);
            });
        }

        private void OnDropFile(string path)
        {
            InputPath = path;
            RaisePropertyChanged(nameof(InputPath));
        }

        private void OnDbListDropFile(string path)
        {
            LoadDatabase.Execute(path);
        }

        private void OnOpenFilePathDialog()
        {
            var dialog = new OpenFileDialog
            {
                DefaultExt = "fdb",
                Filter = "すべてのファイル(*.*)|*.*"
            };
            if (dialog.ShowDialog() != true) return;
            if (!File.Exists(dialog.FileName)) return;
            LoadDatabase.Execute(dialog.FileName);
        }

        private void OnSetSqlTemplate(SqlKind sqlKind)
        {
            if (SelectedTableItem == null) return;

            TagSelectedValue.Query = CreateSqlSentence(SelectedTableItem, sqlKind);
            RaisePropertyChanged(nameof(Queries));
        }

        private void OnExecLimitedSql(string count)
        {
            if (SelectedTableItem == null) return;

            TagSelectedValue.Query = CreateSqlSentence(SelectedTableItem, SqlKind.Select, int.Parse(count));
            RaisePropertyChanged(nameof(Queries));
            ExecuteQuery.Execute(null);
        }

        private void OnExecSqlTemplate(SqlKind sqlKind)
        {
            SetSqlTemplate.Execute(sqlKind);
            ExecuteQuery.Execute(null);
        }

        private void OnShutdownDatabase()
        {
            if (CurrentDatabase == null) return;

            Databases.Remove(CurrentDatabase);
        }

        private void OnChangeConfig()
        {
            if (CurrentDatabase == null) return;
            var vm = new ConnectionSettingsViewModel(CurrentDatabase.DbInfo);
            MessengerInstance.Send(new MessageBase(vm, "WindowOpen"));
        }

        private void OnShowEntity()
        {
            if (CurrentDatabase == null) return;
            var vm = new EntityRelationshipViewModel(CurrentDatabase);
            MessengerInstance.Send(new MessageBase(vm, "EntityWindowOpen"));
        }

        private void OnAddTab()
        {
            TagSelectedValue.Header = $"Query{Queries.Count}";
            Queries.Add(QueryTabViewModel.GetNewInstance());
            RaisePropertyChanged(nameof(Queries));
        }

        private void OnDeleteTabItem()
        {
            var item = TagSelectedValue;
            var idx = Queries.IndexOf(item);
            TagSelectedIndex = idx - 1;
            RaisePropertyChanged(nameof(TagSelectedIndex));
            Queries.Remove(item);
        }

        private void OnLoadHistory()
        {
            if (Path.GetDirectoryName(Environment.ProcessPath) is { } path)
            {
                _history.LoadData(path);
            }
        }

        private void OnSaveHistory()
        {
            if (Path.GetDirectoryName(Environment.ProcessPath) is { } path)
            {
                _history.SaveData(path);
            }
        }

        private void OnOpenGitPage()
        {
            var psi = new ProcessStartInfo
            {
                FileName = "https://github.com/degarashi0913/FAManagementStudio",
                UseShellExecute = true
            };
            Process.Start(psi);
        }

        private void OnPinedCommand()
        {
            Datasource[0].Header = $"Pin{Datasource.Count}";
            Datasource.Insert(0, new QueryResultViewModel("Result"));
            SelectedResultIndex = 0;
            RaisePropertyChanged(nameof(SelectedResultIndex));
        }

        private bool CanExecutePinedCommand() => 0 < Datasource[0].Result.Count;

        private void OnReleasePinCommand(QueryResultViewModel data)
        {
            Datasource.Remove(data);
        }

        private void OnSetSqlDataTemplate(string s)
        {
            if (CurrentDatabase == null) return;
            if (SelectedTableItem == null) return;

            var table = GetTreeViewTableName(CurrentDatabase, SelectedTableItem);
            var result = "";
            if (s == "table")
            {
                result = table.GetDdl(CurrentDatabase);
            }
            else if (s == "insert")
            {
                if (table is TableViewViewModel) return;
                var columns = table.Columns.Select(x => x.ColumName).ToArray();
                var escapedColumnsStr = string.Join(", ", columns.Select(x => _queryBuilder.EscapeKeyWord(x)).ToArray());

                var insertTemplate = $"insert into {table.TableName} ({escapedColumnsStr})";

                var qry = new QueryInfo(false);
                var res = qry.ExecuteQuery(CurrentDatabase.ConnectionString, _queryBuilder.CreateSelectStatement(table.TableName, columns, 0)).First();

                var sb = new StringBuilder();

                foreach (DataRow row in res.View.Rows)
                {
                    sb.Append(insertTemplate + " values(");
                    sb.Append(string.Join(", ", row.ItemArray
                                                    .Select(x =>
                                                    {
                                                        var type = x?.GetType();
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
        }

        private void OnShowPathSettings()
        {
            var vm = new BasePathSettingsViewModel(QueryProjects);
            MessengerInstance.Send(new MessageBase(vm, "BasePathSettingsWindowOpen"));
        }

        private void OnReloadFileView()
        {
            _queryProjects.Clear();
            SetQueryProject();
        }

        private void OnProjectItemOpen(object obj)
        {
            if (obj is not QueryProjectFileViewModel item) return;
            var idx = Queries.IndexOf(TagSelectedValue);
            try
            {
                Queries[idx].Header = Path.GetFileNameWithoutExtension(item.FullPath);
                Queries[idx].Query = Queries[idx].FileLoad(item.FullPath, Encoding.UTF8);
            }
            catch { }
        }

        private void OnProjectItemDrop(string path)
        {
            _queryProjects.Add(QueryProjectViewModel.GetData(path).First());
            AppSettingsManager.QueryProjectBasePaths.Add(path);
        }

        private static ITableViewModel GetTreeViewTableName(DbViewModel db, object treeItem)
            => treeItem switch
            {
                ITableViewModel table => table,
                ColumViewModel table => db.Tables.First(x => x.Columns.Any(c => c == (ColumViewModel)treeItem)),
                _ => throw new NotImplementedException($"{treeItem.GetType()} is not supported")
            };

        private string CreateSqlSentence(object treeItem, SqlKind sqlKind, int limitCount = 0)
        {
            if (CurrentDatabase == null) throw new InvalidOperationException("CurrentDatabase is null");

            var table = GetTreeViewTableName(CurrentDatabase, treeItem);

            string[] columns = treeItem is ColumViewModel col
                   ? [col.ColumName]
                   : [.. table.Columns.Select(x => x.ColumName)];

            return sqlKind switch
            {
                SqlKind.Select => _queryBuilder.CreateSelectStatement(table.TableName, columns, limitCount),
                SqlKind.Insert => _queryBuilder.CreateInsertStatement(table.TableName, columns),
                SqlKind.Update => _queryBuilder.CreateUpdateStatement(table.TableName, columns),
                _ => ""
            };
        }

        #endregion

        private async Task SetNewVersionStatusAsync()
        {
            try
            {
                var latestVersion = "";
                var version = Assembly.GetExecutingAssembly().GetName().Version;
                if (version == null) return;

                var versionStr = $"{version.Major}.{version.Minor}.{version.Build}";
                if ((AppSettingsManager.StartTime - AppSettingsManager.PreviousActivation).Days < 1)
                {
                    latestVersion = string.IsNullOrEmpty(AppSettingsManager.Version) ? versionStr : AppSettingsManager.Version;
                }
                else
                {
                    latestVersion = await GetNewVersion();
                    AppSettingsManager.Version = latestVersion;
                }

                if (latestVersion != versionStr)
                {
                    ExistNewVersion = Visibility.Visible;
                }
            }
            catch
            {
            }
        }

        private static async Task<string> GetNewVersion()
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("FAManagementStudio");
            var response = await httpClient.GetAsync("https://github.com/degarashi0913/FAManagementStudio/releases/latest");
            response.EnsureSuccessStatusCode();

            var html = await response.Content.ReadAsStringAsync();
            var title = ExtractTitleRegex().Match(html).Groups["title"].Value;
            return ExtractVersionRegex().Match(title).Groups["version"].Value;
        }

        private ObservableCollection<IProjectNodeViewModel> _queryProjects = [];
        public ReadOnlyObservableCollection<IProjectNodeViewModel> QueryProjects { get; }

        private void SetQueryProject()
        {
            foreach (var pItem in QueryProjectViewModel.GetData([.. AppSettingsManager.QueryProjectBasePaths]))
            {
                _queryProjects.Add(pItem);
            }
        }

        public FirebirdRecommender Recommender { get; } = new FirebirdRecommender();

        [GeneratedRegex(@"\<title\>(?<title>.*)\<\/title\>")]
        private static partial Regex ExtractTitleRegex();
        [GeneratedRegex(@"FAManagementStudio-v(?<version>\d*\.\d*\.\d*)")]
        private static partial Regex ExtractVersionRegex();
    }
}
