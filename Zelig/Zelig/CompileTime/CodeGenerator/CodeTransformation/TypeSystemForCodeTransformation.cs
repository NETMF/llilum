//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

#if DEBUG
#define VERIFY_REDUCTION
#endif

//#define VERIFY_REDUCTION

#define DEBUG_IMAGEBUILDER_PERF
#define ARMv7M_BUILD__LLVM_IR_ONLY


namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    using Microsoft.Zelig.MetaData;
    using Microsoft.Zelig.MetaData.Normalized;

    using Microsoft.Zelig.Runtime.TypeSystem;

    using Microsoft.Zelig.LLVM;

    public delegate void ControlFlowGraphEnumerationCallback( ControlFlowGraphStateForCodeTransformation cfg );

    public partial class TypeSystemForCodeTransformation : TypeSystemForIR
    {
        internal sealed class FlagProhibitedUses : Transformations.ScanTypeSystem
        {
            //
            // State
            //

            //
            // Constructor Methods
            //

            internal FlagProhibitedUses( TypeSystemForCodeTransformation typeSystem )
                : base( typeSystem, typeof( FlagProhibitedUses ) )
            {
            }

            //--//

            protected override object ShouldSubstitute( object target,
                                                        out SubstitutionAction result )
            {
                if(target == m_typeSystem.ReachabilitySet.ProhibitedSet)
                {
                    result = SubstitutionAction.Keep;
                }
                else
                {
                    if(m_typeSystem.ReachabilitySet.IsProhibited( target ))
                    {
                        throw TypeConsistencyErrorException.Create( "Found unexpected reference to {0} from {1}", target, GetContextDump( ) );
                    }

                    result = SubstitutionAction.Unknown;
                }

                return null;
            }
        }

        public class DelayedType : ConstantExpression.DelayedValue,
            ITransformationContextTarget,
            CompilationSteps.ICallClosureComputationTarget
        {
            //
            // State
            //

            TypeSystemForCodeTransformation m_owner;
            VTable                          m_vTable;

            //
            // Constructor Methods
            //

            internal DelayedType( TypeSystemForCodeTransformation owner,
                                  VTable vTable )
            {
                m_owner = owner;
                m_vTable = vTable;
            }

            //--//

            //
            // MetaDataEquality Methods
            //

            public override bool Equals( object obj )
            {
                if(obj is DelayedType)
                {
                    var other = ( DelayedType )obj;

                    if(m_vTable == other.m_vTable)
                    {
                        return true;
                    }
                }

                return false;
            }

            public override int GetHashCode( )
            {
                return m_vTable.GetHashCode( );
            }

            //
            // Helper Methods
            //

            //--//

            void ITransformationContextTarget.ApplyTransformation( TransformationContext context )
            {
                var context2 = ( TransformationContextForCodeTransformation )context;

                context2.Push( this );

                context2.Transform( ref m_owner );
                context2.Transform( ref m_vTable );

                context2.Pop( );
            }

            void CompilationSteps.ICallClosureComputationTarget.ExpandClosure( CompilationSteps.ComputeCallsClosure.Context context )
            {
                context.CoverObject( m_vTable );
            }

            //--//

            private DataManager.ObjectDescriptor TryToGetValue( )
            {
                TypeRepresentation td;
                var                odVTable = m_owner.DataManagerInstance.ConvertToObjectDescriptor( m_vTable, out td ) as DataManager.ObjectDescriptor;

                if(odVTable != null)
                {
                    return odVTable.Get( (InstanceFieldRepresentation)m_owner.WellKnownFields.VTable_Type ) as DataManager.ObjectDescriptor;
                }

                return null;
            }

            //
            // Access Methods
            //

            public override bool CanEvaluate
            {
                get
                {
                    return TryToGetValue( ) != null;
                }
            }

            public override object Value
            {
                get
                {
                    return TryToGetValue( );
                }
            }

            public VTable VTable
            {
                get
                {
                    return m_vTable;
                }
            }

            //--//

            //
            // Debug Methods
            //

            public override string ToString( )
            {
                return string.Format( "<delayed type for {0}>", m_vTable );
            }
        }

        public class DelayedSize : ConstantExpression.DelayedValue,
            ITransformationContextTarget,
            CompilationSteps.ICallClosureComputationTarget
        {
            //
            // State
            //

            TypeRepresentation m_td;

            //
            // Constructor Methods
            //

            internal DelayedSize( TypeRepresentation td )
            {
                m_td = td;
            }

            //--//

            //
            // MetaDataEquality Methods
            //

            public override bool Equals( object obj )
            {
                if(obj is DelayedSize)
                {
                    var other = ( DelayedSize )obj;

                    if(m_td == other.m_td)
                    {
                        return true;
                    }
                }

                return false;
            }

            public override int GetHashCode( )
            {
                return m_td.GetHashCode( );
            }

            //
            // Helper Methods
            //

            //--//

            void ITransformationContextTarget.ApplyTransformation( TransformationContext context )
            {
                context.Push( this );

                context.Transform( ref m_td );

                context.Pop( );
            }

            void CompilationSteps.ICallClosureComputationTarget.ExpandClosure( CompilationSteps.ComputeCallsClosure.Context context )
            {
                context.CoverObject( m_td );
            }

            //--//

            //
            // Access Methods
            //

            public override bool CanEvaluate
            {
                get
                {
                    return m_td.ValidLayout;
                }
            }

            public override object Value
            {
                get
                {
                    return m_td.Size;
                }
            }

            public TypeRepresentation Type
            {
                get
                {
                    return m_td;
                }
            }

            //--//

            //
            // Debug Methods
            //

            public override string ToString( )
            {
                return string.Format( "<delayed size of {0}>", m_td );
            }
        }

        public class DelayedOffset : ConstantExpression.DelayedValue,
            ITransformationContextTarget,
            CompilationSteps.ICallClosureComputationTarget
        {
            //
            // State
            //

            FieldRepresentation m_fd;

            //
            // Constructor Methods
            //

            internal DelayedOffset( FieldRepresentation fd )
            {
                m_fd = fd;
            }

            //--//

            //
            // MetaDataEquality Methods
            //

            public override bool Equals( object obj )
            {
                if(obj is DelayedOffset)
                {
                    var other = ( DelayedOffset )obj;

                    if(m_fd == other.m_fd)
                    {
                        return true;
                    }
                }

                return false;
            }

            public override int GetHashCode( )
            {
                return m_fd.GetHashCode( );
            }

            //
            // Helper Methods
            //

            //--//

            void ITransformationContextTarget.ApplyTransformation( TransformationContext context )
            {
                context.Push( this );

                context.Transform( ref m_fd );

                context.Pop( );
            }

            void CompilationSteps.ICallClosureComputationTarget.ExpandClosure( CompilationSteps.ComputeCallsClosure.Context context )
            {
                context.CoverObject( m_fd );
            }

            //--//

            //
            // Access Methods
            //

            public override bool CanEvaluate
            {
                get
                {
                    return m_fd.ValidLayout;
                }
            }

            public override object Value
            {
                get
                {
                    return m_fd.Offset;
                }
            }

            public FieldRepresentation Field
            {
                get
                {
                    return m_fd;
                }
            }

            //--//

            //
            // Debug Methods
            //

            public override string ToString( )
            {
                return string.Format( "<delayed offset of {0}>", m_fd );
            }
        }

        public class LinearHierarchyBuilder
        {
            List<TypeRepresentation>        m_lst;
            TypeSystemForCodeTransformation m_typeSystem;

            public LinearHierarchyBuilder( List<TypeRepresentation> lst, TypeSystemForCodeTransformation typeSystem )
            {
                m_lst = lst;
                m_typeSystem = typeSystem;

                m_lst.RemoveAll( tr =>
                {
                    CustomAttributeRepresentation sfpf = tr.FindCustomAttribute( typeSystem.WellKnownTypes.Microsoft_Zelig_Runtime_SingletonFactoryPlatformFilterAttribute );
                    if(sfpf != null)
                    {
                        object obj = sfpf.GetNamedArg( "PlatformVersionFilter" );
                        if(obj != null)
                        {
                            uint filter = (uint)obj;

                            if((filter & typeSystem.PlatformAbstraction.PlatformVersion) != typeSystem.PlatformAbstraction.PlatformVersion)
                            {
                                // This type is not an allowed extension for the current platform
                                return true;
                            }
                        }
                    }

                    CustomAttributeRepresentation pf = tr.FindCustomAttribute( typeSystem.WellKnownTypes.Microsoft_Zelig_Runtime_ProductFilterAttribute );
                    if(pf != null)
                    {
                        object obj = pf.GetNamedArg( "ProductFilter" );
                        if(obj != null)
                        {
                            string product = (string)obj;

                            if(product != typeSystem.Product.FullName)
                            {
                                // This type is not an allowed extension for the current platform
                                return true;
                            }
                        }
                    }

                    return false;
                } );
            }

            public TypeRepresentation Build( )
            {

                var hierarchy = new LinkedList< TypeRepresentation >();

                //
                // There can be only one sealed type in a linear hierachy
                //
                foreach(TypeRepresentation target in m_lst)
                {
                    TypeRepresentation type;
                    if(m_typeSystem.IsSealedType( target, out type ))
                    {
                        if(hierarchy.Count > 0)
                        {
                            return null;
                        }

                        hierarchy.AddFirst( new LinkedListNode<TypeRepresentation>( type ) );
                    }
                }

                //
                // initialize the hierarchy with the last item in the input list
                //
                if(hierarchy.Count == 0)
                {

                    TypeRepresentation last = m_lst[ m_lst.Count - 1 ];

                    hierarchy.AddFirst( last );

                    m_lst.Remove( last );
                }

                //
                // Scan the input list and find the father of the last item in the hierarchy and the son of the first item.                
                // At every step we need to always grow the hierarchy of at least one item, or else we are looking at a 
                // disjoint inheritance line or a tree
                //
                int previousCount;
                do
                {
                    previousCount = hierarchy.Count;

                    TypeRepresentation first = hierarchy.First.Value;
                    TypeRepresentation last  = hierarchy.Last .Value;

                    foreach(TypeRepresentation item in m_lst)
                    {
                        if(last.Extends == item)
                        {
                            hierarchy.AddLast( item );

                            last = item;
                        }
                        else if(item.Extends == first)
                        {
                            hierarchy.AddFirst( item );

                            first = item;
                        }
                    }

                } while(previousCount < hierarchy.Count);

                //
                // if we used all objects in the input list, then we have a linear hierarchy
                //
                if(m_lst.Count == hierarchy.Count)
                {
                    return hierarchy.First.Value;
                }

                return null;
            }
        }

        public class MethodCall
        {
            private readonly MethodRepresentation m_target;
            private readonly int                  m_level;

            //--//

            public MethodCall( MethodRepresentation target, int level )
            {
                m_target = target;
                m_level = level;
            }

            public MethodRepresentation Target
            {
                get
                {
                    return m_target;
                }
            }

            public int Level
            {
                get
                {
                    return m_level;
                }
            }

            public override string ToString( )
            {
                return String.Format( "[{0}]:{1}", m_level, m_target.ToShortStringNoReturnValue() );
            }
        }

        //--//

        //
        // State
        //

        public static readonly object Lock = new Object( );
        
        private Type                                                                                          m_activeProduct;
        private Abstractions.Platform                                                                         m_activePlatform;
        private Abstractions.CallingConvention                                                                m_activeCallingConvention;

        private PendingAnalysisQueue< ByteCodeConverter                                                     > m_pendingByteCodeAnalysis;

        private Reachability                                                                                  m_reachabilityEstimated;
        private Reachability                                                                                  m_reachability;

        private ImageBuilders.Core                                                                            m_imageBuilder;
        private DataManager                                                                                   m_dataManager;
        private GrowOnlyHashTable   < TypeRepresentation  , GrowOnlyHashTable< object, ConstantExpression > > m_uniqueConstants;
        private GrowOnlyHashTable   < BaseRepresentation  , InstanceFieldRepresentation                     > m_globalRootMap;
        private ConstantExpression                                                                            m_globalRoot;
        private GrowOnlyHashTable   < Type                , TypeRepresentation                              > m_lookupType;
        private GrowOnlyHashTable   < Type                , string                                          > m_lookupTypeLinkedToRuntime;
        private GrowOnlySet         < Annotation                                                            > m_uniqueAnnotations;

        private GrowOnlyHashTable   < BaseRepresentation  , Abstractions.PlacementRequirements              > m_placementRequirements;
        private GrowOnlyHashTable   < MethodRepresentation, CustomAttributeRepresentation                   > m_hardwareExceptionHandlers;
        private GrowOnlyHashTable   < MethodRepresentation, CustomAttributeRepresentation                   > m_debuggerHookHandlers;
        private GrowOnlyHashTable   < MethodRepresentation, CustomAttributeRepresentation                   > m_singletonFactories;
        private GrowOnlyHashTable   < TypeRepresentation  , TypeRepresentation                              > m_singletonFactoriesFallback;

        private GrowOnlyHashTable   < TypeRepresentation  , TypeRepresentation                              > m_garbageCollectionExtensions;
        private GrowOnlySet         < FieldRepresentation                                                   > m_garbageCollectionExclusions;

        private GrowOnlySet         < TypeRepresentation                                                    > m_referenceCountingExcludedTypes;
        private GrowOnlyHashTable   < string              , List< MethodRepresentation >                    > m_automaticReferenceCountingExclusions;

        private GrowOnlyHashTable   < TypeRepresentation  , CustomAttributeRepresentation                   > m_memoryMappedPeripherals;
        private GrowOnlyHashTable   < FieldRepresentation , CustomAttributeRepresentation                   > m_registerAttributes;

        private GrowOnlyHashTable   < TypeRepresentation  , CustomAttributeRepresentation                   > m_memoryMappedBitFieldPeripherals;
        private GrowOnlyHashTable   < FieldRepresentation , BitFieldDefinition                              > m_bitFieldRegisterAttributes;

        //--//                                                                            

        private GrowOnlyHashTable   < TypeRepresentation  , List< TypeRepresentation   >                    > m_directDescendants;
        private GrowOnlyHashTable   < TypeRepresentation  , List< TypeRepresentation   >                    > m_concreteImplementations;
        private GrowOnlyHashTable   < TypeRepresentation  , List< TypeRepresentation   >                    > m_nestedClasses;
        private GrowOnlyHashTable   < TypeRepresentation  , List< TypeRepresentation   >                    > m_interfaceImplementors;
        private GrowOnlyHashTable   < TypeRepresentation  , List< TypeRepresentation   >                    > m_genericTypeInstantiations;
        private GrowOnlyHashTable   < MethodRepresentation, List< MethodRepresentation >                    > m_genericMethodInstantiations;

        private GrowOnlyHashTable   < TypeRepresentation  , TypeRepresentation                              > m_forcedDevirtualizations;
        private GrowOnlySet         < TypeRepresentation                                                    > m_implicitInstances;
        
        private GrowOnlyHashTable   < MethodRepresentation, List< MethodCall           >                    > m_callersToMethod;
        private GrowOnlyHashTable   < MethodRepresentation, List< MethodCall           >                    > m_callsFromMethod;
            
        private List                < string                                                                > m_nativeImportDirectories;
        private List                < string                                                                > m_nativeImportLibraries;

        //Miguel: Field added to keep LLVM info through the whole process
        private LLVMModuleManager                                                                             m_module;
        //--//

        //
        // Constructor Methods
        //

        public TypeSystemForCodeTransformation( IEnvironmentProvider env )
            : base( env )
        {
            m_dataManager                           = new DataManager( this );
            m_reachability                          = new Reachability( );
            m_uniqueConstants                       = HashTableFactory.NewWithReferenceEquality<TypeRepresentation, GrowOnlyHashTable<object, ConstantExpression>>( );
            m_globalRootMap                         = HashTableFactory.NewWithReferenceEquality<BaseRepresentation, InstanceFieldRepresentation>( );
            m_lookupType                            = HashTableFactory.NewWithReferenceEquality<Type, TypeRepresentation>( );
            m_lookupTypeLinkedToRuntime             = HashTableFactory.NewWithReferenceEquality<Type, string>( );
            m_uniqueAnnotations                     = SetFactory.New<Annotation>( );


            m_placementRequirements                 = HashTableFactory.New<BaseRepresentation, Abstractions.PlacementRequirements>( );
            m_hardwareExceptionHandlers             = HashTableFactory.New<MethodRepresentation, CustomAttributeRepresentation>( );
            m_debuggerHookHandlers                  = HashTableFactory.New<MethodRepresentation, CustomAttributeRepresentation>( );
            m_singletonFactories                    = HashTableFactory.New<MethodRepresentation, CustomAttributeRepresentation>( );
            m_singletonFactoriesFallback            = HashTableFactory.New<TypeRepresentation, TypeRepresentation>( );

            m_garbageCollectionExtensions           = HashTableFactory.New<TypeRepresentation, TypeRepresentation>( );
            m_garbageCollectionExclusions           = SetFactory.New<FieldRepresentation>( );

            m_referenceCountingExcludedTypes        = SetFactory.New<TypeRepresentation>( );
            m_automaticReferenceCountingExclusions  = HashTableFactory.New<string, List<MethodRepresentation>>( );

            m_memoryMappedPeripherals               = HashTableFactory.New<TypeRepresentation, CustomAttributeRepresentation>( );
            m_registerAttributes                    = HashTableFactory.New<FieldRepresentation, CustomAttributeRepresentation>( );

            m_memoryMappedBitFieldPeripherals       = HashTableFactory.New<TypeRepresentation, CustomAttributeRepresentation>( );
            m_bitFieldRegisterAttributes            = HashTableFactory.New<FieldRepresentation, BitFieldDefinition>( );
            
            m_callersToMethod                       = HashTableFactory.NewWithReferenceEquality<MethodRepresentation, List<MethodCall>>( );
            m_callsFromMethod                       = HashTableFactory.NewWithReferenceEquality<MethodRepresentation, List<MethodCall>>( );

            m_nativeImportDirectories               = new List<string>( );
            m_nativeImportLibraries                 = new List<string>( );

            m_module = new LLVMModuleManager( this, "" );

            //--//

            foreach( System.Reflection.FieldInfo fi in ReflectionHelper.GetAllPublicInstanceFields( typeof( WellKnownTypes ) ) )
            {
                var attrib = ReflectionHelper.GetAttribute<LinkToRuntimeTypeAttribute>( fi, false );

                if( attrib != null )
                {
                    m_lookupTypeLinkedToRuntime[ attrib.Target ] = fi.Name;
                }
            }
        }

        //
        // Helper Methods
        //

        protected override void InitializeForMetaDataImport( )
        {
            base.InitializeForMetaDataImport( );

            m_pendingByteCodeAnalysis = new PendingAnalysisQueue<ByteCodeConverter>( );
        }

        protected override void RegisterMetaDataNotifications( )
        {
            base.RegisterMetaDataNotifications( );

            //--//

            WellKnownTypes wkt = this.WellKnownTypes;


            RegisterForTypeLayoutDelegation( wkt.System_String, delegate( TypeRepresentation td, GrowOnlySet<TypeRepresentation> history )
            {
                //
                // Strings are a bit strange: their base size is not the same of the size of all their fields...
                //
                td.Size = 2 * sizeof( uint );
                td.VirtualTable.ElementSize = sizeof( char );

                WellKnownFields wkf = this.WellKnownFields;

                wkf.StringImpl_ArrayLength.Offset = 0 * sizeof( uint );
                wkf.StringImpl_StringLength.Offset = 1 * sizeof( uint );
                wkf.StringImpl_FirstChar.Offset = 2 * sizeof( uint );

                return true;
            } );

            RegisterForTypeLayoutDelegation( wkt.Microsoft_Zelig_Runtime_TypeSystem_GlobalRoot, delegate( TypeRepresentation td, GrowOnlySet<TypeRepresentation> history )
            {
                Array.Sort( td.Fields, ( a, b ) => a.Name.CompareTo( b.Name ) );

                return false;
            } );

            //--//

            var cache = GetEnvironmentService<IR.CompilationSteps.DelegationCache>( );

            cache.Register( this );
        }

        protected override void CleanupAfterMetaDataImport( )
        {
            base.CleanupAfterMetaDataImport( );

            m_pendingByteCodeAnalysis = null;
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        internal void NotifyCompilationPhaseInner( CompilationSteps.PhaseDriver phase )
        {
            NotifyCompilationPhase( phase );
        }

        protected virtual void NotifyCompilationPhase( CompilationSteps.PhaseDriver phase )
        {
        }

        public virtual SourceCodeTracker GetSourceCodeTracker( )
        {
            return null;
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        public override void ResolveAll( )
        {
            while( true )
            {
                base.ResolveAll( );

                if( m_pendingByteCodeAnalysis.Dispatch( ) )
                {
                    continue;
                }

                //
                // HACK: try to mirror generic instantiations between class extensions and their targets...
                //
                if( IR.CompilationSteps.ApplyClassExtensions.Hack_MirrorGenericInstantiations( this ) )
                {
                    continue;
                }

                break;
            }

            CheckWellKnownMethods( );
            CheckWellKnownFields( );

            CleanupAfterMetaDataImport( );
        }

        [System.Diagnostics.Conditional( "DEBUG" )]
        [System.Diagnostics.Conditional( "CHECKS_ASSERT" )]
        private void CheckWellKnownMethods( )
        {
            WellKnownMethods target = this.WellKnownMethods;

            foreach( System.Reflection.FieldInfo fi in ReflectionHelper.GetAllPublicInstanceFields( target.GetType( ) ) )
            {
                if( fi.GetValue( target ) == null )
                {
                    throw TypeConsistencyErrorException.Create( "Cannot find method to associate with 'WellKnownMethods.{0}'", fi.Name );
                }
            }
        }

        [System.Diagnostics.Conditional( "DEBUG" )]
        [System.Diagnostics.Conditional( "CHECKS_ASSERT" )]
        private void CheckWellKnownFields( )
        {
            WellKnownFields target = this.WellKnownFields;

            foreach( System.Reflection.FieldInfo fi in ReflectionHelper.GetAllPublicInstanceFields( target.GetType( ) ) )
            {
                if( fi.GetValue( target ) == null )
                {
                    throw TypeConsistencyErrorException.Create( "Cannot find field to associate with 'WellKnownFields.{0}'", fi.Name );
                }
            }
        }

        //--//--//

        protected override void PerformDelayedMethodAnalysis( MethodRepresentation target,
                                                              ref ConversionContext context )
        {
            base.PerformDelayedMethodAnalysis( target, ref context );

            MetaDataMethodBase metadata = GetAssociatedMetaData( target );
            if( metadata.Instructions != null )
            {
                ConvertByteCodeToIR( target, context, metadata.Locals, metadata.DebugInformation );
            }
        }

        //--//

        private void ConvertByteCodeToIR( MethodRepresentation md,
                                          ConversionContext context,
                                          SignatureType[] locals,
                                          Debugging.MethodDebugInfo debugInfo )
        {
            if( md.Code == null )
            {
                var converter = new ByteCodeConverter( this, ref context, md, locals, debugInfo );

                converter.ExpandByteCodeArguments( );

                this.QueueDelayedByteCodeAnalysis( converter, ref context );
            }
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        internal void QueueDelayedByteCodeAnalysis( ByteCodeConverter converter,
                                                    ref ConversionContext context )
        {
            m_pendingByteCodeAnalysis.Schedule( converter, ref context, delegate( ByteCodeConverter target, ref ConversionContext ctx )
            {
                target.PerformDelayedByteCodeAnalysis( );
            } );
        }

        //--//

        public void EnumerateFlowGraphs( ControlFlowGraphEnumerationCallback callback )
        {
            foreach( TypeRepresentation td in this.Types )
            {
                foreach( MethodRepresentation md in td.Methods )
                {
                    var cfg = TypeSystemForCodeTransformation.GetCodeForMethod( md );
                    if( cfg != null )
                    {
                        callback( cfg );
                    }
                }
            }
        }

        public void BuildCrossReference( )
        {
            BuildMethodAndInterfaceMaps( );
            BuildGenericInstantiationTables( );
            BuildHierarchyTables( );
        }

        public void BuildMethodAndInterfaceMaps( )
        {
            foreach( TypeRepresentation td in this.Types )
            {
                td.ClearMethodTableAndInterfaceMaps( this );
            }

            foreach( TypeRepresentation td in this.Types )
            {
                td.BuildMethodTableAndInterfaceMaps( this );
            }
        }

        public void BuildGenericInstantiationTables( )
        {
            m_genericTypeInstantiations = HashTableFactory.NewWithReferenceEquality<TypeRepresentation, List<TypeRepresentation>>( );
            m_genericMethodInstantiations = HashTableFactory.NewWithReferenceEquality<MethodRepresentation, List<MethodRepresentation>>( );

            foreach( TypeRepresentation td in this.Types )
            {
                TypeRepresentation tdTemplate = td.GenericTemplate;
                if( tdTemplate != null )
                {
                    HashTableWithListFactory.AddUnique( m_genericTypeInstantiations, tdTemplate, td );
                }

                foreach( MethodRepresentation md in td.Methods )
                {
                    MethodRepresentation mdTemplate = md.GenericTemplate;
                    if( mdTemplate != null )
                    {
                        HashTableWithListFactory.AddUnique( m_genericMethodInstantiations, mdTemplate, md );
                    }
                }
            }
        }

        public void BuildHierarchyTables( )
        {
            m_directDescendants       = HashTableFactory.NewWithReferenceEquality<TypeRepresentation, List<TypeRepresentation>>( );
            m_concreteImplementations = HashTableFactory.NewWithReferenceEquality<TypeRepresentation, List<TypeRepresentation>>( );
            m_interfaceImplementors   = HashTableFactory.NewWithReferenceEquality<TypeRepresentation, List<TypeRepresentation>>( );
            m_nestedClasses           = HashTableFactory.NewWithReferenceEquality<TypeRepresentation, List<TypeRepresentation>>( );

            m_forcedDevirtualizations = HashTableFactory.NewWithReferenceEquality<TypeRepresentation, TypeRepresentation>( );
            m_implicitInstances = SetFactory.NewWithReferenceEquality<TypeRepresentation>( );

            //
            // BUGBUG: Generic method instantiations are not captured under the Types hierarchy, they are kind of free-floating methods...
            //
            foreach( TypeRepresentation td in this.Types )
            {
                TypeRepresentation td2;

                td2 = td.Extends;
                if( td2 != null )
                {
                    HashTableWithListFactory.AddUnique( m_directDescendants, td2, td );
                }

                if( td.IsAbstract == false )
                {
                    HashTableWithListFactory.AddUnique( m_concreteImplementations, td, td );

                    while( td2 != null )
                    {
                        HashTableWithListFactory.AddUnique( m_concreteImplementations, td2, td );

                        td2 = td2.Extends;
                    }
                }

                td2 = td.EnclosingClass;
                if( td2 != null )
                {
                    HashTableWithListFactory.AddUnique( m_nestedClasses, td2, td );
                }

                if( td is InterfaceTypeRepresentation )
                {
                    HashTableWithListFactory.Create( m_interfaceImplementors, td );
                }
                else
                {
                    AddInterfaces( td, td.Interfaces );
                }
            }

            foreach( TypeRepresentation td in this.Types )
            {
                if( td.HasBuildTimeFlag( TypeRepresentation.BuildTimeAttributes.ForceDevirtualization ) )
                {
                    var lst = CollectConcreteImplementations( td );

                    switch( lst.Count )
                    {
                        case 0:
                            throw TypeConsistencyErrorException.Create( "'{0}' is marked as ForcedInstantiation, but no candidate was found", td );

                        case 1:
                            m_forcedDevirtualizations[ td ] = lst[ 0 ];
                            break;

                        default:
                            //
                            // if we have more than one
                            //
                            var mostDerived = new LinearHierarchyBuilder( lst, this ).Build();

                            if( mostDerived != null )
                            {
                                m_forcedDevirtualizations[ td ] = mostDerived;
                                break;
                            }

                            var  sb     = new System.Text.StringBuilder( );
                            bool fFirst = true;

                            foreach( var tdCandidate in lst )
                            {
                                if( fFirst )
                                {
                                    fFirst = false;
                                }
                                else
                                {
                                    sb.AppendFormat( ", " );
                                }

                                sb.AppendFormat( "{0}", tdCandidate.FullNameWithAbbreviation );
                            }

                            throw TypeConsistencyErrorException.Create( "'{0}' is marked as ForcedInstantiation, but found multiple candidates: {1}", td, sb );
                    }
                }

                if( td.HasBuildTimeFlag( TypeRepresentation.BuildTimeAttributes.ImplicitInstance ) )
                {
                    m_implicitInstances.Insert( td );
                }
            }
        }

        private void AddInterfaces( TypeRepresentation td,
                                    InterfaceTypeRepresentation[] itfArray )
        {
            foreach( var itf in itfArray )
            {
                HashTableWithListFactory.AddUnique( m_interfaceImplementors, itf, td );

                AddInterfaces( td, itf.Interfaces );
            }
        }

        public TypeRepresentation FindSingleConcreteImplementation( TypeRepresentation td )
        {
            List< TypeRepresentation > lst = CollectConcreteImplementations( td );

            if( lst.Count == 1 )
            {
                return lst[ 0 ];
            }

            return null;
        }

        public List<TypeRepresentation> CollectConcreteImplementations( TypeRepresentation td )
        {
            var lst = new List<TypeRepresentation>( );

            CollectConcreteImplementations( td, lst );

            //
            // If there was a fallback, then there will be at list one item in the list. 
            //
            if(lst.Count >= 1)
            {
                TypeRepresentation tdFallback;

                //
                // If there is more than one item, try and remove the fallback.
                //
                if(lst.Count > 1)
                {
                    if(m_singletonFactoriesFallback.TryGetValue( td, out tdFallback ))
                    {
                        lst.Remove( tdFallback );
                    }
                }

                // Check with the configuration provider that any choice specified using 
                // -CompilationOption is enforcable. Match the whole hyerarchy. 
                var cfgProv = GetEnvironmentService<IConfigurationProvider>( );

                object match;
                if(cfgProv.GetValue( td.Name, out match ) && match is string)
                {
                    string target = (string)match;
                    var newList = new List<TypeRepresentation>( );

                    foreach(var tdCandidate in lst)
                    {
                        var source = tdCandidate;

                        while(source != null)
                        {
                            if(source.Name == target)
                            {
                                newList.Add( tdCandidate );

                                break;
                            }

                            source = source.Extends;
                        }
                    }

                    if(newList.Count == 0)
                    {
                        throw TypeConsistencyErrorException.Create( "Type {0} mandated by explicit configration option {1) could not be found", target, td.Name );
                    }

                    lst = newList;
                }
            }

            return lst;
        }

        private void CollectConcreteImplementations( TypeRepresentation td,
                                                     List<TypeRepresentation> lst )
        {
            List< TypeRepresentation > tdLst;

            if( td.IsAbstract == false )
            {
                lst.Add( td );
            }

            if( m_directDescendants.TryGetValue( td, out tdLst ) )
            {
                foreach( TypeRepresentation tdSub in tdLst )
                {
                    CollectConcreteImplementations( tdSub, lst );
                }
            }
        }

        //--//

        internal void ExpandCallsClosure( CompilationSteps.ComputeCallsClosure computeCallsClosure )
        {
            ExpandCallsClosure_GlobalRoot           ( computeCallsClosure );
            ExpandCallsClosure_PlacementRequirements( computeCallsClosure );
            ExpandCallsClosure_HardwareExceptions   ( computeCallsClosure );
            ExpandCallsClosure_HardwareModel        ( computeCallsClosure );
            ExpandCallsClosure_GarbageCollection    ( computeCallsClosure );
            ExpandCallsClosure_ExportedMethods      ( computeCallsClosure );

            m_activeCallingConvention.ExpandCallsClosure( computeCallsClosure );
            m_activePlatform         .ExpandCallsClosure( computeCallsClosure );
        }

        private void ExpandCallsClosure_GlobalRoot( CompilationSteps.ComputeCallsClosure computeCallsClosure )
        {
            computeCallsClosure.Expand( m_globalRoot );
            computeCallsClosure.Expand( m_globalRoot.Type );
            computeCallsClosure.Expand( m_globalRoot.Value );
            computeCallsClosure.ExpandValueIfKeyIsReachable( m_globalRootMap );
        }

        private void ExpandCallsClosure_PlacementRequirements( CompilationSteps.ComputeCallsClosure computeCallsClosure )
        {
            computeCallsClosure.ExpandValueIfKeyIsReachable( m_placementRequirements );
        }

        private void ExpandCallsClosure_HardwareExceptions( CompilationSteps.ComputeCallsClosure computeCallsClosure )
        {
            computeCallsClosure.Expand( TryGetHandler( Runtime.HardwareException.Reset ) );
        }
        
        private void ExpandCallsClosure_ExportedMethods( CompilationSteps.ComputeCallsClosure computeCallsClosure )
        {
            this.EnumerateMethods( delegate( MethodRepresentation md )
            {
                if(md.HasBuildTimeFlag( MethodRepresentation.BuildTimeAttributes.Exported ))
                {
                    computeCallsClosure.Expand( md );
                }
            } );
        }
        
        private void ExpandCallsClosure_HardwareModel( CompilationSteps.ComputeCallsClosure computeCallsClosure )
        {
            computeCallsClosure.ExpandValueIfKeyIsReachable( m_memoryMappedPeripherals );
            computeCallsClosure.ExpandValueIfKeyIsReachable( m_registerAttributes );

            computeCallsClosure.ExpandValueIfKeyIsReachable( m_memoryMappedBitFieldPeripherals );
            computeCallsClosure.ExpandValueIfKeyIsReachable( m_bitFieldRegisterAttributes );
        }

        private void ExpandCallsClosure_GarbageCollection( CompilationSteps.ComputeCallsClosure computeCallsClosure )
        {
            computeCallsClosure.ExpandValueIfKeyIsReachable( m_garbageCollectionExtensions );
        }

        //--//

        public override void RefreshHashCodesAfterTypeSystemRemapping( )
        {
            base.RefreshHashCodesAfterTypeSystemRemapping( );

            Annotation[] array = m_uniqueAnnotations.ToArray( );

            m_uniqueAnnotations.Clear( );

            foreach( var an in array )
            {
                if( IsUseProhibited( an ) == false )
                {
                    m_uniqueAnnotations.Insert( an );
                }
            }
        }

        internal bool IsUseProhibited( object obj )
        {
            return m_reachability.IsProhibited( obj );
        }

        internal void ProhibitUse( object obj )
        {
            m_reachability.ExpandProhibition( obj );
        }

        internal void EstimateReduction( )
        {
            m_reachabilityEstimated = m_reachability.CloneForProhibitionEstimation( );

            Reduce( m_reachabilityEstimated, false );
        }

        internal void PerformReduction( )
        {
            m_reachabilityEstimated = null;

            Reduce( m_reachability, true );
        }

        protected override void Reduce( TypeSystem.Reachability reachability,
                                        bool fApply )
        {
            base.Reduce( reachability, fApply );

            //--//

            m_dataManager.Reduce( reachability, fApply );

            //--//

            var uniqueConstants = m_uniqueConstants.CloneSettings( );

            foreach( TypeRepresentation td in m_uniqueConstants.Keys )
            {
                if( reachability.Contains( td ) )
                {
                    var ht    = HashTableFactory.New<object, ConstantExpression>( );
                    var htOld = m_uniqueConstants[ td ];

                    foreach( object obj in htOld.Keys )
                    {
                        var ex = htOld[ obj ];

                        if( reachability.Contains( ex ) )
                        {
                            object val = ex.Value;

                            if( val != null && reachability.Contains( val ) == false )
                            {
                                if( reachability.IsProhibited( val ) )
                                {
                                    continue;
                                }

                                if( val is DataManager.DataDescriptor )
                                {
                                    continue;
                                }
                            }

                            ht[ obj ] = ex;
                        }
                    }

                    if( ht.Count > 0 )
                    {
                        uniqueConstants[ td ] = ht;
                    }
                }
            }

            if( fApply )
            {
                m_uniqueConstants = uniqueConstants;
            }

            //--//

            ReduceHashTable( ref m_globalRootMap, reachability, true, true, fApply );
            ReduceHashTable( ref m_lookupType, reachability, false, true, fApply );
            ReduceSet( ref m_uniqueAnnotations, reachability, fApply );

            ReduceHashTable( ref m_placementRequirements, reachability, true, false, fApply );
            ReduceHashTable( ref m_hardwareExceptionHandlers, reachability, true, false, fApply );
            ReduceHashTable( ref m_debuggerHookHandlers, reachability, true, false, fApply );
            ReduceHashTable( ref m_singletonFactories, reachability, true, true, fApply );
            ReduceHashTable( ref m_singletonFactoriesFallback, reachability, true, true, fApply );

            ReduceHashTable( ref m_garbageCollectionExtensions, reachability, true, false, fApply );
            ReduceSet( ref m_garbageCollectionExclusions, reachability, fApply );

            ReduceHashTable( ref m_memoryMappedPeripherals, reachability, true, false, fApply );
            ReduceHashTable( ref m_registerAttributes, reachability, true, false, fApply );

            ReduceHashTable( ref m_memoryMappedBitFieldPeripherals, reachability, true, false, fApply );
            ReduceHashTable( ref m_bitFieldRegisterAttributes, reachability, true, false, fApply );

            //--//

            if( reachability.Contains( m_globalRoot.Type ) == false )
            {
                reachability.ExpandProhibition( m_globalRoot );

                if( fApply )
                {
                    m_globalRoot = null;
                }
            }

            //--//

            if( fApply )
            {
#if VERIFY_REDUCTION
                foreach( Object obj in reachability.IncludedSet )
                {
                    CHECKS.ASSERT( reachability.IsProhibited( obj ) == false, "Mutual exclusion violation for {0}", obj );
                }

                foreach( Object obj in reachability.ProhibitedSet )
                {
                    CHECKS.ASSERT( reachability.Contains( obj ) == false, "Mutual exclusion violation for {0}", obj );
                }
#endif

                var flagProhibitedUses = new FlagProhibitedUses( this );
                flagProhibitedUses.ProcessTypeSystem( );
            }
        }

        //--//
        
        internal void FlattenCallsDatabase( CompilationSteps.CallsDataBase callsDb, bool fCallsTo )
        {
            CompilationSteps.ParallelTransformationsHandler.EnumerateMethods( this, target =>
            {
                var analysis = new Queue<MethodCall>();
                var analyzed = SetFactory.NewWithReferenceEquality<MethodRepresentation>();

                var call = new MethodCall( target, 0 );

                analysis.Enqueue( call );

                List<MethodCall> lst = null;
                
                if(fCallsTo)
                {
                    lock (m_callersToMethod)
                    {
                        HashTableWithListFactory.Create<MethodRepresentation, MethodCall>( m_callersToMethod, call.Target );

                        lst = m_callersToMethod[ call.Target ];
                    }
                }
                else
                {
                    lock (m_callsFromMethod)
                    {
                        HashTableWithListFactory.Create<MethodRepresentation, MethodCall>( m_callsFromMethod, call.Target );

                        lst = m_callsFromMethod[ call.Target ];
                    }
                }

                while(analysis.Count > 0)
                {
                    var current = analysis.Dequeue();

                    var calls = fCallsTo ? callsDb.CallsToMethod( current.Target ) : callsDb.CallsFromMethod( current.Target ) ;

                    if(calls != null)
                    {
                        foreach(var opCall in calls)
                        {
                            var method = fCallsTo ? opCall.BasicBlock.Owner.Method : opCall.TargetMethod;

                            if(analyzed.Contains( method ))
                            {
                                continue;
                            }

                            var call1 = new MethodCall( method, current.Level + 1 );

                            analysis.Enqueue( call1 );

                            analyzed.Insert( method );

                            lst.Add( call1 );
                        }
                    }
                }
            } );
        }

        //--//

        public override void ApplyTransformation( TransformationContext contextIn )
        {
            var context = ( TransformationContextForCodeTransformation )contextIn;

            context.Push( this );

            base.ApplyTransformation( contextIn );

            context.TransformGeneric( ref m_activePlatform );
            context.TransformGeneric( ref m_activeCallingConvention );

            context.Transform( ref m_reachability );
            context.Transform( ref m_imageBuilder );
            context.Transform( ref m_dataManager );
            context.Transform( ref m_uniqueConstants );
            context.Transform( ref m_globalRootMap );
            context.Transform( ref m_globalRoot );

            context.Transform( ref m_placementRequirements );
            context.Transform( ref m_hardwareExceptionHandlers );
            context.Transform( ref m_debuggerHookHandlers );
            context.Transform( ref m_singletonFactories );
            context.Transform( ref m_singletonFactoriesFallback );

            context.Transform( ref m_garbageCollectionExtensions );
            context.Transform( ref m_garbageCollectionExclusions );

            context.Transform( ref m_memoryMappedPeripherals );
            context.Transform( ref m_registerAttributes );

            context.Transform( ref m_memoryMappedBitFieldPeripherals );
            context.Transform( ref m_bitFieldRegisterAttributes );

            context.Pop( );
        }

        //--//

        public Operator.OperatorLevel GetLevel( Operator op )
        {
            return op.GetLevel( m_activePlatform );
        }

        public Operator.OperatorLevel GetLevel( Expression ex )
        {
            return ex.GetLevel( m_activePlatform );
        }

        public bool ExpressionAlreadyInScalarForm( Expression ex )
        {
            return ( ex == null || GetLevel( ex ) <= Operator.OperatorLevel.ScalarValues );
        }

        public bool OperatorAlreadyInScalarForm( Operator op )
        {
            foreach( var exLhs in op.Results )
            {
                if( ExpressionAlreadyInScalarForm( exLhs ) == false )
                {
                    return false;
                }
            }

            foreach( var exRhs in op.Arguments )
            {
                if( ExpressionAlreadyInScalarForm( exRhs ) == false )
                {
                    return false;
                }
            }

            return true;
        }

        //
        // Access Methods
        //

        public Type Product
        {
            get
            {
                return m_activeProduct;
            }

            set
            {
                if( m_activeProduct != null )
                {
                    throw TypeConsistencyErrorException.Create( "Cannot change Product after it's been set" );
                }

                m_activeProduct = value;
            }
        }

        public Abstractions.Platform PlatformAbstraction
        {
            get
            {
                return m_activePlatform;
            }

            set
            {
                if( m_activePlatform != null )
                {
                    throw TypeConsistencyErrorException.Create( "Cannot change PlatformAbstraction after it's been set" );
                }

                m_activePlatform = value;
            }
        }

        public Abstractions.CallingConvention CallingConvention
        {
            get
            {
                return m_activeCallingConvention;
            }

            set
            {
                if( m_activeCallingConvention != null )
                {
                    throw TypeConsistencyErrorException.Create( "Cannot change CallingConvention after it's been set" );
                }

                m_activeCallingConvention = value;
            }
        }

        public Reachability EstimatedReachabilitySet
        {
            get
            {
                return m_reachabilityEstimated;
            }
        }

        public Reachability ReachabilitySet
        {
            get
            {
                return m_reachability;
            }
        }

        public ImageBuilders.Core ImageBuilder
        {
            get
            {
                return m_imageBuilder;
            }
        }

        public DataManager DataManagerInstance
        {
            get
            {
                return m_dataManager;
            }
        }

        public ConstantExpression GlobalRoot
        {
            get
            {
                return m_globalRoot;
            }
        }

        //--//

        public GrowOnlyHashTable<BaseRepresentation, Abstractions.PlacementRequirements> PlacementRequirements
        {
            get
            {
                return m_placementRequirements;
            }
        }

        public GrowOnlyHashTable<MethodRepresentation, CustomAttributeRepresentation> HardwareExceptionHandlers
        {
            get
            {
                return m_hardwareExceptionHandlers;
            }
        }

        public GrowOnlyHashTable<MethodRepresentation, CustomAttributeRepresentation> DebuggerHookHandlers
        {
            get
            {
                return m_debuggerHookHandlers;
            }
        }

        public GrowOnlyHashTable<MethodRepresentation, CustomAttributeRepresentation> SingletonFactories
        {
            get
            {
                return m_singletonFactories;
            }
        }

        public GrowOnlyHashTable<TypeRepresentation, TypeRepresentation> SingletonFactoriesFallback
        {
            get
            {
                return m_singletonFactoriesFallback;
            }
        }

        public GrowOnlyHashTable<TypeRepresentation, CustomAttributeRepresentation> MemoryMappedPeripherals
        {
            get
            {
                return m_memoryMappedPeripherals;
            }
        }

        public GrowOnlyHashTable<FieldRepresentation, CustomAttributeRepresentation> RegisterAttributes
        {
            get
            {
                return m_registerAttributes;
            }
        }

        public GrowOnlyHashTable<TypeRepresentation, CustomAttributeRepresentation> MemoryMappedBitFieldPeripherals
        {
            get
            {
                return m_memoryMappedBitFieldPeripherals;
            }
        }

        public GrowOnlyHashTable<FieldRepresentation, BitFieldDefinition> BitFieldRegisterAttributes
        {
            get
            {
                return m_bitFieldRegisterAttributes;
            }
        }
        
        public GrowOnlyHashTable<MethodRepresentation, List<MethodCall>> FlattenedCallsDataBase_CallsTo
        {
            get
            {
                return m_callersToMethod;
            }
        }

        public GrowOnlyHashTable<MethodRepresentation, List<MethodCall>> FlattenedCallsDataBase_CallsFrom
        {
            get
            {
                return m_callsFromMethod; ;
            }
        }

        //--//

        public GrowOnlyHashTable<TypeRepresentation, TypeRepresentation> GarbageCollectionExtensions
        {
            get
            {
                return m_garbageCollectionExtensions;
            }
        }

        public GrowOnlySet<FieldRepresentation> GarbageCollectionExclusions
        {
            get
            {
                return m_garbageCollectionExclusions;
            }
        }

        [Flags]
        public enum ReferenceCountingStatus
        {
            Disabled = 0,
            Enabled  = 0x1,
            Strict   = 0x2,

            EnabledStrict = Enabled | Strict,
        }

        public ReferenceCountingStatus ReferenceCountingGarbageCollectionStatus
        {
            get; set;
        }

        public bool IsReferenceCountingGarbageCollectionEnabled
        {
            get
            {
                var status = this.ReferenceCountingGarbageCollectionStatus;
                return (status & ReferenceCountingStatus.Enabled) == ReferenceCountingStatus.Enabled;
            }
        }

        public bool IsReferenceCountingType( TypeRepresentation td )
        {
            if(!this.IsReferenceCountingGarbageCollectionEnabled ||
                td == null ||
                !( td is ReferenceTypeRepresentation ) ||
                td.IsOpenType ||
                td.IsDelayedType)
            {
                return false;
            }

            // If type T is excluded, then all its derived classes would be as well.
            while(td != null)
            {
                if(m_referenceCountingExcludedTypes.Contains( td ) || m_implicitInstances.Contains( td ))
                {
                    return false;
                }
                td = td.Extends;
            }

            return true;
        }

        public bool ShouldExcludeMethodFromReferenceCounting( MethodRepresentation md )
        {
            if(md.IsGenericInstantiation)
            {
                md = md.GenericTemplate;
            }

            var methodsList = m_automaticReferenceCountingExclusions.GetValue( md.Name );
            if(methodsList != null)
            {
                var matches = methodsList.FindAll( method => method.MatchSignature( md, null ) );

                foreach(var match in matches)
                {
                    if(match == md || match.OwnerType.IsSuperClassOf( md.OwnerType, null ))
                    {
                        // Match if it's in the list or it overrides a method in the list.
                        return true;
                    }
                }
            }

            return false;
        }

        //--//

        public List<string> NativeImportDirectories
        {
            get
            {
                return m_nativeImportDirectories;
            }
            set
            {
                m_nativeImportDirectories = value;
            }
        }

        public List<string> NativeImportLibraries
        {
            get
            {
                return m_nativeImportLibraries;
            }
            set
            {
                m_nativeImportLibraries = value;
            }
        }

        public GrowOnlyHashTable<TypeRepresentation, List<TypeRepresentation>> DirectDescendant
        {
            get
            {
                return m_directDescendants;
            }
        }

        public GrowOnlyHashTable<TypeRepresentation, List<TypeRepresentation>> NestedClasses
        {
            get
            {
                return m_nestedClasses;
            }
        }

        public GrowOnlyHashTable<TypeRepresentation, List<TypeRepresentation>> InterfaceImplementors
        {
            get
            {
                return m_interfaceImplementors;
            }
        }

        public GrowOnlyHashTable<TypeRepresentation, List<TypeRepresentation>> GenericTypeInstantiations
        {
            get
            {
                return m_genericTypeInstantiations;
            }
        }

        public GrowOnlyHashTable<MethodRepresentation, List<MethodRepresentation>> GenericMethodInstantiations
        {
            get
            {
                return m_genericMethodInstantiations;
            }
        }

        public GrowOnlyHashTable<TypeRepresentation, TypeRepresentation> ForcedDevirtualizations
        {
            get
            {
                return m_forcedDevirtualizations;
            }
        }

        public GrowOnlySet<TypeRepresentation> ImplicitInstances
        {
            get
            {
                return m_implicitInstances;
            }
        }

        //--//

        public List<Configuration.Environment.ImageSection> Image
        {
            get
            {
                if( m_imageBuilder != null )
                {
                    return m_imageBuilder.CollectImage( );
                }

                return null;
            }
        }

        public LLVMModuleManager Module
        {
            get
            {
                return m_module;
            }
        }

        //--//

        public bool IsSealedType( TypeRepresentation target,
                                  out TypeRepresentation targetSealed )
        {
            if( ( target.Flags & TypeRepresentation.Attributes.Sealed ) != 0 )
            {
                targetSealed = target;
                return true;
            }

            //
            // BUGBUG: This can be used only for bounded applications.
            //
            if( m_concreteImplementations != null )
            {
                List< TypeRepresentation > tdLst;

                if( m_concreteImplementations.TryGetValue( target, out tdLst ) )
                {
                    if( tdLst.Count == 1 )
                    {
                        targetSealed = tdLst[ 0 ];
                        return true;
                    }
                }
            }

            targetSealed = null;
            return false;
        }

        //--//

        public override ControlFlowGraphState CreateControlFlowGraphState( MethodRepresentation md,
                                                                               TypeRepresentation[] localVars,
                                                                               string[] localVarNames,
                                                                           out VariableExpression[] arguments,
                                                                           out VariableExpression[] locals )
        {
            var cfg = new ControlFlowGraphStateForCodeTransformation( this, md );

            //
            // Create entry and exit basic blocks.
            //
            {
                var entryBasicBlock = new EntryBasicBlock( cfg );
                var exitBasicBlock  = new ExitBasicBlock( cfg );

                entryBasicBlock.AddOperator( UnconditionalControlOperator.New( null, exitBasicBlock ) );
            }

            locals = cfg.AllocateVariables( localVars, localVarNames );
            arguments = cfg.Arguments;

            //--//

            md.Code = cfg;

            return cfg;
        }

        public override ConstantExpression CreateNullPointer( TypeRepresentation td )
        {
            if( td is ValueTypeRepresentation )
            {
                td = GetManagedPointerToType( ( ValueTypeRepresentation )td );
            }

            return CreateConstant( td, null );
        }

        public override ConstantExpression CreateConstant( TypeRepresentation td,
                                                           object value )
        {
            var dd = value as DataManager.DataDescriptor;
            if( dd != null )
            {
                CHECKS.ASSERT( td == dd.Context, "Mismatch between formal and actual type: {0} != {1}", td, dd );

                td = dd.Context;
            }

            GrowOnlyHashTable< object, ConstantExpression > ht;

            lock( m_uniqueConstants )
            {
                if( m_uniqueConstants.TryGetValue( td, out ht ) == false )
                {
                    ht = HashTableFactory.New<object, ConstantExpression>( );

                    m_uniqueConstants[ td ] = ht;
                }
            }

            lock( ht )
            {
                ConstantExpression res;

                if( ht.TryGetValue( value, out res ) )
                {
                    return res;
                }
            }

            var valueOrig = value;

            if( dd != null )
            {
                value = dd;
            }
            else if( value != null )
            {
                if( td is ScalarTypeRepresentation )
                {
                    //
                    // These are stored as boxed values.
                    //
                }
                else if( value is ConstantExpression.DelayedValue )
                {
                    //
                    // These are stored as delayed values.
                    //
                }
                else
                {
                    var flags = DataManager.Attributes.Constant | DataManager.Attributes.SuitableForConstantPropagation;

                    value = m_dataManager.ConvertToObjectDescriptor( td, flags, null, value );
                }
            }

            var resNew = new ConstantExpression( td, value );

            lock( ht )
            {
                ht[ valueOrig ] = resNew;
            }

            return resNew;
        }

        public override ConstantExpression CreateConstant( int value )
        {
            return CreateConstant( this.WellKnownTypes.System_Int32, value );
        }

        public override ConstantExpression CreateConstant( uint value )
        {
            return CreateConstant( this.WellKnownTypes.System_UInt32, value );
        }

        public override ConstantExpression CreateConstantFromObject( object value )
        {
            TypeRepresentation tdTarget;
            object             od = m_dataManager.ConvertToObjectDescriptor( value, out tdTarget );

            return CreateConstant( tdTarget, od );
        }

        public override Expression[] CreateConstantsFromObjects( params object[] array )
        {
            var res = new Expression[ array.Length ];

            for( int i = 0; i < res.Length; i++ )
            {
                var value = array[ i ];

                if( value is Expression )
                {
                    res[ i ] = ( Expression )value;
                }
                else if( value == null )
                {
                    res[ i ] = CreateNullPointer( this.WellKnownTypes.System_Object );
                }
                else
                {
                    res[ i ] = CreateConstantFromObject( value );
                }
            }

            return res;
        }

        public override ConstantExpression CreateConstantForType( VTable vTable )
        {
            var val = new DelayedType( this, vTable );

            if( val.CanEvaluate )
            {
                return CreateConstantFromObject( val.Value );
            }
            else
            {
                return CreateConstant( this.WellKnownTypes.System_RuntimeType, val );
            }
        }

        public override ConstantExpression CreateConstantForTypeSize( TypeRepresentation td )
        {
            object val;

            if( td.ValidLayout )
            {
                val = td.Size;
            }
            else
            {
                val = new DelayedSize( td );
            }

            return CreateConstant( this.WellKnownTypes.System_UInt32, val );
        }

        public override ConstantExpression CreateConstantForFieldOffset( FieldRepresentation fd )
        {
            object val;

            if( fd.ValidLayout )
            {
                val = fd.Offset;
            }
            else
            {
                val = new DelayedOffset( fd );
            }

            return CreateConstant( this.WellKnownTypes.System_UInt32, val );
        }

        public override Expression CreateRuntimeHandle( object o )
        {
            DataManager.ObjectDescriptor od = CreateDescriptorForRuntimeHandle( o );

            return CreateConstant( od.Context, od );
        }

        public DataManager.ObjectDescriptor CreateDescriptorForRuntimeHandle( object o )
        {
            WellKnownTypes  wkt = this.WellKnownTypes;
            WellKnownFields wkf = this.WellKnownFields;

            //
            // We have to create an instance of Runtime<X>Handle, and we have to set its m_value field to the proper input.
            // However, we will be called *before* open class definitions have been applied.
            // This is the reason why we declare a type and use the field from a different class.
            //
            if( o is TypeRepresentation )
            {
                var td = ( TypeRepresentation )o;

                return CreateRuntimeHandleDescriptor( wkt.System_RuntimeTypeHandle, wkf.RuntimeTypeHandleImpl_m_value, td.VirtualTable );
            }

            if( o is FieldRepresentation )
            {
                var fd = ( FieldRepresentation )o;

                return CreateRuntimeHandleDescriptor( wkt.System_RuntimeFieldHandle, wkf.RuntimeFieldHandleImpl_m_value, fd );
            }

            if( o is MethodRepresentation )
            {
                var md = ( MethodRepresentation )o;

                return CreateRuntimeHandleDescriptor( wkt.System_RuntimeMethodHandle, wkf.RuntimeMethodHandleImpl_m_value, md );
            }

            throw TypeConsistencyErrorException.Create( "Unexpected input for runtime handle creation: {0}", o );
        }

        private DataManager.ObjectDescriptor CreateRuntimeHandleDescriptor( TypeRepresentation td,
                                                                            FieldRepresentation fd,
                                                                            object val )
        {
            DataManager.ObjectDescriptor od = m_dataManager.BuildObjectDescriptor( td, DataManager.Attributes.Constant | DataManager.Attributes.SuitableForConstantPropagation, null );

            od.ConvertAndSet( ( InstanceFieldRepresentation )fd, DataManager.Attributes.Constant | DataManager.Attributes.SuitableForConstantPropagation, null, val );

            return od;

        }

        //--//

        public override Annotation CreateUniqueAnnotation( Annotation an )
        {
            Annotation oldAn;

            if( m_uniqueAnnotations.Contains( an, out oldAn ) )
            {
                return oldAn;
            }

            lock( TypeSystemForCodeTransformation.Lock )
            {
                if( m_uniqueAnnotations.Contains( an, out oldAn ) )
                {
                    return oldAn;
                }

                m_uniqueAnnotations.Insert( an );

                return an;
            }
        }

        //--//

        public Abstractions.PlacementRequirements GetPlacementRequirements( BaseRepresentation owner )
        {
            Abstractions.PlacementRequirements pr;

            m_placementRequirements.TryGetValue( owner, out pr );

            return pr;
        }

        public Abstractions.PlacementRequirements CreatePlacementRequirements( BaseRepresentation owner )
        {
            Abstractions.PlacementRequirements pr = GetPlacementRequirements( owner );

            if( pr == null )
            {
                pr = new Abstractions.PlacementRequirements( sizeof( uint ), 0 );

                m_placementRequirements[ owner ] = pr;
            }

            return pr;
        }

        //--//

        public override Expression[] AddTypePointerToArgumentsOfStaticMethod( MethodRepresentation md,
                                                                              params Expression[] rhs )
        {
            return ArrayUtility.InsertAtHeadOfNotNullArray( rhs, CreateNullPointer( md.ThisPlusArguments[ 0 ] ) );
        }

        public void SubstituteWithCallToHelper( string name,
                                                Operator op )
        {
            SubstituteWithCallToHelper( GetWellKnownMethod( name ), op );
        }

        public void SubstituteWithCallToHelper( MethodRepresentation md,
                                                Operator op )
        {
            Expression[] rhs = AddTypePointerToArgumentsOfStaticMethod( md, op.Arguments );

            StaticCallOperator opCall = StaticCallOperator.New( op.DebugInfo, CallOperator.CallKind.Direct, md, op.Results, rhs );

            op.SubstituteWithOperator( opCall, Operator.SubstitutionFlags.Default );
        }

        //--//

        public Runtime.HardwareException ExtractHardwareExceptionSettingsForMethod( MethodRepresentation md )
        {
            CustomAttributeRepresentation ca;

            if( m_hardwareExceptionHandlers.TryGetValue( md, out ca ) )
            {
                return ( Runtime.HardwareException )ca.FixedArgsValues[ 0 ];
            }

            return Runtime.HardwareException.None;
        }

        public MethodRepresentation GetHandler( Runtime.HardwareException he )
        {
            MethodRepresentation md = TryGetHandler( he );

            if( md != null )
            {
                return md;
            }

            throw TypeConsistencyErrorException.Create( "Cannot find handler for {0}", he );
        }

        public MethodRepresentation TryGetHandler( Runtime.HardwareException he )
        {
            foreach( MethodRepresentation md in m_hardwareExceptionHandlers.Keys )
            {
                CustomAttributeRepresentation ca = m_hardwareExceptionHandlers[ md ];

                if( ( Runtime.HardwareException )ca.FixedArgsValues[ 0 ] == he )
                {
                    return md;
                }
            }

            return null;
        }

        //--//

        public Runtime.DebuggerHook ExtractDebuggerHookSettingsForMethod( MethodRepresentation md )
        {
            CustomAttributeRepresentation ca;

            if( m_debuggerHookHandlers.TryGetValue( md, out ca ) )
            {
                return ( Runtime.DebuggerHook )ca.FixedArgsValues[ 0 ];
            }

            return Runtime.DebuggerHook.None;
        }

        public MethodRepresentation GetHandler( Runtime.DebuggerHook dh )
        {
            MethodRepresentation md = TryGetHandler( dh );

            if( md != null )
            {
                return md;
            }

            throw TypeConsistencyErrorException.Create( "Cannot find handler for {0}", dh );
        }

        public MethodRepresentation TryGetHandler( Runtime.DebuggerHook dh )
        {
            foreach( MethodRepresentation md in m_debuggerHookHandlers.Keys )
            {
                CustomAttributeRepresentation ca = m_debuggerHookHandlers[ md ];

                if( ( Runtime.DebuggerHook )ca.FixedArgsValues[ 0 ] == dh )
                {
                    return md;
                }
            }

            return null;
        }

        //--//

        public static ControlFlowGraphStateForCodeTransformation GetCodeForMethod( MethodRepresentation md )
        {
            return ( ControlFlowGraphStateForCodeTransformation )md.Code;
        }

        //--//

        public TypeRepresentation GetTypeRepresentationFromObject( object obj )
        {
            return GetTypeRepresentationFromType( obj.GetType( ) );
        }

        public TypeRepresentation GetTypeRepresentationFromType( Type t )
        {
            if( t == null )
            {
                return null;
            }

            TypeRepresentation td = TryGetTypeRepresentationFromType( t );

            if( td != null )
            {
                return td;
            }

            throw TypeConsistencyErrorException.Create( "Cannot lookup a TypeRepresentation for {0}", t );
        }

        public TypeRepresentation TryGetTypeRepresentationFromType( Type t )
        {
            lock( TypeSystemForCodeTransformation.Lock )
            {
                TypeRepresentation res;

                if( m_lookupType.TryGetValue( t, out res ) )
                {
                    return res;
                }

                if( t.IsArray )
                {
                    var tdElement = TryGetTypeRepresentationFromType( t.GetElementType( ) );
                    if( tdElement == null )
                    {
                        return null;
                    }

                    foreach( TypeRepresentation td in this.Types )
                    {
                        if( td is ArrayReferenceTypeRepresentation )
                        {
                            var tdArray = ( ArrayReferenceTypeRepresentation )td;

                            if( tdArray.ContainedType == tdElement )
                            {
                                int rank = t.GetArrayRank( );

                                if( tdArray is SzArrayReferenceTypeRepresentation )
                                {
                                    if( rank == 1 )
                                    {
                                        m_lookupType[ t ] = td;

                                        return td;
                                    }
                                }
                                else
                                {
                                    var tdArray2 = ( MultiArrayReferenceTypeRepresentation )tdArray;

                                    if( tdArray2.Rank == rank )
                                    {
                                        m_lookupType[ t ] = td;

                                        return td;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    string wellKnownName;

                    if( m_lookupTypeLinkedToRuntime.TryGetValue( t, out wellKnownName ) )
                    {
                        return GetWellKnownType( wellKnownName );
                    }

                    //--//

                    string name     = t.Name;
                    string asmlName = t.Assembly.GetName( ).Name;

                    if( t.IsNested )
                    {
                        TypeRepresentation tdEnclosing = TryGetTypeRepresentationFromType( t.DeclaringType );
                        if( tdEnclosing == null )
                        {
                            return null;
                        }

                        foreach( TypeRepresentation td in this.Types )
                        {
                            if( td.Name == name && td.EnclosingClass == tdEnclosing )
                            {
                                if( td.Owner.Name == asmlName )
                                {
                                    m_lookupType[ t ] = td;

                                    return td;
                                }
                            }
                        }
                    }
                    else
                    {
                        string nameSpace = t.Namespace;

                        foreach( TypeRepresentation td in this.Types )
                        {
                            if( td.Name == name && td.Namespace == nameSpace )
                            {
                                if( td.Owner.Name == asmlName )
                                {
                                    m_lookupType[ t ] = td;

                                    return td;
                                }
                            }
                        }
                    }
                }

                return null;
            }
        }

        public Type TryGetTypeFromTypeRepresentation( TypeRepresentation td )
        {
            var wkt = this.WellKnownTypes;

            foreach( System.Reflection.FieldInfo fi in ReflectionHelper.GetAllPublicInstanceFields( wkt.GetType( ) ) )
            {
                var obj = fi.GetValue( wkt ) as TypeRepresentation;

                if( obj == td )
                {
                    var attrib = ReflectionHelper.GetAttribute<LinkToRuntimeTypeAttribute>( fi, false );

                    if( attrib != null )
                    {
                        return attrib.Target;
                    }
                }
            }

            return null;
        }

        //--//

        public Expression GetVTable( TypeRepresentation td )
        {
            TypeRepresentation tdVal = this.WellKnownTypes.Microsoft_Zelig_Runtime_TypeSystem_VTable;

            // Remap boxed type virtual tables to their underlying type's.
            if (td is BoxedValueTypeRepresentation)
            {
                td = td.UnderlyingType;
            }

            DataManager.Attributes attributes = DataManager.Attributes.Constant | DataManager.Attributes.SuitableForConstantPropagation;
            return CreateConstant( tdVal, m_dataManager.ConvertToObjectDescriptor( tdVal, attributes, null, td.VirtualTable ) );
        }

        public Expression GetTypeRepresentation( TypeRepresentation target )
        {
            TypeRepresentation tdTarget;
            object             od = m_dataManager.ConvertToObjectDescriptor( target, out tdTarget );

            return CreateConstant( tdTarget, od );
        }

        //--//

        internal DataManager.ObjectDescriptor GetGlobalRoot( )
        {
            return ( DataManager.ObjectDescriptor )m_globalRoot.Value;
        }

        internal DataManager.ObjectDescriptor GenerateGlobalRoot( )
        {
            lock( TypeSystemForCodeTransformation.Lock )
            {
                if( m_globalRoot == null )
                {
                    DataManager.ObjectDescriptor od = m_dataManager.BuildObjectDescriptor( this.WellKnownTypes.Microsoft_Zelig_Runtime_TypeSystem_GlobalRoot, DataManager.Attributes.Mutable | DataManager.Attributes.SuitableForConstantPropagation, null );

                    m_globalRoot = CreateConstant( this.WellKnownTypes.Microsoft_Zelig_Runtime_TypeSystem_GlobalRoot, od );
                }
            }

            return GetGlobalRoot( );
        }

        internal Expression GenerateConstantForSingleton( TypeRepresentation td,
                                                          DataManager.Attributes flags )
        {
            DataManager.ObjectDescriptor od = GenerateSingleton( td, flags );

            return CreateConstant( od.Context, od );
        }

        internal DataManager.ObjectDescriptor GenerateSingleton( TypeRepresentation td,
                                                                 DataManager.Attributes flags )
        {
            InstanceFieldRepresentation res;

            return GenerateSingleton( td, flags, out res );
        }

        internal DataManager.ObjectDescriptor GenerateSingleton( TypeRepresentation td,
                                                                     DataManager.Attributes flags,
                                                                 out InstanceFieldRepresentation res )
        {
            lock( TypeSystemForCodeTransformation.Lock )
            {
                DataManager.ObjectDescriptor odRoot = GenerateGlobalRoot( );

                //
                // Special case for the GlobalRoot instance itself.
                //
                if( td == odRoot.Context )
                {
                    res = null;

                    return odRoot;
                }

                if( m_globalRootMap.TryGetValue( td, out res ) == false )
                {
                    TypeRepresentation tdRoot = odRoot.Context;

                    string fieldName = string.Format( "Singleton_{0}_{1:X8}", td.FullNameWithAbbreviation, td.ToString( ).GetHashCode( ) );

                    res = new InstanceFieldRepresentation( tdRoot, fieldName, td );

                    m_globalRootMap[ td ] = res;

                    tdRoot.AddField( res );

                    odRoot.Set( res, m_dataManager.BuildObjectDescriptor( td, flags, null ) );
                }

                return ( DataManager.ObjectDescriptor )odRoot.Get( res );
            }
        }

        public DataManager.ObjectDescriptor GetSingleton( TypeRepresentation td )
        {
            if( td != null )
            {
                lock( TypeSystemForCodeTransformation.Lock )
                {
                    InstanceFieldRepresentation res;

                    if( m_globalRoot != null && m_globalRootMap.TryGetValue( td, out res ) )
                    {
                        DataManager.ObjectDescriptor odRoot = GetGlobalRoot( );

                        return ( DataManager.ObjectDescriptor )odRoot.Get( res );
                    }
                }
            }

            return null;
        }

        public TypeRepresentation FindActualSingleton( TypeRepresentation td )
        {
            if( td != null )
            {
                lock( TypeSystemForCodeTransformation.Lock )
                {
                    foreach( TypeRepresentation td2 in m_globalRootMap.Keys )
                    {
                        if( td == td2 || td.IsSuperClassOf( td2, null ) )
                        {
                            return td2;
                        }
                    }
                }
            }

            return null;
        }

        //--//

        public ConstantExpression GenerateRootAccessor( Operator site )
        {
            GenerateGlobalRoot( );

            return m_globalRoot;
        }

        public Expression GenerateSingletonAccessor( Operator site,
                                                     TypeRepresentation td,
                                                     DataManager.Attributes flags,
                                                     bool fNeverNull )
        {
            lock( TypeSystemForCodeTransformation.Lock )
            {
                InstanceFieldRepresentation  res;
                ConstantExpression           exRoot = GenerateRootAccessor( site );
                DataManager.ObjectDescriptor od     = GenerateSingleton( td, flags, out res );
                TemporaryVariableExpression  tmp    = site.BasicBlock.Owner.AllocateTemporary( td, null );

                if( res != null )
                {
                    //
                    // 'exRoot' will never be null, so we don't need to check for it.
                    //
                    LoadInstanceFieldOperator opNew = LoadInstanceFieldOperator.New( site.DebugInfo, res, tmp, exRoot, false );

                    if( fNeverNull )
                    {
                        opNew.AddAnnotation( NotNullAnnotation.Create( this ) );
                    }

                    site.AddOperatorBefore( opNew );
                }
                else
                {
                    var opNew = SingleAssignmentOperator.New( site.DebugInfo, tmp, exRoot );

                    opNew.AddAnnotation( NotNullAnnotation.Create( this ) );
                }

                return tmp;
            }
        }

        public InstanceFieldRepresentation AddStaticFieldToGlobalRoot( StaticFieldRepresentation fd )
        {
            lock( TypeSystemForCodeTransformation.Lock )
            {
                DataManager.ObjectDescriptor odRoot = GenerateGlobalRoot( );
                InstanceFieldRepresentation  res;

                if( m_globalRootMap.TryGetValue( fd, out res ) == false )
                {
                    TypeRepresentation tdRoot = this.WellKnownTypes.Microsoft_Zelig_Runtime_TypeSystem_GlobalRoot;

                    string fieldName = string.Format( "StaticField_{0}_{1}_{2:X8}", fd.OwnerType.FullNameWithAbbreviation, fd.Name, fd.ToString( ).GetHashCode( ) );

                    res = new InstanceFieldRepresentation( tdRoot, fieldName, fd.FieldType );

                    res.LinkAsImplementationOf( fd );

                    tdRoot.AddField( res );

                    m_globalRootMap[ fd ] = res;

                    //
                    // For static fields that are value types, we need to create a nested ObjectDescriptor.
                    //
                    TypeRepresentation fdType = res.FieldType;

                    if( fdType is ValueTypeRepresentation )
                    {
                        DataManager.ObjectDescriptor od = m_dataManager.BuildObjectDescriptor( fdType, DataManager.Attributes.Mutable, null );

                        od.SetNesting( odRoot, res, -1 );

                        odRoot.Set( res, od );
                    }
                }

                return res;
            }
        }

        public InstanceFieldRepresentation GetEntityFromGlobalRoot( BaseRepresentation bd )
        {
            lock( TypeSystemForCodeTransformation.Lock )
            {
                InstanceFieldRepresentation res;

                m_globalRootMap.TryGetValue( bd, out res );

                return res;
            }
        }

        public Expression GenerateConstantArrayAccessor( Operator site,
                                                         ArrayReferenceTypeRepresentation td,
                                                         Array array,
                                                         Abstractions.PlacementRequirements pr )
        {
            return CreateConstant( td, m_dataManager.ConvertToObjectDescriptor( td, DataManager.Attributes.Constant | DataManager.Attributes.SuitableForConstantPropagation, pr, array ) );
        }

        //--//

        public override CodePointer CreateCodePointer( MethodRepresentation md )
        {
            return m_dataManager.CreateCodePointer( md );
        }

        public override bool IncludeInGCInfo( InstanceFieldRepresentation fd )
        {
            return m_garbageCollectionExclusions.Contains( fd ) == false;
        }

        //--//

#if DEBUG_IMAGEBUILDER_PERF
        PerformanceCounters.Timing m_timing;

        private void Debug_TimestampedWriteLine( string format,
                                                 params object[] args )
        {
            Console.Write( "{0,13:F6}: ", ( float )PerformanceCounters.Timing.ToMicroSeconds( m_timing.Sample( ) ) / ( 1000 * 1000 ) );

            Console.WriteLine( format, args );
        }
#endif

        public void GenerateImage( CompilationSteps.PhaseDriver phase )
        {
            m_imageBuilder = new ImageBuilders.Core( this );

            this.PlatformAbstraction.GetListOfMemoryBlocks( m_imageBuilder.MemoryBlocks );

            bool fRecompile = true;
            bool fDone      = false;

#if DEBUG_IMAGEBUILDER_PERF
            int round = 0;

            m_timing.Start( );

            Debug_TimestampedWriteLine( "START ##########################################################################" );
#endif

            while( !fDone )
            {
#if DEBUG_IMAGEBUILDER_PERF
                Debug_TimestampedWriteLine( "Round: {0}", ++round );
#endif

                fDone = true;

                if( fRecompile )
                {
                    fRecompile = false;

#if DEBUG_IMAGEBUILDER_PERF
                    Debug_TimestampedWriteLine( "Emitting..." );
#endif

                    m_imageBuilder.RestartCompilation( );

                    foreach( MethodRepresentation md in this.HardwareExceptionHandlers.Keys )
                    {
                        var cfg = TypeSystemForCodeTransformation.GetCodeForMethod( md );

                        m_imageBuilder.CompileMethod( cfg );
                    }

                    foreach( MethodRepresentation md in this.DebuggerHookHandlers.Keys )
                    {
                        var cfg = TypeSystemForCodeTransformation.GetCodeForMethod( md );

                        m_imageBuilder.CompileMethod( cfg );
                    }

                    foreach( MethodRepresentation md in this.ExportedMethods )
                    {
                        var cfg = TypeSystemForCodeTransformation.GetCodeForMethod( md );

                        m_imageBuilder.CompileMethod( cfg );
                    }

                    // 
                    // Include all methods for LLVM flow only
                    //
                    if(PlatformAbstraction.CodeGenerator == TargetModel.ArmProcessor.InstructionSetVersion.CodeGenerator_LLVM)
                    {
                        foreach(var td in this.Types)
                        {
                            foreach(var md in td.Methods)
                            {
                                //
                                // Do not include exported methods, exceptions and debugger hooks twice
                                //
                                if(this.HardwareExceptionHandlers.ContainsKey( md ) ||
                                   this.DebuggerHookHandlers     .ContainsKey( md ) ||
                                   this.ExportedMethods          .Contains   ( md )  )
                                {
                                    continue;
                                }

                                var cfg = TypeSystemForCodeTransformation.GetCodeForMethod( md );

                                m_imageBuilder.CompileMethod( cfg );
                            }
                        }
                    }

                    m_imageBuilder.ProcessPending( );

#if DEBUG_IMAGEBUILDER_PERF
                    Debug_TimestampedWriteLine( "Done emitting" );
#endif
                }

                //--//
                                    
#if ARMv7M_BUILD__LLVM_IR_ONLY
                if( PlatformAbstraction.CodeGenerator != TargetModel.ArmProcessor.InstructionSetVersion.CodeGenerator_LLVM )
#endif
                {
                    if(m_imageBuilder.AssignAbsoluteAddressesToCode( ) == false)
                    {
#if DEBUG_IMAGEBUILDER_PERF
                        Debug_TimestampedWriteLine( "Failed AssignAbsoluteAddressesToCode" );
#endif

                        fRecompile = true;
                        fDone = false;
                        continue;
                    }
                }

                //--//

                m_dataManager.RefreshValues( phase );

                m_imageBuilder.RewindToDataDescriptorsPhase( );

                if( m_imageBuilder.ImportDataDescriptors( ) == false )
                {
#if DEBUG_IMAGEBUILDER_PERF
                    Debug_TimestampedWriteLine( "Failed ImportDataDescriptors" );
#endif

                    fRecompile = true;
                    fDone = false;
                    continue;
                }

                                    
#if ARMv7M_BUILD__LLVM_IR_ONLY
                if( PlatformAbstraction.CodeGenerator != TargetModel.ArmProcessor.InstructionSetVersion.CodeGenerator_LLVM )
#endif
                {
                    m_imageBuilder.AssignAbsoluteAddressesToDataDescriptors( );
                }
                m_imageBuilder.CommitMemoryMap( );

                //--//
                                    
#if ARMv7M_BUILD__LLVM_IR_ONLY
                if( PlatformAbstraction.CodeGenerator != TargetModel.ArmProcessor.InstructionSetVersion.CodeGenerator_LLVM )
#endif
                {
                    if(m_imageBuilder.ApplyRelocation( ) == false)
                    {
#if DEBUG_IMAGEBUILDER_PERF
                        Debug_TimestampedWriteLine( "Failed ApplyRelocation" );
#endif

                        fRecompile = true;
                        fDone = false;
                        continue;
                    }
                }
                                                    
#if ARMv7M_BUILD__LLVM_IR_ONLY
                if( PlatformAbstraction.CodeGenerator != TargetModel.ArmProcessor.InstructionSetVersion.CodeGenerator_LLVM )
#endif
                {
                    if(m_imageBuilder.CreateCodeMaps( ) == false)
                    {
#if DEBUG_IMAGEBUILDER_PERF
                        Debug_TimestampedWriteLine( "Failed CreateCodeMaps" );
#endif

                        fDone = false;
                    }
                }
                                                    
#if ARMv7M_BUILD__LLVM_IR_ONLY
                if( PlatformAbstraction.CodeGenerator != TargetModel.ArmProcessor.InstructionSetVersion.CodeGenerator_LLVM )
#endif
                {
                    if(m_imageBuilder.CreateExceptionHandlingTables( ) == false)
                    {
#if DEBUG_IMAGEBUILDER_PERF
                        Debug_TimestampedWriteLine( "Failed CreateExceptionHandlingTables" );
#endif

                        fDone = false;
                    }
                }
                                                    
#if ARMv7M_BUILD__LLVM_IR_ONLY
                if( PlatformAbstraction.CodeGenerator != TargetModel.ArmProcessor.InstructionSetVersion.CodeGenerator_LLVM )
#endif
                {
                    if(m_imageBuilder.CreateAvailableMemoryTables( ) == false)
                    {
#if DEBUG_IMAGEBUILDER_PERF
                        Debug_TimestampedWriteLine( "Failed CreateAvailableMemoryTables" );
#endif

                        fDone = false;
                    }
                }
                                                    
#if ARMv7M_BUILD__LLVM_IR_ONLY
                if( PlatformAbstraction.CodeGenerator != TargetModel.ArmProcessor.InstructionSetVersion.CodeGenerator_LLVM )
#endif
                {
                    if(m_imageBuilder.CreateImageRelocationData( ) == false)
                    {
#if DEBUG_IMAGEBUILDER_PERF
                        Debug_TimestampedWriteLine( "Failed CreateImageRelocationData" );
#endif

                        fDone = false;
                    }
                }
            }

#if DEBUG_IMAGEBUILDER_PERF
            Debug_TimestampedWriteLine( "Done" );
#endif

            if( PlatformAbstraction.CodeGenerator == TargetModel.ArmProcessor.InstructionSetVersion.CodeGenerator_LLVM )
            {
                Module.Compile( );
            }

            m_imageBuilder.CacheSourceCore( );
        }

        public void DropCompileTimeObjects( )
        {
            this.ReachabilitySet.ProhibitedSet.Clear( );
                                                
#if ARMv7M_BUILD__LLVM_IR_ONLY
                if( PlatformAbstraction.CodeGenerator != TargetModel.ArmProcessor.InstructionSetVersion.CodeGenerator_LLVM )
#endif
            {
                if(m_imageBuilder != null)
                {
                    m_imageBuilder.DropCompileTimeObjects( );
                }
            }
        }

        //
        // Debug Methods
        //

        static System.IO.StreamWriter s_dump;

        [MethodImplAttribute( MethodImplOptions.Synchronized )]
        protected override void InnerReport( string format,
                                             params object[] args )
        {
            if( s_dump == null )
            {
                string file = string.Format( "typesystem_{0:yyyy-MM-dd--HH-mm-ss}.txt", DateTime.Now );

                s_dump = new System.IO.StreamWriter( new System.IO.FileStream( file, System.IO.FileMode.Create ) );
            }

            s_dump.WriteLine( format, args );
            s_dump.Flush( );
        }

        public void DisassembleImage( string baseDirectory )
        {
            m_imageBuilder.Disassemble( baseDirectory );
        }

        public void DisassembleImageInOneFile( string fileName )
        {
            m_imageBuilder.DisassembleInOneFile( fileName );
        }
    }
}
