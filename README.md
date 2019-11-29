# NsqClient [![NuGet](https://img.shields.io/nuget/v/NsqClient?color=success)](https://www.nuget.org/packages/NsqClient) [![License](https://img.shields.io/github/license/matteocontrini/NsqClient?color=success)](https://github.com/matteocontrini/NsqClient/blob/master/LICENSE)

A basic but reliable **.NET Standard 2.0** library for publishing and subscribing to [NSQ](https://nsq.io/).

It's currently used in production to handle tens of thousands of messages per week.

## Features:

- Support for NSQ 1.2.0
- Publishing of `string` or `byte[]` messages to topics
- Subscribing with callbacks (with Finish, Touch and Requeue operations)
- Settings: MaxInFlight (RDY) and MsgTimeout
- Automatic reconnection when the connection is lost
- Async API

## Currently not supported

These features are not supported, but might be in the future.

- Discovery
- Backoff
- TLS
- Snappy
- AUTH

## Install

Install [from NuGet](https://www.nuget.org/packages/NsqClient/).

```
PM> Install-Package NsqClient
```

or

```
> dotnet add package NsqClient
```

## Usage

### Publishing

Create an instance of `INsqProducer`, connect (by default to `localhost:4150`) and then call `PublishAsync`:

```csharp
INsqProducer producer = new NsqProducer();
await producer.ConnectAsync();

string topicName = "mytopic";
string message = "mymessage";
await producer.PublishAsync(topicName, message);
```

You can also specify the connection parameters explicitly:

```csharp
INsqProducer producer = new NsqProducer(new NsqProducerOptions("hostname", 4150));
```

And publish messages as  `byte[]` instead of `string`:

```csharp
byte[] msg = Encoding.UTF8.GetBytes("my_message");
await producer.PublishAsync("topic_name", msg);
```

## Subscribing

Create an `INsqConsumer` instance and register the `OnMessage` event.

```csharp
string topicName = "mytopic";
string channelName = "mychannel";
INsqConsumer consumer = new NsqConsumer(new NsqConsumerOptions(topicName, channelName));

connection.OnMessage += OnMessage;

await connection.ConnectAsync();
```

Then handle the message in the callback:

```csharp
private async void OnMessage(object sender, NsqMessageEventArgs msg)
{
    Console.WriteLine("Received new message");
    Console.WriteLine("Processing attempt number: {0}", msg.Attempt);
    Console.WriteLine("Raw payload length: {0}", msg.Body.Length)
    Console.WriteLine("Payload string:\n{0}", msg.BodyString);

    await msg.Finish();
}
```

The available operations on a message are:

- `msg.Finish()` to complete the message
- `msg.Requeue()` to requeue with a 1 second delay
- `msg.Requeue(TimeSpan)` to requeue with a custom delay
- `msg.Touch()` to touch the message so that it's not delivered again

The `NsqConsumerOptions` class has many constructors that allow to set:

- `hostname` and `port` of nsqd
- the `topic` name
- the `channel` name
- the `maxInFlight` value: maximum number of messages that will be processed by this consumer at a given time
- the `msgTimeout` for this client, after which the message will be delivered again by the server

The `maxInFlight` value can also be adjusted at any given time with the `SetMaxInFlight(int)` method.

## Error handling

`INsqProducer` and `INsqConsumer` both provide a way to handle and log connection errors and reconnections.

### Connection errors

The `OnError` event is raised when there's an internal exception while reading from the stream, or when an error frame is received from NSQ.

```csharp
connection.OnError += OnError;

private static void OnError(object sender, NsqErrorEventArgs eventArgs)
{
    Console.WriteLine("OnError: {0}", eventArgs.Exception);
}
```

### Disconnections

The `OnDisconnected` event is raised when the client disconnects from the NSQ instance. A property `WillReconnect` tells whether the client will attempt to reconnect (true except when shutting down).

```csharp
connection.OnDisconnected += OnDisconnected;

private static void OnDisconnected(object sender, NsqDisconnectionEventArgs e)
{
    Console.WriteLine("OnDisconnected: Disconnected. Will reconnect? " + e.WillReconnect);
}
```

### Reconnections

The `OnReconnected` event is raised after the client has successfully reconnected to the NSQ server.

```csharp
connection.OnReconnected += OnReconnected;

private static void OnReconnected(object sender, NsqReconnectionEventArgs e)
{
    Console.WriteLine($"OnReconnected: Reconnected after {e.Attempts} attempts");
    Console.WriteLine($"In {e.ReconnectedAfter.TotalSeconds} seconds");
}
```
