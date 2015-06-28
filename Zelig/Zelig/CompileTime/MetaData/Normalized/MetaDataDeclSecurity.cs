//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.MetaData.Normalized
{
    using System;

    //
    // Don't remove "sealed" attribute unless you read comments on Equals method.
    //
    public sealed class MetaDataDeclSecurity : MetaDataObject,
        IMetaDataUnique
    {
        //
        // State
        //

        internal IMetaDataHasDeclSecurity m_owner;
        internal short                    m_action;
        internal byte[]                   m_permissionSet;

        //
        // Constructor Methods
        //

        internal MetaDataDeclSecurity( int token ) : base( token )
        {
        }

        //
        // MetaDataEquality Methods
        //

        public override bool Equals( object obj )
        {
            if(obj is MetaDataDeclSecurity) // Since the class is sealed (no subclasses allowed), there's no need to compare using .GetType()
            {
                MetaDataDeclSecurity other = (MetaDataDeclSecurity)obj;

                throw new NotNormalized( "MetaDataEquality" );
            }

            return false;
        }

        public override int GetHashCode()
        {
            throw new NotNormalized( "MetaDataEquality" );
        }

        //
        // Helper Methods
        //

        internal override MetaDataObject MakeUnique()
        {
            throw new NotNormalized( "MakeUnique" );
        }

        //
        // Access Methods
        //

        public short Action
        {
            get
            {
                return m_action;
            }
        }

        public IMetaDataHasDeclSecurity Owner
        {
            get
            {
                return m_owner;
            }
        }

        public byte[] PermissionSet
        {
            get
            {
                return m_permissionSet;
            }
        }

        // Debug Methods

        public override String ToString()
        {
            return "MetaDataDeclSecurity(" + m_owner + "," + m_action + "," + m_permissionSet + ")";
        }

        public override void Dump( IMetaDataDumper writer )
        {
            throw new NotNormalized( "Dump" );
        }
    }
}
