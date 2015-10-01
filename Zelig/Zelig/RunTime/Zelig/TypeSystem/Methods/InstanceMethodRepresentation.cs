//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TypeSystem
{
    using System;
    using System.Collections.Generic;


    public abstract class InstanceMethodRepresentation : MethodRepresentation
    {
        //
        // Constructor Methods
        //

        protected InstanceMethodRepresentation( TypeRepresentation ownerType      ,
                                                GenericContext     genericContext ) : base( ownerType, genericContext )
        {
        }

        //--//

        //
        // Debug Methods
        //

        public override String ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder( "InstanceMethodRepresentation(" );

            PrettyToString( sb, true, true );

            sb.Append( ")" );

            return sb.ToString();
        }
    }
}
