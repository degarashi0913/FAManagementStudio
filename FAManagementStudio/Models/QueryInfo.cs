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
            col.ColumnName = colName;
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
                    col.ColumnName = schema.Rows[i]["ColumnName"].ToString();
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
            var querys = GetStatement(input.Trim());
            var list = new List<AnalyzedQuery>();

            for (int i = 0; i < querys.Length; i++)
            {
                var query = querys[i].Trim();
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
                else
                {
                    list.Add(new AnalyzedQuery() { Type = QueryType.Othres, Query = query });
                }
            }
            return list;
        }

        private string[] GetStatement(string input)
        {
            var list = new List<string>();
            var inputStr = input;
            var lowerString = input.ToLower();

            var key = "((create|alter)[\\s\\n]+(trigger|procedure)[\\s\\n]+|(execute[\\s\\n]+block[\\s\\n]+))";
            var reg = Regex.Match(inputStr, key, RegexOptions.IgnoreCase);
            var idx = 0;
            if (reg.Success)
            {
                while (reg.Success)
                {
                    //prev
                    if (idx < reg.Index)
                    {
                        foreach (var item in EscapeNewLine(inputStr.Substring(idx, reg.Index - idx)))
                        {
                            list.Add(item);
                        }
                    }

                    var chIdx = Regex.Match(inputStr.Substring(reg.Index), "\\s+begin\\s+", RegexOptions.IgnoreCase).Index + reg.Index + 6;
                    var nestedCount = 1;
                    while ((0 < nestedCount) && (chIdx < inputStr.Length))
                    {
                        chIdx++;
                        var word = GetWord(ref inputStr, ref chIdx);
                        if (word == "begin") nestedCount++;
                        else if (word == "end") nestedCount--;
                    }

                    var query = reg.Index < chIdx ? inputStr.Substring(reg.Index, chIdx - reg.Index) : inputStr.Substring(reg.Index).Trim();
                    list.Add(query);
                    idx = chIdx;
                    reg = reg.NextMatch();
                }
            }
            //reminder
            if (idx < inputStr.Length)
            {
                foreach (var item in EscapeNewLine(inputStr.Substring(idx)))
                {
                    list.Add(item);
                }
            }
            return list.ToArray();
        }

        private string GetWord(ref string statement, ref int startIdx)
        {
            var origin = startIdx;
            var limit = statement.Length;
            var length = 0;
            var startFlg = false;
            while (startIdx < limit)
            {
                var ch = statement[origin + length];
                switch (ch)
                {
                    case ' ':
                    case '\r':
                    case '\n':
                    case ';':
                        if (startFlg)
                        {
                            goto end;
                        }
                        else
                        {
                            origin++;
                        }
                        break;
                    default:
                        startFlg = true;
                        length++;
                        break;
                }
            }
            startIdx = statement.Length;
            return statement.Substring(origin);
            end:
            startIdx = origin + length;
            return statement.Substring(origin, length);
        }


        private IEnumerable<string> GetWord(string stetmenet, int startIdx = 0, int length = 0)
        {
            var stIdx = startIdx;
            var edIdx = startIdx;
            var flg = false;
            var limit = 0 < length ? length : stetmenet.Length;
            while (edIdx < limit)
            {
                var ch = stetmenet[edIdx];
                switch (ch)
                {
                    case ' ':
                    case '\r':
                    case '\n':
                        if (flg)
                        {
                            yield return stetmenet.Substring(stIdx, edIdx - stIdx);
                            stIdx = edIdx + 1;
                            flg = false;
                        }
                        else
                        {
                            stIdx = edIdx + 1;
                        }
                        break;
                    case ';':
                        if (flg)
                        {
                            yield return stetmenet.Substring(stIdx, edIdx - stIdx);
                            yield return ";";
                            stIdx = edIdx + 1;
                            flg = false;
                        }
                        else
                        {
                            stIdx = edIdx + 1;
                        }
                        break;
                    default:
                        flg = true;
                        break;
                }
                edIdx++;
            }
            if (stIdx < edIdx)
            {
                yield return stetmenet.Substring(stIdx, edIdx - stIdx);
            }
        }

        private IEnumerable<string> EscapeNewLine(string statement)
        {
            var array = statement.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < array.Length; i++)
            {
                var query = array[i].Trim();
                //Escape Comment State
                query = Regex.Replace(query, "--.*(\r\n)?", "");

                query = query.Trim(' ', '\r', '\n');
                if (string.IsNullOrEmpty(query)) continue;

                yield return query;
            }
        }
    }
}
