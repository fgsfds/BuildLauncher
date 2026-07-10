using Avalonia.Controls;
using Avalonia.Desktop.Controls;
using Avalonia.Layout;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.Input;

namespace Tests.Unit;

public sealed class AddonListControlBaseTests
{
    private sealed class TestProxy : AddonListControlBase
    {
        private TestProxy() : base(null!) { }

        public static void Notify(Panel panel) => NotifyPortButtonsCanExecuteChanged(panel);
        public static StackPanel CreateContent(Bitmap? icon, string name) => CreatePortButtonContent(icon, name);
        public static Button? FindButton(Panel panel, string text) => FindButtonByText(panel, text);
        public static void RemoveButton(Panel panel, string text) => RemoveButtonByText(panel, text);
        public static void ClearMenu(ContextMenu? contextMenu) => ClearContextMenu(contextMenu);
    }


    [Fact]
    public void Notify_WithButtonsWithRelayCommand_NotifiesAll()
    {
        var panel = new StackPanel();
        var notified1 = false;
        var notified2 = false;

        var cmd1 = new RelayCommand(() => { });
        cmd1.CanExecuteChanged += (_, _) => notified1 = true;

        var cmd2 = new RelayCommand(() => { });
        cmd2.CanExecuteChanged += (_, _) => notified2 = true;

        panel.Children.Add(
            new Button
            {
                Command = cmd1
            }
            );

        panel.Children.Add(
            new Button
            {
                Command = cmd2
            }
            );

        panel.Children.Add(new TextBlock());

        TestProxy.Notify(panel);

        Assert.True(notified1);
        Assert.True(notified2);
    }

    [Fact]
    public void Notify_WithNoButtons_DoesNotThrow()
    {
        var panel = new StackPanel();

        TestProxy.Notify(panel);
    }

    [Fact]
    public void Notify_WithNonCommandButtons_DoesNotNotify()
    {
        var panel = new StackPanel();
        var notified = false;

        var cmd = new RelayCommand(() => { });
        cmd.CanExecuteChanged += (_, _) => notified = true;

        panel.Children.Add(new Button());

        TestProxy.Notify(panel);

        Assert.False(notified);
    }

    [Fact]
    public void CreateContent_WithNullIcon_ReturnsStackPanelWithTextOnly()
    {
        var result = TestProxy.CreateContent(null, "Test Port");

        Assert.Single(result.Children);
        Assert.IsType<TextBlock>(result.Children[0]);
        Assert.Equal("Test Port", ((TextBlock)result.Children[0]).Text);
    }

    [Fact]
    public void CreateContent_SetsHorizontalOrientation()
    {
        var result = TestProxy.CreateContent(null, "Any");

        Assert.Equal(Orientation.Horizontal, result.Orientation);
    }

    [Fact]
    public void FindButton_WhenButtonExists_ReturnsButton()
    {
        var panel = new StackPanel();

        panel.Children.Add(
            new Button
            {
                Content = new TextBlock
                {
                    Text = "Target"
                }
            }
            );

        panel.Children.Add(
            new Button
            {
                Content = new TextBlock
                {
                    Text = "Other"
                }
            }
            );

        var result = TestProxy.FindButton(panel, "Target");

        Assert.NotNull(result);
        Assert.Equal("Target", ((TextBlock)result.Content!).Text);
    }

    [Fact]
    public void FindButton_WhenNoMatch_ReturnsNull()
    {
        var panel = new StackPanel();

        panel.Children.Add(
            new Button
            {
                Content = new TextBlock
                {
                    Text = "Other"
                }
            }
            );

        var result = TestProxy.FindButton(panel, "Missing");

        Assert.Null(result);
    }

    [Fact]
    public void FindButton_WithEmptyPanel_ReturnsNull()
    {
        var result = TestProxy.FindButton(new StackPanel(), "Anything");

        Assert.Null(result);
    }

    [Fact]
    public void FindButton_IgnoresNonButtonChildren()
    {
        var panel = new StackPanel();

        panel.Children.Add(
            new TextBlock
            {
                Text = "Target"
            }
            );

        panel.Children.Add(
            new Button
            {
                Content = new TextBlock
                {
                    Text = "Target"
                }
            }
            );

        var result = TestProxy.FindButton(panel, "Target");

        Assert.NotNull(result);
        Assert.IsType<Button>(result);
    }

    [Fact]
    public void RemoveButton_WhenButtonExists_RemovesIt()
    {
        var panel = new StackPanel();

        panel.Children.Add(
            new Button
            {
                Content = new TextBlock
                {
                    Text = "Target"
                }
            }
            );

        panel.Children.Add(
            new Button
            {
                Content = new TextBlock
                {
                    Text = "Other"
                }
            }
            );

        TestProxy.RemoveButton(panel, "Target");

        Assert.Single(panel.Children);
        Assert.Equal("Other", ((TextBlock)((Button)panel.Children[0]).Content!).Text);
    }

    [Fact]
    public void RemoveButton_WhenNoMatch_DoesNothing()
    {
        var panel = new StackPanel();

        panel.Children.Add(
            new Button
            {
                Content = new TextBlock
                {
                    Text = "Other"
                }
            }
            );

        TestProxy.RemoveButton(panel, "Missing");

        Assert.Single(panel.Children);
    }

    [Fact]
    public void RemoveButton_WithEmptyPanel_DoesNotThrow()
    {
        TestProxy.RemoveButton(new StackPanel(), "Anything");
    }

    [Fact]
    public void ClearMenu_WhenNull_DoesNotThrow()
    {
        TestProxy.ClearMenu(null);
    }
}
