using FAManagementStudio.Models;
using FirebirdSql.Data.FirebirdClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Text;

namespace FAManagementStudioTests
{
    [TestClass]
    public class ConnectionTestFb3
    {

        private void SetupTestDbFb3()
        {
            FbConnection.CreateDatabase(GetFb3ConnectionString(), overwrite: true);
            CreateTestTablesFb3(GetFb3ConnectionString());
        }

        private void CreateTestTablesFb3(string connectionString)
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
                sb.Append("varchar_test varchar(100),");
                sb.Append("boolean_test boolean");
                sb.Append(")");

                command.CommandText = sb.ToString();
                con.Open();
                command.ExecuteNonQuery();
            }
        }

        private string GetFb3ConnectionString()
        {
            var builder = new FbConnectionStringBuilder();
            builder.DataSource = "localhost";
            builder.Database = @"TestFb3.fdb";
            builder.Charset = "UTF8";
            builder.UserID = "SYSDBA";
            builder.Password = "masterkey";
            builder.ServerType = FbServerType.Embedded;
            builder.ClientLibrary = @"fb3\fbclient";
            builder.Pooling = false;

            return builder.ToString();
        }

        [TestMethod]
        public void ExecuteQueryTestFb3()
        {
            SetupTestDbFb3();
            var inf = new QueryInfo(false);
            var conStr = GetFb3ConnectionString();
            inf.ExecuteQuery(conStr, "select * from test").ToList()[0].View.Rows.Count.Is(0);

            var result = inf.ExecuteQuery(conStr, "insert into test values (1, 123456789000, null, '12asd', '2016-07-24', 50000, 2.5, 2.5555, 4500, 10, '10:00:00', current_timestamp, 'asdfghjk', true);update test set varchar_test = 'testtesttesttest' where int_test = 1;select * from test").ToList();
            result[0].View.Rows[0].Is(x => ((string)x[0]).Contains("実行しました。"));
            result[1].View.Rows[0].Is(x => ((string)x[0]).Contains("更新しました。"));
            result[2].View.Rows.Count.Is(1);
        }
    }
}
