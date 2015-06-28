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


        [ZeligIR.CompilationSteps.OptimizationHandler(RunOnce=false, RunInSSAForm=true)]
        private static void InferBitMaskConditionals( ZeligIR.CompilationSteps.PhaseExecution.NotificationContext nc )
        {
            ZeligIR.ControlFlowGraphStateForCodeTransformation cfg = nc.CurrentCFG;

            foreach(var opCtrl in cfg.FilterOperators< ZeligIR.ConditionCodeConditionalControlOperator >())
            {
                ZeligIR.CompareOperator opCmp = cfg.FindSingleDefinition( opCtrl.FirstArgument ) as ZeligIR.CompareOperator;
                if(opCmp != null)
                {
                    bool               fNullOnRight;
                    ZeligIR.Expression ex = opCmp.IsBinaryOperationAgainstZeroValue( out fNullOnRight );
                    if(ex != null)
                    {
                        ZeligIR.BinaryOperator opDef = cfg.FindSingleDefinition( ex ) as ZeligIR.BinaryOperator;
                        if(opDef != null)
                        {
                            switch(opDef.Alu)
                            {
                                case ZeligIR.BinaryOperator.ALU.AND:
                                    //
                                    // Found code pattern:
                                    //
                                    //  t1 = A and B
                                    //  cc = compare t1 <-> 0
                                    //  if cc equal/not equal, then branch B1 else branch B2
                                    //
                                    // Substitute with:
                                    //
                                    //  cc = bittest A and B 
                                    //  if cc equal/not equal, then branch B1 else branch B2
                                    //
                                    opCmp.SubstituteWithOperator( ZeligIR.BitTestOperator.New( opCmp.DebugInfo, opCmp.FirstResult, opDef.FirstArgument, opDef.SecondArgument ), ZeligIR.Operator.SubstitutionFlags.CopyAnnotations );
                                    break;

                                case ZeligIR.BinaryOperator.ALU.SHR:
                                    {
                                        ZeligIR.ConstantExpression exRightShift = opDef.SecondArgument as ZeligIR.ConstantExpression;
                                        if(exRightShift != null && opDef.Signed == false)
                                        {
                                            ZeligIR.BinaryOperator opDef2 = cfg.FindSingleDefinition( opDef.FirstArgument ) as ZeligIR.BinaryOperator;
                                            if(opDef2 != null && opDef2.Alu == ZeligIR.BinaryOperator.ALU.SHL)
                                            {
                                                ZeligIR.ConstantExpression exLeftShift = opDef2.SecondArgument as ZeligIR.ConstantExpression;
                                                if(exLeftShift != null)
                                                {
                                                    //
                                                    // Found code pattern:
                                                    //
                                                    //  t1 = A << X
                                                    //  t2 = t1 >> Y
                                                    //  cc = compare t2 <-> 0
                                                    //  if cc equal/not equal, then branch B1 else branch B2
                                                    //
                                                    // Substitute with:
                                                    //
                                                    //  cc = bittest A and <bit field mask>
                                                    //  if cc equal/not equal, then branch B1 else branch B2
                                                    //
                                                    ulong leftShift;
                                                    ulong rightShift;

                                                    if(exLeftShift .GetAsUnsignedInteger( out leftShift  ) &&
                                                       exRightShift.GetAsUnsignedInteger( out rightShift )  )
                                                    {
                                                        int maskSize = (int)(32 - rightShift);

                                                        if(maskSize <= 8) // Only optimize bit tests that can fit in the immediate encoding of the TST instruction.
                                                        {
                                                            uint mask = ((1u << maskSize) - 1u) << (int)(32 - ((int)leftShift + maskSize));

                                                            opCmp.SubstituteWithOperator( ZeligIR.BitTestOperator.New( opCmp.DebugInfo, opCmp.FirstResult, opDef2.FirstArgument, cfg.TypeSystem.CreateConstant( mask ) ), ZeligIR.Operator.SubstitutionFlags.CopyAnnotations );
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                //
                                                // Found code pattern:
                                                //
                                                //  t1 = A >> Y
                                                //  cc = compare t1 <-> 0
                                                //  if cc equal/not equal, then branch B1 else branch B2
                                                //
                                                // Substitute with:
                                                //
                                                //  cc = bittest A and <bit field mask>
                                                //  if cc equal/not equal, then branch B1 else branch B2
                                                //
                                                ulong rightShift;

                                                if(exRightShift.GetAsUnsignedInteger( out rightShift ))
                                                {
                                                    int maskSize = (int)(32 - rightShift);

                                                    if(maskSize <= 8) // Only optimize bit tests that can fit in the immediate encoding of the TST instruction.
                                                    {
                                                        uint mask = ((1u << maskSize) - 1u) << (int)(32 - maskSize);

                                                        opCmp.SubstituteWithOperator( ZeligIR.BitTestOperator.New( opCmp.DebugInfo, opCmp.FirstResult, opDef.FirstArgument, cfg.TypeSystem.CreateConstant( mask ) ), ZeligIR.Operator.SubstitutionFlags.CopyAnnotations );
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                }
            }
        }
    }
}
