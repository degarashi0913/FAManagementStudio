using FAManagementStudio.Common;

namespace FAManagementStudio.Models.Db;

public class ColumInfo(string name, FieldType type, ColumConstraintsInfo? inf, string domainName, bool nullFlag, bool fieldNullFlag, string defaultSource)
{
    public string ColumName { get; set; } = name;
    public FieldType ColumType { get; set; } = type;
    public string DomainName { get; set; } = domainName;
    public bool NullFlag { get; set; } = nullFlag;
    public bool FieldNullFlag { get; set; } = fieldNullFlag;
    public ColumConstraintsInfo? ConstraintsInf { get; set; } = inf;
    public string DefaultSource { get; set; } = defaultSource;
}
public class ColumConstraintsInfo
{
    public ColumConstraintsInfo() { }
    public ColumConstraintsInfo(ConstraintsKind kind)
    {
        Kind = kind;
    }
    public string ForeignKeyTableName { get; set; } = string.Empty;
    public ConstraintsKind Kind { get; private set; } = ConstraintsKind.None;
    public void SetKind(ConstraintsKind kind)
    {
        Kind |= kind;
    }
}