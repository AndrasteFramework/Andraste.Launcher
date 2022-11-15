using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using Andraste.Host;
using Andraste.Host.Logging;
using Andraste.Shared.ModManagement;
using Andraste.Shared.ModManagement.Json;

namespace Launcher
{
    #nullable enable
    public class Launcher : EntryPoint
    {
        public static void Main(string[] args)
        {
            var launcher = new Launcher();
            switch (args.Length)
            {
                case 2:
                    launcher.Launch(args[0], args[1]);
                    break;
                case 1:
                    launcher.Launch(args[0], "Andraste.Payload.Generic.dll");
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("No argument passed that contains the path to the game! Drag the game exe onto of this file or create a shortcut!");
                    Console.WriteLine("The second argument can be used to override the local DLL file name");
                    Console.WriteLine("Press ANY key to exit");
                    Console.ReadKey();
                    break;
            }
        }

        public void Launch(string exePath, string dllName)
        {
            // Boot up Andraste
            Initialize();

            // Unfortunately, .NET FX requires us to add the config file with the bindings redirect, otherwise it fails to load assemblies.
            // This fails when you run the game multiple times with different .configs (or if the .config is locked by the file?), but that's a corner case.
            // TODO: In theory we'd need to merge files, because here, dllName.config does not containing transitive rewrites that are part in Andraste.Shared.dll.config
            var bindingRedirectFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dllName + ".config");
            var bindingRedirectShared = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Andraste.Shared.dll.config");
            if (File.Exists(bindingRedirectFile))
            {
                File.Copy(bindingRedirectFile, exePath + ".config", true);
                // For some reason, debugging has shown that sometimes, it tries to resolve the .configs in the Launcher directory. Is that dependant on the app?
                File.Copy(bindingRedirectFile, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.GetFileName(exePath)) + ".config", true);
                //File.Copy(bindingRedirectShared, Path.Combine(Path.GetDirectoryName(exePath)!, "Andraste.Shared.dll.config"), true);
            }
            else if (File.Exists(bindingRedirectShared))
            {
                Console.WriteLine("Warning: Framework does not have a specific binding redirect file. Trying Andraste.Shared");
                File.Copy(bindingRedirectShared, exePath + ".config", true);
            }
            else
            {
                Console.WriteLine($"Warning: Could not find a binding redirect file at {bindingRedirectFile}. Try to have your IDE generate one.");
            }
            Process? proc = null;
            try
            {
                proc = StartApplication(exePath, "", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dllName));
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("There was an error while injecting into target:");
                Console.ResetColor();
                Console.WriteLine(e.ToString());
                Console.WriteLine("Press any key to continue");
                Console.ReadLine();
            }

            if (proc == null)
            {
                Console.WriteLine("Could not find the game process - Has it crashed?");
                Console.WriteLine("Press any key to continue");
                Console.ReadLine();
                return;
            }

            Console.Title = $"Andraste Console Launcher - Attached to PID {proc.Id}";

            #region Logging
            var output = new FileLoggingHost(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "output.log"));
            var err = new FileLoggingHost(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log"));
            output.LoggingEvent += (sender, args) => Console.WriteLine(args.Text);
            err.LoggingEvent += (sender, args) => Console.Error.WriteLine(args.Text);
            output.StartListening();
            err.StartListening();
            #endregion

            // Keep this thread (and thus the application) running
            proc.WaitForExit();

            // Dispose/Cleanup
            output.StopListening();
            err.StopListening();

            #if !DEBUG
            Console.WriteLine("Process exited");
            Console.WriteLine("Press any key to continue");
            Console.ReadLine();
            #endif
        }
    }
    #nullable restore
}
