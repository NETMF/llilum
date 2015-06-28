//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//
// Originally based on the Bartok code base.
//

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Microsoft.Zelig.MetaData.Importer
{
    public sealed class MetaDataFieldRVA : MetaDataObject,
        IMetaDataNormalize
    {
        // BUGBUG: Should be machine dependent!
        private const int c_machineIntSize = 4;

        //
        // State
        //

        private int           m_rva;
        private byte[]        m_dataBytes;
        private MetaDataField m_field;

        //
        // Constructor Methods
        //

        private MetaDataFieldRVA( int index ) : base( TokenType.FieldRVA, index )
        {
        }

        // Helper methods to work around limitations in generics, see Parser.InitializeTable<T>

        internal static MetaDataObject.CreateInstance GetCreator()
        {
            return new MetaDataObject.CreateInstance( Creator );
        }

        private static MetaDataObject Creator( int index )
        {
            return new MetaDataFieldRVA( index );
        }

        //--//

        internal override void Parse( Parser             parser ,
                                      Parser.TableSchema ts     ,
                                      ArrayReader        reader )
        {
            Parser.IndexReader fieldReader = ts.m_columns[1].m_reader;
            int                fieldIndex;

            m_rva      =              reader.ReadInt32();
            fieldIndex = fieldReader( reader );

            m_field = parser.getField( fieldIndex );

            SignatureField fieldSignature = (SignatureField)m_field.Signature;

            SignatureType fieldType = fieldSignature.FieldType;

            int dataSize = this.getFieldSize( fieldType );

            ArrayReader reader2 = parser.ImageReaderAtVirtualAddress( m_rva );
            if(reader2 != null)
            {
                m_dataBytes = reader2.ReadUInt8Array( dataSize );
            }
            else
            {
                m_dataBytes = new byte[dataSize];
            }

            m_field.SetRVA( this );
        }

        //
        // IMetaDataNormalize methods
        //

        Normalized.MetaDataObject IMetaDataNormalize.AllocateNormalizedObject( MetaDataNormalizationContext context )
        {
            switch(context.Phase)
            {
                case MetaDataNormalizationPhase.CreationOfFieldDefinitions:
                    {
                        Normalized.MetaDataFieldRVA fieldRVA = new Normalized.MetaDataFieldRVA( m_token );

                        fieldRVA.m_dataBytes = m_dataBytes;

                        return fieldRVA;
                    }
            }

            throw context.InvalidPhase( this );
        }

        void IMetaDataNormalize.ExecuteNormalizationPhase( Normalized.IMetaDataObject   obj     ,
                                                           MetaDataNormalizationContext context )
        {
            throw context.InvalidPhase( this );
        }

        //
        // Access Methods
        //

        public byte[] DataBytes
        {
            get
            {
                return m_dataBytes;
            }
        }

        public MetaDataField Field
        {
            get
            {
                return m_field;
            }
        }

        public int RVA
        {
            get
            {
                return m_rva;
            }
        }

        //
        // Helper Methods
        //

        private int getFieldSize( SignatureType fieldType )
        {
            switch(fieldType.ElementType)
            {
                case ElementTypes.VOID:
                    return 0;

                case ElementTypes.BOOLEAN:
                case ElementTypes.I1:
                case ElementTypes.U1:
                    return 1;

                case ElementTypes.CHAR:
                case ElementTypes.I2:
                case ElementTypes.U2:
                    return 2;

                case ElementTypes.I4:
                case ElementTypes.U4:
                case ElementTypes.R4:
                    return 4;

                case ElementTypes.I8:
                case ElementTypes.U8:
                case ElementTypes.R8:
                    return 8;

                case ElementTypes.OBJECT:
                case ElementTypes.STRING:
                case ElementTypes.FNPTR:
                case ElementTypes.CLASS:
                case ElementTypes.PTR:
                case ElementTypes.BYREF:
                case ElementTypes.U:
                case ElementTypes.I:
                    return c_machineIntSize;

                case ElementTypes.TYPEDBYREF:
                    return 2 * c_machineIntSize;

                case ElementTypes.VALUETYPE:
                    {
                        SignatureType.ClassOrStruct fieldTypeClassOrStruct = (SignatureType.ClassOrStruct)fieldType;

                        IMetaDataTypeDefOrRef classObject = fieldTypeClassOrStruct.ClassObject;

                        if(!(classObject is MetaDataTypeDefinition))
                        {
                            return -1;
                        }

                        MetaDataTypeDefinition typedef = (MetaDataTypeDefinition)classObject;

                        if((typedef.Flags & TypeAttributes.Interface) != 0 ||
                           (typedef.Flags & TypeAttributes.Abstract ) != 0  )
                        {
                            return -1;
                        }

                        int classSize = 0;
                        int packSize  = 0;

                        if(typedef.ClassLayout != null)
                        {
                            classSize = typedef.ClassLayout.ClassSize;
                            packSize  = typedef.ClassLayout.PackingSize;
                        }

                        int instanceFieldSize = 0;

                        if(typedef.Fields != null)
                        {
                            if((typedef.Flags & TypeAttributes.ExplicitLayout) != 0)
                            {
                                foreach(MetaDataField mdField in typedef.Fields)
                                {
                                    if((mdField.Flags & FieldAttributes.Static) == 0)
                                    {
                                        SignatureType nestedFieldType = mdField.Signature.FieldType;

                                        int fieldSize = this.getFieldSize( nestedFieldType );
                                        int offset    = mdField.Layout.Offset;
                                        int fieldEnd  = fieldSize + offset;

                                        if(fieldEnd > instanceFieldSize)
                                        {
                                            instanceFieldSize = fieldEnd;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                foreach(MetaDataField mdField in typedef.Fields)
                                {
                                    if((mdField.Flags & FieldAttributes.Static) == 0)
                                    {
                                        SignatureType nestedFieldType = mdField.Signature.FieldType;

                                        int fieldSize = this.getFieldSize( nestedFieldType );
                                        if(fieldSize == -1)
                                        {
                                            return -1;
                                        }

                                        if(packSize > 1)
                                        {
                                            int delta = instanceFieldSize % packSize;
                                            if(delta > 0)
                                            {
                                                instanceFieldSize += packSize - delta;
                                            }
                                        }

                                        instanceFieldSize += fieldSize;
                                    }
                                }
                            }
                        }

                        if(instanceFieldSize > classSize)
                        {
                            return instanceFieldSize;
                        }
                        else if(classSize > 0)
                        {
                            return classSize;
                        }
                        else
                        {
                            return 1;
                        }
                    }

                case ElementTypes.ARRAY:
                    {
                        SignatureType.Array fieldTypeArray = (SignatureType.Array)fieldType;

                        int                             elementCount = 1;
                        SignatureType.Array.Dimension[] dimensions   = fieldTypeArray.Dimensions;
                        int                             rank         = dimensions.Length;

                        for(int i = 0; i < rank; i++)
                        {
                            int dimSize = (int)(dimensions[i].m_upperBound - dimensions[i].m_lowerBound);
                            if(dimSize == 0)
                            {
                                // Must be an array of pointers to other arrays
                                return elementCount * c_machineIntSize;
                            }
                            else
                            {
                                elementCount *= dimSize;
                            }
                        }

                        int elementSize = this.getFieldSize( fieldTypeArray.BaseType );

                        return elementCount * elementSize;
                    }

                case ElementTypes.PINNED:
                    {
                        SignatureType.Prefix fieldTypePrefix = (SignatureType.Prefix)fieldType;

                        return this.getFieldSize( fieldTypePrefix.InnerType );
                    }

                case ElementTypes.SZARRAY:
                    // Should be machine dependent!
                    return 4;

                case ElementTypes.SENTINEL:
                case ElementTypes.END:
                default:
                    return -1;
            }
        }

        //
        // Debug Methods
        //

        public override String ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder( "MetaDataFieldRVA(" );

            sb.Append( m_field );
            sb.Append( "," );

            if(m_dataBytes == null)
            {
                sb.Append( m_rva );
            }
            else
            {
                sb.Append( "[" );

                for(int i = 0; i < m_dataBytes.Length; i++)
                {
                    if(i != 0)
                    {
                        sb.Append( "," );
                    }

                    sb.Append( m_dataBytes[i].ToString( "x2" ) );
                }

                sb.Append( "]" );
            }

            sb.Append( ")" );

            return sb.ToString();
        }
    }
}
