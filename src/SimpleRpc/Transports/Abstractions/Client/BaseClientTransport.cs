using System;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Fasterflect;

namespace SimpleRpc.Transports.Abstractions.Client
{
    public abstract class BaseClientTransport : IInterceptor, IClientTransport
    {
        private static readonly ProxyGenerator _proxyGenerator = new ProxyGenerator();

        public abstract object HandleSync(RpcRequest rpcRequest);

        public abstract Task HandleAsync(RpcRequest rpcRequest);

        public abstract Task<T> HandleAsyncWithResult<T>(RpcRequest rpcRequest);

        public object BuildProxy(Type t)
        {
            return _proxyGenerator.CreateInterfaceProxyWithoutTarget(t, this);
        }

        public void Intercept(IInvocation invocation)
        {
            var rpcRequest = new RpcRequest
            {
                Method = new MethodModel(invocation.Method, invocation.GenericArguments),
                Parameters = invocation.Arguments
            };

            if (typeof(Task).IsAssignableFrom(invocation.Method.ReturnType))
            {
                //Task<T>
                if (invocation.Method.ReturnType.IsGenericType)
                {
                    invocation.ReturnValue = this.CallMethod(
                        invocation.Method.ReturnType.GetGenericArguments(), 
                        nameof(HandleAsyncWithResult),
                        rpcRequest);
                }
                else
                {
                    //Task
                    invocation.ReturnValue = HandleAsync(rpcRequest);
                }
            }
            else if (invocation.Method.ReturnType != typeof(void))
            {          
                //T
                invocation.ReturnValue = HandleSync(rpcRequest);
            }
            else
            {
                //void
                HandleSync(rpcRequest);
            }
        }
    }
}
