//using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace ComicBookReader.App.Framework
{
    public static class StorageFileExtensions
    {
        /// <summary> 
        /// Asynchronously write a string to a file 
        /// </summary> 
        /// <param name="storageFile">StorageFile to write text to</param> 
        /// <param name="content">Text to write</param> 
        /// <returns>Task/ void if used with await</returns> 
        async public static Task WriteAllTextAsync(this StorageFile storageFile, string content)
        {
            using (var inputStream = await storageFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                using (var writeStream = inputStream.GetOutputStreamAt(0))
                {
                    using (DataWriter writer = new DataWriter(writeStream))
                    {
                        writer.WriteString(content);
                        await writer.StoreAsync();
                        await writeStream.FlushAsync();
                    }
                }
            }
        }

        /// <summary> 
        /// Asynchronously read a string from a file 
        /// </summary> 
        /// <param name="storageFile">StorageFile to read text from</param> 
        /// <returns>Task/ void if used with await</returns> 
        async public static Task<string> ReadAllTextAsync(this StorageFile storageFile)
        {
            var returnValue = default(string);
            using (var inputStream = await storageFile.OpenAsync(FileAccessMode.Read))
            {
                using (var readStream = inputStream.GetInputStreamAt(0))
                {
                    using (var reader = new DataReader(readStream))
                    {
                        uint fileLength = await reader.LoadAsync((uint)inputStream.Size);
                        returnValue = reader.ReadString(fileLength);
                    }
                }
            }

            return returnValue;
        }


        public static async Task<byte[]> ReadAllBytes(this StorageFile storageFile)
        {
            using (var istream = await storageFile.OpenReadAsync())
            {
                using (var reader = new DataReader(istream))
                {
                    var size = (uint)istream.Size;
                    var bytes = new byte[size];

                    await reader.LoadAsync(size);
                    reader.ReadBytes(bytes);
                    reader.DetachStream();
                    return bytes;
                }
            }
        }

        public static async Task WriteAllBytes(this StorageFile storageFile, byte[] data)
        {
            using (var ostream = await storageFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                using (var writer = new DataWriter(ostream))
                {
                    writer.WriteBytes(data);
                    await writer.StoreAsync();

                    writer.DetachStream();
                    await ostream.FlushAsync();
                }
            }
        }

        //public static async Task<ZipFile> ReadZipFile(this StorageFile storageFile)
        //{
        //    var bytes = await storageFile.ReadAllBytes();
        //    using (var memoryStream = new MemoryStream())
        //    {
        //        memoryStream.Write(bytes, 0, bytes.Length);
        //        memoryStream.Position = 0;

        //        return ZipFile.Read(memoryStream);
        //    }
        //}
    }
}
