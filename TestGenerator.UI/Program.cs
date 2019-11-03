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
            conveyor.Post(@"..\..\..\NUnitTest\Files\MyClass.cs");
            conveyor.Post(@"..\..\..\Test.Files\Faker.cs");
            conveyor.Post(@"..\..\..\Test.Files\Faker.cs");

            conveyor.Complete();
            conveyor.Complition.Wait();
            //wait
        }
    }
}