using FAManagementStudio.Common;
using FirebirdSql.Data.FirebirdClient;
using System;
using System.Collections.Generic;

namespace FAManagementStudio.Models.Db;

public class DatabaseInfo(FirebirdInfo fbInfo) : BindableBase
{
    public string Path => fbInfo.Path;
    internal string ConnectionString => fbInfo.Builder.ConnectionString;
    public bool CanLoadDatabase => fbInfo.IsTargetOdsVersion();
    public FbConnectionStringBuilder Builder => fbInfo.Builder;

    public void CreateDatabase()
    {
        FbConnection.CreateDatabase(fbInfo.Builder.ConnectionString, overwrite: false);
    }

    public string GetDefaultCharSet(FbConnection con)
    {
        using var command = con.CreateCommand();
        command.CommandText = @"select RDB$CHARACTER_SET_NAME CharSet from RDB$DATABASE";
        return command.ExecuteScalar() as string ?? "UTF8";
    }

    public IEnumerable<TableInfo> GetTables(FbConnection con)
    {
        using var command = con.CreateCommand();
        command.CommandText = @"select trim(rdb$relation_name) AS Name from rdb$relations where rdb$relation_type = 0 and rdb$system_flag = 0 order by rdb$relation_name asc";
        var reader = command.ExecuteReader();
        while (reader.Read())
        {
            yield return new TableInfo((string)reader["Name"]);
        }
    }

    public IEnumerable<TableInfo> GetSystemTables(FbConnection con)
    {
        using var command = con.CreateCommand();
        command.CommandText = @"select trim(rdb$relation_name) AS Name from rdb$relations where rdb$relation_type = 0 and rdb$system_flag = 1 order by rdb$relation_name asc";
        var reader = command.ExecuteReader();
        while (reader.Read())
        {
            yield return new TableInfo((string)reader["Name"], 1);
        }
    }

    public IEnumerable<ViewInfo> GetViews(FbConnection con)
    {
        using var command = con.CreateCommand();
        command.CommandText = @"select trim(rdb$relation_name) AS Name, rdb$view_source Source from rdb$relations where rdb$relation_type = 1 and rdb$system_flag = 0 order by rdb$relation_name asc";
        var reader = command.ExecuteReader();
        while (reader.Read())
        {
            yield return new ViewInfo((string)reader["Name"], ((string)reader["Source"]).Trim(' ', '\r', '\n'));
        }
    }
    public IEnumerable<DomainInfo> GetDomain(FbConnection con)
    {
        using var command = con.CreateCommand();
        command.CommandText =
            """
            select distinct f.rdb$field_type Type, f.rdb$field_sub_type SubType , f.rdb$character_length CharSize, trim(f.rdb$field_name) FieldName, f.rdb$field_precision FieldPrecision, f.rdb$field_scale FieldScale, f.rdb$field_length FieldLength, coalesce(f.rdb$validation_source, '') ValidationSource, coalesce(f.rdb$default_source, '') DefaultSource, f.rdb$null_flag NullFlag
              from rdb$fields f
              where f.rdb$FIELD_NAME not starting with 'RDB$' and f.rdb$FIELD_NAME not starting with 'MON$' and f.rdb$FIELD_NAME not starting with 'SEC$'
              order by f.rdb$field_name;
            """;
        var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var name = (string)reader["FieldName"];
            var size = (reader["CharSize"] == DBNull.Value) ? null : (short?)reader["CharSize"];
            var subType = (reader["SubType"] == DBNull.Value) ? null : (short?)reader["SubType"];
            var precision = (reader["FieldPrecision"] == DBNull.Value) ? null : (short?)reader["FieldPrecision"];
            var scale = (reader["FieldScale"] == DBNull.Value) ? null : (short?)reader["FieldScale"];
            var fieldLength = (reader["FieldLength"] == DBNull.Value) ? null : (short?)reader["FieldLength"];
            var type = new FieldType((short)reader["Type"], subType, size, precision, scale, fieldLength);
            var validationSource = (string)reader["ValidationSource"];
            var defaultSource = (string)reader["DefaultSource"];
            var nullFlag = reader["NullFlag"] == DBNull.Value;
            yield return new DomainInfo(name, type, validationSource, defaultSource, nullFlag);
        }
    }
    public IEnumerable<ProcedureInfo> GetProcedures(FbConnection con)
    {
        using var command = con.CreateCommand();
        command.CommandText =
            """
            select rdb$procedure_name Name, rdb$procedure_source Source
              from rdb$procedures
              where rdb$procedure_source is not null;
            """;
        var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var name = (string)reader["Name"];
            var source = (string)reader["Source"];
            yield return new ProcedureInfo(name, source);
        }
    }
    public IEnumerable<SequenceInfo> GetSequences(FbConnection con)
    {
        using var command = con.CreateCommand();
        command.CommandText =
            """
            execute block
            returns (
                GeneratorName char(31),
                CurrentValue bigint
            )
            as
            begin
                for select rdb$generator_name
                    from rdb$generators
                    where rdb$system_flag = 0
                    into GeneratorName
                do
                begin
                    execute statement
                        'select gen_id(' || GeneratorName || ', 0) from rdb$database'
                    into CurrentValue;
                    suspend;
                end
            end; 
            """;
        var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var name = (string)reader["GeneratorName"];
            var value = (long)reader["CurrentValue"];
            yield return new SequenceInfo(name, value);
        }
    }
}
