using System.Runtime.InteropServices;

namespace $safeprojectname$
{
    public class UsTicker
    {
        [DllImport("C")]
        public static extern uint us_ticker_read();
    }
}
