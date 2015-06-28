using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Zelig.Test
{
    public static class TestConsole
    {
        public static void Write(string message)
        {
            System.Console.Write("[TEST] " + message);
        }

        public static void WriteLine(string message)
        {
            System.Console.WriteLine("[ZELIG TEST] " + message);
        }
    }
}
