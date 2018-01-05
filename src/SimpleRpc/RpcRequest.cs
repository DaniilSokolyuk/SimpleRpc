using System;
using System.Threading.Tasks;
using Fasterflect;
using Microsoft.Extensions.DependencyInjection;

namespace SimpleRpc
{
    public class RpcRequest
    {       
        public MethodModel Method { get; set; }

        public object[] Parameters { get; set; }

        public async Task<object> Invoke(IServiceProvider serviceProvider)
        {
            var resolvedType = serviceProvider.GetRequiredService(Method.DeclaringType);

            var result = resolvedType.CallMethod(
                Method.GenericArguments,
                Method.MethodName,
                Method.ParameterTypes,
                Parameters);

            if (result is Task task)
            {
                await task;

                if (task.GetType().IsGenericType)
                {
                    return task.GetPropertyValue(nameof(Task<object>.Result));
                }

                return null;
            }

            return result;
        }
    }
}
