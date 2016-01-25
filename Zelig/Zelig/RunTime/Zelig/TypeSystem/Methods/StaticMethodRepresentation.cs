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
    public class StaticMethodRepresentation : MethodRepresentation
    {
        //
        // Constructor Methods
        //

        public StaticMethodRepresentation( TypeRepresentation ownerType      ,
                                           GenericContext     genericContext ) : base( ownerType, genericContext )
        {
        }

        //--//

        //
        // Helper Methods
        //

        protected override MethodRepresentation AllocateInstantiation( InstantiationContext ic             ,
                                                                       TypeRepresentation   ownerType      ,
                                                                       GenericContext       genericContext )
        {
            return new StaticMethodRepresentation( ownerType, genericContext );
        }

        //--//

        //
        // Debug Methods
        //

        public override String ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder( "StaticMethodRepresentation(" );

            PrettyToString( sb );

            sb.Append( ")" );

            return sb.ToString();
        }
    }
}
