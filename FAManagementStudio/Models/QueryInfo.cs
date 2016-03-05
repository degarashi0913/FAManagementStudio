using FAManagementStudio.Common;
using FirebirdSql.Data.FirebirdClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Text.RegularExpressions;

namespace FAManagementStudio.Models
{
    class QueryInfo : BindableBase
    {
        public ObservableCollection<DataView> Result = new ObservableCollection<DataView>();
        public void ExecuteQuery(string connectionString, string query)
        {
            if (string.IsNullOrEmpty(query?.Trim())) return;
            try
            {
                Result.Clear();
                using (var con = new FbConnection(connectionString))
                {
                    con.Open();
                    foreach (var item in AnalyzeQuery(query))
                    {
                        DataView view = null;
                        try
                        {
                            switch (item.Type)
                            {
                                case QueryType.Select:
                                    view = ExecuteReader(con, item.Query);
                                    break;
                                case QueryType.Update:
                                    view = ExecuteUpdate(con, item.Query);
                                    break;
                                case QueryType.Othres:
                                    view = ExecuteNonQeuery(con, item.Query);
                                    break;
                            }
                        }
                        catch (FbException e)
                        {
                            view = GetScalaView("Exception", e.Message);
                        }
                        Result.Add(view);
                    }
                }
            }
            catch (Exception e)
            {
                Result.Add(GetScalaView("Exception", e.Message));
            }
        }

        private DataView GetScalaView(string colName, string messege)
        {
            var table = new DataTable();

            var col = new DataColumn();
            col.ColumnName = colName;
            col.DataType = typeof(string);
            table.Columns.Add(col);

            var row = table.NewRow();
            row[0] = messege;
            table.Rows.Add(row);

            return table.DefaultView;
        }

        private DataView ExecuteReader(FbConnection con, string query)
        {
            using (var command = con.CreateCommand())
            {
                command.CommandText = query;
                var reader = command.ExecuteReader();

                var schema = reader.GetSchemaTable();
                var table = new DataTable();
                for (var i = 0; i < schema.Rows.Count; i++)
                {
                    var col = new DataColumn();
                    col.ColumnName = schema.Rows[i]["ColumnName"].ToString();
                    col.DataType = Type.GetType(schema.Rows[i]["DataType"].ToString());
                    table.Columns.Add(col);
                }

                while (reader.Read())
                {
                    var row = table.NewRow();
                    for (var i = 0; i < reader.FieldCount; i++)
                    {
                        row[i] = reader[i];
                    }
                    table.Rows.Add(row);
                }
                return table.DefaultView;
            }
        }

        private DataView ExecuteUpdate(FbConnection con, string query)
        {
            using (var command = con.CreateCommand())
            {
                command.CommandText = query;
                var num = command.ExecuteNonQuery();

                return GetScalaView("Message", $"{num}行更新しました。");
            }
        }

        private DataView ExecuteNonQeuery(FbConnection con, string query)
        {
            using (var command = con.CreateCommand())
            {
                command.CommandText = query;
                command.ExecuteNonQuery();

                return GetScalaView("Message", $"実行しました。");
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

            var key = "(create|alter)[\\s\\n]+(trigger|procedure)[\\s\\n]+";
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
                        if (chIdx + 6 < inputStr.Length)
                        {
                            if (((lowerString[chIdx] == ' ') || (lowerString[chIdx] == '\n') || (lowerString[chIdx] == ';')) &&
                                (lowerString[chIdx + 1] == 'b') &&
                                (lowerString[chIdx + 2] == 'e') &&
                                (lowerString[chIdx + 3] == 'g') &&
                                (lowerString[chIdx + 4] == 'i') &&
                                (lowerString[chIdx + 5] == 'n') &&
                                ((lowerString[chIdx + 6] == ' ') || (lowerString[chIdx + 6] == '\r')))
                            {
                                nestedCount++;
                                chIdx += 7;
                            }
                        }
                        if (chIdx + 3 < inputStr.Length)
                        {
                            if (((lowerString[chIdx] == ' ') || (lowerString[chIdx] == '\n') || (lowerString[chIdx] == ';')) &&
                                (lowerString[chIdx + 1] == 'e') &&
                                (lowerString[chIdx + 2] == 'n') &&
                                (lowerString[chIdx + 3] == 'd'))
                            {
                                nestedCount--;
                                chIdx += 4;
                            }
                        }
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

        private IEnumerable<string> EscapeNewLine(string statement)
        {
            var array = statement.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < array.Length; i++)
            {
                var query = array[i].Trim();
                if (string.IsNullOrEmpty(query)) continue;
                var chIdx = 0;
                while ((query[chIdx] == '\r') || (query[chIdx] == '\n'))
                {
                    chIdx++;
                }
                if (chIdx == query.Length) continue;
                if (0 < chIdx)
                {
                    query = query.Substring(chIdx);
                }

                yield return query;
            }
        }
    }
}
