using System;
using System.Runtime.InteropServices;

namespace Llvm.NET
{
    /// <summary>LLVM MemoryBuffer</summary>
    public sealed class MemoryBuffer
        : IDisposable
    {
        /// <summary>Load a file as an LLVM Memory Buffer</summary>
        /// <param name="path">Path of the file to load into a <see cref="MemoryBuffer"/></param>
        public MemoryBuffer( string path )
        {
            IntPtr msg;
            if( LLVMNative.CreateMemoryBufferWithContentsOfFile( path, out OpaqueHandle_, out msg ).Succeeded )
                return;

            var msgText = string.Empty;
            if( msg != IntPtr.Zero )
            {
                msgText = Marshal.PtrToStringAnsi( msg );
                LLVMNative.DisposeMessage( msg );
            }

            throw new InternalCodeGeneratorException( msgText );
        }

        /// <summary>Size of the buffer</summary>
        int Size
        {
            get
            {
                if( OpaqueHandle.Pointer == IntPtr.Zero )
                    return 0;

                return LLVMNative.GetBufferSize( OpaqueHandle );
            }
        }

        public void Dispose( )
        {
            if( OpaqueHandle.Pointer != IntPtr.Zero )
            {
                LLVMNative.DisposeMemoryBuffer( OpaqueHandle );
                OpaqueHandle_ = default(LLVMMemoryBufferRef);
            }
        }

        internal LLVMMemoryBufferRef OpaqueHandle => OpaqueHandle_;
        private LLVMMemoryBufferRef OpaqueHandle_;
    }
}
