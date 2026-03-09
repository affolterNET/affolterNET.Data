using System.Collections.Generic;
using System.Linq;
using affolterNET.Data.Models.Filters;
using affolterNET.Data.Queries;
using ExamplePgUserDate.Data;
using Xunit;

namespace ExamplePgUserDate.IntegrationTest.Queries
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
            CQB<IEnumerable<example_pg_user_date_demo_table>>()
                .Arrange(db => new LoadEntityQuery<example_pg_user_date_demo_table>())
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
            var filter = new RootFilter(example_pg_user_date_demo_table.Cols.Message)
            {
                Value = "It is working!"
            };
            CQB<IEnumerable<example_pg_user_date_demo_table>>()
                .Arrange(db => new LoadEntityQuery<example_pg_user_date_demo_table>(filter))
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
            CQB<IEnumerable<example_pg_user_date_v_demo>>()
                .Arrange(db => new LoadEntityQuery<example_pg_user_date_v_demo>())
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
            var filter = new RootFilter(example_pg_user_date_v_demo.Cols.Message)
            {
                Value = "It is working!"
            };
            CQB<IEnumerable<example_pg_user_date_v_demo>>()
                .Arrange(db => new LoadEntityQuery<example_pg_user_date_v_demo>(filter))
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
            CQB<IEnumerable<example_pg_user_date_demo_table>>()
                .Arrange(db =>
                {
                    var singleEntry = db.Select<example_pg_user_date_demo_table>().ExecuteSingle();
                    return new LoadEntityQuery<example_pg_user_date_demo_table>(singleEntry.Id);
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
            CQB<IEnumerable<example_pg_user_date_demo_table>>()
                .Arrange(db =>
                {
                    var singleEntry = db.Select<example_pg_user_date_demo_table>().ExecuteSingle();
                    var filter = new RootFilter(example_pg_user_date_demo_table.Cols.Message)
                    {
                        Value = singleEntry.Message
                    };
                    return new LoadEntityQuery<example_pg_user_date_demo_table>(filter);
                })
                .ActAndAssert((result, ah) =>
                {
                    var list = result.Data.ToList();
                    Assert.Single(list);
                    Assert.Equal("It is working!", list.First().Message);
                });
        }
    }
}
