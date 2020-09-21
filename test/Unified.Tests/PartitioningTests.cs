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
        public void PartitionNumberTest()
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

        [Fact]
        public void PartitionNumberStringTest()
        {
            var dict = new Dictionary<string, List<UnifiedId>>();
            var all = 100000;

            var partitions = 16U;
            for (var i = 0; i <= all; i++)
            {
                var x = UnifiedId.NewId();
                var p = x.PartitionNumberAsString(partitions);
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

        [Fact]
        public void PartitionKeyTest()
        {
            // Let's emulate the partitioned database.
            var db = new Dictionary<string, List<UnifiedId>>();

            // We will use 10M records, just to execute it fast.
            var all = 10000000;
            for (var i = 0; i <= all; i++)
            {
                // Generate random Id.
                var id = UnifiedId.NewId();

                // Get it's partition key. Number of partitions could be customized, default 16K.
                var partition = id.PartitionKey(1);

                // Initialize partitions in your DB.
                if (!db.ContainsKey(partition))
                {
                    db.Add(partition, new List<UnifiedId>());
                }

                // Add values to partitions.
                db[partition].Add(id);
            }

            var partitions = 16U;

            float max = db.Max(x => x.Value.Count);
            float min = db.Min(x => x.Value.Count);

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
