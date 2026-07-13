using System;
using System.Globalization;
using Xunit;

namespace Unified.Tests
{
    /// <summary>
    /// Tests for the modern .NET parsing/formatting interfaces added on net8.0+ targets:
    /// IParsable, ISpanParsable, IFormattable and ISpanFormattable. These make UnifiedId a
    /// first-class citizen for generic parsing, model binding and allocation-free serialization.
    /// </summary>
    public class ModernInterfacesTests
    {
        private const string Sample = "FVVVVVVVVVVVU";

        // Proves the generic constraint is satisfied by calling the static abstract member generically.
        private static T ParseViaGeneric<T>(string text)
            where T : IParsable<T> => T.Parse(text, CultureInfo.InvariantCulture);

        private static T SpanParseViaGeneric<T>(ReadOnlySpan<char> text)
            where T : ISpanParsable<T> => T.Parse(text, CultureInfo.InvariantCulture);

        [Fact]
        public void SatisfiesIParsableConstraint()
        {
            var parsed = ParseViaGeneric<UnifiedId>(Sample);
            Assert.Equal(new UnifiedId(Sample), parsed);
        }

        [Fact]
        public void IParsableParseAndTryParse()
        {
            var expected = new UnifiedId(Sample);
            Assert.Equal(expected, UnifiedId.Parse(Sample, CultureInfo.InvariantCulture));

            Assert.True(UnifiedId.TryParse(Sample, CultureInfo.InvariantCulture, out var parsed));
            Assert.Equal(expected, parsed);

            Assert.False(UnifiedId.TryParse("not-valid", CultureInfo.InvariantCulture, out var invalid));
            Assert.Equal(UnifiedId.Empty, invalid);

            Assert.False(UnifiedId.TryParse(null, CultureInfo.InvariantCulture, out var fromNull));
            Assert.Equal(UnifiedId.Empty, fromNull);
        }

        [Fact]
        public void SatisfiesISpanParsableConstraint()
        {
            var parsed = SpanParseViaGeneric<UnifiedId>(Sample.AsSpan());
            Assert.Equal(new UnifiedId(Sample), parsed);
        }

        [Fact]
        public void ISpanParsableParseAndTryParse()
        {
            var expected = new UnifiedId(Sample);
            Assert.Equal(expected, UnifiedId.Parse(Sample.AsSpan(), CultureInfo.InvariantCulture));

            Assert.True(UnifiedId.TryParse(Sample.AsSpan(), CultureInfo.InvariantCulture, out var parsed));
            Assert.Equal(expected, parsed);

            Assert.False(UnifiedId.TryParse("nope".AsSpan(), CultureInfo.InvariantCulture, out var invalid));
            Assert.Equal(UnifiedId.Empty, invalid);
        }

        [Fact]
        public void IFormattableReturnsCanonicalRepresentation()
        {
            IFormattable id = new UnifiedId(Sample);

            // format and provider are accepted but ignored: there is one canonical HEX32 representation.
            Assert.Equal(Sample, id.ToString(null, CultureInfo.InvariantCulture));
            Assert.Equal(Sample, id.ToString("anything", null));
        }

        [Fact]
        public void ISpanFormattableWritesThirteenChars()
        {
            ISpanFormattable id = new UnifiedId(Sample);

            Span<char> buffer = stackalloc char[13];
            Assert.True(id.TryFormat(buffer, out var written, default, null));
            Assert.Equal(13, written);
            Assert.Equal(Sample, buffer.ToString());
        }

        [Fact]
        public void ISpanFormattableFailsWhenBufferTooSmall()
        {
            ISpanFormattable id = new UnifiedId(Sample);

            Span<char> tooSmall = stackalloc char[12];
            Assert.False(id.TryFormat(tooSmall, out var written, default, null));
            Assert.Equal(0, written);
        }

        [Fact]
        public void InterpolationUsesSpanFormatting()
        {
            // The interpolated-string handler routes through ISpanFormattable on net8+.
            var id = new UnifiedId(Sample);
            Assert.Equal($"id={Sample}", $"id={id}");
        }

        [Fact]
        public void RoundTripsThroughSpanFormatAndParse()
        {
            var original = UnifiedId.NewId();

            Span<char> buffer = stackalloc char[13];
            Assert.True(((ISpanFormattable)original).TryFormat(buffer, out var written, default, null));

            var parsed = UnifiedId.Parse(buffer[..written], CultureInfo.InvariantCulture);
            Assert.Equal(original, parsed);
        }
    }
}
