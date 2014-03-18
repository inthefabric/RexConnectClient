## RexConnectClient

RexConnectClient is a client library for communication with [RexConnect](https://github.com/inthefabric/RexConnect) (for C#/.NET).

#### Install with NuGet
[RexConnectClient.Core](http://nuget.org/packages/RexConnectClient)
```
PM> Install-Package RexConnectClient 
```

#### Basic Usage
Create a new RexConnect request:
```cs
string requestId = "1234";

var r = new Request(requestId);
r.AddConfigSetting(RexConn.ConfigSetting.Pretty, "1");
r.AddSessionAction(RexConn.SessionAction.Start);
r.AddQuery("a = g.addVertex([name='a']);");
r.AddQuery("b = g.addVertex([name='b']);");
r.AddQuery("g.addEdge(a, b, 'knows');");
r.AddSessionAction(RexConn.SessionAction.Commit);
r.AddSessionAction(RexConn.SessionAction.Close);
```

Setup a request context:
```cs
string hostName = "localhost";
int port = 8185;

var ctx = new RexConnContext(r, hostName, port);
var data = new RexConnDataAccess(ctx);
```

Then execute the request:
```c
IResponseResult result = data.Execute();
```

#### Fabric

RexConnect and RexConnectClient were built to support the [Fabric](https://github.com/inthefabric/Fabric) project.


[![githalytics.com alpha](https://cruel-carlota.gopagoda.com/a591fd3d28f0f38ab762f471959496d5 "githalytics.com")](http://githalytics.com/inthefabric/RexConnectClient)
