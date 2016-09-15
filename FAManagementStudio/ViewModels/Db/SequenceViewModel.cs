using FAManagementStudio.Common;
using FAManagementStudio.Models;

namespace FAManagementStudio.ViewModels
{
    public class SequenceViewModel : BindableBase, IAddtionalDbInfo
    {
        private SequenceInfo _inf;
        public SequenceViewModel(SequenceInfo inf)
        {
            _inf = inf;
        }
        public string Name { get { return _inf.Name; } }
        public long CurrentValue { get { return _inf.CurrentValue; } }
    }
}
