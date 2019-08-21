using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Xunit;
using Xunit.Abstractions;

namespace TestExamplesXunit
{
    public class TestCatOne
    {
        private readonly ITestOutputHelper output;

        public TestCatOne(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void ShortTest()
        {
            output.WriteLine("Hello");
            Assert.True(true);
            output.WriteLine("Bye");
        }

        [Fact]
        public void LongTest()
        {
            output.WriteLine("Hola");
            Thread.Sleep(2000);
            Assert.True(true);
        }

        [Fact]
        public void HttpContextNotNull()
        {
            Assert.NotNull(HttpContext.Current);
        }

        [Fact]
        public void HttpSessionNotNull()
        {
            HttpContext.Current.Session["IShouldNotBeNull"] = true;
            HttpContext.Current.Session["IShouldHaveAName"] = "Your name";
            Assert.True((bool)HttpContext.Current.Session["IShouldNotBeNull"]);
            Assert.Equal("Your name", HttpContext.Current.Session["IShouldHaveAName"]);
        }

        [Fact]
        public void ThisTestShouldFail()
        {
            Assert.Equal(2, 3);
        }
        
        [Fact(Skip ="The reason to ignore this test")]
        public void ThisTestShouldBeIgnored()
        {
            Assert.True(false);
        }

        [Theory]
        [InlineData("1")]
        [InlineData("2")]
        [InlineData("3")]
        public void TheoryWorks(string input)
        {
            Assert.True(int.TryParse(input, out _));
        }
    }
}
