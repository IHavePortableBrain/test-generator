using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestGenerator.UI
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // Create the image processing network if needed.
            var conveyor = new SrcFileToTestFileСonveyor(); //??=

            // Post the selected path to the network.
            //foreACH
            conveyor.Post(@"D:\! 5 semester\SPP\test-generator\Test.Files\new.txt");
            conveyor.Post(@"D:\! 5 semester\SPP\test-generator\Test.Files\new.txt");
            conveyor.Post(@"D:\! 5 semester\SPP\test-generator\Test.Files\new.txt");
            conveyor.Post(@"D:\! 5 semester\SPP\test-generator\Test.Files\new.txt");
            //wait
            Console.ReadKey();
        }
    }
}