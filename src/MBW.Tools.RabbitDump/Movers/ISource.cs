using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks.Dataflow;

namespace MBW.Tools.RabbitDump.Movers
{
    interface ISource
    {
        void SendData(ITargetBlock<MessageItem> target, CancellationToken cancellationToken);

        void Acknowledge(ICollection<MessageItem> items);
    }
}