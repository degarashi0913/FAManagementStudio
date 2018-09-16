using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace FAManagementStudio.Models
{
    public class QueryResult
    {
        public DataTable View { get; }
        public TimeSpan ExecuteTime { get; }
        public int Count { get; set; }
        public string ExecuteSql { get; }
        public string ExecutionPlan { get; }

        public QueryResult(DataTable table, TimeSpan time, string sql, int count = 0, string executionPlan = "")
        {
            View = table;
            ExecuteTime = time;
            ExecuteSql = sql;
            Count = count;
            ExecutionPlan = executionPlan;
        }
    }
}
