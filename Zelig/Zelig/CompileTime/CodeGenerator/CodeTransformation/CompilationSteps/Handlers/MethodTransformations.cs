//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//
namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

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

                    GrowOnlySet<VariableExpression> addRefVariables = SetFactory.New<VariableExpression>( );

                    var prologueStart = cfg.GetInjectionPoint( BasicBlock.Qualifier.PrologueStart );
                    var mdAddRef = wkm.ObjectHeader_AddReference;

                    foreach(var arg in cfg.Arguments)
                    {
                        // Skip over the first argument (this pointer) 
                        // and arguments that aren't being used (as indicated by an invalid spanning tree index)
                        // and arguments that are not the right types
                        if(arg.Number != 0 && arg.SpanningTreeIndex >= 0 && ts.IsReferenceCountingType( arg.Type ))
                        {
                            var defChain = cfg.DataFlow_DefinitionChains[ arg.SpanningTreeIndex ];

                            Debug.Assert( defChain.Length > 0 && defChain[ 0 ] is InitialValueOperator );

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

                                addRefVariables.Insert( arg );
                            }
                        }
                    }

                    // Find the return variable, if any.
                    var returnOperator = (ReturnControlOperator)cfg.ExitBasicBlock?.FlowControl;
                    var returnVariable = returnOperator?.Arguments.Length > 0 ? returnOperator.FirstArgument : null;

                    // Initialize all reference counting variables to null
                    foreach(var variable in cfg.DataFlow_SpanningTree_Variables)
                    {
                        if(( variable is TemporaryVariableExpression || variable is LocalVariableExpression ) &&
                            ts.IsReferenceCountingType( variable.Type ))
                        {
                            var nullExpression = new ConstantExpression( variable.Type, null );
                            var op = SingleAssignmentOperator.New( null, variable, nullExpression );
                            prologueStart.AddOperator( op );

                            modified = true;
                            phase.AddToModifiedOperator( op );

                            if(variable != returnVariable)
                            {
                                addRefVariables.Insert( variable );
                            }

                        }
                    }

                    var epilogueStart = cfg.GetInjectionPoint( BasicBlock.Qualifier.EpilogueStart );
                    var mdRelease = wkm.ObjectHeader_ReleaseReference;

                    if(epilogueStart != null)
                    {
                        foreach(var variable in addRefVariables)
                        {
                            var rhsRelease = ts.AddTypePointerToArgumentsOfStaticMethod( mdRelease, variable );
                            var callRelease = StaticCallOperator.New( null, CallOperator.CallKind.Direct, mdRelease, rhsRelease );

                            epilogueStart.AddOperator( callRelease );

                            phase.IncrementInjectionCount( mdRelease );

                            modified = true;
                        }
                    }

                    if(modified)
                    {
                        nc.StartScan( );
                    }
                }
            }
        }
    }
}
