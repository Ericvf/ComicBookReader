using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace ComicBookReader.App.Framework
{
    public static class VisualTreeHelperExtensions
    {
        public static T FindChild<T>(this DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;

            T childElement = null; int childrenCount = VisualTreeHelper.GetChildrenCount(parent); for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                T childType = child as T; if (childType == null)
                {
                    childElement = FindChild<T>(child); if (childElement != null) break;
                }
                else
                {
                    childElement = (T)child; break;
                }
            } return childElement;
        }

        public static T FirstVisualAncestorOfType<T>(this DependencyObject element) where T : DependencyObject
        {
            if (element == null) return null;

            var parent = VisualTreeHelper.GetParent(element) as DependencyObject;
            while (parent != null)
            {
                if (parent is T)
                    return (T)parent;

                parent = VisualTreeHelper.GetParent(parent) as DependencyObject;
            }
            return null;
        }

        public static T LastVisualAncestorOfType<T>(this DependencyObject element) where T : DependencyObject
        {
            T item = null;
            var parent = VisualTreeHelper.GetParent(element) as DependencyObject;
            while (parent != null)
            {
                if (parent is T)
                    item = (T)parent;

                parent = VisualTreeHelper.GetParent(parent) as DependencyObject;
            }
            return item;
        }
    }

}
