using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SimpleRpc.Sample.Shared;

namespace SimpleRPC.Sample.Server
{
    public class FooServiceImpl : IFooService
    {
        public void Plus(int a, int b)
        {
            Console.WriteLine(a + b);
        }

        public string Concat(string a, string b)
        {
            return a + b;
        }

        public async Task WriteFooAsync(string a, string b)
        {
            await Task.Delay(10);
            Console.WriteLine(a + b);
        }

        public async Task<string> ConcatAsync(string a, string b)
        {
            await Task.Delay(10);
            return a + b;
        }

        public async Task<string> ReturnGenericTypeAsString<T>()
        {
            await Task.Delay(10);
            return typeof(T).ToString();
        }

        public Task<T> GetFooGenericReturn<T>()
        {
            throw new NotImplementedException();
        }

        public Task<string> ArgsToStringAndReturn<T, TT>(T arg, TT arg2)
        {
            return Task.FromResult(arg.ToString() + arg2);
        }

        public Task<TOut> GetFooGenericReturnWithArg<T, TT, TOut>(T arg, TT arg2)
        {
            throw new NotImplementedException();
        }
    }
}
