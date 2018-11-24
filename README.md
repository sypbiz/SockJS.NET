SockJS.NET
==========
A .NET implementation of the SockJS client

Componenets
-----------
### [syp.biz.SockJS.NET.Common](https://github.com/sypbiz/SockJS.NET/tree/master/syp.biz/SockJS.NET/syp.biz.SockJS.NET.Common)
Includes all interfaces, extensions, enums and utils which are common to the client, user of the client and for extending the client.

### [syp.biz.SockJS.NET.Client](https://github.com/sypbiz/SockJS.NET/tree/master/syp.biz/SockJS.NET/syp.biz.SockJS.NET.Client)
The client library containing the actual SockJS client to be used by consuming applications.

### [syp.biz.SockJS.NET.Test](https://github.com/sypbiz/SockJS.NET/tree/master/syp.biz/SockJS.NET/syp.biz.SockJS.NET.Test)
A test application which consumes the client library.

### [server](https://github.com/sypbiz/SockJS.NET/tree/master/server)
A node.js SockJS server to be used in conjunction with the test application.

Basic Usage
-----
```csharp
SockJS.SetLogger(new ConsoleLogger()); // sets the global logger
var sockJs = new SockJS("http://localhost:9999/echo"); // creates a client and points it to the local node.js server
sockJs.AddEventListener("open", (sender, e) =>
{
    Console.WriteLine("Connection opened");
    sockJs.Send("foo");
});
sockJs.AddEventListener("message", (sender, e) =>
{
    var stringifiedArgs = string.Join(",", e.Select(o => o?.ToString() ?? null));
    Console.WriteLine($"Message received: {stringifiedArgs}");
    if (e[0] is TransportMessageEvent msg)
    {
        var dataString = msg.Data?.ToString();
        Console.WriteLine($"Message received: Data = {dataString}");

        if (dataString == "foo")
        {
            Console.WriteLine("Echo successful -> closing connection");
            sockJs.Close();
        }
    }
});
sockJs.AddEventListener("close", (sender, e) =>
{
    Console.WriteLine("Connection closed");
});
```

References
----------
This library is a .NET port of the [SockJS Client](https://github.com/sockjs/sockjs-client) JavaScript library and is subject to the same license published [here](https://raw.githubusercontent.com/sockjs/sockjs-client/cc6ae9531bda2d4ee80e52fab246933558790163/LICENSE), via commit **cc6ae9531bda2d4ee80e52fab246933558790163**