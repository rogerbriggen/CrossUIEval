using Avalonia.Media;
using BackendUiEval.Grpc;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AvaloniaUiEval.ViewModels;

public partial class AccentChipVm : ObservableObject
{
    public AccentColor Value { get; }
    public IBrush Fill { get; }

    [ObservableProperty] private IBrush _strokeBrush = Brushes.LightGray;
    [ObservableProperty] private double _strokeThickness = 1;

    public AccentChipVm(AccentColor value)
    {
        Value = value;
        Fill = new SolidColorBrush(AccentPalette.ToColor(value));
    }

    public void RefreshSelected(AccentColor selected)
    {
        if (selected == Value)
        {
            StrokeBrush = Brushes.Black;
            StrokeThickness = 4;
        }
        else
        {
            StrokeBrush = Brushes.LightGray;
            StrokeThickness = 1;
        }
    }
}

public static class AccentPalette
{
    public static Color ToColor(AccentColor c) => c switch
    {
        AccentColor.Blue => Color.Parse("#3F6BD8"),
        AccentColor.Green => Color.Parse("#3CAE5C"),
        AccentColor.Red => Color.Parse("#D84A4A"),
        AccentColor.Orange => Color.Parse("#E8843C"),
        AccentColor.Purple => Color.Parse("#8B5CD8"),
        AccentColor.Teal => Color.Parse("#2EA39A"),
        AccentColor.Pink => Color.Parse("#D85C9A"),
        _ => Colors.LightGray,
    };
}
