//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TypeSystem
{
    using System;
    using System.Collections.Generic;


    public class AbstractReferenceTypeRepresentation : ReferenceTypeRepresentation
    {
        //
        // Constructor Methods
        //

        public AbstractReferenceTypeRepresentation( AssemblyRepresentation owner          ,
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
            AbstractReferenceTypeRepresentation tdRes = new AbstractReferenceTypeRepresentation( m_owner, m_builtinType, m_flags, new GenericContext( this, ic.TypeParameters ) );

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
            System.Text.StringBuilder sb = new System.Text.StringBuilder( "AbstractReferenceTypeRepresentation(" );

            PrettyToString( sb, true, false );

            sb.Append( ")" );

            return sb.ToString();
        }
    }
}
