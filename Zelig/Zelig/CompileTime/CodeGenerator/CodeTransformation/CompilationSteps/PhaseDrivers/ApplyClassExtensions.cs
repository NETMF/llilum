//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    internal sealed class ApplyClassExtensions : Transformations.ScanTypeSystem
    {
        //
        // State
        //

        private Transformations.ReverseIndexTypeSystem                  m_reverseIndex;
        private List< Transformations.PerformClassExtension.Duplicate > m_duplicates;
        private bool                                                    m_fChanged;

        //
        // Constructor Methods
        //

        internal ApplyClassExtensions( TypeSystemForCodeTransformation typeSystem ) : base( typeSystem, typeof(ApplyClassExtensions) )
        {
            m_fChanged = false;
        }

        //
        // Helper Methods
        //

        internal static bool Hack_MirrorGenericInstantiations( TypeSystemForCodeTransformation typeSystem )
        {
            bool fChanged = false;

            typeSystem.BuildGenericInstantiationTables();

            foreach(TypeRepresentation td in typeSystem.Types.ToArray())
            {
                CustomAttributeRepresentation ca = td.FindCustomAttribute( typeSystem.WellKnownTypes.Microsoft_Zelig_Runtime_ExtendClassAttribute );
                if(ca != null)
                {
                    object             objTarget = ca.FixedArgsValues[0];
                    TypeRepresentation tdTarget  = objTarget as TypeRepresentation;

                    object obj = ca.GetNamedArg( "PlatformVersionFilter" );
                    if(obj != null)
                    {
                        uint filter = (uint)obj;

                        if(( filter & typeSystem.PlatformAbstraction.PlatformVersion ) != typeSystem.PlatformAbstraction.PlatformVersion)
                        {
                            // This type is not an allowed extension for the current platform
                            continue;
                        }
                    }

                    if(tdTarget == null)
                    {
                        tdTarget = typeSystem.GetWellKnownTypeNoThrow( objTarget as string );
                    }

                    if(tdTarget == null)
                    {
                        throw TypeConsistencyErrorException.Create( "Missing target class to extend: {0}", objTarget );
                    }

                    if(td.IsOpenType == false && tdTarget.IsOpenType)
                    {
                        tdTarget = typeSystem.CreateInstantiationOfGenericTemplate( tdTarget, td.GenericParameters );
                    }

                    if(tdTarget.IsOpenType)
                    {
                        List< TypeRepresentation > lstTdTarget;

                        if(typeSystem.GenericTypeInstantiations.TryGetValue( tdTarget, out lstTdTarget ))
                        {
                            List< TypeRepresentation > lstTd;

                            typeSystem.GenericTypeInstantiations.TryGetValue( td, out lstTd );

                            foreach(var td2 in lstTdTarget)
                            {
                                var tdNew = typeSystem.CreateInstantiationOfGenericTemplate( td, td2.GenericParameters );

                                if(td != tdNew && (lstTd == null || lstTd.Contains( tdNew ) == false))
                                {
                                    fChanged = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        fChanged |= Hack_ProcessType( typeSystem, td, tdTarget );
                    }
                }
            }

            return fChanged;
        }

        private static bool Hack_ProcessType( TypeSystemForCodeTransformation typeSystem ,
                                              TypeRepresentation              td         ,
                                              TypeRepresentation              tdTarget   )
        {
            bool fChanged = false;

            var ht = IR.Transformations.PerformClassExtension.Hack_GetSubstitutionTable( typeSystem, td, tdTarget );

            foreach(var mdOverriding in ht.Keys)
            {
                if(mdOverriding.IsOpenMethod)
                {
                    var mdOverridden = ht[mdOverriding];

                    List< MethodRepresentation > lstMdOverridden;

                    if(typeSystem.GenericMethodInstantiations.TryGetValue( mdOverridden, out lstMdOverridden ))
                    {
                        List< MethodRepresentation > lstMdOverriding;

                        typeSystem.GenericMethodInstantiations.TryGetValue( mdOverriding, out lstMdOverriding );

                        foreach(var md in lstMdOverridden)
                        {
                            if(md.IsOpenMethod == false)
                            {
                                var mdNew = typeSystem.CreateInstantiationOfGenericTemplate( mdOverriding, md.GenericParameters );

                                if(lstMdOverriding == null || lstMdOverriding.Contains( mdNew ) == false)
                                {
                                    fChanged = true;
                                }
                            }
                        }
                    }
                }
            }

            return fChanged;
        }

        //--//

        internal void Run()
        {
            m_reverseIndex = new Transformations.ReverseIndexTypeSystem( m_typeSystem );
            m_duplicates   = new List< Transformations.PerformClassExtension.Duplicate >();


            //
            // We need to deal with Generic Types and Methods.
            //
            // For generic types, we need to ensure we have as many specialization of the overridding type as those of the overridden one.
            //
            // For generic methods, we need to ensure that the overriding generic method is instantiated as many times as the overridden one.
            //

            //
            // To speed up the processing of mapping from the overridding entities to the overridden ones,
            // we build a reverse index of the whole type system.
            //
            using(new Transformations.ExecutionTiming( "ReverseIndexTypeSystem" ))
            {
                m_reverseIndex.ProcessTypeSystem();
            }

            //
            // Process types that are generic parameters types first
            // 

            var genericParametersTypes = new List<TypeRepresentation>();

            foreach(TypeRepresentation td in m_typeSystem.Types)
            {
                foreach(var parameter in td.GenericParameters)
                {
                    if(parameter is DelayedMethodParameterTypeRepresentation || parameter is DelayedTypeParameterTypeRepresentation)
                    {
                        continue;
                    }

                    genericParametersTypes.Add( parameter );
                }
            }
            
            foreach(TypeRepresentation td in genericParametersTypes)
            {
                ProcessType( td );
            }
            
            //
            // Process all other types
            // 

            foreach(TypeRepresentation td in m_typeSystem.Types.ToArray())
            {
                ProcessType( td );
            }

            if(m_fChanged)
            {
                if(m_duplicates.Count > 0)
                {
                    Transformations.PerformClassExtension pfe = new Transformations.PerformClassExtension( m_typeSystem, m_reverseIndex, m_duplicates );
    
                    pfe.ProcessDuplicates();
                }

                //
                // We need to rebuild all the hash tables.
                //
                TypeSystemForCodeTransformation typeSystem = m_typeSystem;

                Transform( ref typeSystem );

                typeSystem.RefreshHashCodesAfterTypeSystemRemapping();
            }
        }

        //--//

        protected override bool ShouldRefreshHashCodes()
        {
            return true;
        }

        //--//

        private void ProcessType( TypeRepresentation td )
        {
            if(m_typeSystem.ReachabilitySet.IsProhibited( td ) == false)
            {
                CustomAttributeRepresentation ca = td.FindCustomAttribute( m_typeSystem.WellKnownTypes.Microsoft_Zelig_Runtime_ExtendClassAttribute );
                if(ca != null)
                {   

                    object             objTarget       = ca.FixedArgsValues[0];
                    TypeRepresentation tdTarget        = objTarget as TypeRepresentation;
                    bool               fNoConstructors = false;
                    object             obj;

                    obj = ca.GetNamedArg( "PlatformVersionFilter" );
                    if(obj != null)
                    {
                        uint filter = (uint)obj;

                        if((filter & m_typeSystem.PlatformAbstraction.PlatformVersion) != m_typeSystem.PlatformAbstraction.PlatformVersion)
                        {
                            // This type is not an allowed extension for the current platform
                            return;
                        }
                    }

                    if(tdTarget == null)
                    {
                        tdTarget = m_typeSystem.GetWellKnownTypeNoThrow( objTarget as string );
                    }

                    if(tdTarget == null)
                    {
                        throw TypeConsistencyErrorException.Create( "Missing target class to extend: {0}", objTarget );
                    }

                    if(td.IsOpenType == false && tdTarget.IsOpenType)
                    {
                        List< TypeRepresentation > lstTdTarget;

                        if(m_typeSystem.GenericTypeInstantiations.TryGetValue( tdTarget, out lstTdTarget ) == false)
                        {
                            throw TypeConsistencyErrorException.Create( "Found mismatch between instantiation of class extension and target class: {0} not compatible with {1}", td, tdTarget );
                        }

                        tdTarget = null;

                        foreach(var tdTarget2 in lstTdTarget)
                        {
                            if(tdTarget2.IsOpenType == false && ArrayUtility.ArrayEqualsNotNull( td.GenericParameters, tdTarget2.GenericParameters, 0 ))
                            {
                                tdTarget = tdTarget2;
                            }
                        }

                        if(tdTarget == null)
                        {
                            throw TypeConsistencyErrorException.Create( "Missing target class to extend: {0}", objTarget );
                        }
                    }

                    obj = ca.GetNamedArg( "NoConstructors" );
                    if(obj != null)
                    {
                        fNoConstructors = (bool)obj;
                    }

                    obj = ca.GetNamedArg( "ProcessAfter" );
                    if(obj != null)
                    {
                        TypeRepresentation tdPre = (TypeRepresentation)obj;

                        if(tdPre.HasCustomAttribute( m_typeSystem.WellKnownTypes.Microsoft_Zelig_Runtime_ExtendClassAttribute ))
                        {
                            ProcessType( tdPre );
                        }
                    }

                    TypeRepresentation tdExtends = td.Extends;
                    if(tdExtends != null)
                    {
                        ProcessType( tdExtends );
                    }

                    //
                    // HACK: Generic type extension is handled at the end of meta data import.
                    //
                    if(td.IsOpenType || tdTarget.IsOpenType)
                    {
                        return;
                    }

                    using(new Transformations.ExecutionTiming( "PerformClassExtension from {0} to {1}", td, tdTarget ))
                    {
                        var pfe = new Transformations.PerformClassExtension( m_typeSystem, m_reverseIndex, m_duplicates, td, tdTarget, fNoConstructors );
        
                        pfe.Execute();
                    }

                    m_fChanged = true;
                }
            }
        }
    }
}
