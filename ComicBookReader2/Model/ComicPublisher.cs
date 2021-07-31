using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Media.Imaging;

namespace ComicBookReader.App.Model
{
    public class ComicPublisher : INotifyPropertyChanged
    {
        public string Title { get; set; }
        public int Items
        {
            get
            {
                if (this.Comics == null)
                    return 0;

                return this.Comics.Count;
            }
        }
        public ObservableCollection<ComicBookItem> Comics { get; set; }

        private BitmapSource thumbnail;
        public BitmapSource Thumbnail
        {
            get
            {
                return this.thumbnail;
            }
            set
            {
                this.thumbnail = value;
                this.RaisePropertyChanged("Thumbnail");
            }
        }

        private void RaisePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
