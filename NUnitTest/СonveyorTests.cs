using NUnit.Framework;
using TestGenerator;

namespace TestGenerator.Tests
{
    public class ÑonveyorTests
    {
        [SetUp]
        public void Setup()
        {
            Conveyor conv = new Conveyor();
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