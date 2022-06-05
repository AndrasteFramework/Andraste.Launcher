using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Andraste.Host;
using Andraste.Host.Logging;

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

            Console.Title = $"TDU2 Modding Framework - Attached to PID {proc.Id}";

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
