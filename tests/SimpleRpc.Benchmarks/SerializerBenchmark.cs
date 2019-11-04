using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using SimpleRpc.Serialization;

namespace SimpleRpc.Benchmarks
{
    [MemoryDiagnoser]
    [InProcess]
    public class SerializerBenchmark
    {
        [GlobalSetup]
        public void Setup()
        {
            PopulateTestData();

            SerializationHelper.GetByName(Constants.DefaultSerializers.Ceras).SerializeAsync(_serializedCeras, _data, _data.GetType());
            SerializationHelper.GetByName(Constants.DefaultSerializers.Wire).SerializeAsync(_serializedWire, _data, _data.GetType());
            SerializationHelper.GetByName(Constants.DefaultSerializers.MessagePack).SerializeAsync(_serializedMsgPack, _data, _data.GetType());
        }

        private Stream _serializedMsgPack = new MemoryStream();
        private Stream _serializedCeras = new MemoryStream();
        private Stream _serializedWire = new MemoryStream();

        private IList<TestClass> _data = new List<TestClass>();

        //[Benchmark]
        public async Task CerasSerialize()
        {
            using (var ms = new MemoryStream())
            {
                await SerializationHelper.GetByName(Constants.DefaultSerializers.Ceras).SerializeAsync(ms, _data, _data.GetType());
            }
        }

        //[Benchmark]
        public async Task CerasDeserialize()
        {
            _serializedCeras.Position = 0;
            var obj = await SerializationHelper.GetByName(Constants.DefaultSerializers.Ceras).DeserializeAsync(_serializedCeras, _data.GetType());
            var result = (IList<TestClass>)obj;

            if (!result[0].Equals(_data[0]))
            {
                throw new Exception("error");
            }
        }

        //[Benchmark]
        public async Task WireSerialize()
        {
            using (var ms = new MemoryStream())
            {
                await SerializationHelper.GetByName(Constants.DefaultSerializers.Wire).SerializeAsync(ms, _data, _data.GetType());
            }
        }

        //[Benchmark]
        public async Task WireDeserialize()
        {
            _serializedWire.Position = 0;
            var obj = await SerializationHelper.GetByName(Constants.DefaultSerializers.Wire).DeserializeAsync(_serializedWire, _data.GetType());
            var result = (IList<TestClass>)obj;

            if (!result[0].Equals(_data[0]))
            {
                throw new Exception("error");
            }
        }


        [Benchmark]
        public async Task MsgPackSerialize()
        {
            using (var ms = new MemoryStream())
            {
                await SerializationHelper.GetByName(Constants.DefaultSerializers.MessagePack).SerializeAsync(ms, _data, _data.GetType());
            }
        }

        [Benchmark]
        public async Task MsgPackDeserialize()
        {
            _serializedMsgPack.Position = 0;
            var obj = await SerializationHelper.GetByName(Constants.DefaultSerializers.MessagePack).DeserializeAsync(_serializedMsgPack, _data.GetType());
            var result = (IList<TestClass>)obj;

            if (!result[0].Equals(_data[0]))
            {
                throw new Exception("error");
            }
        }

        protected void PopulateTestData()
        {
            for (int i = 0; i < 1000; i++)
            {
                var data = new TestClass()
                {
                    Exception = new Exception("Test message", new ArgumentNullException("param", "Cannot be null")),
                    DateTime = new DateTime(1944, 6, 6), // DDay
                    Enum = TestEnum.Five,
                    String = "On June 6, 1944, more than 160,000 Allied troops landed along a 50-mile stretch of heavily-fortified French coastline",
                    Struct = new TestStruct()
                    {
                        Boolean = true,
                        Long = long.MaxValue,
                        Decimal = decimal.MinusOne,
                        Double = double.MaxValue,
                        Int = int.MaxValue,
                        Short = short.MaxValue,
                        ULong = ulong.MinValue,
                        Byte = byte.MaxValue,
                        Char = char.MaxValue,
                        Float = float.MinValue,
                        UShort = ushort.MinValue,
                        UInt = uint.MaxValue,
                        Sbyte = sbyte.MaxValue,
                        StringField = "On June 6, 1944, more than 160,000 Allied troops landed along a 50-mile stretch of heavily-fortified French coastline",
                    },
                    Decimal = decimal.MaxValue,
                    Float = float.MaxValue,
                    Long = long.MinValue,
                    Int = int.MinValue,
                    Double = double.Epsilon,
                    Char = char.MaxValue,
                    Byte = byte.MaxValue,
                    Sbyte = sbyte.MaxValue,
                    Short = short.MaxValue,
                    UInt = uint.MaxValue,
                    ULong = ulong.MaxValue,
                    UShort = ushort.MaxValue,
                    Boolean = true
                };

                _data.Add(data);
            }
        }
    }
}