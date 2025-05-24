using FAManagementStudio.Common;
using FirebirdSql.Data.FirebirdClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

namespace FAManagementStudio.Models;

public partial class QueryInfo(bool showExecutePlan) : BindableBase
{
    public IEnumerable<QueryResult> ExecuteQuery(string connectionString, string query)
    {
        if (string.IsNullOrEmpty(query?.Trim())) yield break;
        using var con = new FbConnection(connectionString);
        con.Open();
        foreach (var item in AnalyzeQuery(query))
        {
            yield return InnerExecuteQuery(con, item);
        }
    }

    private QueryResult InnerExecuteQuery(FbConnection con, AnalyzedQuery item)
    {
        try
        {
            return item.Type switch
            {
                QueryType.Select => ExecuteReader(con, item.Query, showExecutePlan),
                QueryType.Update => ExecuteUpdate(con, item.Query),
                QueryType.Others => ExecuteNonQuery(con, item.Query),
                _ => throw new NotImplementedException($"QueryType {item.Type} is not implemented.")
            };
        }
        catch (FbException e)
        {
            return new(GetScalaView("Exception", e.Message), new(), item.Query);
        }
    }

    private DataTable GetScalaView(string colName, string message)
    {
        var table = new DataTable();

        var col = new DataColumn();
        col.ColumnName = "0";
        col.Caption = colName;
        col.DataType = typeof(string);
        table.Columns.Add(col);

        var row = table.NewRow();
        row[0] = message;
        table.Rows.Add(row);

        return table;
    }

    private QueryResult ExecuteReader(FbConnection con, string query, bool showExecutePlan)
    {
        using var command = con.CreateCommand();
        command.CommandText = query;
        var reader = command.ExecuteReader();

        var schema = reader.GetSchemaTable();
        var table = new DataTable();
        for (var i = 0; i < schema.Rows.Count; i++)
        {
            ;

            var col = new DataColumn
            {
                ColumnName = i.ToString(),
                Caption = schema.Rows[i]["ColumnName"].ToString(),
                DataType = Type.GetType(schema.Rows[i]["DataType"].ToString() ?? "System.Object")
            };
            table.Columns.Add(col);
        }
        var startTime = DateTime.Now;
        while (reader.Read())
        {
            var row = table.NewRow();
            for (var i = 0; i < reader.FieldCount; i++)
            {
                row[i] = reader[i];
            }
            table.Rows.Add(row);
        }
        var executeTime = DateTime.Now - startTime;
        var plan = showExecutePlan ? command.GetCommandPlan() : "";

        return new QueryResult(table, executeTime, query, table.Rows.Count, plan);
    }

    private QueryResult ExecuteUpdate(FbConnection con, string query)
    {
        using var command = con.CreateCommand();
        command.CommandText = query;
        var startTime = DateTime.Now;
        var num = command.ExecuteNonQuery();

        var executeTime = DateTime.Now - startTime;

        return new QueryResult(GetScalaView("Message", $"{num}行更新しました。"), executeTime, query);
    }

    private QueryResult ExecuteNonQuery(FbConnection con, string query)
    {
        using var command = con.CreateCommand();
        command.CommandText = query;
        var startTime = DateTime.Now;
        var res = command.ExecuteNonQuery();

        var executeTime = DateTime.Now - startTime;

        return new QueryResult(GetScalaView("Message", $"実行しました。"), executeTime, query);
    }

    private enum QueryType { Select, Update, Others };

    private record struct AnalyzedQuery(QueryType Type, string Query);

    private IReadOnlyCollection<AnalyzedQuery> AnalyzeQuery(string input)
        => [.. QueryAnalyzer.Analyze(input.Trim())
            .Where(query => !query.StartsWith("--", StringComparison.OrdinalIgnoreCase))
            .Select(query =>
                {
                    if (query.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
                    {
                        return new AnalyzedQuery(QueryType.Select, query);
                    }
                    else if (query.StartsWith("UPDATE", StringComparison.OrdinalIgnoreCase))
                    {
                        return new(QueryType.Update, query);
                    }
                    else if (ExecuteBlockRegex().Match(query).Success)
                    {
                        return new(QueryType.Select, query);
                    }
                    else
                    {
                        return new(QueryType.Others, query);
                    }
                })
            ];

    [GeneratedRegex("execute[\\s\\n]+block[\\s\\n]+returns[\\s\\n(]+", RegexOptions.IgnoreCase)]
    private static partial Regex ExecuteBlockRegex();
}
