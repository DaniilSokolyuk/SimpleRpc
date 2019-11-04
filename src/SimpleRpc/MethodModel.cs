using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SimpleRpc
{
    public class MethodModel
    {
        public MethodModel(Type declaringType, string methodName, Type[] parameterTypes, Type[] genericArguments)
        {
            DeclaringType = declaringType;
            MethodName = methodName;
            ParameterTypes = parameterTypes;
            GenericArguments = genericArguments;
        }

        public MethodModel(MethodInfo method, Type[] genericArguments) :
            this(
                method.DeclaringType,
                method.Name,
                method.GetParameters().Select(t => t.ParameterType).ToArray(),
                genericArguments)
        {
        }

        public Type DeclaringType { get; }

        public string MethodName { get; }

        public Type[] ParameterTypes { get; }

        public Type[] GenericArguments { get; }
    }

    public sealed class MethodModelEqualityComparer : IEqualityComparer<MethodModel>
    {
        public static readonly MethodModelEqualityComparer Instance = new MethodModelEqualityComparer();

        public bool Equals(MethodModel x, MethodModel y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (ReferenceEquals(x, null))
                return false;
            if (ReferenceEquals(y, null))
                return false;
            if (x.GetType() != y.GetType())
                return false;
            return x.DeclaringType == y.DeclaringType &&
                   string.Equals(x.MethodName, y.MethodName) &&
                   ArrayEqualityComparer<Type>.Instance.Equals(x.ParameterTypes, y.ParameterTypes) &&
                   ArrayEqualityComparer<Type>.Instance.Equals(x.GenericArguments, y.GenericArguments);
        }

        public int GetHashCode(MethodModel obj)
        {
            unchecked
            {
                var hashCode = (obj.DeclaringType != null ? obj.DeclaringType.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (obj.MethodName != null ? obj.MethodName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (obj.ParameterTypes != null ? ArrayEqualityComparer<Type>.Instance.GetHashCode(obj.ParameterTypes) : 0);
                hashCode = (hashCode * 397) ^ (obj.GenericArguments != null ? ArrayEqualityComparer<Type>.Instance.GetHashCode(obj.GenericArguments) : 0);
                return hashCode;
            }
        }
    }
}
