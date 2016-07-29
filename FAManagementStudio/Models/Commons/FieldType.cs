namespace FAManagementStudio.Models
{
    public class FieldType
    {
        public short Type { get; set; }
        public short? FieldSubType { get; set; }
        public short? CharactorLength { get; set; }
        public short? FieldPrecision { get; set; }
        public short? FieldScale { get; set; }

        public FieldType(short type, short? subType, short? cLength, short? precision, short? scale)
        {
            Type = type;
            FieldSubType = subType;
            CharactorLength = cLength;
            FieldPrecision = precision;
            FieldScale = scale;
        }

        public override string ToString()
        {
            return GetTypeFromFirebirdType(Type, FieldSubType, CharactorLength, FieldPrecision, FieldScale);
        }

        private string GetFixedPointDataType(string typeName, short? subType, short? precision, short? scale)
        {
            if (subType.HasValue && subType != 0)
            {
                var fixedPoint = $"({precision}";
                if (scale.HasValue && scale != 0)
                {
                    fixedPoint += $",{-scale}";
                }
                fixedPoint += ")";

                if (subType == 1) return $"NUMERIC{fixedPoint}";
                if (subType == 2) return $"DECIMAL{fixedPoint}";
            }
            return typeName;
        }

        private string GetTypeFromFirebirdType(short type, short? subType, short? cLength, short? precision, short? scale)
        {
            switch (type)
            {
                case 7:
                    return GetFixedPointDataType("SMALLINT", subType, precision, scale);
                case 8:
                    return GetFixedPointDataType("INTEGER", subType, precision, scale);
                case 9:
                    return "QUAD";
                case 10:
                    return "FLOAT";
                case 11:
                    return "D_FLOAT";
                case 12:
                    return "DATE";
                case 13:
                    return "TIME";
                case 14:
                    return $"CHAR({cLength})";
                case 16:
                    return GetFixedPointDataType("BIGINT", subType, precision, scale);
                case 23:
                    return "BOOLEAN";
                case 27:
                    return "DOUBLE PRECISION";
                case 35:
                    return "TIMESTAMP";
                case 37:
                    return $"VARCHAR({cLength})";
                case 40:
                    return "CSTRING";
                case 45:
                    return "BLOB_ID";
                case 261:
                    return "BLOB";
                default:
                    return "";
            }
        }
    }
}
