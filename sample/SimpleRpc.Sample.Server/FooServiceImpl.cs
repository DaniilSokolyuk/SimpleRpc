using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SimpleRpc.Sample.Shared;

namespace SimpleRPC.Sample.Server
{
    public class FooServiceImpl : IFooService
    {
        public ValueTask<int> ValueTaskOfValueType(int result)
        {
            Console.WriteLine("ValueTaskOfValueType" + result);
            return new ValueTask<int>(result);
        }

        public ValueTask<string> ValueTaskOfReferenceType(string result)
        {
            Console.WriteLine("ValueTaskOfReferenceType" + result);
            return new ValueTask<string>(result);
        }
        
        public void Plus(int a, int b)
        {
            Console.WriteLine("void:" + a + b);
        }

        public string Concat(string a, string b)
        {
            return "func" + a + b;
        }

        public async Task WriteFooAsync(string a, string b)
        {
            await Task.Delay(10);
            Console.WriteLine("task normal" + a + b);
        }

        public async Task<string> ConcatAsync(string a, string b)
        {
            await Task.Delay(10);
            return "task<T>" + a + b;
        }

        public async Task<string> ReturnGenericTypeAsString<T>()
        {
            await Task.Delay(10);
            return "task<T>" + typeof(T).ToString();
        }

        public Task<IEnumerable<string>> ReturnGenericIEnumerable<T>()
        {
            return Task.FromResult((IEnumerable<string>)new Stack<string>(new string[] { typeof(T).Name, "1", "2", "3" }));
        }

        public Task<T> ThrowException<T>()
        {
            throw new ArgumentException("This is an expected error message!!!");
        }
    }
}
