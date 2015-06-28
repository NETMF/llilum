//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TypeSystem
{
    using System;
    using System.Collections.Generic;


    public sealed class InstanceFieldRepresentation : FieldRepresentation
    {
        //
        // State
        //

        internal StaticFieldRepresentation m_implementationOf;

        //
        // Constructor Methods
        //

        public InstanceFieldRepresentation( TypeRepresentation ownerType ,
                                            string             name      ) : base( ownerType, name )
        {
        }

        public InstanceFieldRepresentation( TypeRepresentation ownerType ,
                                            string             name      ,
                                            TypeRepresentation fieldType ) : base( ownerType, name )
        {
            m_fieldType = fieldType;
        }

        //--//

        //
        // Helper Methods
        //

        public override void ApplyTransformation( TransformationContext context )
        {
            context.Push( this );

            base.ApplyTransformation( context );

            context.Transform( ref m_implementationOf );

            context.Pop();
        }

        //--//

        public void LinkAsImplementationOf( StaticFieldRepresentation fd )
        {
            fd  .m_implementedBy    = this;
            this.m_implementationOf = fd;

            this.m_flags     = fd.Flags & ~Attributes.Static;
            this.m_fixedSize = fd.FixedSize;
        }

        //--//

        //
        // Access Methods
        //

        public StaticFieldRepresentation ImplementationOf
        {
            get
            {
                return m_implementationOf;
            }
        }

        //--//

        //
        // Debug Methods
        //

        public override String ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder( "InstanceFieldRepresentation(" );

            PrettyToString( sb, true );

            if(m_implementationOf != null)
            {
                sb.AppendFormat( ", implements {0}::{1}", m_implementationOf.OwnerType.FullNameWithAbbreviation, m_implementationOf.Name );
            }

            sb.Append( ")" );

            return sb.ToString();
        }
    }
}
