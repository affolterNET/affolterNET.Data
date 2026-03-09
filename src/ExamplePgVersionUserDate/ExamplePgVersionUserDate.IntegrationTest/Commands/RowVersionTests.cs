using System;
using affolterNET.Data;
using affolterNET.Data.Commands;
using affolterNET.Data.Extensions;
using affolterNET.Data.Models;
using ExamplePgVersionUserDate.Data;
using Xunit;
using Xunit.Abstractions;

namespace ExamplePgVersionUserDate.IntegrationTest.Commands;

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
                var dto = new example_pg_version_user_date_demo_table
                {
                    Id = Guid.NewGuid(),
                    Message = "Ich bin drin",
                    Status = "Neu",
                };
                dto.SetInserted("test");
                return new SaveEntityCommand<example_pg_version_user_date_demo_table>(dto);
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
                var dto = new example_pg_version_user_date_demo_table
                {
                    Id = Guid.NewGuid(),
                    Message = "Ich bin drin",
                    Status = "Neu",
                };
                dto.SetInserted("test");
                db.Insert(dto);
                var reloaded = db.SelectById<example_pg_version_user_date_demo_table>(dto.Id);
                Assert.NotNull(reloaded);
                reloaded.Message = "I was reloaded";
                return new SaveEntityCommand<example_pg_version_user_date_demo_table>(reloaded);
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
                var dto = new example_pg_version_user_date_demo_table
                {
                    Id = Guid.NewGuid(),
                    Message = "Ich bin drin",
                    Status = "Neu",
                };
                dto.SetInserted("test");
                db.Insert(dto);
                var reloaded = db.SelectById<example_pg_version_user_date_demo_table>(dto.Id);
                Assert.NotNull(reloaded);

                // different update
                db.Update<example_pg_version_user_date_demo_table>().WithUpdate(example_pg_version_user_date_demo_table.Cols.Message.StripQuoting(), "somebody else").Execute();

                reloaded.Message = "I was reloaded";
                return new SaveEntityCommand<example_pg_version_user_date_demo_table>(reloaded);
            });
        Assert.Throws<ConcurrencyException>(() => builder.Act());
    }

    [Fact]
    public void CanNotUpdateWithoutTimestamp()
    {
        var builder = CQB<SaveInfo>()
            .Arrange(db =>
            {
                var dto = new example_pg_version_user_date_demo_table
                {
                    Id = Guid.NewGuid(),
                    Message = "Ich bin drin",
                    Status = "Neu",
                };
                dto.SetInserted("test");
                db.Insert(dto);
                var reloaded = db.SelectById<example_pg_version_user_date_demo_table>(dto.Id);
                Assert.NotNull(reloaded);
                reloaded.VersionTimestamp = 99999;
                reloaded.Message = "I was reloaded";
                return new SaveEntityCommand<example_pg_version_user_date_demo_table>(reloaded);
            });
        Assert.Throws<ConcurrencyException>(() => builder.Act());
    }
}
