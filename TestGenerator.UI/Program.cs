﻿using System;
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
            var conveyor = new Conveyor();

            Console.WriteLine(conveyor.Post(@"..\..\..\NUnitTest\Files\MyClass.cs"));
            Console.WriteLine(conveyor.Post(@"..\..\..\Test.Files\Faker.cs"));

            conveyor.Complete();
            Console.WriteLine(conveyor.Post(@"..\..\..\NUnitTest\Files\MyClass.cs"));

            conveyor.Complition.Wait();
        }
    }
}