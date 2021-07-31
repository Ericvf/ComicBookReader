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
using Windows.UI.Popups;
using ComicBookReader.App.Model;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ComicBookReader.App.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Import : ComicBookReader.App.Common.LayoutAwarePage
    {
        ImportVM vm = ComicBookReader2.App.ImportVM;

        public Import()
        {
            this.InitializeComponent();
            this.DataContext = vm;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (this.vm.ActivatedFiles != null && this.vm.ActivatedFiles.Count > 0)
            {
                this.IsHitTestVisible = false;

                ComicBookItem item = null;

                try
                {
                    await this.importMessage.MessageOut().PlayAsync();
                    await this.loadingMessage.MessageIn().PlayAsync();

                    this.importMessage.Hide();
                    this.loadingMessage.Opacity = 1;

                    item = await this.vm.ImportActivatedFiles();
                    await this.loadingMessage.MessageOut().PlayAsync();
                    await this.doneMessage.MessageIn().PlayAsync();

                    await this.doneMessage.MessageOut().PlayAsync();
                    await this.pageHeader.PageHeaderOut().PlayAsync();
                }
                catch (Exception ex)
                {
                    var dlg = new MessageDialog(ex.ToString(), "Unhandled exception (please report to e.feggelen@gmail.com)");
                    dlg.ShowAsync();
                }

                ComicBookReader2.App.HomeVM.Invalidate();
                this.IsHitTestVisible = true;

                if (item != null)
                {
                    this.Frame.Navigate(typeof(ComicBookViewer), item);
                }
                else
                {
                    this.Frame.Navigate(typeof(Home));
                }

            }
        }

        protected async override void GoBack(object sender, RoutedEventArgs e)
        {
            this.IsHitTestVisible = false;
            this.pageHeader.PageHeaderOut().Play();
            await this.importMessage.MessageOut().PlayAsync();

            this.IsHitTestVisible = true;
            this.Frame.Navigate(typeof(Home));
        }

        private async void appbarImportFolder_Click(object sender, RoutedEventArgs e)
        {
            this.IsHitTestVisible = false;
            await this.importMessage.MessageOut().PlayAsync();

            var folder = await this.vm.PickFolder();
            if (folder != null)
            {
                await this.loadingMessage.MessageIn().PlayAsync();
                await this.vm.ImportFolder(folder);
                await this.loadingMessage.MessageOut().PlayAsync();
                await this.doneMessage.MessageIn().PlayAsync();
                
                await this.doneMessage.MessageOut().PlayAsync();
                await this.pageHeader.PageHeaderOut().PlayAsync();

                ComicBookReader2.App.HomeVM.Invalidate();
                this.IsHitTestVisible = true;
                this.Frame.Navigate(typeof(Home));
            }
            else
            {
                await this.importMessage.MessageIn().PlayAsync();
                this.IsHitTestVisible = true;
            }
        }

        private async void appbarImportFile_Click(object sender, RoutedEventArgs e)
        {
            this.IsHitTestVisible = false;
            await this.importMessage.MessageOut().PlayAsync();

            var files = await this.vm.PickFile();
            if (files != null)
            {
                await this.loadingMessage.MessageIn().PlayAsync();
                await this.vm.ImportFile(files.ToList());
                await this.loadingMessage.MessageOut().PlayAsync();
                await this.doneMessage.MessageIn().PlayAsync();

                await this.doneMessage.MessageOut().PlayAsync();
                await this.pageHeader.PageHeaderOut().PlayAsync();

                ComicBookReader2.App.HomeVM.Invalidate();
                this.IsHitTestVisible = true;
                this.Frame.Navigate(typeof(Home));
            }
            else
            {
                await this.importMessage.MessageIn().PlayAsync();
                this.IsHitTestVisible = true;
            }
        }
    }
}
