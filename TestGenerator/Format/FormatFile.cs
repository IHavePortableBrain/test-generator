using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestGenerator.Format
{
    public class FormatFile
    {
        public string Name { get; private set; }
        public string Content { get; private set; }

        public FormatFile(string name, string content)
        {
            Name = name;
            Content = content;
        }
    }
}