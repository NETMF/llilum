//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.Emulation.ArmProcessor.FlashMemory
{
    using System;
    using System.Collections.Generic;

    using EncDef       = Microsoft.Zelig.TargetModel.ArmProcessor.EncodingDefinition;
    using ElementTypes = Microsoft.Zelig.MetaData.ElementTypes;

    //--//

    public class S29WS064 : Simulator.MemoryHandler
    {
        const uint Sector4KWord  =  4 * 1024 * sizeof(ushort);
        const uint Sector32KWord = 32 * 1024 * sizeof(ushort);
        const uint ChipSize      =  8 * 1024 * 1024;

        //--//

        static ushort[] sequence_CFI     = new ushort[] { 0x055, 0x0098 };

        static ushort[] sequence_PROGRAM = new ushort[] { 0x555, 0x00AA ,
                                                          0x2AA, 0x0055 ,
                                                          0x555, 0x00A0 };

        static ushort[] sequence_ERASE   = new ushort[] { 0x555, 0x00AA ,
                                                          0x2AA, 0x0055 ,
                                                          0x555, 0x0080 ,
                                                          0x555, 0x00AA ,
                                                          0x2AA, 0x0055 };

        enum Mode
        {
            Normal ,
            Query  ,
            Program,
            Erase  ,
        }

        class SequenceTracker
        {
            //
            // State
            //

            internal ushort[] m_sequence;
            internal int      m_pos;
            internal ulong    m_lastWrite;


            //
            // Constructor Methods
            //

            internal SequenceTracker( ushort[] sequence )
            {
                m_sequence = sequence;
                m_pos      = 0;
            }

            //
            // Helper Methods
            //

            internal void Reset()
            {
                m_pos = 0;
            }

            internal void Advance( uint      relativeAddress ,
                                   ushort    value           ,
                                   Simulator owner           )
            {
                //
                // If the writes are too spaced out, abort.
                //
                if(m_pos > 0 && owner.ClockTicks - m_lastWrite > 5 * 30)
                {
                    Reset();
                    return;
                }

                if(IsMatch( relativeAddress, value, m_sequence, m_pos ) == false)
                {
                    Reset();
                    return;
                }

                m_pos       += 2;
                m_lastWrite  = owner.ClockTicks;
            }

            private static bool IsMatch( uint     relativeAddress ,
                                         ushort   value           ,
                                         ushort[] sequence        ,
                                         int      pos             )
            {
                return (sequence[pos  ] == relativeAddress / sizeof(ushort) &&
                        sequence[pos+1] == value                             );
            }

            //
            // Access Methods
            //

            internal bool IsRecognized
            {
                get
                {
                    return m_pos == m_sequence.Length;
                }
            }
        }

        class BankState
        {
            static ushort[] query_results = new ushort[]
            {
                0x10, 0x0051, //
                0x11, 0x0052, // Query Unique ASCII string "QRY"
                0x12, 0x0059, //

                0x13, 0x0002, // Primary OEM Command Set 
                0x14, 0x0000, //

                0x15, 0x0040, // Address for Primary Extended Table
                0x16, 0x0000, //

                0x17, 0x0000, // Alternate OEM Command Set 
                0x18, 0x0000, //

                0x2C, 0x0003, // Number of Erase Block Regions within device.

                0x2D, 0x0007, // Erase Block Region 1 Information
                0x2E, 0x0000, //
                0x2F, 0x0020, //
                0x30, 0x0000, //

                0x31, 0x007D, // Erase Block Region 2 Information
                0x32, 0x0000, //
                0x33, 0x0000, //
                0x34, 0x0001, //

                0x35, 0x0007, // Erase Block Region 3 Information
                0x36, 0x0000, //
                0x37, 0x0020, //
                0x38, 0x0000, //
            };

            const ushort resetCommand = 0x00F0;
            const ushort eraseCommand = 0x0030;

            const ushort c_DQ7 = (ushort)(1u << 7);
            const ushort c_DQ6 = (ushort)(1u << 6);
            const ushort c_DQ3 = (ushort)(1u << 3);
            const ushort c_DQ2 = (ushort)(1u << 2);

            //
            // State
            //

            internal uint            m_addressStart;
            internal uint            m_addressEnd;
            internal uint[]          m_sectorSizes;
            internal Mode            m_mode;

            internal ushort          m_lastStatusValue;
            internal ulong           m_timer;

            internal int             m_erasingSectorIndex;
            internal uint            m_programmingAddress;
            internal ushort          m_programmingValue;

            //
            // Constructor Methods
            //

            internal BankState(        uint   addressStart     ,
                                params uint[] sectorDefinition )
            {
                uint totalSize    = 0;
                uint totalSectors = 0;

                for(int i = 0; i < sectorDefinition.Length; i += 2)
                {
                    uint sectorSize = sectorDefinition[i  ];
                    uint sectorNum  = sectorDefinition[i+1];

                    totalSectors += sectorNum;
                    totalSize    += sectorNum * sectorSize;
                }

                m_addressStart = addressStart;
                m_addressEnd   = addressStart + totalSize;
                m_sectorSizes  = new uint[totalSectors];
                m_mode         = Mode.Normal;

                uint offset = 0;

                for(int i = 0; i < sectorDefinition.Length; i += 2)
                {
                    uint sectorSize = sectorDefinition[i  ];
                    uint sectorNum  = sectorDefinition[i+1];

                    while(sectorNum-- > 0)
                    {
                        m_sectorSizes[offset++] = sectorSize;
                    }
                }
            }

            //
            // Helper Methods
            //

            internal void HandleWrite( Simulator owner   ,
                                       uint      address ,
                                       ushort    value   )
            {
                switch(m_mode)
                {
                    case Mode.Normal:
                        break;

                    case Mode.Query:
                        if(address == 0 && value == resetCommand)
                        {
                            m_mode = Mode.Normal;
                        }
                        break;

                    case Mode.Program:
                        m_programmingAddress = address;
                        m_programmingValue   = value;

                        m_timer           = owner.ClockTicks + 10 * 30; // BUGBUG: We need a way to convert from Ticks to Time.
                        m_lastStatusValue = (ushort)(~value & c_DQ7); // DQ7#
                        break;

                    case Mode.Erase:
                        uint offset = address - m_addressStart;

                        for(int i = 0; i < m_sectorSizes.Length; i++)
                        {
                            if(offset < m_sectorSizes[i])
                            {
                                m_erasingSectorIndex = i;
                                break;
                            }

                            offset -= m_sectorSizes[i];
                        }

                      //m_timer           = owner.ClockTicks + 400 * 1000 * 30; // BUGBUG: We need a way to convert from Ticks to Time.
                        m_timer           = owner.ClockTicks + 40 * 30; // BUGBUG: We need a way to convert from Ticks to Time.
                        m_lastStatusValue = c_DQ3;
                        break;
                }
            }

            internal bool HandleRead(     Simulator owner   ,
                                          uint      address ,
                                      out ushort    value   )
            {
                switch(m_mode)
                {
                    case Mode.Normal:
                        break;

                    case Mode.Query:
                        for(int i = 0; i < query_results.Length; i += 2)
                        {
                            if(query_results[i] == address / sizeof(ushort))
                            {
                                value = query_results[i+1];
                                return true;
                            }
                        }

                        value = 0xFFFF;
                        return true;

                    case Mode.Program:
                        if(owner.ClockTicks < m_timer)
                        {
                            value = m_lastStatusValue;

                            m_lastStatusValue ^= c_DQ6; // Toggle DQ6.
                            return true;
                        }

                        m_mode = Mode.Normal;
                        break;

                    case Mode.Erase:
                        if(owner.ClockTicks < m_timer)
                        {
                            value = m_lastStatusValue;

                            m_lastStatusValue ^= c_DQ6 | c_DQ2; // Toggle DQ6 and DQ2.
                            return true;
                        }

                        m_mode = Mode.Normal;
                        break;
                }

                value = 0xFFFF;

                return false;
            }

            internal bool FindSector(     uint address     ,
                                      out uint sectorStart ,
                                      out uint sectorEnd   )
            {
                if(m_addressStart <= address && address < m_addressEnd)
                {
                    uint sector = m_addressStart;

                    foreach(uint sectorSize in m_sectorSizes)
                    {
                        uint sectorNext = sector + sectorSize;

                        if(sector <= address && address < sectorNext)
                        {
                            sectorStart = sector;
                            sectorEnd   = sectorNext;
                            return true;
                        }

                        sector = sectorNext;
                    }
                }

                sectorStart = 0;
                sectorEnd   = 0;
                return false;
            }
        }

        //
        // State
        //

        BankState[]     m_banks;
        Mode            m_mode;
        SequenceTracker m_sequenceTracker_CFI;
        SequenceTracker m_sequenceTracker_Program;
        SequenceTracker m_sequenceTracker_Erase;

        //
        // Constructor Methods
        //

        public S29WS064()
        {
            m_banks = new BankState[4];
            m_mode  = Mode.Normal;

            //
            // Bank A
            //
            m_banks[0] = new BankState( 0, Sector4KWord, 8, Sector32KWord, 15 );

            //
            // Bank B
            //
            m_banks[1] = new BankState( m_banks[0].m_addressEnd, Sector32KWord, 48 );

            //
            // Bank C
            //
            m_banks[2] = new BankState( m_banks[1].m_addressEnd, Sector32KWord, 48 );

            //
            // Bank D
            //
            m_banks[3] = new BankState( m_banks[2].m_addressEnd, Sector32KWord, 48, Sector4KWord, 8 );

            //--//

            m_sequenceTracker_CFI     = new SequenceTracker( sequence_CFI     );
            m_sequenceTracker_Program = new SequenceTracker( sequence_PROGRAM );
            m_sequenceTracker_Erase   = new SequenceTracker( sequence_ERASE   );
        }

        //
        // Helper Methods
        //

        public override void Initialize( Simulator owner        ,
                                         ulong     rangeLength  ,
                                         uint      rangeWidth   ,
                                         uint      readLatency  ,
                                         uint      writeLatency )
        {
            base.Initialize( owner, rangeLength, rangeWidth, readLatency, writeLatency );

            //
            // Erase the whole chip.
            //
            for(int i = 0; i < m_target.Length; i++)
            {
                m_target[i] = 0xFFFFFFFF;
            }
        }

        public override uint Read( uint                                           address         ,
                                   uint                                           relativeAddress ,
                                   TargetAdapterAbstractionLayer.MemoryAccessType kind            )
        {
            if(m_owner.AreTimingUpdatesEnabled)
            {
                BankState bank;
                uint      sectorStart;
                uint      sectorEnd;

                if(FindSector( relativeAddress, out bank, out sectorStart, out sectorEnd ))
                {
                    ushort value;

                    if(bank.HandleRead( m_owner, relativeAddress, out value ))
                    {
                        UpdateClockTicksForLoad( address, kind );

                        return value;
                    }
                }
            }

            return base.Read( address, relativeAddress, kind );
        }

        public override void Write( uint                                           address         ,
                                    uint                                           relativeAddress ,
                                    uint                                           value           ,
                                    TargetAdapterAbstractionLayer.MemoryAccessType kind            )
        {
            if(m_owner.AreTimingUpdatesEnabled)
            {
                BankState bank;
                uint      sectorStart;
                uint      sectorEnd;

                if(kind != TargetAdapterAbstractionLayer.MemoryAccessType.UINT16)
                {
                    throw new TargetAdapterAbstractionLayer.BusErrorException( address, kind );
                }

                UpdateClockTicksForStore( address, kind );

                ushort valueShort = (ushort)value;

                if(m_mode == Mode.Normal)
                {
                    m_sequenceTracker_CFI    .Advance( relativeAddress, valueShort, m_owner );
                    m_sequenceTracker_Program.Advance( relativeAddress, valueShort, m_owner );
                    m_sequenceTracker_Erase  .Advance( relativeAddress, valueShort, m_owner );

                    if(m_sequenceTracker_CFI.IsRecognized)
                    {
                        foreach(var bank2 in m_banks)
                        {
                            bank2.m_mode = Mode.Query;
                        }

                        m_mode = Mode.Query;
                        return;
                    }

                    if(m_sequenceTracker_Program.IsRecognized)
                    {
                        m_mode = Mode.Program;
                        return;
                    }

                    if(m_sequenceTracker_Erase.IsRecognized)
                    {
                        m_mode = Mode.Erase;
                        return;
                    }
                }

                if(m_mode != Mode.Normal)
                {
                    m_sequenceTracker_CFI    .Reset();
                    m_sequenceTracker_Program.Reset();
                    m_sequenceTracker_Erase  .Reset();

                    if(FindSector( relativeAddress, out bank, out sectorStart, out sectorEnd ))
                    {
                        bank.m_mode = m_mode;

                        bank.HandleWrite( m_owner, relativeAddress, valueShort );

                        switch(m_mode)
                        {
                            case Mode.Query:
                                if(bank.m_mode == Mode.Normal)
                                {
                                    foreach(var bank2 in m_banks)
                                    {
                                        bank2.m_mode = Mode.Normal;
                                    }

                                    m_mode = Mode.Normal;
                                }
                                break;

                            case Mode.Program:
                                base.Write( relativeAddress, relativeAddress, value, kind );
                                m_mode = Mode.Normal;
                                break;

                            case Mode.Erase:
                                while(sectorStart < sectorEnd)
                                {
                                    base.Write( sectorStart, sectorStart, 0xFFFF, TargetAdapterAbstractionLayer.MemoryAccessType.UINT16 );

                                    sectorStart += sizeof(ushort);
                                }
                                m_mode = Mode.Normal;
                                break;
                        }
                    }
                }

                return;
            }

            base.Write( address, relativeAddress, value, kind );
        }

        //--//

        bool FindSector(     uint      address     ,
                         out BankState bank        ,
                         out uint      sectorStart ,
                         out uint      sectorEnd   )
        {
            foreach(BankState bank2 in m_banks)
            {
                if(bank2.FindSector( address, out sectorStart, out sectorEnd ))
                {
                    bank = bank2;
                    return true;
                }
            }

            bank        = null;
            sectorStart = 0;
            sectorEnd   = 0;
            return false;
        }
    }
}
