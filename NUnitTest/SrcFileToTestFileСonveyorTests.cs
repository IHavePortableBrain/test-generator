using NUnit.Framework;
using TestGenerator;

namespace TestGenerator.Tests
{
    public class SrcFileToTestFile—onveyorTests
    {
        [SetUp]
        public void Setup()
        {
            SrcFileToTestFile—onveyor conv = new SrcFileToTestFile—onveyor();
            conv.Post(@"D:\! 5 semester\SPP\test-generator\Test.Files\new.txt");
        }

        [Test]
        public void TestDegereeOfParallelism()
        {
        }

        [Test]
        public void Test()
        {
        }
    }
}