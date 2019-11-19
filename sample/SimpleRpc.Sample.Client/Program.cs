using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SimpleRpc.Sample.Shared;
using SimpleRpc.Serialization.Hyperion;
using SimpleRpc.Transports;
using SimpleRpc.Transports.Http.Client;
using System.Collections;

namespace SimpleRpc.Sample.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.ReadKey();

            var sc = new ServiceCollection();

            sc.AddSimpleRpcClient("sample", new HttpClientTransportOptions
            {
                Url = "http://127.0.0.1:5000/rpc",
                Serializer = "HyperionMessageSerializer"
            })
                .AddSimpleRpcHyperionSerializer();

            sc.AddSimpleRpcProxy<IFooService>("sample");
            // or
            sc.AddSimpleRpcProxy(typeof(IFooService), "sample");

            var pr = sc.BuildServiceProvider();

            var service = pr.GetService<IFooService>();

            var tt = typeof(ICollection<string>);
                
            service.Plus(1, 5);
            Console.WriteLine(service.Concat("Foo", "Bar"));

            await service.WriteFooAsync("TaskFoo", "TaskBar");

            Console.WriteLine(await service.ConcatAsync("sadasd", "asdsd"));
            Console.WriteLine(await service.ReturnGenericTypeAsString<ICollection<string>>());
            Console.WriteLine(string.Join(", ", await service.ReturnGenericIEnumerable<int>()));
            

            try
            {
               await service.ThrowException<object>();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.ReadLine();
        }
    }
}
