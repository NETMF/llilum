
using Microsoft.Zelig.Runtime;

namespace Microsoft.Zelig.Test
{
    public static class TestConsole
    {
        public static void WriteLine(string message)
        {
            BugCheck.Log( message );
        }
    }
}
