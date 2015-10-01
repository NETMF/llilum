//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TypeSystem
{
    using System;
    using System.Collections.Generic;

    public struct ConversionContext
    {
        //
        // State
        //

        public TypeRepresentation   m_typeContextOwner;
        public TypeRepresentation[] m_typeContext;

        public MethodRepresentation m_methodContextOwner;
        public TypeRepresentation[] m_methodContext;

        //--//

        public void Initialize()
        {
            SetContextAsTypeParameters( TypeRepresentation.SharedEmptyArray );
        }

        //--//

        public bool SameContents( ref ConversionContext other )
        {
            if(m_typeContextOwner   == other.m_typeContextOwner   &&
               m_methodContextOwner == other.m_methodContextOwner  )
            {
                if(ArrayUtility.ArrayEquals( m_typeContext  , other.m_typeContext   ) &&
                   ArrayUtility.ArrayEquals( m_methodContext, other.m_methodContext )  )
                {
                    return true;
                }
            }

            return false;
        }

        //--//

        //
        // Helper Methods
        //

        public void SetContextAsType( TypeRepresentation td )
        {
            CHECKS.ASSERT( td != null, "'td' cannot be null" );

            m_typeContextOwner   = td;
            m_typeContext        = td.GenericParameters;
            m_methodContextOwner = null;
            m_methodContext      = TypeRepresentation.SharedEmptyArray;
        }

        public void SetContextAsTypeParameters( TypeRepresentation[] parameters )
        {
            CHECKS.ASSERT( parameters != null, "'parameters' cannot be null" );

            m_typeContextOwner   = null;
            m_typeContext        = parameters;
            m_methodContextOwner = null;
            m_methodContext      = TypeRepresentation.SharedEmptyArray;
        }

        public void AdjustContextAsType( int numParameters )
        {
            //
            // When expanding a nested class, we need to expand the enclosing class.
            // If both the nested class and the enclosing class are generics, we need to drop type parameters
            // when we switch to the enclosing class.
            //
            if(m_typeContext.Length > numParameters)
            {
                m_typeContext = ArrayUtility.ExtractSliceFromNotNullArray( m_typeContext, 0, numParameters );
            }
        }

        public void SetContextAsMethod( MethodRepresentation md )
        {
            CHECKS.ASSERT( md != null, "'md' cannot be null" );

            m_methodContextOwner = md;
            m_methodContext      = md.GenericParameters;
        }

        public void SetContextAsMethodParameters( TypeRepresentation[] parameters )
        {
            CHECKS.ASSERT( parameters != null, "'parameters' cannot be null" );

            m_methodContextOwner = null;
            m_methodContext      = parameters;
        }

        //--//

        //
        // Debug Methods
        //

        public override string ToString()
        {
            System.Text.StringBuilder sb = new  System.Text.StringBuilder();

            InnerToString( sb );

            return sb.ToString();
        }

        internal void InnerToString( System.Text.StringBuilder sb )
        {
            if(m_typeContext != null && m_typeContext.Length > 0)
            {
                sb.Append( " => Type:[" );

                for(int i = 0; i < m_typeContext.Length; i++)
                {
                    if(i != 0) sb.Append( "," );

                    sb.Append( m_typeContext[i] );
                }

                sb.Append( "]" );
            }
            else if(m_typeContextOwner != null)
            {
                sb.Append( " => "             );
                sb.Append( m_typeContextOwner );
            }

            if(m_methodContext != null && m_methodContext.Length > 0)
            {
                sb.Append( " => Method:[" );

                for(int i = 0; i < m_methodContext.Length; i++)
                {
                    if(i != 0) sb.Append( "," );

                    sb.Append( m_methodContext[i] );
                }

                sb.Append( "]" );
            }
            else if(m_methodContextOwner != null)
            {
                sb.Append( " => "               );
                sb.Append( m_methodContextOwner );
            }
        }
    }
}
