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
                _testableFileInfo = new FileInfo(_testableFileContent);
                return MakeTestClass();
            });
        }

        private string MakeTestClass()
        {
            StringBuilder sb = new StringBuilder(StringBuilderInitCapacity);
            sb.AppendLine("using NUnit.Framework;");
            sb.AppendLine("using Moq;");

            return "42";
        }
    }
}