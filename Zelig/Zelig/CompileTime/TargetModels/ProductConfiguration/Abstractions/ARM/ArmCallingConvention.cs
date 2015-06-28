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


    public class ArmCallingConvention : ZeligIR.Abstractions.CallingConvention
    {
        class ArmCallState : ZeligIR.Abstractions.CallingConvention.CallState
        {
            //
            // State
            //

            internal int  m_numberOfWordsPassedInRegistersForIntegers;
            internal int  m_numberOfWordsPassedInRegistersForFloatingPoints;
            internal int  m_numberOfWordsPerArgument;

            internal uint m_nextRegForArgumentsInteger;
            internal uint m_nextRegForArgumentsFloatingPoint;
            internal uint m_nextRegForResultValueInteger; 
            internal uint m_nextRegForResultValueFloatingPoint;

            internal uint m_nextStackIn;
            internal uint m_nextStackLocal;
            internal uint m_nextStackOut;

            //
            // Constructor Methods
            //

            internal ArmCallState() // Default constructor required by TypeSystemSerializer.
            {
            }

            internal ArmCallState( Direction direction                                       ,
                                   int       numberOfWordsPassedInRegistersForIntegers       ,
                                   int       numberOfWordsPassedInRegistersForFloatingPoints ,
                                   int       numberOfWordsPerArgument                        ) : base( direction )
            {
                m_numberOfWordsPassedInRegistersForIntegers       = numberOfWordsPassedInRegistersForIntegers;
                m_numberOfWordsPassedInRegistersForFloatingPoints = numberOfWordsPassedInRegistersForFloatingPoints;
                m_numberOfWordsPerArgument                        = numberOfWordsPerArgument;
            }

            //
            // Helper Methods
            //

            public override void ApplyTransformation( TransformationContext context )
            {
                ZeligIR.TransformationContextForCodeTransformation context2 = (ZeligIR.TransformationContextForCodeTransformation)context;

                context2.Push( this );

                base.ApplyTransformation( context2 );

                context2.Transform( ref m_numberOfWordsPassedInRegistersForIntegers       );
                context2.Transform( ref m_numberOfWordsPassedInRegistersForFloatingPoints );
                context2.Transform( ref m_numberOfWordsPerArgument                        );

                context2.Transform( ref m_nextRegForArgumentsInteger                      );
                context2.Transform( ref m_nextRegForArgumentsFloatingPoint                );
                context2.Transform( ref m_nextRegForResultValueInteger                    );
                context2.Transform( ref m_nextRegForResultValueFloatingPoint              );
                                                                           
                context2.Transform( ref m_nextStackIn                                     );
                context2.Transform( ref m_nextStackLocal                                  );
                context2.Transform( ref m_nextStackOut                                    );

                context2.Pop();
            }

            public override ZeligIR.Abstractions.RegisterDescriptor GetNextRegister( ZeligIR.Abstractions.Platform                                     platform   ,
                                                                                     TypeRepresentation                                                sourceTd   ,
                                                                                     TypeRepresentation                                                fragmentTd ,
                                                                                     ZeligIR.ControlFlowGraphStateForCodeTransformation.KindOfFragment kind       )
            {
                switch(kind)
                {
                    case ZeligIR.ControlFlowGraphStateForCodeTransformation.KindOfFragment.CopyIncomingArgumentFromPhysicalToPseudoRegister:
                    case ZeligIR.ControlFlowGraphStateForCodeTransformation.KindOfFragment.AllocatePhysicalRegisterForArgument:
                    case ZeligIR.ControlFlowGraphStateForCodeTransformation.KindOfFragment.AllocatePhysicalRegisterForReturnValue:
                        int idx = GetNextIndex( platform, sourceTd, fragmentTd, kind );

                        return platform.GetRegisters()[idx];
                }

                throw TypeConsistencyErrorException.Create( "Unexpected fragment kind {0}", kind );
            }

            public override int GetNextIndex( ZeligIR.Abstractions.Platform                                     platform   ,
                                              TypeRepresentation                                                sourceTd   ,
                                              TypeRepresentation                                                fragmentTd ,
                                              ZeligIR.ControlFlowGraphStateForCodeTransformation.KindOfFragment kind       )
            {
                switch(kind)
                {
                    case ZeligIR.ControlFlowGraphStateForCodeTransformation.KindOfFragment.CopyIncomingArgumentFromPhysicalToPseudoRegister:
                    case ZeligIR.ControlFlowGraphStateForCodeTransformation.KindOfFragment.AllocatePhysicalRegisterForArgument:
                        return GetNextIndex( platform, fragmentTd, ref m_nextRegForArgumentsInteger, ref m_nextRegForArgumentsFloatingPoint );

                    case ZeligIR.ControlFlowGraphStateForCodeTransformation.KindOfFragment.AllocatePhysicalRegisterForReturnValue:
                        return GetNextIndex( platform, fragmentTd, ref m_nextRegForResultValueInteger, ref m_nextRegForResultValueFloatingPoint );

                    case ZeligIR.ControlFlowGraphStateForCodeTransformation.KindOfFragment.CopyIncomingArgumentFromStackToPseudoRegister:
                    case ZeligIR.ControlFlowGraphStateForCodeTransformation.KindOfFragment.AllocateStackInLocation:
                        return (int)m_nextStackIn++;

                    case ZeligIR.ControlFlowGraphStateForCodeTransformation.KindOfFragment.AllocateStackLocalLocation:
                        return (int)m_nextStackLocal++;

                    case ZeligIR.ControlFlowGraphStateForCodeTransformation.KindOfFragment.AllocateStackOutLocation:
                        return (int)m_nextStackOut++;
                }

                throw TypeConsistencyErrorException.Create( "Unexpected fragment kind {0}", kind );
            }

            public override bool CanMapToRegister( ZeligIR.Abstractions.Platform platform ,
                                                   TypeRepresentation            td       )
            {
                uint requiredRegisters = td.SizeOfHoldingVariableInWords;

                if(ShouldUseFloatingPointRegister( platform, td ))
                {
                    uint nextReg = m_nextRegForArgumentsFloatingPoint;

                    if(requiredRegisters == 2 && ((nextReg & 1) != 0)) nextReg++; // Align to Double-Precision register boundary.

                    return (          requiredRegisters <= m_numberOfWordsPassedInRegistersForFloatingPoints &&
                            nextReg + requiredRegisters <= m_numberOfWordsPassedInRegistersForFloatingPoints  );
                }
                else
                {
                    return (                               requiredRegisters <= m_numberOfWordsPassedInRegistersForIntegers &&
                            m_nextRegForArgumentsInteger + requiredRegisters <= m_numberOfWordsPassedInRegistersForIntegers  );
                }
            }

            public override bool CanMapResultToRegister( ZeligIR.Abstractions.Platform platform ,
                                                         TypeRepresentation            td       )
            {
                uint requiredRegisters = td.SizeOfHoldingVariableInWords;
        
                return (requiredRegisters <= m_numberOfWordsPerArgument);
            }

            //--//

            private static bool ShouldUseFloatingPointRegister( ZeligIR.Abstractions.Platform platform ,
                                                                TypeRepresentation            td       )
            {
                if(td.IsFloatingPoint)
                {
                    ArmPlatform paArm = (ArmPlatform)platform;

                    if(paArm.HasVFPv2)
                    {
                        return true;
                    }
                }

                return false;
            }

            private static int GetNextIndex(     ZeligIR.Abstractions.Platform platform             ,
                                                 TypeRepresentation            td                   ,
                                             ref uint                          nextRegInteger       ,
                                             ref uint                          nextRegFloatingPoint )
            {
                uint encoding;

                if(ShouldUseFloatingPointRegister( platform, td ))
                {
                    uint requiredRegisters = td.SizeOfHoldingVariableInWords;
                    uint nextReg           = nextRegFloatingPoint;

                    if(requiredRegisters == 2 && ((nextReg & 1) != 0)) nextReg++; // Align to Double-Precision register boundary.

                    nextRegFloatingPoint = nextReg + requiredRegisters;

                    if(requiredRegisters == 2)
                    {
                        encoding = EncodingDefinition_VFP.c_register_d0 + (nextReg / 2);
                    }
                    else
                    {
                        encoding = EncodingDefinition_VFP.c_register_s0 + nextReg;
                    }
                }
                else
                {
                    uint nextReg = nextRegInteger++;

                    encoding = EncodingDefinition.c_register_r0 + nextReg;
                }

                foreach(var regDesc in platform.GetRegisters())
                {
                    if(regDesc.Encoding == encoding)
                    {
                        return regDesc.Index;
                    }
                }

                throw TypeConsistencyErrorException.Create( "Failed to find register with encoding {0}", encoding );
            }
        }

        //
        // State
        //

        internal int m_numberOfWordsPassedInRegistersForIntegers       = 4;
        internal int m_numberOfWordsPassedInRegistersForFloatingPoints = 4;
        internal int m_numberOfWordsPerArgument                        = 2;

        //
        // Constructor Methods
        //


        public ArmCallingConvention() // Default constructor required by TypeSystemSerializer.
        {
        }

        public ArmCallingConvention( ZeligIR.TypeSystemForCodeTransformation typeSystem ) : base( typeSystem )
        {
        }

        //
        // Helper Methods
        //

        public override void RegisterForNotifications( ZeligIR.TypeSystemForCodeTransformation  ts    ,
                                                       ZeligIR.CompilationSteps.DelegationCache cache )
        {
        }

        //--//

        public override CallState CreateCallState( Direction direction )
        {
            ArmPlatform paArm = (ArmPlatform)m_typeSystem.PlatformAbstraction;

            if(paArm.HasVFPv2)
            {
                return new ArmCallState( direction, m_numberOfWordsPassedInRegistersForIntegers, m_numberOfWordsPassedInRegistersForFloatingPoints, m_numberOfWordsPerArgument );
            }
            else
            {
                return new ArmCallState( direction, m_numberOfWordsPassedInRegistersForIntegers, 0, m_numberOfWordsPerArgument );
            }
        }

        public override ZeligIR.Expression[] AssignArgument( ZeligIR.ControlFlowGraphStateForCodeTransformation cfg         ,
                                                             ZeligIR.Operator                                   insertionOp ,
                                                             ZeligIR.Expression                                 ex          ,
                                                             TypeRepresentation                                 td          ,
                                                             ZeligIR.Abstractions.CallingConvention.CallState   callState   )
        {
            ZeligIR.VariableExpression                                        sourceVar = ex as ZeligIR.VariableExpression;
            ZeligIR.VariableExpression.DebugInfo                              debugInfo;
            ZeligIR.ControlFlowGraphStateForCodeTransformation.KindOfFragment kind;

            if(callState.Direction == Direction.Caller)
            {
                //
                // On the caller side, we don't associate the fragments with the original expression,
                // since the same fragments will be used for multiple expressions, on different method calls.
                //
                sourceVar = null;
                debugInfo = null;
            }
            else
            {
                debugInfo = sourceVar.DebugName;
            }

            if(callState.CanMapToRegister( cfg.TypeSystem.PlatformAbstraction, td ))
            {
                if(callState.Direction == Direction.Callee)
                {
                    kind = ZeligIR.ControlFlowGraphStateForCodeTransformation.KindOfFragment.CopyIncomingArgumentFromPhysicalToPseudoRegister;
                }
                else
                {
                    kind = ZeligIR.ControlFlowGraphStateForCodeTransformation.KindOfFragment.AllocatePhysicalRegisterForArgument;
                }

                return cfg.MapExpressionToFragments( insertionOp, debugInfo, sourceVar, td, callState, kind );
            }
            else
            {
                if(callState.Direction == Direction.Callee)
                {
                    kind = ZeligIR.ControlFlowGraphStateForCodeTransformation.KindOfFragment.CopyIncomingArgumentFromStackToPseudoRegister;
                }
                else
                {
                    kind = ZeligIR.ControlFlowGraphStateForCodeTransformation.KindOfFragment.AllocateStackOutLocation;
                }

                return cfg.MapExpressionToFragments( insertionOp, debugInfo, sourceVar, td, callState, kind );
            }
        }

        public override ZeligIR.Expression[] AssignReturnValue( ZeligIR.ControlFlowGraphStateForCodeTransformation cfg           ,
                                                                ZeligIR.Operator                                   insertionOp   ,
                                                                ZeligIR.VariableExpression                         exReturnValue ,
                                                                TypeRepresentation                                 tdReturnValue ,
                                                                ZeligIR.Abstractions.CallingConvention.CallState   callState     )
        {
            ZeligIR.VariableExpression.DebugInfo debugInfo;

            if(exReturnValue != null)
            {
                debugInfo = exReturnValue.DebugName;
            }
            else
            {
                debugInfo = null;
            }

            if(callState.Direction == Direction.Caller)
            {
                //
                // On the caller side, we don't associate the fragments with the original expression,
                // since the same fragments will be used for multiple expressions, on different method calls.
                //
                exReturnValue = null;
                debugInfo     = null;
            }

            if(callState.CanMapResultToRegister( cfg.TypeSystem.PlatformAbstraction, tdReturnValue ))
            {
                return cfg.MapExpressionToFragments( insertionOp, debugInfo, exReturnValue, tdReturnValue, callState, ZeligIR.ControlFlowGraphStateForCodeTransformation.KindOfFragment.AllocatePhysicalRegisterForReturnValue );
            }
            else
            {
                ZeligIR.ControlFlowGraphStateForCodeTransformation.KindOfFragment kind;

                if(callState.Direction == Direction.Callee)
                {
                    kind = ZeligIR.ControlFlowGraphStateForCodeTransformation.KindOfFragment.AllocateStackInLocation;
                }
                else
                {
                    kind = ZeligIR.ControlFlowGraphStateForCodeTransformation.KindOfFragment.AllocateStackOutLocation;
                }

                //
                // For methods whose return value is too big, share the return value variable as the first slot on the stack.
                //
                return cfg.MapExpressionToFragments( insertionOp, debugInfo, exReturnValue, tdReturnValue, callState, kind );
            }
        }

        public override ZeligIR.VariableExpression[] CollectExpressionsToInvalidate( ZeligIR.ControlFlowGraphStateForCodeTransformation cfg          ,
                                                                                     ZeligIR.CallOperator                               op           ,
                                                                                     ZeligIR.Expression[]                               resFragments )
        {
            ZeligIR.VariableExpression[]                     res       = ZeligIR.VariableExpression.SharedEmptyArray;
            ZeligIR.Abstractions.CallingConvention.CallState callState = cfg.TypeSystem.CallingConvention.CreateCallState( Direction.Caller );

            //
            // Because of the aliasing between physical registers, it's important that we add the expressions to invalidate
            // in the right order:
            //
            //  1) Actual return values (first because we need that value after the call)
            //  2) Formal return values
            //  3) Call arguments
            //  4) Registers and stack locations affected by the calling convention.
            //
            res = MergeFragments( res, resFragments );

            if(op != null)
            {
                MethodRepresentation md  = op.TargetMethod;

                {
                    TypeRepresentation tdReturn = md.ReturnType;
                    if(tdReturn != cfg.TypeSystem.WellKnownTypes.System_Void)
                    {
                        ZeligIR.Expression[] resFormalFragments = AssignReturnValue( cfg, null, null, tdReturn, callState );

                        res = MergeFragments( res, resFormalFragments );
                    }
                }

                foreach(TypeRepresentation tdArg in md.ThisPlusArguments)
                {
                    ZeligIR.Expression[] argFragments = AssignArgument( cfg, null, null, tdArg, callState );

                    res = MergeFragments( res, argFragments );
                }
            }

            foreach(ZeligIR.Abstractions.RegisterDescriptor regDesc in m_typeSystem.PlatformAbstraction.GetRegisters())
            {
                bool fAdd = false;

                if(regDesc.InIntegerRegisterFile)
                {
                    //
                    // These registers are part of the calling convention, they can be scratched.
                    //
                    if((regDesc.Encoding -  EncodingDefinition.c_register_r0) < m_numberOfWordsPassedInRegistersForIntegers ||
                        regDesc.Encoding == EncodingDefinition.c_register_r12                                               ||
                        regDesc.Encoding == EncodingDefinition.c_register_lr                                                 )
                    {
                        fAdd = true;
                    }
                }

                if(regDesc.InFloatingPointRegisterFile)
                {
                    //
                    // These registers are part of the calling convention, they can be scratched.
                    //
                    if(regDesc.IsDoublePrecision)
                    {
                        if(ArmCompilationState.GetDoublePrecisionEncoding( regDesc ) < m_numberOfWordsPassedInRegistersForFloatingPoints)
                        {
                            fAdd = true;
                        }
                    }
                    else
                    {
                        if(ArmCompilationState.GetSinglePrecisionEncoding( regDesc ) < m_numberOfWordsPassedInRegistersForFloatingPoints)
                        {
                            fAdd = true;
                        }
                    }
                }

                if(fAdd)
                {
                    res = AddUniqueToFragment( res, cfg.AllocatePhysicalRegister( regDesc ) );
                }
            }
    
            res = AddUniqueToFragment( res, cfg.AllocateConditionCode() );

            return CollectAddressTakenExpressionsToInvalidate( cfg, res, op );
        }

        public override bool ShouldSaveRegister( ZeligIR.Abstractions.RegisterDescriptor regDesc )
        {
            if(regDesc.IsSpecial)
            {
                return false;
            }

            if(regDesc.IsSystemRegister)
            {
                return false;
            }

            if(regDesc.InIntegerRegisterFile)
            {
                if((regDesc.Encoding -  EncodingDefinition.c_register_r0 ) < m_numberOfWordsPassedInRegistersForIntegers ||
                    regDesc.Encoding == EncodingDefinition.c_register_r12                                                ||
                    regDesc.Encoding == EncodingDefinition.c_register_lr                                                  )
                {
                    //
                    // These registers are part of the calling convention, they can be scratched.
                    //
                    return false;
                }
            }

            if(regDesc.InFloatingPointRegisterFile)
            {
                if(regDesc.IsDoublePrecision)
                {
                    if(ArmCompilationState.GetDoublePrecisionEncoding( regDesc ) < m_numberOfWordsPassedInRegistersForFloatingPoints)
                    {
                        //
                        // These registers are part of the calling convention, they can be scratched.
                        //
                        return false;
                    }
                }
                else
                {
                    if(ArmCompilationState.GetSinglePrecisionEncoding( regDesc ) < m_numberOfWordsPassedInRegistersForFloatingPoints)
                    {
                        //
                        // These registers are part of the calling convention, they can be scratched.
                        //
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
