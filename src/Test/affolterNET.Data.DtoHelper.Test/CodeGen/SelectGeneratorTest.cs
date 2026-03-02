using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using affolterNET.Data.DtoHelper.CodeGen;
using affolterNET.Data.DtoHelper.Database;
using affolterNET.Data.DtoHelper.Dialect;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;
using Xunit.Abstractions;

namespace affolterNET.Data.DtoHelper.Test.CodeGen
{
    public class SelectGeneratorTest
    {
        private readonly ITestOutputHelper _output;

        public SelectGeneratorTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void GenerateOnePkTest()
        {
            var generatorCfg = new GeneratorCfg();
            var dialect = generatorCfg.SqlDialect;
            var tbl = new Table(generatorCfg) { Name = "T_Test", Schema = "dbo" };
            var cols = new List<Column>();
            cols.Add(new Column(generatorCfg) { Name = "TestId", PropertyName = "TestId", IsPK = true });
            cols.Add(new Column(generatorCfg) { Name = "Bezeichnung", PropertyName = "Bezeichnung" });
            // ReSharper disable once PossibleNullReferenceException
            typeof(Table)
                .GetField("AllColumns",BindingFlags.Instance|BindingFlags.NonPublic)!
                .SetValue(tbl, cols);
            var gen = new SelectGenerator(tbl, dialect);
            var list = new List<MemberDeclarationSyntax>();
            gen.Generate(mds => list.Add(mds));

            var m = Assert.Single(list) as BaseMethodDeclarationSyntax;
            Assert.NotNull(m);
            Assert.NotNull(m!.Body);
            var method = Regex.Replace(m.Body!.ToString(), @"\s+", " ", RegexOptions.Multiline);
            _output.WriteLine(method);
            const string expectation = "{ var cols = \"[TestId], [Bezeichnung]\".GetColumns(affolterNET.Data.Extensions.QuoteStyle.Brackets, excludedColumns)";
            var parts = method.Split(";");
            Assert.True(parts.Length > 0);
            Assert.Equal(expectation, parts[0]);
        }

        [Fact]
        public void GenerateTwoPkTest()
        {
            var generatorCfg = new GeneratorCfg();
            var dialect = generatorCfg.SqlDialect;
            var tbl = new Table(generatorCfg) { Name = "T_TestOther", Schema = "dbo" };
            var cols = new List<Column>();
            cols.Add(new Column(generatorCfg) { Name = "TestId", PropertyName = "TestId", IsPK = true });
            cols.Add(new Column(generatorCfg) { Name = "OtherId", PropertyName = "OtherId", IsPK = true });
            cols.Add(new Column(generatorCfg) { Name = "Bezeichnung", PropertyName = "Bezeichnung" });
            // ReSharper disable once PossibleNullReferenceException
            typeof(Table)
                .GetField("AllColumns",BindingFlags.Instance|BindingFlags.NonPublic)!
                .SetValue(tbl, cols);
            var gen = new SelectGenerator(tbl, dialect);
            var list = new List<MemberDeclarationSyntax>();
            gen.Generate(mds => list.Add(mds));
            var m = Assert.Single(list) as MethodDeclarationSyntax;
            Assert.NotNull(m);
            Assert.NotNull(m!.Body);
            var method = Regex.Replace(m!.Body!.ToString(), @"\s+", " ", RegexOptions.Multiline);
            _output.WriteLine(method);
            const string expectation = "{ var cols = \"[TestId], [OtherId], [Bezeichnung]\".GetColumns(affolterNET.Data.Extensions.QuoteStyle.Brackets, excludedColumns)";
            var parts = method.Split(";");
            Assert.True(parts.Length > 0);
            Assert.Equal(expectation, parts[0]);
        }
    }
}
