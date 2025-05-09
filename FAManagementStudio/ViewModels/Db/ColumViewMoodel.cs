using FAManagementStudio.Models.Db;

namespace FAManagementStudio.ViewModels.Db;

public class ColumViewModel(ColumInfo inf) : ViewModelBase
{
    public string ColumName { get { return inf.ColumName; } }
    public string DisplayName
    {
        get
        {
            if (inf.DomainName.StartsWith("RDB$"))
            {
                var nullStr = inf.NullFlag ? "" : ", NOT NULL";
                return $"{inf.ColumName} ({inf.ColumType}{nullStr})";
            }
            else
            {
                return $"{inf.ColumName} ({inf.DomainName})";
            }
        }
    }
    public string ColumType
    {
        get
        {
            if (inf.DomainName.StartsWith("RDB$"))
            {
                return inf.ColumType.ToString();
            }
            else
            {
                return inf.DomainName;
            }
        }
    }
    public string ColumDataType
    {
        get { return inf.ColumType.ToString(); }
    }

    public bool IsDomainType
    {
        get { return !inf.DomainName.StartsWith("RDB$"); }
    }

    public ColumConstraintsInfo? ConstraintsInf { get { return inf.ConstraintsInf; } }
    public bool NullFlag { get { return inf.NullFlag; } }
    public bool FieldNullFlag { get { return inf.FieldNullFlag; } }
    public string ToolTip { get { return inf.DefaultSource; } }
    public string DefaultSource { get { return inf.DefaultSource; } }
}
