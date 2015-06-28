//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.Transformations
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    internal sealed class LocateFieldsInCode
    {
        //
        // State
        //

        private TypeSystemForCodeTransformation                                   m_typeSystem;
        private Transformations.ReverseIndexTypeSystem                            m_reverseIndex;
        private GrowOnlyHashTable< FieldRepresentation, GrowOnlySet< Operator > > m_fieldReferences;

        //
        // Constructor Methods
        //

        internal LocateFieldsInCode( TypeSystemForCodeTransformation typeSystem )
        {
            m_typeSystem = typeSystem;
        }

        //
        // Helper Methods
        //

        internal void Run()
        {
            m_reverseIndex    = new Transformations.ReverseIndexTypeSystem( m_typeSystem );
            m_fieldReferences = HashTableFactory.NewWithReferenceEquality< FieldRepresentation, GrowOnlySet< Operator > >();

            using(new Transformations.ExecutionTiming( "ReverseIndexTypeSystem" ))
            {
                m_reverseIndex.ProcessTypeSystem();
            }

            m_typeSystem.EnumerateFields( delegate( FieldRepresentation fd )
            {
                GrowOnlySet< object > set = m_reverseIndex[fd];

                if(set != null)
                {
                    foreach(object obj in set)
                    {
                        Operator op = obj as Operator;

                        if(op != null)
                        {
                            HashTableWithSetFactory.AddWithReferenceEquality( m_fieldReferences, fd, op );
                        }
                    }
                }
            } );
        }

        //--//

        //
        // Access Methods
        //

        internal GrowOnlyHashTable< FieldRepresentation, GrowOnlySet< Operator > > FieldReferences
        {
            get
            {
                return m_fieldReferences;
            }
        }
    }
}
