using FAManagementStudio.Common;

namespace FAManagementStudio.Models
{
    public class ColumInfo
    {
        public string ColumName { get; set; }
        public FieldType ColumType { get; set; }
        public string DomainName { get; set; }
        public bool NullFlag { get; set; }
        public bool FieldNullFlag { get; set; }
        public ConstraintsInfo ConstraintsInf { get; set; }
        public string DefaultSource { get; set; }

        public ColumInfo(string name, FieldType type, ConstraintsInfo inf, string domainName, bool nullFlag, bool fieldNullFlag, string defaltSource)
        {
            ColumName = name;
            ColumType = type;
            ConstraintsInf = inf;
            DomainName = domainName;
            NullFlag = nullFlag;
            FieldNullFlag = fieldNullFlag;
            DefaultSource = defaltSource;
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
