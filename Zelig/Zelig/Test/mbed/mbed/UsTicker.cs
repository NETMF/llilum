using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Zelig.Test.mbed
{
    public class UsTicker
    {
        [DllImport("C")]
        public static extern uint us_ticker_read();
    }
}
