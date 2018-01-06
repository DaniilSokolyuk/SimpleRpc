using System;
using System.Threading.Tasks;
using Fasterflect;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NUnit.Framework;
using SimpleRpc.Transports;
using SimpleRpc.Transports.Abstractions.Client;
using SimpleRpc.Transports.Http.Client;
using SimpleRpc.Transports.Http.Server;

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
                        services.AddSimpleRpcServer(new HttpServerTransportOptions {Path = "/"});

                        services.AddSingleton(_serverMock);
                    })
                    .Configure(builder =>
                    {
                        builder.UseSimpleRpcServer();
                    }));

            var clientServices = new ServiceCollection();

            clientServices.AddSimpleRpcClient("test", new HttpClientTransportOptions
            {
                Url = $"{server.BaseAddress}"
            });

            clientServices.AddSimpleRpcProxy<ITestInterface>("test");

            var clientProvider = clientServices.BuildServiceProvider();

            clientProvider.GetService<ClientConfiguration>().Transport.SetFieldValue("_httpClient", server.CreateClient());

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
    }
}
