using AnimationExtensions;
using ComicBookReader.App.Common;
using System;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ComicBookReader.App.UserControls
{
    public sealed partial class AboutPopup : UserControl, ITaskPaneControl
    {
        public AboutPopup()
        {
            this.InitializeComponent();
        }

        public void Show()
        {
            var items = this.aboutPanel.FindChilden<StackPanel>();
            items.For((i, f) => f
                .Move(150, 0)
                .Fade()
                .Wait(i * 300)
                .Move(0, 0, 1500, Eq.OutElastic)
                .Fade(1, 1000, Eq.OutSine)
            ).Play();
        }

        private async void btnVisitSharpCompress(object sender, RoutedEventArgs e)
        {
            var uri = new Uri("http://sharpcompress.codeplex.com/");
            await Launcher.LaunchUriAsync(uri);
        }

        private async void btnVisitSuprLogo(object sender, RoutedEventArgs e)
        {
            var uri = new Uri("http://www.appbyfex.com/Home/SuprLogo");
            await Launcher.LaunchUriAsync(uri);
        }

        private async void btnVisitAxNet(object sender, RoutedEventArgs e)
        {
            var uri = new Uri("http://www.appbyfex.com/Home/Axnet");
            await Launcher.LaunchUriAsync(uri);
        }

        private async void btnRateAndReview(object sender, RoutedEventArgs e)
        {
            var uri = new Uri("ms-windows-store:REVIEW?PFN=48246Appbyfex.ComicBookReader_9rf50bhnd1c34");
            await Launcher.LaunchUriAsync(uri);
        }

        private async void btnAppByFex(object sender, RoutedEventArgs e)
        {
            var uri = new Uri("mailto:e.feggelen@outlook.com");
            await Launcher.LaunchUriAsync(uri);
        }
    }
}