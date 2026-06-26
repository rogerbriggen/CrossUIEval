using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Timestamp = Google.Protobuf.WellKnownTypes.Timestamp;

namespace AvaloniaUiEval.Converters;

public class TimestampDisplayConverter : IValueConverter
{
    public static readonly TimestampDisplayConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is Timestamp ts ? ts.ToDateTime().ToLocalTime().ToString("yyyy-MM-dd HH:mm", culture) : "";

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
