using System;
using System.Collections.Generic;
using System.Linq;
using affolterNET.Data.DtoHelper.Database;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace affolterNET.Data.DtoHelper.CodeGen
{
    public class ClassesGenerator
    {
        private readonly GeneratorCfg _cfg;

        public ClassesGenerator(GeneratorCfg cfg)
        {
            _cfg = cfg;
        }

        public NamespaceDeclarationSyntax Generate(NamespaceDeclarationSyntax ns, Tables tables)
        {
            var cds = new List<MemberDeclarationSyntax>();
            var dialect = _cfg.SqlDialect;

            // dtos
            foreach (var tbl in tables)
            {
                if (string.IsNullOrWhiteSpace(tbl.ObjectName))
                {
                    throw new InvalidOperationException($"{nameof(tbl.ObjectName)} was empty");
                }

                // class
                var classDeclaration = SyntaxFactory.ClassDeclaration(tbl.ObjectName);
                classDeclaration = classDeclaration.AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
                if (tbl.IsView)
                {
                    foreach (var viewBaseType in _cfg.ViewBaseTypes)
                    {
                        classDeclaration = classDeclaration.AddBaseListTypes(
                            SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName(viewBaseType)));
                    }
                }
                else
                {
                    foreach (var baseType in _cfg.BaseTypes)
                    {
                        classDeclaration = classDeclaration.AddBaseListTypes(
                            SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName(baseType)));
                    }
                }

                var cd = new ClassGenerator(tbl, dialect);
                classDeclaration = cd.Generate(classDeclaration);
                cds.Add(classDeclaration);

                // ListContents - to use contents like Enums
                var lcCfg = _cfg.TableContents.FirstOrDefault(te => te.TableName.ToLower() == tbl.Name.ToLower() && te.SchemaName.ToLower() == tbl.Schema.ToLower());
                if (lcCfg != null)
                {
                    var staticClassDeclaration = SyntaxFactory.ClassDeclaration(lcCfg.ClassName);
                    staticClassDeclaration = staticClassDeclaration.AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
                    staticClassDeclaration = staticClassDeclaration.AddModifiers(SyntaxFactory.Token(SyntaxKind.StaticKeyword));
                    var eg = new ListContentsGenerator(tbl, lcCfg, _cfg);
                    classDeclaration = eg.Generate(staticClassDeclaration);
                    cds.Add(classDeclaration);
                }
            }

            return ns.AddMembers(cds.ToArray());
        }
    }
}
