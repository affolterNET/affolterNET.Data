using System;
using System.Collections.Generic;
using System.Linq;
using affolterNET.Data.Models.Filters;
using affolterNET.Data.Queries;
using ExamplePg.Data;
using Xunit;

namespace ExamplePg.IntegrationTest.Queries
{
    [Collection(nameof(ExampleFixture))]
    public class LoadEntityQueryTest : IntegrationTest
    {
        public LoadEntityQueryTest(ExampleFixture dbFixture) : base(dbFixture)
        {
        }

        [Fact]
        public void LoadAllEntitiesTest()
        {
            CQB<IEnumerable<example_pg_demo_table>>()
                .Arrange(db => new LoadEntityQuery<example_pg_demo_table>())
                .ActAndAssert((result, ah) =>
                {
                    var list = result.Data.ToList();
                    Assert.Single(list);
                    Assert.Equal("It is working!", list.First().Message);
                });
        }


        [Fact]
        public void LoadEntityWithFilterTest()
        {
            var filter = new RootFilter(example_pg_demo_table.Cols.Message)
            {
                Value = "It is working!"
            };
            CQB<IEnumerable<example_pg_demo_table>>()
                .Arrange(db => new LoadEntityQuery<example_pg_demo_table>(filter))
                .ActAndAssert((result, ah) =>
                {
                    var list = result.Data.ToList();
                    Assert.Single(list);
                    Assert.Equal("It is working!", list.First().Message);
                });
        }

        [Fact]
        public void LoadEntityWithSingleFilterTest()
        {
            CQB<IEnumerable<example_pg_demo_table>>()
                .Arrange(db =>
                    LoadEntityQuery<example_pg_demo_table>.CreateWithFilter(example_pg_demo_table.Cols.Message,
                        "It is working!"))
                .ActAndAssert((result, ah) =>
                {
                    var list = result.Data.ToList();
                    Assert.Single(list);
                    Assert.Equal("It is working!", list.First().Message);
                });
        }

        [Fact]
        public void LoadAllEntitiesInViewTest()
        {
            CQB<IEnumerable<example_pg_v_demo>>()
                .Arrange(db => new LoadEntityQuery<example_pg_v_demo>())
                .ActAndAssert((result, ah) =>
                {
                    var list = result.Data.ToList();
                    Assert.Single(list);
                    Assert.Equal("It is working!", list.First().Message);
                });
        }

        [Fact]
        public void LoadEntityWithFilterInViewTest()
        {
            var filter = new RootFilter(example_pg_v_demo.Cols.Message)
            {
                Value = "It is working!"
            };
            CQB<IEnumerable<example_pg_v_demo>>()
                .Arrange(db => new LoadEntityQuery<example_pg_v_demo>(filter))
                .ActAndAssert((result, ah) =>
                {
                    var list = result.Data.ToList();
                    Assert.Single(list);
                    Assert.Equal("It is working!", list.First().Message);
                });
        }

        [Fact]
        public void LoadByIdTest()
        {
            CQB<IEnumerable<example_pg_demo_table>>()
                .Arrange(db =>
                {
                    var singleEntry = db.Select<example_pg_demo_table>().ExecuteSingle();
                    return new LoadEntityQuery<example_pg_demo_table>(singleEntry.Id);
                })
                .ActAndAssert((result, ah) =>
                {
                    var list = result.Data.ToList();
                    Assert.Single(list);
                    Assert.Equal("It is working!", list.First().Message);
                });
        }

        [Fact]
        public void LoadByOtherTest()
        {
            CQB<IEnumerable<example_pg_demo_table>>()
                .Arrange(db =>
                {
                    var singleEntry = db.Select<example_pg_demo_table>().ExecuteSingle();
                    var filter = new RootFilter(example_pg_demo_table.Cols.Message)
                    {
                        Value = singleEntry.Message
                    };
                    return new LoadEntityQuery<example_pg_demo_table>(filter);
                })
                .ActAndAssert((result, ah) =>
                {
                    var list = result.Data.ToList();
                    Assert.Single(list);
                    Assert.Equal("It is working!", list.First().Message);
                });
        }

        [Fact]
        public void LoadWithDateOnlyNullableTest()
        {
            CQB<IEnumerable<example_pg_demo_table>>()
                .Arrange(db =>
                {
                    db.Insert(new example_pg_demo_table
                    {
                        Id = Guid.NewGuid(), Message = "hat Ende", DateTest = new DateOnly(2023, 1, 1),
                        DateEndTest = new DateOnly(2024, 1, 1), Status = "geht es?"
                    });
                    return new LoadEntityQuery<example_pg_demo_table>();
                })
                .ActAndAssert((result, ah) =>
                {
                    Assert.NotNull(result.SqlCommand);
                    Assert.NotEmpty(result.SqlCommand!);
                });
        }
    }
}
