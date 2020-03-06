using System;
using System.Collections.Generic;
using System.Text;

namespace BenchmarkProject
{

    public interface TestInterface
    {
        int Get(string value);
    }

    public class TestModel : TestInterface
    {
        public int Get(string value)
        {
            return value.Length+1;
        }
    }
}
