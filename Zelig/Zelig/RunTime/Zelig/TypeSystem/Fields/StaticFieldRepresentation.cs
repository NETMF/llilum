//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TypeSystem
{
    using System;
    using System.Collections.Generic;


    public sealed class StaticFieldRepresentation : FieldRepresentation
    {
        //
        // State
        //

        internal InstanceFieldRepresentation m_implementedBy;

        //
        // Constructor Methods
        //

        public StaticFieldRepresentation( TypeRepresentation ownerType ,
                                          string             name      ) : base( ownerType, name )
        {
        }

        //--//

        //
        // Helper Methods
        //

        public override void ApplyTransformation( TransformationContext context )
        {
            context.Push( this );

            base.ApplyTransformation( context );

            context.Transform( ref m_implementedBy );

            context.Pop();
        }

        //
        // Access Methods
        //

        public InstanceFieldRepresentation ImplementedBy
        {
            get
            {
                return m_implementedBy;
            }
        }

        //--//

        //
        // Debug Methods
        //

        public override String ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder( "StaticFieldRepresentation(" );

            PrettyToString( sb, true );

            if(m_implementedBy != null)
            {
                sb.AppendFormat( ", implemented by {0}::{1}", m_implementedBy.OwnerType.FullNameWithAbbreviation, m_implementedBy.Name );
            }

            sb.Append( ")" );

            return sb.ToString();
        }
    }
}
