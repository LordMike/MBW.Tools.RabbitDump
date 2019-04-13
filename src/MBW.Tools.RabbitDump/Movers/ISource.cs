using System.Collections.Generic;

namespace MBW.Tools.RabbitDump.Movers
{
    interface ISource
    {
        IEnumerable<MessageItem> GetData();
    }
}