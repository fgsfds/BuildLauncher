using Mods.Helpers;

namespace Tests
{
    public class VersionCompareTest
    {
        [Theory]
        [InlineData("1.1", "==1.1")]
        [InlineData("1.1", ">=1.1")]
        [InlineData("1.1", ">=1.0")]
        [InlineData("1.1", "<=1.1")]
        [InlineData("1.1", "<=1.2")]
        [InlineData("1.1", ">1.0")]
        [InlineData("1.1", "<1.3")]
        public void CompareTest(string v1, string v2)
        {
            var result = VersionComparer.Compare(v1, v2);

            Assert.True(result);
        }

        [Theory]
        [InlineData("1.1-a1", "==1.1-a1")]
        [InlineData("1.1-a1", ">=1.1-a1")]
        [InlineData("1.1-a1", ">=1.1-a0")]
        [InlineData("1.1-a1", "<=1.1-a1")]
        [InlineData("1.1-a1", "<=1.1-a2")]
        [InlineData("1.1-a1", ">1.1-a0")]
        [InlineData("1.1-a1", "<1.1-a3")]
        public void CompareTest2(string v1, string v2)
        {
            var result = VersionComparer.Compare(v1, v2);

            Assert.True(result);
        }
    }
}
