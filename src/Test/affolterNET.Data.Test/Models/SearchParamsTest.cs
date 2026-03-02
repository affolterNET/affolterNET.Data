using affolterNET.Data.Models;
using Newtonsoft.Json;
using Xunit;

namespace affolterNET.Data.Test.Models
{
    public class SearchParamsTest
    {
        [Fact]
        public void CanDeserializeEmptyObjectTest()
        {
            var json = "{}";
            var p = JsonConvert.DeserializeObject<SearchParams>(json);
            Assert.NotNull(p);
        }

        [Fact]
        public void OverviewLayoutDeserializeTest()
        {
            var json =
                "{ GridLayout: { Columns: [ { prefix: '', column: 'ein' }, { prefix: '', column: 'zwei' }, { prefix: '', column: 'drei' } ] } }";
            var p = JsonConvert.DeserializeObject<SearchParams>(json)!;
            Assert.NotNull(p.GridLayout);
            Assert.NotNull(p.GridLayout.Columns);
            Assert.Equal(3, p.GridLayout.Columns.Count);
        }

        [Fact]
        public void RootFilterNotNullTest()
        {
            var json = "{}";
            var p = JsonConvert.DeserializeObject<SearchParams>(json);
            Assert.NotNull(p);
            Assert.NotNull(p.RootFilter);
        }

        [Fact]
        public void SortOrderDeserializeTest()
        {
            var json = @"
            {
              sortOrder: [ { attribute: { prefix: 'i', column: 'Email' }, desc: true } ]
            }";
            var p = JsonConvert.DeserializeObject<SearchParams>(json)!;
            Assert.NotNull(p.SortOrder);
            Assert.Single(p.SortOrder);
            Assert.True(p.SortOrder[0].Desc);
            Assert.NotNull(p.SortOrder[0].Attribute);
            Assert.Equal("i", p.SortOrder[0].Attribute!.Prefix);
            Assert.Equal("Email", p.SortOrder[0].Attribute!.Column);
        }
    }
}