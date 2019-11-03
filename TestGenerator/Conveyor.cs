using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace TestGenerator
{
    public class Conveyor
    {
        public readonly int MaxDegreeOfParallelism = Environment.ProcessorCount;
        public List<string> SavedPathes { get; private set; }

        private readonly TransformBlock<string, string> LoadTestableFileBlock;
        private readonly TransformBlock<string, string> GenerateTestClassBlock;
        private readonly ActionBlock<string> SaveTestClassFileBlock;

        private readonly Mutex DirectoryWorkMutex = new Mutex();

        private bool _isComplited = false;
        public Task Complition => SaveTestClassFileBlock.Completion;

        //TODO: file name is kept in dataflow to tsve result ot file with proper name; result class with file name field
        public Conveyor()
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
                    await SaveToFile(testClassCode, @"..\..\..\Test.Files");
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
            return true;
        }

        public void Complete()
        {
            LoadTestableFileBlock.Complete();
            _isComplited = true;
        }

        private async Task<string> ReadFileContent(string path)
        {
            using (StreamReader reader = new StreamReader(path))
            {
                return await reader.ReadToEndAsync(); //QUESTION: why this method await needed for no runtime exception; and next method
            }
        }

        private Task<string> GenerateTestClassFile(string testableFileContent)
        {
            ContentParser cp = new ContentParser();
            return cp.GetTestClassFrom(testableFileContent);
        }

        private Task SaveToFile(string toSave, string outDir)
        {
            int i = 0;
            string savePath = null;

            DirectoryWorkMutex.WaitOne();//QUESTION: why lock was not working but mutex works?
            do
            {
                savePath = outDir + "\\" + i++ + ".cs";
            } while (File.Exists(savePath));

            SavedPathes.Add(savePath);
            Task saveToFileTask = Task.Run(() =>
            {
                using (StreamWriter saveFileStream = new StreamWriter(savePath))
                {
                    saveFileStream.Write(toSave.ToCharArray(), 0, toSave.Length);
                }
            });
            DirectoryWorkMutex.ReleaseMutex();//QUESTION: am i right that mutex wont be blocked until awaitable task done (if there is any inside critical code)

            return saveToFileTask;
        }

        public void ClearSavedPathes()
        {
            SavedPathes.Clear();
        }
    }
}