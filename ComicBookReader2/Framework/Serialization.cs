using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace ComicBookReader.App.Framework
{
    public static class Serialization
    {
        internal static string SerializeJson<T>(T input)
            where T : class
        {
            var serializer = new DataContractJsonSerializer(typeof(T));

            MemoryStream ms = new MemoryStream();
            serializer.WriteObject(ms, input);
            var buffer = ms.ToArray();

            string json = Encoding.UTF8.GetString(buffer, 0, buffer.Length);

            return json;
        }

        internal static T DeserializeJson<T>(string content)
            where T : class
        {
            var serializer = new DataContractJsonSerializer(typeof(T));
            var returnValue = default(T);

            using (MemoryStream stream = new MemoryStream(Encoding.Unicode.GetBytes(content)))
            {
                returnValue = serializer.ReadObject(stream) as T;
            }

            return returnValue;
        }
    }
}
