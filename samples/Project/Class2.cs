using System;
using System.Collections.Generic;
using System.Text;

namespace Project
{
    public abstract class MyAbstract
    {
        public abstract void Show();
        public virtual void Show2()
        {
            Console.WriteLine("aa");
        }
        public override string ToString()
        {
            return base.ToString();
        }
    }
    public interface MyInterface1
    {
        string GetName(int i);
    }
    public interface MyInterface2
    {
        int GetAge(string age);
    }





    public class TestProxy
    {
        public string Abcdefg(int arg)
        {
            return arg.ToString();
        }
    }
}
