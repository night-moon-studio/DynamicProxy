using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Mathematics;
using BenchmarkDotNet.Order;
using Natasha;
using System;
using System.Collections.Generic;
using System.Text;

namespace BenchmarkProject
{

    [MemoryDiagnoser, MarkdownExporter, RPlotExporter]
    [MinColumn, MaxColumn, MeanColumn, MedianColumn]
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn(NumeralSystem.Arabic)]
    [CategoriesColumn]
    public class OriginTest
    {
        private static readonly Func<TestInterface> _func;
        private static readonly TestInterface _instance;
        private int A;
        static OriginTest()
        {

            Func<string,int> action = item=>item.Length+1;

            //创建联合接口代理
            var proxier = new Proxier<TestInterface>();
            
            //proxier.UseSingleton();
            proxier["Get"] = "return value.Length;";
            //proxier["Get"] = action;
            _func = proxier.GetDefaultCreator<TestInterface>();
            _instance = _func();
            for (int i = 0; i < 10000000; i++)
            {
                _func();
            }
            int value = _instance.Get("jaja");
            var result = new TestModel();
            value = result.Get("jaja");

        }


        [Benchmark(Description = "Origin")]
        public void Orgin()
        {
            TestInterface result = new TestModel();
            A = result.Get("jaja");
        }

        [Benchmark(Description = "ProxyGetInstance")]
        public void NatashaGetProxy()
        {
            TestInterface result = _func();
        }
        [Benchmark(Description = "Proxy")]
        public void NatashaProxy()
        {
            A = _func().Get("jaja");
        }

        [Benchmark(Description = "ProxyInstance")]
        public void NatashaProxyInstance()
        {
            A = _instance.Get("jaja");
        }
    }

}
