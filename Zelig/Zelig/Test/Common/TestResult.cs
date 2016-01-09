using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Zelig.Test
{
    [Flags]
    public enum TestResult
    {
        Pass            = 0,
        Fail            = 0x1,
        Skip            = 0x2,
        KnownFailure    = 0x4,
    }
}
