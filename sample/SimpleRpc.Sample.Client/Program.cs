using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using SimpleRpc.Sample.Shared;
using SimpleRpc.Transports;
using SimpleRpc.Transports.Abstractions.Client;
using SimpleRpc.Transports.Http;
using SimpleRpc.Transports.Http.Client;
using SimpleRpc.Transports.Http.Server;

namespace SimpleRpc.Sample.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var sc = new ServiceCollection();

            sc.AddSimpleRpcClient("sample", new HttpClientTransportOptions
            {
                Url = "http://127.0.0.1:5000/rpc"                
            });


            sc.AddSingleton(sp => (IFooService)typeof(IFooService).CreateRpcProxy("sample"));

            var pr = sc.BuildServiceProvider();

            var service = pr.GetService<IFooService>();


            service.Plus(1,5);
            Console.WriteLine(service.Concat("Foo", "Bar"));

            service.WriteFooAsync("TaskFoo", "TaskBar").GetAwaiter().GetResult();

            Console.WriteLine(service.ConcatAsync("sadasd", "asdsd").GetAwaiter().GetResult());
            Console.WriteLine(service.ReturnGenericTypeAsString<ICollection<string>>().GetAwaiter().GetResult());


            Console.ReadLine();
        }
    }
}
