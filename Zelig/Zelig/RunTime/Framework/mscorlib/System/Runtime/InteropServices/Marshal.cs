// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*=============================================================================
**
** Class: Marshal
**
**
** Purpose: This class contains methods that are mainly used to marshal 
**          between unmanaged and managed types.
**
**
=============================================================================*/

namespace System.Runtime.InteropServices
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
////using System.Reflection.Emit;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;
////using System.Runtime.Remoting;
////using System.Runtime.Remoting.Activation;
    using System.Runtime.CompilerServices;
////using System.Runtime.Remoting.Proxies;
    using System.Globalization;
    using System.Runtime.ConstrainedExecution;
////using System.Runtime.Versioning;
////using Win32Native = Microsoft.Win32.Win32Native;
////using Microsoft.Win32.SafeHandles;

    //========================================================================
    // All public methods, including PInvoke, are protected with linkchecks.  
    // Remove the default demands for all PInvoke methods with this global 
    // declaration on the class.
    //========================================================================

////[SuppressUnmanagedCodeSecurityAttribute()]
    public static class Marshal
    {
////    //====================================================================
////    // Defines used inside the Marshal class.
////    //====================================================================
////    private const int LMEM_FIXED = 0;
////    private const int LMEM_MOVEABLE = 2;
////    private static readonly IntPtr HIWORDMASK = unchecked( new IntPtr( (long)0xffffffffffff0000L ) );
////
////    // Win32 has the concept of Atoms, where a pointer can either be a pointer
////    // or an int.  If it's less than 64K, this is guaranteed to NOT be a 
////    // pointer since the bottom 64K bytes are reserved in a process' page table.
////    // We should be careful about deallocating this stuff.  Extracted to
////    // a function to avoid C# problems with lack of support for IntPtr.
////    // We have 2 of these methods for slightly different semantics for NULL.
////    private static bool IsWin32Atom( IntPtr ptr )
////    {
////        long lPtr = (long)ptr;
////        return 0 == (lPtr & (long)HIWORDMASK);
////    }
////
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
////    private static bool IsNotWin32Atom( IntPtr ptr )
////    {
////        long lPtr = (long)ptr;
////        return 0 != (lPtr & (long)HIWORDMASK);
////    }
////
////    //====================================================================
////    // The default character size for the system.
////    //====================================================================
////    public static readonly int SystemDefaultCharSize = 3 - Win32Native.lstrlen( new sbyte[] { 0x41, 0x41, 0, 0 } );
////
////    //====================================================================
////    // The max DBCS character size for the system.
////    //====================================================================
////    public static readonly int SystemMaxDBCSCharSize = GetSystemMaxDBCSCharSize();
////
////
////    //====================================================================
////    // The name, title and description of the assembly that will contain 
////    // the dynamically generated interop types. 
////    //====================================================================
////    private const String s_strConvertedTypeInfoAssemblyName = "InteropDynamicTypes";
////    private const String s_strConvertedTypeInfoAssemblyTitle = "Interop Dynamic Types";
////    private const String s_strConvertedTypeInfoAssemblyDesc = "Type dynamically generated from ITypeInfo's";
////    private const String s_strConvertedTypeInfoNameSpace = "InteropDynamicTypes";
////
////
////    //====================================================================
////    // Helper method to retrieve the system's maximum DBCS character size.
////    //====================================================================
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImplAttribute( MethodImplOptions.InternalCall )]
////    private static extern int GetSystemMaxDBCSCharSize();
////
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static String PtrToStringAnsi( IntPtr ptr )
////    {
////        if(Win32Native.NULL == ptr)
////        {
////            return null;
////        }
////        else if(IsWin32Atom( ptr ))
////        {
////            return null;
////        }
////        else
////        {
////            int nb = Win32Native.lstrlenA( ptr );
////            if(nb == 0)
////            {
////                return string.Empty;
////            }
////            else
////            {
////                StringBuilder sb = new StringBuilder( nb );
////                Win32Native.CopyMemoryAnsi( sb, ptr, new IntPtr( 1 + nb ) );
////                return sb.ToString();
////            }
////        }
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImplAttribute( MethodImplOptions.InternalCall )]
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static extern String PtrToStringAnsi( IntPtr ptr, int len );
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImplAttribute( MethodImplOptions.InternalCall )]
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static extern String PtrToStringUni( IntPtr ptr, int len );
////
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static String PtrToStringAuto( IntPtr ptr, int len )
////    {
////        return (SystemDefaultCharSize == 1) ? PtrToStringAnsi( ptr, len ) : PtrToStringUni( ptr, len );
////    }
////
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static String PtrToStringUni( IntPtr ptr )
////    {
////        if(Win32Native.NULL == ptr)
////        {
////            return null;
////        }
////        else if(IsWin32Atom( ptr ))
////        {
////            return null;
////        }
////        else
////        {
////            int nc = Win32Native.lstrlenW( ptr );
////            StringBuilder sb = new StringBuilder( nc );
////            Win32Native.CopyMemoryUni( sb, ptr, new IntPtr( 2 * (1 + nc) ) );
////            return sb.ToString();
////        }
////    }
////
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static String PtrToStringAuto( IntPtr ptr )
////    {
////        if(Win32Native.NULL == ptr)
////        {
////            return null;
////        }
////        else if(IsWin32Atom( ptr ))
////        {
////            return null;
////        }
////        else
////        {
////            int nc = Win32Native.lstrlen( ptr );
////            StringBuilder sb = new StringBuilder( nc );
////            Win32Native.lstrcpy( sb, ptr );
////            return sb.ToString();
////        }
////    }
    
        //====================================================================
        // SizeOf()
        //====================================================================
////    [ResourceExposure( ResourceScope.None )]
        [MethodImplAttribute( MethodImplOptions.InternalCall )]
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
        public static extern int SizeOf( Object structure );
    
////    [ResourceExposure( ResourceScope.None )]
        [MethodImplAttribute( MethodImplOptions.InternalCall )]
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
        public static extern int SizeOf( Type t );
    
    
////    //====================================================================
////    // OffsetOf()
////    //====================================================================
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static IntPtr OffsetOf( Type t, String fieldName )
////    {
////        if(t == null)
////            throw new ArgumentNullException( "t" );
////
////        FieldInfo f = t.GetField( fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
////        if(f == null)
////            throw new ArgumentException( Environment.GetResourceString( "Argument_OffsetOfFieldNotFound", t.FullName ), "fieldName" );
////        else if(!(f is RuntimeFieldInfo))
////            throw new ArgumentException( Environment.GetResourceString( "Argument_MustBeRuntimeFieldInfo" ), "fieldName" );
////
////        return OffsetOfHelper( ((RuntimeFieldInfo)f).GetFieldHandle().Value );
////    }
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImplAttribute( MethodImplOptions.InternalCall )]
////    private static extern IntPtr OffsetOfHelper( IntPtr f );
////
////    //====================================================================
////    // UnsafeAddrOfPinnedArrayElement()
////    //
////    // IMPORTANT NOTICE: This method does not do any verification on the
////    // array. It must be used with EXTREME CAUTION since passing in 
////    // an array that is not pinned or in the fixed heap can cause 
////    // unexpected results !
////    //====================================================================
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImplAttribute( MethodImplOptions.InternalCall )]
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static extern IntPtr UnsafeAddrOfPinnedArrayElement( Array arr, int index );
////
////
////    //====================================================================
////    // Copy blocks from CLR arrays to native memory.
////    //====================================================================
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static void Copy( int[] source, int startIndex, IntPtr destination, int length )
////    {
////        CopyToNative( source, startIndex, destination, length );
////    }
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static void Copy( char[] source, int startIndex, IntPtr destination, int length )
////    {
////        CopyToNative( source, startIndex, destination, length );
////    }
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static void Copy( short[] source, int startIndex, IntPtr destination, int length )
////    {
////        CopyToNative( source, startIndex, destination, length );
////    }
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static void Copy( long[] source, int startIndex, IntPtr destination, int length )
////    {
////        CopyToNative( source, startIndex, destination, length );
////    }
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static void Copy( float[] source, int startIndex, IntPtr destination, int length )
////    {
////        CopyToNative( source, startIndex, destination, length );
////    }
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static void Copy( double[] source, int startIndex, IntPtr destination, int length )
////    {
////        CopyToNative( source, startIndex, destination, length );
////    }
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static void Copy( byte[] source, int startIndex, IntPtr destination, int length )
////    {
////        CopyToNative( source, startIndex, destination, length );
////    }
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static void Copy( IntPtr[] source, int startIndex, IntPtr destination, int length )
////    {
////        CopyToNative( source, startIndex, destination, length );
////    }
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImplAttribute( MethodImplOptions.InternalCall )]
////    private static extern void CopyToNative( Object source, int startIndex, IntPtr destination, int length );
////
////    //====================================================================
////    // Copy blocks from native memory to CLR arrays
////    //====================================================================
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static void Copy( IntPtr source, int[] destination, int startIndex, int length )
////    {
////        CopyToManaged( source, destination, startIndex, length );
////    }
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static void Copy( IntPtr source, char[] destination, int startIndex, int length )
////    {
////        CopyToManaged( source, destination, startIndex, length );
////    }
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static void Copy( IntPtr source, short[] destination, int startIndex, int length )
////    {
////        CopyToManaged( source, destination, startIndex, length );
////    }
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static void Copy( IntPtr source, long[] destination, int startIndex, int length )
////    {
////        CopyToManaged( source, destination, startIndex, length );
////    }
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static void Copy( IntPtr source, float[] destination, int startIndex, int length )
////    {
////        CopyToManaged( source, destination, startIndex, length );
////    }
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static void Copy( IntPtr source, double[] destination, int startIndex, int length )
////    {
////        CopyToManaged( source, destination, startIndex, length );
////    }
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static void Copy( IntPtr source, byte[] destination, int startIndex, int length )
////    {
////        CopyToManaged( source, destination, startIndex, length );
////    }
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static void Copy( IntPtr source, IntPtr[] destination, int startIndex, int length )
////    {
////        CopyToManaged( source, destination, startIndex, length );
////    }
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImplAttribute( MethodImplOptions.InternalCall )]
////    private static extern void CopyToManaged( IntPtr source, Object destination, int startIndex, int length );
////
////    //====================================================================
////    // Read from memory
////    //====================================================================
////    [DllImport( Win32Native.SHIM, EntryPoint = "ND_RU1" )]
////    [ResourceExposure( ResourceScope.None )]
////    public static extern byte ReadByte( [MarshalAs( UnmanagedType.AsAny ), In] Object ptr, int ofs );
////
////    [DllImport( Win32Native.SHIM, EntryPoint = "ND_RU1" )]
////    [ResourceExposure( ResourceScope.None )]
////    public static extern byte ReadByte( IntPtr ptr, int ofs );
////
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static byte ReadByte( IntPtr ptr )
////    {
////        return ReadByte( ptr, 0 );
////    }
////
////    [DllImport( Win32Native.SHIM, EntryPoint = "ND_RI2" )]
////    [ResourceExposure( ResourceScope.None )]
////    public static extern short ReadInt16( [MarshalAs( UnmanagedType.AsAny ), In] Object ptr, int ofs );
////
////    [DllImport( Win32Native.SHIM, EntryPoint = "ND_RI2" )]
////    [ResourceExposure( ResourceScope.None )]
////    public static extern short ReadInt16( IntPtr ptr, int ofs );
////
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static short ReadInt16( IntPtr ptr )
////    {
////        return ReadInt16( ptr, 0 );
////    }
////
////    [DllImport( Win32Native.SHIM, EntryPoint = "ND_RI4" ), ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
////    [ResourceExposure( ResourceScope.None )]
////    public static extern int ReadInt32( [MarshalAs( UnmanagedType.AsAny ), In] Object ptr, int ofs );
////
////    [DllImport( Win32Native.SHIM, EntryPoint = "ND_RI4" ), ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
////    [ResourceExposure( ResourceScope.None )]
////    public static extern int ReadInt32( IntPtr ptr, int ofs );
////
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode ), ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
////    public static int ReadInt32( IntPtr ptr )
////    {
////        return ReadInt32( ptr, 0 );
////    }
////
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode ), ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
////    public static IntPtr ReadIntPtr( [MarshalAs( UnmanagedType.AsAny ), In] Object ptr, int ofs )
////    {
////        return (IntPtr) ReadInt32(ptr, ofs);
////    }
////
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode ), ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
////    public static IntPtr ReadIntPtr( IntPtr ptr, int ofs )
////    {
////        return (IntPtr) ReadInt32(ptr, ofs);
////    }
////
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode ), ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
////    public static IntPtr ReadIntPtr( IntPtr ptr )
////    {
////        return (IntPtr) ReadInt32(ptr, 0);
////    }
////
////    [DllImport( Win32Native.SHIM, EntryPoint = "ND_RI8" ), ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
////    [ResourceExposure( ResourceScope.None )]
////    public static extern long ReadInt64( [MarshalAs( UnmanagedType.AsAny ), In] Object ptr, int ofs );
////
////    [DllImport( Win32Native.SHIM, EntryPoint = "ND_RI8" ), ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
////    [ResourceExposure( ResourceScope.None )]
////    public static extern long ReadInt64( IntPtr ptr, int ofs );
////
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode ), ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
////    public static long ReadInt64( IntPtr ptr )
////    {
////        return ReadInt64( ptr, 0 );
////    }
////
////
////    //====================================================================
////    // Write to memory
////    //====================================================================
////    [DllImport( Win32Native.SHIM, EntryPoint = "ND_WU1" )]
////    [ResourceExposure( ResourceScope.None )]
////    public static extern void WriteByte( IntPtr ptr, int ofs, byte val );
////
////    [DllImport( Win32Native.SHIM, EntryPoint = "ND_WU1" )]
////    [ResourceExposure( ResourceScope.None )]
////    public static extern void WriteByte( [MarshalAs( UnmanagedType.AsAny ), In, Out] Object ptr, int ofs, byte val );
////
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static void WriteByte( IntPtr ptr, byte val )
////    {
////        WriteByte( ptr, 0, val );
////    }
////
////    [DllImport( Win32Native.SHIM, EntryPoint = "ND_WI2" )]
////    [ResourceExposure( ResourceScope.None )]
////    public static extern void WriteInt16( IntPtr ptr, int ofs, short val );
////
////    [DllImport( Win32Native.SHIM, EntryPoint = "ND_WI2" )]
////    [ResourceExposure( ResourceScope.None )]
////    public static extern void WriteInt16( [MarshalAs( UnmanagedType.AsAny ), In, Out] Object ptr, int ofs, short val );
////
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static void WriteInt16( IntPtr ptr, short val )
////    {
////        WriteInt16( ptr, 0, val );
////    }
////
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static void WriteInt16( IntPtr ptr, int ofs, char val )
////    {
////        WriteInt16( ptr, ofs, (short)val );
////    }
////
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static void WriteInt16( [In, Out]Object ptr, int ofs, char val )
////    {
////        WriteInt16( ptr, ofs, (short)val );
////    }
////
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static void WriteInt16( IntPtr ptr, char val )
////    {
////        WriteInt16( ptr, 0, (short)val );
////    }
////
////    [DllImport( Win32Native.SHIM, EntryPoint = "ND_WI4" )]
////    [ResourceExposure( ResourceScope.None )]
////    public static extern void WriteInt32( IntPtr ptr, int ofs, int val );
////
////    [DllImport( Win32Native.SHIM, EntryPoint = "ND_WI4" )]
////    [ResourceExposure( ResourceScope.None )]
////    public static extern void WriteInt32( [MarshalAs( UnmanagedType.AsAny ), In, Out] Object ptr, int ofs, int val );
////
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static void WriteInt32( IntPtr ptr, int val )
////    {
////        WriteInt32( ptr, 0, val );
////    }
////
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static void WriteIntPtr( IntPtr ptr, int ofs, IntPtr val )
////    {
////        WriteInt32(ptr, ofs, (int)val);
////    }
////
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static void WriteIntPtr( [MarshalAs( UnmanagedType.AsAny ), In, Out] Object ptr, int ofs, IntPtr val )
////    {
////        WriteInt32(ptr, ofs, (int)val);
////    }
////
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static void WriteIntPtr( IntPtr ptr, IntPtr val )
////    {
////        WriteInt32(ptr, 0, (int)val);
////    }
////
////    [DllImport( Win32Native.SHIM, EntryPoint = "ND_WI8" )]
////    [ResourceExposure( ResourceScope.None )]
////    public static extern void WriteInt64( IntPtr ptr, int ofs, long val );
////
////    [DllImport( Win32Native.SHIM, EntryPoint = "ND_WI8" )]
////    [ResourceExposure( ResourceScope.None )]
////    public static extern void WriteInt64( [MarshalAs( UnmanagedType.AsAny ), In, Out] Object ptr, int ofs, long val );
////
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static void WriteInt64( IntPtr ptr, long val )
////    {
////        WriteInt64( ptr, 0, val );
////    }
////
////
////    //====================================================================
////    // GetLastWin32Error
////    //====================================================================
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImplAttribute( MethodImplOptions.InternalCall )]
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
////    public static extern int GetLastWin32Error();
////
////
////    //====================================================================
////    // SetLastWin32Error
////    //====================================================================
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImplAttribute( MethodImplOptions.InternalCall )]
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
////    internal static extern void SetLastWin32Error( int error );
////
////
////    //====================================================================
////    // GetHRForLastWin32Error
////    //====================================================================
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
////    public static int GetHRForLastWin32Error()
////    {
////        int dwLastError = GetLastWin32Error();
////        if((dwLastError & 0x80000000) == 0x80000000)
////            return dwLastError;
////        else
////            return (dwLastError & 0x0000FFFF) | unchecked( (int)0x80070000 );
////    }
////
////
////    //====================================================================
////    // Prelink
////    //====================================================================
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static void Prelink( MethodInfo m )
////    {
////        if(m == null)
////            throw new ArgumentNullException( "m" );
////
////        if(!(m is RuntimeMethodInfo))
////            throw new ArgumentException( Environment.GetResourceString( "Argument_MustBeRuntimeMethodInfo" ) );
////
////        RuntimeMethodHandle method = m.MethodHandle;
////
////        InternalPrelink( method.Value );
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImplAttribute( MethodImplOptions.InternalCall )]
////    private static extern void InternalPrelink( IntPtr m );
////
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static void PrelinkAll( Type c )
////    {
////        if(c == null)
////            throw new ArgumentNullException( "c" );
////
////        MethodInfo[] mi = c.GetMethods();
////        if(mi != null)
////        {
////            for(int i = 0; i < mi.Length; i++)
////            {
////                Prelink( mi[i] );
////            }
////        }
////    }
////
////    //====================================================================
////    // NumParamBytes
////    //====================================================================
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static int NumParamBytes( MethodInfo m )
////    {
////        if(m == null)
////            throw new ArgumentNullException( "m" );
////
////        if(!(m is RuntimeMethodInfo))
////            throw new ArgumentException( Environment.GetResourceString( "Argument_MustBeRuntimeMethodInfo" ) );
////
////        RuntimeMethodHandle method = m.GetMethodHandle();
////
////        return InternalNumParamBytes( method.Value );
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImplAttribute( MethodImplOptions.InternalCall )]
////    private static extern int InternalNumParamBytes( IntPtr m );
////
////    //====================================================================
////    // Win32 Exception stuff
////    // These are mostly interesting for Structured exception handling,
////    // but need to be exposed for all exceptions (not just SEHException).
////    //====================================================================
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImplAttribute( MethodImplOptions.InternalCall )]
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static extern /* struct _EXCEPTION_POINTERS* */ IntPtr GetExceptionPointers();
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImplAttribute( MethodImplOptions.InternalCall )]
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static extern int GetExceptionCode();
////
////
////    //====================================================================
////    // Marshals data from a structure class to a native memory block.
////    // If the structure contains pointers to allocated blocks and
////    // "fDeleteOld" is true, this routine will call DestroyStructure() first. 
////    //====================================================================
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImplAttribute( MethodImplOptions.InternalCall ), ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static extern void StructureToPtr( Object structure, IntPtr ptr, bool fDeleteOld );
////
////
////    //====================================================================
////    // Marshals data from a native memory block to a preallocated structure class.
////    //====================================================================
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static void PtrToStructure( IntPtr ptr, Object structure )
////    {
////        PtrToStructureHelper( ptr, structure, false );
////    }
////
////
////    //====================================================================
////    // Creates a new instance of "structuretype" and marshals data from a
////    // native memory block to it.
////    //====================================================================
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static Object PtrToStructure( IntPtr ptr, Type structureType )
////    {
////        if(ptr == Win32Native.NULL) return null;
////
////        if(structureType == null)
////            throw new ArgumentNullException( "structureType" );
////
////        if(structureType.IsGenericType)
////            throw new ArgumentException( Environment.GetResourceString( "Argument_NeedNonGenericType" ), "structureType" );
////
////        Object structure = Activator.InternalCreateInstanceWithNoMemberAccessCheck( structureType, true );
////        PtrToStructureHelper( ptr, structure, true );
////        return structure;
////    }
////
////
////    //====================================================================
////    // Helper function to copy a pointer into a preallocated structure.
////    //====================================================================
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImplAttribute( MethodImplOptions.InternalCall )]
////    private static extern void PtrToStructureHelper( IntPtr ptr, Object structure, bool allowValueClasses );
////
////
////    //====================================================================
////    // Freeds all substructures pointed to by the native memory block.
////    // "structureclass" is used to provide layout information.
////    //====================================================================
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImplAttribute( MethodImplOptions.InternalCall )]
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static extern void DestroyStructure( IntPtr ptr, Type structuretype );
////
////
////    //====================================================================
////    // Returns the HInstance for this module.  Returns -1 if the module 
////    // doesn't have an HInstance.  In Memory (Dynamic) Modules won't have 
////    // an HInstance.
////    //====================================================================
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    [ResourceExposure( ResourceScope.Machine )]
////    [ResourceConsumption( ResourceScope.Machine )]
////    public static IntPtr GetHINSTANCE( Module m )
////    {
////        if(m == null)
////            throw new ArgumentNullException( "m" );
////        return GetHINSTANCE( m.GetNativeHandle() );
////    }
////
////    [ResourceExposure( ResourceScope.Machine )]
////    [DllImport( JitHelpers.QCall, CharSet = CharSet.Unicode ), SuppressUnmanagedCodeSecurity]
////    private extern static IntPtr GetHINSTANCE( ModuleHandle m );
////
////    //====================================================================
////    // Throws a CLR exception based on the HRESULT.
////    //====================================================================
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static void ThrowExceptionForHR( int errorCode )
////    {
////        if(errorCode < 0)
////            ThrowExceptionForHRInternal( errorCode, Win32Native.NULL );
////    }
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static void ThrowExceptionForHR( int errorCode, IntPtr errorInfo )
////    {
////        if(errorCode < 0)
////            ThrowExceptionForHRInternal( errorCode, errorInfo );
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImplAttribute( MethodImplOptions.InternalCall )]
////    internal static extern void ThrowExceptionForHRInternal( int errorCode, IntPtr errorInfo );
////
////
////    //====================================================================
////    // Converts the HRESULT to a CLR exception.
////    //====================================================================
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static Exception GetExceptionForHR( int errorCode )
////    {
////        if(errorCode < 0)
////            return GetExceptionForHRInternal( errorCode, Win32Native.NULL );
////        else
////            return null;
////    }
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static Exception GetExceptionForHR( int errorCode, IntPtr errorInfo )
////    {
////        if(errorCode < 0)
////            return GetExceptionForHRInternal( errorCode, errorInfo );
////        else
////            return null;
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImplAttribute( MethodImplOptions.InternalCall )]
////    internal static extern Exception GetExceptionForHRInternal( int errorCode, IntPtr errorInfo );
////
////
////    //====================================================================
////    // Converts the CLR exception to an HRESULT. This function also sets
////    // up an IErrorInfo for the exception.
////    //====================================================================
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImplAttribute( MethodImplOptions.InternalCall )]
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static extern int GetHRForException( Exception e );
////
////    //====================================================================
////    // This method is intended for compiler code generators rather
////    // than applications. 
////    //====================================================================
////    // REVIEW: This is not in the obsolete list to be removed for Whidbey!
////    [ObsoleteAttribute( "The GetUnmanagedThunkForManagedMethodPtr method has been deprecated and will be removed in a future release.", false )]
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImplAttribute( MethodImplOptions.InternalCall )]
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static extern IntPtr GetUnmanagedThunkForManagedMethodPtr( IntPtr pfnMethodToWrap, IntPtr pbSignature, int cbSignature );
////
////    //====================================================================
////    // This method is intended for compiler code generators rather
////    // than applications. 
////    //====================================================================
////    // REVIEW: This is not in the obsolete list to be removed for Whidbey!
////    [ObsoleteAttribute( "The GetManagedThunkForUnmanagedMethodPtr method has been deprecated and will be removed in a future release.", false )]
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImplAttribute( MethodImplOptions.InternalCall )]
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static extern IntPtr GetManagedThunkForUnmanagedMethodPtr( IntPtr pfnMethodToWrap, IntPtr pbSignature, int cbSignature );
////
////    //====================================================================
////    // The hosting APIs allow a sophisticated host to schedule fibers
////    // onto OS threads, so long as they notify the runtime of this
////    // activity.  A fiber cookie can be redeemed for its managed Thread
////    // object by calling the following service.
////    //====================================================================
////    // REVIEW: This is not in the obsolete list to be removed for Whidbey!
////    [ObsoleteAttribute( "The GetThreadFromFiberCookie method has been deprecated.  Use the hosting API to perform this operation.", false )]
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static Thread GetThreadFromFiberCookie( int cookie )
////    {
////        if(cookie == 0)
////            throw new ArgumentException( Environment.GetResourceString( "Argument_ArgumentZero" ), "cookie" );
////
////        return InternalGetThreadFromFiberCookie( cookie );
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImplAttribute( MethodImplOptions.InternalCall )]
////    private static extern Thread InternalGetThreadFromFiberCookie( int cookie );
////
////
////    //====================================================================
////    // Memory allocation and dealocation.
////    //====================================================================
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
////    public static IntPtr AllocHGlobal( IntPtr cb )
////    {
////        IntPtr pNewMem = Win32Native.LocalAlloc_NoSafeHandle( LMEM_FIXED, cb );
////
////        if(pNewMem == Win32Native.NULL)
////        {
////            throw new OutOfMemoryException();
////        }
////        return pNewMem;
////    }
////
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
////    public static IntPtr AllocHGlobal( int cb )
////    {
////        return AllocHGlobal( (IntPtr)cb );
////    }
////
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
////    public static void FreeHGlobal( IntPtr hglobal )
////    {
////        if(IsNotWin32Atom( hglobal ))
////        {
////            if(Win32Native.NULL != Win32Native.LocalFree( hglobal ))
////            {
////                ThrowExceptionForHR( GetHRForLastWin32Error() );
////            }
////        }
////    }
////
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static IntPtr ReAllocHGlobal( IntPtr pv, IntPtr cb )
////    {
////        IntPtr pNewMem = Win32Native.LocalReAlloc( pv, cb, LMEM_MOVEABLE );
////        if(pNewMem == Win32Native.NULL)
////        {
////            throw new OutOfMemoryException();
////        }
////        return pNewMem;
////    }
////
////
////    //====================================================================
////    // String convertions.
////    //====================================================================          
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static IntPtr StringToHGlobalAnsi( String s )
////    {
////        if(s == null)
////        {
////            return Win32Native.NULL;
////        }
////        else
////        {
////            int nb = (s.Length + 1) * SystemMaxDBCSCharSize;
////
////            // Overflow checking
////            if(nb < s.Length)
////                throw new ArgumentOutOfRangeException( "s" );
////
////            IntPtr len = new IntPtr( nb );
////            IntPtr hglobal = Win32Native.LocalAlloc_NoSafeHandle( LMEM_FIXED, len );
////
////            if(hglobal == Win32Native.NULL)
////            {
////                throw new OutOfMemoryException();
////            }
////            else
////            {
////                Win32Native.CopyMemoryAnsi( hglobal, s, len );
////                return hglobal;
////            }
////        }
////    }
////
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static IntPtr StringToCoTaskMemAnsi( String s )
////    {
////        if(s == null)
////        {
////            return Win32Native.NULL;
////        }
////        else
////        {
////            int nb = (s.Length + 1) * SystemMaxDBCSCharSize;
////
////            // Overflow checking
////            if(nb < s.Length)
////                throw new ArgumentOutOfRangeException( "s" );
////
////            IntPtr hglobal = Win32Native.CoTaskMemAlloc( nb );
////
////            if(hglobal == Win32Native.NULL)
////            {
////                throw new OutOfMemoryException();
////            }
////            else
////            {
////                Win32Native.CopyMemoryAnsi( hglobal, s, new IntPtr( nb ) );
////                return hglobal;
////            }
////        }
////    }
////
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static IntPtr StringToHGlobalUni( String s )
////    {
////        if(s == null)
////        {
////            return Win32Native.NULL;
////        }
////        else
////        {
////            int nb = (s.Length + 1) * 2;
////
////            // Overflow checking
////            if(nb < s.Length)
////                throw new ArgumentOutOfRangeException( "s" );
////
////            IntPtr len = new IntPtr( nb );
////            IntPtr hglobal = Win32Native.LocalAlloc_NoSafeHandle( LMEM_FIXED, len );
////
////            if(hglobal == Win32Native.NULL)
////            {
////                throw new OutOfMemoryException();
////            }
////            else
////            {
////                Win32Native.CopyMemoryUni( hglobal, s, len );
////                return hglobal;
////            }
////        }
////    }
////
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static IntPtr StringToHGlobalAuto( String s )
////    {
////        return (SystemDefaultCharSize == 1) ? StringToHGlobalAnsi( s ) : StringToHGlobalUni( s );
////    }
////
////
////    //====================================================================
////    // check if the object is classic COM component
////    //====================================================================
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static bool IsComObject( Object o )
////    {
////        return false;
////    }
////
////
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static Delegate GetDelegateForFunctionPointer( IntPtr ptr, Type t )
////    {
////        // Validate the parameters
////        if(ptr == IntPtr.Zero)
////            throw new ArgumentNullException( "ptr" );
////
////        if(t == null)
////            throw new ArgumentNullException( "t" );
////
////        if((t as RuntimeType) == null)
////            throw new ArgumentException( Environment.GetResourceString( "Argument_MustBeRuntimeType" ), "t" );
////
////        if(t.IsGenericType)
////            throw new ArgumentException( Environment.GetResourceString( "Argument_NeedNonGenericType" ), "t" );
////
////        Type c = t.BaseType;
////        if(c == null || (c != typeof( Delegate ) && c != typeof( MulticastDelegate )))
////            throw new ArgumentException( Environment.GetResourceString( "Arg_MustBeDelegate" ), "t" );
////
////        return GetDelegateForFunctionPointerInternal( ptr, t );
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImplAttribute( MethodImplOptions.InternalCall )]
////    internal static extern Delegate GetDelegateForFunctionPointerInternal( IntPtr ptr, Type t );
////
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static IntPtr GetFunctionPointerForDelegate( Delegate d )
////    {
////        if(d == null)
////            throw new ArgumentNullException( "d" );
////
////        return GetFunctionPointerForDelegateInternal( d );
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImplAttribute( MethodImplOptions.InternalCall )]
////    internal static extern IntPtr GetFunctionPointerForDelegateInternal( Delegate d );
////
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static IntPtr SecureStringToBSTR( SecureString s )
////    {
////        if(s == null)
////        {
////            throw new ArgumentNullException( "s" );
////        }
////
////        return s.ToBSTR();
////    }
////
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static IntPtr SecureStringToCoTaskMemAnsi( SecureString s )
////    {
////        if(s == null)
////        {
////            throw new ArgumentNullException( "s" );
////        }
////
////        return s.ToAnsiStr( false );
////    }
////
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static IntPtr SecureStringToGlobalAllocAnsi( SecureString s )
////    {
////        if(s == null)
////        {
////            throw new ArgumentNullException( "s" );
////        }
////
////        return s.ToAnsiStr( true );
////    }
////
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static IntPtr SecureStringToCoTaskMemUnicode( SecureString s )
////    {
////        if(s == null)
////        {
////            throw new ArgumentNullException( "s" );
////        }
////
////        return s.ToUniStr( false );
////    }
////
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static IntPtr SecureStringToGlobalAllocUnicode( SecureString s )
////    {
////        if(s == null)
////        {
////            throw new ArgumentNullException( "s" );
////        }
////
////        return s.ToUniStr( true );
////    }
////
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static void ZeroFreeBSTR( IntPtr s )
////    {
////        Win32Native.ZeroMemory( s, (uint)(Win32Native.SysStringLen( s ) * 2) );
////        FreeBSTR( s );
////    }
////
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static void ZeroFreeCoTaskMemAnsi( IntPtr s )
////    {
////        Win32Native.ZeroMemory( s, (uint)(Win32Native.lstrlenA( s )) );
////        FreeCoTaskMem( s );
////    }
////
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static void ZeroFreeGlobalAllocAnsi( IntPtr s )
////    {
////        Win32Native.ZeroMemory( s, (uint)(Win32Native.lstrlenA( s )) );
////        FreeHGlobal( s );
////    }
////
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static void ZeroFreeCoTaskMemUnicode( IntPtr s )
////    {
////        Win32Native.ZeroMemory( s, (uint)(Win32Native.lstrlenW( s ) * 2) );
////        FreeCoTaskMem( s );
////    }
////
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static void ZeroFreeGlobalAllocUnicode( IntPtr s )
////    {
////        Win32Native.ZeroMemory( s, (uint)(Win32Native.lstrlenW( s ) * 2) );
////        FreeHGlobal( s );
////    }
    }
}
