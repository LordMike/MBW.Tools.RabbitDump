# MBW.Tools.RabbitDump [![Generic Build](https://github.com/LordMike/MBW.Tools.RabbitDump/actions/workflows/dotnet.yml/badge.svg)](https://github.com/LordMike/MBW.Tools.RabbitDump/actions/workflows/dotnet.yml) [![NuGet](https://img.shields.io/nuget/v/MBW.Tools.RabbitDump.svg)](https://www.nuget.org/packages/MBW.Tools.RabbitDump) [![GHPackages](https://img.shields.io/badge/package-alpha-green)](https://github.com/LordMike/MBW.Tools.RabbitDump/packages/??)

A `dotnet tool` to import / export from RabbitMQ queues

### Features

* Define source and destinations and move messages between them
* Fast exports and imports (using batches and push-based subscriptions)
* Runnable and installable as a tool using `dotnet tool`
* Supports one-shot and continuous transfers of messages

### Usage

To install, run: `dotnet tool install --global MBW.Tools.RabbitDump`. See the help using `rabbitdump --help`.

**To export:**
```
rabbitdump --input amqp://myuser:mypass@server:port --output data.zip queue1 queue2
```

**To import:**
```
rabbitdump --input data.zip --output amqp://myuser:mypass@server:port
```

#### Sources

RabbitDump supports these sources

| Name | Type (for overriding) | Example | Notes |
|----|----|----|----|
| **Zip**  | `zip` | `--input myfile.zip` | Zips can only be a one-shot source |
| **AMQP** | `amqp` | `amqp` | `--input amqp://user:pass@localhost:port/vhost`<br/>`--input amqp://user:pass@localhost:port/` | |

#### destinations

RabbitDump supports these destinations

| Name | Type (for overriding) | Example | Notes |
|----|----|----|----|
| **Zip**  | `zip` | `--output myfile.zip` | The zip must not exist before starting |
| **AMQP** | `amqp` | `--output amqp://user:pass@localhost:port/vhost`<br/>`--output amqp://user:pass@localhost:port/` | Messages are mandatory, the receiver must be able to route them |

### Todo

* Replay messages with same delays as when they were first created
* RabbitMQ API support to list queues, support wildcards in queue names
* Add AWS SQS queues
* Option to append to zip outputs
