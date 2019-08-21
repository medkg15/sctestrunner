using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Xunit;

namespace XunitTestExamples
{
    public class TestCatOne
    {
        [Fact]
        public void ShortTest()
        {
            Console.WriteLine("Hello");
            Assert.True(true);
            Console.WriteLine("Bye");
        }

        [Fact]
        public void LongTest()
        {
            Console.WriteLine("Hola");
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
    }
}
