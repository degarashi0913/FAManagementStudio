using FAManagementStudio.Models;
using FirebirdSql.Data.FirebirdClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FAManagementStudioTests
{
    [TestClass]
    public class ConnectionTestFb2
    {

        private void SetupTestDbFb2()
        {
            FbConnection.CreateDatabase(GetFb2ConnectionString(), 4096, true, true);
            CreateTestTablesFb2(GetFb2ConnectionString());
        }

        private void CreateTestTablesFb2(string connectionString)
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

        private string GetFb2ConnectionString()
        {
            var builder = new FbConnectionStringBuilder();
            builder.DataSource = "localhost";
            builder.Database = @"TestFb2.fdb";
            builder.Charset = "UTF8";
            builder.UserID = "SYSDBA";
            builder.Password = "masterkey";
            builder.ServerType = FbServerType.Embedded;
            builder.ClientLibrary = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName),@"fb25\fbembed");
            builder.Pooling = false;

            return builder.ToString();
        }

        [TestMethod]
        public void ExecuteQueryTest()
        {
            SetupTestDbFb2();
            var inf = new QueryInfo(false);
            inf.ExecuteQuery(GetFb2ConnectionString(), "select * from test").ToList()[0].View.Rows.Count.Is(0);

            var result = inf.ExecuteQuery(GetFb2ConnectionString(), "insert into test(int_test, char_test) values (1, 'aaaaaaaaaa');update test set varchar_test = 'testtesttesttest' where int_test = 1;select * from test").ToList();
            result[0].View.Rows[0].Is(x => ((string)x[0]).Contains("実行しました。"));
            result[1].View.Rows[0].Is(x => ((string)x[0]).Contains("更新しました。"));
            result[2].View.Rows.Count.Is(1);
        }
    }
}
