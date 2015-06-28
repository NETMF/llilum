// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==

namespace Microsoft.Zelig.Runtime
{
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;

    using RT = Microsoft.Zelig.Runtime;
    using TS = Microsoft.Zelig.Runtime.TypeSystem;
    using System.Runtime.InteropServices;


    [ExtendClass( typeof( System.IO.NativeFileStream ))]
    internal class NativeFileStreamImpl
    {
        uint m_fileHandle;
        uint m_volumeHandle;

        internal static string[] SplitVolumeFromPath( string path )
        {
            string[] values = new string[2];

            path = path.TrimStart( '\\' );

            if(path.Length == 0) 
#if EXCEPTION_STRINGS
                throw new IOException( "InvalidPath" );
#else
                throw new IOException();
#endif

            int idx = path.IndexOf( '\\' );

            if(idx == -1)
            {
                idx = path.Length;
                values[1] = "\\";
            }
            else
            {
                // include the '\\'
                values[1] = path.Substring( idx, path.Length - idx );
            }

            values[0] = path.Substring( 0, idx );

            return values;
        }

        [DiscardTargetImplementation]
        internal NativeFileStreamImpl( string path, int bufferSize )
        {
            if(bufferSize < 0) throw new ArgumentException();

            string[] paths = SplitVolumeFromPath( path );

            m_volumeHandle = FileSystemVolumeList.FindVolume( paths[0], paths[0].Length );

            HRESULT hr = FileSystemVolume.Open( m_volumeHandle, paths[1], ref m_fileHandle );

            HR.ThrowOnFailure( hr );
        }

        //[RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_NativeFileLibrary )]
        public int Read( byte[] buf, int offset, int count, int timeout )
        {
            int bytesRead = 0;

            if(buf == null                ) throw new ArgumentNullException();
            if(count < 0 || offset < 0    ) throw new ArgumentOutOfRangeException(); 
            if(offset + count > buf.Length) throw new ArgumentException();

            HRESULT hr = FileSystemVolume.Read( m_volumeHandle, m_fileHandle, buf, offset, count, ref bytesRead );

            HR.ThrowOnFailure( hr );

            return bytesRead;
        }

        //[RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_NativeFileLibrary )]
        public int Write( byte[] buf, int offset, int count, int timeout )
        {
            int bytesWritten = 0;

            if(buf == null                ) throw new ArgumentNullException();
            if(count < 0 || offset < 0    ) throw new ArgumentOutOfRangeException(); 
            if(offset + count > buf.Length) throw new ArgumentException();

            HRESULT hr = FileSystemVolume.Write( m_volumeHandle, m_fileHandle, buf, offset, count, ref bytesWritten );

            HR.ThrowOnFailure( hr );

            return bytesWritten;
        }

        //[RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_NativeFileLibrary )]
        public long Seek( long offset, uint origin )
        {
            long pos = 0;

            HRESULT hr = FileSystemVolume.Seek( m_volumeHandle, m_fileHandle, offset, origin, ref pos );

            HR.ThrowOnFailure( hr );

            return pos;
        }

        //[RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_NativeFileLibrary )]
        public void Flush()
        {
            HRESULT hr = FileSystemVolume.Flush( m_volumeHandle, m_fileHandle );

            HR.ThrowOnFailure( hr );
        }

        //[RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_NativeFileLibrary )]
        public long GetLength()
        {
            long len = 0;

            HRESULT hr = FileSystemVolume.GetLength( m_volumeHandle, m_fileHandle, ref len );

            HR.ThrowOnFailure( hr );

            return len;
        }

        //[RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_NativeFileLibrary )]
        public void SetLength( long length )
        {
            if(length < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            HRESULT hr = FileSystemVolume.SetLength( m_volumeHandle, m_fileHandle, length );

            HR.ThrowOnFailure( hr );
        }

        //[RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_NativeFileLibrary )]
        public void GetStreamProperties( out bool canRead, out bool canWrite, out bool canSeek )
        {
            StreamDriverDetails sdd = new StreamDriverDetails();

            HRESULT hr = FileSystemVolume.GetDriverDetails( m_volumeHandle, m_fileHandle, ref sdd );

            HR.ThrowOnFailure( hr );

            canRead  = sdd.canRead  != 0;
            canWrite = sdd.canWrite != 0;
            canSeek  = sdd.canSeek  != 0;
        }

        //[RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_NativeFileLibrary )]
        public void Close()
        {
            HRESULT hr = FileSystemVolume.Close( m_volumeHandle, m_fileHandle );

            HR.ThrowOnFailure( hr );
        }
    }


    [ExtendClass( typeof( System.IO.NativeFindFile ), NoConstructors=false)]
    internal class NativeFindFileImpl
    {
        uint m_fileFindHandle;
        uint m_volumeHandle;

        [DiscardTargetImplementation]
        internal NativeFindFileImpl( string path, string searchPattern )
        {
            string[] paths = NativeFileStreamImpl.SplitVolumeFromPath( path );

            m_volumeHandle = FileSystemVolumeList.FindVolume( paths[0], paths[0].Length );

            HRESULT hr = FileSystemVolume.FindOpen( m_volumeHandle, paths[1], ref m_fileFindHandle );

            HR.ThrowOnFailure( hr );
        }

        //[RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_NativeFileFindLibrary )]
        public NativeFileInfo GetNext()
        {
            FileInfo findData = new FileInfo();
            NativeFileInfo retVal = new NativeFileInfo();
            char[] fileName = new char[256];
            int      found    = 0;
            int      fileNameLen = fileName.Length;

            HRESULT hr = FileSystemVolume.FindNext( m_volumeHandle, m_fileFindHandle, ref findData, fileName, ref fileNameLen, ref found );

            HR.ThrowOnFailure( hr );

            if(found != 0)
            {
                retVal.Attributes       = findData.Attributes;
                retVal.CreationTime     = findData.CreationTime;
                retVal.FileName         = new string( fileName, 0, fileNameLen );
                retVal.LastAccessTime   = findData.LastAccessTime;
                retVal.LastWriteTime    = findData.LastWriteTime;
                retVal.Size             = findData.Size;

                return retVal;
            }

            return null;
        }

        //[RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_NativeFileFindLibrary )]
        public void Close()
        {
            HRESULT hr = FileSystemVolume.FindClose( m_volumeHandle, m_fileFindHandle );

            HR.ThrowOnFailure( hr );
        }

        //[RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_NativeFileFindLibrary )]
        public static NativeFileInfo GetFileInfo( String path )
        {
            FileInfo fileInfo     = new FileInfo();
            int      found        = 0;
            string[] paths        = NativeFileStreamImpl.SplitVolumeFromPath( path );
            uint     volumeHandle = FileSystemVolumeList.FindVolume( paths[0], paths[0].Length );

            HRESULT hr = FileSystemVolume.GetFileInfo( volumeHandle, paths[1], ref fileInfo, ref found );

            HR.ThrowOnFailure( hr );

            if(found != 0)
            {
                NativeFileInfo nfi  = new NativeFileInfo();
                nfi.FileName        = path;
                nfi.Attributes      = fileInfo.Attributes;
                nfi.CreationTime    = fileInfo.CreationTime;
                nfi.LastAccessTime  = fileInfo.LastAccessTime;
                nfi.LastWriteTime   = fileInfo.LastWriteTime;
                nfi.Size            = fileInfo.Size;

                return nfi;
            }

            return null;
        }

    }

    [ExtendClass( typeof( System.IO.NativeIO ))]
    internal static class NativeIOImpl
    {
        //[RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_NativeIOLibrary )]
        internal static void Format( String nameSpace, String fileSystem, String volumeLabel, uint parameter )
        {
            uint volHandle = FileSystemVolumeList.FindVolume( nameSpace, nameSpace.Length );

            HR.ThrowOnFailure( FileSystemVolume.Format( volHandle, volumeLabel, parameter ) );
        }

        //[RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_NativeIOLibrary )]
        internal static void Delete( string path )
        {
            string[] paths = NativeFileStreamImpl.SplitVolumeFromPath( path );

            uint volHandle = FileSystemVolumeList.FindVolume( paths[0], paths[0].Length );

            HR.ThrowOnFailure( FileSystemVolume.Delete( volHandle, paths[1] ) );
        }

        //[RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_NativeIOLibrary )]
        internal static bool Move( string sourceFileName, string destFileName )
        {
            string []pathsSrc = NativeFileStreamImpl.SplitVolumeFromPath( sourceFileName );
            string []pathsDst = NativeFileStreamImpl.SplitVolumeFromPath( destFileName   );

            string volSrc = pathsSrc[0];

            if(volSrc != pathsDst[0]) 
#if EXCEPTION_STRINGS
                throw new InvalidOperationException( "Cannot move between volumes" );
#else
                throw new InvalidOperationException();
#endif

            uint volHandle = FileSystemVolumeList.FindVolume( volSrc, volSrc.Length );

            return HR.Succeeded( FileSystemVolume.Move( volHandle, pathsSrc[1], pathsDst[1] ) );
        }

        //[RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_NativeIOLibrary )]
        internal static void CreateDirectory( string path )
        {
            string[] paths = NativeFileStreamImpl.SplitVolumeFromPath( path );

            uint volHandle = FileSystemVolumeList.FindVolume( paths[0], paths[0].Length );

            HR.ThrowOnFailure( FileSystemVolume.CreateDirectory( volHandle, paths[1] ) );
        }

        //[RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_NativeIOLibrary )]
        internal static uint GetAttributes( string path )
        {
            uint attribs = 0;

            string[] paths = NativeFileStreamImpl.SplitVolumeFromPath( path );

            uint volHandle = FileSystemVolumeList.FindVolume( paths[0], paths[0].Length );

            HR.ThrowOnFailure( FileSystemVolume.GetAttributes( volHandle, paths[1], ref attribs ) );

            return attribs;
        }

        //[RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_NativeIOLibrary )]
        internal static void SetAttributes( string path, uint attributes )
        {
            string[] paths = NativeFileStreamImpl.SplitVolumeFromPath( path );

            uint volHandle = FileSystemVolumeList.FindVolume( paths[0], paths[0].Length );

            HR.ThrowOnFailure( FileSystemVolume.SetAttributes( volHandle, paths[1], attributes ) );
        }
    }


    internal enum HRESULT : int
    {
        SUCCEED = 0,
    }

    internal enum HR_Faculty : ushort
    {
        IO                  = 0x4f00,
        FileIO              = 0x6000,
        InvalidDriver       = 0x6100,
        FileNotFound        = 0x6200,
        DirectoryNotFound   = 0x6300,
        VolumeNotFound      = 0x6400,
        PathTooLong         = 0x6500,
        DirectoryNotEmpty   = 0x6600,
        UnauthorizedAccess  = 0x6700,
        PathAlreadyExists   = 0x6800,
        TooManyOpenHandles  = 0x6900,

        InvalidParameter    = 0x7d00,
        ArgumentNull        = 0x4e00,
        OutOfRange          = 0x2500,
        NullReference       = 0x2100,
        NotImplemented      = 0x4a00,
    }

    internal class HR
    {
        internal static bool Succeeded( HRESULT hr )
        {
            return hr >= 0;
        }

        internal static bool Failed( HRESULT hr )
        {
            return hr < 0;
        }

        internal static uint GetCode( HRESULT hr )
        {
            return (uint)hr & 0xFFFF;
        }

        internal static HR_Faculty GetFacility( HRESULT hr )
        {
            return (HR_Faculty)(((uint)hr >> 16) & 0x7FFF);
        }

        internal static void ThrowOnFailure( HRESULT hr )
        {
            if((int)hr < 0)
            {
                switch(HR.GetFacility( hr ))
                {
                    case HR_Faculty.NotImplemented:
                        throw new NotImplementedException();

                    case HR_Faculty.OutOfRange:
                        throw new ArgumentOutOfRangeException();

                    case HR_Faculty.NullReference:
                    case HR_Faculty.ArgumentNull:
                        throw new ArgumentNullException();

                    case HR_Faculty.InvalidParameter:
                        throw new ArgumentException();

                    case HR_Faculty.IO:
                    case HR_Faculty.FileIO:
                    case HR_Faculty.InvalidDriver:
                    case HR_Faculty.FileNotFound:
                    case HR_Faculty.DirectoryNotFound:
                    case HR_Faculty.VolumeNotFound:
                    case HR_Faculty.PathTooLong:
                    case HR_Faculty.DirectoryNotEmpty:
                    case HR_Faculty.UnauthorizedAccess:
                    case HR_Faculty.PathAlreadyExists:
                    case HR_Faculty.TooManyOpenHandles:
                    default:
#if EXCEPTION_STRINGS
                        throw new IOException( "HRESULT: " + ( (uint)hr ).ToString() );
#else
                        throw new IOException();
#endif
                }
            }
        }
    }

    [StructLayout( LayoutKind.Explicit )]
    internal struct FileInfo
    {
        [FieldOffset( 0 )]
        internal uint Attributes;
        [FieldOffset( 8 )]
        internal Int64 CreationTime;
        [FieldOffset( 16 )]
        internal Int64 LastAccessTime;
        [FieldOffset( 24 )]
        internal Int64 LastWriteTime;
        [FieldOffset( 32 )]
        internal Int64 Size;
    };


    [StructLayout( LayoutKind.Explicit )]
    public struct StreamDriverDetails
    {
        [FieldOffset(  0 )]
        public int BufferingStrategy;
        [FieldOffset(  4 )]
        public IntPtr inputBuffer;
        [FieldOffset(  8 )]
        public IntPtr outputBuffer;
        [FieldOffset( 12 )]
        public int inputBufferSize;
        [FieldOffset( 16 )]
        public int outputBufferSize;
        [FieldOffset( 20 )]
        public int canRead;
        [FieldOffset( 24 )]
        public int canWrite;
        [FieldOffset( 28 )]
        public int canSeek;
        [FieldOffset( 32 )]
        public int readTimeout;
        [FieldOffset( 36 )]
        public int writeTimeout;
    }


    internal class FileSystemVolume
    {
        const string c_FileSystemVolumeLibrary = @"BlockStorageFileSystem.obj";

        //[RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_FileSystemVolumeLibrary )]
        //internal extern static bool    InitializeVolume  ( uint volumeHandle );
        //[RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_FileSystemVolumeLibrary )]
        //internal extern static bool    UninitializeVolume( uint volumeHandle );
        [RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_FileSystemVolumeLibrary )]
        internal extern static HRESULT Format            ( uint volumeHandle, string volumeLabel, uint parameters );
        //[RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_FileSystemVolumeLibrary )]
        //internal extern static HRESULT GetSizeInfo       ( uint volumeHandle, ref Int64 totalSize, ref Int64 totalFreeSpace );
        //[RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_FileSystemVolumeLibrary )]
        //internal extern static HRESULT FlushAll          ( uint volumeHandle );
        [RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_FileSystemVolumeLibrary )]
        internal extern static HRESULT FindOpen          ( uint volumeHandle, string path, ref uint findHandle );
        [RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_FileSystemVolumeLibrary )]
        internal extern static HRESULT FindNext          ( uint volumeHandle, uint findHandle, ref FileInfo findData, char[] fileName, ref int fileNameLen, ref int found );
        [RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_FileSystemVolumeLibrary )]
        internal extern static HRESULT FindClose         ( uint volumeHandle, uint findHandle );
        [RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_FileSystemVolumeLibrary )]
        internal extern static HRESULT GetFileInfo       ( uint volumeHandle, string path, ref FileInfo fileInfo, ref int found );
        [RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_FileSystemVolumeLibrary )]
        internal extern static HRESULT CreateDirectory   ( uint volumeHandle, string path );
        [RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_FileSystemVolumeLibrary )]
        internal extern static HRESULT Move              ( uint volumeHandle, string oldPath, string newPath );
        [RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_FileSystemVolumeLibrary )]
        internal extern static HRESULT Delete            ( uint volumeHandle, string path );
        [RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_FileSystemVolumeLibrary )]
        internal extern static HRESULT GetAttributes     ( uint volumeHandle, string path, ref uint attributes );
        [RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_FileSystemVolumeLibrary )]
        internal extern static HRESULT SetAttributes     ( uint volumeHandle, string path, uint attributes );
        [RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_FileSystemVolumeLibrary )]
        internal extern static HRESULT GetDriverDetails  ( uint volumeHandle, uint handle, ref StreamDriverDetails details );
        [RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_FileSystemVolumeLibrary )]
        internal extern static HRESULT Open              ( uint volumeHandle, string path, ref uint handle );
        [RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_FileSystemVolumeLibrary )]
        internal extern static HRESULT Close             ( uint volumeHandle, uint handle );
        [RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_FileSystemVolumeLibrary )]
        internal extern static HRESULT Read              ( uint volumeHandle, uint handle, byte[] buffer, int offset, int count, ref int bytesRead );
        [RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_FileSystemVolumeLibrary )]
        internal extern static HRESULT Write             ( uint volumeHandle, uint handle, byte[] buffer, int offset, int count, ref int bytesWritten );
        [RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_FileSystemVolumeLibrary )]
        internal extern static HRESULT Flush             ( uint volumeHandle, uint handle );
        [RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_FileSystemVolumeLibrary )]
        internal extern static HRESULT Seek              ( uint volumeHandle, uint handle, Int64 offset, uint origin, ref Int64 position );
        [RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_FileSystemVolumeLibrary )]
        internal extern static HRESULT GetLength         ( uint volumeHandle, uint handle, ref Int64 length );
        [RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_FileSystemVolumeLibrary )]
        internal extern static HRESULT SetLength( uint volumeHandle, uint handle, Int64 length );


        [RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_FileSystemVolumeLibrary )]
        internal extern static HRESULT GetName          ( uint volumeHandle, char[] name, ref int nameLen );
        [RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_FileSystemVolumeLibrary )]
        internal extern static HRESULT GetLabel         ( uint volumeHandle, char[] label, ref int labelLen );
        [RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_FileSystemVolumeLibrary )]
        internal extern static HRESULT GetFileSystem    ( uint volumeHandle, char[] fileSystem, ref int fileSystemLen );
        [RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_FileSystemVolumeLibrary )]
        internal extern static HRESULT GetSize          ( uint volumeHandle, ref long availSize, ref long totalSize );

        //--//

        //[RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_FileSystemVolumeLibrary )]
        //internal extern static bool ValidateStreamDriver();
        //[RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_FileSystemVolumeLibrary )]
        //internal extern static bool ValidateFind();
    };

    internal class FileSystemVolumeList
    {
        //    internal static bool InitializeVolumes();
        //    internal static bool UninitializeVolumes();
        //    internal static uint AddVolume( string nameSpace, uint serialNumber, uint deviceFlags,
        //                           BlockStorageDevice blockStorageDevice, uint volumeId, bool init );

        //    internal static bool RemoveVolume( FileSystemVolume fsv, bool uninit );
        const string c_RamBSDriverLibrary = @"BlockStorageFileSystem.obj";

        //[RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_RamBSDriverLibrary )]
        //internal extern static uint GetFirstVolume();

        //[RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_RamBSDriverLibrary )]
        //internal extern static uint GetNextVolume( ref FileSystemVolume volume );

        [RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_RamBSDriverLibrary )]
        internal extern static HRESULT GetVolumes( uint[] volume, ref int volumeCount );

        //[RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_RamBSDriverLibrary )]
        //internal extern static uint GetNumVolumes();

        [RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_RamBSDriverLibrary )]
        internal extern static uint FindVolume( string nameSpace, int namespaceLen );

        [RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_RamBSDriverLibrary )]
        internal extern static void AddRamBlockStorageFileStream();

        //[RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_FileSystemVolumeListLibrary )]
        //internal extern static bool Contains( uint volumeId );
    };


}
