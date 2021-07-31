using ComicBookReader.App.Model;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Search;
using System.Collections.Specialized;
using System.Diagnostics;

namespace ComicBookReader.App.ViewModel
{
    public class CollectionGroup : INotifyPropertyChanged
    {
        /// <summary>
        /// Group Title
        /// </summary>
        public string Title { get; set; }
        public int Index { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="title"></param>
        public CollectionGroup(string title, int index)
        {
            this.Title = title;
            this.Index = index;
            this.View = new ObservableCollection<object>();
        }

        /// <summary>
        /// The View for the Group
        /// </summary>
        private ObservableCollection<object> view = new ObservableCollection<object>();
        public ObservableCollection<object> View
        {
            get
            {
                return this.view;
            }
            set
            {
                this.view = value;
                this.OnPropertyChanged("View");
            }
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

    public class VM : INotifyPropertyChanged
    {
        #region Notification Properties

        public bool IsLoading
        {
            get { return _IsLoading; }
            set
            {
                if (_IsLoading != value)
                {
                    _IsLoading = value;
                    OnPropertyChanged(IsLoadingPropertyName);
                }
            }
        }
        private bool _IsLoading;
        public const string IsLoadingPropertyName = "IsLoading";

        #endregion

        #region INotifyPropertyChanged

        internal void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }

    public class HomeVM : VM
    {
        private ObservableCollection<CollectionGroup> collectionGroups = new ObservableCollection<CollectionGroup>();
        public ObservableCollection<CollectionGroup> CollectionGroups
        {
            get
            {
                return this.collectionGroups;
            }
            set
            {
                this.collectionGroups = value;
                this.OnPropertyChanged("CollectionGroups");
            }
        }

        CollectionGroup comicsGroup = new CollectionGroup(ComicBookReader2.App.ResourceLoader.GetString("GroupComics"), 0);
        CollectionGroup seriesGroup = new CollectionGroup(ComicBookReader2.App.ResourceLoader.GetString("GroupSeries"), 1);
        CollectionGroup publishersGroup = new CollectionGroup(ComicBookReader2.App.ResourceLoader.GetString("GroupPublishers"), 2);

        private ObservableCollection<ComicBookItem> allComics = new ObservableCollection<ComicBookItem>();
        public ObservableCollection<ComicBookItem> AllComics
        {
            get
            {
                return allComics;
            }
        }

        private ObservableCollection<ComicSeries> allSeries = new ObservableCollection<ComicSeries>();
        public ObservableCollection<ComicSeries> AllSeries
        {
            get
            {
                return allSeries;
            }
        }

        private ObservableCollection<ComicPublisher> allPublishers = new ObservableCollection<ComicPublisher>();
        public ObservableCollection<ComicPublisher> AllPublishers
        {
            get
            {
                return allPublishers;
            }
        }

        public HomeVM()
        {
            this.CollectionGroups.Add(comicsGroup);
            this.CollectionGroups.Add(seriesGroup);
            this.CollectionGroups.Add(publishersGroup);
        }

        internal async Task LoadComics()
        {
            if (this.allComics.Count > 0)
                return;

            var comicsBookItems = new List<ComicBookItem>();
            var comicsFolder = await ComicBookReader2.App.LocalStorageFolder.CreateFolderAsync("ComicBookReader", CreationCollisionOption.OpenIfExists);

            if (comicsFolder != null)
            {
                // Create file query for CBI files
                var queryOptions = new QueryOptions(CommonFileQuery.DefaultQuery, new[] { ".cbi" });
                queryOptions.FolderDepth = FolderDepth.Deep;

                var storageQueryResult = comicsFolder.CreateFileQueryWithOptions(queryOptions);
                var files = await storageQueryResult.GetFilesAsync();
                if (files != null && files.Count > 0)
                {
                    foreach (var file in files)
                    {
                        try
                        {
                            var comicBookItem = await ComicBookItem.LoadFromStorageFile(file);
                            this.allComics.Add(comicBookItem);
                        }
                        catch
                        {
                            Debug.WriteLine("Unable to load StorageFile: {0}", file.Path);
                        }
                    }
                }
            }

        }

        public void RefreshGroups()
        {
            this.comicsGroup.View.Clear();
            this.seriesGroup.View.Clear();
            this.publishersGroup.View.Clear();

            var topComics = this.allComics.OrderBy(c => c.ComicBookInfo.LastModified).Take(4);
            foreach (var t in topComics)
                this.comicsGroup.View.Add(t);


            var topSeries = this.allSeries.OrderBy(s => s.Title).Take(4);
            foreach (var t in topSeries)
                this.seriesGroup.View.Add(t);


            var topPublishers = this.allPublishers.OrderBy(s => s.Title).Take(4);
            foreach (var t in topPublishers)
                this.publishersGroup.View.Add(t);
        }

        internal async Task LoadSeries()
        {
            if (this.allSeries.Count > 0)
                return;

            var series = from c in this.allComics
                         group c by c.Series into g
                         select new
                         {
                             g.Key,
                             Items = g.ToList()
                         };

            foreach (var s in series)
            {
                var item = s.Items.FirstOrDefault();
                await item.LoadThumbnail();

                var cs = new ComicSeries()
                {
                    Title = s.Key,
                    Comics = new ObservableCollection<ComicBookItem>(s.Items),
                    Thumbnail = item.Thumbnail
                };

                this.allSeries.Add(cs);
            }


        }

        internal async Task LoadPublishers()
        {
            if (this.allPublishers.Count > 0)
                return;

            var publishers = from c in this.allComics
                             group c by c.Publisher into g
                             select new
                             {
                                 g.Key,
                                 Items = g.ToList()
                             };

            foreach (var s in publishers)
            {
                var item = s.Items.FirstOrDefault();
                await item.LoadThumbnail();

                var cs = new ComicPublisher()
                {
                    Title = s.Key,
                    Comics = new ObservableCollection<ComicBookItem>(s.Items),
                    Thumbnail = item.Thumbnail
                };

                this.allPublishers.Add(cs);
            }
        }

        internal void Invalidate()
        {
            this.allComics.Clear();
            this.allSeries.Clear();
            this.allPublishers.Clear();
        }

        internal void DeleteComic(ComicBookItem comicBookItem)
        {
            if (this.allComics.Contains(comicBookItem))
                this.allComics.Remove(comicBookItem);

            var seriesToRemove = new List<ComicSeries>();
            foreach (var serie in this.allSeries)
            {
                if (serie.Comics.Contains(comicBookItem))
                    serie.Comics.Remove(comicBookItem);

                if (serie.Comics.Count == 0)
                    seriesToRemove.Add(serie);
            }

            if (seriesToRemove.Count > 0)
            {
                foreach (var serie in seriesToRemove)
                    this.allSeries.Remove(serie);
            }

            var publishersToRemove = new List<ComicPublisher>();
            foreach (var publisher in this.allPublishers)
            {
                if (publisher.Comics.Contains(comicBookItem))
                    publisher.Comics.Remove(comicBookItem);

                if (publisher.Comics.Count == 0)
                    publishersToRemove.Add(publisher);
            }

            if (publishersToRemove.Count > 0)
            {
                foreach (var publisher in publishersToRemove)
                    this.allPublishers.Remove(publisher);
            }
        }
    }
}
