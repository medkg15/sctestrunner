namespace NUnitContrib.Web.TestExamples
{
    using System;
    using System.Threading;
    using NUnit.Framework;

    [TestFixture, Category("Cat Two")]
    public class TestCatTwo
    {
        [Test, Category("Cat Two")]
        public void ShortTest()
        {
            Assert.IsTrue(true);
        }

        [Test, Category("Cat Three")]
        public void ShouldBeCatTwoAndThree()
        {
            Assert.IsTrue(true);
        }

        [Test]
        public void ShouldThrowException()
        {
            throw new Exception("This exception is a test");
        }

        [Test, ExpectedException]
        public void ShouldCatchException()
        {
            throw new Exception("This exception is another test");
        }

        [Test, Category("Cat Two")]
        public void LongTest()
        {
            Thread.Sleep(3000);
            Assert.IsTrue(true);
        }
    }
}
