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
        public List<BaseMethodInfo> Constructors { get; private set; }

        public ClassInfo()
        {
            Methods = new List<BaseMethodInfo>();
            Constructors = new List<BaseMethodInfo>();
        }

        internal void Initialize(ClassDeclarationSyntax cds)
        {
            Name = cds.Identifier.ToString();
            foreach (var mds in cds.DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                BaseMethodInfo miToAdd = new BaseMethodInfo();
                miToAdd.Initialize(mds);
                Methods.Add(miToAdd);
            }

            foreach (var ctor in cds.DescendantNodes().OfType<ConstructorDeclarationSyntax>())
            {
                BaseMethodInfo miToAdd = new BaseMethodInfo();
                miToAdd.Initialize(ctor);
                Constructors.Add(miToAdd);
            }
        }
    }
}