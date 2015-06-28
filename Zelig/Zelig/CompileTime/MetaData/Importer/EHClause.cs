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
    public class EHClause
    {
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

        private readonly ExceptionFlag         m_flags;
        private readonly int                   m_tryOffset;
        private readonly int                   m_tryEnd;
        private readonly int                   m_handlerOffset;
        private readonly int                   m_handlerEnd;
        private readonly int                   m_filterOffset;
        private          IMetaDataTypeDefOrRef m_classObject;

        //
        // Constructor Methods
        //

        internal EHClause( int                   flags         ,
                           int                   tryOffset     ,
                           int                   tryLength     ,
                           int                   handlerOffset ,
                           int                   handlerLength ,
                           int                   filterOffset  ,
                           IMetaDataTypeDefOrRef classObject   )
        {
            m_flags         = (ExceptionFlag)flags;
            m_tryOffset     = tryOffset;
            m_tryEnd        = tryOffset + tryLength;
            m_handlerOffset = handlerOffset;
            m_handlerEnd    = handlerOffset + handlerLength;
            m_filterOffset  = filterOffset;
            m_classObject   = classObject;
        }

        internal Normalized.EHClause Normalize( MetaDataNormalizationContext context )
        {
            Normalized.EHClause ehNew = new Normalized.EHClause();

            ehNew.m_flags         = (Normalized.EHClause.ExceptionFlag)m_flags;
            ehNew.m_tryOffset     =                                    m_tryOffset;
            ehNew.m_tryEnd        =                                    m_tryEnd;
            ehNew.m_handlerOffset =                                    m_handlerOffset;
            ehNew.m_handlerEnd    =                                    m_handlerEnd;
            ehNew.m_filterOffset  =                                    m_filterOffset;

            context.GetNormalizedObject( m_classObject, out ehNew.m_classObject, MetaDataNormalizationMode.Default );

            return ehNew;
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

        public IMetaDataTypeDefOrRef TypeObject
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

            sb.Append( m_flags );
            sb.Append( ",try(" );
            sb.Append( m_tryOffset.ToString( "x" ) );
            sb.Append( "," );
            sb.Append( m_tryEnd.ToString( "x" ) );
            sb.Append( "),handler(" );
            sb.Append( m_handlerOffset.ToString( "x" ) );
            sb.Append( "," );
            sb.Append( m_handlerEnd.ToString( "x" ) );
            sb.Append( ")," );
            sb.Append( m_filterOffset.ToString( "x" ) );
            sb.Append( "," );
            sb.Append( m_classObject );

            sb.Append( ")" );

            return sb.ToString();
        }
    }
}
