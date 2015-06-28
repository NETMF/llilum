//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.MetaData.Normalized
{
    using System;

    //
    // Don't remove "sealed" attribute unless you read comments on Equals method.
    //
    public sealed class MetaDataTypeDefinitionArrayMulti : MetaDataTypeDefinitionArray,
        IMetaDataUnique
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

        internal uint        m_rank;
        internal Dimension[] m_dimensions;

        //
        // Constructor Methods
        //

        internal MetaDataTypeDefinitionArrayMulti( MetaDataAssembly owner ,
                                                   int              token ) : base( owner, token )
        {
            m_elementType = ElementTypes.ARRAY;
        }

        //
        // MetaDataEquality Methods
        //

        public override bool Equals( object obj )
        {
            if(obj is MetaDataTypeDefinitionArrayMulti) // Since the class is sealed (no subclasses allowed), there's no need to compare using .GetType()
            {
                MetaDataTypeDefinitionArrayMulti other = (MetaDataTypeDefinitionArrayMulti)obj;

                if(m_rank == other.m_rank)
                {
                    if(ArrayUtility.ArrayEquals( m_dimensions, other.m_dimensions ))
                    {
                        return InnerEquals( other );
                    }
                }
            }

            return false;
        }

        public override int GetHashCode()
        {
            return InnerGetHashCode();
        }

        //
        // Helper Methods
        //

        public override bool Match( MetaDataTypeDefinitionAbstract typeContext   ,
                                    MetaDataMethodAbstract         methodContext ,
                                    MetaDataTypeDefinitionAbstract type          )
        {
            throw new NotNormalized( "Match" );
        }

        //
        // Access Methods
        //

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

        public override string FullName
        {
            get
            {
                return AppendQualifiers( m_objectType.FullName );
            }
        }

        public override string FullNameWithAbbreviation
        {
            get
            {
                return AppendQualifiers( m_objectType.FullNameWithAbbreviation );
            }
        }

        public override string ToString()
        {
            return QualifiedToString( "MetaDataTypeDefinitionArrayMulti" );
        }

        public override String ToString( IMetaDataDumper context )
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.Append( m_objectType.ToStringWithAbbreviations( context ) );

            sb.Append( "[" );
            for(int i = 0; i < m_rank; i++)
            {
                if(i != 0)
                {
                    sb.Append( "," );
                }

                if(i < m_dimensions.Length)
                {
                    Dimension dm = m_dimensions[i];

                    if(dm.m_lowerBound != 0 || dm.m_upperBound != 0)
                    {
                        sb.Append( dm.m_lowerBound );
                        sb.Append( ".." );
                        sb.Append( dm.m_upperBound );
                    }
                }
            }

            sb.Append( "]" );

            return sb.ToString();
        }

        public override void Dump( IMetaDataDumper writer )
        {
            throw new NotNormalized( "Dump" );
        }

        //--//

        private string AppendQualifiers( string prefix )
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder( prefix );

            sb.Append( "[" );
            for(int i = 0; i < m_rank; i++)
            {
                if(i != 0)
                {
                    sb.Append( "," );
                }

                if(i < m_dimensions.Length)
                {
                    Dimension dm = m_dimensions[i];

                    if(dm.m_lowerBound != 0 || dm.m_upperBound != 0)
                    {
                        sb.Append( dm.m_lowerBound );
                        sb.Append( ".." );
                        sb.Append( dm.m_upperBound );
                    }
                }
            }

            sb.Append( "]" );

            return sb.ToString();
        }
    }
}
