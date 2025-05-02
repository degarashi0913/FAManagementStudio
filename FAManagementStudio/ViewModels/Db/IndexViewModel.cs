using FAManagementStudio.Common;
using FAManagementStudio.Models;
using System.Collections.Generic;

namespace FAManagementStudio.ViewModels;

public class IndexViewModel(IndexInfo indexInfo) : ViewModelBase
{
    public string IndexName => indexInfo.Name;

    public string IsUnique => indexInfo.UniqueFlag ? "〇" : "×";
    public ConstraintsKind Kind => indexInfo.Kind;

    public string ForeignKeyName => indexInfo.ForeignKeyName;
    public string UpdateRule => indexInfo.UpdateRule;
    public string DeleteRule => indexInfo.DeleteRule;

    public string TableName => indexInfo.TableName;

    public IReadOnlyList<string> FieldNames => indexInfo.FieldNames;
}
