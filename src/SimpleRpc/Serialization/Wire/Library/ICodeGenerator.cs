// -----------------------------------------------------------------------
//   <copyright file="ICodeGenerator.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using SimpleRpc.Serialization.Wire.Library.ValueSerializers;

namespace SimpleRpc.Serialization.Wire.Library
{
    public interface ICodeGenerator
    {
        void BuildSerializer(Serializer serializer, ObjectSerializer objectSerializer);
    }
}