using FAManagementStudio.Common;
using System.Collections.Generic;

namespace FAManagementStudio.ViewModels
{
    public class ViewModelBase : BindableBase
    {
        protected Messenger MessengerInstance { get; set; } = Messenger.Instance;
    }

    public interface ITableViewModel
    {
        string TableName { get; }
        string GetDdl(DbViewModel dbVm);
        List<ColumViewMoodel> Colums { get; }
        TableKind Kind { get; }
    }
}
