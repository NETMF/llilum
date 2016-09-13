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


    public class LlvmForArmV7MCallingConvention : ArmCallingConvention
    {
        public LlvmForArmV7MCallingConvention( ZeligIR.TypeSystemForCodeTransformation typeSystem )
            : base( typeSystem )
        {
        }

        public override void RegisterForNotifications( ZeligIR.TypeSystemForCodeTransformation ts,
                                                     ZeligIR.CompilationSteps.DelegationCache cache )
        {

        }

        //--//

        //////public override CallState CreateCallState( Direction direction )
        //////{
        //////    throw new Exception( "CreateCallState not implemented" );
        //////}

        //////public override ZeligIR.Expression[] AssignArgument( ZeligIR.ControlFlowGraphStateForCodeTransformation cfg,
        //////                                                     ZeligIR.Operator insertionOp,
        //////                                                     ZeligIR.Expression ex,
        //////                                                     TypeRepresentation td,
        //////                                                     ZeligIR.Abstractions.CallingConvention.CallState callState )
        //////{
        //////    throw new Exception( "AssignArgument not implemented" );   
        //////}

        //////public override ZeligIR.Expression[] AssignReturnValue( ZeligIR.ControlFlowGraphStateForCodeTransformation cfg,
        //////                                                        ZeligIR.Operator insertionOp,
        //////                                                        ZeligIR.VariableExpression exReturnValue,
        //////                                                        TypeRepresentation tdReturnValue,
        //////                                                        ZeligIR.Abstractions.CallingConvention.CallState callState )
        //////{
        //////    throw new Exception( "AssignReturnValue not implemented" );
        //////}

        //////public override ZeligIR.VariableExpression[] CollectExpressionsToInvalidate( ZeligIR.ControlFlowGraphStateForCodeTransformation cfg,
        //////                                                                             ZeligIR.CallOperator op,
        //////                                                                             ZeligIR.Expression[] resFragments )
        //////{
        //////    throw new Exception( "CollectExpressionsToInvalidate not implemented" );
        //////}

        //////public override bool ShouldSaveRegister( ZeligIR.Abstractions.RegisterDescriptor regDesc )
        //////{
        //////    throw new Exception( "ShouldSaveRegister not implemented" );
        //////}
    }

}
