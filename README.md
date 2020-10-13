SockJS.NET
==========
An asynchronous .NET implementation of the SockJS client

Components
-----------
### [`syp.biz.SockJS.NET.Common`](https://github.com/sypbiz/SockJS.NET/tree/master/syp.biz/SockJS.NET/syp.biz.SockJS.NET.Common)
Includes all interfaces, extensions, enums and utils which are common to the client, user of the client and for extending the client.

### [`syp.biz.SockJS.NET.Client`](https://github.com/sypbiz/SockJS.NET/tree/master/syp.biz/SockJS.NET/syp.biz.SockJS.NET.Client)
The client library containing the actual SockJS client to be used by consuming applications.

### [`syp.biz.SockJS.NET.Test`](https://github.com/sypbiz/SockJS.NET/tree/master/syp.biz/SockJS.NET/syp.biz.SockJS.NET.Test)
A test application which consumes the client library.

### [`server`](https://github.com/sypbiz/SockJS.NET/tree/master/server)
A node.js SockJS server to be used in conjunction with the test application.

Basic Usage
-----------
```csharp
var sockJs = new SockJS("http://localhost:9999/echo");
sockJs.Connected += async (sender, e) =>
{
    // this event is triggered once the connection is established
    try
    {
        Console.WriteLine("Connected...");
        await sockJs.Send(JsonConvert.SerializeObject(new { foo = "bar" }));
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {e}");
    }
};

sockJs.Message += async (sender, msg) =>
{
    // this event is triggered every time a message is received
    try
    {
        Console.WriteLine($"Message: {msg}");
        await sockJs.Disconnect(); // disconnect after first received message
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {e}");
    }
};

sockJs.Disconnected += (sender, e) =>
{
    // this event is triggered when the connection is disconnected (for any reason)
    Console.WriteLine("Disconnected");
};

await sockJs.Connect(); // connect to the server
```

Advanced Usage
--------------
### Customize Configuration
```csharp
// create a default configuration file (default values)
var config = Configuration.Factory.BuildDefault("http://localhost:9999/echo"); 
var sockJs = new SockJs(config);
```
### Customize Logger
```csharp
config.Logger = new ConsoleLogger();
```

### Customize Default Request Headers
```csharp
config.DefaultHeaders = new WebHeaderCollection
{
    {HttpRequestHeader.UserAgent, "Custom User Agent"},
    {"application-key", "foo-bar"}
};
```

### Customize Transports
```csharp
// add custom transport implementations
config.TransportFactories.Add(new CustomTransportFactory());

// remove custom/built-in transport implementation
config.TransportFactories.Remove(config.TransportFactories.First(t => t.Name == "websocket-system"));

// disable transport
config.TransportFactories.First(t => t.Name == "websocket-system").Enabled = false;
```

Note
----
Built-in WebSocket connection (`websocket-system`) is implemented via [`System.Net.WebSockets.ClientWebSocket`](https://docs.microsoft.com/en-us/dotnet/api/system.net.websockets.clientwebsocket?view=netstandard-2.0) and as such, is not supported on  Windows 7, Windows Vista SP2, and Windows Server 2008. See [Remarks section](https://docs.microsoft.com/en-us/dotnet/api/system.net.websockets.clientwebsocket?view=netstandard-2.0#remarks).

References
----------
This library is based on the [`SockJS-client`](https://github.com/sockjs/sockjs-client) JavaScript library ([license](https://github.com/sockjs/sockjs-client/blob/master/LICENSE)).

Dependencies
------------
- [`Newtonsoft.Json`](https://www.nuget.org/packages/Newtonsoft.Json) ([license](https://github.com/JamesNK/Newtonsoft.Json/blob/master/LICENSE.md))