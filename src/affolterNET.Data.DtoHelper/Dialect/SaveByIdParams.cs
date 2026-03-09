namespace affolterNET.Data.DtoHelper.Dialect;

public record SaveByIdParams(
    string TableName,
    string PkColumn,
    string UpdateCall,
    string InsertCall,
    string SelectCall,
    string PkParamName,
    bool HasAutoIncrementPk,
    bool HasSelect);
