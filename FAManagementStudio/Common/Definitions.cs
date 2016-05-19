using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FAManagementStudio.Common
{
    public enum ConstraintsKind { None, Primary, Foreign, Unique, Check, NotNull };
    public enum SqlKind { Select, Insert, Update }
}
