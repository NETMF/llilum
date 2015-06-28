//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed partial class ImplementInternalMethods
    {
        //
        // Helper Methods
        //

        private bool ImplementObjectEquals( MethodRepresentation md )
        {
            if(TypeSystemForCodeTransformation.GetCodeForMethod( md ) == null)
            {
                ControlFlowGraphStateForCodeTransformation cfg             = (ControlFlowGraphStateForCodeTransformation)m_typeSystem.CreateControlFlowGraphState( md );

                var                                        bb              = cfg.CreateFirstNormalBasicBlock();
                var                                        bbExit          = cfg.NormalizedExitBasicBlock;
                NormalBasicBlock                           bbTRUE          = new NormalBasicBlock( cfg );
                NormalBasicBlock                           bbFALSE         = new NormalBasicBlock( cfg );
                NormalBasicBlock                           bbNext;
                                                   
                VariableExpression                         variable_return = cfg.ReturnValue;
                VariableExpression                         pThis           = cfg.Arguments[0];
                VariableExpression                         obj             = cfg.Arguments[1];
                TemporaryVariableExpression                other;

                //
                // Create proper flow control for exit basic block.
                //
                cfg.AddReturnOperator();

                //
                // Create TRUE branch.
                //
                bbTRUE.AddOperator( SingleAssignmentOperator    .New( null, variable_return, m_typeSystem.CreateConstant( md.ReturnType, 1 ) ) );
                bbTRUE.AddOperator( UnconditionalControlOperator.New( null, bbExit                                                           ) );

                //
                // Create FALSE branch.
                //
                bbFALSE.AddOperator( SingleAssignmentOperator    .New( null, variable_return, m_typeSystem.CreateConstant( md.ReturnType, 0 ) ) );
                bbFALSE.AddOperator( UnconditionalControlOperator.New( null, bbExit                                                           ) );

                ////    ;;;91             if(!(obj is T))
                ////    ;;;91             ^
                ////              Op_4   :  $other = Arg_1(obj) isinst BoxedValueTypeRepresentation(T)
                ////              Op_6   :  if $other != ZERO then goto $Next else goto $Label_FALSE
                ////
                bbNext = new NormalBasicBlock( cfg );
                other  = cfg.AllocateTemporary( pThis.Type, null );
                bb.AddOperator( IsInstanceOperator.New( null, pThis.Type, other, obj ) );

                bb.FlowControl = BinaryConditionalControlOperator.New( null, other, bbFALSE, bbNext );

                bb = bbNext;

                foreach(FieldRepresentation fd in md.OwnerType.Fields)
                {
                    if(fd is InstanceFieldRepresentation)
                    {
                        TypeRepresentation fdType = fd.FieldType;

                        bbNext = new NormalBasicBlock( cfg );

                        if(fdType is ScalarTypeRepresentation    ||
                           fdType is ReferenceTypeRepresentation ||
                           fdType is PointerTypeRepresentation    )
                        {
                            //// $pThisField <= [Arg_0(this), InstanceFieldRepresentation(fd)]
                            //// $otherField <= [$other, InstanceFieldRepresentation(fd)]
                            //// if $pThisField EQ.unsigned $otherField then goto $Label_FALSE else goto $Next
                            TemporaryVariableExpression pThisField = cfg.AllocateTemporary( fdType, null );
                            TemporaryVariableExpression otherField = cfg.AllocateTemporary( fdType, null );

                            bb.AddOperator( LoadInstanceFieldOperator.New( null, fd, pThisField, pThis, true ) );
                            bb.AddOperator( LoadInstanceFieldOperator.New( null, fd, otherField, other, true ) );

                            bb.AddOperator( CompareConditionalControlOperator.New( null, CompareAndSetOperator.ActionCondition.EQ, false, pThisField, otherField, bbFALSE, bbNext ) );
                        }
////                    else if(fdType is ValueTypeRepresentation)
////                    {
////                        ManagedPointerTypeRepresentation fdTypeAddr     = m_typeSystem.GetManagedPointerToType( fdType );
////                        TemporaryVariableExpression      tmp            = cfg.AllocateTemporary( m_typeSystem.WellKnownTypes.System_Int32 );
////                        TemporaryVariableExpression      pThisFieldAddr = cfg.AllocateTemporary( fdTypeAddr );
////                        TemporaryVariableExpression      otherFieldAddr = cfg.AllocateTemporary( fdTypeAddr );
////
////                        bb.AddOperator( LoadInstanceFieldAddressOperator.New( null, fd, pThisFieldAddr, pThis ) );
////                        bb.AddOperator( LoadInstanceFieldAddressOperator.New( null, fd, otherFieldAddr, other ) );
////
////                        bb.AddOperator( InstanceCallOperator.New( null, CallOperator.CallKind.Virtual, m_typeSystem.WellKnownMethods.Object_Equals, tmp, new Expression[] { pThisFieldAddr, otherFieldAddr } ) );
////                        bb.AddOperator( BinaryConditionalControlOperator.New( null, tmp, bbFALSE, bbNext ) );
////                    }
                        else
                        {
                            bb.AddOperator( UnconditionalControlOperator.New( null, bbNext ) );
                            //throw TypeConsistencyErrorException.Create( "Don't know how to generate comparison code for field {0}", fd );
                        }

                        bb = bbNext;
                    }
                }

                bb.AddOperator( UnconditionalControlOperator.New( null, bbTRUE ) );

                return false;
            }

            return true;
        }
    }
}
