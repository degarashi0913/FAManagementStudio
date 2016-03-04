using FAManagementStudio.Common;
using FirebirdSql.Data.FirebirdClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;

namespace FAManagementStudio.Models
{
    class DatabaseInfo : BindableBase
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
                command.CommandText = @"select rdb$relation_name AS Name from rdb$relations where rdb$view_source is null and rdb$system_flag = 0 order by rdb$relation_name asc";
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
        }
    }

    class TableInfo
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
                    $"select rf.rdb$field_name Name, f.rdb$field_type Type, f.rdb$field_length Size " +
                        "from rdb$relation_fields rf " +
                        "join rdb$relations r on rf.rdb$relation_name = r.rdb$relation_name " +
                            "and r.rdb$view_blr is null " +
                            "and(r.rdb$system_flag is null or r.rdb$system_flag = 0) " +
                        "join rdb$fields f on f.rdb$field_name = rf.rdb$field_source " +
                       $"where rf.rdb$relation_name = '{this.TableName}' " +
                        "order by rf.rdb$field_position; ";
                var reader = command.ExecuteReader();
                Chiled.Clear();
                while (reader.Read())
                {
                    Chiled.Add(new ColumInfo(((string)reader["Name"]).TrimEnd(), (short)reader["Type"], (short)reader["Size"]));
                }
            }
        }
    }

    public class ColumInfo
    {
        public string ColumName { get; set; }
        public string ColumType { get; set; }
        public string DisplayName { get { return $"{ColumName}({ColumType})"; } }
        public ColumInfo(string name, int type, int size)
        {
            ColumName = name;
            ColumType = GetTypeFromFirebirdType(type) + $"({size.ToString()})";
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
