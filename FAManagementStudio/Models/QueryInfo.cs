using FAManagementStudio.Common;
using FirebirdSql.Data.FirebirdClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
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
            var querys = input.Trim().Split(';');
            var list = new List<AnalyzedQuery>();

            for (int i = 0; i < querys.Length; i++)
            {
                var query = querys[i];
                var idx = 0;
                var reg = Regex.Match(query, "(SELECT|INSERT|UPDATE|CREATE|DROP)", RegexOptions.IgnoreCase);
                while (reg.Success)
                {
                    idx = reg.Index;
                    reg = reg.NextMatch();
                    var partQuerty = "";
                    if (reg.Success)
                    {
                        partQuerty = query.Substring(idx, reg.Index - idx);
                        //next : select statement
                        if (reg.Value.ToUpper() == "SELECT")
                        {
                            // first: insert statement
                            if (partQuerty.StartsWith("INSERT", StringComparison.OrdinalIgnoreCase))
                            {
                                partQuerty = (reg = reg.NextMatch()).Success ? query.Substring(idx, reg.Index - idx) : partQuerty = query.Substring(idx);
                            }
                            // first:not insert statement
                            else
                            {
                                var subQueryIdx = reg.Index;
                                var subPart = partQuerty;
                                while (subPart.Contains("(") && !subPart.Contains(")"))
                                {
                                    var subQueryCount = 1;
                                    while ((subQueryIdx < query.Length) && (0 < subQueryCount))
                                    {
                                        var ch = query[subQueryIdx];
                                        if (ch == ')')
                                        {
                                            subQueryCount--;
                                        }
                                        else if (ch == '(')
                                        {
                                            subQueryCount++;
                                        }
                                        subQueryIdx++;
                                    }
                                    while ((reg.Index < subQueryIdx) && (0 < reg.Index))
                                    {
                                        reg = reg.NextMatch();
                                    }
                                    if (reg.Index < 1) break;
                                    subPart = query.Substring(subQueryIdx, reg.Index - subQueryIdx);
                                    subQueryIdx = reg.Index;
                                }
                                partQuerty = 0 < reg.Index ? query.Substring(idx, subQueryIdx - idx) : query.Substring(idx);
                            }
                        }
                    }
                    else
                    {
                        partQuerty = query.Substring(idx);
                    }

                    if (partQuerty.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
                    {
                        list.Add(new AnalyzedQuery() { Type = QueryType.Select, Query = partQuerty });
                    }
                    else if (partQuerty.StartsWith("UPDATE", StringComparison.OrdinalIgnoreCase))
                    {
                        list.Add(new AnalyzedQuery() { Type = QueryType.Update, Query = partQuerty });
                    }
                    else
                    {
                        list.Add(new AnalyzedQuery() { Type = QueryType.Othres, Query = partQuerty });
                    }
                }
            }
            return list;
        }
    }
}
