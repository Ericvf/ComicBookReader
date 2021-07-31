using ComicBookReader.App.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Search;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;

namespace ComicBookReader.App.Model
{
    public class ComicBookItem : INotifyPropertyChanged
    {
        public ComicbookInfo ComicBookInfo { get; set; }
        public StorageFolder StorageFolder { get; set; }
        public string StorageFile { get; set; }

        private BitmapImage thumbnail;
        public BitmapImage Thumbnail
        {
            get
            {
                return this.thumbnail;
            }
            set
            {
                this.thumbnail = value;
                this.OnPropertyChanged("Thumbnail");
            }
        }

        private ObservableCollection<StorageItemThumbnail> thumbnails;
        public ObservableCollection<StorageItemThumbnail> Thumbnails
        {
            get
            {
                return this.thumbnails;
            }
            set
            {
                this.thumbnails = value;
                this.OnPropertyChanged("Thumbnails");
            }
        }

        #region Properties
        public string Title
        {
            get
            {
                return this.ComicBookInfo.Content.Title;
            }
            set
            {
                this.ComicBookInfo.Content.Title = value;


            }
        }

        public string Series
        {
            get
            {
                return this.ComicBookInfo.Content.Series;
            }
            set
            {
                this.ComicBookInfo.Content.Series = value;
            }
        }

        public string Publisher
        {
            get
            {
                return this.ComicBookInfo.Content.Publisher;
            }
            set
            {
                this.ComicBookInfo.Content.Publisher = value;
            }
        }

        public const string IssuePropertyName = "Issue";
        public int Issue
        {
            get
            {
                return this.ComicBookInfo.Content.Issue;
            }
            set
            {
                this.ComicBookInfo.Content.Issue = value;
                OnPropertyChanged(IssuePropertyName);


            }
        }

        public const string NumberOfIssuesPropertyName = "NumberOfIssues";
        public int NumberOfIssues
        {
            get
            {
                return this.ComicBookInfo.Content.NumberOfIssues;
            }
            set
            {
                this.ComicBookInfo.Content.NumberOfIssues = value;
                OnPropertyChanged(NumberOfIssuesPropertyName);
            }
        }

        public int NumberOfPages
        {
            get { return _NumberOfPages; }
            set
            {
                if (_NumberOfPages != value)
                {
                    _NumberOfPages = value;
                    OnPropertyChanged(NumberOfPagesPropertyName);
                }
            }
        }
        private int _NumberOfPages;
        public const string NumberOfPagesPropertyName = "NumberOfPages";

        public ulong FileSize
        {
            get {
                return _FileSize; 
            
            }
            set
            {
                if (_FileSize != value)
                {
                    _FileSize = value;
                    OnPropertyChanged(FileSizePropertyName);
                }
            }
        }
        private ulong _FileSize;
        public const string FileSizePropertyName = "FileSize";
        #endregion

        async internal Task<List<StorageFile>> GetFiles()
        {
            var files = await this.StorageFolder.GetFilesAsync();
            this.NumberOfPages = files.Count - 1;
            return files.ToList();
        }

        #region StorageHelpers
        async internal static Task<ComicBookItem> LoadFromStorageFile(StorageFile file)
        {
            var content = await file.ReadAllTextAsync();
            var comicBookInfo = Serialization.DeserializeJson<ComicbookInfo>(content);

            if (comicBookInfo.XData == null)
                comicBookInfo.XData = new ComicbookInfo.XComicBookReader();

            var item = new ComicBookItem()
            {
                ComicBookInfo = comicBookInfo,
                StorageFile = file.Name,
            };

            var dir = file.Path.Substring(0, file.Path.LastIndexOf("\\"));
            item.StorageFolder = await StorageFolder.GetFolderFromPathAsync(dir);
            await item.LoadThumbnail();

            return item;
        }

        async internal static Task SaveToStorageFile(StorageFolder folder, string file, ComicbookInfo comicBookInfo)
        {
            comicBookInfo.AppId = @"ComicBookReader.App/001";

            try
            {
                var cbiFile = await folder.CreateFileAsync(file, CreationCollisionOption.ReplaceExisting);
                var cbiData = Serialization.SerializeJson<ComicbookInfo>(comicBookInfo);

                await cbiFile.WriteAllTextAsync(cbiData);
            }
            catch (FileNotFoundException)
            {

            }
        }
        #endregion

        async internal Task GetFolderSize()
        {
            var files = await this.GetFiles();
            ulong totalSize = 0;

            foreach (var file in files)
            {
                var props = await file.GetBasicPropertiesAsync();
                totalSize += props.Size;
            }

            this.FileSize = totalSize;
        }

        async internal Task Save()
        {

                await SaveToStorageFile(this.StorageFolder, this.StorageFile, this.ComicBookInfo);
        }

        async internal Task LoadThumbnails()
        {
            this.Thumbnails = new ObservableCollection<StorageItemThumbnail>();
            var files = await this.GetFiles();

            foreach (var file in files)
            {
                var ext = file.FileType.ToLower();
                if (ext == @".jpg" || ext == @".png")
                {
                    try
                    {
                        // Find and add thumb
                        var thumb = await file.GetThumbnailAsync(ThumbnailMode.SingleItem);
                        thumbnails.Add(thumb);
                    }
                    catch
                    {
                    }
                }
            }
        }

        async internal Task LoadThumbnail()
        {
            if (this.Thumbnail == null)
            {
                var queryOptions = new QueryOptions(CommonFileQuery.DefaultQuery, new[] { @".jpg", @".png" });
                var storageQueryResult = this.StorageFolder.CreateFileQueryWithOptions(queryOptions);
                var files = await storageQueryResult.GetFilesAsync(0,1);
                if (files.Count > 0)
                {
                    var firstFile = files.FirstOrDefault();
                    // Find and add thumb
                    var thumb = await firstFile.GetThumbnailAsync(ThumbnailMode.SingleItem);
                    var bs = new BitmapImage();
                    bs.SetSource(thumb);

                    this.Thumbnail = bs;
                }
            }
        }

        async internal Task DeleteThumbnail(int index)
        {
            var dialog = new MessageDialog(ComicBookReader2.App.ResourceLoader.GetString("ComicDetailsDeletePage"), ComicBookReader2.App.ResourceLoader.GetString("GeneralDelete"));

            dialog.Commands.Add(new UICommand(ComicBookReader2.App.ResourceLoader.GetString("GeneralDontDelete"), (command) =>
            {
            }));

            dialog.Commands.Add(new UICommand(ComicBookReader2.App.ResourceLoader.GetString("GeneralGoAhead"), async (command) =>
            {
                this.Thumbnails.RemoveAt(index);
                var files = await this.GetFiles();
                await files[index].DeleteAsync();
                await this.LoadThumbnail();
                await this.GetFolderSize();

            }));

            await dialog.ShowAsync();
        }

        async internal Task DeleteComic()
        {
            ComicBookReader2.App.HomeVM.DeleteComic(this);

            try
            {
                var files = await this.StorageFolder.GetFilesAsync();
                foreach (var file in files)
                    await file.DeleteAsync();

                await this.StorageFolder.DeleteAsync();
            }
            catch
            {
            }
            //if (this.StorageFolder != null)
            //{
            //    var dialog = new MessageDialog(App.ResourceLoader.GetString("ComicDetailsDeleteComic"), App.ResourceLoader.GetString("GeneralDelete"));

            //    dialog.Commands.Add(new UICommand(App.ResourceLoader.GetString("GeneralDontDelete"), (command) =>
            //    {
            //    }));

            //    dialog.Commands.Add(new UICommand(App.ResourceLoader.GetString("GeneralGoAhead"), async (command) =>
            //    {
            //        App.HomeVM.Invalidate();


            //        //var files = await this.GetFiles();
            //        //foreach (var file in files)
            //        //{
            //        //    await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
            //        //}

            //        //try
            //        //{
            //        //    await this.StorageFolder.DeleteAsync(StorageDeleteOption.PermanentDelete);
            //        //}
            //        //catch
            //        //{
            //        //}
            //    }));

            //    dialog.DefaultCommandIndex = 1;
                
            //    await dialog.ShowAsync();
            //}
        }

        #region INotifyPropertyChanged

        internal void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
