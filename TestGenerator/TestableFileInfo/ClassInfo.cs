using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestGenerator.TestableFileInfo
{
    internal class ClassInfo
    {
        public string Name { get; private set; }
        public List<BaseMethodInfo> Methods { get; private set; }

        public ClassInfo()
        {
            Methods = new List<BaseMethodInfo>();
        }

        internal void Initialize(ClassDeclarationSyntax cds)
        {
            Name = cds.Identifier.ToString();
            foreach (BaseMethodDeclarationSyntax mds in cds.DescendantNodes().OfType<BaseMethodDeclarationSyntax>())
            {
                BaseMethodInfo miToAdd = new BaseMethodInfo();
                miToAdd.Initialize(mds);
                Methods.Add(miToAdd);
            }
        }
    }
}