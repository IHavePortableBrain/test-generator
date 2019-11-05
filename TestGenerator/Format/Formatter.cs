using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestGenerator.Extension;
using TestGenerator.TestableFileInfo;

namespace TestGenerator.Format
{
    internal class Formatter
    {
        private const int StringBuilderInitCapacity = 16 * 1024;
        private const string Tab = "    ";
        private string TestObjectVarName;
        private readonly StringBuilder _sb = new StringBuilder(StringBuilderInitCapacity);

        private int StackFrameBaseNumber;

        private FileInfo _testableFileInfo;

        //Must return Task, and there shoud be no task.Wait inside
        internal Task<FormatFile> MakeTestClassFile(FileInfo testableFileInfo)
        {
            return Task.Run(() =>
            {
                _testableFileInfo = testableFileInfo;
                return new FormatFile(MakeTestClassFileName(), MakeTestClassFileContent());//QUESTION: is it okey i am implicitly using field _testableFileInfo?
            });
        }

        private string MakeTestClassFileName()
        {
            return _testableFileInfo.Namespaces[0].Classes[0].Name;
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

        private void AppendNoIndent(string str = "")
        {
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
            AddSetUp(ci);
            foreach (BaseMethodInfo mi in ci.Methods)
            {
                AddMethodTest(mi);
            }
            AppendLine("}");
        }

        private void AddSetUp(ClassInfo ci)
        {
            var mockTypesByVarName = new List<KeyValuePair<string, string>>();

            TestObjectVarName = ci.Name.GetPrivateVarName();

            AppendFormat("private {0} {1};\n", ci.Name, TestObjectVarName);

            if (ci.Constructors.Any())
                foreach (var kvp in ci.Constructors[0]
                    .ParamTypeNamesByParamName)
                {
                    string varName = kvp.Key.GetPrivateVarName();
                    if (kvp.Value[0] == 'I')
                    {
                        AppendFormat("private Mock<{0}> {1};\n", kvp.Value, varName);
                        mockTypesByVarName.Add(new KeyValuePair<string, string>(varName, kvp.Value));
                    }
                    else
                    {
                        string fullTypeName = kvp.Value.GetFullTypeName();
                        System.Type type = System.Type.GetType(fullTypeName);
                        object defaultValue = type?.GetDefault();
                        AppendFormat("private {0} {1} = {2};\n",
                            kvp.Value,
                            kvp.Key.GetPrivateVarName(),
                            defaultValue ?? "null");
                    }
                }
            AppendLine();

            AppendLine("[SetUp]");
            AppendLine("public void SetUp()");
            AppendLine("{");

            AddSetUpArrange(ci, mockTypesByVarName);

            AppendLine("}");

            AppendLine();
        }

        private void AddSetUpArrange(ClassInfo ci, List<KeyValuePair<string, string>> mockTypesByVarName)
        {
            if (!ci.Constructors.Any())
                return;

            foreach (var kvp in mockTypesByVarName)
            {
                AppendFormat("{0} = new Mock<{1}>();\n", kvp.Key, kvp.Value);
            }
            AppendFormat("{0} = new {1}(", TestObjectVarName, ci.Name);

            List<KeyValuePair<string, string>> paramPairs = ci
                .Constructors[0]
                .ParamTypeNamesByParamName;
            for (int i = 0; i < paramPairs.Count - 1; i++)
            {
                AppendNoIndent(paramPairs[i].Key.GetPrivateVarName() + ".Object, ");
            }
            if (paramPairs.Count > 0)
            {
                AppendNoIndent(paramPairs[paramPairs.Count - 1].Key.GetPrivateVarName());
            }
            AppendNoIndent(");\n");
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

        private void AddMethodTestArrange(BaseMethodInfo mi)
        {
            foreach (var kvp in mi.ParamTypeNamesByParamName)
            {
                string fullTypeName = kvp.Value.GetFullTypeName();
                System.Type type = System.Type.GetType(fullTypeName);
                object defaultValue = type?.GetDefault();
                AppendFormat("{0} {1} = {2};\n",
                    kvp.Value,
                    kvp.Key,
                    defaultValue ?? "null");
            }
            AppendLine();
        }

        //QUESTION: [MethodImpl(MethodImplOptions.AggressiveInlining)] is evil?
        private void AddMethodTestAct(BaseMethodInfo mi)
        {
            if (mi.ReturnTypeName != "void")
            {
                AppendFormat("{0} actual = ", mi.ReturnTypeName);
                AppendNoIndent(TestObjectVarName + "." + mi.Name + "(");
            }
            else
                AppendFormat("{0}", TestObjectVarName + "." + mi.Name + "(");

            List<KeyValuePair<string, string>> paramPairs = mi.ParamTypeNamesByParamName;
            for (int i = 0; i < paramPairs.Count - 1; i++)
            {
                AppendNoIndent(paramPairs[i].Key + ", ");
            }
            if (paramPairs.Count > 0)
            {
                AppendNoIndent(paramPairs[paramPairs.Count - 1].Key);
            }
            AppendNoIndent(");\n");

            AppendLine();
        }

        private void AddMethodTestAssert(BaseMethodInfo mi)
        {
            if (mi.ReturnTypeName != "void")
            {
                string fullTypeName = mi.ReturnTypeName.GetFullTypeName();
                System.Type type = System.Type.GetType(fullTypeName);
                object defaultValue = type?.GetDefault();

                AppendFormat("{0} expected = {1};\n", mi.ReturnTypeName, defaultValue);
                AppendLine("Assert.That(actual, Is.EqualTo(expected));");
            }

            AppendLine("Assert.Fail(\"autogenerated\");");
        }
    }
}