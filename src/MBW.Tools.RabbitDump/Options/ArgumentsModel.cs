using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
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

        internal TransferMode TransferMode { get; set; } = TransferMode.OneShot;

        [Option("-c|--continuous", Description = "Continue subscribing to events from the Input")]
        public bool UseContinuous
        {
            get => TransferMode == TransferMode.Continuous;
            set => TransferMode = TransferMode.Continuous;
        }

        [Option("-s|--one-shot", Description = "Only run the data move once - we'll make best efforts to stop in a timely manner for AMQP inputs")]
        public bool UseOneShot
        {
            get => TransferMode == TransferMode.OneShot;
            set => TransferMode = TransferMode.OneShot;
        }

        [Option("--count", Description = "Limit the transfer to a number of items")]
        [Range(1, int.MaxValue)]
        public int? MessageLimit { get; set; }

        // ReSharper disable once UnusedMember.Global
        public ValidationResult OnValidate()
        {
            // Input/output types
            switch (InputType)
            {
                case InputType.Amqp:
                    if (!IsValidAmqp(Input))
                        return new ValidationResult("Invalid AMQP uri in input");
                    break;
                case InputType.Zip:
                    if (!File.Exists(Input))
                        return new ValidationResult("The input zip file does not exist");
                    if (!UseOneShot)
                        return new ValidationResult("Zip inputs can only be one-shots");
                    break;
                default:
                    return new ValidationResult("Unknown input type");
            }

            switch (OutputType)
            {
                case OutputType.Amqp:
                    if (!IsValidAmqp(Output))
                        return new ValidationResult("Invalid AMQP uri in output");
                    break;
                case OutputType.Zip:
                    if (File.Exists(Output))
                        return new ValidationResult("The output zip file already exists");
                    break;
                default:
                    return new ValidationResult("Unknown output type");
            }

            // Input AMQP must have queues
            if (InputType == InputType.Amqp && !Arguments.Any())
                return new ValidationResult("Missing queues for input");

            return ValidationResult.Success;
        }

        private bool IsValidAmqp(string amqp)
        {
            if (!Uri.TryCreate(amqp, UriKind.Absolute, out var uri))
                return false;

            return string.Equals("amqp", uri.Scheme, StringComparison.OrdinalIgnoreCase);
        }
    }
}