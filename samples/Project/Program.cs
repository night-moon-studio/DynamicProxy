using Natasha;
using System;

namespace Project
{
    class Program
    {
        static void Main(string[] args)
        {
            //准备委托
            TestProxy test = new TestProxy();
            Func<int, string> nameFunc = test.Abcdefg;
            Func<string, int> ageFunc = item => item.Length;
            Func<string> toStringFunc = () => "重写已重载的方法！";
            Action action = () => { Console.WriteLine("复用此函数！"); };

            //创建联合接口代理
            var proxier = new Proxier<MyAbstract, MyInterface1, MyInterface2>();
            proxier["GetName"] = nameFunc;
            //proxier["GetAge"] = ageFunc;
            proxier.SetMethod("GetAge", "return age.Length;");
            proxier["Show"] = action;
            proxier["ToString"] = toStringFunc;
            proxier["Show2"] = action;


            //获取接口实例委托
            var func = proxier.GetCreator<MyInterface1>();
            //创建接口实例
            MyInterface1 interface1 = func();
            Console.WriteLine(interface1.GetName(100));


            //获取接口实例委托
            var func2 = proxier.GetCreator<MyInterface2>();
            //创建接口实例
            MyInterface2 interface2 = func2();
            Console.WriteLine(interface2.GetAge("abcdefg"));


            //获取接口实例委托
            var func3 = proxier.GetCreator<MyAbstract>();
            //创建接口实例
            MyAbstract interface3 = func3();
            interface3.Show();
            interface3.Show2();
            Console.WriteLine(interface3.ToString());

            Console.ReadKey();
        }
    }
}
