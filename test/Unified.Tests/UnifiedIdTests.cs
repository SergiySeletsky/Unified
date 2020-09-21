using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Xunit;

namespace Unified.Tests
{
    public class UnifiedIdTests
    {
        [Fact]
        public void UnifiedIdTest()
        {
            var stl = Guid.NewGuid().ToString().Length;
            var id = new UnifiedId();
            Assert.Equal(UnifiedId.Empty, id);

            id = new UnifiedId(long.MinValue);
            Assert.Equal("8000000000000", id);

            id = new UnifiedId(ulong.MaxValue - 1);
            Assert.Equal("FVVVVVVVVVVVU", id);

            id = new UnifiedId("FVVVVVVVVVVVU");
            Assert.Equal(ulong.MaxValue - 1, id.ToUInt64());

            Assert.Throws<FormatException>(() => new UnifiedId("FVVVVVVVVVVVUN"));
            Assert.Throws<FormatException>(() => new UnifiedId("FVVVVVVVVVVVu"));
            Assert.Throws<FormatException>(() => new UnifiedId("FVVVVVVVVVVVW"));
        }

        [Fact]
        public void PartitionKeyTest()
        {
            var id = new UnifiedId("FVSRVVVVVVVVU");
            Assert.Equal("FVS", id.PartitionKey());
            Assert.Equal("FVSR", id.PartitionKey(4));
            Assert.Throws<ArgumentOutOfRangeException>(() => id.PartitionKey(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => id.PartitionKey(20));
        }

        [Fact]
        public void PartitionNumberAsStringTest()
        {
            var id = new UnifiedId("FVSRVVVVVVVVU");
            Assert.Equal("9997", id.PartitionNumberAsString());
            Assert.Equal("099", id.PartitionNumberAsString(100));
            Assert.Equal("98", id.PartitionNumberAsString(99));
            Assert.Equal("2", id.PartitionNumberAsString(3));
            Assert.Throws<ArgumentOutOfRangeException>(() => id.PartitionNumberAsString(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => id.PartitionNumberAsString(uint.MaxValue));
        }

        [Fact]
        public void PartitionNumberTest()
        {
            var id = new UnifiedId("FVSRVVVVVVVVU");
            Assert.Equal(32761U, id.PartitionNumber());
            Assert.Equal(99U, id.PartitionNumber(100));
            Assert.Equal(98U, id.PartitionNumber(99));
            Assert.Equal(2U, id.PartitionNumber(3));
            Assert.Throws<ArgumentOutOfRangeException>(() => id.PartitionNumber(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => id.PartitionNumber(uint.MaxValue));
        }

        [Fact]
        public void ParseTest()
        {
            var id = new UnifiedId("FVVVVVVVVVVVU");
            var parsedId = UnifiedId.Parse(id);
            Assert.Equal(id, parsedId);

            Assert.Equal(UnifiedId.Empty, UnifiedId.Parse(string.Empty));
            Assert.Throws<FormatException>(() => UnifiedId.Parse("VVVVVVVVVVVVV"));
            Assert.Throws<FormatException>(() => UnifiedId.Parse("FVVVVVVVVVVVUN"));
            Assert.Throws<FormatException>(() => UnifiedId.Parse("FVVVVVVVVVVVu"));
            Assert.Throws<FormatException>(() => UnifiedId.Parse("FVVVVVVVVVVVW"));
        }

        [Fact]
        public void TryParseTest()
        {
            var id = new UnifiedId("FVVVVVVVVVVVU");
            var success = UnifiedId.TryParse(id, out var parsedId);
            Assert.True(success);
            Assert.Equal(id, parsedId);

            Assert.False(UnifiedId.TryParse(string.Empty, out parsedId));
            Assert.Equal(UnifiedId.Empty, parsedId);
            Assert.False(UnifiedId.TryParse("VVVVVVVVVVVVV", out parsedId));
            Assert.Equal(UnifiedId.Empty, parsedId);
            Assert.False(UnifiedId.TryParse("FVVVVVVVVVVVUN", out parsedId));
            Assert.Equal(UnifiedId.Empty, parsedId);
            Assert.False(UnifiedId.TryParse("FVVVVVVVVVVVu", out parsedId));
            Assert.Equal(UnifiedId.Empty, parsedId);
            Assert.False(UnifiedId.TryParse("FVVVVVVVVVVVW", out parsedId));
            Assert.Equal(UnifiedId.Empty, parsedId);
        }

        [Fact]
        public void NewIdTest()
        {
            var id = UnifiedId.NewId();
            Assert.NotEqual(UnifiedId.Empty, id);
        }

        [Fact]
        public void FromBytesTest()
        {
            var id = UnifiedId.FromBytes(new byte[] { 10 });
            Assert.NotEqual(UnifiedId.Empty, id);
            var array = Array.Empty<byte>();
            Assert.Throws<ArgumentOutOfRangeException>(() => UnifiedId.FromBytes(array));
#pragma warning disable CS8604 // Possible null reference argument.
            array = null;
            Assert.Throws<ArgumentNullException>(() => UnifiedId.FromBytes(array));
#pragma warning restore CS8604 // Possible null reference argument.
        }

        [Fact]
        public void FromGuidTest()
        {
            var id = UnifiedId.FromGuid(Guid.NewGuid());
            Assert.NotEqual(UnifiedId.Empty, id);
            Assert.Throws<ArgumentOutOfRangeException>(() => UnifiedId.FromGuid(Guid.Empty));
        }

        [Fact]
        public void FromUInt64Test()
        {
            var id = UnifiedId.FromUInt64(ulong.MaxValue);
            Assert.NotEqual(ulong.MaxValue, id.ToUInt64());
            Assert.Throws<ArgumentOutOfRangeException>(() => UnifiedId.FromUInt64(0));
        }

        [Fact]
        public void FromInt64Test()
        {
            var id = UnifiedId.FromInt64(long.MinValue);
            Assert.NotEqual(long.MinValue, id.ToInt64());
            Assert.Throws<ArgumentOutOfRangeException>(() => UnifiedId.FromInt64(0));
        }

        [Fact]
        public void FromStringTest()
        {
            var id = UnifiedId.FromString(nameof(UnifiedId));
            Assert.NotEqual(nameof(UnifiedId), id.ToString());
            Assert.Throws<ArgumentNullException>(() => UnifiedId.FromString(string.Empty));
        }

        [Fact]
        public void GetObjectDataTest()
        {
            var id = UnifiedId.NewId();

            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, id);
                ms.Position = 0;

                var deserialized = formatter.Deserialize(ms);
                Assert.Equal(id, deserialized);
            }
        }

        [Fact]
        public void ToStringTest()
        {
            var testId = "FVVVVVVVVVVVU";
            var id = new UnifiedId(testId);
            Assert.Equal(testId, id.ToString());
        }

        [Fact]
        public void ToUInt64Test()
        {
            var id = new UnifiedId(ulong.MaxValue);
            Assert.Equal(ulong.MaxValue, id.ToUInt64());
        }

        [Fact]
        public void ToInt64Test()
        {
            var id = new UnifiedId(long.MinValue);
            Assert.Equal(long.MinValue, id.ToInt64());
        }

        [Fact]
        public void EqualsTest()
        {
            var id = UnifiedId.Parse("FVVVVVVVVVVVV");

            Assert.True(id.Equals("FVVVVVVVVVVVV"));
            Assert.True(id.Equals(ulong.MaxValue));
            Assert.True(id.Equals(id.ToInt64()));
            Assert.True(id.Equals(id));

            Assert.True(id.Equals((object)"FVVVVVVVVVVVV"));
            Assert.True(id.Equals((object)ulong.MaxValue));
            Assert.True(id.Equals((object)id.ToInt64()));
            Assert.True(id.Equals((object)id));
            Assert.False(id.Equals(new object()));

            Assert.True(id == "FVVVVVVVVVVVV");
            Assert.True(id == ulong.MaxValue);
            Assert.True(id == id.ToInt64());
            Assert.True(id == new UnifiedId(id.ToUInt64()));

            Assert.False(id != "FVVVVVVVVVVVV");
            Assert.False(id != ulong.MaxValue);
            Assert.False(id != id.ToInt64());
            Assert.False(id != new UnifiedId(id.ToUInt64()));

            Assert.True("FVVVVVVVVVVVV" == id);
            Assert.True(ulong.MaxValue == id);
            Assert.True(id.ToInt64() == id);
            Assert.True(new UnifiedId(id.ToUInt64()) == id);

            Assert.False("FVVVVVVVVVVVV" != id);
            Assert.False(ulong.MaxValue != id);
            Assert.False(id.ToInt64() != id);
            Assert.False(new UnifiedId(id.ToUInt64()) != id);
        }

        [Fact]
        public void CastTest()
        {
            UnifiedId id = "FVVVVVVVVVVVV";
            Assert.True(id == (UnifiedId)"FVVVVVVVVVVVV");

            id = ulong.MaxValue;
            Assert.True(id == ulong.MaxValue);

            id = id.ToInt64();
            Assert.True(id == id.ToInt64());

            string str = id;
            Assert.True(str == "FVVVVVVVVVVVV");

            ulong num = id;
            Assert.True(num == ulong.MaxValue);

            long num2 = id;
            Assert.True(num2 == id);
        }

        [Fact]
        public void GetHashCodeTest()
        {
            UnifiedId id = "FVVVVVVVVVVVV";
            UnifiedId id2 = ulong.MaxValue;
            Assert.Equal(id.GetHashCode(), id2.GetHashCode());
        }

        [Fact]
        public void CompareToTest()
        {
            UnifiedId id = "FVVVVVVVVVVVU";
            UnifiedId id2 = ulong.MaxValue - 1;
            Assert.Equal(0, id.CompareTo(id2));
            Assert.Equal(0, id.CompareTo(id2.ToString()));
            Assert.Equal(1, id.CompareTo(string.Empty));
            Assert.Equal(0, id.CompareTo(id2.ToInt64()));
            Assert.Equal(1, id.CompareTo(new object()));
            Assert.Equal(-1, id.CompareTo(ulong.MaxValue));
            Assert.Equal(-1, id.CompareTo(new UnifiedId(ulong.MaxValue)));
            Assert.Equal(1, id.CompareTo(UnifiedId.Empty));
        }

        [Fact]
        public void CloneTest()
        {
            var id = UnifiedId.NewId();
            Assert.Equal(id, id.Clone());
        }

        [Fact]
        public void ComparatorsTest()
        {
            var id1 = new UnifiedId(1);
            var id2 = new UnifiedId(2);
            Assert.True(id1 < id2);
            Assert.True(id1 <= id2);
            Assert.True(id2 > id1);
            Assert.True(id2 >= id1);

            Assert.False(id1 > id2);
            Assert.False(id1 >= id2);
            Assert.False(id2 < id1);
            Assert.False(id2 <= id1);
        }
    }
}
