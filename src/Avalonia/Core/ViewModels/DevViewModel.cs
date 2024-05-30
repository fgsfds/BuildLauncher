using Avalonia.Platform.Storage;
using BuildLauncher.Helpers;
using ClientCommon.Config;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mods;

namespace BuildLauncher.ViewModels
{
    internal sealed partial class DevViewModel : ObservableObject
    {
        private readonly IConfigProvider _config;
        private readonly FilesUploader _filesUploader;

        public DevViewModel(
            IConfigProvider config,
            FilesUploader filesUploader
            )
        {
            _config = config;
            _filesUploader = filesUploader;

            ApiPasswordTextBox = _config.ApiPassword;
        }


        #region Binding Properties

        /// <summary>
        /// Use local API parameter
        /// </summary>
        public bool LocalApiCheckbox
        {
            get => _config.UseLocalApi;
            set
            {
                _config.UseLocalApi = value;
                OnPropertyChanged(nameof(LocalApiCheckbox));
            }
        }

        [ObservableProperty]
        private string _uploadingStatusMessage = string.Empty;

        #endregion


        #region Relay Commands

        [RelayCommand]
        private async Task AddAddonAsync()
        {

            FilePickerFileType z64 = new("Zipped addon")
            {
                Patterns = new[] { "*.zip" }
            };

            var files = await Properties.TopLevel.StorageProvider.OpenFilePickerAsync(
                new FilePickerOpenOptions
                {
                    Title = "Choose addon file",
                    AllowMultiple = false,
                    FileTypeFilter = [z64]
                }).ConfigureAwait(true);

            if (files.Count == 0)
            {
                return;
            }

            var result = await _filesUploader.AddAddonToDatabaseAsync(files[0].Path.LocalPath);

            if (result)
            {
                UploadingStatusMessage = "Success";
            }
            else
            {
                UploadingStatusMessage = "Error";
            }
        }

        [ObservableProperty]
        private string _apiPasswordTextBox;
        partial void OnApiPasswordTextBoxChanged(string value)
        {
            var configValue = _config.ApiPassword;

            if (value.Equals(configValue))
            {
                return;
            }
            else
            {
                _config.ApiPassword = value;
            }
        }

        #endregion
    }
}
