//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TypeSystem
{
    using System;
    using System.Collections.Generic;


    public sealed class ManagedPointerTypeRepresentation : PointerTypeRepresentation
    {
        //
        // Constructor Methods
        //

        public ManagedPointerTypeRepresentation( AssemblyRepresentation owner       ,
                                                 TypeRepresentation     pointerType ) : base( owner, BuiltInTypes.BYREF, pointerType )
        {
        }

        //--//

        //
        // Helper Methods
        //

        protected override TypeRepresentation AllocateInstantiation( InstantiationContext ic )
        {
            ManagedPointerTypeRepresentation tdRes = new ManagedPointerTypeRepresentation( m_owner, ic.Instantiate( m_pointerType ) );

            tdRes.PopulateInstantiation( this, ic );

            return tdRes;
        }

        //--//

        //
        // Access Methods
        //

        public override bool CanBeAssignedFrom( TypeRepresentation rvalue ,
                                                EquivalenceSet     set    )
        {
            if(rvalue is BoxedValueTypeRepresentation)
            {
                //
                // Going from a boxed valuetype to a byref is OK.
                //
                return m_pointerType.EqualsThroughEquivalence( rvalue.UnderlyingType, set );
            }

            return base.CanBeAssignedFrom( rvalue, set );
        }

        //--//

        //
        // Debug Methods
        //

        public override String ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder( "ManagedPointerTypeRepresentation(" );

            PrettyToString( sb, true, false );

            sb.Append( ")" );

            return sb.ToString();
        }

        internal override void PrettyToString( System.Text.StringBuilder sb                 ,
                                               bool                      fPrefix            ,
                                               bool                      fWithAbbreviations )
        {
            base.PrettyToString( sb, fPrefix, fWithAbbreviations );

            sb.Append( "&" );
        }
    }
}
