using System.Windows.Input;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Xaml.Interactivity;

namespace Avalonia.Desktop.Behaviors;

public class FileDropBehavior : Behavior<Interactive>
{
    public static readonly StyledProperty<ICommand?> CommandProperty = AvaloniaProperty.Register<FileDropBehavior, ICommand?>(nameof(Command));

    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <inheritdoc />
    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject is not null)
        {
            DragDrop.SetAllowDrop(AssociatedObject, true);
            AssociatedObject.AddHandler(DragDrop.DropEvent, OnDrop);
        }
    }

    /// <inheritdoc />
    protected override void OnDetaching()
    {
        base.OnDetaching();

        AssociatedObject?.RemoveHandler(DragDrop.DropEvent, OnDrop);
    }

    private void OnDrop(object? sender, DragEventArgs e)
    {
        var files = e.DataTransfer.TryGetFiles();

        if (files is null || files.Length == 0)
        {
            return;
        }

        var filePaths = files
                       .Select(f => f.TryGetLocalPath())
                       .Where(path => path is not null)
                       .ToList();

        if (filePaths.Count != 0 && Command is not null && Command.CanExecute(filePaths))
        {
            Command.Execute(filePaths);
        }
    }
}
