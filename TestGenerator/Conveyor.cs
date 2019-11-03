using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using TestGenerator.TestableFileInfo;
using TestGenerator.Format;

namespace TestGenerator
{
    public class Conveyor
    {
        public readonly int MaxDegreeOfParallelism = Environment.ProcessorCount;
        public List<string> SavedPathes { get; private set; }

        private readonly TransformBlock<string, string> LoadTestableFileBlock;
        private readonly TransformBlock<string, FileInfo> GatherInfoBlock;
        private readonly TransformBlock<FileInfo, string> GenerateTestClassBlock;
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

            GatherInfoBlock = new TransformBlock<string, FileInfo>(
                async testableFileContent =>
                {
                    return await GatherInfo(testableFileContent);
                },
                executionOptions);

            GenerateTestClassBlock = new TransformBlock<FileInfo, string>(
                async gatheredInfo =>
                {
                    return await GenerateTestClassFile(gatheredInfo);
                },
                executionOptions);

            SaveTestClassFileBlock = new ActionBlock<string>(
                async testClassCode =>
                {
                    await SaveToFile(testClassCode, @"..\..\..\Test.Files");
                },
                executionOptions);

            LoadTestableFileBlock.LinkTo(GatherInfoBlock, linkOptions, fileContent => fileContent.Length > 0);
            GatherInfoBlock.LinkTo(GenerateTestClassBlock, linkOptions, fileInfo =>
                fileInfo != null && fileInfo.Namespaces.Count > 0);
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
            using (var reader = new System.IO.StreamReader(path))
            {
                return await reader.ReadToEndAsync(); //QUESTION: why this method await needed for no runtime exception; and next method
            }
        }

        private Task<string> GenerateTestClassFile(FileInfo fi)
        {
            Formatter formatter = new Formatter();
            return formatter.MakeTestClassFile(fi);
        }

        internal Task<FileInfo> GatherInfo(string testableFileContent)//QUESTION: i need this class being private but for test it must be internal
        {
            return Task.Run(() =>
            {
                var result = new FileInfo();//QUESTION: Fat,implicitly slow ctor or simple ctor + Init method?
                result.Initialize(testableFileContent);//QUESTION: why test runtime crashes because of lack of Collection.Immutable; check out NUnitTest NuGet added

                return result;
            });
        }

        private Task SaveToFile(string toSave, string outDir)
        {
            int i = 0;
            string savePath = null;

            DirectoryWorkMutex.WaitOne();//QUESTION: why lock was not working but mutex works?
            do
            {
                savePath = outDir + "\\" + i++ + ".cs";
            } while (System.IO.File.Exists(savePath));

            SavedPathes.Add(savePath);
            Task saveToFileTask = Task.Run(() =>
            {
                using (var saveFileStream = new System.IO.StreamWriter(savePath))
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