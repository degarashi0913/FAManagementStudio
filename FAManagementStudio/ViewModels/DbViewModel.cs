using FAManagementStudio.Common;
using FAManagementStudio.Models;
using FirebirdSql.Data.FirebirdClient;
using System.Collections.Generic;

namespace FAManagementStudio.ViewModels
{
    public class DbViewModel : BindableBase
    {
        public DbViewModel()
        {
        }

        private DatabaseInfo _dbInfo = new DatabaseInfo();

        public string DisplayDbName { get { return _dbInfo.Path.Substring(_dbInfo.Path.LastIndexOf('\\') + 1); } }

        public List<TableViewModel> Tables { get; } = new List<TableViewModel>();
        public List<TriggerViewModel> Triggers { get; } = new List<TriggerViewModel>();

        public string ConnectionString { get { return _dbInfo.ConnectionString; } }
        public string Path { get { return _dbInfo.Path; } }

        public bool CanExecute()
        {
            return !string.IsNullOrEmpty(_dbInfo.ConnectionString);
        }

        public void CreateDatabase(string path)
        {
            using (var con = GetConnection(path))
            {
                con.Open();
                _dbInfo.CreateDatabase(con);
            }
            LoadDatabase(path);
        }

        public void LoadDatabase(string path)
        {
            _dbInfo.Path = path;
            using (var con = GetConnection(path))
            {
                con.Open();
                foreach (var item in _dbInfo.GetTables(con))
                {
                    var vm = new TableViewModel(item);
                    foreach (var colums in item.GetColums(con))
                    {
                        vm.ChildItems.Add(new ColumViewMoodel(colums));
                    }
                    Tables.Add(vm);
                }
                RaisePropertyChanged(nameof(Tables));
                foreach (var item in _dbInfo.GetTrigger(con))
                {
                    Triggers.Add(new TriggerViewModel(item));
                }
                RaisePropertyChanged(nameof(Triggers));
            }
        }

        public void ReloadDatabase()
        {
            LoadDatabase(_dbInfo.Path);
        }

        private FbConnection GetConnection(string path)
        {
            var builder = new FbConnectionStringBuilder();
            builder.DataSource = "localhost";
            builder.Database = path;
            builder.Charset = FbCharset.Utf8.ToString();
            builder.UserID = "SYSDBA";
            builder.Password = "masterkey";
            builder.ServerType = FbServerType.Embedded;
            builder.Pooling = false;

            return new FbConnection(builder.ConnectionString);
        }
    }
}

public class TableViewModel : BindableBase
{
    private TableInfo _inf;
    public TableViewModel(TableInfo inf)
    {
        _inf = inf;
    }
    public string TableName { get { return _inf.TableName; } }
    public string DisplayName { get { return _inf.TableName; } }
    public List<ColumViewMoodel> ChildItems { get; } = new List<ColumViewMoodel>();
}

public class ColumViewMoodel : BindableBase
{
    private ColumInfo _inf;
    public ColumViewMoodel(ColumInfo inf)
    {
        _inf = inf;
    }
    public string ColumName { get { return _inf.ColumName; } }
    public string DisplayName { get { return $"{_inf.ColumName} ({_inf.ColumType})"; } }
    public ConstraintsKeyKind KeyKind { get { return _inf.KeyKind; } }
}

public class TriggerViewModel : BindableBase
{
    private TriggerInfo _inf;
    public TriggerViewModel(TriggerInfo inf)
    {
        _inf = inf;
    }
    public string Source { get { return _inf.Source; } }
    public string TableName { get { return _inf.TableName; } }
    public string Name { get { return _inf.Name; } }
}


