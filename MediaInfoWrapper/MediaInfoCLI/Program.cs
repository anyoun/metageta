using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MediaInfoLib;

namespace MediaInfoCLI
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 1 && args[0] == "--list-parameters")
                Console.WriteLine(new MediaInfoWrapper().GetParametersCsv());
            else if(args.Length == 1)
                TestPath(args[0]);
            else if(args.Length == 0)
                TestPath(".");
        }

        private static void TestPath(string path)
        {
            var mi = new MediaInfoWrapper();
            if (File.Exists(path))
                TestFile(mi, path);
            else if (Directory.Exists(path))
                foreach (var file in Directory.GetFiles(path))
                    TestFile(mi, file);
            else
                Console.WriteLine("Can't find path: {0}", path);
        }

        private static void TestFile(MediaInfoWrapper mediaInfo, string path)
        {
            Console.WriteLine(mediaInfo.ReadFile(path));
        }
    }
}
