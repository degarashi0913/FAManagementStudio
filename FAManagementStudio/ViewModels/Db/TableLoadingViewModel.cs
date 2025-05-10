using FAManagementStudio.Common;
using FAManagementStudio.ViewModels.Commons;
using System.Collections.Generic;

namespace FAManagementStudio.ViewModels.Db;

public class TableLoadingViewModel() : ViewModelBase, ITableViewModel
{
    public string TableName => "Loading";
    public IReadOnlyCollection<ColumViewModel> Columns => [];
    public TableKind Kind => TableKind.Table;
    
    public string GetDdl(DbViewModel dbVm) => string.Empty;
}
