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
            get
            {
                var domainType = _inf.DomainType.ToString();
                if (!_inf.IsNullFlag)
                {
                    domainType += $" (NOT NULL)";
                }
                return domainType;
            }
        }
        public string ValidationSource { get { return _inf.ValidationSource; } }
        public string DefaultSource { get { return _inf.DefaultSource; } }
    }
}
