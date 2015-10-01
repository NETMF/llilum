//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TypeSystem
{
    using System;
    using System.Collections.Generic;


    public abstract class TypeRepresentation : BaseRepresentation
    {
        //
        // This is just a copy of Microsoft.Zelig.MetaData.TypeAttributes, needed to break the dependency of TypeSystem from MetaData.
        //
        [Flags]
        public enum Attributes
        {
            VisibilityMask     = 0x00000007,
            NotPublic          = 0x00000000, // Class is not public scope.
            Public             = 0x00000001, // Class is public scope.
            NestedPublic       = 0x00000002, // Class is nested with public visibility.
            NestedPrivate      = 0x00000003, // Class is nested with private visibility.
            NestedFamily       = 0x00000004, // Class is nested with family visibility.
            NestedAssembly     = 0x00000005, // Class is nested with assembly visibility.
            NestedFamANDAssem  = 0x00000006, // Class is nested with family and assembly visibility.
            NestedFamORAssem   = 0x00000007, // Class is nested with family or assembly visibility.

            // Use this mask to retrieve class layout information
            // 0 is AutoLayout, 0x2 is SequentialLayout, 4 is ExplicitLayout
            LayoutMask         = 0x00000018,
            AutoLayout         = 0x00000000, // Class fields are auto-laid out
            SequentialLayout   = 0x00000008, // Class fields are laid out sequentially
            ExplicitLayout     = 0x00000010, // Layout is supplied explicitly
            // end layout mask

            // Use this mask to distinguish a type declaration as a Class, ValueType or Interface
            ClassSemanticsMask = 0x00000020,
            Class              = 0x00000000, // Type is a class.
            Interface          = 0x00000020, // Type is an interface.

            // Special semantics in addition to class semantics.
            Abstract           = 0x00000080, // Class is abstract
            Sealed             = 0x00000100, // Class is concrete and may not be extended
            SpecialName        = 0x00000400, // Class name is special.  Name describes how.

            // Implementation attributes.
            Import             = 0x00001000, // Class / interface is imported
            Serializable       = 0x00002000, // The class is Serializable.

            // Use tdStringFormatMask to retrieve string information for native interop
            StringFormatMask   = 0x00030000,
            AnsiClass          = 0x00000000, // LPTSTR is interpreted as ANSI in this class
            UnicodeClass       = 0x00010000, // LPTSTR is interpreted as UNICODE
            AutoClass          = 0x00020000, // LPTSTR is interpreted automatically
            CustomFormatClass  = 0x00030000, // A non-standard encoding specified by CustomFormatMask
            CustomFormatMask   = 0x00C00000, // Use this mask to retrieve non-standard encoding information for native interop. The meaning of the values of these 2 bits is unspecified.

            // end string format mask

            BeforeFieldInit    = 0x00100000, // Initialize the class any time before first static field access.

            // Flags reserved for runtime use.
            ReservedMask       = 0x00040800,
            RTSpecialName      = 0x00000800, // Runtime should check name encoding.
            HasSecurity        = 0x00040000, // Class has security associate with it.

            //--//

            None               = 0x00000000,
        }

        //
        // This is just a copy of Microsoft.Zelig.MetaData.ElementTypes, needed to break the dependency of TypeSystem from MetaData.
        //
        public enum BuiltInTypes : byte
        {
            END         = 0x00,
            VOID        = 0x01,
            BOOLEAN     = 0x02,
            CHAR        = 0x03,
            I1          = 0x04,
            U1          = 0x05,
            I2          = 0x06,
            U2          = 0x07,
            I4          = 0x08,
            U4          = 0x09,
            I8          = 0x0A,
            U8          = 0x0B,
            R4          = 0x0C,
            R8          = 0x0D,
            STRING      = 0x0E,
            ///////////////////////// every type above PTR will be simple type
            PTR         = 0x0F,    // PTR <type>
            BYREF       = 0x10,    // BYREF <type>
            ///////////////////////// Please use VALUETYPE. VALUECLASS is deprecated.
            VALUETYPE   = 0x11,    // VALUETYPE <class Token>
            CLASS       = 0x12,    // CLASS <class Token>
            VAR         = 0x13,    // a class type variable VAR <U1>
            ARRAY       = 0x14,    // MDARRAY <type> <rank> <bcount> <bound1> ... <lbcount> <lb1> ...
            GENERICINST = 0x15,    // instantiated type
            TYPEDBYREF  = 0x16,    // This is a simple type.
            I           = 0x18,    // native integer size
            U           = 0x19,    // native unsigned integer size
            FNPTR       = 0x1B,    // FNPTR <complete sig for the function including calling convention>
            OBJECT      = 0x1C,    // Shortcut for System.Object
            SZARRAY     = 0x1D,    // Shortcut for single dimension zero lower bound array
            ///////////////////////// SZARRAY <type>
            ///////////////////////// This is only for binding
            MVAR        = 0x1E,    // a method type variable MVAR <U1>
            CMOD_REQD   = 0x1F,    // required C modifier : E_T_CMOD_REQD <mdTypeRef/mdTypeDef>
            CMOD_OPT    = 0x20,    // optional C modifier : E_T_CMOD_OPT <mdTypeRef/mdTypeDef>
            ///////////////////////// This is for signatures generated internally (which will not be persisted in any way).
            INTERNAL    = 0x21,    // INTERNAL <typehandle>
            ///////////////////////// Note that this is the max of base type excluding modifiers
            MAX         = 0x22,    // first invalid element type
            MODIFIER    = 0x40,
            SENTINEL    = 0x01 | MODIFIER, // sentinel for varargs
            PINNED      = 0x05 | MODIFIER
        }

        [Flags]
        public enum BuildTimeAttributes : uint
        {
            ForceDevirtualization = 0x00000001,
            ImplicitInstance      = 0x00000002,

            NoVTable              = 0x00000004, // The class does not have a vtable.

            FlagsAttribute        = 0x00000010,
        }

        public enum InstantiationFlavor
        {
            Delayed  ,
            Class    ,
            ValueType,
        }

        public sealed class GenericContext
        {
            //
            // State
            //

            private TypeRepresentation           m_template;
            private TypeRepresentation[]         m_parameters;
            private GenericParameterDefinition[] m_parametersDefinition;

            //
            // Constructor Methods
            //

            public GenericContext( TypeRepresentation   template   ,
                                   TypeRepresentation[] parameters )
            {
                m_template   = template;
                m_parameters = parameters;

                if(template != null && parameters != null)
                {
                    CHECKS.ASSERT( template.GenericParametersDefinition.Length <= parameters.Length, "Mismatch" );

                    if(template.GenericParametersDefinition.Length < parameters.Length)
                    {
                        parameters = ArrayUtility.ExtractSliceFromNotNullArray( parameters, 0, template.GenericParametersDefinition.Length );
                    }
                }
            }

            //--//

            //
            // Helper Methods
            //

            public void ApplyTransformation( TransformationContext context )
            {
                context.Push( this );

                context.Transform( ref m_template             );
                context.Transform( ref m_parameters           );
                context.Transform( ref m_parametersDefinition );

                context.Pop();
            }

            //--//

            //
            // Helper Methods
            //

            internal void CompleteIdentity(     TypeSystem                                     typeSystem    ,
                                                MetaData.Normalized.MetaDataGenericTypeParam[] genericParams ,
                                            ref ConversionContext                              localContext  )
            {
                if(m_template == null)
                {
                    m_parametersDefinition = new GenericParameterDefinition[genericParams.Length];

                    for(int i = 0; i < genericParams.Length; i++)
                    {
                        CHECKS.ASSERT( genericParams[i].Number == i, "Found out of sync Generic Type Parameter definition" );

                        typeSystem.Analyze_GenericParameterDefinition( genericParams[i], ref m_parametersDefinition[i], ref localContext );
                    }
                }
            }

            //--//

            //
            // Access Methods
            //

            public TypeRepresentation Template
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

            public bool IsOpenType
            {
                get
                {
                    if(m_parametersDefinition != null)
                    {
                        if(m_parameters.Length == 0)
                        {
                            return true;
                        }

                        foreach(TypeRepresentation td in m_parameters)
                        {
                            if(td.IsOpenType)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }
            }

            public bool IsDelayedType
            {
                get
                {
                    if(m_parameters.Length == 0)
                    {
                        return false;
                    }

                    foreach(TypeRepresentation td in m_parameters)
                    {
                        if(td.IsDelayedType)
                        {
                            return true;
                        }
                    }

                    return false;
                }
            }
        }

        public struct InterfaceMap
        {
            public static readonly InterfaceMap[] SharedEmptyArray = new InterfaceMap[0];

            //
            // State
            //

            public InterfaceTypeRepresentation Interface;
            public MethodRepresentation[]      Methods;
        }

        public static readonly TypeRepresentation[] SharedEmptyArray = new TypeRepresentation[0];

        //
        // State
        //

        protected AssemblyRepresentation          m_owner;
        protected BuiltInTypes                    m_builtinType;
        protected Attributes                      m_flags;
        protected BuildTimeAttributes             m_buildFlags;
        protected string                          m_name;
        protected string                          m_namespace;
        protected TypeRepresentation              m_enclosingClass;

        protected GenericContext                  m_genericContext;

        protected TypeRepresentation              m_extends;
        protected InterfaceTypeRepresentation[]   m_interfaces;
        protected FieldRepresentation[]           m_fields;
        protected MethodRepresentation[]          m_methods;
        protected MethodImplRepresentation[]      m_methodImpls;

        //
        // These fields are synthesized from the analysis of the type hierarchy and the type characteristics.
        //
        [WellKnownField( "TypeRepresentation_InterfaceMethodTables" )] protected InterfaceMap[]         m_interfaceMethodTables;
        [WellKnownField( "TypeRepresentation_MethodTable" )]           protected MethodRepresentation[] m_methodTable;
        [WellKnownField( "TypeRepresentation_VTable" )]                protected VTable                 m_vTable;

        //
        // Constructor Methods
        //

        protected TypeRepresentation( AssemblyRepresentation owner       ,
                                      BuiltInTypes           builtinType ,
                                      Attributes             flags       ) : this( owner, builtinType, flags, null )
        {
        }

        protected TypeRepresentation( AssemblyRepresentation owner          ,
                                      BuiltInTypes           builtinType    ,
                                      Attributes             flags          ,
                                      GenericContext         genericContext )
        {
            m_owner                 = owner;
            m_builtinType           = builtinType;
            m_flags                 = flags;

            m_genericContext        = genericContext;

            m_interfaces            = InterfaceTypeRepresentation.SharedEmptyArray;
            m_fields                = FieldRepresentation        .SharedEmptyArray;
            m_methods               = MethodRepresentation       .SharedEmptyArray;
            m_methodImpls           = MethodImplRepresentation   .SharedEmptyArray;

            m_interfaceMethodTables = null;
            m_methodTable           = null;

            m_vTable                = new VTable( this );
            m_vTable.BaseSize       = uint.MaxValue;
        }

        //
        // MetaDataEquality Methods
        //

        public override bool EqualsThroughEquivalence( object         obj ,
                                                       EquivalenceSet set )
        {
            if(obj is TypeRepresentation)
            {
                TypeRepresentation other = (TypeRepresentation)obj;

                if(                               m_builtinType          == other.m_builtinType            &&
                                                  m_flags                == other.m_flags                  &&
                                                  m_buildFlags           == other.m_buildFlags             &&
                                                  m_name                 == other.m_name                   &&
                                                  m_namespace            == other.m_namespace              &&
                        EqualsThroughEquivalence( m_enclosingClass       ,  other.m_enclosingClass , set ) &&
                   ArrayEqualsThroughEquivalence( this.GenericParameters ,  other.GenericParameters, set )  )
                {
                    CHECKS.ASSERT( this.GetType() == other.GetType(), "Found two inconsistent TypeRepresentations" );

                    return true;
                }
            }

            return false;
        }

        public override bool Equals( object obj )
        {
            return this.EqualsThroughEquivalence( obj, null );
        }

        public override int GetHashCode()
        {
            return (int)m_builtinType ^
                        m_name.GetHashCode();
        }

        [Inline]
        public static bool operator ==( TypeRepresentation left  ,
                                        TypeRepresentation right )
        {
            return Object.Equals( left, right );
        }

        [Inline]
        public static bool operator !=( TypeRepresentation left  ,
                                        TypeRepresentation right )
        {
            return !(left == right);
        }

        //--//

        //
        // Helper Methods
        //

        protected void Done( TypeSystem typeSystem )
        {
            typeSystem.NotifyNewTypeComplete( this );
        }

        protected abstract void SetShapeCategory( TypeSystem typeSystem );

        internal void CompleteIdentity(     TypeSystem                                         typeSystem ,
                                        ref ConversionContext                                  context    ,
                                            MetaData.Normalized.MetaDataTypeDefinitionAbstract td         )
        {
            SetShapeCategory( typeSystem );

            //--//

            if(td is MetaData.Normalized.MetaDataTypeDefinitionBase)
            {
                MetaData.Normalized.MetaDataTypeDefinitionBase td2 = (MetaData.Normalized.MetaDataTypeDefinitionBase)td;

                m_name           =                         td2.Name;
                m_namespace      =                         td2.Namespace;
                m_enclosingClass = typeSystem.ConvertToIR( td2.EnclosingClass, context, true );

                if(td is MetaData.Normalized.MetaDataTypeDefinitionGeneric)
                {
                    MetaData.Normalized.MetaDataTypeDefinitionGeneric td3 = (MetaData.Normalized.MetaDataTypeDefinitionGeneric)td;

                    ConversionContext localContext = context; localContext.SetContextAsType( this );

                    MetaData.Normalized.MetaDataGenericTypeParam[] genericParams = td3.GenericParams;
                    if(genericParams != null)
                    {
                        CHECKS.ASSERT(m_genericContext != null, "Defining a generic type, but {0} doesn't have a generic context", this );

                        m_genericContext.CompleteIdentity( typeSystem, genericParams, ref localContext );
                    }
                }
            }
        }

        //--//

        internal void PerformTypeAnalysis(     TypeSystem        typeSystem ,
                                           ref ConversionContext context    )
        {
#if GENERICS
            typeSystem.QueueDelayedTypeAnalysis( this, ref context );
#else
            if(this.IsDelayedType == false)
            {
                typeSystem.QueueDelayedTypeAnalysis( this, ref context );
            }
            else
            {
                Done( typeSystem );
            }
#endif
        }

        internal void PerformDelayedTypeAnalysis(     TypeSystem        typeSystem ,
                                                  ref ConversionContext context    )
        {
            PerformInnerDelayedTypeAnalysis( typeSystem, ref context );

            Done( typeSystem );
        }

        protected abstract void PerformInnerDelayedTypeAnalysis(     TypeSystem        typeSystem ,
                                                                 ref ConversionContext context    );

        internal void PerformDelayedCustomAttributeAnalysis(     TypeSystem        typeSystem ,
                                                             ref ConversionContext context    )
        {
            MetaData.Normalized.MetaDataTypeDefinitionAbstract metadata = typeSystem.GetAssociatedMetaData( this );

            typeSystem.ConvertToIR( metadata.CustomAttributes, context, this, -1 );

            foreach(FieldRepresentation fd in m_fields)
            {
                fd.ProcessCustomAttributes( typeSystem, ref context );
            }
        }

        //--//

        protected void AnalyzeEntries(     TypeSystem                                           typeSystem  ,
                                       ref ConversionContext                                    context     ,
                                           MetaData.Normalized.MetaDataTypeDefinitionAbstract[] interfaces  ,
                                           MetaData.Normalized.MetaDataField[]                  fields      ,
                                           MetaData.Normalized.MetaDataMethodBase[]             methods     ,
                                           MetaData.Normalized.MetaDataMethodImpl[]             methodImpls )
        {
            if(interfaces != null)
            {
                AnalyzeInterfaces( typeSystem, ref context, interfaces );
            }

            if(fields != null)
            {
                AnalyzeFields( typeSystem, ref context, fields );
            }

            if(methods != null)
            {
                AnalyzeMethods( typeSystem, ref context, methods );
            }

            if(methodImpls != null)
            {
                AnalyzeMethodImpls( typeSystem, ref context, methodImpls );
            }

            //--//

            if(this is InterfaceTypeRepresentation)
            {
                ; // Interfaces cannot have an interface map.
            }
            else if(this.IsOpenType)
            {
                ; // There's no point in building an interface map for a type that cannot be instantiated.
            }
            else
            {
                ;
            }

            typeSystem.QueueDelayedCustomAttributeAnalysis( this, ref context );
        }

        private void AnalyzeInterfaces(     TypeSystem                                           typeSystem ,
                                        ref ConversionContext                                    context    ,
                                            MetaData.Normalized.MetaDataTypeDefinitionAbstract[] interfaces )
        {
            foreach(MetaData.Normalized.MetaDataTypeDefinitionAbstract itf in interfaces)
            {
                TypeRepresentation itfRes = typeSystem.ConvertToIR( itf, context );

                if(itfRes is InterfaceTypeRepresentation)
                {
                    InterfaceTypeRepresentation itfNew = (InterfaceTypeRepresentation)itfRes;

                    this.AddInterface( itfNew );
                }
                else
                {
                    throw TypeConsistencyErrorException.Create( "'{0}' is not an interface", itf );
                }
            }
        }

        private void AnalyzeFields(     TypeSystem                          typeSystem ,
                                    ref ConversionContext                   context    ,
                                        MetaData.Normalized.MetaDataField[] fields     )
        {
            foreach(MetaData.Normalized.MetaDataField fd in fields)
            {
                if((fd.Flags & MetaData.FieldAttributes.Literal) == 0)
                {
                    typeSystem.ConvertToIR( this, fd, context );
                }
                else
                {
                    var enu = this as EnumerationTypeRepresentation;
                    if(enu != null)
                    {
                        var val = fd.DefaultValue;

                        if(val != null)
                        {
                            enu.AddLiteral( fd.Name, val );
                        }
                    }
                }
            }
        }

        private void AnalyzeMethods(     TypeSystem                               typeSystem ,
                                     ref ConversionContext                        context    ,
                                         MetaData.Normalized.MetaDataMethodBase[] methods    )
        {
            foreach(MetaData.Normalized.MetaDataMethodBase md in methods)
            {
                typeSystem.ConvertToIR( md, context );
            }
        }

        private void AnalyzeMethodImpls(     TypeSystem                               typeSystem  ,
                                         ref ConversionContext                        context     ,
                                             MetaData.Normalized.MetaDataMethodImpl[] methodImpls )
        {
            foreach(MetaData.Normalized.MetaDataMethodImpl mi in methodImpls)
            {
                MethodRepresentation body        = typeSystem.ConvertToIR( mi.Body       , context );
                MethodRepresentation declaration = typeSystem.ConvertToIR( mi.Declaration, context );

                MethodImplRepresentation miRes = new MethodImplRepresentation( body, declaration );

                this.AddMethodImpl( miRes );
            }
        }

        //--//

        public override void ApplyTransformation( TransformationContext context )
        {
            context.Push( this );

            base.ApplyTransformation( context );

            context.Transform( ref m_owner          );
            context.Transform( ref m_builtinType    );
            context.Transform( ref m_flags          );
            context.Transform( ref m_buildFlags     );
            context.Transform( ref m_name           );
            context.Transform( ref m_namespace      );
            context.Transform( ref m_enclosingClass );
                                            
            context.Transform( ref m_genericContext );
                                            
            context.Transform( ref m_extends        );
            context.Transform( ref m_interfaces     );
            context.Transform( ref m_fields         );
            context.Transform( ref m_methods        );
            context.Transform( ref m_methodImpls    );

            context.Transform( ref m_vTable         );

            context.Pop();
        }

        //--//
    
        internal override void ProhibitUse( TypeSystem.Reachability reachability ,
                                            bool                    fApply       )
        {
            base.ProhibitUse( reachability, fApply );

            reachability.ExpandProhibition( m_vTable );

            foreach(FieldRepresentation fd in m_fields)
            {
                fd.ProhibitUse( reachability, fApply );
            }

            foreach(MethodRepresentation md in m_methods)
            {
                md.ProhibitUse( reachability, fApply );
            }
        }

        internal override void Reduce( TypeSystem.Reachability reachability ,
                                       bool                    fApply       )
        {
            base.Reduce( reachability, fApply );

            for(int i = m_interfaces.Length; --i >= 0; )
            {
                InterfaceTypeRepresentation itf = m_interfaces[i];

                if(reachability.Contains( itf ) == false)
                {
                    if(fApply)
                    {
                        RemoveInterface( itf );
                    }
                }
            }

            for(int i = m_fields.Length; --i >= 0; )
            {
                FieldRepresentation fd = m_fields[i];

                if(reachability.Contains( fd ) == false)
                {
                    CHECKS.ASSERT( (fd is InstanceFieldRepresentation) == false || this.ValidLayout == false, "Cannot remove a field after the type has been laid out" );

                    fd.ProhibitUse( reachability, fApply );

                    if(fApply)
                    {
                        RemoveField( fd );
                    }
                }
                else
                {
                    fd.Reduce( reachability, fApply );
                }
            }

            for(int i = m_methods.Length; --i >= 0; )
            {
                MethodRepresentation md = m_methods[i];

                if(reachability.Contains( md ) == false)
                {
                    md.ProhibitUse( reachability, fApply );  

                    if(fApply)
                    {
                        RemoveMethod( md );
                    }
                }
                else
                {
                    md.Reduce( reachability, fApply );
                }
            }

            for(int i = m_methodImpls.Length; --i >= 0; )
            {
                MethodImplRepresentation mid = m_methodImpls[i];

                if(reachability.Contains( mid.Declaration ) == false ||
                   reachability.Contains( mid.Body        ) == false  )
                {
                    mid.ProhibitUse( reachability, fApply );

                    if(fApply)
                    {
                        RemoveMethodImpl( mid );
                    }
                }
            }
        }

        //--//

        public void ClearMethodTableAndInterfaceMaps( TypeSystem typeSystem )
        {
            m_methodTable           = null;
            m_interfaceMethodTables = null;

            m_vTable.MethodPointers          = null;
            m_vTable.InterfaceMethodPointers = null;
        }

        public void BuildMethodTableAndInterfaceMaps( TypeSystem typeSystem )
        {
            BuildMethodTable ( typeSystem );
            BuildInterfaceMap( typeSystem );
        }

        internal void BuildGcInfo( TypeSystem typeSystem )
        {
            if(this.IsOpenType == false)
            {
                var lst = new List< GCInfo.Pointer >();

                CollectPointerInfo( typeSystem, lst, this, 0 );

                int num = lst.Count;

                if(num > 0)
                {
                    var array = new GCInfo.Pointer[num];

                    m_vTable.GCInfo.Pointers = array;

                    for(int i = 0; i < num; i++)
                    {
                        array[num - 1 - i] = lst[i];
                    }
                }
                else
                {
                    m_vTable.GCInfo.Pointers = null;
                }
            }
        }

        private static void CollectPointerInfo( TypeSystem             typeSystem ,
                                                List< GCInfo.Pointer > lst        ,
                                                TypeRepresentation     td         ,
                                                int                    offset     )
        {
            while(td != null)
            {
                FieldRepresentation[] fds = td.Fields;

                for(int i = fds.Length; --i >= 0; )
                {
                    var fd = fds[i] as InstanceFieldRepresentation;
                    if(fd != null && typeSystem.IncludeInGCInfo( fd ))
                    {
                        TypeRepresentation fdType   = fd.FieldType;
                        int                fdOffset = offset + fd.Offset;
                        GCInfo.Kind        fdKind   = fdType.ClassifyAsPointer();

                        switch(fdKind)
                        {
                            case GCInfo.Kind.AggregateType:
                                CollectPointerInfo( typeSystem, lst, fdType, fdOffset );
                                break;

                            case GCInfo.Kind.Heap     :
                            case GCInfo.Kind.Internal :
                            case GCInfo.Kind.Potential:
                                {
                                    GCInfo.Pointer ptr = new GCInfo.Pointer();

                                    ptr.Kind          =         fdKind;
                                    ptr.OffsetInWords = (short)(fdOffset / sizeof(uint));

                                    lst.Add( ptr );
                                }
                                break;
                        }
                    }
                }

                td = td.Extends;
            }
        }

        //--//

        private void BuildMethodTable( TypeSystem typeSystem )
        {
            if(m_methodTable == null)
            {
                if(this.IsDelayedType)
                {
                    m_methodTable = MethodRepresentation.SharedEmptyArray;
                }
                else
                {
                    TypeRepresentation parent = this.Extends;
                    if(parent != null)
                    {
                        parent.BuildMethodTable( typeSystem );

                        m_methodTable = ArrayUtility.CopyNotNullArray( parent.m_methodTable );

                        CHECKS.ASSERT( m_methodTable != null, "Parent class {0} for {1} has not been initialized properly", parent, this );
                    }
                    else
                    {
                        m_methodTable = MethodRepresentation.SharedEmptyArray;
                    }

                    int methodTableLength = m_methodTable.Length;

                    foreach(MethodRepresentation md in m_methods)
                    {
                        md.CreateCodePointer( typeSystem );

                        if(GetDeclarationOfMethodImpl( md ) != null)
                        {
                            continue; // Skip any method with an explicit MethodImpl.
                        }

                        if(md.IsOpenMethod == false && md is VirtualMethodRepresentation)
                        {
                            if(md.VTableLayoutFlags == MethodRepresentation.Attributes.ReuseSlot)
                            {
                                string name = md.Name;
                                int    i;

                                //
                                // Search from the bottom, to take care of "new" virtual methods.
                                //
                                for(i = methodTableLength - 1; i >= 0; i--)
                                {
                                    MethodRepresentation md2 = m_methodTable[i];

                                    if(md2.MatchNameAndSignature( name, md, null ))
                                    {
                                        if(md2 is FinalMethodRepresentation)
                                        {
                                            throw TypeConsistencyErrorException.Create( "{0} cannot override final method {1}", md, md2 );
                                        }

                                        break;
                                    }
                                }

                                if(i >= 0)
                                {
                                    m_methodTable[i] = md;
                                    continue;
                                }
                            }

                            m_methodTable = ArrayUtility.AppendToNotNullArray( m_methodTable, md );
                            methodTableLength++;
                        }
                    }

                    if((this is AbstractReferenceTypeRepresentation) == false)
                    {
                        for(int i = 0; i < methodTableLength; i++)
                        {
                            MethodRepresentation md = m_methodTable[i];

                            if((md.Flags & MethodRepresentation.Attributes.Abstract) != 0)
                            {
                                throw TypeConsistencyErrorException.Create( "Type {0} is missing a definition for abstract method {1}", this, md );
                            }
                        }
                    }
                }

                m_vTable.MethodPointers = ConvertToMethodPointers( m_methodTable );
            }
        }

        //--//

        private void BuildInterfaceMap( TypeSystem typeSystem )
        {
            if(m_interfaceMethodTables == null)
            {
                m_interfaceMethodTables          = InterfaceMap.SharedEmptyArray;
                m_vTable.InterfaceMethodPointers = VTable.InterfaceMap.SharedEmptyArray;

                if(this is InterfaceTypeRepresentation)
                {
                    //
                    // The interface method table for an interface is its method table.
                    //
                    InterfaceMap im;

                    im.Interface = (InterfaceTypeRepresentation)this;
                    im.Methods   = m_methodTable;

                    m_interfaceMethodTables = ArrayUtility.AppendToNotNullArray( m_interfaceMethodTables, im );

                    //--//

                    VTable.InterfaceMap im2;

                    im2.Interface      = this.VirtualTable;
                    im2.MethodPointers = ConvertToMethodPointers( m_methodTable );

                    m_vTable.InterfaceMethodPointers = ArrayUtility.AppendToNotNullArray( m_vTable.InterfaceMethodPointers, im2 );
                }
                else
                {
                    TypeRepresentation td = this;
                    while(td != null)
                    {
                        foreach(InterfaceTypeRepresentation itf in td.m_interfaces)
                        {
                            AddInterfaceImplementation( typeSystem, itf );
                        }

                        td = td.Extends;
                    }
                }
            }
        }

        private void AddInterfaceImplementation( TypeSystem                  typeSystem ,
                                                 InterfaceTypeRepresentation itf        )
        {
            foreach(InterfaceTypeRepresentation itfSub in itf.m_interfaces)
            {
                AddInterfaceImplementation( typeSystem, itfSub );
            }

            int count = m_interfaceMethodTables.Length;

            for(int i = 0; i < count; i++)
            {
                if(m_interfaceMethodTables[i].Interface == itf)
                {
                    return;
                }
            }

            InterfaceMap im;
            int          len = itf.m_methods.Length;

            im.Interface = itf;

            if(len > 0)
            {
                im.Methods = new MethodRepresentation[len];

                //
                // Section 12.2 of ECMA spec, Partition II
                //
                for(int i = 0; i < len; i++)
                {
                    MethodRepresentation mdItf     = itf.m_methods[i];
                    string               mdItfName = mdItf.Name;

                    for(TypeRepresentation td = this; ; td = td.Extends)
                    {
                        MethodRepresentation mdTarget;

                        if(td == null)
                        {
                            //throw TypeConsistencyErrorException.Create( "Type {0} does not implement method {1} for interface {2}", this, mdItf, itf );

                            //
                            // [ZachL] - Provide stub implemenation for interface methods that are never called.  ComputeCallsClosure was changed so that
                            //           interface methods were not automatically included just because the interface and a type implementing that interface
                            //           were used.  If the method was not called on a particular type then we don't need to drag its implementation and
                            //           everything it references in.
                            //
                            mdTarget = typeSystem.WellKnownMethods.ThreadImpl_ThrowNullException;
                        }
                        else
                        {
                            //
                            // We have to do two passes:
                            //
                            //  - The first one looks for explicit method implementation.
                            //  - The second one looks only for a simple name+signature match.
                            //
                            mdTarget = td.GetMethodImpl( mdItf );

                            if(mdTarget == null)
                            {
                                mdTarget = td.FindMatch( mdItfName, mdItf, null );
                            }
                        }

                        if(mdTarget != null)
                        {
                            im.Methods[i] = mdTarget;
                            break;
                        }
                    }
                }
            }
            else
            {
                im.Methods = MethodRepresentation.SharedEmptyArray;
            }

            m_interfaceMethodTables = ArrayUtility.AppendToNotNullArray( m_interfaceMethodTables, im );

            //--//

            VTable.InterfaceMap im2;

            im2.Interface      = itf.VirtualTable;
            im2.MethodPointers = ConvertToMethodPointers( im.Methods );

            m_vTable.InterfaceMethodPointers = ArrayUtility.AppendToNotNullArray( m_vTable.InterfaceMethodPointers, im2 );
        }

        private static CodePointer[] ConvertToMethodPointers( MethodRepresentation[] mdArray )
        {
            int           len = mdArray.Length;
            CodePointer[] res = new CodePointer[len];

            for(int i = 0; i < len; i++)
            {
                res[i] = mdArray[i].CodePointer;
            }

            return res;
        }

        //--//

        public TypeRepresentation Instantiate( InstantiationContext ic )
        {
            TypeRepresentation instantiatedTd = AllocateInstantiation( ic );

            TypeRepresentation instantiatedTdRes = ic.Lookup( instantiatedTd, true );
            if(instantiatedTdRes != null)
            {
                return instantiatedTdRes;
            }
            else
            {
                //m_typeSystem.Types.Add( instantiatedTd );

                return instantiatedTd;
            }
        }

        protected abstract TypeRepresentation AllocateInstantiation( InstantiationContext ic );

        protected virtual void PopulateInstantiation( TypeRepresentation   tdTemplate ,
                                                      InstantiationContext ic         )
        {
            m_builtinType    =                 tdTemplate.m_builtinType;
            m_flags          =                 tdTemplate.m_flags;
            m_buildFlags     =                 tdTemplate.m_buildFlags;
            m_name           =                 tdTemplate.m_name;
            m_namespace      =                 tdTemplate.m_namespace;
            m_enclosingClass = ic.Instantiate( tdTemplate.m_enclosingClass );

            m_extends        = ic.Instantiate( tdTemplate.m_extends        );
////    protected InterfaceTypeRepresentation[]   m_interfaces;
////    protected FieldRepresentation[]           m_fields;
////    protected MethodRepresentation[]          m_methods;
////    protected MethodImplRepresentation[]      m_methodImpls;
        }

        //--//

        public void AddInterface( InterfaceTypeRepresentation itf )
        {
            m_interfaces = ArrayUtility.AppendToNotNullArray( m_interfaces, itf );
        }

        public void AddField( FieldRepresentation fd )
        {
            m_fields = ArrayUtility.AppendToNotNullArray( m_fields, fd );
        }

        public void AddMethod( MethodRepresentation md )
        {
            m_methods = ArrayUtility.AppendToNotNullArray( m_methods, md );
        }

        public void AddMethodImpl( MethodImplRepresentation mi )
        {
            for(int i = 0; i < m_methodImpls.Length; i++)
            {
                MethodImplRepresentation miOld = m_methodImpls[i];

                if(miOld.Declaration == mi.Declaration)
                {
                    throw TypeConsistencyErrorException.Create( "Multiple methods are used to implement '{0}' on '{1}'", mi.Declaration, this );
                }
            }

            m_methodImpls = ArrayUtility.AppendToNotNullArray( m_methodImpls, mi );
        }

        //--//

        public void RemoveInterface( InterfaceTypeRepresentation itf )
        {
            m_interfaces = ArrayUtility.RemoveUniqueFromNotNullArray( m_interfaces, itf );
        }
    
        public void RemoveField( FieldRepresentation fd )
        {
            m_fields = ArrayUtility.RemoveUniqueFromNotNullArray( m_fields, fd );
        }
    
        public void RemoveMethod( MethodRepresentation md )
        {
            m_methods = ArrayUtility.RemoveUniqueFromNotNullArray( m_methods, md );
    
            for(int i = m_methodImpls.Length; --i >= 0; )
            {
                if(m_methodImpls[i].Body == md)
                {
                    m_methodImpls = ArrayUtility.RemoveAtPositionFromNotNullArray( m_methodImpls, i );
                }
            }
        }

        public void RemoveMethodImpl( MethodImplRepresentation mid )
        {
            for(int i = m_methodImpls.Length; --i >= 0; )
            {
                if(m_methodImpls[i] == mid)
                {
                    m_methodImpls = ArrayUtility.RemoveAtPositionFromNotNullArray( m_methodImpls, i );
                }
            }
        }

        //--//

        public static TypeRepresentation FindCommonType( TypeRepresentation tdLeft  ,
                                                         TypeRepresentation tdRight )
        {
            while(tdLeft != null && tdRight != null)
            {
                if(tdLeft.CanBeAssignedFrom( tdRight, null ))
                {
                    return tdLeft;
                }

                if(tdRight.CanBeAssignedFrom( tdLeft, null ))
                {
                    return tdRight;
                }

                tdLeft  = tdLeft .Extends;
                tdRight = tdRight.Extends;
            }

            return null;
        }

        //--//

        public FieldRepresentation FindField( string name )
        {
            TypeRepresentation td = this;

            while(td != null)
            {
                foreach(FieldRepresentation fd in td.Fields)
                {
                    if(fd.Name == name)
                    {
                        return fd;
                    }
                }

                td = td.Extends;
            }

            return null;
        }

        public InstanceFieldRepresentation FindFieldAtOffset( int offset )
        {
            TypeRepresentation td = this;

            while(td != null)
            {
                foreach(FieldRepresentation fd in td.Fields)
                {
                    InstanceFieldRepresentation fdI = fd as InstanceFieldRepresentation;

                    if(fdI != null && fdI.Offset == offset)
                    {
                        return fdI;
                    }
                }

                td = td.Extends;
            }

            return null;
        }

        public MethodRepresentation FindMatch( string searchName )
        {
            foreach(MethodRepresentation md in m_methods)
            {
                if(md.Name == searchName)
                {
                    return md;
                }
            }

            return null;
        }

        public MethodRepresentation FindMatch( MethodRepresentation search ,
                                               EquivalenceSet       set    )
        {
            return FindMatch( search.Name, search, set );
        }

        public MethodRepresentation FindMatch( string               searchName      ,
                                               MethodRepresentation searchSignature ,
                                               EquivalenceSet       set             )
        {
            foreach(MethodRepresentation md in m_methods)
            {
                if(md.MatchNameAndSignature( searchName, searchSignature, set ))
                {
                    return md;
                }
            }

            return null;
        }

        public FinalizerMethodRepresentation FindDestructor()
        {
            for(var td = this; td != null; td = td.Extends)
            {
                foreach(MethodRepresentation md in m_methods)
                {
                    var res = md as FinalizerMethodRepresentation;
                    if(res != null)
                    {
                        return res;
                    }
                }
            }

            return null;
        }

        public ConstructorMethodRepresentation FindDefaultConstructor()
        {
            foreach(MethodRepresentation md in m_methods)
            {
                if(md is ConstructorMethodRepresentation && md.ThisPlusArguments.Length == 1)
                {
                    return (ConstructorMethodRepresentation)md;
                }
            }

            return null;
        }

        public StaticConstructorMethodRepresentation FindDefaultStaticConstructor()
        {
            foreach(MethodRepresentation md in m_methods)
            {
                if(md is StaticConstructorMethodRepresentation)
                {
                    return (StaticConstructorMethodRepresentation)md;
                }
            }

            return null;
        }

        public MethodRepresentation[] FindInterfaceTable( InterfaceTypeRepresentation itf )
        {
            for(int i = 0; i < m_interfaceMethodTables.Length; i++)
            {
                if(m_interfaceMethodTables[i].Interface == itf)
                {
                    return m_interfaceMethodTables[i].Methods;
                }
            }

            return null;
        }

        public InterfaceTypeRepresentation FindInstantiationOfGenericInterface(        InterfaceTypeRepresentation itf        ,
                                                                                params TypeRepresentation[]        parameters )
        {
            var td = this;

            while(td != null)
            {
                foreach(var itf2 in td.m_interfaces)
                {
                    if(itf2.GenericTemplate == itf && itf2.IsGenericInstantiation)
                    {
                        if(ArrayUtility.ArrayEqualsNotNull( itf2.GenericParameters, parameters, 0 ))
                        {
                            return itf2;
                        }
                    }
                }

                td = td.Extends;
            }

            return null;
        }

        //--//

        public void Substituite( FieldRepresentation oldFD ,
                                 FieldRepresentation newFD )
        {
            int pos = ArrayUtility.FindInNotNullArray( m_fields, oldFD );

            m_fields = ArrayUtility.ReplaceAtPositionOfNotNullArray( m_fields, pos, newFD );
        }

        public void Substituite( MethodRepresentation oldMD ,
                                 MethodRepresentation newMD )
        {
            newMD.PrepareToSubstitute( oldMD );

            int pos = ArrayUtility.FindInNotNullArray( m_methods, oldMD );

            m_methods = ArrayUtility.ReplaceAtPositionOfNotNullArray( m_methods, pos, newMD );

            //--//

            //
            // Update the MethodImpl array, to point to the new method.
            //
            for(int i = 0; i < m_methodImpls.Length; i++)
            {
                MethodImplRepresentation mi = m_methodImpls[i];

                if(mi.Body == oldMD)
                {
                    mi = new MethodImplRepresentation( newMD, mi.Declaration );

                    m_methodImpls[i] = mi;
                }
            }
        }

        public VirtualMethodRepresentation CreateOverride( TypeSystem                  typeSystem ,
                                                           VirtualMethodRepresentation mdSource   )
        {
            if(mdSource.Generic != null)
            {
                throw TypeConsistencyErrorException.Create( "Overriding of generic method '{0}' is not supported", mdSource );
            }
            else
            {
                VirtualMethodRepresentation md;

                if(this is ValueTypeRepresentation)
                {
                    md = new FinalMethodRepresentation( this, null );
                }
                else
                {
                    md = new VirtualMethodRepresentation( this, null );
                }

                AddMethod( md );

                md.Override( typeSystem, mdSource );

                return md;
            }
        }

        internal ConstructorMethodRepresentation CreateMatchingConstructor( TypeSystem                      typeSystem ,
                                                                            ConstructorMethodRepresentation mdSource   )
        {
            if(mdSource.Generic != null)
            {
                throw TypeConsistencyErrorException.Create( "Overriding of generic method '{0}' is not supported", mdSource );
            }
            else
            {
                ConstructorMethodRepresentation md = new ConstructorMethodRepresentation( this, null );

                AddMethod( md );

                md.Override( typeSystem, mdSource );

                return md;
            }
        }

        //--//

        internal virtual void InvalidateLayout()
        {
            m_vTable.BaseSize = uint.MaxValue;
        }

        //--//

        public abstract GCInfo.Kind ClassifyAsPointer();

        //--//

        public override void EnumerateCustomAttributes( CustomAttributeAssociationEnumerationCallback callback )
        {
            base.EnumerateCustomAttributes( callback );

            foreach(FieldRepresentation fd in m_fields)
            {
                fd.EnumerateCustomAttributes( callback );
            }

            foreach(MethodRepresentation md in m_methods)
            {
                md.EnumerateCustomAttributes( callback );
            }
        }

        public bool IsSubClassOf( TypeRepresentation rvalue ,
                                  EquivalenceSet     set    )
        {
            if(rvalue != null)
            {
                return rvalue.IsSuperClassOf( this, set );
            }

            return false;
        }

        public bool IsSuperClassOf( TypeRepresentation rvalue ,
                                    EquivalenceSet     set    )
        {
            if(rvalue != null)
            {
                while(true)
                {
                    rvalue = rvalue.Extends;
                    if(rvalue == null)
                    {
                        break;
                    }

                    if(this.EqualsThroughEquivalence( rvalue, set ))
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }

        public virtual bool CanBeAssignedFrom( TypeRepresentation rvalue ,
                                               EquivalenceSet     set    )
        {
            if(this.EqualsThroughEquivalence( rvalue, set ))
            {
                return true;
            }

            //
            // Interfaces can always be cast to Object, but they don't actually inherit from Object,
            // so the loop below would fail.
            //
            if(rvalue is InterfaceTypeRepresentation &&
               this.IsObject                          )
            {
                return true;
            }

            if(rvalue is EnumerationTypeRepresentation)
            {
                EnumerationTypeRepresentation rvalue2 = (EnumerationTypeRepresentation)rvalue;

                return CanBeAssignedFrom( rvalue2.UnderlyingType, set );
            }

            if(this.StackEquivalentType == StackEquivalentType.NativeInt &&
               rvalue is PointerTypeRepresentation                        )
            {
                return true; // Always allow a cast from a pointer to a native int.
            }

            if(rvalue is ArrayReferenceTypeRepresentation)
            {
                if(rvalue.Extends == this)
                {
                    //
                    // Any array can be cast to its base class, System.Array.
                    //
                    return true;
                }
            }

            return IsSuperClassOf( rvalue, set );
        }

        //--//

        public MethodRepresentation GetDeclarationOfMethodImpl( MethodRepresentation body )
        {
            foreach(MethodImplRepresentation mi in m_methodImpls)
            {
                if(mi.Body == body)
                {
                    return mi.Declaration;
                }
            }

            return null;
        }

        public MethodRepresentation GetMethodImpl( MethodRepresentation declaration )
        {
            foreach(MethodImplRepresentation mi in m_methodImpls)
            {
                if(mi.Declaration == declaration)
                {
                    return mi.Body;
                }
            }

            return null;
        }

        public virtual InstantiationFlavor GetInstantiationFlavor( TypeSystem typeSystem )
        {
            return InstantiationFlavor.Class;
        }

        //--//

        //
        // Access Methods
        //

        public AssemblyRepresentation Owner
        {
            get
            {
                return m_owner;
            }
        }

        public BuiltInTypes BuiltInType
        {
            get
            {
                return m_builtinType;
            }
        }

        public Attributes Flags
        {
            get
            {
                return m_flags;
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
                if(tdTemplate != null)
                {
                    res |= tdTemplate.ExpandedBuildTimeFlags;
                }

                return res;
            }
        }

        public bool HasBuildTimeFlag( BuildTimeAttributes bta )
        {
            return (this.ExpandedBuildTimeFlags & bta) != 0;
        }
        
        public string Name
        {
            get
            {
                return m_name;
            }
        }

        public string Namespace
        {
            get
            {
                return m_namespace;
            }
        }

        public TypeRepresentation EnclosingClass
        {
            get
            {
                return m_enclosingClass;
            }
        }

        public bool IsGenericInstantiation
        {
            get
            {
                return this.IsOpenType == false && this.GenericParameters.Length > 0;
            }
        }

        public GenericContext Generic
        {
            get
            {
                return m_genericContext;
            }
        }

        public TypeRepresentation GenericTemplate
        {
            get
            {
                if(m_genericContext != null)
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
                if(m_genericContext != null)
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
                if(m_genericContext != null)
                {
                    return m_genericContext.ParametersDefinition;
                }

                return null;
            }
        }

        public TypeRepresentation Extends
        {
            get
            {
                return m_extends;
            }
        }

        public InterfaceTypeRepresentation[] Interfaces
        {
            get
            {
                return m_interfaces;
            }
        }

        public FieldRepresentation[] Fields
        {
            get
            {
                return m_fields;
            }
        }

        public MethodRepresentation[] Methods
        {
            get
            {
                return m_methods;
            }
        }

        public MethodImplRepresentation[] MethodImpls
        {
            get
            {
                return m_methodImpls;
            }
        }

        public InterfaceMap[] InterfaceMaps
        {
            get
            {
                return m_interfaceMethodTables;
            }
        }

        public MethodRepresentation[] MethodTable
        {
            get
            {
                return m_methodTable;
            }
        }

        public VTable VirtualTable
        {
            get
            {
                return m_vTable;
            }
        }

        //--//

        public string FullName
        {
            get
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                PrettyToString( sb, false, false );

                return sb.ToString();
            }
        }

        public string FullNameWithAbbreviation
        {
            get
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                PrettyToString( sb, false, true );

                return sb.ToString();
            }
        }

        public virtual TypeRepresentation ContainedType
        {
            get
            {
                return null;
            }
        }

        public virtual TypeRepresentation UnderlyingType
        {
            get
            {
                return this;
            }
        }

        public bool IsObject
        {
            get
            {
                return (this is ConcreteReferenceTypeRepresentation && m_extends == null);
            }
        }

        public virtual bool IsOpenType
        {
            get
            {
                if(m_genericContext != null)
                {
                    return m_genericContext.IsOpenType;
                }

                return false;
            }
        }

        public virtual bool IsDelayedType
        {
            get
            {
                if(m_genericContext != null)
                {
                    return m_genericContext.IsDelayedType;
                }

                return false;
            }
        }

        public bool IsNestedType
        {
            get
            {
                return m_enclosingClass != null;
            }
        }

        public bool IsAbstract
        {
            get
            {
                return (m_flags & TypeRepresentation.Attributes.Abstract) != 0;
            }
        }

        //--//

        public virtual bool IsNumeric
        {
            get
            {
                return false;
            }
        }

        public virtual bool IsSigned
        {
            get
            {
                return false;
            }
        }

        public virtual bool IsInteger
        {
            get
            {
                return false;
            }
        }

        public virtual bool IsFloatingPoint
        {
            get
            {
                return false;
            }
        }

        public abstract bool CanPointToMemory
        {
            get;
        }

        //--//

        public virtual StackEquivalentType StackEquivalentType
        {
            get
            {
                switch(this.BuiltInType)
                {
                    case BuiltInTypes.VOID   : return StackEquivalentType.Void;
                    case BuiltInTypes.BOOLEAN: return StackEquivalentType.Int32;
                    case BuiltInTypes.CHAR   : return StackEquivalentType.Int32;
                    case BuiltInTypes.I1     : return StackEquivalentType.Int32;
                    case BuiltInTypes.U1     : return StackEquivalentType.Int32;
                    case BuiltInTypes.I2     : return StackEquivalentType.Int32;
                    case BuiltInTypes.U2     : return StackEquivalentType.Int32;
                    case BuiltInTypes.I4     : return StackEquivalentType.Int32;
                    case BuiltInTypes.U4     : return StackEquivalentType.Int32;
                    case BuiltInTypes.I8     : return StackEquivalentType.Int64;
                    case BuiltInTypes.U8     : return StackEquivalentType.Int64;
                    case BuiltInTypes.R4     : return StackEquivalentType.Float;
                    case BuiltInTypes.R8     : return StackEquivalentType.Float;
                    case BuiltInTypes.I      : return StackEquivalentType.NativeInt;
                    case BuiltInTypes.U      : return StackEquivalentType.NativeInt;
                    default                  : return StackEquivalentType.Object;
                }
            }
        }

        public bool ValidLayout
        {
            get
            {
                return m_vTable.BaseSize != uint.MaxValue;
            }
        }

        public uint Size
        {
            get
            {
                CHECKS.ASSERT( this.ValidLayout, "Cannot access the 'Size' property on type '{0}' before the type has been laid out", this );

                return m_vTable.BaseSize;
            }

            set
            {
                m_vTable.BaseSize = value;
            }
        }

        public abstract uint SizeOfHoldingVariable
        {
            get;
        }

        public uint SizeOfHoldingVariableInWords
        {
            get
            {
                uint size = this.SizeOfHoldingVariable;

                //
                // Round to word size.
                //
                size = (size + (sizeof(uint)-1)) / sizeof(uint);

                return size;
            }
        }

        //--//

        //
        // Debug Methods
        //

        private string GetAbbreviation()
        {
            switch(m_builtinType)
            {
                case BuiltInTypes.VOID   : return "void"   ;
                case BuiltInTypes.BOOLEAN: return "bool"   ;
                case BuiltInTypes.CHAR   : return "char"   ;
                case BuiltInTypes.I1     : return "sbyte"  ;
                case BuiltInTypes.U1     : return "byte"   ;
                case BuiltInTypes.I2     : return "short"  ;
                case BuiltInTypes.U2     : return "ushort" ;
                case BuiltInTypes.I4     : return "int"    ;
                case BuiltInTypes.U4     : return "uint"   ;
                case BuiltInTypes.I8     : return "long"   ;
                case BuiltInTypes.U8     : return "ulong"  ;
                case BuiltInTypes.R4     : return "float"  ;
                case BuiltInTypes.R8     : return "double" ;
                case BuiltInTypes.STRING : return "string" ;
                case BuiltInTypes.OBJECT : return "object" ;
                default                  : return null;
            }
        }

        internal virtual void PrettyToString( System.Text.StringBuilder sb                 ,
                                              bool                      fPrefix            ,
                                              bool                      fWithAbbreviations )
        {
            if(fPrefix)
            {
                if(this.IsDelayedType)
                {
                    sb.Append( "delayed " );
                }
                else if(this.IsOpenType)
                {
                    sb.Append( "generic " );
                }
            }

            string name = null;

            if(fWithAbbreviations)
            {
                name = GetAbbreviation();
            }

            if(name != null)
            {
                sb.Append( name );
            }
            else
            {
                if(this.IsNestedType)
                {
                    this.EnclosingClass.PrettyToString( sb, false, fWithAbbreviations );
                    sb.Append( "." );
                }

                if(this.Namespace != null && this.Namespace.Length != 0)
                {
                    sb.Append( this.Namespace );
                    sb.Append( "."            );
                }

                sb.Append( this.Name );
            }

            if(m_genericContext != null)
            {
                sb.Append( "<" );

                TypeRepresentation[] parameters = m_genericContext.Parameters;
                if(parameters.Length > 0)
                {
                    for(int i = 0; i < parameters.Length; i++)
                    {
                        if(i != 0) sb.Append( "," );

                        parameters[i].PrettyToString( sb, fPrefix, fWithAbbreviations );
                    }
                }
                else
                {
                    GenericParameterDefinition[] defs = m_genericContext.ParametersDefinition;
                    if(defs != null)
                    {
                        for(int i = 0; i < defs.Length; i++)
                        {
                            if(i != 0) sb.Append( "," );

                            sb.Append( defs[i].Name );
                        }
                    }
                }

                sb.Append( ">" );
            }
        }
    }
}
