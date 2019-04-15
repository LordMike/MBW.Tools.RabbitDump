using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks.Dataflow;
using MBW.Tools.RabbitDump.Options;
using Newtonsoft.Json;

namespace MBW.Tools.RabbitDump.Movers.Zip
{
    class ZipDestination : ZipBase, IDestination
    {
        private readonly ZipArchive _zip;

        public ZipDestination(ArgumentsModel model)
        {
            FileStream zipFs = File.Create(model.Output);
            _zip = new ZipArchive(zipFs, ZipArchiveMode.Create);
        }

        public override void Dispose()
        {
            _zip.Dispose();
        }

        public (ITargetBlock<MessageItem> writer, IDataflowBlock finalBlock) GetWriter(ISource acknowledgeSource)
        {
            ActionBlock<MessageItem> block = new ActionBlock<MessageItem>(item =>
            {
                Guid id = Guid.NewGuid();

                ZipArchiveEntry entry = _zip.CreateEntry(id + DataExtension, CompressionLevel.Optimal);
                using (Stream entryFs = entry.Open())
                    entryFs.Write(item.Data);

                entry = _zip.CreateEntry(id + MetaExtension, CompressionLevel.Optimal);
                using (Stream entryFs = entry.Open())
                {
                    using (StreamWriter sw = new StreamWriter(entryFs, Encoding))
                    using (JsonTextWriter tw = new JsonTextWriter(sw))
                        Serializer.Serialize(tw, item);
                }

                acknowledgeSource.Acknowledge(new List<MessageItem> { item });
            });

            return (block, block);
        }
    }
}