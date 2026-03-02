using affolterNET.Data.Models.Filters;
using Newtonsoft.Json;
using Xunit;

namespace affolterNET.Data.Test.Models.Filters
{
    public class SqlAttributeTest
    {
        [Fact]
        public void FromEmptyJsonTest()
        {
            var json = "{}";
            var attr = JsonConvert.DeserializeObject<SqlAttribute>(json)!;
            Assert.Null(attr.Column);
            Assert.Null(attr.Prefix);
        }

        [Theory]
        [InlineData("{ Column: '[Test]' }", "Test", null, "[Test]", "@1Test", "1Test")]
        [InlineData("{ Column: '[Test]', Prefix: 'i' }", "Test", "i", "i.[Test]", "@i1Test", "i1Test")]
        public void FromJsonTests(string json, string expCol, string expPrefix, string expString, string expParamIdent, string expParam)
        {
            var attr = JsonConvert.DeserializeObject<SqlAttribute>(json)!;
            Assert.Equal(expCol, attr.Column);
            Assert.Equal(expPrefix, attr.Prefix);
            Assert.Equal(expString, attr.ToString());
            Assert.Equal(expParamIdent, attr.ToSqlParamIdentifier(1));
            Assert.Equal(expParam, attr.ToParam(1));
        }

        [Fact]
        public void StripsSquareBracketsTest()
        {
            var attr = new SqlAttribute("[Test]");
            Assert.Equal("Test", attr.Column);
        }
    }
}