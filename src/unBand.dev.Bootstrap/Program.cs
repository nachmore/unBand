using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using unBand.CargoClientEditor;

namespace unBand.dev.Bootstrap
{
    class Program
    {
        static void Main(string[] args)
        {
            // The idea behind the bootstrapper is to create the .unBand version of the CargoClient
            // dll so that we can add it as a reference for the projects that needs it
            var binPath = Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\");

            // dll rewriting requires us to load references for that dll, which are in the 
            // Band Sync directory, so copy the files over so that Mono.Cecil doesn't croak
            CopyBandDlls();
            
            CreateDll(binPath);

            // since we'll often run this in VS, prevent the window from closing
            System.Console.ReadKey();
        }

        private static void CopyBandDlls()
        {
            var unbandDir = new DirectoryInfo(CargoDll.GetOfficialBandDllPath());
            var destinationDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var files = unbandDir.GetFiles("*.dll");

            foreach (var file in files)
            {
                var destination = Path.Combine(destinationDir, file.Name);
                File.Copy(file.FullName, destination, overwrite: true);
            }
        }

        private static void CreateDll(string path)
        {
            // noop if it already exists
            Directory.CreateDirectory(path);

            CargoDll.GenerateUnbandDlls(path);

            System.Console.WriteLine("Final (modified) Band Dlls placed in: " + path);
        }

    }
}
