//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class MemoryMappedPeripheralAnnotation : Annotation
    {
        //
        // State
        //

        private CustomAttributeRepresentation m_ca;

        //
        // Constructor Methods
        //

        private MemoryMappedPeripheralAnnotation( CustomAttributeRepresentation ca )
        {
            m_ca = ca;
        }

        public static MemoryMappedPeripheralAnnotation Create( TypeSystemForIR               ts ,
                                                               CustomAttributeRepresentation ca )
        {
            return (MemoryMappedPeripheralAnnotation)MakeUnique( ts, new MemoryMappedPeripheralAnnotation( ca ) );
        }

        //
        // Equality Methods
        //

        public override bool Equals( Object obj )
        {
            if(obj is MemoryMappedPeripheralAnnotation)
            {
                MemoryMappedPeripheralAnnotation other = (MemoryMappedPeripheralAnnotation)obj;

                return m_ca == other.m_ca;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return m_ca.GetHashCode();
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

            context.Transform( ref m_ca );

            context.Pop();
        }

        //--//

        //
        // Access Methods
        //

        public CustomAttributeRepresentation Target
        {
            get
            {
                return m_ca;
            }
        }

        //--//

        //
        // Debug Methods
        //

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            return dumper.FormatOutput( "<MemoryMappedPeripheral: {0}>", m_ca );
        }
    }
}