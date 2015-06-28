//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class PostInvalidationAnnotation : InvalidationAnnotation
    {
        //
        // State
        //

        //
        // Constructor Methods
        //

        private PostInvalidationAnnotation( VariableExpression target ) : base( target )
        {
        }

        public static PostInvalidationAnnotation Create( TypeSystemForIR    ts     ,
                                                         VariableExpression target )
        {
            return (PostInvalidationAnnotation)MakeUnique( ts, new PostInvalidationAnnotation( target ) );
        }

        //
        // Equality Methods
        //

        public override bool Equals( Object obj )
        {
            var other = obj as PostInvalidationAnnotation;
            if(other != null)
            {
                return base.Equals( obj );
            }

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        //
        // Helper Methods
        //

        public override Annotation Clone( CloningContext context )
        {
            var target = (VariableExpression)context.Clone( this.Target );

            return RegisterAndCloneState( context, MakeUnique( context.TypeSystem, new PostInvalidationAnnotation( target ) ) );
        }

        //--//

        //
        // Access Methods
        //

        //--//

        //
        // Debug Methods
        //

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            return dumper.FormatOutput( "<PostInvalidation: {0}>", this.Target );
        }
    }
}