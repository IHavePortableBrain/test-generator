using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using TestGenerator.TestableFileInfo;

namespace TestGenerator
{
    internal class ContentParser
    {
        private const int StringBuilderInitCapacity = 16 * 1024;
        private const string Tab = "    ";
        private readonly StringBuilder _sb = new StringBuilder(StringBuilderInitCapacity);

        private int BaseStackFrameNumber;

        private FileInfo _testableFileInfo;
        private string _testableFileContent;

        //Must return Task, and there shoud be no task.Wait inside
        internal Task<string> GetTestClassFrom(string testableFileContent)
        {
            _testableFileContent = testableFileContent;
            return Task.Run(() =>
            {
                _testableFileInfo = GatherInfo(testableFileContent);
                return MakeTestClassFileContent();//QUESTION: is it okey i am implicitly using field _testableFileInfo?
            });
        }

        internal FileInfo GatherInfo(string testableFileContent)//QUESTION: i need this class being private but for test it must be internal
        {
            var result = new FileInfo();//QUESTION: Fat,implicitly slow ctor or simple ctor + Init method?
            result.Initialize(testableFileContent);//QUESTION: why test runtime crashes because of lack of Collection.Immutable; check out NUnitTest NuGet added dependency

            return result;
        }

        private string MakeTestClassFileContent()
        {
            BaseStackFrameNumber = new StackTrace().FrameCount;

            //QUESTION: better use string.Join(Environment.NewLine, new string[]{ "line1", "line2"}?
            AppendLine(@"using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Moq;");

            foreach (NamespaceInfo ns in _testableFileInfo.Namespaces)
            {
                AppendFormat("using {0};\n", ns.Name);
            }

            AppendLine();

            AppendFormat("namespace {0}.Tests\n", _testableFileInfo.Namespaces[0].Name);
            AppendLine("{");
            foreach (NamespaceInfo ns in _testableFileInfo.Namespaces)
            {
                foreach (ClassInfo ci in ns.Classes)
                {
                    AddTestClass(ci);
                }
            }
            AppendLine("}");

            return _sb.ToString();
        }

        private void Append(string str = "")
        {
            AddIndent();
            _sb.Append(str);
        }

        private void AppendLine(string str = "")
        {
            AddIndent();
            _sb.AppendLine(str);
        }

        private void AppendFormat(string format, params object[] args)
        {
            AddIndent();
            _sb.AppendFormat(format, args);
        }

        private void AddIndent()
        {
            int currDepth = new StackTrace().FrameCount;
            for (int i = 0; i < currDepth - 2 - BaseStackFrameNumber; i++)
                _sb.Append(Tab);
        }

        private void AddTestClasses()
        {
        }

        private void AddTestClass(ClassInfo ci)
        {
            AppendFormat("public class {0}Tests\n", ci.Name);
            AppendLine("{");
            //AddSetUp(ci);
            foreach (BaseMethodInfo mi in ci.Methods)
            {
                AddMethodTest(mi);
            }
            AppendLine("}");
        }

        private void AddSetUp(ClassInfo ci)
        {
            throw new NotImplementedException();
        }

        private void AddMethodTest(BaseMethodInfo mi)
        {
            AppendLine("[Test]");
            AppendFormat("public void {0}Test()\n", mi.Name);

            AppendLine("{");
            AddMethodTestArrange(mi);
            AddMethodTestAct(mi);
            AddMethodTestAssert(mi);
            AppendLine("}");

            AppendLine();
        }

        private void AddMethodTestBody(BaseMethodInfo mi)
        {
        }

        private void AddMethodTestAssert(BaseMethodInfo mi)
        {
            _sb.AppendLine("Assert by _sb");
        }

        //QUESTION: [MethodImpl(MethodImplOptions.AggressiveInlining)] is evil?
        private void AddMethodTestAct(BaseMethodInfo mi)
        {
            AppendLine("Act by method");
        }

        private void AddMethodTestArrange(BaseMethodInfo mi)
        {
        }
    }
}