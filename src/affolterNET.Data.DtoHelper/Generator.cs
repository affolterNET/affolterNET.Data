using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using affolterNET.Data.DtoHelper.CodeGen;
using affolterNET.Data.DtoHelper.Database;
using affolterNET.Data.DtoHelper.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace affolterNET.Data.DtoHelper
{
    public class Generator
    {
        private readonly GeneratorCfg _cfg;

        private readonly ClassesGenerator _cg;

        private readonly IFileHandler _fh;

        private readonly TablesLoader _tl;

        private NamespaceDeclarationSyntax? _ns;

        private CompilationUnitSyntax? _root;

        public Generator(GeneratorCfg cfg, IFileHandler fh)
        {
            _cfg = cfg;
            _fh = fh;
            _tl = new TablesLoader(_cfg);
            _cg = new ClassesGenerator(cfg);
        }

        public async Task Generate()
        {
            var tr = LoadTables();

            await SetRootAndNamespace();

            FillStatics(tr);

            FillDtos(tr.Tables);

            WriteTargetFile();
        }

        private TablesResult LoadTables()
        {
            var res = _tl.LoadTables();
            if ((res.Error != null) || (res.Ex != null))
            {
                throw new InvalidOperationException(res.Error, res.Ex);
            }

            return res;
        }

        private async Task SetRootAndNamespace()
        {
            if (string.IsNullOrWhiteSpace(_cfg.Namespace))
            {
                throw new InvalidOperationException($"{nameof(_cfg.Namespace)} was empty");
            }

            // Parse the code into a SyntaxTree.
            var tree = CSharpSyntaxTree.ParseText(string.Empty);

            // Get the root CompilationUnitSyntax.
            _root = await tree.GetRootAsync().ConfigureAwait(false) as CompilationUnitSyntax;

            if (_root == null)
            {
                throw new InvalidOperationException("Root des Codefiles war null");
            }

            // usings
            var usings = _cfg.Usings.Select(
                u =>
                {
                    var us = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(u));
                    if (u.IndexOf("DataAnnotations", StringComparison.Ordinal) > -1)
                    {
                        us = us.WithAlias(SyntaxFactory.NameEquals(SyntaxFactory.IdentifierName("Da")));
                    }

                    return us;
                }).ToArray();
            _root = _root.AddUsings(usings);

            // namespace
            _ns = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(_cfg.Namespace));

            // comments
            var comm = _cfg.Comments.Select(SyntaxFactory.Comment).ToArray();
            _ns = _ns.WithLeadingTrivia(comm);
        }

        private void FillStatics(TablesResult tr)
        {
            var staticsBuilder = new StringBuilder();
            var staticsViewsBuilder = new StringBuilder();
            foreach (var tbl in tr.Tables)
            {
                var ifstring = $"if (typeof({tbl.ObjectName}) == typeof(T)) {{ return new {tbl.ObjectName}(); }}";
                if (tbl.IsView)
                {
                    staticsViewsBuilder.Append(ifstring);
                }
                else
                {
                    staticsBuilder.Append(ifstring);
                }
            }

            var statics = staticsBuilder.ToString();
            var staticsViews = staticsViewsBuilder.ToString();

            // tablefactory
            var getTableString =
                $"public IDtoBase Get<T>() where T : IDtoBase {{ {statics} throw new InvalidOperationException(); }}";
            var dbString = $"public class DtoFactory: IDtoFactory {{{getTableString}}}";
            var sgStaticTableFactory = new StringGenerator(dbString);
            var listTables = new List<MemberDeclarationSyntax>();
            sgStaticTableFactory.Generate(mds => listTables.Add(mds));
            _ns = _ns!.AddMembers(listTables.ToArray());

            // viewfactory
            var getViewString =
                $"public IViewBase Get<T>() where T : IViewBase {{ {staticsViews} throw new InvalidOperationException(); }}";
            var viewString = $"public class ViewFactory: IViewFactory {{{getViewString}}}";
            var sgStaticViewFactory = new StringGenerator(viewString);
            var listViews = new List<MemberDeclarationSyntax>();
            sgStaticViewFactory.Generate(mds => listViews.Add(mds));
            _ns = _ns.AddMembers(listViews.ToArray());
        }

        private void FillDtos(Tables tables)
        {
            // classes
            _ns = _cg.Generate(_ns!, tables);
            _root = _root!.AddMembers(_ns);
        }

        private void WriteTargetFile()
        {
            // Write the new file.
            _fh.WriteCode(_root!.NormalizeWhitespace().ToFullString());
        }
    }
}