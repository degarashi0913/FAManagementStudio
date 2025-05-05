using FirebirdSql.Data.FirebirdClient;

namespace FAManagementStudio.Models.Firebird;

internal static class FbCommandExtensions
{
    public static short GetInt16OrDefault(this FbDataReader reader, int index, short defaultValue = 0)
        => reader.IsDBNull(index) ? defaultValue : reader.GetInt16(index);

    public static string GetStringOrDefault(this FbDataReader reader, int index)
        => reader.IsDBNull(index) ? string.Empty : reader.GetString(index);
}
