//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.MetaData.Normalized
{
    using System;

    //
    // Don't remove "sealed" attribute unless you read comments on Equals method.
    //
    public sealed class EHClause : IMetaDataUnique
    {
        public static readonly EHClause[] SharedEmptyArray = new EHClause[0];

        //
        // Internal Classes
        //

        public enum ExceptionFlag
        {
            None    = 0x0000,
            Filter  = 0x0001,
            Finally = 0x0002,
            Fault   = 0x0004,
        }

        //
        // State
        //

        internal ExceptionFlag                  m_flags;
        internal int                            m_tryOffset;
        internal int                            m_tryEnd;
        internal int                            m_handlerOffset;
        internal int                            m_handlerEnd;
        internal int                            m_filterOffset;
        internal MetaDataTypeDefinitionAbstract m_classObject;

        //
        // Constructor Methods
        //

        internal EHClause()
        {
        }

        //
        // MetaDataEquality Methods
        //

        public override bool Equals( object obj )
        {
            if(obj is EHClause) // Since the class is sealed (no subclasses allowed), there's no need to compare using .GetType()
            {
                EHClause other = (EHClause)obj;

                if(m_flags         == other.m_flags         &&
                   m_tryOffset     == other.m_tryOffset     &&
                   m_tryEnd        == other.m_tryEnd        &&
                   m_handlerOffset == other.m_handlerOffset &&
                   m_handlerEnd    == other.m_handlerEnd    &&
                   m_filterOffset  == other.m_filterOffset  &&
                   m_classObject   == other.m_classObject    )
                {
                    return true;
                }
            }

            return false;
        }

        public override int GetHashCode()
        {
            return m_tryOffset << 12 ^
                   m_tryEnd          ;
        }

        //
        // Access Methods
        //

        public ExceptionFlag Flags
        {
            get
            {
                return m_flags;
            }
        }

        public int TryOffset
        {
            get
            {
                return m_tryOffset;
            }
        }

        public int TryEnd
        {
            get
            {
                return m_tryEnd;
            }
        }

        public int HandlerOffset
        {
            get
            {
                return m_handlerOffset;
            }
        }

        public int HandlerEnd
        {
            get
            {
                return m_handlerEnd;
            }
        }

        public int FilterOffset
        {
            get
            {
                return m_filterOffset;
            }
        }

        public MetaDataTypeDefinitionAbstract TypeObject
        {
            get
            {
                return m_classObject;
            }
        }

        //
        // Debug Methods
        //

        public override String ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder( "EHClause(" );

            sb.Append      ( m_flags                                                );
            sb.AppendFormat( ",try({0:x},{1:x})"    , m_tryOffset    , m_tryEnd     );
            sb.AppendFormat( ",handler({0:x},{1:x})", m_handlerOffset, m_handlerEnd );
            sb.AppendFormat( ",filter({0:x})"       , m_filterOffset                );
            sb.Append      ( ","                                                    );
            sb.Append      ( m_classObject                                          );

            sb.Append( ")" );

            return sb.ToString();
        }
    }
}
