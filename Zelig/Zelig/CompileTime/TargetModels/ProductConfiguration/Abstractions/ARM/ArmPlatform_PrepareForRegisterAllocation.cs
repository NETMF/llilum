//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Configuration.Environment.Abstractions
{


    using Microsoft.Zelig.TargetModel.ArmProcessor;

    using ZeligIR = Microsoft.Zelig.CodeGeneration.IR;

    public partial class ArmPlatform
    {
        private static EncodingDefinition_ARM s_Encoding = (EncodingDefinition_ARM)CurrentInstructionSetEncoding.GetEncoding();

        //--//

        class PrepareForRegisterAllocation
        {
            //
            // State
            //

            ArmPlatform m_owner;

            //
            // Constructor Methods
            //

            internal PrepareForRegisterAllocation( ArmPlatform owner )
            {
                m_owner = owner;
            }

            //
            // Helper Methods
            //

            //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
            //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
            //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//


            [ZeligIR.CompilationSteps.PhaseFilter( typeof(ZeligIR.CompilationSteps.Phases.PrepareForRegisterAllocation) )]
            [ZeligIR.CompilationSteps.OperatorHandler( typeof(ZeligIR.LoadIndirectOperator              ) )]
            [ZeligIR.CompilationSteps.OperatorHandler( typeof(ZeligIR.StoreIndirectOperator             ) )]
            [ZeligIR.CompilationSteps.OperatorHandler( typeof(ZeligIR.SetIfConditionIsTrueOperator      ) )]
            [ZeligIR.CompilationSteps.OperatorHandler( typeof(ZeligIR.ConditionalCompareOperator        ) )]
            [ZeligIR.CompilationSteps.OperatorHandler( typeof(ZeligIR.AddressAssignmentOperator         ) )]
            [ZeligIR.CompilationSteps.OperatorHandler( typeof(ZeligIR.AbstractUnaryOperator             ) )]
            [ZeligIR.CompilationSteps.OperatorHandler( typeof(ZeligIR.ConversionOperator                ) )]
            [ZeligIR.CompilationSteps.OperatorHandler( typeof(ZeligIR.MultiWayConditionalControlOperator) )]
            [ZeligIR.CompilationSteps.OperatorHandler( typeof(ARM.MoveToCoprocessor                     ) )]
            [ZeligIR.CompilationSteps.OperatorHandler( typeof(ARM.MoveFromCoprocessor                   ) )]
            [ZeligIR.CompilationSteps.OperatorHandler( typeof(ARM.SetStatusRegisterOperator             ) )]
            [ZeligIR.CompilationSteps.OperatorHandler( typeof(ARM.LoadIndirectOperatorWithIndexUpdate   ) )]
            [ZeligIR.CompilationSteps.OperatorHandler( typeof(ARM.StoreIndirectOperatorWithIndexUpdate  ) )]
            private void Handle_GenericOperator( ZeligIR.CompilationSteps.PhaseExecution.NotificationContext nc )
            {
                var op  = nc.CurrentOperator;
                var cfg = nc.CurrentCFG;

                if(ConvertToPseudoRegister( cfg, op ))
                {
                    nc.MarkAsModified();
                    return;
                }
            }

            [ZeligIR.CompilationSteps.PhaseFilter( typeof(ZeligIR.CompilationSteps.Phases.PrepareForRegisterAllocation) )]
            [ZeligIR.CompilationSteps.OperatorHandler( typeof(ZeligIR.SingleAssignmentOperator) )]
            private void Handle_AssignmentOperator( ZeligIR.CompilationSteps.PhaseExecution.NotificationContext nc )
            {
                var op  = (ZeligIR.SingleAssignmentOperator)nc.CurrentOperator;
                var lhs =                                   op.FirstResult;
                var rhs =                                   op.FirstArgument;
                var ts  =                                   nc.TypeSystem;

                if(ts.GetLevel( lhs ) == ZeligIR.Operator.OperatorLevel.StackLocations)
                {
                    if(rhs is ZeligIR.ConstantExpression || ts.GetLevel( rhs ) == ZeligIR.Operator.OperatorLevel.StackLocations)
                    {
                        //
                        // Constant to stack or stack to stack is not allowed.
                        //
                        if(ConvertToPseudoRegister( nc.CurrentCFG, op ))
                        {
                            nc.MarkAsModified();
                            return;
                        }
                    }
                }

                var lhsRegDesc = ZeligIR.Abstractions.RegisterDescriptor.ExtractFromExpression( lhs );
                if(lhsRegDesc != null && lhsRegDesc.IsSystemRegister)
                {
                    if(rhs is ZeligIR.ConstantExpression || ts.GetLevel( rhs ) == ZeligIR.Operator.OperatorLevel.StackLocations)
                    {
                        //
                        // Constant or stack to system register is not allowed.
                        //
                        MoveToPseudoRegister( nc.CurrentCFG, op, rhs );
                        nc.MarkAsModified();
                        return;
                    }
                }
            }

            [ZeligIR.CompilationSteps.PhaseFilter( typeof(ZeligIR.CompilationSteps.Phases.PrepareForRegisterAllocation) )]
            [ZeligIR.CompilationSteps.OperatorHandler( typeof(ZeligIR.CompareOperator           ) )]
            [ZeligIR.CompilationSteps.OperatorHandler( typeof(ZeligIR.BitTestOperator           ) )]
            [ZeligIR.CompilationSteps.OperatorHandler( typeof(ZeligIR.ConditionalCompareOperator) )]
            private void Handle_GenericDataProcessingOperator( ZeligIR.CompilationSteps.PhaseExecution.NotificationContext nc )
            {
                var op = nc.CurrentOperator;

                if(ConvertToPseudoRegister( nc.CurrentCFG, op ))
                {
                    nc.MarkAsModified();
                    return;
                }

                foreach(ZeligIR.Expression ex in op.Arguments)
                {
                    ZeligIR.ConstantExpression exConst = ex as ZeligIR.ConstantExpression;

                    if(exConst != null)
                    {
                        if(CanBeUsedAsAnImmediateValue( nc, exConst ) == false)
                        {
                            MoveToPseudoRegister( nc.CurrentCFG, op, exConst );
                            nc.MarkAsModified();
                            break;
                        }
                    }
                }
            }

            [ZeligIR.CompilationSteps.PhaseFilter( typeof(ZeligIR.CompilationSteps.Phases.PrepareForRegisterAllocation) )]
            [ZeligIR.CompilationSteps.OperatorHandler( typeof(ZeligIR.AbstractBinaryOperator) )]
            private void Handle_AbstractBinaryOperator( ZeligIR.CompilationSteps.PhaseExecution.NotificationContext nc )
            {
                var op = (ZeligIR.AbstractBinaryOperator)nc.CurrentOperator;

                if(ConvertToPseudoRegister( nc.CurrentCFG, op ))
                {
                    nc.MarkAsModified();
                    return;
                }

                foreach(ZeligIR.Expression ex in op.Arguments)
                {
                    ZeligIR.ConstantExpression exConst = ex as ZeligIR.ConstantExpression;

                    if(exConst != null)
                    {
                        bool fOk = CanBeUsedAsAnImmediateValue( nc, exConst );

                        if(fOk)
                        {
                            switch(op.Alu)
                            {
                                case ZeligIR.BinaryOperator.ALU.MUL:
                                    //
                                    // Multiplication operands should be in registers.
                                    //
                                    fOk = false;
                                    break;

                                case ZeligIR.BinaryOperator.ALU.SHL:
                                case ZeligIR.BinaryOperator.ALU.SHR:
                                    //
                                    // Left operand should be in a register.
                                    //
                                    if(exConst == op.FirstArgument)
                                    {
                                        fOk = false;
                                    }
                                    break;
                            }
                        }

                        if(fOk == false)
                        {
                            MoveToPseudoRegister( nc.CurrentCFG, op, exConst );
                            nc.MarkAsModified();
                            break;
                        }
                    }
                }
            }

            [ZeligIR.CompilationSteps.PhaseFilter( typeof(ZeligIR.CompilationSteps.Phases.PrepareForRegisterAllocation) )]
            [ZeligIR.CompilationSteps.OperatorHandler( typeof(ARM.BinaryOperatorWithShift) )]
            private void Handle_ARM_BinaryOperatorWithShift( ZeligIR.CompilationSteps.PhaseExecution.NotificationContext nc )
            {
                var op  = (ARM.BinaryOperatorWithShift)nc.CurrentOperator;
                var cfg = nc.CurrentCFG;

                if(ConvertToPseudoRegister( cfg, op ))
                {
                    nc.MarkAsModified();
                    return;
                }

                if(MoveToPseudoRegisterIfConstant( cfg, op, op.FirstArgument ))
                {
                    nc.MarkAsModified();
                    return;
                }

                if(MoveToPseudoRegisterIfConstant( cfg, op, op.SecondArgument ))
                {
                    nc.MarkAsModified();
                    return;
                }
            }

            //--//

            private bool CanBeUsedAsAnImmediateValue( ZeligIR.CompilationSteps.PhaseExecution.NotificationContext nc      ,
                                                      ZeligIR.ConstantExpression                                  exConst )
            {
                ZeligIR.Operator op = nc.CurrentOperator;

                if(m_owner.HasVFPv2)
                {
                    if(exConst.IsValueFloatingPoint)
                    {
                        if(exConst.IsEqualToZero() && op is ZeligIR.CompareOperator)
                        {
                            return true;
                        }

                        return false;
                    }
                }

                if(exConst.IsEqualToZero())
                {
                    return true;
                }

                if(exConst.IsValueInteger       ||
                   exConst.IsValueFloatingPoint  )
                {
                    ulong val;

                    if(exConst.GetAsRawUlong( out val ))
                    {
                        uint valSeed;
                        uint valRot;

                        if(s_Encoding.check_DataProcessing_ImmediateValue( (uint)val, out valSeed, out valRot ) == true)
                        {
                            return true;
                        }

                        if(s_Encoding.check_DataProcessing_ImmediateValue( ~(uint)val, out valSeed, out valRot ) == true)
                        {
                            var opBin = op as ZeligIR.BinaryOperator;
                            if(opBin != null)
                            {
                                if(opBin.Alu == ZeligIR.AbstractBinaryOperator.ALU.AND)
                                {
                                    return true; // This can be represented as a BIC.
                                }
                            }
                        }
                    }
                }

                return false;
            }
        }

        public static bool ConvertToPseudoRegister( ZeligIR.ControlFlowGraphStateForCodeTransformation cfg ,
                                                    ZeligIR.Operator                                   op  )
        {
            ZeligIR.PseudoRegisterExpression var;
            bool                             fModified = false;

            for(int i = op.Results.Length; --i >= 0; )
            {
                var lhs = op.Results[i]; // Make sure we get the latest Lhs, it might have changed as part of the conversion.

                var = AllocatePseudoRegisterIfNeeded( cfg, lhs );
                if(var != null)
                {
                    op.AddOperatorAfter( ZeligIR.SingleAssignmentOperator.New( op.DebugInfo, lhs, var ) );

                    op.SubstituteDefinition( lhs, var );
                    fModified = true;
                }
            }

            if(op is ZeligIR.AddressAssignmentOperator)
            {
                // Rvalue must be in stack locations.
            }
            else
            {
                for(int i = op.Arguments.Length; --i >= 0; )
                {
                    var ex = op.Arguments[i]; // Make sure we get the latest Rhs, it might have changed as part of the conversion.

                    bool fConstantOk = true;

                    if(op is ZeligIR.LoadIndirectOperator            ||
                       op is ARM.LoadIndirectOperatorWithIndexUpdate  )
                    {
                        if(i == 0) // Destination value cannot be a constant.
                        {
                            fConstantOk = false;

                            //
                            // If the pointer is an object allocated at compile-time,
                            // we can leave it alone, it will be removed later, see EmitCode_LoadIndirectOperator.
                            //
                            var constEx = ex as ZeligIR.ConstantExpression;
                            if(constEx != null)
                            {
                                var dd = constEx.Value as ZeligIR.DataManager.DataDescriptor;
                                if(dd != null)
                                {
                                    if(dd.CanPropagate)
                                    {
                                        fConstantOk = true;
                                    }
                                }
                            }
                        }
                    }
                    else if(op is ZeligIR.StoreIndirectOperator            ||
                            op is ARM.StoreIndirectOperatorWithIndexUpdate  )
                    {
                        if(i == 0) // Destination value cannot be a constant.
                        {
                            fConstantOk = false;
                        }

                        if(i == 1) // Source value cannot be a constant.
                        {
                            fConstantOk = false;
                        }
                    }
                    else if(op is ZeligIR.ZeroExtendOperator ||
                            op is ZeligIR.SignExtendOperator || 
                            op is ZeligIR.TruncateOperator    )
                    {
                        fConstantOk = false;
                    }

                    var = AllocatePseudoRegisterIfNeeded( cfg, ex, fConstantOk );
                    if(var != null)
                    {
                        op.AddOperatorBefore( ZeligIR.SingleAssignmentOperator.New( op.DebugInfo, var, ex ) );

                        op.SubstituteUsage( ex, var );
                        fModified = true;
                    }
                }
            }

            return fModified;
        }
    }
}
