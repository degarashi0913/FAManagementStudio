using FAManagementStudio.Common;
using FirebirdSql.Data.FirebirdClient;
using System;
using System.Collections.ObjectModel;

namespace FAManagementStudio.Models
{
    public class DatabaseInfo : BindableBase
    {
        public string Path
        {
            get; set;
        }
        public string UserId { get; set; }
        public string Password { get; set; }

        public string DisplayName { get { return Path.Substring(Path.LastIndexOf('\\') + 1); } }

        internal string ConnectionString { get; private set; }

        public ObservableCollection<TableInfo> Chiled { get; } = new ObservableCollection<TableInfo>();

        public ObservableCollection<TriggerInfo> Trrigers { get; } = new ObservableCollection<TriggerInfo>();

        public void CreateDatabase(string path)
        {
            this.Path = path;
            var builder = new FbConnectionStringBuilder();
            builder.DataSource = "localhost";
            builder.Database = path;
            builder.Charset = FbCharset.Utf8.ToString();
            builder.UserID = "SYSDBA";
            builder.Password = "masterkey";
            builder.ServerType = FbServerType.Embedded;
            builder.Pooling = false;

            FbConnection.CreateDatabase(builder.ConnectionString);
        }
        public void LoadDatabase(string path)
        {
            this.Path = path;

            var builder = new FbConnectionStringBuilder();
            builder.DataSource = "localhost";
            builder.Database = path;
            builder.Charset = FbCharset.Utf8.ToString();
            builder.UserID = "SYSDBA";
            builder.Password = "masterkey";
            builder.ServerType = FbServerType.Embedded;
            builder.Pooling = false;

            ConnectionString = builder.ConnectionString;
            using (var con = new FbConnection(builder.ConnectionString))
            using (var command = con.CreateCommand())
            {
                command.CommandText = @"select rdb$relation_name AS Name from rdb$relations where rdb$relation_type = 0 and rdb$system_flag = 0 order by rdb$relation_name asc";
                con.Open();
                var reader = command.ExecuteReader();
                Chiled.Clear();
                while (reader.Read())
                {
                    var table = new TableInfo(((string)reader["Name"]).TrimEnd());
                    Chiled.Add(table);
                    table.GetColums(con);
                }
            }
            GetTrigger();
        }

        private void GetTrigger()
        {
            using (var con = new FbConnection(this.ConnectionString))
            using (var command = con.CreateCommand())
            {
                command.CommandText = @"select rdb$trigger_name Name, rdb$relation_name TableName, rdb$trigger_source Source from rdb$triggers where rdb$system_flag = 0";
                con.Open();
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Trrigers.Add(new TriggerInfo((string)reader["Name"], (string)reader["TableName"], (string)reader["Source"]));
                }
            }
        }
    }

    public class TriggerInfo
    {
        public TriggerInfo(string name, string tableName, string source)
        {
            Name = name;
            TableName = tableName;
            Source = source;
        }
        public string Name { get; set; }
        public string Source { get; set; }
        public string TableName { get; set; }
    }

    public class TableInfo
    {
        public TableInfo(string name)
        {
            this.TableName = name;
        }
        public string TableName { get; set; }
        public string DisplayName { get { return TableName; } }

        public ObservableCollection<ColumInfo> Chiled { get; set; } = new ObservableCollection<ColumInfo>();

        public void GetColums(FbConnection con)
        {
            using (var command = con.CreateCommand())
            {
                command.CommandText =
                    $"select rf.rdb$field_name Name, f.rdb$field_type Type, f.rdb$character_length CharSize, ky.rdb$constraint_type Key " +
                        "from rdb$relation_fields rf " +
                        "join rdb$relations r on rf.rdb$relation_name = r.rdb$relation_name " +
                                            "and r.rdb$view_blr is null " +
                                            "and rdb$relation_type = 0 and r.rdb$system_flag = 0 " +
                        "join rdb$fields f on f.rdb$field_name = rf.rdb$field_source " +
                        "left outer join (select rel.rdb$relation_name, seg.rdb$field_name, rel.rdb$constraint_type " +
                                            "from rdb$relation_constraints rel " +
                                            "left outer join  rdb$indices  idx on rel.rdb$index_name = idx.rdb$index_name " +
                                            "left outer join rdb$index_segments seg on idx.rdb$index_name = seg.rdb$index_name " +
                                            "where rel.rdb$constraint_type = 'PRIMARY KEY' or rel.rdb$constraint_type = 'FOREIGN KEY') ky " +
                         "on ky.rdb$relation_name = rf.rdb$relation_name and  ky.rdb$field_name = rf.rdb$field_name " +
                     $"where rf.rdb$relation_name = '{this.TableName}' " +
                      "order by rf.rdb$field_position; ";
                var reader = command.ExecuteReader();
                Chiled.Clear();
                while (reader.Read())
                {
                    var key = (reader["Key"] == DBNull.Value) ? "" : (string)reader["Key"];
                    var size = (reader["CharSize"] == DBNull.Value) ? null : (short?)reader["CharSize"];
                    Chiled.Add(new ColumInfo(((string)reader["Name"]).TrimEnd(), (short)reader["Type"], size, GetKey(key)));
                }
            }
        }

        public ConstraintsKeyKind GetKey(string keyName)
        {
            if (string.IsNullOrEmpty(keyName)) return ConstraintsKeyKind.None;
            if (keyName == "PRIMARY KEY")
            {
                return ConstraintsKeyKind.Primary;
            }
            else if (keyName == "FOREIGN KEY")
            {
                return ConstraintsKeyKind.Foreign;
            }
            //not define UNIQUE, CHECK , NOT NULL
            return ConstraintsKeyKind.None;
        }
    }



    public class ColumInfo
    {
        public string ColumName { get; set; }
        public string ColumType { get; set; }
        public string DisplayName { get { return $"{ColumName} ({ColumType})"; } }
        public ConstraintsKeyKind KeyKind { get; set; }

        public ColumInfo(string name, int type, short? size, ConstraintsKeyKind keyKind)
        {
            ColumName = name;
            ColumType = GetTypeFromFirebirdType(type) + (size.HasValue ? $"({size.ToString()})" : "");
            KeyKind = keyKind;
        }
        public string GetTypeFromFirebirdType(int i)
        {
            switch (i)
            {
                case 7:
                    return "SMALLINT";
                case 8:
                    return "INTEGER";
                case 9:
                    return "QUAD";
                case 10:
                    return "FLOAT";
                case 11:
                    return "D_FLOAT";
                case 12:
                    return "DATE";
                case 13:
                    return "TIME";
                case 14:
                    return "CHAR";
                case 16:
                    return "INT64";
                case 17:
                    return "BOOLEAN";
                case 27:
                    return "DOUBLE";
                case 35:
                    return "TIMESTAMP";
                case 37:
                    return "VARCHAR";
                case 40:
                    return "CSTRING";
                case 45:
                    return "BLOB_ID";
                case 261:
                    return "BLOB";
                default:
                    return "";
            }
        }
    }
}
