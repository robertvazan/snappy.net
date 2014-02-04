using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snappy
{
    class Native64 : NativeProxy
    {
        public static Native64 Instance = new Native64();

        Native64() : base("snappy64.dll") { }
    }
}
