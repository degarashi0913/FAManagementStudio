using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FAManagementStudio.Models.Tests
{
    [TestClass()]
    public class QueryAnalyzerTests
    {
        [TestMethod()]
        public void GetStatementTest()
        {
            var input1 = "select * from A";
            IReadOnlyCollection<string> expected1 = ["select * from A"];
            QueryAnalyzer.Analyze(input1).SequenceEqual(expected1);

            var input2 = "select * from A;";
            IReadOnlyCollection<string> expected2 = ["select * from A;"];
            QueryAnalyzer.Analyze(input2).SequenceEqual(expected2);

            var input3 = "select * from A; select * from B";
            IReadOnlyCollection<string> expected3 = ["select * from A;", "select * from B"];
            QueryAnalyzer.Analyze(input3).SequenceEqual(expected3);

            var input4 = @"" + Environment.NewLine +
                @" create trigger set_foo_primary for foo" + Environment.NewLine +
                @"before insert" + Environment.NewLine +
                @"" + Environment.NewLine +
                @"as begin" + Environment.NewLine +
                @"" + Environment.NewLine +
                @"new.a = gen_id(gen_foo, 1);" + Environment.NewLine +
                @"end" + Environment.NewLine +
                @";" + Environment.NewLine +
                @"  select* from A;" + Environment.NewLine +
                @" create table V(a integer, b nvarchar(5))";
            IReadOnlyCollection<string> expected4 = [
                @"create trigger set_foo_primary for foo" + Environment.NewLine +
                @"before insert" + Environment.NewLine +
                @"" + Environment.NewLine +
                @"as begin" + Environment.NewLine +
                @"" + Environment.NewLine +
                @"new.a = gen_id(gen_foo, 1);" + Environment.NewLine +
                @"end" + Environment.NewLine +
                @";",
                "select* from A;", "create table V(a integer, b nvarchar(5))"];
            QueryAnalyzer.Analyze(input4).SequenceEqual(expected4);
        }

        [TestMethod()]
        public void GetStatementTest2()
        {
            var input1 =
@"--comment1
create trigger set_foo_primary for foo
before insert
as begin
new.a = gen_id(gen_foo, 1);
end;

--comment2  
select *
from test
where a = 1;

--comment3
create trigger set_foo_primary for foo2
before insert
as begin
new.a = gen_id(gen_foo, 1);
end;

select * from fuga where hoho = 'eeee'
--comment4";

            IReadOnlyCollection<string> expected1 = [
@"--comment1",
@"create trigger set_foo_primary for foo
before insert
as begin
new.a = gen_id(gen_foo, 1);
end;",
@"--comment2  ",
@"select *
from test
where a = 1;",
@"--comment3",
@"create trigger set_foo_primary for foo2
before insert
as begin
new.a = gen_id(gen_foo, 1);
end;",
@"select * from fuga where hoho = 'eeee'
--comment4"
                ];

            QueryAnalyzer.Analyze(input1).SequenceEqual(expected1);
        }

        [TestMethod()]
        public void GetStatementTest3()
        {
            var input1 =
@"--comment1
select * from fuga where hoho = 'eeee' --comment2
--comment3";
            IReadOnlyCollection<string> expected1 = [
@"--comment1",
@"select * from fuga where hoho = 'eeee' --comment2
--comment3"];
            QueryAnalyzer.Analyze(input1).SequenceEqual(expected1);
        }

        [TestMethod()]
        public void GetStatementTest4()
        {
            var input =
@"-- ExecuteSample1
execute block
as
declare i int = 0;
begin
  while (i < 128) do
  begin
    insert into AsciiTable values (:i, ascii_char(:i));
    i = i + 1;
  end end;

--  ExecuteSample2
execute block (x double precision = ?, y double precision = ?)
returns (gmean double precision)
as
begin
  gmean = sqrt(x*y);
  suspend;
end;";
            IReadOnlyCollection<string> expected1 = [
@"-- ExecuteSample1",
@"execute block
as
declare i int = 0;
begin
  while (i < 128) do
  begin
    insert into AsciiTable values (:i, ascii_char(:i));
    i = i + 1;
  end end;",
@"--  ExecuteSample2",
@"execute block (x double precision = ?, y double precision = ?)
returns (gmean double precision)
as
begin
  gmean = sqrt(x*y);
  suspend;
end;"
                ];

            QueryAnalyzer.Analyze(input).SequenceEqual(expected1);
        }

        [TestMethod()]
        public void GetStatementTest5()
        {
            var input =
"""
-- ExecuteSample1
CREATE TRIGGER BI_ORDER_ID FOR ""ORDER""
ACTIVE BEFORE INSERT POSITION 0
AS BEGIN
  IF (NEW.ORDER_ID IS NULL) THEN
    NEW.ORDER_ID = NEXT VALUE FOR SEQ_ORDER_ID;
END;

-- ExecuteSample2
CREATE PROCEDURE CREATE_ORDER (
    IN_CUSTOMER_ID INTEGER,
    IN_TOTAL_AMOUNT DECIMAL(10,2)
)
AS
DECLARE VARIABLE NEW_ORDER_ID INTEGER;
BEGIN
  NEW_ORDER_ID = NEXT VALUE FOR SEQ_ORDER_ID;
  INSERT INTO ""ORDER"" (ORDER_ID, CUSTOMER_ID, ORDER_DATE, TOTAL_AMOUNT, CREATED_AT)
  VALUES (:NEW_ORDER_ID, :IN_CUSTOMER_ID, CURRENT_DATE, :IN_TOTAL_AMOUNT, CURRENT_TIMESTAMP);
END;
""";

            IReadOnlyCollection<string> expected = [
"-- ExecuteSample1",
"""
CREATE TRIGGER BI_ORDER_ID FOR ""ORDER""
ACTIVE BEFORE INSERT POSITION 0
AS BEGIN
  IF (NEW.ORDER_ID IS NULL) THEN
    NEW.ORDER_ID = NEXT VALUE FOR SEQ_ORDER_ID;
END;
""",
"-- ExecuteSample2",
"""
CREATE PROCEDURE CREATE_ORDER (
    IN_CUSTOMER_ID INTEGER,
    IN_TOTAL_AMOUNT DECIMAL(10,2)
)
AS
DECLARE VARIABLE NEW_ORDER_ID INTEGER;
BEGIN
  NEW_ORDER_ID = NEXT VALUE FOR SEQ_ORDER_ID;
  INSERT INTO ""ORDER"" (ORDER_ID, CUSTOMER_ID, ORDER_DATE, TOTAL_AMOUNT, CREATED_AT)
  VALUES (:NEW_ORDER_ID, :IN_CUSTOMER_ID, CURRENT_DATE, :IN_TOTAL_AMOUNT, CURRENT_TIMESTAMP);
END;
"""];

            QueryAnalyzer.Analyze(input).SequenceEqual(expected);
        }
    }
}