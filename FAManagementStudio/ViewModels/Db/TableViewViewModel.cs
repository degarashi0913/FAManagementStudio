using FAManagementStudio.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FAManagementStudio.ViewModels
{
    public class TableViewViewModel : BindableBase, ITableViewModel
    {
        private string _name;
        public TableViewViewModel(string name, string source)
        {
            _name = name;
            Source = source;
        }
        public string TableName { get { return _name; } }
        public TableKind Kind { get; } = TableKind.View;
        public List<ColumViewMoodel> Colums { get; } = new List<ColumViewMoodel>();
        public string Source { get; private set; }

        public string GetDdl(DbViewModel dbVm)
        {
            return $"CREATE VIEW {TableName} ({string.Join(", ", Colums.Select(x => x.ColumName).ToArray())}) AS" + Environment.NewLine
                + Source;
        }
    }
}
