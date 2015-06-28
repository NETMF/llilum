//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps.Handlers
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public class OperatorHandlers_HighLevel
    {
        [CompilationSteps.PhaseFilter( typeof(Phases.HighLevelTransformations) )]
        [CompilationSteps.OperatorHandler( typeof(ObjectAllocationOperator) )]
        private static void Handle_ObjectAllocationOperator( PhaseExecution.NotificationContext nc )
        {
            ObjectAllocationOperator        op               = (ObjectAllocationOperator)nc.CurrentOperator;
            TypeSystemForCodeTransformation ts               = nc.TypeSystem;
            WellKnownMethods                wkm              = ts.WellKnownMethods;
            MethodRepresentation            mdAllocateObject = wkm.TypeSystemManager_AllocateObject;
            MethodRepresentation            md               = op.Method;
            TypeRepresentation              td               = op.Type;
            CallOperator                    call;
            Expression[]                    rhs;
            Expression                      exStringLen      = null;

            //
            // Is this a special object?
            //
            for(var td2 = td; td2 != null; td2 = td2.Extends)
            {
                if(ts.GarbageCollectionExtensions.ContainsKey( td2 ))
                {
                    mdAllocateObject = wkm.TypeSystemManager_AllocateObjectWithExtensions;
                    break;
                }
            }

            if(md != null)
            {
                if(md == wkm.StringImpl_ctor_char_int)
                {
                    exStringLen = op.SecondArgument;
                }
                else if(md == wkm.StringImpl_ctor_charArray)
                {
                    TemporaryVariableExpression tmp = nc.CurrentCFG.AllocateTemporary( ts.WellKnownTypes.System_Int32, null );

                    op.AddOperatorBefore( ArrayLengthOperator.New( op.DebugInfo, tmp, op.FirstArgument ) );

                    exStringLen = tmp;
                }
                else if(md == wkm.StringImpl_ctor_charArray_int_int)
                {
                    exStringLen = op.ThirdArgument;
                }
            }

            //
            // Special case to allocate strings built out of characters.
            //
            if(exStringLen != null)
            {
                mdAllocateObject = wkm.StringImpl_FastAllocateString;

                rhs  = ts.AddTypePointerToArgumentsOfStaticMethod( mdAllocateObject, exStringLen );
                call = StaticCallOperator.New( op.DebugInfo, CallOperator.CallKind.Direct, mdAllocateObject, op.Results, rhs );
                op.SubstituteWithOperator( call, Operator.SubstitutionFlags.CopyAnnotations );
                call.AddAnnotation( NotNullAnnotation.Create( ts ) );

                nc.MarkAsModified();
            }
            else
            {
                TemporaryVariableExpression typeSystemManagerEx = FetchTypeSystemManager( nc, op );

                rhs  = new Expression[] { typeSystemManagerEx, ts.GetVTable( op.Type ) };
                call = InstanceCallOperator.New( op.DebugInfo, CallOperator.CallKind.Virtual, mdAllocateObject, op.Results, rhs, true );
                op.SubstituteWithOperator( call, Operator.SubstitutionFlags.CopyAnnotations );
                call.AddAnnotation( NotNullAnnotation.Create( ts ) );

                var mdFinalizer = td.FindDestructor();
                if(mdFinalizer != null && mdFinalizer.OwnerType != ts.WellKnownTypes.System_Object)
                {
                    var mdHelper   = wkm.Finalizer_Allocate;
                    var rhsHelper  = ts.AddTypePointerToArgumentsOfStaticMethod( mdHelper, call.FirstResult );
                    var callHelper = StaticCallOperator.New( call.DebugInfo, CallOperator.CallKind.Direct, mdHelper, rhsHelper );

                    call.AddOperatorAfter( callHelper );
                }

                nc.MarkAsModified();
            }
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof(Phases.HighLevelTransformations) )]
        [CompilationSteps.OperatorHandler( typeof(ArrayAllocationOperator) )]
        private static void Handle_ArrayAllocationOperator( PhaseExecution.NotificationContext nc )
        {
            ArrayAllocationOperator         op                  = (ArrayAllocationOperator)nc.CurrentOperator;
            TypeSystemForCodeTransformation ts                  = nc.TypeSystem;
            TemporaryVariableExpression     typeSystemManagerEx = FetchTypeSystemManager( nc, op );
            MethodRepresentation            mdAllocateArray     = ts.WellKnownMethods.TypeSystemManager_AllocateArray;
            CallOperator                    call;
            Expression[]                    rhs;
            Expression                      exLen = op.FirstArgument;

            rhs  = new Expression[] { typeSystemManagerEx, ts.GetVTable( op.Type ), exLen };
            call = InstanceCallOperator.New( op.DebugInfo, CallOperator.CallKind.Virtual, mdAllocateArray, op.Results, rhs, true );
            op.SubstituteWithOperator( call, Operator.SubstitutionFlags.CopyAnnotations );
            call.AddAnnotation( NotNullAnnotation.Create( ts ) );

            ConstantExpression exLenConst = exLen as ConstantExpression;
            if(exLenConst != null && exLenConst.IsValueInteger)
            {
                ulong val; exLenConst.GetAsRawUlong( out val );

                call.AddAnnotation( FixedLengthArrayAnnotation.Create( ts, (int)val ) );
            }

            nc.MarkAsModified();
        }

        [CompilationSteps.PhaseFilter( typeof(Phases.HighLevelTransformations) )]
        [CompilationSteps.OperatorHandler( typeof(ArrayLengthOperator) )]
        private static void Handle_ArrayLengthOperator( PhaseExecution.NotificationContext nc )
        {
            ArrayLengthOperator             op         = (ArrayLengthOperator)nc.CurrentOperator;
            TypeSystemForCodeTransformation typeSystem = nc.TypeSystem;

            InstanceCallOperator opNew = InstanceCallOperator.New( op.DebugInfo, CallOperator.CallKind.Direct, typeSystem.WellKnownMethods.ArrayImpl_get_Length, op.Results, new Expression[] { op.FirstArgument }, true );
            op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.Default );

            nc.MarkAsModified();
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof(Phases.HighLevelTransformations) )]
        [CompilationSteps.OperatorHandler( typeof(ThrowControlOperator) )]
        private static void Handle_ThrowControlOperator( PhaseExecution.NotificationContext nc )
        {
            ThrowControlOperator            op                  = (ThrowControlOperator)nc.CurrentOperator;
            TypeSystemForCodeTransformation typeSystem          = nc.TypeSystem;
            TemporaryVariableExpression     typeSystemManagerEx = FetchTypeSystemManager( nc, op );
            MethodRepresentation            mdThrow             = typeSystem.WellKnownMethods.TypeSystemManager_Throw;
            Debugging.DebugInfo             debugInfo           = op.DebugInfo;
            CallOperator                    call;
            Expression[]                    rhs;

            rhs  = new Expression[] { typeSystemManagerEx, op.FirstArgument };
            call = InstanceCallOperator.New( debugInfo, CallOperator.CallKind.Virtual, mdThrow, rhs, true );
            call.CopyAnnotations( op );
            op.AddOperatorBefore( call );

            op.SubstituteWithOperator( DeadControlOperator.New( debugInfo ), Operator.SubstitutionFlags.Default );

            nc.MarkAsModified();
        }

        [CompilationSteps.PhaseFilter( typeof(Phases.HighLevelTransformations) )]
        [CompilationSteps.OperatorHandler( typeof(RethrowControlOperator) )]
        private static void Handle_RethrowControlOperator( PhaseExecution.NotificationContext nc )
        {
            RethrowControlOperator          op                  = (RethrowControlOperator)nc.CurrentOperator;
            TypeSystemForCodeTransformation typeSystem          = nc.TypeSystem;
            TemporaryVariableExpression     typeSystemManagerEx = FetchTypeSystemManager( nc, op );
            MethodRepresentation            mdRethrow           = typeSystem.WellKnownMethods.TypeSystemManager_Rethrow;
            Debugging.DebugInfo             debugInfo           = op.DebugInfo;
            CallOperator                    call;
            Expression[]                    rhs;

            rhs  = new Expression[] { typeSystemManagerEx };
            call = InstanceCallOperator.New( debugInfo, CallOperator.CallKind.Virtual, mdRethrow, rhs, true );
            call.CopyAnnotations( op );
            op.AddOperatorBefore( call );

            op.SubstituteWithOperator( DeadControlOperator.New( debugInfo ), Operator.SubstitutionFlags.Default );

            nc.MarkAsModified();
        }

        [CompilationSteps.PhaseFilter( typeof(Phases.HighLevelTransformations) )]
        [CompilationSteps.OperatorHandler( typeof(FetchExceptionOperator) )]
        private static void Handle_FetchExceptionOperator( PhaseExecution.NotificationContext nc )
        {
            FetchExceptionOperator          op         = (FetchExceptionOperator)nc.CurrentOperator;
            TypeSystemForCodeTransformation typeSystem = nc.TypeSystem;
            MethodRepresentation            md         = typeSystem.WellKnownMethods.ThreadImpl_GetCurrentException;
            CallOperator                    call;
            Expression[]                    rhs;

            rhs  = typeSystem.AddTypePointerToArgumentsOfStaticMethod( md );
            call = StaticCallOperator.New( op.DebugInfo, CallOperator.CallKind.Direct, md, op.Results, rhs );
            op.SubstituteWithOperator( call, Operator.SubstitutionFlags.Default );

            nc.MarkAsModified();
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof(Phases.HighLevelTransformations) )]
        [CompilationSteps.OperatorHandler( typeof(CastOperator) )]
        private static void Handle_CastOperator( PhaseExecution.NotificationContext nc )
        {
            WellKnownMethods wkm = nc.TypeSystem.WellKnownMethods;
            CastOperator     op  = (CastOperator)nc.CurrentOperator;

            Handle_GenericCastOperator( nc, op.Type, wkm.TypeSystemManager_CastToSealedType, wkm.TypeSystemManager_CastToType, wkm.TypeSystemManager_CastToInterface );
        }

        [CompilationSteps.PhaseFilter( typeof(Phases.HighLevelTransformations) )]
        [CompilationSteps.OperatorHandler( typeof(IsInstanceOperator) )]
        private static void Handle_IsInstanceOperator( PhaseExecution.NotificationContext nc )
        {
            WellKnownMethods   wkm = nc.TypeSystem.WellKnownMethods;
            IsInstanceOperator op  = (IsInstanceOperator)nc.CurrentOperator;

            Handle_GenericCastOperator( nc, op.Type, wkm.TypeSystemManager_CastToSealedTypeNoThrow, wkm.TypeSystemManager_CastToTypeNoThrow, wkm.TypeSystemManager_CastToInterfaceNoThrow );
        }

        private static void Handle_GenericCastOperator( PhaseExecution.NotificationContext nc           ,
                                                        TypeRepresentation                 target       ,
                                                        MethodRepresentation               mdSealedType ,
                                                        MethodRepresentation               mdType       ,
                                                        MethodRepresentation               mdInterface  )
        {
            TypeSystemForCodeTransformation typeSystem = nc.TypeSystem;
            Operator                        op         = nc.CurrentOperator;
            Expression                      src        = op.FirstArgument;

            if(target.CanBeAssignedFrom( src.Type, null ))
            {
                var opNew = SingleAssignmentOperator.New( op.DebugInfo, op.FirstResult, src );
                op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.Default );
            }
            else
            {
                MethodRepresentation md;
                Expression[]         rhs;
                CallOperator         call;

                if(target is InterfaceTypeRepresentation)
                {
                    md = mdInterface;
                }
                else if(target is ArrayReferenceTypeRepresentation)
                {
                    md = mdType;
                }
                else
                {
                    TypeRepresentation targetSealed;

                    if(typeSystem.IsSealedType( target, out targetSealed ))
                    {
                        target = targetSealed;

                        md = mdSealedType;
                    }
                    else
                    {
                        md = mdType;
                    }
                }

                rhs  = typeSystem.AddTypePointerToArgumentsOfStaticMethod( md, src, typeSystem.GetVTable( target ) );
                call = StaticCallOperator.New( op.DebugInfo, CallOperator.CallKind.Direct, md, op.Results, rhs );
                op.SubstituteWithOperator( call, Operator.SubstitutionFlags.Default );
            }

            nc.MarkAsModified();
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof(Phases.HighLevelTransformations) )]
        [CompilationSteps.OperatorHandler( typeof(LoadStaticFieldAddressOperator) )]
        private static void Handle_LoadStaticFieldAddressOperator( PhaseExecution.NotificationContext nc )
        {
            LoadStaticFieldAddressOperator  op     = (LoadStaticFieldAddressOperator)nc.CurrentOperator;
            TypeSystemForCodeTransformation ts     = nc.TypeSystem;
            Expression                      exRoot = ts.GenerateRootAccessor( op );
            InstanceFieldRepresentation     fd     = ts.AddStaticFieldToGlobalRoot( (StaticFieldRepresentation)op.Field );

            //
            // 'exRoot' will never be null, so we don't need to add null checks.
            //
            LoadInstanceFieldAddressOperator opNew = LoadInstanceFieldAddressOperator.New( op.DebugInfo, fd, op.FirstResult, exRoot, false );
            opNew.AddAnnotation( NotNullAnnotation.Create( ts ) );
            op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.Default );

            nc.MarkAsModified();
        }

        [CompilationSteps.PhaseFilter( typeof(Phases.HighLevelTransformations) )]
        [CompilationSteps.OperatorHandler( typeof(LoadStaticFieldOperator) )]
        private static void Handle_LoadStaticFieldOperator( PhaseExecution.NotificationContext nc )
        {
            LoadStaticFieldOperator         op         = (LoadStaticFieldOperator)nc.CurrentOperator;
            TypeSystemForCodeTransformation typeSystem = nc.TypeSystem;
            Expression                      exRoot     = typeSystem.GenerateRootAccessor( op );
            InstanceFieldRepresentation     fd         = typeSystem.AddStaticFieldToGlobalRoot( (StaticFieldRepresentation)op.Field );

            //
            // 'exRoot' will never be null, so we don't need to check for it.
            //
            LoadInstanceFieldOperator opNew = LoadInstanceFieldOperator.New( op.DebugInfo, fd, op.FirstResult, exRoot, false );
            op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.Default );

            nc.MarkAsModified();
        }

        [CompilationSteps.PhaseFilter( typeof(Phases.HighLevelTransformations) )]
        [CompilationSteps.OperatorHandler( typeof(StoreStaticFieldOperator) )]
        private static void Handle_StoreStaticFieldOperator( PhaseExecution.NotificationContext nc )
        {
            StoreStaticFieldOperator        op         = (StoreStaticFieldOperator)nc.CurrentOperator;
            TypeSystemForCodeTransformation typeSystem = nc.TypeSystem;
            Expression                      exRoot     = typeSystem.GenerateRootAccessor( op );
            InstanceFieldRepresentation     fd         = typeSystem.AddStaticFieldToGlobalRoot( (StaticFieldRepresentation)op.Field );

            //
            // 'exRoot' will never be null, so we don't need to check for it.
            //
            StoreInstanceFieldOperator opNew = StoreInstanceFieldOperator.New( op.DebugInfo, fd, exRoot, op.FirstArgument, false );
            op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.Default );

            nc.MarkAsModified();
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof(Phases.HighLevelTransformations) )]
        [CompilationSteps.OperatorHandler( typeof(MkRefAnyOperator) )]
        private static void Handle_MkRefAnyOperator( PhaseExecution.NotificationContext nc )
        {
            MkRefAnyOperator    op      = (MkRefAnyOperator)nc.CurrentOperator;
            VariableExpression  lhs     =                   op.FirstResult;
            TypeRepresentation  target  =                   lhs.Type;
            FieldRepresentation fdValue =                   target.FindField( "Value" );
            FieldRepresentation fdType  =                   target.FindField( "Type"  );

            op.AddOperatorBefore     ( StoreInstanceFieldOperator.New( op.DebugInfo, fdValue, lhs, op.FirstArgument                  , true )                                     );
            op.SubstituteWithOperator( StoreInstanceFieldOperator.New( op.DebugInfo, fdType , lhs, nc.TypeSystem.GetVTable( op.Type ), true ), Operator.SubstitutionFlags.Default );

            nc.MarkAsModified();
        }

        [CompilationSteps.PhaseFilter( typeof(Phases.HighLevelTransformations) )]
        [CompilationSteps.OperatorHandler( typeof(RefAnyValOperator) )]
        private static void Handle_RefAnyValOperator( PhaseExecution.NotificationContext nc )
        {
            RefAnyValOperator   op      = (RefAnyValOperator)nc.CurrentOperator;
            TypeRepresentation  target  =                    op.FirstArgument.Type;
            FieldRepresentation fdValue =                    target.FindField( "Value" );

            //
            // TODO: Check that target.Type and op.Type are the same.
            //
            LoadInstanceFieldOperator opNew = LoadInstanceFieldOperator.New( op.DebugInfo, fdValue, op.FirstResult, op.FirstArgument, true );
            op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.Default );

            nc.MarkAsModified();
        }

        [CompilationSteps.PhaseFilter( typeof(Phases.HighLevelTransformations) )]
        [CompilationSteps.OperatorHandler( typeof(RefAnyTypeOperator) )]
        private static void Handle_RefAnyTypeOperator( PhaseExecution.NotificationContext nc )
        {
            RefAnyTypeOperator  op     = (RefAnyTypeOperator)nc.CurrentOperator;
            TypeRepresentation  target =                    op.FirstArgument.Type;
            FieldRepresentation fdType =                    target.FindField( "Type" );

            LoadInstanceFieldOperator opNew = LoadInstanceFieldOperator.New( op.DebugInfo, fdType, op.FirstResult, op.FirstArgument, true );
            op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.Default );

            nc.MarkAsModified();
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof(Phases.HighLevelTransformations) )]
        [CompilationSteps.OperatorHandler( typeof(BoxOperator) )]
        private static void Handle_BoxOperator( PhaseExecution.NotificationContext nc )
        {
            BoxOperator op  = (BoxOperator)nc.CurrentOperator;
            var         lhs = op.FirstResult;

            //
            // Try to detect the pattern of many methods in generic types:
            //
            //   private int FindEntry( TKey key )
            //   {
            //       if(key == null)
            //       {
            //
            // 'null' is always a reference and if TKey is a value type, the compiler has to generate a boxing operation.
            // But boxed values are also non-null, so we can delete the boxing and substitute a constant for the outcome of the compare.
            //
            {
                var cfg = nc.CurrentCFG;
                var use = cfg.FindSingleUse( lhs );

                if(use != null)
                {
                    bool fNullOnRight;

                    if(use.IsBinaryOperationAgainstZeroValue( out fNullOnRight ) == lhs)
                    {
                        if(use is CompareAndSetOperator)
                        {
                            var  use2        = (CompareAndSetOperator)use;
                            bool fSubstitute = false;
                            int  val         = 0;

                            switch(use2.Condition)
                            {
                                case CompareAndSetOperator.ActionCondition.EQ:
                                    fSubstitute = true;
                                    val         = 0;
                                    break;

                                case CompareAndSetOperator.ActionCondition.NE:
                                    fSubstitute = true;
                                    val         = 1;
                                    break;
                            }

                            if(fSubstitute)
                            {
                                op.Delete();

                                use2.SubstituteWithOperator( SingleAssignmentOperator.New( use2.DebugInfo, use2.FirstResult, cfg.TypeSystem.CreateConstant( val ) ), Operator.SubstitutionFlags.Default );

                                nc.StopScan();
                                return;
                            }
                        }

                        if(use is CompareConditionalControlOperator)
                        {
                            var        use2        = (CompareConditionalControlOperator)use;
                            bool       fSubstitute = false;
                            BasicBlock target      = null;

                            switch(use2.Condition)
                            {
                                case CompareAndSetOperator.ActionCondition.EQ:
                                    fSubstitute = true;
                                    target      = use2.TargetBranchNotTaken;
                                    break;

                                case CompareAndSetOperator.ActionCondition.NE:
                                    fSubstitute = true;
                                    target      = use2.TargetBranchTaken;
                                    break;
                            }

                            if(fSubstitute)
                            {
                                op.Delete();

                                use2.SubstituteWithOperator( UnconditionalControlOperator.New( use2.DebugInfo, target ), Operator.SubstitutionFlags.Default );

                                nc.StopScan();
                                return;
                            }
                        }
                    }
                }
            }

            op.AddOperatorBefore     ( ObjectAllocationOperator.New( op.DebugInfo, op.Type.UnderlyingType, lhs                                  )                                     );
            op.SubstituteWithOperator( StoreIndirectOperator   .New( op.DebugInfo, lhs.Type              , lhs, op.FirstArgument, null, 0, true ), Operator.SubstitutionFlags.Default );

            nc.MarkAsModified();
        }

        [CompilationSteps.PhaseFilter( typeof(Phases.HighLevelTransformations) )]
        [CompilationSteps.OperatorHandler( typeof(UnboxOperator) )]
        private static void Handle_UnboxOperator( PhaseExecution.NotificationContext nc )
        {
            UnboxOperator op = (UnboxOperator)nc.CurrentOperator;

            op.AddOperatorBefore     ( NullCheckOperator       .New( op.DebugInfo,                 op.FirstArgument )                                     );
            op.SubstituteWithOperator( SingleAssignmentOperator.New( op.DebugInfo, op.FirstResult, op.FirstArgument ), Operator.SubstitutionFlags.Default );

            nc.MarkAsModified();
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof(Phases.HighLevelTransformations) )]
        [CompilationSteps.OperatorHandler( typeof(BinaryOperator) )]
        private static void Handle_RemoveTypeConversions( PhaseExecution.NotificationContext nc )
        {
            BinaryOperator                             op  = (BinaryOperator)nc.CurrentOperator;
            ControlFlowGraphStateForCodeTransformation cfg =                 nc.CurrentCFG;
            
            VariableExpression exLhs        = op.FirstResult;
            ConstantExpression exConstLeft  = op.FirstArgument  as ConstantExpression;
            ConstantExpression exConstRight = op.SecondArgument as ConstantExpression;
            ConversionOperator use          = IsArgumentAsTypeConversion( cfg, exLhs            , false );
            ConversionOperator defLeft      = IsArgumentAsTypeConversion( cfg, op.FirstArgument , true  );
            ConversionOperator defRight     = IsArgumentAsTypeConversion( cfg, op.SecondArgument, true  );

            if(defLeft != null && defRight != null)
            {
                Expression leftPre       = defLeft .FirstArgument;
                Expression leftPost      = defLeft .FirstResult;
                Expression rightPre      = defRight.FirstArgument;
                Expression rightPost     = defRight.FirstResult;
                uint       leftPreSize   = leftPre  .Type.UnderlyingType.Size;
                uint       leftPostSize  = leftPost .Type.UnderlyingType.Size;
                uint       rightPreSize  = rightPre .Type.UnderlyingType.Size;
                uint       rightPostSize = rightPost.Type.UnderlyingType.Size;

                if(leftPreSize  == leftPostSize  && defLeft .SignificantSize == leftPostSize  &&
                   rightPreSize == rightPostSize && defRight.SignificantSize == rightPostSize  )
                {
                    //
                    // The extend ops are no-ops, most likely due to the requirement of loading an int/long into the evaluation stack.
                    //
                    // Shortcut the binary operator to use the pre-extension values.
                    //
                    ShortcutBinaryOperation( nc, op, op.Signed, exLhs, leftPre, rightPre );
                    return;
                }

                if(leftPreSize  == 4 && leftPostSize  == 8 &&
                   rightPreSize == 4 && rightPostSize == 8  )
                {
                    //
                    // This is a 32 -> 64 bit extension. Can we remove the 64 bit operation?
                    //
                    if(use != null && use.SignificantSize == 4)
                    {
                        //
                        // It's a 32->64->32 sequence of operations.
                        //
                        if(op.CheckOverflow == false)
                        {
                            switch(op.Alu)
                            {
                                case BinaryOperator.ALU.ADD:
                                case BinaryOperator.ALU.SUB:
                                case BinaryOperator.ALU.MUL:
                                case BinaryOperator.ALU.AND:
                                case BinaryOperator.ALU.OR :
                                    ShortcutBinaryOperation( nc, op, use.FirstResult.Type.IsSigned, use.FirstResult, leftPre, rightPre );

                                    use.Delete();
                                    nc.StopScan();
                                    return;
                            }
                        }
                    }
                    else
                    {
                        //
                        // It's a 32x32->64 sequence of operations.
                        //
                        if(op.CheckOverflow == false)
                        {
                            switch(op.Alu)
                            {
                                case BinaryOperator.ALU.MUL:
                                    ShortcutBinaryOperation( nc, op, exLhs.Type.IsSigned, exLhs, leftPre, rightPre );
                                    return;
                            }
                        }
                    }
                }
            }

            if(defLeft != null && exConstRight != null)
            {
                Expression leftPre      = defLeft.FirstArgument;
                Expression leftPost     = defLeft.FirstResult;
                uint       leftPreSize  = leftPre     .Type.UnderlyingType.Size;
                uint       leftPostSize = leftPost    .Type.UnderlyingType.Size;
                uint       rightSize    = exConstRight.Type.UnderlyingType.Size;

                if(leftPreSize == leftPostSize && defLeft.SignificantSize == leftPostSize)
                {
                    //
                    // The extend ops are no-ops, most likely due to the requirement of loading an int/long into the evaluation stack.
                    //
                    // Shortcut the binary operator to use the pre-extension values.
                    //
                    ShortcutBinaryOperation( nc, op, op.Signed, exLhs, leftPre, exConstRight );
                    return;
                }

                if(leftPreSize == 4 && leftPostSize == 8 && rightSize == 8)
                {
                    ulong val;

                    if(exConstRight.GetAsRawUlong( out val ))
                    {
                        ConstantExpression exConstRightShort = nc.TypeSystem.CreateConstant( leftPre.Type, ConstantExpression.ConvertToType( leftPre.Type, val ) );

                        //
                        // This is a 32 -> 64 bit extension. Can we remove the 64 bit operation?
                        //
                        if(use != null && use.SignificantSize == 4)
                        {
                            //
                            // It's a 32->64->32 sequence of operations.
                            //
                            if(op.CheckOverflow == false)
                            {
                                switch(op.Alu)
                                {
                                    case BinaryOperator.ALU.ADD:
                                    case BinaryOperator.ALU.SUB:
                                    case BinaryOperator.ALU.MUL:
                                    case BinaryOperator.ALU.AND:
                                    case BinaryOperator.ALU.OR :
                                        ShortcutBinaryOperation( nc, op, use.FirstResult.Type.IsSigned, use.FirstResult, leftPre, exConstRightShort );

                                        use.Delete();
                                        nc.StopScan();
                                        return;
                                }
                            }
                        }
                        else
                        {
                            ulong valPost;

                            exConstRightShort.GetAsRawUlong( out valPost );

                            if(valPost == val)
                            {
                                //
                                // It's a 32x32->64 sequence of operations.
                                //
                                if(op.CheckOverflow == false)
                                {
                                    switch(op.Alu)
                                    {
                                        case BinaryOperator.ALU.MUL:
                                            ShortcutBinaryOperation( nc, op, exLhs.Type.IsSigned, exLhs, leftPre, exConstRightShort );
                                            return;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if(exConstLeft != null && defRight != null)
            {
                Expression rightPre      = defRight.FirstArgument;
                Expression rightPost     = defRight.FirstResult;
                uint       rightPreSize  = rightPre   .Type.UnderlyingType.Size;
                uint       rightPostSize = rightPost  .Type.UnderlyingType.Size;
                uint       leftSize      = exConstLeft.Type.UnderlyingType.Size;

                if(rightPreSize == rightPostSize && defRight.SignificantSize == rightPostSize)
                {
                    //
                    // The extend ops are no-ops, most likely due to the requirement of loading an int/long into the evaluation stack.
                    //
                    // Shortcut the binary operator to use the pre-extension values.
                    //
                    ShortcutBinaryOperation( nc, op, op.Signed, exLhs, exConstLeft, rightPre );
                    return;
                }

                if(rightPreSize == 4 && rightPostSize == 8 && leftSize == 8)
                {
                    ulong val;

                    if(exConstRight.GetAsRawUlong( out val ))
                    {
                        ConstantExpression exConstLeftShort = nc.TypeSystem.CreateConstant( rightPre.Type, ConstantExpression.ConvertToType( rightPre.Type, val ) );

                        //
                        // This is a 32 -> 64 bit extension. Can we remove the 64 bit operation?
                        //
                        if(use != null && use.SignificantSize == 4)
                        {
                            //
                            // It's a 32->64->32 sequence of operations.
                            //
                            if(op.CheckOverflow == false)
                            {
                                switch(op.Alu)
                                {
                                    case BinaryOperator.ALU.ADD:
                                    case BinaryOperator.ALU.SUB:
                                    case BinaryOperator.ALU.MUL:
                                    case BinaryOperator.ALU.AND:
                                    case BinaryOperator.ALU.OR :
                                        ShortcutBinaryOperation( nc, op, use.FirstResult.Type.IsSigned, use.FirstResult, exConstLeftShort, rightPre );

                                        use.Delete();
                                        nc.StopScan();
                                        return;
                                }
                            }
                        }
                        else
                        {
                            ulong valPost;

                            exConstLeftShort.GetAsRawUlong( out valPost );

                            if(valPost == val)
                            {
                                //
                                // It's a 32x32->64 sequence of operations.
                                //
                                if(op.CheckOverflow == false)
                                {
                                    switch(op.Alu)
                                    {
                                        case BinaryOperator.ALU.MUL:
                                            ShortcutBinaryOperation( nc, op, exLhs.Type.IsSigned, exLhs, exConstLeftShort, rightPre );
                                            return;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static ConversionOperator IsArgumentAsTypeConversion( ControlFlowGraphStateForCodeTransformation cfg            ,
                                                                      Expression                                 ex             ,
                                                                      bool                                       fGetDefinition )
        {
            if(ex is VariableExpression)
            {
                Operator[][] useChains = cfg.DataFlow_UseChains;
                Operator[][] defChains = cfg.DataFlow_DefinitionChains;
                int          idx       = ex.SpanningTreeIndex;
                Operator[]   uses      = useChains[idx];
                Operator[]   defs      = defChains[idx];

                if(uses.Length == 1 &&
                   defs.Length == 1  )
                {
                    Operator res = fGetDefinition ? defs[0] : uses[0];

                    return res as ConversionOperator;
                }
            }

            return null;
        }

        private static void ShortcutBinaryOperation( PhaseExecution.NotificationContext nc      ,
                                                     BinaryOperator                     op      ,
                                                     bool                               fSigned ,
                                                     VariableExpression                 lhs     ,
                                                     Expression                         left    ,
                                                     Expression                         right   )
        {
            BinaryOperator opNew = BinaryOperator.New( op.DebugInfo, op.Alu, fSigned, op.CheckOverflow, lhs, left, right );

            op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.CopyAnnotations );

            nc.MarkAsModified();
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        private static TemporaryVariableExpression FetchTypeSystemManager( PhaseExecution.NotificationContext nc ,
                                                                           Operator                           op )
        {
            TypeSystemForCodeTransformation typeSystem          = nc.TypeSystem;
            MethodRepresentation            mdTypeSystemManager = typeSystem.WellKnownMethods.TypeSystemManager_get_Instance;
            TemporaryVariableExpression     typeSystemManagerEx = nc.AllocateTemporary( mdTypeSystemManager.ReturnType );
            CallOperator                    call;
            Expression[]                    rhs;

            rhs  = typeSystem.AddTypePointerToArgumentsOfStaticMethod( mdTypeSystemManager );
            call = StaticCallOperator.New( op.DebugInfo, CallOperator.CallKind.Direct, mdTypeSystemManager, VariableExpression.ToArray( typeSystemManagerEx ), rhs );
            op.AddOperatorBefore( call );

            nc.MarkAsModified();

            return typeSystemManagerEx;
        }
    }
}
