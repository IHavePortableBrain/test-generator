using System.Collections.Generic;

namespace TestGenerator.TestableFileInfo
{
    internal class NamespaceInfo
    {
        public string Name { get; private set; }
        public List<ClassInfo> Classes { get; private set; }

        public NamespaceInfo(string namespaceContent)
        {
        }
    }
}