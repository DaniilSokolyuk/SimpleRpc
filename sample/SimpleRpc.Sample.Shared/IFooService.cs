using System.Threading.Tasks;

namespace SimpleRpc.Sample.Shared
{
    public interface IFooService
    {
        void Plus(int a, int b);

        string Concat(string a, string b);

        Task WriteFooAsync(string a, string b);

        Task<string> ConcatAsync(string a, string b);

        Task<string> ReturnGenericTypeAsString<T>();

        Task<T> GetFooGenericReturn<T>();

        Task<string> ArgsToStringAndReturn<T, TT>(T arg, TT arg2);

        Task<TOut> GetFooGenericReturnWithArg<T, TT, TOut>(T arg, TT arg2);
    }
}
