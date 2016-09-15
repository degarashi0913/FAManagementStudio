using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FAManagementStudio.Models
{
    public class ProcedureInfo
    {
        public ProcedureInfo(string name, string source)
        {
            ProcedureName = name;
            Source = source;
        }
        public string ProcedureName { get; set; }
        public string Source { get; set; }
    }
}
