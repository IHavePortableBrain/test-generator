﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace TestGenerator
{
    public class SrcFileToTestFileСonveyor
    {
        public readonly int MaxDegreeOfParallelism = Environment.ProcessorCount;
        public List<string> SavedPathes { get; private set; }

        private readonly TransformBlock<string, string> LoadTestableFileBlock;
        private readonly TransformBlock<string, string> GenerateTestClassBlock;
        private readonly ActionBlock<string> SaveTestClassFileBlock;

        private readonly Mutex Mutex = new Mutex();

        private bool _isComplited = false;
        public Task Complition => SaveTestClassFileBlock.Completion;

        public SrcFileToTestFileСonveyor()
        {
            SavedPathes = new List<string>();
            var executionOptions = new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = MaxDegreeOfParallelism };
            var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };

            LoadTestableFileBlock = new TransformBlock<string, string>(
                async path =>
                {
                    return await ReadFileContent(path);
                },
                executionOptions);

            GenerateTestClassBlock = new TransformBlock<string, string>(
                async testableFileContent =>
                {
                    return await GenerateTestClassFile(testableFileContent);
                },
                executionOptions);

            SaveTestClassFileBlock = new ActionBlock<string>(
                async testClassCode =>
                {
                    await SaveToFile(testClassCode);
                },
                executionOptions);

            LoadTestableFileBlock.LinkTo(GenerateTestClassBlock, linkOptions, fileContent => fileContent.Length > 0);
            GenerateTestClassBlock.LinkTo(SaveTestClassFileBlock, linkOptions);
        }

        public bool Post(string testableFilePath)
        {
            if (_isComplited)
                return false;
            LoadTestableFileBlock.Post(testableFilePath);
            //TODO: multithreading task completion
            return true;
        }

        public void Complete()
        {
            LoadTestableFileBlock.Complete();
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
            string outDir = @"..\..\..\Test.Files";
            int i = 0;
            string savePath = null;

            Mutex.WaitOne();//QUESTION: why lock was not working but mutex works?
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
            Mutex.ReleaseMutex();//QUESTION: am i right that mutex wont be blocked until awaitable task done (if there is any inside critical code)

            return saveToFileTask;
        }

        public void ClearSavedPathes()
        {
            SavedPathes.Clear();
        }
    }
}