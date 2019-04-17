using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace MBW.Tools.RabbitDump.Utilities
{
    static class Serialization
    {
        public static JsonSerializer Serializer { get; }

        public static Encoding Encoding { get; }

        static Serialization()
        {
            Serializer = JsonSerializer.Create(new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            Encoding = new UTF8Encoding(false);
        }

        public static void Serialize<T>(Stream target, T @object)
        {
            using (StreamWriter sw = new StreamWriter(target, Encoding))
            using (JsonTextWriter tw = new JsonTextWriter(sw))
                Serializer.Serialize(tw, @object);
        }

        public static T Deserialize<T>(Stream source)
        {
            using (StreamReader sr = new StreamReader(source, Encoding))
            using (JsonTextReader tr = new JsonTextReader(sr))
                return Serializer.Deserialize<T>(tr);
        }
    }
}