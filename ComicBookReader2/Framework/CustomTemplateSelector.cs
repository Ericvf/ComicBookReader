using ComicBookReader.App.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ComicBookReader.App.Framework
{
    public class CustomSampleDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate FirstDataTemplate { get; set; }
        public DataTemplate SecondDataTemplate { get; set; }
        public DataTemplate ThirdDataTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is ComicBookItem)
                return FirstDataTemplate;

            if (item is ComicSeries)
                return SecondDataTemplate;

            if (item is ComicPublisher)
                return ThirdDataTemplate;

            return null;
        }
    }
}
