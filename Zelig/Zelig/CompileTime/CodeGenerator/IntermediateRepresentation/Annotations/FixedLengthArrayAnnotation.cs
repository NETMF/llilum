//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class FixedLengthArrayAnnotation : Annotation
    {
        //
        // State
        //

        private int m_length;

        //
        // Constructor Methods
        //

        private FixedLengthArrayAnnotation( int length )
        {
            m_length = length;
        }

        public static FixedLengthArrayAnnotation Create( TypeSystemForIR ts     ,
                                                         int             length )
        {
            return (FixedLengthArrayAnnotation)MakeUnique( ts, new FixedLengthArrayAnnotation( length ) );
        }

        //
        // Equality Methods
        //

        public override bool Equals( Object obj )
        {
            if(obj is FixedLengthArrayAnnotation)
            {
                FixedLengthArrayAnnotation other = (FixedLengthArrayAnnotation)obj;

                return m_length == other.m_length;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return 0x4FF536EC ^ m_length;
        }

        //
        // Helper Methods
        //

        public override Annotation Clone( CloningContext context )
        {
            return this; // Nothing to change.
        }

        //--//

        public override void ApplyTransformation( TransformationContextForIR context )
        {
            context.Push( this );
            
            base.ApplyTransformation( context );

            context.Transform( ref m_length );

            context.Pop();
        }

        //--//

        //
        // Access Methods
        //

        public int ArrayLength
        {
            get
            {
                return m_length;
            }
        }

        //--//

        //
        // Debug Methods
        //

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            return dumper.FormatOutput( "<FixedLengthArrayAnnotation: {0}>", m_length );
        }
    }
}