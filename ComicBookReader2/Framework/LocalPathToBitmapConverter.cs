using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;

namespace ComicBookReader.App.Framework
{
    /// <summary>
    /// Value converter that translates true to <see cref="Visibility.Visible"/> and false to
    /// <see cref="Visibility.Collapsed"/>.
    /// </summary>
    public sealed class LocalPathToBitmapSourceConverter : IValueConverter
    {
        public async Task<BitmapImage> PathToStorageThumbnail(string filePath)
        {

            var file = await StorageFile.GetFileFromPathAsync(filePath);
            var fileStream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
            var source = new BitmapImage();
            source.SetSource(fileStream);

            return source;

        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return value;

            if (value.GetType() == typeof(string))
            {
                return PathToStorageThumbnail((string)value);
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
