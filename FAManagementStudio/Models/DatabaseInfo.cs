using FAManagementStudio.Common;
using FirebirdSql.Data.FirebirdClient;
using System;
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
            var constraints = GetConstrains(con);

            using (var command = con.CreateCommand())
            {
                command.CommandText =
                    $"select rf.rdb$field_name Name, f.rdb$field_type Type, f.rdb$field_sub_type SubType , f.rdb$character_length CharSize, rf.rdb$field_source FieldSource, rf.rdb$null_flag NullFlag, f.rdb$field_precision FieldPrecision, f.rdb$field_scale FieldScale " +
                        "from rdb$relation_fields rf " +
                        "join rdb$relations r on rf.rdb$relation_name = r.rdb$relation_name " +
                                            "and r.rdb$view_blr is null " +
                                            "and r.rdb$relation_type = 0 and r.rdb$system_flag = 0 " +
                        "join rdb$fields f on f.rdb$field_name = rf.rdb$field_source " +
                    $"where rf.rdb$relation_name = '{this.TableName}' " +
                     "order by rf.rdb$field_position; ";
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var name = ((string)reader["Name"]).TrimEnd();
                    var size = (reader["CharSize"] == DBNull.Value) ? null : (short?)reader["CharSize"];
                    var subType = (reader["SubType"] == DBNull.Value) ? null : (short?)reader["SubType"];
                    var nullFlag = reader["NullFlag"] == DBNull.Value;
                    var precision = (reader["FieldPrecision"] == DBNull.Value) ? null : (short?)reader["FieldPrecision"];
                    var scale = (reader["FieldScale"] == DBNull.Value) ? null : (short?)reader["FieldScale"];
                    var type = new FieldType((short)reader["Type"], subType, size, precision, scale);

                    var constraintInfo = new ConstraintsInfo();
                    if (constraints.ContainsKey(name))
                    {
                        constraintInfo = constraints[name];
                    }

                    yield return new ColumInfo(name, type, constraintInfo, ((string)reader["FieldSource"]).TrimEnd(), nullFlag);
                }
            }
        }

        private Dictionary<string, ConstraintsInfo> GetConstrains(FbConnection con)
        {
            var dic = new Dictionary<string, ConstraintsInfo>();
            using (var command = con.CreateCommand())
            {
                command.CommandText =
                    $"select seg.rdb$field_name Name, rel.rdb$constraint_type Type " +
                     "from rdb$relation_constraints rel " +
                     "left outer join rdb$indices idx on rel.rdb$index_name = idx.rdb$index_name " +
                     "left outer join rdb$index_segments seg on idx.rdb$index_name = seg.rdb$index_name " +
                    $"where rel.rdb$relation_name = '{this.TableName}' and seg.rdb$field_name != '' ";
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var name = ((string)reader["Name"]).TrimEnd();
                    ConstraintsInfo inf;
                    if (!dic.TryGetValue(name, out inf))
                    {
                        inf = new ConstraintsInfo();
                        dic.Add(name, inf);
                    }
                    inf.SetKind(GetConstraintType((string)reader["Type"]));
                }
            }
            return dic;
        }

        private ConstraintsKind GetConstraintType(string input)
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
                        tmpInf.Kind = GetConstraintType((string)reader["ConstraintType"]);
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

    public class ViewInfo
    {
        public ViewInfo(string name, string source)
        {
            this.ViewName = name;
            this.Source = source;
        }
        public string ViewName { get; set; }

        public string Source { get; set; }
        public IEnumerable<ColumInfo> GetColums(FbConnection con)
        {
            using (var command = con.CreateCommand())
            {
                command.CommandText =
                    $"select rf.rdb$field_name Name, f.rdb$field_type Type, f.rdb$field_sub_type SubType , f.rdb$character_length CharSize, rf.rdb$field_source FieldSource, rf.rdb$null_flag NullFlag, f.rdb$field_precision FieldPrecision, f.rdb$field_scale FieldScale " +
                        "from rdb$relation_fields rf " +
                        "join rdb$relations r on rf.rdb$relation_name = r.rdb$relation_name " +
                                            "and r.rdb$view_blr is not null " +
                                            "and r.rdb$relation_type = 1 and r.rdb$system_flag = 0 " +
                        "join rdb$fields f on f.rdb$field_name = rf.rdb$field_source " +
                     $"where rf.rdb$relation_name = '{this.ViewName}' " +
                      "order by rf.rdb$field_position; ";
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var size = (reader["CharSize"] == DBNull.Value) ? null : (short?)reader["CharSize"];
                    var subType = (reader["SubType"] == DBNull.Value) ? null : (short?)reader["SubType"];
                    var nullFlag = reader["NullFlag"] == DBNull.Value;
                    var precision = (reader["FieldPrecision"] == DBNull.Value) ? null : (short?)reader["FieldPrecision"];
                    var scale = (reader["FieldScale"] == DBNull.Value) ? null : (short?)reader["FieldScale"];
                    var type = new FieldType((short)reader["Type"], subType, size, precision, scale);

                    yield return new ColumInfo(((string)reader["Name"]).TrimEnd(), type, null, ((string)reader["FieldSource"]).TrimEnd(), nullFlag);
                }
            }
        }

    }

    public class FieldType
    {
        public short Type { get; set; }
        public short? FieldSubType { get; set; }
        public short? CharactorLength { get; set; }
        public short? FieldPrecision { get; set; }
        public short? FieldScale { get; set; }

        public FieldType(short type, short? subType, short? cLength, short? precision, short? scale)
        {
            Type = type;
            FieldSubType = subType;
            CharactorLength = cLength;
            FieldPrecision = precision;
            FieldScale = scale;
        }

        public override string ToString()
        {
            return GetTypeFromFirebirdType(Type, FieldSubType, CharactorLength, FieldPrecision, FieldScale);
        }

        private string GetFixedPointDataType(string typeName, short? subType, short? precision, short? scale)
        {
            if (subType.HasValue && subType != 0)
            {
                var fixedPoint = $"({precision}";
                if (scale.HasValue && scale != 0)
                {
                    fixedPoint += $",{-scale}";
                }
                fixedPoint += ")";

                if (subType == 1) return $"NUMERIC{fixedPoint}";
                if (subType == 2) return $"DECIMAL{fixedPoint}";
            }
            return typeName;
        }

        private string GetTypeFromFirebirdType(short type, short? subType, short? cLength, short? precision, short? scale)
        {
            switch (type)
            {
                case 7:
                    return GetFixedPointDataType("SMALLINT", subType, precision, scale);
                case 8:
                    return GetFixedPointDataType("INTEGER", subType, precision, scale);
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
                    return $"CHAR({cLength})";
                case 16:
                    return GetFixedPointDataType("BIGINT", subType, precision, scale);
                case 17:
                    return "BOOLEAN";
                case 27:
                    return "DOUBLE PRECISION";
                case 35:
                    return "TIMESTAMP";
                case 37:
                    return $"VARCHAR({cLength})";
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

    public class ColumInfo
    {
        public string ColumName { get; set; }
        public FieldType ColumType { get; set; }
        public string DomainName { get; set; }
        public bool NullFlag { get; set; }

        private ConstraintsInfo _inf;

        public ConstraintsKind KeyKind { get { return _inf.Kind; } }

        public ColumInfo(string name, FieldType type, ConstraintsInfo inf, string domainName, bool nullFlag)
        {
            ColumName = name;
            ColumType = type;
            _inf = inf;
            DomainName = domainName;
            NullFlag = nullFlag;
        }
    }

    public class ConstraintsInfo
    {
        public ConstraintsInfo() { }
        public ConstraintsInfo(ConstraintsKind kind)
        {
            Kind = kind;
        }

        public ConstraintsKind Kind { get; private set; } = ConstraintsKind.None;
        public void SetKind(ConstraintsKind kind)
        {
            Kind |= kind;
        }
    }
}
