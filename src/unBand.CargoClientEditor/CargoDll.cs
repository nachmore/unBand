using Microsoft.Win32;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace unBand.CargoClientEditor
{
    public static class CargoDll
    {

        private const string CARGO_DLL_NAME = "Microsoft.Cargo.Client.Desktop8.dll";
        public const string UNBAND_CARGO_DLL_NAME = "Microsoft.Cargo.Client.Desktop8.unBand.dll";

        private static List<string> _bandDlls = new List<string>()
        {
            "Microsoft.Band.Admin.Desktop.dll",
            "Microsoft.Band.Admin.dll",
            "Microsoft.Band.Desktop.dll",
            "Microsoft.Band.dll"
        };

        public static string GetUnBandCargoDll(string unbandBandDllPath = null)
        {
            var officialDllPath = GetOfficialBandDllPath();

            if (unbandBandDllPath == null)
            {
                unbandBandDllPath = GetUnBandAppDataDir();
            }

            foreach (var dllName in _bandDlls)
            {
                var officialDll = Path.Combine(officialDllPath, dllName);
                var unbandDll = Path.Combine(unbandBandDllPath, dllName);

                if (!(File.Exists(unbandDll) && GetVersion(officialDll) == GetVersion(unbandDll)))
                {
                    CreateUnBandCargoDll(officialDll, unbandDll);
                }
            }
            
            return unbandBandDllPath;
        }

        private static void CreateUnBandCargoDll(string officialDll, string unBandCargoDll)
        {
            var module = ModuleDefinition.ReadModule(officialDll);

            // make everything public - this little bit of magic (that hopefully no one will ever see, because
            // it's horrible) will allow us to extend the dll
            foreach (var type in module.Types)
            {
                type.IsPublic = true;

                // we also need to make CargoClient's constructor public (so that we can avoid various checks)

                if (type.Name == "CargoClient")
                {
                    foreach (var method in type.Methods)
                    {
                        if (method.Name.Contains(".ctor"))
                        {
                            method.IsPublic = true;
                        }
                    }

                    // modify internal fields. If we were in a minefield before imagine where we are now?
                    // (these have no properties, or no properties with a setter)
                    foreach (var field in type.Fields)
                    {
                        if (field.Name.Contains("deviceTransportApp"))
                        {
                            field.IsPublic = true;
                        }
                    }
                }
            }

            module.Write(unBandCargoDll);
        }

        private static string GetVersion(string dllPath)
        {
            try
            {
                var assembly = AssemblyDefinition.ReadAssembly(dllPath);

                return assembly.Name.Version.ToString();
            }
            catch 
            {
                return "Invalid DLL";
            }
        }

        private static string GetUnBandAppDataDir()
        {
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "unBand");

            // TODO: creating a file here feels like a dirty side affect
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            return Path.Combine(dir);
        }

        public static string GetOfficialBandDllPath()
        {
            // let's try and find the dll
            var dllLocations = new List<string>()
                {
                    GetDllLocationFromRegistry(),

                    // fallback path
                    Path.Combine(GetProgramFilesx86(), "Microsoft Band Sync")
                };

            foreach (string location in dllLocations)
            {
                var path = Path.GetDirectoryName(location);
                if (Directory.Exists(path))
                {
                    return path;
                }
            }

            throw new FileNotFoundException("Could not find Band Sync app on your machine. I looked in:\n\n" + string.Join("\n", dllLocations));
        }

        private static string GetDllLocationFromRegistry()
        {
            var sid = System.Security.Principal.WindowsIdentity.GetCurrent().User.ToString();

            var regRoot = Microsoft.Win32.RegistryHive.LocalMachine;
            string regKeyName = @"SOFTWARE\MICROSOFT\Windows\CurrentVersion\Installer\UserData\" + sid + @"\Components\23439AC101C46D55BBCE6A082085E137";
            string regValueName = "6A5C0F782DABC24499D24EB7E14D7951";

            RegistryKey regKey;

            if (Environment.Is64BitOperatingSystem)
            {
                regKey = RegistryKey.OpenBaseKey(regRoot, RegistryView.Registry64);
            }
            else
            {
                regKey = RegistryKey.OpenBaseKey(regRoot, RegistryView.Default);
            }

            regKey = regKey.OpenSubKey(regKeyName);

            if (regKey != null)
            {
                return regKey.GetValue(regValueName).ToString();
            }

            return "[not found] " + regKeyName + "\\" + regValueName;

        }

        private static string GetProgramFilesx86()
        {
            var envVar = (Environment.Is64BitProcess ? "ProgramFiles(x86)" : "ProgramFiles");

            return Environment.GetEnvironmentVariable(envVar);
        }
    }
}
