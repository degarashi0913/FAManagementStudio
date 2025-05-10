using FAManagementStudio.Common;
using FAManagementStudio.ViewModels.Commons;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FAManagementStudio.ViewModels.Db;

public class TableViewModel(string name, IReadOnlyCollection<ColumViewModel> columns, bool isSystemTable = false) : ViewModelBase, ITableViewModel
{
    public string TableName => name;
    public bool IsSystemTable => isSystemTable;
    public IReadOnlyCollection<ColumViewModel> Columns => columns;
    public TableKind Kind => TableKind.Table;
    public List<TriggerViewModel> Triggers { get; } = [];
    public List<IndexViewModel> Indexes { get; } = [];

    public string GetDdl(DbViewModel dbVm)
    {
        var columns = Columns.Select(x =>
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

        var indexWithConstraints = Indexes
                .Where(x => x.Kind != ConstraintsKind.None)
                .Select(x =>
                {
                    var sql = x.IndexName.StartsWith("rdb", StringComparison.OrdinalIgnoreCase) ? "" : $"CONSTRAINT {x.IndexName} ";
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

        var domain = Columns.Where(x => x.IsDomainType)
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
        return domainStr + $"CREATE TABLE {TableName} ({Environment.NewLine}  {string.Join($",{Environment.NewLine}  ", columns.Union(indexWithConstraints).ToArray()) + Environment.NewLine})";
    }
}
