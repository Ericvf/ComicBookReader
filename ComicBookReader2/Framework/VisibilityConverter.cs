using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace ComicBookReader.App.Framework
{
    public sealed class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool visibility = false;
            bool inversed = parameter != null;

            if (value is bool)
                visibility = (bool)value;

            //if (value is string)
            //    visibility = !string.IsNullOrEmpty((string)value);

            //if (value is BitmapImage && value != null)
            //    visibility = true;

            if (value is IList)
                visibility = (value as IList).Count > 0;

            if (value is int)
                visibility = (int)value > 0;

            if (inversed)
                visibility = !visibility;

            return visibility ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
