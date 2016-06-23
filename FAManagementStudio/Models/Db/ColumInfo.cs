using FAManagementStudio.Common;

namespace FAManagementStudio.Models
{
    public class ColumInfo
    {
        public string ColumName { get; set; }
        public FieldType ColumType { get; set; }
        public string DomainName { get; set; }
        public bool NullFlag { get; set; }
        public ConstraintsInfo ConstraintsInf { get; set; }

        public ColumInfo(string name, FieldType type, ConstraintsInfo inf, string domainName, bool nullFlag)
        {
            ColumName = name;
            ColumType = type;
            ConstraintsInf = inf;
            DomainName = domainName;
            NullFlag = nullFlag;
        }
    }
    public class ConstraintsInfo
    {
        public ConstraintsInfo() { }
        public ConstraintsInfo(ConstraintsKind kind)
        {
            Kind = kind;
        }
        public string ForeignKeyTableName { get; set; }
        public ConstraintsKind Kind { get; private set; } = ConstraintsKind.None;
        public void SetKind(ConstraintsKind kind)
        {
            Kind |= kind;
        }
    }
}
