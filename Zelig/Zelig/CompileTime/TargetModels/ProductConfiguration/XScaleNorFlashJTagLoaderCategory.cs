//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//#define DEBUG_CMD_PERF


namespace Microsoft.Zelig.Configuration.Environment
{
    using System;
    using System.Threading;
    using System.Collections.Generic;

    using Cfg = Microsoft.Zelig.Configuration.Environment;


    public abstract class XScaleNorFlashJTagLoaderCategory : JtagLoaderCategory
    {
        class NorFlashLoader : Emulation.Hosting.JTagCustomer
        {
            const uint cmd_Signature      = 0xDEADC000;
            const uint cmd_Mask           = 0xFFFFFF00;
                                        
            const byte cmd_Hello          = 0x01;
            const byte cmd_EnterCFI       = 0x02; // Arg: Address
            const byte cmd_ExitCFI        = 0x03; // Arg: Address
            const byte cmd_ReadMemory8    = 0x04; // Arg: Address, Size                 => <Size> values
            const byte cmd_ReadMemory16   = 0x05; // Arg: Address, Size                 => <Size> values
            const byte cmd_ReadMemory32   = 0x06; // Arg: Address, Size                 => <Size> values
            const byte cmd_ChecksumMemory = 0x07; // Arg: Address, Size                 => CRC value, AND of all memory 
            const byte cmd_EraseSector    = 0x08; // Arg: Address                       => Status value
            const byte cmd_ProgramMemory  = 0x09; // Arg: Address, Size, [32bits words] => Status value
            const byte cmd_EndOfStream    = 0xFF;

            const uint erasedMemoryPattern = 0xFFFFFFFFu;

            const uint baseIRAM  = 0x5C000000;
            const uint blockSize = 64 * 1024;

            const uint baseHostToDevice = baseIRAM + blockSize * 1;
            const uint baseDeviceToHost = baseIRAM + blockSize * 2;

            delegate void ProgressCallback( uint pos, uint total );

            //--//

            enum ScheduledWork
            {
                None           ,
                Program        ,
                EraseAndProgram,
            }

            class EraseSection
            {
                //
                // State
                //

                internal uint            m_baseAddress;
                internal uint            m_endAddress;
                internal uint            m_numEraseBlocks;
                internal uint            m_sizeEraseBlocks;
                internal ScheduledWork[] m_scheduledWork;
            }

            class PendingImageSection
            {
                //
                // State
                //

                internal readonly ImageSection m_imgSection;
                internal readonly uint         m_offset;
                internal readonly uint         m_len;

                //
                // Constructor Methods
                //

                internal PendingImageSection( ImageSection imgSection ,
                                              uint         offset     ,
                                              uint         len        )
                {
                    m_imgSection = imgSection;
                    m_offset     = offset;
                    m_len        = len;
                }
            }

            class ImageSection
            {
                //
                // State
                //

                internal readonly uint   m_address;
                internal readonly byte[] m_data;
            
                //
                // Constructor Methods
                //

                internal ImageSection( Configuration.Environment.ImageSection section )
                {
                    m_address = section.Address;
                    m_data    = section.Payload;
                }

                //
                // Helper Methods
                //

                internal PendingImageSection Intersects( uint eraseStart ,
                                                         uint eraseEnd   )
                {
                    uint addressStart = m_address;
                    uint addressEnd   = m_address + (uint)m_data.Length;

                    if(addressStart < eraseStart)
                    {
                        if(addressEnd > eraseStart)
                        {
                            uint offset = eraseStart - addressStart;
                            uint len    = Math.Min( eraseEnd, addressEnd ) - eraseStart;

                            return new PendingImageSection( this, offset, len );
                        }
                    }
                    else if(addressStart < eraseEnd)
                    {
                        uint offset = 0;
                        uint len    = Math.Min( eraseEnd, addressEnd ) - addressStart;

                        return new PendingImageSection( this, offset, len );
                    }

                    return null;
                }
            }

            class DelayedMemoryAccess
            {
                //
                // State
                //

                const int maxSize = 1024;

                Emulation.Hosting.JTagConnector m_jtag;
                uint                            m_baseAddress;

                uint                            m_currentAddress;
                uint[]                          m_data;
                uint                            m_posWriter;
                uint                            m_posReader;

                //
                // Constructor Methods
                //

                internal DelayedMemoryAccess( Emulation.Hosting.JTagConnector jtag        ,
                                              uint                            baseAddress )
                {
                    m_jtag           = jtag;
                    m_baseAddress    = baseAddress;
                    m_currentAddress = m_baseAddress;
                }

                //
                // Helper Methods
                //

                internal void Reset()
                {
                    m_currentAddress = m_baseAddress;
                    m_posReader      = 0;
                    m_posWriter      = 0;
                }

                internal void FillBuffer()
                {
                    if(m_posReader == m_posWriter)
                    {
                        m_data = m_jtag.ReadMemoryBlock( m_currentAddress, maxSize );

                        m_currentAddress += maxSize * sizeof(uint);
                        m_posReader       = 0;
                        m_posWriter       = maxSize;
                    }
                }

                internal void FlushBuffer()
                {
                    if(m_posWriter != 0)
                    {
                        m_jtag.WriteMemoryBlock( m_currentAddress, m_data, 0, (int)m_posWriter );

                        m_currentAddress += m_posWriter * sizeof(uint);
                        m_posWriter       = 0;
                    }
                }

                internal void Write( uint value )
                {
                    if(m_data == null)
                    {
                        m_data = new uint[maxSize];
                    }

                    if(m_posWriter >= maxSize)
                    {
                        FlushBuffer();
                    }

                    m_data[m_posWriter++] = value;
                }

                internal uint Read()
                {
                    FillBuffer();

                    return m_data[m_posReader++];
                }
            }

            //
            // State
            //

            ProductCategory                    m_product;
            Emulation.Hosting.AbstractHost     m_owner;
            Emulation.Hosting.JTagConnector    m_jtag;
            Emulation.Hosting.ProcessorControl m_processorControl;
            Emulation.Hosting.ProcessorStatus  m_processorStatus;

            DelayedMemoryAccess                m_writer;
            DelayedMemoryAccess                m_reader;

            uint                               m_flashBaseAddress;
            ulong                              m_flashSize;
            EraseSection[]                     m_eraseSections;

            //
            // Constructor Methods
            //

            public NorFlashLoader()
            {
            }

            //
            // Helper Methods
            //

            public override void Deploy( Emulation.Hosting.AbstractHost                      owner    ,
                                         Cfg.ProductCategory                                 product  ,
                                         List< Configuration.Environment.ImageSection >      image    ,
                                         Emulation.Hosting.ProcessorControl.ProgressCallback callback )
            {
                m_owner   = owner;
                m_product = product;

                owner.GetHostingService( out m_jtag             );
                owner.GetHostingService( out m_processorControl );
                owner.GetHostingService( out m_processorStatus  );

                m_writer = new DelayedMemoryAccess( m_jtag, baseHostToDevice );
                m_reader = new DelayedMemoryAccess( m_jtag, baseDeviceToHost );

                try
                {
                    bool fGot = false;

                    foreach(MemoryCategory mem in product.SearchValues< MemoryCategory >())
                    {
                        if((mem.Characteristics & Runtime.MemoryAttributes.FLASH) != 0)
                        {
                            m_flashBaseAddress = mem.BaseAddress;
                            m_flashSize        = mem.SizeInBytes;

                            fGot = true;
                            break;
                        }
                    }

                    if(fGot == false)
                    {
                        throw TypeConsistencyErrorException.Create( "Product {0} does not have a valid FLASH memory", product.GetType() );
                    }

                    //--//

                    Start();

                    uint position     = 0;
                    uint total        = 0;
                    uint totalPending = 0;

                    List< ImageSection        > lst        = new List< ImageSection        >();
                    List< PendingImageSection > lstPending = new List< PendingImageSection >();

                    //
                    // Collect work items.
                    //
                    foreach(var section in image)
                    {
                        if(section.NeedsRelocation == false)
                        {
                            var imgSection = new ImageSection( section );

                            lst.Add( imgSection );

                            total += (uint)section.Payload.Length;
                        }
                    }

                    //
                    // Verify if we need to perform any actions.
                    //
                    foreach(var imgSection in lst)
                    {
                        bool fMemoryErased;

                        if(VerifyChecksum( imgSection.m_address, imgSection.m_data, 0, imgSection.m_data.Length, out fMemoryErased ) == false)
                        {
                            //
                            // Found an item that needs attention. Map it to the various FLASH blocks.
                            //
                            foreach(var eraseSection in m_eraseSections)
                            {
                                for(uint pos = 0; pos < eraseSection.m_numEraseBlocks; pos++)
                                {
                                    uint eraseStart = eraseSection.m_baseAddress + pos * eraseSection.m_sizeEraseBlocks;
                                    uint eraseEnd   = eraseStart                 +       eraseSection.m_sizeEraseBlocks;

                                    PendingImageSection pending = imgSection.Intersects( eraseStart, eraseEnd );
                                    if(pending != null)
                                    {
                                        lstPending.Add( pending );

                                        totalPending += pending.m_len;

                                        //--//

                                        if(eraseSection.m_scheduledWork[pos] != ScheduledWork.EraseAndProgram)
                                        {
                                            if(fMemoryErased == false)
                                            {
                                                callback( "Erasing {1}", 0.0f, (float)total );

                                                EraseMemory( eraseStart, eraseEnd );

                                                eraseSection.m_scheduledWork[pos] = ScheduledWork.EraseAndProgram;
                                            }
                                            else
                                            {
                                                eraseSection.m_scheduledWork[pos] = ScheduledWork.Program;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if(lstPending.Count > 0)
                    {
                        callback( "Programming {0}/{1} (total image {2})", 0, totalPending, total );

                        foreach(var pending in lstPending)
                        {
                            ProgramMemory( pending.m_imgSection.m_address, pending.m_imgSection.m_data, pending.m_offset, pending.m_len, delegate( uint positionSub, uint totalSub )
                            {
                                callback( "Programming {0}/{1} (total image {2})", position + positionSub, totalPending, total );
                            } );

                            position += pending.m_len;

                            callback( "Programming {0}/{1} (total image {2})", position, totalPending, total );
                        }
                    }

                    //--//

                    callback( "Preparing for execution..." );

                    Emulation.Hosting.ProcessorControl svcPC; owner.GetHostingService( out svcPC );

                    svcPC.ResetState( product );
                }
                finally
                {
                    m_jtag.Cleanup();
                }
            }

            //--//

            private void SendData( uint data )
            {
////            Console.WriteLine( "Data: 0x{0:X8}", data );

                m_writer.Write( data );
            }

            private uint ReceiveData()
            {
                return m_reader.Read();
            }

            private void VerifyData( uint expected )
            {
                SendData( cmd_Signature | cmd_EndOfStream );

                m_writer.FlushBuffer();

                LetDeviceRun();

                m_writer.Reset();
                m_reader.Reset();

                uint data = ReceiveData();

                if(data != expected)
                {
                    throw TypeConsistencyErrorException.Create( "Failed to synchronize with device, expecting {0:X8}, got {1:X8}", expected, data );
                }
            }

            //--//

            private void LoadLoaderImage()
            {
                JtagLoaderCategory jtagLoader = m_product.SearchValue< Cfg.JtagLoaderCategory >();
                if(jtagLoader == null)
                {
                    throw TypeConsistencyErrorException.Create( "Product {0} does not have a JTAG loader", m_product.GetType() );
                }

                foreach(uint address in jtagLoader.LoaderData.Keys)
                {
                    m_jtag.WriteMemoryBlock( address, ToUint( jtagLoader.LoaderData[address] ) );
                }

                m_jtag.ProgramCounter = jtagLoader.EntryPoint;
            }

            private void ParseFlashSectors()
            {
                for(int tries = 0; tries < 3; tries++)
                {
                    Execute_EnterCFI();

                    ushort[] cfg = Execute_ReadMemory16( m_flashBaseAddress, 128 );

                    if(cfg[0x10] == 'Q' &&
                       cfg[0x11] == 'R' &&
                       cfg[0x12] == 'Y'  )
                    {
                        uint numEraseBlockRegions = cfg[0x2C];
                        uint baseAddress          = m_flashBaseAddress;

                        m_eraseSections = new EraseSection[numEraseBlockRegions];

                        for(uint pos = 0; pos < numEraseBlockRegions; pos++)
                        {
                            EraseSection section = new EraseSection();

                            section.m_baseAddress     = baseAddress;
                            section.m_numEraseBlocks  = ((uint)cfg[0x2D + pos * 4] + ((uint)cfg[0x2E + pos * 4] << 8)) + 1u;
                            section.m_sizeEraseBlocks = ((uint)cfg[0x2F + pos * 4] + ((uint)cfg[0x30 + pos * 4] << 8)) * 256;

                            section.m_scheduledWork   = new ScheduledWork[section.m_numEraseBlocks];


                            baseAddress += section.m_numEraseBlocks * section.m_sizeEraseBlocks;

                            section.m_endAddress = baseAddress;

                            m_eraseSections[pos] = section;
                        }

                        Execute_ExitCFI();
                        return;
                    }

                    Execute_ExitCFI();
                }

                throw TypeConsistencyErrorException.Create( "Cannot enter CFI mode" );
            }

            //--//

            private void Execute_Hello()
            {
                SendData( cmd_Signature | cmd_Hello );

                VerifyData( cmd_Signature | cmd_Hello );
            }

            private void Execute_EnterCFI()
            {
                SendData( cmd_Signature | cmd_EnterCFI );
                SendData( m_flashBaseAddress           );

                VerifyData( cmd_Signature | cmd_EnterCFI );
            }

            private void Execute_ExitCFI()
            {
                SendData( cmd_Signature | cmd_ExitCFI );
                SendData( m_flashBaseAddress          );

                VerifyData( cmd_Signature | cmd_ExitCFI );
            }

            private byte[] Execute_ReadMemory8( uint address ,
                                                int  size    )
            {
                SendData( cmd_Signature | cmd_ReadMemory8 );
                SendData( address                         );
                SendData( (uint)size                      );

                VerifyData( cmd_Signature | cmd_ReadMemory8 );

                byte[] res = new byte[size];

                for(int i = 0; i < size; i++)
                {
                    res[i] = (byte)ReceiveData();
                }

                return res;
            }

            private ushort[] Execute_ReadMemory16( uint address ,
                                                   int  size    )
            {
                SendData( cmd_Signature | cmd_ReadMemory16 );
                SendData( address                          );
                SendData( (uint)size                       );

                VerifyData( cmd_Signature | cmd_ReadMemory16 );

                ushort[] res = new ushort[size];

                for(int i = 0; i < size; i++)
                {
                    res[i] = (ushort)ReceiveData();
                }

                return res;
            }

            private uint[] Execute_ReadMemory32( uint address ,
                                                 int  size    )
            {
                SendData( cmd_Signature | cmd_ReadMemory32 );
                SendData( address                          );
                SendData( (uint)size                       );

                VerifyData( cmd_Signature | cmd_ReadMemory32 );

                uint[] res = new uint[size];

                for(int i = 0; i < size; i++)
                {
                    res[i] = ReceiveData();
                }

                return res;
            }

            private void Execute_ChecksumMemory(     uint address   ,
                                                     int  size      ,
                                                 out uint checksum  ,
                                                 out uint memoryAND )
            {
                SendData( cmd_Signature | cmd_ChecksumMemory );
                SendData( address                            );
                SendData( (uint)size                         );

                VerifyData( cmd_Signature | cmd_ChecksumMemory );

                checksum  = ReceiveData();
                memoryAND = ReceiveData();
            }

            private uint Execute_EraseSector( uint address )
            {
                SendData( cmd_Signature | cmd_EraseSector );
                SendData( address                         );

                VerifyData( cmd_Signature | cmd_EraseSector );

                return ReceiveData();
            }

            private uint Execute_ProgramMemory( uint   address ,
                                                uint[] data    ,
                                                int    size    ,
                                                int    offset  )
            {
                SendData( cmd_Signature | cmd_ProgramMemory );
                SendData( address                           );
                SendData( (uint)size                        );

                while(size-- > 0)
                {
                    SendData( data[offset++] );
                }

                VerifyData( cmd_Signature | cmd_ProgramMemory );

                return ReceiveData();
            }

            //--//

            private void Start()
            {
                if(m_jtag.IsTargetStopped() == false)
                {
                    m_jtag.StopTarget();
                }

                LoadLoaderImage();

                LetDeviceRun();

                ParseFlashSectors();
            }

            private void LetDeviceRun()
            {
#if DEBUG_CMD_PERF
                System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch();
    
                st.Start();
#endif

                if(m_jtag.RunDevice( 10000 ) == false)
                {
                    throw new TimeoutException();
                }

#if DEBUG_CMD_PERF
                st.Stop();
////            if(m_processorStatus.ProgramCounter != 0x5C000638)
////            {
////                Console.WriteLine( "BAD PC: 0x{0:X8} {1}", m_processorStatus.ProgramCounter, fStop );
////            }
                Console.WriteLine( "PC: 0x{0:X8} {1}", m_processorStatus.ProgramCounter, st.ElapsedMilliseconds );
#endif

                //
                // Skip breakpoint instruction.
                //
                m_processorStatus.ProgramCounter = m_processorStatus.ProgramCounter + sizeof(uint);
            }

            private bool VerifyChecksum(     uint   address       ,
                                             byte[] data          ,
                                             int    offset        ,
                                             int    size          ,
                                         out bool   fMemoryErased )
            {
                uint[] buf = ToUint( data, offset, size );
                int    len = buf.Length;

                if(VerifyChecksumInner( (uint)(address + offset), 0, len, buf, out fMemoryErased))
                {
                    return true;
                }

////            if(fMemoryErased == false)
////            {
////                for(uint chunk = 0; chunk < len; chunk += 8192)
////                {
////                    int subLen = (int)(Math.Min( (int)(chunk + 8192), len ) - chunk);
////
////                    uint[] buf2 = Execute_ReadMemory32( (uint)(address + offset + chunk), subLen );
////
////                    for(int pos = 0; pos < subLen; pos++)
////                    {
////                        if(buf2[pos] != buf[chunk+pos])
////                        {
////                            Console.WriteLine( "Mismatch: 0x{0:X8}: 0x{1:X8} should be 0x{2:X8}", address + offset + (chunk + pos) * sizeof(uint), buf2[pos], buf[chunk+pos] );
////                        }
////                    }
////                }
////            }

                return false;
            }

            private bool VerifyChecksumInner(     uint   address       ,
                                                  uint   offset        ,
                                                  int    len           ,
                                                  uint[] data          ,
                                              out bool   fMemoryErased )
            {
                uint checksum;
                uint memoryAND;

                Execute_ChecksumMemory( address + offset * sizeof(uint), len, out checksum, out memoryAND );

                uint localChecksum  = 0;
                uint localMemoryAND = erasedMemoryPattern;

                for(int pos = 0; pos < len; pos++)
                {
                    uint val = data[offset+pos];

                    localChecksum = ((localChecksum & 1) << 31) | (localChecksum >> 1);

                    localChecksum  += val;
                    localMemoryAND &= val;
                }

                fMemoryErased = (memoryAND == erasedMemoryPattern);

                if(localChecksum == checksum)
                {
                    return true;
                }

                if(fMemoryErased && localMemoryAND == erasedMemoryPattern)
                {
                    return true;
                }

                return false;
            }

            private void EraseMemory( uint start ,
                                      uint end   )
            {
                while(true)
                {
                    EraseSection section = FindSector( start );

                    if(section == null)
                    {
                        break;
                    }

                    uint address = section.m_baseAddress;

                    for(int j = 0; j < section.m_numEraseBlocks; j++)
                    {
                        uint endAddress = address + section.m_sizeEraseBlocks;

                        if(address <= start && start < endAddress)
                        {
                            Execute_EraseSector( address );

                            uint checksum;
                            uint memoryAND;

                            Execute_ChecksumMemory( address, (int)section.m_sizeEraseBlocks / sizeof(uint), out checksum, out memoryAND );

                            if(memoryAND != erasedMemoryPattern)
                            {
                                throw Emulation.Hosting.AbstractEngineException.Create( Emulation.Hosting.AbstractEngineException.Kind.Deployment, "Hardware problem: failed to erase sector at {0:X8}, size={1}", address, section.m_sizeEraseBlocks );
                            }

                            start = endAddress;
                            if(start >= end)
                            {
                                return;
                            }
                        }

                        address = endAddress;
                    }
                }
            }

            private uint ProgramMemory( uint             address  ,
                                        byte[]           data     ,
                                        uint             offset   ,
                                        uint             len      ,
                                        ProgressCallback callback )
            {
                uint rem = offset % sizeof(uint);

                if(rem != 0)
                {
                    offset -= rem;
                    len    += rem;
                }

                uint[] buf        = ToUint( data, (int)offset, (int)len );
                int    lenInWords = buf.Length;
                int    posInWords = 0;

                while(posInWords < lenInWords)
                {
                    uint addressBlock = address + offset + (uint)posInWords * sizeof(uint);

                    EraseSection section = FindSector( addressBlock );
                    if(section == null)
                    {
                        break;
                    }

                    int  countInWords = Math.Min( 32768 / sizeof(uint), lenInWords - posInWords );
                    uint res          = Execute_ProgramMemory( addressBlock, buf, countInWords, posInWords );
                    if(res != 0)
                    {
                        return res;
                    }

                    posInWords += countInWords;

                    callback( (uint)(posInWords * sizeof(uint)), (uint)(lenInWords * sizeof(uint)) );
                }

                return 0;
            }

            //--//

            private EraseSection FindSector( uint address )
            {
                for(int i = 0; i < m_eraseSections.Length; i++)
                {
                    EraseSection section = m_eraseSections[i];

                    if(section.m_baseAddress <= address && address < section.m_endAddress)
                    {
                        return section;
                    }
                }

                return null;
            }
        }

        //--//

        protected XScaleNorFlashJTagLoaderCategory( string name ,
                                                    byte[] file )
        {
            using(System.IO.StreamReader reader = new System.IO.StreamReader( new System.IO.MemoryStream( file ) ))
            {
                List< Emulation.ArmProcessor.SRecordParser.Block > blocks = new List< Emulation.ArmProcessor.SRecordParser.Block >();

                m_entryPoint = Emulation.ArmProcessor.SRecordParser.Parse( reader, name, blocks );

                foreach(Emulation.ArmProcessor.SRecordParser.Block block in blocks)
                {
                    m_loaderData[block.address] = block.data.ToArray();
                }
            }
        }

        protected override object GetServiceInner( Type t )
        {
            if(t == typeof(Emulation.Hosting.JTagCustomer))
            {
                return new NorFlashLoader();
            }

            return base.GetServiceInner( t );
        }
    }
}
