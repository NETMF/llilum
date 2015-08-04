//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//#define COMPUTECALLSCLOSURE_FORCE_SINGLE_THREADED


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public interface ICallClosureComputationTarget
    {
        void ExpandClosure( ComputeCallsClosure.Context host );
    }

    public sealed class ComputeCallsClosure
    {
        public delegate void Notification( Context host, Operator op );

        public class Context
        {
            //
            // State
            //

            private readonly ComputeCallsClosure     m_owner;
            private readonly TypeSystem.Reachability m_reachabilitySet;

            //
            // Constructor Methods
            //
            internal Context( ComputeCallsClosure     owner           ,
                              TypeSystem.Reachability reachabilitySet )
            {
                m_owner           = owner;
                m_reachabilitySet = reachabilitySet;
            }

            //
            // Helper Methods
            //

            internal void ProcessMethod( MethodRepresentation md )
            {
                CoverObject( md );

                ControlFlowGraphStateForCodeTransformation cfg = TypeSystemForCodeTransformation.GetCodeForMethod( md );
                if(cfg != null)
                {
                    Transformations.ScanCodeWithCallback.Execute( m_owner.m_typeSystem, this, cfg, delegate( Operator op, object target )
                    {
                        if(target != null)
                        {
                            MethodRepresentation mdTarget = target as MethodRepresentation;

                            if(mdTarget != null)
                            {
                                m_owner.QueueMethodForProcessing( mdTarget );
                            }

                            CoverObject( target );

                            if(target is Operator)
                            {
                                Operator opTarget = (Operator)target;
                                Type     type     = opTarget.GetType();

                                while(type != null)
                                {
                                    List< Notification > lst;

                                    if(m_owner.m_delegation.TryGetValue( type, out lst ))
                                    {
                                        foreach(Notification dlg in lst)
                                        {
                                            dlg( this, opTarget );
                                        }
                                    }

                                    type = type.BaseType;
                                }

                                CallOperator call = opTarget as CallOperator;
                                if(call != null)
                                {
                                    m_owner.m_callsDatabase.RegisterCallSite( call );
                                }

                                var tdCodePointer = m_owner.m_typeSystem.WellKnownTypes.Microsoft_Zelig_Runtime_TypeSystem_CodePointer;
                                foreach(var ex in opTarget.Arguments)
                                {
                                    var exConst = ex as ConstantExpression;
                                    if(exConst != null && exConst.Type == tdCodePointer)
                                    {
                                        var od = (DataManager.ObjectDescriptor)exConst.Value;
                                        if(od != null)
                                        {
                                            var cp = (CodePointer)od.Source;

                                            var mdDelegate = m_owner.m_typeSystem.DataManagerInstance.GetCodePointerFromUniqueID( cp.Target );

                                            CoverObject( mdDelegate );
                                        }
                                    }
                                }
                            }
                        }

                        return Transformations.ScanCodeWithCallback.CallbackResult.Proceed;
                    } );
                }
            }

            public void CoverObject( object obj )
            {
                if(obj == null) return;

                if(obj is MethodRepresentation)
                {
                    CoverMethod( (MethodRepresentation)obj );
                }
                else if(obj is FieldRepresentation)
                {
                    CoverField( (FieldRepresentation)obj );
                }
                else if(obj is TypeRepresentation)
                {
                    CoverType( (TypeRepresentation)obj );
                }
                else if(obj is AssemblyRepresentation)
                {
                    CoverAssembly( (AssemblyRepresentation)obj );
                }
                else if(obj is CustomAttributeRepresentation)
                {
                    CoverCustomAttribute( (CustomAttributeRepresentation)obj );
                }
                else if(obj is CustomAttributeAssociationRepresentation)
                {
                    CoverCustomAttributeAssociation( (CustomAttributeAssociationRepresentation)obj );
                }
                else if(obj is ICallClosureComputationTarget)
                {
                    ICallClosureComputationTarget itf = (ICallClosureComputationTarget)obj;

                    itf.ExpandClosure( this );
                }
                else
                {
                    m_reachabilitySet.ExpandPending( obj );
                }
            }

            private void CoverReflectionType( object obj )
            {
                TypeRepresentation td = m_owner.m_typeSystem.TryGetTypeRepresentationFromType( obj.GetType() );

                if(td != null)
                {
                    CoverType( td );
                }
            }

            private void CoverAssembly( AssemblyRepresentation asml )
            {
                m_reachabilitySet.ExpandPending( asml );
            }

            private void CoverType( TypeRepresentation td )
            {
                while(td != null)
                {
                    if(m_reachabilitySet.ExpandPending( td ))
                    {
                        break;
                    }

                    CoverReflectionType( td );

                    CoverAssembly( td.Owner );

                    CoverType( td.Extends );

                    CoverType( td.EnclosingClass );

                    //
                    // If a generic type, include the context.
                    //
                    TypeRepresentation.GenericContext gc = td.Generic;
                    if(gc != null)
                    {
                        CoverType( gc.Template );

                        foreach(TypeRepresentation td2 in gc.Parameters)
                        {
                            CoverType( td2 );
                        }

                        GenericParameterDefinition[] parametersDefinition = gc.ParametersDefinition;
                        if(parametersDefinition != null)
                        {
                            for(int i = 0; i < parametersDefinition.Length; i++)
                            {
                                foreach(TypeRepresentation constraint in parametersDefinition[i].Constraints)
                                {
                                    CoverType( constraint );
                                }
                            }
                        }
                    }

                    //
                    // These are weird objects, because they actually contain a use of themselves in their definition.
                    // We want to keep the fields alive, so that the size computation doesn't generate the wrong values.
                    //
                    if(td is ScalarTypeRepresentation)
                    {
                        foreach(FieldRepresentation fd in td.Fields)
                        {
                            if(fd is InstanceFieldRepresentation)
                            {
                                CoverField( fd );
                            }
                        }
                    }

                    if(td is ArrayReferenceTypeRepresentation)
                    {
                        //
                        // Keep alive also the managed pointer to the elements of the array, it will be used later.
                        //
                        CoverType( m_owner.m_typeSystem.GetManagedPointerToType( td.ContainedType ) );
                    }

                    //
                    // For a non-pointer type, keep alive any associated pointer type.
                    //
                    if(!(td is PointerTypeRepresentation))
                    {
                        foreach(TypeRepresentation td2 in m_owner.m_typeSystem.Types)
                        {
                            PointerTypeRepresentation ptr = td2 as PointerTypeRepresentation;

                            if(ptr != null && ptr.ContainedType == td)
                            {
                                CoverType( ptr );
                            }
                        }
                    }

                    //
                    // Look for all the fields, if they have an "AssumeReferenced" attribute, force them to be included.
                    //
                    {
                        TypeRepresentation tdAttrib = m_owner.m_typeSystem.WellKnownTypes.Microsoft_Zelig_Runtime_TypeSystem_AssumeReferencedAttribute;

                        foreach(FieldRepresentation fd in td.Fields)
                        {
                            if(fd.HasCustomAttribute( tdAttrib ))
                            {
                                CoverField( fd );
                            }
                        }
                    }

                    //
                    // Look for all the methods that are marked as exception handlers and include them.
                    //
                    {
                        var heh = m_owner.m_typeSystem.HardwareExceptionHandlers;

                        foreach(MethodRepresentation md in td.Methods)
                        {
                            CustomAttributeRepresentation ca;

                            if(heh.TryGetValue( md, out ca ))
                            {
                                CoverMethod         ( md );
                                CoverCustomAttribute( ca );
                            }
                        }
                    }

                    //
                    // Look for all the methods that are marked as debugger handlers and include them.
                    //
                    {
                        var dhh = m_owner.m_typeSystem.DebuggerHookHandlers;

                        foreach(MethodRepresentation md in td.Methods)
                        {
                            CustomAttributeRepresentation ca;

                            if(dhh.TryGetValue( md, out ca ))
                            {
                                CoverMethod         ( md );
                                CoverCustomAttribute( ca );
                            }
                        }
                    }

                    td = td.ContainedType;
                }
            }

            private void CoverField( FieldRepresentation fd )
            {
                if(fd == null) return;

                if(m_reachabilitySet.ExpandPending( fd ))
                {
                    return;
                }

                CoverReflectionType( fd );

                CoverType( fd.OwnerType );
                CoverType( fd.FieldType );

                if(fd is StaticFieldRepresentation)
                {
                    StaticFieldRepresentation fdS = (StaticFieldRepresentation)fd;

                    CoverField( fdS.ImplementedBy );

                    CoverStaticConstructor( fdS );
                }

                if(fd is InstanceFieldRepresentation)
                {
                    InstanceFieldRepresentation fdI = (InstanceFieldRepresentation)fd;

                    CoverField( fdI.ImplementationOf );

                    CoverStaticConstructor( fdI.ImplementationOf );
                }
            }

            private void CoverStaticConstructor( StaticFieldRepresentation fd )
            {
                if(fd == null) return;

                StaticConstructorMethodRepresentation md = fd.OwnerType.FindDefaultStaticConstructor();

                if(md != null)
                {
                    CoverMethod( md                                                                       );
                    CoverMethod( m_owner.m_typeSystem.WellKnownMethods.TypeSystemManager_InvokeStaticConstructors );
                }
            }

            private void CoverMethod( MethodRepresentation md )
            {
                if(md == null) return;

                if(m_reachabilitySet.ExpandPending( md ))
                {
                    return;
                }

                CoverReflectionType( md );

                m_owner.QueueMethodForProcessing( md );

                //
                // If a generic method, include the context.
                //
                MethodRepresentation.GenericContext gc = md.Generic;
                if(gc != null)
                {
                    CoverMethod( gc.Template );

                    foreach(TypeRepresentation td in gc.Parameters)
                    {
                        CoverType( td );
                    }

                    GenericParameterDefinition[] parametersDefinition = gc.ParametersDefinition;
                    if(parametersDefinition != null)
                    {
                        for(int i = 0; i < parametersDefinition.Length; i++)
                        {
                            foreach(TypeRepresentation constraint in parametersDefinition[i].Constraints)
                            {
                                CoverType( constraint );
                            }
                        }
                    }
                }

                CoverType( md.OwnerType );

                CoverType( md.ReturnType );

                foreach(TypeRepresentation td in md.ThisPlusArguments)
                {
                    CoverType( td );
                }

                ///
                /// For imported methods we need to make sure that any passed structures are preserved.
                /// The type system reduction phase will attempt to remove member fields if they are 
                /// not used or covered here which will leave them with a different signature/size than
                /// the native imported method is expecting.
                ///
                if(0 != ( md.BuildTimeFlags & MethodRepresentation.BuildTimeAttributes.Imported ))
                {
                    foreach(CustomAttributeAssociationRepresentation caa in md.CustomAttributes)
                    {
                        CoverCustomAttributeAssociation( caa );
                    }

                    HashSet<TypeRepresentation> importTypes = new HashSet<TypeRepresentation>();

                    foreach(TypeRepresentation tr in md.ThisPlusArguments)
                    {
                        // Make imported structures do not lose members during type system reduction
                        if(
                            tr.BuiltInType                == TypeRepresentation.BuiltInTypes.BYREF     && 
                            tr.UnderlyingType.BuiltInType == TypeRepresentation.BuiltInTypes.VALUETYPE )
                        {
                            CoverImportedDataType( tr.UnderlyingType, importTypes );
                        }
                        // also check for arrays of structs
                        else if(
                            tr.BuiltInType                == TypeRepresentation.BuiltInTypes.ARRAY     &&
                            tr.ContainedType.BuiltInType  == TypeRepresentation.BuiltInTypes.VALUETYPE )
                        {
                            CoverImportedDataType( tr.ContainedType, importTypes );
                        }

                    }
                }
            }

            /// <summary>
            /// Cover all fields from an imported structure so that its size/signature remains consistent.
            /// </summary>
            /// <param name="tr"></param>
            /// <param name="coveredTypes"></param>
            private void CoverImportedDataType( TypeRepresentation tr, HashSet<TypeRepresentation> coveredTypes )
            {
                if(coveredTypes.Contains( tr )) return;

                coveredTypes.Add( tr );

                CoverType( tr );

                foreach(FieldRepresentation fr in tr.Fields)
                {
                    CoverField( fr );

                    CoverImportedDataType( fr.FieldType, coveredTypes );
                }
            }

            private void CoverCustomAttribute( CustomAttributeRepresentation ca )
            {
                if(ca == null) return;

                if(m_reachabilitySet.ExpandPending( ca ))
                {
                    return;
                }

                CoverReflectionType( ca );

                CoverMethod( ca.Constructor );
            }

            private void CoverCustomAttributeAssociation( CustomAttributeAssociationRepresentation caa )
            {
                if(caa == null) return;

                if(m_reachabilitySet.ExpandPending( caa ))
                {
                    return;
                }

                CoverReflectionType( caa );

                CoverObject( caa.Target          );
                CoverObject( caa.CustomAttribute );
            }

            //--//

            internal void IncludeGeneric( object obj )
            {
                CHECKS.ASSERT( m_owner.m_typeSystem.IsUseProhibited( obj ) == false, "Found use of '{0}' after the entity was marked illegal to use", obj );

                if(obj is TypeRepresentation)
                {
                    Include( (TypeRepresentation)obj );
                }
                else if(obj is TypeRepresentation.GenericContext)
                {
                    Include( (TypeRepresentation.GenericContext)obj );
                }
                else if(obj is FieldRepresentation)
                {
                    Include( (FieldRepresentation)obj );
                }
                else if(obj is MethodRepresentation)
                {
                    Include( (MethodRepresentation)obj );
                }
                else if(obj is MethodRepresentation.GenericContext)
                {
                    Include( (MethodRepresentation.GenericContext)obj );
                }
                else if(obj is CustomAttributeRepresentation)
                {
                    Include( (CustomAttributeRepresentation)obj );
                }
                else if(obj is VTable)
                {
                    Include( (VTable)obj );
                }
                else if(obj is DataManager.DataDescriptor)
                {
                    Include( (DataManager.DataDescriptor)obj );
                }
            }

            private void Include( TypeRepresentation td )
            {
                CHECKS.ASSERT( m_owner.m_typeSystem.IsUseProhibited( td ) == false, "Found use of '{0}' after the entity was marked illegal to use", td );

                CoverType( td );

                //--//

                TypeRepresentation tdSystem_Attribute = m_owner.m_typeSystem.WellKnownTypes.System_Attribute;
                if(tdSystem_Attribute != null && tdSystem_Attribute.IsSuperClassOf( td, null ))
                {
                    //
                    // If we touch an attribute, make sure all the constructor methods used by actual instances are touched.
                    //
                    m_owner.m_typeSystem.EnumerateCustomAttributes( delegate( CustomAttributeAssociationRepresentation caa )
                    {
                        CustomAttributeRepresentation ca   = caa.CustomAttribute;
                        MethodRepresentation          mdCa = ca .Constructor;

                        if(mdCa.OwnerType == td)
                        {
                            //
                            // BUGBUG: The named arguments are not included!
                            //
                            if(m_reachabilitySet.Contains( mdCa ) == false)
                            {
                                CoverMethod( mdCa );
                            }
                        }
                    } );
                }

                //--//

                //
                // Include all the concrete subclasses, because we need at least one implementation.
                //
                if(td.HasBuildTimeFlag( TypeRepresentation.BuildTimeAttributes.ForceDevirtualization ))
                {
                    foreach(TypeRepresentation td2 in m_owner.m_typeSystem.Types)
                    {
                        if(td2.IsAbstract == false)
                        {
                            for(TypeRepresentation td3 = td2; td3 != null; td3 = td3.Extends)
                            {
                                if(td3 == td)
                                {
                                    CoverType( td2 );
                                    break;
                                }
                            }
                        }
                    }
                }

                //--//

                //
                // Since call closure has reached a new type, make sure that all the overrides to already touched virtual methods are included.
                //
                foreach(MethodRepresentation md in td.Methods)
                {
                    if(md is VirtualMethodRepresentation && md.IsOpenMethod == false && td.IsOpenType == false && m_reachabilitySet.Contains( md ) == false)
                    {
                        int index = md.FindVirtualTableIndex();

                        if(index == -1)
                        {
                            continue;
                        }

                        MethodRepresentation mdRoot = md;
                        TypeRepresentation   tdRoot = td;
                        while((mdRoot.Flags & MethodRepresentation.Attributes.NewSlot) == 0)
                        {
                            tdRoot = tdRoot.Extends; if(tdRoot == null) break;
                            
                            mdRoot = tdRoot.MethodTable[index];

                            if(m_reachabilitySet.Contains( mdRoot ))
                            {
                                CoverMethod( md );
                                break;
                            }
                        }
                    }
                }


                //
                // Also, include the methods associated with already touched interfaces.
                //
                foreach(TypeRepresentation.InterfaceMap map in td.InterfaceMaps)
                {
                    if(m_reachabilitySet.Contains( map.Interface ))
                    {
                        MethodRepresentation[] mdOverride = map.Methods;
                        MethodRepresentation[] mdDeclaration = map.Interface.FindInterfaceTable( map.Interface );

                        for(int i = 0; i < mdDeclaration.Length; i++)
                        {
                            if(m_reachabilitySet.Contains( mdDeclaration[i] ))
                            {
                                if(m_reachabilitySet.Contains( mdOverride[i] ) == false)
                                {
                                    CoverMethod( mdOverride[i] );
                                }
                            }
                        }
                    }
                }
            }

            private void Include( TypeRepresentation.GenericContext ctx )
            {
                if(ctx.Template != null)
                {
                    Include( ctx.Template );
                }

                foreach(TypeRepresentation td in ctx.Parameters)
                {
                    Include( td );
                }

                if(ctx.ParametersDefinition != null)
                {
                    foreach(GenericParameterDefinition def in ctx.ParametersDefinition)
                    {
                        foreach(TypeRepresentation td in def.Constraints)
                        {
                            Include( td );
                        }
                    }
                }
            }

            private void Include( FieldRepresentation fd )
            {
                CHECKS.ASSERT( m_reachabilitySet.Contains( fd.OwnerType ), "ComputeCallsClosure.GlobalReachabilitySet does not contain '{0}', although it should", fd.OwnerType );

                CoverField( fd );
            }

            private void Include( MethodRepresentation md )
            {
                CHECKS.ASSERT( m_owner.m_typeSystem.IsUseProhibited( md ) == false, "Found use of '{0}' after the entity was marked illegal to use", md );
                CHECKS.ASSERT( m_owner.m_pendingHistory.Contains( md ), "ComputeCallsClosure.EntitiesReferencedByMethod does not contain '{0}', although it should", md );
                CHECKS.ASSERT( m_reachabilitySet.Contains( md.OwnerType ), "ComputeCallsClosure.GlobalReachabilitySet does not contain '{0}', although it should", md.OwnerType );

                CoverMethod( md );

                if(md is VirtualMethodRepresentation && !(md is FinalMethodRepresentation))
                {
                    //
                    // Include all the overrides for this method in all the subclasses already reached by the closure computation.
                    //
                    int                index = md.FindVirtualTableIndex();
                    TypeRepresentation td    = md.OwnerType;

                    List< TypeRepresentation > lst;
                    if(m_owner.m_typeSystem.DirectDescendant.TryGetValue( td, out lst ))
                    {
                        foreach(TypeRepresentation td2 in lst)
                        {
                            MethodRepresentation md2 = td2.MethodTable[index];

                            if(m_reachabilitySet.Contains( md2.OwnerType ))
                            {
                                CoverMethod( md2 );
                            }
                        }
                    }

                    //
                    // A virtual method defines a slot when it's first introduced.
                    // Make sure all the methods from this one up to the root of the override tree are included.
                    //
                    MethodRepresentation mdRoot = md;
                    while((mdRoot.Flags & MethodRepresentation.Attributes.NewSlot) == 0)
                    {
                        td = td.Extends; if(td == null) break;

                        mdRoot = td.MethodTable[index];

                        if(m_reachabilitySet.Contains( mdRoot ) == false)
                        {
                            CoverMethod( mdRoot );
                        }
                    }


                    //--//

                    //
                    // The method belongs to an interface, we have to include all the implementations in all the touched types.
                    //
                    if(td is InterfaceTypeRepresentation)
                    {
                        InterfaceTypeRepresentation itf      = (InterfaceTypeRepresentation)td;
                        int                         itfIndex = md.FindInterfaceTableIndex();

                        foreach(TypeRepresentation td2 in m_owner.m_typeSystem.InterfaceImplementors[itf])
                        {
                            if(m_reachabilitySet.Contains( td2 ))
                            {
                                MethodRepresentation mdOverride = td2.FindInterfaceTable( itf )[itfIndex];

                                if(m_reachabilitySet.Contains( mdOverride ) == false)
                                {
                                    CoverMethod( mdOverride );
                                }
                            }
                        }
                    }
                }
            }

            private void Include( MethodRepresentation.GenericContext ctx )
            {
                Include( ctx.Template );

                foreach(TypeRepresentation td in ctx.Parameters)
                {
                    Include( td );
                }

                if(ctx.ParametersDefinition != null)
                {
                    foreach(GenericParameterDefinition def in ctx.ParametersDefinition)
                    {
                        foreach(TypeRepresentation td in def.Constraints)
                        {
                            Include( td );
                        }
                    }
                }
            }
            
            private void Include( CustomAttributeRepresentation ca )
            {
                CHECKS.ASSERT( m_owner.m_typeSystem.IsUseProhibited( ca ) == false, "Found use of '{0}' after the entity was marked illegal to use", ca );

                CoverCustomAttribute( ca );
            }

            private void Include( VTable vTable )
            {
                Include( vTable.TypeInfo );
            }

            private void Include( DataManager.DataDescriptor dd )
            {
                lock(m_owner.m_delayedExpansion)
                {
                    m_owner.m_delayedExpansion.Insert( dd );
                }
            }

            internal bool Contains( object obj )
            {
                return m_reachabilitySet.Contains( obj );
            }

            //--//

            //
            // Access Methods
            //

            public TypeSystemForCodeTransformation TypeSystem
            {
                get
                {
                    return m_owner.m_typeSystem;
                }
            }

            internal TypeSystem.Reachability ReachabilitySet
            {
                get
                {
                    return m_reachabilitySet;
                }
            }
        }

        //
        // State
        //

        private readonly TypeSystemForCodeTransformation                                  m_typeSystem;
        private readonly CallsDataBase                                                    m_callsDatabase;
        private readonly PhaseDriver                                                      m_phase;
        private readonly GrowOnlyHashTable< Type, List< Notification > >                  m_delegation;
        private readonly GrowOnlySet      < DataManager.DataDescriptor >                  m_delayedExpansion;
        private readonly GrowOnlySet      < MethodRepresentation       >                  m_pendingHistory;
        private readonly Queue            < MethodRepresentation       >                  m_pending;
                                                                  
        private readonly Context                                                          m_globalContext;
        private readonly GrowOnlyHashTable< MethodRepresentation, GrowOnlySet< object > > m_entitiesReferencedByMethods;

        //
        // Constructor Methods
        //

        public ComputeCallsClosure( TypeSystemForCodeTransformation typeSystem            ,
                                    DelegationCache                 cache                 ,
                                    CallsDataBase                   callsDatabase         ,
                                    PhaseDriver                     phase                 ,
                                    bool                            fCollectPerMethodInfo )
        {
            m_typeSystem       = typeSystem;
            m_callsDatabase    = callsDatabase;
            m_phase            = phase;
            m_delegation       = HashTableFactory.New                     < Type, List< Notification > >();
            m_delayedExpansion = SetFactory      .NewWithReferenceEquality< DataManager.DataDescriptor >();
            m_pendingHistory   = SetFactory      .NewWithReferenceEquality< MethodRepresentation       >();
            m_pending          = new Queue                                < MethodRepresentation       >();

            m_globalContext    = new Context( this, typeSystem.ReachabilitySet );

            if(fCollectPerMethodInfo)
            {
                m_entitiesReferencedByMethods = HashTableFactory.NewWithReferenceEquality< MethodRepresentation, GrowOnlySet< object > >();
            }

            typeSystem.ReachabilitySet.RestartComputation();

            cache.HookNotifications( typeSystem, this, phase );
        }

        //
        // Helper Methods
        //

        public void Execute( MethodRepresentation mdStart )
        {
            if(m_globalContext.Contains( mdStart ) == false)
            {
                QueueMethodForProcessing( mdStart );

                ProcessInner();
            }
        }
        
        public void Expand( object val )
        {
            m_globalContext.CoverObject( val );

            ProcessInner();
        }

        public void ExpandContents< TKey, TValue >( GrowOnlyHashTable< TKey, TValue > ht )
        {
            foreach(TKey key in ht.Keys)
            {
                Expand( key     );
                Expand( ht[key] );
            }
        }

        public void ExpandValueIfKeyIsReachable< TKey, TValue >( GrowOnlyHashTable< TKey, TValue > ht )
        {
            foreach(TKey key in ht.Keys)
            {
                if(m_globalContext.Contains( key ))
                {
                    Expand( ht[key] );
                }
            }
        }

        //--//

        private void QueueMethodForProcessing( MethodRepresentation md )
        {
            lock(m_pendingHistory)
            {
                if(m_pendingHistory.Insert( md ) == true)
                {
                    return;
                }
            }

            lock(m_pending)
            {
                m_pending.Enqueue( md );
            }
        }

        private void ProcessInner()
        {
            while(IsThereWorkToDo())
            {
                ProcessMethods();

                ProcessDataDescriptors();
            }
        }

        private bool IsThereWorkToDo()
        {
            if(m_pending.Count > 0)
            {
                return true;
            }

            if(m_globalContext.ReachabilitySet.HasPendingItems())
            {
                return true;
            }

            return false;
        }

        private void Analyze( MethodRepresentation md )
        {
            m_globalContext.ProcessMethod( md );

            if(m_entitiesReferencedByMethods != null)
            {
                GrowOnlySet< object > entitiesReferencedByMethod;

                lock(m_entitiesReferencedByMethods)
                {
                    if(m_entitiesReferencedByMethods.ContainsKey( md )) return;

                    entitiesReferencedByMethod = SetFactory.NewWithWeakEquality< object >();

                    m_entitiesReferencedByMethods[md] = entitiesReferencedByMethod;
                }

                var currentMethodReachabilitySet = TypeSystemForCodeTransformation.Reachability.CreateForUpdate( entitiesReferencedByMethod );

                Context ctx = new Context( this, currentMethodReachabilitySet );

                ctx.ProcessMethod( md );
            }
        }

        private void ProcessMethods()
        {
            using(ParallelTransformationsHandler handler = new ParallelTransformationsHandler( Analyze ))
            {
                while(true)
                {
                    DrainIncrementalSet();
                    if(m_pending.Count == 0)
                    {
                        break;
                    }

                    var methods = m_pending.ToArray();
                    m_pending.Clear();

#if COMPUTECALLSCLOSURE_FORCE_SINGLE_THREADED
                    foreach(var md in methods)
                    {
                        Analyze( md );
                    }
#else
                    foreach(var md in methods)
                    {
                        handler.Queue( md );
                    }

                    handler.Synchronize();
#endif
                }
            }
        }

        private void DrainIncrementalSet()
        {
            while(true)
            {
                object[] set = m_globalContext.ReachabilitySet.ApplyPendingSet( true );

                if(set == null)
                {
                    break;
                }

                foreach(object obj in set)
                {
                    m_globalContext.IncludeGeneric( obj );
                }
            }
        }

        private void ProcessDataDescriptors()
        {
            TypeSystem.Reachability reachability = m_globalContext.ReachabilitySet.CloneForIncrementalUpdate();

            foreach(DataManager.DataDescriptor dd in m_delayedExpansion)
            {
                if(reachability.Contains( dd ))
                {
                    dd.IncludeExtraTypes( reachability, m_phase );
                }
            }

            object[] set = reachability.ApplyPendingSet( false );
            if(set != null)
            {
                foreach(object obj in set)
                {
                    m_globalContext.CoverObject( obj );
                }
            }
        }

        //--//

        public void RegisterForNotification( Type         type         ,
                                             Notification notification )
        {
            HashTableWithListFactory.AddUnique( m_delegation, type, notification );
        }

        //--//

        //
        // Access Methods
        //

        public TypeSystemForCodeTransformation TypeSystem
        {
            get
            {
                return m_typeSystem;
            }
        }

        public GrowOnlyHashTable< MethodRepresentation, GrowOnlySet< object > > EntitiesReferencedByMethods
        {
            get
            {
                return m_entitiesReferencedByMethods;
            }
        }
    }
}
