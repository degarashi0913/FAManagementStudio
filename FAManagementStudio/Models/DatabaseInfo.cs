using FAManagementStudio.Common;
using FirebirdSql.Data.FirebirdClient;
using System;
using System.Collections.Generic;

namespace FAManagementStudio.Models
{
    public class DatabaseInfo : BindableBase
    {
        public string Path { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }

        internal string ConnectionString { get; private set; }

        public void CreateDatabase(FbConnection con)
        {
            FbConnection.CreateDatabase(con.ConnectionString);
        }
        public IEnumerable<TableInfo> GetTables(FbConnection con)
        {
            ConnectionString = con.ConnectionString;
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

    public class IndexInfo
    {
        public IndexInfo() { }
        public string Name { get; set; }
        public ConstraintsKind Kind { get; set; }
        public string ForigenKeyName { get; set; }
        public List<string> FieldNames { get; } = new List<string>();
        public string TableName { get; set; }

    }

    public class TableInfo
    {
        public TableInfo(string name)
        {
            this.TableName = name;
        }
        public string TableName { get; set; }

        public IEnumerable<ColumInfo> GetColums(FbConnection con)
        {
            using (var command = con.CreateCommand())
            {
                command.CommandText =
                    $"select rf.rdb$field_name Name, f.rdb$field_type Type, f.rdb$field_sub_type SubType , f.rdb$character_length CharSize, ky.rdb$constraint_type ConstraintType, rf.rdb$field_source FieldSource, rf.rdb$null_flag NullFlag " +
                        "from rdb$relation_fields rf " +
                        "join rdb$relations r on rf.rdb$relation_name = r.rdb$relation_name " +
                                            "and r.rdb$view_blr is null " +
                                            "and rdb$relation_type = 0 and r.rdb$system_flag = 0 " +
                        "join rdb$fields f on f.rdb$field_name = rf.rdb$field_source " +
                        "left outer join (select rel.rdb$relation_name, seg.rdb$field_name, rel.rdb$constraint_type " +
                                            "from rdb$relation_constraints rel " +
                                            "left outer join  rdb$indices  idx on rel.rdb$index_name = idx.rdb$index_name " +
                                            "left outer join rdb$index_segments seg on idx.rdb$index_name = seg.rdb$index_name) ky " +
                         "on ky.rdb$relation_name = rf.rdb$relation_name and  ky.rdb$field_name = rf.rdb$field_name " +
                     $"where rf.rdb$relation_name = '{this.TableName}' " +
                      "order by rf.rdb$field_position; ";
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var key = (reader["ConstraintType"] == DBNull.Value) ? "" : (string)reader["ConstraintType"];
                    var size = (reader["CharSize"] == DBNull.Value) ? null : (short?)reader["CharSize"];
                    var subType = (reader["SubType"] == DBNull.Value) ? null : (short?)reader["SubType"];
                    var nullFlag = reader["NullFlag"] == DBNull.Value;
                    yield return new ColumInfo(((string)reader["Name"]).TrimEnd(), (short)reader["Type"], subType, size, GetConstraint(key), ((string)reader["FieldSource"]).TrimEnd(), nullFlag);
                }
            }
        }

        private ConstraintsKind GetConstraint(string input)
        {
            var name = input.Trim();
            if (string.IsNullOrEmpty(name)) return ConstraintsKind.None;
            if (name == "PRIMARY KEY")
            {
                return ConstraintsKind.Primary;
            }
            else if (name == "FOREIGN KEY")
            {
                return ConstraintsKind.Foreign;
            }
            else if (name == "UNIQUE")
            {
                return ConstraintsKind.Unique;
            }
            else if (name == "CHECK")
            {
                return ConstraintsKind.Check;
            }
            else if (name == "NOT NULL")
            {
                return ConstraintsKind.NotNull;
            }
            return ConstraintsKind.None;
        }

        public IEnumerable<TriggerInfo> GetTrigger(FbConnection con)
        {
            using (var command = con.CreateCommand())
            {
                command.CommandText = $"select rdb$trigger_name Name, rdb$relation_name TableName, rdb$trigger_source Source from rdb$triggers where rdb$relation_name = '{this.TableName}' and rdb$system_flag = 0";
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    yield return new TriggerInfo((string)reader["Name"], (string)reader["TableName"], (string)reader["Source"]);
                }
            }
        }

        public IEnumerable<IndexInfo> GetIndex(FbConnection con)
        {
            using (var command = con.CreateCommand())
            {
                command.CommandText =
                    $"select idx.rdb$index_name Name, seg.rdb$field_name FiledName, constrain.rdb$constraint_type ConstraintType, rdb$foreign_key ForeignKey from rdb$indices idx " +
                    $"left outer join rdb$index_segments seg on idx.rdb$index_name = seg.rdb$index_name " +
                    $"left outer join rdb$relation_constraints constrain on idx.rdb$index_name = constrain.rdb$index_name " +
                    $"where idx.rdb$relation_name = '{this.TableName}' and idx.rdb$system_flag = 0 order by seg.rdb$field_position";
                var reader = command.ExecuteReader();
                var tmpName = "";
                IndexInfo tmpInf = null;
                while (reader.Read())
                {
                    if (tmpName == ((string)reader["Name"]).Trim())
                    {
                        tmpInf.FieldNames.Add(((string)reader["FiledName"]).Trim());
                    }
                    else
                    {
                        if (tmpInf != null)
                        {
                            tmpName = "";
                            yield return tmpInf;
                        }
                        tmpInf = new IndexInfo();
                        tmpName = ((string)reader["Name"]).Trim();
                        tmpInf.Name = tmpName;
                        tmpInf.Kind = GetConstraint((string)reader["ConstraintType"]);
                        tmpInf.ForigenKeyName = tmpInf.Kind == ConstraintsKind.Foreign ? (string)reader["ForeignKey"] : "";
                        tmpInf.TableName = this.TableName;
                        tmpInf.FieldNames.Add(((string)reader["FiledName"]).Trim());
                    }
                }
                if (tmpInf != null)
                {
                    yield return tmpInf;
                }
            }
        }
    }



    public class ColumInfo
    {
        public string ColumName { get; set; }
        public string ColumType { get; set; }
        public string DomainName { get; set; }
        public bool NullFlag { get; set; }

        public ConstraintsKind KeyKind { get; set; }

        public ColumInfo(string name, short type, short? subSype, short? size, ConstraintsKind keyKind, string domainName, bool nullFlag)
        {
            ColumName = name;
            ColumType = GetTypeFromFirebirdType(type, subSype) + (size.HasValue ? $"({size.ToString()})" : "");
            KeyKind = keyKind;
            DomainName = domainName;
            NullFlag = nullFlag;
        }

        public string GetTypeFromFirebirdType(short type, short? subType)
        {
            switch (type)
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
                    return "BIGINT";
                case 17:
                    return "BOOLEAN";
                case 27:
                    return "DOUBLE PRECISION";
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
