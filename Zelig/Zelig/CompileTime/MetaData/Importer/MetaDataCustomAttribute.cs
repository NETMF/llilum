//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//
// Originally based on the Bartok code base.
//

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Zelig.MetaData.Importer
{
    public sealed class MetaDataCustomAttribute : MetaDataObject,
        IMetaDataNormalize
    {
        //
        // State
        //

        private IMetaDataHasCustomAttribute  m_parent;
        private IMetaDataCustomAttributeType m_type;
        private ArrayReader                  m_buffer;

        //
        // Constructor Methods
        //

        private MetaDataCustomAttribute( int index ) : base( TokenType.CustomAttribute, index )
        {
        }

        // Helper methods to work around limitations in generics, see Parser.InitializeTable<T>

        internal static MetaDataObject.CreateInstance GetCreator()
        {
            return new MetaDataObject.CreateInstance( Creator );
        }

        private static MetaDataObject Creator( int index )
        {
            return new MetaDataCustomAttribute( index );
        }

        //--//

        internal override void Parse( Parser             parser ,
                                      Parser.TableSchema ts     ,
                                      ArrayReader        reader )
        {
            Parser.IndexReader parentReader = ts.m_columns[0].m_reader;
            Parser.IndexReader typeReader   = ts.m_columns[1].m_reader;
            int                parentIndex;
            int                typeIndex;

            parentIndex =                         parentReader   ( reader );
            typeIndex   =                         typeReader     ( reader );
            m_buffer    = new ArrayReader( parser.readIndexAsBlob( reader ) );

            m_parent = parser.getHasCustomAttribute ( parentIndex );
            m_type   = parser.getCustomAttributeType( typeIndex  );

            ((MetaDataObject)m_parent).AddCustomAttribute( this );

            //// custom attribute starts with a prolog with value of 0x0001
            //if(this.valueBuffer.Length < 2)
            //{
            //    Console.Out.WriteLine( "WARNING: Custome Attrbute should have at least two bytes in size" );
            //}
        }

        //
        // IMetaDataNormalize methods
        //

        Normalized.MetaDataObject IMetaDataNormalize.AllocateNormalizedObject( MetaDataNormalizationContext context )
        {
            switch(context.Phase)
            {
                case MetaDataNormalizationPhase.ResolutionOfCustomAttributes:
                    {
                        Normalized.MetaDataMethodBase method;

                        context.GetNormalizedObject( m_type, out method, MetaDataNormalizationMode.Default );

                        if(method.Name != ".ctor")
                        {
                            throw IllegalMetaDataFormatException.Create( "Custom attribute with unexpected method name: {0}", method.Name );
                        }

                        ArrayReader reader = new ArrayReader( m_buffer, 0 );

                        if(reader.ReadUInt8() != 0x01 ||
                           reader.ReadUInt8() != 0x00  )
                        {
                            throw IllegalMetaDataFormatException.Create( "Custom Attribute doesn't start with 0x0001!" );
                        }

                        Normalized.SignatureMethod signature  = method.Signature;
                        Normalized.SignatureType[] parameters = signature.Parameters;
                        int                        fixedCount = parameters.Length;

                        Object[] fixedArgs = new Object[fixedCount];

                        for(int i = 0; i < fixedCount; i++)
                        {
                            Normalized.SignatureType parameter = parameters[i];

                            Object value = ExtractParameter( parameter.Type, reader, context );

                            fixedArgs[i] = value;
                        }

                        short namedCount = ((reader.IsEOF) ? (short)0 : reader.ReadInt16());

                        ////if(namedCount > this.buffer.Length && this.Name == "System.Runtime.CompilerServices.RequiredAttributeAttribute")
                        ////{
                        ////    // Some CLR libraries have been compiled against a version of
                        ////    // mscorlib that had a fixed parameter to RequiredAttribute.
                        ////    // Simply ignore whatever the parameter was!
                        ////    namedCount = 0;
                        ////}

                        Normalized.MetaDataCustomAttribute.NamedArg[] namedArgs = new Normalized.MetaDataCustomAttribute.NamedArg[namedCount];
                        for(int i = 0; i < namedCount; i++)
                        {
                            SerializationTypes propOrField = (SerializationTypes)reader.ReadUInt8();

                            if(propOrField == SerializationTypes.FIELD || propOrField == SerializationTypes.PROPERTY)
                            {
                                SerializationTypes fieldType   = (SerializationTypes)reader.ReadUInt8();
                                SerializationTypes arrayType   = (SerializationTypes)ElementTypes.END;
                                String             enumName    =                     null;

                                switch(fieldType)
                                {
                                    case SerializationTypes.SZARRAY:
                                        {
                                            arrayType = (SerializationTypes)reader.ReadUInt8();
                                            if(arrayType == SerializationTypes.ENUM)
                                            {
                                                throw new Exception( "Not implemented: Array of ENUM for named field/property" );
                                            }
                                        }
                                        break;

                                    case SerializationTypes.ENUM:
                                        {
                                            enumName = reader.ReadCompressedString();
                                        }
                                        break;
                                }

                                String name = reader.ReadCompressedString();
                                Object value;

                                if(fieldType == SerializationTypes.TAGGED_OBJECT)
                                {
                                    fieldType = (SerializationTypes)reader.ReadUInt8();
                                }

                                if(enumName != null)
                                {
                                    Normalized.MetaDataTypeDefinitionBase typeDef = context.ResolveName( enumName );

                                    value = ExtractEnumValue( typeDef, reader, context );
                                }
                                else if(fieldType == SerializationTypes.SZARRAY)
                                {
                                    value = ExtractArrayValue( arrayType, reader, context );
                                }
                                else
                                {
                                    value = ExtractValue( fieldType, reader, context );
                                }

                                namedArgs[i] = new Normalized.MetaDataCustomAttribute.NamedArg( propOrField == SerializationTypes.FIELD, -1, name, fieldType, value );
                            }
                            else
                            {
                                throw IllegalMetaDataFormatException.Create( "Unknown prop-or-field type: {0}", propOrField );
                            }
                        }

                        Normalized.MetaDataCustomAttribute ca = new Normalized.MetaDataCustomAttribute( context.GetAssemblyFromContext(), m_token );

                        ca.m_constructor = method;
                        ca.m_fixedArgs   = fixedArgs;
                        ca.m_namedArgs   = namedArgs;

                        return ca.MakeUnique();
                    }
            }

            throw context.InvalidPhase( this );
        }

        void IMetaDataNormalize.ExecuteNormalizationPhase( Normalized.IMetaDataObject   obj     ,
                                                           MetaDataNormalizationContext context )
        {
            throw context.InvalidPhase( this );
        }

        //--//

        private static Object ExtractParameter( Normalized.MetaDataTypeDefinitionAbstract type    ,
                                                ArrayReader                               reader  ,
                                                MetaDataNormalizationContext              context )
        {
            switch(type.ElementType)
            {
                case ElementTypes.VALUETYPE:
                    {
                        String superName = type.Extends.FullName;

                        if(superName != "System.Enum")
                        {
                            throw IllegalMetaDataFormatException.Create( "Found valuetype that wasn't an Enum: {0}", type.Extends );
                        }

                        if(!(type is Normalized.MetaDataTypeDefinition))
                        {
                            throw IllegalMetaDataFormatException.Create( "Found valuetype that wasn't a simple type: {0}", type );
                        }

                        return ExtractEnumValue( (Normalized.MetaDataTypeDefinition)type, reader, context );
                    }

                case ElementTypes.CLASS:
                    {
                        String className = type.FullName;

                        // handle cases for reference types first 
                        if(className == "System.String")
                        {
                            return reader.ReadCompressedString();
                        }
                        else if(className == "System.Object")
                        {
                            goto case ElementTypes.OBJECT;
                        }
                        else if(className == "System.Type")
                        {
                            string typeName = reader.ReadCompressedString();

                            return context.ResolveName( typeName );
                        }

                        // Enums are just ints, e.g. the case of AttributeTargets param for AttributeUsage attribute
                        if (type.Extends.ElementType == ElementTypes.VALUETYPE)
                        {
                            return reader.ReadInt32();
                        }

                        throw new Exception( "Not implemented: object encoding an array (class was " + type +")" );
                    }

                case ElementTypes.OBJECT:
                case (ElementTypes)SerializationTypes.TAGGED_OBJECT:
                    {
                        SerializationTypes objectType = (SerializationTypes)reader.ReadUInt8();

                        switch(objectType)
                        {
                            case SerializationTypes.ENUM:
                            case SerializationTypes.TYPE:
                                {
                                    return ExtractValue( objectType, reader, context );
                                }

                            default:
                                {
                                    throw new Exception( "Found OBJECT type with type " + objectType );
                                }
                        }
                    }

                case ElementTypes.SZARRAY:
                    {
                        Normalized.MetaDataTypeDefinitionArraySz typeArray = (Normalized.MetaDataTypeDefinitionArraySz)type;

                        return ExtractArrayValue( typeArray.ObjectType, reader, context );
                    }

                default:
                    {
                        return ExtractValue( (SerializationTypes)type.ElementType, reader, context );
                    }
            }
        }

        private static Object ExtractValue( SerializationTypes           type    ,
                                            ArrayReader                  reader  ,
                                            MetaDataNormalizationContext context )
        {
            switch(type)
            {
                case SerializationTypes.BOOLEAN: return       reader.ReadBoolean();
                case SerializationTypes.CHAR   : return (char)reader.ReadUInt16 ();
                case SerializationTypes.I1     : return       reader.ReadInt8   ();
                case SerializationTypes.U1     : return       reader.ReadUInt8  ();
                case SerializationTypes.I2     : return       reader.ReadInt16  ();
                case SerializationTypes.U2     : return       reader.ReadUInt16 ();
                case SerializationTypes.I4     : return       reader.ReadInt32  ();
                case SerializationTypes.U4     : return       reader.ReadUInt32 ();
                case SerializationTypes.I8     : return       reader.ReadInt64  ();
                case SerializationTypes.U8     : return       reader.ReadUInt64 ();
                case SerializationTypes.R4     : return       reader.ReadSingle ();
                case SerializationTypes.R8     : return       reader.ReadDouble ();

                case SerializationTypes.STRING:
                    return reader.ReadCompressedString();

                case SerializationTypes.TYPE:
                    {
                        String typeName = reader.ReadCompressedString();

                        return context.ResolveName( typeName );
                    }

                case SerializationTypes.ENUM:
                    {
                        String typeName = reader.ReadCompressedString();

                        Normalized.MetaDataTypeDefinitionBase typeDef = context.ResolveName( typeName );

                        return ExtractEnumValue( typeDef, reader, context );
                    }

                default:
                    throw IllegalMetaDataFormatException.Create( "Found unexpected type {0} in custom attribute parameter", type );
            }
        }

        private static Object ExtractEnumValue( Normalized.MetaDataTypeDefinitionBase typeDef ,
                                                ArrayReader                           reader  ,
                                                MetaDataNormalizationContext          context )
        {
            foreach(Normalized.MetaDataField classField in typeDef.Fields)
            {
                if((classField.Flags & FieldAttributes.Static) == 0)
                {
                    if(classField.Name != "value__")
                    {
                        throw IllegalMetaDataFormatException.Create( "Found enum with non-static field '{0}'", classField.Name );
                    }

                    return ExtractValue( (SerializationTypes)classField.FieldSignature.TypeSignature.Type.ElementType, reader, context );
                }
            }

            throw IllegalMetaDataFormatException.Create( "Found enum without non-static field" );
        }

        private static Object ExtractArrayValue( Normalized.MetaDataTypeDefinitionAbstract type    ,
                                                 ArrayReader                               reader  ,
                                                 MetaDataNormalizationContext              context )
        {
            int arraySize = reader.ReadInt32();

            if(arraySize >= 0)
            {
                if(type.ElementType == ElementTypes.CLASS)
                {
                    Object[] array = new Object[arraySize];

                    for(int i = 0; i < arraySize; i++)
                    {
                        array[i] = ExtractParameter( type, reader, context );
                    }

                    return array;
                }
                else
                {
                    return ExtractArrayValue( (SerializationTypes)type.ElementType, arraySize, reader, context );
                }
            }
            else
            {
                throw new Exception( "Not implemented: custom attribute class array with negative length" );
            }
        }


        private static Object ExtractArrayValue( SerializationTypes           elementType ,
                                                 ArrayReader                  reader      ,
                                                 MetaDataNormalizationContext context     )
        {
            int arraySize = reader.ReadInt32();

            if(arraySize >= 0)
            {
                return ExtractArrayValue( elementType, arraySize, reader, context );
            }
            else
            {
                throw new Exception( "Not implemented: custom atribute array with negative length" );
            }
        }


        private static Object ExtractArrayValue( SerializationTypes           elementType ,
                                                 int                          arraySize   ,
                                                 ArrayReader                  reader      ,
                                                 MetaDataNormalizationContext context     )
        {
            switch(elementType)
            {
                case SerializationTypes.BOOLEAN:
                    {
                        bool[] array = new bool[arraySize];

                        for(int i = 0; i < arraySize; i++)
                        {
                            array[i] = reader.ReadBoolean();
                        }

                        return array;
                    }

                case SerializationTypes.CHAR:
                    {
                        char[] array = new char[arraySize];

                        for(int i = 0; i < arraySize; i++)
                        {
                            array[i] = (char)reader.ReadUInt16();
                        }

                        return array;
                    }

                case SerializationTypes.I1:
                    {
                        sbyte[] array = new sbyte[arraySize];

                        for(int i = 0; i < arraySize; i++)
                        {
                            array[i] = reader.ReadInt8();
                        }

                        return array;
                    }

                case SerializationTypes.U1:
                    {
                        byte[] array = new byte[arraySize];

                        for(int i = 0; i < arraySize; i++)
                        {
                            array[i] = reader.ReadUInt8();
                        }

                        return array;
                    }

                case SerializationTypes.I2:
                    {
                        short[] array = new short[arraySize];

                        for(int i = 0; i < arraySize; i++)
                        {
                            array[i] = reader.ReadInt16();
                        }

                        return array;
                    }

                case SerializationTypes.U2:
                    {
                        ushort[] array = new ushort[arraySize];

                        for(int i = 0; i < arraySize; i++)
                        {
                            array[i] = reader.ReadUInt16();
                        }

                        return array;
                    }

                case SerializationTypes.I4:
                    {
                        int[] array = new int[arraySize];

                        for(int i = 0; i < arraySize; i++)
                        {
                            array[i] = reader.ReadInt32();
                        }

                        return array;
                    }

                case SerializationTypes.U4:
                    {
                        uint[] array = new uint[arraySize];

                        for(int i = 0; i < arraySize; i++)
                        {
                            array[i] = reader.ReadUInt32();
                        }

                        return array;
                    }

                case SerializationTypes.I8:
                    {
                        long[] array = new long[arraySize];

                        for(int i = 0; i < arraySize; i++)
                        {
                            array[i] = reader.ReadInt64();
                        }

                        return array;
                    }

                case SerializationTypes.U8:
                    {
                        ulong[] array = new ulong[arraySize];

                        for(int i = 0; i < arraySize; i++)
                        {
                            array[i] = reader.ReadUInt64();
                        }

                        return array;
                    }

                case SerializationTypes.R4:
                    {
                        float[] array = new float[arraySize];

                        for(int i = 0; i < arraySize; i++)
                        {
                            array[i] = reader.ReadSingle();
                        }

                        return array;
                    }

                case SerializationTypes.R8:
                    {
                        double[] array = new double[arraySize];

                        for(int i = 0; i < arraySize; i++)
                        {
                            array[i] = reader.ReadDouble();
                        }

                        return array;
                    }

                case SerializationTypes.STRING:
                    {
                        String[] array = new String[arraySize];

                        for(int i = 0; i < arraySize; i++)
                        {
                            array[i] = reader.ReadCompressedString();
                        }

                        return array;
                    }

                case SerializationTypes.OBJECT:
                    {
                        Object[] array = new Object[arraySize];

                        for(int i = 0; i < arraySize; i++)
                        {
                            SerializationTypes type = (SerializationTypes)reader.ReadUInt8();

                            array[i] = ExtractValue( type, reader, context );
                        }

                        return array;
                    }

                default:
                    throw new Exception( "Not implemented: custom attribute array of type " + elementType );
            }
        }

        //
        // Access Methods
        //

        public IMetaDataHasCustomAttribute Parent
        {
            get
            {
                return m_parent;
            }
        }

        public IMetaDataCustomAttributeType Type
        {
            get
            {
                return m_type;
            }
        }

        //--//

        //
        // Debug Methods
        //

        public override String ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder( "MetaDataCustomAttribute(" );

            sb.Append( m_type );
            sb.Append( "," );

            sb.Append( m_parent );
            sb.Append( ")[" );
            sb.Append( m_buffer );
            sb.Append( "]" );

            return sb.ToString();
        }
    }
}
