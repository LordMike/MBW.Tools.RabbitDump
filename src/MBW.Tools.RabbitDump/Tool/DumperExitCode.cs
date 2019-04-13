namespace MBW.Tools.RabbitDump.Tool
{
    enum DumperExitCode
    {
        Ok = 0,
        ParsingFailure = 1,
        ProcessError = 90,
        SourceError = 91,
        DestinationError = 92,
        GenericError = 100,
    }
}