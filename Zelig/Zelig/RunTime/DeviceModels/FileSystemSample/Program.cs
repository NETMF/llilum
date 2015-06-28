//
// Copyright (c) Microsoft Corporation.   All rights reserved.
//

namespace FileSystemSample
{
    using System;
    using RT = Microsoft.Zelig.Runtime;
    using System.Runtime.InteropServices;
    using System.Threading;
    using Microsoft.Zelig.Runtime;
    using System.IO;
    using System.Text;

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

    public class FileSystemTest
    {
        [RT.ExportedMethod]
        public static Int64 _Z17Time_GetLocalTimev()
        {
            return DateTime.Now.Ticks;
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

            Console.WriteLine( "Finished" );
        }
    }
}
