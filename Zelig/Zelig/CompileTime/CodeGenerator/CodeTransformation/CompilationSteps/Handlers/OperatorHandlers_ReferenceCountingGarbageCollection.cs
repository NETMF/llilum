//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps.Handlers
{
    using Microsoft.Zelig.CodeGeneration.IR.CompilationSteps.Phases;
    using Runtime.TypeSystem;
    using System;

    public class OperatorHandlers_ReferenceCountingGarbageCollection
    {
        [CompilationSteps.PhaseFilter( typeof( Phases.ReferenceCountingGarbageCollection ) )]
        [CompilationSteps.OperatorHandler( typeof( SingleAssignmentOperator ) )]
        [CompilationSteps.OperatorHandler( typeof( CallOperator ) )]
        [CompilationSteps.OperatorHandler( typeof( StoreInstanceFieldOperator ) )]
        [CompilationSteps.OperatorHandler( typeof( LoadInstanceFieldOperator ) )]
        [CompilationSteps.OperatorHandler( typeof( StoreElementOperator ) )]
        [CompilationSteps.OperatorHandler( typeof( LoadElementOperator ) )]
        [CompilationSteps.OperatorHandler( typeof( StoreIndirectOperator ) )]
        [CompilationSteps.OperatorHandler( typeof( LoadIndirectOperator ) )]
        private static void Handle_AssignmentOperators( PhaseExecution.NotificationContext nc )
        {
            var op = nc.CurrentOperator;
            var ts = nc.TypeSystem;
            var phase = (ReferenceCountingGarbageCollection)nc.Phase;

            if(!ts.ShouldExcludeMethodFromReferenceCounting( op.BasicBlock.Owner.Method ) &&
                !phase.IsOperatorModified( op ))
            {
                if(op is SingleAssignmentOperator)
                {
                    Handle_SingleAssignmentOperator( nc );
                }
                else if(op is CallOperator)
                {
                    Handle_CallOperator( nc );
                }
                else if(op is StoreInstanceFieldOperator)
                {
                    Handle_StoreInstanceFieldOperator( nc );
                }
                else if(op is LoadInstanceFieldOperator)
                {
                    Handle_LoadInstanceFieldOperator( nc );
                }
                else if(op is StoreElementOperator)
                {
                    Handle_StoreElementOperator( nc );
                }
                else if(op is LoadElementOperator)
                {
                    Handle_LoadElementOperator( nc );
                }
                else if(op is StoreIndirectOperator)
                {
                    Handle_StoreIndirectOperator( nc );
                }
                else if(op is LoadIndirectOperator)
                {
                    Handle_LoadIndirectOperator( nc );
                }

                phase.AddToModifiedOperator( op );
            }
        }

        private enum RefCountType
        {
            None,
            RefCounted,
            Pointer
        }

        private static RefCountType GetRefCountType( PhaseExecution.NotificationContext nc,
                                                     Expression                         exp )
        {
            var rctype = RefCountType.None;
            var variable = exp as VariableExpression;
            if(variable != null && !variable.SkipReferenceCounting)
            {
                rctype = GetRefCountType( nc, variable.Type );
            }
            return rctype;
        }

        private static RefCountType GetRefCountType( PhaseExecution.NotificationContext nc,
                                                     TypeRepresentation                 type )
        {
            var rctype = RefCountType.None;

            var ts = nc.TypeSystem;
            if(ts.IsReferenceCountingType( type ))
            {
                rctype = RefCountType.RefCounted;
            }
            else if(type == ts.WellKnownTypes.System_UIntPtr || type == ts.WellKnownTypes.System_IntPtr)
            {
                rctype = RefCountType.Pointer;
            }

            return rctype;
        }

        private static void InsertAddRefBefore( PhaseExecution.NotificationContext nc, Expression argument )
        {
            InsertMethodHelper( nc,
                                nc.TypeSystem.WellKnownMethods.ObjectHeader_AddReference,
                                argument,
                                insertBefore: true );
        }
        private static void InsertAddRefAfter( PhaseExecution.NotificationContext nc, Expression argument )
        {
            InsertMethodHelper( nc,
                                nc.TypeSystem.WellKnownMethods.ObjectHeader_AddReference,
                                argument,
                                insertBefore: false );
        }
        private static void InsertReleaseBefore( PhaseExecution.NotificationContext nc, Expression argument )
        {
            InsertMethodHelper( nc,
                                nc.TypeSystem.WellKnownMethods.ObjectHeader_ReleaseReference,
                                argument,
                                insertBefore: true );
        }

        private static void InsertMethodHelper( PhaseExecution.NotificationContext      nc,
                                                Runtime.TypeSystem.MethodRepresentation md,
                                                Expression                              argument,
                                                bool                                    insertBefore )
        {
            var op = nc.CurrentOperator;
            var ts = nc.TypeSystem;
            var rhs = ts.AddTypePointerToArgumentsOfStaticMethod( md, argument );
            var call = StaticCallOperator.New( op.DebugInfo, CallOperator.CallKind.Direct, md, rhs );

            if(insertBefore)
            {
                op.AddOperatorBefore( call );
            }
            else
            {
                op.AddOperatorAfter( call );
            }

            var phase = (Phases.ReferenceCountingGarbageCollection)nc.Phase;
            phase.AddToModifiedOperator( call );
            phase.IncrementInjectionCount( md );

            nc.MarkAsModified( );
        }

        private static void Handle_SingleAssignmentOperator( PhaseExecution.NotificationContext nc )
        {
            // Syntax: FirstResult = FirstArgument
            var op = (SingleAssignmentOperator)nc.CurrentOperator;
            var lhsRefCountType = GetRefCountType( nc, op.FirstResult );
            var rhsRefCountType = GetRefCountType( nc, op.FirstArgument );

            if(lhsRefCountType == RefCountType.RefCounted)
            {
                // Need to Addref the RHS if both LHS and RHS are ref-counted. This means that we won't addref when
                // we assign ref-counted object to non-refcounted objects or IntPtr / UIntptr (effectively treating
                // them as weak reference)
                if(rhsRefCountType == RefCountType.RefCounted)
                {
                    InsertAddRefBefore( nc, op.FirstArgument );
                }

                // If LHS has a ref-counted type, we should always call Release (it'll just be no-op if the
                // actual object is not)
                InsertReleaseBefore( nc, op.FirstResult );

                // We are assigning a weak pointer (IntPtr / UIntPtr) to a ref-counted type. We need to
                // assume a reference since it's now assigned to a proper object.
                if(rhsRefCountType == RefCountType.Pointer)
                {
                    InsertAddRefAfter( nc, op.FirstResult );
                }
            }
        }

        private static void Handle_CallOperator( PhaseExecution.NotificationContext nc )
        {
            // Syntax: FirstResult = TargetMethod()
            var op = (CallOperator)nc.CurrentOperator;
            var lhsRefCountType = 
                ( op.Results.Length > 0 ) ? GetRefCountType( nc, op.FirstResult.Type ) : RefCountType.None;

            if(lhsRefCountType == RefCountType.RefCounted)
            {
                InsertReleaseBefore( nc, op.FirstResult );
            }
        }

        private static void Handle_StoreInstanceFieldOperator( PhaseExecution.NotificationContext nc )
        {
            // Syntax: FirstArgument.Field = SecondArgument
            Handle_StoreOperators(
                nc         : nc,
                lhsType    : ( (StoreFieldOperator)nc.CurrentOperator ).Field.FieldType,
                loadLhsAddr: ( op, tempVar ) => LoadInstanceFieldAddressOperator.New( 
                                                    op.DebugInfo,
                                                    ( (FieldOperator)op ).Field,
                                                    tempVar,
                                                    op.FirstArgument,
                                                    fNullCheck: false ),
                rhs        : nc.CurrentOperator.SecondArgument );
        }

        private static void Handle_LoadInstanceFieldOperator( PhaseExecution.NotificationContext nc )
        {
            // Syntax: FirstResult = FirstArgument.Field
            Handle_LoadOperators(
                nc         : nc,
                lhs        : nc.CurrentOperator.FirstResult,
                rhsType    : ( (LoadFieldOperator)nc.CurrentOperator ).Field.FieldType,
                loadRhsAddr: ( op, tempVar ) => LoadInstanceFieldAddressOperator.New(
                                                    op.DebugInfo,
                                                    ( (FieldOperator)op ).Field,
                                                    tempVar,
                                                    op.FirstArgument,
                                                    fNullCheck: false ) );
        }

        private static void Handle_StoreElementOperator( PhaseExecution.NotificationContext nc )
        {
            // Syntax: FirstArgument[SecondArgument] = ThirdArgument
            Handle_StoreOperators(
                nc         : nc,
                lhsType    : nc.CurrentOperator.FirstArgument.Type.ContainedType,
                loadLhsAddr: ( op, tempVar ) => LoadElementAddressOperator.New(
                                                    op.DebugInfo,
                                                    tempVar,
                                                    op.FirstArgument,
                                                    op.SecondArgument,
                                                    ( (ElementOperator)op ).AccessPath,
                                                    fNullCheck: false ),
                rhs        : nc.CurrentOperator.ThirdArgument );
        }

        private static void Handle_LoadElementOperator( PhaseExecution.NotificationContext nc )
        {
            // Syntax: FirstResult = FirstArgument[SecondArgument]
            Handle_LoadOperators(
                nc         : nc,
                lhs        : nc.CurrentOperator.FirstResult,
                rhsType    : nc.CurrentOperator.FirstArgument.Type.ContainedType,
                loadRhsAddr: ( op, tempVar ) => LoadElementAddressOperator.New(
                                                    op.DebugInfo,
                                                    tempVar,
                                                    op.FirstArgument,
                                                    op.SecondArgument,
                                                    ( (ElementOperator)op ).AccessPath,
                                                    fNullCheck: false ) );
        }

        private static void Handle_StoreIndirectOperator( PhaseExecution.NotificationContext nc )
        {
            // Syntax: FirstArgument[Offset] = SecondArgument
            Handle_StoreOperators(
                nc         : nc,
                lhsType    : nc.CurrentOperator.FirstArgument.Type.ContainedType,
                loadLhsAddr: delegate( Operator op, TemporaryVariableExpression tempVar )
                             {
                                 CHECKS.ASSERT( ( (IndirectOperator)op ).Offset == 0, "No support for non-zero offset" );
                                 return SingleAssignmentOperator.New( op.DebugInfo, tempVar, op.FirstArgument );
                             },
                rhs        : nc.CurrentOperator.SecondArgument );
        }

        private static void Handle_LoadIndirectOperator( PhaseExecution.NotificationContext nc )
        {
            // Syntax: FirstResult = FirstArgument[Offset]
            Handle_LoadOperators(
                nc         : nc,
                lhs        : nc.CurrentOperator.FirstResult,
                rhsType    : nc.CurrentOperator.FirstArgument.Type.ContainedType,
                loadRhsAddr: delegate ( Operator op, TemporaryVariableExpression tempVar )
                             {
                                 CHECKS.ASSERT( ( (IndirectOperator)op ).Offset == 0, "No support for non-zero offset" );
                                 return SingleAssignmentOperator.New( op.DebugInfo, tempVar, op.FirstArgument );
                             } );
        }

        private static void Handle_StoreOperators(
            PhaseExecution.NotificationContext                    nc,
            TypeRepresentation                                    lhsType,
            Func<Operator, TemporaryVariableExpression, Operator> loadLhsAddr,
            Expression                                            rhs )
        {
            var op = nc.CurrentOperator;
            var lhsRefCountType = GetRefCountType( nc, lhsType );
            if(lhsRefCountType == RefCountType.RefCounted)
            {
                var tempAddr = nc.CurrentCFG.AllocateTemporary(
                    nc.TypeSystem.GetManagedPointerToType( nc.TypeSystem.WellKnownTypes.System_Object ),
                    null );
                Operator loadAddrOp = loadLhsAddr( op, tempAddr );
                op.AddOperatorBefore( loadAddrOp );

                var swap = nc.TypeSystem.WellKnownMethods.ReferenceCountingCollector_Swap;
                var swapRHS = nc.TypeSystem.AddTypePointerToArgumentsOfStaticMethod(
                    swap,
                    tempAddr,
                    rhs );
                var swapCall = StaticCallOperator.New(
                    op.DebugInfo,
                    CallOperator.CallKind.Direct,
                    swap,
                    VariableExpression.SharedEmptyArray,
                    swapRHS );

                op.SubstituteWithOperator( swapCall, Operator.SubstitutionFlags.Default );

                var phase = (ReferenceCountingGarbageCollection)nc.Phase;
                phase.AddToModifiedOperator( swapCall );
                phase.IncrementInjectionCount( swap );
            }
        }

        private static void Handle_LoadOperators(
            PhaseExecution.NotificationContext nc,
            VariableExpression lhs,
            TypeRepresentation rhsType,
            Func<Operator, TemporaryVariableExpression, Operator> loadRhsAddr )
        {
            var op = nc.CurrentOperator;
            var lhsRefCountType = GetRefCountType( nc, lhs );
            var rhsRefCountType = GetRefCountType( nc, rhsType );

            // Note that we are calling Release() first then AddRef() here. This is OK because the field
            // will always hold a valid reference, so the first Release() can never cause the ref count to
            // bounce zero.

            // If LHS has a ref-counted type, we should always call Release (it'll just be no-op if the
            // actual object is not)
            if(lhsRefCountType == RefCountType.RefCounted)
            {
                InsertReleaseBefore( nc, lhs );

                // Need to Addref the RHS if it's a ref-counted type.
                if(rhsRefCountType == RefCountType.RefCounted)
                {
                    // Create a temporary variable to store the address of the field object
                    var tempAddr = nc.CurrentCFG.AllocateTemporary( 
                        nc.TypeSystem.GetManagedPointerToType( nc.TypeSystem.WellKnownTypes.System_Object ),
                        null );
                    var loadAddrOp = loadRhsAddr(op, tempAddr);
                    op.AddOperatorBefore( loadAddrOp );

                    var loadAndAddRef = nc.TypeSystem.WellKnownMethods.ReferenceCountingCollector_LoadAndAddReference;
                    var loadAndAddRefRHS = nc.TypeSystem.AddTypePointerToArgumentsOfStaticMethod( loadAndAddRef, tempAddr );
                    var loadAndAddRefCall = StaticCallOperator.New( op.DebugInfo,
                                                                    CallOperator.CallKind.Direct,
                                                                    loadAndAddRef,
                                                                    new VariableExpression[] { lhs },
                                                                    loadAndAddRefRHS );

                    op.SubstituteWithOperator( loadAndAddRefCall, Operator.SubstitutionFlags.Default );

                    var phase = (ReferenceCountingGarbageCollection)nc.Phase;
                    phase.AddToModifiedOperator( loadAndAddRefCall );
                    phase.IncrementInjectionCount( loadAndAddRef );
                }
            }
        }

        [CompilationSteps.PhaseFilter( typeof( Phases.ReferenceCountingGarbageCollection ) )]
        [CompilationSteps.OperatorHandler( typeof( StaticCallOperator ) )]
        private static void Handle_RedundantRefCountingCalls( PhaseExecution.NotificationContext nc )
        {
            var op = (StaticCallOperator)nc.CurrentOperator;
            var ts = nc.TypeSystem;
            var wkm = ts.WellKnownMethods;
            var md = op.TargetMethod;

            // Remove redundant AddReference(null) and ReleaseReference(null)
            if(md == wkm.ObjectHeader_AddReference || md == wkm.ObjectHeader_ReleaseReference)
            {
                var arg = op.SecondArgument as ConstantExpression;
                if(arg != null && arg.Value == null)
                {
                    op.Delete( );
                    ( (ReferenceCountingGarbageCollection)nc.Phase ).DecrementInjectionCount( md );
                    nc.MarkAsModified( );
                }
            }
        }

        [CompilationSteps.PhaseFilter( typeof( Phases.ReferenceCountingGarbageCollection ) )]
        [CompilationSteps.OperatorHandler( typeof( StaticCallOperator ) )]
        private static void Handle_ReplaceFastAllocateString( PhaseExecution.NotificationContext nc )
        {
            var op = (StaticCallOperator)nc.CurrentOperator;
            var ts = nc.TypeSystem;
            var wkm = ts.WellKnownMethods;
            var allocString = wkm.StringImpl_FastAllocateString;

            if(allocString != null && op.TargetMethod == allocString)
            {
                var allocStrCall = StaticCallOperator.New( op.DebugInfo,
                                                           CallOperator.CallKind.Direct,
                                                           wkm.StringImpl_FastAllocateReferenceCountingString,
                                                           op.Results,
                                                           op.Arguments );
                op.SubstituteWithOperator( allocStrCall, Operator.SubstitutionFlags.Default );

                var phase = (ReferenceCountingGarbageCollection)nc.Phase;
                if(phase.IsOperatorModified( op ))
                {
                    phase.AddToModifiedOperator( allocStrCall );
                }

                nc.MarkAsModified( );
            }
        }

        [CompilationSteps.PhaseFilter( typeof( Phases.ReferenceCountingGarbageCollection ) )]
        [CompilationSteps.OperatorHandler( typeof( StaticCallOperator ) )]
        private static void Handle_ReplaceInternalInterlockedMethods( PhaseExecution.NotificationContext nc )
        {
            // Various Interlocked methods like Exchange and CompareExchange are being replaced by
            // LLVM atomic instruction, and as such, they do not properly account for reference counting
            // This handler will wrap the calls with the proper reference counting methods (as implemented
            // in InterlockedImpl.ReferenceCounting* methods)

            var op = (StaticCallOperator)nc.CurrentOperator;
            var ts = nc.TypeSystem;
            var wkm = ts.WellKnownMethods;

            var internalExchange        = wkm.InterlockedImpl_InternalExchange_Template;
            var internalCompareExchange = wkm.InterlockedImpl_InternalCompareExchange_Template;
            var refCountExchange        = wkm.ReferenceCountingCollector_ReferenceCountingExchange;
            var refCountCompareExchange = wkm.ReferenceCountingCollector_ReferenceCountingCompareExchange;

            var md = op.TargetMethod;
            var owner = op.BasicBlock.Owner.Method;

            // Do not replace the call in ReferenceCountingExchange or ReferenceCountingCompareExchange
            if(owner == refCountExchange || owner == refCountCompareExchange)
            {
                return;
            }

            if(md.IsGenericInstantiation && 
                (md.GenericTemplate == internalExchange || md.GenericTemplate == internalCompareExchange))
            {
                var isCompareExchange = md.GenericTemplate == internalCompareExchange;

                // Generate the unsafe cast from T& to object&
                var tempPtr = nc.CurrentCFG.AllocateTemporary(
                    nc.TypeSystem.GetManagedPointerToType( nc.TypeSystem.WellKnownTypes.System_Object ),
                    null );
                var unsafeCastOp = SingleAssignmentOperator.New( op.DebugInfo, tempPtr, op.SecondArgument );

                op.AddOperatorBefore( unsafeCastOp );

                // Replace the T& with the new temporary
                Expression[] args = (Expression[])op.Arguments.Clone( );
                args[ 1 ] = tempPtr;

                var call = StaticCallOperator.New(
                    op.DebugInfo,
                    CallOperator.CallKind.Direct,
                    isCompareExchange ? refCountCompareExchange : refCountExchange,
                    op.Results,
                    args );

                op.SubstituteWithOperator( call, Operator.SubstitutionFlags.Default );

                var phase = (ReferenceCountingGarbageCollection)nc.Phase;
                if(phase.IsOperatorModified( op ))
                {
                    phase.AddToModifiedOperator( call );
                }

                nc.MarkAsModified( );
            }
        }
    }
}
