using Natasha;
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
            var proxier = new Proxier<TestAbstact>();
            proxier["Get"] = "return value.Length;";
            var func = proxier.GetCreator<TestAbstact>();
            TestAbstact testAbstact = func();
            Assert.Equal(1, testAbstact.Get("1"));
        }
        [Fact(DisplayName = "抽象方法默认调用")]
        public void Test1()
        {
            var proxier = new Proxier<TestAbstact>();
            var func = proxier.GetCreator<TestAbstact>();
            TestAbstact testAbstact = func();
            Assert.Equal(0, testAbstact.Get("0"));
        }
        [Fact(DisplayName = "接口方法默认调用")]
        public void Test2()
        {
            var proxier = new Proxier<TestInterface1>();
            var func = proxier.GetCreator<TestInterface1>();
            var testAbstact = func();
            Assert.Equal(0, testAbstact.GetInfo1("0"));
        }
        [Fact(DisplayName = "接口方法实现")]
        public void Test3()
        {
            var proxier = new Proxier<TestInterface2>();
            proxier["GetInfo2"] = "return value.Length;";
            var func = proxier.GetCreator<TestInterface2>();
            var testAbstact = func();
            Assert.Equal(1, testAbstact.GetInfo2("0"));
        }
        [Fact(DisplayName = "重载方法默认调用")]
        public void Test4()
        {
            var proxier = new Proxier<TestOverride>();
            var func = proxier.GetCreator<TestOverride>();
            var testAbstact = func();
            Assert.Equal(11, testAbstact.Get("0"));
        }
        [Fact(DisplayName = "虚方法重写")]
        public void Test5()
        {
            var proxier = new Proxier<TestVirtual2>();
            proxier["Get"] = "return value.Length+5;";
            var func = proxier.GetCreator<TestVirtual2>();
            var testAbstact = func();
            Assert.Equal(6, testAbstact.Get("0"));
        }
        [Fact(DisplayName = "虚方法默认调用")]
        public void Test6()
        {
            var proxier = new Proxier<TestVirtual2>();
            var func = proxier.GetCreator<TestVirtual2>();
            var testAbstact = func();
            Assert.Equal(11, testAbstact.Get("0"));
        }
    }
}
