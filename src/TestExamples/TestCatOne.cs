namespace NUnitContrib.Web.TestExamples
{
    using System;
    using System.Threading;
    using System.Web;
    using NUnit.Framework;

    [TestFixture]
    public class TestCatOne
    {
        [Test(Description = "Description One"), Category("Cat One")]
        public void ShortTest()
        {
            Console.WriteLine("Hello");
            Assert.IsTrue(true);
            Console.WriteLine("Bye");
        }

        [Test, Category("Cat One")]
        public void LongTest()
        {
            Console.WriteLine("Hola");
            Thread.Sleep(2000);
            Assert.IsTrue(true);
        }

        [Test, Category("Cat One")]
        public void HttpContextNotNull()
        {
            Assert.NotNull(HttpContext.Current);
        }

        [Test]
        public void HttpSessionNotNull()
        {
            HttpContext.Current.Session["IShouldNotBeNull"] = true;
            HttpContext.Current.Session["IShouldHaveAName"] = "Your name";
            Assert.IsTrue((bool)HttpContext.Current.Session["IShouldNotBeNull"]);
            Assert.AreEqual(HttpContext.Current.Session["IShouldHaveAName"], "Your name");
        }

        [Test]
        public void ThisTestShouldFail()
        {
            Assert.AreEqual(2, 3);
        }

        [Test, Ignore("The reason to ignore this test")]
        public void ThisTestShouldBeIgnored()
        {
            Assert.IsTrue(false);
        }
    }
}
