using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Runtime.Caching;
using TestGenerator.TestableFileInfo;
using System.Linq;

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
            using (var sr = new System.IO.StreamReader(@"..\..\..\Files\MyClass.cs"))
            {
                TestableFileContent = sr.ReadToEnd();
            }
        }

        [Test]
        public void GatherInfoTest()
        {
            conv = new Conveyor(@"..\..\..\Files\out");

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

            List<BaseMethodInfo> actualConstructors = actualClasses[0].Constructors;
            Assert.AreEqual(1, actualConstructors.Count);

            Assert.AreEqual("MyClass", actualConstructors[0].Name);
            Assert.AreEqual(0, actualConstructors[0].ParamTypeNamesByParamName.Count);
            Assert.AreEqual(null, actualConstructors[0].ReturnTypeName);

            List<BaseMethodInfo> actualMethods = actualClasses[0].Methods;
            Assert.AreEqual(3, actualMethods.Count);

            Assert.AreEqual("PrivateStringMethod", actualMethods[0].Name);
            Assert.AreEqual(2, actualMethods[0].ParamTypeNamesByParamName.Count);
            var expectedParams = new List<KeyValuePair<string, string>> {
                new KeyValuePair<string, string>("str1" ,"string"),
                new KeyValuePair<string, string>("int1","int"),
                };
            Assert.AreEqual(expectedParams, actualMethods[0].ParamTypeNamesByParamName);
            Assert.AreEqual("string", actualMethods[0].ReturnTypeName);

            Assert.AreEqual("PublicVoidMethod1", actualMethods[1].Name);
            Assert.AreEqual(0, actualMethods[1].ParamTypeNamesByParamName.Count);
            Assert.AreEqual("void", actualMethods[1].ReturnTypeName);

            Assert.AreEqual("PublicVoidMethod2", actualMethods[2].Name);
            Assert.AreEqual(2, actualMethods[2].ParamTypeNamesByParamName.Count);
            expectedParams = new List<KeyValuePair<string, string>> {
                new KeyValuePair<string, string>("d","decimal"),
                new KeyValuePair<string, string>("os","OperatingSystem"),
                };
            Assert.AreEqual(expectedParams, actualMethods[2].ParamTypeNamesByParamName);
            Assert.AreEqual("void", actualMethods[2].ReturnTypeName);
        }

        [Test]
        public void ParallelWorkTest()
        {
            Stopwatch sw = new Stopwatch();

            sw.Restart();
            conv = new Conveyor(@"..\..\..\Files\out");
            conv.Post(@"..\..\..\Files\MyClass.cs");
            conv.Post(@"..\..\..\Files\MyClass.cs");
            conv.Complete();
            conv.Complition.Wait();
            sw.Stop();
            long twoFileProcessingElapsed = sw.ElapsedMilliseconds;

            ClearCache();

            sw.Restart();
            conv = new Conveyor(@"..\..\..\Files\out");
            conv.Post(@"..\..\..\Files\MyClass.cs");
            conv.Complete();
            conv.Complition.Wait();
            sw.Stop();
            long oneFileProcessingElapsed = sw.ElapsedMilliseconds;

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

        private void ClearCache()
        {
            ObjectCache cache = MemoryCache.Default;
            List<string> cacheKeys = cache.Select(kvp => kvp.Key).ToList();
            foreach (string cacheKey in cacheKeys)
            {
                cache.Remove(cacheKey);
            }
        }
    }
}