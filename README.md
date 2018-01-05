SimpleRpc
===
[![License](https://img.shields.io/badge/license-apache%202.0-60C060.svg)](https://github.com/DaniilSokolyuk/SimpleRpc/blob/master/LICENSE)
[![NuGet](https://img.shields.io/nuget/dt/SimpleRpc.svg)](https://www.nuget.org/packages/SimpleRpc)
[![NuGet](https://img.shields.io/nuget/v/SimpleRpc.svg)](https://www.nuget.org/packages/SimpleRpc)

A simple and fast RPC library for .NET, .NET Core, over [IServiceCollection](https://github.com/aspnet/DependencyInjection) (you can use any supported DI container)

Quick Start
---
For .NET 4.6+, NET Standard 2 (.NET Core) available in NuGet

```
Install-Package SimpleRpc
```

Client
```
var sc = new ServiceCollection();

sc.AddSimpleRpcClient("sample", new HttpClientTransportOptions
{
    Url = "http://127.0.0.1:5000/rpc"                
});

sc.AddSimpleRpcProxy<IFooService>("sample");
// or
sc.AddSimpleRpcProxy(typeof(IFooService), "sample");

var pr = sc.BuildServiceProvider();

var service = pr.GetService<IFooService>();

service.Plus(1,5);
```

Server
```
public void ConfigureServices(IServiceCollection services)
{
    services.AddSingleton<IFooService, FooServiceImpl>();

    services.AddSimpleRpcServer(new HttpServerTransportOptions {Path = "/rpc"});
}

public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    app.UseSimpleRpcServer();
}
```

Transports
---

| Transport  | Client options            | Server options             |
| ---------- |:-------------------------:|:--------------------------:|
| HTTP       | HttpClientTransportOptions| HttpServerTransportOptions |


Serializers
---

| Serializer                              | Short name          
| --------------------------------------- |:----------:|
| MessagePack (lz4 compression, default)  | msgpack    | 
| Wire (lz4 compression)                  | wire       |


---
