//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//
namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps.Handlers
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Zelig.Runtime.TypeSystem;


    public class MethodTransformations
    {
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof(Phases.HighLevelTransformations     ) )]
        [CompilationSteps.PhaseFilter( typeof(Phases.HighLevelToMidLevelConversion) )]
        [CompilationSteps.PhaseFilter( typeof(Phases.MidLevelToLowLevelConversion ) )]
        [CompilationSteps.PhaseFilter( typeof(Phases.ExpandAggregateTypes         ) )]
        [CompilationSteps.PhaseFilter( typeof(Phases.ReferenceCountingGarbageCollection) )]
        [CompilationSteps.PostFlowGraphHandler()]
        private static void ProcessFlowGraphAfterTransformations( PhaseExecution.NotificationContext nc )
        {
            ControlFlowGraphStateForCodeTransformation cfg = nc.CurrentCFG;
            if(cfg != null)
            {
                if(Transformations.CommonMethodRedundancyElimination.Execute( cfg ))
                {
                    nc.StopScan();
                    return;
                }

                cfg.DropDeadVariables();
            }
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PreFlowGraphHandler()]
        private void InjectPrologueAndEpilogue( PhaseExecution.NotificationContext nc )
        {
            var inlineOptions = nc.TypeSystem.GetEnvironmentService<IInlineOptions>( );
            if( inlineOptions != null && !inlineOptions.InjectPrologAndEpilog )
                return;

            if(nc.Phase.ComparePositionTo< Phases.HighLevelTransformations >() >= 0 &&
               nc.Phase.ComparePositionTo< Phases.ReduceTypeSystem         >() <  0  )
            {
                ControlFlowGraphStateForCodeTransformation cfg = nc.CurrentCFG;
                if(cfg != null && cfg.SetProperty( "MethodWrappersAdded" ) == false)
                {
                    var wkm = nc.TypeSystem.WellKnownMethods;

                    {
                        var prologueStart = cfg.GetInjectionPoint( BasicBlock.Qualifier.PrologueStart );
                        var prologueEnd   = cfg.GetInjectionPoint( BasicBlock.Qualifier.PrologueEnd   );
    
                        AddCallToWrapper( nc, prologueStart, prologueEnd, wkm.Microsoft_Zelig_Runtime_AbstractMethodWrapper_Prologue, wkm.Microsoft_Zelig_Runtime_AbstractMethodWrapper_Prologue2 );
                    }

                    {
                        var epilogueStart = cfg.GetInjectionPoint( BasicBlock.Qualifier.EpilogueStart );
                        var epilogueEnd   = cfg.GetInjectionPoint( BasicBlock.Qualifier.EpilogueEnd   );

                        AddCallToWrapper( nc, epilogueStart, epilogueEnd, wkm.Microsoft_Zelig_Runtime_AbstractMethodWrapper_Epilogue, wkm.Microsoft_Zelig_Runtime_AbstractMethodWrapper_Epilogue2 );
                    }

                    nc.StartScan();
                }
            }
        }

        private static void AddCallToWrapper( PhaseExecution.NotificationContext nc          ,
                                              BasicBlock                         targetStart ,
                                              BasicBlock                         targetEnd   ,
                                              MethodRepresentation               mdNormal    ,
                                              MethodRepresentation               mdException )
        {
            if(targetStart != null)
            {
                var                  cfg      = nc.CurrentCFG;
                var                  ts       = nc.TypeSystem;
                var                  md       = cfg.Method;
                var                  attribs  = md.ExpandedBuildTimeFlags;
                var                  he       = ts.ExtractHardwareExceptionSettingsForMethod( md );
                var                  tdHelper = ts.PlatformAbstraction.GetMethodWrapperType();
                var                  exHelper = ts.GenerateConstantForSingleton( tdHelper, DataManager.Attributes.Constant | DataManager.Attributes.SuitableForConstantPropagation );
                Expression[]         rhsHelper;
                MethodRepresentation mdHelper;

                if(ts.PlatformAbstraction.HasRegisterContextArgument( md ))
                {
                    attribs |= MethodRepresentation.BuildTimeAttributes.SaveFullProcessorContext;
                }

                if(he == Runtime.HardwareException.None)
                {
                    rhsHelper = ts.CreateConstantsFromObjects( exHelper, md.OwnerType.FullName, md.ToShortString(), attribs );
                    mdHelper  = mdNormal;
                }
                else
                {
                    rhsHelper = ts.CreateConstantsFromObjects( exHelper, md.OwnerType.FullName, md.ToShortString(), attribs, he );
                    mdHelper  = mdException;
                }

                CallOperator callHelper;

                MethodRepresentation mdHelperDirect = mdHelper.FindVirtualTarget( tdHelper );
                if(mdHelperDirect != null)
                {
                    callHelper = InstanceCallOperator.New( null, CallOperator.CallKind.Overridden, mdHelperDirect, rhsHelper, false );
                }
                else
                {
                    callHelper = InstanceCallOperator.New( null, CallOperator.CallKind.Virtual, mdHelper, rhsHelper, false );
                }

                targetStart.AddOperator( callHelper );
            }
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof(Phases.ExpandAggregateTypes) )]
        [CompilationSteps.PreFlowGraphHandler()]
        private static void ProcessFlowGraphBeforeExpandAggregateTypes( PhaseExecution.NotificationContext nc )
        {
            ControlFlowGraphStateForCodeTransformation cfg = nc.CurrentCFG;
            if(cfg != null)
            {
                cfg.MapVariables();
            }
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof(Phases.ConvertToSSA) )]
        [CompilationSteps.PreFlowGraphHandler()]
        private static void ConvertToSSA( PhaseExecution.NotificationContext nc )
        {
            ControlFlowGraphStateForCodeTransformation cfg = nc.CurrentCFG;
            if(cfg != null)
            {
                if(Transformations.StaticSingleAssignmentForm.ConvertInto( cfg ))
                {
                    nc.MarkAsModified();
                }
            }
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        /// <summary>
        /// Coerce constant call arguments to their target type. This saves having to inject a cast at translation time.
        /// </summary>
        /// <param name="nc">Context for this transformation.</param>
        [CompilationSteps.OperatorHandler(typeof(CallOperator))]
        private void CoerceConstantParameters( PhaseExecution.NotificationContext nc )
        {
            CallOperator         call         = nc.GetOperatorAndThrowIfNotCall();
            TypeRepresentation[] argTypes     = call.TargetMethod.ThisPlusArguments;
            Expression[]         arguments    = call.Arguments;
            bool                 isIndirect   = call is IndirectCallOperator;

            // We always skip the 'this' pointer, so start at 1.
            for( int i = 1 ; i < argTypes.Length; ++i )
            {
                CoerceConstantParameter(ref arguments[isIndirect ? (i + 1) : i], argTypes[i]);
            }
        }

        private static void CoerceConstantParameter( ref Expression argExpr, TypeRepresentation targetType )
        {
            var constExpr = argExpr as ConstantExpression;
            if( constExpr == null )
            {
                return;
            }

            if ( targetType != argExpr.Type )
            {
                // This works for all scalar numeric types, including enums.
                ulong valueAsUInt64;
                if( constExpr.GetAsRawUlong( out valueAsUInt64 ) )
                {
                    object value = ConstantExpression.ConvertToType( targetType, valueAsUInt64 );
                    argExpr = new ConstantExpression( targetType, value );
                }
            }
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.OperatorHandler( typeof(CallOperator) )]
        private static void AttemptDevirtualization( PhaseExecution.NotificationContext nc )
        {
            if(AttemptDevirtualization( nc.TypeSystem, nc.GetOperatorAndThrowIfNotCall() ))
            {
                nc.MarkAsModified();
            }
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        public static bool AttemptDevirtualization( TypeSystemForCodeTransformation typeSystem ,
                                                    CallOperator                    call       )
        {
            MethodRepresentation md = call.TargetMethod;

            if(call.CallType == CallOperator.CallKind.Virtual)
            {
                if(!(md is VirtualMethodRepresentation) ||
                     md is FinalMethodRepresentation     )
                {
                    CallOperator callNew = InstanceCallOperator.New( call.DebugInfo, CallOperator.CallKind.Overridden, md, call.Results, call.Arguments, true );
                    call.SubstituteWithOperator( callNew, Operator.SubstitutionFlags.CopyAnnotations );

                    return true;
                }
                else
                {
                    TypeRepresentation tdTarget = null;

                    {
                        TypeRepresentation tdForcedDevirtualization;

                        if(typeSystem.ForcedDevirtualizations.TryGetValue( md.OwnerType, out tdForcedDevirtualization ))
                        {
                            tdTarget = tdForcedDevirtualization;
                        }
                    }

                    if(tdTarget == null)
                    {
                        Expression         exThis = call.FirstArgument;
                        TypeRepresentation tdThis = exThis.Type.UnderlyingType;

                        if(tdThis is ValueTypeRepresentation || (tdThis.Flags & TypeRepresentation.Attributes.Sealed) != 0)
                        {
                            tdTarget = tdThis;
                        }
                        else if(tdThis.IsSubClassOf( md.OwnerType, null ))
                        {
                            MethodRepresentation md2 = md.FindVirtualTarget( tdThis );

                            if((md2.Flags & MethodRepresentation.Attributes.Final) != 0)
                            {
                                tdTarget = tdThis;
                            }
                        }

                        if(tdTarget == null)
                        {
                            //
                            // BUGBUG: This is valid only if we are in bounded application mode!!!
                            //
                            tdTarget = typeSystem.FindSingleConcreteImplementation( tdThis );
                        }
                    }

                    if(tdTarget != null)
                    {
                        MethodRepresentation md2 = md.FindVirtualTarget( tdTarget );

                        CallOperator callNew = InstanceCallOperator.New( call.DebugInfo, CallOperator.CallKind.Overridden, md2, call.Results, call.Arguments, true );
                        call.SubstituteWithOperator( callNew, Operator.SubstitutionFlags.CopyAnnotations );

                        return true;
                    }
                }
            }

            return false;
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof( Phases.ReferenceCountingGarbageCollection ) )]
        [CompilationSteps.PreFlowGraphHandler( )]
        private static void InjectReferenceCountingSetupAndCleanup( PhaseExecution.NotificationContext nc )
        {
            ControlFlowGraphStateForCodeTransformation cfg = nc.CurrentCFG;
            if(cfg != null && cfg.SetProperty( "ReferenceCountingAdded" ) == false)
            {
                var ts = nc.TypeSystem;

                if(!ts.ShouldExcludeMethodFromReferenceCounting( cfg.Method ))
                {
                    bool modified = false;

                    var phase = (Phases.ReferenceCountingGarbageCollection)nc.Phase;
                    var wkm = ts.WellKnownMethods;

                    var defChains = cfg.DataFlow_DefinitionChains;
                    var useChains = cfg.DataFlow_UseChains;
                    var variables = cfg.DataFlow_SpanningTree_Variables;
                    var arguments = cfg.Arguments;

                    GrowOnlySet<VariableExpression> needRelease = SetFactory.New<VariableExpression>( );

                    var prologueStart = cfg.GetInjectionPoint( BasicBlock.Qualifier.PrologueStart );
                    var epilogueStart = cfg.GetInjectionPoint( BasicBlock.Qualifier.EpilogueStart );

                    cfg.ResetCacheCheckpoint( );

                    var mdAddRef  = wkm.ObjectHeader_AddReference;
                    var mdRelease = wkm.ObjectHeader_ReleaseReference;

                    foreach(var arg in arguments)
                    {
                        var skipReferenceCounting = true;

                        // Skip over the first argument (this pointer) 
                        // and arguments that aren't being used (as indicated by an invalid spanning tree index)
                        // and arguments that are not the right types
                        if(arg.Number != 0 && arg.SpanningTreeIndex >= 0 && ts.IsReferenceCountingType( arg.Type ))
                        {
                            var defChain = defChains[ arg.SpanningTreeIndex ];

                            CHECKS.ASSERT( defChain.Length > 0 && defChain[ 0 ] is InitialValueOperator,
                                "Argument {0} does not have an InitialValueOperator", arg);

                            // Looking at the definition chain, we only want to AddRef/Release arguments that are modified
                            // within the method, so we can manage the lifetime of the new value accordingly. If the argument is
                            // never modified, then the caller would hold a reference to guarantee its lifetime for the
                            // duration of the method call.
                            if(defChain.Length > 1)
                            {
                                var rhsAddref = ts.AddTypePointerToArgumentsOfStaticMethod( mdAddRef, arg );
                                var callAddref = StaticCallOperator.New( null, CallOperator.CallKind.Direct, mdAddRef, rhsAddref );

                                prologueStart.AddOperator( callAddref );

                                phase.IncrementInjectionCount( mdAddRef );

                                modified = true;

                                needRelease.Insert( arg );

                                skipReferenceCounting = false;
                            }
                        }

                        if(skipReferenceCounting)
                        {
                            arg.SkipReferenceCounting = true;
                        }
                    }

                    var returnVariable = FindReturnVariable( cfg, defChains, useChains );

                    var skippables = FindSkippableVariables( cfg, variables, defChains, useChains );

                    foreach(var variable in variables)
                    {
                        if(skippables[ variable.SpanningTreeIndex ])
                        {
                            variable.SkipReferenceCounting = true;
                        }
                        else if (!(variable is ArgumentVariableExpression))
                        {
                            var nullExpression = new ConstantExpression( variable.Type, null );
                            var op = SingleAssignmentOperator.New( null, variable, nullExpression );
                            prologueStart.AddOperator( op );

                            modified = true;
                            phase.AddToModifiedOperator( op );

                            // Do not release the return variable because we need to pass the 
                            // ref count back to the caller on return.
                            if(variable != returnVariable)
                            {
                                needRelease.Insert( variable );
                            }
                        }
                    }

                    if(epilogueStart != null)
                    {
                        foreach(var variable in needRelease)
                        {
                            var rhsRelease = ts.AddTypePointerToArgumentsOfStaticMethod( mdRelease, variable );
                            var callRelease = StaticCallOperator.New( null, CallOperator.CallKind.Direct, mdRelease, rhsRelease );

                            epilogueStart.AddOperator( callRelease );

                            phase.IncrementInjectionCount( mdRelease );

                            modified = true;
                        }

                        // A ref counting return variable should carry a ref count to pass on to the caller,
                        // so if we skip reference counting on the variable, we'd need to take a ref count
                        // prior to returning.
                        if( returnVariable != null &&
                            returnVariable.SkipReferenceCounting &&
                            ts.IsReferenceCountingType( returnVariable.Type ))
                        {
                            var rhsAddRef = ts.AddTypePointerToArgumentsOfStaticMethod( mdAddRef, returnVariable );
                            var callAddRef = StaticCallOperator.New( null, CallOperator.CallKind.Direct, mdAddRef, rhsAddRef );

                            epilogueStart.AddOperator( callAddRef );
                            phase.IncrementInjectionCount( mdAddRef );

                            modified = true;
                        }
                    }

                    if(modified)
                    {
                        nc.StartScan( );
                    }

                    cfg.AssertNoCacheRefreshSinceCheckpoint( );
                }
            }
        }

        private static VariableExpression FindReturnVariable(
            ControlFlowGraphStateForCodeTransformation cfg,
            Operator[][] defChains,
            Operator[][] useChains )
        {
            var ts = cfg.TypeSystem;

            // Find the return variable, if any.
            var returnOperator = (ReturnControlOperator)cfg.ExitBasicBlock?.FlowControl;
            var returnVariable = returnOperator?.Arguments.Length > 0 ? returnOperator.FirstArgument as VariableExpression : null;

            if(returnVariable != null && ts.IsReferenceCountingType( returnVariable.Type ))
            {
                var returnVarSubstitute = FindReturnVariableSubstitute( defChains, useChains, returnVariable );

                if(returnVarSubstitute != null)
                {
                    returnVariable.SkipReferenceCounting = true;
                    returnVariable = returnVarSubstitute;
                }
            }

            return returnVariable;
        }

        // If we can find the following pattern:
        //     tmp = local;
        //      ... <== additional code OK, as long as local is not changed
        //     return tmp;
        // then we can avoid calling AddReference on tmp and ReleaseReference on local.
        // Instead, just hand off the existing ref count on local on return.
        private static VariableExpression FindReturnVariableSubstitute(
            Operator[][] defChains,
            Operator[][] useChains,
            VariableExpression returnVariable )
        {
            // Find the pattern
            var assignOp = ControlFlowGraphState.CheckSingleDefinition( defChains, returnVariable ) as SingleAssignmentOperator;
            var returnOp = ControlFlowGraphState.CheckSingleUse       ( useChains, returnVariable ) as ReturnControlOperator;

            if(assignOp == null || returnOp == null)
            {
                return null;
            }
            
            var returnVarCandidate = assignOp.FirstArgument as VariableExpression;

            if(returnVarCandidate == null)
            {
                return null;
            }
            
            // If we ever take the address of the expression, assume it can be modified.
            foreach(Operator op in useChains[ returnVarCandidate.SpanningTreeIndex ])
            {
                if(op is AddressAssignmentOperator)
                {
                    return null;
                }
            }

            // Gather information about where the substitute is being defined / written
            var substituteDefChain = defChains[ returnVarCandidate.SpanningTreeIndex ];
            var substituteDefBasicBlocks = SetFactory.New<BasicBlock>( );
            foreach(Operator defOp in substituteDefChain)
            {
                substituteDefBasicBlocks.Insert( defOp.BasicBlock );
            }

            // Starting with the basic block that the "tmp = local" assignment took place,
            // find out if local is being redefined in this basic block.
            var currentBasicBlock = assignOp.BasicBlock;
            if(substituteDefBasicBlocks.Contains( currentBasicBlock ))
            {
                // If true, we're OK as long as those definitions happen after the assign operator.
                var currentBlockDefs = new List<Operator>( substituteDefChain ).FindAll(
                    defOp => defOp.BasicBlock == currentBasicBlock );
                if(currentBlockDefs.TrueForAll(
                    defOp => defOp.GetBasicBlockIndex( ) < assignOp.GetBasicBlockIndex( ) ))
                {
                    // Move on to the next basic block, as long as there's no branches
                    var flowCtrl = currentBasicBlock.FlowControl as UnconditionalControlOperator;
                    if(flowCtrl != null)
                    {
                        currentBasicBlock = flowCtrl.TargetBranch;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }

            // Go through the rest of the basic blocks to make sure local is not redefined,
            // and the control flow is linear.
            while(currentBasicBlock != returnOp.BasicBlock)
            {
                if(!substituteDefBasicBlocks.Contains( currentBasicBlock ))
                {
                    // Move on to the next basic block, as long as there's no branches
                    var flowCtrl = currentBasicBlock.FlowControl as UnconditionalControlOperator;
                    if(flowCtrl != null)
                    {
                        currentBasicBlock = flowCtrl.TargetBranch;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }

            return returnVarCandidate;
        }

        private static BitVector FindSkippableVariables(
            ControlFlowGraphStateForCodeTransformation cfg,
            VariableExpression[] variables,
            Operator[][] defChains,
            Operator[][] useChains )
        {
            var skippables = new BitVector( variables.Length );

            // In strict mode, we assume any object can be accessed by multiple threads at the same time
            // so we need to take a local reference to all field / element / indirect load.
            var allowLoads = 
                cfg.TypeSystem.ReferenceCountingGarbageCollectionStatus != 
                TypeSystemForCodeTransformation.ReferenceCountingStatus.EnabledStrict;

            // In this initial pass, we go through each of the variable and mark off the variables that
            // are already marked skippable and of the wrong types, as well as calling the IsVariableSkippable
            // helper with the incomplete skippables information.
            foreach(var variable in variables)
            {
                int i = variable.SpanningTreeIndex;
                if( !cfg.TypeSystem.IsReferenceCountingType( variable.Type ) ||
                    variable.SkipReferenceCounting ||
                    IsVariableSkippable( defChains[ i ], useChains[ i ], skippables, allowLoads ))
                {
                    skippables[ i ] = true;
                }
            }

            // We will repeatedly call IsVariableSkippable helper on all variables with the most up-to-date
            // skippables information until it stops changing. This will help us get the case where a variable
            // is assigned from a previously marked skippable variable.
            bool done;
            do
            {
                done = true;
                foreach(var variable in variables)
                {
                    int i = variable.SpanningTreeIndex;
                    if(!skippables[ i ])
                    {
                        if (IsVariableSkippable( defChains[ i ], useChains[ i ], skippables, allowLoads ))
                        {
                            skippables[ i ] = true;
                            done = false;
                        }
                    }
                }
            }
            while(!done);

            return skippables;
        }

        private static bool IsVariableSkippable( Operator[] defChain, Operator[] useChain, BitVector skippables, bool allowLoads )
        {
            // A variable is skippable only if all its definitions match the criteria. 
            foreach(var defOp in defChain)
            {
                if(defOp is CallOperator)
                {
                    // A ref count is always passed by return, so can't skip.
                    return false;
                }
                else if (defOp is SingleAssignmentOperator)
                {
                    var assignOp = (SingleAssignmentOperator)defOp;
                    var source = assignOp.FirstArgument;

                    // Single assignment is OK if we're assigning from a constant or another skippable 
                    // variable.
                    if(   source is ConstantExpression ||
                        ( source is VariableExpression && skippables[ source.SpanningTreeIndex ] ))
                    {
                        continue;
                    }
                    
                    return false;
                }
                else if(defOp is LoadInstanceFieldOperator ||
                        defOp is LoadElementOperator ||
                        defOp is LoadIndirectOperator)
                {
                    if(allowLoads)
                    {
                        // Special case for volatile fields -- we need to properly ref count them to ensure
                        // correctness.
                        if(defOp is LoadInstanceFieldOperator)
                        {
                            var fieldOp = (LoadInstanceFieldOperator)defOp;
                            if(( fieldOp.Field.Flags & FieldRepresentation.Attributes.IsVolatile ) != 0)
                            {
                                return false;
                            }
                        }

                        continue;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (defOp is InitialValueOperator)
                {
                    // First assignment of an argument -- we're OK since the caller will always have a ref count
                    continue;
                }
                else
                {
                    CHECKS.ASSERT( false, "Unexpected definition operators: %s", defOp );
                }
            }

            // In addition, if the variable's address is taken, we have to assume that it can be use 
            // elsewhere where it might be addref / released, so we cannot skip it.
            foreach(var useOp in useChain)
            {
                if(useOp is AddressAssignmentOperator)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
