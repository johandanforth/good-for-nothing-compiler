// The original code was originally Copyright © Microsoft Corporation.  All rights reserved.
// The original code was published with an article at https://msdn.microsoft.com/en-us/magazine/cc136756.aspx by Joel Pobar.
// The original terms were specified at http://www.microsoft.com/info/cpyright.htm but that page is long dead :)

using System;
using System.IO;

internal static class Program
{
    private static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("Usage: gfn.exe program.gfn");
            return;
        }

        try
        {
            Scanner scanner;
            using (TextReader input = File.OpenText(args[0]))
            {
                scanner = new Scanner(input);
            }
            var parser = new Parser(scanner.Tokens);

            var moduleName = Path.GetFileNameWithoutExtension(args[0]) + ".exe";
            var codeGen = new CodeGen(parser.Result, moduleName);
            codeGen.Compile();
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
        }
    }
}