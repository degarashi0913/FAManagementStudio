using FAManagementStudio.Models;
using FAManagementStudio.ViewModels;
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
            var dbVm = new DbViewModel();
            var table = new TableViewModel(new TableInfo("TEST"));
            dbVm.Tables.Add(table);

            var col1 = new ColumInfo("COL1", 8, null, null, FAManagementStudio.Common.ConstraintsKind.Primary);
            table.Colums.Add(new ColumViewMoodel(col1));
            var col2 = new ColumInfo("COL2", 37, null, 100, FAManagementStudio.Common.ConstraintsKind.NotNull);
            table.Colums.Add(new ColumViewMoodel(col2));
            var col3 = new ColumInfo("COL3", 35, null, null, FAManagementStudio.Common.ConstraintsKind.NotNull);
            table.Colums.Add(new ColumViewMoodel(col3));
            var col4 = new ColumInfo("COL4", 261, null, null, FAManagementStudio.Common.ConstraintsKind.None);
            table.Colums.Add(new ColumViewMoodel(col4));

            var idx = new IndexInfo();
            idx.Name = "RDB$PRIMARYKEY1";
            idx.TableName = table.TableName;
            idx.Kind = FAManagementStudio.Common.ConstraintsKind.Primary;
            idx.FieldNames.Add(col1.ColumName);
            var idxVm = new IndexViewModel(idx);

            dbVm.Indexes.Add(idxVm);
            table.Indexs.Add(idxVm);

            table.GetDdl(dbVm).Is("CREATE TABLE TEST (\r\n  COL1 INTEGER,\r\n  COL2 VARCHAR(100) NOT NULL,\r\n  COL3 TIMESTAMP NOT NULL,\r\n  COL4 BLOB,\r\n  PRIMARY KEY (COL1)\r\n)");
        }
    }
}