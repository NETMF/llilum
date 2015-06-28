//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.Transformations
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    internal abstract class ScanCode : ScanTypeSystem
    {
        //
        // State
        //

        protected GrowOnlyHashTable< object, List< Operator > > m_entries;
        bool                                                    m_fStop;

        //
        // Constructor Methods
        //

        protected ScanCode( TypeSystemForCodeTransformation typeSystem     ,
                            object                          scanOriginator ) : base( typeSystem, scanOriginator )
        {
            m_entries = HashTableFactory.NewWithReferenceEquality< object, List< Operator > >();
        }

        //
        // Helper Methods
        //

        internal override void RefreshHashCodes()
        {
            base.RefreshHashCodes();

            m_entries.RefreshHashCodes();
        }

        //--//

        internal bool ProcessType( TypeRepresentation td )
        {
            foreach(MethodRepresentation md in td.Methods)
            {
                if(!ProcessMethod( md )) return false;
            }

            return true;
        }

        internal bool ProcessMethod( MethodRepresentation md )
        {
            return ProcessMethod( TypeSystemForCodeTransformation.GetCodeForMethod( md ) );
        }

        internal bool ProcessMethod( ControlFlowGraphState cfg )
        {
            Transform( ref cfg );

            return m_fStop == false;
        }

        //--//

        protected List< Operator > CreateEntry( object key )
        {
            return HashTableWithListFactory.Create( m_entries, key );
        }

        protected void RecordOccurence( List< Operator > lst ,
                                        Operator         op  )
        {
            if(lst.IndexOf( op ) < 0)
            {
                lst.Add( op );
            }
        }

        protected abstract bool PerformAction( Operator op     ,
                                               object   target );

        protected override object ShouldSubstitute(     object             target ,
                                                    out SubstitutionAction result )
        {
            if(m_fStop)
            {
                result = SubstitutionAction.Keep;
                return null;
            }

            Operator op = (Operator)FindContext( typeof(Operator) );
            if(!PerformAction( op, target ))
            {
                m_fStop = true;

                result = SubstitutionAction.Keep;
                return null;
            }

            if(target is TypeSystem                               ||
               target is CustomAttributeAssociationRepresentation ||
               target is CustomAttributeRepresentation            ||
               target is BaseRepresentation                       ||
               target is DataManager.DataDescriptor                )
            {
                //
                // Don't follow type system entities or data, we just want to stay within the code.
                //
                result = SubstitutionAction.Keep;
            }
            else
            {
                result = SubstitutionAction.Unknown;
            }

            return null;
        }

        //
        // Access Methods
        //

        internal List< Operator > this[object index]
        {
            get
            {
                if(index != null)
                {
                    List< Operator > lst;

                    if(m_entries.TryGetValue( index, out lst ))
                    {
                        return lst;
                    }
                }
                
                return null;
            }
        }

        internal GrowOnlyHashTable< object, List< Operator > > Entries
        {
            get
            {
                return m_entries;
            }
        }
    }
}
