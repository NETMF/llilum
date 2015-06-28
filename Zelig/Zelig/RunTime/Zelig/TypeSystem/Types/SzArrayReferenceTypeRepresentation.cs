//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TypeSystem
{
    using System;
    using System.Collections.Generic;


    public sealed class SzArrayReferenceTypeRepresentation : ArrayReferenceTypeRepresentation
    {
        //
        // Constructor Methods
        //

        public SzArrayReferenceTypeRepresentation( AssemblyRepresentation owner       ,
                                                   TypeRepresentation     elementType ) : base( owner, BuiltInTypes.SZARRAY, Attributes.None, elementType )
        {
        }

        //--//

        //
        // Helper Methods
        //

        protected override void SetShapeCategory( TypeSystem typeSystem )
        {
            m_vTable.ShapeCategory = VTable.Shape.SzArray;
        }

        //--//

        protected override TypeRepresentation AllocateInstantiation( InstantiationContext ic )
        {
            SzArrayReferenceTypeRepresentation tdRes = new SzArrayReferenceTypeRepresentation( m_owner, ic.Instantiate( m_elementType ) );

            tdRes.PopulateInstantiation( this, ic );

            return tdRes;
        }

        //--//

        public override bool SameShape( ArrayReferenceTypeRepresentation other )
        {
            return other is SzArrayReferenceTypeRepresentation;
        }

        //--//

        //
        // Access Methods
        //

        //--//

        //
        // Debug Methods
        //

        public override String ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder( "SzArrayReferenceTypeRepresentation(" );

            PrettyToString( sb, true, false );

            sb.Append( ")" );

            return sb.ToString();
        }

        internal override void PrettyToString( System.Text.StringBuilder sb                 ,
                                               bool                      fPrefix            ,
                                               bool                      fWithAbbreviations )
        {
            m_elementType.PrettyToString( sb, fPrefix, fWithAbbreviations );

            sb.Append( "[]" );
        }
    }
}
