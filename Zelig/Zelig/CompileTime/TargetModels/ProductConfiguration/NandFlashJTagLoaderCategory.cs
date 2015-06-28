//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Configuration.Environment
{
    using System;
    using System.Threading;
    using System.Collections.Generic;

    using Cfg = Microsoft.Zelig.Configuration.Environment;


    public abstract class NandFlashJtagLoaderCategory : JtagLoaderCategory
    {
        class NandFlashLoader : Emulation.Hosting.JTagCustomer
        {
            enum BlockState
            {
                Uninitialized       ,
                DataLoaded          ,
                DataLoadedAndChecked,
                BadBlock            ,
            }

            class BlockEntry
            {
                internal NandFlashLoader m_owner;
                internal BlockState      m_state;
                internal uint            m_index;
                internal uint            m_physicalAddress;
                                   
                internal uint[]          m_data;
                internal uint            m_local_logicalAddress;
                internal uint            m_local_checksum;
                internal bool            m_local_fErased;
                                   
                internal uint            m_device_logicalAddress;
                internal uint            m_device_checksum;
                internal bool            m_device_fErased;
                internal bool            m_device_fBadBlock;

                //
                // Constructor Methods
                //

                internal BlockEntry( NandFlashLoader owner   ,
                                     uint            index   ,
                                     uint            address )
                {
                    m_owner                = owner;
                    m_state                = BlockState.Uninitialized;
                    m_index                = index;
                    m_physicalAddress      = address;
                    m_local_logicalAddress = address;
                }

                //
                // Helper Methods
                //

                internal void MoveToState( BlockState targetState )
                {
                    while(m_state < targetState)
                    {
                        m_state++;

                        switch(m_state)
                        {
                            case BlockState.DataLoaded:
                                m_data = new uint[m_owner.m_flashBlockSizeInWords];

                                for(int i = 0; i < m_owner.m_flashBlockSizeInWords; i++)
                                {
                                    m_data[i] = erasedMemoryPattern;
                                }
                                break;

                            case BlockState.DataLoadedAndChecked:
                                uint localChecksum  = 0;
                                uint localMemoryAND = erasedMemoryPattern;

                                for(int pos = 0; pos < m_data.Length; pos++)
                                {
                                    uint val = m_data[pos];

                                    localChecksum = ((localChecksum & 1) << 31) | (localChecksum >> 1);

                                    localChecksum  += val;
                                    localMemoryAND &= val;
                                }

                                m_local_checksum =  localChecksum;
                                m_local_fErased  = (localMemoryAND == erasedMemoryPattern);

                                SetState();
                                break;
                        }
                    }
                }

                internal bool CanBePreparedForProgramming()
                {
                    if(this.IsInitialized)
                    {
                        if(m_state == BlockState.BadBlock)
                        {
                            return false;
                        }

                        this.MoveToState( BlockState.DataLoadedAndChecked );

                        if(m_device_fBadBlock || (m_device_fErased == false && this.ChecksumMatch == false))
                        {
////                        Console.WriteLine( "Erasing block {0:X8}", m_physicalAddress );
                            m_owner.Execute_EraseBlock( m_physicalAddress );

                            SetState();

                            if(m_device_fBadBlock)
                            {
                                m_state = BlockState.BadBlock;
                                return false;
                            }
                        }

                        return true;
                    }

                    return false;
                }

                internal bool ShouldBeProgrammed()
                {
                    if(this.IsInitialized)
                    {
                        if(m_state == BlockState.BadBlock)
                        {
                            return false;
                        }

                        this.MoveToState( BlockState.DataLoadedAndChecked );

                        return this.ChecksumMatch == false;
                    }

                    return false;
                }

                internal bool Program()
                {
                    if(this.ShouldBeProgrammed())
                    {
                        for(int retry = 1; retry < 5; retry++)
                        {
                            if(this.CanBePreparedForProgramming() == false)
                            {
                                return false;
                            }

////                        Console.WriteLine( "Programming block {0:X8}, logical {1:X8}", physicalAddress, logicalAddress );

                            uint pages = m_owner.m_flashBlockSizeInWords / m_owner.m_flashPageSizeInWords;

////                        System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch();
////                        Console.WriteLine( "Programming {0} pages", pages );
////                        st.Start();

                            m_owner.Execute_ProgramMemory_Header( m_physicalAddress, pages );

                            uint logicalAddress  = m_local_logicalAddress;

                            for(uint offset = 0; offset < m_owner.m_flashBlockSizeInWords; offset += m_owner.m_flashPageSizeInWords)
                            {
                                m_owner.Execute_ProgramMemory_Data( m_data, m_owner.m_flashPageSizeInWords, offset, new uint[] { logicalAddress } );

                                logicalAddress += m_owner.m_flashPageSize;
                            }

                            bool fSuccess = m_owner.Execute_ProgramMemory_Feedback();

////                        st.Stop();
////                        Console.WriteLine( "Done in {0} msec", st.ElapsedMilliseconds );

////                        uint[] page = m_owner.Execute_ReadPage( m_physicalAddress + 1 * m_owner.m_flashPageSize, out fSuccess) ;
                            SetState();

                            if(this.ChecksumMatch)
                            {
                                break;
                            }
                            else
                            {
                                Thread.Sleep( 200 * retry );
                            }
                        }

                        if(m_device_fBadBlock || this.ChecksumMatch == false)
                        {
                            m_state = BlockState.BadBlock;
                            return false;
                        }
                    }

                    return true;
                }

                internal void MoveLocalState( BlockEntry other )
                {
                    m_data                 = other.m_data;
                    m_local_logicalAddress = other.m_local_logicalAddress;
                    m_local_checksum       = other.m_local_checksum;
                    m_local_fErased        = other.m_local_fErased;
                }

                //--//

                private void SetState()
                {
                    uint memoryAND;

                    m_device_logicalAddress = 0xFFFFFFFF;
                    m_device_fErased        = false;
                    m_device_fBadBlock      = false;

                    if(m_owner.Execute_ChecksumMemory( m_physicalAddress, m_data.Length, out m_device_checksum, out memoryAND ) ||
                       m_owner.Execute_ChecksumMemory( m_physicalAddress, m_data.Length, out m_device_checksum, out memoryAND )  )
                    {
                        m_device_fErased = (memoryAND == erasedMemoryPattern);

                        if(m_device_fErased == false)
                        {
                            bool   fSuccess;
                            uint[] spare = m_owner.Execute_ReadSpare( m_physicalAddress, out fSuccess );
                            if(fSuccess)
                            {
                                m_device_logicalAddress = spare[0];
                            }
                            else
                            {
                                m_device_fBadBlock = true;
                            }
                        }
                    }
                    else
                    {
                        m_device_fBadBlock = true;
                    }
                }

                //
                // Access Methods
                //

                internal bool IsInitialized
                {
                    get
                    {
                        return m_state != BlockState.Uninitialized;
                    }
                }

                internal bool HasData
                {
                    get
                    {
                        switch(m_state)
                        {
                            case BlockState.DataLoaded          :
                            case BlockState.DataLoadedAndChecked:
                                return true;
                        }

                        return false;
                    }
                }

                internal uint Size
                {
                    get
                    {
                        if(this.HasData)
                        {
                            return (uint)m_data.Length * sizeof(uint);
                        }

                        return 0;
                    }
                }

                private bool ChecksumMatch
                {
                    get
                    {
                        if(m_local_checksum       == m_device_checksum       &&
                           m_local_logicalAddress == m_device_logicalAddress  )
                        {
                            return true;
                        }

                        return false;
                    }
                }
            }

            const uint cmd_Signature      = 0xDEADC000;
            const uint cmd_Mask           = 0xFFFFFF00;
                                        
            const byte cmd_Hello          = 0x01;
            const byte cmd_GetConfig      = 0x02; //                               => <Page Count> <Page Size> <Usable Page Size> <Block Size>
            const byte cmd_ReadPage       = 0x03; // Arg: Address                  => <Status value> <values>
            const byte cmd_ReadSpare      = 0x04; // Arg: Address                  => <Status value> <values>
            const byte cmd_ChecksumMemory = 0x05; // Arg: Address                  => <Status value> <CRC value>, <AND of all memory>
            const byte cmd_EraseBlock     = 0x06; // Arg: Address                  => <Status value>
            const byte cmd_ProgramPage    = 0x07; // Arg: Address, # pages [bytes] => <Status value page0> .... <Status value pageN>

            const uint erasedMemoryPattern = 0xFFFFFFFFu;

            //--//

            //
            // State
            //

            ProductCategory                             m_product;
            Emulation.Hosting.AbstractHost              m_owner;
            Emulation.Hosting.DebugCommunicationChannel m_dcc;
            Emulation.Hosting.JTagConnector             m_jtag;

            uint                                        m_flashPageCount;
            uint                                        m_flashPageSize;
            uint                                        m_flashSpareSize;
            uint                                        m_flashUsableSpareSize;
            uint                                        m_flashBlockSize;
            Thread                                      m_workerThread;

            uint                                        m_flashPageSizeInWords;
            uint                                        m_flashSpareSizeInWords;
            uint                                        m_flashBlockSizeInWords;
            BlockEntry[]                                m_blocks;

            //
            // Constructor Methods
            //

            public NandFlashLoader()
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

                //--//

                try
                {
                    Start();

                    //////////////////////////////////////////////////////////////
                    //
                    // DEBUG!! DEBUG!! DEBUG!! DEBUG!! DEBUG!!
                    //
                    ////            uint[] page1 = Execute_ReadPage( 0xA0000000u );
                    ////            uint[] page2 = Execute_ReadPage( 0xA0000200u );
                    ////            uint[] page3 = Execute_ReadPage( 0xA0010000u );
                    ////
                    ////            uint[] page1 = Execute_ReadPage( m_flashPageSize * 0 );
                    ////            uint[] page2 = Execute_ReadPage( m_flashPageSize * 1 );
                    ////            uint[] page3 = Execute_ReadPage( m_flashPageSize * 128 );
                    ////
                    ////            uint   checksum;
                    ////            uint   memoryAND;
                    ////
                    ////            Execute_ChecksumMemory( m_flashPageSize *   0, (int)m_flashPageSize / sizeof(uint), out checksum, out memoryAND );
                    ////            Execute_ChecksumMemory( m_flashPageSize * 128, (int)m_flashPageSize / sizeof(uint), out checksum, out memoryAND );
                    ////
                    ////            Execute_ProgramMemory( m_flashPageSize * 128, page2, (int)m_flashPageSize / sizeof(uint), 0 );
                    ////
                    ////            uint[] page4 = Execute_ReadPage( m_flashPageSize * 128 );
                    //
                    // DEBUG!! DEBUG!! DEBUG!! DEBUG!! DEBUG!!
                    //
                    //////////////////////////////////////////////////////////////

                    //--//
    
                    float position = 0;
                    float total    = 0;
    
                    //
                    // Split the image into the blocks.
                    //
                    foreach(var section in image)
                    {
                        if(section.NeedsRelocation == false)
                        {
                            uint   address = section.Address;
                            byte[] data    = section.Payload;
    
                            SplitIntoBlocks( address, data );
                        }
                    }
    
                    foreach(var bs in m_blocks)
                    {
                        total += bs.Size;
                    }
    
                    callback( "Checking {1}", 0.0f, total );
    
                    foreach(var bs in m_blocks)
                    {
                        if(bs.HasData)
                        {
                            bs.MoveToState( BlockState.DataLoadedAndChecked );
                        }
                    }
    
                    callback( "Programming {0}/{1}", 0.0f, total );
    
                    foreach(var bs in m_blocks)
                    {
                        if(bs.Program() == false)
                        {
                            if(Redistribute( bs ) == false)
                            {
                                throw Emulation.Hosting.AbstractEngineException.Create( Emulation.Hosting.AbstractEngineException.Kind.Deployment, "Hardware problem: failed to find good block for {0:X8}", bs.m_local_logicalAddress );
                            }
                        }
    
                        position += bs.Size;
    
                        callback( "Programming {0}/{1}", position, total );
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

                    m_jtag.RunDevice();
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

////            using(var file = new System.IO.FileStream( @"s:\imagedump.bin", System.IO.FileMode.Create ))
////            {
////                foreach(uint address in jtagLoader.LoaderData.Keys)
////                {
////                    var section = jtagLoader.LoaderData[address];
////
////                    file.Seek( address - 0x08000000, System.IO.SeekOrigin.Begin );
////                    file.Write( section, 0, section.Length );
////                }
////            }

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
                Execute_GetConfig();
            }

            //--//

            private void Execute_Hello()
            {
                SendData( cmd_Signature | cmd_Hello );

                VerifyData( cmd_Signature | cmd_Hello );
            }

            private void Execute_GetConfig()
            {
                SendData( cmd_Signature | cmd_GetConfig );

                VerifyData( cmd_Signature | cmd_GetConfig );

                m_flashPageCount       = ReceiveData();
                m_flashPageSize        = ReceiveData();
                m_flashSpareSize       = ReceiveData();
                m_flashUsableSpareSize = ReceiveData();
                m_flashBlockSize       = ReceiveData();

                m_flashPageSizeInWords  = m_flashPageSize  / sizeof(uint);
                m_flashSpareSizeInWords = m_flashSpareSize / sizeof(uint);
                m_flashBlockSizeInWords = m_flashBlockSize / sizeof(uint);
                m_blocks                = new BlockEntry[(m_flashPageCount * m_flashPageSize) / m_flashBlockSize];

                for(uint i = 0; i < m_blocks.Length; i++)
                {
                    m_blocks[i] = new BlockEntry( this, i, i * m_flashBlockSize );
                }
            }

            private uint[] Execute_ReadPage(     uint address  ,
                                             out bool fSuccess )
            {
                SendData( cmd_Signature | cmd_ReadPage );
                SendData( address                      );

                VerifyData( cmd_Signature | cmd_ReadPage );

                fSuccess = ReceiveData() != 0;

                uint[] res = new uint[m_flashPageSizeInWords + m_flashSpareSizeInWords];

                for(int i = 0; i < res.Length; i++)
                {
                    res[i] = ReceiveData();
                }

                return res;
            }

            private uint[] Execute_ReadSpare(     uint address  ,
                                              out bool fSuccess )
            {
                SendData( cmd_Signature | cmd_ReadSpare );
                SendData( address                       );

                VerifyData( cmd_Signature | cmd_ReadSpare );

                fSuccess = ReceiveData() != 0;

                uint[] res = new uint[(m_flashSpareSize) / sizeof(uint)];

                for(int i = 0; i < res.Length; i++)
                {
                    res[i] = ReceiveData();
                }

                return res;
            }

            private bool Execute_ChecksumMemory(     uint address   ,
                                                     int  size      ,
                                                 out uint checksum  ,
                                                 out uint memoryAND )
            {
                SendData( cmd_Signature | cmd_ChecksumMemory );
                SendData( address                            );
                SendData( (uint)size                         );

                VerifyData( cmd_Signature | cmd_ChecksumMemory );

                bool status = ReceiveData() != 0;

                if(status)
                {
                    checksum  = ReceiveData();
                    memoryAND = ReceiveData();
                }
                else
                {
                    checksum  = 0;
                    memoryAND = 0;
                }

                return status;
            }

            private bool Execute_EraseBlock( uint address )
            {
                SendData( cmd_Signature | cmd_EraseBlock );
                SendData( address                         );

                VerifyData( cmd_Signature | cmd_EraseBlock );

                return ReceiveData() != 0;
            }

            private bool Execute_ProgramMemory( uint   address ,
                                                uint[] data    ,
                                                uint   size    ,
                                                uint   offset  ,
                                                uint[] spare   )
            {
                Execute_ProgramMemory_Header( address, 1 );

                Execute_ProgramMemory_Data( data, size, offset, spare );

                return Execute_ProgramMemory_Feedback();
            }

            private void Execute_ProgramMemory_Header( uint address ,
                                                       uint pages   )
            {
                SendData( cmd_Signature | cmd_ProgramPage );
                SendData( address                         );
                SendData( pages                           );

                VerifyData( cmd_Signature | cmd_ProgramPage );
            }

            private void Execute_ProgramMemory_Data( uint[] data   ,
                                                     uint   size   ,
                                                     uint   offset ,
                                                     uint[] spare  )
            {
                Execute_ProgramMemory_Data( data , offset, size                                  , m_flashPageSizeInWords  );
                Execute_ProgramMemory_Data( spare,      0, spare != null ? (uint)spare.Length : 0, m_flashSpareSizeInWords );
            }

            private void Execute_ProgramMemory_Data( uint[] data     ,
                                                     uint   offset   ,
                                                     uint   count    ,
                                                     uint   required )
            {
                for(uint i = 0; i < required; i++)
                {
                    SendData( i < count ? data[offset++] : 0xFFFFFFFF );
                }
            }

            private bool Execute_ProgramMemory_Feedback()
            {
                return ReceiveData() != 0;
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

            //--//

            private void SplitIntoBlocks( uint   address ,
                                          byte[] data    )
            {
                uint   start            = address;
                uint   end              = address + (uint)data.Length;
                uint[] buf              = ToUint( data );
                int    bufLengthInWords = buf.Length;
                int    posInWords       = 0;

                while(posInWords < bufLengthInWords)
                {
                    uint offsetInBytes = address & (m_flashBlockSize - 1);
                    int  lenInBytes    = Math.Min( (int)(m_flashBlockSize - offsetInBytes), (bufLengthInWords - posInWords) * sizeof(uint) );

                    BlockEntry bs = GetBlock( address );

                    bs.MoveToState( BlockState.DataLoaded );

                    if(lenInBytes > 0)
                    {
                        Buffer.BlockCopy( buf, posInWords * sizeof(uint), bs.m_data, (int)offsetInBytes, lenInBytes );
                    }

                    address    += (uint)lenInBytes;
                    posInWords +=       lenInBytes / sizeof(uint);
                }
            }

            private BlockEntry GetBlock( uint address )
            {
                return m_blocks[(address / m_flashBlockSize) % m_blocks.Length];
            }

            private bool Redistribute( BlockEntry bs )
            {
                BlockEntry bsDst = FindFirstUntouchedBlock( bs );
                if(bsDst == null)
                {
                    return false;
                }

                //--//

                uint index = bsDst.m_index;

                while(true)
                {
                    BlockEntry bsSrc = m_blocks[--index];

                    bsDst.MoveLocalState( bsSrc );

                    if(bsSrc == bs)
                    {
                        break;
                    }

                    bsDst = bsSrc;
                }

                return true;
            }

            private BlockEntry FindFirstUntouchedBlock( BlockEntry bs )
            {
                for(uint i = bs.m_index + 1; i < m_blocks.Length; i++)
                {
                    BlockEntry bsNext = m_blocks[i];

                    if(bsNext.IsInitialized == false)
                    {
                        bsNext.MoveToState( BlockState.DataLoadedAndChecked );

                        return bsNext;
                    }
                }

                return null;
            }
        }

        //--//

        protected NandFlashJtagLoaderCategory( string name ,
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
                return new NandFlashLoader();
            }

            return base.GetServiceInner( t );
        }
    }
}
