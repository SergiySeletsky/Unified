using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Unified.Tests
{
    public class UnifiedIdTests
    {
        private readonly ITestOutputHelper output;

        public UnifiedIdTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void HexTest()
        {
            var val = "AGQ8BJPM1IA7V";
            var id = UnifiedId.Parse(val);
            var val2 = id.ToString();

            Assert.Equal(val, val2);
        }

        [Fact]
        public void ParseTest()
        {
            for (var i = 0; i <= 10000; i++)
            {
                var id = UnifiedId.NewId();
                var parsedId = UnifiedId.Parse(id);
                Assert.Equal(id, parsedId);
            }
        }

        [Fact]
        public void TestPartition()
        {
            var dict = new Dictionary<ulong, List<UnifiedId>>();
            var all = 10000000;
            for (var i = 0; i <= all; i++)
            {
                var x = UnifiedId.NewId();
                var p = x.PartitionKeyNumberUInt64(512);
                if (!dict.ContainsKey(p))
                {
                    dict.Add(p, new List<UnifiedId>());
                }

                dict[p].Add(x);
            }

            float max = dict.Max(x => x.Value.Count);
            float min = dict.Min(x => x.Value.Count);

            var diff = max - min;

            var partitions = 512;
            var maxp = (max / (all / partitions)) * 100;
            var minp = (min / (all / partitions)) * 100;
            var diffp = (diff / (all / partitions)) * 100;

            output.WriteLine($"Elements tested: {all} partition size: {all / 100}");
            output.WriteLine($"MAX: {max:N1}({maxp:N1}%) MIN: {min:N1}({minp:N1}%) DIFF: {diff:N1}({diffp:N1}%)");

            Assert.True(diffp < 10);
        }
    }
}
