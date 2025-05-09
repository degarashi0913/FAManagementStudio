using FAManagementStudio.Common;
using FAManagementStudio.ViewModels.Db;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FAManagementStudio.ViewModels
{
    public class TableViewViewModel : BindableBase, ITableViewModel
    {
        public TableViewViewModel(string name, string source)
        {
            TableName = name;
            Source = source;
        }
        public string TableName { get; }
        public TableKind Kind { get; } = TableKind.View;
        public List<ColumViewModel> Colums { get; } = new List<ColumViewModel>();
        public string Source { get; private set; }

        public string GetDdl(DbViewModel dbVm)
        {
            return $"CREATE VIEW {TableName} ({string.Join(", ", Colums.Select(x => x.ColumName).ToArray())}) AS" + Environment.NewLine
                + Source;
        }
    }
}
