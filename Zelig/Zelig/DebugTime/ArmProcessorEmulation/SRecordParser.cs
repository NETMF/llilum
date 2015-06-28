//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Emulation.ArmProcessor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    public static class SRecordParser
    {
        public class Block
        {
            //
            // State
            //

            public uint         address;
            public MemoryStream data;
        }

        public static uint Parse( string        file   ,
                                  List< Block > blocks )
        {
            if(System.IO.File.Exists( file ) == false)
            {
                throw new System.IO.FileNotFoundException( String.Format( "Cannot find {0}", file ) );
            }

            using(System.IO.StreamReader reader = new StreamReader( file ))
            {
                return Parse( reader, file, blocks );
            }
        }

        public static uint Parse( System.IO.StreamReader reader   ,
                                  string                 fileName ,
                                  List< Block >          blocks   )
        {
            string line;
            int    lineNum    = 0;
            uint   entrypoint = 0;

            while((line = reader.ReadLine()) != null)
            {
                char[] lineBytes = line.ToCharArray();
                int    len       = lineBytes.Length;
                int    i;

                lineNum++; if(len == 0) continue;

                if((lineBytes[0] != 'S' && lineBytes[0] != 's') ||
                   (lineBytes[1] != '3' && lineBytes[1] != '7')  )
                {
                    throw new System.ArgumentException( String.Format( "Unknown format at line {0} of {1}:\n {2}", lineNum, fileName, line ) );
                }

                int num = Byte.Parse( new string( lineBytes, 2, 2 ),  System.Globalization.NumberStyles.HexNumber );
                if(num != ((len / 2) - 2))
                {
                    throw new System.ArgumentException( String.Format( "Incorrect length at line {0} of {1}: {2}", lineNum, fileName, num ) );
                }

                byte crc = (byte)num;

                for(i = 4; i<len - 2; i += 2)
                {
                    crc += Byte.Parse( new string( lineBytes, i, 2 ), System.Globalization.NumberStyles.HexNumber );
                }

                byte checksum = Byte.Parse( new string( lineBytes, len - 2, 2 ), System.Globalization.NumberStyles.HexNumber );

                if((checksum ^ crc) != 0xFF)
                {
                    throw new System.ArgumentException( String.Format( "Incorrect crc at line {0} of {1}: got {2:X2}, expected {3:X2}", lineNum, fileName, crc, checksum ) );
                }

                num -= 5;

                uint address = UInt32.Parse( new string( lineBytes, 4, 8 ), System.Globalization.NumberStyles.HexNumber );

                if(lineBytes[1] == '7')
                {
                    entrypoint = address;
                    break;
                }
                else
                {
                    Block bl = new Block();

                    bl.address = address;
                    bl.data    = new MemoryStream();

                    for(i=0; i<num; i++)
                    {
                        bl.data.WriteByte( Byte.Parse( new string( lineBytes, 12 + i * 2, 2 ), System.Globalization.NumberStyles.HexNumber ) );
                    }

                    for(i=0; i<blocks.Count; i++)
                    {
                        Block bl2  = (Block)blocks[i];
                        int   num2 = (int)bl2.data.Length;

                        if(bl2.address + num2 == bl.address)
                        {
                            byte[] data = bl.data.ToArray();

                            bl2.data.Write( data, 0, data.Length );

                            bl = null;
                            break;
                        }

                        if(bl.address + num == bl2.address)
                        {
                            byte[] data = bl2.data.ToArray();

                            bl.data.Write( data, 0, data.Length );

                            bl2.address = bl.address;
                            bl2.data    = bl.data;

                            bl = null;
                            break;
                        }

                        if(bl.address < bl2.address)
                        {
                            blocks.Insert( i, bl );

                            bl = null;
                            break;
                        }
                    }

                    if(bl != null) blocks.Add( bl );
                }
            }

            return entrypoint;
        }

        public static void EncodeEntrypoint( Stream stream  ,
                                             uint   address )
        {
            StreamWriter writer = new StreamWriter( stream, Encoding.ASCII );

            byte crc = (byte)(5);

            writer.Write( "S7{0:X2}{1:X8}", 5, address );

            crc += (byte)(address >>  0);
            crc += (byte)(address >>  8);
            crc += (byte)(address >> 16);
            crc += (byte)(address >> 24);

            writer.WriteLine( "{0:X2}", (byte)~crc );

            writer.Flush();
        }

        public static void Encode( Stream stream  ,
                                   byte[] buf     ,
                                   uint   address )
        {
            StreamWriter writer = new StreamWriter( stream, Encoding.ASCII );

            uint len    = (uint)buf.Length;
            int  offset = 0;

            while(len > 0)
            {
                uint size = len > 16 ? 16 : len;
                byte crc  = (byte)(size + 5);

                writer.Write( "S3{0:X2}{1:X8}", size + 5, address );

                crc += (byte)(address >>  0);
                crc += (byte)(address >>  8);
                crc += (byte)(address >> 16);
                crc += (byte)(address >> 24);

                for(uint i=0; i<size; i++)
                {
                    byte v = buf[offset++];

                    writer.Write( "{0:X2}", v );

                    crc += v;
                }

                address += size;
                len     -= size;

                writer.WriteLine( "{0:X2}", (byte)~crc );
            }

            writer.Flush();
        }
    }
}
