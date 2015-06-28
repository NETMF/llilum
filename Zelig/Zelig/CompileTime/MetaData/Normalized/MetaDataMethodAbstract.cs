//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.MetaData.Normalized
{
    using System;

    public abstract class MetaDataMethodAbstract : MetaDataObject,
        IMetaDataHasDeclSecurity,
        IMetaDataMethodDefOrRef,
        IMetaDataMemberForwarded,
        IMetaDataCustomAttributeType,
        IMetaDataTypeOrMethodDef
    {
        //
        // Constructor Methods
        //

        protected MetaDataMethodAbstract( int token ) : base( token )
        {
        }

        //--//

        //
        // Helper Methods
        //

        public abstract bool IsOpenMethod
        {
            get;
        }

        //
        // Debug Methods
        //

        public abstract string FullName
        {
            get;
        }

        public abstract String ToString( IMetaDataDumper context );
    }
}
