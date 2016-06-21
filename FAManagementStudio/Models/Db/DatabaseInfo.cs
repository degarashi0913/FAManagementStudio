using FAManagementStudio.Common;
using FirebirdSql.Data.FirebirdClient;
using System.Collections.Generic;

namespace FAManagementStudio.Models
{
    public class DatabaseInfo : BindableBase
    {
        private string _path;
        public string Path
        {
            get
            {
                return _path;
            }
            set
            {
                _path = value;
                _builder.Database = value;
            }
        }
        public string UserId { get; set; }
        public string Password { get; set; }

        internal string ConnectionString
        {
            get
            {
                return _builder.ConnectionString;
            }
        }

        private FbConnectionStringBuilder _builder = new FbConnectionStringBuilder()
        {
            DataSource = "localhost",
            Charset = FbCharset.Utf8.ToString(),
            UserID = "SYSDBA",
            Password = "masterkey",
            ServerType = FbServerType.Embedded,
            Pooling = false
        };

        public FbConnectionStringBuilder Builder { get { return _builder; } }

        public void CreateDatabase(FbConnection con)
        {
            FbConnection.CreateDatabase(con.ConnectionString);
        }
        public IEnumerable<TableInfo> GetTables(FbConnection con)
        {
            using (var command = con.CreateCommand())
            {
                command.CommandText = @"select rdb$relation_name AS Name from rdb$relations where rdb$relation_type = 0 and rdb$system_flag = 0 order by rdb$relation_name asc";
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    yield return new TableInfo(((string)reader["Name"]).TrimEnd());
                }
            }
        }
        public IEnumerable<ViewInfo> GetViews(FbConnection con)
        {
            using (var command = con.CreateCommand())
            {
                command.CommandText = @"select rdb$relation_name AS Name, rdb$view_source Source from rdb$relations where rdb$relation_type = 1 and rdb$system_flag = 0 order by rdb$relation_name asc";
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    yield return new ViewInfo(((string)reader["Name"]).TrimEnd(), ((string)reader["Source"]).Trim(' ', '\r', '\n'));
                }
            }
        }
    }
}
