//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Configuration.Environment
{
    using System;
    using System.Threading;
    using System.Collections.Generic;

    using Cfg = Microsoft.Zelig.Configuration.Environment;


    public abstract class NorFlashJtagLoaderCategory : JtagLoaderCategory
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

            const uint erasedMemoryPattern = 0xFFFFFFFFu;

            delegate void ProgressCallback( uint pos, uint total );

            //--//

            class EraseSection
            {
                internal uint m_baseAddress;
                internal uint m_endAddress;
                internal uint m_numEraseBlocks;
                internal uint m_sizeEraseBlocks;
            }

            //
            // State
            //

            ProductCategory                             m_product;
            Emulation.Hosting.AbstractHost              m_owner;
            Emulation.Hosting.DebugCommunicationChannel m_dcc;
            Emulation.Hosting.JTagConnector             m_jtag;

            uint                                        m_flashBaseAddress;
            ulong                                       m_flashSize;
            EraseSection[]                              m_eraseSections;
            Thread                                      m_workerThread;

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

                owner.GetHostingService( out m_dcc  );
                owner.GetHostingService( out m_jtag );

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

                try
                {
                    Start();

                    float position = 0;
                    float total    = 0;

                    foreach(var section in image)
                    {
                        if(section.NeedsRelocation == false)
                        {
                            total += section.Payload.Length;
                        }
                    }

                    callback( "Erasing {1}", 0.0f, (float)total );

////                EraseMemory( m_flashBaseAddress, m_flashBaseAddress + 384 * 1024 );

                    while(true)
                    {
                        bool fModified = false;

                        foreach(var section in image)
                        {
                            if(section.NeedsRelocation == false)
                            {
                                bool   fMemoryErased;
                                uint   address = section.Address;
                                byte[] data    = section.Payload;

                                if(VerifyChecksum( address, data, out fMemoryErased ) == false)
                                {
                                    if(!fMemoryErased)
                                    {
                                        EraseMemory( address, address + (uint)data.Length );
                                        fModified = true;
                                    }
                                }
                            }
                        }

                        if(!fModified) break;
                    }

                    callback( "Programming {0}/{1}", 0, total );

                    foreach(var section in image)
                    {
                        if(section.NeedsRelocation == false)
                        {
                            bool   fMemoryErased;
                            uint   address = section.Address;
                            byte[] data    = section.Payload;

                            if(VerifyChecksum( address, data, out fMemoryErased ) == false)
                            {
                                ProgramMemory( address, data, delegate( uint positionSub, uint totalSub )
                                {
                                    callback( "Programming {0}/{1}", position + positionSub, total );
                                } );

                            }

                            position += data.Length;

                            callback( "Programming {0}/{1}", position, total );
                        }
                    }
                }
                finally
                {
                    Stop();
                }

                //--//

                callback( "Preparing for execution..." );

                Emulation.Hosting.ProcessorControl svcPC; owner.GetHostingService( out svcPC );

                svcPC.ResetState( product );
            }

            //--//

            private void WorkerThread()
            {
                try
                {
                    m_dcc.Start();

                    if(m_jtag.RunDevice( Timeout.Infinite ) == true)
                    {
                        throw TypeConsistencyErrorException.Create( "Device stopped unexpectedly during deployment!" );
                    }
                }
                finally
                {
                    m_dcc.Stop();
                }
            }

            private void SendData( uint data )
            {
                if(m_dcc.WriteFromDebugger( data, 10 * 1000 ) == false)
                {
                    throw TypeConsistencyErrorException.Create( "Timeout while sending data to device" );
                }
            }

            private uint ReceiveData()
            {
                uint data;

                if(m_dcc.ReadFromDebugger( out data, 10 * 1000 ) == false)
                {
                    throw TypeConsistencyErrorException.Create( "Timeout while receiving data from device" );
                }

                return data;
            }

            private void VerifyData( uint expected )
            {
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

                //
                // Start execution on worker thread
                //

                m_workerThread = new Thread( WorkerThread );
                m_workerThread.Start();

                Execute_Hello();
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

                VerifyData( cmd_Signature | cmd_ProgramMemory );

                while(size-- > 0)
                {
                    SendData( data[offset++] );
                }

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

                ParseFlashSectors();
            }

            private void Stop()
            {
                m_jtag.AbortRunDevice();

                m_workerThread.Join();

                m_jtag.Cleanup();
            }

            private bool VerifyChecksum(     uint   address       ,
                                             byte[] data          ,
                                         out bool   fMemoryErased )
            {
                uint[] buf = ToUint( data );
                int    len = buf.Length;
                uint   checksum;
                uint   memoryAND;

                Execute_ChecksumMemory( address, len, out checksum, out memoryAND );

                uint localChecksum  = 0;
                uint localMemoryAND = erasedMemoryPattern;

                for(int pos = 0; pos < len; pos++)
                {
                    uint val = buf[pos];

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
                                        ProgressCallback callback )
            {
                uint[] buf    = ToUint( data );
                int    pos    = 0;
                int    lenBuf = buf.Length;

                while(pos < lenBuf)
                {
                    EraseSection section = FindSector( address );
                    if(section == null)
                    {
                        break;
                    }

                    int  len = Math.Min( 128, lenBuf - pos );
                    uint res = Execute_ProgramMemory( address, buf, len, pos );
                    if(res != 0)
                    {
                        return res;
                    }

                    address += (uint)(len * sizeof(uint));
                    pos     +=        len;

                    callback( (uint)(pos * sizeof(uint)), (uint)lenBuf );
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

        protected NorFlashJtagLoaderCategory( string name ,
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
