using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.Win32;
using Mono.Cecil;
using Raptor.Api;
using Raptor.Modifications;

namespace Raptor
{
    internal static class Program
    {
        private static Assembly _terrariaAssembly;
        private static readonly Dictionary<string, Assembly> loadedAssemblies = new Dictionary<string, Assembly>();


        [STAThread]
        private static void Main(string[] args)
        {
            var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Re-Logic\Terraria");
            var rootPath = (string)key?.GetValue("Install_Path", null);
            if (rootPath == null)
            {
                ShowError("Could not find Terraria installation path.");
                return;
            }
            var terrariaPath = Path.Combine(rootPath, "Terraria.exe");
            if (!File.Exists(terrariaPath))
            {
                ShowError("Could not find Terraria executable.");
                return;
            }
            if (File.Exists("Terraria.exe"))
            {
                ShowError("Raptor should not be placed in the same directory as Terraria");
                return;
            }

            // If necessary, request for administrative privileges and create symlinks to the Content folder and the
            // native DLL. This is necessary because on Windows, only administrators can create symlinks by default.
            // We use symlinks instead of hard links, as they are more versatile and can link across drives.
            if (args.Length == 1 && args[0] == "setup")
            {
                NativeMethods.CreateSymbolicLink("Content", Path.Combine(rootPath, "Content"), 1);
                NativeMethods.CreateSymbolicLink("ReLogic.Native.dll", Path.Combine(rootPath, "ReLogic.Native.dll"), 0);
                return;
            }
            if (!Directory.Exists("Content") || !File.Exists("ReLogic.Native.dll"))
            {
                using (var process = new Process())
                {
                    process.StartInfo =
                        new ProcessStartInfo(Assembly.GetEntryAssembly().Location, "setup") {Verb = "runas"};
                    try
                    {
                        process.Start();
                        process.WaitForExit();
                    }
                    catch (Win32Exception)
                    {
                        ShowError("Could not create symbolic links as permission was not given.");
                        return;
                    }
                    if (!Directory.Exists("Content") || !File.Exists("ReLogic.Native.dll"))
                    {
                        ShowError("Could not create symbolic links.");
                        return;
                    }
                }
            }

            var assembly = AssemblyDefinition.ReadAssembly(terrariaPath);
            var modifications = from t in Assembly.GetExecutingAssembly().GetTypes()
                                where t.IsSubclassOf(typeof(Modification))
                                select (Modification)Activator.CreateInstance(t);
            foreach (var modification in modifications)
            {
                modification.Apply(assembly);
            }
            using (var stream = new MemoryStream())
            {
                assembly.Write(stream);
#if PATCHING || DEBUG
                assembly.Write("debug.exe");
#endif
                _terrariaAssembly = Assembly.Load(stream.ToArray());
            }

            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
            Run(args);
        }

        private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {

            string fileName = args.Name.Split(',')[0];
            string path = Path.Combine("plugins", fileName + ".dll");

            if (fileName == "Terraria")
            {
                return _terrariaAssembly;
            }

            if (File.Exists(path))
            {
                Assembly assembly;
                if (!loadedAssemblies.TryGetValue(fileName, out assembly))
                {
                    assembly = Assembly.LoadFrom(path);
                    loadedAssemblies.Add(fileName, assembly);
                    return assembly;
                }
            }
            else
            {
                var resourceName = new AssemblyName(args.Name).Name + ".dll";
                resourceName = _terrariaAssembly.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith(resourceName));
                if (resourceName == null)
                {
                    return null;
                }

                using (var stream = _terrariaAssembly.GetManifestResourceStream(resourceName))
                {
                    // ReSharper disable once PossibleNullReferenceException
                    var bytes = new byte[stream.Length];
                    stream.Read(bytes, 0, bytes.Length);
                    return Assembly.Load(bytes);
                }
            }
            return null;
        }

        private static void Run(string[] args)
        {
            using (var clientApi = new ClientApi())
            {
                clientApi.LoadPlugins();
                Terraria.Program.LaunchGame(args);
            }
        }

        private static void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
