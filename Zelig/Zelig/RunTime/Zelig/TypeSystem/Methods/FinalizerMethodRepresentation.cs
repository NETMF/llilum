//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//
// #define TRANSFORMATIONCONTEXT__USE_EMIT

namespace Microsoft.Zelig.Runtime.TypeSystem
{
    using System;
    using System.Collections.Generic;


#if TRANSFORMATIONCONTEXT__USE_EMIT
    [AllowCompileTimeIntrospection]
#endif
    public sealed class FinalizerMethodRepresentation : VirtualMethodRepresentation
    {
        //
        // Constructor Methods
        //

        public FinalizerMethodRepresentation( TypeRepresentation ownerType      ,
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
            return new FinalizerMethodRepresentation( ownerType, genericContext );
        }

        //--//

        //
        // Debug Methods
        //

        public override String ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder( "FinalizerMethodRepresentation(" );

            PrettyToString( sb, true, true );

            sb.Append( ")" );

            return sb.ToString();
        }
    }
}
