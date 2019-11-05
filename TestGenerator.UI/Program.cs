using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using TestGenerator.Format;

namespace TestGenerator.UI
{
    internal class Program
    {
        public static readonly int MaxDegreeOfParallelism = Environment.ProcessorCount;

        public static List<string> SavedPathes = new List<string>();
        private static readonly Mutex DirectoryWorkMutex = new Mutex();

        private static ExecutionDataflowBlockOptions _executionOptions =
            new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = MaxDegreeOfParallelism };

        private static readonly TransformBlock<string, string> LoadTestableFileBlock = new TransformBlock<string, string>(
                async path =>
                {
                    return await ReadFileContent(path);
                },
                _executionOptions);

        private static readonly ActionBlock<FormatFile> SaveTestClassFileBlock = new ActionBlock<FormatFile>(
                async formatTestClassFile =>
                {
                    await SaveFile(formatTestClassFile, SaveDir);
                },
                _executionOptions);

        private static readonly string SaveDir = @"..\..\..\NUnitTest\Files\out";

        private static void Main(string[] args)
        {
            var conveyor = new Conveyor();

            var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };
            LoadTestableFileBlock.LinkTo(conveyor, linkOptions);
            conveyor.LinkTo(SaveTestClassFileBlock, linkOptions);

            Console.WriteLine(LoadTestableFileBlock.Post(@"..\..\..\NUnitTest\Files\DependentClass.cs"));
            Console.WriteLine(LoadTestableFileBlock.Post(@"..\..\..\NUnitTest\Files\MyClass.cs"));

            LoadTestableFileBlock.Complete();
            SaveTestClassFileBlock.Completion.Wait();

            foreach (var path in SavedPathes)
            {
                Console.WriteLine(path);
            }
        }

        private static Task SaveFile(FormatFile ff, string outDir)
        {
            string toSave = ff.Content;
            string fName = ff.Name + "Tests";
            int i = 0;
            string savePath = null;

            DirectoryWorkMutex.WaitOne();//QUESTION: why lock was not working but mutex works?

            savePath = outDir + "\\" + fName + ".cs";
            if (System.IO.File.Exists(savePath))
            {
                do
                {
                    savePath = outDir + "\\" + fName + i++ + ".cs";
                } while (System.IO.File.Exists(savePath));
            }

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

        private static async Task<string> ReadFileContent(string path)
        {
            using (var reader = new System.IO.StreamReader(path))
            {
                return await reader.ReadToEndAsync(); //QUESTION: why this method await needed for no runtime exception; and next method
            }
        }
    }
}