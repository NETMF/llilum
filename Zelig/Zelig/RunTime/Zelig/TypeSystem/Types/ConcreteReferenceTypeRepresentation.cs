//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TypeSystem
{
    using System;
    using System.Collections.Generic;


    public sealed class ConcreteReferenceTypeRepresentation : ReferenceTypeRepresentation
    {
        //
        // Constructor Methods
        //

        public ConcreteReferenceTypeRepresentation( AssemblyRepresentation owner          ,
                                                    BuiltInTypes           builtinType    ,
                                                    Attributes             flags          ,
                                                    GenericContext         genericContext ) : base( owner, builtinType, flags, genericContext )
        {
        }

        //--//

        //
        // Helper Methods
        //

        protected override TypeRepresentation AllocateInstantiation( InstantiationContext ic )
        {
            ConcreteReferenceTypeRepresentation tdRes = new ConcreteReferenceTypeRepresentation( m_owner, m_builtinType, m_flags, new GenericContext( this, ic.TypeParameters ) );

            tdRes.PopulateInstantiation( this, ic );

            return tdRes;
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
            System.Text.StringBuilder sb = new System.Text.StringBuilder( "ConcreteReferenceTypeRepresentation(" );

            PrettyToString( sb, true, false );

            sb.Append( ")" );

            return sb.ToString();
        }
    }
}
