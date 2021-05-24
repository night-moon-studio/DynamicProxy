using Natasha;
using Natasha.CSharp;
using System;
using System.Collections.Generic;
using System.Text;
using UTProject.Model;
using Xunit;

namespace UTProject
{
    [Trait("抽象类代理", "")]
    public class AbstractUT
    {
        [Fact(DisplayName = "抽象方法实现")]
        public void Test()
        {
            NatashaInitializer.InitializeAndPreheating();
            var proxier = new Proxier<TestAbstact>();
            proxier["Get"] = "return value.Length;";
            var func = proxier.GetDefaultCreator<TestAbstact>();
            TestAbstact testAbstact = func();
            Assert.Equal(1, testAbstact.Get("1"));
        }
        [Fact(DisplayName = "抽象方法默认调用")]
        public void Test1()
        {
            NatashaInitializer.InitializeAndPreheating();
            var proxier = new Proxier<TestAbstact>();
            var func = proxier.GetDefaultCreator<TestAbstact>();
            TestAbstact testAbstact = func();
            Assert.Equal(0, testAbstact.Get("0"));
        }
        [Fact(DisplayName = "接口方法默认调用")]
        public void Test2()
        {
            NatashaInitializer.InitializeAndPreheating();
            var proxier = new Proxier<TestInterface1>();
            var func = proxier.GetDefaultCreator<TestInterface1>();
            var testAbstact = func();
            Assert.Equal(0, testAbstact.GetInfo1("0"));
        }
        [Fact(DisplayName = "接口方法实现")]
        public void Test3()
        {
            NatashaInitializer.InitializeAndPreheating();
            var proxier = new Proxier<TestInterface2>();
            proxier["GetInfo2"] = "return value.Length;";
            var func = proxier.GetDefaultCreator<TestInterface2>();
            var testAbstact = func();
            Assert.Equal(1, testAbstact.GetInfo2("0"));
        }
        [Fact(DisplayName = "重载方法默认调用")]
        public void Test4()
        {
            NatashaInitializer.InitializeAndPreheating();
            var proxier = new Proxier<TestOverride>();
            var func = proxier.GetDefaultCreator<TestOverride>();
            var testAbstact = func();
            Assert.Equal(11, testAbstact.Get("0"));
        }
        [Fact(DisplayName = "虚方法重写")]
        public void Test5()
        {
            NatashaInitializer.InitializeAndPreheating();
            var proxier = new Proxier<TestVirtual2>();
            proxier["Get"] = "return value.Length+5;";
            var func = proxier.GetDefaultCreator<TestVirtual2>();
            var testAbstact = func();
            Assert.Equal(6, testAbstact.Get("0"));
        }
        [Fact(DisplayName = "虚方法默认调用")]
        public void Test6()
        {
            NatashaInitializer.InitializeAndPreheating();
            var proxier = new Proxier<TestVirtual2>();
            var func = proxier.GetDefaultCreator<TestVirtual2>();
            var testAbstact = func();
            Assert.Equal(11, testAbstact.Get("0"));
        }


        [Fact(DisplayName = "抽象类异步默认调用")]
        public async void Test7()
        {
            NatashaInitializer.InitializeAndPreheating();
            var proxier = new Proxier<TestTaskAbstract>();
            var func = proxier.GetDefaultCreator<TestTaskAbstract>();
            var testAbstact = func();
            Assert.Equal(0, await testAbstact.Get("0"));
        }
        [Fact(DisplayName = "接口异步默认调用")]
        public async void Test8()
        {
            NatashaInitializer.InitializeAndPreheating();
            var proxier = new Proxier<TaskTestInterface1>();
            var func = proxier.GetDefaultCreator<TaskTestInterface1>();
            var testAbstact = func();
            Assert.Equal(0, await testAbstact.Get("0"));
        }

        [Fact(DisplayName = "抽象方法构造函数实现")]
        public void Test9()
        {
            NatashaInitializer.InitializeAndPreheating();
            var proxier = new Proxier<TestAbstact>();
            proxier.ClassBuilder.Ctor(ctor => ctor
            .Public()
            .Param<int>("value")
            .Body("_value = value;")
            );
            proxier.ClassBuilder.PrivateReadonlyField<int>("_value");
            proxier["Get"] = "return _value;";
            var func = proxier.GetCreator<int>();
            TestAbstact testAbstact = func(10);
            Assert.Equal(10, testAbstact.Get("1"));
        }
        [Fact(DisplayName = "抽象方法构造函数实现")]
        public void Test10()
        {
            NatashaInitializer.InitializeAndPreheating();
            var proxier = new Proxier<TestAbstact>();
            proxier.SetSingleton();
            proxier.ClassBuilder.Ctor(ctor => ctor
            .Public()
            .Param<int>("value")
            .Body("_value = value;")
            );
            proxier.ClassBuilder.PrivateReadonlyField<int>("_value");
            proxier["Get"] = "return _value;";
            proxier.InitSingletonCreator(10);
            TestAbstact testAbstact = proxier.Singleton;
            Assert.Equal(10, testAbstact.Get("1"));
        }
    }

}
