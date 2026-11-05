using Avalonia.Controls;
using Avalonia.Desktop.Pages;
using Avalonia.Desktop.ViewModels;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Ports.Ports;
using Ports.Providers;
using Tests.Unit.Sequential.Helpers;

namespace Tests.Unit.Sequential;

/// <summary> Exercises custom-port selection, validation and editor commands through the real page. </summary>
public sealed class PortsPageTests
{
    /// <summary> Add opens the editor, Save displays validation, and Cancel closes without saving. </summary>
    [AvaloniaFact]
    public void Add_ValidateAndCancel_DoesNotSave()
    {
        HeadlessView.Prepare();
        var provider = new Mock<IPortsProvider>();
        provider.Setup(x => x.GetCustomPorts()).Returns([]);
        var vm = new PortsViewModel(Mock.Of<IViewModelsFactory>(), provider.Object, [], NullLogger<PortsViewModel>.Instance);
        using var host = new HeadlessView(new PortsPage { DataContext = vm });
        Assert.False(host.Button("Save").IsEffectivelyVisible);
        host.Click(host.Button("Add"));
        host.Click(host.Button("Save"));
        Assert.Contains(host.Window.GetVisualDescendants().OfType<TextBlock>(), x => x.IsEffectivelyVisible && x.Text == "Name is required");
        var name = host.Window.GetVisualDescendants().OfType<TextBox>().Single(x => x.PlaceholderText == "Name");
        name.Text = "My port";
        host.Click(host.Button("Save"));
        Assert.Equal("Path to exe is required", vm.ErrorMessage);
        Assert.Equal("My port", vm.SelectedCustomPortName);
        host.Click(host.Button("Cancel"));
        Assert.False(host.Button("Save").IsEffectivelyVisible);
        provider.Verify(x => x.AddOrChangeCustomPort(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Core.All.Enums.PortEnum>()), Times.Never);
    }

    /// <summary> Selecting a row enables editing and deletion and forwards the selected name. </summary>
    [AvaloniaFact]
    public void Selection_EnablesActionsAndDeletesSelectedPort()
    {
        HeadlessView.Prepare();
        var port = new CustomPort { Name = "Custom", Path = "custom-port", BasePort = new StubPort() };
        var provider = new Mock<IPortsProvider>();
        provider.Setup(x => x.GetCustomPorts()).Returns([port]);
        var vm = new PortsViewModel(Mock.Of<IViewModelsFactory>(), provider.Object, [], NullLogger<PortsViewModel>.Instance);
        using var host = new HeadlessView(new PortsPage { DataContext = vm });
        Assert.False(host.Button("Edit").IsEffectivelyEnabled);
        Assert.False(host.Button("Delete").IsEffectivelyEnabled);
        var grid = host.Window.GetVisualDescendants().OfType<DataGrid>().Single();
        grid.SelectedItem = port;
        Assert.Same(port, vm.SelectedCustomPort);
        host.Click(host.Button("Edit"));
        Assert.Equal("Custom", host.Window.GetVisualDescendants().OfType<TextBox>().Single(x => x.PlaceholderText == "Name").Text);
        host.Click(host.Button("Cancel"));
        host.Click(host.Button("Delete"));
        provider.Verify(x => x.DeleteCustomPort("Custom"), Times.Once);
    }

    /// <summary> A complete custom-port form saves its values and closes the editor. </summary>
    [AvaloniaFact]
    public void Add_ValidForm_SavesCustomPort()
    {
        HeadlessView.Prepare();
        var executable = Path.GetTempFileName();
        try
        {
            var port = new StubPort();
            var provider = new Mock<IPortsProvider>();
            provider.Setup(x => x.GetCustomPorts()).Returns([]);
            var factory = new Mock<IViewModelsFactory>();
            factory.Setup(x => x.GetPortViewModel(port)).Returns(new PortViewModel(null!, null!, port, NullLogger<PortViewModel>.Instance));
            var vm = new PortsViewModel(factory.Object, provider.Object, [port], NullLogger<PortsViewModel>.Instance);
            using var host = new HeadlessView(new PortsPage { DataContext = vm });
            host.Click(host.Button("Add"));
            host.Window.GetVisualDescendants().OfType<TextBox>().Single(x => x.PlaceholderText == "Name").Text = "My port";
            host.Window.GetVisualDescendants().OfType<TextBox>().Single(x => x.PlaceholderText == "Path").Text = executable;
            host.Window.GetVisualDescendants().OfType<ComboBox>().Single().SelectedItem = Core.All.Enums.PortEnum.Stub;
            host.Click(host.Button("Save"));
            provider.Verify(x => x.AddOrChangeCustomPort(null, "My port", executable, Core.All.Enums.PortEnum.Stub), Times.Once);
            Assert.False(host.Button("Save").IsEffectivelyVisible);
            Assert.Empty(vm.ErrorMessage);
        }
        finally
        {
            File.Delete(executable);
        }
    }
}
