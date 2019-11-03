using System;
using System.Text;
using System.Threading.Tasks;
using TestGenerator.TestableFileInfo;

namespace TestGenerator
{
    internal class TestClassFromFileContentParser
    {
        private const int StringBuilderInitCapacity = 16 * 1024;
        private FileInfo _testableFileInfo;
        private string _testableFileContent;

        //Must return Task, and there shoud be no task.Wait inside
        internal Task<string> GetTestClassFrom(string testableFileContent)
        {
            _testableFileContent = testableFileContent;
            return Task.Run(() =>
            {
                _testableFileInfo = GatherInfo(testableFileContent);
                return MakeTestClass();//QUESTION: is it okey i am implicitly using field _testableFileInfo?
            });
        }

        internal FileInfo GatherInfo(string testableFileContent)//QUESTION: i need this class being private but for test it must be internal
        {
            var result = new FileInfo();//QUESTION: Fat,implicitly slow ctor or simple ctor + Init method?
            result.Initialize(testableFileContent);//QUESTION: why test runtime crashes because of lack of Collection.Immutable; check out NUnitTest NuGet added dependency

            return result;
        }

        private string MakeTestClass()
        {
            StringBuilder sb = new StringBuilder(StringBuilderInitCapacity);

            //QUESTION: better use string.Join(Environment.NewLine, new string[]{ "line1", "line2"}?
            sb.AppendLine(@"using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Moq;");

            foreach (NamespaceInfo ns in _testableFileInfo.Namespaces)
            {
                sb.AppendFormat("using {0}; ", ns.Name);
            }

            return sb.ToString();
        }
    }
}