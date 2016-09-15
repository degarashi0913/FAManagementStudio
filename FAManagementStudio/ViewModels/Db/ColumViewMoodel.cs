using FAManagementStudio.Common;
using FAManagementStudio.Models;

namespace FAManagementStudio.ViewModels
{
    public class ColumViewMoodel : ViewModelBase
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
                    var nullStr = _inf.NullFlag ? "" : ", NOT NULL";
                    return $"{_inf.ColumName} ({_inf.ColumType.ToString()}{nullStr})";
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

        public ColumConstraintsInfo ConstraintsInf { get { return _inf.ConstraintsInf; } }
        public bool NullFlag { get { return _inf.NullFlag; } }
        public bool FieldNullFlag { get { return _inf.FieldNullFlag; } }
        public string ToolTip { get { return _inf.DefaultSource; } }
        public string DefaultSource { get { return _inf.DefaultSource; } }
    }
}
