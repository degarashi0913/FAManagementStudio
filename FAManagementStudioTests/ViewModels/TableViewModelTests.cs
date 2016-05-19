using FAManagementStudio.Common;
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
        public void GetDdlTest1()
        {
            var dbVm = new DbViewModel();
            var table = new TableViewModel("TEST", TableKind.Table);
            dbVm.Tables.Add(table);

            var col1 = new ColumInfo("COL1", new FieldType(8, null, null, null, null), FAManagementStudio.Common.ConstraintsKind.Primary, "RDB$1", false);
            table.Colums.Add(new ColumViewMoodel(col1));
            var col2 = new ColumInfo("COL2", new FieldType(37, null, 100, null, null), FAManagementStudio.Common.ConstraintsKind.None, "RDB$2", false);
            table.Colums.Add(new ColumViewMoodel(col2));
            var col3 = new ColumInfo("COL3", new FieldType(35, null, null, null, null), FAManagementStudio.Common.ConstraintsKind.None, "RDB$3", false);
            table.Colums.Add(new ColumViewMoodel(col3));
            var col4 = new ColumInfo("COL4", new FieldType(261, null, null, null, null), FAManagementStudio.Common.ConstraintsKind.None, "RDB$4", true);
            table.Colums.Add(new ColumViewMoodel(col4));

            var idx = new IndexInfo();
            idx.Name = "RDB$PRIMARYKEY1";
            idx.TableName = table.TableName;
            idx.Kind = FAManagementStudio.Common.ConstraintsKind.Primary;
            idx.FieldNames.Add(col1.ColumName);
            var idxVm = new IndexViewModel(idx);

            dbVm.Indexes.Add(idxVm);
            table.Indexs.Add(idxVm);

            table.GetDdl(dbVm).Is(
"CREATE TABLE TEST (" + Environment.NewLine +
"  COL1 INTEGER NOT NULL," + Environment.NewLine +
"  COL2 VARCHAR(100) NOT NULL," + Environment.NewLine +
"  COL3 TIMESTAMP NOT NULL," + Environment.NewLine +
"  COL4 BLOB," + Environment.NewLine +
"  PRIMARY KEY (COL1)" + Environment.NewLine +
")");
        }
        [TestMethod()]
        public void GetDdlTest2()
        {
            var dbVm = new DbViewModel();
            var table = new TableViewModel("TEST", TableKind.Table);
            dbVm.Tables.Add(table);

            var col1 = new ColumInfo("COL1", new FieldType(8, null, null, null, null), FAManagementStudio.Common.ConstraintsKind.Primary, "RDB$1", false);
            table.Colums.Add(new ColumViewMoodel(col1));
            var col2 = new ColumInfo("COL2", new FieldType(37, null, 100, null, null), FAManagementStudio.Common.ConstraintsKind.Primary, "RDB$2", false);
            table.Colums.Add(new ColumViewMoodel(col2));
            var col3 = new ColumInfo("COL3", new FieldType(35, null, null, null, null), FAManagementStudio.Common.ConstraintsKind.None, "RDB$3", false);
            table.Colums.Add(new ColumViewMoodel(col3));
            var col4 = new ColumInfo("COL4", new FieldType(261, null, null, null, null), FAManagementStudio.Common.ConstraintsKind.None, "RDB$4", true);
            table.Colums.Add(new ColumViewMoodel(col4));
            var col5 = new ColumInfo("COL5", new FieldType(7, 1, null, 4, 0), FAManagementStudio.Common.ConstraintsKind.None, "RDB$4", true);
            table.Colums.Add(new ColumViewMoodel(col5));
            var col6 = new ColumInfo("COL6", new FieldType(8, 2, null, 4, -2), FAManagementStudio.Common.ConstraintsKind.None, "RDB$4", true);
            table.Colums.Add(new ColumViewMoodel(col6));
            var col7 = new ColumInfo("COL7", new FieldType(16, 2, null, 10, -4), FAManagementStudio.Common.ConstraintsKind.None, "RDB$4", true);
            table.Colums.Add(new ColumViewMoodel(col7));

            var idx = new IndexInfo();
            idx.Name = "COMPLEXKEY";
            idx.TableName = table.TableName;
            idx.Kind = FAManagementStudio.Common.ConstraintsKind.Primary;
            idx.FieldNames.Add(col1.ColumName);
            idx.FieldNames.Add(col2.ColumName);
            var idxVm = new IndexViewModel(idx);

            dbVm.Indexes.Add(idxVm);
            table.Indexs.Add(idxVm);

            table.GetDdl(dbVm).Is(
"CREATE TABLE TEST (" + Environment.NewLine +
"  COL1 INTEGER NOT NULL," + Environment.NewLine +
"  COL2 VARCHAR(100) NOT NULL," + Environment.NewLine +
"  COL3 TIMESTAMP NOT NULL," + Environment.NewLine +
"  COL4 BLOB," + Environment.NewLine +
"  COL5 NUMERIC(4)," + Environment.NewLine +
"  COL6 DECIMAL(4,2)," + Environment.NewLine +
"  COL7 DECIMAL(10,4)," + Environment.NewLine +
"  CONSTRAINT COMPLEXKEY PRIMARY KEY (COL1, COL2)" + Environment.NewLine +
")");
        }
        [TestMethod()]
        public void GetDdlTest3()
        {
            var dbVm = new DbViewModel();
            var table = new TableViewModel("TEST", TableKind.Table);
            dbVm.Tables.Add(table);

            var col1 = new ColumInfo("COL1", new FieldType(8, null, null, null, null), FAManagementStudio.Common.ConstraintsKind.Primary, "SARARY", false);
            table.Colums.Add(new ColumViewMoodel(col1));
            var col2 = new ColumInfo("COL2", new FieldType(37, null, 100, null, null), FAManagementStudio.Common.ConstraintsKind.Primary, "NAME", false);
            table.Colums.Add(new ColumViewMoodel(col2));
            var col3 = new ColumInfo("COL3", new FieldType(35, null, null, null, null), FAManagementStudio.Common.ConstraintsKind.None, "RDB$3", false);
            table.Colums.Add(new ColumViewMoodel(col3));
            var col4 = new ColumInfo("COL4", new FieldType(261, null, null, null, null), FAManagementStudio.Common.ConstraintsKind.None, "RDB$4", true);
            table.Colums.Add(new ColumViewMoodel(col4));

            var idx = new IndexInfo();
            idx.Name = "COMPLEXKEY";
            idx.TableName = table.TableName;
            idx.Kind = FAManagementStudio.Common.ConstraintsKind.Primary;
            idx.FieldNames.Add(col1.ColumName);
            idx.FieldNames.Add(col2.ColumName);
            var idxVm = new IndexViewModel(idx);

            dbVm.Indexes.Add(idxVm);
            table.Indexs.Add(idxVm);

            table.GetDdl(dbVm).Is(
"CREATE DOMAIN SARARY AS INTEGER;" + Environment.NewLine +
"CREATE DOMAIN NAME AS VARCHAR(100);" + Environment.NewLine +
"CREATE TABLE TEST (" + Environment.NewLine +
"  COL1 SARARY NOT NULL," + Environment.NewLine +
"  COL2 NAME NOT NULL," + Environment.NewLine +
"  COL3 TIMESTAMP NOT NULL," + Environment.NewLine +
"  COL4 BLOB," + Environment.NewLine +
"  CONSTRAINT COMPLEXKEY PRIMARY KEY (COL1, COL2)" + Environment.NewLine +
")");
        }
    }
}