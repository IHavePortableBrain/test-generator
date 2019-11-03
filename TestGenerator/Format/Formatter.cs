using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using TestGenerator.TestableFileInfo;

namespace TestGenerator.Format
{
    internal class Formatter
    {
        private const int StringBuilderInitCapacity = 16 * 1024;
        private const string Tab = "    ";
        private readonly StringBuilder _sb = new StringBuilder(StringBuilderInitCapacity);

        private int StackFrameBaseNumber;

        private FileInfo _testableFileInfo;

        //Must return Task, and there shoud be no task.Wait inside
        internal Task<string> MakeTestClassFile(FileInfo testableFileInfo)
        {
            return Task.Run(() =>
            {
                _testableFileInfo = testableFileInfo;
                return MakeTestClassFileContent();//QUESTION: is it okey i am implicitly using field _testableFileInfo?
            });
        }

        private string MakeTestClassFileContent()
        {
            StackFrameBaseNumber = new StackTrace().FrameCount;

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
            for (int i = 0; i < currDepth - 2 - StackFrameBaseNumber; i++)
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