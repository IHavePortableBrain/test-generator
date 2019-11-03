using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using TestGenerator.TestableFileInfo;

namespace TestGenerator.Tests
{
    public class ÑonveyorTests
    {
        private Conveyor conv;
        private string TestableFileContent;
        private System.IO.DirectoryInfo outDi = new System.IO.DirectoryInfo(@"..\..\..\Files\out");

        [SetUp]
        public void Setup()
        {
            ClearOutDir();
            conv = new Conveyor(@"..\..\..\Files\out");
            using (var sr = new System.IO.StreamReader(@"..\..\..\Files\MyClass.cs"))
            {
                TestableFileContent = sr.ReadToEnd();
            }
        }

        [Test]
        public void GatherInfoTest()
        {
            Task<FileInfo> gatherTask = conv.GatherInfo(TestableFileContent);
            gatherTask.Wait();
            FileInfo actual = gatherTask.Result;

            FileInfo expected; //QUESTION: i cant fill fields of expected cause they are incapsulated ; so i should assert for each field of actual?

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

        [Test]
        public void ParallelWorkTest()
        {
            Stopwatch sw = new Stopwatch();

            sw.Start();
            conv.Post(@"..\..\..\Files\MyClass.cs");
            conv.Complete();
            conv.Complition.Wait();
            sw.Stop();
            long oneFileProcessingElapsed = sw.ElapsedTicks;

            sw.Restart();
            conv = new Conveyor(@"..\..\..\Files\out");
            conv.Post(@"..\..\..\Files\MyClass.cs");
            conv.Post(@"..\..\..\Files\MyClass.cs");
            conv.Complete();
            conv.Complition.Wait();
            sw.Stop();
            long twoFileProcessingElapsed = sw.ElapsedTicks;

            Assert.Less(twoFileProcessingElapsed, 2 * oneFileProcessingElapsed);
            FileAssert.Exists(@"..\..\..\Files\out\MyClassTests.cs");
            FileAssert.Exists(@"..\..\..\Files\out\MyClassTests0.cs");
            FileAssert.Exists(@"..\..\..\Files\out\MyClassTests1.cs");
            System.IO.FileInfo fi1 = Array.Find(outDi.GetFiles(), fi => fi.Name == "MyClassTests.cs");
            System.IO.FileInfo fi2 = Array.Find(outDi.GetFiles(), fi => fi.Name == "MyClassTests0.cs");
            System.IO.FileInfo fi3 = Array.Find(outDi.GetFiles(), fi => fi.Name == "MyClassTests1.cs");
            Assert.AreEqual(fi1.Length, fi2.Length);
            Assert.AreEqual(fi2.Length, fi3.Length);
        }

        private void ClearOutDir()
        {
            foreach (System.IO.FileInfo file in outDi.GetFiles())
            {
                file.Delete();
            }
        }
    }
}