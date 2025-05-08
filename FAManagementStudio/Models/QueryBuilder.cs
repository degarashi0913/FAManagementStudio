using System.Collections.Generic;
using System.Linq;

namespace FAManagementStudio.Models;

public class QueryBuilder
{
    private readonly HashSet<string> _sqlKeyWord = ["index"];

    public string EscapeKeyWord(string column)
        => _sqlKeyWord.Contains(column.ToLower()) ? $"'{column}'" : column;

    public string CreateSelectStatement(string tableName, string[] columns, int topCount)
    {
        var escapedColumnsStr = string.Join(", ", [.. columns.Select(EscapeKeyWord)]);
        var topSentence = 0 < topCount ? $" first({topCount})" : "";
        return $"select{topSentence} {escapedColumnsStr} from {tableName}";
    }

    public string CreateInsertStatement(string tableName, string[] columns)
    {
        var escapedColumnsStr = string.Join(", ", columns.Select(EscapeKeyWord).ToArray());
        var valuesStr = string.Join(", ", columns.Select(x => $"@{x.ToLower()}").ToArray());
        return $"insert into {tableName} ({escapedColumnsStr}) values ({valuesStr})";
    }

    public string CreateUpdateStatement(string tableName, string[] columns)
    {
        var setStr = string.Join(", ", [.. columns.Select(x => $"{EscapeKeyWord(x)} = @{x.ToLower()}")]);
        return $"update {tableName} set {setStr}";
    }
}
