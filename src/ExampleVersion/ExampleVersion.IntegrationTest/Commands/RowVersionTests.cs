using System;
using affolterNET.Data;
using affolterNET.Data.Commands;
using affolterNET.Data.Extensions;
using affolterNET.Data.Models;
using ExampleVersion.Data;
using Xunit;
using Xunit.Abstractions;

namespace ExampleVersion.IntegrationTest.Commands;

[Collection(nameof(ExampleFixture))]
public class RowVersionTests : IntegrationTest
{
    public RowVersionTests(ExampleFixture dbFixture, ITestOutputHelper log) : base(dbFixture)
    {

    }

    [Fact]
    public void CanInsert()
    {
        CQB<SaveInfo>()
            .Arrange(db =>
            {
                var dto = new ExampleVersion_T_DemoTable
                {
                    Id = Guid.NewGuid(),
                    Message = "Ich bin drin",
                    Status = "Neu",
                };
                dto.SetInserted("test");
                return new SaveEntityCommand<ExampleVersion_T_DemoTable>(dto);
            })
            .ActAndAssert((result, ah) =>
            {
                Assert.Equal(Constants.Inserted, result.Data.Action);
            });
    }
    
    [Fact]
    public void CanUpdate()
    {
        CQB<SaveInfo>()
            .Arrange(db =>
            {
                var dto = new ExampleVersion_T_DemoTable
                {
                    Id = Guid.NewGuid(),
                    Message = "Ich bin drin",
                    Status = "Neu",
                };
                dto.SetInserted("test");
                db.Insert(dto);
                var reloaded = db.SelectById<ExampleVersion_T_DemoTable>(dto.Id);
                Assert.NotNull(reloaded);
                reloaded.Message = "I was reloaded";
                return new SaveEntityCommand<ExampleVersion_T_DemoTable>(reloaded);
            })
            .ActAndAssert((result, ah) =>
            {
                Assert.Equal(Constants.Updated, result.Data.Action);
            });
    }
    
    [Fact]
    public void CanNotUpdateWhenChanged()
    {
        var builder = CQB<SaveInfo>()
            .Arrange(db =>
            {
                var dto = new ExampleVersion_T_DemoTable
                {
                    Id = Guid.NewGuid(),
                    Message = "Ich bin drin",
                    Status = "Neu",
                };
                dto.SetInserted("test");
                db.Insert(dto);
                var reloaded = db.SelectById<ExampleVersion_T_DemoTable>(dto.Id);
                Assert.NotNull(reloaded);

                // different update
                db.Update<ExampleVersion_T_DemoTable>().WithUpdate(ExampleVersion_T_DemoTable.Cols.Message.StripSquareBrackets(), "somebody else").Execute();

                reloaded.Message = "I was reloaded";
                return new SaveEntityCommand<ExampleVersion_T_DemoTable>(reloaded);
            });
        Assert.Throws<ConcurrencyException>(() => builder.Act());
    }

    [Fact]
    public void CanNotUpdateWithoutTimestamp()
    {
        var builder = CQB<SaveInfo>()
            .Arrange(db =>
            {
                var dto = new ExampleVersion_T_DemoTable
                {
                    Id = Guid.NewGuid(),
                    Message = "Ich bin drin",
                    Status = "Neu",
                };
                dto.SetInserted("test");
                db.Insert(dto);
                var reloaded = db.SelectById<ExampleVersion_T_DemoTable>(dto.Id);
                Assert.NotNull(reloaded);
                reloaded.VersionTimestamp = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 };
                reloaded.Message = "I was reloaded";
                return new SaveEntityCommand<ExampleVersion_T_DemoTable>(reloaded);
            });
        Assert.Throws<ConcurrencyException>(() => builder.Act());
    }
}