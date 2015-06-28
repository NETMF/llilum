//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Emulation.ArmProcessor
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Microsoft.Zelig.TargetModel.ArmProcessor;
    using Cfg          = Microsoft.Zelig.Configuration.Environment;
    using EncDef       = Microsoft.Zelig.TargetModel.ArmProcessor.EncodingDefinition_ARM;


    public abstract class SimulatorCore : Hosting.AbstractEngine
    {
        
        private static EncodingDefinition_ARM     s_Encoding    = (EncodingDefinition_ARM)CurrentInstructionSetEncoding.GetEncoding();
        private static EncodingDefinition_VFP_ARM s_EncodingVFP = (EncodingDefinition_VFP_ARM)CurrentInstructionSetEncoding.GetVFPEncoding();

        public delegate bool ProcessCoprocessorRegisterTransfer( ref uint value            ,
                                                                     bool fFromCoprocessor ,
                                                                     uint CRn              ,
                                                                     uint CRm              ,
                                                                     uint Op1              ,
                                                                     uint Op2              );

        public delegate bool ProcessCoprocessorDataTransfer( uint address ,
                                                             uint CRd     ,
                                                             bool fIsLoad ,
                                                             bool fLong   );

        public delegate bool ProcessCoprocessorDataOperation( uint CRn ,
                                                              uint CRm ,
                                                              uint CRd ,
                                                              uint Op1 ,
                                                              uint Op2 );

        //--//

        class DebugCommunicationChannelImpl : Emulation.Hosting.DebugCommunicationChannel
        {
            const uint c_ProcessorHasData = 0x1;
            const uint c_DebuggerHasData  = 0x2;

            //
            // State
            //

            SimulatorCore   m_owner;

            uint            m_status;
            Queue< uint >   m_fromProcessor;
            Queue< uint >   m_fromDebugger;

            uint            m_coproc;

            int             m_fromDebugger_Read_Pending;
            bool            m_fromDebugger_Read_Abort;

            AutoResetEvent  m_dataAvailableForDebuggerToRead;
            AutoResetEvent  m_dataAvailableForDebuggerToWrite;

            //
            // Constructor Methods
            //

            internal DebugCommunicationChannelImpl( uint coproc, SimulatorCore owner )
            {
                m_owner = owner;
                m_coproc = coproc;

                m_fromProcessor                   = new Queue< uint >();
                m_fromDebugger                    = new Queue< uint >();
                m_dataAvailableForDebuggerToRead  = new AutoResetEvent( false );
                m_dataAvailableForDebuggerToWrite = new AutoResetEvent( false );

                m_owner.RegisterService( typeof(Emulation.Hosting.DebugCommunicationChannel), this );
            }

            //
            // Helper Methods
            //

            internal void Attach()
            {
                m_owner.RegisterCoprocessorRegisterTransfer( m_coproc, ProcessCoprocessor_RegisterTransfer );
            }

            public override bool ReadFromProcessor( out uint value )
            {
                lock(this)
                {
                    if(m_fromDebugger.Count > 0)
                    {
                        value = m_fromDebugger.Dequeue();

                        m_dataAvailableForDebuggerToWrite.Set();

                        if(m_fromDebugger.Count == 0)
                        {
                            m_status &= ~c_ProcessorHasData;
                        }

                        return true;
                    }

                    value = 0;
                    return false;
                }
            }

            public override bool WriteFromProcessor( uint value )
            {
                lock(this)
                {
                    m_fromProcessor.Enqueue( value );

                    m_status |= c_DebuggerHasData;

                    m_dataAvailableForDebuggerToRead.Set();

                    return true;
                }
            }

            public override bool ReadFromDebugger( out uint value   ,
                                                       int  timeout )
            {
                while(true)
                {
                    lock(this)
                    {
                        if(m_fromProcessor.Count > 0)
                        {
                            value = m_fromProcessor.Dequeue();

                            if(m_fromProcessor.Count == 0)
                            {
                                m_status &= ~c_DebuggerHasData;
                            }

                            return true;
                        }

                        if(m_fromDebugger_Read_Abort)
                        {
                            if(m_fromDebugger_Read_Pending == 0)
                            {
                                m_fromDebugger_Read_Abort = false;
                            }

                            break;
                        }

                        m_fromDebugger_Read_Pending++;
                    }

                    bool fSignaled = m_dataAvailableForDebuggerToRead.WaitOne( timeout, false );

                    lock(this)
                    {
                        m_fromDebugger_Read_Pending--;

                        if(m_fromDebugger_Read_Abort)
                        {
                            if(m_fromDebugger_Read_Pending == 0)
                            {
                                m_fromDebugger_Read_Abort = false;
                            }

                            break;
                        }
                    }

                    if(fSignaled == false)
                    {
                        break;
                    }
                }

                value = 0;
                return false;
            }

            public override bool WriteFromDebugger( uint value   ,
                                                    int  timeout )
            {
                lock(this)
                {
                    m_fromDebugger.Enqueue( value );

                    m_status |= c_ProcessorHasData;

                    return true;
                }
            }

            public override void AbortDebuggerRead()
            {
                lock(this)
                {
                    m_fromDebugger_Read_Abort = true;

                    m_dataAvailableForDebuggerToRead.Set();
                }
            }

            public override void AbortDebuggerWrite()
            {
                //
                // Nothing to do, debugger writes are non-blocking.
                //
            }

            //--//

            private bool ProcessCoprocessor_RegisterTransfer( ref uint value            ,
                                                                  bool fFromCoprocessor ,
                                                                  uint CRn              ,
                                                                  uint CRm              ,
                                                                  uint Op1              ,
                                                                  uint Op2              )
            {
                if(fFromCoprocessor)
                {
                    if(Op1 == 0 && CRn ==  1 && CRm == 0 && Op2 == 0)
                    {
                        ReadFromProcessor( out value );
                        return true;
                    }

                    if(Op1 == 0 && CRn ==  0 && CRm == 0 && Op2 == 0)
                    {
                        value = m_status;
                        return true;
                    }

                    if(m_coproc == 15)
                    {
                        // TODO: Add support for this option (used by iMote)
                        //if(Op1 == 0 && CRn == 2 && CRm == 0 && Op2 == 0)
                        return true;
                    }
                }
                else
                {
                    if(Op1 == 0 && CRn ==  1 && CRm == 0 && Op2 == 0)
                    {
                        WriteFromProcessor( value );
                        return true;
                    }

                    // TODO: Add support for Coproc 15 - cache (used by iMote)
                    if(m_coproc == 15)
                    {
                        // 7, 5, 0, 0
                        // 8, 7, 0, 0
                        return true;
                    }
                    // TODO: Add support for this option (used by iMote)
                    else if(CRn == 6)
                    {
                        // coproc14 - 0, 6, 0 , 0, 3 - enable turbo
                        return true;
                    }
                }

                return false;
            }
        }

        class MemoryProviderImpl : Emulation.Hosting.MemoryProvider
        {
            //
            // State
            //

            SimulatorCore m_owner;

            //
            // Constructor Methods
            //

            internal MemoryProviderImpl( SimulatorCore               owner ,
                                         Cfg.CacheControllerCategory cache ) : base( owner, cache )
            {
                m_owner = owner;
            }

            //
            // Helper Methods
            //

            protected override bool CanAccessUInt8( uint address )
            {
                return m_owner.CanAccess( address, TargetAdapterAbstractionLayer.MemoryAccessType.UINT8 );
            }

            protected override bool CanAccessUInt16( uint address )
            {
                return m_owner.CanAccess( address, TargetAdapterAbstractionLayer.MemoryAccessType.UINT16 );
            }

            protected override bool CanAccessUInt32( uint address )
            {
                return m_owner.CanAccess( address, TargetAdapterAbstractionLayer.MemoryAccessType.UINT32 );
            }

            protected override bool CanAccessUInt64( uint address )
            {
                if(m_owner.CanAccess( address               , TargetAdapterAbstractionLayer.MemoryAccessType.UINT32 ) &&
                   m_owner.CanAccess( address + sizeof(uint), TargetAdapterAbstractionLayer.MemoryAccessType.UINT32 )  )
                {
                    return true;
                }

                return false;
            }

            //--//

            protected override byte ReadUInt8( uint address )
            {
                return (byte)m_owner.Load( address, TargetAdapterAbstractionLayer.MemoryAccessType.UINT8 );
            }

            protected override ushort ReadUInt16( uint address )
            {
                return (ushort)m_owner.Load( address, TargetAdapterAbstractionLayer.MemoryAccessType.UINT16 );
            }

            protected override uint ReadUInt32( uint address )
            {
                return m_owner.Load( address, TargetAdapterAbstractionLayer.MemoryAccessType.UINT32 );
            }

            protected override ulong ReadUInt64( uint address )
            {
                uint resultLow  = m_owner.Load( address               , TargetAdapterAbstractionLayer.MemoryAccessType.UINT32 );
                uint resultHigh = m_owner.Load( address + sizeof(uint), TargetAdapterAbstractionLayer.MemoryAccessType.UINT32 );

                return (ulong)resultHigh << 32 | resultLow;
            }

            protected override byte[] ReadBlock( uint address   ,
                                                 int  size      ,
                                                 uint alignment )
            {
                byte[] res = new byte[size];

                if(alignment == 4)
                {
                    uint[] tmp = new uint[size / sizeof(uint)];

                    for(uint pos = 0; pos < tmp.Length; pos++)
                    {
                        tmp[pos] = m_owner.Load( address + pos * sizeof(uint), TargetAdapterAbstractionLayer.MemoryAccessType.UINT32 );
                    }

                    Buffer.BlockCopy( tmp, 0, res, 0, size );
                }
                else
                {
                    for(uint pos = 0; pos < size; pos++)
                    {
                        res[pos] = (byte)m_owner.Load( address + pos * sizeof(byte), TargetAdapterAbstractionLayer.MemoryAccessType.UINT8 );
                    }
                }

                return res;
            }

            //--//

            protected override void WriteUInt8( uint address ,
                                                byte result  )
            {
                m_owner.Store( address, result, TargetAdapterAbstractionLayer.MemoryAccessType.UINT8 );
            }

            protected override void WriteUInt16( uint   address ,
                                                 ushort result  )
            {
                m_owner.Store( address, result, TargetAdapterAbstractionLayer.MemoryAccessType.UINT16 );
            }

            protected override void WriteUInt32( uint address ,
                                                 uint result  )
            {
                m_owner.Store( address, result, TargetAdapterAbstractionLayer.MemoryAccessType.UINT32 );
            }
            
            protected override void WriteUInt64( uint  address ,
                                                 ulong result  )
            {
                m_owner.Store( address               , (uint) result       , TargetAdapterAbstractionLayer.MemoryAccessType.UINT32 );
                m_owner.Store( address + sizeof(uint), (uint)(result >> 32), TargetAdapterAbstractionLayer.MemoryAccessType.UINT32 );
            }

            protected override void WriteBlock( uint   address   ,
                                                byte[] result    ,
                                                uint   alignment )
            {
                int size = result.Length;

                if(alignment == 4)
                {
                    uint[] tmp = new uint[size / sizeof(uint)];

                    Buffer.BlockCopy( result, 0, tmp, 0, size );

                    for(uint pos = 0; pos < tmp.Length; pos++)
                    {
                        m_owner.Store( address + pos * sizeof(uint), tmp[pos], TargetAdapterAbstractionLayer.MemoryAccessType.UINT32 );
                    }
                }
                else
                {
                    for(uint pos = 0; pos < size; pos++)
                    {
                        m_owner.Store( address + pos * sizeof(byte), result[pos], TargetAdapterAbstractionLayer.MemoryAccessType.UINT8 );
                    }
                }
            }
        }

        //--//

        public struct TimingState
        {
            //
            // State
            //

            internal int   m_recursionCount;
            internal ulong m_clockTicks;
            internal ulong m_sleepTicks;
            internal ulong m_busAccess_Read;
            internal ulong m_busAccess_Write;
            internal ulong m_busAccess_WaitStates;
        }

        public class TimingStateSmartHandler : IDisposable
        {
            //
            // State
            //

            SimulatorCore m_owner;
            TimingState   m_backup;

            //
            // Constructor Methods
            //

            internal TimingStateSmartHandler( SimulatorCore owner )
            {
                m_owner = owner;

                owner.SuspendTimingUpdates( ref m_backup );
            }

            //
            // Helper Methods
            //

            public void Dispose()
            {
                m_owner.ResumeTimingUpdates( ref m_backup );
            }
        }

        protected struct TrackExecution
        {
            //
            // State
            //

            ulong m_clockTicks;
            ulong m_busAccess_Read;
            ulong m_busAccess_Write;
            ulong m_busAccess_WaitStates;

            //--//

            internal void Start( Simulator st )
            {
                m_clockTicks           = st.m_clockTicks          ;
                m_busAccess_Read       = st.m_busAccess_Read      ;
                m_busAccess_Write      = st.m_busAccess_Write     ;
                m_busAccess_WaitStates = st.m_busAccess_WaitStates;
            }

            internal void End( Simulator st )
            {
                m_clockTicks           = st.m_clockTicks           - m_clockTicks          ;
                m_busAccess_Read       = st.m_busAccess_Read       - m_busAccess_Read      ;
                m_busAccess_Write      = st.m_busAccess_Write      - m_busAccess_Write     ;
                m_busAccess_WaitStates = st.m_busAccess_WaitStates - m_busAccess_WaitStates;
            }

            public override string ToString()
            {
                return String.Format( "C:{0} R:{1} W:{2} WS:{3}", m_clockTicks, m_busAccess_Read, m_busAccess_Write, m_busAccess_WaitStates );
            }
        }

        protected class TrackCall
        {
            //
            // State
            //

            internal uint           m_pc;
            internal uint           m_lr;
            internal uint           m_sp;
            internal string         m_name;

            internal TrackExecution m_te;
        }

        //--//

        protected enum RegisterLookup : uint
        {
            R0       = EncDef.c_register_r0 ,
            R1       = EncDef.c_register_r1 ,
            R2       = EncDef.c_register_r2 ,
            R3       = EncDef.c_register_r3 ,
            R4       = EncDef.c_register_r4 ,
            R5       = EncDef.c_register_r5 ,
            R6       = EncDef.c_register_r6 ,
            R7       = EncDef.c_register_r7 ,
            R8       = EncDef.c_register_r8 ,
            R9       = EncDef.c_register_r9 ,
            R10      = EncDef.c_register_r10,
            R11      = EncDef.c_register_r11,
            R12      = EncDef.c_register_r12,
            R13      = EncDef.c_register_r13,
            R14      = EncDef.c_register_r14,
            PC       = EncDef.c_register_r15,
            CPSR     = EncDef.c_register_cpsr,
            SPSR     = EncDef.c_register_spsr,

            R13_svc ,
            R14_svc ,
            SPSR_svc,

            R13_abt ,
            R14_abt ,
            SPSR_abt,

            R13_und ,
            R14_und ,
            SPSR_und,

            R13_irq ,
            R14_irq ,
            SPSR_irq,

            R8_fiq  ,
            R9_fiq  ,
            R10_fiq ,
            R11_fiq ,
            R12_fiq ,
            R13_fiq ,
            R14_fiq ,
            SPSR_fiq,

            REALMAX = SPSR     + 1,
            TOTAL   = SPSR_fiq + 1,
        }

        protected class BankedRegisters
        {
            internal uint              m_mode;
            internal uint[]            m_lookup;
            internal List< TrackCall > m_callQueue;

            //
            // Constructor Methods
            //

            internal BankedRegisters( uint mode )
            {
                m_mode      = mode;
                m_lookup    = new uint[(int)RegisterLookup.REALMAX];
                m_callQueue = new List< TrackCall >();

                for(RegisterLookup idx = RegisterLookup.R0; idx < RegisterLookup.REALMAX; idx++)
                {
                    SetLookup( idx, idx );
                }

                switch(mode)
                {
                    case EncDef.c_psr_mode_USER:
                        return;

                    case EncDef.c_psr_mode_FIQ:
                        SetLookup( RegisterLookup.R8  , RegisterLookup.R8_fiq   );
                        SetLookup( RegisterLookup.R9  , RegisterLookup.R9_fiq   );
                        SetLookup( RegisterLookup.R10 , RegisterLookup.R10_fiq  );
                        SetLookup( RegisterLookup.R11 , RegisterLookup.R11_fiq  );
                        SetLookup( RegisterLookup.R12 , RegisterLookup.R12_fiq  );
                        SetLookup( RegisterLookup.R13 , RegisterLookup.R13_fiq  );
                        SetLookup( RegisterLookup.R14 , RegisterLookup.R14_fiq  );
                        SetLookup( RegisterLookup.SPSR, RegisterLookup.SPSR_fiq );
                        break;

                    case EncDef.c_psr_mode_IRQ:
                        SetLookup( RegisterLookup.R13 , RegisterLookup.R13_irq  );
                        SetLookup( RegisterLookup.R14 , RegisterLookup.R14_irq  );
                        SetLookup( RegisterLookup.SPSR, RegisterLookup.SPSR_irq );
                        break;

                    case EncDef.c_psr_mode_SVC:
                        SetLookup( RegisterLookup.R13 , RegisterLookup.R13_svc  );
                        SetLookup( RegisterLookup.R14 , RegisterLookup.R14_svc  );
                        SetLookup( RegisterLookup.SPSR, RegisterLookup.SPSR_svc );
                        break;

                    case EncDef.c_psr_mode_ABORT:
                        SetLookup( RegisterLookup.R13 , RegisterLookup.R13_abt  );
                        SetLookup( RegisterLookup.R14 , RegisterLookup.R14_abt  );
                        SetLookup( RegisterLookup.SPSR, RegisterLookup.SPSR_abt );
                        break;

                    case EncDef.c_psr_mode_UNDEF:
                        SetLookup( RegisterLookup.R13 , RegisterLookup.R13_und  );
                        SetLookup( RegisterLookup.R14 , RegisterLookup.R14_und  );
                        SetLookup( RegisterLookup.SPSR, RegisterLookup.SPSR_und );
                        break;
                }
            }

            //
            // Helper Methods
            //

            private void SetLookup( RegisterLookup realRegister   ,
                                    RegisterLookup bankedRegister )
            {
                m_lookup[(uint)realRegister] = (uint)bankedRegister;
            }
        }

        //
        // State
        //

        protected Cfg.ProductCategory                                                  m_product;
        protected ulong                                                                m_clockFrequency;
                                                                                   
        protected int                                                                  m_suspendCount;
        protected ulong                                                                m_clockTicks;
        protected ulong                                                                m_sleepTicks;
        protected ulong                                                                m_busAccess_Read;
        protected ulong                                                                m_busAccess_Write;
        protected ulong                                                                m_busAccess_WaitStates;
                                                                                   
        protected uint                                                                 m_pc;
        private   uint[]                                                               m_registerFile = new uint[(int)RegisterLookup.TOTAL];
        protected uint                                                                 m_cpsr;
        protected uint                                                                 m_interruptStatus;
                                                                                   
        protected BankedRegisters                                                      m_currentBank;
        protected BankedRegisters                                                      m_registers_Mode_USER  = new BankedRegisters( EncDef.c_psr_mode_USER  );
        protected BankedRegisters                                                      m_registers_Mode_FIQ   = new BankedRegisters( EncDef.c_psr_mode_FIQ   );
        protected BankedRegisters                                                      m_registers_Mode_IRQ   = new BankedRegisters( EncDef.c_psr_mode_IRQ   );
        protected BankedRegisters                                                      m_registers_Mode_SVC   = new BankedRegisters( EncDef.c_psr_mode_SVC   );
        protected BankedRegisters                                                      m_registers_Mode_ABORT = new BankedRegisters( EncDef.c_psr_mode_ABORT );
        protected BankedRegisters                                                      m_registers_Mode_UNDEF = new BankedRegisters( EncDef.c_psr_mode_UNDEF );
                                                                                   
        protected uint[]                                                               m_lookupConditions     = new uint[EncDef.c_psr_cc_num];
                                                                                   
        private   DebugCommunicationChannelImpl                                        m_implDebugCommunicationChannel;
        private   DebugCommunicationChannelImpl                                        m_implDebugCommunicationChannel2;
        private   MemoryProviderImpl                                                   m_memoryProviderImpl;

        private   GrowOnlyHashTable< uint, ProcessCoprocessorRegisterTransfer > m_coprocessorsRegisterTransfer = HashTableFactory.New< uint, ProcessCoprocessorRegisterTransfer >();
        private   GrowOnlyHashTable< uint, ProcessCoprocessorDataTransfer     > m_coprocessorsDataTransfer     = HashTableFactory.New< uint, ProcessCoprocessorDataTransfer     >();
        private   GrowOnlyHashTable< uint, ProcessCoprocessorDataOperation    > m_coprocessorsDataOperation    = HashTableFactory.New< uint, ProcessCoprocessorDataOperation    >();

        //
        // Constructor Methods
        //

        protected SimulatorCore(InstructionSet iset) : base(iset)
        {
            m_pc          = 0;
            m_cpsr        = EncDef.c_psr_I | EncDef.c_psr_F | EncDef.c_psr_mode_SVC;
            m_currentBank = m_registers_Mode_SVC;

            //
            // Precompute a lookup table for condition codes.
            //
            for(int value = 0; value < EncDef.c_psr_cc_num; value++)
            {
                uint res = 0;
                uint psr = (uint)(value << (int)EncDef.c_psr_bit_V);

                if(Zero(psr) != 0) res |= 1U << (int)EncDef.c_cond_EQ;
                if(Zero(psr) == 0) res |= 1U << (int)EncDef.c_cond_NE;
                if(Carry(psr) != 0) res |= 1U << (int)EncDef.c_cond_CS;
                if(Carry(psr) == 0) res |= 1U << (int)EncDef.c_cond_CC;
                if(Negative(psr) != 0) res |= 1U << (int)EncDef.c_cond_MI;
                if(Negative(psr) == 0) res |= 1U << (int)EncDef.c_cond_PL;
                if(Overflow(psr) != 0) res |= 1U << (int)EncDef.c_cond_VS;
                if(Overflow(psr) == 0) res |= 1U << (int)EncDef.c_cond_VC;
                if(Zero(psr) == 0 &&  Carry(psr) != 0) res |= 1U << (int)EncDef.c_cond_HI;
                if(Zero(psr) != 0 ||  Carry(psr) == 0) res |= 1U << (int)EncDef.c_cond_LS;
                if(Negative(psr) == Overflow(psr)) res |= 1U << (int)EncDef.c_cond_GE;
                if(Negative(psr) != Overflow(psr)) res |= 1U << (int)EncDef.c_cond_LT;
                if(Zero(psr) == 0 &&  Negative(psr) == Overflow(psr)) res |= 1U << (int)EncDef.c_cond_GT;
                if(Zero(psr) != 0 ||  Negative(psr) != Overflow(psr)) res |= 1U << (int)EncDef.c_cond_LE;
                res |= 1U << (int)EncDef.c_cond_AL;
                m_lookupConditions[value] = res;
            }

            m_implDebugCommunicationChannel = new DebugCommunicationChannelImpl(14, this);
            m_implDebugCommunicationChannel2 = new DebugCommunicationChannelImpl(15, this);
            m_memoryProviderImpl = new MemoryProviderImpl(this, null);
        }

        //--//

        //
        // Helper Methods
        //

        //--//

        public abstract bool CanAccess         ( uint address,             TargetAdapterAbstractionLayer.MemoryAccessType kind );
        public abstract uint Load              ( uint address,             TargetAdapterAbstractionLayer.MemoryAccessType kind );
        public abstract void Store             ( uint address, uint value, TargetAdapterAbstractionLayer.MemoryAccessType kind );
        public abstract uint TimeMemoryAccess  ( uint address            , TargetAdapterAbstractionLayer.MemoryAccessType kind );
        public abstract uint GetPhysicalAddress( uint address                                                                  );

        //--//

        protected virtual void SetHardwareModel( Cfg.ProductCategory product )
        {
            m_product = product;

            Cfg.ProcessorCategory proc = product.SearchValue< Cfg.ProcessorCategory >();

            m_clockFrequency = proc.CoreClockFrequency;
        }

        public virtual void Reset()
        {
            StopPlugIns();

            //--//

            ResetClockTicks();

            m_pc   = 0;
            m_cpsr = EncDef.c_psr_I | EncDef.c_psr_F | EncDef.c_psr_mode_SVC;

            Array.Clear( m_registerFile, 0, m_registerFile.Length );

            m_coprocessorsRegisterTransfer.Clear();
            m_coprocessorsDataTransfer    .Clear();
            m_coprocessorsDataOperation   .Clear();

            SetRegisterDirect( RegisterLookup.SPSR    , m_cpsr );
            SetRegisterDirect( RegisterLookup.SPSR_fiq, m_cpsr );
            SetRegisterDirect( RegisterLookup.SPSR_irq, m_cpsr );
            SetRegisterDirect( RegisterLookup.SPSR_svc, m_cpsr );
            SetRegisterDirect( RegisterLookup.SPSR_abt, m_cpsr );
            SetRegisterDirect( RegisterLookup.SPSR_und, m_cpsr );

            m_implDebugCommunicationChannel.Attach();
            m_implDebugCommunicationChannel2.Attach();
        }

        //--//

        public void UpdateSleepTicks( long slept )
        {
            if(this.AreTimingUpdatesEnabled)
            {
                m_sleepTicks += (ulong)slept;
                m_clockTicks += (ulong)slept;
            }
        }

        //--//

        public uint GetRegister( uint idx )
        {
            return m_registerFile[ m_currentBank.m_lookup[ idx ] ];
        }

        public void SetRegister( uint idx   ,
                                 uint value )
        {
            m_registerFile[ m_currentBank.m_lookup[ idx ] ] = value;
        }

        //--//

        protected uint GetRegister( RegisterLookup idx )
        {
            return GetRegister( (uint)idx );
        }

        protected void SetRegister( RegisterLookup idx   ,
                                    uint           value )
        {
            SetRegister( (uint)idx, value );
        }

        protected uint GetRegisterDirect( RegisterLookup reg )
        {
            return m_registerFile[(int)reg];
        }

        protected void SetRegisterDirect( RegisterLookup reg   ,
                                          uint           value )
        {
            m_registerFile[(int)reg] = value;
        }

        //--//

        public void SetIrqStatus( bool fActive )
        {
            if(fActive) m_interruptStatus |=  EncDef.c_psr_I;
            else        m_interruptStatus &= ~EncDef.c_psr_I;
        }

        public void SetFiqStatus( bool fActive )
        {
            if(fActive) m_interruptStatus |=  EncDef.c_psr_F;
            else        m_interruptStatus &= ~EncDef.c_psr_F;
        }

        public void SetResetVector( uint resetVector )
        {
            m_pc = resetVector;
        }

        public virtual void SwitchMode( uint mode )
        {
            m_currentBank = GetShadowRegisters( mode );
            m_cpsr        = mode;
        }

        //--//

        protected BankedRegisters GetShadowRegisters( uint mode )
        {
            switch(mode & EncDef.c_psr_mode)
            {
                case EncDef.c_psr_mode_FIQ  : return m_registers_Mode_FIQ;
                case EncDef.c_psr_mode_IRQ  : return m_registers_Mode_IRQ;
                case EncDef.c_psr_mode_SVC  : return m_registers_Mode_SVC;
                case EncDef.c_psr_mode_ABORT: return m_registers_Mode_ABORT;
                case EncDef.c_psr_mode_UNDEF: return m_registers_Mode_UNDEF;
            }

            return m_registers_Mode_USER;
        }

        //--//

        public bool CheckConditions( uint conditionField )
        {
            return (m_lookupConditions[ (m_cpsr >> EncDef.c_psr_cc_shift) & EncDef.c_psr_cc_mask ] & (1 << (int)conditionField )) != 0;
        }

        public static uint Negative( uint psr ) { return EncDef.OPCODE_DECODE_EXTRACTFIELD( psr, EncDef.c_psr_bit_N, 1 ); }
        public static uint Zero    ( uint psr ) { return EncDef.OPCODE_DECODE_EXTRACTFIELD( psr, EncDef.c_psr_bit_Z, 1 ); }
        public static uint Carry   ( uint psr ) { return EncDef.OPCODE_DECODE_EXTRACTFIELD( psr, EncDef.c_psr_bit_C, 1 ); }
        public static uint Overflow( uint psr ) { return EncDef.OPCODE_DECODE_EXTRACTFIELD( psr, EncDef.c_psr_bit_V, 1 ); }

        public uint Negative() { return Negative( m_cpsr ); }
        public uint Zero    () { return Zero    ( m_cpsr ); }
        public uint Carry   () { return Carry   ( m_cpsr ); }
        public uint Overflow() { return Overflow( m_cpsr ); }

        //--//

        public void SaveTimingUpdates( ref TimingState state )
        {
            state.m_clockTicks           = m_clockTicks          ;
            state.m_sleepTicks           = m_sleepTicks          ;
            state.m_busAccess_Read       = m_busAccess_Read      ;
            state.m_busAccess_Write      = m_busAccess_Write     ;
            state.m_busAccess_WaitStates = m_busAccess_WaitStates;
        }

        public void RestoreTimingUpdates( ref TimingState state )
        {
            m_clockTicks           = state.m_clockTicks          ;
            m_sleepTicks           = state.m_sleepTicks          ;
            m_busAccess_Read       = state.m_busAccess_Read      ;
            m_busAccess_Write      = state.m_busAccess_Write     ;
            m_busAccess_WaitStates = state.m_busAccess_WaitStates;
        }

        public void SuspendTimingUpdates( ref TimingState state )
        {
            if(state.m_recursionCount++ == 0)
            {
                SaveTimingUpdates( ref state );
            }

            m_suspendCount++;
        }

        public void ResumeTimingUpdates( ref TimingState state )
        {
            m_suspendCount--;

            if(--state.m_recursionCount == 0)
            {
                RestoreTimingUpdates( ref state );
            }
        }

        protected void ResetClockTicks()
        {
            m_suspendCount         = 0;
            m_clockTicks           = 0;
            m_sleepTicks           = 0;
            m_busAccess_Read       = 0;
            m_busAccess_Write      = 0;
            m_busAccess_WaitStates = 0;
        }

        protected void Execute( uint op )
        {
            uint                                           Operand2;
            uint                                           shifterCarry;
            uint                                           shiftType;
            uint                                           dataPointer;
            TargetAdapterAbstractionLayer.MemoryAccessType kind;

            {
                uint pcNext = m_pc + 4;

                m_pc                                 = pcNext;
                m_registerFile[EncDef.c_register_pc] = pcNext + 4;
            }

            if(CheckConditions( s_Encoding.get_ConditionCodes( op ) ) == false)
            {
                return;
            }

            switch(EncDef.OPCODE_DECODE_EXTRACTFIELD( op, 25, 3 ))
            {
                case 0: // 00000000[0E000000]
                //PARTIAL HIT MRS:
                //PARTIAL HIT MSR_1:
                //PARTIAL HIT DataProcessing_2:
                //PARTIAL HIT DataProcessing_3:
                //PARTIAL HIT Multiply:
                //PARTIAL HIT MultiplyLong:
                //PARTIAL HIT SingleDataSwap:
                //PARTIAL HIT BranchAndExchange:
                //PARTIAL HIT HalfwordDataTransfer_1:
                //PARTIAL HIT HalfwordDataTransfer_2:
                    switch(EncDef.OPCODE_DECODE_EXTRACTFIELD(op,4,4))
                    {
                        case 0: // 00000000[0e0000f0]
                        //PARTIAL HIT MRS:
                        //PARTIAL HIT MSR_1:
                        //PARTIAL HIT DataProcessing_2:
                            if((op & EncDef.opmask_MRS  ) == EncDef.op_MRS  ) goto parse_MRS;
                            if((op & EncDef.opmask_MSR_1) == EncDef.op_MSR_1) goto parse_MSR_1;

                            goto parse_DataProcessing_2;


                        case 1: // 00000010[0e0000f0]
                        //PARTIAL HIT DataProcessing_3:
                        //PARTIAL HIT BranchAndExchange:
                            if((op & EncDef.opmask_BranchAndExchange) == EncDef.op_BranchAndExchange) goto parse_BranchAndExchange;

                            goto parse_DataProcessing_3;


                        case 2: // 00000020[0e0000f0]
                        case 4: // 00000040[0e0000f0]
                        case 6: // 00000060[0e0000f0]
                        case 8: // 00000080[0e0000f0]
                        case 10: // 000000a0[0e0000f0]
                        case 12: // 000000c0[0e0000f0]
                        case 14: // 000000e0[0e0000f0]
                        //HIT DataProcessing_2:
                            goto parse_DataProcessing_2;


                        case 3: // 00000030[0e0000f0]
                        case 5: // 00000050[0e0000f0]
                        //HIT DataProcessing_3:
                            goto parse_DataProcessing_3;

                        case 7: // 00000070[0e0000f0]
                        //PARTIAL HIT DataProcessing_3:
                        //PARTIAL HIT Breakpoint:
                            if((op & EncDef.opmask_Breakpoint) == EncDef.op_Breakpoint) goto parse_Breakpoint;

                            //HIT DataProcessing_3:
                            goto parse_DataProcessing_3;

                        case 9: // 00000090[0e0000f0]
                        //PARTIAL HIT Multiply:
                        //PARTIAL HIT MultiplyLong:
                        //PARTIAL HIT SingleDataSwap:
                            // +---------+---+---+---#########---+---+---+---------+---------+---------+---+---+---+---+---------+
                            // | Cond    | 0 | 0 | 0 # 0 | 0 # 0 | A | S | Rd      | Rn      | Rs      | 1 | 0 | 0 | 1 | Rm      | Multiply
                            // +---------+---+---+---#---+---#---+---+---+---------+---------+---------+---+---+---+---+---------+
                            // | Cond    | 0 | 0 | 0 # 0 | 1 # U | A | S | RdHi    | RdLo    | Rn      | 1 | 0 | 0 | 1 | Rm      | Multiply Long
                            // +---------+---+---+---#---+---#---+---+---+---------+---------+---------+---+---+---+---+---------+
                            // | Cond    | 0 | 0 | 0 # 1 | 0 # B | 0 | 0 | Rn      | Rd      | 0 0 0 0 | 1 | 0 | 0 | 1 | Rm      | Single Data Swap
                            // +---------+---+---+---#########---+---+---+---------+---------+---------+---+---+---+---+---------+
                            switch(EncDef.OPCODE_DECODE_EXTRACTFIELD( op, 23, 2 ))
                            {
                                case 0: // 00000090[0f8000f0]
                                //HIT Multiply:
                                    goto parse_Multiply;


                                case 1: // 00800090[0f8000f0]
                                //HIT MultiplyLong:
                                    goto parse_MultiplyLong;


                                case 2: // 01000090[0f8000f0]
                                //HIT SingleDataSwap:
                                    goto parse_SingleDataSwap;


                                case 3: // 01800090[0f8000f0]
                                //UNDEFINED
                                    break;
                            }
                            break;

                        case 11: // 000000b0[0e0000f0]
                        case 13: // 000000d0[0e0000f0]
                        case 15: // 000000f0[0e0000f0]
                        //PARTIAL HIT HalfwordDataTransfer_1:
                        //PARTIAL HIT HalfwordDataTransfer_2:
                            {
                                // +---------+---+---+---+---+---#####---+---+---------+---------+---------+---+---+---+---+---------+
                                // | Cond    | 0 | 0 | 0 | P | U # 0 # W | L | Rn      | Rd      | 0 0 0 0 | 1 | S | H | 1 | Rm      | Halfword Data Transfer: register offset
                                // +---------+---+---+---+---+---#####---+---+---------+---------+---------+---+---+---+---+---------+
                                // | Cond    | 0 | 0 | 0 | P | U # 1 # W | L | Rn      | Rd      | Offset  | 1 | S | H | 1 | Offset  | Halfword Data Transfer: immediate offset
                                // +---------+---+---+---+---+---#####---+---+---------+---------+---------+---+---+---+---+---------+
                                if(EncDef.OPCODE_DECODE_CHECKFLAG( op, 22 ))
                                {
                                    Operand2 = s_Encoding.get_HalfWordDataTransfer_Offset( op );
                                }
                                else
                                {
                                    Operand2 = GetRegister( s_Encoding.get_Register4( op ) );
                                }

                                goto parse_HalfwordDataTransfer;
                            }
                    }
                    break;

                case 1: // 02000000[0E000000]
                //PARTIAL HIT MSR_2:
                //PARTIAL HIT DataProcessing_1:
                    if((op & EncDef.opmask_MSR_2) == EncDef.op_MSR_2) goto parse_MSR_2;

                    goto parse_DataProcessing_1;


                case 2: // 04000000[0E000000]
                //HIT SingleDataTransfer_1:
                    goto parse_SingleDataTransfer_1;


                case 3: // 06000000[0E000000]
                //PARTIAL HIT SingleDataTransfer_2:
                //PARTIAL HIT SingleDataTransfer_3:
                //PARTIAL HIT Undefined:
                    if((op & EncDef.opmask_SingleDataTransfer_2) == EncDef.op_SingleDataTransfer_2) goto parse_SingleDataTransfer_2;
                    if((op & EncDef.opmask_SingleDataTransfer_3) == EncDef.op_SingleDataTransfer_3) goto parse_SingleDataTransfer_3;
                    if((op & EncDef.opmask_Undefined           ) == EncDef.op_Undefined           ) goto parse_Undefined;
                    break;

                case 4: // 08000000[0E000000]
                //HIT BlockDataTransfer:
                    goto parse_BlockDataTransfer;


                case 5: // 0A000000[0E000000]
                //HIT Branch:
                    goto parse_Branch;


                case 6: // 0C000000[0E000000]
                //HIT CoprocDataTransfer:
                    goto parse_CoprocDataTransfer;

                case 7: // 0E000000[0E000000]
                //PARTIAL HIT CoprocDataOperation:
                //PARTIAL HIT CoprocRegisterTransfer:
                //PARTIAL HIT SoftwareInterrupt:
                    if((op & EncDef.opmask_CoprocDataOperation   ) == EncDef.op_CoprocDataOperation   ) goto parse_CoprocDataOperation;
                    if((op & EncDef.opmask_CoprocRegisterTransfer) == EncDef.op_CoprocRegisterTransfer) goto parse_CoprocRegisterTransfer;
                    if((op & EncDef.opmask_SoftwareInterrupt     ) == EncDef.op_SoftwareInterrupt     ) goto parse_SoftwareInterrupt;
                    break;
            }

            throw new NotSupportedException();

            //--//

        parse_MRS:
            {
                uint res;

                if(s_Encoding.get_StatusRegister_IsSPSR( op ))
                {
                    res = GetRegister( RegisterLookup.SPSR );
                }
                else
                {
                    res = m_cpsr;
                }

                SetRegister( s_Encoding.get_Register2( op ), res );
                return;
            }

        parse_MSR_1:
            {
                Operand2 = GetRegister( s_Encoding.get_Register4( op ) );

                goto parse_MSR;
            }

        parse_MSR_2:
            {
                Operand2 = s_Encoding.get_DataProcessing_ImmediateValue( op );

                goto parse_MSR;
            }

        parse_MSR:
            {
                uint fields = s_Encoding.get_StatusRegister_Fields( op );

                if(s_Encoding.get_StatusRegister_IsSPSR( op ))
                {
                    uint psr = GetRegister( RegisterLookup.SPSR );

                    psr = Execute_MSR( psr, Operand2, fields );

                    SetRegister( RegisterLookup.SPSR, psr );
                }
                else
                {
                    if((m_cpsr & EncDef.c_psr_mode) == EncDef.c_psr_mode_USER)
                    {
                        //
                        // Things not allowed in user mode.
                        //
                        if((fields & (EncDef.c_psr_field_c | EncDef.c_psr_field_x | EncDef.c_psr_field_s)) != 0)
                        {
                            throw new NotSupportedException();
                        }
                    }

                    uint cpsr = Execute_MSR( m_cpsr, Operand2, fields );

                    ProcessInterruptDisabling( m_cpsr, cpsr );

                    SwitchMode( cpsr );
                }

                return;
            }

            //--//

        parse_DataProcessing_1:
            {
                Operand2     = s_Encoding.get_DataProcessing_ImmediateValue   ( op );
                shifterCarry = s_Encoding.get_DataProcessing_ImmediateRotation( op ) != 0 ? (Operand2 >> 31) : this.Carry();

                goto parse_DataProcessing;
            }

        parse_DataProcessing_2:
            {
                shiftType = s_Encoding.get_Shift_Type     ( op );
                Operand2  = s_Encoding.get_Shift_Immediate( op );

                if(Operand2 == 0)
                {
                    switch(shiftType)
                    {
                        case EncDef.c_shift_LSR:
                        case EncDef.c_shift_ASR:
                            Operand2 = 32;
                            break;

                        case EncDef.c_shift_ROR:
                            Operand2  = 1;
                            shiftType = EncDef.c_shift_RRX;
                            break;
                    }
                }

                goto parse_DataProcessing_2and3;
            }

        parse_DataProcessing_3:
            {
                shiftType =              s_Encoding.get_Shift_Type    ( op );
                Operand2  = GetRegister( s_Encoding.get_Shift_Register( op ) );

                //
                // Extra internal cycle.
                //
                m_clockTicks++;

                goto parse_DataProcessing_2and3;
            }

        parse_DataProcessing_2and3:
            {
                uint shift = Operand2;

                Operand2     = GetRegister( s_Encoding.get_Register4( op ) );
                shifterCarry = this.Carry();

                if(shift != 0)
                {
                    switch(shiftType)
                    {
                        case EncDef.c_shift_LSL:
                            if(shift < 32)
                            {
                                shifterCarry = (Operand2 >> (int)(32 - shift)) & 1;
                                Operand2     = (Operand2 << (int)      shift )    ;
                            }
                            else if(shift == 32)
                            {
                                shifterCarry = Operand2 & 1;
                                Operand2     = 0;
                            }
                            else
                            {
                                shifterCarry = 0;
                                Operand2     = 0;
                            }
                            break;

                        case EncDef.c_shift_LSR:
                            if(shift < 32)
                            {
                                shifterCarry = (Operand2 >> (int)(shift - 1)) & 1;
                                Operand2     = (Operand2 >> (int) shift     )    ;
                            }
                            else if(shift == 32)
                            {
                                shifterCarry = (Operand2 >> 31) & 1;
                                Operand2     = 0;
                            }
                            else
                            {
                                shifterCarry = 0;
                                Operand2     = 0;
                            }
                            break;

                        case EncDef.c_shift_ASR:
                            if(shift < 32)
                            {
                                shifterCarry = (            Operand2  >> (int)(shift - 1)) & 1;
                                Operand2     = (uint)(((int)Operand2) >> (int) shift     )    ;
                            }
                            else
                            {
                                shifterCarry = (Operand2 >> 31) & 1;
                                Operand2     = shifterCarry != 0 ? 0xFFFFFFFF : 0x0;
                            }
                            break;

                        case EncDef.c_shift_ROR:
                            shift %= 32;

                            shifterCarry = (Operand2 >> (int)(shift - 1)) & 1;
                            Operand2     = (Operand2 >> (int) shift     )     | (Operand2 << (int)(32 - shift));
                            break;

                        case EncDef.c_shift_RRX:
                            shifterCarry =                                        (Operand2  & 1);
                            Operand2     = (this.Carry() != 0 ? 0x80000000 : 0) | (Operand2 >> 1);
                            break;
                    }
                }

                goto parse_DataProcessing;
            }

        parse_DataProcessing:
            {
                uint Operand1 = GetRegister( s_Encoding.get_Register1( op ) );
                uint dst      =              s_Encoding.get_Register2( op );
                uint overflow = this.Overflow();
                uint carry    = this.Carry   ();
                uint res;

                switch(s_Encoding.get_DataProcessing_Operation( op ))
                {
                    case EncDef.c_operation_TST: dst   = 16; goto case EncDef.c_operation_AND;
                    case EncDef.c_operation_AND: res   = Operand1 & Operand2; carry = shifterCarry; break;

                    case EncDef.c_operation_TEQ: dst   = 16; goto case EncDef.c_operation_EOR;
                    case EncDef.c_operation_EOR: res   = Operand1 ^ Operand2; carry = shifterCarry; break;

                    case EncDef.c_operation_CMP: dst   = 16; goto case EncDef.c_operation_SUB;
                    case EncDef.c_operation_SUB: carry = 1;  goto case EncDef.c_operation_SBC;
                    case EncDef.c_operation_SBC:
                        {
                            ulong res64 = (ulong)Operand1 - (ulong)Operand2; if(carry == 0) res64--;

                            carry = (uint)(res64 >> 32) ^ 1;
                            res   = (uint) res64;

                            overflow = ((Operand1 ^ Operand2) & (Operand1 ^ res)) >> 31;
                        }
                        break;

                    case EncDef.c_operation_RSB: carry = 1; goto case EncDef.c_operation_RSC;
                    case EncDef.c_operation_RSC:
                        {
                            ulong res64 = (ulong)Operand2 - (ulong)Operand1; if(carry == 0) res64--;

                            carry = (uint)(res64 >> 32) ^ 1;
                            res   = (uint) res64;

                            overflow = ((Operand1 ^ Operand2) & (Operand2 ^ res)) >> 31;
                        }
                        break;

                    case EncDef.c_operation_CMN: dst   = 16; goto case EncDef.c_operation_ADD;
                    case EncDef.c_operation_ADD: carry = 0;  goto case EncDef.c_operation_ADC;
                    case EncDef.c_operation_ADC:
                        {
                            ulong res64 = (ulong)Operand2 + (ulong)Operand1; if(carry != 0) res64++;

                            carry = (uint)(res64 >> 32);
                            res   = (uint) res64;

                            overflow = (~(Operand1 ^ Operand2) & (Operand1 ^ res)) >> 31;
                        }
                        break;

                    case EncDef.c_operation_ORR: res = Operand1 |  Operand2; carry = shifterCarry; break;
                    case EncDef.c_operation_BIC: res = Operand1 & ~Operand2; carry = shifterCarry; break;

                    case EncDef.c_operation_MOV: res =             Operand2; carry = shifterCarry; break;
                    case EncDef.c_operation_MVN: res =            ~Operand2; carry = shifterCarry; break;

                    default:
                        throw new NotSupportedException();
                }

                switch(dst)
                {
                    case 16:
                        break;

                    case EncDef.c_register_pc:
                        SimulatePipelineMiss();

                        m_pc = res;
                        break;

                    default:
                        SetRegister( dst, res );
                        break;
                }

                if(s_Encoding.get_ShouldSetConditions( op ))
                {
                    if(dst == EncDef.c_register_pc)
                    {
                        SwitchMode( GetRegister( RegisterLookup.SPSR ) );
                    }
                    else
                    {
                        uint psr = m_cpsr & ~(EncDef.c_psr_V | EncDef.c_psr_C | EncDef.c_psr_Z | EncDef.c_psr_N);

                        if((int)      res <  0) psr |= EncDef.c_psr_N;
                        if(           res == 0) psr |= EncDef.c_psr_Z;
                        if((carry    & 1) != 0) psr |= EncDef.c_psr_C;
                        if((overflow & 1) != 0) psr |= EncDef.c_psr_V;

                        m_cpsr = psr;
                    }
                }

                return;
            }

            //--//

        parse_Multiply:
            {
                uint Op1 = GetRegister( s_Encoding.get_Register3( op ) );
                uint Op2 = GetRegister( s_Encoding.get_Register4( op ) );
                uint res;

                {
                    uint mCycles;

                    if     ((Op1 & 0xFFFFFF00) == 0) mCycles = 1;
                    else if((Op1 & 0xFFFF0000) == 0) mCycles = 2;
                    else if((Op1 & 0xFF000000) == 0) mCycles = 3;
                    else                             mCycles = 4;

                    m_clockTicks += mCycles;
                }

                res = Op1 * Op2;

                if(s_Encoding.get_Multiply_IsAccumulate( op ))
                {
                    res += GetRegister( s_Encoding.get_Register2( op ) );

                    //
                    // Extra internal cycle.
                    //
                    m_clockTicks++;
                }

                SetRegister( s_Encoding.get_Register1( op ), res );

                if(s_Encoding.get_ShouldSetConditions( op ))
                {
                    uint psr = m_cpsr & ~(EncDef.c_psr_Z | EncDef.c_psr_N);

                    if((int)res <  0) psr |= EncDef.c_psr_N;
                    if(     res == 0) psr |= EncDef.c_psr_Z;

                    m_cpsr = psr;
                }

                return;
            }

        parse_MultiplyLong:
            {
                uint  Op1 = GetRegister( s_Encoding.get_Register3( op ) );
                uint  Op2 = GetRegister( s_Encoding.get_Register4( op ) );
                ulong res;

                {
                    uint mCycles;

                    if     ((Op1 & 0xFFFFFF00) == 0) mCycles = 2;
                    else if((Op1 & 0xFFFF0000) == 0) mCycles = 3;
                    else if((Op1 & 0xFF000000) == 0) mCycles = 4;
                    else                             mCycles = 5;

                    m_clockTicks += mCycles;
                }

                if(s_Encoding.get_Multiply_IsSigned( op ))
                {
                    res = (ulong)((long)(int)Op1 * (long)(int)Op2);
                }
                else
                {
                    res = (ulong)Op1 * (ulong)Op2;
                }

                res = Execute_AccumulateAndStore( res, s_Encoding.get_Register1( op ), s_Encoding.get_Register2( op ), s_Encoding.get_Multiply_IsAccumulate( op ) );

                if(s_Encoding.get_ShouldSetConditions( op ))
                {
                    uint psr = m_cpsr & ~(EncDef.c_psr_Z | EncDef.c_psr_N);

                    if((long)res <  0) psr |= EncDef.c_psr_N;
                    if(      res == 0) psr |= EncDef.c_psr_Z;

                    m_cpsr = psr;
                }

                return;
            }

            //--//

        parse_BranchAndExchange:
            {
                uint reg = s_Encoding.get_Register4( op );

                uint lr = GetRegister( reg );

                SimulatePipelineMiss();

                m_pc = lr;

                return;
            }

        parse_Branch:
            {
                uint pc = GetRegister( EncDef.c_register_pc );

                if(s_Encoding.get_Branch_IsLink( op ))
                {
                    SetRegister( EncDef.c_register_lr, pc - 4 ); // R15 contains PC+8, LR has to point to PC+4;
                }

                pc = (uint)((int)pc + s_Encoding.get_Branch_Offset( op ));

                SimulatePipelineMiss();

                m_pc = pc;

                return;
            }

            //--//

        parse_SingleDataSwap:
            {
                // Not supported for now.
                throw new NotSupportedException();
            }

        parse_SingleDataTransfer_1:
            {
                Operand2 = s_Encoding.get_DataTransfer_Offset( op );

                goto parse_SingleDataTransfer;
            }

        parse_SingleDataTransfer_2:
            {
                shiftType = s_Encoding.get_Shift_Type     ( op );
                Operand2  = s_Encoding.get_Shift_Immediate( op );

                if(Operand2 == 0)
                {
                    switch(shiftType)
                    {
                        case EncDef.c_shift_LSR:
                        case EncDef.c_shift_ASR:
                            Operand2 = 32;
                            break;

                        case EncDef.c_shift_ROR:
                            Operand2  = 1;
                            shiftType = EncDef.c_shift_RRX;
                            break;
                    }
                }

                goto parse_SingleDataTransfer_2and3;
            }

        parse_SingleDataTransfer_3:
            {
                shiftType =              s_Encoding.get_Shift_Type    ( op );
                Operand2  = GetRegister( s_Encoding.get_Shift_Register( op ) );

                goto parse_SingleDataTransfer_2and3;
            }

        parse_SingleDataTransfer_2and3:
            {
                uint shift = Operand2;

                Operand2 = GetRegister( s_Encoding.get_Register4( op ) );

                if(shift != 0)
                {
                    switch(shiftType)
                    {
                    case EncDef.c_shift_LSL: Operand2 =       (      Operand2  << (int)shift )                                  ; break;
                    case EncDef.c_shift_LSR: Operand2 =       (      Operand2  >> (int)shift )                                  ; break;
                    case EncDef.c_shift_ASR: Operand2 = (uint)(((int)Operand2) >> (int)shift )                                  ; break;
                    case EncDef.c_shift_ROR: Operand2 =       (      Operand2  >> (int)shift ) | (Operand2 << (int)(32 - shift)); break;
                    }
                }

                goto parse_SingleDataTransfer;
            }

        parse_SingleDataTransfer:
            {
                dataPointer = s_Encoding.get_Register2                  ( op );
                kind        = s_Encoding.get_DataTransfer_IsByteTransfer( op ) ? TargetAdapterAbstractionLayer.MemoryAccessType.UINT8 : TargetAdapterAbstractionLayer.MemoryAccessType.UINT32;

                goto parse_DataTransfer;
            }

        parse_BlockDataTransfer:
            {
                uint address = GetRegister( s_Encoding.get_Register1( op ) );
                uint addressNext;

                uint Rd  = 0;
                uint Num = 0;
                uint Lst;

                Lst = s_Encoding.get_BlockDataTransfer_RegisterList( op );
                while(Lst != 0)
                {
                    if((Lst & 1) != 0) Num++;

                    Lst >>= 1;
                }

                bool load     = s_Encoding.get_DataTransfer_IsLoad       ( op );
                bool preIndex = s_Encoding.get_DataTransfer_IsPreIndexing( op );
                bool up       = s_Encoding.get_DataTransfer_IsUp         ( op );

                if(up)
                {
                    addressNext = address;

                    if(preIndex) addressNext += 4;
                }
                else
                {
                    addressNext = address - Num * 4;

                    if(!preIndex) addressNext += 4;
                }

                if(load)
                {
                    //
                    // Extra internal cycle.
                    //
                    m_clockTicks++;
                }

                Lst = s_Encoding.get_BlockDataTransfer_RegisterList( op );
                while(Lst != 0)
                {
                    if((Lst & 1) != 0)
                    {
                        if(load)
                        {
                            SetRegister( Rd, Load( addressNext, TargetAdapterAbstractionLayer.MemoryAccessType.UINT32 ) );

                            if(Rd == EncDef.c_register_pc)
                            {
                                SimulatePipelineMiss();

                                m_pc = GetRegister( EncDef.c_register_pc );
                            }
                        }
                        else
                        {
                            Store( addressNext, GetRegister( Rd ), TargetAdapterAbstractionLayer.MemoryAccessType.UINT32 );
                        }

                        addressNext += 4;
                    }

                    Rd++;
                    Lst >>= 1;
                }

                if(s_Encoding.get_DataTransfer_ShouldWriteBack( op ))
                {
                    if(up) address += 4 * Num;
                    else   address -= 4 * Num;

                    SetRegister( s_Encoding.get_Register1( op ), address );
                }

                if(s_Encoding.get_BlockDataTransfer_LoadPSR( op ))
                {
                    SwitchMode( GetRegister( RegisterLookup.SPSR ) );
                }

                return;
            }

        parse_HalfwordDataTransfer:
            {
                switch(s_Encoding.get_HalfWordDataTransfer_Kind( op ))
                {
                    case EncDef.c_halfwordkind_U2: kind = TargetAdapterAbstractionLayer.MemoryAccessType.UINT16; break;
                    case EncDef.c_halfwordkind_I1: kind = TargetAdapterAbstractionLayer.MemoryAccessType.SINT8 ; break;
                    case EncDef.c_halfwordkind_I2: kind = TargetAdapterAbstractionLayer.MemoryAccessType.SINT16; break;

                    default:
                        throw new NotSupportedException();
                }

                dataPointer = s_Encoding.get_Register2( op );

                goto parse_DataTransfer;
            }

        parse_DataTransfer:
            {
                uint address = GetRegister( s_Encoding.get_Register1( op ) );
                uint addressPost;

                bool load     = s_Encoding.get_DataTransfer_IsLoad       ( op );
                bool preIndex = s_Encoding.get_DataTransfer_IsPreIndexing( op );
                bool up       = s_Encoding.get_DataTransfer_IsUp         ( op );

                if(up) addressPost = address + Operand2;
                else   addressPost = address - Operand2;

                if(preIndex) address = addressPost;

                if(load)
                {
                    SetRegister( dataPointer, Load( address, kind ) );

                    //
                    // Extra internal cycle.
                    //
                    m_clockTicks++;

                    if(dataPointer == EncDef.c_register_pc)
                    {
                        SimulatePipelineMiss();

                        m_pc = GetRegister( dataPointer );
                    }
                }
                else
                {
                    Store( address, GetRegister( dataPointer ), kind );
                }

                if(s_Encoding.get_DataTransfer_ShouldWriteBack( op ) || preIndex == false)
                {
                    SetRegister( s_Encoding.get_Register1( op ), addressPost );
                }

                return;
            }

        parse_CoprocRegisterTransfer:
            {
                uint                               CpNum       = s_Encoding.get_Coproc_CpNum                ( op );
                uint                               Op1         = s_Encoding.get_CoprocRegisterTransfer_Op1  ( op );
                uint                               Rd          = s_Encoding.get_CoprocRegisterTransfer_Rd   ( op );
                uint                               CRn         = s_Encoding.get_CoprocRegisterTransfer_CRn  ( op );
                uint                               CRm         = s_Encoding.get_CoprocRegisterTransfer_CRm  ( op );
                uint                               Op2         = s_Encoding.get_CoprocRegisterTransfer_Op2  ( op );
                bool                               fFromCoproc = s_Encoding.get_CoprocRegisterTransfer_IsMRC( op );
                ProcessCoprocessorRegisterTransfer dlg;

                if(m_coprocessorsRegisterTransfer.TryGetValue( CpNum, out dlg ))
                {
                    uint value;

                    if(fFromCoproc)
                    {
                        value = 0;
                    }
                    else
                    {
                        value = GetRegister( Rd );
                    }

                    if(dlg( ref value, fFromCoproc, CRn, CRm, Op1, Op2 ))
                    {
                        if(fFromCoproc)
                        {
                            SetRegister( Rd, value );
                        }

                        return;
                    }
                }

                if(ProcessUnsupportedOperation( op ))
                {
                    return;
                }

                throw new NotImplementedException();
            }

        parse_CoprocDataTransfer:
            {
                uint                           CpNum = s_Encoding.get_Coproc_CpNum( op );
                ProcessCoprocessorDataTransfer dlg;
    
                if(m_coprocessorsDataTransfer.TryGetValue( CpNum, out dlg ))
                {
                    uint address = GetRegister( s_Encoding.get_CoprocDataTransfer_Rn( op ) );
                    uint offset  =              s_Encoding.get_CoprocDataTransfer_Offset( op ) * 4;
                    uint addressPost;

                    uint CRd      = s_Encoding.get_CoprocDataTransfer_CRd          ( op ); 
                    bool load     = s_Encoding.get_CoprocDataTransfer_IsLoad       ( op );
                    bool preIndex = s_Encoding.get_CoprocDataTransfer_IsPreIndexing( op );
                    bool up       = s_Encoding.get_CoprocDataTransfer_IsUp         ( op );
                    bool wide     = s_Encoding.get_CoprocDataTransfer_IsWide       ( op );

                    if(up) addressPost = address + offset;
                    else   addressPost = address - offset;

                    if(preIndex) address = addressPost;

                    if(dlg( address, CRd, load, wide ))
                    {
                        return;
                    }

                    if(s_Encoding.get_CoprocDataTransfer_ShouldWriteBack( op ) || preIndex == false)
                    {
                        SetRegister( s_Encoding.get_CoprocDataTransfer_Rn( op ), addressPost );
                    }

                    return;
                }

                if(ProcessUnsupportedOperation( op ))
                {
                    return;
                }

                throw new NotImplementedException();
            }

        parse_CoprocDataOperation:
            {
                uint                            CpNum = s_Encoding.get_Coproc_CpNum( op );
                ProcessCoprocessorDataOperation dlg;

                if(m_coprocessorsDataOperation.TryGetValue( CpNum, out dlg ))
                {
                    uint Op1 = s_Encoding.get_CoprocDataOperation_Op1( op );
                    uint CRd = s_Encoding.get_CoprocDataOperation_CRd( op );
                    uint CRn = s_Encoding.get_CoprocDataOperation_CRn( op );
                    uint CRm = s_Encoding.get_CoprocDataOperation_CRm( op );
                    uint Op2 = s_Encoding.get_CoprocDataOperation_Op2( op );

                    if(dlg( CRn, CRm, CRd, Op1, Op2 ))
                    {
                        return;
                    }
                }

                if(ProcessUnsupportedOperation( op ))
                {
                    return;
                }

                throw new NotImplementedException();
            }

        parse_SoftwareInterrupt:
            {
                uint cpsrPost = m_cpsr & ~EncDef.c_psr_mode;

                cpsrPost |= EncDef.c_psr_mode_SVC;
                cpsrPost |= EncDef.c_psr_I;

                SwitchMode( cpsrPost );

                SetRegister( EncDef.c_register_lr, m_pc );

                m_pc = 0x00000008;
                return;
            }

        parse_Breakpoint:
            {
                if(ProcessUnsupportedOperation( op ))
                {
                    return;
                }

                throw new NotImplementedException();
            }

        parse_Undefined:
            {
                if(ProcessUnsupportedOperation( op ))
                {
                    return;
                }

                throw new NotImplementedException();
            }
        }

        protected abstract void ProcessInterruptDisabling( uint cpsrPre  ,
                                                           uint cpsrPost );

        protected abstract bool ProcessUnsupportedOperation( uint op );

        //--//

        public TimeSpan ClockTicksToTime( long clockTicks )
        {
            double val = clockTicks;

            return new TimeSpan( (long)(val * TimeSpan.TicksPerSecond / m_clockFrequency) );
        }

        public long TimeToClockTicks( TimeSpan time )
        {
            double val = (double)time.Ticks;

            return (long)(val * m_clockFrequency / TimeSpan.TicksPerSecond);
        }

        //--//

        public void RegisterCoprocessorRegisterTransfer( uint                               cpNum ,
                                                         ProcessCoprocessorRegisterTransfer dlg   )
        {
            m_coprocessorsRegisterTransfer[cpNum] = dlg;
        }

        public void RegisterCoprocessorDataTransfer( uint                           cpNum ,
                                                     ProcessCoprocessorDataTransfer dlg   )
        {
            m_coprocessorsDataTransfer[cpNum] = dlg;
        }

        public void RegisterCoprocessorDataOperation( uint                            cpNum ,
                                                      ProcessCoprocessorDataOperation dlg   )
        {
            m_coprocessorsDataOperation[cpNum] = dlg;
        }

        //--//

        private uint Execute_MSR( uint psr     ,
                                  uint operand ,
                                  uint fields  )
        {
            uint mask = 0;

            if((fields & EncDef.c_psr_field_c) != 0) mask |= 0x000000FF;
            if((fields & EncDef.c_psr_field_x) != 0) mask |= 0x0000FF00;
            if((fields & EncDef.c_psr_field_s) != 0) mask |= 0x00FF0000;
            if((fields & EncDef.c_psr_field_f) != 0) mask |= 0xFF000000;

            operand = (psr & ~mask) | (operand & mask);

            return operand;
        }

        private ulong Execute_AccumulateAndStore( ulong res        ,
                                                  uint  RdHiIdx    ,
                                                  uint  RdLoIdx    ,
                                                  bool  accumulate )
        {
            if(accumulate)
            {
                res += (ulong)GetRegister( RdHiIdx ) << 32 | (ulong)GetRegister( RdLoIdx );

                //
                // Extra internal cycle.
                //
                m_clockTicks++;
            }

            SetRegister( RdHiIdx, (uint)(res >> 32) );
            SetRegister( RdLoIdx, (uint) res        );

            return res;
        }

        private void SimulatePipelineMiss()
        {
            uint wasteCycle;

            wasteCycle = Load( m_pc  , TargetAdapterAbstractionLayer.MemoryAccessType.FETCH );
            wasteCycle = Load( m_pc+4, TargetAdapterAbstractionLayer.MemoryAccessType.FETCH );
        }

        //
        // Access Methods
        //

        public bool AreTimingUpdatesEnabled
        {
            get
            {
                return m_suspendCount == 0;
            }
        }

        public Cfg.ProductCategory Product
        {
            get
            {
                return m_product;
            }
        }

        public ulong ClockFrequency
        {
            get
            {
                return m_clockFrequency;
            }
        }

        public ulong ClockTicks
        {
            get
            {
                return m_clockTicks;
            }
        }

        public ulong SleepTicks
        {
            get
            {
                return m_sleepTicks;
            }
        }
    }
}
