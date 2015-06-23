using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TrueCraftPushRestart
{
    class Program
    {
        private static TrueCraftBuilder _builder;
        private static TrueCraftRestarter _restarter;

        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Arguments: [Host] [TruCraft.exe path] [GitHub secret key]");

                return;
            }

            string host = args[0];
            string path = args[1];
            string secret = args[2];

            _builder = new TrueCraftBuilder(Path.Combine(Path.GetDirectoryName(path), "TrueCraft"));
            _builder.Pull();
            _builder.Build();

            _restarter = new TrueCraftRestarter(path);
            _restarter.Start();

            GitHubHookListener listener = new GitHubHookListener(host, secret);
            listener.PushReceived += Listener_PushReceived;
            listener.Start();

            Console.CancelKeyPress += Console_CancelKeyPress;

            Thread.Sleep(Timeout.Infinite);
        }

        private static void Listener_PushReceived(object sender, EventArgs e)
        {
            _restarter.Stop();

            _builder.Pull();
            _builder.Build();

            _restarter.Start();
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Process.GetCurrentProcess().Kill();
        }
    }
}
