using FAManagementStudio.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    [TestClass()]
    public class TableViewModelTests
    {
        [TestMethod()]
        public void GetDdlTest()
        {
            var table = new TableViewModel(new TableInfo("test"));
            var col1 = new ColumInfo("col1", 8, null, FAManagementStudio.Common.ConstraintsKind.Primary);
            table.ChildItems.Add(new ColumViewMoodel(col1));
            var col2 = new ColumInfo("col2", 37, 100, FAManagementStudio.Common.ConstraintsKind.NotNull);
            table.ChildItems.Add(new ColumViewMoodel(col2));
            var col3 = new ColumInfo("col3", 35, null, FAManagementStudio.Common.ConstraintsKind.NotNull);
            table.ChildItems.Add(new ColumViewMoodel(col3));
            var col4 = new ColumInfo("col4", 261, null, FAManagementStudio.Common.ConstraintsKind.None);
            table.ChildItems.Add(new ColumViewMoodel(col4));

            table.GetDdl().Is("CREATE TABLE TEST (COL1 INTEGER PRIMARY KEY, COL2 VARCHAR(100) NOT NULL, COL3 TIMESTAMP NOT NULL, COL4 BLOB)");
        }
    }
}