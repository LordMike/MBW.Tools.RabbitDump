using System;
using System.Text;
using Newtonsoft.Json;

namespace MBW.Tools.RabbitDump.Movers.Zip
{
    abstract class ZipBase : IDisposable
    {
        protected const string DataExtension = ".data";
        protected const string MetaExtension = ".meta";

        protected readonly UTF8Encoding Encoding = new UTF8Encoding(false);
        protected readonly JsonSerializer Serializer = JsonSerializer.Create();

        public abstract void Dispose();
    }
}