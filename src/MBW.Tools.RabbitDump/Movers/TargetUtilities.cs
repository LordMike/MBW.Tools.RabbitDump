using System;
using MBW.Tools.RabbitDump.Movers.Amqp;
using MBW.Tools.RabbitDump.Movers.Zip;

namespace MBW.Tools.RabbitDump.Movers
{
    static class TargetUtilities
    {
        public static Type GetSourceType(InputType inputType)
        {
            switch (inputType)
            {
                case InputType.Amqp:
                    return typeof(AmqpSource);
                case InputType.Zip:
                    return typeof(ZipSource);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static Type GetDestinationType(OutputType outputType)
        {
            switch (outputType)
            {
                case OutputType.Amqp:
                    return typeof(AmqpDestination);
                case OutputType.Zip:
                    return typeof(ZipDestination);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}