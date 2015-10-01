// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==

#define DEBUG_PTRS

namespace System
{
    using System;
    using System.Reflection;
////using System.Reflection.Emit;
    using System.Runtime.ConstrainedExecution;
    using System.Diagnostics;
    using System.Runtime.Serialization;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
////using System.Runtime.Versioning;
////using System.Text;
    using System.Globalization;
////using System.Security;
////using Microsoft.Win32.SafeHandles;
////using StackCrawlMark = System.Threading.StackCrawlMark;

    internal unsafe struct AssemblyHandle
    {
        #region Public Static Members
////    internal static AssemblyHandle EmptyHandle
////    {
////        get
////        {
////            return new AssemblyHandle();
////        }
////    }
        #endregion

        #region Private Data Members
        private IntPtr m_ptr;
        #endregion

        internal IntPtr Value
        {
            get
            {
                return m_ptr;
            }
        }

        #region Constructor
        internal AssemblyHandle( IntPtr pAssembly )
        {
            m_ptr = pAssembly;
        }
        #endregion

        #region Internal Members

        public override int GetHashCode()
        {
            return m_ptr.GetHashCode();
        }

        public override bool Equals( object obj )
        {
            if(!(obj is AssemblyHandle))
            {
                return false;
            }

            return Equals( (AssemblyHandle)obj );
        }

        public unsafe bool Equals( AssemblyHandle handle )
        {
            return handle.m_ptr == m_ptr;
        }

        // Slow path
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    [SuppressUnmanagedCodeSecurity]
////    [ResourceExposure( ResourceScope.None )]
////    private extern static void GetAssembly( AssemblyHandle handle, ObjectHandleOnStack retAssembly );
////
////    [MethodImpl( MethodImplOptions.NoInlining )] // make sure that the slow path is not inlined
////    private static Assembly GetAssembly( IntPtr handle )
////    {
////        Assembly assembly = null;
////
////        GetAssembly( new AssemblyHandle( handle ), JitHelpers.GetObjectHandleOnStack( ref assembly ) );
////
////        return assembly;
////    }
////
////    // Fast path
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    [ResourceExposure( ResourceScope.None )]
////    private extern static Assembly GetAssemblyIfExists( IntPtr assembly );
////
////    internal Assembly GetAssembly()
////    {
////        // Cache the handle to avoid races
////        IntPtr handle = m_ptr;
////        if(handle.IsNull())
////        {
////            return null;
////        }
////        // Try the fast path first
////        Assembly assembly = GetAssemblyIfExists( handle );
////        if(assembly != null)
////        {
////            return assembly;
////        }
////        // Fast path did not work. Use the slow path.
////        return GetAssembly( handle );
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private extern IntPtr _GetManifestModule();
////
////    internal ModuleHandle GetManifestModule()
////    {
////        return new ModuleHandle( _GetManifestModule() );
////    }
////
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private extern bool _AptcaCheck( IntPtr sourceAssembly );
////
////    internal bool AptcaCheck( AssemblyHandle sourceAssembly )
////    {
////        return _AptcaCheck( (IntPtr)sourceAssembly.Value );
////    }
////
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern int GetToken();
        #endregion
    }
}
