using System;
using System.Diagnostics;
using Andraste.Host;
using Andraste.Host.Logging;

namespace Launcher
{
    #nullable enable
    public class Launcher : EntryPoint
    {
        private static readonly string exePath = FILL IN
        private static readonly string dllPath = FILL IN

        public static void Main(string[] args)
        {
            new Launcher().Launch();
        }

        public void Launch()
        {
            // Boot up Andraste
            Initialize();

            Process? proc = null;
            try
            {
                proc = StartApplication(exePath, "", dllPath);
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
            var output = new FileLoggingHost("output.log");
            var err = new FileLoggingHost("error.log");
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

            Console.WriteLine("Process exited");
            Console.WriteLine("Press any key to continue");
            Console.ReadLine();
        }
    }
    #nullable restore
}
