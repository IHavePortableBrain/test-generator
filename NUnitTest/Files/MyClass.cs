using System;
using System.Collections.Generic;
using System.Text;

namespace TestGenerator.Tests.Files
{
    internal class MyClass
    {
        public MyClass()
        {
        }

        private string PrivateStringMethod(string str1, int int1)
        {
            return "42";
        }

        private void PublicVoidMethod1()
        {
        }

        private void PublicVoidMethod2(decimal d, OperatingSystem os)
        {
        }
    }
}