using System;
using TwoWayMapper.Conversion.Converters;
using Xunit;

namespace TwoWayMapper.Test
{
    public class GenericConversionTest
    {
        [Fact]
        public void TryConvertBool_WhenInvokedForIntConverter_ShouldConvert()
        {
            // Arrange
            var converter = TryParseConverter<bool>.FromString;

            // Act
            var result = converter.Convert("true");

            // Assert
            Assert.True(result.IsConvertedSuccessfully);
            Assert.Equal(result.ConvertedValue, true);
            Assert.Null(result.ErrorMessage);
        }

        [Fact]
        public void TryConvertByte_WhenInvokedForIntConverter_ShouldConvert()
        {
            // Arrange
            var converter = TryParseConverter<byte>.FromString;

            // Act
            var result = converter.Convert("123");

            // Assert
            Assert.True(result.IsConvertedSuccessfully);
            Assert.Equal(result.ConvertedValue, (byte)123);
            Assert.Null(result.ErrorMessage);
        }

        [Fact]
        public void TryConvertChar_WhenInvokedForIntConverter_ShouldConvert()
        {
            // Arrange
            var converter = TryParseConverter<char>.FromString;

            // Act
            var result = converter.Convert("B");

            // Assert
            Assert.True(result.IsConvertedSuccessfully);
            Assert.Equal(result.ConvertedValue, 'B');
            Assert.Null(result.ErrorMessage);
        }

        [Fact]
        public void TryConvertDateTime_WhenInvokedForIntConverter_ShouldConvert()
        {
            // Arrange
            var converter = TryParseConverter<DateTime>.FromString;
            var date = DateTime.Now;

            // Act
            var result = converter.Convert(date.Date.ToString());

            // Assert
            Assert.True(result.IsConvertedSuccessfully);
            Assert.Equal(result.ConvertedValue, date.Date);
            Assert.Null(result.ErrorMessage);
        }

        [Fact]
        public void TryConvertDateTimeOffset_WhenInvokedForIntConverter_ShouldConvert()
        {
            // Arrange
            var converter = TryParseConverter<DateTimeOffset>.FromString;
            var date = new DateTimeOffset(2014, 1, 22, 2, 12, 11, TimeSpan.FromHours(3));

            // Act
            var result = converter.Convert(date.ToString());

            // Assert
            Assert.True(result.IsConvertedSuccessfully);
            Assert.Equal(result.ConvertedValue, date);
            Assert.Null(result.ErrorMessage);
        }

        [Fact]
        public void TryConvertDecimal_WhenInvokedForIntConverter_ShouldConvert()
        {
            // Arrange
            var converter = TryParseConverter<decimal>.FromString;

            // Act
            var result = converter.Convert(((decimal)123.344).ToString());

            // Assert
            Assert.True(result.IsConvertedSuccessfully);
            Assert.Equal(result.ConvertedValue, (decimal)123.344);
            Assert.Null(result.ErrorMessage);
        }

        [Fact]
        public void TryConvertDouble_WhenInvokedForIntConverter_ShouldConvert()
        {
            // Arrange
            var converter = TryParseConverter<double>.FromString;

            // Act
            var result = converter.Convert(((double)123.344).ToString());

            // Assert
            Assert.True(result.IsConvertedSuccessfully);
            Assert.Equal(result.ConvertedValue, (double)123.344);
            Assert.Null(result.ErrorMessage);
        }

        [Fact]
        public void TryConvertGuid_WhenInvokedForIntConverter_ShouldConvert()
        {
            // Arrange
            var converter = TryParseConverter<Guid>.FromString;
            var guid = Guid.NewGuid();

            // Act
            var result = converter.Convert(guid.ToString());

            // Assert
            Assert.True(result.IsConvertedSuccessfully);
            Assert.Equal(result.ConvertedValue, guid);
            Assert.Null(result.ErrorMessage);
        }

        [Fact]
        public void TryConvertInt16_WhenInvokedForIntConverter_ShouldConvert()
        {
            // Arrange
            var converter = TryParseConverter<short>.FromString;

            // Act
            var result = converter.Convert("123");

            // Assert
            Assert.True(result.IsConvertedSuccessfully);
            Assert.Equal(result.ConvertedValue, (short)123);
            Assert.Null(result.ErrorMessage);
        }

        [Fact]
        public void TryConvertInt64_WhenInvokedForIntConverter_ShouldConvert()
        {
            // Arrange
            var converter = TryParseConverter<long>.FromString;

            // Act
            var result = converter.Convert("123");

            // Assert
            Assert.True(result.IsConvertedSuccessfully);
            Assert.Equal(result.ConvertedValue, (long)123);
            Assert.Null(result.ErrorMessage);
        }

        [Fact]
        public void TryConvertInt32_WhenInvokedForIntConverter_ShouldConvert()
        {
            // Arrange
            var converter = TryParseConverter<int>.FromString;

            // Act
            var result = converter.Convert("123");

            // Assert
            Assert.True(result.IsConvertedSuccessfully);
            Assert.Equal(result.ConvertedValue, 123);
            Assert.Null(result.ErrorMessage);
        }

        [Fact]
        public void TryConvertSingle_WhenInvokedForIntConverter_ShouldConvert()
        {
            // Arrange
            var converter = TryParseConverter<float>.FromString;

            // Act
            var result = converter.Convert(((float)123.344).ToString());

            // Assert
            Assert.True(result.IsConvertedSuccessfully);
            Assert.Equal(result.ConvertedValue, (float)123.344);
            Assert.Null(result.ErrorMessage);
        }

        [Fact]
        public void TryConvertTimeSpan_WhenInvokedForIntConverter_ShouldConvert()
        {
            // Arrange
            var converter = TryParseConverter<TimeSpan>.FromString;
            var expected = TimeSpan.FromSeconds(1233);

            // Act
            var result = converter.Convert(expected.ToString());

            // Assert
            Assert.True(result.IsConvertedSuccessfully);
            Assert.Equal(result.ConvertedValue, expected);
            Assert.Null(result.ErrorMessage);
        }
    }
}