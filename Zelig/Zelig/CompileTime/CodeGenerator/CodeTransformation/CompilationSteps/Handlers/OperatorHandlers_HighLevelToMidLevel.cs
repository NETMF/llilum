//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps.Handlers
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public class OperatorHandlers_HighLevelToMidLevel
    {
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof(Phases.HighLevelToMidLevelConversion) )]
        [CompilationSteps.OperatorHandler( typeof(LeaveControlOperator) )]
        private static void Handle_LeaveControlOperator( PhaseExecution.NotificationContext nc )
        {
            LeaveControlOperator op = (LeaveControlOperator)nc.CurrentOperator;

            UnconditionalControlOperator opNew = UnconditionalControlOperator.New( op.DebugInfo, op.TargetBranch );
            op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.Default );

            nc.MarkAsModified();
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        private static VariableExpression LoadVirtualMethodPointer(
            PhaseExecution.NotificationContext nc,
            InstanceCallOperator call,
            Expression target,
            MethodRepresentation method)
        {
            TypeSystemForCodeTransformation typeSystem = nc.TypeSystem;
            Debugging.DebugInfo             debugInfo  = call.DebugInfo;
            TemporaryVariableExpression     methodPointer;

            if ( method.OwnerType is InterfaceTypeRepresentation )
            {
                // Load MethodPointers for interface.
                MethodRepresentation        mdVTableGetInterface = typeSystem.WellKnownMethods.VTable_GetInterface;
                TemporaryVariableExpression methodPointers       = nc.AllocateTemporary( mdVTableGetInterface.ReturnType );
                Expression[]                rhs                  = typeSystem.AddTypePointerToArgumentsOfStaticMethod( mdVTableGetInterface, target, typeSystem.GetVTable( method.OwnerType ) );
                CallOperator                newCall              = StaticCallOperator.New( debugInfo, CallOperator.CallKind.Direct, mdVTableGetInterface, VariableExpression.ToArray( methodPointers ), rhs );
                call.AddOperatorBefore( newCall );

                // Load the code pointer from the list. No null or range check, we know both will always pass.
                int idx = method.FindInterfaceTableIndex();
                methodPointer = nc.AllocateTemporary( methodPointers.Type.ContainedType );
                call.AddOperatorBefore( LoadElementOperator.New( debugInfo, methodPointer, methodPointers, typeSystem.CreateConstant( idx ), null, false ) );
            }
            else
            {
                // Load the target object's VTable.
                MethodRepresentation        mdVTableGet = typeSystem.WellKnownMethods.VTable_Get;
                TemporaryVariableExpression tmpVTable   = nc.AllocateTemporary( mdVTableGet.OwnerType );
                Expression[]                rhs         = typeSystem.AddTypePointerToArgumentsOfStaticMethod( mdVTableGet, target );
                CallOperator                newCall     = StaticCallOperator.New( debugInfo, CallOperator.CallKind.Direct, mdVTableGet, VariableExpression.ToArray( tmpVTable ), rhs );
                call.AddOperatorBefore( newCall );

                // Get method code pointers. The VTable can never be null here, so don't add null-checks.
                FieldRepresentation         fdMethodPointers = typeSystem.WellKnownFields.VTable_MethodPointers;
                TemporaryVariableExpression methodPointers   = nc.AllocateTemporary( fdMethodPointers.FieldType );
                call.AddOperatorBefore( LoadInstanceFieldOperator.New( debugInfo, fdMethodPointers, methodPointers, tmpVTable, false ) );

                // Load the code pointer from the list. No null or range check, we know both will always pass.
                int index = method.FindVirtualTableIndex();
                methodPointer = nc.AllocateTemporary( methodPointers.Type.ContainedType );
                call.AddOperatorBefore( LoadElementOperator.New( debugInfo, methodPointer, methodPointers, typeSystem.CreateConstant( index ), null, false ) );
            }

            return methodPointer;
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof(Phases.HighLevelToMidLevelConversion) )]
        [CompilationSteps.OperatorHandler( typeof(InstanceCallOperator) )]
        private static void Handle_Convert_VirtualCallOperator( PhaseExecution.NotificationContext nc )
        {
            InstanceCallOperator            op         = (InstanceCallOperator)nc.CurrentOperator;
            TypeSystemForCodeTransformation typeSystem = nc.TypeSystem;

            if(op.CallType == CallOperator.CallKind.Virtual)
            {
                if(MethodTransformations.AttemptDevirtualization( typeSystem, op ))
                {
                    nc.MarkAsModified();
                    return;
                }

                VariableExpression methodPointer = LoadVirtualMethodPointer( nc, op, op.FirstArgument, op.TargetMethod );
                Expression[] rhs = ArrayUtility.InsertAtHeadOfNotNullArray( op.Arguments, methodPointer );
                CallOperator call = IndirectCallOperator.New( op.DebugInfo, op.TargetMethod, op.Results, rhs, false );

                op.SubstituteWithOperator( call, Operator.SubstitutionFlags.CopyAnnotations );

                nc.MarkAsModified();
            }
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof(Phases.HighLevelToMidLevelConversion) )]
        [CompilationSteps.OperatorHandler( typeof(MethodRepresentationOperator) )]
        private static void Handle_Convert_DelegateCreation( PhaseExecution.NotificationContext nc )
        {
            ControlFlowGraphStateForCodeTransformation cfg        =                               nc.CurrentCFG;
            MethodRepresentationOperator               op         = (MethodRepresentationOperator)nc.CurrentOperator;
            MethodRepresentation                       mdDelegate =                               op.Method;
            VariableExpression                         mdPtr      =                               op.FirstResult;
            Operator                                   def        =                               cfg.FindSingleDefinition( mdPtr );
            InstanceCallOperator                       call       =                               cfg.FindSingleUse       ( mdPtr ) as InstanceCallOperator;

            if(def != null && call != null)
            {
                TypeSystemForCodeTransformation typeSystem = nc.TypeSystem;
                WellKnownMethods                wkm        = typeSystem.WellKnownMethods;
                Debugging.DebugInfo             debugInfo  = call.DebugInfo;
                Expression                      dlg        = call.FirstArgument;
                Expression                      target     = call.SecondArgument;
                Expression                      exCode;
                CallOperator                    newCall;
                Expression[]                    rhs;

                if(op.Arguments.Length == 0)
                {
                    //
                    // Direct method.
                    //
                    CodePointer    cp  = typeSystem.CreateCodePointer( mdDelegate );
                    WellKnownTypes wkt = typeSystem.WellKnownTypes;

                    exCode = typeSystem.CreateConstant( wkt.Microsoft_Zelig_Runtime_TypeSystem_CodePointer, cp );
                }
                else
                {
                    //
                    // Virtual method.
                    //
                    Expression source = op.FirstArgument;
                    if(source != target)
                    {
                        throw TypeConsistencyErrorException.Create( "Cannot create an instance delegate on '{0}' when the method comes from '{1}'", target, source );
                    }

                    exCode = LoadVirtualMethodPointer( nc, call, target, mdDelegate );
                }

                foreach( MethodRepresentation mdParent in call.TargetMethod.OwnerType.Extends.Methods )
                {
                    if( mdParent is ConstructorMethodRepresentation )
                    {
                        rhs = new Expression[ ] { dlg, target, exCode };
                        newCall = InstanceCallOperator.New( debugInfo, CallOperator.CallKind.Direct, mdParent, call.Results, rhs, true );

                        call.SubstituteWithOperator( newCall, Operator.SubstitutionFlags.CopyAnnotations );

                        //
                        // Make sure we get rid of the LDFTN/LDVIRTFTN operators.
                        //
                        op.Delete( );

                        nc.StopScan( );
                        return;
                    }
                }
            }

            throw TypeConsistencyErrorException.Create( "Unsupported delegate creation at {0}", op );
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof(Phases.HighLevelToMidLevelConversion) )]
        [CompilationSteps.OperatorHandler( typeof(FieldOperator) )]
        private static void Handle_FieldOperator( PhaseExecution.NotificationContext nc )
        {
            TypeSystemForCodeTransformation            ts        =                nc.TypeSystem;
            FieldOperator                              op        = (FieldOperator)nc.CurrentOperator;
            ControlFlowGraphStateForCodeTransformation cfg       =                nc.CurrentCFG;
            Debugging.DebugInfo                        debugInfo = op.DebugInfo;
            FieldRepresentation                        fd        = op.Field;

            {
                CustomAttributeRepresentation caFd;

                if(ts.RegisterAttributes.TryGetValue( fd, out caFd ))
                {
                    TypeRepresentation tdField = fd.FieldType;

                    if(tdField is ScalarTypeRepresentation)
                    {
                        //
                        // Scalar fields are handled normally.
                        //
                    }
                    else if(ts.MemoryMappedBitFieldPeripherals.ContainsKey( tdField ))
                    {
                        //
                        // Bitfields are handled normally.
                        //
                    }
                    else
                    {
                        TypeRepresentation tdTarget;

                        if(tdField is ArrayReferenceTypeRepresentation)
                        {
                            tdTarget = tdField.ContainedType; 
                        }
                        else
                        {
                            tdTarget = tdField;
                        }

                        if(ts.MemoryMappedPeripherals.ContainsKey( tdTarget ) == false)
                        {
                            if(tdTarget is ScalarTypeRepresentation)
                            {
                                //
                                // Accept memory-mapped arrays of scalar types.
                                //
                            }
                            else if(ts.MemoryMappedBitFieldPeripherals.ContainsKey( tdTarget ))
                            {
                                //
                                // Arrays of bitfields are handled normally.
                                //
                            }
                            else
                            {
                                throw TypeConsistencyErrorException.Create( "Cannot use type '{0}' for Register field '{1}'", tdField.FullNameWithAbbreviation, fd.ToShortString() );
                            }
                        }

                        Operator opNew;
                        uint     offset  = (uint)caFd.GetNamedArg( "Offset" );  

                        if(op is LoadInstanceFieldAddressOperator)
                        {
                            throw TypeConsistencyErrorException.Create( "Cannot take the address of section '{0}' of memory-mapped peripheral '{1}'", fd.Name, fd.OwnerType.FullNameWithAbbreviation );
                        }
                        else if(op is LoadInstanceFieldOperator)
                        {
                            opNew = BinaryOperator.New( debugInfo, BinaryOperator.ALU.ADD, false, false, op.FirstResult, op.FirstArgument, nc.TypeSystem.CreateConstant( offset ) );
                            opNew.AddAnnotation( NotNullAnnotation               .Create( ts       ) );
                            opNew.AddAnnotation( MemoryMappedPeripheralAnnotation.Create( ts, caFd ) );
                        }
                        else if(op is StoreInstanceFieldOperator)
                        {
                            throw TypeConsistencyErrorException.Create( "Cannot update reference to section '{0}' of memory-mapped peripheral '{1}'", fd.Name, fd.OwnerType.FullNameWithAbbreviation );
                        }
                        else
                        {
                            throw TypeConsistencyErrorException.Create( "Unexpected operator {0} at phase {1}", op, nc.Phase );
                        }

                        CHECKS.ASSERT( opNew.MayThrow == false, "Failed to lower call operator {0}", op );

                        op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.CopyAnnotations );

                        nc.MarkAsModified();
                        return;
                    }
                }
            }

            {
                BitFieldDefinition bfDef;

                if(ts.BitFieldRegisterAttributes.TryGetValue( fd, out bfDef ))
                {
                    if((fd.FieldType is ScalarTypeRepresentation) == false)
                    {
                        throw TypeConsistencyErrorException.Create( "Unexpected operator {0} at phase {1}", op, nc.Phase );
                    }

                    bool fLoad;

                    if(op is LoadInstanceFieldAddressOperator)
                    {
                        throw TypeConsistencyErrorException.Create( "Cannot take the address of section '{0}' of memory-mapped peripheral '{1}'", fd.Name, fd.OwnerType.FullNameWithAbbreviation );
                    }
                    else if(op is LoadInstanceFieldOperator)
                    {
                        fLoad = true;
                    }
                    else if(op is StoreInstanceFieldOperator)
                    {
                        fLoad = false;
                    }
                    else
                    {
                        throw TypeConsistencyErrorException.Create( "Unexpected operator {0} at phase {1}", op, nc.Phase );
                    }

                    //--//

                    bool                          fSigned     = fd.FieldType.IsSigned;
                    TypeRepresentation            td          = fd.OwnerType;
                    CustomAttributeRepresentation caTd        = ts.MemoryMappedBitFieldPeripherals[td];
                    TypeRepresentation            tdPhysical  = caTd.GetNamedArg< TypeRepresentation >( "PhysicalType" );
                    VariableExpression            tmp         = nc.CurrentCFG.AllocateTemporary( tdPhysical, null );
                    Operator                      opNew;

                    op.AddOperatorBefore( LoadIndirectOperator.New( debugInfo, tdPhysical, tmp, op.FirstArgument, null, fd.Offset, true, false ) );

                    if(bfDef.Sections.Length == 1)
                    {
                        var                      sec       = bfDef.Sections[0];
                        uint                     position  = sec.Position;
                        uint                     size      = sec.Size;
                        Runtime.BitFieldModifier modifiers = sec.Modifiers;

                        //--//

                        uint mask        = (1u << (int)size) - 1u;
                        uint maskShifted = mask << (int)position;

                        if(fLoad)
                        {
                            if(position == 0 && fSigned == false)
                            {
                                //
                                // TODO: Add ability to verify with Platform Abstraction if it's possible to encode the constant in a single instruction.
                                //
                                opNew = BinaryOperator.New( debugInfo, BinaryOperator.ALU.AND, false, false, op.FirstResult, tmp, ts.CreateConstant( mask ) );
                            }
                            else
                            {
                                //
                                // We need to load the physical register, and extract the bitfield, which we achieve with a left/right shift pair.
                                //
                                VariableExpression tmp2 = nc.CurrentCFG.AllocateTemporary( tdPhysical, null );

                                op.AddOperatorBefore( BinaryOperator.New( debugInfo, BinaryOperator.ALU.SHL, false  , false, tmp2          , tmp , ts.CreateConstant( 32 - (position + size) ) ) );
                                opNew =               BinaryOperator.New( debugInfo, BinaryOperator.ALU.SHR, fSigned, false, op.FirstResult, tmp2, ts.CreateConstant( 32 -             size  ) );
                            }
                        }
                        else
                        {
                            if(size == 1 && op.SecondArgument is ConstantExpression)
                            {
                                //
                                // Setting/clearing a single bit. No need to perform complex masking.
                                //
                                ConstantExpression ex = (ConstantExpression)op.SecondArgument;
                                ulong              val;

                                if(ex.GetAsUnsignedInteger( out val ) == false)
                                {
                                    throw TypeConsistencyErrorException.Create( "Expecting a numeric constant, got '{0}'", ex );
                                }

                                VariableExpression tmp2 = nc.CurrentCFG.AllocateTemporary( tdPhysical, null );

                                if((val & 1) != 0)
                                {
                                    ConstantExpression exMask = ts.CreateConstant(  maskShifted );

                                    op.AddOperatorBefore( BinaryOperator.New( debugInfo, BinaryOperator.ALU.OR, false, false, tmp2, tmp, exMask ) );
                                }
                                else
                                {
                                    ConstantExpression exMaskNot = ts.CreateConstant( ~maskShifted );

                                    op.AddOperatorBefore( BinaryOperator.New( debugInfo, BinaryOperator.ALU.AND, false, false, tmp2, tmp, exMaskNot ) );
                                }

                                opNew = StoreIndirectOperator.New( debugInfo, td, op.FirstArgument, tmp2, null, fd.Offset, false );
                            }
                            else
                            {
                                //
                                // We need to load the physical register, shift it and mask it, then mask and merge with the new value, and finally write it back.
                                //
                                VariableExpression tmp2      = nc.CurrentCFG.AllocateTemporary( tdPhysical, null );
                                VariableExpression tmp3      = nc.CurrentCFG.AllocateTemporary( tdPhysical, null );
                                VariableExpression tmp4      = nc.CurrentCFG.AllocateTemporary( tdPhysical, null );
                                VariableExpression tmp5      = nc.CurrentCFG.AllocateTemporary( tdPhysical, null );
                                ConstantExpression exMask    = ts.CreateConstant(  maskShifted );
                                ConstantExpression exMaskNot = ts.CreateConstant( ~maskShifted );

                                op.AddOperatorBefore( BinaryOperator.New( debugInfo, BinaryOperator.ALU.SHL, false, false, tmp2, op.SecondArgument, ts.CreateConstant( position ) ) );
                                op.AddOperatorBefore( BinaryOperator.New( debugInfo, BinaryOperator.ALU.AND, false, false, tmp3, tmp2             , exMask                      ) );
                                op.AddOperatorBefore( BinaryOperator.New( debugInfo, BinaryOperator.ALU.AND, false, false, tmp4, tmp              , exMaskNot                   ) );
                                op.AddOperatorBefore( BinaryOperator.New( debugInfo, BinaryOperator.ALU.OR , false, false, tmp5, tmp3             , tmp4                        ) );

                                opNew = StoreIndirectOperator.New( debugInfo, td, op.FirstArgument, tmp5, null, fd.Offset, false );
                            }
                        }
                    }
                    else
                    {
                        uint               totalSize = bfDef.TotalSize;
                        VariableExpression lastVar   = null;

                        foreach(var sec in bfDef.Sections)
                        {
                            uint                     position  = sec.Position;
                            uint                     size      = sec.Size;
                            uint                     offset    = sec.Offset;
                            Runtime.BitFieldModifier modifiers = sec.Modifiers;

                            //--//

                            uint               mask        = (1u << (int)size) - 1u;
                            uint               maskShifted = mask << (int)position;
                            VariableExpression newVar      = nc.CurrentCFG.AllocateTemporary( tdPhysical, null );

                            if(fLoad)
                            {
                                bool fLastSection = (offset + size == totalSize);
                                bool fSigned2     = fSigned && fLastSection;

                                VariableExpression newVarUnshifted = nc.CurrentCFG.AllocateTemporary( tdPhysical, null );

                                if(position == 0 && fSigned2 == false)
                                {
                                    //
                                    // TODO: Add ability to verify with Platform Abstraction if it's possible to encode the constant in a single instruction.
                                    //
                                    op.AddOperatorBefore( BinaryOperator.New( debugInfo, BinaryOperator.ALU.AND, false, false, newVarUnshifted, tmp, ts.CreateConstant( mask ) ) );
                                }
                                else
                                {
                                    //
                                    // We need to load the physical register, and extract the bitfield, which we achieve with a left/right shift pair.
                                    //
                                    VariableExpression tmp2 = nc.CurrentCFG.AllocateTemporary( tdPhysical, null );

                                    op.AddOperatorBefore( BinaryOperator.New( debugInfo, BinaryOperator.ALU.SHL, false   , false, tmp2           , tmp , ts.CreateConstant( 32 - (position + size) ) ) );
                                    op.AddOperatorBefore( BinaryOperator.New( debugInfo, BinaryOperator.ALU.SHR, fSigned2, false, newVarUnshifted, tmp2, ts.CreateConstant( 32 -             size  ) ) );
                                }

                                op.AddOperatorBefore( BinaryOperator.New( debugInfo, BinaryOperator.ALU.SHL, false, false, newVar, newVarUnshifted, ts.CreateConstant( offset ) ) );

                                if(lastVar != null)
                                {
                                    VariableExpression mergeVar = nc.CurrentCFG.AllocateTemporary( tdPhysical, null );

                                    op.AddOperatorBefore( BinaryOperator.New( debugInfo, BinaryOperator.ALU.OR, false, false, mergeVar, lastVar, newVar ) );

                                    lastVar = mergeVar;
                                }
                                else
                                {
                                    lastVar = newVar;
                                }
                            }
                            else
                            {
                                if(size == 1 && op.SecondArgument is ConstantExpression)
                                {
                                    //
                                    // Setting/clearing a single bit. No need to perform complex masking.
                                    //
                                    ConstantExpression ex = (ConstantExpression)op.SecondArgument;
                                    ulong              val;

                                    if(ex.GetAsUnsignedInteger( out val ) == false)
                                    {
                                        throw TypeConsistencyErrorException.Create( "Expecting a numeric constant, got '{0}'", ex );
                                    }

                                    if((val & (1u << (int)offset)) != 0)
                                    {
                                        ConstantExpression exMask = ts.CreateConstant(  maskShifted );

                                        op.AddOperatorBefore( BinaryOperator.New( debugInfo, BinaryOperator.ALU.OR, false, false, newVar, tmp, exMask ) );
                                    }
                                    else
                                    {
                                        ConstantExpression exMaskNot = ts.CreateConstant( ~maskShifted );

                                        op.AddOperatorBefore( BinaryOperator.New( debugInfo, BinaryOperator.ALU.AND, false, false, newVar, tmp, exMaskNot ) );
                                    }
                                }
                                else
                                {
                                    //
                                    // We need to load the physical register, shift it and mask it, then mask and merge with the new value, and finally write it back.
                                    //
                                    VariableExpression tmp2      = nc.CurrentCFG.AllocateTemporary( tdPhysical, null );
                                    VariableExpression tmp3      = nc.CurrentCFG.AllocateTemporary( tdPhysical, null );
                                    VariableExpression tmp4      = nc.CurrentCFG.AllocateTemporary( tdPhysical, null );
                                    ConstantExpression exMask    = ts.CreateConstant(  maskShifted );
                                    ConstantExpression exMaskNot = ts.CreateConstant( ~maskShifted );

                                    if(position > offset)
                                    {
                                        op.AddOperatorBefore( BinaryOperator.New( debugInfo, BinaryOperator.ALU.SHL, false, false, tmp2, op.SecondArgument, ts.CreateConstant( position - offset ) ) );
                                    }
                                    else
                                    {
                                        op.AddOperatorBefore( BinaryOperator.New( debugInfo, BinaryOperator.ALU.SHR, false, false, tmp2, op.SecondArgument, ts.CreateConstant( offset - position ) ) );
                                    }

                                    op.AddOperatorBefore( BinaryOperator.New( debugInfo, BinaryOperator.ALU.AND, false, false, tmp3  , tmp2, exMask    ) );
                                    op.AddOperatorBefore( BinaryOperator.New( debugInfo, BinaryOperator.ALU.AND, false, false, tmp4  , tmp , exMaskNot ) );
                                    op.AddOperatorBefore( BinaryOperator.New( debugInfo, BinaryOperator.ALU.OR , false, false, newVar, tmp3, tmp4      ) );
                                }

                                lastVar = newVar;
                                tmp     = lastVar;
                            }
                        }

                        if(fLoad)
                        {
                            opNew = SingleAssignmentOperator.New( debugInfo, op.FirstResult, lastVar );
                        }
                        else
                        {
                            opNew = StoreIndirectOperator.New( debugInfo, td, op.FirstArgument, lastVar, null, fd.Offset, false );
                        }
                    }

                    CHECKS.ASSERT( opNew.MayThrow == false, "Failed to lower call operator {0}", op );

                    op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.CopyAnnotations );

                    nc.MarkAsModified();
                    return;
                }
            }

            if(op.MayThrow)
            {
                FieldOperator opNew;

                op.AddOperatorBefore( NullCheckOperator.New( debugInfo, op.FirstArgument ) );

                if(op is LoadInstanceFieldAddressOperator)
                {
                    opNew = LoadInstanceFieldAddressOperator.New( debugInfo, fd, op.FirstResult, op.FirstArgument, false );
                    opNew.AddAnnotation( NotNullAnnotation.Create( ts ) );
                }
                else if(op is LoadInstanceFieldOperator)
                {
                    opNew = LoadInstanceFieldOperator.New( debugInfo, fd, op.FirstResult, op.FirstArgument, false );
                }
                else if(op is StoreInstanceFieldOperator)
                {
                    opNew = StoreInstanceFieldOperator.New( debugInfo, fd, op.FirstArgument, op.SecondArgument, false );
                }
                else
                {
                    throw TypeConsistencyErrorException.Create( "Unexpected operator {0} at phase {1}", op, nc.Phase );
                }

                CHECKS.ASSERT( opNew.MayThrow == false, "Failed to lower call operator {0}", op );

                op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.CopyAnnotations );

                op = opNew;

                nc.MarkAsModified();
            }

            if(op is LoadInstanceFieldOperator)
            {
                if((fd.Flags & FieldRepresentation.Attributes.NeverNull) != 0)
                {
                    if(op.AddAnnotation( NotNullAnnotation.Create( ts ) ))
                    {
                        nc.MarkAsModified();
                    }
                }

                if((fd.Flags & FieldRepresentation.Attributes.HasFixedSize) != 0)
                {
                    if(op.AddAnnotation( FixedLengthArrayAnnotation.Create( ts, fd.FixedSize ) ))
                    {
                        nc.MarkAsModified();
                    }
                }
            }
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof(Phases.HighLevelToMidLevelConversion) )]
        [CompilationSteps.OperatorHandler( typeof(ElementOperator) )]
        private static void Handle_ElementOperator( PhaseExecution.NotificationContext nc )
        {
            TypeSystemForCodeTransformation  ts             = nc.TypeSystem;
            ElementOperator                  op             = (ElementOperator)nc.CurrentOperator;
            Debugging.DebugInfo              debugInfo      = op.DebugInfo;
            Expression                       exArray        = op.FirstArgument;
            TypeRepresentation               tdElement      = exArray.Type.ContainedType;
            MemoryMappedPeripheralAnnotation anMemoryMapped = null;

            if(exArray is VariableExpression)
            {
                foreach(Operator arrayDef in nc.CurrentCFG.DataFlow_DefinitionChains[exArray.SpanningTreeIndex])
                {
                    MemoryMappedPeripheralAnnotation an = arrayDef.GetAnnotation< MemoryMappedPeripheralAnnotation >();

                    if(an != null)
                    {
                        if(tdElement is ScalarTypeRepresentation)
                        {
                            anMemoryMapped = an;
                            break;
                        }

                        if(ts.MemoryMappedBitFieldPeripherals.ContainsKey( tdElement ))
                        {
                            anMemoryMapped = an;
                            break;
                        }

                        //--//

                        CustomAttributeRepresentation ca;

                        if(ts.MemoryMappedPeripherals.TryGetValue( tdElement, out ca ))
                        {
                            uint size = (uint)ca.GetNamedArg( "Length" );

                            if(op is LoadElementAddressOperator)
                            {
                                throw TypeConsistencyErrorException.Create( "Cannot take the address of an array of memory-mapped sections '{0}'", tdElement.FullNameWithAbbreviation );
                            }
                            else if(op is LoadElementOperator)
                            {
                                TemporaryVariableExpression tmpOffset = nc.AllocateTemporary( nc.TypeSystem.WellKnownTypes.System_UInt32 );
                                Operator                    opNew;

                                opNew = BinaryOperator.New( debugInfo, BinaryOperator.ALU.MUL, false, false, tmpOffset, op.SecondArgument, nc.TypeSystem.CreateConstant( size ) );
                                op.AddOperatorBefore( opNew );

                                opNew = BinaryOperator.New( debugInfo, BinaryOperator.ALU.ADD, false, false, op.FirstResult, exArray, tmpOffset );
                                opNew.AddAnnotation( NotNullAnnotation               .Create( ts     ) );
                                opNew.AddAnnotation( MemoryMappedPeripheralAnnotation.Create( ts, ca ) );

                                CHECKS.ASSERT( opNew.MayThrow == false, "Failed to lower call operator {0}", op );

                                op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.CopyAnnotations );

                                nc.MarkAsModified();
                                return;
                            }
                            else if(op is StoreElementOperator)
                            {
                                throw TypeConsistencyErrorException.Create( "Cannot update reference to an array of memory-mapped sections '{0}'", tdElement.FullNameWithAbbreviation );
                            }
                            else
                            {
                                throw TypeConsistencyErrorException.Create( "Unexpected operator {0} at phase {1}", op, nc.Phase );
                            }
                        }
                    }
                }
            }

            if(op.MayThrow)
            {
                ElementOperator opNew;

                if(anMemoryMapped == null)
                {
                    op.AddOperatorBefore( NullCheckOperator      .New( debugInfo, op.FirstArgument                    ) );
                    op.AddOperatorBefore( OutOfBoundCheckOperator.New( debugInfo, op.FirstArgument, op.SecondArgument ) );
                }

                if(op is LoadElementAddressOperator)
                {
                    opNew = LoadElementAddressOperator.New( debugInfo, op.FirstResult, op.FirstArgument, op.SecondArgument, op.AccessPath, false );
                    opNew.AddAnnotation( NotNullAnnotation.Create( ts ) );
                }
                else if(op is LoadElementOperator)
                {
                    opNew = LoadElementOperator.New( debugInfo, op.FirstResult, op.FirstArgument, op.SecondArgument, op.AccessPath, false );
                }
                else if(op is StoreElementOperator)
                {
                    opNew = StoreElementOperator.New( debugInfo, op.FirstArgument, op.SecondArgument, op.ThirdArgument, op.AccessPath, false );
                }
                else
                {
                    throw TypeConsistencyErrorException.Create( "Unexpected operator {0} at phase {1}", op, nc.Phase );
                }

                if(anMemoryMapped != null)
                {
                    opNew.AddAnnotation( anMemoryMapped );
                }

                CHECKS.ASSERT( opNew.MayThrow == false, "Failed to lower call operator {0}", op );

                op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.CopyAnnotations );

                nc.MarkAsModified();
            }
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof(Phases.HighLevelToMidLevelConversion) )]
        [CompilationSteps.OperatorHandler( typeof(IndirectOperator) )]
        private static void Handle_IndirectOperator( PhaseExecution.NotificationContext nc )
        {
            IndirectOperator op = (IndirectOperator)nc.CurrentOperator;

            if(op.MayThrow)
            {
                Debugging.DebugInfo debugInfo   = op.DebugInfo;
                bool                fIsVolatile = op.MayMutateExistingStorage;
                Operator            opNew;

                op.AddOperatorBefore( NullCheckOperator.New( debugInfo, op.FirstArgument ) );

                if(op is LoadIndirectOperator)
                {
                    opNew = LoadIndirectOperator.New( debugInfo, op.Type, op.FirstResult, op.FirstArgument, op.AccessPath, op.Offset, fIsVolatile, false );
                }
                else if(op is StoreIndirectOperator)
                {
                    opNew = StoreIndirectOperator.New( debugInfo, op.Type, op.FirstArgument, op.SecondArgument, op.AccessPath, op.Offset, false );
                }
                else
                {
                    throw TypeConsistencyErrorException.Create( "Unexpected operator {0} at phase {1}", op, nc.Phase );
                }

                CHECKS.ASSERT( opNew.MayThrow == false, "Failed to lower call operator {0}", op );

                op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.CopyAnnotations );

                nc.MarkAsModified();
            }
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof(Phases.HighLevelToMidLevelConversion) )]
        [CompilationSteps.OperatorHandler( typeof(CallOperator) )]
        private static void Handle_CallOperator( PhaseExecution.NotificationContext nc )
        {
            CallOperator op = (CallOperator)nc.CurrentOperator;

            if(nc.TypeSystem.GetLevel( op )> Operator.OperatorLevel.ConcreteTypes_NoExceptions)
            {
                Debugging.DebugInfo debugInfo = op.DebugInfo;
                CallOperator        opNew;

                if(op is InstanceCallOperator)
                {
                    op.AddOperatorBefore( NullCheckOperator.New( debugInfo, op.FirstArgument ) );

                    opNew = InstanceCallOperator.New( debugInfo, op.CallType, op.TargetMethod, op.Results, op.Arguments, false );
                }
                else if(op is IndirectCallOperator)
                {
                    op.AddOperatorBefore( NullCheckOperator.New( debugInfo, op.FirstArgument ) );

                    if(op.TargetMethod is InstanceMethodRepresentation)
                    {
                        op.AddOperatorBefore( NullCheckOperator.New( debugInfo, op.SecondArgument ) );
                    }

                    opNew = IndirectCallOperator.New( debugInfo, op.TargetMethod, op.Results, op.Arguments, false );
                }
                else
                {
                    throw TypeConsistencyErrorException.Create( "Unexpected operator {0} at phase {1}", op, nc.Phase );
                }

                CHECKS.ASSERT( nc.TypeSystem.GetLevel( opNew )<= Operator.OperatorLevel.ConcreteTypes_NoExceptions, "Failed to lower call operator {0}", op );

                op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.CopyAnnotations );

                nc.MarkAsModified();
            }
        }
    }
}
