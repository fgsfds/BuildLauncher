using System.Collections.Immutable;
using Avalonia.Desktop.ViewModels;
using Avalonia.Headless.XUnit;
using Core.All.Enums;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Ports.Ports;
using Ports.Providers;

namespace Tests.Unit;

public sealed class PortsViewModelTests
{
    private readonly Mock<IPortsProvider> _portsProviderMock;
    private readonly PortsViewModel _viewModel;

    public PortsViewModelTests()
    {
        _portsProviderMock = new Mock<IPortsProvider>();
        _portsProviderMock.Setup(x => x.GetCustomPorts()).Returns([]);

        var factoryMock = new Mock<IViewModelsFactory>();

        _viewModel = new PortsViewModel(
            factoryMock.Object,
            _portsProviderMock.Object,
            [],
            NullLogger<PortsViewModel>.Instance
        );
    }

    [AvaloniaFact]
    public void Constructor_PortsTypesFromPorts()
    {
        var factoryMock = new Mock<IViewModelsFactory>();
        var portsProviderMock = new Mock<IPortsProvider>();
        portsProviderMock.Setup(x => x.GetCustomPorts()).Returns([]);

        var stubPort = new StubPort();

        factoryMock
            .Setup(x => x.GetPortViewModel(stubPort))
            .Returns(new PortViewModel(null!, null!, stubPort, null!));

        var ports = new BasePort[]
        {
            stubPort
        };

        var vm = new PortsViewModel(
            factoryMock.Object,
            portsProviderMock.Object,
            ports,
            NullLogger<PortsViewModel>.Instance
        );

        Assert.Single(vm.PortsTypes);
        Assert.Equal(PortEnum.Stub, vm.PortsTypes[0]);
    }

    [AvaloniaFact]
    public void Constructor_PortsList_Empty()
    {
        Assert.Empty(_viewModel.PortsList);
    }

    [AvaloniaFact]
    public void Constructor_PortsTypes_Empty()
    {
        Assert.Empty(_viewModel.PortsTypes);
    }

    [AvaloniaFact]
    public void CustomPorts_DelegatesToProvider()
    {
        _ = _viewModel.CustomPorts;

        _portsProviderMock.Verify(x => x.GetCustomPorts(), Times.Once);
    }

    [AvaloniaFact]
    public void PortsList_SetGet_Works()
    {
        Assert.Empty(_viewModel.PortsList);
        var list = ImmutableList<PortViewModel>.Empty.AddRange([]);
        _viewModel.PortsList = list;
        Assert.Same(list, _viewModel.PortsList);
    }

    [AvaloniaFact]
    public void ErrorMessage_Default_Empty()
    {
        Assert.Equal(string.Empty, _viewModel.ErrorMessage);
    }

    [AvaloniaFact]
    public void HasUpdates_Default_False()
    {
        Assert.False(_viewModel.HasUpdates);
    }

    [AvaloniaFact]
    public void IsEditorVisible_Default_False()
    {
        Assert.False(_viewModel.IsEditorVisible);
    }

    [AvaloniaFact]
    public void SelectedCustomPort_Default_Null()
    {
        Assert.Null(_viewModel.SelectedCustomPort);
    }

    [AvaloniaFact]
    public void AddCustomPortCommand_ClearsFieldsAndShowsEditor()
    {
        _viewModel.SelectedCustomPortName = "existing";
        _viewModel.SelectedCustomPortPath = "C:\\existing.exe";
        _viewModel.SelectedCustomPortType = PortEnum.EDuke32;
        _viewModel.SelectedCustomPort = new CustomPort
        {
            Name = "old",
            Path = "C:\\old.exe",
            BasePort = new StubPort()
        };
        _viewModel.ErrorMessage = "some error";
        _viewModel.IsEditorVisible = false;

        _viewModel.AddCustomPortCommand.Execute(null);

        Assert.Equal(string.Empty, _viewModel.ErrorMessage);
        Assert.Equal(string.Empty, _viewModel.SelectedCustomPortName);
        Assert.Equal(string.Empty, _viewModel.SelectedCustomPortPath);
        Assert.Null(_viewModel.SelectedCustomPortType);
        Assert.Null(_viewModel.SelectedCustomPort);
        Assert.True(_viewModel.IsEditorVisible);
    }

    [AvaloniaFact]
    public void EditCustomPort_CanExecute_Null_ReturnsFalse()
    {
        Assert.Null(_viewModel.SelectedCustomPort);
        Assert.False(_viewModel.EditCustomPortCommand.CanExecute(null));
    }

    [AvaloniaFact]
    public void EditCustomPort_CanExecute_NotNull_ReturnsTrue()
    {
        _viewModel.SelectedCustomPort = new CustomPort
        {
            Name = "test",
            Path = "test.exe",
            BasePort = new StubPort()
        };

        Assert.True(_viewModel.EditCustomPortCommand.CanExecute(null));
    }

    [AvaloniaFact]
    public void EditCustomPortCommand_PopulatesFormAndShowsEditor()
    {
        var customPort = new CustomPort
        {
            Name = "MyPort",
            Path = "C:\\myport.exe",
            BasePort = new StubPort()
        };
        _viewModel.SelectedCustomPort = customPort;

        _viewModel.EditCustomPortCommand.Execute(null);

        Assert.Equal("MyPort", _viewModel.SelectedCustomPortName);
        Assert.Equal("C:\\myport.exe", _viewModel.SelectedCustomPortPath);
        Assert.Equal(PortEnum.Stub, _viewModel.SelectedCustomPortType);
        Assert.True(_viewModel.IsEditorVisible);
    }

    [AvaloniaFact]
    public void EditCustomPortCommand_NullSelected_CanExecuteFalse()
    {
        Assert.Null(_viewModel.SelectedCustomPort);
        Assert.False(_viewModel.EditCustomPortCommand.CanExecute(null));
    }

    [AvaloniaFact]
    public void DeleteCustomPort_CanExecute_Null_ReturnsFalse()
    {
        Assert.Null(_viewModel.SelectedCustomPort);
        Assert.False(_viewModel.DeleteCustomPortCommand.CanExecute(null));
    }

    [AvaloniaFact]
    public void DeleteCustomPort_CanExecute_NotNull_ReturnsTrue()
    {
        _viewModel.SelectedCustomPort = new CustomPort
        {
            Name = "test",
            Path = "test.exe",
            BasePort = new StubPort()
        };

        Assert.True(_viewModel.DeleteCustomPortCommand.CanExecute(null));
    }

    [AvaloniaFact]
    public void DeleteCustomPortCommand_DeletesAndRefreshes()
    {
        var customPort = new CustomPort
        {
            Name = "DeleteMe",
            Path = "C:\\delete.exe",
            BasePort = new StubPort()
        };
        _viewModel.SelectedCustomPort = customPort;

        _viewModel.DeleteCustomPortCommand.Execute(null);

        _portsProviderMock.Verify(x => x.DeleteCustomPort("DeleteMe"), Times.Once);
    }

    [AvaloniaFact]
    public void DeleteCustomPortCommand_SelectedPortNull_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => _viewModel.DeleteCustomPortCommand.Execute(null));
    }

    [AvaloniaFact]
    public void SaveCustomPortCommand_EmptyName_ShowsError()
    {
        _viewModel.AddCustomPortCommand.Execute(null);
        _viewModel.SelectedCustomPortName = string.Empty;
        _viewModel.SelectedCustomPortPath = "C:\\test.exe";
        _viewModel.SelectedCustomPortType = PortEnum.Stub;

        _viewModel.SaveCustomPortCommand.Execute(null);

        Assert.Equal("Name is required", _viewModel.ErrorMessage);
        Assert.True(_viewModel.IsEditorVisible);
    }

    [AvaloniaFact]
    public void SaveCustomPortCommand_EmptyPath_ShowsError()
    {
        _viewModel.AddCustomPortCommand.Execute(null);
        _viewModel.SelectedCustomPortName = "TestPort";
        _viewModel.SelectedCustomPortPath = string.Empty;
        _viewModel.SelectedCustomPortType = PortEnum.Stub;

        _viewModel.SaveCustomPortCommand.Execute(null);

        Assert.Equal("Path to exe is required", _viewModel.ErrorMessage);
        Assert.True(_viewModel.IsEditorVisible);
    }

    [AvaloniaFact]
    public void SaveCustomPortCommand_NullType_ShowsError()
    {
        _viewModel.AddCustomPortCommand.Execute(null);
        _viewModel.SelectedCustomPortName = "TestPort";
        _viewModel.SelectedCustomPortPath = "C:\\test.exe";

        _viewModel.SaveCustomPortCommand.Execute(null);

        Assert.Equal("Port type is required", _viewModel.ErrorMessage);
        Assert.True(_viewModel.IsEditorVisible);
    }

    [AvaloniaFact]
    public void SaveCustomPortCommand_DuplicateNameForNewPort_ShowsError()
    {
        var existing = new CustomPort
        {
            Name = "ExistingPort",
            Path = "C:\\existing.exe",
            BasePort = new StubPort()
        };
        _portsProviderMock.Setup(x => x.GetCustomPorts()).Returns([existing]);

        var factoryMock = new Mock<IViewModelsFactory>();

        var vm = new PortsViewModel(
            factoryMock.Object,
            _portsProviderMock.Object,
            [],
            NullLogger<PortsViewModel>.Instance
        );

        vm.AddCustomPortCommand.Execute(null);
        vm.SelectedCustomPortName = "ExistingPort";
        vm.SelectedCustomPortPath = "C:\\unique.exe";
        vm.SelectedCustomPortType = PortEnum.Stub;

        vm.SaveCustomPortCommand.Execute(null);

        Assert.Equal("Port with the same name already exists", vm.ErrorMessage);
        Assert.True(vm.IsEditorVisible);
    }

    [AvaloniaFact]
    public void SaveCustomPortCommand_ExeNotFound_ShowsError()
    {
        _viewModel.AddCustomPortCommand.Execute(null);
        _viewModel.SelectedCustomPortName = "NewPort";
        _viewModel.SelectedCustomPortPath = "C:\\nonexistent\\port.exe";
        _viewModel.SelectedCustomPortType = PortEnum.Stub;

        _viewModel.SaveCustomPortCommand.Execute(null);

        Assert.Equal("Executable doesn't exist", _viewModel.ErrorMessage);
        Assert.True(_viewModel.IsEditorVisible);
    }

    [AvaloniaFact]
    public void SaveCustomPortCommand_ValidData_SavesAndHidesEditor()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            _portsProviderMock.Setup(x => x.GetCustomPorts()).Returns([]);

            var factoryMock = new Mock<IViewModelsFactory>();

            var vm = new PortsViewModel(
                factoryMock.Object,
                _portsProviderMock.Object,
                [],
                NullLogger<PortsViewModel>.Instance
            );

            vm.AddCustomPortCommand.Execute(null);
            vm.SelectedCustomPortName = "NewValidPort";
            vm.SelectedCustomPortPath = tempFile;
            vm.SelectedCustomPortType = PortEnum.Stub;

            vm.SaveCustomPortCommand.Execute(null);

            _portsProviderMock.Verify(
                x => x.AddOrChangeCustomPort(null, "NewValidPort", tempFile, PortEnum.Stub),
                Times.Once
            );
            Assert.False(vm.IsEditorVisible);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [AvaloniaFact]
    public void SaveCustomPortCommand_UpdateExisting_ValidData()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            var existing = new CustomPort
            {
                Name = "OldPort",
                Path = "C:\\old.exe",
                BasePort = new StubPort()
            };
            _portsProviderMock.Setup(x => x.GetCustomPorts()).Returns([existing]);

            var factoryMock = new Mock<IViewModelsFactory>();

            var vm = new PortsViewModel(
                factoryMock.Object,
                _portsProviderMock.Object,
                [],
                NullLogger<PortsViewModel>.Instance
            );

            vm.SelectedCustomPort = existing;

            vm.EditCustomPortCommand.Execute(null);
            vm.SelectedCustomPortPath = tempFile;

            vm.SaveCustomPortCommand.Execute(null);

            _portsProviderMock.Verify(
                x => x.AddOrChangeCustomPort("OldPort", "OldPort", tempFile, PortEnum.Stub),
                Times.Once
            );
            Assert.False(vm.IsEditorVisible);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [AvaloniaFact]
    public void SaveCustomPortCommand_DuplicateNameAllowedOnEdit_DoesNotShowDuplicateError()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            var existing = new CustomPort
            {
                Name = "ExistingPort",
                Path = "C:\\existing.exe",
                BasePort = new StubPort()
            };
            _portsProviderMock.Setup(x => x.GetCustomPorts()).Returns([existing]);

            var factoryMock = new Mock<IViewModelsFactory>();

            var vm = new PortsViewModel(
                factoryMock.Object,
                _portsProviderMock.Object,
                [],
                NullLogger<PortsViewModel>.Instance
            );

            vm.SelectedCustomPort = existing;

            vm.EditCustomPortCommand.Execute(null);
            vm.SelectedCustomPortPath = tempFile;

            vm.SaveCustomPortCommand.Execute(null);

            _portsProviderMock.Verify(
                x => x.AddOrChangeCustomPort("ExistingPort", "ExistingPort", tempFile, PortEnum.Stub),
                Times.Once
            );
            Assert.False(vm.IsEditorVisible);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [AvaloniaFact]
    public void CancelCommand_HidesEditor()
    {
        _viewModel.AddCustomPortCommand.Execute(null);
        Assert.True(_viewModel.IsEditorVisible);

        _viewModel.CancelCommand.Execute(null);

        Assert.False(_viewModel.IsEditorVisible);
    }

    [AvaloniaFact]
    public void OnPortChanged_EmptyPortsList_HasUpdatesFalse()
    {
        _viewModel.PortsList = [];

        _viewModel.OnPortChanged(PortEnum.Stub);

        Assert.False(_viewModel.HasUpdates);
    }

    [AvaloniaFact]
    public void SelectedCustomPort_Changed_RaisesCanExecuteChanged()
    {
        var editCanExecuteChanged = false;
        var deleteCanExecuteChanged = false;

        _viewModel.EditCustomPortCommand.CanExecuteChanged += (_, _) => editCanExecuteChanged = true;
        _viewModel.DeleteCustomPortCommand.CanExecuteChanged += (_, _) => deleteCanExecuteChanged = true;

        _viewModel.SelectedCustomPort = new CustomPort
        {
            Name = "test",
            Path = "test.exe",
            BasePort = new StubPort()
        };

        Assert.True(editCanExecuteChanged);
        Assert.True(deleteCanExecuteChanged);
    }

    [AvaloniaFact]
    public void PropertyChanged_ErrorMessage_Fires()
    {
        var fired = false;
        _viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(PortsViewModel.ErrorMessage))
                fired = true;
        };

        _viewModel.ErrorMessage = "test";

        Assert.True(fired);
    }

    [AvaloniaFact]
    public void PropertyChanged_IsEditorVisible_Fires()
    {
        var fired = false;
        _viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(PortsViewModel.IsEditorVisible))
                fired = true;
        };

        _viewModel.IsEditorVisible = true;

        Assert.True(fired);
    }

    [AvaloniaFact]
    public void SelectedCustomPortName_PropertyChanged_Fires()
    {
        var fired = false;
        _viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(PortsViewModel.SelectedCustomPortName))
                fired = true;
        };

        _viewModel.SelectedCustomPortName = "NewName";

        Assert.True(fired);
    }
}
