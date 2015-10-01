//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TypeSystem
{
    using System;
    using System.Collections.Generic;


#if TRANSFORMATIONCONTEXT__USE_EMIT
    [AllowCompileTimeIntrospection]
#endif
    public sealed class StaticConstructorMethodRepresentation : StaticMethodRepresentation
    {
        //
        // Constructor Methods
        //

        public StaticConstructorMethodRepresentation( TypeRepresentation ownerType      ,
                                                      GenericContext     genericContext ) : base( ownerType, genericContext )
        {
        }

        //--//

        //
        // Helper Methods
        //

        //--//

        //
        // Debug Methods
        //

        public override String ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder( "StaticConstructorMethodRepresentation(" );

            PrettyToString( sb, true, true );

            sb.Append( ")" );

            return sb.ToString();
        }
    }
}
