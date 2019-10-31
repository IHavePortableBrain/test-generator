using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace TestGenerator
{
    public class SrcFileToTestFileСonveyor
    {
        private readonly object LockObj = new object();
        private readonly Mutex Mutex = new Mutex();

        public readonly int MaxDegreeOfParallelism = Environment.ProcessorCount;
        private readonly TransformBlock<string, string> LoadTestableFileBlock;
        private readonly TransformBlock<string, string> GenerateTestClassBlock;
        private readonly ActionBlock<string> SaveTestClassFileBlock;
        public List<string> SavedPathes { get; private set; }
        //private Task<string> ReadFileTask;
        //private Task<string> GenerateTestFileTask;
        //private Task SaveTestClassTask;

        public SrcFileToTestFileСonveyor()
        {
            SavedPathes = new List<string>();

            LoadTestableFileBlock = new TransformBlock<string, string>(
                async path =>
                {
                    return await ReadFileContent(path);
                },
                new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = MaxDegreeOfParallelism
                });

            GenerateTestClassBlock = new TransformBlock<string, string>(
                async testableFileContent =>
                {
                    return await GenerateTestClassFile(testableFileContent);
                },
                new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = MaxDegreeOfParallelism
                });

            SaveTestClassFileBlock = new ActionBlock<string>(
                async testClassCode =>
                {
                    await SaveToFile(testClassCode);
                },
                new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = MaxDegreeOfParallelism
                });

            LoadTestableFileBlock.LinkTo(GenerateTestClassBlock, fileContent => fileContent.Length > 0);
            GenerateTestClassBlock.LinkTo(SaveTestClassFileBlock);
        }

        public void Post(string testableFilePath)
        {
            LoadTestableFileBlock.Post(testableFilePath);
        }

        private async Task<string> ReadFileContent(string path)
        {
            using (StreamReader reader = new StreamReader(path))
            {
                return await reader.ReadToEndAsync();
            }
        }

        private async Task<string> GenerateTestClassFile(string testableFileContent)
        {
            return await Task.Run(async () =>
            {
                TestClassFromFileContentParser cp = new TestClassFromFileContentParser();
                return await cp.GetTestClassFrom(testableFileContent);
            });
        }

        private Task SaveToFile(string toSave)
        {
            string outDir = @"D:\! 5 semester\SPP\test-generator\Test.Files";
            int i = 0;
            string savePath = null;

            Mutex.WaitOne();
            do
            {
                savePath = outDir + "\\" + i++ + ".cs";
            } while (File.Exists(savePath));

            SavedPathes.Add(savePath);
            Task saveToFileTask;
            using (StreamWriter saveFileStream = new StreamWriter(savePath))
            {
                saveToFileTask = saveFileStream.WriteAsync(toSave.ToCharArray(), 0, toSave.Length);
            }
            Mutex.ReleaseMutex();

            return saveToFileTask; //QUESTION: am i right that mutex wont be blocked until awaitable task done (if there is any inside critical code)
        }

        public void ClearSavedPathes()
        {
            SavedPathes.Clear();
        }
    }
}