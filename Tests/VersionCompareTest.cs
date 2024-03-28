using System.Numerics;

namespace Tests
{
    public class VersionCompareTest
    {
        [Fact]
        public void CompareTest()
        {
            var v1 = "1.1-rc3";
            var v2 = "<=1.1-rc3";

            List<int> expected = new(2);

            if (v2.StartsWith("=="))
            {
                v2 = v2[2..];
                expected.Add(0);
            }
            else if (v2.StartsWith(">="))
            {
                v2 = v2[2..];
                expected.Add(0);
                expected.Add(1);
            }
            else if (v2.StartsWith("<="))
            {
                v2 = v2[2..];
                expected.Add(0);
                expected.Add(-1);

            }
            else if (v2.StartsWith('>'))
            {
                v2 = v2[1..];
                expected.Add(1);

            }
            else if (v2.StartsWith('<'))
            {
                v2 = v2[1..];
                expected.Add(-1);
            }
            else
            {
                throw new NotImplementedException();
            }
            
            var result = string.CompareOrdinal(v1, v2);

            var b = expected.Contains(result);
        }
    }
}
