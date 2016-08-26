using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FAManagementStudio.Models
{
    public class GeneratorInfo
    {
        public GeneratorInfo(string name, long currentValue)
        {
            Name = name;
            CurrentValue = currentValue;
        }
        public string Name { get; set; }
        public long CurrentValue { get; set; }
    }
}
