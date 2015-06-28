//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.MetaData.Normalized
{
    using System;

    //
    // Don't remove "sealed" attribute unless you read comments on Equals method.
    //
    public sealed class MetaDataMethodGeneric : MetaDataMethodBase,
        IMetaDataUnique
    {
        //
        // State
        //

        internal MetaDataGenericMethodParam[] m_genericParams;

        //
        // Constructor Methods
        //

        internal MetaDataMethodGeneric( MetaDataTypeDefinitionAbstract owner ,
                                        int                            token ) : base( owner, token )
        {
        }

        //--//

        //
        // MetaDataEquality Methods
        //

        public override bool Equals( object obj )
        {
            if(obj is MetaDataMethodGeneric) // Since the class is sealed (no subclasses allowed), there's no need to compare using .GetType()
            {
                MetaDataMethodGeneric other = (MetaDataMethodGeneric)obj;

                if(ArrayUtility.ArrayEquals( m_genericParams, other.m_genericParams ))
                {
                    return InnerEquals( other );
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

        public override bool IsOpenMethod
        {
            get
            {
                return true;
            }
        }

        public override bool UsesTypeParameters
        {
            get
            {
                return base.UsesTypeParameters;
            }
        }

        public override bool UsesMethodParameters
        {
            get
            {
                return true;
            }
        }

        //--//

        //
        // Access Methods
        //

        public MetaDataGenericMethodParam[] GenericParams
        {
            get
            {
                return m_genericParams;
            }
        }

        //
        // Debug Methods
        //

        public override string ToString()
        {
            return "MetaDataMethodGeneric(" + this.FullName + ")";
        }

        public override String ToString( IMetaDataDumper context )
        {
            return InnerToString( context, m_genericParams );
        }

        public override void Dump( IMetaDataDumper writer )
        {
            InnerDump( writer, m_genericParams );
        }
    }
}
