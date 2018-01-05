// -----------------------------------------------------------------------
//   <copyright file="ConsistentArraySerializerFactory.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using SimpleRpc.Serialization.Wire.Library.Extensions;
using SimpleRpc.Serialization.Wire.Library.ValueSerializers;

namespace SimpleRpc.Serialization.Wire.Library.SerializerFactories
{
    public class ConsistentArraySerializerFactory : ValueSerializerFactory
    {
        public override bool CanSerialize(Serializer serializer, Type type)
        {
            return type.IsOneDimensionalPrimitiveArray();
        }

        public override bool CanDeserialize(Serializer serializer, Type type)
        {
            return CanSerialize(serializer, type);
        }

        public override ValueSerializer BuildSerializer(Serializer serializer, Type type,
            ConcurrentDictionary<Type, ValueSerializer> typeMapping)
        {
            var res = ConsistentArraySerializer.Instance;
            typeMapping.TryAdd(type, res);
            return res;
        }
    }
}