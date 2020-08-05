using System;

namespace PowershellHelpToMd
{
    class Program
    {
        static void Main(string[] args)
        {
            var cmdLets = XmlParser.ParseCmdLets(args[0]);
        }
    }
}
