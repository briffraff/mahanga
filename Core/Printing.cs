using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using mahanga.Core.Interface;

namespace mahanga.Core
{
    public class Printing : IPrinting
    {
        public void ToolHeader(string name, string description, string version)
        {
            description = description.Length % 2 == 0 ? " " + description : description;

            string dashDec = new string('-', 10);
            string ldec = dashDec + "*** ";
            string rdec = " ***" + dashDec;

            int descLen = description.Length + ldec.Length + rdec.Length;
            string extra = new string('-', (descLen - (name.Length + ldec.Length + rdec.Length)) / 2);
            string extra2 = new string('-', (descLen - version.Length) / 2);

            Console.WriteLine(new string('-', descLen));
            Console.WriteLine($"{extra}{ldec}{name}{rdec}{extra}");
            Console.WriteLine($"{ldec}{description}{rdec}");
            Console.WriteLine($"{extra2}{version}{extra2}");
        }

        public void NewLine()
        {
            Console.WriteLine();
        }

        public void PrintingScanResults(Stopwatch watch, HashSet<string> allFiles, HashSet<string> allFootwears)
        {
            Console.WriteLine($"files count : {allFiles.Count + allFootwears.Count}");
            Console.WriteLine($"Execution Time: {watch.Elapsed.TotalSeconds} sec");
        }
    }
}
