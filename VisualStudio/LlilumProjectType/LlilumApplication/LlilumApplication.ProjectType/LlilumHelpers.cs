
namespace LlilumApplication
{
    using System.Diagnostics;
    using System.Threading.Tasks;

    public static class LlilumHelpers
    {
        public static Task TryKillPyocdAsync()
        {
            return Task.Run( ( ) =>
            {
                foreach( var proc in Process.GetProcessesByName( "pyocd_win" ) )
                {
                    proc.Kill( );
                    proc.WaitForExit( );
                }
            } );
        }

        public static Task TryKillOpenOcdAsync()
        {
            return Task.Run(() =>
            {
                foreach (var proc in Process.GetProcessesByName("openocd"))
                {
                    proc.Kill();
                    proc.WaitForExit();
                }
            });
        }
    }
}
