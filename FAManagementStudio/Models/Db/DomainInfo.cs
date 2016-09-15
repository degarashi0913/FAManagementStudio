using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FAManagementStudio.Models
{
    public class DomainInfo
    {
        public DomainInfo(string name, FieldType type, string validationSource, string dafaultSource, bool isNullFlag)
        {
            DomainName = name;
            DomainType = type;
            ValidationSource = validationSource;
            DefaultSource = dafaultSource;
            IsNullFlag = isNullFlag;
        }
        public string DomainName { get; set; }
        public FieldType DomainType { get; set; }
        public string ValidationSource { get; set; }
        public string DefaultSource { get; set; }
        public bool IsNullFlag { get; set; }
    }
}
