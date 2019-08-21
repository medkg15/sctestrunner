namespace TestExamplesXunit
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;

    public class TestCatTwo
    {
        [Fact]
        public void ShortTest()
        {
            Assert.True(true);
        }

        [Fact]
        public void ShouldBeCatTwoAndThree()
        {
            Assert.True(true);
        }

        [Fact]
        public void ShouldThrowException()
        {
            MethodThrowsException();
        }

        private void MethodThrowsException()
        {
            //add to stack trace to test formatting
            throw new Exception("This exception is a test");
        }

        [Fact]
        public async Task ShouldCatchException()
        {
            await Assert.ThrowsAsync<Exception>(() => throw new Exception("This exception is another test"));
        }

        [Fact]
        public void LongTest()
        {
            Thread.Sleep(3000);
            Assert.True(true);
        }
    }
}
