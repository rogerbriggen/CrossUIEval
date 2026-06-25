using System.Globalization;
using BackendUiEval.Grpc;

namespace MauiUiEval.Converters;

public class AccentColorToBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is AccentColor c ? new SolidColorBrush(ToColor(c)) : new SolidColorBrush(Colors.LightGray);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();

    private static Color ToColor(AccentColor c) => c switch
    {
        AccentColor.Blue => Color.FromArgb("#3F6BD8"),
        AccentColor.Green => Color.FromArgb("#3CAE5C"),
        AccentColor.Red => Color.FromArgb("#D84A4A"),
        AccentColor.Orange => Color.FromArgb("#E8843C"),
        AccentColor.Purple => Color.FromArgb("#8B5CD8"),
        AccentColor.Teal => Color.FromArgb("#2EA39A"),
        AccentColor.Pink => Color.FromArgb("#D85C9A"),
        _ => Colors.LightGray,
    };
}
