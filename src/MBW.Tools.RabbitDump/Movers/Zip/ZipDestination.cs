using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks.Dataflow;
using MBW.Tools.RabbitDump.Options;
using MBW.Tools.RabbitDump.Utilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MBW.Tools.RabbitDump.Movers.Zip
{
    class ZipDestination : ZipBase, IDestination,IDisposable
    {
        private readonly ILogger<ZipDestination> _logger;
        private readonly ZipArchive _zip;

        public ZipDestination(ArgumentsModel model, ILogger<ZipDestination> logger)
        {
            _logger = logger;
            _logger.LogDebug("Writing to zip file at {File}", model.Input);

            FileStream zipFs = File.Create(model.Output);
            _zip = new ZipArchive(zipFs, ZipArchiveMode.Create);
        }

        public  void Dispose()
        {
            _zip.Dispose();
        }

        public (ITargetBlock<MessageItem> writer, IDataflowBlock finalBlock) GetWriter(ISource acknowledgeSource)
        {
            // Prepare file name format
            string nameFormat = $"m-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}-";
            int idx = 0;

            ActionBlock<MessageItem> block = new ActionBlock<MessageItem>(item =>
            {
                string name = nameFormat + idx++;

                ZipArchiveEntry entry = _zip.CreateEntry(name + DataExtension, CompressionLevel.Optimal);
                using (Stream entryFs = entry.Open())
                    entryFs.Write(item.Data);

                entry = _zip.CreateEntry(name + MetaExtension, CompressionLevel.Optimal);
                using (Stream entryFs = entry.Open())
                    Serialization.Serialize(entryFs, item);

                acknowledgeSource.Acknowledge(new List<MessageItem> { item });
            });

            return (block, block);
        }
    }
}