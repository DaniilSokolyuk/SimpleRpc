using System;
using System.Reflection;
using System.Threading.Tasks;
using Fasterflect;
using Microsoft.Extensions.DependencyInjection;
using SimpleRpc.Utils.ObjectMethodExecutor;

namespace SimpleRpc
{
    public class RpcRequest
    {       
        public MethodModel Method { get; set; }

        public object[] Parameters { get; set; }

        public async Task<object> Invoke(IServiceProvider serviceProvider)
        {
            var resolvedType = serviceProvider.GetRequiredService(Method.DeclaringType);

            var typeinfo = resolvedType.GetType().GetTypeInfo();

            //TODO: ???
            var method = typeinfo.Method(Method.GenericArguments, Method.MethodName, Method.ParameterTypes);
            
            var executor = ObjectMethodExecutor.Create(method, typeinfo);

            if (executor.IsMethodAsync)
            {
                if (executor.MethodReturnType == typeof(Task))
                {
                    await (Task)executor.Execute(resolvedType, Parameters);
                }
                else
                {
                    return await executor.ExecuteAsync(resolvedType, Parameters);
                }
            }
            else
            {
                return executor.Execute(resolvedType, Parameters);
            }

            return null;
        }
    }
}
