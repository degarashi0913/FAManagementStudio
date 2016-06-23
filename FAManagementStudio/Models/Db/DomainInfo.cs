using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FAManagementStudio.Models
{
    public class DomainInfo
    {
        public DomainInfo(string name, FieldType type)
        {
            DomainName = name;
            DomainType = type;
        }
        public string DomainName { get; set; }
        public FieldType DomainType { get; set; }

    }
}
