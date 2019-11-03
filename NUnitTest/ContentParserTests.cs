using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using TestGenerator.TestableFileInfo;

namespace TestGenerator.Tests
{
    public class ContentParserTests
    {
        private ContentParser Parser;
        private string TestableFileContent;

        [SetUp]
        public void Setup()
        {
            Parser = new ContentParser();
            using (StreamReader sr = new StreamReader(@"..\..\..\Files\MyClass.cs"))
            {
                TestableFileContent = sr.ReadToEnd();
            }
        }

        [Test]
        public void TestGatheredFileInfo()
        {
            TestableFileInfo.FileInfo actual = Parser.GatherInfo(TestableFileContent);

            TestableFileInfo.FileInfo expected; //QUESTION: i cant fill fields of expected cause they are incapsulated ; so i should assert for each field of actual?

            List<NamespaceInfo> actualNs = actual.Namespaces;
            Assert.AreEqual(1, actualNs.Count);
            Assert.AreEqual("TestGenerator.Tests.Files", actualNs[0].Name);

            List<ClassInfo> actualClasses = actual.Namespaces[0].Classes;
            Assert.AreEqual(1, actualClasses.Count);
            Assert.AreEqual("MyClass", actualClasses[0].Name);

            List<BaseMethodInfo> actualMethods = actualClasses[0].Methods;
            Assert.AreEqual(4, actualMethods.Count);

            Assert.AreEqual("MyClass", actualMethods[0].Name);
            Assert.AreEqual(0, actualMethods[0].ParamTypeNamesByParamName.Count);
            Assert.AreEqual(null, actualMethods[0].ReturnTypeName);

            Assert.AreEqual("PrivateStringMethod", actualMethods[1].Name);
            Assert.AreEqual(2, actualMethods[1].ParamTypeNamesByParamName.Count);
            var expectedParams = new List<KeyValuePair<string, string>> {
                new KeyValuePair<string, string>("string", "str1"),
                new KeyValuePair<string, string>("int", "int1"),
                };
            Assert.AreEqual(expectedParams, actualMethods[1].ParamTypeNamesByParamName);
            Assert.AreEqual("string", actualMethods[1].ReturnTypeName);

            Assert.AreEqual("PublicVoidMethod1", actualMethods[2].Name);
            Assert.AreEqual(0, actualMethods[2].ParamTypeNamesByParamName.Count);
            Assert.AreEqual("void", actualMethods[2].ReturnTypeName);

            Assert.AreEqual("PublicVoidMethod2", actualMethods[3].Name);
            Assert.AreEqual(2, actualMethods[3].ParamTypeNamesByParamName.Count);
            expectedParams = new List<KeyValuePair<string, string>> {
                new KeyValuePair<string, string>("decimal", "d"),
                new KeyValuePair<string, string>("OperatingSystem", "os"),
                };
            Assert.AreEqual(expectedParams, actualMethods[3].ParamTypeNamesByParamName);
            Assert.AreEqual("void", actualMethods[3].ReturnTypeName);
        }
    }
}