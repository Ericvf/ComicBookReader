using ComicBookReader.App.Model;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace ComicBookReader.App.ViewModel
{
    public class ComicBookViewerVM : INotifyPropertyChanged
    {
        public ComicBookItem ComicBookItem { get; set; }

        public class ComicBookPage
        {
            public int Width { get; set; }
            public int Height { get; set; }
            public string Name { get; set; }
            public BitmapImage BitmapImage { get; set; }
            public bool IsRegular { get; set; }
            public ComicBookPage PrevPage { get; set; }
            public ComicBookPage NextPage { get; set; }
        }

        private ObservableCollection<ComicBookPage> pages = new ObservableCollection<ComicBookPage>();
        private ObservableCollection<ComicBookPage> pages2 = new ObservableCollection<ComicBookPage>();
        public ObservableCollection<ComicBookPage> Pages
        {
            get
            {
                return this.isLeftToRight ? this.pages : this.pages2;
            }
            set
            {
                this.pages = value;
                this.RaisePropertyChanged("Pages");
            }
        }

        private ComicBookPage selectedItem;
        public ComicBookPage SelectedItem
        {
            get
            {
                return this.selectedItem;
            }
            set
            {
                this.selectedItem = value;

                if (this.Pages.Contains(value))
                    this.CurrentPage = this.pages.IndexOf(value) + 1;

                this.RaisePropertyChanged("SelectedItem");
            }
        }

        private int currentPage;
        public int CurrentPage
        {
            get
            {
                return this.currentPage;
            }
            private set
            {
                this.currentPage = value;
                this.RaisePropertyChanged("CurrentPage");
            }
        }

        private bool isTwoPage = false;
        public bool IsTwoPage
        {
            get
            {
                return this.isTwoPage;
            }
            set
            {
                this.isTwoPage = value;
                ComicBookReader2.App.IsTwoPage = value;
                this.RaisePropertyChanged("IsTwoPage");
            }
        }

        private bool isLeftToRight = true;
        public bool IsLeftToRight
        {
            get
            {
                return this.isLeftToRight;
            }
            set
            {
                this.isLeftToRight = value;
                ComicBookReader2.App.IsLeftToRight = value;
                this.RaisePropertyChanged("IsLeftToRight");
            }
        }

        public ComicBookViewerVM(ComicBookItem comicBookItem)
        {
            this.ComicBookItem = comicBookItem;
        }

        internal async Task LoadPages()
        {
            this.IsLeftToRight = ComicBookReader2.App.IsLeftToRight;
            this.IsTwoPage = ComicBookReader2.App.IsTwoPage;

            this.pages.Clear();

            var files = await this.ComicBookItem.GetFiles();

            foreach (var file in files)
            {
                if (file.Name.EndsWith(@"jpg") || file.Name.EndsWith(@"png"))
                {
                    var fileStream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);

                    var source = new BitmapImage();
                    source.SetSource(fileStream);

                    var page = new ComicBookPage()
                    {
                        Width = source.PixelWidth,
                        Height = source.PixelHeight,
                        BitmapImage = source,
                        Name = file.Path,
                    };

                    page.IsRegular = page.Height > page.Width;
                    this.pages.Add(page);
                }
            }

            this.pages2 = new ObservableCollection<ComicBookPage>(this.pages.Reverse());

            FixTwoPages();

            int pageIndex = this.ComicBookItem.ComicBookInfo.XData.LastPage;
            this.SelectedItem = this.pages[pageIndex];

        }

        private void FixTwoPages()
        {
            for (int i = 0; i < this.Pages.Count; i++)
            {
                var page = this.Pages[i];
                if (i > 0) page.PrevPage = this.Pages[i - 1]; else page.PrevPage = null;
                if (i < this.Pages.Count - 1) page.NextPage = this.Pages[i + 1]; else page.NextPage = null;
            }
        }

        #region INotifyPropertyChanged

        internal void RaisePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        internal void ToggleLtR()
        {
            var selectedItem = this.SelectedItem;

            this.IsLeftToRight = !this.IsLeftToRight;
            this.FixTwoPages();

            this.RaisePropertyChanged("Pages");
            this.SelectedItem = selectedItem;
        }
    }
}
