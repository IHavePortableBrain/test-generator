using NUnit.Framework;
using TestGenerator;

namespace Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            SrcFileToTestFile—onveyor conv = new SrcFileToTestFile—onveyor();
            conv.Post(@"D:\! 5 semester\SPP\test-generator\Test.Files\new.txt");
        }
    }
}