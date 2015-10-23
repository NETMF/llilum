using System;
using System.Runtime.InteropServices;

namespace Llvm.NET.DebugInfo
{
    /// <summary>see <a href="http://llvm.org/docs/LangRef.html#difile"/></summary>
    public class DIFile 
        : DIScope
    {
        internal DIFile( LLVMMetadataRef handle )
            : base( handle )
        {
        }

        public string FileName
        {
            get
            {
                IntPtr name = NativeMethods.GetDIFileName( MetadataHandle );
                return Marshal.PtrToStringAnsi( name );
            }
        }

        public string Directory
        {
            get
            {
                IntPtr dir = NativeMethods.GetDIFileDirectory( MetadataHandle );
                return Marshal.PtrToStringAnsi( dir );
            }
        }

        public string Path => System.IO.Path.Combine( Directory, FileName );
    }
}
