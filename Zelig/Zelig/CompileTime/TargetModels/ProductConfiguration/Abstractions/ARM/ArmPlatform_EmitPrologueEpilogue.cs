//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Configuration.Environment.Abstractions
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;

    using Microsoft.Zelig.TargetModel.ArmProcessor;

    using ZeligIR = Microsoft.Zelig.CodeGeneration.IR;


    public partial class ArmPlatform
    {
        //
        // Helper Methods
        //

        public bool Emit__WasRegisterTouched( ZeligIR.Abstractions.RegisterDescriptor regDesc           ,
                                              BitVector                               modifiedRegisters )
        {
            return modifiedRegisters[ (int)regDesc.Encoding ];
        }

        public uint Emit__GetIntegerList( ArmCompilationState cs              ,
                                          BitVector           registersToSave )
        {
            uint mask = 0;

            foreach(ZeligIR.Abstractions.RegisterDescriptor regToSave in this.GetRegisters())
            {
                if(regToSave.InIntegerRegisterFile && regToSave.IsLinkAddress == false)
                {
                    if(registersToSave[ (int)regToSave.Encoding ])
                    {
                        mask |= 1U << (int)ArmCompilationState.GetIntegerEncoding( regToSave );
                    }
                }
            }

            return mask;
        }

        public void Emit__GetFloatingPointList(     ArmCompilationState                     cs              ,
                                                    BitVector                               registersToSave ,
                                                out ZeligIR.Abstractions.RegisterDescriptor regLowest       ,
                                                out ZeligIR.Abstractions.RegisterDescriptor regHighest      )
        {
            regLowest  = null;
            regHighest = null;

            foreach(ZeligIR.Abstractions.RegisterDescriptor regToSave in this.GetRegisters())
            {
                if(regToSave.InFloatingPointRegisterFile)
                {
                    if(registersToSave[ (int)regToSave.Encoding ])
                    {
                        CHECKS.ASSERT( regToSave.IsDoublePrecision, "Expecting double-precision register, got {0}", regToSave );

                        if(regLowest  == null || regLowest .Encoding > regToSave.Encoding) regLowest  = regToSave;
                        if(regHighest == null || regHighest.Encoding < regToSave.Encoding) regHighest = regToSave;
                    }
                }
            }
        }

        //--//

        public override void ComputeSetOfRegistersToSave(     ZeligIR.Abstractions.CallingConvention             cc                ,
                                                              ZeligIR.ControlFlowGraphStateForCodeTransformation cfg               ,
                                                              BitVector                                          modifiedRegisters ,
                                                          out BitVector                                          registersToSave   ,
                                                          out Runtime.HardwareException                          he                )
        {
            ZeligIR.TypeSystemForCodeTransformation ts           = cfg.TypeSystem;
            MethodRepresentation                    md           = cfg.Method;
            bool                                    fFullContext = false;

            registersToSave = new BitVector();
            he              = ts.ExtractHardwareExceptionSettingsForMethod( md );

            if(he != Runtime.HardwareException.None)
            {
                //
                // Make sure we also save the registers trashed by the calling convention.
                //
                foreach(ZeligIR.Abstractions.RegisterDescriptor regToSave in GetRegisters())
                {
                    if(regToSave.InIntegerRegisterFile)
                    {
                        if(he == Runtime.HardwareException.FastInterrupt && regToSave.Encoding >= EncodingDefinition.c_register_r8)
                        {
                            continue;
                        }
                    }
                    else
                    {
                        //
                        // Nothing special to do, save all the FP registers.
                        //
                    }

                    if(cc.ShouldSaveRegister( regToSave ) == false &&
                       regToSave.IsSpecial                == false  )
                    {
                        registersToSave.Set( (int)regToSave.Encoding );
                    }
                }

                if(HasRegisterContextArgument( md ))
                {
                    fFullContext = true;
                }
            }

            if(md.HasBuildTimeFlag( MethodRepresentation.BuildTimeAttributes.SaveFullProcessorContext ))
            {
                fFullContext = true;
            }

            foreach(ZeligIR.Abstractions.RegisterDescriptor regToSave in this.GetRegisters())
            {
                int pos = (int)regToSave.Encoding;

                if(fFullContext)
                {
                    if(regToSave.IsSpecial)
                    {
                        continue;
                    }
                }
                else
                {
                    if(cc.ShouldSaveRegister( regToSave ) == false)
                    {
                        continue;
                    }

                    if(modifiedRegisters[pos] == false)
                    {
                        continue;
                    }
                }

                registersToSave.Set( pos );
            }

            ComputeRegisterFlushFixup( registersToSave );
        }

        protected abstract void ComputeRegisterFlushFixup( BitVector registersToSave );
    }
}
