using FAManagementStudio.Common;
using FAManagementStudio.Models;
using FAManagementStudio.Models.Db;
using FirebirdSql.Data.FirebirdClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace FAManagementStudio.ViewModels.Db;

public class DbViewModel(DatabaseInfo dbInfo) : ViewModelBase
{
    public static DbViewModel CreateDatabase(string path, FirebirdType type, FbCharset charset)
    {
        var dbInfo = new DatabaseInfo(new FirebirdInfo(path, type, charset));
        dbInfo.CreateDatabase();
        var vm = new DbViewModel(dbInfo);
        return vm;
    }

    // TODO: 後で削除する。Test用に抽象化する
    public static DbViewModel CreateDatabaseForTest()
    {
        var dbInfo = new DatabaseInfo(new FirebirdInfo());
        var vm = new DbViewModel(dbInfo);
        return vm;
    }

    public DatabaseInfo DbInfo => dbInfo;

    public string DisplayDbName => dbInfo.Path[(dbInfo.Path.LastIndexOf('\\') + 1)..];

    public List<ITableViewModel> Tables { get; private set; } =
    [
        //暫定
        new TableViewModel("Loading")
    ];

    private List<TriggerViewModel>? _triggers;
    public IReadOnlyList<TriggerViewModel> Triggers
    {
        get => _triggers ??= [.. Tables.Where(x => x is TableViewModel).SelectMany(x => ((TableViewModel)x).Triggers)];
    }

    private List<IndexViewModel>? _indexes;
    public IReadOnlyList<IndexViewModel> Indexes
    {
        get => _indexes ??= [.. Tables.Where(x => x is TableViewModel).SelectMany(x => ((TableViewModel)x).Indexs)];
    }

    // TODO: 後で削除する。
    [Obsolete("Test用の一時的な実装です。")]
    public void SetIndexes(List<IndexViewModel> indexes)
    {
        _indexes = indexes;
        RaisePropertyChanged(nameof(Indexes));
    }

    private List<DomainViewModel>? _domains;
    public IReadOnlyList<DomainViewModel> Domains
    {
        get => _domains ??= GetDomains();
    }

    private List<DomainViewModel> GetDomains()
    {
        using var con = new FbConnection(dbInfo.ConnectionString);
        con.Open();
        return [.. dbInfo.GetDomain(con).Select(x => new DomainViewModel(x))];
    }
    private List<ProcedureViewModel>? _procedures;
    public IReadOnlyList<ProcedureViewModel> Procedures
    {
        get => _procedures ??= GetProcedures();
    }

    private List<ProcedureViewModel> GetProcedures()
    {
        using var con = new FbConnection(dbInfo.ConnectionString);
        con.Open();
        return [.. dbInfo.GetProcedures(con).Select(x => new ProcedureViewModel(x))];
    }
    private List<SequenceViewModel>? _sequences;
    public IReadOnlyList<SequenceViewModel> Sequences
    {
        get => _sequences ??= GetSequences();
    }

    private List<SequenceViewModel> GetSequences()
    {
        using var con = new FbConnection(dbInfo.ConnectionString);
        con.Open();
        return [.. dbInfo.GetSequences(con).Select(x => new SequenceViewModel(x))];
    }

    public string ConnectionString { get { return dbInfo.ConnectionString; } }
    public string Path { get { return dbInfo.Path; } }

    public AdditionalDbInfoControl AdditionalInfo { get; private set; }

    public bool CanExecute()
        => !string.IsNullOrEmpty(dbInfo.ConnectionString);

    public async Task LoadDatabase()
    {
        var table = new List<ITableViewModel>();
        await Task.Run(() =>
         {
             using var con = new FbConnection(dbInfo.ConnectionString);
             con.Open();
             foreach (var item in dbInfo.GetTables(con))
             {
                 var vm = new TableViewModel(item.TableName);
                 vm.Colums.AddRange([.. item.GetColumns(con).Select(x => new ColumViewMoodel(x))]);
                 vm.Triggers.AddRange([.. item.GetTrigger(con).Select(x => new TriggerViewModel(x))]);
                 vm.Indexs.AddRange([.. item.GetIndex(con).Select(x => new IndexViewModel(x))]);
                 table.Add(vm);
             }

             foreach (var item in dbInfo.GetViews(con))
             {
                 var vm = new TableViewViewModel(item.ViewName, item.Source);
                 vm.Colums.AddRange([.. item.GetColums(con).Select(x => new ColumViewMoodel(x))]);
                 table.Add(vm);
             }
         });
        Tables = table;
        AdditionalInfo = new AdditionalDbInfoControl(this);
        AdditionalInfo.InitContent();
        RaisePropertyChanged(nameof(Tables));
        RaisePropertyChanged(nameof(AdditionalInfo));
    }

    public async Task LoadSystemTables(DatabaseInfo dbInf)
    {
        dbInfo = dbInf;
        var table = new List<ITableViewModel>();
        await Task.Run(() =>
        {
            using var con = new FbConnection(dbInfo.ConnectionString);
            con.Open();
            foreach (var item in dbInfo.GetSystemTables(con))
            {
                var vm = new TableViewModel(item.TableName, true);
                vm.Colums.AddRange([.. item.GetColumns(con).Select(x => new ColumViewMoodel(x))]);
                table.Add(vm);
            }
        });
        Tables.AddRange(table);
        CollectionViewSource.GetDefaultView(Tables).Refresh();
        RaisePropertyChanged(nameof(Tables));
    }

    private ICommand? _reloadDatabase;
    public ICommand ReloadDatabase => _reloadDatabase ??= new RelayCommand(async () => await Reload());

    private ICommand? _changeSystemTables;
    public ICommand ChangeSystemTables => _changeSystemTables ??= new RelayCommand<bool>(async x => await OnChangeSystemTablesAsync(x));

    public async Task Reload()
    {
        Tables.Clear();
        _triggers = null;
        _indexes = null;
        _domains = null;
        _procedures = null;
        _sequences = null;
        await LoadDatabase();
    }

    private async Task OnChangeSystemTablesAsync(bool isEnable)
    {
        if (isEnable)
        {
            await LoadSystemTables(DbInfo);
        }
        else
        {
            foreach (var table in Tables.Where(x => (x is TableViewModel model) && model.IsSystemTable).ToArray())
            {
                Tables.Remove(table);
            }
            CollectionViewSource.GetDefaultView(Tables).Refresh();
            RaisePropertyChanged(nameof(Tables));
        }
    }

    public bool IsSystemTableChecked { get; set; }
}