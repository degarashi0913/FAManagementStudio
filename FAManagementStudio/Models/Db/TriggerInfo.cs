namespace FAManagementStudio.Models
{
    public class TriggerInfo
    {
        public TriggerInfo(string name, string tableName, string source)
        {
            Name = name;
            TableName = tableName;
            Source = source;
        }
        public string Name { get; set; }
        public string Source { get; set; }
        public string TableName { get; set; }
    }
}
