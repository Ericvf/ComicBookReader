using AnimationExtensions;
using ComicBookReader.App.ViewModel;
using ComicBookReader.App.Animations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using ComicBookReader.App.Model;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ComicBookReader.App.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AllPublishers : ComicBookReader.App.Common.LayoutAwarePage
    {
        HomeVM VM = ComicBookReader2.App.HomeVM;

        public AllPublishers()
        {
            this.InitializeComponent();
            this.DataContext = this.VM;
        }

        protected async override void GoBack(object sender, RoutedEventArgs e)
        {
            this.gridView.PageOut().Play();
            await this.pageHeader.PageHeaderOut().PlayAsync();
            base.GoBack(sender, e);
        }

        internal async void gv_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.IsHitTestVisible = false;

            this.gridView.PageOut().Play();
            await this.pageHeader.PageHeaderOut().PlayAsync();

            Frame.Navigate(typeof(Publisher), e.ClickedItem as ComicPublisher);
        }


        private void tbImport_Click(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Views.Import));
        }

    }
}
