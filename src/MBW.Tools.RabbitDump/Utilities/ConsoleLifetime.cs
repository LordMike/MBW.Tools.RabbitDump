using System.Threading;

namespace MBW.Tools.RabbitDump.Utilities
{
    class ConsoleLifetime
    {
        public CancellationToken CancellationToken { get; }

        public ConsoleLifetime(CancellationToken cancellationToken)
        {
            CancellationToken = cancellationToken;
        }
    }
}