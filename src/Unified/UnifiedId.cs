using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;

namespace Unified
{
    /// <summary>
    /// Represents Immutable Unified Identifier.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [DataContract]
    [Serializable]
    public struct UnifiedId : IComparable, ICloneable, ISerializable,
    IComparable<UnifiedId>, IEquatable<UnifiedId>,
    IComparable<string>, IEquatable<string>,
    IComparable<ulong>, IEquatable<ulong>,
    IComparable<long>, IEquatable<long>
    {
        /// <summary>
        /// Empty UnifiedId.
        /// </summary>
        public static readonly UnifiedId Empty;

        /// <summary>
        /// HEX x32 Length should be 13 symbols.
        /// </summary>
        private const int Length = 13;

        /// <summary>
        /// Shift for (x32 >> 5 == 1) dimensions.
        /// </summary>
        private const int X32Shift = 5;

        /// <summary>
        /// FNV x64 Prime https://en.wikipedia.org/wiki/Prime_number.
        /// </summary>
        private const ulong Prime = 1099511628211U;

        /// <summary>
        /// FNV x64 Offset basis.
        /// </summary>
        private const ulong Offset = 14695981039346656037U;

        // Set of symbols used for numeric dimensions encoding
        private static readonly char[] Symbols = new char[32]
        {
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F',
            'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V'
        };

        // Immutable member variable
        [DataMember]
        private readonly ulong hash;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnifiedId"/> struct.
        /// </summary>
        /// <param name="hash">Hash.</param>
        public UnifiedId(ulong hash)
        {
            this.hash = hash;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnifiedId"/> struct.
        /// </summary>
        /// <param name="hash">Hash.</param>
        public UnifiedId(long hash)
        {
            var bytes = BitConverter.GetBytes(hash);
            this.hash = BitConverter.ToUInt64(bytes, 0);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnifiedId"/> struct.
        /// </summary>
        /// <param name="hex">UnifiedId HEX.</param>
        public UnifiedId(string hex)
        {
            hash = Parse(hex);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnifiedId"/> struct.
        /// </summary>
        /// <param name="serializationInfo">SerializationInfo.</param>
        /// <param name="streamingContext">StreamingContext.</param>
        private UnifiedId(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            hash = serializationInfo.GetUInt64(nameof(hash));
        }

        /// <summary>
        /// Implicit convert to ulong.
        /// </summary>
        /// <param name="id">UnifiedId.</param>
        public static implicit operator ulong(UnifiedId id) => id.hash;

        /// <summary>
        /// Explicit convert from ulong.
        /// </summary>
        /// <param name="hash">ulong hash.</param>
        public static implicit operator UnifiedId(ulong hash) => new UnifiedId(hash);

        /// <summary>
        /// Implicit convert to long.
        /// </summary>
        /// <param name="id">UnifiedId.</param>
        public static implicit operator long(UnifiedId id) => id.ToInt64();

        /// <summary>
        /// Explicit convert from long.
        /// </summary>
        /// <param name="hash">long hash.</param>
        public static implicit operator UnifiedId(long hash) => new UnifiedId(hash);

        /// <summary>
        /// Implicit convert to string.
        /// </summary>
        /// <param name="id">UnifiedId.</param>
        public static implicit operator string(UnifiedId id) => id.ToString();

        /// <summary>
        /// Explicit convert from string.
        /// </summary>
        /// <param name="hex">ulong hash.</param>
        public static implicit operator UnifiedId(string hex) => Parse(hex);

        /// <summary>
        /// Equality operator.
        /// </summary>
        /// <param name="a">Left UnifiedId.</param>
        /// <param name="b">Right UnifiedId.</param>
        /// <returns>Result of equality boolean.</returns>
        public static bool operator ==(UnifiedId a, UnifiedId b)
        {
            return a.hash == b.hash;
        }

        /// <summary>
        /// Not equality operator.
        /// </summary>
        /// <param name="a">Left UnifiedId.</param>
        /// <param name="b">Right UnifiedId.</param>
        /// <returns>Result of equality boolean.</returns>
        public static bool operator !=(UnifiedId a, UnifiedId b)
        {
            return a.hash != b.hash;
        }

        /// <summary>
        /// Equality operator.
        /// </summary>
        /// <param name="a">Left UnifiedId.</param>
        /// <param name="b">Right HEX string.</param>
        /// <returns>Result of equality boolean.</returns>
        public static bool operator ==(UnifiedId a, string b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Not equality operator.
        /// </summary>
        /// <param name="a">Left UnifiedId.</param>
        /// <param name="b">Right HEX string.</param>
        /// <returns>Result of equality boolean.</returns>
        public static bool operator !=(UnifiedId a, string b)
        {
            return !a.Equals(b);
        }

        /// <summary>
        /// Equality operator.
        /// </summary>
        /// <param name="a">Left HEX string.</param>
        /// <param name="b">Right UnifiedId.</param>
        /// <returns>Result of equality boolean.</returns>
        public static bool operator ==(string a, UnifiedId b)
        {
            return b.Equals(a);
        }

        /// <summary>
        /// Not equality operator.
        /// </summary>
        /// <param name="a">Left HEX string.</param>
        /// <param name="b">Right UnifiedId.</param>
        /// <returns>Result of equality boolean.</returns>
        public static bool operator !=(string a, UnifiedId b)
        {
            return !b.Equals(a);
        }

        /// <summary>
        /// Less equality operator.
        /// </summary>
        /// <param name="left">Left UnifiedId.</param>
        /// <param name="right">Right UnifiedId.</param>
        /// <returns>Result of equality boolean.</returns>
        public static bool operator <(UnifiedId left, UnifiedId right)
        {
            return left.CompareTo(right) < 0;
        }

        /// <summary>
        /// Less or equal equality operator.
        /// </summary>
        /// <param name="left">Left UnifiedId.</param>
        /// <param name="right">Right UnifiedId.</param>
        /// <returns>Result of equality boolean.</returns>
        public static bool operator <=(UnifiedId left, UnifiedId right)
        {
            return left.CompareTo(right) <= 0;
        }

        /// <summary>
        /// More equality operator.
        /// </summary>
        /// <param name="left">Left UnifiedId.</param>
        /// <param name="right">Right UnifiedId.</param>
        /// <returns>Result of equality boolean.</returns>
        public static bool operator >(UnifiedId left, UnifiedId right)
        {
            return left.CompareTo(right) > 0;
        }

        /// <summary>
        /// More or equal equality operator.
        /// </summary>
        /// <param name="left">Left UnifiedId.</param>
        /// <param name="right">Right UnifiedId.</param>
        /// <returns>Result of equality boolean.</returns>
        public static bool operator >=(UnifiedId left, UnifiedId right)
        {
            return left.CompareTo(right) >= 0;
        }

        /// <summary>
        /// Get uniform virtual partition for UnifiedId.
        /// </summary>
        /// <param name="tier">Partitioning tier. Default range Standard 512 partitions 00-FV.</param>
        /// <returns>Uniform virtual partition.</returns>
        public string PartitionKey(Tier tier = Tier.Standard)
        {
            return NewHex(hash, (int)tier);
        }

        /// <summary>
        /// Get fixed-string virtual partition for UnifiedId. Slower but provide partition count configuration. 
        /// </summary>
        /// <param name="count">Partition count. Default range 000-999.</param>
        /// <returns>Fixed-string virtual partition.</returns>
        public string PartitionKeyNumberString(ushort count = 999)
        {
            var partition = hash / (ulong.MaxValue / count);
            var digits = Convert.ToInt32(Math.Log10(count) + 1);
            return $"{partition}".PadLeft(digits, '0');
        }

        /// <summary>
        /// Get numbered virtual partition for UnifiedId. Provide partition count configuration. 
        /// </summary>
        /// <param name="count">Partition count.</param>
        /// <returns>Numeric virtual partition. Default range 0-1024.</returns>
        public ulong PartitionKeyNumberUInt64(ushort count = 1024)
        {
            return hash / (ulong.MaxValue / count);
        }

        /// <summary>
        /// Parse UnifiedId HEX to UnifiedId. throws exception if invalid.
        /// </summary>
        /// <param name="hex">HEX.</param>
        /// <returns>UnifiedId.</returns>
        public static UnifiedId Parse(string hex)
        {
            if (string.IsNullOrWhiteSpace(hex) || hex == Empty)
            {
                return Empty;
            }

            if (hex.Length != Length)
            {
                throw new FormatException($"Argument '{nameof(hex)}'({hex}) should have length of {Length} symbols, actual length is {hex.Length} symbols.");
            }

            foreach(var symbol in hex)
            {
                if(!char.IsUpper(symbol) && !Array.Exists(Symbols, x => x == symbol))
                {
                    throw new FormatException($"Argument '{nameof(hex)}'({hex}) should contain only allowed capital symbols from '0' to 'V'.");
                }
            }

            var hash = Decode(hex);

            return new UnifiedId(hash);
        }

        /// <summary>
        /// TryParse UnifiedId HEX to UnifiedId.
        /// </summary>
        /// <param name="hex">HEX.</param>
        /// <param name="id">UnifiedId.</param>
        /// <returns>UnifiedId.</returns>
        public static bool TryParse(string hex, out UnifiedId id)
        {
            if (string.IsNullOrWhiteSpace(hex) || hex == Empty)
            {
                id = Empty;
                return false;
            }

            if (hex.Length != Length)
            {
                id = Empty;
                return false;
            }

            foreach (var symbol in hex)
            {
                if (!char.IsUpper(symbol) && !Array.Exists(Symbols, x => x == symbol))
                {
                    id = Empty;
                    return false;
                }
            }

            var hash = Decode(hex);

            id = new UnifiedId(hash);

            return true;
        }

        /// <summary>
        /// Generate x64 FNV hash based on random GUID.
        /// </summary>
        /// <returns>UnifiedId.</returns>
        public static UnifiedId NewId()
        {
            return FromGuid(Guid.NewGuid());
        }

        /// <summary>
        /// Generates new UnifiedId from byte[].
        /// </summary>
        /// <param name="bytes">Array of bytes to generate new hash.</param>
        /// <returns>UnifiedId.</returns>
        public static UnifiedId FromBytes(byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            if (bytes.Length == 0)
            {
                throw new ArgumentOutOfRangeException($"Argument {nameof(bytes)} should not be empty.");
            }

            return new UnifiedId(NewHash(bytes));
        }

        /// <summary>
        /// Generates new UnifiedId from byte[].
        /// </summary>
        /// <param name="guid">GUID to generate new hash.</param>
        /// <returns>UnifiedId.</returns>
        public static UnifiedId FromGuid(Guid guid)
        {
            return FromBytes(guid.ToByteArray());
        }

        /// <summary>
        /// Explicit operation from UInt64.
        /// </summary>
        /// <returns>UnifiedId.</returns>
        public static UnifiedId FromUInt64(ulong hash)
        {
            return new UnifiedId(hash);
        }

        /// <summary>
        /// Explicit operation from Int64.
        /// </summary>
        /// <returns>UnifiedId.</returns>
        public static UnifiedId FromInt64(long hash)
        {
            return new UnifiedId(hash);
        }

        /// <summary>
        /// Generates new UnifiedId from UTF8 text.
        /// </summary>
        /// <param name="text">UTF8 Text to generate new hash.</param>
        /// <returns>UnifiedId.</returns>
        public static UnifiedId FromString(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentNullException(nameof(text));
            }

            var bytes = Encoding.UTF8.GetBytes(text);
            return FromBytes(bytes);
        }

        /// <summary>
        /// GetObjectData.
        /// </summary>
        /// <param name="info">SerializationInfo.</param>
        /// <param name="context">StreamingContext.</param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            info.AddValue(nameof(hash), hash, typeof(ulong));
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return NewHex(hash);
        }

        /// <summary>
        /// ToUInt64.
        /// </summary>
        /// <returns>ULong hash.</returns>
        public ulong ToUInt64()
        {
            return hash;
        }

        /// <summary>
        /// Return Long hash with space negative shift.
        /// </summary>
        /// <returns>Long hash.</returns>
        public long ToInt64()
        {
            var bytes = BitConverter.GetBytes(hash);
            return BitConverter.ToInt64(bytes, 0);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return hash.GetHashCode();
        }

        /// <inheritdoc/>
        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }

            if (!(obj is UnifiedId))
            {
                throw new ArgumentException($"Argument {nameof(obj)} must be UnifiedId.");
            }

            var other = (UnifiedId)obj;

            return this.CompareTo(other);
        }

        /// <inheritdoc/>
        public int CompareTo(UnifiedId other)
        {
            if (this.hash < other.hash)
            {
                return -1;
            }

            if (this.hash > other.hash)
            {
                return 1;
            }

            return 0;
        }

        /// <inheritdoc/>
        public bool Equals(UnifiedId other)
        {
            return this.hash.Equals(other.hash);
        }

        /// <inheritdoc/>
        public object Clone()
        {
            return new UnifiedId(this.hash);
        }

        /// <inheritdoc/>
        public int CompareTo(string other)
        {
            if (string.IsNullOrWhiteSpace(other))
            {
                return 1;
            }

            var parsed = Parse(other);

            return hash.CompareTo(parsed);
        }

        /// <inheritdoc/>
        public bool Equals(string other)
        {
            return NewHex(hash).Equals(other, StringComparison.InvariantCulture);
        }

        /// <inheritdoc/>
        public int CompareTo(ulong other)
        {
            return hash.CompareTo(other);
        }

        /// <inheritdoc/>
        public bool Equals(ulong other)
        {
            return hash.Equals(other);
        }

        /// <inheritdoc/>
        public int CompareTo(long other)
        {
            return ToInt64().CompareTo(other);
        }

        /// <inheritdoc/>
        public bool Equals(long other)
        {
            return ToInt64().Equals(other);
        }

        /// <summary>
        /// Generate x64 FNV hash based on data bytes.
        /// </summary>
        /// <param name="bytes">Array of bytes.</param>
        /// <returns>ulong hash.</returns>
        private static ulong NewHash(byte[] bytes)
        {
            var hash = Offset; // FNV offset basis

            foreach (var @byte in bytes)
            {
                hash ^= @byte;
                hash *= Prime; // FNV prime
            }

            return hash;
        }

        /// <summary>
        /// Generate x32 HEX from ulong hash.
        /// </summary>
        /// <param name="hash">Hash.</param>
        /// <param name="length">Length of HEX. Should be 13 for full x32 encode.</param>
        /// <returns>HEX.</returns>
        private static string NewHex(ulong hash, int length = Length)
        {
            if (hash == 0)
            {
                return "0000000000000";
            }

            var hex = new char[length];
            for (var grade = 0; grade < length; grade++)
            {
                // slice = hash >> slice(shift * grade)
                var slice = (uint)(hash >> (X32Shift * (Length - grade - 1)));

                // index = slice & 31 = (32(1<<shift) - 1)
                var index = (byte)(slice & 31);
                hex[grade] = Symbols[index];
            }

            return new string(hex);
        }

        /// <summary>
        /// Decode HEX to Number.
        /// </summary>
        /// <param name="hex">String HEX.</param>s
        /// <returns>Unsigned x64 integer.</returns>
        private static ulong Decode(string hex)
        {
            ulong hash = 0;
            for (var grade = 0; grade < hex.Length; grade++)
            {
                var index = (ulong)Array.LastIndexOf(Symbols, hex[grade]);

                // slice grade and convert to number
                var slice = index << (X32Shift * (hex.Length - grade - 1));
                hash += slice;
            }

            return hash;
        }
    }
}
