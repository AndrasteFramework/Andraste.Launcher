using System.Threading;
using Andraste.Host;
using Andraste.Host.CommandLine;

namespace Andraste.Launcher
{
    public class Launcher : EntryPoint
    {
        public static void Main(string[] args)
        {
            var online = false;
            var mutex = new Mutex(false, online ? "44938b8f" : "957e4cc3"); // TDU2 specific hack.
            new CliEntryPoint().InvokeSync(args, null);
        }
    }
}
