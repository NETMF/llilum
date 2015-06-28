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

        public int GetOffsetLimit( TypeRepresentation target )
        {
            if(HasVFPv2)
            {
                if(target.IsFloatingPoint)
                {
                    return 256 * sizeof(uint);
                }
            }

            switch(target.SizeOfHoldingVariable)
            {
                case 1:
                    return 256;

                case 2:
                    return 256;

                case 4:
                    return 4096;

                default:
                    throw TypeConsistencyErrorException.Create( "Unsupported size of load/store: {0}", target );
            }
        }

        [ZeligIR.CompilationSteps.PhaseFilter( typeof(ZeligIR.CompilationSteps.Phases.SplitComplexOperators) )]
        [ZeligIR.CompilationSteps.OperatorHandler( typeof(ZeligIR.IndirectOperator) )]
        private void HandleSplit_IndirectOperator( ZeligIR.CompilationSteps.PhaseExecution.NotificationContext nc )
        {
            ZeligIR.IndirectOperator op = (ZeligIR.IndirectOperator)nc.CurrentOperator;

            if(Math.Abs( op.Offset ) >= GetOffsetLimit( op.Type ))
            {
                ZeligIR.TypeSystemForCodeTransformation            ts        = nc.TypeSystem;
                ZeligIR.ControlFlowGraphStateForCodeTransformation cfg       = nc.CurrentCFG;
                Debugging.DebugInfo                                debugInfo = op.DebugInfo;
                TypeRepresentation                                 td        = op.Type;
                TypeRepresentation                                 tdPtr     = ts.GetManagedPointerToType( td );
                ZeligIR.PseudoRegisterExpression                   tmpPtr    = cfg.AllocatePseudoRegister( tdPtr );
                ZeligIR.Operator                                   opNew;

                //
                // Compute the address of the location to access.
                //
                op.AddOperatorBefore( ZeligIR.BinaryOperator.New( debugInfo, ZeligIR.BinaryOperator.ALU.ADD, false, false, tmpPtr, op.FirstArgument, ts.CreateConstant( op.Offset ) ) );

                if(op is ZeligIR.StoreIndirectOperator)
                {
                    opNew = ZeligIR.StoreIndirectOperator.New( debugInfo, td, tmpPtr, op.SecondArgument, op.AccessPath, 0, false );
                }
                else
                {
                    opNew = ZeligIR.LoadIndirectOperator.New( debugInfo, td, op.FirstResult, tmpPtr, op.AccessPath, 0, op.MayMutateExistingStorage, false );
                }

                op.SubstituteWithOperator( opNew, ZeligIR.Operator.SubstitutionFlags.Default );
                opNew.AddAnnotation( ZeligIR.BlockCopyPropagationAnnotation.Create( nc.TypeSystem, 0, false, false ) );

                nc.MarkAsModified();
            }
        }
    }
}
