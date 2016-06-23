using FAManagementStudio.Common;
using System.Collections.Generic;

namespace FAManagementStudio.Models
{
    public class IndexInfo
    {
        public IndexInfo() { }
        public string Name { get; set; }
        public ConstraintsKind Kind { get; set; }
        public string ForigenKeyName { get; set; }
        public List<string> FieldNames { get; } = new List<string>();
        public string TableName { get; set; }
    }
}
