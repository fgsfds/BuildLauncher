using Avalonia.Controls;
using Avalonia.Desktop.ViewModels;
using Avalonia.Media;
using Common.Client.Config;
using Common.Helpers;

namespace Avalonia.Desktop.Controls;

public sealed partial class RightPanelControl : UserControl
{
    private readonly IBrush HightlightColor = SolidColorBrush.Parse("#FFCC00");
    private IConfigProvider _configProvider;

    public RightPanelControl()
    {
        InitializeComponent();
    }

    public void InitializeControl(IConfigProvider configProvider)
    {
        _configProvider = configProvider;
        SetStars();

        Description.PropertyChanged += OnPropertyChanged;
        Rating.PropertyChanged += OnPropertyChanged;
    }

    private void Button_PointerEntered1(object? sender, Input.PointerEventArgs e)
    {
        Star1.Content = "★";
        Star2.Content = "☆";
        Star3.Content = "☆";
        Star4.Content = "☆";
        Star5.Content = "☆";
    }

    private void Button_PointerEntered2(object? sender, Input.PointerEventArgs e)
    {
        Star1.Content = "★";
        Star2.Content = "★";
        Star3.Content = "☆";
        Star4.Content = "☆";
        Star5.Content = "☆";
    }

    private void Button_PointerEntered3(object? sender, Input.PointerEventArgs e)
    {
        Star1.Content = "★";
        Star2.Content = "★";
        Star3.Content = "★";
        Star4.Content = "☆";
        Star5.Content = "☆";
    }

    private void Button_PointerEntered4(object? sender, Input.PointerEventArgs e)
    {
        Star1.Content = "★";
        Star2.Content = "★";
        Star3.Content = "★";
        Star4.Content = "★";
        Star5.Content = "☆";
    }

    private void Button_PointerEntered5(object? sender, Input.PointerEventArgs e)
    {
        Star1.Content = "★";
        Star2.Content = "★";
        Star3.Content = "★";
        Star4.Content = "★";
        Star5.Content = "★";
    }

    private void SetStars()
    {
        Guard2.ThrowIfNotType<RightPanelViewModel>(DataContext, out var rightPanelViewModel);

        if (rightPanelViewModel.SelectedAddon is null)
        {
            return;
        }

        var hasRating = _configProvider.Rating.TryGetValue(rightPanelViewModel.SelectedAddon.Id, out var rating);

        var theme = Application.Current!.ActualThemeVariant;
        _ = Application.Current.TryGetResource("ButtonForeground", theme, out var defaultColor);

        //don't judge me
        if (!hasRating || rating == 0)
        {
            Star1.Content = "☆";
            Star2.Content = "☆";
            Star3.Content = "☆";
            Star4.Content = "☆";
            Star5.Content = "☆";
            Star1.Foreground = (SolidColorBrush)defaultColor!;
            Star2.Foreground = (SolidColorBrush)defaultColor!;
            Star3.Foreground = (SolidColorBrush)defaultColor!;
            Star4.Foreground = (SolidColorBrush)defaultColor!;
            Star5.Foreground = (SolidColorBrush)defaultColor!;
        }
        else if (rating == 1)
        {
            Star1.Content = "★";
            Star2.Content = "☆";
            Star3.Content = "☆";
            Star4.Content = "☆";
            Star5.Content = "☆";
            Star1.Foreground = HightlightColor;
            Star2.Foreground = (SolidColorBrush)defaultColor!;
            Star3.Foreground = (SolidColorBrush)defaultColor!;
            Star4.Foreground = (SolidColorBrush)defaultColor!;
            Star5.Foreground = (SolidColorBrush)defaultColor!;
        }
        else if (rating == 2)
        {
            Star1.Content = "★";
            Star2.Content = "★";
            Star3.Content = "☆";
            Star4.Content = "☆";
            Star5.Content = "☆";
            Star1.Foreground = HightlightColor;
            Star2.Foreground = HightlightColor;
            Star3.Foreground = (SolidColorBrush)defaultColor!;
            Star4.Foreground = (SolidColorBrush)defaultColor!;
            Star5.Foreground = (SolidColorBrush)defaultColor!;
        }
        else if (rating == 3)
        {
            Star1.Content = "★";
            Star2.Content = "★";
            Star3.Content = "★";
            Star4.Content = "☆";
            Star5.Content = "☆";
            Star1.Foreground = HightlightColor;
            Star2.Foreground = HightlightColor;
            Star3.Foreground = HightlightColor;
            Star4.Foreground = (SolidColorBrush)defaultColor!;
            Star5.Foreground = (SolidColorBrush)defaultColor!;
        }
        else if (rating == 4)
        {
            Star1.Content = "★";
            Star2.Content = "★";
            Star3.Content = "★";
            Star4.Content = "★";
            Star5.Content = "☆";
            Star1.Foreground = HightlightColor;
            Star2.Foreground = HightlightColor;
            Star3.Foreground = HightlightColor;
            Star4.Foreground = HightlightColor;
            Star5.Foreground = (SolidColorBrush)defaultColor!;
        }
        else if (rating == 5)
        {
            Star1.Content = "★";
            Star2.Content = "★";
            Star3.Content = "★";
            Star4.Content = "★";
            Star5.Content = "★";
            Star1.Foreground = HightlightColor;
            Star2.Foreground = HightlightColor;
            Star3.Foreground = HightlightColor;
            Star4.Foreground = HightlightColor;
            Star5.Foreground = HightlightColor;
        }
    }

    private void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (sender is null)
        {
            return;
        }

        SetStars();
    }

    private void Button_PointerExited(object? sender, Input.PointerEventArgs e)
    {
        if (sender is null)
        {
            return;
        }

        SetStars();
    }
}
