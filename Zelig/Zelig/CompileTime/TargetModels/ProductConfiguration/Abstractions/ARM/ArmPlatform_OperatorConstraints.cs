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

        protected override bool CanFitTypeInPhysicalRegister( TypeRepresentation td )
        {
            if(td.ValidLayout)
            {
                if(td.IsFloatingPoint)
                {
                    if(this.HasVFPv2)
                    {
                        return true;
                    }
                }

                if(td.SizeOfHoldingVariable <= sizeof(uint))
                {
                    return true;
                }
            }

            return false;
        }

        public override bool CanPropagateCopy( ZeligIR.SingleAssignmentOperator opSrc               ,
                                               ZeligIR.Operator                 opDst               ,
                                               int                              exIndexInDst        ,
                                               ZeligIR.VariableExpression[]     variables           ,
                                               BitVector[]                      variableUses        ,
                                               BitVector[]                      variableDefinitions ,
                                               ZeligIR.Operator[]               operators           )
        {
            ZeligIR.Expression exSrc = opSrc.FirstArgument;

            //
            // Don't propagate system registers.
            //
            {
                ZeligIR.PhysicalRegisterExpression regSrc = ZeligIR.PhysicalRegisterExpression.Extract( exSrc );

                if(regSrc != null && regSrc.RegisterDescriptor.IsSystemRegister)
                {
                    return false;
                }

                if(opDst is ZeligIR.SingleAssignmentOperator)
                {
                    ZeligIR.PhysicalRegisterExpression regDst = ZeligIR.PhysicalRegisterExpression.Extract( opDst.FirstResult );

                    if(regDst != null && regDst.RegisterDescriptor.IsSystemRegister)
                    {
                        return false;
                    }
                }
            }

            //--//

            ZeligIR.Abstractions.RegisterClass constraintDst = ZeligIR.RegisterAllocationConstraintAnnotation.ComputeConstraintsForRHS( opDst, exIndexInDst );

            if(constraintDst != ZeligIR.Abstractions.RegisterClass.None)
            {
                //
                // The destination expression is constrained, make sure all the definitions of the source expression are compatible.
                //
                ZeligIR.Abstractions.RegisterClass constraintSrc = ZeligIR.Abstractions.RegisterClass.None;

                foreach(int def in variableDefinitions[exSrc.SpanningTreeIndex])
                {
                    constraintSrc |= ZeligIR.RegisterAllocationConstraintAnnotation.ComputeConstraintsForLHS( operators[def], (ZeligIR.VariableExpression)exSrc );
                }

                if((constraintSrc & constraintDst) != constraintDst)
                {
                    //
                    // Source is less constrained than the destination, we cannot propagate the expression.
                    //
                    return false;
                }
            }

            return true;
        }

        //--//

        [ZeligIR.CompilationSteps.OptimizationHandler(RunOnce=false, RunInSSAForm=true)]
        private static void RemoveConstantFromIndirectOperators( ZeligIR.CompilationSteps.PhaseExecution.NotificationContext nc )
        {
            ZeligIR.ControlFlowGraphStateForCodeTransformation cfg = nc.CurrentCFG;

            foreach(var opInd in cfg.FilterOperators< ZeligIR.IndirectOperator >())
            {
                var debugInfo = opInd.DebugInfo;
                var rhs       = opInd.FirstArgument as ZeligIR.ConstantExpression;

                if(rhs != null)
                {
                    var tmp = cfg.AllocatePseudoRegister( rhs.Type );

                    opInd.AddOperatorBefore( ZeligIR.SingleAssignmentOperator.New( debugInfo, tmp, rhs ) );

                    opInd.SubstituteUsage( rhs, tmp );
                    opInd.AddAnnotation( ZeligIR.BlockCopyPropagationAnnotation.Create( nc.TypeSystem, 0, false, false ) );
                }
            }
        }
    }
}
