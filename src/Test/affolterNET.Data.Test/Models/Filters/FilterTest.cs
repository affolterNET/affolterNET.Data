using System;
using System.Linq;
using affolterNET.Data.Models.Filters;
using Newtonsoft.Json;
using Xunit;

namespace affolterNET.Data.Test.Models.Filters
{
    public class FilterTest
    {
        private T Create<T>(string? json = null)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                var inst = Activator.CreateInstance<T>();
                return inst;
            }

            return JsonConvert.DeserializeObject<T>(json)!;
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void EmptyFilterTest(bool fromJson)
        {
            var json = fromJson ? "{}" : string.Empty;
            var rootFilter = Create<RootFilter>(json);
            Assert.Equal(string.Empty, rootFilter.ToString());
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void SingleFilterTest(bool fromJson)
        {
            var json = fromJson ? "{ Attribute: { column: 'Name', prefix: 'i' } }" : string.Empty;
            var rootFilter = Create<RootFilter>(json);
            Assert.NotNull(rootFilter);
            if (fromJson)
            {
                Assert.NotNull(rootFilter.Attribute);
                Assert.Equal("Name", rootFilter.Attribute!.Column);
                Assert.Equal("i", rootFilter.Attribute.Prefix);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void MultipleFiltersTest(bool fromJson)
        {
            var json = fromJson
                ? @"{
                Filters: [ 
                    { Attribute: { column: 'Name', prefix: 'i' }, Comparer: 'equals', FilterType: 'text' },
                    { Attribute: { column: 'Test', prefix: 'i' }, Comparer: 'equals', FilterType: 'text' }
                ],
                UseAnd: true  }"
                : string.Empty;
            var rootFilter = Create<RootFilter>(json);
            Assert.NotNull(rootFilter);
            if (fromJson)
            {
                Assert.Null(rootFilter.Attribute);
                Assert.Equal(2, rootFilter.Filters.Count);
                Assert.Equal("where (i.[Name] = @i2Name and i.[Test] = @i3Test)", rootFilter.ToString());
            }
        }

        [Theory]
        [InlineData(
            "contains",
            "startsWith",
            "where (i.[Vorname] like '%' + @i2Vorname + '%' and i.[Nachname] like @i3Nachname + '%')")]
        [InlineData(
            "endsWith",
            "equals",
            "where (i.[Vorname] like '%' + @i2Vorname and i.[Nachname] = @i3Nachname)")]
        [InlineData(
            "notEqual",
            "notContains",
            "where ((i.[Vorname] is null or i.[Vorname] <> @i2Vorname) and (i.[Nachname] is null or i.[Nachname] not like '%' + @i3Nachname + '%'))")]
        public void TextFilterTest(string filterType1, string filterType2, string expected)
        {
            var json = @"
            {
                Filters: [
                    { Attribute: { column: 'Vorname', prefix: 'i' }, FilterType: 'text', Comparer: '" + filterType1 +
                       @"' },
                    { Attribute: { column: 'Nachname', prefix: 'i' }, FilterType: 'text', Comparer: '" + filterType2 +
                       @"' }
                ],
            }";
            var rootFilter = Create<RootFilter>(json);
            Assert.True(rootFilter.IsValid());
            Assert.Equal(2, rootFilter.Filters.Count);
            Assert.Equal(expected, rootFilter.ToString());
        }

        [Theory]
        [InlineData(
            "equals",
            "notEqual",
            "where (i.[Alter] = @i2Alter and i.[Durchschnittsnote] <> @i3Durchschnittsnote)")]
        [InlineData(
            "lessThanOrEqual",
            "lessThan",
            "where (i.[Alter] <= @i2Alter and i.[Durchschnittsnote] < @i3Durchschnittsnote)")]
        [InlineData(
            "greaterThan",
            "greaterThanOrEqual",
            "where (i.[Alter] > @i2Alter and i.[Durchschnittsnote] >= @i3Durchschnittsnote)")]
        //public const string InRange = "inRange";
        public void NumberFilterTest(string filterType1, string filterType2, string expected)
        {
            var json = @"
            {
                Filters: [
                    { Attribute: { column: 'Alter', prefix: 'i' }, FilterType: 'number', Comparer: '" + filterType1 +
                       @"' },
                    { Attribute: { column: 'Durchschnittsnote', prefix: 'i' }, FilterType: 'number', Comparer: '" +
                       filterType2 +
                       @"' }
                ],
            }";
            var rootFilter = Create<RootFilter>(json);
            Assert.True(rootFilter.IsValid());
            Assert.Equal(2, rootFilter.Filters.Count);
            Assert.Equal(expected, rootFilter.ToString());
        }

        [Theory]
        [InlineData("equals", "notEqual", "where (i.[Geburtstag] = @i2Geburtstag and i.[Start] <> @i3Start)")]
        [InlineData("lessThan", "greaterThan", "where (i.[Geburtstag] < @i2Geburtstag and i.[Start] > @i3Start)")]
        //public const string InRange = "inRange";
        public void DateFilterTest(string filterType1, string filterType2, string expected)
        {
            var json = @"
            {
                Filters: [
                    { Attribute: { column: 'Geburtstag', prefix: 'i' }, FilterType: 'date', Comparer: '" +
                       filterType1 +
                       @"' },
                    { Attribute: { column: 'Start', prefix: 'i' }, FilterType: 'date', Comparer: '" + filterType2 +
                       @"' }
                ],
            }";
            var rootFilter = Create<RootFilter>(json);
            Assert.True(rootFilter.IsValid());
            Assert.Equal(2, rootFilter.Filters.Count);
            Assert.Equal(expected, rootFilter.ToString());
        }

        [Theory]
        [InlineData(false, "text", "equal")]
        [InlineData(true, "text", "equals")]
        [InlineData(true, "text", "notEqual")]
        [InlineData(true, "text", "contains")]
        [InlineData(true, "text", "notContains")]
        [InlineData(true, "text", "startsWith")]
        [InlineData(true, "text", "endsWith")]
        [InlineData(false, "date", "equal")]
        [InlineData(true, "date", "equals")]
        [InlineData(true, "date", "notEqual")]
        [InlineData(true, "date", "lessThan")]
        [InlineData(true, "date", "greaterThan")]
        [InlineData(true, "date", "inRange")]
        [InlineData(false, "number", "equal")]
        [InlineData(true, "number", "equals")]
        [InlineData(true, "number", "notEqual")]
        [InlineData(true, "number", "lessThan")]
        [InlineData(true, "number", "lessThanOrEqual")]
        [InlineData(true, "number", "greaterThan")]
        [InlineData(true, "number", "greaterThanOrEqual")]
        [InlineData(true, "number", "inRange")]
        public void IsComparerValidTest(bool isvalid, string filterType, string comparer)
        {
            var f = new RootFilter("Test") { Comparer = comparer, FilterType = filterType };
            Assert.Equal(isvalid, f.IsValid());
        }

        [Fact]
        public void GetAttributeNamesTest()
        {
            var filter = new RootFilter();
            filter.Filters.Add(new Filter("Abc", "i"));
            var subfilter1 = new Filter { UseAnd = false };
            subfilter1.Filters.Add(new Filter("Cba", "f"));
            subfilter1.Filters.Add(new Filter("Zyx", "e"));
            filter.Filters.Add(subfilter1);
            var subfilter2 = new Filter();
            subfilter1.Filters.Add(new Filter("Stadt", "f"));
            subfilter1.Filters.Add(new Filter("Land", "e"));
            filter.Filters.Add(subfilter2);

            var names = filter.GetAttributes().Select(a => a.Column).ToList();
            Assert.Contains("Abc", names);
            Assert.Contains("Cba", names);
            Assert.Contains("Zyx", names);
            Assert.Contains("Stadt", names);
            Assert.Contains("Land", names);
        }
        
        [Fact]
        public void GetAttributeNamesWithAddFilterTest()
        {
            var filter = new RootFilter();
            filter.AddFilter("Abc", null,"i");
            var subfilter1 = new Filter { UseAnd = false };
            subfilter1.AddFilter("Cba", null, "f");
            subfilter1.AddFilter("Zyx", null, "e");
            filter.AddFilter(subfilter1);
            var subfilter2 = new Filter();
            subfilter1.AddFilter("Stadt", null, "f");
            subfilter1.AddFilter("Land", null, "e");
            filter.AddFilter(subfilter2);

            var names = filter.GetAttributes().Select(a => a.Column).ToList();
            Assert.Contains("Abc", names);
            Assert.Contains("Cba", names);
            Assert.Contains("Zyx", names);
            Assert.Contains("Stadt", names);
            Assert.Contains("Land", names);
        }

        [Fact]
        public void NestedFiltersTest()
        {
            var json = @"
            {
                UseAnd: false,
                Filters: [
                    {
                        UseAnd: true,
                        Filters: [
                            { Attribute: { column: 'Vorname', prefix: 'i' }, Comparer: 'equals', FilterType: 'text' },
                            { Attribute: { column: 'Nachname', prefix: 'i' }, Comparer: 'equals', FilterType: 'text' }
                        ]
                    },
                    {
                        Filters: [
                            { Attribute: { column: 'Ort', prefix: 'i' }, Comparer: 'startsWith', FilterType: 'text' },
                        ]
                    }

                ],
            }";
            var rootFilter = Create<RootFilter>(json);
            Assert.NotNull(rootFilter);
            Assert.Equal(
                "where ((i.[Vorname] = @i3Vorname and i.[Nachname] = @i4Nachname) or (i.[Ort] like @i6Ort + '%'))",
                rootFilter.ToString());
        }
    }
}