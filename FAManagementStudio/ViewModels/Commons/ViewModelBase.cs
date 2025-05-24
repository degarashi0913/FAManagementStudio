using FAManagementStudio.Common;
using FAManagementStudio.ViewModels.Db;
using System.Collections.Generic;

namespace FAManagementStudio.ViewModels.Commons;

public class ViewModelBase : BindableBase
{
    protected Messenger MessengerInstance { get; set; } = Messenger.Instance;
}

public interface ITableViewModel
{
    string TableName { get; }
    string GetDdl(DbViewModel dbVm);
    IReadOnlyCollection<ColumViewModel> Columns { get; }
    TableKind Kind { get; }
}
