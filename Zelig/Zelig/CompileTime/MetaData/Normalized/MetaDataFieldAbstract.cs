//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.MetaData.Normalized
{
    using System;

    public abstract class MetaDataFieldAbstract : MetaDataObject,
        IMetaDataHasConstant,
        IMetaDataHasFieldMarshal,
        IMetaDataMemberForwarded,
        IMetaDataUnique
    {
        //
        // Constructor Methods
        //

        protected MetaDataFieldAbstract( int token ) : base( token )
        {
        }

        //
        // Helper Methods
        //

        public abstract bool IsOpenField
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
