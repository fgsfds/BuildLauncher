using Avalonia.Controls;
using Avalonia.Desktop.ViewModels;
using Avalonia.Interactivity;
using Avalonia.Media;
using Common.Client.Interfaces;
using Common.Helpers;

namespace Avalonia.Desktop.Controls;

public sealed partial class RightPanelControl : UserControl
{
    private readonly IBrush HightlightColor = SolidColorBrush.Parse("#FFCC00");
    private readonly IBrush DisabledColor = SolidColorBrush.Parse("#919191");
    private IConfigProvider _configProvider;

    private RightPanelViewModel _viewModel;

    public RightPanelControl()
    {
        InitializeComponent();
    }

    public void InitializeControl(IConfigProvider configProvider)
    {
        _configProvider = configProvider;

        Guard2.ThrowIfNotType<RightPanelViewModel>(DataContext, out var rightPanelViewModel);
        _viewModel = rightPanelViewModel;
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
        if (_viewModel?.SelectedAddon is null)
        {
            return;
        }

        if (!Star1.IsEnabled)
        {
            return;
        }

        var hasRating = _configProvider.Rating.TryGetValue(_viewModel.SelectedAddon.Id, out var rating);

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

    public async void Button_Click1(object sender, RoutedEventArgs args)
    {
        ChangeStarsState(false);

        var command = _viewModel.ChangeRatingCommand;
        await command.ExecuteAsync("1").ConfigureAwait(true);

        ChangeStarsState(true);

        SetStars();
    }

    public async void Button_Click2(object sender, RoutedEventArgs args)
    {
        ChangeStarsState(false);

        var command = _viewModel.ChangeRatingCommand;
        await command.ExecuteAsync("2").ConfigureAwait(true);

        ChangeStarsState(true);

        SetStars();
    }

    public async void Button_Click3(object sender, RoutedEventArgs args)
    {
        ChangeStarsState(false);

        var command = _viewModel.ChangeRatingCommand;
        await command.ExecuteAsync("3").ConfigureAwait(true);

        ChangeStarsState(true);

        SetStars();
    }

    public async void Button_Click4(object sender, RoutedEventArgs args)
    {
        ChangeStarsState(false);

        var command = _viewModel.ChangeRatingCommand;
        await command.ExecuteAsync("4").ConfigureAwait(true);

        ChangeStarsState(true);

        SetStars();
    }

    public async void Button_Click5(object sender, RoutedEventArgs args)
    {
        ChangeStarsState(false);

        var command = _viewModel.ChangeRatingCommand;
        await command.ExecuteAsync("5").ConfigureAwait(true);

        ChangeStarsState(true);

        SetStars();
    }


    private void ChangeStarsState(bool isEnabled)
    {
        Star1.IsEnabled = isEnabled;
        Star2.IsEnabled = isEnabled;
        Star3.IsEnabled = isEnabled;
        Star4.IsEnabled = isEnabled;
        Star5.IsEnabled = isEnabled;

        if (!isEnabled)
        {
            Star1.Foreground = DisabledColor;
            Star2.Foreground = DisabledColor;
            Star3.Foreground = DisabledColor;
            Star4.Foreground = DisabledColor;
            Star5.Foreground = DisabledColor;
        }
    }

    private void Button_PointerExited(object? sender, Input.PointerEventArgs e)
    {
        if (sender is null)
        {
            return;
        }

        SetStars();
    }

    private void Rating_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (sender is null)
        {
            return;
        }

        SetStars();
    }
}
