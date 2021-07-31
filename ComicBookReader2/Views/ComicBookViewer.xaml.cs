using AnimationExtensions;
using ComicBookReader.App.Framework;
using ComicBookReader.App.Model;
using ComicBookReader.App.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.Store;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using ComicBookReader.App.Animations;
using System.Threading.Tasks;

namespace ComicBookReader.App.Views
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public partial class ComicBookViewer : ComicBookReader.App.Common.LayoutAwarePage
    {
        private ComicBookViewerVM VM;
        private bool isZoom;
        private bool fadeInLeft;
        private bool fadeInRight;
        private bool showAds;

        public ComicBookViewer()
        {
            this.InitializeComponent();
        }

        protected async override void SaveState(Dictionary<String, Object> pageState)
        {
            await this.VM.ComicBookItem.Save();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (!ComicBookReader2.App.LicenseInformation.ProductLicenses[@"Remove Ads"].IsActive)
            {
                this.adControl.Visibility = Windows.UI.Xaml.Visibility.Visible;
                this.showAds = true;
            }

            if (e.Parameter != null)
            {
                this.VM = new ComicBookViewerVM(e.Parameter as ComicBookItem);
                this.progressRing.IsActive = true;
                await this.VM.LoadPages();

                this.blackBg.Visibility = Windows.UI.Xaml.Visibility.Visible;
                this.blackBg.Fade(1, 500, Eq.OutSine).Play();
                this.progressRing.IsActive = false;
                this.DataContext = this.VM;
            }

        }

        protected async override void GoBack(object sender, RoutedEventArgs e)
        {
            this.BottomAppBar.IsOpen = false;
            this.TopAppBar.IsOpen = false;

            await this.VM.ComicBookItem.Save();
            await this.blackBg.Fade(0, 300, Eq.OutSine).PlayAsync();
            await this.flipView.PageOut().PlayAsync();
            base.GoBack(sender, e);
        }

        private async void flipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Check for valid selection
            if (this.flipView.SelectedIndex < 0 || this.flipView.SelectedIndex > this.flipView.Items.Count)
                return;

            // Find the item that was selected
            var nextItem = e.AddedItems.FirstOrDefault();
            if (nextItem != null)
            {
                // Find the items container
                var itemContainer = this.flipView.ItemContainerGenerator.ContainerFromIndex(this.flipView.SelectedIndex) as FrameworkElement;
                if (itemContainer != null)
                {
                    // Find the scrollviewer
                    var scrollViewer = itemContainer.FindChild<ScrollViewer>();
                    if (scrollViewer != null)
                    {
                        // Adjust the zoom for the current page
                        this.AdjustZoom(scrollViewer);

                        // Check if we used the navigation buttons 
                        if (fadeInLeft)
                        {
                            fadeInLeft = false;

                            // Fade in from left
                            await itemContainer
                                .Move(100, 0)
                                .Fade()
                                .Then()
                                .Move(0, 0, 300, Eq.OutBack)
                                .Fade(1, 300, Eq.OutSine).PlayAsync();
                        }
                        else if (fadeInRight)
                        {
                            fadeInRight = false;

                            // Fade in from right
                            await itemContainer
                                .Move(-100, 0)
                                .Fade()
                                .Then()
                                .Move(0, 0, 300, Eq.OutBack)
                                .Fade(1, 300, Eq.OutSine).PlayAsync();
                        }
                        else if (itemContainer.Opacity == 0)
                        {
                            // Show if not visible
                            await itemContainer
                                .Move()
                                .Fade(1, 300, Eq.OutSine).PlayAsync();
                        }
                    }
                }
            }
        }

        private void flipView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var selectedContainer = this.flipView.ItemContainerGenerator.ContainerFromIndex(this.flipView.SelectedIndex);
            if (selectedContainer != null)
                this.AdjustZoom(selectedContainer.FindChild<ScrollViewer>());
        }

        private void flipView_ImageLoaded(object sender, RoutedEventArgs e)
        {
            var image = sender as Image;
            if (image != null)
            {
                var comicBookPage = image.DataContext as ComicBookReader.App.ViewModel.ComicBookViewerVM.ComicBookPage;
                if (comicBookPage != null)
                {
                    if (comicBookPage.Width == 0 || comicBookPage.Height == 0)
                    {
                        comicBookPage.Width = 500;
                        comicBookPage.Height = 1000;
                    }

                    // Force sizing because of bugs
                    image.Width = comicBookPage.Width;
                    image.Height = comicBookPage.Height;
                }

                var scrollViewer = image.FindParent<ScrollViewer>();
                if (scrollViewer != null)
                    this.AdjustZoom(scrollViewer);
            }
        }

        private void ScrollViewer_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            // Find selected scrollviewer
            //var selectedContainer = this.flipView.ItemContainerGenerator.ContainerFromIndex(this.flipView.SelectedIndex);
            //var scrollViewer = selectedContainer.FindChild<ScrollViewer>();

            var scrollViewer = sender as ScrollViewer;
            if (scrollViewer != null)
            {
                isZoom = scrollViewer.ZoomFactor <= scrollViewer.MinZoomFactor;
                AdjustZoom(scrollViewer);
                e.Handled = true;
            }
        }

        private async void rectPrev_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await Prev();
        }

        private async void rectNext_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await Next();
        }

        private async void btnAppBarFirst_Click(object sender, RoutedEventArgs e)
        {
            await this.First();
        }

        private async void btnAppBarPrev_Click(object sender, RoutedEventArgs e)
        {
            await this.Prev();
        }

        private async void btnAppBarNext_Click(object sender, RoutedEventArgs e)
        {
            await this.Next();
        }

        private async void btnAppBarLast_Click(object sender, RoutedEventArgs e)
        {
            await this.Last();
        }

        private void AdjustZoom(ScrollViewer scrollViewer)
        {
            bool isTwoPage = this.VM.IsTwoPage;
            var images = scrollViewer.FindChilden<Image>();

            var prevImage = images[0];
            var image = images[1];
            var nextImage = images[2];

            if (image != null)
            {
                var comicBookPage = image.DataContext as ComicBookReader.App.ViewModel.ComicBookViewerVM.ComicBookPage;

                var w = comicBookPage.Width;
                var h = comicBookPage.Height;

                if (w == 0 || h == 0)
                    return;

                if (this.VM.IsLeftToRight)
                {
                    if (isTwoPage && comicBookPage.IsRegular
                        && comicBookPage.NextPage != null && comicBookPage.NextPage.IsRegular)
                    {
                        nextImage.Visibility = Windows.UI.Xaml.Visibility.Visible;
                        w += comicBookPage.NextPage.Width;
                    }
                    else if (isTwoPage && comicBookPage.IsRegular
                       && comicBookPage.PrevPage != null && comicBookPage.PrevPage.IsRegular)
                    {
                        prevImage.Visibility = Windows.UI.Xaml.Visibility.Visible;
                        w += comicBookPage.PrevPage.Width;
                    }
                    else
                    {
                        prevImage.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                        nextImage.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    }
                }
                else
                {
                    if (isTwoPage && comicBookPage.IsRegular
                       && comicBookPage.PrevPage != null && comicBookPage.PrevPage.IsRegular)
                    {
                        prevImage.Visibility = Windows.UI.Xaml.Visibility.Visible;
                        w += comicBookPage.PrevPage.Width;
                    }
                    else
                        if (isTwoPage && comicBookPage.IsRegular
                            && comicBookPage.NextPage != null && comicBookPage.NextPage.IsRegular)
                        {
                            nextImage.Visibility = Windows.UI.Xaml.Visibility.Visible;
                            w += comicBookPage.NextPage.Width;
                        }
                        else
                        {
                            prevImage.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                            nextImage.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                        }
                }

                var sw = this.flipView.ActualWidth;
                var sh = this.flipView.ActualHeight;

                if (sw == 0 || sh == 0)
                    return;

                float zoomFactorW = (float)(sw / w);
                float zoomFactorH = (float)(sh / h);

                bool widthBigger = zoomFactorW > zoomFactorH;

                var minZ = widthBigger ? zoomFactorH : zoomFactorW;
                var maxZ = !widthBigger ? zoomFactorH : zoomFactorW;
                scrollViewer.MinZoomFactor = minZ;
                scrollViewer.MaxZoomFactor = maxZ;

                scrollViewer.ZoomToFactor(!isZoom ? minZ : maxZ);
                scrollViewer.ScrollToVerticalOffset(0);
            }
        }

        private async Task First()
        {
            var curIndex = this.flipView.SelectedIndex;
            if (curIndex > 0)
            {
                var curContainer = this.flipView.ItemContainerGenerator.ContainerFromIndex(curIndex) as FrameworkElement;
                if (curContainer != null)
                {
                    await curContainer
                          .Fade(0, 300, Eq.OutSine)
                          .Move(100, 0, 300, Eq.OutSine)
                      .PlayAsync();
                    fadeInRight = true;
                }

                this.flipView.SelectedIndex = 0;
            }
        }

        private async Task Prev()
        {
            var curIndex = this.flipView.SelectedIndex;
            var newIndex = Math.Max(0, curIndex - 1);

            if (this.VM.IsTwoPage && this.VM.SelectedItem.IsRegular &&
                this.VM.SelectedItem.PrevPage != null &&
                this.VM.SelectedItem.PrevPage.IsRegular &&
                this.VM.SelectedItem.PrevPage.PrevPage != null &&
                this.VM.SelectedItem.PrevPage.PrevPage.IsRegular)
            {
                newIndex = Math.Max(0, curIndex - 2);
            }

            if (curIndex > newIndex)
            {
                var curContainer = this.flipView.ItemContainerGenerator.ContainerFromIndex(curIndex) as FrameworkElement;
                if (curContainer != null)
                {
                    await curContainer
                          .Fade(0, 300, Eq.OutSine)
                          .Move(100, 0, 300, Eq.OutSine)
                      .PlayAsync();
                    fadeInRight = true;
                }

                this.flipView.SelectedIndex = newIndex;
            }
        }

        private async Task Next()
        {

            var curIndex = this.flipView.SelectedIndex;
            var newIndex = Math.Min(curIndex + 1, this.VM.Pages.Count - 1);

            if (this.VM.IsTwoPage && this.VM.SelectedItem.IsRegular &&
                this.VM.SelectedItem.NextPage != null &&
                this.VM.SelectedItem.NextPage.IsRegular &&
                this.VM.SelectedItem.NextPage.NextPage != null &&
                this.VM.SelectedItem.NextPage.NextPage.IsRegular)
            {
                newIndex = Math.Min(curIndex + 2, this.VM.Pages.Count - 1);
            }

            if (curIndex < newIndex)
            {
                var curContainer = this.flipView.ItemContainerGenerator.ContainerFromIndex(curIndex) as FrameworkElement;
                if (curContainer != null)
                {
                    await curContainer
                        .Fade(0, 300, Eq.OutSine)
                        .Move(-100, 0, 300, Eq.OutSine)
                    .PlayAsync();
                    fadeInLeft = true;
                }

                this.flipView.SelectedIndex = newIndex;
            }
        }

        private async Task Last()
        {
            var curIndex = this.flipView.SelectedIndex;
            var newIndex = this.VM.Pages.Count - 1;

            if (curIndex < newIndex)
            {
                var curContainer = this.flipView.ItemContainerGenerator.ContainerFromIndex(curIndex) as FrameworkElement;
                if (curContainer != null)
                {
                    await curContainer
                        .Fade(0, 300, Eq.OutSine)
                        .Move(-100, 0, 300, Eq.OutSine)
                    .PlayAsync();
                    fadeInLeft = true;
                }

                this.flipView.SelectedIndex = newIndex;
            }
        }

        private async void RemoveAdsPurchase_Click(object sender, TappedRoutedEventArgs e)
        {
            if (!ComicBookReader2.App.LicenseInformation.ProductLicenses[@"Remove Ads"].IsActive)
            {
                try
                {
                    await CurrentApp.RequestProductPurchaseAsync(@"Remove Ads", false);
                    if (ComicBookReader2.App.LicenseInformation.ProductLicenses[@"Remove Ads"].IsActive)
                    {
                        this.adControl.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                        this.showAds = false;
                    }
                }
                catch
                {
                }
            }
        }

        private async void appbarBottom_Opened(object sender, object e)
        {
            if (this.showAds)
                await this.adContainer.Move(0, -100, 300, Eq.OutSine).PlayAsync();
        }

        private async void appbarBottom_Closed(object sender, object e)
        {
            if (this.showAds)
                await this.adContainer.Move(0, 0, 300, Eq.OutSine).PlayAsync();
        }

        private async void ToggleSplitPage(object sender, RoutedEventArgs e)
        {
            this.VM.IsTwoPage = !this.VM.IsTwoPage;

            var curIndex = this.flipView.SelectedIndex;
            var curContainer = this.flipView.ItemContainerGenerator.ContainerFromIndex(curIndex) as FrameworkElement;
            var sv = curContainer.FindChild<ScrollViewer>();
            if (sv != null)
            {
                await sv
                       .Fade(0, 500, Eq.OutSine)
                       .Move(0, 100, 500, Eq.InBack)
                   .PlayAsync();

                this.AdjustZoom(sv);

                await sv
                       .Fade(1, 500, Eq.OutSine)
                       .Move(0, 0, 500, Eq.OutBack)
                   .PlayAsync();

            }
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var curIndex = this.flipView.SelectedIndex;
            var curContainer = this.flipView.ItemContainerGenerator.ContainerFromIndex(curIndex) as FrameworkElement;
            var sv = curContainer.FindChild<ScrollViewer>();
            if (sv != null)
            {
                await sv
                       .Fade(0, 500, Eq.OutSine)
                       .Move(0, 100, 500, Eq.InBack)
                   .PlayAsync();

                this.VM.ToggleLtR();

                await sv
                       .Fade(1, 500, Eq.OutSine)
                       .Move(0, 0, 500, Eq.OutBack)
                   .PlayAsync();

            }
        }
    }
}
