using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrueCraftPushRestart
{
    public class TrueCraftRestarter
    {
        public string Path { get; set; }

        private Process _proc;

        public TrueCraftRestarter(string path)
        {
            Path = path;
        }

        public void Restart()
        {
            Stop();
            Start();
        }

        public void Stop()
        {
            if (_proc != null && !_proc.HasExited)
            {
                _proc.Kill();
            }
        }

        public void Start()
        {
            ProcessStartInfo psi;

            if (MonoHelper.IsRunningMono)
                psi = new ProcessStartInfo("mono", Path);
            else
                psi = new ProcessStartInfo(Path);

            _proc = Process.Start(psi);
        }
    }
}
