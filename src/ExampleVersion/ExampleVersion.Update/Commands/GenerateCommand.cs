using System.ComponentModel;
using affolterNET.Data.DtoHelper;
using affolterNET.Data.DtoHelper.CodeGen;
using affolterNET.Data.DtoHelper.Database;
using Spectre.Console.Cli;

namespace ExampleVersion.Update.Commands
{
    public class GenerateCommand : AsyncCommand<GenerateCommand.Settings>
    {
        // ReSharper disable once ClassNeverInstantiated.Global
        public class Settings : CommandSettings
        {
            [CommandArgument(0, "<connstring>")]
            public string ConnString { get; set; } = null!;

            [CommandArgument(1, "[namespace]")]
            [DefaultValue("ExampleVersion.Data")]
            public string NameSpace { get; set; } = null!;

            [CommandArgument(2, "[targetfile]")]
            [DefaultValue("../../ExampleVersion.Data/Dto.cs")]
            public string TargetFile { get; set; } = null!;

            [CommandOption("-b|--baseclass")]
            [DefaultValue("IDtoBase")]
            public string BaseClass { get; set; } = null!;
            
            [CommandOption("-v|--viewbaseclass")]
            [DefaultValue("IViewBase")]
            public string ViewBaseClass { get; set; } = null!;
        }
        
        public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
        {
            {
                var tlp = new GeneratorCfg()
                    .WithConn(settings.ConnString)
                    .WithTargetFile(settings.TargetFile)
                    .WithTableNameExclusion("dbo_sysdiagrams")
                    .WithTableNameExclusion("dbo_SchemaVersions")
                    .WithSchemaExclusion("dbo")
                    .WithSchemaExclusion("Example")
                    .WithSchemaExclusion("ExampleHistory")
                    .WithSchemaExclusion("ExampleUserDate")
                    .WithSchemaExclusion("ExampleVersionUserDate")
                    .WithSchemaExclusion("ExampleVersionUserDateHistory")
                    .WithContentsList("ExampleVersion","T_DemoTableType", "Id", "Name", "ExampleVersionDemoTableTypes")
                    .WithUsing("System")
                    .WithUsing("System.Collections.Generic")
                    .WithUsing("System.Data")
                    .WithUsing("System.Linq")
                    .WithUsing("affolterNET.Data")
                    .WithUsing("affolterNET.Data.Extensions")
                    .WithUsing("affolterNET.Data.Interfaces")
                    .WithUsing("Dapper")
                    .WithUsing("System.ComponentModel.DataAnnotations")
                    .WithComment("// ReSharper disable InconsistentNaming")
                    .WithComment("// ReSharper disable MemberCanBePrivate.Global")
                    .WithComment("// ReSharper disable ClassNeverInstantiated.Global")
                    .WithComment("// ReSharper disable UnusedAutoPropertyAccessor.Global")
                    .WithComment("// ReSharper disable UnusedMember.Global")
                    .WithComment("// ReSharper disable StyleCop.SA1001")
                    .WithComment("// ReSharper disable StyleCop.SA1402")
                    .WithComment("// ReSharper disable StyleCop.SA1101")
                    .WithComment("// ReSharper disable StyleCop.SA1310")
                    .WithComment("// ReSharper disable StyleCop.SA1201")
                    .WithComment("// ReSharper disable StyleCop.SA1401")
                    .WithComment("// ReSharper disable StyleCop.SA1311")
                    .WithComment("// ReSharper disable StyleCop.SA1516")
                    .WithComment("// ReSharper disable StyleCop.SA1015")
                    .WithComment("// ReSharper disable StyleCop.SA1012")
                    .WithComment("// ReSharper disable StyleCop.SA1013")
                    .WithComment("// ReSharper disable StyleCop.SA1113")
                    .WithComment("// ReSharper disable StyleCop.SA1115")
                    .WithComment("// ReSharper disable StyleCop.SA1116")
                    .WithVersion(version => version == "VersionTimestamp")
                    .WithNamespace(settings.NameSpace)
                    .WithBaseType(settings.BaseClass)
                    .WithBaseViewType(settings.ViewBaseClass);
                var gen = new Generator(tlp, new FileHandler(tlp));
                await gen.Generate();
                return 0;
            }
        }
    }
}