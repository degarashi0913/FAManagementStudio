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
                Triggers.AddRange(Tables.SelectMany(x => ((TableViewModel)x).Triggers));
                Indexes.AddRange(Tables.SelectMany(x => ((TableViewModel)x).Indexs));
                foreach (var item in _dbInfo.GetViews(con))
                {
                    var vm = new ViewViewModel(item.ViewName, item.Source);
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

public interface ITableViewModel
{
    string TableName { get; }
    string GetDdl(DbViewModel dbVm);
    List<ColumViewMoodel> Colums { get; }
    TableKind Kind { get; }
}

public class TableViewModel : BindableBase, ITableViewModel
{
    private string _name;
    public TableViewModel(string name)
    {
        _name = name;
    }
    public string TableName { get { return _name; } }
    public List<ColumViewMoodel> Colums { get; } = new List<ColumViewMoodel>();
    public TableKind Kind { get; } = TableKind.Table;
    public List<TriggerViewModel> Triggers { get; } = new List<TriggerViewModel>();
    public List<IndexViewModel> Indexs { get; } = new List<IndexViewModel>();

    public string GetDdl(DbViewModel dbVm)
    {
        var colums = Colums.Select(x =>
        {
            var sql = $"{x.ColumName} {x.ColumType}";
            if (!x.NullFlag)
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

        var domain = Colums.Where(x => x.IsDomainType)
                            .Select(x => new { x.ColumType, x.ColumDataType })
                            .Distinct()
                            .Select(x => $"CREATE DOMAIN {x.ColumType} AS {x.ColumDataType};\r\n");
        var domainStr = string.Join("", domain.ToArray());
        return domainStr + $"CREATE TABLE {TableName} ({Environment.NewLine}  { string.Join($",{Environment.NewLine}  ", colums.Union(index).ToArray()) + Environment.NewLine})";
    }
}

public class ViewViewModel : BindableBase, ITableViewModel
{
    private string _name;
    public ViewViewModel(string name, string source)
    {
        _name = name;
        Source = source;
    }
    public string TableName { get { return _name; } }
    public TableKind Kind { get; } = TableKind.View;
    public List<ColumViewMoodel> Colums { get; } = new List<ColumViewMoodel>();
    public string Source { get; private set; }
    public string GetDdl(DbViewModel dbVm)
    {
        return $"CREATE VIEW {TableName} ({string.Join(", ", Colums.Select(x => x.ColumName).ToArray())}) AS" + Environment.NewLine
            + Source;
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
    public string DisplayName
    {
        get
        {
            if (_inf.DomainName.StartsWith("RDB$"))
            {
                return $"{_inf.ColumName} ({_inf.ColumType.ToString()})";
            }
            else
            {
                return $"{_inf.ColumName} ({_inf.DomainName})";
            }
        }
    }
    public string ColumType
    {
        get
        {
            if (_inf.DomainName.StartsWith("RDB$"))
            {
                return _inf.ColumType.ToString();
            }
            else
            {
                return _inf.DomainName;
            }
        }
    }

    public string ColumDataType
    {
        get { return _inf.ColumType.ToString(); }
    }

    public bool IsDomainType
    {
        get { return !_inf.DomainName.StartsWith("RDB$"); }
    }

    public ConstraintsKind KeyKind { get { return _inf.KeyKind; } }
    public bool NullFlag { get { return _inf.NullFlag; } }
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


