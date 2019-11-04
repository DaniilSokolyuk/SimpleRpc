using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using SimpleRpc.Serialization.Ceras;
using SimpleRpc.Serialization.Hyperion;
using AutoFixture;

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

            _ceras = new CerasMessageSerializer();
            _hyperion = new HyperionMessageSerializer();

            _hyperion.SerializeAsync(_serializedWire, _data, _data.GetType()).GetAwaiter().GetResult();
            _ceras.SerializeAsync(_serializedCeras, _data, _data.GetType()).GetAwaiter().GetResult();
        }

        private CerasMessageSerializer _ceras;
        private HyperionMessageSerializer _hyperion;


        private Stream _serializedCeras = new MemoryStream();
        private Stream _serializedWire = new MemoryStream();

        private IList<TestClass> _data = new List<TestClass>();

        [Benchmark]
        public async Task CerasSerialize()
        {
            using (var ms = new MemoryStream())
            {
                await _ceras.SerializeAsync(ms, _data, _data.GetType());
            }
        }

        [Benchmark]
        public async Task CerasDeserialize()
        {
            _serializedCeras.Position = 0;
            var obj = await _ceras.DeserializeAsync(_serializedCeras, _data.GetType());
            var result = (IList<TestClass>)obj;

            if (!result[0].Equals(_data[0]))
            {
                throw new Exception("error");
            }
        }

        [Benchmark]
        public async Task WireSerialize()
        {
            using (var ms = new MemoryStream())
            {
                await _hyperion.SerializeAsync(ms, _data, _data.GetType());
            }
        }

        [Benchmark]
        public async Task WireDeserialize()
        {
            _serializedWire.Position = 0;
            var obj = await _hyperion.DeserializeAsync(_serializedWire, _data.GetType());
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
                Fixture fixture = new Fixture();
                var customization = new SupportMutableValueTypesCustomization();
                customization.Customize(fixture);

                var fixt = fixture.Create<TestClass>();

                _data.Add(fixt);
            }
        }
    }
}