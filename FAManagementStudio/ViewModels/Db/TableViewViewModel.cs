using FAManagementStudio.Common;
using FAManagementStudio.ViewModels.Commons;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FAManagementStudio.ViewModels.Db;

public class TableViewViewModel(string name, string source, IReadOnlyCollection<ColumViewModel> columns) : BindableBase, ITableViewModel
{
    public string TableName { get; } = name;
    public TableKind Kind { get; } = TableKind.View;
    public string Source { get; } = source;
    public IReadOnlyCollection<ColumViewModel> Columns { get; } = columns;

    public string GetDdl(DbViewModel dbVm)
    {
        return $"CREATE VIEW {TableName} ({string.Join(", ", Columns.Select(x => x.ColumName).ToArray())}) AS" + Environment.NewLine
            + Source;
    }
}
