using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrueCraftPushRestart
{
    public static class MonoHelper
    {
        public static bool IsRunningMono
        {
            get
            {
                try
                {
                    return Type.GetType("Mono.Runtime") != null;
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}
