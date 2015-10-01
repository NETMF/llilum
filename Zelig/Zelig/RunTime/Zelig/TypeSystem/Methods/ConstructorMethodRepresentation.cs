//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//
//#define TRANSFORMATIONCONTEXT__USE_EMIT

namespace Microsoft.Zelig.Runtime.TypeSystem
{
    using System;
    using System.Collections.Generic;

#if TRANSFORMATIONCONTEXT__USE_EMIT
    [AllowCompileTimeIntrospection]
#endif
    public sealed class ConstructorMethodRepresentation : InstanceMethodRepresentation
    {
        //
        // Constructor Methods
        //

        public ConstructorMethodRepresentation( TypeRepresentation ownerType      ,
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
            return new ConstructorMethodRepresentation( ownerType, genericContext );
        }

        //--//

        public void Override( TypeSystem                      typeSystem ,
                              ConstructorMethodRepresentation mdSource   )
        {
            CloneSettings( typeSystem, mdSource );

            //--//

            m_flags &= ~Attributes.Abstract;
            m_flags &= ~Attributes.SpecialName;

            m_flags &= ~Attributes.PinvokeImpl;
            m_flags &= ~Attributes.UnmanagedExport;
        }

        //--//

        //
        // Debug Methods
        //

        public override String ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder( "ConstructorMethodRepresentation(" );

            PrettyToString( sb, true, true );

            sb.Append( ")" );

            return sb.ToString();
        }
    }
}
