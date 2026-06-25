using Google.Protobuf.WellKnownTypes;
using Microsoft.UI.Xaml.Data;

namespace UnoplatformUI.Presentation.Pages;

public class TimestampDisplayConverter : IValueConverter
{
    public object Convert(object value, System.Type targetType, object parameter, string language)
        => value is Timestamp ts
            ? ts.ToDateTime().ToLocalTime().ToString("yyyy-MM-dd HH:mm")
            : "";

    public object ConvertBack(object value, System.Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}
