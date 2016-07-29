using System;

namespace FAManagementStudio.Common
{
    [Flags]
    public enum ConstraintsKind
    {
        None = 0,
        Primary = 1,
        Foreign = 1 << 1,
        Unique = 1 << 2,
        Check = 1 << 3,
        NotNull = 1 << 4
    }
    public enum TableKind { Table, View }
    public enum SqlKind { Select, Insert, Update }
    public enum FirebirdType { Fb25, Fb3 }
}
