using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace Llvm.NET
{
    internal static partial class NativeMethods
    {
        /// <summary>Dynamically loads a DLL from a directory dependent on the current architecture</summary>
        /// <param name="moduleName">name of the DLL</param>
        /// <param name="rootPath">Root path to find the DLL from</param>
        /// <returns>Handle for the DLL</returns>
        /// <remarks>
        /// <para>This method will detect the architecture the code is executing on (i.e. x86 or x64)
        /// and will load the dll from an architecture specific sub folder of <paramref name="rootPath"/>.
        /// This allows use of AnyCPU builds and interop to simplify build processes from needing to
        /// deal with "mixed" configurations or other accidental compbinations that are a pain to 
        /// sort out and keep straight when the tools insist on creating AnyCPU projects and "mixed" configurations
        /// by default.</para>
        /// <para>If the <paramref name="rootPath"/>Is <see langword="null"/>, empty or all whitespace then
        /// the standard DLL search paths are used. This assumes the correct variant of the DLL is available
        /// (e.g. for a 32 bit system a 32 bit native DLL is found). This allows for either building as AnyCPU
        /// plus shipping multiple native DLLs, or building for a specific CPU type while shipping only one native
        /// DLL. Different products or projects may have different needs so this covers those cases.
        /// </para>
        /// </remarks>
        internal static IntPtr LoadWin32Library( string moduleName, string rootPath )
        {
            if( string.IsNullOrWhiteSpace( moduleName ) )
                throw new ArgumentNullException( nameof( moduleName ) );

            string libPath;
            if( string.IsNullOrWhiteSpace( rootPath ) )
                libPath = moduleName;
            else
            {
                if( Environment.Is64BitProcess )
                    libPath = Path.Combine( rootPath, "x64", moduleName );
                else
                    libPath = Path.Combine( rootPath, "x86", moduleName );
            }

            IntPtr moduleHandle = LoadLibrary( libPath );
            if( moduleHandle == IntPtr.Zero )
            {
                var lasterror = Marshal.GetLastWin32Error( );
                throw new Win32Exception( lasterror );
            }
            return moduleHandle;
        }

        [DllImport("kernel32", SetLastError=true, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)]string lpFileName);
    }
}
