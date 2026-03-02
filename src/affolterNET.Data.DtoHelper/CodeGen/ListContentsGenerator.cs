using System;
using System.Collections.Generic;
using System.Linq;
using affolterNET.Data.DtoHelper.Database;
using affolterNET.Data.DtoHelper.Dialect;
using affolterNET.Data.DtoHelper.Extensions;
using Dapper;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace affolterNET.Data.DtoHelper.CodeGen;

public class ListContentsGenerator
{
    private readonly Table _tbl;
    private readonly ListContentsCfg _ctsCfg;
    private readonly GeneratorCfg _genCfg;
    private readonly List<MemberDeclarationSyntax> _list;
    private readonly Column _idColumn;
    private readonly Column _nameColumn;

    public ListContentsGenerator(Table tbl, ListContentsCfg ctsCfg, GeneratorCfg genCfg)
    {
        _tbl = tbl;
        _ctsCfg = ctsCfg;
        _genCfg = genCfg;
        var idc =_tbl.Columns.FirstOrDefault(c => c.Name == _ctsCfg.IdAttribute);
        _idColumn = idc ?? throw new InvalidOperationException(
            $"{nameof(ListContentsGenerator)}: Column {_ctsCfg.IdAttribute} not found");
        var nc = _tbl.Columns.FirstOrDefault(c => c.Name == _ctsCfg.NameAttribute);
        _nameColumn = nc ?? throw new InvalidOperationException(
            $"{nameof(ListContentsGenerator)}: Column {_ctsCfg.NameAttribute} not found");
        if (nc.PropertyType != "string")
        {
            throw new InvalidOperationException($"Cannot use {nc.PropertyType}-column as name for ListContents");
        }

        _list = new List<MemberDeclarationSyntax>();
    }

    public ClassDeclarationSyntax Generate(ClassDeclarationSyntax scd)
    {
        var dialect = _genCfg.SqlDialect;
        using var conn = dialect.CreateConnection(_genCfg.ConnString!);
        var tableName = dialect.QuoteTableName(_tbl.Schema, _tbl.Name);
        var cmd = $"select {dialect.QuoteIdentifier(_ctsCfg.IdAttribute)} as Id, {dialect.QuoteIdentifier(_ctsCfg.NameAttribute)} as Value from {tableName}";
        var results = conn.Query(cmd).ToList();

        var dictName = "_dict";
        List<string> dictLines = new();
        foreach (var row in results)
        {
            // private fields
            string name = row.Value;
            var propertyName = name.CleanMemberName();
            dictLines.Add(_idColumn.PropertyType == "Guid" ?
                                          $"{{ Guid.Parse(\"{row.Id}\"), \"{propertyName}\" }}" :
                                          $"{{ {row.Id}, \"{propertyName}\" }}");
            // props
            var sgp = new StringGenerator($"public static {_idColumn.PropertyType} {name.CleanMemberName()} => {dictName}.First(kvp => kvp.Value == \"{propertyName}\").Key;");
            sgp.Generate(Add);
        }

        var sgdict = new StringGenerator(
            @$"private static Dictionary<{_idColumn.PropertyType}, string> {dictName} = new() {{{Environment.NewLine}
                {string.Join($",{Environment.NewLine}", dictLines)}
            }};");
        sgdict.Generate(Add);

        // get by string
        var returnStatement = _idColumn.PropertyType == "Guid"
            ? "return entry.Equals(default(KeyValuePair<Guid, string>)) ? null : entry.Value;"
            : "return entry.Equals(default(KeyValuePair<int, string>)) ? null : entry.Value;";
        var sgs = new StringGenerator($"public static string? Get{_ctsCfg.ClassName.CleanMemberName()}String (this Guid g) {{" +
                                      $"var entry = {dictName}.FirstOrDefault(kvp => kvp.Key.Equals(g));" +
                                      returnStatement +
                                      "}");
        sgs.Generate(Add);

        scd = scd.AddMembers(_list.ToArray());
        return scd;
    }

    private void Add(MemberDeclarationSyntax mds)
    {
        _list.Add(mds);
    }
}
