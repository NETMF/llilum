//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Debugger.ArmProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.Text;
    using System.IO;
    using System.Windows.Forms;
    using System.Threading;

    using EncDef             = Microsoft.Zelig.TargetModel.ArmProcessor.EncodingDefinition;
    using InstructionSet     = Microsoft.Zelig.TargetModel.ArmProcessor.InstructionSet;
    using IR                 = Microsoft.Zelig.CodeGeneration.IR;
    using RT                 = Microsoft.Zelig.Runtime;
    using TS                 = Microsoft.Zelig.Runtime.TypeSystem;


    public class RegisterContext
    {
        public abstract class AbstractValue
        {
            //
            // State
            //

            protected readonly RegisterContext                    m_owner;
            public    readonly IR.Abstractions.RegisterDescriptor Register;

            //
            // Constructor Methods
            //

            protected AbstractValue( RegisterContext                    owner ,
                                     IR.Abstractions.RegisterDescriptor reg   )
            {
                this.m_owner  = owner;
                this.Register = reg;
            }

            //
            // Helper Methods
            //

            internal abstract AbstractValue Clone( RegisterContext context );

            public abstract Emulation.Hosting.BinaryBlob GetValue();

            public abstract bool SetValue( Emulation.Hosting.BinaryBlob bb );

            //
            // Access Methods
            //

            public bool IsAvailable
            {
                get
                {
                    return m_owner.IsAvailable( this.Register );
                }
            }

            public abstract bool CanUpdate
            {
                get;
            }
        }

        public class ValueInProcessorRegister : AbstractValue
        {
            //
            // State
            //

            private readonly GrowOnlyHashTable< uint, uint > m_physicalRegisterSet;

            //
            // Constructor Methods
            //

            internal ValueInProcessorRegister( RegisterContext                    owner               ,
                                               IR.Abstractions.RegisterDescriptor reg                 ,
                                               GrowOnlyHashTable< uint, uint >    physicalRegisterSet ) : base( owner, reg )
            {
                m_physicalRegisterSet = physicalRegisterSet;
            }

            //
            // Helper Methods
            //

            internal override AbstractValue Clone( RegisterContext owner )
            {
                return new ValueInProcessorRegister( owner, this.Register, m_physicalRegisterSet );
            }

            //--//

            public override Emulation.Hosting.BinaryBlob GetValue()
            {
                var reg = this.Register;
                var bb  = new Emulation.Hosting.BinaryBlob( (int)(reg.PhysicalStorageSize * sizeof(uint)) );

                for(uint pos = 0; pos < reg.PhysicalStorageSize; pos++)
                {
                    bb.WriteUInt32( m_physicalRegisterSet[reg.PhysicalStorageOffset + pos], (int)(pos * sizeof(uint)) );
                }

                return bb;
            }

            public override bool SetValue( Emulation.Hosting.BinaryBlob bb )
            {
                var reg = this.Register;

                for(uint pos = 0; pos < reg.PhysicalStorageSize; pos++)
                {
                    m_physicalRegisterSet[reg.PhysicalStorageOffset + pos] = bb != null ? bb.ReadUInt32( (int)(pos * sizeof(uint)) ) : 0;
                }

                if(m_owner.m_svc != null)
                {
                    m_owner.m_svc.SetRegister( reg, bb );
                }

                return true;
            }

            //
            // Access Methods
            //

            public override bool CanUpdate
            {
                get
                {
                    return this.IsAvailable;
                }
            }
        }

        public class ValueInMemory : AbstractValue
        {
            //
            // State
            //

            private readonly MemoryDelta m_memDelta;
            private readonly uint        m_address;

            //
            // Constructor Methods
            //

            internal ValueInMemory( RegisterContext                    owner    ,
                                    IR.Abstractions.RegisterDescriptor reg      ,
                                    MemoryDelta                        memDelta ,
                                    uint                               address  ) : base( owner, reg )
            {
                m_memDelta = memDelta;
                m_address  = address;
            }

            //
            // Helper Methods
            //

            internal override AbstractValue Clone( RegisterContext owner )
            {
                return new ValueInMemory( owner, this.Register, m_memDelta, m_address );
            }

            //--//

            public override Emulation.Hosting.BinaryBlob GetValue()
            {
                var reg = this.Register;
                var bb  = new Emulation.Hosting.BinaryBlob( (int)(reg.PhysicalStorageSize * sizeof(uint)) );

                for(uint pos = 0; pos < reg.PhysicalStorageSize; pos++)
                {
                    uint val;

                    if(m_memDelta.GetUInt32( m_address + pos * sizeof(uint), out val ) == false)
                    {
                        return null;
                    }

                    bb.WriteUInt32( val, (int)(pos * sizeof(uint)) );
                }

                return bb;
            }

            public override bool SetValue( Emulation.Hosting.BinaryBlob bb )
            {
                var reg = this.Register;

                for(uint pos = 0; pos < reg.PhysicalStorageSize; pos++)
                {
                    uint val = bb.ReadUInt32( (int)(pos * sizeof(uint)) );

                    if(m_memDelta.SetUInt32( m_address + pos * sizeof(uint), val ) == false)
                    {
                        return false;
                    }
                }

                return true;
            }

            //
            // Access Methods
            //

            public override bool CanUpdate
            {
                get
                {
                    return this.IsAvailable;
                }
            }
        }

        internal class RegisterValue_ReadOnly : AbstractValue
        {
            //
            // State
            //

            private readonly Emulation.Hosting.BinaryBlob m_bb;

            //
            // Constructor Methods
            //

            internal RegisterValue_ReadOnly( RegisterContext                    owner ,
                                             IR.Abstractions.RegisterDescriptor reg   ,
                                             Emulation.Hosting.BinaryBlob       bb    ) : base( owner, reg )
            {
                m_bb = bb;
            }

            //
            // Helper Methods
            //

            internal override AbstractValue Clone( RegisterContext owner )
            {
                return new RegisterValue_ReadOnly( owner, this.Register, m_bb );
            }

            //--//

            public override Emulation.Hosting.BinaryBlob GetValue()
            {
                return m_bb;
            }

            public override bool SetValue( Emulation.Hosting.BinaryBlob bb )
            {
                return false;
            }

            //
            // Access Methods
            //

            public override bool CanUpdate
            {
                get
                {
                    return false;
                }
            }
        }

        //
        // State
        //

        private readonly IR.TypeSystemForCodeTransformation                                     m_typeSystem;
        private          Emulation.Hosting.ProcessorStatus                                      m_svc;
        private readonly IR.Abstractions.RegisterDescriptor[]                                   m_registers;
        private readonly GrowOnlyHashTable< uint                              , uint          > m_physicalRegisterSet;
        private readonly GrowOnlyHashTable< IR.Abstractions.RegisterDescriptor, AbstractValue > m_lookup;
        private readonly RegisterContext                                                        m_previousContext;

        //
        // Constructor Methods
        //

        public RegisterContext( IR.TypeSystemForCodeTransformation typeSystem )
        {
            m_typeSystem          = typeSystem;
            m_registers           = typeSystem.PlatformAbstraction.GetRegisters();
            m_physicalRegisterSet = HashTableFactory.New                     < uint                              , uint                  >();
            m_lookup              = HashTableFactory.NewWithReferenceEquality< IR.Abstractions.RegisterDescriptor, AbstractValue >();

            foreach(var regDesc in m_registers)
            {
                for(uint pos = 0; pos < regDesc.PhysicalStorageSize; pos++)
                {
                    m_physicalRegisterSet[regDesc.PhysicalStorageOffset + pos] = 0;
                }

                SetLocationInProcessor( regDesc );
            }
        }

        public RegisterContext( RegisterContext other )
        {
            m_typeSystem          = other.m_typeSystem;
            m_svc                 = other.m_svc;
            m_registers           = other.m_registers;
            m_physicalRegisterSet = other.m_physicalRegisterSet.Clone();
            m_lookup              = other.m_lookup             .CloneSettings();
            m_previousContext     = other;

            foreach(var key in other.m_lookup.Keys)
            {
                m_lookup[key] = other.m_lookup[key].Clone( this );
            }
        }

        //
        // Helper Methods
        //

        public void UpdateStackFrame( uint pc ,
                                      uint sp )
        {
            UpdateReadOnlyValue( this.InnerPC, pc );
            UpdateReadOnlyValue( this.InnerSP, sp );
        }

        private void UpdateReadOnlyValue( IR.Abstractions.RegisterDescriptor regDesc ,
                                          uint                               value   )
        {
            m_lookup[regDesc] = new RegisterValue_ReadOnly( this, regDesc, Emulation.Hosting.BinaryBlob.Wrap( value ) );
        }

        public void SetLocationInProcessor( IR.Abstractions.RegisterDescriptor regDesc )
        {
            m_lookup[regDesc] = new ValueInProcessorRegister( this, regDesc, m_physicalRegisterSet );
        }

        public void SetLocationInMemory( uint        encoding ,
                                         MemoryDelta memDelta ,
                                         uint        address  )
        {
            SetLocationInMemory( GetRegisterDescriptor( encoding ), memDelta, address );
        }

        public void SetLocationInMemory( IR.Abstractions.RegisterDescriptor regDesc  ,
                                         MemoryDelta                        memDelta ,
                                         uint                               address  )
        {
            m_lookup[regDesc] = new ValueInMemory( this, regDesc, memDelta, address );
        }

        //--//

        public bool IsAvailable( IR.Abstractions.RegisterDescriptor regDesc )
        {
            if(m_lookup.ContainsKey( regDesc ) == false)
            {
                return false;
            }

            //
            // Registers from top-level frames are always available.
            //
            if(m_previousContext == null)
            {
                return true;
            }

            if((regDesc.PhysicalClass & IR.Abstractions.RegisterClass.StatusRegister) != 0)
            {
                return false;
            }

            if(m_typeSystem.CallingConvention.ShouldSaveRegister( regDesc ))
            {
                if(regDesc.IsLinkAddress)
                {
                    return false;
                }

                return true;
            }

            if(regDesc.IsSpecial)
            {
                return true;
            }

            return false;
        }

        //--//

        public uint GetValueAsUInt( uint encoding )
        {
            return GetValueAsUInt( GetRegisterDescriptor( encoding ) );
        }

        public uint GetValueAsUInt( IR.Abstractions.RegisterDescriptor regDesc )
        {
            var av = GetValue( regDesc );
            if(av != null)
            {
                var bb = av.GetValue();
                if(bb != null)
                {
                    return bb.ReadUInt32( 0 );
                }
            }
            
            return 0;
        }

        //--//

        public AbstractValue GetValue( uint encoding )
        {
            return GetValue( GetRegisterDescriptor( encoding ) );
        }

        public AbstractValue GetValue( IR.Abstractions.RegisterDescriptor regDesc )
        {
            AbstractValue val;

            m_lookup.TryGetValue( regDesc, out val );

            return val;
        }

        //--//

        public bool SetValue( uint                         encoding ,
                              Emulation.Hosting.BinaryBlob bb       )
        {
            return SetValue( GetRegisterDescriptor( encoding ), bb );
        }

        public bool SetValue( IR.Abstractions.RegisterDescriptor regDesc ,
                              Emulation.Hosting.BinaryBlob       bb      )
        {
            AbstractValue val;

            if(m_lookup.TryGetValue( regDesc, out val ))
            {
                return val.SetValue( bb );
            }

            return false;
        }

        //--//

        public static bool IsValueCompatible( IR.Abstractions.RegisterDescriptor regDesc ,
                                              object                             value   )
        {
            if(regDesc == null)
            {
                return false;
            }

            if(regDesc.InFloatingPointRegisterFile)
            {
                if(regDesc.IsDoublePrecision)
                {
                    if(!(value is double))
                    {
                        return false;
                    }
                }
                else
                {
                    if(!(value is float))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if(!(value is uint))
                {
                    return false;
                }
            }

            return true;
        }

        //--//

        public IR.Abstractions.RegisterDescriptor GetRegisterDescriptor( uint encoding )
        {
            foreach(IR.Abstractions.RegisterDescriptor regDesc in m_registers)
            {
                if(regDesc.Encoding == encoding)
                {
                    return regDesc;
                }
            }

            return null;
        }

        //--//

        //
        // Access Methods
        //

        internal Emulation.Hosting.ProcessorStatus ProcessorStatus
        {
            set
            {
                m_svc = value;
            }
        }

        public uint ProgramCounter
        {
            get
            {
                return GetValueAsUInt( this.InnerPC );
            }
        }

        public uint StackPointer
        {
            get
            {
                return GetValueAsUInt( this.InnerSP );
            }
        }

        public IR.Abstractions.RegisterDescriptor[] Keys
        {
            get
            {
                return m_registers;
            }
        }

        private IR.Abstractions.RegisterDescriptor InnerPC
        {
            get
            {
                foreach(IR.Abstractions.RegisterDescriptor regDesc in m_registers)
                {
                    if((regDesc.PhysicalClass & IR.Abstractions.RegisterClass.ProgramCounter) != 0)
                    {
                        return regDesc;
                    }
                }

                return null;
            }
        }

        private IR.Abstractions.RegisterDescriptor InnerSP
        {
            get
            {
                foreach(IR.Abstractions.RegisterDescriptor regDesc in m_registers)
                {
                    if((regDesc.PhysicalClass & IR.Abstractions.RegisterClass.StackPointer) != 0)
                    {
                        return regDesc;
                    }
                }

                return null;
            }
        }
    }
}