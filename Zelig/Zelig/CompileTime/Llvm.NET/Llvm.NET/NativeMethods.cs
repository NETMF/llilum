using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace Llvm.NET
{
    internal static class NativeMethods
    {
        /// <summary>Dynamically loads a DLL from a directory dependent on the current architecture</summary>
        /// <param name="rootPath">Root path to find the DLL from</param>
        /// <param name="name">name of the DLL</param>
        /// <returns>Handle for the DLL</returns>
        /// <remarks>
        /// This method will detect the architecture the code is executing on (i.e. x86 or x64)
        /// and will load the dll from an architecture specific sub folder of <paramref name="rootPath"/>.
        /// This allows use of AnyCPU builds and interop to simplify build processes from needing to
        /// deal with "mixed" configurations or other accidental compbinations that are a pain to 
        /// sort out and keep straight when the tools insist on creating AnyCPU projects and "mixed" configurations
        /// by default. 
        /// </remarks>
        internal static IntPtr LoadWin32Library( string rootPath, string name )
        {
            if( string.IsNullOrEmpty( rootPath ) )
                throw new ArgumentNullException( nameof( rootPath ) );
            
            if( string.IsNullOrEmpty( name ) )
                throw new ArgumentNullException( nameof( name ) );

            string libPath;
            if( Environment.Is64BitProcess )
                libPath = Path.Combine( rootPath, "x64", name );
            else
                libPath = Path.Combine( rootPath, "x86", name );

            IntPtr moduleHandle = LoadLibrary( libPath );
            if( moduleHandle == IntPtr.Zero )
            {
                var lasterror = Marshal.GetLastWin32Error( );
                throw new Win32Exception( lasterror );
            }
            return moduleHandle;
        }

        [DllImport("kernel32", SetLastError=true, CharSet = CharSet.Ansi)]
        static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)]string lpFileName);
    }
}
