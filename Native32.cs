using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snappy
{
    class Native32 : NativeProxy
    {
        public static Native32 Instance = new Native32();

        Native32() : base("snappy32.dll") { }
    }
}
