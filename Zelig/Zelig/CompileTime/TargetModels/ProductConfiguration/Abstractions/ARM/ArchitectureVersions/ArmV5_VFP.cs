//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Configuration.Environment.Abstractions.Architectures
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;

    using Microsoft.Zelig.TargetModel.ArmProcessor;

    using ZeligIR = Microsoft.Zelig.CodeGeneration.IR;


    public sealed partial class ArmV5_VFP : ArmPlatform
    {
        const Capabilities c_ProcessorCapabilities = Capabilities.ARMv5 | Capabilities.VFPv2;

        //
        // Constructor Methods
        //

        public ArmV5_VFP() : base( c_ProcessorCapabilities ) // Default constructor required by TypeSystemSerializer.
        {
        }

        public ArmV5_VFP( ZeligIR.TypeSystemForCodeTransformation typeSystem ,
                          MemoryMapCategory                       memoryMap  ) : base( typeSystem, memoryMap, c_ProcessorCapabilities )
        {
        }

        //
        // Helper Methods
        //

        public override void RegisterForNotifications( ZeligIR.TypeSystemForCodeTransformation  ts    ,
                                                       ZeligIR.CompilationSteps.DelegationCache cache )
        {
            base.RegisterForNotifications( ts, cache );

            cache.Register( new FloatingPointEmulation() );

            cache.Register( new CollectRegisterAllocationConstraints() );

            cache.Register( new ArmV4.Optimizations( this ) );
        }

        //--//

        public override TypeRepresentation GetMethodWrapperType()
        {
            return m_typeSystem.GetWellKnownType( "Microsoft_Zelig_ProcessorARMv5_VFP_MethodWrapper" );
        }

        public override bool HasRegisterContextArgument( MethodRepresentation md )
        {
            if(md.ThisPlusArguments.Length > 1)
            {
                TypeRepresentation td = md.ThisPlusArguments[1];

                if(td is PointerTypeRepresentation)
                {
                    td = td.UnderlyingType;

                    if(td == m_typeSystem.GetWellKnownType( "Microsoft_Zelig_ProcessorARMv5_VFP_RegistersOnStack" ))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        //--//

        protected override void ComputeRegisterFlushFixup( BitVector registersToSave )
        {
            while(true)
            {
                bool fDone = true;
    
                foreach(ZeligIR.Abstractions.RegisterDescriptor regToSave in this.GetRegisters())
                {
                    int pos = (int)regToSave.Encoding;

                    if(registersToSave[pos])
                    {
                        uint size = regToSave.PhysicalStorageSize;

                        foreach(ZeligIR.Abstractions.RegisterDescriptor regInterfere in regToSave.InterfersWith)
                        {
                            if(regInterfere.PhysicalStorageSize > size)
                            {
                                registersToSave.Set  ( (int)regInterfere.Encoding );
                                registersToSave.Clear(      pos                   );
                                fDone = false;
                                break;
                            }
                        }
                    }
                }

                if(fDone)
                {
                    break;
                }
            }
        }
    }
}
