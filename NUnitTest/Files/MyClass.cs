using System;
using System.Collections.Generic;
using System.Text;

namespace TestGenerator.Tests.Files
{
    public class MyClass
    {
        public MyClass()
        {
        }

        private string PrivateStringMethod(string str1, int int1)
        {
            return "42";
        }

        public void PublicVoidMethod1()
        {
        }

        public void PublicVoidMethod2(decimal d, OperatingSystem os)
        {
        }
    }
}