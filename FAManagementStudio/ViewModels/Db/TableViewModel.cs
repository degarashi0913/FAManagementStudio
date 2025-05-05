using FAManagementStudio.Common;
using FAManagementStudio.ViewModels.Db;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FAManagementStudio.ViewModels
{
    public class TableViewModel : ViewModelBase, ITableViewModel
    {
        public TableViewModel(string name, bool isSystemTable = false)
        {
            TableName = name;
            IsSystemTable = isSystemTable;
        }
        public string TableName { get; }
        public bool IsSystemTable { get; }
        public List<ColumViewMoodel> Colums { get; } = new List<ColumViewMoodel>();
        public TableKind Kind { get; } = TableKind.Table;
        public List<TriggerViewModel> Triggers { get; } = new List<TriggerViewModel>();
        public List<IndexViewModel> Indexs { get; } = new List<IndexViewModel>();

        public string GetDdl(DbViewModel dbVm)
        {
            var colums = Colums.Select(x =>
            {
                var sql = $"{x.ColumName} {x.ColumType}";
                if (!x.NullFlag)
                {
                    sql += " NOT NULL";
                }
                if (!x.IsDomainType && !string.IsNullOrEmpty(x.DefaultSource))
                {
                    sql += " " + x.DefaultSource;
                }
                return sql;
            });

            var indexWithConstraints = Indexs
                    .Where(x => x.Kind != ConstraintsKind.None)
                    .Select(x =>
                    {
                        var sql = x.IndexName.StartsWith("rdb", System.StringComparison.OrdinalIgnoreCase) ? "" : $"CONSTRAINT {x.IndexName} ";
                        switch (x.Kind)
                        {
                            case ConstraintsKind.Primary:
                                sql += $"PRIMARY KEY ({string.Join(", ", x.FieldNames.ToArray())})";
                                break;
                            case ConstraintsKind.Foreign:
                                var targetPrimaryIdx = dbVm.Indexes.Where(dbIdx => dbIdx.IndexName == x.ForeignKeyName).First();
                                sql += $"FOREIGN KEY ({string.Join(", ", x.FieldNames.ToArray())}) REFERENCES {targetPrimaryIdx.TableName} ({string.Join(", ", targetPrimaryIdx.FieldNames.ToArray())})";
                                if (!string.IsNullOrEmpty(x.DeleteRule)) sql += $" ON DELETE {x.DeleteRule}";
                                if (!string.IsNullOrEmpty(x.UpdateRule)) sql += $" ON UPDATE {x.UpdateRule}";
                                break;
                            case ConstraintsKind.Unique:
                                sql += $"UNIQUE ({string.Join(", ", x.FieldNames.ToArray())})";
                                break;
                            default:
                                return "";
                        }
                        return sql;
                    }).Where(x => !string.IsNullOrEmpty(x));

            var domain = Colums.Where(x => x.IsDomainType)
                                .Select(x => new { x.ColumType, x.ColumDataType, x.FieldNullFlag, x.DefaultSource })
                                .Distinct()
                                .Select(x =>
                                {
                                    var baseStr = $"CREATE DOMAIN {x.ColumType} AS {x.ColumDataType}";
                                    if (!x.FieldNullFlag)
                                    {
                                        baseStr += " NOT NULL";
                                    }
                                    if (!string.IsNullOrEmpty(x.DefaultSource))
                                    {
                                        baseStr += " " + x.DefaultSource;
                                    }
                                    return baseStr + ";" + Environment.NewLine;
                                });
            var domainStr = string.Join("", domain.ToArray());
            return domainStr + $"CREATE TABLE {TableName} ({Environment.NewLine}  {string.Join($",{Environment.NewLine}  ", colums.Union(indexWithConstraints).ToArray()) + Environment.NewLine})";
        }
    }
}
