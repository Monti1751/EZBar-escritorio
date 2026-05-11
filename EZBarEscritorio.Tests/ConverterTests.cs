using FluentAssertions;
using EZBarEscritorio.Infrastructure.Converters;
using System.Globalization;
using System.Windows;
using Xunit;

namespace EZBarEscritorio.Tests
{
    public class ConverterTests
    {
        [Fact]
        public void InverseBooleanToVisibilityConverter_Works()
        {
            var converter = new InverseBooleanToVisibilityConverter();

            // true -> Collapsed
            converter.Convert(true, typeof(Visibility), null, CultureInfo.CurrentCulture)
                .Should().Be(Visibility.Collapsed);

            // false -> Visible
            converter.Convert(false, typeof(Visibility), null, CultureInfo.CurrentCulture)
                .Should().Be(Visibility.Visible);
        }
    }
}
