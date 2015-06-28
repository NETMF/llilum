//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//#define TRACK_TIMING

//#define TRACK_TIMING_LEVEL2
//#define TRACK_TIMING_LEVEL3
//#define TRACK_TIMING_LEVEL4


namespace Microsoft.Zelig.CodeGeneration.IR.Transformations
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;

#if TRACK_TIMING
    internal class ExecutionTiming : IDisposable
    {
        [ThreadStatic] static int s_nesting;

        string                     m_text;
        PerformanceCounters.Timing m_counter;

        internal ExecutionTiming(        string   text ,
                                  params object[] args )
        {
            m_text = string.Format( text, args );

            s_nesting++;

            m_counter.Start();
        }

        public void Dispose()
        {
            m_counter.Stop( true );

            Console.WriteLine( "Execution time {0,8}usec : {1}{2}", m_counter.TotalInclusiveMicroSeconds, new string( ' ', (--s_nesting) * 3 ), m_text );
        }
    }
#else
    internal class ExecutionTiming : IDisposable
    {
        internal ExecutionTiming(        string   text ,
                                  params object[] args )
        {
        }

        public void Dispose()
        {
        }
    }
#endif

    //--//

    internal class PerformClassExtension : ScanTypeSystem
    {
        internal struct Duplicate
        {
            //
            // State
            //

            internal object From;
            internal object To;

            //
            // Constructor Methods
            //

            internal Duplicate( object from, object to )
            {
                this.From = from;
                this.To   = to;
            }
        }

        //
        // State
        //

        ReverseIndexTypeSystem                                          m_reverseIndex;
        GrowOnlySet< object >                                           m_newObjectsForReverseIndex;
        List< Duplicate >                                               m_duplicates;
        TypeRepresentation                                              m_overridingTD;
        TypeRepresentation                                              m_overriddenTD;
        bool                                                            m_fNoConstructors;
                                                
        bool                                                            m_fSameExtends;
                                
        GrowOnlyHashTable< object, object >                             m_remapAliases;
        LocateUsageInCode                                               m_inlineCandidates;

        GrowOnlyHashTable< MethodRepresentation, MethodRepresentation > m_substitutionTable;

        object                                                          m_activeContext;
        object                                                          m_replaceFrom;
        object                                                          m_replaceTo;

        //
        // Constructor Methods
        //

        internal PerformClassExtension( TypeSystemForCodeTransformation typeSystem     ,
                                        ReverseIndexTypeSystem          reverseIndex   ,
                                        List< Duplicate >               duplicates     ) : base( typeSystem, typeof(PerformClassExtension) )
        {
            m_reverseIndex              = reverseIndex;
            m_duplicates                = duplicates;

            //--//

            m_newObjectsForReverseIndex = SetFactory.New< object >();

            m_remapAliases              = HashTableFactory.New< object, object >();
            m_inlineCandidates          = new LocateUsageInCode( typeSystem );

            m_substitutionTable         = HashTableFactory.New< MethodRepresentation, MethodRepresentation >();
        }

        internal PerformClassExtension( TypeSystemForCodeTransformation typeSystem      ,
                                        ReverseIndexTypeSystem          reverseIndex    ,
                                        List< Duplicate >               duplicates      ,
                                        TypeRepresentation              overridingTD    ,
                                        TypeRepresentation              overriddenTD    ,
                                        bool                            fNoConstructors ) : this( typeSystem, reverseIndex, duplicates )
        {
            m_overridingTD    = overridingTD;
            m_overriddenTD    = overriddenTD;
            m_fNoConstructors = fNoConstructors;

            //--//

            m_fSameExtends    = (overridingTD.Extends == overriddenTD.Extends);

            //
            // Either the two classes have the same super-class, or the overridding one derives from Object.
            //
            if(!m_fSameExtends)
            {
                if(overridingTD.Extends != typeSystem.WellKnownTypes.System_Object)
                {
                    throw TypeConsistencyErrorException.Create( "Cannot extend '{0}' with '{1}', their superclasses are incompatible", overriddenTD, overridingTD );
                }
            }
        }

        //--//

        //
        // Helper Methods
        //

        internal static GrowOnlyHashTable< MethodRepresentation, MethodRepresentation > Hack_GetSubstitutionTable( TypeSystemForCodeTransformation typeSystem   ,
                                                                                                                   TypeRepresentation              overridingTD ,
                                                                                                                   TypeRepresentation              overriddenTD )
        {
            var pce = new PerformClassExtension( typeSystem, null, null );

            pce.m_overridingTD = overridingTD;
            pce.m_overriddenTD = overriddenTD;

            EquivalenceSet set = new EquivalenceSet( delegate( BaseRepresentation context1,
                                                               BaseRepresentation br1     ,
                                                               BaseRepresentation context2,
                                                               BaseRepresentation br2     )
            {
                DelayedMethodParameterTypeRepresentation td1 = context1 as DelayedMethodParameterTypeRepresentation;
                DelayedMethodParameterTypeRepresentation td2 = context2 as DelayedMethodParameterTypeRepresentation;

                if(td1 != null && td2 != null)
                {
                    MethodRepresentation md1 = br1 as MethodRepresentation;
                    MethodRepresentation md2 = br2 as MethodRepresentation;

                    if(md1 != null && md2 != null)
                    {
                        if(md1.IsOpenMethod && md2.IsOpenMethod)
                        {
                            if(md1.GenericParametersDefinition[td1.ParameterNumber].IsCompatible( ref md2.GenericParametersDefinition[td2.ParameterNumber] ))
                            {
                                return true;
                            }
                        }
                    }
                }
                return false;
            } );

            set.AddEquivalencePair( overridingTD, overriddenTD );

            foreach(MethodRepresentation md in pce.m_overridingTD.Methods)
            {
                pce.m_substitutionTable[md] = pce.FindTargetMethod( md, set );
            }

            return pce.m_substitutionTable;
        }

        //--//

        internal void Execute()
        {
#if GENERICS_DEBUG
            if(m_overridingTD.ToString() == "AbstractReferenceTypeRepresentation(Microsoft.Zelig.Runtime.InterlockedImpl)")
            {
            }
#endif

            //
            // 1) We need to deal with Generic Types and Methods.
            //
            // For generic types, we need to ensure we have as many specialization of the overridding type as those of the overridden one.
            //
            // For generic methods, we need to ensure that the overriding generic method is instantiated as many times as the overridden one.
            //
#if TRACK_TIMING_LEVEL2
            using(new ExecutionTiming( "InstantiateGenericTypesAndMethods for {0}", m_overridingTD ))
#endif
//// HACK: We performed the instantiation at metadata import time.
////        {
////            InstantiateGenericTypesAndMethods();
////        }

            //
            // 2) Go through all the whole type system and replace references from overridingTD to overriddenTD.
            //
#if TRACK_TIMING_LEVEL2
            using(new ExecutionTiming( "PrepareFieldAliasing for {0}", m_overridingTD ))
#endif
            {
                PrepareFieldAliasing();
            }

#if TRACK_TIMING_LEVEL2
            using(new ExecutionTiming( "PrepareMethodAliasing for {0}", m_overridingTD ))
#endif
            {
                PrepareMethodAliasing();
            }

#if TRACK_TIMING_LEVEL2
            using(new ExecutionTiming( "RemapTypesFromOverridingToOverridden for {0}", m_overridingTD ))
#endif
            {
                RemapTypesFromOverridingToOverridden();
            }

#if TRACK_TIMING_LEVEL2
            using(new ExecutionTiming( "ProcessMethodAliasing for {0}", m_overridingTD ))
#endif
            {
                ProcessMethodAliasing();
            }

            //
            // 3) Move fields and methods from overriding type to overridden one:
            //      a) fields should not collide,
            //

            GrowOnlyHashTable< object, object > remap = HashTableFactory.New< object, object >();

#if TRACK_TIMING_LEVEL2
            using(new ExecutionTiming( "MoveFields for {0}", m_overridingTD ))
#endif
            {
                MoveFields( remap );
            }

#if TRACK_TIMING_LEVEL2
            using(new ExecutionTiming( "MoveMethods for {0}", m_overridingTD ))
#endif
            {
                MoveMethods( remap );
            }

#if TRACK_TIMING_LEVEL2
            using(new ExecutionTiming( "ProcessTypeSystem for {0}", m_overridingTD ))
#endif
            {
                ProcessRemaps( remap );
            }

            FlushNewObjects();
        }

        //--//

        private void InstantiateGenericTypesAndMethods()
        {
            GrowOnlyHashTable< MethodRepresentation, List< MethodRepresentation > > ht = null;

            if(m_overridingTD.IsOpenType || m_overridingTD.Generic != null ||
               m_overriddenTD.IsOpenType || m_overriddenTD.Generic != null  )
            {
                throw TypeConsistencyErrorException.Create( "Class Extension for generic types not supported yet: '{0}' => '{1}'", m_overridingTD, m_overriddenTD );
            }

            foreach(MethodRepresentation md in m_overridingTD.Methods)
            {
                if(md.IsOpenMethod)
                {
                    EquivalenceSet set = new EquivalenceSet( delegate( BaseRepresentation context1,
                                                                       BaseRepresentation br1     ,
                                                                       BaseRepresentation context2,
                                                                       BaseRepresentation br2     )
                    {
                        DelayedMethodParameterTypeRepresentation td1 = context1 as DelayedMethodParameterTypeRepresentation;
                        DelayedMethodParameterTypeRepresentation td2 = context2 as DelayedMethodParameterTypeRepresentation;

                        if(td1 != null && td2 != null)
                        {
                            MethodRepresentation md1 = br1 as MethodRepresentation;
                            MethodRepresentation md2 = br2 as MethodRepresentation;

                            if(md1 != null && md2 != null)
                            {
                                if(md1.IsOpenMethod && md2.IsOpenMethod)
                                {
                                    if(md1.GenericParametersDefinition[td1.ParameterNumber].IsCompatible( ref md2.GenericParametersDefinition[td2.ParameterNumber] ))
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                        return false;
                    } );

                    set.AddEquivalencePair( m_overridingTD, m_overriddenTD );

                    MethodRepresentation overriddenMD = FindTargetMethod( md, set );

                    if(overriddenMD != null)
                    {
                        if(ht == null)
                        {
                            m_typeSystem.BuildHierarchyTables();

                            ht = m_typeSystem.GenericMethodInstantiations;
                        }

                        if(ht.ContainsKey( overriddenMD ))
                        {
                            foreach(MethodRepresentation overriddenInstantiationMD in ht[overriddenMD])
                            {
                                if(m_overridingTD.FindMatch( overriddenInstantiationMD, set ) == null)
                                {
                                    InstantiationContext ic;
                                    MethodRepresentation instantiatedMD = m_typeSystem.InstantiateMethod( md, null, overriddenInstantiationMD.GenericParameters, out ic );

                                    ControlFlowGraphStateForCodeTransformation cfg = TypeSystemForCodeTransformation.GetCodeForMethod( md );
                                    if(cfg != null)
                                    {
                                        instantiatedMD.Code = cfg.Clone( ic );
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void PrepareFieldAliasing()
        {
            //
            // AliasForBaseField should only act as a placeholder for a field on the target class.
            //
            foreach(FieldRepresentation fd in m_overridingTD.Fields)
            {
                FieldRepresentation aliasedFD;

                aliasedFD = FindAliasedField( m_typeSystem.WellKnownTypes.Microsoft_Zelig_Runtime_AliasForBaseFieldAttribute, fd, m_overriddenTD );
                if(aliasedFD != null)
                {
                    foreach(CustomAttributeAssociationRepresentation caa in fd.CustomAttributes)
                    {
                        aliasedFD.CopyCustomAttribute( caa );
                    }

                    m_remapAliases[fd] = aliasedFD;

                    m_overridingTD.RemoveField( fd );

                    m_typeSystem.ProhibitUse( fd );
                    continue;
                }
            }
        }

        private static FieldRepresentation FindAliasedField( TypeRepresentation  customAttribute ,
                                                             FieldRepresentation fd              ,
                                                             TypeRepresentation  targetType      )
        {
            CustomAttributeRepresentation ca = fd.FindCustomAttribute( customAttribute );
            if(ca != null)
            {
                fd.RemoveCustomAttribute( ca );

                string targetField;

                if(ca.FixedArgsValues.Length == 0)
                {
                    targetField = fd.Name;
                }
                else
                {
                    targetField = (string)ca.FixedArgsValues[0];
                }

                FieldRepresentation aliasedFD = targetType.FindField( targetField );
                if(aliasedFD == null)
                {
                    throw TypeConsistencyErrorException.Create( "Field '{0}' is aliased to non-existing or incompatible field {1}", fd, targetField );
                }

                return aliasedFD;
            }

            return null;
        }

        //--//

        private void PrepareMethodAliasing()
        {
            //
            // AliasForBaseMethod should only act as a placeholder for a call to another method.
            // AliasForSuperMethod should only act as a placeholder for a call to another method.
            // Both should not be invoked from any other class other than this one.
            // Implementation should be empty.
            //
            foreach(MethodRepresentation md in m_overridingTD.Methods)
            {
                MethodRepresentation aliasedMD;

                aliasedMD = FindAliasedMethod( m_typeSystem.WellKnownTypes.Microsoft_Zelig_Runtime_AliasForBaseMethodAttribute, md, m_overriddenTD );
                if(aliasedMD != null)
                {
                    //
                    // Any call to this method should be inlined, if the target method is going to be overridden.
                    //
                    m_inlineCandidates.AddLookup( md );

                    m_remapAliases[md] = aliasedMD;

                    m_overridingTD.RemoveMethod( md );

                    m_typeSystem.ProhibitUse( md );
                    continue;
                }

                aliasedMD = FindAliasedMethod( m_typeSystem.WellKnownTypes.Microsoft_Zelig_Runtime_AliasForSuperMethodAttribute, md, m_overriddenTD.Extends );
                if(aliasedMD != null)
                {
                    m_remapAliases[md] = aliasedMD;

                    m_overridingTD.RemoveMethod( md );

                    m_typeSystem.ProhibitUse( md );
                    continue;
                }
            }
        }

        private static MethodRepresentation FindAliasedMethod( TypeRepresentation   customAttribute ,
                                                               MethodRepresentation md              ,
                                                               TypeRepresentation   targetType      )
        {
            CustomAttributeRepresentation ca = md.FindCustomAttribute( customAttribute );
            if(ca != null)
            {
                string targetMethod = (string)ca.FixedArgsValues[0];

                MethodRepresentation aliasedMD = targetType.FindMatch( targetMethod, md, null );
                if(aliasedMD == null)
                {
                    throw TypeConsistencyErrorException.Create( "Method '{0}' is aliased to non-existing or incompatible method {1}", md, targetMethod );
                }

                if(TypeSystemForCodeTransformation.GetCodeForMethod( md ) != null)
                {
                    throw TypeConsistencyErrorException.Create( "Method '{0}' cannot be an alias for a base method and have an implementation", md );
                }

                if(md.MemberAccess != MethodRepresentation.Attributes.Private && !(md is VirtualMethodRepresentation))
                {
                    throw TypeConsistencyErrorException.Create( "Alias Method '{0}' has to be marked 'private'", md );
                }

                return aliasedMD;
            }

            return null;
        }

        //--//

        private void RemapTypesFromOverridingToOverridden()
        {
            //
            // Remove the type from the list and process it unconditionally.
            //
            m_typeSystem.Types.Remove( m_overridingTD );

            ProcessRemap( m_overridingTD             , m_overriddenTD              );
            ProcessRemap( m_overridingTD.VirtualTable, m_overriddenTD.VirtualTable );

            //--//

            m_remapAliases    .RefreshHashCodes();
            m_inlineCandidates.RefreshHashCodes();
        }

        //--//

        private void ProcessMethodAliasing()
        {
            foreach(MethodRepresentation md in m_overridingTD.Methods)
            {
                m_substitutionTable[md] = FindTargetMethod( md, null );
            }

            //
            // If any of the overriding methods has a call to one of the aliased method, the calls have to be inlined.
            //
            if(m_inlineCandidates.Entries.Count > 0)
            {
                m_inlineCandidates.ProcessType( m_overridingTD );

                foreach(List< Operator > lst in m_inlineCandidates.Entries.Values)
                {
                    foreach(Operator op in lst)
                    {
                        CallOperator call = (CallOperator)op;
                        if(call != null)
                        {
                            MethodRepresentation md         = call.TargetMethod;
                            MethodRepresentation mdRemapped = (MethodRepresentation)m_remapAliases[md];

                            //
                            // If the aliased method is going to be overridden, we need to inline it.
                            //
                            if(m_substitutionTable.ContainsValue( mdRemapped ))
                            {
                                call.TargetMethod = mdRemapped;

                                Transformations.InlineCall.Execute( call, NotifyNewObjects );
                            }
                        }
                    }
                }
            }

            ProcessRemaps( m_remapAliases );
        }

        private MethodRepresentation FindTargetMethod( MethodRepresentation md  ,
                                                       EquivalenceSet       set )
        {
            string targetMethod;

            CustomAttributeRepresentation ca = md.FindCustomAttribute( m_typeSystem.WellKnownTypes.Microsoft_Zelig_Runtime_AliasForTargetMethodAttribute );
            if(ca != null)
            {
                targetMethod = (string)ca.FixedArgsValues[0];
            }
            else
            {
                targetMethod = md.Name;
            }

            MethodRepresentation overriddenMD;

            if(md.HasCustomAttribute( m_typeSystem.WellKnownTypes.Microsoft_Zelig_Runtime_InjectAtEntryPointAttribute ))
            {
                TypeRepresentation[] tdThisPlusArguments = md.ThisPlusArguments;

                if(md.ReturnType != m_typeSystem.WellKnownTypes.System_Void)
                {
                    throw TypeConsistencyErrorException.Create( "Method '{0}' cannot have a return value because it's marked as InjectAtEntryPoint", md );
                }

                overriddenMD = null;

                foreach(MethodRepresentation md2 in m_overriddenTD.Methods)
                {
                    if(md2.Name == targetMethod)
                    {
                        //
                        // Don't compare the first argument, a method signature does not include any 'this' pointer.
                        //
                        if(BaseRepresentation.ArrayEqualsThroughEquivalence( tdThisPlusArguments, md2.ThisPlusArguments, 1, -1, set ))
                        {
                            overriddenMD = md2;
                            break;
                        }
                    }
                }

                if(overriddenMD == null)
                {
                    throw TypeConsistencyErrorException.Create( "Type '{0}' doesn't have a method compatible for injection of '{0}'", m_overriddenTD.FullNameWithAbbreviation, md.ToShortString() );
                }
            }
            else if(md.HasCustomAttribute( m_typeSystem.WellKnownTypes.Microsoft_Zelig_Runtime_InjectAtExitPointAttribute ))
            {
                TypeRepresentation[] tdThisPlusArguments  = md.ThisPlusArguments;
                int                  numThisPlusArguments = tdThisPlusArguments.Length;

                overriddenMD = null;

                foreach(MethodRepresentation md2 in m_overriddenTD.Methods)
                {
                    if(md2.Name == targetMethod)
                    {
                        TypeRepresentation[] td2ThisPlusArguments  = md2.ThisPlusArguments;
                        int                  num2ThisPlusArguments = td2ThisPlusArguments.Length;

                        if(BaseRepresentation.EqualsThroughEquivalence( md.ReturnType, md2.ReturnType, set ))
                        {
                            //
                            // There' an extra parameter in the overriding method, for the result value.
                            //
                            if(numThisPlusArguments == num2ThisPlusArguments + 1)
                            {
                                //
                                // Make sure the extra parameter is compatible with the return type.
                                //
                                if(BaseRepresentation.EqualsThroughEquivalence( tdThisPlusArguments[numThisPlusArguments-1], md2.ReturnType, set ))
                                {
                                    //
                                    // Don't compare the first argument, a method signature does not include any 'this' pointer.
                                    //
                                    if(BaseRepresentation.ArrayEqualsThroughEquivalence( tdThisPlusArguments, td2ThisPlusArguments, 1, num2ThisPlusArguments - 1, set ))
                                    {
                                        overriddenMD = md2;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }

                if(overriddenMD == null)
                {
                    throw TypeConsistencyErrorException.Create( "Type '{0}' doesn't have a method compatible for injection of '{0}'", m_overriddenTD.FullNameWithAbbreviation, md.ToShortString() );
                }
            }
            else
            {
                overriddenMD = m_overriddenTD.FindMatch( targetMethod, md, set );
            }

            if(ca != null)
            {
                if(overriddenMD == null)
                {
                    throw TypeConsistencyErrorException.Create( "Method '{0}' is aliased to non-existing method {1}", md, targetMethod );
                }
            }
            else
            {
                if(overriddenMD != null)
                {
                    if((overriddenMD is ConstructorMethodRepresentation) != (md is ConstructorMethodRepresentation))
                    {
                        throw TypeConsistencyErrorException.Create( "Method '{0}' cannot override incompatible method '{1}'", md, overriddenMD );
                    }
                }
            }

            return overriddenMD;
        }

        //--//

        private void MoveFields( GrowOnlyHashTable< object, object > remap )
        {
            foreach(FieldRepresentation overridingFD in m_overridingTD.Fields)
            {
                m_overriddenTD.AddField( overridingFD );
            }
        }

        //--//

        private void MoveMethods( GrowOnlyHashTable< object, object > remap )
        {
            ConstructorMethodRepresentation overiddenDefaultConstructor = m_overriddenTD.FindDefaultConstructor();

            foreach(MethodRepresentation overridingMD in m_substitutionTable.Keys)
            {
                CHECKS.ASSERT(overridingMD.OwnerType == m_overriddenTD, "Remapping from {0} to {1} failed on method {2}", m_overridingTD, m_overriddenTD, overridingMD );

                MethodRepresentation                       overriddenMD            = m_substitutionTable[overridingMD];
                ControlFlowGraphStateForCodeTransformation overridingCFG           =                        TypeSystemForCodeTransformation.GetCodeForMethod( overridingMD );
                ControlFlowGraphStateForCodeTransformation overriddenCFG           = overriddenMD != null ? TypeSystemForCodeTransformation.GetCodeForMethod( overriddenMD ) : null;
                bool                                       fAddToOverridden        = false;
                bool                                       fSubstituteToOverridden = false;
                bool                                       fRemoveOverriding       = false;
                bool                                       fMigrateMethodImpl      = false;

                if(overridingMD is ConstructorMethodRepresentation       ||
                   overridingMD is StaticConstructorMethodRepresentation  )
                {
                    bool fDiscard = overridingMD.HasCustomAttribute( m_typeSystem.WellKnownTypes.Microsoft_Zelig_Runtime_DiscardTargetImplementationAttribute   );
                    bool fMerge   = overridingMD.HasCustomAttribute( m_typeSystem.WellKnownTypes.Microsoft_Zelig_Runtime_MergeWithTargetImplementationAttribute );

                    if(overridingCFG == null && fMerge)
                    {
                        throw TypeConsistencyErrorException.Create( "Constructor method '{0}' cannot be an internal call and be decorated with the MergeWithTargetImplementation attribute", overridingMD );
                    }

                    if(overridingMD is ConstructorMethodRepresentation)
                    {
                        if(m_fNoConstructors) continue;

                        if(!fDiscard && !fMerge)
                        {
                            throw TypeConsistencyErrorException.Create( "Constructor method '{0}' should be decorated with DiscardTargetImplementation or MergeWithTargetImplementation", overridingMD );
                        }

                        ////
                        ////  When extending a type, constructors have to be merged, and the first thing to check is whether or not oldTD and newTD share the same super class:
                        ////  
                        ////  1) Same Extends?
                        ////      Yes)
                        ////          2) New Constructor?
                        ////              Yes)
                        ////                  Just add newMD to the list of methods on oldTD.
                        ////                  DONE)
                        ////                  
                        ////              No)
                        ////                  [Either Merge or Discard should be set at this point]
                        ////                  Check Merge and Discard flags (for merge, only same super class calls are allowed, single operator, just forward arguments).
                        ////                  Substitute oldMD for newMD.
                        ////                  DONE)
                        ////     
                        ////      No)
                        ////          [In this case, newTD should extend System.Object, nothing else is allowed.]
                        ////          3) New Constructor?
                        ////              Yes)
                        ////                  4) Does oldTD have a default constructor?
                        ////                      Yes)
                        ////                          Remove call to newTD super constructor, inline in its place the default constructor.
                        ////                          Substitute oldMD for newMD.
                        ////                          DONE)
                        ////                      
                        ////                      No)
                        ////                          Just add newMD to the list of methods on oldTD.
                        ////                          DONE)
                        ////              
                        ////              No)
                        ////                  [Either Merge or Discard should be set at this point]
                        ////
                        ////                  5) Is Merge?
                        ////                      Yes)
                        ////                          Remove call to newTD super constructor, inline in its place the old constructor.
                        ////                          Substitute oldMD for newMD.
                        ////                          DONE)
                        ////
                        ////                      No) [Then it's Discard]
                        ////                          Unless base() in oldMD is a call to the default constructor, fail.
                        ////                          Change call to object() with oldTD.super()
                        ////                          Substitute oldMD for newMD.
                        ////                          DONE)
                        ////

                        if(m_fSameExtends) // 1)
                        {
                            if(overriddenMD == null) // 2)
                            {
                                if(fMerge)
                                {
                                    if(overiddenDefaultConstructor == null)
                                    {
                                        throw TypeConsistencyErrorException.Create( "Overriding constructor '{0}' cannot be merged, neither a matching nor a default constructor could be found", overridingMD );
                                    }

                                    CallOperator oldCall = FindSuperConstructorCall( overridingCFG );
                                    CallOperator newCall = InstanceCallOperator.New( oldCall.DebugInfo, oldCall.CallType, overiddenDefaultConstructor, new Expression[] { oldCall.FirstArgument }, true );

                                    NotifyNewObjects( null, newCall );

                                    oldCall.AddOperatorAfter( newCall );

                                    overridingCFG.TraceToFile( "InlineCall" );

                                    Transformations.InlineCall.Execute( newCall, NotifyNewObjects );

                                    overridingCFG.TraceToFile( "InlineCall-Post" );

                                    //
                                    // Now we are left with two calls to the base constructor, we have to remove the second one.
                                    //
                                    bool fGotGoodOne = false;

                                    foreach(Operator op in oldCall.BasicBlock.Operators)
                                    {
                                        CallOperator call = op as CallOperator;
                                        if(call != null && call.TargetMethod is ConstructorMethodRepresentation)
                                        {
                                            if(fGotGoodOne == false)
                                            {
                                                fGotGoodOne = true;
                                            }
                                            else
                                            {
                                                call.Delete();
                                                break;
                                            }
                                        }
                                    }
                                }

                                fAddToOverridden = true;
                            }
                            else
                            {
                                if(fMerge)
                                {
                                    // TODO: Check that the prologues of the two methods are the same.

                                    //
                                    // Create a proper invocation list.
                                    //
                                    VariableExpression[] args = overridingCFG.Arguments;
                                    Expression[]         rhs  = new Expression[args.Length];

                                    for(int i = 0; i < args.Length; i++)
                                    {
                                        rhs[i] = args[i];
                                    }

                                    //
                                    // Substitute old call to overridden constructor, then inline the whole thing.
                                    //
                                    CallOperator oldCall = FindSuperConstructorCall( overridingCFG );
                                    CallOperator newCall = InstanceCallOperator.New( oldCall.DebugInfo, oldCall.CallType, overriddenMD, rhs, true );

                                    NotifyNewObjects( null, newCall );

                                    oldCall.SubstituteWithOperator( newCall, Operator.SubstitutionFlags.CopyAnnotations );

                                    overridingCFG.TraceToFile( "InlineCall" );

                                    Transformations.InlineCall.Execute( newCall, NotifyNewObjects );

                                    overridingCFG.TraceToFile( "InlineCall-Post" );
                                }

                                fSubstituteToOverridden = true;
                            }
                        }
                        else // Different super class
                        {
                            if(overriddenMD == null) // 3)
                            {
                                if(overiddenDefaultConstructor != null) // 4)
                                {
                                    CallOperator call = FindSuperConstructorCall( overridingCFG );

                                    call.TargetMethod = overiddenDefaultConstructor;

                                    overridingCFG.TraceToFile( "InlineCall" );

                                    Transformations.InlineCall.Execute( call, NotifyNewObjects );

                                    overridingCFG.TraceToFile( "InlineCall-Post" );
                                }

                                fAddToOverridden = true;
                            }
                            else
                            {
                                if(fMerge) // 5)
                                {
                                    // TODO: Check that the prologues of the two methods are the same.

                                    CallOperator oldCall = FindSuperConstructorCall( overridingCFG );
                                    CallOperator newCall = InstanceCallOperator.New( oldCall.DebugInfo, oldCall.CallType, overriddenMD, oldCall.Arguments, true );

                                    NotifyNewObjects( null, newCall );

                                    oldCall.SubstituteWithOperator( newCall, Operator.SubstitutionFlags.CopyAnnotations );

                                    overridingCFG.TraceToFile( "InlineCall" );

                                    Transformations.InlineCall.Execute( newCall, NotifyNewObjects );

                                    overridingCFG.TraceToFile( "InlineCall-Post" );
                                }

                                fSubstituteToOverridden = true;
                            }
                        }
                    }
                    else
                    {
                        if(overriddenMD == null)
                        {
                            fAddToOverridden = true;
                        }
                        else
                        {
                            //
                            // Inline the other static constructor.
                            //
                            Expression[] rhs = new Expression[] { overriddenCFG.Arguments[0] };

                            CallOperator call = InstanceCallOperator.New( null, CallOperator.CallKind.Direct, overriddenMD, rhs, true );

                            NotifyNewObjects( null, call );

                            BasicBlock bb = overridingCFG.NormalizedEntryBasicBlock;

                            bb.FirstOperator.AddOperatorBefore( call );

                            overridingCFG.TraceToFile( "InlineCall" );

                            Transformations.InlineCall.Execute( call, NotifyNewObjects );

                            overridingCFG.TraceToFile( "InlineCall-Post" );

                            //--//

                            fSubstituteToOverridden = true;
                        }
                    }
                }
                else
                {
                    //
                    // Non-constructor case.
                    //
                    bool fInsertPre  = overridingMD.HasCustomAttribute( m_typeSystem.WellKnownTypes.Microsoft_Zelig_Runtime_InjectAtEntryPointAttribute );
                    bool fInsertPost = overridingMD.HasCustomAttribute( m_typeSystem.WellKnownTypes.Microsoft_Zelig_Runtime_InjectAtExitPointAttribute  );

                    if(fInsertPre && fInsertPost)
                    {
                        throw TypeConsistencyErrorException.Create( "Method '{0}' cannot be decorated with both InjectAtEntryPoint and InjectAtExitPoint", overridingMD );
                    }

                    if(fInsertPre || fInsertPost)
                    {
                        if(overriddenMD == null)
                        {
                            throw TypeConsistencyErrorException.Create( "Method '{0}' cannot be injected into target because target is missing", overridingMD );
                        }

                        if(overriddenCFG == null)
                        {
                            throw TypeConsistencyErrorException.Create( "Method '{0}' cannot be injected into target because target doesn't have an implementation", overridingMD );
                        }
                    }

                    if(fInsertPre)
                    {
                        BasicBlock injectBasicBlock = overriddenCFG.GetInjectionPoint( BasicBlock.Qualifier.EntryInjectionEnd );

                        //
                        // Create a proper invocation list.
                        //
                        VariableExpression[] args = overriddenCFG.Arguments;
                        Expression[]         rhs  = new Expression[args.Length];

                        for(int i = 0; i < args.Length; i++)
                        {
                            rhs[i] = args[i];
                        }

                        CallOperator newCall = InstanceCallOperator.New( null, CallOperator.CallKind.Direct, overridingMD, rhs, true );

                        NotifyNewObjects( null, newCall );

                        injectBasicBlock.AddOperator( newCall );

                        overriddenCFG.TraceToFile( "InjectCall" );

                        Transformations.InlineCall.Execute( newCall, NotifyNewObjects );

                        overriddenCFG.TraceToFile( "InjectCall-Post" );

                        fRemoveOverriding = true;
                    }
                    else if(fInsertPost)
                    {
                        BasicBlock injectBasicBlock = overriddenCFG.GetInjectionPoint( BasicBlock.Qualifier.ExitInjectionStart );

                        //
                        // Create a proper invocation list.
                        //
                        VariableExpression[] args = overriddenCFG.Arguments;
                        Expression[]         rhs  = new Expression[args.Length + 1];

                        for(int i = 0; i < args.Length; i++)
                        {
                            rhs[i] = args[i];
                        }

                        rhs[args.Length] = overriddenCFG.ReturnValue;

                        CallOperator newCall = InstanceCallOperator.New( null, CallOperator.CallKind.Direct, overridingMD, VariableExpression.ToArray( overriddenCFG.ReturnValue ), rhs, true );

                        NotifyNewObjects( null, newCall );

                        injectBasicBlock.AddOperator( newCall );

                        overriddenCFG.TraceToFile( "InjectCall" );

                        Transformations.InlineCall.Execute( newCall, NotifyNewObjects );

                        overriddenCFG.TraceToFile( "InjectCall-Post" );

                        fRemoveOverriding = true;
                    }
                    else
                    {
                        //
                        // The default case is very simple, either add or substitute.
                        //
                        if(overriddenMD == null)
                        {
                            fAddToOverridden = true;
                        }
                        else
                        {
                            fSubstituteToOverridden = true;
                        }
                    }
                }

                if(fAddToOverridden)
                {
                    m_overriddenTD.AddMethod( overridingMD );

                    fMigrateMethodImpl = true;
                }

                if(fSubstituteToOverridden)
                {
                    m_overriddenTD.Substituite( overriddenMD, overridingMD );

                    remap[overriddenMD] = overridingMD;

                    fMigrateMethodImpl = true;
                }

                if(fRemoveOverriding)
                {
                    m_overridingTD.RemoveMethod( overridingMD );

                    remap[overridingMD] = overriddenMD;
                }

                if(fMigrateMethodImpl)
                {
                    MigrateMethodImpl( m_overriddenTD, m_overridingTD, overridingMD );
                }
            }
        }

        //--//

        private void NotifyNewObjects( object from, object to )
        {
            m_newObjectsForReverseIndex.Insert( to );
        }

        private static CallOperator FindSuperConstructorCall( ControlFlowGraphStateForCodeTransformation cfg )
        {
            var bb = cfg.NormalizedEntryBasicBlock;

            while(true)
            {
                Operator[] ops = bb.Operators;

                CallOperator call = ops[0] as CallOperator;
                if(call != null && call.TargetMethod is ConstructorMethodRepresentation)
                {
                    return call;
                }

                var ctrl = bb.FlowControl as UnconditionalControlOperator;
                if(ctrl == null)
                {
                    break;
                }

                bb = ctrl.TargetBranch;
            }

            throw TypeConsistencyErrorException.Create( "Cannot find call to constructor in '{0}'", cfg.Method );
        }

        private static void MigrateMethodImpl( TypeRepresentation   overriddenTD ,
                                               TypeRepresentation   overridingTD ,
                                               MethodRepresentation overridingMD )
        {
            foreach(MethodImplRepresentation mi in overridingTD.MethodImpls)
            {
                if(mi.Body == overridingMD)
                {
                    overriddenTD.AddMethodImpl( new MethodImplRepresentation( overridingMD, mi.Declaration ) );
                }
            }
        }

        //--//

        internal void ProcessDuplicates()
        {
            while(m_duplicates.Count > 0)
            {
                Duplicate[] duplicates = m_duplicates.ToArray();

                m_duplicates.Clear();

                foreach(Duplicate duplicate in duplicates)
                {
                    object from = duplicate.From;
                    object to   = duplicate.To;

                    if(m_typeSystem.ReachabilitySet.IsProhibited( from ) == false &&
                       m_typeSystem.ReachabilitySet.IsProhibited( to   ) == false  )
                    {
                        if(Object.Equals( from, to ))
                        {
                            //
                            // Types are kept in a list, which doesn't shrink when we process remaps.
                            // We'll have to manually force an update.
                            //
                            if(from is TypeRepresentation)
                            {
                                var tdFrom = (TypeRepresentation)from;

                                m_typeSystem.RemoveDuplicateType( tdFrom );
                            }

                            //
                            // Fields are kept in an array, which doesn't shrink when we process remaps.
                            // We'll have to manually force an update.
                            //
                            if(from is FieldRepresentation)
                            {
                                var fdFrom = (FieldRepresentation)from;
                                var fdTo   = (FieldRepresentation)to;
                                var td     = fdFrom.OwnerType;
    
                                if(ArrayUtility.FindReferenceInNotNullArray( td.Fields, fdFrom ) >= 0 &&
                                   ArrayUtility.FindReferenceInNotNullArray( td.Fields, fdTo   ) >= 0)
                                {
                                    td.RemoveField( fdFrom );
                                }
                            }
    
                            //
                            // Methods are kept in an array, which doesn't shrink when we process remaps.
                            // We'll have to manually force an update.
                            //
                            if(from is MethodRepresentation)
                            {
                                var mdFrom = (MethodRepresentation)from;
                                var mdTo   = (MethodRepresentation)to;
                                var td     = mdFrom.OwnerType;
    
                                if(ArrayUtility.FindReferenceInNotNullArray( td.Methods, mdFrom ) >= 0 &&
                                   ArrayUtility.FindReferenceInNotNullArray( td.Methods, mdTo   ) >= 0)
                                {
                                    td.RemoveMethod( mdFrom );
                                }
                            }

                            ProcessRemap( from, to );
                        }
                    }
                }
            }
        }

        private void ProcessRemaps( GrowOnlyHashTable< object, object > remap )
        {
            object[] keys   = remap.KeysToArray();
            object[] values = remap.ValuesToArray();

            for(int i = 0; i < keys.Length; i++)
            {
#if TRACK_TIMING_LEVEL3
                using(new ExecutionTiming( "Remap from {0} to {1} : {2}", keys[i], values[i], m_newObjectsForReverseIndex.Count ))
#endif
                {
                    ProcessRemap( keys[i], values[i] );
                }
            }
        }

        private void ProcessRemap( object from ,
                                   object to   )
        {
            m_replaceFrom = from;
            m_replaceTo   = to;

            object[] arrayFrom = null;
            object[] arrayTo   = null;

            if(m_reverseIndex.CanBeTracked( from ) == false)
            {
                throw TypeConsistencyErrorException.Create( "Got unexpected source object to remap: {0}", from );
            }

            if(m_reverseIndex.CanBeTracked( to ) == false)
            {
                throw TypeConsistencyErrorException.Create( "Got unexpected destination object to remap: {0}", to );
            }

////        List< object[] > pre = null;
////
////        {
////            pre = CollectUsageContext.Execute( m_typeSystem, from );
////        }

            FlushNewObjects();

            GrowOnlySet< object > setFrom = m_reverseIndex[from];
            if(setFrom != null)
            {
                foreach(object context in setFrom)
                {
                    m_activeContext = context;

                    this.Reset();

                    object obj = context; Transform( ref obj );
                }

                GrowOnlySet< object > setTo = m_reverseIndex[to];
                if(setTo != null)
                {
                    arrayFrom = setFrom.ToArray();
                    arrayTo   = setTo.ToArray();
                }

                m_reverseIndex.Merge( from, to );
            }

            m_typeSystem.ProhibitUse( from );

////        if(pre != null)
////        {
////            List< object[] > post = CollectUsageContext.Execute( m_typeSystem, from );
////        }

            if(arrayTo != null)
            {
                GrowOnlySet< object > set = SetFactory.New< object >();
    
                foreach(object obj in arrayTo)
                {
                    CheckForDuplicates( obj, set );
                }
    
                foreach(object obj in arrayFrom)
                {
                    CheckForDuplicates( obj, set );
                }
            }
        }

        private void FlushNewObjects()
        {
            if(m_newObjectsForReverseIndex.Count > 0)
            {
                foreach(object obj in m_newObjectsForReverseIndex)
                {
#if TRACK_TIMING_LEVEL4
                    using(new ExecutionTiming( "Update for {0}", obj ))
#endif
                    {
                        m_reverseIndex.Update( obj );
                    }
                }

                m_newObjectsForReverseIndex.Clear();
            }
        }

        private void CheckForDuplicates( object                obj ,
                                         GrowOnlySet< object > set )
        {
            object objOld;

            if(set.Contains( obj, out objOld ) == true)
            {
                if(Object.ReferenceEquals( obj, objOld ) == false)
                {
                    m_duplicates.Add( new Duplicate( obj, objOld ) );
                }
            }
            else
            {
                set.Insert( obj );
            }
        }

        //--//

        protected override object ShouldSubstitute(     object             target ,
                                                    out SubstitutionAction result )
        {
            object context = this.TopContext();

            if(context == null)
            {
                result = SubstitutionAction.Unknown;
                return null;
            }

            if(Object.ReferenceEquals( context, m_activeContext ))
            {
                if(Object.ReferenceEquals( m_replaceFrom, target ))
                {
                    result = SubstitutionAction.Substitute;
                    return m_replaceTo;
                }
                else
                {
                    result = SubstitutionAction.Unknown;
                    return null;
                }
            }

            result = SubstitutionAction.Keep;
            return null;
        }
    }
}
