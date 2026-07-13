using Avalonia.Desktop.ViewModels;
using Moq;
using Tools.Tools;

namespace Tests.Unit;

public sealed class ToolsViewModelTests
{
    static ToolsViewModelTests() => Helpers.HeadlessAvaloniaApp.EnsureInitialized();

    [Fact]
    public void Constructor_EmptyTools_ReturnsEmptyToolsList()
    {
        var factoryMock = new Mock<IViewModelsFactory>();

        var vm = new ToolsViewModel(factoryMock.Object, []);

        Assert.Empty(vm.ToolsList);
        Assert.False(vm.HasUpdates);
    }

    [Fact]
    public void Constructor_WithTools_PopulatesToolsList()
    {
        var factoryMock = new Mock<IViewModelsFactory>();
        var mapster32 = new Mapster32(null!);
        var xmapedit = new XMapEdit(null!);

        factoryMock
            .Setup(x => x.GetToolViewModel(mapster32))
            .Returns(new ToolViewModel(null!, null!, mapster32, null!));

        factoryMock
            .Setup(x => x.GetToolViewModel(xmapedit))
            .Returns(new ToolViewModel(null!, null!, xmapedit, null!));

        var vm = new ToolsViewModel(factoryMock.Object, [mapster32, xmapedit]);

        Assert.Equal(2, vm.ToolsList.Count);
    }

    [Fact]
    public void Constructor_ToolsListContainsCorrectToolTypes()
    {
        var factoryMock = new Mock<IViewModelsFactory>();
        var dosBlood = new DOSBlood(null!);

        factoryMock
            .Setup(x => x.GetToolViewModel(dosBlood))
            .Returns(new ToolViewModel(null!, null!, dosBlood, null!));

        var vm = new ToolsViewModel(factoryMock.Object, [dosBlood]);

        Assert.Single(vm.ToolsList);
        Assert.Same(dosBlood, vm.ToolsList[0].Tool);
    }

    [Fact]
    public void ToolsList_SetGet_Works()
    {
        var factoryMock = new Mock<IViewModelsFactory>();
        var vm = new ToolsViewModel(factoryMock.Object, []);

        Assert.Empty(vm.ToolsList);

        var mapster32 = new Mapster32(null!);
        var list = System.Collections.Immutable.ImmutableList<ToolViewModel>.Empty
            .Add(new ToolViewModel(null!, null!, mapster32, null!));

        vm.ToolsList = list;
        Assert.Same(list, vm.ToolsList);
    }

    [Fact]
    public void HasUpdates_Default_False()
    {
        var factoryMock = new Mock<IViewModelsFactory>();
        var vm = new ToolsViewModel(factoryMock.Object, []);

        Assert.False(vm.HasUpdates);
    }

    [Fact]
    public void OnToolChanged_NotifiesHasUpdates()
    {
        var factoryMock = new Mock<IViewModelsFactory>();
        var mapster32 = new Mapster32(null!);
        var toolVm = new ToolViewModel(null!, null!, mapster32, null!);

        factoryMock
            .Setup(x => x.GetToolViewModel(mapster32))
            .Returns(toolVm);

        var vm = new ToolsViewModel(factoryMock.Object, [mapster32]);

        var propertyChanged = false;
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(ToolsViewModel.HasUpdates))
                propertyChanged = true;
        };

        vm.HasUpdates = true;

        Assert.True(propertyChanged);
    }
}
