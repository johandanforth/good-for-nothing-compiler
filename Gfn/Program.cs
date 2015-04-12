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

        var path = args[0];
        var moduleName = Path.GetFileNameWithoutExtension(path) + ".exe";

        Scanner scanner;
        using (TextReader input = File.OpenText(path))
        {
            scanner = new Scanner(input);
        }
        var parser = new Parser(scanner.Tokens);
        parser.Parse();

        var codeGen = new CodeGen(parser.Result, moduleName);
        codeGen.Compile();
        Console.WriteLine("Successfully compiled to " + moduleName);
    }
}