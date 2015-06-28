using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using RT = Microsoft.Zelig.Runtime;
using System.Runtime.InteropServices;
using Microsoft.Zelig.Runtime;
using System.Reflection;

namespace FileSystemTest
{
    [StructLayout( LayoutKind.Explicit )]
    public struct SYSTEMTIME
    {
        [FieldOffset( 0 )]
        public ushort wYear;
        [FieldOffset( 2 )]
        public ushort wMonth;
        [FieldOffset( 4 )]
        public ushort wDayOfWeek;
        [FieldOffset( 6 )]
        public ushort wDay;
        [FieldOffset( 8 )]
        public ushort wHour;
        [FieldOffset( 10 )]
        public ushort wMinute;
        [FieldOffset( 12 )]
        public ushort wSecond;
        [FieldOffset( 14 )]
        public ushort wMilliseconds;
    };

    public class TestApp
    {
        [RT.ExportedMethod]
        public static Int64 _Z17Time_GetLocalTimev()
        {
            return DateTime.UtcNow.Ticks;
        }

        [RT.ExportedMethod]
        public static Int64 _Z19Time_FromSystemTimePK10SYSTEMTIME( ref SYSTEMTIME sysTime )
        {
            DateTime dt = new DateTime( sysTime.wYear, sysTime.wMonth, sysTime.wDay, sysTime.wHour, sysTime.wMinute, sysTime.wSecond, sysTime.wMilliseconds, DateTimeKind.Utc );

            return dt.Ticks;
        }

        [RT.ExportedMethod]
        public static int _Z17Time_ToSystemTimexP10SYSTEMTIME( Int64 time, ref SYSTEMTIME sysTime )
        {
            DateTime dt = new DateTime( time, DateTimeKind.Utc );

            sysTime.wYear         = (ushort)dt.Year;
            sysTime.wMonth        = (ushort)dt.Month;
            sysTime.wDay          = (ushort)dt.Day;
            sysTime.wDayOfWeek    = (ushort)dt.DayOfWeek;
            sysTime.wHour         = (ushort)dt.Hour;
            sysTime.wMinute       = (ushort)dt.Minute;
            sysTime.wSecond       = (ushort)dt.Second;
            sysTime.wMilliseconds = (ushort)dt.Millisecond;

            return 1;
        }


        [RT.ExportedMethod]
        public static int Extern__Storage_Write( uint address, IntPtr buffer, uint offset, uint len )
        {
            try
            {
                byte[] bufTmp = new byte[len];

                unsafe
                {
                    byte* ptr = (byte*)buffer.ToPointer();
                    int i = 0;
                    uint end = offset + len;

                    for(; offset < end; offset++)
                    {
                        bufTmp[i++] = ptr[offset];
                    }
                }

                return Storage.Instance.Write( new UIntPtr( address ), bufTmp, 0, len ) ? 1 : 0;
            }
            catch
            {
                return 0;
            }
        }

        [RT.ExportedMethod]
        public static int Extern__Storage_Read( uint address, IntPtr buffer, uint offset, uint len )
        {
            try
            {
                byte[] bufTmp = new byte[len];

                Storage.Instance.Read( new UIntPtr( address ), bufTmp, 0, len );

                unsafe
                {
                    byte* ptr = (byte*)buffer.ToPointer();
                    uint end = offset + len;
                    int i = 0;

                    for(; offset < end; offset++)
                    {
                        ptr[offset] = bufTmp[i++];
                    }
                }

                return 1;
            }
            catch
            {
                return 0;
            }
        }

        [RT.ExportedMethod]
        public static int Extern__Storage_Memset( uint address, byte data, int len )
        {
            try
            {
                uint dataPair = (uint)data;

                dataPair |= dataPair << 8;
                dataPair |= dataPair << 16;

                while(0 != (address & 0x3) )
                {
                    Storage.Instance.WriteByte( new UIntPtr( address ), data );
                    len -= 1;
                    address += 1;
                }

                while(len > 4)
                {
                    Storage.Instance.WriteWord( new UIntPtr( address ), dataPair );
                    len     -= 4;
                    address += 4;
                }

                while(len > 0)
                {
                    Storage.Instance.WriteByte( new UIntPtr( address ), data );
                    len     -= 1;
                    address += 1;
                }

                return 1;
            }
            catch
            {
                return 0;
            }
        }

        [RT.ExportedMethod]
        public static int Extern__Storage_IsErased( uint address, int len )
        {
            try
            {
                while(len > 0)
                {
                    UIntPtr ptr = new UIntPtr( address );

                    if(Storage.Instance.ReadWord( ptr ) != 0xFFFFFFFFu) return 0;

                    address += 4;
                    len     -= 4;
                }

                return 1;
            }
            catch
            {
                return 0;
            }
        }

        [RT.ExportedMethod]
        public static int Extern__Storage_EraseBlock( uint address, int len )
        {
            try
            {
                return Storage.Instance.EraseSectors( new UIntPtr( address ), new UIntPtr( address + (uint)len ) ) ? 1 : 0;
            }
            catch
            {
                return 0;
            }
        }


        static void Main()
        {
            string dir = "\\ROOT\\Dir1";
            string file = dir + "\\MyF.txt";

            DirectoryInfo di;

            VolumeInfo v = new VolumeInfo( "ROOT" );

            if(!v.IsFormatted)
            {
                v.Format( 0 );
            }

            if(!Directory.Exists( dir ))
            {
                di = Directory.CreateDirectory( dir );
            }

            if(File.Exists( file ))
            {
                File.Delete( file );
            }

            using(FileStream fs = new FileStream( file, FileMode.CreateNew, FileAccess.Write ))
            {
                byte[] data = UTF8Encoding.UTF8.GetBytes( "Hello World!" );
                fs.Write( data, 0, data.Length );
            }

            using(FileStream fsIn = new FileStream( file, FileMode.Open, FileAccess.Read ))
            {
                byte[] data = new byte[128];

                int len = fsIn.Read( data, 0, data.Length );

                string str = UTF8Encoding.UTF8.GetString( data, 0, len );

                Console.WriteLine( str );
            }

            FileEnum fe = new FileEnum( dir, FileEnumFlags.Files );

            if(fe.MoveNext())
            {
                Console.WriteLine( fe.Current.ToString() );
            }

            IOTests.Initialize();

            IOTests.RunTests( new CreateDirectory() );
            IOTests.RunTests( new Delete() );
            IOTests.RunTests( new DI_Constructor() );
            IOTests.RunTests( new DirectoryInfoTests() );
            IOTests.RunTests( new DirectoryTests() );
            IOTests.RunTests( new Exists() );
            IOTests.RunTests( new GetDirectories() );
            IOTests.RunTests( new GetFiles() );
            IOTests.RunTests( new GetSetCurrentDirectory() );
            IOTests.RunTests( new Move() );

            IOTests.RunTests( new Copy() );
            IOTests.RunTests( new Create() );
            IOTests.RunTests( new FileDelete() );
            IOTests.RunTests( new FileExists() );
            IOTests.RunTests( new FileInfoTests() );
            IOTests.RunTests( new FileTests() );
            IOTests.RunTests( new GetSetAttributes() );
            IOTests.RunTests( new Open_FM() );
            IOTests.RunTests( new Open_FM_FA() );
            IOTests.RunTests( new Open_FM_FA_FS() );
            IOTests.RunTests( new OpenRead() );
            IOTests.RunTests( new OpenWrite() );
            IOTests.RunTests( new RWAllBytes() );

            IOTests.RunTests( new CanRead() );
            IOTests.RunTests( new CanSeek() );
            IOTests.RunTests( new CanWrite() );
            IOTests.RunTests( new Constructors_FileAccess() );
            IOTests.RunTests( new Constructors_FileMode() );
            IOTests.RunTests( new Constructors_FileShare() );
            IOTests.RunTests( new FileStreamTests() );
            IOTests.RunTests( new Flush() );
            IOTests.RunTests( new PropertyTests() );
            IOTests.RunTests( new Read() );
            IOTests.RunTests( new Seek() );
            IOTests.RunTests( new Write() );

            IOTests.RunTests( new MemoryStreamCanRead() );
            IOTests.RunTests( new MemoryStreamCanSeek() );
            IOTests.RunTests( new MemoryStreamCanWrite() );
            IOTests.RunTests( new Close() );
            IOTests.RunTests( new MemoryStreamFlush() );
            IOTests.RunTests( new Length() );
            IOTests.RunTests( new MemoryStream_Ctor() );
            IOTests.RunTests( new Position() );
            IOTests.RunTests( new MemoryStreamRead() );
            IOTests.RunTests( new ReadByte() );
            IOTests.RunTests( new MemoryStreamSeek() );
            IOTests.RunTests( new SetLength() );
            IOTests.RunTests( new ToArray() );
            IOTests.RunTests( new MemoryStreamWrite() );
            IOTests.RunTests( new WriteByte() );
            IOTests.RunTests( new WriteTo() );

            IOTests.RunTests( new ChangeExtensions() );
            IOTests.RunTests( new Combine() );
            IOTests.RunTests( new GetDirectoryName() );
            IOTests.RunTests( new GetExtension() );
            IOTests.RunTests( new GetFileName() );
            IOTests.RunTests( new GetFileNameWithoutExtension() );
            IOTests.RunTests( new GetFullPath() );
            IOTests.RunTests( new GetPathRoot() );
            IOTests.RunTests( new HasExtension() );
            IOTests.RunTests( new IsPathRooted() );
            IOTests.RunTests( new PathTests() );

            IOTests.RunTests( new VolumeLabelTests() );

            Log.Comment( "\n\n!!!!!! FINISHED !!!!!!!\n\n" );

        }
    }
}
