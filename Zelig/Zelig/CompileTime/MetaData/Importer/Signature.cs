//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//
// Originally based on the Bartok code base.
//

using System;
using System.Collections.Generic;
using System.Text;

/**********************************************************************
 *
 * The Microsoft.Zelig.MetaData.Importer.Signature* classes represent the signatures read
 * from the Blob heap of the input file(s).
 *
 * Since the signatures have references to MetaData* objects, the
 * signatures are initially just an array of bytes.  When all the
 * MetaData objects have been created, the array of bytes is processed
 * and the references resolved.
 *
 **********************************************************************/

namespace Microsoft.Zelig.MetaData.Importer
{
    public abstract class SignatureType : Signature
    {
        //
        // State
        //

        protected readonly ElementTypes m_elementType;
        protected          Modifier     m_modifierChain;

        //
        // Constructor Methods
        //

        protected SignatureType( ElementTypes elementType )
        {
            m_elementType = elementType;
        }

        //--//

        internal void SetModifierChain( Modifier modifierChain )
        {
            m_modifierChain = modifierChain;
        }

        //--//

        internal Normalized.MetaDataSignature ApplyModifiers( Normalized.MetaDataTypeDefinitionAbstract obj     ,
                                                              MetaDataNormalizationContext              context )
        {
            Normalized.SignatureType sig = Normalized.SignatureType.Create( obj );

            if(m_modifierChain != null)
            {
                m_modifierChain.Apply( sig, context );
            }

            return (Normalized.MetaDataSignature)sig.MakeUnique();
        }

        //
        // Access Methods
        //

        public ElementTypes ElementType
        {
            get
            {
                return m_elementType;
            }
        }

        public Modifier ModifierChain
        {
            get
            {
                return m_modifierChain;
            }
        }

        //
        // Debug Methods
        //

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append( m_elementType );

            if(m_modifierChain != null)
            {
                sb.Append( "," );
                sb.Append( m_modifierChain );
            }

            return sb.ToString();
        }

        //--//--//

        public class BuiltIn : SignatureType,
            IMetaDataNormalizeSignature
        {
            //
            // Constructor Methods
            //

            public BuiltIn( ElementTypes elementType ) : base( elementType )
            {
            }

            //--//

            //
            // IMetaDataNormalizeSignature methods
            //

            Normalized.MetaDataSignature IMetaDataNormalizeSignature.AllocateNormalizedObject( MetaDataNormalizationContext context )
            {
                Normalized.MetaDataTypeDefinitionAbstract res = context.LookupBuiltIn( m_elementType );

                return ApplyModifiers( res, context );
            }

            //
            // Debug Methods
            //

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();

                sb.Append( base.ToString() );

                return sb.ToString();
            }
        }

        public class ClassOrStruct : SignatureType,
            IMetaDataNormalizeSignature
        {
            //
            // State
            //

            protected IMetaDataTypeDefOrRef m_classObject;

            //
            // Constructor Methods
            //

            public ClassOrStruct( ElementTypes          elementType ,
                                  IMetaDataTypeDefOrRef classObject ) : base( elementType )
            {
                m_classObject = classObject;
            }

            //
            // IMetaDataNormalizeSignature methods
            //

            Normalized.MetaDataSignature IMetaDataNormalizeSignature.AllocateNormalizedObject( MetaDataNormalizationContext context )
            {
                Normalized.MetaDataTypeDefinitionAbstract res;

                context.GetNormalizedObject( m_classObject, out res, MetaDataNormalizationMode.LookupExisting );

                return ApplyModifiers( res, context );
            }

            //
            // Access Methods
            //

            public IMetaDataTypeDefOrRef ClassObject
            {
                get
                {
                    return m_classObject;
                }
            }

            //
            // Debug Methods
            //

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder( "ClassOrStruct(" );

                sb.Append( base.ToString() );

                sb.Append( ","           );
                sb.Append( m_classObject );

                sb.Append( ")" );

                return sb.ToString();
            }
        }

        public class Prefix : SignatureType,
            IMetaDataNormalizeSignature
        {
            //
            // State
            //

            protected readonly SignatureType m_type;

            //
            // Constructor Methods
            //

            public Prefix( ElementTypes  elementType ,
                           SignatureType type        ) : base( elementType )
            {
                m_type = type;
            }

            //
            // IMetaDataNormalizeSignature methods
            //

            Normalized.MetaDataSignature IMetaDataNormalizeSignature.AllocateNormalizedObject( MetaDataNormalizationContext context )
            {
                Normalized.SignatureType sig;

                context.GetNormalizedSignature( m_type, out sig, MetaDataNormalizationMode.Default );

                //--//

                Normalized.MetaDataTypeDefinitionByRef byref = new Normalized.MetaDataTypeDefinitionByRef( context.GetAssemblyFromContext(), 0, m_elementType );

                byref.m_type = sig.Type;

                return ApplyModifiers( byref, context );
            }

            //
            // Access Methods
            //

            public SignatureType InnerType
            {
                get
                {
                    return m_type;
                }
            }

            //
            // Debug Methods
            //

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder( "Prefix(" );

                sb.Append( base.ToString() );

                sb.Append( ","    );
                sb.Append( m_type );

                sb.Append( ")" );

                return sb.ToString();
            }
        }

        public class Method : SignatureType,
            IMetaDataNormalizeSignature
        {
            //
            // State
            //

            protected readonly SignatureMethod m_method;

            //
            // Constructor Methods
            //

            public Method( SignatureMethod method ) : base( ElementTypes.FNPTR )
            {
                m_method = method;
            }

            //
            // IMetaDataNormalizeSignature methods
            //

            Normalized.MetaDataSignature IMetaDataNormalizeSignature.AllocateNormalizedObject( MetaDataNormalizationContext context )
            {
                throw IllegalMetaDataFormatException.Create( "FNPTR elements in signatures not supported" );
            }

            //
            // Access Methods
            //

            public SignatureMethod Signature
            {
                get
                {
                    return m_method;
                }
            }

            //
            // Debug Methods
            //

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder( "Method(" );

                sb.Append( base.ToString() );

                sb.Append( ","      );
                sb.Append( m_method );

                sb.Append( ")" );

                return sb.ToString();
            }
        }

        public abstract class BaseArray : SignatureType
        {
            //
            // Constructor Methods
            //

            protected BaseArray( ElementTypes elementType ) : base( elementType )
            {
            }
        }

        public class SzArray : BaseArray,
            IMetaDataNormalizeSignature
        {
            //
            // State
            //

            protected readonly SignatureType m_typeObject;

            //
            // Constructor Methods
            //

            public SzArray( SignatureType typeObject ) : base( ElementTypes.SZARRAY )
            {
                m_typeObject = typeObject;
            }

            //
            // IMetaDataNormalizeSignature methods
            //

            Normalized.MetaDataSignature IMetaDataNormalizeSignature.AllocateNormalizedObject( MetaDataNormalizationContext context )
            {
                Normalized.SignatureType sigTypeObject;

                context.GetNormalizedSignature( m_typeObject, out sigTypeObject, MetaDataNormalizationMode.Default );

                //--//

                Normalized.MetaDataTypeDefinitionArraySz arrayNew = new Normalized.MetaDataTypeDefinitionArraySz( context.GetAssemblyFromContext(), 0 );

                arrayNew.m_elementType = m_elementType;
                arrayNew.m_extends     = context.LookupBuiltIn( ElementTypes.SZARRAY );
                arrayNew.m_objectType  = sigTypeObject.Type;

                return ApplyModifiers( arrayNew, context );
            }

            //
            // Access Methods
            //

            public SignatureType TypeObject
            {
                get
                {
                    return m_typeObject;
                }
            }

            //
            // Debug Methods
            //

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder( "SzArray(" );

                sb.Append( base.ToString() );

                sb.Append( ","          );
                sb.Append( m_typeObject );

                sb.Append( ")" );

                return sb.ToString();
            }
        }

        public class Array : BaseArray,
            IMetaDataNormalizeSignature
        {
            public struct Dimension
            {
                //
                // State
                //

                public uint m_lowerBound;
                public uint m_upperBound;
            }

            //
            // State
            //

            protected readonly SignatureType m_type;
            protected readonly uint          m_rank;
            protected readonly Dimension[]   m_dimensions;

            //
            // Constructor Methods
            //

            public Array( SignatureType type       ,
                          uint          rank       ,
                          Dimension[]   dimensions ) : base( ElementTypes.ARRAY )
            {
                m_type       = type;
                m_rank       = rank;
                m_dimensions = dimensions;
            }

            //
            // IMetaDataNoIMetaDataNormalizeSignaturermalize methods
            //

            Normalized.MetaDataSignature IMetaDataNormalizeSignature.AllocateNormalizedObject( MetaDataNormalizationContext context )
            {
                Normalized.SignatureType sigTypeObject;

                context.GetNormalizedSignature( m_type, out sigTypeObject, MetaDataNormalizationMode.Default );

                //--//

                Normalized.MetaDataTypeDefinitionArrayMulti arrayNew = new Normalized.MetaDataTypeDefinitionArrayMulti( context.GetAssemblyFromContext(), 0 );

                arrayNew.m_extends    = context.LookupBuiltIn( ElementTypes.SZARRAY );
                arrayNew.m_objectType = sigTypeObject.Type;

                arrayNew.m_rank       = m_rank;
                arrayNew.m_dimensions = new Normalized.MetaDataTypeDefinitionArrayMulti.Dimension[m_dimensions.Length];

                for(int i = 0; i < m_dimensions.Length; i++)
                {
                    arrayNew.m_dimensions[i].m_lowerBound = m_dimensions[i].m_lowerBound;
                    arrayNew.m_dimensions[i].m_upperBound = m_dimensions[i].m_upperBound;
                }

                return ApplyModifiers( arrayNew, context );
            }

            //
            // Access Methods
            //

            public SignatureType BaseType
            {
                get
                {
                    return m_type;
                }
            }

            public uint Rank
            {
                get
                {
                    return m_rank;
                }
            }

            public Dimension[] Dimensions
            {
                get
                {
                    return m_dimensions;
                }
            }

            //
            // Debug Methods
            //

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder( "Array(" );

                sb.Append( base.ToString() );

                sb.Append( ","    );
                sb.Append( m_type );
                sb.Append( "["  );

                for(int i = 0; i < m_rank; i++)
                {
                    uint lower = m_dimensions[i].m_lowerBound;
                    uint upper = m_dimensions[i].m_upperBound;

                    if(i != 0)
                    {
                        sb.Append( "," );
                    }

                    if(lower != 0 || upper != 0)
                    {
                        sb.Append( lower );

                        if(upper > lower)
                        {
                            sb.Append( ".."  );
                            sb.Append( upper );
                        }
                    }
                }
                sb.Append( "]" );

                sb.Append( ")" );

                return sb.ToString();
            }
        }

        public class GenericParameter : SignatureType,
            IMetaDataNormalizeSignature
        {
            //
            // State
            //

            private readonly uint m_number;

            //
            // Constructor Methods
            //

            public GenericParameter( ElementTypes elementType ,
                                     uint         number      ) : base( elementType )
            {
                m_number = number;
            }

            //
            // IMetaDataNormalizeSignature methods
            //

            Normalized.MetaDataSignature IMetaDataNormalizeSignature.AllocateNormalizedObject( MetaDataNormalizationContext context )
            {
                switch(m_elementType)
                {
                    case ElementTypes.VAR:
                        {
                            Normalized.MetaDataTypeDefinitionDelayed tdNew = new Normalized.MetaDataTypeDefinitionDelayed( context.GetAssemblyForDelayedParameters(), 0 );

                            tdNew.m_elementType     = ElementTypes.VAR;
                            tdNew.m_parameterNumber = (int)m_number;
                            tdNew.m_isMethodParam   = false;

                            return ApplyModifiers( tdNew, context );
                        }

                    case ElementTypes.MVAR:
                        {
                            Normalized.MetaDataTypeDefinitionDelayed tdNew = new Normalized.MetaDataTypeDefinitionDelayed( context.GetAssemblyForDelayedParameters(), 0 );

                            tdNew.m_elementType     = ElementTypes.MVAR;
                            tdNew.m_parameterNumber = (int)m_number;
                            tdNew.m_isMethodParam   = true;

                            return ApplyModifiers( tdNew, context );
                        }
                }

                throw IllegalMetaDataFormatException.Create( "'{0}' cannot be resolved in the context of '{1}'", this, context );
            }

            //
            // Access Methods
            //

            public uint Number
            {
                get
                {
                    return m_number;
                }
            }

            //
            // Debug Methods
            //

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder( "GenParam(" );

                sb.Append( base.ToString() );

                sb.Append( ","      );
                sb.Append( m_number );

                sb.Append( ")" );

                return sb.ToString();
            }
        }

        public class GenericInstantiation : SignatureType,
            IMetaDataNormalizeSignature
        {
            //
            // State
            //

            private readonly SignatureType   m_type;
            private readonly SignatureType[] m_parameters;

            //
            // Constructor Methods
            //

            public GenericInstantiation( SignatureType   type       ,
                                         SignatureType[] parameters ) : base( ElementTypes.GENERICINST )
            {
                m_type       = type;
                m_parameters = parameters;
            }

            //
            // IMetaDataNormalizeSignature methods
            //

            Normalized.MetaDataSignature IMetaDataNormalizeSignature.AllocateNormalizedObject( MetaDataNormalizationContext context )
            {
                Normalized.SignatureType   baseType;
                Normalized.SignatureType[] parameters;

                context.GetNormalizedSignature     ( m_type      , out baseType  , MetaDataNormalizationMode.Default );
                context.GetNormalizedSignatureArray( m_parameters, out parameters, MetaDataNormalizationMode.Default );

                Normalized.MetaDataTypeDefinitionGenericInstantiation tdNew = new Normalized.MetaDataTypeDefinitionGenericInstantiation( context.GetAssemblyFromContext(), 0 );

                tdNew.m_baseType   = (Normalized.MetaDataTypeDefinitionGeneric)baseType.Type;
                tdNew.m_parameters = parameters;

                return ApplyModifiers( tdNew, context );
            }

            //
            // Access Methods
            //

            public SignatureType Type
            {
                get
                {
                    return m_type;
                }
            }

            public SignatureType[] Parameters
            {
                get
                {
                    return m_parameters;
                }
            }

            //
            // Debug Methods
            //

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder( "GenInst(" );

                sb.Append( base.ToString() );

                sb.Append( ","    );
                sb.Append( m_type );
                sb.Append( "<"  );

                for(int i = 0; i < m_parameters.Length; i++)
                {
                    if(i != 0)
                    {
                        sb.Append( "," );
                    }

                    sb.Append( m_parameters[i] );
                }

                sb.Append( ">" );

                sb.Append( ")" );

                return sb.ToString();
            }
        }

        //--//

        public class Modifier
        {
            //
            // State
            //

            private ElementTypes          m_mod;
            private IMetaDataTypeDefOrRef m_classObject;
            private Modifier              m_next;

            //
            // Constructor Methods
            //

            public Modifier( ElementTypes          mod         ,
                             IMetaDataTypeDefOrRef classObject ,
                             Modifier              next        )
            {
                m_mod         = mod;
                m_classObject = classObject;
                m_next        = next;
            }

            internal void Apply( Normalized.SignatureType     sig     ,
                                 MetaDataNormalizationContext context )
            {
                Normalized.MetaDataTypeDefinitionAbstract classObject;

                context.GetNormalizedObject( m_classObject, out classObject, MetaDataNormalizationMode.Default );

                sig.m_modifiers = ArrayUtility.AppendToArray( sig.m_modifiers, classObject );

                if(m_next != null)
                {
                    m_next.Apply( sig, context );
                }
            }

            //
            // Access Methods
            //

            public ElementTypes Kind
            {
                get
                {
                    return m_mod;
                }
            }

            public IMetaDataTypeDefOrRef ClassObject
            {
                get
                {
                    return m_classObject;
                }
            }

            public Modifier Next
            {
                get
                {
                    return m_next;
                }
            }

            //
            // Debug Methods
            //

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();

                sb.Append( m_mod         );
                sb.Append( "("           );
                sb.Append( m_classObject );
                sb.Append( ")"           );

                if(m_next != null)
                {
                    sb.Append( " " );

                    sb.Append( m_next );
                }

                return sb.ToString();
            }
        }
    }

    //--//

    public abstract class Signature
    {
        [Flags]
        internal enum ParseFlags
        {
            Default              = 0x00,
            AllowCustomModifiers = 0x01,
            AllowPinned          = 0x02,
            AllowByRef           = 0x04,
            AllowTypedByRef      = 0x08,
            AllowVoid            = 0x10,
            MustBeWholeSignature = 0x20,
        }

        public enum CallingConventions : byte
        {
            Default            = 0x00,
            Unmanaged_cdecl    = 0x01,
            Unmanaged_sdtcall  = 0x02,
            Unmanaged_thiscall = 0x03,
            Unmanaged_fastcall = 0x04,
            VarArg             = 0x05,
            Field              = 0x06,
            LocalVar           = 0x07,
            Property           = 0x08,
            Unmanaged          = 0x09,
            GenericInst        = 0x0A,
            Mask               = 0x0F,
            Generic            = 0x10,
            HasThis            = 0x20,
            ExplicitThis       = 0x40
        }

        //
        // Helper Methods
        //

        internal static void CheckEndOfSignature( ArrayReader reader )
        {
            if(reader.IsEOF == false)
            {
                throw IllegalMetaDataFormatException.Create( "Only read {0} bytes of signature {1}", reader.Position, reader );
            }
        }

        internal static Signature ParseUnknown( Parser      parser ,
                                                ArrayReader reader )
        {
            return Signature.ParseUnknown( parser, reader, ParseFlags.MustBeWholeSignature );
        }

        internal static Signature ParseUnknown( Parser      parser ,
                                                ArrayReader reader ,
                                                ParseFlags  flags  )
        {
            if((flags & ParseFlags.MustBeWholeSignature) != 0)
            {
                flags &= ~ParseFlags.MustBeWholeSignature;

                Signature type = ParseUnknown( parser, reader, flags );

                Signature.CheckEndOfSignature( reader );

                //Console.Out.WriteLine( "{0}", type );

                return type;
            }

            //--//

            uint unmaskedCallingConvention = reader.ReadCompressedUInt32();

            CallingConventions callingConvention = (CallingConventions)(unmaskedCallingConvention & (uint)CallingConventions.Mask);

            switch(callingConvention)
            {
                case CallingConventions.Field:
                    {
                        //
                        // Section 23.2.4 of ECMA spec, Partition II
                        //

                        //
                        // ParseFlags.AllowByRef is added to allow parsing of managed C++ assemblies.
                        //
                        SignatureType type = Signature.ParseType( parser, reader, ParseFlags.AllowCustomModifiers | ParseFlags.AllowByRef );

                        return new SignatureField( type );
                    }

                case CallingConventions.LocalVar:
                    {
                        //
                        // Section 23.2.6 of ECMA spec, Partition II
                        //
                        uint count = reader.ReadCompressedUInt32();

                        SignatureType[] locals = new SignatureType[count];
                        for(int i = 0; i < count; i++)
                        {
                            locals[i] = Signature.ParseSignatureLocal( parser, reader );
                        }

                        return new SignatureLocalVar( locals );
                    }

                case CallingConventions.Property:
                    {
                        //
                        // Section 23.2.5 of ECMA spec, Partition II
                        //
                        uint            paramCount = reader.ReadCompressedUInt32();
                        SignatureType   returnType = Signature.ParseReturnType( parser, reader );
                        SignatureType[] parameters = new SignatureType[paramCount];

                        for(int i = 0; i < paramCount; i++)
                        {
                            parameters[i] = Signature.ParseParam( parser, reader );
                        }

                        return new SignatureProperty( returnType, parameters );
                    }

                case CallingConventions.GenericInst:
                    {
                        //
                        // Section 22.29, 23.2.15 of ECMA spec, Partition II
                        //
                        uint            genericParamCount = reader.ReadCompressedUInt32();
                        SignatureType[] genericParameters = new SignatureType[genericParamCount];

                        for(int i = 0; i < genericParamCount; i++)
                        {
                            genericParameters[i] = Signature.ParseParam( parser, reader );
                        }

                        return new SignatureMethodSpec( genericParameters );
                    }

                default:
                    {
                        //
                        // Section 23.2.1, 23.2.2, and 23.2.3 of ECMA spec, Partition II
                        //
                        uint genericParamCount = 0;

                        if((unmaskedCallingConvention & (uint)CallingConventions.Generic) != 0)
                        {
                            genericParamCount = reader.ReadCompressedUInt32();
                        }

                        uint            paramCount       = reader.ReadCompressedUInt32();
                        SignatureType   returnType       = Signature.ParseReturnType( parser, reader );
                        int             sentinelLocation = -1;
                        SignatureType[] parameters       = new SignatureType[paramCount];

                        for(int i = 0; i < paramCount; i++)
                        {
                            byte first = reader.PeekUInt8();
                            if(first == (byte)ElementTypes.SENTINEL)
                            {
                                reader.Seek( 1 );

                                sentinelLocation = i;
                            }

                            parameters[i] = Signature.ParseParam( parser, reader );
                        }

                        return new SignatureMethod( (CallingConventions)unmaskedCallingConvention, genericParamCount, sentinelLocation, returnType, parameters );
                    }
            }
        }

        internal static Signature ParseMemberRef( Parser      parser ,
                                                  ArrayReader reader )
        {
            Signature sig = Signature.ParseUnknown( parser, reader );

            if(sig is SignatureField)
            {
            }
            else if(sig is SignatureMethod)
            {
            }
            else
            {
                throw IllegalMetaDataFormatException.Create( "Not a member ref signature: {0}", sig );
            }

            return sig;
        }

        internal static SignatureType ParseType( Parser      parser ,
                                                 ArrayReader reader ,
                                                 ParseFlags  flags  )
        {
            if((flags & ParseFlags.AllowCustomModifiers) != 0)
            {
                //
                // The flag is not reset to allow parsing of managed C++ assemblies,
                // where a type can have multiple custom modifiers throughout the signature.
                //
                SignatureType.Modifier modifier = ParseOptionalModifier( parser, reader );
                if(modifier != null)
                {
                    SignatureType type = Signature.ParseType( parser, reader, flags );

                    type.SetModifierChain( modifier );

                    return type;
                }
            }

            ElementTypes elementType = (ElementTypes)reader.ReadUInt8();
            switch(elementType)
            {
                case ElementTypes.VOID:
                    {
                        if((flags & ParseFlags.AllowVoid) == 0)
                        {
                            throw IllegalMetaDataFormatException.Create( "Unexpected VOID element at {0} in {1}", reader.Position, reader );
                        }

                        goto case ElementTypes.BOOLEAN;
                    }

                case ElementTypes.TYPEDBYREF:
                    {
                        // APPCOMPACT: Although not allowed, v1.1 mscorlib.dll contains a TYPEDBYREF field...
                        //if((flags & ParseFlags.AllowTypedByRef) == 0)
                        //{
                        //    throw new IllegalMetaDataFormatException( "Unexpected TYPEDBYREF element at " + reader.Position + " in " + reader );
                        //}

                        goto case ElementTypes.BOOLEAN;
                    }

                case ElementTypes.BOOLEAN:
                case ElementTypes.CHAR   :
                case ElementTypes.I1     :
                case ElementTypes.U1     :
                case ElementTypes.I2     :
                case ElementTypes.U2     :
                case ElementTypes.I4     :
                case ElementTypes.U4     :
                case ElementTypes.I8     :
                case ElementTypes.U8     :
                case ElementTypes.R4     :
                case ElementTypes.R8     :
                case ElementTypes.U      :
                case ElementTypes.I      :
                case ElementTypes.OBJECT :
                case ElementTypes.STRING :
                    {
                        return new SignatureType.BuiltIn( elementType );
                    }

                case ElementTypes.VALUETYPE:
                case ElementTypes.CLASS    :
                    {
                        // Followed by: TypeDefOrRefEncoded
                        int typeEncoded = reader.ReadCompressedToken();

                        return new SignatureType.ClassOrStruct( elementType, (IMetaDataTypeDefOrRef)parser.getObjectFromToken( typeEncoded ) );
                    }

                case ElementTypes.SZARRAY:
                    {
                        // Followed by: CustomMod* Type
                        SignatureType type = Signature.ParseType( parser, reader, ParseFlags.AllowCustomModifiers );

                        return new SignatureType.SzArray( type );
                    }

                case ElementTypes.ARRAY:
                    {
                        // Followed by: Type ArrayShape
                        SignatureType type = Signature.ParseType( parser, reader, ParseFlags.Default );

                        uint rank = reader.ReadCompressedUInt32();
                        if(rank == 0)
                        {
                            throw IllegalMetaDataFormatException.Create( "ARRAY with rank 0" );
                        }

                        SignatureType.Array.Dimension[] dimensions = new SignatureType.Array.Dimension[rank];

                        uint numUpperBounds = reader.ReadCompressedUInt32();
                        if(numUpperBounds > rank)
                        {
                            throw IllegalMetaDataFormatException.Create( "ARRAY with upper bounds > rank" );
                        }

                        for(int i = 0; i < numUpperBounds; i++)
                        {
                            dimensions[i].m_upperBound = reader.ReadCompressedUInt32();
                        }

                        uint numLowerBounds = reader.ReadCompressedUInt32();
                        if(numLowerBounds > rank)
                        {
                            throw IllegalMetaDataFormatException.Create( "ARRAY with lower bounds > rank" );
                        }

                        for(int i = 0; i < numLowerBounds; i++)
                        {
                            dimensions[i].m_lowerBound = reader.ReadCompressedUInt32();
                        }

                        return new SignatureType.Array( type, rank, dimensions );
                    }

                case ElementTypes.FNPTR:
                    {
                        // Followed by: MethodDefSig or MethodRefSig

                        SignatureMethod signature = SignatureMethod.Parse( parser, reader, ParseFlags.Default );

                        return new SignatureType.Method( signature );
                    }

                case ElementTypes.VAR :
                case ElementTypes.MVAR:
                    {
                        // Generic type variables
                        uint number = reader.ReadCompressedUInt32();

                        return new SignatureType.GenericParameter( elementType, number );
                    }

                case ElementTypes.GENERICINST:
                    {
                        // Generic type instantiation
                        SignatureType type = Signature.ParseType( parser, reader, ParseFlags.Default );

                        uint            typeCount  = reader.ReadCompressedUInt32();
                        SignatureType[] typeParams = new SignatureType[typeCount];
                        for(int i = 0; i < typeCount; i++)
                        {
                            SignatureType paramType =

                            typeParams[i] = Signature.ParseType( parser, reader, ParseFlags.Default );
                        }

                        return new SignatureType.GenericInstantiation( type, typeParams );
                    }

                case ElementTypes.CMOD_REQD:
                case ElementTypes.CMOD_OPT :
                    throw IllegalMetaDataFormatException.Create( "Unexpected custom modifier: 0x{0:X2} at {1} in {2}", (byte)elementType, reader.Position, reader );

                case ElementTypes.PINNED:
                    {
                        // APPCOMPACT: Although not allowed, v1.1 mscorlib.dll contains a PINNED field...
                        //if((flags & ParseFlags.AllowPinned) == 0)
                        //{
                        //    throw new IllegalMetaDataFormatException( "Unexpected PINNED prefix at " + reader.Position + " in " + reader );
                        //}

                        flags &= ~ParseFlags.AllowPinned;

                        // APPCOMPACT: fixed (void* pBuffer) gets encoded as PINNED BYREF VOID.
                        if((flags & ParseFlags.AllowByRef) != 0)
                        {
                            flags |= ParseFlags.AllowVoid;
                        }

                        SignatureType type = Signature.ParseType( parser, reader, flags );

                        return new SignatureType.Prefix( elementType, type );
                    }

                case ElementTypes.PTR:
                    {
                        // Modifiers
                        SignatureType type = Signature.ParseType( parser, reader, ParseFlags.AllowCustomModifiers | ParseFlags.AllowVoid );

                        return new SignatureType.Prefix( elementType, type );
                    }

                case ElementTypes.BYREF:
                    {
                        if((flags & ParseFlags.AllowByRef) == 0)
                        {
                            throw IllegalMetaDataFormatException.Create( "Unexpected BYREF prefix at {0} in {1}", reader.Position, reader );
                        }

                        flags &= ~ParseFlags.AllowByRef;

                        SignatureType type = Signature.ParseType( parser, reader, flags );

                        return new SignatureType.Prefix( elementType, type );
                    }

                default:
                    {
                        throw IllegalMetaDataFormatException.Create( "Unknown signature type: 0x{0:X2} at {1} in {2}", (byte)elementType, reader.Position, reader );
                    }
            }
        }

        protected static SignatureType.Modifier ParseOptionalModifier( Parser      parser ,
                                                                       ArrayReader reader )
        {
            SignatureType.Modifier result = null;

            while(reader.IsEOF == false)
            {
                ElementTypes mod = (ElementTypes)reader.PeekUInt8();

                if(mod == ElementTypes.CMOD_REQD ||
                   mod == ElementTypes.CMOD_OPT   )
                {
                    reader.Seek( 1 );

                    int typeEncoded = reader.ReadCompressedToken();

                    result = new SignatureType.Modifier( mod, (IMetaDataTypeDefOrRef)parser.getObjectFromToken( typeEncoded ), result );
                }
                else
                {
                    break;
                }
            }

            return result;
        }

        protected static SignatureType ParseParam( Parser      parser ,
                                                   ArrayReader reader )
        {
            return Signature.ParseType( parser, reader, ParseFlags.AllowCustomModifiers | ParseFlags.AllowByRef | ParseFlags.AllowTypedByRef );
        }

        protected static SignatureType ParseReturnType( Parser      parser ,
                                                        ArrayReader reader )
        {
            return Signature.ParseType( parser, reader, ParseFlags.AllowCustomModifiers | ParseFlags.AllowByRef | ParseFlags.AllowTypedByRef | ParseFlags.AllowVoid );
        }

        protected static SignatureType ParseSignatureLocal( Parser      parser ,
                                                            ArrayReader reader )
        {
            return Signature.ParseType( parser, reader, ParseFlags.AllowCustomModifiers | ParseFlags.AllowPinned | ParseFlags.AllowByRef | ParseFlags.AllowTypedByRef );
        }
    }
}
