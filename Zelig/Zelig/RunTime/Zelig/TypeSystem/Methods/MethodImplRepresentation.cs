//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TypeSystem
{
    using System;
    using System.Collections.Generic;


    public sealed class MethodImplRepresentation
    {
        public static readonly MethodImplRepresentation[] SharedEmptyArray = new MethodImplRepresentation[0];

        //
        // State
        //

        private MethodRepresentation m_body;
        private MethodRepresentation m_declaration;

        //
        // Constructor Methods
        //

        public MethodImplRepresentation( MethodRepresentation body        ,
                                         MethodRepresentation declaration )
        {
            m_body        = body;
            m_declaration = declaration;
        }

        //
        // MetaDataEquality Methods
        //

        public bool EqualsThroughEquivalence( object         obj ,
                                              EquivalenceSet set )
        {
            if(obj is MethodImplRepresentation)
            {
                MethodImplRepresentation other = (MethodImplRepresentation)obj;

                if(BaseRepresentation.EqualsThroughEquivalence( m_body       , other.m_body       , set ) &&
                   BaseRepresentation.EqualsThroughEquivalence( m_declaration, other.m_declaration, set )  )
                {
                    return true;
                }
            }

            return false;
        }

        public override bool Equals( object obj )
        {
            return this.EqualsThroughEquivalence( obj, null );
        }

        public override int GetHashCode()
        {
            return m_body.GetHashCode();
        }

        //--//

        //
        // Helper Methods
        //
        public void ApplyTransformation( TransformationContext context )
        {
            context.Push( this );

            context.Transform( ref m_body        );
            context.Transform( ref m_declaration );

            context.Pop();
        }

        //--//

        internal void ProhibitUse( TypeSystem.Reachability reachability ,
                                   bool                    fApply       )
        {
            reachability.ExpandProhibition( this );
        }

        //
        // Access Methods
        //

        public MethodRepresentation Body
        {
            get
            {
                return m_body;
            }
        }

        public MethodRepresentation Declaration
        {
            get
            {
                return m_declaration;
            }
        }

        //--//

        //
        // Debug Methods
        //

        public override String ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder( "MethodImplRepresentation(" );

            sb.AppendFormat( "{0} => {1}", m_declaration.ToShortString(), m_body.ToShortString() );

            sb.Append( ")" );

            return sb.ToString();
        }
    }
}
