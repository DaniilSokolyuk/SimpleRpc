using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using System.Threading.Tasks;

namespace SimpleRpc.Benchmarks
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //var tt = new SerializerBenchmark();

            //tt.Setup();

            //for (int i = 0; i < 100; i++)
            //{
            //    await tt.MsgPackDeserialize();
            //}

            BenchmarkRunner.Run<SerializerBenchmark>();
        }
    }
}
