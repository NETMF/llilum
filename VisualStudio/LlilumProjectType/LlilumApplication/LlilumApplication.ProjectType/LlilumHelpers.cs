
namespace LlilumApplication
{
    using System.Diagnostics;
    using System.Threading.Tasks;

    public static class LlilumHelpers
    {
        public static void TryKillPyocd()
        {
            Process[] procs = Process.GetProcessesByName("pyocd_win");
            foreach(var proc in procs)
            {
                proc.Kill();
                proc.WaitForExit();
            }
        }
    }
}
