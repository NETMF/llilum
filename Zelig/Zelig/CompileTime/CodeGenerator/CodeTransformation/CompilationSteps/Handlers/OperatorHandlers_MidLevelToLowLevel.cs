//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps.Handlers
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public class OperatorHandlers_MidLevelToLowLevel
    {
        [CompilationSteps.PhaseFilter( typeof(Phases.MidLevelToLowLevelConversion) )]
        [CompilationSteps.OperatorHandler( typeof(FieldOperator) )]
        private static void Handle_FieldOperator( PhaseExecution.NotificationContext nc )
        {
            FieldOperator       op          = (FieldOperator)nc.CurrentOperator;
            FieldRepresentation fd          =                op.Field;
            Debugging.DebugInfo debugInfo   =                op.DebugInfo;
            bool                fIsVolatile =                (fd.Flags & FieldRepresentation.Attributes.IsVolatile) != 0;
            Operator            opNew;

            CHECKS.ASSERT( op.MayThrow == false, "Unexpected throwing operator: {0}", op );

            if(op is LoadInstanceFieldAddressOperator)
            {
                opNew = BinaryOperator.New( debugInfo, BinaryOperator.ALU.ADD, false, false, op.FirstResult, op.FirstArgument, nc.TypeSystem.CreateConstant( fd.Offset ) );
            }
            else if(op is LoadInstanceFieldOperator)
            {
                opNew = LoadIndirectOperator.New( debugInfo, fd.FieldType, op.FirstResult, op.FirstArgument, new FieldRepresentation[] { fd }, fd.Offset, fIsVolatile, false );
            }
            else if(op is StoreInstanceFieldOperator)
            {
                opNew = StoreIndirectOperator.New( debugInfo, fd.FieldType, op.FirstArgument, op.SecondArgument, new FieldRepresentation[] { fd }, fd.Offset, false );
            }
            else
            {
                throw TypeConsistencyErrorException.Create( "Unexpected operator {0} at phase {1}", op, nc.Phase );
            }

            op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.CopyAnnotations );

            nc.MarkAsModified();
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof(Phases.MidLevelToLowLevelConversion) )]
        [CompilationSteps.OperatorHandler( typeof(ElementOperator) )]
        private static void Handle_ElementOperator( PhaseExecution.NotificationContext nc )
        {
            ElementOperator             op           = (ElementOperator)nc.CurrentOperator;
            Debugging.DebugInfo         debugInfo    =                  op.DebugInfo;
            Expression                  exArray      =                  op.FirstArgument;
            Expression                  exIndex      =                  op.SecondArgument;
            TypeRepresentation          tdArray      =                  exArray.Type;
            TypeRepresentation          tdElement    =                  tdArray.ContainedType;
            TypeRepresentation          tdElementPtr =                  nc.TypeSystem.GetManagedPointerToType( tdElement );
            TemporaryVariableExpression tmpPtr       =                  nc.CurrentCFG.AllocateTemporary( tdElementPtr                             , null );
            TemporaryVariableExpression tmpOffset    =                  nc.CurrentCFG.AllocateTemporary( nc.TypeSystem.WellKnownTypes.System_Int32, null );
            Operator                    opNew;

            //
            // Get a pointer to the first element.
            //
            if(op.HasAnnotation< MemoryMappedPeripheralAnnotation >())
            {
                //
                // A memory-mapped array doesn't have any length field.
                //
                opNew = SingleAssignmentOperator.New( debugInfo, tmpPtr, exArray );
            }
            else
            {
                //
                // Skip the length field.
                //
                opNew = BinaryOperator.New( debugInfo, BinaryOperator.ALU.ADD, false, false, tmpPtr, exArray, nc.TypeSystem.CreateConstant( tdArray.VirtualTable.BaseSize ) );
            }
            opNew.AddAnnotation( NotNullAnnotation.Create( nc.TypeSystem ) );
            op.AddOperatorBefore( opNew );

            //
            // Compute byte-offset within the array.
            //
            op.AddOperatorBefore( BinaryOperator.New( debugInfo, BinaryOperator.ALU.MUL, false, false, tmpOffset, exIndex, nc.TypeSystem.CreateConstant( tdArray.VirtualTable.ElementSize ) ) );

            //
            // Compute the address of the element to access.
            //
            op.AddOperatorBefore( BinaryOperator.New( debugInfo, BinaryOperator.ALU.ADD, false, false, tmpPtr, tmpPtr, tmpOffset ) );

            if(op is LoadElementAddressOperator)
            {
                opNew = SingleAssignmentOperator.New( debugInfo, op.FirstResult, tmpPtr );
            }
            else if(op is LoadElementOperator)
            {
                opNew = LoadIndirectOperator.New( debugInfo, tdElement, op.FirstResult, tmpPtr, op.AccessPath, 0, false, false );
            }
            else if(op is StoreElementOperator)
            {
                opNew = StoreIndirectOperator.New( debugInfo, tdElement, tmpPtr, op.ThirdArgument, op.AccessPath, 0, false );
            }
            else
            {
                throw TypeConsistencyErrorException.Create( "Unexpected operator {0} at phase {1}", op, nc.Phase );
            }

            op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.CopyAnnotations );

            nc.MarkAsModified();
        }
    }
}
