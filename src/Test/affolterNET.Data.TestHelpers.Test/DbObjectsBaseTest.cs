using System;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace affolterNET.Data.TestHelpers.Test
{
    public class DbObjectsBaseTest
    {
        private readonly ITestOutputHelper _output;

        public DbObjectsBaseTest(ITestOutputHelper output)
        {
            _output = output;
        }
        
        [Fact]
        public void Get_DifferentObjects_SameName_Test()
        {
            var testee = new DbObjects(_output);
            testee.Create1("a");
            Assert.Throws<ArgumentException>(() => testee.Create2("a"));
        }
        
        [Fact]
        public void Get_SameObjects_SameName_Test()
        {
            var testee = new DbObjects(_output);
            testee.Create1("a", "erster");
            Assert.Throws<ArgumentException>(() => testee.Create1("a", "zweiter"));
        }

        [Fact]
        public void GetAllTest()
        {
            var testee = new DbObjects(_output);
            
            testee.Create1("a");
            testee.Create1("b");
            testee.Create1("c");
            
            testee.Create2("d");
            testee.Create2("e");
            testee.Create2("f");
            
            testee.Create3("g");
            testee.Create3("h");
            testee.Create3("i");

            var a = testee.GetAll<Obj2>().ToList();
            Assert.NotNull(a);
            Assert.Equal(3, a.Count);
            Assert.Contains("d", a.Select(o => o.Name));
            Assert.Contains("e", a.Select(o => o.Name));
            Assert.Contains("f", a.Select(o => o.Name));
        }
        
        [Fact]
        public void ClosureTest() {
            var testee = new DbObjects(_output);
            var t1 = testee.Create1WithParams("a", 1, true);
            Assert.Equal(1, t1.Num);
            Assert.True(t1.Bol);
            var t2 = testee.Create1WithParams("b", 2, true);
            Assert.Equal(2, t2.Num);
            Assert.True(t2.Bol);
            var t3 = testee.Create1WithParams("d",3, false);
            Assert.Equal(3, t3.Num);
            Assert.False(t3.Bol);
            var t4 = testee.Create1WithParams("e", 4, true);
            Assert.Equal(4, t4.Num);
            Assert.True(t4.Bol);
            var t5 = testee.Create1WithParams("g", 5, false);
            Assert.Equal(5, t5.Num);
            Assert.False(t5.Bol);
            var t6 = testee.Create1WithParams("h", 6, false);
            Assert.Equal(6, t6.Num);
            Assert.False(t6.Bol);
            var t7 = testee.Create1WithParams("i", 7, true);
            Assert.Equal(7, t7.Num);
            Assert.True(t7.Bol);
        }
    }

    public class Obj1
    {
        public string? Name { get; set; }

        public int Num { get; set; }

        public bool Bol { get; set; }
    }

    public class Obj2
    {
        public string? Name { get; set; }
    }

    public class Obj3
    {
        public string? Name { get; set; }
    }

    public class DbObjects : DbObjectsBase
    {
        public const string Test1 = "test 1";
        public const string Test2 = "test 2";
        
        public DbObjects(ITestOutputHelper output): base(output)
        {}

        public Obj1 Create1WithParams(string name, int num, bool check, string? objname = null)
        {
            objname ??= name;
            return Set(() => new Obj1 {Name = check ? Test1 : Test2, Num = num, Bol = check}, _ => name);
        }

        public void Create1(string name, string? objname = null)
        {
            objname ??= name;
            Set(() => new Obj1 { Name = objname }, _ => name);
        }

        public void Create2(string name, string? objname = null)
        {
            objname ??= name;
            Set(() => new Obj2 { Name = objname }, _ => name);
        }

        public void Create3(string name, string? objname = null)
        {
            objname ??= name;
            Set(() => new Obj3 { Name = objname }, _ => name);
        }
    }
}