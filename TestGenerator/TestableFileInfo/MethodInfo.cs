using System.Collections.Generic;

namespace TestGenerator.TestableFileInfo
{
    internal class MethodInfo
    {
        public string Name { get; private set; }
        public List<KeyValuePair<string, string>> ParamTypeNamesByParamName { get; private set; } //for future SetUp method of class with IInterface param dependent constructor
        public KeyValuePair<string, string> ReturnTypeNameByValueName { get; private set; }                                                                                // relying on interfaces are named according naming convention

        public MethodInfo(string methodContent)
        {
        }
    }
}