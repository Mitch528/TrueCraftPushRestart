using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibGit2Sharp;

namespace TrueCraftPushRestart
{
    public class TrueCraftBuilder
    {
        public string TrueCraftDir { get; set; }

        public TrueCraftBuilder(string dir)
        {
            TrueCraftDir = dir;
        }

        public void Clone()
        {
            var co = new CloneOptions();
            co.RecurseSubmodules = true;
            co.OnProgress = ProgressChanged;

            Repository.Clone("git://github.com/SirCmpwn/TrueCraft.git", TrueCraftDir, co);
        }

        public void Pull()
        {
            string dir = new DirectoryInfo(Path.Combine(TrueCraftDir, ".git")).FullName;

            if (!Directory.Exists(dir))
                Clone();

            using (var repo = new Repository(dir))
            {
                PullOptions options = new PullOptions();
                options.FetchOptions = new FetchOptions();
                options.FetchOptions.OnProgress = ProgressChanged;

                repo.Network.Pull(new Signature("truecraft", "git@truecraft.io", new DateTimeOffset(DateTime.Now)), options);
            }
        }

        private bool ProgressChanged(string status)
        {
            Console.WriteLine(status);

            return true;
        }


        public void Build()
        {
            RunNuGet();

            ProcessStartInfo psi;
            if (MonoHelper.IsRunningMono)
            {
                psi = new ProcessStartInfo("xbuild");
                psi.WorkingDirectory = TrueCraftDir;
            }
            else
            {
                throw new NotImplementedException();
            }

            Process buildProc = Process.Start(psi);

            if (buildProc == null)
                throw new NullReferenceException();

            buildProc.WaitForExit();

            string binDir = Path.Combine(TrueCraftDir, "TrueCraft", "bin", "Debug");
            string parent = new DirectoryInfo(TrueCraftDir).Parent.FullName;

            CopyFiles(binDir, parent);
        }

        public void RunNuGet()
        {
            string dir = new DirectoryInfo(TrueCraftDir).Parent.FullName;

            if (MonoHelper.IsRunningMono)
            {
                ProcessStartInfo psi;

                string nuget = Path.Combine(dir, "nuget.exe");
                if (File.Exists(nuget))
                {
                    psi = new ProcessStartInfo("mono", nuget + " restore");
                }
                else
                {
                    psi = new ProcessStartInfo("nuget", "restore");
                }

                psi.WorkingDirectory = TrueCraftDir;

                Process nugetProc = Process.Start(psi);

                if (nugetProc == null)
                    throw new NullReferenceException();

                nugetProc.WaitForExit();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private static void CopyFiles(string srcDir, string dstDir)
        {
            foreach (FileInfo file in new DirectoryInfo(srcDir).GetFiles())
            {
                string dest = Path.Combine(dstDir, file.Name);

                file.CopyTo(dest, true);
            }

            foreach (DirectoryInfo dir in new DirectoryInfo(srcDir).GetDirectories())
            {
                string dest = Path.Combine(dstDir, dir.Name);

                if (!Directory.Exists(dest))
                    Directory.CreateDirectory(dest);

                CopyFiles(dir.FullName, dest);
            }
        }
    }
}
