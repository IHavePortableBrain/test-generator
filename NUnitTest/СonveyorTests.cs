using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Runtime.Caching;
using TestGenerator.TestableFileInfo;
using System.Linq;
using System.Threading.Tasks.Dataflow;
using TestGenerator.Format;
using System.Threading;

namespace TestGenerator.Tests
{
    public class ÑonveyorTests
    {
        private Conveyor conv;
        private string TestableFileContent;

        private const string OutDir = @"..\..\..\Files\out";
        private System.IO.DirectoryInfo outDi = new System.IO.DirectoryInfo(OutDir);

        public static readonly int MaxDegreeOfParallelism = Environment.ProcessorCount;

        private static readonly ExecutionDataflowBlockOptions _executionOptions =
            new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = MaxDegreeOfParallelism };

        private readonly DataflowLinkOptions linkOptions =
            new DataflowLinkOptions { PropagateCompletion = true };

        private ActionBlock<FormatFile> SaveTestClassFileBlock;

        private readonly Mutex DirectoryWorkMutex = new Mutex();

        [SetUp]
        public void Setup()
        {
            ClearOutDir();
            using (var sr = new System.IO.StreamReader(@"..\..\..\Files\MyClass.cs"))
            {
                TestableFileContent = sr.ReadToEnd();
            }

            SaveTestClassFileBlock = new ActionBlock<FormatFile>(
                async formatTestClassFile =>
                {
                    await SaveFile(formatTestClassFile, OutDir);
                },
                _executionOptions);
        }

        [Test]
        public void GatherInfoTest()
        {
            conv = new Conveyor();

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
            Assert.AreEqual(2, actualMethods.Count);

            Assert.AreEqual("PublicVoidMethod1", actualMethods[0].Name);
            Assert.AreEqual(0, actualMethods[0].ParamTypeNamesByParamName.Count);
            Assert.AreEqual("void", actualMethods[0].ReturnTypeName);

            Assert.AreEqual("PublicVoidMethod2", actualMethods[1].Name);
            Assert.AreEqual(2, actualMethods[1].ParamTypeNamesByParamName.Count);
            var expectedParams = new List<KeyValuePair<string, string>> {
                new KeyValuePair<string, string>("d","decimal"),
                new KeyValuePair<string, string>("os","OperatingSystem"),
                };
            Assert.AreEqual(expectedParams, actualMethods[1].ParamTypeNamesByParamName);
            Assert.AreEqual("void", actualMethods[1].ReturnTypeName);
        }

        [Test]
        public void ParallelWorkTest()
        {
            Stopwatch sw = new Stopwatch();

            sw.Restart();
            conv = new Conveyor();
            conv.LinkTo(SaveTestClassFileBlock, linkOptions);

            conv.Post(TestableFileContent);
            conv.Complete();
            SaveTestClassFileBlock.Completion.Wait();
            sw.Stop();
            long oneFileProcessingElapsed = sw.ElapsedMilliseconds;
            //SaveTestClassFileBlock.Completion.Status;
            SaveTestClassFileBlock = new ActionBlock<FormatFile>(
                async formatTestClassFile =>
                {
                    await SaveFile(formatTestClassFile, OutDir);
                },
                _executionOptions);

            ClearCache();

            sw.Restart();
            conv = new Conveyor();
            conv.LinkTo(SaveTestClassFileBlock, linkOptions);

            conv.Post(TestableFileContent);
            conv.Post(TestableFileContent);
            conv.Complete();
            conv.Completion.Wait();
            SaveTestClassFileBlock.Completion.Wait();
            sw.Stop();
            long twoFileProcessingElapsed = sw.ElapsedMilliseconds;

            Assert.Less(twoFileProcessingElapsed, 2 * oneFileProcessingElapsed);
            FileAssert.Exists(OutDir + @"\MyClassTests.cs");
            FileAssert.Exists(OutDir + @"\MyClassTests0.cs");
            FileAssert.Exists(OutDir + @"\MyClassTests1.cs");
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

        private Task SaveFile(FormatFile ff, string outDir)
        {
            string toSave = ff.Content;
            string fName = ff.Name + "Tests";
            int i = 0;
            string savePath = null;

            DirectoryWorkMutex.WaitOne();

            savePath = outDir + "\\" + fName + ".cs";
            if (System.IO.File.Exists(savePath))
            {
                do
                {
                    savePath = outDir + "\\" + fName + i++ + ".cs";
                } while (System.IO.File.Exists(savePath));
            }

            Task saveToFileTask = Task.Run(() =>
            {
                using (var saveFileStream = new System.IO.StreamWriter(savePath))
                {
                    saveFileStream.Write(toSave.ToCharArray(), 0, toSave.Length);
                }
            });
            DirectoryWorkMutex.ReleaseMutex();

            return saveToFileTask;
        }
    }
}