namespace MBW.Tools.RabbitDump.Movers
{
    interface IDestination
    {
        void WriteData(MessageItem item);
    }
}