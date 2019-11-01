using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;

namespace TestGenerator.TestableFileInfo
{
    internal class NamespaceInfo
    {
        public string Name { get; private set; }
        public List<ClassInfo> Classes { get; private set; }

        public NamespaceInfo()
        {
            Classes = new List<ClassInfo>();
        }

        internal void Initialize(NamespaceDeclarationSyntax nds)
        {
            Name = nds.Name.ToString();
            foreach (ClassDeclarationSyntax cds in nds.DescendantNodes().OfType<ClassDeclarationSyntax>())
            {
                ClassInfo ciToAdd = new ClassInfo();
                ciToAdd.Initialize(cds);
                Classes.Add(ciToAdd);
            }
        }
    }
}