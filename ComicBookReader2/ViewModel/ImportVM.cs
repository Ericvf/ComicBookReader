using System.Runtime.InteropServices.WindowsRuntime;

using ComicBookReader.App.Framework;
using ComicBookReader.App.Model;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Search;
using Windows.Storage.Streams;
using SharpCompress.Archive;
using Windows.UI.Popups;
using Windows.Storage.FileProperties;
using Windows.Graphics.Imaging;
using Windows.Foundation;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.IO;
using SharpCompress.Archive.Zip;

namespace ComicBookReader.App.ViewModel
{
    public class ImportVM : VM
    {
        public double Progress
        {
            get { return _Progress; }
            set
            {
                if (_Progress != value)
                {
                    _Progress = value;
                    OnPropertyChanged(ProgressPropertyName);
                }
            }
        }
        private double _Progress;
        public const string ProgressPropertyName = "Progress";

        public int ItemIndex
        {
            get { return _ItemIndex; }
            set
            {
                if (_ItemIndex != value)
                {
                    _ItemIndex = value;
                    OnPropertyChanged(ItemIndexPropertyName);
                }
            }
        }
        private int _ItemIndex = 1;
        public const string ItemIndexPropertyName = "ItemIndex";

        public int ItemCount
        {
            get { return _ItemCount; }
            set
            {
                if (_ItemCount != value)
                {
                    _ItemCount = value;
                    OnPropertyChanged(ItemCountPropertyName);
                }
            }
        }
        private int _ItemCount = 0;
        public const string ItemCountPropertyName = "ItemCount";

        internal async Task<IReadOnlyList<StorageFile>> PickFile()
        {
            var openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.List;
            openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            openPicker.FileTypeFilter.Add(@".cbz");
            openPicker.FileTypeFilter.Add(@".cbr");
            openPicker.FileTypeFilter.Add(@".zip");
            openPicker.FileTypeFilter.Add(@".rar");

            return await openPicker.PickMultipleFilesAsync();
        }

        public async Task<StorageFolder> PickFolder()
        {
            var openPicker = new FolderPicker();
            openPicker.ViewMode = PickerViewMode.List;
            openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            openPicker.FileTypeFilter.Add(@".cbr");
            openPicker.FileTypeFilter.Add(@".cbz");
            openPicker.FileTypeFilter.Add(@".zip");
            openPicker.FileTypeFilter.Add(@".rar");

            return await openPicker.PickSingleFolderAsync();
        }

        public async Task ImportFile(List<StorageFile> files)
        {
            var comicsFolder = await ComicBookReader2.App.LocalStorageFolder
               .CreateFolderAsync(@"ComicBookReader", CreationCollisionOption.OpenIfExists);

            this.ItemIndex = 1;
            this.ItemCount = 1;

            if (files != null && files.Count > 0)
            {
                this.ItemCount = files.Count;

                for (int i = 0; i < files.Count; i++)
                {
                    var folder = await comicsFolder.CreateFolderAsync(Guid.NewGuid().ToString(), CreationCollisionOption.FailIfExists);
                    this.ItemIndex = i + 1;

                    ComicBookItem item = null;

                    try
                    {
                        if (files[i].FileType == @".cbz" || files[i].FileType == @".zip")
                        {
                            item = await this.ImportCbz(files[i], folder);
                        }
                        else if (files[i].FileType == @".cbr" || files[i].FileType == @".rar")
                        {
                            item = await this.ImportCbr(files[i], folder);
                        }

                        item.ComicBookInfo.Content.Issue = i + 1;
                        item.ComicBookInfo.Content.NumberOfIssues = files.Count;
                        await item.Save();

                        // items.Add(item);
                    }
                    catch (Exception ex)
                    {
                        var dialog = new MessageDialog(ex.Message, "Exception");
                        dialog.ShowAsync();
                    }
                }
            }


            ComicBookReader2.App.HomeVM.Invalidate();
            this.ItemCount = 0;
            this.ItemIndex = 0;


            //var folder = await comicsFolder.CreateFolderAsync(
            //    Guid.NewGuid().ToString(),
            //    CreationCollisionOption.FailIfExists);

            //ComicBookItem item = null;
            //try
            //{
            //    if (file.FileType.ToLower() == @".cbz")
            //    {
            //        item = await this.ImportCbz(file, folder);
            //    }
            //    else if (file.FileType.ToLower() == @".cbr")
            //    {
            //        item = await this.ImportCbr(file, folder);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    var dialog = new MessageDialog(ex.Message, "Exception");
            //    dialog.ShowAsync();
            //}

            //var pubPrompt = new InputPrompt();
            //pubPrompt.Title = @"Input publisher name";
            //pubPrompt.Message = @"Please use the textbox to provide a new publisher name:";
            //pubPrompt.Save += (s, ex) =>
            //{
            //    item.Publisher = pubPrompt.NewName;
            //};

            //pubPrompt.OpenPopup();

            //var seriesPrompt = new InputPrompt();
            //seriesPrompt.Title = @"Input series name";
            //seriesPrompt.Message = @"Please use the textbox to provide a new series name:";
            //seriesPrompt.Save += async (s, ex) =>
            //{
            //    item.Series = seriesPrompt.NewName;
            //};

            //seriesPrompt.OpenPopup();
        }

        public async Task ImportFolder(StorageFolder importFolder)
        {
            var comicsFolder = await ComicBookReader2.App.LocalStorageFolder.CreateFolderAsync(@"ComicBookReader", CreationCollisionOption.OpenIfExists);

            var queryOptions = new QueryOptions(CommonFileQuery.DefaultQuery, new[] { @".cbz", @".cbr", @".zip", @".rar" });
            var storageQueryResult = importFolder.CreateFileQueryWithOptions(queryOptions);
            var files = await storageQueryResult.GetFilesAsync();

            // var items = new List<ComicBookItem>();

            if (files != null && files.Count > 0)
            {
                this.ItemCount = files.Count;

                for (int i = 0; i < files.Count; i++)
                {
                    var folder = await comicsFolder.CreateFolderAsync(Guid.NewGuid().ToString(), CreationCollisionOption.FailIfExists);
                    this.ItemIndex = i + 1;

                    ComicBookItem item = null;

                    try
                    {
                        if (files[i].FileType == @".cbz" || files[i].FileType == @".zip")
                        {
                            item = await this.ImportCbz(files[i], folder);
                        }
                        else if (files[i].FileType == @".cbr" || files[i].FileType == @".zip")
                        {
                            item = await this.ImportCbr(files[i], folder);
                        }

                        item.ComicBookInfo.Content.Issue = i + 1;
                        item.ComicBookInfo.Content.NumberOfIssues = files.Count;
                        await item.Save();

                        // items.Add(item);
                    }
                    catch (Exception ex)
                    {
                        var dialog = new MessageDialog(ex.Message, "Exception");
                        dialog.ShowAsync();
                    }
                }
            }

            //var pubPrompt = new InputPrompt();
            //pubPrompt.Title = @"Input publisher name";
            //pubPrompt.Message = @"Please use the textbox to provide a new publisher name:";
            //pubPrompt.Save += (s, ex) =>
            //{
            //    foreach (var item in items)
            //        item.Publisher = pubPrompt.NewName;

            //    App.HomeVM.Invalidate();
            //};

            //pubPrompt.OpenPopup();

            //var seriesPrompt = new InputPrompt();
            //seriesPrompt.Title = @"Input series name";
            //seriesPrompt.Message = @"Please use the textbox to provide a new series name:";
            //seriesPrompt.Save += async (s, ex) =>
            //{
            //    foreach (var item in items)
            //    {
            //        item.Series = seriesPrompt.NewName;
            //        await item.Save();
            //        App.HomeVM.Invalidate();

            //    }
            //};

            //seriesPrompt.OpenPopup();

            ComicBookReader2.App.HomeVM.Invalidate();
            this.ItemCount = 0;
            this.ItemIndex = 0;
        }

        internal async Task<ComicBookItem> ImportCbz(StorageFile archiveFile, StorageFolder targetFolder)
        {
            this.Progress = 0;
            int d = 0;

            var x = await archiveFile.OpenReadAsync();
            var archive = ZipArchive.Open(x.AsStreamForRead());

            //var zipArchive = await archiveFile.ReadZipFile();
            //string comments = zipArchive.Comment;

            string comments = string.Empty;


            try
            {
                var t = archive.Entries.Count;
                for (int i = 0; i < t; i++)
                {
                    var entry = archive.Entries.ElementAt(i);

                    if (entry.Size == 0 || entry.FilePath.EndsWith(@"/"))
                        continue;

                    var index = entry.FilePath.LastIndexOf(@".");
                    var sindex = entry.FilePath.LastIndexOf(@"/") + 1;

                    if (index > 0)
                    {
                        var ext = entry.FilePath.Substring(index);
                        if (ext.ToLower() == @".jpg" || ext.ToLower() == @".png")
                        {
                            using (var fileStream = entry.OpenEntryStream())
                            {
                                var filename = string.Format("{0}_{1}{2}", entry.FilePath.Substring(sindex, index - sindex), i, ext.ToLower());
                                var fileName2 = string.Format("{0}_{1}", entry.FilePath.Substring(sindex, index - sindex), ext.ToLower());
                                var targetFile = await targetFolder.CreateFileAsync(filename);

                                var fileOutputStream = await targetFile.OpenAsync(FileAccessMode.ReadWrite);
                                await RandomAccessStream.CopyAndCloseAsync(fileStream.AsInputStream(), fileOutputStream);
                                d++;

                                await LoadTileImageInternalAsync(targetFile, targetFolder, fileName2);

                                this.Progress = (double)(i) / (double)(t);
                            }
                        }
                    }
                }
            }
            finally
            {
            }

            if (d == 0)
                return null;

            var comicBookItem = new ComicBookItem();
            comicBookItem.StorageFolder = targetFolder;
            var cbiFile = await targetFolder.CreateFileAsync(archiveFile.DisplayName + ".cbi", CreationCollisionOption.ReplaceExisting);

            comicBookItem.StorageFile = cbiFile.Name;

            if (!string.IsNullOrEmpty(comments))
            {
                var comicBookInfo = Serialization.DeserializeJson<ComicbookInfo>(comments);

                if (comicBookInfo.XData == null)
                    comicBookInfo.XData = new ComicbookInfo.XComicBookReader();

                comicBookItem.ComicBookInfo = comicBookInfo;
            }
            else
            {
                var folderName = Path.GetDirectoryName(archiveFile.Path);
                var folderIndexer = folderName.LastIndexOf("\\");
                if (folderIndexer > 0)
                    folderName = folderName.Substring(folderIndexer + 1);

                comicBookItem.ComicBookInfo = ComicbookInfo.CreateNew(archiveFile.DisplayName, folderName, "Unknown", 0, 0, 0, 0, 0, 0);
            }

            await comicBookItem.Save();
            await comicBookItem.LoadThumbnail();
            return comicBookItem;
        }


        internal async Task<ComicBookItem> ImportCbr(StorageFile archiveFile, StorageFolder targetFolder)
        {
            this.Progress = 0;

            var x = await archiveFile.OpenReadAsync();
            int i = 0;
            int d = 0;

            try
            {
                var archive = ArchiveFactory.Open(x.AsStreamForRead());
                int t = archive.Entries.Count();
                foreach (var entry in archive.Entries)
                {
                    i++;

                    if (!entry.IsDirectory)
                    {
                        //byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(entry.FilePath);
                        //byte[] asciiArray = System.Text.Encoding.Convert(System.Text.Encoding.UTF8, System.Text.Encoding.Unicode, byteArray);
                        //string filepath = System.Text.Encoding.Unicode.GetString(asciiArray, 0, asciiArray.Length);

                        var index = entry.FilePath.LastIndexOf(@".");
                        var sindex = entry.FilePath.LastIndexOf(@"/") + 1;
                        if (index > 0)
                        {
                            var ext = entry.FilePath.Substring(index);
                            if (ext.ToLower() == @".jpg" || ext.ToLower() == @".png")
                            {
                                var file = await targetFolder.CreateFileAsync(string.Format("{0}{1}", entry.FilePath.Substring(sindex, index - sindex), ext.ToLower()));
                                var fileName2 = string.Format("{0}_{1}", entry.FilePath.Substring(sindex, index - sindex), ext.ToLower());

                                using (IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                                {
                                    using (IOutputStream outputStream = fileStream.GetOutputStreamAt(0))
                                    {
                                        //entry.WriteTo(outputStream.AsStreamForWrite());
                                        using (MemoryStream ms = new MemoryStream())
                                        {
                                            entry.WriteTo(ms);
                                            await outputStream.WriteAsync(ms.GetWindowsRuntimeBuffer());
                                        }

                                        await outputStream.FlushAsync();
                                        d++;
                                    }
                                }

                                await LoadTileImageInternalAsync(file, targetFolder, fileName2);
                            }
                        }
                    }

                    this.Progress = (double)i / (double)(t);
                }
            }
            finally
            {

            }

            if (d == 0)
                return null;

            var cbiFile = await targetFolder.CreateFileAsync(archiveFile.DisplayName + @".cbi", CreationCollisionOption.ReplaceExisting);

            var folderName = Path.GetDirectoryName(archiveFile.Path);
            var folderIndexer = folderName.LastIndexOf(@"\");
            if (folderIndexer > 0)
                folderName = folderName.Substring(folderIndexer + 1);

            var comicBookItem = new ComicBookItem();
            comicBookItem.StorageFolder = targetFolder;
            comicBookItem.StorageFile = cbiFile.Name;
            comicBookItem.ComicBookInfo = ComicbookInfo.CreateNew(archiveFile.DisplayName, folderName, "Unknown", 0, 0, 0, 0, 0, 0);
            await comicBookItem.Save();

            await comicBookItem.LoadThumbnail();
            return comicBookItem;
        }

        public IReadOnlyList<IStorageItem> ActivatedFiles { get; set; }

        internal void PreloadFiles(IReadOnlyList<IStorageItem> readOnlyList)
        {
            this.ActivatedFiles = readOnlyList;
        }

        internal async Task<ComicBookItem> ImportActivatedFiles()
        {
            var comicsFolder = await ComicBookReader2.App.LocalStorageFolder.CreateFolderAsync(@"ComicBookReader", CreationCollisionOption.OpenIfExists);
            var files = new List<IStorageItem>(this.ActivatedFiles);
            this.ActivatedFiles = null;

            this.ItemCount = files.Count;
            var items = new List<ComicBookItem>();

            for (int i = 0; i < files.Count; i++)
            {
                var file = files[i] as StorageFile;
                if (file == null)
                    continue;

                var folder = await comicsFolder.CreateFolderAsync(Guid.NewGuid().ToString(), CreationCollisionOption.FailIfExists);
                this.ItemIndex = i + 1;

                ComicBookItem item = null;

                try
                {
                    if (file.FileType == @".cbz" || file.FileType == @".zip")
                    {
                        item = await this.ImportCbz(file, folder);
                    }
                    else if (file.FileType == @".cbr" || file.FileType == @".rar")
                    {
                        item = await this.ImportCbr(file, folder);
                    }

                    if (item != null)
                    {
                        item.ComicBookInfo.Content.Issue = i + 1;
                        item.ComicBookInfo.Content.NumberOfIssues = files.Count;
                        await item.Save();

                        items.Add(item);
                    }
                }
                catch (Exception ex)
                {
                    var dialog = new MessageDialog(ex.Message, "Exception");
                    dialog.ShowAsync();
                }
            }

            return items.FirstOrDefault();
        }



        internal static async Task LoadTileImageInternalAsync(StorageFile origFile, StorageFolder storageFolder, string newFileName)
        {
            ImageProperties properties = await origFile.Properties.GetImagePropertiesAsync();
            uint width = properties.Width;
            uint height = properties.Height;

            double mw = 1200d / (double)width;
            double mh = 1600d / (double)height;
            var r = mw < mh ? mw : mh;

            if (r >= 1)
                return;

            string imagePath = origFile.Name;
            //StorageFile origFile = await ApplicationData.Current.LocalFolder.GetFileAsync(imagePath);
            var filename = System.IO.Path.GetFileNameWithoutExtension(imagePath);
            var ext = System.IO.Path.GetExtension(imagePath);
            var tileName = filename + "_" + ext;

            // open file for the new tile image file
            StorageFile tileFile = await storageFolder.CreateFileAsync(newFileName, CreationCollisionOption.ReplaceExisting);
            using (IRandomAccessStream tileStream = await tileFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                // get width and height from the original image
                IRandomAccessStreamWithContentType stream = await origFile.OpenReadAsync();


                // get proper decoder for the input file - jpg/png/gif
                BitmapDecoder decoder = await GetProperDecoder(stream, imagePath);
                if (decoder == null) return; // should not happen
                // get byte array of actual decoded image
                PixelDataProvider data = await decoder.GetPixelDataAsync();
                byte[] bytes = data.DetachPixelData();

                // create encoder for saving the tile image
                BitmapPropertySet propertySet = new BitmapPropertySet();
                //// create class representing target jpeg quality - a bit obscure, but it works
                BitmapTypedValue qualityValue = new BitmapTypedValue(1, PropertyType.Single);
                propertySet.Add("ImageQuality", qualityValue);
                // create the target jpeg decoder
                BitmapEncoder be = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, tileStream, propertySet);


                be.BitmapTransform.ScaledWidth = (uint)(width * r);
                be.BitmapTransform.ScaledHeight = (uint)(height * r);
                be.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight, width, height, 96.0, 96.0, bytes);

                //// crop the image, if it's too big
                //if (width > MaxImageWidth || height > MaxImageHeight)
                //{
                //    BitmapBounds bounds = new BitmapBounds();
                //    if (width > MaxImageWidth)
                //    {
                //        bounds.Width = MaxImageWidth;
                //        bounds.X = (width - MaxImageWidth) / 2;
                //    }
                //    else bounds.Width = width;
                //    if (height > MaxImageHeight)
                //    {
                //        bounds.Height = MaxImageHeight;
                //        bounds.Y = (height - MaxImageHeight) / 2;
                //    }
                //    else bounds.Height = height;
                //    be.BitmapTransform.Bounds = bounds;
                //}

                // save the target jpg to the file
                await be.FlushAsync();
            }

            await origFile.DeleteAsync();
        }

        private static async Task<BitmapDecoder> GetProperDecoder(IRandomAccessStreamWithContentType stream, string imagePath)
        {
            string ext = System.IO.Path.GetExtension(imagePath);
            switch (ext)
            {
                case ".jpg":
                case ".jpeg":
                    return await BitmapDecoder.CreateAsync(BitmapDecoder.JpegDecoderId, stream);
                case ".png":
                    return await BitmapDecoder.CreateAsync(BitmapDecoder.PngDecoderId, stream);
                case ".gif":
                    return await BitmapDecoder.CreateAsync(BitmapDecoder.GifDecoderId, stream);
            }
            return null;
        }


    }
}
