using affolterNET.Data.DtoHelper.CodeGen;

namespace affolterNET.Data.DtoHelper.Database
{
    public class Column : ElementBase
    {
        private readonly GeneratorCfg _cfg;

        public Column(GeneratorCfg cfg)
        {
            _cfg = cfg;
        }

        public bool Ignore { get; set; }

        public bool IsAutoIncrement { get; set; }

        public bool IsNullable { get; set; }

        public bool IsPK { get; set; }

        public int? MaxLength { get; set; }

        public string Name { get; set; } = null!;

        public string? PropertyName { get; set; }

        public string? PropertyType { get; set; }
        
        public string? DataType { get; set; }

        public bool IsVersionCol()
        {
            return _cfg.VersionFunc(Name);
        }

        public bool IsInsertTriggerField()
        {
            return _cfg.InsertUserFunc(Name) || _cfg.InsertDateFunc(Name);
        }

        public bool IsUpdateTriggerField(bool forInsert = false)
        {
            if (_cfg.UpdateUserFunc(Name))
            {
                return !(forInsert && _cfg.AddUpdateUserToInsert);
            }

            if (_cfg.UpdateDateFunc(Name))
            {
                return !forInsert || !_cfg.AddUpdateDateToInsert;
            }

            return false;
        }

        public bool IsActiveCol()
        {
            return _cfg.IsActiveFunc(Name);
        }

        public bool IsPkWithAutoincrement()
        {
            return IsPK && IsAutoIncrement;
        }

        public string CheckNullable()
        {
            if (IsNullable)
            {
                return "?";
            }

            return string.Empty;
        }

        public string WriteProperty(int indent)
        {
            var myIndent = GetIndent(indent);
            var isNullable = string.Empty;
            var dapperKey = string.Empty;
            if (IsPK)
            {
                dapperKey = string.Format("{0}[Da.Key]", myIndent);
            }
            else
            {
                if (!IsNullable)
                {
                    isNullable = string.Format("{0}[Da.Required]", myIndent);    
                }
            }

            var dataType = string.Format($"{{0}}[Da.DataType(\"{DataType}\")]", myIndent);

            var maxLength = string.Empty;
            if (MaxLength.HasValue)
            {
                maxLength = string.Format("{0}[Da.MaxLength({1})]", myIndent, MaxLength);
            }

            var defaultValue = string.Empty;
            if (IsVersionCol() && DataType == "timestamp")
            {
                defaultValue = " = {0,0,0,0,0,0,0,0};";
            }
            else if (!IsNullable && IsReferenceType())
            {
                defaultValue = " = null!;";
            }
            var prop = string.Format(
                "{0}{1}{2}{3}{4}public {5}{6} {7} {{ get; set;}}{8}",
                dataType,
                dapperKey,
                maxLength,
                isNullable,
                myIndent,
                PropertyType,
                CheckNullable(),
                PropertyName,
                defaultValue);

            return prop;
        }

        public string WriteSetId(int indent)
        {
            if (!IsPK)
            {
                return string.Empty;
            }

            var myIndent = GetIndent(indent);
            var setid = string.Format("{0}public void SetId(object id) {{", myIndent);
            if (PropertyType == "string")
            {
                setid = string.Format("{0}{1}{2} = id.ToString();", setid, GetIndent(indent + 4), PropertyName);
            }
            else if (PropertyType == "Guid")
            {
                setid = string.Format("{0}{1}if (!Guid.TryParse(id.ToString(), out var guidId))", setid, GetIndent(indent + 4));
                setid = string.Format("{0}{1}{{", setid, GetIndent(indent + 4));
                setid = string.Format("{0}{1}throw new InvalidOperationException(\"invalid id\");", setid, GetIndent(indent + 8));
                setid = string.Format("{0}{1}}}", setid, GetIndent(indent + 4));
                setid = string.Format("{0}{1}{2} = guidId;", setid, GetIndent(indent + 4), PropertyName);
            }
            else
            {
                setid = string.Format("{0}{1}var intId = Convert.ToInt32(id);", setid, GetIndent(indent + 4));
                setid = string.Format("{0}{1}{2} = intId;", setid, GetIndent(indent + 4), PropertyName);
            }

            setid = string.Format("{0}{1}}}", setid, myIndent);
            return setid;
        }

        public string WriteIsAutoincrementId(int indent)
        {
            if (!IsPK)
            {
                return string.Empty;
            }

            var myIndent = GetIndent(indent);
            var setid = string.Format("{0}public bool IsAutoincrementId() {{", myIndent);
            if (IsAutoIncrement)
            {
                setid = string.Format("{0}{1}return true;", setid, GetIndent(indent + 4));
            }
            else
            {
                setid = string.Format("{0}{1}return false;", setid, GetIndent(indent + 4));
            }

            setid = string.Format("{0}{1}}}", setid, myIndent);
            return setid;
        }

        public bool IsReferenceType()
        {
            return PropertyType == "string" ||
                   PropertyType == "byte[]" ||
                   PropertyType == "Microsoft.SqlServer.Types.SqlGeography" ||
                   PropertyType == "Microsoft.SqlServer.Types.SqlGeometry";
        }

        public bool IsDefaultCol()
        {
            return IsUpdateTriggerField() || IsInsertTriggerField() || IsActiveCol() || IsVersionCol();
        }
    }
}