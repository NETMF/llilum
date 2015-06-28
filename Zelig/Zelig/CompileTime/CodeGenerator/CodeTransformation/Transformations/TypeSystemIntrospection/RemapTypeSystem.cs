//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.Transformations
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    internal sealed class RemapTypeSystem : ScanTypeSystem
    {
        //
        // State
        //

        GrowOnlyHashTable< object, object > m_remap;

        //
        // Constructor Methods
        //

        internal RemapTypeSystem( TypeSystemForCodeTransformation typeSystem ) : base( typeSystem, typeof(RemapTypeSystem) )
        {
            m_remap = HashTableFactory.NewWithReferenceEquality< object, object >();
        }

        internal override void RefreshHashCodes()
        {
            base.RefreshHashCodes();

            m_remap.RefreshHashCodes();
        }

        internal int NumberOfRemapRecords
        {
            get
            {
                return m_remap.Count;
            }
        }

        internal object this[ object key ]
        {
            get
            {
                if(key != null)
                {
                    object substitute;

                    if(m_remap.TryGetValue( key, out substitute ))
                    {
                        key = substitute;
                    }
                }

                return key;
            }

            set
            {
                m_remap[key] = value;
            }
        }

        //--//

        protected override object ShouldSubstitute(     object             target ,
                                                    out SubstitutionAction result )
        {
            object substitute;

            if(m_remap.TryGetValue( target, out substitute ))
            {
                if(substitute == null)
                {
                    throw TypeConsistencyErrorException.Create( "Found unexpected reference to {0}", target );
                }

                result = SubstitutionAction.Substitute;

                return substitute;
            }
            else
            {
                result = SubstitutionAction.Unknown;
                
                return null;
            }
        }

        //--//

        //
        // Access Methods
        //

        internal GrowOnlyHashTable< object, object > RemapTable
        {
            get
            {
                return m_remap;
            }
        }
    }
}
