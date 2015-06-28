//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TypeSystem
{
    using System;
    using System.Collections.Generic;


    public sealed class UnmanagedPointerTypeRepresentation : PointerTypeRepresentation
    {
        //
        // Constructor Methods
        //

        public UnmanagedPointerTypeRepresentation( AssemblyRepresentation owner       ,
                                                   TypeRepresentation     pointerType ) : base( owner, BuiltInTypes.PTR, pointerType )
        {
        }

        //--//

        //
        // Helper Methods
        //

        protected override TypeRepresentation AllocateInstantiation( InstantiationContext ic )
        {
            UnmanagedPointerTypeRepresentation tdRes = new UnmanagedPointerTypeRepresentation( m_owner, ic.Instantiate( m_pointerType ) );

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
            if(this.EqualsThroughEquivalence( rvalue, set ))
            {
                return true;
            }

            StackEquivalentType seRValue = rvalue.StackEquivalentType;

            if(seRValue == StackEquivalentType.Pointer   ||
               seRValue == StackEquivalentType.NativeInt  )
            {
                //
                // Any pointer can be cast to an unmanaged pointer.
                //
                return true;
            }

            return false;
        }

        //--//

        //
        // Debug Methods
        //

        public override String ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder( "UnmanagedPointerTypeRepresentation(" );

            PrettyToString( sb, true, false );

            sb.Append( ")" );

            return sb.ToString();
        }

        internal override void PrettyToString( System.Text.StringBuilder sb                 ,
                                               bool                      fPrefix            ,
                                               bool                      fWithAbbreviations )
        {
            base.PrettyToString( sb, fPrefix, fWithAbbreviations );

            sb.Append( "*" );
        }
    }
}
