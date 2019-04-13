using System;
using System.Reflection;
using MBW.Tools.RabbitDump.Movers;
using MBW.Tools.RabbitDump.Options;
using MBW.Tools.RabbitDump.Tool;
using MBW.Tools.RabbitDump.Utilities;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace MBW.Tools.RabbitDump
{
    static class Program
    {
        public static int Main(string[] args)
        {
            //Console.WriteLine(Process.GetCurrentProcess().Id);
            //while (!Debugger.IsAttached)
            //{
            //    Thread.Sleep(100);
            //}

            CommandLineApplication<ArgumentsModel> app = new CommandLineApplication<ArgumentsModel>();

            IHostBuilder hostBuilder = new HostBuilder()
                .ConfigureLogging((context, builder) =>
                {
                    builder.AddSerilog(dispose: true);
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton(app.Model);
                    services.AddSingleton<Dumper>();
                });

            app.FullName = "RabbitMQ Dumper";
            app.VersionOption("-v|--version", () => Assembly.GetEntryAssembly().GetName().Version.ToString());
            app.Conventions
                .UseAttributes()
                .SetRemainingArgsPropertyOnModel()
                .SetSubcommandPropertyOnModel()
                .SetParentPropertyOnModel()
                .UseOnValidateMethodFromModel()
                .UseOnValidationErrorMethodFromModel()
                .UseConstructorInjection()
                .UseDefaultHelpOption();

            app.OnExecute(() =>
            {
                LoggerConfiguration loggerConfiguration = new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .WriteTo.Console();

                if (app.Model.Verbose)
                    loggerConfiguration.MinimumLevel.Debug();

                Log.Logger = loggerConfiguration.CreateLogger();

                hostBuilder.ConfigureServices(services =>
                {
                    Type inputType = TargetUtilities.GetSourceType(app.Model.InputType);
                    Type outputType = TargetUtilities.GetDestinationType(app.Model.OutputType);

                    services
                        .AddSingleton<ISource>(inputType)
                        .AddSingleton<IDestination>(outputType);
                });

                using (IHost host = hostBuilder.Build())
                {
                    Dumper dumper = host.Services.GetRequiredService<Dumper>();
                    return (int)dumper.Run();
                }
            });

            try
            {
                return app.Execute(args);
            }
            catch (CommandParsingException e)
            {
                Console.Error.WriteLine($"There was an error processing the argumentsModel: {e.Message}");
                return (int)DumperExitCode.ParsingFailure;
            }
        }
    }
}
