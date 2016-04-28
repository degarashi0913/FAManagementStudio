using FirebirdSql.Data.FirebirdClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;

namespace FAManagementStudio.Models.Tests
{
    [TestClass()]
    public class QueryInfoTests
    {
        [TestMethod()]
        public void GetWordTest()
        {
            var inf = new QueryInfo();
            var input = @"select * from test t where t.Hoge='' and t.Fuga = '' ";
            //idx = 0, length = 0
            (inf.AsDynamic().GetWord(input, 0, 0) as IEnumerable<string>).ToArray().IsStructuralEqual(new[] { "select", "*", "from", "test", "t", "where", "t.Hoge=''", "and", "t.Fuga", "=", "''" });
            var input2 = @"select * from test t where t.Hoge='' and t.Fuga = ''";
            (inf.AsDynamic().GetWord(input2, 0, 0) as IEnumerable<string>).ToArray().IsStructuralEqual(new[] { "select", "*", "from", "test", "t", "where", "t.Hoge=''", "and", "t.Fuga", "=", "''" });
            var input3 = @"select * from test t where t.Hoge='' and t.Fuga = '';";
            (inf.AsDynamic().GetWord(input3, 0, 0) as IEnumerable<string>).ToArray().IsStructuralEqual(new[] { "select", "*", "from", "test", "t", "where", "t.Hoge=''", "and", "t.Fuga", "=", "''", ";" });

            //idx = any, length = 0
            (inf.AsDynamic().GetWord(input, 1, 0) as IEnumerable<string>).ToArray().IsStructuralEqual(new[] { "elect", "*", "from", "test", "t", "where", "t.Hoge=''", "and", "t.Fuga", "=", "''" });
            (inf.AsDynamic().GetWord(input, 7, 0) as IEnumerable<string>).ToArray().IsStructuralEqual(new[] { "*", "from", "test", "t", "where", "t.Hoge=''", "and", "t.Fuga", "=", "''" });
            (inf.AsDynamic().GetWord(input, input.Length, 0) as IEnumerable<string>).ToArray().IsStructuralEqual(new string[] { });

            //idx = any, length = any
            (inf.AsDynamic().GetWord(input, 1, 2) as IEnumerable<string>).ToArray().IsStructuralEqual(new[] { "e" });
            (inf.AsDynamic().GetWord(input, 7, 28) as IEnumerable<string>).ToArray().IsStructuralEqual(new[] { "*", "from", "test", "t", "where", "t" });
            (inf.AsDynamic().GetWord(input, 0, input.Length) as IEnumerable<string>).ToArray().IsStructuralEqual(new[] { "select", "*", "from", "test", "t", "where", "t.Hoge=''", "and", "t.Fuga", "=", "''" });
            (inf.AsDynamic().GetWord(input, input.Length, input.Length) as IEnumerable<string>).ToArray().IsStructuralEqual(new string[] { });
        }

        [TestMethod()]
        public void GetStatementTest()
        {
            var inf = new QueryInfo();
            var input1 = "select * from A";
            (inf.AsDynamic().GetStatement(input1) as string[]).IsStructuralEqual(new[] { "select * from A" });

            var input2 = "select * from A;";
            (inf.AsDynamic().GetStatement(input2) as string[]).IsStructuralEqual(new[] { "select * from A" });

            var input3 = "select * from A; select * from B";
            (inf.AsDynamic().GetStatement(input3) as string[]).IsStructuralEqual(new[] { "select * from A", "select * from B" });

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
            (inf.AsDynamic().GetStatement(input4) as string[]).IsStructuralEqual(new[] {
                  @"create trigger set_foo_primary for foo" + Environment.NewLine +
                      @"before insert" + Environment.NewLine +
                      @"" + Environment.NewLine +
                      @"as begin" + Environment.NewLine +
                      @"" + Environment.NewLine +
                      @"new.a = gen_id(gen_foo, 1);" + Environment.NewLine +
                      @"end" ,
                "select* from A", "create table V(a integer, b nvarchar(5))" });
        }

        [TestMethod()]
        public void GetStatementTest2()
        {
            var inf = new QueryInfo();
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

            (inf.AsDynamic().GetStatement(input1) as string[]).IsStructuralEqual(new[] {
@"create trigger set_foo_primary for foo
before insert
as begin
new.a = gen_id(gen_foo, 1);
end",
@"select *
from test
where a = 1",
@"create trigger set_foo_primary for foo2
before insert
as begin
new.a = gen_id(gen_foo, 1);
end",
@"select * from fuga where hoho = 'eeee'"});
        }

        [TestMethod()]
        public void GetStatementTest3()
        {
            var inf = new QueryInfo();
            var input1 =
@"--comment1
select * from fuga where hoho = 'eeee' --comment2
--comment3";

            (inf.AsDynamic().GetStatement(input1) as string[]).IsStructuralEqual(new[] {
@"select * from fuga where hoho = 'eeee'"});
        }


        private void SetupTestDb()
        {
            FbConnection.CreateDatabase(GetConnectionString(), 4096, true, true);
            CreateTestTables(GetConnectionString());
        }

        private void CreateTestTables(string connectionString)
        {
            using (var con = new FbConnection(connectionString))
            using (var command = con.CreateCommand())
            {
                var sb = new StringBuilder();

                sb.Append("recreate table test(");
                sb.Append("int_test integer default 0 not null primary key,");
                sb.Append("bigint_test bigint,");
                sb.Append("blob_test blob,");
                sb.Append("char_test char(20),");
                sb.Append("date_test date,");
                sb.Append("decimal_test decimal,");
                sb.Append("double_test double precision,");
                sb.Append("float_test float,");
                sb.Append("numeric_test numeric,");
                sb.Append("smallint_test smallint,");
                sb.Append("time_test time,");
                sb.Append("timestamp_test timestamp,");
                sb.Append("varchar_test varchar(100)");
                sb.Append(")");

                command.CommandText = sb.ToString();
                con.Open();
                command.ExecuteNonQuery();
            }
        }

        private string GetConnectionString()
        {
            var builder = new FbConnectionStringBuilder();
            builder.DataSource = "localhost";
            builder.Database = @"tests.fdb";
            builder.Charset = FbCharset.Utf8.ToString();
            builder.UserID = "SYSDBA";
            builder.Password = "masterkey";
            builder.ServerType = FbServerType.Embedded;
            builder.Pooling = false;

            return builder.ToString();
        }

        [TestMethod]
        public void ExecuteQueryTest()
        {
            SetupTestDb();
            var inf = new QueryInfo();
            inf.ExecuteQuery(GetConnectionString(), "select * from test").ToList()[0].View.Rows.Count.Is(0);

            var result = inf.ExecuteQuery(GetConnectionString(), "insert into test(int_test, char_test) values (1, 'aaaaaaaaaa');update test set varchar_test = 'testtesttesttest' where int_test = 1;select * from test").ToList();
            result[0].View.Rows[0].Is(x => ((string)x[0]).Contains("実行しました。"));
            result[1].View.Rows[0].Is(x => ((string)x[0]).Contains("更新しました。"));
            result[2].View.Rows.Count.Is(1);
        }
    }
}