// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class:  __Error
**
**
** Purpose: Centralized error methods for the IO package.  
** Mostly useful for translating Win32 HRESULTs into meaningful
** error strings & exceptions.
**
**
===========================================================*/

using System;
using System.Runtime.InteropServices;
//using Win32Native = Microsoft.Win32.Win32Native;
using System.Text;
//using System.Globalization;
//using System.Security;
//using System.Security.Permissions;

namespace System.IO
{
    // Only static data no need to serialize
    internal static class __Error
    {
        internal static void EndOfFile()
        {
#if EXCEPTION_STRINGS
            throw new EndOfStreamException( Environment.GetResourceString( "IO.EOF_ReadBeyondEOF" ) );
#else
            throw new EndOfStreamException();
#endif
        }

        internal static void FileNotOpen()
        {
#if EXCEPTION_STRINGS
            throw new ObjectDisposedException( null, Environment.GetResourceString( "ObjectDisposed_FileClosed" ) );
#else
            throw new ObjectDisposedException( null );
#endif
        }

        internal static void StreamIsClosed()
        {
#if EXCEPTION_STRINGS
            throw new ObjectDisposedException( null, Environment.GetResourceString( "ObjectDisposed_StreamClosed" ) );
#else
            throw new ObjectDisposedException( null );
#endif
        }

        internal static void MemoryStreamNotExpandable()
        {
#if EXCEPTION_STRINGS
            throw new NotSupportedException( Environment.GetResourceString( "NotSupported_MemStreamNotExpandable" ) );
#else
            throw new NotSupportedException();
#endif
        }

        internal static void ReaderClosed()
        {
#if EXCEPTION_STRINGS
            throw new ObjectDisposedException( null, Environment.GetResourceString( "ObjectDisposed_ReaderClosed" ) );
#else
            throw new ObjectDisposedException( null );
#endif
        }

        internal static void ReadNotSupported()
        {
#if EXCEPTION_STRINGS
            throw new NotSupportedException( Environment.GetResourceString( "NotSupported_UnreadableStream" ) );
#else
            throw new NotSupportedException();
#endif
        }

        internal static void SeekNotSupported()
        {
#if EXCEPTION_STRINGS
            throw new NotSupportedException( Environment.GetResourceString( "NotSupported_UnseekableStream" ) );
#else
            throw new NotSupportedException();
#endif
        }

        internal static void WrongAsyncResult()
        {
#if EXCEPTION_STRINGS
            throw new ArgumentException( Environment.GetResourceString( "Arg_WrongAsyncResult" ) );
#else
            throw new ArgumentException();
#endif
        }

        internal static void EndReadCalledTwice()
        {
            // Should ideally be InvalidOperationExc but we can't maitain parity with Stream and FileStream without some work
#if EXCEPTION_STRINGS
            throw new ArgumentException( Environment.GetResourceString( "InvalidOperation_EndReadCalledMultiple" ) );
#else
            throw new ArgumentException();
#endif
        }

        internal static void EndWriteCalledTwice()
        {
            // Should ideally be InvalidOperationExc but we can't maintain parity with Stream and FileStream without some work
#if EXCEPTION_STRINGS
            throw new ArgumentException( Environment.GetResourceString( "InvalidOperation_EndWriteCalledMultiple" ) );
#else
            throw new ArgumentException();
#endif
        }

////    // Given a possible fully qualified path, ensure that we have path
////    // discovery permission to that path.  If we do not, return just the 
////    // file name.  If we know it is a directory, then don't return the 
////    // directory name.
////    internal static String GetDisplayablePath( String path, bool isInvalidPath )
////    {
////        if(String.IsNullOrEmpty( path ))
////            return path;
////
////        // Is it a fully qualified path?
////        bool isFullyQualified = false;
////        if(path.Length < 2)
////            return path;
////        if(Path.IsDirectorySeparator( path[0] ) && Path.IsDirectorySeparator( path[1] ))
////            isFullyQualified = true;
////        else if(path[1] == Path.VolumeSeparatorChar)
////        {
////            isFullyQualified = true;
////        }
////
////        if(!isFullyQualified && !isInvalidPath)
////            return path;
////
////        bool safeToReturn = false;
////        try
////        {
////            if(!isInvalidPath)
////            {
////                new FileIOPermission( FileIOPermissionAccess.PathDiscovery, new String[] { path }, false, false ).Demand();
////                safeToReturn = true;
////            }
////        }
////        catch(SecurityException)
////        {
////        }
////        catch(ArgumentException)
////        {
////            // ? and * characters cause ArgumentException to be thrown from HasIllegalCharacters
////            // inside FileIOPermission.AddPathList
////        }
////        catch(NotSupportedException)
////        {
////            // paths like "!Bogus\\dir:with/junk_.in it" can cause NotSupportedException to be thrown
////            // from Security.Util.StringExpressionSet.CanonicalizePath when ':' is found in the path
////            // beyond string index position 1.  
////        }
////
////        if(!safeToReturn)
////        {
////            if(Path.IsDirectorySeparator( path[path.Length - 1] ))
////                path = Environment.GetResourceString( "IO.IO_NoPermissionToDirectoryName" );
////            else
////                path = Path.GetFileName( path );
////        }
////
////        return path;
////    }
////
////    internal static void WinIOError()
////    {
////        int errorCode = Marshal.GetLastWin32Error();
////        WinIOError( errorCode, String.Empty );
////    }
////
////    // After calling GetLastWin32Error(), it clears the last error field,
////    // so you must save the HResult and pass it to this method.  This method
////    // will determine the appropriate exception to throw dependent on your 
////    // error, and depending on the error, insert a string into the message 
////    // gotten from the ResourceManager.
////    internal static void WinIOError( int errorCode, String maybeFullPath )
////    {
////        // This doesn't have to be perfect, but is a perf optimization.
////        bool isInvalidPath = errorCode == Win32Native.ERROR_INVALID_NAME || errorCode == Win32Native.ERROR_BAD_PATHNAME;
////        String str = GetDisplayablePath( maybeFullPath, isInvalidPath );
////
////        switch(errorCode)
////        {
////            case Win32Native.ERROR_FILE_NOT_FOUND:
////                if(str.Length == 0)
////                    throw new FileNotFoundException( Environment.GetResourceString( "IO.FileNotFound" ) );
////                else
////                    throw new FileNotFoundException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "IO.FileNotFound_FileName" ), str ), str );
////
////            case Win32Native.ERROR_PATH_NOT_FOUND:
////                if(str.Length == 0)
////                    throw new DirectoryNotFoundException( Environment.GetResourceString( "IO.PathNotFound_NoPathName" ) );
////                else
////                    throw new DirectoryNotFoundException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "IO.PathNotFound_Path" ), str ) );
////
////            case Win32Native.ERROR_ACCESS_DENIED:
////                if(str.Length == 0)
////                    throw new UnauthorizedAccessException( Environment.GetResourceString( "UnauthorizedAccess_IODenied_NoPathName" ) );
////                else
////                    throw new UnauthorizedAccessException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "UnauthorizedAccess_IODenied_Path" ), str ) );
////
////            case Win32Native.ERROR_ALREADY_EXISTS:
////                if(str.Length == 0)
////                    goto default;
////                throw new IOException( Environment.GetResourceString( "IO.IO_AlreadyExists_Name", str ), Win32Native.MakeHRFromErrorCode( errorCode ), maybeFullPath );
////
////            case Win32Native.ERROR_FILENAME_EXCED_RANGE:
////                throw new PathTooLongException( Environment.GetResourceString( "IO.PathTooLong" ) );
////
////            case Win32Native.ERROR_INVALID_DRIVE:
////                throw new DriveNotFoundException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "IO.DriveNotFound_Drive" ), str ) );
////
////            case Win32Native.ERROR_INVALID_PARAMETER:
////                throw new IOException( Win32Native.GetMessage( errorCode ), Win32Native.MakeHRFromErrorCode( errorCode ), maybeFullPath );
////
////            case Win32Native.ERROR_SHARING_VIOLATION:
////                if(str.Length == 0)
////                    throw new IOException( Environment.GetResourceString( "IO.IO_SharingViolation_NoFileName" ), Win32Native.MakeHRFromErrorCode( errorCode ), maybeFullPath );
////                else
////                    throw new IOException( Environment.GetResourceString( "IO.IO_SharingViolation_File", str ), Win32Native.MakeHRFromErrorCode( errorCode ), maybeFullPath );
////
////            case Win32Native.ERROR_FILE_EXISTS:
////                if(str.Length == 0)
////                    goto default;
////                throw new IOException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "IO.IO_FileExists_Name" ), str ), Win32Native.MakeHRFromErrorCode( errorCode ), maybeFullPath );
////
////            case Win32Native.ERROR_OPERATION_ABORTED:
////                throw new OperationCanceledException();
////
////            default:
////                throw new IOException( Win32Native.GetMessage( errorCode ), Win32Native.MakeHRFromErrorCode( errorCode ), maybeFullPath );
////        }
////    }
////
////    // An alternative to WinIOError with friendlier messages for drives
////    internal static void WinIODriveError( String driveName )
////    {
////        int errorCode = Marshal.GetLastWin32Error();
////        WinIODriveError( driveName, errorCode );
////    }
////
////    internal static void WinIODriveError( String driveName, int errorCode )
////    {
////        switch(errorCode)
////        {
////            case Win32Native.ERROR_PATH_NOT_FOUND:
////            case Win32Native.ERROR_INVALID_DRIVE:
////                throw new DriveNotFoundException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "IO.DriveNotFound_Drive" ), driveName ) );
////
////            default:
////                WinIOError( errorCode, driveName );
////                break;
////        }
////    }

        internal static void WriteNotSupported()
        {
#if EXCEPTION_STRINGS
            throw new NotSupportedException( Environment.GetResourceString( "NotSupported_UnwritableStream" ) );
#else
            throw new NotSupportedException();
#endif
        }

        internal static void WriterClosed()
        {
#if EXCEPTION_STRINGS
            throw new ObjectDisposedException( null, Environment.GetResourceString( "ObjectDisposed_WriterClosed" ) );
#else
            throw new ObjectDisposedException( null );
#endif
        }

////    // From WinError.h
////    internal const int ERROR_FILE_NOT_FOUND = Win32Native.ERROR_FILE_NOT_FOUND;
////    internal const int ERROR_PATH_NOT_FOUND = Win32Native.ERROR_PATH_NOT_FOUND;
////    internal const int ERROR_ACCESS_DENIED = Win32Native.ERROR_ACCESS_DENIED;
////    internal const int ERROR_INVALID_PARAMETER = Win32Native.ERROR_INVALID_PARAMETER;
    }
}
