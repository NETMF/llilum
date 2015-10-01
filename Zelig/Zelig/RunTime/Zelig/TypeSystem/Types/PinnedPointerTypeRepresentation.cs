//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TypeSystem
{
    using System;
    using System.Collections.Generic;


    public sealed class PinnedPointerTypeRepresentation : PointerTypeRepresentation
    {
        //
        // Constructor Methods
        //

        public PinnedPointerTypeRepresentation( AssemblyRepresentation owner       ,
                                                TypeRepresentation     pointerType ) : base( owner, BuiltInTypes.PINNED, pointerType )
        {
        }

        //--//

        //
        // Helper Methods
        //

        protected override TypeRepresentation AllocateInstantiation( InstantiationContext ic )
        {
            PinnedPointerTypeRepresentation tdRes = new PinnedPointerTypeRepresentation( m_owner, ic.Instantiate( m_pointerType ) );

            tdRes.PopulateInstantiation( this, ic );

            return tdRes;
        }

        //--//

        //
        // Debug Methods
        //

        public override String ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder( "PinnedPointerTypeRepresentation(" );

            PrettyToString( sb, true, false );

            sb.Append( ")" );

            return sb.ToString();
        }
    }
}
