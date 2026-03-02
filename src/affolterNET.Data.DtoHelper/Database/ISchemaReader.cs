using System.Data;
using affolterNET.Data.DtoHelper.CodeGen;
using affolterNET.Data.DtoHelper.Dialect;

namespace affolterNET.Data.DtoHelper.Database;

public interface ISchemaReader
{
    Tables ReadSchema(IDbConnection cn, GeneratorCfg cfg, ISqlDialect dialect);
}
