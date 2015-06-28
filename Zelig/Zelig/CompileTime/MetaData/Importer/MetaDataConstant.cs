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
    public sealed class MetaDataConstant : MetaDataObject,
        IMetaDataNormalize
    {
        //
        // State
        //

        private ElementTypes         m_type;
        private IMetaDataHasConstant m_parent;
        private byte[]               m_valueBuffer;

        //
        // Constructor Methods
        //

        private MetaDataConstant( int index ) : base( TokenType.Constant, index )
        {
        }

        // Helper methods to work around limitations in generics, see Parser.InitializeTable<T>

        internal static MetaDataObject.CreateInstance GetCreator()
        {
            return new MetaDataObject.CreateInstance( Creator );
        }

        private static MetaDataObject Creator( int index )
        {
            return new MetaDataConstant( index );
        }

        //--//

        internal override void Parse( Parser             parser ,
                                      Parser.TableSchema ts     ,
                                      ArrayReader        reader )
        {
            Parser.IndexReader parentReader = ts.m_columns[1].m_reader;
            byte               padding;
            int                parentIndex;

            m_type        = (ElementTypes)          reader.ReadUInt8();
            padding       =                         reader.ReadUInt8();
            parentIndex   =        parentReader   ( reader );
            m_valueBuffer = parser.readIndexAsBlob( reader );

            m_parent = parser.getHasConstant( parentIndex );

            if(m_parent is MetaDataField)
            {
                MetaDataField parentField = (MetaDataField)m_parent;

                parentField.SetConstant( this );
            }

            if(m_parent is MetaDataParam)
            {
                MetaDataParam parentParam = (MetaDataParam)m_parent;

                parentParam.SetConstant( this );
            }

            if(m_parent is MetaDataProperty)
            {
                MetaDataProperty parentProperty = (MetaDataProperty)m_parent;

                parentProperty.SetConstant( this );
            }
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
                        Normalized.MetaDataConstant constNew = new Normalized.MetaDataConstant( m_token );

                        constNew.m_value = this.Value;

                        return constNew;
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

        public ElementTypes Type
        {
            get
            {
                return m_type;
            }
        }

        // May be one of MetaData{Field,ParamDef,Property}
        public IMetaDataHasConstant Parent
        {
            get
            {
                return m_parent;
            }
        }

        public byte[] ValueBuffer
        {
            get
            {
                return m_valueBuffer;
            }
        }

        public Object ValueToType( ElementTypes type )
        {
            if(m_type == type || SameSizeType( type, this.Type ))
            {
                return ValueFromBuffer( type, this.ValueBuffer );
            }
            else
            {
                long value = LongValueFromBuffer( this.Type, this.ValueBuffer );
                switch(type)
                {
                    case ElementTypes.BOOLEAN: return (value != 0) ? true : false;
                    case ElementTypes.I1     : return (sbyte )value;
                    case ElementTypes.U1     : return (byte  )value;
                    case ElementTypes.CHAR   : return (char  )value;
                    case ElementTypes.I2     : return (short )value;
                    case ElementTypes.U2     : return (ushort)value;
                    case ElementTypes.I4     : return (int   )value;
                    case ElementTypes.U4     : return (uint  )value;
                    case ElementTypes.I8     : return (long  )value;
                    case ElementTypes.U8     : return (ulong )value;
                    default                  : return         value;
                }
            }
        }

        private bool SameSizeType( ElementTypes type1 ,
                                   ElementTypes type2 )
        {
            switch(type1)
            {
                case ElementTypes.BOOLEAN:
                case ElementTypes.I1:
                case ElementTypes.U1:
                    switch(type2)
                    {
                        case ElementTypes.BOOLEAN:
                        case ElementTypes.I1:
                        case ElementTypes.U1:
                            return true;
                    }
                    break;

                case ElementTypes.CHAR:
                case ElementTypes.I2:
                case ElementTypes.U2:
                    switch(type2)
                    {
                        case ElementTypes.CHAR:
                        case ElementTypes.I2:
                        case ElementTypes.U2:
                            return true;
                    }
                    break;

                case ElementTypes.I4:
                case ElementTypes.U4:
                    switch(type2)
                    {
                        case ElementTypes.I4:
                        case ElementTypes.U4:
                            return true;
                    }
                    break;

                case ElementTypes.I8:
                case ElementTypes.U8:
                    switch(type2)
                    {
                        case ElementTypes.I8:
                        case ElementTypes.U8:
                            return true;
                    }
                    break;

                case ElementTypes.U:
                case ElementTypes.I:
                    switch(type2)
                    {
                        case ElementTypes.U:
                        case ElementTypes.I:
                            return true;
                    }
                    break;
            }

            return false;
        }

        public Object Value
        {
            get
            {
                return ValueFromBuffer( this.Type, this.ValueBuffer );
            }
        }

        private static long LongValueFromBuffer( ElementTypes type   ,
                                                 byte[]       buffer )
        {
            ArrayReader reader = new ArrayReader( buffer );

            switch(type)
            {
                case ElementTypes.BOOLEAN: return       reader.ReadBoolean() ? 1L : 0L;
                case ElementTypes.CHAR   : return (long)reader.ReadChar  ();
                case ElementTypes.I1     : return (long)reader.ReadInt8  ();
                case ElementTypes.U1     : return (long)reader.ReadUInt8 ();
                case ElementTypes.I2     : return (long)reader.ReadInt16 ();
                case ElementTypes.U2     : return (long)reader.ReadUInt16();
                case ElementTypes.I4     : return (long)reader.ReadInt32 ();
                case ElementTypes.U4     : return (long)reader.ReadUInt32();
                case ElementTypes.I8     : return (long)reader.ReadInt64 ();
                case ElementTypes.U8     : return (long)reader.ReadUInt64();
                case ElementTypes.R4     : return (long)reader.ReadSingle();
                case ElementTypes.R8     : return (long)reader.ReadDouble();

                default:
                    throw IllegalMetaDataFormatException.Create( "Unknown type of constant: {0}", type );
            }
        }

        private static Object ValueFromBuffer( ElementTypes type   ,
                                               byte[]       buffer )
        {
            ArrayReader reader = new ArrayReader( buffer );

            switch(type)
            {
                case ElementTypes.BOOLEAN: return reader.ReadBoolean();
                case ElementTypes.CHAR   : return reader.ReadChar   ();
                case ElementTypes.I1     : return reader.ReadInt8   ();
                case ElementTypes.U1     : return reader.ReadUInt8  ();
                case ElementTypes.I2     : return reader.ReadInt16  ();
                case ElementTypes.U2     : return reader.ReadUInt16 ();
                case ElementTypes.I4     : return reader.ReadInt32  ();
                case ElementTypes.U4     : return reader.ReadUInt32 ();
                case ElementTypes.I8     : return reader.ReadInt64  ();
                case ElementTypes.U8     : return reader.ReadUInt64 ();
                case ElementTypes.R4     : return reader.ReadSingle ();
                case ElementTypes.R8     : return reader.ReadDouble ();
                case ElementTypes.STRING : return reader.ReadUInt16String( buffer.Length / 2 );

                default:
                    throw IllegalMetaDataFormatException.Create( "Unknown type of constant: {0}", type );
            }
        }

        //
        // Debug Methods
        //

        public override String ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder( "MetaDataConstant(" );

            sb.Append( m_type );
            sb.Append( "," );
            sb.Append( m_parent );
            sb.Append( "," );
            sb.Append( this.Value );

            sb.Append( ")" );

            return sb.ToString();
        }
    }
}
