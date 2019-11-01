using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestGenerator.TestableFileInfo
{
    internal class BaseMethodInfo
    {
        public string Name { get; private set; }
        public List<KeyValuePair<string, string>> ParamTypeNamesByParamName { get; private set; } //for future SetUp method of class with IInterface param dependent constructor
        public string ReturnTypeName { get; private set; }                                                                                // relying on interfaces are named according naming convention

        public void Initialize(BaseMethodDeclarationSyntax mds)
        {
            //TODO: if is ctor no ret value; method name extract
            Name = mds.ChildTokens().Last().ToString();

            ReturnTypeName = mds.ChildNodes().OfType<TypeSyntax>().FirstOrDefault()?.ToString();

            ParamTypeNamesByParamName = mds.DescendantNodes().OfType<ParameterSyntax>()
                .Select(ps => new KeyValuePair<string, string>(ps.Type.ToString(), ps.Identifier.ToString()))
                .ToList();
        }
    }
}