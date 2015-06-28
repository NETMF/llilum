//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public abstract class InvalidationAnnotation : Annotation
    {
        //
        // State
        //

        private VariableExpression m_target;

        //
        // Constructor Methods
        //

        protected InvalidationAnnotation( VariableExpression target )
        {
            m_target = target;
        }

        //
        // Equality Methods
        //

        public override bool Equals( Object obj )
        {
            var other = obj as InvalidationAnnotation;
            if(other != null)
            {
                return m_target == other.m_target;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return m_target.Type.GetHashCode();
        }

        //
        // Helper Methods
        //

        public override void ApplyTransformation( TransformationContextForIR context )
        {
            context.Push( this );
            
            base.ApplyTransformation( context );

            context.Transform( ref m_target );

            context.Pop();
        }

        //--//

        //
        // Access Methods
        //

        public VariableExpression Target
        {
            get
            {
                return m_target;
            }
        }

        //--//

        //
        // Debug Methods
        //

    }
}