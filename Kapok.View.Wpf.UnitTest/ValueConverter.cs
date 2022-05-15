using System.Globalization;
using Xunit;

namespace Kapok.View.Wpf.UnitTest;

public class ValueConverter
{
    [Fact]
    public void NullToBoolConverter()
    {
        var converter = new NullToBoolConverter();

        Assert.True((bool)(
                converter.Convert(null, typeof(bool), null, CultureInfo.InvariantCulture
                ) ?? throw new InvalidOperationException()));

        Assert.False((bool)(
            converter.Convert(new object(), typeof(bool), null, CultureInfo.InvariantCulture
            ) ?? throw new InvalidOperationException()));
    }
}