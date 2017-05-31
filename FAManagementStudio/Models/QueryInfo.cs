using FAManagementStudio.Common;
using FirebirdSql.Data.FirebirdClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;

namespace FAManagementStudio.Models
{
    public class QueryInfo : BindableBase
    {
        public bool ShowExecutePlan { private get; set; } = false;
        public IEnumerable<QueryResult> ExecuteQuery(string connectionString, string query)
        {
            if (string.IsNullOrEmpty(query?.Trim())) yield break;
            using (var con = new FbConnection(connectionString))
            {
                con.Open();
                foreach (var item in AnalyzeQuery(query))
                {
                    yield return InnerExecuteQuery(con, item);
                }
            }
        }

        private QueryResult InnerExecuteQuery(FbConnection con, AnalyzedQuery item)
        {

            QueryResult result = null;
            try
            {
                switch (item.Type)
                {
                    case QueryType.Select:
                        result = ExecuteReader(con, item.Query, ShowExecutePlan);
                        break;
                    case QueryType.Update:
                        result = ExecuteUpdate(con, item.Query);
                        break;
                    case QueryType.Othres:
                        result = ExecuteNonQeuery(con, item.Query);
                        break;
                }
            }
            catch (FbException e)
            {
                result = new QueryResult(GetScalaView("Exception", e.Message), new TimeSpan(), item.Query);
            }
            return result;
        }

        private DataTable GetScalaView(string colName, string messege)
        {
            var table = new DataTable();

            var col = new DataColumn();
            col.ColumnName = "0";
            col.Caption = colName;
            col.DataType = typeof(string);
            table.Columns.Add(col);

            var row = table.NewRow();
            row[0] = messege;
            table.Rows.Add(row);

            return table;
        }

        private QueryResult ExecuteReader(FbConnection con, string query, bool showExecutePlan)
        {
            using (var command = con.CreateCommand())
            {
                command.CommandText = query;
                var res = command.BeginExecuteReader(null, null);
                var reader = command.EndExecuteReader(res);

                var schema = reader.GetSchemaTable();
                var table = new DataTable();
                for (var i = 0; i < schema.Rows.Count; i++)
                {
                    var col = new DataColumn();
                    col.ColumnName = i.ToString();
                    col.Caption = schema.Rows[i]["ColumnName"].ToString();
                    col.DataType = Type.GetType(schema.Rows[i]["DataType"].ToString());
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
                var plan = showExecutePlan ? command.CommandPlan : "";

                return new QueryResult(table, executeTime, query, table.Rows.Count, plan);
            }
        }

        private QueryResult ExecuteUpdate(FbConnection con, string query)
        {
            using (var command = con.CreateCommand())
            {
                command.CommandText = query;
                var startTime = DateTime.Now;
                var res = command.BeginExecuteNonQuery(null, null);
                var num = command.EndExecuteNonQuery(res);

                var executeTime = DateTime.Now - startTime;

                return new QueryResult(GetScalaView("Message", $"{num}行更新しました。"), executeTime, query);
            }
        }

        private QueryResult ExecuteNonQeuery(FbConnection con, string query)
        {
            using (var command = con.CreateCommand())
            {
                command.CommandText = query;
                var startTime = DateTime.Now;
                var res = command.BeginExecuteNonQuery(null, null);
                var num = command.EndExecuteNonQuery(res);

                var executeTime = DateTime.Now - startTime;

                return new QueryResult(GetScalaView("Message", $"実行しました。"), executeTime, query);
            }
        }

        private enum QueryType { Select, Update, Othres };

        private struct AnalyzedQuery { public QueryType Type; public string Query; }

        private List<AnalyzedQuery> AnalyzeQuery(string input)
        {
            var querys = QueryAnalyzer.Analyze(input.Trim());
            var list = new List<AnalyzedQuery>();

            for (int i = 0; i < querys.Length; i++)
            {
                var query = querys[i];
                if (query.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
                {
                    list.Add(new AnalyzedQuery() { Type = QueryType.Select, Query = query });
                }
                else if (query.StartsWith("UPDATE", StringComparison.OrdinalIgnoreCase))
                {
                    list.Add(new AnalyzedQuery() { Type = QueryType.Update, Query = query });
                }
                else if (Regex.Match(query, "execute[\\s\\n]+block[\\s\\n]+returns[\\s\\n(]+", RegexOptions.IgnoreCase).Success)
                {
                    list.Add(new AnalyzedQuery() { Type = QueryType.Select, Query = query });
                }
                else if (query.StartsWith("--", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                else
                {
                    list.Add(new AnalyzedQuery() { Type = QueryType.Othres, Query = query });
                }
            }
            return list;
        }
    }
}
