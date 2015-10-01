//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TypeSystem
{
    using System;
    using System.Collections.Generic;


    public delegate void CustomAttributeAssociationEnumerationCallback( CustomAttributeAssociationRepresentation caa );

    public sealed class CustomAttributeAssociationRepresentation
    {
        public static readonly CustomAttributeAssociationRepresentation[] SharedEmptyArray = new CustomAttributeAssociationRepresentation[0];

        //
        // State
        //

        private CustomAttributeRepresentation m_ca;
        private BaseRepresentation            m_target;
        private int                           m_paramIndex;

        //
        // Constructor Methods
        //

        public CustomAttributeAssociationRepresentation( CustomAttributeRepresentation ca         ,
                                                         BaseRepresentation            target     ,
                                                         int                           paramIndex )
        {
            m_ca         = ca;
            m_target     = target;
            m_paramIndex = paramIndex;
        }

        //
        // MetaDataEquality Methods
        //

        public override bool Equals( object obj )
        {
            if(obj is CustomAttributeAssociationRepresentation)
            {
                CustomAttributeAssociationRepresentation other = (CustomAttributeAssociationRepresentation)obj;

                if(m_ca         == other.m_ca         &&
                   m_target     == other.m_target     &&
                   m_paramIndex == other.m_paramIndex  )
                {
                    return true;
                }
            }

            return false;
        }

        public override int GetHashCode()
        {
            return m_ca.GetHashCode();
        }

        //--//

        //
        // Helper Methods
        //

        public void ApplyTransformation( TransformationContext context )
        {
            context.Push( this );

            context.Transform( ref m_ca         );
            context.Transform( ref m_target     );
            context.Transform( ref m_paramIndex );

            context.Pop();
        }

        //--//

        //
        // Access Methods
        //

        public CustomAttributeRepresentation CustomAttribute
        {
            get
            {
                return m_ca;
            }
        }

        public BaseRepresentation Target
        {
            get
            {
                return m_target;
            }
        }

        public int ParameterIndex
        {
            get
            {
                return m_paramIndex;
            }
        }
    }
}
