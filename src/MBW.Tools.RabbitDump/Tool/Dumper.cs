using System;
using System.Collections.Generic;
using MBW.Tools.RabbitDump.Movers;
using MBW.Tools.RabbitDump.Utilities;
using Microsoft.Extensions.Logging;

namespace MBW.Tools.RabbitDump.Tool
{
    class Dumper
    {
        private readonly ISource _source;
        private readonly IDestination _destination;
        private readonly ILogger<Dumper> _logger;

        public Dumper(ISource source, IDestination destination, ILogger<Dumper> logger)
        {
            _source = source;
            _destination = destination;
            _logger = logger;
        }

        public DumperExitCode Run()
        {
            int count = 0;

            _logger.LogDebug("Begin moving data, with {Source} => {Destination}", _source, _destination);
            try
            {
                using (IEnumerator<MessageItem> enumerator = _source.GetData().GetEnumerator())
                {
                    while (true)
                    {
                        try
                        {
                            bool moved = enumerator.MoveNext();
                            if (!moved)
                                break;
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e, "There was an error in the source");
                            return DumperExitCode.SourceError;
                        }

                        try
                        {
                            _destination.WriteData(enumerator.Current);
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e, "There was an error in the destination");
                            return DumperExitCode.DestinationError;
                        }

                        count++;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "There was a generic error while copying data");
                return DumperExitCode.GenericError;
            }
            finally
            {
                (_source as IDisposable).TryDispose();
                (_destination as IDisposable).TryDispose();
            }

            _logger.LogInformation("Copied {Count} messages from source to destination", count);

            return DumperExitCode.Ok;
        }
    }
}