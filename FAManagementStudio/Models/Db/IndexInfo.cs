using FAManagementStudio.Common;
using System.Collections.Generic;

namespace FAManagementStudio.Models;

public record class IndexInfo(
    string Name,
    bool UniqueFlag,
    ConstraintsKind Kind,
    string ForeignKeyName,
    IReadOnlyList<string> FieldNames,
    string TableName,
    string UpdateRule, // only ForeignKey
    string DeleteRule  // onlyForeignKey
    );