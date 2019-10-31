using System.Collections.Generic;

namespace TestGenerator.TestableFileInfo
{
    internal class ClassInfo
    {
        public string Name { get; private set; }
        public List<MethodInfo> Methods { get; private set; }

        public ClassInfo(string classContent)
        {
        }
    }
}