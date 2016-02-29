using FAManagementStudio.Common;
using FirebirdSql.Data.FirebirdClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;

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
                using (var con = new FbConnection(connectionString))
                using (var command = con.CreateCommand())
                {
                    con.Open();
                    command.CommandText = query;
                    var reader = command.ExecuteReader();
                    Result.Clear();
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
                    Result.Add(table.DefaultView);
                }
            }
            catch (Exception e)
            {
                Result.Clear();
                var table = new DataTable();

                var col = new DataColumn();
                col.ColumnName = "Exception";
                col.DataType = typeof(string);
                table.Columns.Add(col);

                var row = table.NewRow();
                row[0] = e.Message;
                table.Rows.Add(row);

                Result.Add(table.DefaultView);
            }
        }
    }
}
