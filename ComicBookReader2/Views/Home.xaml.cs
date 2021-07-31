using ComicBookReader.App.Animations;
using ComicBookReader.App.Model;
using ComicBookReader.App.ViewModel;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.ApplicationSettings;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ComicBookReader.App.Views
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class Home : ComicBookReader.App.Common.LayoutAwarePage, INotifyPropertyChanged
    {
        private HomeVM vm = ComicBookReader2.App.HomeVM;

        public Home()
        {
            this.InitializeComponent();
            this.DataContext = this.vm;
        }

        protected async override void OnNavigatedTo(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await LoadData();            
        }


        private async Task LoadData()
        {
            this.vm.IsLoading = true;
            await this.loading.MessageIn().PlayAsync();

            await this.vm.LoadComics();
            await this.vm.LoadSeries();
            await this.vm.LoadPublishers();
            this.vm.RefreshGroups();

            this.DataContext = null;
            this.DataContext = vm;

            await this.loading.MessageOut().PlayAsync();

            this.vm.IsLoading = false;
        }

        public async Task Ax_PageOut()
        {
            this.itemGridView.IsHitTestVisible = false;
            this.pageHeader.PageHeaderOut().Play();
            this.NoComics.PageOut().Play();
            await itemGridView.PageOut().PlayAsync();
        }

        private async void appbarImportClick(object sender, RoutedEventArgs e)
        {
            await this.Ax_PageOut();
            this.Frame.Navigate(typeof(Views.Import));
        }

        private async void tbImport_Click(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            await this.Ax_PageOut();
            this.Frame.Navigate(typeof(Views.Import));
        }

        private async void gvItem_Click(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is ComicBookItem)
            {
                await this.Ax_PageOut();
                this.Frame.Navigate(typeof(Views.ComicBookDetails), e.ClickedItem as ComicBookItem);
            }
            else if (e.ClickedItem is ComicSeries)
            {
                await this.Ax_PageOut();
                this.Frame.Navigate(typeof(Views.Series), e.ClickedItem as ComicSeries);
            }
            if (e.ClickedItem is ComicPublisher)
            {
                await this.Ax_PageOut();
                this.Frame.Navigate(typeof(Views.Publisher), e.ClickedItem as ComicPublisher);
            }
        }

        private async void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            this.vm.Invalidate();
            await this.LoadData();
        }

        private async void gvGroup_Click(object sender, RoutedEventArgs e)
        {
            var senderButton = sender as Button;
            if (senderButton != null && senderButton.Tag is int)
            {
                int groupIndex = (int)senderButton.Tag;

                if (groupIndex == 0)
                {
                    await this.Ax_PageOut();
                    this.Frame.Navigate(typeof(Views.AllComics));
                }
                else if (groupIndex == 1)
                {
                    await this.Ax_PageOut();
                    this.Frame.Navigate(typeof(Views.AllSeries));
                }
                else if (groupIndex == 2)
                {
                    await this.Ax_PageOut();
                    this.Frame.Navigate(typeof(Views.AllPublishers));
                }
            }
        }
    }
}
