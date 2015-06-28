//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.MetaData.Normalized
{
    using System;

    //
    // Don't remove "sealed" attribute unless you read comments on Equals method.
    //
    public sealed class MetaDataMethod : MetaDataMethodBase,
        IMetaDataUnique
    {
        //
        // Constructor Methods
        //

        internal MetaDataMethod( MetaDataTypeDefinitionAbstract owner ,
                                 int                            token ) : base( owner, token )
        {
        }

        //--//

        //
        // MetaDataEquality Methods
        //

        public override bool Equals( object obj )
        {
            if(obj is MetaDataMethod) // Since the class is sealed (no subclasses allowed), there's no need to compare using .GetType()
            {
                MetaDataMethod other = (MetaDataMethod)obj;

                return InnerEquals( other );
            }

            return false;
        }

        public override int GetHashCode()
        {
            return (int)m_implFlags               ^
                   (int)m_flags                   ^
                        m_name     .GetHashCode() ^
                        m_signature.GetHashCode();
        }

        //
        // Helper Methods
        //

        public override bool IsOpenMethod
        {
            get
            {
                return false;
            }
        }

        //--//

        //
        // Debug Methods
        //

        public override string ToString()
        {
            return "MetaDataMethod(" + this.FullName + ")";
        }

        public override String ToString( IMetaDataDumper context )
        {
            return InnerToString( context, null );
        }

        public override void Dump( IMetaDataDumper writer )
        {
            InnerDump( writer, null );
        }
    }
}
