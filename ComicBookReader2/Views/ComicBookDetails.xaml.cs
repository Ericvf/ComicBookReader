using AnimationExtensions;
using ComicBookReader.App.Animations;
using ComicBookReader.App.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.FileProperties;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace ComicBookReader.App.Views
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class ComicBookDetails : ComicBookReader.App.Common.LayoutAwarePage
    {
        private ComicBookItem VM;

        public ComicBookDetails()
        {
            this.InitializeComponent();
            properties.Hide();
        }

        protected async override void GoBack(object sender, RoutedEventArgs e)
        {
            this.pageHeader.PageHeaderOut().Play();
            this.properties.MessageOut().Play();

            await this.gridView.PageOut().PlayAsync();
            await this.VM.Save();

            base.GoBack(sender, e);
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            
            if (e.Parameter != null)
            {
                this.progressRing.IsActive = true;
                this.VM = e.Parameter as ComicBookItem;

                await this.VM.LoadThumbnails();
                await this.VM.GetFolderSize();

                this.progressRing.IsActive = false;
                this.DataContext = this.VM;

                this.properties.Hide();
                this.properties.Visibility = Windows.UI.Xaml.Visibility.Visible;
                await this.properties
                    .Move(25, 0)
                    .Move(0, 0, 500, Eq.OutSine)
                    .Fade(1, 500)
                    .PlayAsync();
            }

        }

        private async void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.gridView.IsHitTestVisible = false;

            
            var vm = e.ClickedItem as StorageItemThumbnail;
           

            if (vm != null)
            {
                var index = this.VM.Thumbnails.IndexOf(vm);
                this.VM.ComicBookInfo.XData.LastPage = index;
            }

            await this.VM.Save();

            await OpenComic();
        }

        private async Task OpenComic()
        {
            this.properties.MessageOut().Play();
            this.pageHeader.PageHeaderOut().Play();
            await gridView.PageOut().PlayAsync();

            this.Frame.Navigate(typeof(ComicBookViewer), this.VM);
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected  override void SaveState(Dictionary<String, Object> pageState)
        {
        }

        public static Rect GetElementRect(FrameworkElement element)
        {
            GeneralTransform buttonTransform = element.TransformToVisual(null);
            Point point = buttonTransform.TransformPoint(new Point());
            return new Rect(point, new Size(element.ActualWidth, element.ActualHeight));
        }

        private async void Image_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            var menu = new PopupMenu();
            menu.Commands.Add(new UICommand(ComicBookReader2.App.ResourceLoader.GetString("GeneralDelete"), async (command) =>
            {
                var element = (e.OriginalSource as FrameworkElement);
                var gv = element.FindParent<GridViewItem>();
                var vm = gv.DataContext as StorageItemThumbnail;
                var index = this.VM.Thumbnails.IndexOf(vm);

                await this.VM.DeleteThumbnail(index);
            }));

            var chosenCommand = await menu.ShowForSelectionAsync(GetElementRect((FrameworkElement)sender));
        }

        private async void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            await this.OpenComic();
        }

        private async void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new MessageDialog(ComicBookReader2.App.ResourceLoader.GetString("ComicDetailsDeleteComic"), ComicBookReader2.App.ResourceLoader.GetString("GeneralDelete"));
            dialog.Commands.Add(new UICommand(ComicBookReader2.App.ResourceLoader.GetString("GeneralDontDelete"), (command) =>
            {
            }));

            dialog.Commands.Add(new UICommand(ComicBookReader2.App.ResourceLoader.GetString("GeneralGoAhead"), async (command) =>
            {
                //App.HomeVM.RefreshGroups();
                //App.HomeVM.Invalidate();

                await this.VM.DeleteComic();

                this.pageHeader.PageHeaderOut().Play();
                this.properties.MessageOut().Play();

                await this.gridView.PageOut().PlayAsync();
                base.GoBack(sender, e);
            }));

            dialog.DefaultCommandIndex = 1;
            await dialog.ShowAsync();
        }
    }
}
