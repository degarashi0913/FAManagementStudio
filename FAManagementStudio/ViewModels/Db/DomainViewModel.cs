using FAManagementStudio.Common;
using FAManagementStudio.Models;

namespace FAManagementStudio.ViewModels
{
    public class DomainViewModel : BindableBase, IAddtionalDbInfo
    {
        private DomainInfo _inf;
        public DomainViewModel(DomainInfo inf)
        {
            _inf = inf;
        }
        public string DomainName { get { return _inf.DomainName; } }

        public string DomainType
        {
            get { return _inf.DomainType.ToString(); }
        }
    }
}
