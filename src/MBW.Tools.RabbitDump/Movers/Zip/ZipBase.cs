using System;
using System.Text;
using Newtonsoft.Json;

namespace MBW.Tools.RabbitDump.Movers.Zip
{
    abstract class ZipBase : IDisposable
    {
        protected const string DataExtension = ".data";
        protected const string MetaExtension = ".meta";

        public abstract void Dispose();
    }
}