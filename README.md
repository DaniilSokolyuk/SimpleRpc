SimpleRpc
===
[![Build status](https://ci.appveyor.com/api/projects/status/github/DaniilSokolyuk/SimpleRpc?svg=true)](https://ci.appveyor.com/project/DaniilSokolyuk/simplerpc/branch/master)
[![License](https://img.shields.io/badge/license-mit%202.0-60C060.svg)](https://github.com/DaniilSokolyuk/SimpleRpc/blob/master/LICENSE)
[![NuGet](https://img.shields.io/nuget/dt/SimpleRpc.svg)](https://www.nuget.org/packages/SimpleRpc)
[![NuGet](https://img.shields.io/nuget/v/SimpleRpc.svg)](https://www.nuget.org/packages/SimpleRpc)

A simple and fast contractless RPC library for .NET and .NET Core, over [IServiceCollection](https://github.com/aspnet/DependencyInjection) (you can use any supported DI container)

Quick Start
---
For .NET 4.6+, NET Standard 2 (.NET Core) available in NuGet

```
Install-Package SimpleRpc
```

### Client
```C#
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

### Server

In your `Startup` class...
```C#
public void ConfigureServices(IServiceCollection services)
{
    services.AddSimpleRpcServer(new HttpServerTransportOptions {Path = "/rpc"});

    services.AddSingleton<IFooService, FooServiceImpl>();
}

public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    app.UseSimpleRpcServer();
}
```

Sample Projects
---
[Samples](https://github.com/DaniilSokolyuk/SimpleRpc/tree/master/sample) contains examples for using of SimpleRpc

Serializers
---
[Ceras](https://github.com/rikimaru0345/Ceras) is using by default based on benchmarks

|           Method |     Mean |     Error |    StdDev |    Gen 0 |   Gen 1 |   Gen 2 |  Allocated |
|----------------- |---------:|----------:|----------:|---------:|--------:|--------:|-----------:|
|   CerasSerialize | 2.663 ms | 0.0146 ms | 0.0137 ms |  58.5938 | 58.5938 | 58.5938 |  254.96 KB |
| CerasDeserialize | 1.403 ms | 0.0048 ms | 0.0043 ms | 216.7969 | 82.0313 |       - | 1157.19 KB |
|    WireSerialize | 9.065 ms | 0.0066 ms | 0.0055 ms |  78.1250 |       - |       - |  526.68 KB |
|  WireDeserialize | 5.340 ms | 0.0137 ms | 0.0128 ms | 156.2500 | 62.5000 |       - |  799.37 KB |

Default serializer can be changed in `Serializer` property, for example
```C#
sc.AddSimpleRpcClient("sample", new HttpClientTransportOptions
{
    Url = "http://127.0.0.1:5000/rpc",
    Serializer = "HyperionMessageSerializer"
});
```

| Serializer                      | Name (for client options)          
| ------------------------------- |:-------------------------:|
| Ceras                           | CerasMessageSerializer    | 
| Hyperion                        | HyperionMessageSerializer |


---
