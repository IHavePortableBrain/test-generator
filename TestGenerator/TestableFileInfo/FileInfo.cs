using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TestGenerator.TestableFileInfo
{
    internal class FileInfo
    {
        public List<NamespaceInfo> Namespaces { get; private set; }
        private SyntaxTree _tree;

        public FileInfo()
        {
            //TODO: handle invalid files input somewhere e.g. .txt input
            Namespaces = new List<NamespaceInfo>();
        }

        public void Initialize(string fileContent)
        {
            _tree = CSharpSyntaxTree.ParseText(fileContent);
            SyntaxNode _root = _tree.GetRoot();
            foreach (NamespaceDeclarationSyntax ns in _root.DescendantNodes().OfType<NamespaceDeclarationSyntax>())
            {
                NamespaceInfo niToAdd = new NamespaceInfo();
                niToAdd.Initialize(ns);
                Namespaces.Add(niToAdd);
            }
        }
    }
}