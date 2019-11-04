using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NUnit.Framework;
using SimpleRpc.Serialization.Hyperion;
using SimpleRpc.Transports;
using SimpleRpc.Transports.Http.Client;
using SimpleRpc.Transports.Http.Server;
using NSubstitute.ExceptionExtensions;


namespace SimpleRpc.Tests
{
    public interface ITestInterface
    {
        void Void(int a, int b);

        int Value(int a, int b);

        string String(string a, string b);

        Task Task(string a, string b);

        Task<string> TaskWithReturn(string a, string b);

        Task<string> TaskWithReturnGeneric<T>();

        Task<T> TaskWithReturnGenericReturn<T>();

        Task<string> TaskWithReturnGenericWithArgs<T, TT>(T arg, TT arg2);

        Task<TOut> TaskWithReturnGenericWithArgsGenericOut<T, TT, TOut>(T arg, TT arg2);

        Task<T> ThrowException<T>();
    }

    public class HttpClientTransportTests
    {
        private readonly ITestInterface _client;
        private readonly ITestInterface _serverMock;

        public HttpClientTransportTests()
        {
            _serverMock = Substitute.For<ITestInterface>();

            var server = new TestServer(
                new WebHostBuilder()
                    .ConfigureServices(services =>
                    {
                        services.AddSimpleRpcServer(new HttpServerTransportOptions {Path = "/"})
                                .AddSimpleRpcHyperionSerializer();

                        services.AddSingleton(_serverMock);
                    })
                    .Configure(builder =>
                    {
                        builder.UseSimpleRpcServer();
                    }));

            var clientServices = new ServiceCollection();

            clientServices
                .AddSimpleRpcClient(
                    "test",
                    new HttpClientTransportOptions
                    {
                        Url = $"{server.BaseAddress}",
                        //Serializer = "HyperionMessageSerializer"
                    },
                    httpBuilder => httpBuilder.ConfigurePrimaryHttpMessageHandler(() => server.CreateHandler()))
                .AddSimpleRpcHyperionSerializer();

            clientServices.AddSimpleRpcProxy<ITestInterface>("test");

            var clientProvider = clientServices.BuildServiceProvider();

            _client = clientProvider.GetService<ITestInterface>();
        }


        [Test]
        public async Task TestString()
        {
            //when
            _serverMock.String("a", "b").Returns("ab");

            var result = _client.String("a", "b");

            //should
            _serverMock.Received().String("a", "b");
            result.Should().Be("ab");
        }

        [Test]
        public async Task TestException()
        {
            _serverMock.ThrowException<object>().Throws(new ArgumentException("Error received"));

            Func<Task> act = async () =>
            {
                await _client.ThrowException<object>();
            };

            await act.Should().ThrowAsync<RpcException>()
                .WithMessage("SimpleRpc server exception: RemoteMethodInvocation");
        }
    }
}
