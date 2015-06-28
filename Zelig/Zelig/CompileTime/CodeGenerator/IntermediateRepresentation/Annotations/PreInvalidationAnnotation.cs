//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class PreInvalidationAnnotation : InvalidationAnnotation
    {
        //
        // State
        //

        //
        // Constructor Methods
        //

        private PreInvalidationAnnotation( VariableExpression target ) : base( target )
        {
        }

        public static PreInvalidationAnnotation Create( TypeSystemForIR    ts     ,
                                                        VariableExpression target )
        {
            return (PreInvalidationAnnotation)MakeUnique( ts, new PreInvalidationAnnotation( target ) );
        }

        //
        // Equality Methods
        //

        public override bool Equals( Object obj )
        {
            var other = obj as PreInvalidationAnnotation;
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

            return RegisterAndCloneState( context, MakeUnique( context.TypeSystem, new PreInvalidationAnnotation( target ) ) );
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
            return dumper.FormatOutput( "<PreInvalidation: {0}>", this.Target );
        }
    }
}