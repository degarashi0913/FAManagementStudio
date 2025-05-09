using FAManagementStudio.Common;
using FAManagementStudio.Models.Firebird;
using FirebirdSql.Data.FirebirdClient;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FAManagementStudio.Models.Db;

public class TableInfo(string name, int systemFlag = 0)
{
    public string TableName { get; } = name;
    public int SystemFlag { get; } = systemFlag;

    public IEnumerable<ColumInfo> GetColumns(FbConnection con)
    {
        var constraints = GetConstrains(con);

        using var command = con.CreateCommand();
        command.CommandText =
            $"select trim(rf.rdb$field_name) Name, f.rdb$field_type Type, f.rdb$field_sub_type SubType , f.rdb$character_length CharSize, trim(rf.rdb$field_source) FieldSource, rf.rdb$null_flag NullFlag, f.rdb$null_flag fieldNullFlag, f.rdb$field_precision FieldPrecision, f.rdb$field_scale FieldScale, f.rdb$field_length FieldLength, coalesce(rf.rdb$default_source, '') DefaultSource " +
                "from rdb$relation_fields rf " +
                "join rdb$relations r on rf.rdb$relation_name = r.rdb$relation_name " +
                                    "and r.rdb$view_blr is null " +
                                    $"and r.rdb$relation_type = 0 and r.rdb$system_flag = {SystemFlag} " +
                "join rdb$fields f on f.rdb$field_name = rf.rdb$field_source " +
            $"where rf.rdb$relation_name = '{TableName}' " +
             "order by rf.rdb$field_position; ";
        var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var name = (string)reader["Name"];
            var size = (reader["CharSize"] == DBNull.Value) ? null : (short?)reader["CharSize"];
            var subType = (reader["SubType"] == DBNull.Value) ? null : (short?)reader["SubType"];
            var nullFlag = reader["NullFlag"] == DBNull.Value;
            var fieldNullFlag = reader["FieldNullFlag"] == DBNull.Value;
            var precision = (reader["FieldPrecision"] == DBNull.Value) ? null : (short?)reader["FieldPrecision"];
            var scale = (reader["FieldScale"] == DBNull.Value) ? null : (short?)reader["FieldScale"];
            var fieldLength = (reader["FieldLength"] == DBNull.Value) ? null : (short?)reader["FieldLength"];
            var type = new FieldType((short)reader["Type"], subType, size, precision, scale, fieldLength);
            var defaultSource = (string)reader["DefaultSource"];

            var constraintInfo = constraints.TryGetValue(name, out ColumConstraintsInfo? value) ? value : new();

            yield return new ColumInfo(name, type, constraintInfo, (string)reader["FieldSource"], nullFlag, fieldNullFlag, defaultSource);
        }
    }

    private Dictionary<string, ColumConstraintsInfo> GetConstrains(FbConnection con)
    {
        var dic = new Dictionary<string, ColumConstraintsInfo>();
        using var command = con.CreateCommand();
        command.CommandText =
            "select trim(seg.rdb$field_name) FieldName, rel.rdb$constraint_type Type, trim(idx.foreign_key_table) ForeignKeyTable " +
            "from rdb$relation_constraints rel " +
            "left outer join( " +
            "select idx.rdb$index_name, idx.rdb$relation_name, idx2.rdb$relation_name foreign_key_table " +
            "from rdb$indices idx " +
            "left outer join rdb$indices idx2 on idx.rdb$foreign_key = idx2.rdb$index_name) idx " +
            "on  rel.rdb$index_name = idx.rdb$index_name " +
            "left outer join rdb$index_segments seg on idx.rdb$index_name  = seg.rdb$index_name " +
           $"where rel.rdb$relation_name = '{this.TableName}' and seg.rdb$field_name != '' ";
        var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var name = reader.GetStringOrDefault(0);
            if (!dic.TryGetValue(name, out ColumConstraintsInfo? inf))
            {
                inf = new ColumConstraintsInfo();
                dic.Add(name, inf);
            }
            var type = GetConstraintType(reader.GetStringOrDefault(1));
            inf.SetKind(type);
            if (type == ConstraintsKind.Foreign)
            {
                inf.ForeignKeyTableName = reader.GetStringOrDefault(2);
            }
        }
        return dic;
    }

    private static ConstraintsKind GetConstraintType(string input)
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
        using var command = con.CreateCommand();
        command.CommandText = $"select rdb$trigger_name Name, rdb$relation_name TableName, rdb$trigger_source Source from rdb$triggers where rdb$relation_name = '{this.TableName}' and rdb$system_flag = 0";
        var reader = command.ExecuteReader();
        while (reader.Read())
        {
            yield return new TriggerInfo((string)reader["Name"], (string)reader["TableName"], (string)reader["Source"]);
        }
    }



    // Currently, only simple indexes are supported.
    public IEnumerable<IndexInfo> GetIndex(FbConnection con)
    {
        using var command = con.CreateCommand();
        command.CommandText =
            "select trim(idx.rdb$index_name) Name, trim(seg.rdb$field_name) FiledName, idx.rdb$unique_flag UniqueFlag, rel.rdb$constraint_type ConstraintType, trim(rdb$foreign_key) ForeignKey, trim(ref.rdb$update_rule) UpdateRule, trim(ref.rdb$delete_rule) DeleteRule " +
            "from rdb$indices idx " +
            "left outer join rdb$index_segments seg on idx.rdb$index_name = seg.rdb$index_name " +
            "left outer join rdb$relation_constraints rel on idx.rdb$index_name = rel.rdb$index_name " +
            "left outer join rdb$ref_constraints ref on ref.rdb$constraint_name = rel.rdb$constraint_name " +
            $"where idx.rdb$relation_name = '{TableName}' and idx.rdb$system_flag = 0 order by seg.rdb$field_position";
        var reader = command.ExecuteReader();

        var list = new List<IndexRecord>();

        while (reader.Read())
        {
            var data = new IndexRecord(
                reader.GetStringOrDefault(0),
                reader.GetStringOrDefault(1),
                reader.GetInt16OrDefault(2) == 1,
                reader.GetStringOrDefault(3),
                reader.GetStringOrDefault(4),
                reader.GetStringOrDefault(5),
                reader.GetStringOrDefault(6));

            list.Add(data);
        }

        return [..list.ToLookup(x => x.Name)
             .Select(x =>
             {
                var first = x.ElementAt(0);

                return  new IndexInfo(
                    x.Key,
                    first.UniqueFlag ,
                    GetConstraintType(first.ConstraintType),
                    first.ForeignKey,
                    [.. x.Select(y => y.FiledName)],
                    TableName,
                    first.UpdateRule,
                    first.DeleteRule);
             })];
    }
    private record struct IndexRecord(string Name, string FiledName, bool UniqueFlag, string ConstraintType, string ForeignKey, string UpdateRule, string DeleteRule);
}
