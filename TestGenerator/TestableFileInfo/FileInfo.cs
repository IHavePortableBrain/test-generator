using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;

namespace TestGenerator.TestableFileInfo
{
    internal class FileInfo
    {
        public List<NamespaceInfo> Namespaces { get; private set; }
        private SyntaxTree _tree;

        public FileInfo(string fileContent)
        {
            _tree = CSharpSyntaxTree.ParseText(fileContent);
            var d = _tree.GetRoot().DescendantNodes();
            Namespaces = new List<NamespaceInfo>();
            while (!false) //EofStr
            {
                Namespaces.Add(new NamespaceInfo(ReadNamespace(fileContent)));
            }
        }

        private string ReadNamespace(string fileContent)
        {
            throw new NotImplementedException();
        }
    }
}