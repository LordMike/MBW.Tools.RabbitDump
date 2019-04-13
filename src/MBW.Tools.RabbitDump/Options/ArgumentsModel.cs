using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MBW.Tools.RabbitDump.Movers;
using McMaster.Extensions.CommandLineUtils;

namespace MBW.Tools.RabbitDump.Options
{
    class ArgumentsModel
    {
        private InputType? _inputType;
        private OutputType? _outputType;
        private string _exchange;

        [Required]
        [Option("-i|--input", Description = "Example: amqp://user:password@host:port/vhost")]
        public string Input { get; set; }

        [Option("--input-type", Description = "Overrides auto detection of input type")]
        public InputType InputType
        {
            get
            {
                if (!_inputType.HasValue)
                {
                    // Detect type
                    if (Input.StartsWith("amqp://", StringComparison.OrdinalIgnoreCase))
                        _inputType = InputType.Amqp;
                    else if (Input.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                        _inputType = InputType.Zip;
                    else
                        throw new Exception("Unknown source type");
                }

                return _inputType.Value;
            }
            set => _inputType = value;
        }

        [Required]
        [Option("-o|--output", Description = "Example: amqp://user:password@host:port/vhost")]
        public string Output { get; set; }

        [Option("--output-type", Description = "Overrides auto detection of output type")]
        public OutputType OutputType
        {
            get
            {
                if (!_outputType.HasValue)
                {
                    // Detect type
                    if (Output.StartsWith("amqp://", StringComparison.OrdinalIgnoreCase))
                        _outputType = OutputType.Amqp;
                    else if (Output.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                        _outputType = OutputType.Zip;
                    else
                        throw new Exception("Unknown destination type");
                }

                return _outputType.Value;
            }
            set => _outputType = value;
        }

        [Option("--batch-size", Description = "Batch size to use")]
        [Range(1, 10000)]
        public int BatchSize { get; set; } = 1000;

        [Option("-e|--exchange", Description = "Override the output exchange, use a dash (-) to signify the default exchange")]
        public string Exchange
        {
            get => _exchange;
            set
            {
                if (value == "-")
                    _exchange = "";
                else
                    _exchange = value;
            }
        }

        [Option("-r|--routingKey", Description = "Override the output routing key")]
        public string RoutingKey { get; set; }

        [Argument(0, Description = "When the input is an AMQP uri, this is the queues to dump")]
        public List<string> Arguments { get; set; }

        [Option("-l|--verbose-logging", Description = "Enable verbose output")]
        public bool Verbose { get; set; }
    }
}