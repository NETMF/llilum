//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.Transformations
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    internal sealed class ReverseIndexTypeSystem : ScanTypeSystem
    {
        //
        // State
        //

        GrowOnlyHashTable< object, GrowOnlySet< object > > m_lookup;
        bool                                               m_fPendingRefreshHashCodes;

        //
        // Constructor Methods
        //

        internal ReverseIndexTypeSystem( TypeSystemForCodeTransformation typeSystem ) : base( typeSystem, typeof(ReverseIndexTypeSystem) )
        {
            m_lookup = HashTableFactory.NewWithWeakEquality< object, GrowOnlySet< object > >();
        }

        //--//

        //
        // Helper Methods
        //

        internal override void RefreshHashCodes()
        {
            //
            // The ReverseIndex is extensively used during Class Extension, and thus this method is called many times.
            // Recomputing the hash codes for millions of objects is too expensive, so we keep track that a refresh is needed.
            // When we hit Update later, we refresh the hash codes of the base class before performing the scan.
            //
            m_fPendingRefreshHashCodes = true;

            m_lookup.RefreshHashCodes();
        }

        internal override void ProcessTypeSystem()
        {
            m_lookup.Clear();

            base.ProcessTypeSystem();
        }

        internal void Update( object obj )
        {
            if(m_fPendingRefreshHashCodes)
            {
                m_fPendingRefreshHashCodes = false;

                base.RefreshHashCodes();
            }

            this.TransformGenericReference( obj );
        }

        internal int NumberOfRemapRecords
        {
            get
            {
                return m_lookup.Count;
            }
        }

        internal GrowOnlySet< object > this[ object key ]
        {
            get
            {
                GrowOnlySet< object > val;

                if(m_lookup.TryGetValue( key, out val ))
                {
                    return val;
                }

                return null;
            }
        }

        internal void Merge( object from ,
                             object to   )
        {
            this.RefreshHashCodes();

            GrowOnlySet< object > setFrom = m_lookup[from];
            GrowOnlySet< object > setTo   = m_lookup[to];

            if(setFrom != null)
            {
                if(setTo != null)
                {
                    setTo.RefreshHashCodes();

                    setTo.Merge( setFrom );
                }
                else
                {
                    setFrom.RefreshHashCodes();

                    m_lookup[to] = setFrom;
                }

                m_lookup[from] = null;
            }
        }

        //--//

        protected override object ShouldSubstitute(     object             target ,
                                                    out SubstitutionAction result )
        {
            object context = this.TopContext();

            if(context != null)
            {
                if(CanBeTracked( target ))
                {
                    GrowOnlySet< object > set;

                    if(m_lookup.TryGetValue( target, out set ) == false || set == null)
                    {
                        set = SetFactory.NewWithReferenceEquality< object >();

                        m_lookup[target] = set;
                    }

                    set.Insert( context );
                }
            }

            result = SubstitutionAction.Unknown;
            
            return null;
        }

        //--//

        internal bool CanBeTracked( object obj )
        {
            if(obj is BaseRepresentation                       ||
               obj is MethodImplRepresentation                 || 
               obj is CustomAttributeAssociationRepresentation ||
               obj is CustomAttributeRepresentation            ||
               obj is Annotation                               ||
               obj is VTable                                    )
            {
                return true;
            }

            return false;
        }


        //
        // Access Methods
        //

        internal GrowOnlyHashTable< object, GrowOnlySet< object > > ReverseIndexTable
        {
            get
            {
                return m_lookup;
            }
        }
    }
}
