using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Unified.Tests
{
    public class PartitioningTests
    {
        private readonly ITestOutputHelper output;

        public PartitioningTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void TestPartition()
        {
            var dict = new Dictionary<ulong, List<UnifiedId>>();
            var all = 100000;

            var partitions = 16U;
            for (var i = 0; i <= all; i++)
            {
                var x = UnifiedId.NewId();
                var p = x.PartitionNumber(partitions);
                if (!dict.ContainsKey(p))
                {
                    dict.Add(p, new List<UnifiedId>());
                }

                dict[p].Add(x);
            }

            float max = dict.Max(x => x.Value.Count);
            float min = dict.Min(x => x.Value.Count);

            var diff = max - min;

            var maxp = (max / (all / partitions)) * 100;
            var minp = (min / (all / partitions)) * 100;
            var diffp = (diff / (all / partitions)) * 100;

            output.WriteLine($"Elements tested: {all} partition size: {all / partitions}");
            output.WriteLine($"MAX: {max:N1}({maxp:N1}%) MIN: {min:N1}({minp:N1}%) DIFF: {diff:N1}({diffp:N1}%)");

            Assert.True(diffp < 10);
        }
    }
}
