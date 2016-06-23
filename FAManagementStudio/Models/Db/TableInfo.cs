using FAManagementStudio.Common;
using FirebirdSql.Data.FirebirdClient;
using System;
using System.Collections.Generic;

namespace FAManagementStudio.Models
{
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
                    "select seg.rdb$field_name Name, rel.rdb$constraint_type Type, idx.foreign_key_table ForeignKeyTable " +
                    "from rdb$relation_constraints rel " +
                    "left outer join( " +
                    "  select idx.rdb$index_name, idx.rdb$relation_name, idx2.rdb$relation_name foreign_key_table " +
                    "    from( " +
                    "    select idx.rdb$index_name, idx.rdb$relation_name, idx.rdb$foreign_key  from rdb$indices idx " +
                    "    ) idx " +
                    "    left outer join rdb$indices idx2 on idx.rdb$foreign_key = idx2.rdb$index_name " +
                    ") idx on  rel.rdb$index_name = idx.rdb$index_name " +
                    "left outer join rdb$index_segments seg on idx.rdb$index_name  = seg.rdb$index_name " +
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
                    var type = GetConstraintType((string)reader["Type"]);
                    inf.SetKind(type);
                    if (type == ConstraintsKind.Foreign)
                    {
                        inf.ForeignKeyTableName = ((string)reader["ForeignKeyTable"]).TrimEnd();
                    }
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
                        var constraintType = reader["ConstraintType"] == DBNull.Value ? "" : (string)(reader["ConstraintType"]);
                        tmpInf.Kind = GetConstraintType(constraintType);
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
}
