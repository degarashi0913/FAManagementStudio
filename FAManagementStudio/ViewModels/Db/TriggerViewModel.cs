using FAManagementStudio.Common;
using FAManagementStudio.Models;

namespace FAManagementStudio.ViewModels
{
    public class TriggerViewModel : ViewModelBase, IAddtionalDbInfo
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
}
