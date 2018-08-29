﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace FAManagementStudio.Models.Tests
{
    [TestClass()]
    public class QueryAnalyzerTests
    {
        [TestMethod()]
        public void GetStatementTest()
        {
            var input1 = "select * from A";
            QueryAnalyzer.Analyze(input1).IsStructuralEqual(new[] { "select * from A" });

            var input2 = "select * from A;";
            QueryAnalyzer.Analyze(input2).IsStructuralEqual(new[] { "select * from A;" });

            var input3 = "select * from A; select * from B";
            QueryAnalyzer.Analyze(input3).IsStructuralEqual(new[] { "select * from A;", "select * from B" });

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
            QueryAnalyzer.Analyze(input4).IsStructuralEqual(new[] {
                  @"create trigger set_foo_primary for foo" + Environment.NewLine +
                      @"before insert" + Environment.NewLine +
                      @"" + Environment.NewLine +
                      @"as begin" + Environment.NewLine +
                      @"" + Environment.NewLine +
                      @"new.a = gen_id(gen_foo, 1);" + Environment.NewLine +
                      @"end" + Environment.NewLine +
                      @";",
                "select* from A;", "create table V(a integer, b nvarchar(5))" });
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

            QueryAnalyzer.Analyze(input1).IsStructuralEqual(new[] {
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
});
        }

        [TestMethod()]
        public void GetStatementTest3()
        {
            var input1 =
@"--comment1
select * from fuga where hoho = 'eeee' --comment2
--comment3";

            QueryAnalyzer.Analyze(input1).IsStructuralEqual(new[] {
@"--comment1",
@"select * from fuga where hoho = 'eeee' --comment2
--comment3",});
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
            QueryAnalyzer.Analyze(input).IsStructuralEqual(new[] {
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
});
        }
    }
}