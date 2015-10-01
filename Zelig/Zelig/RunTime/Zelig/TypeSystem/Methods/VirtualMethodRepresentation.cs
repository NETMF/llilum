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
    public class VirtualMethodRepresentation : InstanceMethodRepresentation
    {
        //
        // Constructor Methods
        //

        public VirtualMethodRepresentation( TypeRepresentation ownerType      ,
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
            return new VirtualMethodRepresentation( ownerType, genericContext );
        }

        //--//

        internal void Override( TypeSystem                  typeSystem ,
                                VirtualMethodRepresentation mdSource   )
        {
            CloneSettings( typeSystem, mdSource );

            //--//

            if((m_ownerType.Flags & TypeRepresentation.Attributes.Sealed) != 0)
            {
                m_flags |= Attributes.Final;
            }

            this.VTableLayoutFlags = Attributes.ReuseSlot;

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
            System.Text.StringBuilder sb = new System.Text.StringBuilder( "VirtualMethodRepresentation(" );

            PrettyToString( sb, true, true );

            sb.Append( ")" );

            return sb.ToString();
        }
    }
}
