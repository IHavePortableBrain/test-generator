using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using TestGenerator.TestableFileInfo;
using TestGenerator.Format;

namespace TestGenerator
{
    public class Conveyor : ITargetBlock<string>, IDataflowBlock, ISourceBlock<FormatFile>
    {
        public readonly int MaxDegreeOfParallelism = Environment.ProcessorCount;

        private readonly TransformBlock<string, FileInfo> GatherInfoBlock;
        private readonly TransformBlock<FileInfo, FormatFile> GenerateTestClassBlock;

        public Task Completion => GenerateTestClassBlock.Completion;

        //TODO: file name is kept in dataflow to tsve result ot file with proper name; result class with file name field
        public Conveyor()
        {
            var executionOptions = new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = MaxDegreeOfParallelism };
            var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };

            GatherInfoBlock = new TransformBlock<string, FileInfo>(
                async testableFileContent =>
                {
                    return await GatherInfo(testableFileContent);
                },
                executionOptions);

            GenerateTestClassBlock = new TransformBlock<FileInfo, FormatFile>(
                async gatheredInfo =>
                {
                    return await GenerateTestClassFile(gatheredInfo);
                },
                executionOptions);

            GatherInfoBlock.LinkTo(GenerateTestClassBlock, linkOptions, fileInfo =>
                fileInfo != null && fileInfo.Namespaces.Count > 0);
        }

        public bool Post(string testableFilePath)
        {
            return GatherInfoBlock.Post(testableFilePath);
        }

        private Task<FormatFile> GenerateTestClassFile(FileInfo fi)
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

        public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, string messageValue, ISourceBlock<string> source, bool consumeToAccept)
        {
            return ((ITargetBlock<string>)GatherInfoBlock).OfferMessage(messageHeader, messageValue, source, consumeToAccept);
        }

        public void Fault(Exception exception)
        {
            ((ITargetBlock<string>)GatherInfoBlock).Fault(exception);
        }

        public IDisposable LinkTo(ITargetBlock<FormatFile> target, DataflowLinkOptions linkOptions)
        {
            return GenerateTestClassBlock.LinkTo(target, linkOptions);
        }

        public FormatFile ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<FormatFile> target, out bool messageConsumed)
        {
            return ((ISourceBlock<FormatFile>)GenerateTestClassBlock).ConsumeMessage(messageHeader, target, out messageConsumed);
        }

        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<FormatFile> target)
        {
            return ((ISourceBlock<FormatFile>)GenerateTestClassBlock).ReserveMessage(messageHeader, target);
        }

        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<FormatFile> target)
        {
            ((ISourceBlock<FormatFile>)GenerateTestClassBlock).ReleaseReservation(messageHeader, target);
        }

        public void Complete()
        {
            GatherInfoBlock.Complete();
        }
    }
}