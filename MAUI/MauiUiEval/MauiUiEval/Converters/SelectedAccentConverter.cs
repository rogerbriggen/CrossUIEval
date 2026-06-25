using System.Globalization;

namespace MauiUiEval.Converters;

// MultiBinding inputs: [0] = this chip's AccentColor, [1] = VM.AccentColor (currently selected).
// Parameter picks which visual property to compute.
public class SelectedAccentConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        var selected = values is { Length: >= 2 } && Equals(values[0], values[1]);
        return (parameter as string) switch
        {
            "Stroke" => new SolidColorBrush(selected ? Colors.Black : Colors.LightGray),
            "Thickness" => selected ? 4.0 : 1.0,
            _ => selected,
        };
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
