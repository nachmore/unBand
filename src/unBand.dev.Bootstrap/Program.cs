using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            // dll so that we can add it as a reference for the BT project that needs it
            var binPath = Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\");

            CreateDll(binPath);

            // since we'll often run this in VS, prevent the window from closing
            System.Console.ReadKey();
        }

        private static void CreateDll(string path)
        {
            // noop if it already exists
            Directory.CreateDirectory(path);

            CargoDll.GetUnBandCargoDll(Path.Combine(path, CargoDll.UNBAND_CARGO_DLL_NAME));

            System.Console.WriteLine("Final (modified) Cargo Dll placed in: " + path);
        }
    }
}
