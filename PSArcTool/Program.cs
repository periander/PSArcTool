
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace PSArcTool
{
    [SuppressMessage("ReSharper", "LocalizableElement")]
    class Program
    {
        static void Main(string[] args)
        {
            var waitForEnter = false;

            try
            {

                Console.WriteLine("PSArcTool");
                Console.WriteLine("Periander 2016-08-17");
                Console.WriteLine("I was mucking around with No Man's Sky modding and wanted to make extracting/creating PAK files easier.");
                foreach (var arg in args)
                {
                    Console.WriteLine(arg);
                }

                if (args.Any() &&
                    args.All(arg => File.Exists(arg) && (arg.EndsWith(".pak", StringComparison.OrdinalIgnoreCase) || arg.EndsWith(".psarc", StringComparison.OrdinalIgnoreCase))))
                {
                    foreach (var pakFilePath in args)
                    {
                        Functions.Extract(pakFilePath);
                    }
                }
                else if (args.Any() && args.All(arg => File.Exists(arg) || Directory.Exists(arg)))
                    // A collection of files or folders
                {
                    Functions.Create(args, string.Empty);
                }
                else
                {
                    waitForEnter = true;
                    Console.WriteLine(
                        "Usage: Pass PAK/PSARC files as arguments to extract (click and drag on to PSArcTool). Pass anything else to create a PAK file.");
                }

            }
            catch (Exception exc)
            {
                waitForEnter = true;
                Console.WriteLine(exc);
            }

            if (waitForEnter)
            {
                Console.WriteLine("Press ENTER to quit.");
                Console.ReadLine();
            }
        }
    }
}
