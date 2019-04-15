using System.Threading.Tasks.Dataflow;

namespace MBW.Tools.RabbitDump.Movers
{
    interface IDestination
    {
        (ITargetBlock<MessageItem> writer, IDataflowBlock finalBlock) GetWriter(ISource acknowledgeSource);
    }
}