using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ComicBookReader.App.Model
{
    [DataContract]
    public class ComicbookInfo
    {
        [DataContract]
        public class XComicBookReader
        {
            [DataMember(Name = "page")]
            public int LastPage { get; set; }
        }

        [DataContract]
        public class ComicBookContent
        {
            [DataMember(Name = "series", Order = 0, IsRequired = true)]
            public string Series { get; set; }

            [DataMember(Name = "title", Order = 1, IsRequired = true)]
            public string Title { get; set; }

            [DataMember(Name = "publisher", Order = 2, IsRequired = false, EmitDefaultValue = false)]
            public string Publisher { get; set; }

            [DataMember(Name = "publicationMonth", Order = 3, IsRequired = false, EmitDefaultValue = false)]
            public int PublicationMonth { get; set; }

            [DataMember(Name = "publicationYear", Order = 4, IsRequired = false, EmitDefaultValue = false)]
            public int PublicationYear { get; set; }

            [DataMember(Name = "issue", Order = 5, IsRequired = false, EmitDefaultValue = false)]
            public int Issue { get; set; }

            [DataMember(Name = "numberOfIssues", Order = 6, IsRequired = false, EmitDefaultValue = false)]
            public int NumberOfIssues { get; set; }

            [DataMember(Name = "volume", Order = 7, IsRequired = false, EmitDefaultValue = false)]
            public int Volume { get; set; }

            [DataMember(Name = "numberOfVolumes", Order = 8, IsRequired = false, EmitDefaultValue = false)]
            public int NumberOfVolumes { get; set; }

            [DataMember(Name = "rating", Order = 9, IsRequired = false, EmitDefaultValue = false)]
            public int Rating { get; set; }

            [DataMember(Name = "genre", Order = 10, IsRequired = false, EmitDefaultValue = false)]
            public string Genre { get; set; }

            [DataMember(Name = "language", Order = 11, IsRequired = false, EmitDefaultValue = false)]
            public string Language { get; set; }

            [DataMember(Name = "country", Order = 12, IsRequired = false, EmitDefaultValue = false)]
            public string Country { get; set; }
        }

        [DataMember(Name = "appID", Order = 0)]
        public string AppId { get; set; }

        [DataMember(Name = "lastModified", Order = 1)]
        public string LastModified { get; set; }

        [DataMember(Name = "ComicBookInfo/1.0", Order = 2)]
        public ComicBookContent Content { get; set; }

        [DataMember(Name = "x-ComicBookReader", Order = 13, IsRequired = false, EmitDefaultValue = false)]
        public XComicBookReader XData { get; set; }

        internal static ComicbookInfo CreateNew(string title, string series, string publisher,
            int publicationMonth, int publicationYear, int issue, int numberOfIssues, int volumes, int numberOfVolumes)
        {
            return new ComicbookInfo()
            {
                AppId = @"ComicBookReader.App/001",
                LastModified = DateTime.Now.ToString(),
                Content = new ComicBookContent()
                {
                    Publisher = publisher,
                    Series = series,
                    Title = title.Replace('_', ' '),
                    PublicationMonth = publicationMonth,
                    PublicationYear = publicationYear,
                    Issue = issue,
                    NumberOfIssues = numberOfIssues,
                    Volume = volumes,
                    NumberOfVolumes = numberOfVolumes,
                },
                XData = new XComicBookReader()
                {
                    LastPage = 0
                }
            };
        }
    }
}
