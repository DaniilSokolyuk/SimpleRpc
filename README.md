SimpleRpc
===
[![License](https://img.shields.io/badge/license-apache%202.0-60C060.svg)](https://github.com/DaniilSokolyuk/SimpleRpc/blob/master/LICENSE)
[![NuGet](https://img.shields.io/nuget/dt/SimpleRpc.svg)](https://www.nuget.org/packages/SimpleRpc)
[![NuGet](https://img.shields.io/nuget/v/SimpleRpc.svg)](https://www.nuget.org/packages/SimpleRpc)

A simple and fast RPC library for .NET, .NET Core.

Quick Start
---
For .NET 4.6+, NET Standard 2 (.NET Core) available in NuGet

```
Install-Package SimpleRpc
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
