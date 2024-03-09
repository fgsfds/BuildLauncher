using CommunityToolkit.Mvvm.Input;

namespace BuildLauncher.ViewModels
{
    public interface IPortsButtonControl
    {
        bool SkipIntroCheckbox { get; set; }

        public IRelayCommand? OpenFolderCommand { get; }
    }
}
