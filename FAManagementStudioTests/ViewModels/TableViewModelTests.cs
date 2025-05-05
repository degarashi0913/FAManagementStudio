using FAManagementStudio.Common;
using FAManagementStudio.Models;
using FAManagementStudio.ViewModels;
using FAManagementStudio.ViewModels.Db;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

#pragma warning disable CS0618
namespace FAManagementStudioTests.ViewModels;

[TestClass()]
public class TableViewModelTests
{
    [TestMethod()]
    public void GetDdlTest1()
    {
        var dbVm = DbViewModel.CreateDatabaseForTest();
        var table = new TableViewModel("TEST");
        dbVm.Tables.Add(table);

        var col1 = new ColumInfo("COL1", new FieldType(8, null, null, null, null, 4), new ColumConstraintsInfo(ConstraintsKind.Primary), "RDB$1", false, true, "");
        table.Colums.Add(new ColumViewMoodel(col1));
        var col2 = new ColumInfo("COL2", new FieldType(37, null, 100, null, null, 400), new ColumConstraintsInfo(ConstraintsKind.None), "RDB$2", false, true, "");
        table.Colums.Add(new ColumViewMoodel(col2));
        var col3 = new ColumInfo("COL3", new FieldType(35, null, null, null, null, 8), new ColumConstraintsInfo(ConstraintsKind.None), "RDB$3", false, true, "");
        table.Colums.Add(new ColumViewMoodel(col3));
        var col4 = new ColumInfo("COL4", new FieldType(261, null, null, null, null, 8), new ColumConstraintsInfo(ConstraintsKind.None), "RDB$4", true, true, "");
        table.Colums.Add(new ColumViewMoodel(col4));

        var idx = new IndexInfo("RDB$PRIMARYKEY1", false, ConstraintsKind.Primary, "", [col1.ColumName], table.TableName, "", "");
        var idxVm = new IndexViewModel(idx);


        dbVm.SetIndexes([idxVm]);

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
        var dbVm = DbViewModel.CreateDatabaseForTest();
        var table = new TableViewModel("TEST");
        dbVm.Tables.Add(table);

        var col1 = new ColumInfo("COL1", new FieldType(8, null, null, null, null, 4), new ColumConstraintsInfo(ConstraintsKind.Primary), "RDB$1", false, true, "");
        table.Colums.Add(new ColumViewMoodel(col1));
        var col2 = new ColumInfo("COL2", new FieldType(37, null, 100, null, null, 400), new ColumConstraintsInfo(ConstraintsKind.Primary), "RDB$2", false, true, "");
        table.Colums.Add(new ColumViewMoodel(col2));
        var col3 = new ColumInfo("COL3", new FieldType(35, null, null, null, null, 8), new ColumConstraintsInfo(ConstraintsKind.None), "RDB$3", false, true, "");
        table.Colums.Add(new ColumViewMoodel(col3));
        var col4 = new ColumInfo("COL4", new FieldType(261, null, null, null, null, 8), new ColumConstraintsInfo(ConstraintsKind.None), "RDB$4", true, true, "");
        table.Colums.Add(new ColumViewMoodel(col4));
        var col5 = new ColumInfo("COL5", new FieldType(7, 1, null, 4, 0, 4), new ColumConstraintsInfo(ConstraintsKind.None), "RDB$4", true, true, "");
        table.Colums.Add(new ColumViewMoodel(col5));
        var col6 = new ColumInfo("COL6", new FieldType(8, 2, null, 4, -2, 4), new ColumConstraintsInfo(ConstraintsKind.None), "RDB$4", true, true, "");
        table.Colums.Add(new ColumViewMoodel(col6));
        var col7 = new ColumInfo("COL7", new FieldType(16, 2, null, 10, -4, 8), new ColumConstraintsInfo(ConstraintsKind.None), "RDB$4", true, true, "");
        table.Colums.Add(new ColumViewMoodel(col7));

        var idx = new IndexInfo("COMPLEXKEY", false, ConstraintsKind.Primary, "", [col1.ColumName, col2.ColumName], table.TableName, "", "");
        var idxVm = new IndexViewModel(idx);

        dbVm.SetIndexes([idxVm]);
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
        var dbVm = DbViewModel.CreateDatabaseForTest();
        var table = new TableViewModel("TEST");
        dbVm.Tables.Add(table);

        var col1 = new ColumInfo("COL1", new FieldType(8, null, null, null, null, 4), new ColumConstraintsInfo(ConstraintsKind.Primary), "SARARY", false, true, "");
        table.Colums.Add(new ColumViewMoodel(col1));
        var col2 = new ColumInfo("COL2", new FieldType(37, null, 100, null, null, 400), new ColumConstraintsInfo(ConstraintsKind.Primary), "NAME", false, true, "");
        table.Colums.Add(new ColumViewMoodel(col2));
        var col3 = new ColumInfo("COL3", new FieldType(35, null, null, null, null, 8), new ColumConstraintsInfo(ConstraintsKind.None), "RDB$3", false, true, "");
        table.Colums.Add(new ColumViewMoodel(col3));
        var col4 = new ColumInfo("COL4", new FieldType(261, null, null, null, null, 8), new ColumConstraintsInfo(ConstraintsKind.None), "RDB$4", true, true, "");
        table.Colums.Add(new ColumViewMoodel(col4));
        var col5 = new ColumInfo("COL5", new FieldType(37, null, 100, null, null, 400), new ColumConstraintsInfo(ConstraintsKind.None), "NAME", true, true, "");
        table.Colums.Add(new ColumViewMoodel(col5));
        var col6 = new ColumInfo("COL6", new FieldType(37, null, 100, null, null, 400), new ColumConstraintsInfo(ConstraintsKind.None), "MEMO", true, true, "DEFAULT 'HOGE'");
        table.Colums.Add(new ColumViewMoodel(col6));
        var col7 = new ColumInfo("COL7", new FieldType(37, null, 100, null, null, 400), new ColumConstraintsInfo(ConstraintsKind.None), "MEMO2", true, false, "DEFAULT 'HOGE2'");
        table.Colums.Add(new ColumViewMoodel(col7));

        var idx = new IndexInfo("COMPLEXKEY", false, ConstraintsKind.Primary, "", [col1.ColumName, col2.ColumName], table.TableName, "", "");
        var idxVm = new IndexViewModel(idx);

        dbVm.SetIndexes([idxVm]);
        table.Indexs.Add(idxVm);

        table.GetDdl(dbVm).Is(
"CREATE DOMAIN SARARY AS INTEGER;" + Environment.NewLine +
"CREATE DOMAIN NAME AS VARCHAR(100);" + Environment.NewLine +
"CREATE DOMAIN MEMO AS VARCHAR(100) DEFAULT 'HOGE';" + Environment.NewLine +
"CREATE DOMAIN MEMO2 AS VARCHAR(100) NOT NULL DEFAULT 'HOGE2';" + Environment.NewLine +
"CREATE TABLE TEST (" + Environment.NewLine +
"  COL1 SARARY NOT NULL," + Environment.NewLine +
"  COL2 NAME NOT NULL," + Environment.NewLine +
"  COL3 TIMESTAMP NOT NULL," + Environment.NewLine +
"  COL4 BLOB," + Environment.NewLine +
"  COL5 NAME," + Environment.NewLine +
"  COL6 MEMO," + Environment.NewLine +
"  COL7 MEMO2," + Environment.NewLine +
"  CONSTRAINT COMPLEXKEY PRIMARY KEY (COL1, COL2)" + Environment.NewLine +
")");
    }
    [TestMethod()]
    public void GetDdlTest4()
    {
        var dbVm = DbViewModel.CreateDatabaseForTest();
        var table = new TableViewViewModel("SNOW_LINE", "SELECT CITY, STATE, ALTITUDE FROM CITIES WHERE ALTITUDE > 5000");
        dbVm.Tables.Add(table);

        var col1 = new ColumInfo("CITY", new FieldType(8, null, null, null, null, 4), new ColumConstraintsInfo(ConstraintsKind.Primary), "SARARY", false, true, "");
        table.Colums.Add(new ColumViewMoodel(col1));
        var col2 = new ColumInfo("STATE", new FieldType(37, null, 100, null, null, 400), new ColumConstraintsInfo(ConstraintsKind.Primary), "NAME", false, true, "");
        table.Colums.Add(new ColumViewMoodel(col2));
        var col3 = new ColumInfo("SNOW_ALTITUDE", new FieldType(35, null, null, null, null, 8), new ColumConstraintsInfo(ConstraintsKind.None), "RDB$3", false, true, "");
        table.Colums.Add(new ColumViewMoodel(col3));

        table.GetDdl(dbVm).Is(
"CREATE VIEW SNOW_LINE (CITY, STATE, SNOW_ALTITUDE) AS" + Environment.NewLine +
"SELECT CITY, STATE, ALTITUDE FROM CITIES WHERE ALTITUDE > 5000");
    }

    [TestMethod()]
    public void GetDdlTest_ForignKey1()
    {
        var dbVm = DbViewModel.CreateDatabaseForTest();
        var table = new TableViewModel("TEST");
        dbVm.Tables.Add(table);

        var col1 = new ColumInfo("COL1", new FieldType(8, null, null, null, null, 4), new ColumConstraintsInfo(ConstraintsKind.Primary), "RDB$1", false, true, "");
        table.Colums.Add(new ColumViewMoodel(col1));
        var col2 = new ColumInfo("COL2", new FieldType(37, null, 100, null, null, 400), new ColumConstraintsInfo(ConstraintsKind.Foreign), "RDB$2", false, true, "");
        table.Colums.Add(new ColumViewMoodel(col2));
        var col3 = new ColumInfo("COL3", new FieldType(35, null, null, null, null, 8), new ColumConstraintsInfo(ConstraintsKind.Foreign), "RDB$3", false, true, "");
        table.Colums.Add(new ColumViewMoodel(col3));

        var idx = new IndexInfo("RDB$PRIMARYKEY1", false, ConstraintsKind.Primary, "", [col1.ColumName], table.TableName, "", "");
        var idxVm = new IndexViewModel(idx);

        table.Indexs.Add(idxVm);

        var fpIdx1 = new IndexInfo("RDB$PRIMARYKEY2", false, ConstraintsKind.Primary, "", ["M_COL1"], "MASTER", "", "");
        var fpIdxVm1 = new IndexViewModel(fpIdx1);


        var idx1 = new IndexInfo("RDB$FOREIGNKEY1", false, ConstraintsKind.Foreign, "RDB$PRIMARYKEY2", [col2.ColumName], table.TableName, "", "");
        var idxVm1 = new IndexViewModel(idx1);

        table.Indexs.Add(idxVm1);

        var fpIdx2 = new IndexInfo("RDB$PRIMARYKEY3", false, ConstraintsKind.Primary, "", ["M_COL2"], "MASTER", "", "");
        var fpIdxVm2 = new IndexViewModel(fpIdx2);

        var idx2 = new IndexInfo("C_FOREIGNKEY", false, ConstraintsKind.Foreign, "RDB$PRIMARYKEY3", [col3.ColumName], table.TableName, "CASCADE", "SET DEFAULT");
        var idxVm2 = new IndexViewModel(idx2);

        table.Indexs.Add(idxVm2);
        dbVm.SetIndexes([idxVm, fpIdxVm1, idxVm1, fpIdxVm2, idxVm2]);

        table.GetDdl(dbVm).Is(
"CREATE TABLE TEST (" + Environment.NewLine +
"  COL1 INTEGER NOT NULL," + Environment.NewLine +
"  COL2 VARCHAR(100) NOT NULL," + Environment.NewLine +
"  COL3 TIMESTAMP NOT NULL," + Environment.NewLine +
"  PRIMARY KEY (COL1)," + Environment.NewLine +
"  FOREIGN KEY (COL2) REFERENCES MASTER (M_COL1)," + Environment.NewLine +
"  CONSTRAINT C_FOREIGNKEY FOREIGN KEY (COL3) REFERENCES MASTER (M_COL2) ON DELETE SET DEFAULT ON UPDATE CASCADE" + Environment.NewLine +
")");
    }

    [TestMethod()]
    public void GetDdlTest_UniqueWithConstraint()
    {
        var dbVm = DbViewModel.CreateDatabaseForTest();
        var table = new TableViewModel("TEST");
        dbVm.Tables.Add(table);

        var col1 = new ColumInfo("COL1", new FieldType(8, null, null, null, null, 4), new ColumConstraintsInfo(ConstraintsKind.Primary), "RDB$1", false, true, "");
        table.Colums.Add(new ColumViewMoodel(col1));
        var col2 = new ColumInfo("COL2", new FieldType(37, null, 100, null, null, 400), new ColumConstraintsInfo(ConstraintsKind.Foreign), "RDB$2", false, true, "");
        table.Colums.Add(new ColumViewMoodel(col2));
        var col3 = new ColumInfo("COL3", new FieldType(35, null, null, null, null, 8), new ColumConstraintsInfo(ConstraintsKind.Foreign), "RDB$3", false, true, "");
        table.Colums.Add(new ColumViewMoodel(col3));

        var idx = new IndexInfo("RDB$PRIMARYKEY1", false, ConstraintsKind.Primary, "", [col1.ColumName], table.TableName, "", "");
        var idxVm = new IndexViewModel(idx);
        table.Indexs.Add(idxVm);

        var uniqueIdx = new IndexInfo("RDB$13", true, ConstraintsKind.Unique, "", [col2.ColumName, col3.ColumName], table.TableName, "", "");
        var uniqueIdxVm = new IndexViewModel(uniqueIdx);
        table.Indexs.Add(uniqueIdxVm);
        dbVm.SetIndexes([idxVm, uniqueIdxVm]);

        table.GetDdl(dbVm).Is(
"CREATE TABLE TEST (" + Environment.NewLine +
"  COL1 INTEGER NOT NULL," + Environment.NewLine +
"  COL2 VARCHAR(100) NOT NULL," + Environment.NewLine +
"  COL3 TIMESTAMP NOT NULL," + Environment.NewLine +
"  PRIMARY KEY (COL1)," + Environment.NewLine +
"  UNIQUE (COL2, COL3)" + Environment.NewLine +
")");
    }

}