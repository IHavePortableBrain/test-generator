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
            var conveyor = new Conveyor(@"..\..\..\NUnitTest\Files\out");

            Console.WriteLine(conveyor.Post(@"..\..\..\NUnitTest\Files\DependentClass.cs"));
            Console.WriteLine(conveyor.Post(@"..\..\..\NUnitTest\Files\MyClass.cs"));

            conveyor.Complete();
            conveyor.Complition.Wait();

            foreach (var item in conveyor.SavedPathes)
            {
                Console.WriteLine(item);
            }
        }
    }
}