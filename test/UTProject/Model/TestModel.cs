using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace UTProject.Model
{
    public abstract class TestAbstact
    {
        public abstract int Get(string value);
    }


    public class TestVirtual
    { 
    
        public virtual int GetLength(string value)
        {
            return 0;
        }

    }


    public abstract class TestTaskAbstract 
    {
        public abstract ValueTask<int> Get(string value);
    }
    public interface TaskTestInterface1
    {
        public ValueTask<int> Get(string value);
    }

    



    public class TestVirtual2 : TestAbstact
    {
        public override int Get(string value)
        {
            return value.Length + 10;
        }
    }
    public class TestOverride : TestAbstact
    {
        public override int Get(string value)
        {
            return value.Length + 10;
        }
    }


    public interface TestInterface1
    {
        int GetInfo1(string value);
    }
    public interface TestInterface2
    {
        int GetInfo2(string value);
    }
}
