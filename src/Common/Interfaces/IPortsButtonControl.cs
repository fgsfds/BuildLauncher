using CommunityToolkit.Mvvm.Input;

namespace Common.Interfaces
{
    public interface IPortsButtonControl
    {
        public IRelayCommand? OpenFolderCommand { get; }
    }
}
