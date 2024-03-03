using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Common.Helpers;
using System.Globalization;

namespace BuildLauncher.Helpers
{
    /// <summary>
    /// Converts Stream to Bitmap
    /// </summary>
    public sealed class ImagePathToBitmapConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null)
            {
                return new Bitmap(AssetLoader.Open(new Uri($"avares://BuildLauncher/Assets/blank.png")));
            }

            if (value is not Stream stream)
            {
                ThrowHelper.NotImplementedException();
                return null;
            }

            return new Bitmap(stream);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("ConvertBack method for ImagePathToBitmapConverter is not implemented.");
        }
    }
}
