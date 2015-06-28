//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps.Handlers
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public class OperatorHandlers_FromImplicitToExplictExceptions
    {
        [CompilationSteps.PhaseFilter( typeof(Phases.FromImplicitToExplictExceptions) )]
        [CompilationSteps.OperatorHandler( typeof(BinaryOperator) )]
        private static void Handle_BinaryOperator( PhaseExecution.NotificationContext nc )
        {
            BinaryOperator op = (BinaryOperator)nc.CurrentOperator;

            if(op.CheckOverflow)
            {
                var cc = CreateOverflowCheck( nc, op );

                var opNew = BinaryOperatorWithCarryOut.New( op.DebugInfo, op.Alu, op.Signed, false, op.FirstResult, cc, op.FirstArgument, op.SecondArgument );

                op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.CopyAnnotations );
            }
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof(Phases.FromImplicitToExplictExceptions) )]
        [CompilationSteps.OperatorHandler( typeof(UnaryOperator) )]
        private static void Handle_UnaryOperator( PhaseExecution.NotificationContext nc )
        {
            UnaryOperator op = (UnaryOperator)nc.CurrentOperator;

            if(op.CheckOverflow)
            {
                var cc = CreateOverflowCheck( nc, op );

                var opNew = UnaryOperatorWithCarryOut.New( op.DebugInfo, op.Alu, op.Signed, false, op.FirstResult, cc, op.FirstArgument );

                op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.CopyAnnotations );
            }
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof(Phases.FromImplicitToExplictExceptions) )]
        [CompilationSteps.OperatorHandler( typeof(SignExtendOperator) )]
        private static void Handle_SignExtendOperator( PhaseExecution.NotificationContext nc )
        {
            SignExtendOperator op  = (SignExtendOperator)nc.CurrentOperator;
            VariableExpression lhs = op.FirstResult;
            Expression         rhs = op.FirstArgument;
            var                di  = op.DebugInfo;

            if(lhs.Type.Size == rhs.Type.Size && lhs.Type.Size == op.SignificantSize)
            {
                var opNew = SingleAssignmentOperator.New( di, lhs, rhs );

                op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.CopyAnnotations );

                nc.MarkAsModified();
            }
            else if(op.CheckOverflow)
            {
                var opNew = SignExtendOperator.New( di, op.SignificantSize, false, lhs, rhs );

                op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.CopyAnnotations );

                VerifyNoLossOfPrecision( nc, opNew, lhs, rhs );
            }
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof(Phases.FromImplicitToExplictExceptions) )]
        [CompilationSteps.OperatorHandler( typeof(ZeroExtendOperator) )]
        private static void Handle_ZeroExtendOperator( PhaseExecution.NotificationContext nc )
        {
            ZeroExtendOperator op = (ZeroExtendOperator)nc.CurrentOperator;
            VariableExpression lhs = op.FirstResult;
            Expression         rhs = op.FirstArgument;

            if(lhs.Type.Size == rhs.Type.Size && lhs.Type.Size == op.SignificantSize)
            {
                var opNew = SingleAssignmentOperator.New( op.DebugInfo, lhs, rhs );

                op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.CopyAnnotations );

                nc.MarkAsModified();
            }
            else if(op.CheckOverflow)
            {
                var opNew = ZeroExtendOperator.New( op.DebugInfo, op.SignificantSize, false, lhs, rhs );

                op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.CopyAnnotations );

                VerifyNoLossOfPrecision( nc, opNew, lhs, rhs );
            }
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof(Phases.FromImplicitToExplictExceptions) )]
        [CompilationSteps.OperatorHandler( typeof(TruncateOperator) )]
        private static void Handle_TruncateOperator( PhaseExecution.NotificationContext nc )
        {
            TruncateOperator   op  = (TruncateOperator)nc.CurrentOperator;
            VariableExpression lhs = op.FirstResult;
            Expression         rhs = op.FirstArgument;

            if(lhs.Type.Size == rhs.Type.Size && lhs.Type.Size == op.SignificantSize)
            {
                var opNew = SingleAssignmentOperator.New( op.DebugInfo, lhs, rhs );

                op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.CopyAnnotations );

                nc.MarkAsModified();
            }
            else if(op.CheckOverflow)
            {
                TruncateOperator opNew = TruncateOperator.New( op.DebugInfo, op.SignificantSize, false, lhs, rhs );
    
                op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.CopyAnnotations );

                VerifyNoLossOfPrecision( nc, opNew, lhs, rhs );
            }
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof(Phases.FromImplicitToExplictExceptions) )]
        [CompilationSteps.OperatorHandler( typeof(NullCheckOperator) )]
        private static void Handle_NullCheckOperator( PhaseExecution.NotificationContext nc )
        {
            Operator                 op      = nc.CurrentOperator;
            CompilationConstraints[] ccArray = nc.CurrentCFG.CompilationConstraintsAtOperator( op );
            Expression               addr    = op.FirstArgument;

            //
            // Are we checking the "this" pointer? If so, we know it's not null, other parts of the system will ensure that.
            //
            if(addr == nc.CurrentCFG.Arguments[0])
            {
                op.Delete();
            }
            //
            // Is this a managed pointer? If so, we know it's not null by construction (and if there's a cast to one, the cast will check it).
            //
            else if(addr.Type is ManagedPointerTypeRepresentation)
            {
                op.Delete();
            }
            else if(ControlFlowGraphState.HasCompilationConstraint( ccArray, CompilationConstraints.NullChecks_OFF      ) ||
                    ControlFlowGraphState.HasCompilationConstraint( ccArray, CompilationConstraints.NullChecks_OFF_DEEP )  )
            {
                op.Delete();
            }
            else
            {
                Debugging.DebugInfo debugInfo = op.DebugInfo;
                BasicBlock          current   = op.BasicBlock;
                BasicBlock          continueBB;
                BasicBlock          throwBB;

                SplitAndCall( nc, op, true, nc.TypeSystem.WellKnownMethods.ThreadImpl_ThrowNullException, out continueBB, out throwBB );

                //--//
                                    
                current.AddOperator( BinaryConditionalControlOperator.New( debugInfo, addr, throwBB, continueBB ) );
            }

            nc.MarkAsModified();
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof(Phases.FromImplicitToExplictExceptions) )]
        [CompilationSteps.OperatorHandler( typeof(OutOfBoundCheckOperator) )]
        private static void Handle_OutOfBoundCheckOperator( PhaseExecution.NotificationContext nc )
        {
            Operator op = nc.CurrentOperator;

            CompilationConstraints[] ccArray = nc.CurrentCFG.CompilationConstraintsAtOperator( op );

            if(ControlFlowGraphState.HasCompilationConstraint( ccArray, CompilationConstraints.BoundsChecks_OFF      ) ||
               ControlFlowGraphState.HasCompilationConstraint( ccArray, CompilationConstraints.BoundsChecks_OFF_DEEP )  )
            {
                op.Delete();
            }
            else
            {
                TypeSystemForCodeTransformation ts         = nc.TypeSystem;
                WellKnownTypes                  wkt        = ts.WellKnownTypes;
                WellKnownFields                 wkf        = ts.WellKnownFields;
                WellKnownMethods                wkm        = ts.WellKnownMethods;
                Debugging.DebugInfo             debugInfo  = op.DebugInfo;
                Expression                      exAddress  = op.FirstArgument;
                Expression                      exIndex    = op.SecondArgument;
                BasicBlock                      currentBB  = op.BasicBlock;
                BasicBlock                      continueBB;
                BasicBlock                      throwBB;

                SplitAndCall( nc, op, true, wkm.ThreadImpl_ThrowIndexOutOfRangeException, out continueBB, out throwBB );

                //--//

                ControlFlowGraphStateForCodeTransformation cfg      = nc.CurrentCFG;
                TemporaryVariableExpression                exLength = cfg.AllocateTemporary( wkt.System_UInt32, null );

                currentBB.AddOperator( LoadInstanceFieldOperator        .New( debugInfo, wkf.ArrayImpl_m_numElements, exLength, exAddress, false                                 ) );
                currentBB.AddOperator( CompareConditionalControlOperator.New( debugInfo, CompareAndSetOperator.ActionCondition.LT, false, exIndex, exLength, throwBB, continueBB ) );
            }

            nc.MarkAsModified();
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof(Phases.FromImplicitToExplictExceptions) )]
        [CompilationSteps.OperatorHandler( typeof(OverflowCheckOperator) )]
        private static void Handle_OverflowCheckOperator( PhaseExecution.NotificationContext nc )
        {
            Operator                op        =                          nc.CurrentOperator;
            Debugging.DebugInfo     debugInfo =                          op.DebugInfo;
            ConditionCodeExpression cc        = (ConditionCodeExpression)op.FirstArgument;
            BasicBlock              current   =                          op.BasicBlock;
            BasicBlock              continueBB;
            BasicBlock              throwBB;

            SplitAndCall( nc, op, true, nc.TypeSystem.WellKnownMethods.ThreadImpl_ThrowOverflowException, out continueBB, out throwBB );

            //--//

            current.AddOperator( ConditionCodeConditionalControlOperator.New( debugInfo, ConditionCodeExpression.Comparison.Overflow, cc, continueBB, throwBB ) );

            nc.MarkAsModified();
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        private static ConditionCodeExpression CreateOverflowCheck( PhaseExecution.NotificationContext nc ,
                                                                    Operator                           op )
        {
            ConditionCodeExpression cc = nc.CurrentCFG.EnsureConditionCode( op.Results );

            op.AddOperatorAfter( OverflowCheckOperator.New( op.DebugInfo, cc ) );

            nc.MarkAsModified();

            return cc;
        }

        private static void SplitAndCall(     PhaseExecution.NotificationContext nc              ,
                                              Operator                           op              ,
                                              bool                               fRemoveOperator ,
                                              MethodRepresentation               md              ,
                                          out BasicBlock                         continueBB      ,
                                          out BasicBlock                         throwBB         )
        {
            Debugging.DebugInfo debugInfo = op.DebugInfo;
            BasicBlock          current   = op.BasicBlock;

            continueBB = current.SplitAtOperator( op, fRemoveOperator, false );
            throwBB    = NormalBasicBlock.CreateWithSameProtection( current );

            //--//
                   
            Expression[] rhs = nc.TypeSystem.AddTypePointerToArgumentsOfStaticMethod( md );

            throwBB.AddOperator( StaticCallOperator .New( debugInfo, CallOperator.CallKind.Direct, md, rhs ) );
            throwBB.AddOperator( DeadControlOperator.New( debugInfo                                        ) );

            //--//

            nc.MarkAsModified();
        }

        private static void VerifyNoLossOfPrecision( PhaseExecution.NotificationContext nc  ,
                                                     Operator                           op  ,
                                                     VariableExpression                 lhs ,
                                                     Expression                         rhs )
        {
            var      cfg    = nc.CurrentCFG;
            var      lhsExt = cfg.AllocateTemporary( rhs.Type, null );
            Operator opNew;

            if(lhs.Type.IsSigned)
            {
                opNew = SignExtendOperator.New( op.DebugInfo, lhs.Type.Size, false, lhsExt, lhs );
            }
            else
            {
                opNew = ZeroExtendOperator.New( op.DebugInfo, lhs.Type.Size, false, lhsExt, lhs );
            }

            op.AddOperatorAfter( opNew );

            //--//

            BasicBlock continueBB;
            BasicBlock throwBB;

            SplitAndCall( nc, opNew.GetNextOperator(), false, nc.TypeSystem.WellKnownMethods.ThreadImpl_ThrowOverflowException, out continueBB, out throwBB );

            //--//

            opNew.BasicBlock.AddOperator( CompareConditionalControlOperator.New( op.DebugInfo, CompareAndSetOperator.ActionCondition.NE, false, lhsExt, rhs, continueBB, throwBB ) );

            nc.MarkAsModified();
        }
    }
}
