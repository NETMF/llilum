//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.Runtime.TypeSystem
{
    using System;
    using System.Collections.Generic;


    public class EquivalenceSet
    {
        public delegate bool EquivalenceDelegation( BaseRepresentation context1, BaseRepresentation br1, BaseRepresentation context2, BaseRepresentation br2 );

        //
        // State
        //

        EquivalenceDelegation             m_dlg;
        GrowOnlySet< BaseRepresentation > m_equivalence;

        //
        // Constructor Methods
        //

        public EquivalenceSet( EquivalenceDelegation dlg )
        {
            m_dlg         = dlg;
            m_equivalence = SetFactory.NewWithReferenceEquality< BaseRepresentation >();
        }

        //
        // Helper Methods
        //

        public void AddEquivalencePair( BaseRepresentation br1 ,
                                        BaseRepresentation br2 )
        {
            m_equivalence.Insert( br1 );
            m_equivalence.Insert( br2 );
        }

        public bool AreEquivalent( BaseRepresentation context1 ,
                                   BaseRepresentation br1      ,
                                   BaseRepresentation context2 ,
                                   BaseRepresentation br2      )
        {
            if(m_equivalence.Contains( br1 ) &&
               m_equivalence.Contains( br2 )  )
            {
                return true;
            }

            if(m_dlg != null)
            {
                return m_dlg( context1, br1, context2, br2 );
            }

            return false;
        }
    }
}
