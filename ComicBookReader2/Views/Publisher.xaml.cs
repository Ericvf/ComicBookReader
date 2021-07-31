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
using Windows.UI.Popups;
using System.Threading.Tasks;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ComicBookReader.App.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Publisher : ComicBookReader.App.Common.LayoutAwarePage
    {
        ComicPublisher publisher;
        InputPrompt prompt;

        protected async override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (this.prompt != null && this.prompt.IsOpen)
                await this.prompt.ClosePopup();
            base.OnNavigatingFrom(e);
        }

        public Publisher()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
                 base.OnNavigatedTo(e);
            this.publisher = e.Parameter as ComicPublisher;
            this.DataContext = this.publisher;
       
        }

        protected async override void GoBack(object sender, RoutedEventArgs e)
        {
            this.gridView.PageOut().Play();
            await this.pageHeader.PageHeaderOut().PlayAsync();

            base.GoBack(sender, e);
        }

        private async void gvItem_Click(object sender, ItemClickEventArgs e)
        {
            this.IsHitTestVisible = false;

            this.gridView.PageOut().Play();
            await this.pageHeader.PageHeaderOut().PlayAsync();
            Frame.Navigate(typeof(ComicBookDetails), e.ClickedItem as ComicBookItem);
        }

        private void btnSelectAll_Click(object sender, RoutedEventArgs e)
        {
            this.gridView.SelectAll();
        }

        private void btnEditSeries_Click(object sender, RoutedEventArgs e)
        {
            var inputPrompt = new InputPrompt();
            inputPrompt.Title = ComicBookReader2.App.ResourceLoader.GetString(@"SeriesInputTitle");
            inputPrompt.Message = ComicBookReader2.App.ResourceLoader.GetString(@"SeriesInputMessage");

            if (this.gridView.SelectedItems.Count > 0)
            {
                var item = this.gridView.SelectedItems.FirstOrDefault() as ComicBookItem;
                inputPrompt.NewName = item.Series;
            }
            else
            {
                this.gridView.SelectAll();
                var item = this.gridView.SelectedItems.FirstOrDefault() as ComicBookItem;
                inputPrompt.NewName = item.Series;
            }

            inputPrompt.Save += async (s, ex) =>
            {
                var items = this.gridView.SelectedItems;
                if (items.Count > 0)
                {
                    foreach (var item in items)
                    {
                        if (item is ComicBookItem)
                        {
                            var comicbookItem = item as ComicBookItem;
                            comicbookItem.Series = inputPrompt.NewName;
                            await comicbookItem.Save();
                        }
                    }

                    ComicBookReader2.App.HomeVM.Invalidate();
                    this.GoBack(this, null);
                }
            };

            inputPrompt.OpenPopup();
            this.prompt = inputPrompt;
        }

        private void btnEditPublisher_Click(object sender, RoutedEventArgs e)
        {
            var inputPrompt = new InputPrompt();
            inputPrompt.Title = ComicBookReader2.App.ResourceLoader.GetString(@"PublisherInputTitle");
            inputPrompt.Message = ComicBookReader2.App.ResourceLoader.GetString(@"PublisherInputMessage");

            if (this.gridView.SelectedItems.Count > 0)
            {
                var item = this.gridView.SelectedItems.FirstOrDefault() as ComicBookItem;
                inputPrompt.NewName = item.Publisher;
            }
            else
            {
                this.gridView.SelectAll();
                var item = this.gridView.SelectedItems.FirstOrDefault() as ComicBookItem;
                inputPrompt.NewName = item.Publisher;
            }

            inputPrompt.Save += async (s, ex) =>
            {
                var items = this.gridView.SelectedItems;
                if (items.Count > 0)
                {
                    foreach (var item in items)
                    {
                        if (item is ComicBookItem)
                        {
                            var comicbookItem = item as ComicBookItem;
                            comicbookItem.Publisher = inputPrompt.NewName;
                            await comicbookItem.Save();
                        }
                    }

                    ComicBookReader2.App.HomeVM.Invalidate();
                    this.GoBack(this, null);
                }
            };

            inputPrompt.OpenPopup();
            this.prompt = inputPrompt;
        }

        private async void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (this.gridView.SelectedItems.Count > 0)
            {
                var dialog = new MessageDialog(ComicBookReader2.App.ResourceLoader.GetString(@"ComicDetailsDeleteComics"), ComicBookReader2.App.ResourceLoader.GetString(@"GeneralDelete"));
                dialog.Commands.Add(new UICommand(ComicBookReader2.App.ResourceLoader.GetString(@"GeneralDontDelete"), (command) =>{}));
                dialog.Commands.Add(new UICommand(ComicBookReader2.App.ResourceLoader.GetString(@"GeneralGoAhead"), async (command) =>
                {
                    ComicBookReader2.App.HomeVM.Invalidate();

                    var items = new List<object>(this.gridView.SelectedItems);
                    if (items.Count > 0)
                    {
                        this.IsHitTestVisible = false;

                        foreach (var item in items)
                        {
                            if (item is ComicBookItem)
                            {
                                var comicbookItem = item as ComicBookItem;
                                await comicbookItem.DeleteComic();
                                this.publisher.Comics.Remove(comicbookItem);
                            }
                        }
                        
                        this.IsHitTestVisible = true;

                        if (this.publisher.Comics.Count == 0)
                            this.GoBack(this, null);
                    }
                }));

                dialog.DefaultCommandIndex = 1;
                await dialog.ShowAsync();
            }
        }

        private void tbImport_Click(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Views.Import));
        }
    }
}
