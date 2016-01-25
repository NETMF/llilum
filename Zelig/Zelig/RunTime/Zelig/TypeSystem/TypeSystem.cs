//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//#define TYPESYSTEM_REPORT
//#define TYPESYSTEM_REPORT_VERBOSE
////#define TYPESYSTEM_REDUCE_FOOTPRINT

namespace Microsoft.Zelig.Runtime.TypeSystem
{
    using System;
    using System.Collections.Generic;

    using System.IO;
    using System.Runtime.CompilerServices;

    using Microsoft.Zelig.MetaData.Normalized;


    public delegate void FieldEnumerationCallback( FieldRepresentation fd );

    public delegate void MethodEnumerationCallback( MethodRepresentation md );

    public class TypeSystem
    {
        public delegate void NotificationOfAttributeOnGeneric( ref bool fKeep, CustomAttributeRepresentation ca, BaseRepresentation   owner                 );
        public delegate void NotificationOfAttributeOnType   ( ref bool fKeep, CustomAttributeRepresentation ca, TypeRepresentation   owner                 );
        public delegate void NotificationOfAttributeOnField  ( ref bool fKeep, CustomAttributeRepresentation ca, FieldRepresentation  owner                 );
        public delegate void NotificationOfAttributeOnMethod ( ref bool fKeep, CustomAttributeRepresentation ca, MethodRepresentation owner                 );
        public delegate void NotificationOfAttributeOnParam  ( ref bool fKeep, CustomAttributeRepresentation ca, MethodRepresentation owner, int paramIndex );

        public delegate void NotificationOfNewAssembly( AssemblyRepresentation asml );
        public delegate void NotificationOfNewType    ( TypeRepresentation     td   );
        public delegate void NotificationOfNewField   ( FieldRepresentation    fd   );
        public delegate void NotificationOfNewMethod  ( MethodRepresentation   md   );

        public delegate bool TypeLayoutCallback( TypeRepresentation td, GrowOnlySet< TypeRepresentation > history );

        //--//

        public class Reachability
        {
            //
            // State
            //

            GrowOnlySet< object > m_prohibited;
            GrowOnlySet< object > m_included;
            GrowOnlySet< object > m_pending;

            //
            // Constructor Methods
            //

            public Reachability()
            {
                m_prohibited = SetFactory.NewWithWeakEquality< object >();
                m_included   = SetFactory.NewWithWeakEquality< object >();
                m_pending    = SetFactory.NewWithWeakEquality< object >();
            }

            private Reachability( GrowOnlySet< object > prohibited ,
                                  GrowOnlySet< object > included   ,
                                  GrowOnlySet< object > pending    )
            {
                m_prohibited = prohibited;
                m_included   = included;
                m_pending    = pending;
            }

            //
            // Helper Methods
            //

            public void ApplyTransformation( TransformationContext context )
            {
                context.Push( this );

                context.Transform( ref m_prohibited );

                context.Pop();
            }

            public Reachability CloneForProhibitionEstimation()
            {
                GrowOnlySet< object > prohibited = m_prohibited.Clone();
                GrowOnlySet< object > included   = m_included;
                GrowOnlySet< object > pending    = m_pending;

                return new Reachability( prohibited, included, pending );
            }

            public Reachability CloneForIncrementalUpdate()
            {
                GrowOnlySet< object > prohibited = m_prohibited;
                GrowOnlySet< object > included   = m_included;
                GrowOnlySet< object > pending    = m_pending.Clone();

                return new Reachability( prohibited, included, pending );
            }

            public static Reachability CreateForUpdate( GrowOnlySet< object > set )
            {
                GrowOnlySet< object > prohibited = null;
                GrowOnlySet< object > included   = SetFactory.NewWithWeakEquality< object >();
                GrowOnlySet< object > pending    = set;

                return new Reachability( prohibited, included, pending );
            }

            //--//

            public void RestartComputation()
            {
                m_included.Clear();
                m_pending .Clear();
            }

            public bool HasPendingItems()
            {
                return m_pending.Count > 0;
            }

            public object[] ApplyPendingSet( bool fUpdateIncluded )
            {
                object[] res;

                lock(m_pending)
                {
                    if(m_pending.Count == 0)
                    {
                        return null;
                    }

                    res = m_pending.ToArray();

                    m_pending.Clear();
                }

                if(fUpdateIncluded)
                {
                    lock(m_included)
                    {
                        foreach(object obj in res)
                        {
                            m_included.Insert( obj );
                        }
                    }
                }

                return res;
            }

            //--//

            public bool Contains( object obj )
            {
                CHECKS.ASSERT( obj != null, "Cannot use a null reference in Reachability.Contains calls" );

                lock(m_included)
                {
                    return m_included.Contains( obj );
                }
            }

            public bool PendingContains( object obj )
            {
                CHECKS.ASSERT( obj != null, "Cannot use a null reference in Reachability.PendingContains calls" );

                lock(m_pending)
                {
                    return m_pending.Contains( obj );
                }
            }

            public bool IsProhibited( object obj )
            {
                CHECKS.ASSERT( obj != null, "Cannot use a null reference in Reachability.IsProhibited calls" );

                if(m_prohibited == null)
                {
                    return false;
                }

                lock(m_prohibited)
                {
                    return m_prohibited.Contains( obj );
                }
            }

            //--//

            public bool ExpandProhibition( object obj )
            {
                CHECKS.ASSERT( obj != null, "Cannot use a null reference in Reachability.ExpandProhibition calls" );

                lock(m_prohibited)
                {
                    return m_prohibited.Insert( obj );
                }
            }

            public bool ExpandPending( object obj )
            {
                CHECKS.ASSERT( obj != null, "Cannot use a null reference in Reachability.ExpandPending calls" );

                if(m_included != null)
                {
                    // is the lock needed?
                    lock(m_included)
                    {
                        if(m_included.Contains( obj ))
                        {
                            return true;
                        }
                    }
                }

                lock(m_pending)
                {
                    if(m_pending.Insert( obj ))
                    {
                        return true;
                    }

                    return false;
                }
            }

            //
            // Access Methods
            //

            public GrowOnlySet< object > ProhibitedSet
            {
                get
                {
                    return m_prohibited;
                }
            }

            public GrowOnlySet< object > IncludedSet
            {
                get
                {
                    return m_included;
                }
            }
        }

        //--//

        protected class PendingAnalysis<T>
        {
            public delegate void Invoke( T target, ref ConversionContext context );

            //
            // State
            //

            internal PendingAnalysis<T> m_next;
            internal Invoke             m_dlg;
            public   T                  m_target;
            public   ConversionContext  m_context;

            //--//

            //
            // Debug Methods
            //

            public override string ToString()
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                sb.Append( m_target );

                m_context.InnerToString( sb );

                return sb.ToString();
            }
        }


        protected class PendingAnalysisQueue<T>
        {
            //
            // State
            //

            internal PendingAnalysis<T> m_first;
            internal PendingAnalysis<T> m_last;

            //--//

            public int Count
            {
                get
                {
                    PendingAnalysis<T> pa = m_first;

                    if(pa == null)
                    {
                        return 0;
                    }
                    
                    int count = 1;

                    while(pa.m_next != null)
                    {
                        pa = pa.m_next;
                        count++;
                    }

                    return count;
                }
            }

            public void Schedule(     T                         target  ,
                                  ref ConversionContext         context ,
                                      PendingAnalysis<T>.Invoke dlg     )
            {
                PendingAnalysis<T> pa = new PendingAnalysis<T>();

                if(m_first == null)
                {
                    m_first = pa;
                }

                if(m_last != null)
                {
                    m_last.m_next = pa;
                }

                m_last = pa;

                pa.m_target  = target;
                pa.m_context = context;
                pa.m_dlg     = dlg;
            }

            public bool Dispatch()
            {
                PendingAnalysis<T> pa = m_first;

                if(pa == null)
                {
                    return false;
                }

                m_first = pa.m_next;

                if(m_last == pa)
                {
                    m_last = null;
                }

                pa.m_dlg( pa.m_target, ref pa.m_context );

                return true;
            }
        }


        internal struct Lookup<T> : IEquatable< Lookup<T> > where T : MetaDataObject
        {
            //
            // State
            //

            internal T                 m_metadata;
            internal ConversionContext m_context;

            //
            // MetaDataEquality Methods
            //

            public override bool Equals( object obj )
            {
                if(obj is Lookup<T>)
                {
                    return Equals( (Lookup<T>)obj );
                }

                return false;
            }

            public bool Equals( Lookup<T> other )
            {
                if(m_metadata.Equals      (     other.m_metadata ) &&
                   m_context .SameContents( ref other.m_context  )  )
                {
                    return true;
                }

                return false;
            }

            public override int GetHashCode()
            {
                int result = m_metadata.GetHashCode();

                foreach(TypeRepresentation td in m_context.m_typeContext)
                {
                    result ^= td.GetHashCode();
                }

                foreach(TypeRepresentation td in m_context.m_methodContext)
                {
                    result ^= td.GetHashCode();
                }

                return result;
            }

            //--//

            //
            // Debug Methods
            //

            public override string ToString()
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                if(m_metadata is MetaDataMethodBase)
                {
                    MetaDataMethodBase md = (MetaDataMethodBase)(object)m_metadata;

                    md.PrettyPrintSignature( sb );
                }
                else
                {
                    sb.Append( m_metadata );
                }

                m_context.InnerToString( sb );

                return sb.ToString();
            }

            internal void Set(     T                 metadata ,
                               ref ConversionContext context  )
            {
                m_metadata = metadata;
                m_context  = context;

                if(metadata.UsesTypeParameters == false)
                {
                    m_context.m_typeContextOwner = null;
                    m_context.m_typeContext      = TypeRepresentation.SharedEmptyArray;
                }

                if(metadata.UsesMethodParameters == false)
                {
                    m_context.m_methodContextOwner = null;
                    m_context.m_methodContext      = TypeRepresentation.SharedEmptyArray;
                }
            }

            internal void Set( T                  metadata ,
                               TypeRepresentation owner    )
            {
                m_metadata = metadata;
                m_context.SetContextAsType( owner );
            }
        }

        protected class CustomAttributeNotification
        {
            //
            // State
            //

            internal NotificationOfAttributeOnGeneric m_notificationOfAttributeOnGeneric;
            internal NotificationOfAttributeOnType    m_notificationOfAttributeOnType;
            internal NotificationOfAttributeOnField   m_notificationOfAttributeOnField;
            internal NotificationOfAttributeOnMethod  m_notificationOfAttributeOnMethod;
            internal NotificationOfAttributeOnParam   m_notificationOfAttributeOnParam;
        }

        //
        // State
        //

        private   MethodRepresentation                                                                            m_applicationEntryPoint;
        private   List                < AssemblyRepresentation                                                  > m_assemblies;
        private   List                < TypeRepresentation                                                      > m_types;
        private   List                < ResourceRepresentation                                                  > m_resources;
        private   List                < MethodRepresentation                                                    > m_exportedMethods;
        private   GrowOnlyHashTable   < FieldRepresentation, object                                             > m_defaultValues;
        private   InstantiationContext                                                                            m_instantiationContext;
        private   AssemblyRepresentation                                                                          m_required_mscorlib;
        private   WellKnownTypes                                                                                  m_wellKnownTypes;
        private   WellKnownMethods                                                                                m_wellKnownMethods;
        private   WellKnownFields                                                                                 m_wellKnownFields;
        private   GrowOnlyHashTable< string, TypeRepresentation   >                                               m_wellKnownTypesByName;
        private   GrowOnlyHashTable< string, MethodRepresentation >                                               m_wellKnownMethodsByName;
        private   GrowOnlyHashTable< string, FieldRepresentation  >                                               m_wellKnownFieldsByName;

        //
        // These fields are used only during the MetaData import phase.
        //
        protected IEnvironmentProvider                                                                            m_environmentProvider;

        private   GrowOnlyHashTable   <         MetaDataAssembly                , AssemblyRepresentation        > m_lookupAssemblies;
        private   GrowOnlyHashTable   < Lookup< MetaDataTypeDefinitionAbstract >, TypeRepresentation            > m_lookupTypes;
        private   GrowOnlyHashTable   < Lookup< MetaDataField                  >, FieldRepresentation           > m_lookupFields;
        private   GrowOnlyHashTable   < Lookup< MetaDataMethodAbstract         >, MethodRepresentation          > m_lookupMethods;
        private   GrowOnlyHashTable   < Lookup< MetaDataCustomAttribute        >, CustomAttributeRepresentation > m_lookupAttributes;
        private   GrowOnlyHashTable   < Lookup< MetaDataManifestResource       >, ResourceRepresentation        > m_lookupResources;
               
        private   GrowOnlyHashTable   < AssemblyRepresentation, MetaDataAssembly                                > m_reverseLookupAssemblies;
        private   GrowOnlyHashTable   < TypeRepresentation    , Lookup< MetaDataTypeDefinitionAbstract >        > m_reverseLookupTypes;
        private   GrowOnlyHashTable   < FieldRepresentation   , Lookup< MetaDataField                  >        > m_reverseLookupFields;
        private   GrowOnlyHashTable   < MethodRepresentation  , Lookup< MetaDataMethodAbstract         >        > m_reverseLookupMethods;

        private   NotificationOfNewAssembly                                                                       m_notificationOfNewAssembly;
        private   NotificationOfNewType                                                                           m_notificationOfNewType;
        private   NotificationOfNewField                                                                          m_notificationOfNewField;
        private   NotificationOfNewMethod                                                                         m_notificationOfNewMethod;
               
        private   GrowOnlyHashTable   < TypeRepresentation    , CustomAttributeNotification                     > m_customAttributeNotification;
        private   GrowOnlyHashTable   < TypeRepresentation    , TypeLayoutCallback                              > m_layoutDelegation;
               
        private   PendingAnalysisQueue< TypeRepresentation                                                      > m_pendingTypeAnalysis;
        private   PendingAnalysisQueue< MethodRepresentation                                                    > m_pendingMethodAnalysis;
        private   PendingAnalysisQueue< TypeRepresentation                                                      > m_pendingTypeCustomAttributeAnalysis;
        private   PendingAnalysisQueue< MethodRepresentation                                                    > m_pendingMethodCustomAttributeAnalysis;
               
        private   GenericInstantiationClosure                                                                     m_genericRecursionCheck;

        //
        // Constructor Methods
        //

        protected TypeSystem( IEnvironmentProvider env )
        {
            m_environmentProvider    = env;

            m_assemblies             = new List            < AssemblyRepresentation      >();
            m_types                  = new List            < TypeRepresentation          >();
            m_resources              = new List            < ResourceRepresentation      >();
            m_exportedMethods        = new List            < MethodRepresentation        >();
                                  
            m_defaultValues          = HashTableFactory.New< FieldRepresentation, object >();
                                  
            m_instantiationContext   = new InstantiationContext();
                                  
            m_wellKnownTypes         = new WellKnownTypes  ();
            m_wellKnownMethods       = new WellKnownMethods();
            m_wellKnownFields        = new WellKnownFields ();
            m_wellKnownTypesByName   = HashTableFactory.New< string, TypeRepresentation   >();
            m_wellKnownMethodsByName = HashTableFactory.New< string, MethodRepresentation >();
            m_wellKnownFieldsByName  = HashTableFactory.New< string, FieldRepresentation  >();
        }

        //
        // Helper Methods
        //

        public void InitializeForCompilation()
        {
            InitializeForMetaDataImport();

            BootstrapMetaData();

            RegisterMetaDataNotifications();
        }

        protected virtual void InitializeForMetaDataImport()
        {
            m_lookupAssemblies                     = HashTableFactory.New<         MetaDataAssembly                , AssemblyRepresentation        >();
            m_lookupTypes                          = HashTableFactory.New< Lookup< MetaDataTypeDefinitionAbstract >, TypeRepresentation            >();
            m_lookupFields                         = HashTableFactory.New< Lookup< MetaDataField                  >, FieldRepresentation           >();
            m_lookupMethods                        = HashTableFactory.New< Lookup< MetaDataMethodAbstract         >, MethodRepresentation          >();
            m_lookupAttributes                     = HashTableFactory.New< Lookup< MetaDataCustomAttribute        >, CustomAttributeRepresentation >();
            m_lookupResources                      = HashTableFactory.New< Lookup< MetaDataManifestResource       >, ResourceRepresentation        >();

            //--//

            m_reverseLookupAssemblies              = HashTableFactory.New< AssemblyRepresentation, MetaDataAssembly                         >();
            m_reverseLookupTypes                   = HashTableFactory.New< TypeRepresentation    , Lookup< MetaDataTypeDefinitionAbstract > >();
            m_reverseLookupFields                  = HashTableFactory.New< FieldRepresentation   , Lookup< MetaDataField                  > >();
            m_reverseLookupMethods                 = HashTableFactory.New< MethodRepresentation  , Lookup< MetaDataMethodAbstract         > >();

            //--//

            m_customAttributeNotification          = HashTableFactory.New< TypeRepresentation, CustomAttributeNotification >();
            m_layoutDelegation                     = HashTableFactory.New< TypeRepresentation, TypeLayoutCallback          >();

            m_pendingTypeAnalysis                  = new PendingAnalysisQueue< TypeRepresentation   >();
            m_pendingMethodAnalysis                = new PendingAnalysisQueue< MethodRepresentation >();
            m_pendingTypeCustomAttributeAnalysis   = new PendingAnalysisQueue< TypeRepresentation   >();
            m_pendingMethodCustomAttributeAnalysis = new PendingAnalysisQueue< MethodRepresentation >();

            m_genericRecursionCheck                = new GenericInstantiationClosure();
        }


        protected virtual void RegisterMetaDataNotifications()
        {
        }

        protected virtual void CleanupAfterMetaDataImport()
        {
            m_lookupAssemblies                     = null;
            m_lookupTypes                          = null;
            m_lookupFields                         = null;
            m_lookupMethods                        = null;
            m_lookupAttributes                     = null;
            m_lookupResources                      = null;

            //--//

            m_reverseLookupAssemblies              = null;
            m_reverseLookupTypes                   = null;
            m_reverseLookupFields                  = null;
            m_reverseLookupMethods                 = null;

            //--//

            m_customAttributeNotification          = null;

            m_pendingTypeAnalysis                  = null;
            m_pendingMethodAnalysis                = null;
            m_pendingTypeCustomAttributeAnalysis   = null;
            m_pendingMethodCustomAttributeAnalysis = null;

            m_genericRecursionCheck                = null;
        }

        public virtual void RefreshHashCodesAfterTypeSystemRemapping()
        {
            //
            // We only need to refresh the hash codes for the collections that are not covered by ApplyTransformation.
            //
            m_layoutDelegation.RefreshHashCodes();

            m_instantiationContext.RefreshHashCodes();
        }

        //--//

        private void BootstrapMetaData()
        {
            IMetaDataBootstrap bootstrap = GetEnvironmentService< IMetaDataBootstrap >();

            MetaDataAssembly mscorlib = bootstrap.GetAssembly( "mscorlib" );

            m_required_mscorlib = ConvertToIR( mscorlib );

            PopulateWellKnownTypes( bootstrap );

            m_applicationEntryPoint = ConvertToIRWithoutContext( bootstrap.GetApplicationEntryPoint() );

            if(m_applicationEntryPoint != null)
            {
                m_applicationEntryPoint.BuildTimeFlags |= MethodRepresentation.BuildTimeAttributes.NoInline;
            }
        }

        private void PopulateWellKnownTypes( IMetaDataBootstrap bootstrap )
        {
            foreach(System.Reflection.FieldInfo fi in ReflectionHelper.GetAllPublicInstanceFields( typeof(WellKnownTypes) ))
            {
                foreach(WellKnownTypeLookupAttribute attrib in ReflectionHelper.GetAttributes< WellKnownTypeLookupAttribute >( fi, false ))
                {
                    MetaDataAssembly assm = bootstrap.GetAssembly( attrib.AssemblyName );
                    if(assm != null)
                    {
                        MetaDataTypeDefinitionAbstract td = bootstrap.ResolveType( assm, attrib.Namespace, attrib.Name );

                        TypeRepresentation tdRes = ConvertToIRWithoutContext( td );

                        SetWellKnownType( fi.Name, tdRes );
                    }
                }
            }

            //--//

            IMetaDataBootstrap_FilterTypesByCustomAttributeCallback dlg = delegate( MetaDataTypeDefinitionAbstract td, MetaDataCustomAttribute ca )
            {
                string name = (string)ca.FixedArgs[0];

                TypeRepresentation tdRes    = ConvertToIRWithoutContext( td );
                TypeRepresentation tdResOld = GetWellKnownTypeNoThrow( name );

                if(tdResOld != null && tdResOld != tdRes)
                {
                    throw TypeConsistencyErrorException.Create( "Found the well-known type '{0}' defined more than once: {1} and {2}", name, tdResOld.FullNameWithAbbreviation, tdRes.FullNameWithAbbreviation );
                }

                SetWellKnownType( name, tdRes );
            };

            bootstrap.FilterTypesByCustomAttribute( GetAssociatedMetaData( this.WellKnownTypes.Microsoft_Zelig_Internals_WellKnownTypeAttribute          ), dlg );
            bootstrap.FilterTypesByCustomAttribute( GetAssociatedMetaData( this.WellKnownTypes.Microsoft_Zelig_Runtime_TypeSystem_WellKnownTypeAttribute ), dlg );
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        public virtual void ResolveAll()
        {
            while(true)
            {
                if(m_pendingTypeAnalysis.Dispatch())
                {
                    continue;
                }

                if(m_pendingMethodAnalysis.Dispatch())
                {
                    continue;
                }

                if(m_pendingTypeCustomAttributeAnalysis.Dispatch())
                {
                    continue;
                }

                if(m_pendingMethodCustomAttributeAnalysis.Dispatch())
                {
                    continue;
                }

                break;
            }
        }

        //--//

        protected virtual void PerformDelayedTypeAnalysis(     TypeRepresentation target  ,
                                                           ref ConversionContext  context )
        {
            target.PerformDelayedTypeAnalysis( this, ref context );
        }

        protected virtual void PerformDelayedMethodAnalysis(     MethodRepresentation target  ,
                                                             ref ConversionContext    context )
        {
        }

        protected virtual void PerformDelayedCustomAttributeAnalysis(     TypeRepresentation target  ,
                                                                      ref ConversionContext  context )
        {
            target.PerformDelayedCustomAttributeAnalysis( this, ref context );
        }

        protected virtual void PerformDelayedCustomAttributeAnalysis(     MethodRepresentation target  ,
                                                                      ref ConversionContext    context )
        {
            target.PerformDelayedCustomAttributeAnalysis( this, ref context );
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        internal void NotifyNewAssemblyComplete( AssemblyRepresentation asml )
        {
            var dlg = m_notificationOfNewAssembly;
            if(dlg != null)
            {
                dlg( asml );
            }
        }

        internal void NotifyNewTypeComplete( TypeRepresentation td )
        {
            var dlg = m_notificationOfNewType;
            if(dlg != null)
            {
                dlg( td );
            }
        }

        internal void NotifyNewFieldComplete( FieldRepresentation fd )
        {
            var dlg = m_notificationOfNewField;
            if(dlg != null)
            {
                dlg( fd );
            }
        }

        internal void NotifyNewMethodComplete( MethodRepresentation md )
        {
            var dlg = m_notificationOfNewMethod;
            if(dlg != null)
            {
                dlg( md );
            }
        }

        internal bool NotifyNewCustomAttributeComplete( CustomAttributeAssociationRepresentation caa )
        {
            CustomAttributeRepresentation ca    = caa.CustomAttribute;
            BaseRepresentation            owner = caa.Target;
            TypeRepresentation            caTd  = ca.Constructor.OwnerType;
            CustomAttributeNotification   res;
            bool                          fKeep = true;

            if(m_customAttributeNotification.TryGetValue( caTd, out res ))
            {
                if(res.m_notificationOfAttributeOnGeneric != null)
                {
                    res.m_notificationOfAttributeOnGeneric( ref fKeep, ca, owner );
                }

                if(owner is TypeRepresentation)
                {
                    if(res.m_notificationOfAttributeOnType != null)
                    {
                        res.m_notificationOfAttributeOnType( ref fKeep, ca, (TypeRepresentation)owner );
                    }
                }
                else if(owner is FieldRepresentation)
                {
                    if(res.m_notificationOfAttributeOnField != null)
                    {
                        res.m_notificationOfAttributeOnField( ref fKeep, ca, (FieldRepresentation)owner );
                    }
                }
                else if(owner is MethodRepresentation)
                {
                    if(caa.ParameterIndex == -1)
                    {
                        if(res.m_notificationOfAttributeOnMethod != null)
                        {
                            res.m_notificationOfAttributeOnMethod( ref fKeep, ca, (MethodRepresentation)owner );
                        }
                    }
                    else
                    {
                        if(res.m_notificationOfAttributeOnParam != null)
                        {
                            res.m_notificationOfAttributeOnParam( ref fKeep, ca, (MethodRepresentation)owner, caa.ParameterIndex );
                        }
                    }
                }
            }

            return fKeep;
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        public void InvalidateLayout()
        {
            foreach(TypeRepresentation td in this.Types)
            {
                td.InvalidateLayout();
            }
        }

        public void LayoutTypes( uint memoryAlignment )
        {
            GrowOnlySet< TypeRepresentation > history = SetFactory.NewWithReferenceEquality< TypeRepresentation >();

            foreach(TypeRepresentation td in this.Types)
            {
                EnsureTypeLayout( td, history, memoryAlignment );

                td.BuildGcInfo( this );
            }
        }

        public void EnsureTypeLayout( TypeRepresentation              td              ,
                                      GrowOnlySet<TypeRepresentation> history         ,
                                      uint                            memoryAlignment )
        {
            if(td.ValidLayout)
            {
                return;
            }

            if(history.Insert( td ) == true)
            {
                throw TypeConsistencyErrorException.Create( "Found cyclic dependency for '{0}' while trying to compute its layout", td );
            }

            TypeLayoutCallback callback;

            if(m_layoutDelegation.TryGetValue( td, out callback ))
            {
                if(callback( td, history ))
                {
                    return;
                }
            }

            LayoutType( td, history, memoryAlignment );
        }

        private void LayoutType( TypeRepresentation                td              ,
                                 GrowOnlySet< TypeRepresentation > history         ,
                                 uint                              memoryAlignment )
        {
            if(td.IsOpenType) return;

            if(td is ArrayReferenceTypeRepresentation)
            {
                TypeRepresentation tdElement = td.ContainedType;

                EnsureTypeLayout( tdElement, history, memoryAlignment );

                FieldRepresentation fd = this.WellKnownFields.ArrayImpl_m_numElements;

                if(fd != null)
                {
                    EnsureTypeLayout( fd.FieldType, history, memoryAlignment );

                    td.Size = fd.FieldType.Size;
                }
                else
                {
                    td.Size = 0;
                }

                if(tdElement is ReferenceTypeRepresentation)
                {
                    td.VirtualTable.ElementSize = sizeof(uint);
                }
                else if(tdElement is ScalarTypeRepresentation)
                {
                    td.VirtualTable.ElementSize = tdElement.Size;
                }
                else
                {
                    td.VirtualTable.ElementSize = AddressMath.AlignToBoundary( tdElement.Size, memoryAlignment );
                }
            }
            
            if(td.ValidLayout == false)
            {
                if(td is BoxedValueTypeRepresentation)
                {
                    BoxedValueTypeRepresentation boxed = (BoxedValueTypeRepresentation)td;
                    TypeRepresentation           boxee = boxed.UnderlyingType;

                    EnsureTypeLayout( boxee, history, memoryAlignment );

                    td.Size = boxee.Size;
                }
                else
                {
                    TypeRepresentation.Attributes layout = (td.Flags & TypeRepresentation.Attributes.LayoutMask);
                    uint                          size;
                    uint                          offset;

                    TypeRepresentation tdSuper = td.Extends;
                    if(tdSuper != null)
                    {
                        EnsureTypeLayout( tdSuper, history, memoryAlignment );

                        size = tdSuper.Size;
                    }
                    else
                    {
                        size = 0;
                    }

                    offset = size;

                    foreach(FieldRepresentation fd in td.Fields)
                    {
                        if(fd is InstanceFieldRepresentation)
                        {
                            TypeRepresentation tdField = fd.FieldType;

                            if(tdField is ValueTypeRepresentation)
                            {
                                EnsureTypeLayout( tdField, history, memoryAlignment );
                            }

                            uint fdSize = tdField.SizeOfHoldingVariable;

                            switch(layout)
                            {
                                case TypeRepresentation.Attributes.AutoLayout      :
                                case TypeRepresentation.Attributes.SequentialLayout:
                                    //
                                    // Adjust alignment of field.
                                    //
                                    switch(fdSize)
                                    {
                                        case 1:
                                            break;

                                        case 2:
                                            offset = (offset + 1u) & ~(2u-1u);
                                            break;

                                        case 4:
                                        case 8:
                                            offset = (offset + 3u) & ~(4u-1u);
                                            break;

////                                        // Align on 64 bits.
////                                    case 8:
////                                        offset = (offset + 7u) & ~(8u-1u);
////                                        break;

                                        default:
                                            offset = (offset + 3u) & ~(4u-1u);
                                            break;
                                    }

                                    fd.Offset = (int)offset;

                                    offset += fdSize;

                                    size = offset;
                                    break;

                                case TypeRepresentation.Attributes.ExplicitLayout:
                                    CustomAttributeRepresentation ca = td.FindCustomAttribute( this.WellKnownTypes.System_Runtime_InteropServices_FieldOffsetAttribute );
                                    if(ca != null)
                                    {
                                        fd.Offset = (int)ca.FixedArgsValues[0];
                                    }
    
                                    size = Math.Max( (uint)(fd.Offset + fdSize), size );
                                    break;
                            }
                        }
                    }

                    //fmegen: there is a problem with value types that have a "non-standard" size
                    ValueTypeRepresentation vt = td as ValueTypeRepresentation;
                    if ((null != vt) && ((vt.Flags & TypeRepresentation.Attributes.Abstract) == 0))
                    {
                        // If this is a zero-sized struct, we need to arbitrarily assign it a size of one byte. This
                        // matches the CLR spec and simplifies element offset validation
                        if (size == 0)
                        {
                            size = 1;
                        }

                        //very special case:
                        //a value type must not have a size of 3 bytes since we cannot guarantee
                        //on the ARM platform that we're able to move this in an atomic way.
                        //the arm does only support 1, 2, and 4 bytes store operators
                        //...and, btw, the compiler will fail to generate an opcode for this size ;/
                        if (size == 3)
                        {
                            size = 4;
                        }
                    }

                    td.Size = size;
                }
            }
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        public MetaDataAssembly GetAssociatedMetaData( AssemblyRepresentation asml )
        {
            MetaDataAssembly res;

            return m_reverseLookupAssemblies.TryGetValue( asml, out res ) ? res : null;
        }

        public MetaDataTypeDefinitionAbstract GetAssociatedMetaData( TypeRepresentation td )
        {
            Lookup<MetaDataTypeDefinitionAbstract> res;

            return m_reverseLookupTypes.TryGetValue( td, out res ) ? res.m_metadata : null;
        }

        public MetaDataField GetAssociatedMetaData( FieldRepresentation fd )
        {
            Lookup<MetaDataField> res;

            return m_reverseLookupFields.TryGetValue( fd, out res ) ? res.m_metadata : null;
        }

        public MetaDataMethodBase GetAssociatedMetaData( MethodRepresentation md )
        {
            Lookup<MetaDataMethodAbstract> res;

            return m_reverseLookupMethods.TryGetValue( md, out res ) ? (MetaDataMethodBase)res.m_metadata : null;
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        public void ImportAssembly( MetaDataAssembly asml )
        {
            foreach(MetaDataTypeDefinitionAbstract td in asml.Types)
            {
                //
                // This skips both System.Object (which is already included in the type system bootstrap) and <Module> (which is not supported). 
                //
                if(td.Extends != null)
                {
                    ConvertToIRWithoutContext( td );
                }
            }

            foreach(MetaDataManifestResource res in asml.Resources)
            {
                ConvertToIRWithoutContext( res );
            }
        }

        public AssemblyRepresentation ConvertToIR( MetaDataAssembly asml )
        {
            AssemblyRepresentation asmlIR;

            if(m_lookupAssemblies.TryGetValue( asml, out asmlIR ) == false)
            {
                AssemblyRepresentation.VersionData dstVersion;
                MetaData.MetaDataVersion           srcVersion = asml.Version;

                dstVersion.MajorVersion   =                                                   srcVersion.MajorVersion;
                dstVersion.MinorVersion   =                                                   srcVersion.MinorVersion;
                dstVersion.BuildNumber    =                                                   srcVersion.BuildNumber;
                dstVersion.RevisionNumber =                                                   srcVersion.RevisionNumber;
                dstVersion.Flags          = (AssemblyRepresentation.VersionData.AssemblyFlags)srcVersion.Flags;
                dstVersion.PublicKey      =                                                   srcVersion.PublicKey;

                asmlIR = new AssemblyRepresentation( asml.Name, ref dstVersion );

                m_lookupAssemblies[asml] = asmlIR;

                m_reverseLookupAssemblies[asmlIR] = asml;

                m_assemblies.Add( asmlIR );

                NotifyNewAssemblyComplete( asmlIR );
            }

            return asmlIR;
        }

        //--//

        public TypeRepresentation ConvertToIRWithoutContext( MetaDataTypeDefinitionAbstract td )
        {
            ConversionContext context = new ConversionContext(); context.Initialize();

            return ConvertToIR( td, context );
        }

        public TypeRepresentation GetDefiningType(     MetaDataField     fd      ,
                                                   ref ConversionContext context )
        {
            bool fInstantiateGeneric = (context.m_typeContextOwner != null);

            return ConvertToIR( fd.Owner, context, fInstantiateGeneric );
        }

        public TypeRepresentation GetDefiningType(     MetaDataMethodBase md      ,
                                                   ref ConversionContext  context )
        {
            bool fInstantiateGeneric = (context.m_typeContextOwner != null);

            return ConvertToIR( md.Owner, context, fInstantiateGeneric );
        }

        public TypeRepresentation ConvertToIR( SignatureType     td      ,
                                               ConversionContext context )
        {
            return InnerConvertToIR( td.Type, ref context, false );
        }

        public TypeRepresentation ConvertToIR( MetaDataTypeDefinitionAbstract td      ,
                                               ConversionContext              context )
        {
            return InnerConvertToIR( td, ref context, false );
        }

        public TypeRepresentation ConvertToIR( MetaDataTypeDefinitionAbstract td                  ,
                                               ConversionContext              context             ,
                                               bool                           fInstantiateGeneric )
        {
            return InnerConvertToIR( td, ref context, fInstantiateGeneric );
        }

        private TypeRepresentation InnerConvertToIR(     MetaDataTypeDefinitionAbstract td                  ,
                                                     ref ConversionContext              context             ,
                                                         bool                           fInstantiateGeneric )
        {
            if(td == null)
            {
                return null;
            }

            //--//

            if(ComputeMaxGenericInstantiationDepth( ref context ) > 10)
            {
                throw TypeConsistencyErrorException.Create( "Found generic instantiation {0} levels deep. Is this an infinite generic recursion? {1}", ComputeMaxGenericInstantiationDepth( ref context ), context );
            }

            td = FlattenTypeDefinition( td, ref context, fInstantiateGeneric );

            //--//

            TypeRepresentation tdIR;
            bool               fAllocateDelayedTypeParameter;
            bool               fAllocateDelayedMethodParameter;

            tdIR = ResolveDelayedParameter( td, ref context, out fAllocateDelayedTypeParameter, out fAllocateDelayedMethodParameter );
            if(tdIR != null)
            {
                return tdIR;
            }

            Lookup<MetaDataTypeDefinitionAbstract> lookup = new Lookup<MetaDataTypeDefinitionAbstract>();
            lookup.Set( td, ref context );

            if(m_lookupTypes.TryGetValue( lookup, out tdIR ))
            {
                if(tdIR == null)
                {
                    throw TypeConsistencyErrorException.Create( "Detected recursive resolution attempt for {0}", td );
                }

                ReportVerbose( "Resolved Type {0} => {1}", lookup, tdIR );

                return tdIR;
            }

            //--//

            //
            // Create GenericContext, with a pointer to the template Generic and the parameter types.
            //
            TypeRepresentation.GenericContext genericContext;

            if(td is MetaDataTypeDefinitionGeneric)
            {
                TypeRepresentation[] genericParameters = context.m_typeContext;

                if(genericParameters.Length > 0)
                {
                    TypeRepresentation tdIRTemplate = ConvertToIRWithoutContext( td );

                    genericContext = new TypeRepresentation.GenericContext( tdIRTemplate, genericParameters );
                }
                else
                {
                    genericContext = new TypeRepresentation.GenericContext( null, genericParameters );
                }
            }
            else
            {
                genericContext = null;
            }

            Report( "Converting Type {0}", lookup );

            AssemblyRepresentation asmlIR = ConvertToIR( td.Owner );

            //--//

            if(fAllocateDelayedTypeParameter)
            {
                MetaDataTypeDefinitionDelayed td2 = (MetaDataTypeDefinitionDelayed)td;

                return RegisterAndAnalyze( ref lookup, new DelayedTypeParameterTypeRepresentation( asmlIR, context.m_typeContextOwner, td2.ParameterNumber ) );
            }

            if(fAllocateDelayedMethodParameter)
            {
                MetaDataTypeDefinitionDelayed td2 = (MetaDataTypeDefinitionDelayed)td;

                return RegisterAndAnalyze( ref lookup, new DelayedMethodParameterTypeRepresentation( asmlIR, context.m_methodContextOwner, td2.ParameterNumber ) );
            }

            if(td is MetaDataTypeDefinitionGeneric)
            {
                MetaDataTypeDefinitionGeneric td2 = (MetaDataTypeDefinitionGeneric)td;

                if(m_genericRecursionCheck.VerifyFiniteInstantiationOfGenericType( td2 ) == false)
                {
                    throw TypeConsistencyErrorException.Create( "Detected recursive generic declaration starting from {0}", td );
                }
            }

            m_lookupTypes[lookup] = null;

            //--//

            if(td is MetaDataTypeDefinitionBase)
            {
                MetaDataTypeDefinitionBase      td2         = (MetaDataTypeDefinitionBase     )td;
                MetaDataTypeDefinitionAbstract  tdExtends   =                                  td.Extends;
                TypeRepresentation.BuiltInTypes builtinType = (TypeRepresentation.BuiltInTypes)td2.ElementType;
                TypeRepresentation.Attributes   flags       = (TypeRepresentation.Attributes  )td2.Flags;

                if(tdExtends == null)
                {
                    if((td2.Flags & MetaData.TypeAttributes.ClassSemanticsMask) == MetaData.TypeAttributes.Interface)
                    {
                        return RegisterAndAnalyze( ref lookup, new InterfaceTypeRepresentation( asmlIR, builtinType, flags, genericContext ) );
                    }
                    else
                    {
                        if(IsObjectClass( td2 ) == false)
                        {
                            throw FeatureNotSupportedException.Create( "'{0}' doesn't inherit from System.Object, multiple type system roots not supported!", td2.FullName );
                        }

                        return RegisterAndAnalyze( ref lookup, new ConcreteReferenceTypeRepresentation( asmlIR, builtinType, flags, genericContext ) );
                    }
                }
                else if(tdExtends is MetaDataTypeDefinitionBase && IsValueType( (MetaDataTypeDefinitionBase)tdExtends ))
                {
                    if(td2.IsScalar)
                    {
                        return RegisterAndAnalyze( ref lookup, new ScalarTypeRepresentation( asmlIR, builtinType, flags, genericContext, td2.ScalarSize ) );
                    }
                    else if(IsEnumType( td2 ))
                    {
                        // Special case: System.Enum extends System.ValueType, but is not itself a value type.
                        return RegisterAndAnalyze( ref lookup, new ConcreteReferenceTypeRepresentation( asmlIR, builtinType, flags, genericContext ) );
                    }
                    else
                    {
                        TypeRepresentation valueType = RegisterAndAnalyze( ref lookup, new ValueTypeRepresentation( asmlIR, builtinType, flags, genericContext ) );

                        // For value types, it's extremely likely we'll want to create a managed pointer to the type.
                        // This is usually done when adding a method to the type. Rather than dirtying the type system
                        // later, we create the managed pointer type in advance.
                        CreateManagedPointerToType( valueType );

                        return valueType;
                    }
                }
                else if(tdExtends is MetaDataTypeDefinitionBase && IsEnumType( (MetaDataTypeDefinitionBase)tdExtends ))
                {
                    return RegisterAndAnalyze( ref lookup, new EnumerationTypeRepresentation( asmlIR, builtinType, flags, genericContext, td2.ScalarSize ) );
                }
                else
                {
                    if((td2.Flags & MetaData.TypeAttributes.RTSpecialName) != 0)
                    {
                        throw TypeConsistencyErrorException.Create( "Unexpected special name type '{0}'", td2 );
                    }
                    else if((td2.Flags & MetaData.TypeAttributes.Abstract) != 0)
                    {
                        return RegisterAndAnalyze( ref lookup, new AbstractReferenceTypeRepresentation( asmlIR, builtinType, flags, genericContext ) );
                    }
                    else
                    {
                        return RegisterAndAnalyze( ref lookup, new ConcreteReferenceTypeRepresentation( asmlIR, builtinType, flags, genericContext ) );
                    }
                }
            }
            else if(td is MetaDataTypeDefinitionDelayed)
            {
                throw TypeConsistencyErrorException.Create( "Invalid context [{0}] for delayed parameter {1}", context, td );
            }
            else if(td is MetaDataTypeDefinitionArray)
            {
                MetaDataTypeDefinitionArray td2 = (MetaDataTypeDefinitionArray)td;

                TypeRepresentation elementType = ConvertToIR( td2.ObjectType, context );

                //
                // Make sure there's a managed pointer to the elements of each array.
                //
                CreateManagedPointerToType( elementType );

                if(td2 is MetaDataTypeDefinitionArraySz)
                {
                    MetaDataTypeDefinitionArraySz td3 = (MetaDataTypeDefinitionArraySz)td2;

                    return RegisterAndAnalyze( ref lookup, new SzArrayReferenceTypeRepresentation( asmlIR, elementType ) );
                }
                else
                {
                    MetaDataTypeDefinitionArrayMulti td3 = (MetaDataTypeDefinitionArrayMulti)td2;

                    MetaDataTypeDefinitionArrayMulti     .Dimension[] src = td3.Dimensions;
                    MultiArrayReferenceTypeRepresentation.Dimension[] dst = new MultiArrayReferenceTypeRepresentation.Dimension[src.Length];

                    for(int i = 0; i < src.Length; i++)
                    {
                        dst[i].m_lowerBound = src[i].m_lowerBound;
                        dst[i].m_upperBound = src[i].m_upperBound;
                    }

                    return RegisterAndAnalyze( ref lookup, new MultiArrayReferenceTypeRepresentation( asmlIR, elementType, td3.Rank, dst ) );
                }
            }
            else if(td is MetaDataTypeDefinitionByRef)
            {
                MetaDataTypeDefinitionByRef td2 = (MetaDataTypeDefinitionByRef)td;

                TypeRepresentation pointerType = ConvertToIR( td2.BaseType, context );

                switch(td2.ElementType)
                {
                    case MetaData.ElementTypes.BYREF:
                        return RegisterAndAnalyze( ref lookup, new ManagedPointerTypeRepresentation( asmlIR, pointerType ) );

                    case MetaData.ElementTypes.PTR:
                        return RegisterAndAnalyze( ref lookup, new UnmanagedPointerTypeRepresentation( asmlIR, pointerType ) );

                    case MetaData.ElementTypes.PINNED:
                        return RegisterAndAnalyze( ref lookup, new PinnedPointerTypeRepresentation( asmlIR, pointerType ) );

                    default:
                        throw TypeConsistencyErrorException.Create( "Invalid byref specifier ('{0}') for type ('{1}')", td.ElementType, td );
                }
            }
            else
            {
                throw FeatureNotSupportedException.Create( "Unknown type '{0}'", td );
            }
        }

        //--//

        public MethodRepresentation ConvertToIRWithoutContext( MetaDataMethodAbstract md )
        {
            ConversionContext context = new ConversionContext(); context.Initialize();

            return ConvertToIR( md, context );
        }

        public MethodRepresentation ConvertToIR( MetaDataMethodAbstract md      ,
                                                 ConversionContext      context )
        {
            if(md == null)
            {
                return null;
            }

            //--//

            if(ComputeMaxGenericInstantiationDepth( ref context ) > 10)
            {
                throw TypeConsistencyErrorException.Create( "Found generic instantiation {0} levels deep. Is this an infinite generic recursion? {1}", ComputeMaxGenericInstantiationDepth( ref context ), context );
            }

            md = FlattenMethodDefinition( md, ref context );

            //--//

            MethodRepresentation           mdIR;
            Lookup<MetaDataMethodAbstract> lookup = new Lookup<MetaDataMethodAbstract>();
            lookup.Set( md, ref context );

            if(m_lookupMethods.TryGetValue( lookup, out mdIR ))
            {
                if(mdIR == null)
                {
                    throw TypeConsistencyErrorException.Create( "Detected recursive resolution attempt for {0}", md );
                }

                ReportVerbose( "Resolved Method {0} => {1}", lookup, mdIR );

                return mdIR;
            }

            m_lookupMethods[lookup] = null;

            //--//

            //
            // Create GenericContext, with a pointer to the template Generic and the parameter types.
            //
            MethodRepresentation.GenericContext genericContext;

            if(md is MetaDataMethodGeneric)
            {
                TypeRepresentation[] genericParameters = context.m_methodContext;

                if(genericParameters.Length > 0)
                {
                    MethodRepresentation mdIRTemplate = ConvertToIRWithoutContext( md );

                    genericContext = new MethodRepresentation.GenericContext( mdIRTemplate, genericParameters );
                }
                else
                {
                    genericContext = new MethodRepresentation.GenericContext( null, genericParameters );
                }
            }
            else
            {
                genericContext = null;
            }

            Report( "Converting Method {0}", lookup );

            //--//

            if(md is MetaDataMethodBase)
            {
                MethodRepresentation retVal;
                MetaDataMethodBase md2  = (MetaDataMethodBase)md;
                TypeRepresentation tdIR = GetDefiningType( md2, ref context );

                if(tdIR == null)
                {
                    throw TypeConsistencyErrorException.Create( "Cannot define a new method '{0}' without its owner {0}", md );
                }

                if((md2.Flags & MetaData.MethodAttributes.RTSpecialName) != 0)
                {
                    switch(md2.Name)
                    {
                        case ".cctor":
                            retVal = RegisterAndAnalyze( ref lookup, new StaticConstructorMethodRepresentation( tdIR, genericContext ) );
                            break;

                        case ".ctor":
                            retVal = RegisterAndAnalyze( ref lookup, new ConstructorMethodRepresentation( tdIR, genericContext ) );
                            break;

                            //
                            // Runtime-generated methods for multi-dimensional arrays.
                            //
                        case "Get":
                        case "Set":
                        case "Address":

                            //
                            // Methods on runtime-added interfaces for arrays:
                            //
                            // public interface IList<T> : ICollection<T>
                            // {
                            //     // The Item property provides methods to read and edit entries in the List.
                            //     T this[int index] {
                            //         get;
                            //         set;
                            //     }
                            //
                            //     // Returns the index of a particular item, if it is in the list.
                            //     // Returns -1 if the item isn't in the list.
                            //     int IndexOf(T item);
                            //
                            //     // Inserts value into the list at position index.
                            //     // index must be non-negative and less than or equal to the
                            //     // number of elements in the list.  If index equals the number
                            //     // of items in the list, then value is appended to the end.
                            //     void Insert(int index, T item);
                            //
                            //     // Removes the item at position index.
                            //     void RemoveAt(int index);
                            // }
                        case "get_Item":
                        case "set_Item":
                        case "IndexOf":
                        case "Insert":
                        case "RemoveAt":

                            //
                            // Methods on runtime-added interfaces for arrays:
                            //
                            // public interface ICollection<T> : IEnumerable<T>
                            // {
                            //     // Interfaces are not serialable
                            //     // Number of items in the collections.
                            //     int Count
                            //     {
                            //         get;
                            //     }
                            //
                            //     bool IsReadOnly
                            //     {
                            //         get;
                            //     }
                            //
                            //     void Add( T item );
                            //
                            //     void Clear();
                            //
                            //     bool Contains( T item );
                            //
                            //     // CopyTo copies a collection into an Array, starting at a particular
                            //     // index into the array.
                            //     //
                            //     void CopyTo( T[] array, int arrayIndex );
                            //
                            //     //void CopyTo(int sourceIndex, T[] destinationArray, int destinationIndex, int count);
                            //
                            //     bool Remove( T item );
                            // }
                        case "get_Count":
                        case "get_ReadOnly":
                        case "get_IsReadOnly":
                        case "Add":
                        case "Clear":
                        case "Contains":
                        case "CopyTo":
                        case "Remove":

                            //
                            // Methods on runtime-added interfaces for arrays:
                            //
                            // public interface IEnumerable<T> : IEnumerable
                            // {
                            //     // Interfaces are not serializable
                            //     // Returns an IEnumerator for this enumerable Object.  The enumerator provides
                            //     // a simple way to access all the contents of a collection.
                            //     /// <include file='doc\IEnumerable.uex' path='docs/doc[@for="IEnumerable.GetEnumerator"]/*' />
                            //     new IEnumerator<T> GetEnumerator();
                            // }
                        case "GetEnumerator":
                            CHECKS.ASSERT( (md2.Flags & MetaData.MethodAttributes.Static) == 0, "Unexpected static context for runtime method" );

                            retVal = RegisterAndAnalyze( ref lookup, new RuntimeMethodRepresentation( tdIR, genericContext ) );
                            break;

                        default:
                            throw FeatureNotSupportedException.Create( "Unknown special name method '{0}'", md2 );
                    }
                }
                else if((md2.Flags & MetaData.MethodAttributes.Static) != 0)
                {
                    retVal = RegisterAndAnalyze( ref lookup, new StaticMethodRepresentation( tdIR, genericContext ) );
                }
                else if((md2.Flags & MetaData.MethodAttributes.Virtual) != 0)
                {
                    if(md2.Name == "Finalize" )
                    {
                        retVal = RegisterAndAnalyze( ref lookup, new FinalizerMethodRepresentation( tdIR, genericContext ) );
                    }
                    else
                    {
                        if( ( md2.Flags & MetaData.MethodAttributes.Final ) != 0 )
                        {
                            retVal = RegisterAndAnalyze( ref lookup, new FinalMethodRepresentation( tdIR, genericContext ) );
                        }
                        else
                        {
                            retVal = RegisterAndAnalyze( ref lookup, new VirtualMethodRepresentation( tdIR, genericContext ) );
                        }
                    }
                }
                else
                {
                    retVal = RegisterAndAnalyze( ref lookup, new NonVirtualMethodRepresentation( tdIR, genericContext ) );
                }

                // Either the PDB information doesn't capture the debug info for a method or the
                // Zelig metadata importer doesn't read/use it, either way there's no debug info
                // for the method definition itself. To get around that this scans the original
                // IL instructions to find the first one with debug info.
                if( md2.Instructions != null )
                {
                    foreach( var instruction in md2.Instructions )
                    {
                        if( instruction.DebugInfo != null )
                        {
                            retVal.DebugInfo = instruction.DebugInfo;
                            break;
                        }
                    }
                    
                    // ensure that at least the scope information is valid
                    if( retVal.DebugInfo == null )
                    {
                        retVal.DebugInfo = new Debugging.DebugInfo(string.Empty, 0, 0, 0, 0) { Scope = md };
                    }
                }
                return retVal;
            }
            else
            {
                throw TypeConsistencyErrorException.Create( "You cannot convert method '{0}' to IR", md );
            }
        }

        //--//

        public FieldRepresentation ConvertToIR( TypeRepresentation    tdOwner ,
                                                MetaDataFieldAbstract fdIn    ,
                                                ConversionContext     context )
        {
            if(fdIn == null)
            {
                return null;
            }

            //--//

            //
            // This part is a bit messy...
            //
            // Fields cannot be generic but their type can be.
            // As a consequence, context has to be adjusted to point to the actual type of the holding type.
            // So, we allow only the owner of a field to create it.
            // To make sure the context gets set appropriately, we build 'lookup' with the actual type, not the incoming context.
            //
            MetaDataField      fd   = FlattenFieldDefinition( fdIn, ref context );
            TypeRepresentation tdIR = (tdOwner != null) ? tdOwner : GetDefiningType( fd, ref context );

            if(tdIR == null)
            {
                throw TypeConsistencyErrorException.Create( "Cannot define a new field '{0}' without its owner {0}", fdIn );
            }

            //--//

            FieldRepresentation   fdIR;
            Lookup<MetaDataField> lookup = new Lookup<MetaDataField>();
            lookup.Set( fd, tdIR );

            if(m_lookupFields.TryGetValue( lookup, out fdIR ))
            {
                if(fdIR == null)
                {
                    throw TypeConsistencyErrorException.Create( "Detected recursive resolution attempt for {0}", fd );
                }

                ReportVerbose( "Resolved Field {0} => {1}", lookup, fdIR );

                return fdIR;
            }

            m_lookupFields[lookup] = null;

            //--//

            Report( "Converting Field {0}", lookup );

            if((fd.Flags & MetaData.FieldAttributes.Static) != 0)
            {
                fdIR = RegisterAndAnalyze( ref lookup, new StaticFieldRepresentation( tdIR, fd.Name ) );
            }
            else
            {
                fdIR = RegisterAndAnalyze( ref lookup, new InstanceFieldRepresentation ( tdIR, fd.Name ) );
            }

            if(lookup.m_metadata.Layout != null)
            {
                fdIR.Offset = lookup.m_metadata.Layout.Offset;
            }

            return fdIR;
        }

        //--//

        public void ConvertToIR( MetaDataCustomAttribute[] mdcaa      ,
                                 ConversionContext         context    ,
                                 BaseRepresentation        owner      ,
                                 int                       paramIndex )
        {
            if(mdcaa != null)
            {
                int num = mdcaa.Length;

                for(int i = 0; i < num; i++)
                {
                    CustomAttributeAssociationRepresentation caa = this.ConvertToIR( mdcaa[i], context, owner, paramIndex );
                    if(caa != null)
                    {
                        owner.AddCustomAttribute( caa );
                    }
                }
            }
        }

        public CustomAttributeAssociationRepresentation ConvertToIR( MetaDataCustomAttribute caIn       ,
                                                                     ConversionContext       context    ,
                                                                     BaseRepresentation      owner      ,
                                                                     int                     paramIndex )
        {
            if(caIn == null)
            {
                return null;
            }

            //--//

            CustomAttributeRepresentation     caIR;
            Lookup< MetaDataCustomAttribute > lookup = new Lookup< MetaDataCustomAttribute >();
            lookup.Set( caIn, ref context );

            if(m_lookupAttributes.TryGetValue( lookup, out caIR ))
            {
                if(caIR == null)
                {
                    throw TypeConsistencyErrorException.Create( "Detected recursive resolution attempt for {0}", caIn );
                }
            }
            else
            {
                m_lookupAttributes[lookup] = null;

                //--//

                Report( "Converting Custom Attribute {0}", lookup );

                MetaDataCustomAttribute.NamedArg[] na = caIn.NamedArgs;

                MethodRepresentation constructor     = this.ConvertToIR( caIn.Constructor, context );
                object[]             fixedArgsValues = ArrayUtility.CopyNotNullArray( caIn.FixedArgs );
                string[]             namedArgs       = new string[na.Length];
                object[]             namedArgsValues = new object[na.Length];

                for(int faIdx = 0; faIdx < fixedArgsValues.Length; faIdx++)
                {
                    object obj = fixedArgsValues[faIdx];

                    if(obj is MetaDataTypeDefinitionAbstract)
                    {
                        fixedArgsValues[faIdx] = this.ConvertToIR( (MetaDataTypeDefinitionAbstract)obj, context );
                    }
                }

                for(int naIdx = 0; naIdx < na.Length; naIdx++)
                {
                    object obj = na[naIdx].Value;

                    if(obj is MetaDataTypeDefinitionAbstract)
                    {
                        obj = this.ConvertToIR( (MetaDataTypeDefinitionAbstract)obj, context );
                    }

                    namedArgs      [naIdx] = na[naIdx].Name;
                    namedArgsValues[naIdx] = obj;
                }

                caIR = RegisterAndAnalyze( ref lookup, new CustomAttributeRepresentation( constructor, fixedArgsValues, namedArgs, namedArgsValues ) );
            }

            CustomAttributeAssociationRepresentation caa = new CustomAttributeAssociationRepresentation( caIR, owner, paramIndex );

            if(NotifyNewCustomAttributeComplete( caa ) == true)
            {
                return caa;
            }

            return null;
        }

        //--//

        public ResourceRepresentation ConvertToIRWithoutContext( MetaDataManifestResource res )
        {
            ConversionContext context = new ConversionContext(); context.Initialize();

            return ConvertToIR( res, context );
        }

        public ResourceRepresentation ConvertToIR( MetaDataManifestResource resIn   ,
                                                   ConversionContext        context )
        {
            if(resIn == null)
            {
                return null;
            }

            //--//

            ResourceRepresentation             resIR;
            Lookup< MetaDataManifestResource > lookup = new Lookup< MetaDataManifestResource >();
            lookup.Set( resIn, ref context );

            if(m_lookupResources.TryGetValue( lookup, out resIR ))
            {
                if(resIR == null)
                {
                    throw TypeConsistencyErrorException.Create( "Detected recursive resolution attempt for {0}", resIn );
                }
            }
            else
            {
                m_lookupResources[lookup] = null;

                //--//

                Report( "Converting Resource {0}", lookup );

                AssemblyRepresentation asmlIR = ConvertToIR( resIn.Owner );

                resIR = RegisterAndAnalyze( ref lookup, new ResourceRepresentation( asmlIR ) );
            }

            return resIR;
        }

        //--//

        public TypeRepresentation GetTypeOfValue( object value )
        {
            if(value is int   ) return m_wellKnownTypes.System_Int32;
            if(value is long  ) return m_wellKnownTypes.System_Int64;
            if(value is float ) return m_wellKnownTypes.System_Single;
            if(value is double) return m_wellKnownTypes.System_Double;

            throw new Exception( "The method or operation is not implemented." );
        }

        //--//

        public BoxedValueTypeRepresentation TryToGetBoxedValueType( ValueTypeRepresentation src )
        {
            foreach(TypeRepresentation td in m_types)
            {
                BoxedValueTypeRepresentation boxed = td as BoxedValueTypeRepresentation;

                if(boxed != null && boxed.ContainedType == src)
                {
                    return boxed;
                }
            }

            return null;
        }

        public BoxedValueTypeRepresentation GetBoxedValueType( ValueTypeRepresentation src )
        {
            BoxedValueTypeRepresentation res = TryToGetBoxedValueType( src );
            if(res == null)
            {
                throw TypeConsistencyErrorException.Create( "Cannot find boxed representation for {0}", src );
            }

            return res;
        }

        public ManagedPointerTypeRepresentation TryToGetManagedPointerToType( TypeRepresentation src )
        {
            foreach(TypeRepresentation td in m_types)
            {
                ManagedPointerTypeRepresentation ptr = td as ManagedPointerTypeRepresentation;

                if(ptr != null && ptr.ContainedType == src)
                {
                    return ptr;
                }
            }

            return null;
        }

        public ManagedPointerTypeRepresentation GetManagedPointerToType( TypeRepresentation src )
        {
            ManagedPointerTypeRepresentation res = TryToGetManagedPointerToType( src );
            if(res == null)
            {
                throw TypeConsistencyErrorException.Create( "Cannot find representation for managed pointer to {0}", src );
            }

            return res;
        }

        public SzArrayReferenceTypeRepresentation TryToGetArrayOfType( TypeRepresentation src )
        {
            foreach(TypeRepresentation td in m_types)
            {
                SzArrayReferenceTypeRepresentation array = td as SzArrayReferenceTypeRepresentation;

                if(array != null && array.ContainedType == src)
                {
                    return array;
                }
            }

            return null;
        }

        public SzArrayReferenceTypeRepresentation GetArrayOfType( TypeRepresentation src )
        {
            SzArrayReferenceTypeRepresentation res = TryToGetArrayOfType( src );
            if(res == null)
            {
                throw TypeConsistencyErrorException.Create( "Cannot find representation for array of {0}", src );
            }

            return res;
        }

        //--//

        public bool IsNullable( TypeRepresentation td )
        {
            TypeRepresentation.GenericContext gc = td.Generic;

            if(gc != null && gc.Template == this.WellKnownTypes.System_Nullable_of_T)
            {
                return true;
            }

            return false;
        }

        public TypeRepresentation GetNullableParameter( TypeRepresentation td )
        {
            if(IsNullable( td ))
            {
                return td.GenericParameters[0];
            }

            return null;
        }

        public TypeRepresentation CreateBoxedValueTypeIfNecessary( TypeRepresentation td )
        {
            if(td is ReferenceTypeRepresentation ||
               td is PointerTypeRepresentation    )
            {
                return td;
            }

            return CreateBoxedValueType( td );
        }

        public TypeRepresentation CreateBoxedValueType( TypeRepresentation td )
        {
            return RegisterAndAnalyze( new BoxedValueTypeRepresentation( td ) );
        }

        public TypeRepresentation CreateManagedPointerToType( TypeRepresentation td )
        {
            return RegisterAndAnalyze( new ManagedPointerTypeRepresentation( td.Owner, td ) );
        }

        public TypeRepresentation CreateUnmanagedPointerToType( TypeRepresentation td )
        {
            return RegisterAndAnalyze( new UnmanagedPointerTypeRepresentation( td.Owner, td ) );
        }

        public TypeRepresentation CreateArrayOfType( TypeRepresentation td )
        {
            return RegisterAndAnalyze( new SzArrayReferenceTypeRepresentation( td.Owner, td ) );
        }

        public TypeRepresentation CreateDelayedVersionOfGenericTypeIfNecessary( TypeRepresentation td )
        {
            TypeRepresentation.GenericContext gc = td.Generic;

            if(gc != null && gc.IsOpenType)
            {
                ConversionContext context = new ConversionContext(); context.Initialize();

                context.SetContextAsType( td );

                TypeRepresentation[] parameters = new TypeRepresentation[gc.ParametersDefinition.Length];

                for(int i = 0; i < parameters.Length; i++)
                {
                    parameters[i] = RegisterAndAnalyze( ref context, new DelayedTypeParameterTypeRepresentation( td.Owner, td, i ) );
                }

                InstantiationContext ic = m_instantiationContext.SetParameters( parameters, null );

                TypeRepresentation td2 = ic.Instantiate( td );

                return td2;
            }
            else
            {
                return td;
            }
        }

        public TypeRepresentation CreateInstantiationOfGenericTemplate(        TypeRepresentation   tdTemplate ,
                                                                        params TypeRepresentation[] parameters )
        {
            var context = new ConversionContext();

            context.SetContextAsTypeParameters( parameters );

            return ConvertToIR( GetAssociatedMetaData( tdTemplate ), context, true );
        }

        public MethodRepresentation CreateInstantiationOfGenericTemplate(        MethodRepresentation mdTemplate ,
                                                                          params TypeRepresentation[] parameters )
        {
            var context = new ConversionContext();

            context.SetContextAsType            ( mdTemplate.OwnerType );
            context.SetContextAsMethodParameters( parameters           );

            return ConvertToIR( GetAssociatedMetaData( mdTemplate ), context );
        }


        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        public MethodRepresentation InstantiateMethod(     MethodRepresentation md               ,
                                                           TypeRepresentation[] typeParameters   ,
                                                           TypeRepresentation[] methodParameters ,
                                                       out InstantiationContext ic               )
        {
            ic = m_instantiationContext.SetParameters( typeParameters, methodParameters );

            MethodRepresentation instantiatedMd = md.Instantiate( ic );

            return instantiatedMd;
        }

        //--//

        private Lookup< MetaDataTypeDefinitionAbstract > ReverseLookup( TypeRepresentation td )
        {
            Lookup< MetaDataTypeDefinitionAbstract > res;

            m_reverseLookupTypes.TryGetValue( td, out res );

            return res;
        }

        private Lookup< MetaDataField > ReverseLookup( FieldRepresentation fd )
        {
            Lookup< MetaDataField > res;

            m_reverseLookupFields.TryGetValue( fd, out res );

            return res;
        }

        private Lookup< MetaDataMethodAbstract > ReverseLookup( MethodRepresentation md )
        {
            Lookup< MetaDataMethodAbstract > res;

            m_reverseLookupMethods.TryGetValue( md, out res );

            return res;
        }

        //--//

        private TypeRepresentation RegisterAndAnalyze( TypeRepresentation tdNew )
        {
            TypeRepresentation tdRes = m_instantiationContext.Lookup( tdNew, true );
            if(tdRes != null)
            {
                return tdRes;
            }

            m_types.Add( tdNew );

            ConversionContext context = new ConversionContext(); context.Initialize();

            tdNew.PerformDelayedTypeAnalysis( this, ref context );

            return tdNew;
        }

        private TypeRepresentation RegisterAndAnalyze( ref ConversionContext  context ,
                                                           TypeRepresentation tdNew   )
        {
            MetaData.Normalized.MetaDataTypeDefinitionAbstract metadata = this.GetAssociatedMetaData( tdNew );

            tdNew.CompleteIdentity( this, ref context, metadata );

            TypeRepresentation tdRes = m_instantiationContext.Lookup( tdNew, true );
            if(tdRes != null)
            {
                return tdRes;
            }

            m_types.Add( tdNew );

            tdNew.PerformTypeAnalysis( this, ref context );

            return tdNew;
        }

        private TypeRepresentation RegisterAndAnalyze( ref Lookup< MetaDataTypeDefinitionAbstract > lookup ,
                                                           TypeRepresentation                       tdNew  )
        {
            m_lookupTypes[lookup] = tdNew;

            tdNew.CompleteIdentity( this, ref lookup.m_context, lookup.m_metadata );

            //--//

            ReportVerbose( "Converted Type {0} [Context:{1}]", tdNew, lookup );

            TypeRepresentation tdRes = m_instantiationContext.Lookup( tdNew, true );
            if(tdRes != null)
            {
                ReportVerbose( "Found duplicate for {0} [Context:{1}]: {2} [Context:{3}]", tdNew, lookup, tdRes, ReverseLookup( tdRes ) );

                m_lookupTypes[lookup] = tdRes;

                return tdRes;
            }
            else
            {
                m_lookupTypes[lookup] = tdNew;

                m_reverseLookupTypes[tdNew] = lookup;

                m_types.Add( tdNew );

                tdNew.PerformTypeAnalysis( this, ref lookup.m_context );

                return tdNew;
            }
        }

        private FieldRepresentation RegisterAndAnalyze( ref Lookup< MetaDataField > lookup ,
                                                            FieldRepresentation     fdNew  )
        {
            FieldRepresentation fdRes = m_instantiationContext.Lookup( fdNew, true );
            if(fdRes != null)
            {
                ReportVerbose( "Found duplicate for {0} [Context:{1}]: {2} [Context:{3}]", fdNew, lookup, fdRes, ReverseLookup( fdRes ) );

                m_lookupFields[lookup] = fdRes;

                return fdRes;
            }
            else
            {
                m_lookupFields[lookup] = fdNew;

                m_reverseLookupFields[fdNew] = lookup;

                object defaultValue = lookup.m_metadata.DefaultValue;
                if(defaultValue != null)
                {
                    m_defaultValues[fdNew] = defaultValue;
                }

                fdNew.OwnerType.AddField( fdNew );

                fdNew.PerformFieldAnalysis( this, ref lookup.m_context );

                return fdNew;
            }
        }

        private MethodRepresentation RegisterAndAnalyze( ref Lookup< MetaDataMethodAbstract > lookup ,
                                                             MethodRepresentation             mdNew  )
        {
            m_lookupMethods[lookup] = mdNew;

            mdNew.CompleteIdentity( this, ref lookup.m_context, lookup.m_metadata );

            //--//

            ReportVerbose( "Converted Method {0} [Context:{1}]", mdNew, lookup );

            MethodRepresentation mdRes = m_instantiationContext.Lookup( mdNew, true );
            if(mdRes != null)
            {
                ReportVerbose( "Found duplicate for {0} [Context:{1}]: {2} [Context:{3}]", mdNew, lookup, mdRes, ReverseLookup( mdRes ) );

                m_lookupMethods[lookup] = mdRes;

                return mdRes;
            }
            else
            {
                m_lookupMethods[lookup] = mdNew;

                m_reverseLookupMethods[mdNew] = lookup;

                mdNew.OwnerType.AddMethod( mdNew );

                mdNew.PerformMethodAnalysis( this, ref lookup.m_context );

                return mdNew;
            }
        }

        private CustomAttributeRepresentation RegisterAndAnalyze( ref Lookup< MetaDataCustomAttribute > lookup ,
                                                                      CustomAttributeRepresentation     caNew  )
        {
            ReportVerbose( "Converted CA {0} [Context:{1}]", caNew, lookup );

            CustomAttributeRepresentation caRes = m_instantiationContext.Lookup( caNew, true );
            if(caRes != null)
            {
                ReportVerbose( "Found duplicate for {0} : {1} [Context:{2}]", caNew, caRes, lookup );

                m_lookupAttributes[lookup] = caRes;

                return caRes;
            }
            else
            {
                m_lookupAttributes[lookup] = caNew;

                return caNew;
            }
        }

        private ResourceRepresentation RegisterAndAnalyze( ref Lookup< MetaDataManifestResource > lookup ,
                                                               ResourceRepresentation             resNew )
        {
            m_lookupResources[lookup] = resNew;

            resNew.CompleteIdentity( this, ref lookup.m_context, lookup.m_metadata );

            ReportVerbose( "Converted Resource {0} [Context:{1}]", resNew, lookup );

            ResourceRepresentation resRes = m_instantiationContext.Lookup( resNew, true );
            if(resRes != null)
            {
                ReportVerbose( "Found duplicate for {0} : {1} [Context:{2}]", resNew, resRes, lookup );

                m_lookupResources[lookup] = resRes;

                return resRes;
            }
            else
            {
                m_lookupResources[lookup] = resNew;

                m_resources.Add( resNew );

                return resNew;
            }
        }

        //--//--//--//

        private int ComputeMaxGenericInstantiationDepth( ref ConversionContext context )
        {
            if(context.m_typeContextOwner != null)
            {
                return ComputeMaxGenericInstantiationDepth( context.m_typeContextOwner  );
            }

            if(context.m_typeContext.Length > 0)
            {
                return ComputeMaxGenericInstantiationDepth( context.m_typeContext );
            }

            return 0;
        }

        private int ComputeMaxGenericInstantiationDepth( TypeRepresentation td )
        {
            return ComputeMaxGenericInstantiationDepth( td.GenericParameters ) + 1;
        }

        private int ComputeMaxGenericInstantiationDepth( TypeRepresentation[] genParams )
        {
            int depth = 0;

            if(genParams != null)
            {
                for(int i = 0; i < genParams.Length; i++)
                {
                    int depthParam = ComputeMaxGenericInstantiationDepth( genParams[i] );

                    if(depth < depthParam) depth = depthParam;
                }
            }

            return depth;
        }

        //--//

        private MetaDataTypeDefinitionAbstract FlattenTypeDefinition(     MetaDataTypeDefinitionAbstract td                  ,
                                                                      ref ConversionContext              context             ,
                                                                          bool                           fInstantiateGeneric )
        {
            if(td is MetaDataTypeDefinitionGenericInstantiation)
            {
                MetaDataTypeDefinitionGenericInstantiation td2 = (MetaDataTypeDefinitionGenericInstantiation)td;

                SignatureType[]      instantiationParams = td2.InstantiationParams;
                TypeRepresentation[] genericParameters   = new TypeRepresentation[instantiationParams.Length];

                for(int i = 0; i < instantiationParams.Length; i++)
                {
                    genericParameters[i] = ConvertToIR( instantiationParams[i], context );
                }

                context.SetContextAsTypeParameters( genericParameters );

                return td2.GenericType;
            }
            else if(td is MetaDataTypeDefinitionDelayed)
            {
                return td;
            }
            else if(td is MetaDataTypeDefinitionArray ||
                    td is MetaDataTypeDefinitionByRef  )
            {
                if(td.IsOpenType)
                {
                    //
                    // We need to keep the full context around during delayed array/byref expansion.
                    //
                }
                else
                {
                    context.SetContextAsTypeParameters( TypeRepresentation.SharedEmptyArray );
                }

                return td;
            }
            else if(td is MetaDataTypeDefinitionGeneric && fInstantiateGeneric)
            {
                MetaDataTypeDefinitionGeneric td2 = (MetaDataTypeDefinitionGeneric)td;

                context.AdjustContextAsType( td2.GenericParams.Length );

                return td;
            }
            else
            {
                context.SetContextAsTypeParameters( TypeRepresentation.SharedEmptyArray );

                return td;
            }
        }

        private TypeRepresentation ResolveDelayedParameter(     MetaDataTypeDefinitionAbstract td                              ,
                                                            ref ConversionContext              context                         ,
                                                            out bool                           fAllocateDelayedTypeParameter   ,
                                                            out bool                           fAllocateDelayedMethodParameter )
        {
            fAllocateDelayedTypeParameter   = false;
            fAllocateDelayedMethodParameter = false;

            if(context.m_typeContextOwner is ArrayReferenceTypeRepresentation)
            {
                //
                // Special case for runtime-generated methods on arrays, which normally cannot have generic parameters.
                //
                ArrayReferenceTypeRepresentation dstArray    = (ArrayReferenceTypeRepresentation)context.m_typeContextOwner;
                TypeRepresentation               elementType = dstArray.ContainedType;

                if(td is MetaDataTypeDefinitionDelayed)
                {
                    return elementType;
                }

                if(td is MetaDataTypeDefinitionArray)
                {
                    MetaDataTypeDefinitionArray    tdArray      = (MetaDataTypeDefinitionArray)td;
                    MetaDataTypeDefinitionAbstract elementType2 = tdArray.ObjectType;

                    if(elementType2 is MetaDataTypeDefinitionDelayed)
                    {
                        return dstArray;
                    }

                    if(elementType2 is MetaDataTypeDefinitionGenericInstantiation)
                    {
                        MetaDataTypeDefinitionGenericInstantiation elementTypeInst = (MetaDataTypeDefinitionGenericInstantiation)elementType2;

                        if(elementTypeInst.GenericType == GetAssociatedMetaData( elementType ))
                        {
                            return dstArray;
                        }
                    }
                }
            }

            if(td is MetaDataTypeDefinitionDelayed)
            {
                MetaDataTypeDefinitionDelayed td2 = (MetaDataTypeDefinitionDelayed)td;

                if(td2.IsMethodParameter)
                {
                    if(context.m_methodContext.Length > 0)
                    {
                        return context.m_methodContext[td2.ParameterNumber];
                    }

                    if(context.m_methodContextOwner != null)
                    {
                        fAllocateDelayedMethodParameter = true;
                        return null;
                    }
                }
                else
                {
                    if(context.m_typeContext.Length > 0)
                    {
                        return context.m_typeContext[td2.ParameterNumber];
                    }

                    if(context.m_typeContextOwner != null)
                    {
                        fAllocateDelayedTypeParameter = true;
                        return null;
                    }
                }

                throw TypeConsistencyErrorException.Create( "Invalid context [{0}] for delayed parameter {1}", context, td );
            }

            return null;
        }

        //--//

        private MetaDataMethodAbstract FlattenMethodDefinition(     MetaDataMethodAbstract md      ,
                                                                ref ConversionContext      context )
        {
            if(md is MetaDataMethodGenericInstantiation)
            {
                MetaDataMethodGenericInstantiation md2 = (MetaDataMethodGenericInstantiation)md;

                SignatureType[]      instantiationParams = md2.InstantiationParams;
                TypeRepresentation[] genericParameters   = new TypeRepresentation[instantiationParams.Length];

                for(int i = 0; i < instantiationParams.Length; i++)
                {
                    genericParameters[i] = ConvertToIR( instantiationParams[i], context );
                }

                md = FlattenMethodDefinition( md2.BaseMethod, ref context );

                context.SetContextAsMethodParameters( genericParameters );

                return md;
            }
            else if(md is MetaDataMethodWithContext)
            {
                MetaDataMethodWithContext md2          = (MetaDataMethodWithContext)md;
                TypeRepresentation        td           = ConvertToIR( md2.ContextType, context );
                ConversionContext         contextOuter = context;

                context.SetContextAsType( td );

                if(contextOuter.m_methodContext.Length > 0)
                {
                    context.SetContextAsMethodParameters( contextOuter.m_methodContext );
                }
                else if(contextOuter.m_methodContextOwner != null)
                {
                    context.SetContextAsMethod( contextOuter.m_methodContextOwner );
                }

                return FlattenMethodDefinition( md2.BaseMethod, ref context );
            }
            else if(md is MetaDataMethodBase)
            {
                MetaDataMethodBase md2 = (MetaDataMethodBase)md;

                if(md2.IsOpenMethod == false)
                {
                    MetaDataTypeDefinitionAbstract td = md2.Owner;

                    if(td.IsOpenType == false)
                    {
                        context.SetContextAsTypeParameters( TypeRepresentation.SharedEmptyArray );
                    }
                    else
                    {
                        if(td is MetaDataTypeDefinitionArray ||
                           td is MetaDataTypeDefinitionByRef  )
                        {
                            //
                            // We need to keep the full context around during delayed array/byref expansion.
                            //
                        }
                        else
                        {
                            context.SetContextAsMethodParameters( TypeRepresentation.SharedEmptyArray );
                        }
                    }
                }

                return md2;
            }
            else
            {
                throw TypeConsistencyErrorException.Create( "Cannot flatten method {0}", md );
            }
        }

        //--//

        private MetaDataField FlattenFieldDefinition(     MetaDataFieldAbstract fdIn    ,
                                                      ref ConversionContext     context )
        {
            MetaDataFieldAbstract fd = fdIn;

            while(true)
            {
                if(fd is MetaDataFieldWithContext)
                {
                    MetaDataFieldWithContext fd2 = (MetaDataFieldWithContext)fd;
                    TypeRepresentation       td  = ConvertToIR( fd2.ContextType, context );

                    context.SetContextAsType( td );

                    fd = fd2.BaseField;
                }
                else if(fd is MetaDataField)
                {
                    MetaDataField fd2 = (MetaDataField)fd;

                    if(fd2.IsOpenField == false && fd2.Owner.IsOpenType == false)
                    {
                        context.SetContextAsTypeParameters( TypeRepresentation.SharedEmptyArray );
                    }

                    return fd2;
                }
                else
                {
                    throw TypeConsistencyErrorException.Create( "Cannot flatten field {0}", fdIn );
                }
            }
        }

        //--//

        [System.Diagnostics.Conditional( "TYPESYSTEM_REPORT" )]
        public void Report(        string   format ,
                            params object[] args   )
        {
            InnerReport( format, args );
        }

        [System.Diagnostics.Conditional( "TYPESYSTEM_REPORT_VERBOSE" )]
        public void ReportVerbose(        string   format ,
                                   params object[] args   )
        {
            InnerReport( format, args );
        }

        protected virtual void InnerReport(        string   format ,
                                            params object[] args   )
        {
        }

        //--//

        internal void QueueDelayedTypeAnalysis(     TypeRepresentation td      ,
                                                ref ConversionContext  context )
        {
            m_pendingTypeAnalysis.Schedule( td, ref context, delegate( TypeRepresentation target, ref ConversionContext ctx )
            {
                PerformDelayedTypeAnalysis( target, ref ctx );
            } );
        }

        internal void QueueDelayedMethodAnalysis(     MethodRepresentation md      ,
                                                  ref ConversionContext    context )
        {
            m_pendingMethodAnalysis.Schedule( md, ref context, delegate( MethodRepresentation target, ref ConversionContext ctx )
            {
                PerformDelayedMethodAnalysis( target, ref ctx );
            } );
        }

        internal void QueueDelayedCustomAttributeAnalysis(     TypeRepresentation td      ,
                                                           ref ConversionContext  context )
        {
            m_pendingTypeCustomAttributeAnalysis.Schedule( td, ref context, delegate( TypeRepresentation target, ref ConversionContext ctx )
            {
                PerformDelayedCustomAttributeAnalysis( target, ref ctx );
            } );
        }

        internal void QueueDelayedCustomAttributeAnalysis(     MethodRepresentation md      ,
                                                           ref ConversionContext    context )
        {
            m_pendingMethodCustomAttributeAnalysis.Schedule( md, ref context, delegate( MethodRepresentation target, ref ConversionContext ctx )
            {
                PerformDelayedCustomAttributeAnalysis( target, ref ctx );
            } );
        }

        //--//

        private bool IsMSCORLIB( MetaDataTypeDefinitionAbstract td )
        {
            AssemblyRepresentation asmlIR = ConvertToIR( td.Owner );

            return m_required_mscorlib == asmlIR;
        }

        private bool IsObjectClass( MetaDataTypeDefinitionBase td )
        {
            if(IsMSCORLIB( td )         &&
               td.Namespace == "System" &&
               td.Name      == "Object"  )
            {
                return true;
            }

            return false;
        }

        private bool IsValueType( MetaDataTypeDefinitionBase td )
        {
            if(IsMSCORLIB( td )            &&
               td.Namespace == "System"    &&
               td.Name      == "ValueType"  )
            {
                return true;
            }

            return false;
        }

        private bool IsEnumType( MetaDataTypeDefinitionBase td )
        {
            if(IsMSCORLIB( td )         &&
               td.Namespace == "System" &&
               td.Name      == "Enum"    )
            {
                return true;
            }

            return false;
        }

        //--//

        public virtual void ApplyTransformation( TransformationContext context )
        {
            context.Push( this );

            context.Transform( ref m_applicationEntryPoint  );
            context.Transform( ref m_assemblies             );
            context.Transform( ref m_types                  );
            context.Transform( ref m_resources              );
            context.Transform( ref m_exportedMethods        );
            context.Transform( ref m_defaultValues          );
            context.Transform( ref m_required_mscorlib      );
            context.Transform( ref m_wellKnownTypes         );
            context.Transform( ref m_wellKnownMethods       );
            context.Transform( ref m_wellKnownFields        );
            context.Transform( ref m_wellKnownTypesByName   );
            context.Transform( ref m_wellKnownMethodsByName );
            context.Transform( ref m_wellKnownFieldsByName  );

            context.Pop();
        }

        //--//

        protected virtual void Reduce( TypeSystem.Reachability reachability ,
                                       bool                    fApply       )
        {

            for(int i = m_types.Count; --i >= 0; )
            {
                TypeRepresentation td = m_types[i];

                if(reachability.Contains( td ) == false)
                {
                    td.ProhibitUse( reachability, fApply );
                    
                    if(fApply)
                    {
                        m_types.RemoveAt( i );
                    }
                }
                else
                {
                    td.Reduce( reachability, fApply );
                }
            }

            //--//

            for(int i = m_assemblies.Count; --i >= 0; )
            {
                AssemblyRepresentation asml = m_assemblies[i];

                if(reachability.Contains( asml ) == false)
                {
                    reachability.ExpandProhibition( asml );

                    if(fApply)
                    {
                        m_assemblies.RemoveAt( i );
                    }
                }
            }

            //--//

            TypeSystem.ReduceHashTable( ref m_defaultValues         , reachability, true , false, fApply );
            TypeSystem.ReduceHashTable( ref m_wellKnownTypesByName  , reachability, false, true , fApply );
            TypeSystem.ReduceHashTable( ref m_wellKnownMethodsByName, reachability, false, true , fApply );
            TypeSystem.ReduceHashTable( ref m_wellKnownFieldsByName , reachability, false, true , fApply );

            if(fApply)
            {
                RefreshWellKnownEntities();
            }

            //--//

            if(reachability.Contains( m_applicationEntryPoint ) == false)
            {
                if(fApply)
                {
                    m_applicationEntryPoint = null;
                }
            }
        }

        public void RemoveDuplicateType( TypeRepresentation td )
        {
            for(int i = m_types.Count; --i >= 0; )
            {
                if(Object.ReferenceEquals( td, m_types[i] ))
                {
                    m_types.RemoveAt( i );
                }
            }
        }

        //--//

        public static void ReduceHashTable< TKey, TValue >( ref GrowOnlyHashTable< TKey, TValue > ht                    ,
                                                                Reachability                      reachability          ,
                                                                bool                              fKeyMustBeReachable   ,
                                                                bool                              fValueMustBeReachable ,
                                                                bool                              fApply                )
        {
            if(fApply)
            {
                GrowOnlyHashTable< TKey, TValue > copy = ht.CloneSettings();

                foreach(TKey key in ht.Keys)
                {
                    TValue val = ht[key];

                    if((fKeyMustBeReachable   == false || (key != null && reachability.Contains( key ))) &&
                       (fValueMustBeReachable == false || (val != null && reachability.Contains( val )))  )
                    {
                        copy[key] = val;
                    }
                }

                ht = copy;
            }
        }

        public static void ReduceSet< TKey >( ref GrowOnlySet< TKey > set          ,
                                                  Reachability        reachability ,
                                                  bool                fApply       )
        {
            if(fApply)
            {
                GrowOnlySet< TKey > copy = set.CloneSettings();

                foreach(TKey key in set)
                {
                    if(reachability.Contains( key ))
                    {
                        copy.Insert( key );
                    }
                }

                set = copy;
            }
        }

        //--//

        //
        // Access Methods
        //

        public MethodRepresentation ApplicationEntryPoint
        {
            get
            {
                return m_applicationEntryPoint;
            }
        }

        public List< AssemblyRepresentation > Assemblies
        {
            get
            {
                return m_assemblies;
            }
        }

        public List< TypeRepresentation > Types
        {
            get
            {
                return m_types;
            }
        }

        public List< ResourceRepresentation > Resources
        {
            get
            {
                return m_resources;
            }
        }

        public List< MethodRepresentation > ExportedMethods
        {
            get
            {
                return m_exportedMethods;
            }
        }

        public InstantiationContext InstantiationContext
        {
            get
            {
                return m_instantiationContext;
            }
        }

        public WellKnownTypes WellKnownTypes
        {
            get
            {
                return m_wellKnownTypes;
            }
        }

        public WellKnownMethods WellKnownMethods
        {
            get
            {
                return m_wellKnownMethods;
            }
        }

        public WellKnownFields WellKnownFields
        {
            get
            {
                return m_wellKnownFields;
            }
        }

        public void GetTypeSystemStatistics(ref int types, ref int fields, ref int methods )
        {
            types   = 0;
            fields  = 0;
            methods = 0;

            foreach(var tr in this.Types)
            {
                if(ShouldIncludeInCodeGenStats( tr ))
                {
                    types   += 1;
                    fields  += tr.Fields.Length;
                    methods += tr.Methods.Length;
                }
            }
        }

        public bool ShouldIncludeInCodeGenStats( TypeRepresentation tr )
        {
            if( tr is SzArrayReferenceTypeRepresentation        || 
                tr is BoxedValueTypeRepresentation              || 
                tr is ManagedPointerTypeRepresentation          ||
                tr is UnmanagedPointerTypeRepresentation        ||
                tr is DelayedMethodParameterTypeRepresentation  ||
                tr is DelayedTypeParameterTypeRepresentation    ||
                tr is PinnedPointerTypeRepresentation            )
            {
                return false;
            }

            if(tr.IsOpenType)
            {
                return false;
            }

            if( tr is ArrayReferenceTypeRepresentation )
            {
                TypeRepresentation contained = tr.ContainedType;

                if( contained is DelayedMethodParameterTypeRepresentation ||
                    contained is DelayedTypeParameterTypeRepresentation )
                {
                    return false;
                }
            }

            return true;
        }

        //--//

        public T GetEnvironmentService< T >()
        {
            return (T)m_environmentProvider.GetService( typeof(T) );
        }

        public TypeRepresentation GetWellKnownType( string name )
        {
            TypeRepresentation td = GetWellKnownTypeNoThrow( name );

            if(td == null)
            {
                throw TypeConsistencyErrorException.Create( "Missing well-known type {0}", name );
            }

            return td;
        }

        public MethodRepresentation GetWellKnownMethod( string name )
        {
            MethodRepresentation md = GetWellKnownMethodNoThrow( name );

            if(md == null)
            {
                throw TypeConsistencyErrorException.Create( "Missing well-known method {0}", name );
            }

            return md;
        }

        public FieldRepresentation GetWellKnownField( string name )
        {
            FieldRepresentation fd = GetWellKnownFieldNoThrow( name );

            if(fd == null)
            {
                throw TypeConsistencyErrorException.Create( "Missing well-known field {0}", name );
            }

            return fd;
        }

        public TypeRepresentation GetWellKnownTypeNoThrow( string name )
        {
            return GetWellKnownEntity( m_wellKnownTypes, m_wellKnownTypesByName, name );
        }

        public MethodRepresentation GetWellKnownMethodNoThrow( string name )
        {
            return GetWellKnownEntity( m_wellKnownMethods, m_wellKnownMethodsByName, name );
        }

        public FieldRepresentation GetWellKnownFieldNoThrow( string name )
        {
            return GetWellKnownEntity( m_wellKnownFields, m_wellKnownFieldsByName, name );
        }

        private T GetWellKnownEntity<T>( object                         obj    ,
                                         GrowOnlyHashTable< string, T > lookup ,
                                         string                         name   ) where T : BaseRepresentation
        {
            T res;

            if(lookup.TryGetValue( name, out res ))
            {
                return res;
            }

            System.Reflection.FieldInfo fi = obj.GetType().GetField( name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance );
            if(fi != null)
            {
                return (T)fi.GetValue( obj );
            }

            return null;
        }

        //--//

        public void SetWellKnownType( string             name ,
                                      TypeRepresentation td   )
        {
            SetWellKnownEntity( m_wellKnownTypes, m_wellKnownTypesByName, name, td );
        }

        public void SetWellKnownMethod( string               name ,
                                        MethodRepresentation md   )
        {
            SetWellKnownEntity( m_wellKnownMethods, m_wellKnownMethodsByName, name, md );
        }

        public void SetWellKnownField( string              name ,
                                       FieldRepresentation fd   )
        {
            SetWellKnownEntity( m_wellKnownFields, m_wellKnownFieldsByName, name, fd );
        }

        private static void SetWellKnownEntity< T >( object                         obj    ,
                                                     GrowOnlyHashTable< string, T > lookup ,
                                                     string                         name   ,
                                                     T                              bd     ) where T : BaseRepresentation
        {
            lookup[name] = bd;

            System.Reflection.FieldInfo fi = obj.GetType().GetField( name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance );
            if(fi != null)
            {
                fi.SetValue( obj, bd );
            }
        }

        //--//

        public void RefreshWellKnownEntities()
        {
            RefreshWellKnownEntity( m_wellKnownTypes  , m_wellKnownTypesByName   );
            RefreshWellKnownEntity( m_wellKnownMethods, m_wellKnownMethodsByName );
            RefreshWellKnownEntity( m_wellKnownFields , m_wellKnownFieldsByName  );
        }

        private static void RefreshWellKnownEntity< T >( object                         obj    ,
                                                         GrowOnlyHashTable< string, T > lookup ) where T : BaseRepresentation
        {
            foreach(System.Reflection.FieldInfo fi in ReflectionHelper.GetAllPublicInstanceFields( obj.GetType() ))
            {
                T bd;

                lookup.TryGetValue( fi.Name, out bd );

                fi.SetValue( obj, bd );
            }
        }

        //--//

        public void EnumerateFields( FieldEnumerationCallback callback )
        {
            foreach(TypeRepresentation td in this.Types)
            {
                foreach(FieldRepresentation fd in td.Fields)
                {
                    callback( fd );
                }
            }
        }

        public void EnumerateMethods( MethodEnumerationCallback callback )
        {
            foreach(TypeRepresentation td in this.Types)
            {
                foreach(MethodRepresentation md in td.Methods)
                {
                    callback( md );
                }
            }
        }

        public void EnumerateCustomAttributes( CustomAttributeAssociationEnumerationCallback callback )
        {
            foreach(TypeRepresentation td in m_types)
            {
                td.EnumerateCustomAttributes( callback );
            }
        }

        private CustomAttributeNotification EnsureCustomAttributeNotification( TypeRepresentation td )
        {
            CustomAttributeNotification res;

            if(m_customAttributeNotification.TryGetValue( td, out res ) == false)
            {
                res = new CustomAttributeNotification();

                m_customAttributeNotification[td] = res;
            }

            return res;
        }

        public void RegisterForNotificationOfAttributeOnGeneric( TypeRepresentation               td     ,
                                                                 NotificationOfAttributeOnGeneric target )
        {
            CustomAttributeNotification res = EnsureCustomAttributeNotification( td );

            res.m_notificationOfAttributeOnGeneric += target;
        }

        public void RegisterForNotificationOfAttributeOnType( TypeRepresentation            td     ,
                                                              NotificationOfAttributeOnType target )
        {
            CustomAttributeNotification res = EnsureCustomAttributeNotification( td );

            res.m_notificationOfAttributeOnType += target;
        }

        public void RegisterForNotificationOfAttributeOnField( TypeRepresentation             td     ,
                                                               NotificationOfAttributeOnField target )
        {
            CustomAttributeNotification res = EnsureCustomAttributeNotification( td );

            res.m_notificationOfAttributeOnField += target;
        }

        public void RegisterForNotificationOfAttributeOnMethod( TypeRepresentation              td     ,
                                                                NotificationOfAttributeOnMethod target )
        {
            CustomAttributeNotification res = EnsureCustomAttributeNotification( td );

            res.m_notificationOfAttributeOnMethod += target;
        }

        public void RegisterForNotificationOfAttributeOnParam( TypeRepresentation             td     ,
                                                               NotificationOfAttributeOnParam target )
        {
            CustomAttributeNotification res = EnsureCustomAttributeNotification( td );

            res.m_notificationOfAttributeOnParam += target;
        }

        public void RegisterForNewAssembly( NotificationOfNewAssembly callback )
        {
            m_notificationOfNewAssembly += callback;
        }

        public void RegisterForNewType( NotificationOfNewType callback )
        {
            m_notificationOfNewType += callback;
        }

        public void RegisterForNewField( NotificationOfNewField callback )
        {
            m_notificationOfNewField += callback;
        }

        public void RegisterForNewMethod( NotificationOfNewMethod callback )
        {
            m_notificationOfNewMethod += callback;
        }

        public void RegisterForTypeLayoutDelegation( TypeRepresentation td       ,
                                                     TypeLayoutCallback callback )
        {
            m_layoutDelegation[td] = callback;
        }

        //--//

        public byte[] GetDefaultValue( FieldRepresentation fd )
        {
            object res;

            if(m_defaultValues.TryGetValue( fd, out res ))
            {
                return (byte[])res;
            }

            return null;
        }
        
        public TypeRepresentation GetBuiltInType( TypeRepresentation.BuiltInTypes el )
        {
            switch(el)
            {
                case TypeRepresentation.BuiltInTypes.VOID      : return m_wellKnownTypes.System_Void          ;
                case TypeRepresentation.BuiltInTypes.BOOLEAN   : return m_wellKnownTypes.System_Boolean       ;
                case TypeRepresentation.BuiltInTypes.CHAR      : return m_wellKnownTypes.System_Char          ;
                case TypeRepresentation.BuiltInTypes.I1        : return m_wellKnownTypes.System_SByte         ;
                case TypeRepresentation.BuiltInTypes.U1        : return m_wellKnownTypes.System_Byte          ;
                case TypeRepresentation.BuiltInTypes.I2        : return m_wellKnownTypes.System_Int16         ;
                case TypeRepresentation.BuiltInTypes.U2        : return m_wellKnownTypes.System_UInt16        ;
                case TypeRepresentation.BuiltInTypes.I4        : return m_wellKnownTypes.System_Int32         ;
                case TypeRepresentation.BuiltInTypes.U4        : return m_wellKnownTypes.System_UInt32        ;
                case TypeRepresentation.BuiltInTypes.I8        : return m_wellKnownTypes.System_Int64         ;
                case TypeRepresentation.BuiltInTypes.U8        : return m_wellKnownTypes.System_UInt64        ;
                case TypeRepresentation.BuiltInTypes.R4        : return m_wellKnownTypes.System_Single        ;
                case TypeRepresentation.BuiltInTypes.R8        : return m_wellKnownTypes.System_Double        ;
                case TypeRepresentation.BuiltInTypes.STRING    : return m_wellKnownTypes.System_String        ;

                case TypeRepresentation.BuiltInTypes.VALUETYPE : return m_wellKnownTypes.System_ValueType     ;
                case TypeRepresentation.BuiltInTypes.CLASS     : return m_wellKnownTypes.System_Object        ;
                case TypeRepresentation.BuiltInTypes.TYPEDBYREF: return m_wellKnownTypes.System_TypedReference;
                case TypeRepresentation.BuiltInTypes.I         : return m_wellKnownTypes.System_IntPtr        ;
                case TypeRepresentation.BuiltInTypes.U         : return m_wellKnownTypes.System_UIntPtr       ;
                case TypeRepresentation.BuiltInTypes.OBJECT    : return m_wellKnownTypes.System_Object        ;
                case TypeRepresentation.BuiltInTypes.SZARRAY   : return m_wellKnownTypes.System_Array         ;
            }

            CHECKS.ASSERT( false, "Missing required type {0}", el );

            return null;
        }

        
        private TypeRepresentation[] m_nativeTypes;
        public TypeRepresentation[] BuiltInTypes 
        {
            get
            {
                if( m_nativeTypes == null )
                {
                    m_nativeTypes = new TypeRepresentation[ 21 ];

                    m_nativeTypes[ 0 ] = m_wellKnownTypes.System_Void;
                    m_nativeTypes[ 1 ] = m_wellKnownTypes.System_Boolean;
                    m_nativeTypes[ 2 ] = m_wellKnownTypes.System_Char;
                    m_nativeTypes[ 3 ] = m_wellKnownTypes.System_SByte;
                    m_nativeTypes[ 4 ] = m_wellKnownTypes.System_Byte;
                    m_nativeTypes[ 5 ] = m_wellKnownTypes.System_Int16;
                    m_nativeTypes[ 6 ] = m_wellKnownTypes.System_UInt16;
                    m_nativeTypes[ 7 ] = m_wellKnownTypes.System_Int32;
                    m_nativeTypes[ 8 ] = m_wellKnownTypes.System_UInt32;
                    m_nativeTypes[ 9 ] = m_wellKnownTypes.System_Int64;
                    m_nativeTypes[ 10 ] = m_wellKnownTypes.System_UInt64;
                    m_nativeTypes[ 11 ] = m_wellKnownTypes.System_Single;
                    m_nativeTypes[ 12 ] = m_wellKnownTypes.System_Double;
                    m_nativeTypes[ 13 ] = m_wellKnownTypes.System_String;
                    m_nativeTypes[ 14 ] = m_wellKnownTypes.System_ValueType;
                    m_nativeTypes[ 15 ] = m_wellKnownTypes.System_Object;
                    m_nativeTypes[ 16 ] = m_wellKnownTypes.System_TypedReference;
                    m_nativeTypes[ 17 ] = m_wellKnownTypes.System_IntPtr;
                    m_nativeTypes[ 18 ] = m_wellKnownTypes.System_UIntPtr;
                    m_nativeTypes[ 19 ] = m_wellKnownTypes.System_Object;
                    m_nativeTypes[ 20 ] = m_wellKnownTypes.System_Array;

                }

                return m_nativeTypes;
            }
        }

        //--//

        internal void Analyze_GenericParameterDefinition(     MetaDataGenericParam       src     ,
                                                          ref GenericParameterDefinition dst     ,
                                                          ref ConversionContext          context )
        {
            dst.Flags  = (GenericParameterDefinition.Attributes)src.Flags;
            dst.Name   =                                        src.Name;

            MetaDataTypeDefinitionAbstract[] genericParamConstraints = src.GenericParamConstraints;
            if(genericParamConstraints != null)
            {
                dst.Constraints = new TypeRepresentation[genericParamConstraints.Length];

                for(int j = 0; j < genericParamConstraints.Length; j++)
                {
                    dst.Constraints[j] = ConvertToIR( genericParamConstraints[j], context );
                }
            }
            else
            {
                dst.Constraints = TypeRepresentation.SharedEmptyArray;
            }
        }

        public virtual CodePointer CreateCodePointer( MethodRepresentation md )
        {
            throw new NotImplementedException();
        }

        public virtual bool IncludeInGCInfo( InstanceFieldRepresentation fd )
        {
            throw new NotImplementedException();
        }
    }

    //--//

#if TYPESYSTEM_REDUCE_FOOTPRINT
    [ExtendClass(typeof(TypeRepresentation), NoConstructors=true)]
    public class TypeRepresentationImpl
    {
        public override bool Equals( object obj )
        {
            return Object.ReferenceEquals( this, obj );
        }

        public override int GetHashCode()
        {
            return 0;
        }

        internal virtual void PrettyToString( System.Text.StringBuilder sb                 ,
                                              bool                      fPrefix            ,
                                              bool                      fWithAbbreviations )
        {
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public TypeRepresentation[] GenericParameters
        {
            get
            {
                return TypeRepresentation.SharedEmptyArray;
            }
        }

        public GenericParameterDefinition[] GenericParameterDefinitions
        {
            get
            {
                return null;
            }
        }
    }

    [ExtendClass(typeof(DelayedMethodParameterTypeRepresentation), NoConstructors=true)]
    public sealed class DelayedMethodParameterTypeRepresentationImpl : TypeRepresentationImpl
    {
        internal override void PrettyToString( System.Text.StringBuilder sb                 ,
                                               bool                      fPrefix            ,
                                               bool                      fWithAbbreviations )
        {
        }
    }

    [ExtendClass(typeof(DelayedTypeParameterTypeRepresentation), NoConstructors=true)]
    public sealed class DelayedTypeParameterTypeRepresentationImpl : TypeRepresentationImpl
    {
        internal override void PrettyToString( System.Text.StringBuilder sb                 ,
                                               bool                      fPrefix            ,
                                               bool                      fWithAbbreviations )
        {
        }
    }

    [ExtendClass(typeof(MethodRepresentation), NoConstructors=true)]
    public class MethodRepresentationImpl
    {
        public override bool Equals( object obj )
        {
            return Object.ReferenceEquals( this, obj );
        }

        public override int GetHashCode()
        {
            return 0;
        }

        protected void PrettyToString( System.Text.StringBuilder sb      ,
                                       bool                      fPrefix )
        {
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public TypeRepresentation[] GenericParameters
        {
            get
            {
                return TypeRepresentation.SharedEmptyArray;
            }
        }

        public GenericParameterDefinition[] GenericParametersDefinition
        {
            get
            {
                return null;
            }
        }
    }
#endif
}
