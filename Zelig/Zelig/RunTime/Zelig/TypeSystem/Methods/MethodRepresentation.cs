//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TypeSystem
{
    using System;
    using System.Collections.Generic;


    public abstract class MethodRepresentation : BaseRepresentation
    {
        //
        // This is just a copy of Microsoft.Zelig.MetaData.MethodAttributes, needed to break the dependency of TypeSystem from MetaData.
        //
        [Flags]
        public enum Attributes : ushort
        {
            // member access attributes
            MemberAccessMask = 0x0007, // Use this mask to retrieve accessibility information.
            PrivateScope = 0x0000, // Member not referenceable.
            Private = 0x0001, // Accessible only by the parent type.
            FamANDAssem = 0x0002, // Accessible by sub-types only in this Assembly.
            Assem = 0x0003, // Accessibly by anyone in the Assembly.
            Family = 0x0004, // Accessible only by type and sub-types.
            FamORAssem = 0x0005, // Accessibly by sub-types anywhere, plus anyone in assembly.
            Public = 0x0006, // Accessibly by anyone who has visibility to this scope.

            // method contract attributes.
            Static = 0x0010, // Defined on type, else per instance.
            Final = 0x0020, // Method may not be overridden.
            Virtual = 0x0040, // Method virtual.
            HideBySig = 0x0080, // Method hides by name+sig, else just by name.

            // vtable layout mask - Use this mask to retrieve vtable attributes.
            VtableLayoutMask = 0x0100,
            ReuseSlot = 0x0000, // The default.
            NewSlot = 0x0100, // Method always gets a new slot in the vtable.

            Strict = 0x0200,

            // method implementation attributes.
            Abstract = 0x0400, // Method does not provide an implementation.
            SpecialName = 0x0800, // Method is special.  Name describes how.

            // interop attributes
            PinvokeImpl = 0x2000, // Implementation is forwarded through pinvoke.
            UnmanagedExport = 0x0008, // Managed method exported via thunk to unmanaged code.

            // Reserved flags for runtime use only.
            ReservedMask = 0xD000,
            RTSpecialName = 0x1000, // Runtime should check name encoding.
            HasSecurity = 0x4000, // Method has security associate with it.
            RequireSecObject = 0x8000  // Method calls another method containing security code.
        }

        [Flags]
        public enum BuildTimeAttributes : uint
        {
            Inline = 0x00000001,
            NoInline = 0x00000002,

            BottomOfCallStack = 0x00000010,
            SaveFullProcessorContext = 0x00000020,
            NoReturn = 0x00000040,

            CanAllocate = 0x00000100,
            CannotAllocate = 0x00000200,
            CanAllocateOnReturn = 0x00000400,

            StackAvailable = 0x00001000,
            StackNotAvailable = 0x00002000,
            StackAvailableOnReturn = 0x00004000,

            EnableBoundsChecks = 0x00010000,
            DisableDeepBoundsChecks = 0x00020000,
            DisableBoundsChecks = 0x00040000,

            EnableNullChecks = 0x00100000,
            DisableNullChecks = 0x00200000,
            DisableDeepNullChecks = 0x00400000,

            //LON: 2/16/09
            Exported = 0x01000000,
            Imported = 0x02000000,
        }

        public sealed class GenericContext
        {
            //
            // State
            //

            private MethodRepresentation         m_template;
            private TypeRepresentation[]         m_parameters;
            private GenericParameterDefinition[] m_parametersDefinition;

            //
            // Constructor Methods
            //

            public GenericContext( MethodRepresentation template,
                                   TypeRepresentation[] parameters )
            {
                m_template = template;
                m_parameters = parameters;
            }

            //--//

            //
            // Helper Methods
            //

            public void ApplyTransformation( TransformationContext context )
            {
                context.Push( this );

                context.Transform( ref m_template );
                context.Transform( ref m_parameters );
                context.Transform( ref m_parametersDefinition );

                context.Pop( );
            }

            //--//

            //
            // Helper Methods
            //

            internal void CompleteIdentity( TypeSystem typeSystem,
                                                MetaData.Normalized.MetaDataGenericMethodParam[] genericParams,
                                            ref ConversionContext localContext )
            {
                if( m_template == null )
                {
                    m_parametersDefinition = new GenericParameterDefinition[ genericParams.Length ];

                    for( int i = 0; i < genericParams.Length; i++ )
                    {
                        CHECKS.ASSERT( genericParams[ i ].Number == i, "Found out of sync Generic Type Parameter definition" );

                        typeSystem.Analyze_GenericParameterDefinition( genericParams[ i ], ref m_parametersDefinition[ i ], ref localContext );
                    }
                }
            }

            //--//

            //
            // Access Methods
            //

            public MethodRepresentation Template
            {
                get
                {
                    return m_template;
                }
            }

            public TypeRepresentation[] Parameters
            {
                get
                {
                    return m_parameters;
                }
            }

            public GenericParameterDefinition[] ParametersDefinition
            {
                get
                {
                    return m_parametersDefinition;
                }
            }

            public bool IsOpenMethod
            {
                get
                {
                    if( m_parametersDefinition != null )
                    {
                        if( m_parameters.Length == 0 )
                        {
                            return true;
                        }

                        foreach( TypeRepresentation td in m_parameters )
                        {
                            if( td.IsOpenType )
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }
            }
        }

        public static readonly MethodRepresentation[] SharedEmptyArray = new MethodRepresentation[ 0 ];

        //
        // State
        //

        protected TypeRepresentation                m_ownerType;
        protected Attributes                        m_flags;
        protected BuildTimeAttributes               m_buildFlags;
        protected string                            m_name;
        protected TypeRepresentation                m_returnType;
        protected TypeRepresentation[]              m_thisPlusArguments;
        protected string[]                          m_argumentNames;

        protected GenericContext                    m_genericContext;

        //
        // This field is used during code generation to point to the CodeRepresentation for the method.
        //
        protected object                            m_code;
        protected CodePointer                       m_codePointer;
        protected CodeMap                           m_codeMap;

        //
        // Constructor Methods
        //

        protected MethodRepresentation( TypeRepresentation ownerType,
                                        GenericContext genericContext )
        {
            CHECKS.ASSERT( ownerType != null, "Cannot create a MethodRepresentation without supporting metadata" );

            m_ownerType = ownerType;

            m_genericContext = genericContext;
        }

        //--//


        //
        // MetaDataEquality Methods
        //

        public override bool EqualsThroughEquivalence( object obj,
                                                       EquivalenceSet set )
        {
            if( obj is MethodRepresentation )
            {
                MethodRepresentation other = ( MethodRepresentation )obj;
                
                if (                              m_flags       == other.m_flags                        &&
                                                  m_buildFlags  == other.m_buildFlags                   &&
                                                  m_name        == other.m_name                         &&
                        EqualsThroughEquivalence( m_ownerType, other.m_ownerType, set )                 &&
                        EqualsThroughEquivalence( m_returnType, other.m_returnType, set )               &&
                   ArrayEqualsThroughEquivalence( m_thisPlusArguments, other.m_thisPlusArguments, set ) &&
                   ArrayEqualsThroughEquivalence( this.GenericParameters, other.GenericParameters, set ) )
                {
                    CHECKS.ASSERT( this.GetType( ) == other.GetType( ), "Found two inconsistent MethodRepresentation" );

                    return true;
                }
            }

            return false;
        }

        public override bool Equals( object obj )
        {
            return this.EqualsThroughEquivalence( obj, null );
        }

        public override int GetHashCode( )
        {
            return m_ownerType.GetHashCode( ) ^
                   ToShortString( ).GetHashCode( );
        }

        public static bool operator ==( MethodRepresentation left,
                                        MethodRepresentation right )
        {
            return Object.Equals( left, right );
        }

        public static bool operator !=( MethodRepresentation left,
                                        MethodRepresentation right )
        {
            return !( left == right );
        }

        //--//

        //
        // Helper Methods
        //

        protected void Done( TypeSystem typeSystem )
        {
            typeSystem.NotifyNewMethodComplete( this );
        }

        internal void CompleteIdentity( TypeSystem typeSystem,
                                        ref ConversionContext context,
                                            MetaData.Normalized.MetaDataMethodAbstract md )
        {
            if( md is MetaData.Normalized.MetaDataMethodBase )
            {
                MetaData.Normalized.MetaDataMethodBase md2 = ( MetaData.Normalized.MetaDataMethodBase )md;

                m_flags = ( Attributes )md2.Flags;
                m_name = md2.Name;

#if GENERICS_DEBUG
                if(m_ownerType.ToString().Contains( "Microsoft.Zelig.Runtime.KernelNode`1<T>" ) && m_name == "InsertBefore")
                {
                }
#endif

                ConversionContext localContext; SetLocalContext( out localContext, ref context );

                m_returnType = typeSystem.ConvertToIR( md2.Signature.ReturnType, localContext );

                //
                // We populate the arguments array with all the parameters that will be needed to call the method, 'this' pointer included.
                //
                MetaData.Normalized.SignatureType[] parameters = md2.Signature.Parameters;
                int                                 argNum     = parameters == null ? 0 : parameters.Length;

                m_thisPlusArguments = new TypeRepresentation[ 1 + argNum ];

                for( int i = 0; i < argNum; i++ )
                {
                    m_thisPlusArguments[ 1 + i ] = typeSystem.ConvertToIR( parameters[ i ], localContext );
                }

                //--//

#if GENERICS
                TypeRepresentation td = typeSystem.CreateDelayedVersionOfGenericTypeIfNecessary( m_ownerType );
#else
                TypeRepresentation td = m_ownerType;
#endif

                //
                // Methods for value types get a managed pointer to the value type as the "this" argument.
                //
                if( td is ValueTypeRepresentation )
                {
                    td = typeSystem.CreateManagedPointerToType( ( ValueTypeRepresentation )td );
                }

                m_thisPlusArguments[ 0 ] = td;

                //--//

                if( md2 is MetaData.Normalized.MetaDataMethodGeneric )
                {
                    MetaData.Normalized.MetaDataMethodGeneric md3 = ( MetaData.Normalized.MetaDataMethodGeneric )md2;

                    MetaData.Normalized.MetaDataGenericMethodParam[] genericParams = md3.GenericParams;
                    if( genericParams != null )
                    {
                        CHECKS.ASSERT( m_genericContext != null, "Defining a generic method, but {0} doesn't have a generic context", this );

                        m_genericContext.CompleteIdentity( typeSystem, genericParams, ref localContext );
                    }
                }
            }
        }

        internal void PerformMethodAnalysis( TypeSystem typeSystem,
                                             ref ConversionContext context )
        {
            ConversionContext localContext; SetLocalContext( out localContext, ref context );

#if GENERICS
            typeSystem.QueueDelayedMethodAnalysis( this, ref localContext );
#else
            if( this.IsOpenMethod == false && this.OwnerType.IsOpenType == false )
            {
                typeSystem.QueueDelayedMethodAnalysis( this, ref localContext );
            }
#endif

            typeSystem.QueueDelayedCustomAttributeAnalysis( this, ref localContext );
        }

        internal void PerformDelayedCustomAttributeAnalysis( TypeSystem typeSystem,
                                                             ref ConversionContext context )
        {
            MetaData.Normalized.MetaDataMethodBase metadata = typeSystem.GetAssociatedMetaData( this );

            typeSystem.ConvertToIR( metadata.CustomAttributes, context, this, -1 );

            MetaData.Normalized.MetaDataParam[] paramList = metadata.ParamList;
            if( paramList != null )
            {
                bool fGot = false;

                m_argumentNames = new string[ m_thisPlusArguments.Length ];

                if( this is InstanceMethodRepresentation )
                {
                    m_argumentNames[ 0 ] = "this";
                }

                foreach( MetaData.Normalized.MetaDataParam param in paramList )
                {
                    m_argumentNames[ param.Sequence ] = param.Name;

                    if( param.CustomAttributes != null )
                    {
                        fGot = true;
                    }
                }

                if( fGot )
                {
                    for( int paramIndex = 0; paramIndex < paramList.Length; paramIndex++ )
                    {
                        typeSystem.ConvertToIR( paramList[ paramIndex ].CustomAttributes, context, this, paramIndex );
                    }
                }
            }

            Done( typeSystem );
        }

        //--//

        private void SetLocalContext( out ConversionContext localContext,
                                      ref ConversionContext context )
        {
            localContext = context;

            if( this.OwnerType is ArrayReferenceTypeRepresentation )
            {
                ; // The only methods on array types are system-generated, so keep the context as passed in.
            }
            else
            {
                localContext.SetContextAsType( this.OwnerType );
                localContext.SetContextAsMethod( this );
            }
        }

        //--//

        internal void CloneSettings( TypeSystem typeSystem,
                                     MethodRepresentation mdSource )
        {
            m_flags = mdSource.m_flags;
            m_name = mdSource.m_name;
            m_returnType = mdSource.m_returnType;
            m_thisPlusArguments = ArrayUtility.CopyNotNullArray( mdSource.m_thisPlusArguments );
            m_argumentNames = mdSource.m_argumentNames;

            TypeRepresentation td = m_ownerType;

            // Instance method for value types get a managed pointer to the value type as the "this" argument.
            if( td is ValueTypeRepresentation )
            {
                td = typeSystem.GetManagedPointerToType( ( ValueTypeRepresentation )td );
            }

            m_thisPlusArguments[ 0 ] = td;
        }

        //--//

        internal void PrepareToSubstitute( MethodRepresentation md )
        {
            if( this is VirtualMethodRepresentation )
            {
                if( md is VirtualMethodRepresentation )
                {
                    //
                    // Make sure the "slottiness" is the same.
                    //
                    this.VTableLayoutFlags = md.VTableLayoutFlags;
                }
                else
                {
                    throw TypeConsistencyErrorException.Create( "Cannot substitute a non-virtual method with a virtual one: {0} => {1}", this, md );
                }

            }
            else
            {
                if( md is VirtualMethodRepresentation )
                {
                    throw TypeConsistencyErrorException.Create( "Cannot substitute a virtual method with a non-virtual one: {0} => {1}", this, md );
                }
            }
        }

        //--//

        public override void ApplyTransformation( TransformationContext context )
        {
            context.Push( this );

            base.ApplyTransformation( context );

            context.Transform( ref m_ownerType );
            context.Transform( ref m_flags );
            context.Transform( ref m_buildFlags );
            context.Transform( ref m_name );
            context.Transform( ref m_returnType );
            context.Transform( ref m_thisPlusArguments );
            context.Transform( ref m_argumentNames );

            context.Transform( ref m_genericContext );

            context.Transform( ref m_code );
            context.Transform( ref m_codePointer );
            context.TransformGeneric( ref m_codeMap );

            context.Pop( );
        }

        //--//

        public MethodRepresentation Instantiate( InstantiationContext ic )
        {
            TypeRepresentation ownerType      = ic.Instantiate( m_ownerType );
            GenericContext     genericContext = new GenericContext( this, ic.MethodParameters );

            MethodRepresentation instantiatedMd = AllocateInstantiation( ic, ownerType, genericContext );

            instantiatedMd.PopulateInstantiation( this, ic );

            MethodRepresentation instantiatedMdRes = ic.Lookup( instantiatedMd, true );
            if( instantiatedMdRes != null )
            {
                return instantiatedMdRes;
            }
            else
            {
                ownerType.AddMethod( instantiatedMd );

                return instantiatedMd;
            }
        }

        protected abstract MethodRepresentation AllocateInstantiation( InstantiationContext ic,
                                                                       TypeRepresentation ownerType,
                                                                       GenericContext genericContext );

        protected virtual void PopulateInstantiation( MethodRepresentation mdTemplate,
                                                      InstantiationContext ic )
        {
            m_flags = mdTemplate.m_flags;
            m_buildFlags = mdTemplate.m_buildFlags;
            m_name = mdTemplate.m_name;
            m_returnType = ic.Instantiate( mdTemplate.m_returnType );
            m_thisPlusArguments = ic.Instantiate( mdTemplate.m_thisPlusArguments );
            m_argumentNames = mdTemplate.m_argumentNames;
        }

        //--//

        internal void CreateCodePointer( TypeSystem typeSystem )
        {
            if( m_codePointer.Target == IntPtr.Zero )
            {
                if( this.IsOpenMethod == false && m_ownerType.IsOpenType == false )
                {
                    m_codePointer = typeSystem.CreateCodePointer( this );
                }
            }
        }

        //
        // Access Methods
        //

        public TypeRepresentation OwnerType
        {
            get
            {
                return m_ownerType;
            }
        }

        public Attributes Flags
        {
            get
            {
                return m_flags;
            }
        }

        public Attributes VTableLayoutFlags
        {
            get
            {
                return m_flags & MethodRepresentation.Attributes.VtableLayoutMask;
            }

            set
            {
                m_flags &= ~MethodRepresentation.Attributes.VtableLayoutMask;
                m_flags |= value & MethodRepresentation.Attributes.VtableLayoutMask;
            }
        }

        public BuildTimeAttributes BuildTimeFlags
        {
            get
            {
                return m_buildFlags;
            }

            set
            {
                m_buildFlags = value;
            }
        }

        public BuildTimeAttributes ExpandedBuildTimeFlags
        {
            get
            {
                var res = m_buildFlags;

                var tdTemplate = this.GenericTemplate;
                if( tdTemplate != null )
                {
                    res |= tdTemplate.ExpandedBuildTimeFlags;
                }

                return res;
            }
        }

        public bool HasBuildTimeFlag( BuildTimeAttributes bta )
        {
            return ( this.ExpandedBuildTimeFlags & bta ) != 0;
        }

        public string Name
        {
            get
            {
                return m_name;
            }
        }

        public TypeRepresentation ReturnType
        {
            get
            {
                return m_returnType;
            }
        }

        public TypeRepresentation[] ThisPlusArguments
        {
            get
            {
                return m_thisPlusArguments;
            }
        }

        public string[] ArgumentNames
        {
            get
            {
                return m_argumentNames;
            }
        }

        public bool IsGenericInstantiation
        {
            get
            {
                return this.IsOpenMethod == false && this.GenericParameters.Length > 0;
            }
        }

        public GenericContext Generic
        {
            get
            {
                return m_genericContext;
            }
        }

        public MethodRepresentation GenericTemplate
        {
            get
            {
                if( m_genericContext != null )
                {
                    return m_genericContext.Template;
                }

                return null;
            }
        }

        public TypeRepresentation[] GenericParameters
        {
            get
            {
                if( m_genericContext != null )
                {
                    return m_genericContext.Parameters;
                }

                return TypeRepresentation.SharedEmptyArray;
            }
        }

        public GenericParameterDefinition[] GenericParametersDefinition
        {
            get
            {
                if( m_genericContext != null )
                {
                    return m_genericContext.ParametersDefinition;
                }

                return null;
            }
        }

        public bool IsOpenMethod
        {
            get
            {
                if( m_genericContext != null )
                {
                    if( m_genericContext.IsOpenMethod )
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public Attributes MemberAccess
        {
            get
            {
                return m_flags & MethodRepresentation.Attributes.MemberAccessMask;
            }
        }

        //--//

        public object Code
        {
            get
            {
                return m_code;
            }

            set
            {
                m_code = value;
            }
        }

        public CodePointer CodePointer
        {
            get
            {
                return m_codePointer;
            }
        }

        public CodeMap CodeMap
        {
            get
            {
                return m_codeMap;
            }

            set
            {
                m_codeMap = value;
            }
        }

        //--//

        public CustomAttributeRepresentation FindCustomAttributeForParam( TypeRepresentation target,
                                                                          int param )
        {
            return FindCustomAttributeForParam( target, param, -1 );
        }

        public CustomAttributeRepresentation FindCustomAttributeForParam( TypeRepresentation target,
                                                                          int param,
                                                                          int index )
        {
            return FindCustomAttribute( target, param, index );
        }

        //--//

        public MethodRepresentation FindOverriddenMethod( )
        {
            TypeRepresentation td = m_ownerType;

            while( true )
            {
                td = td.Extends;
                if( td == null )
                {
                    break;
                }

                MethodRepresentation md = td.FindMatch( m_name, this, null );
                if( md != null )
                {
                    return md;
                }
            }

            return null;
        }

        //--//

        public bool MatchNameAndSignature( string name,
                                           MethodRepresentation sig,
                                           EquivalenceSet set )
        {
            return m_name == name && MatchSignature( sig, set );
        }

        public bool MatchNameAndSignature( string name,
                                           TypeRepresentation returnType,
                                           TypeRepresentation[] thisPlusArguments,
                                           EquivalenceSet set )
        {
            return m_name == name && MatchSignature( returnType, thisPlusArguments, set );
        }

        public bool MatchSignature( MethodRepresentation sig,
                                    EquivalenceSet set )
        {
            return MatchSignature( sig.m_returnType, sig.m_thisPlusArguments, set );
        }

        public bool MatchSignature( TypeRepresentation returnType,
                                    TypeRepresentation[] thisPlusArguments,
                                    EquivalenceSet set )
        {
            //
            // Don't compare the first argument, a method signature does not include any 'this' pointer.
            //
            if( ArrayEqualsThroughEquivalence( thisPlusArguments, m_thisPlusArguments, 1, -1, set ) )
            {
                if( returnType.CanBeAssignedFrom( m_returnType, set ) )
                {
                    return true;
                }
            }

            return false;
        }

        //--//

        public int FindInterfaceTableIndex( )
        {
            InterfaceTypeRepresentation itf = m_ownerType as InterfaceTypeRepresentation;
            if( itf != null )
            {
                return ArrayUtility.FindInNotNullArray( itf.FindInterfaceTable( itf ), this );
            }

            return -1;
        }

        public int FindVirtualTableIndex( )
        {
            MethodRepresentation[] methodTable = m_ownerType.MethodTable;

            return ArrayUtility.FindInNotNullArray( methodTable, this );
        }

        public MethodRepresentation FindVirtualTarget( TypeRepresentation tdTarget )
        {
            if( tdTarget.MethodTable == null )
            {
                //
                // Gracefully fail if method tables have not been initialized.
                //
                return null;
            }

            InterfaceTypeRepresentation itf = m_ownerType as InterfaceTypeRepresentation;
            if( itf == null )
            {
                CHECKS.ASSERT( tdTarget.IsSubClassOf( m_ownerType, null ) || tdTarget == m_ownerType, "Cannot find virtual target in a super class" );

                return tdTarget.MethodTable[ FindVirtualTableIndex( ) ];
            }
            else
            {
                MethodRepresentation[] mdArray = tdTarget.FindInterfaceTable( itf );

                CHECKS.ASSERT( mdArray != null, "Target {0} does not implement interface {1}", tdTarget.FullNameWithAbbreviation, itf.FullName );

                return mdArray[ FindInterfaceTableIndex( ) ];
            }
        }

        //--//

        //
        // Debug Methods
        //

        public String ToShortString( )
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder( );

            PrettyToString( sb, true, true );

            return sb.ToString( );
        }

        public String ToShortStringNoReturnValue( )
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder( );

            PrettyToString( sb, true, false );

            return sb.ToString( );
        }

        protected void PrettyToString( System.Text.StringBuilder sb,
                                       bool fPrefix,
                                       bool fIncludeReturnValue )
        {
            if( fPrefix )
            {
                if( this.IsOpenMethod )
                {
                    sb.Append( "generic " );
                }
            }

            if( fIncludeReturnValue )
            {
                if( m_returnType != null )
                {
                    m_returnType.PrettyToString( sb, fPrefix, true );
                    sb.Append( " " );
                }
            }

            m_ownerType.PrettyToString( sb, fPrefix, false );
            sb.Append( "::" );
            sb.Append( m_name );

            if( m_genericContext != null )
            {
                sb.Append( "<" );

                TypeRepresentation[] parameters = m_genericContext.Parameters;
                if( parameters.Length > 0 )
                {
                    for( int i = 0; i < parameters.Length; i++ )
                    {
                        if( i != 0 ) sb.Append( "," );

                        parameters[ i ].PrettyToString( sb, fPrefix, true );
                    }
                }
                else
                {
                    GenericParameterDefinition[] defs = m_genericContext.ParametersDefinition;
                    if( defs != null )
                    {
                        for( int i = 0; i < defs.Length; i++ )
                        {
                            if( i != 0 ) sb.Append( ", " );

                            TypeRepresentation[] genericParamConstraints = defs[ i ].Constraints;
                            if( genericParamConstraints.Length > 0 )
                            {
                                sb.Append( "(" );

                                for( int j = 0; j < genericParamConstraints.Length; j++ )
                                {
                                    TypeRepresentation td = genericParamConstraints[ j ];

                                    if( j != 0 )
                                    {
                                        sb.Append( ", " );
                                    }

                                    td.PrettyToString( sb, false, true );
                                }

                                sb.Append( ")" );
                            }

                            sb.Append( defs[ i ].Name );
                        }
                    }
                }

                sb.Append( ">" );
            }

            sb.Append( "(" );
            if( m_thisPlusArguments != null )
            {
                for( int i = 1; i < m_thisPlusArguments.Length; i++ )
                {
                    if( i != 1 ) sb.Append( "," );

                    TypeRepresentation arg = m_thisPlusArguments[ i ];
                    if( arg != null )
                    {
                        arg.PrettyToString( sb, fPrefix, true );
                    }
                }
            }
            else
            {
                sb.Append( "...pending..." );
            }

            sb.Append( ")" );
        }
    }
}
