using System;
using System.Threading.Tasks;

namespace TestGenerator
{
    internal class TestClassFromFileContentParser
    {
        //Must return Task, and there shoud be no task.Wait inside
        internal static Task Parse(string testableFileContent)
        {
            return Task.Run(() => { throw new NotImplementedException(); });
        }
    }
}