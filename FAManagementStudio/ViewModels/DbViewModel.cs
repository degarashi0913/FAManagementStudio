using FAManagementStudio.Common;
using FAManagementStudio.Models;
using FirebirdSql.Data.FirebirdClient;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.Linq;
using FAManagementStudio.ViewModels;

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
        public List<IndexViewModel> Indexes { get; } = new List<IndexViewModel>();

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
                Triggers.AddRange(Tables.SelectMany(x => x.Triggers));
                Indexes.AddRange(Tables.SelectMany(x => x.Indexs));
                RaisePropertyChanged(nameof(Tables));
                RaisePropertyChanged(nameof(Triggers));
            }
        }

        public void ReloadDatabase()
        {
            Tables.Clear();
            Triggers.Clear();
            LoadDatabase(_dbInfo.Path);
            CollectionViewSource.GetDefaultView(this.Tables).Refresh();
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
    public List<ColumViewMoodel> Colums { get; } = new List<ColumViewMoodel>();
    public List<TriggerViewModel> Triggers { get; } = new List<TriggerViewModel>();
    public List<IndexViewModel> Indexs { get; } = new List<IndexViewModel>();

    public string GetDdl(DbViewModel dbVm)
    {
        var colums = Colums.Select(x =>
        {
            var sql = $"{x.ColumName} {x.ColumType}";
            if (x.KeyKind == ConstraintsKind.NotNull)
            {
                sql += " not null";
            }
            return sql.ToUpper();
        });

        var index = Indexs
                .Select(x =>
                {
                    var sql = x.IndexName.StartsWith("rdb", System.StringComparison.OrdinalIgnoreCase) ? "" : $"CONSTRAINT {x.IndexName} ";
                    switch (x.Kind)
                    {
                        case ConstraintsKind.Primary:
                            sql += $"PRIMARY KEY ({string.Join(", ", x.FieldNames.ToArray())})";
                            break;
                        case ConstraintsKind.Foreign:
                            var idx = dbVm.Indexes.Where(dbIdx => dbIdx.IndexName == x.IndexName).First();
                            sql += $"FOREIGN KEY({string.Join(", ", x.FieldNames.ToArray())}) REFERENCES {idx.TableName} ({string.Join(", ", idx.FieldNames.ToArray())})";
                            break;
                        case ConstraintsKind.Unique:
                            sql += $"UNIQUE ({string.Join(", ", x.FieldNames.ToArray())})";
                            break;
                        default:
                            return "";
                    }
                    return sql;
                });

        return $"CREATE TABLE {TableName} (\r\n  { string.Join(",\r\n  ", colums.Union(index).ToArray())}\r\n)";
    }
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
    public string ColumType { get { return _inf.ColumType; } }
    public ConstraintsKind KeyKind { get { return _inf.KeyKind; } }
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

public class IndexViewModel
{
    private IndexInfo _index;
    public IndexViewModel(IndexInfo inf)
    {
        _index = inf;
    }

    public string IndexName { get { return _index.Name; } }

    public ConstraintsKind Kind { get { return _index.Kind; } }

    public string ForignKeyName { get { return _index.ForigenKeyName; } }

    public string TableName { get { return _index.TableName; } }

    public List<string> FieldNames { get { return _index.FieldNames; } }
}


