using NUnit.Framework;

namespace TestExamples2
{
    [TestFixture]
    [Category("Cat Three")]
    public class TestCatThree
    {
        [Test(Description = "More test")]
        public void MoreTest()
        {
            Assert.IsTrue(true);
        }
    }
}
