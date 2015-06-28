//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


#if DEBUG
#define TRACK_ANNOTATION_IDENTITY
#else
//#define TRACK_ANNOTATION_IDENTITY
#endif

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public abstract class Annotation
    {
        public static readonly Annotation[] SharedEmptyArray = new Annotation[0];

        //
        // State
        //

#if TRACK_ANNOTATION_IDENTITY
        protected static int s_identity;
#endif
        public    int        m_identity;

        //--//

        //
        // Constructor Methods
        //

        protected Annotation() // Default constructor required by TypeSystemSerializer.
        {
#if TRACK_ANNOTATION_IDENTITY
            m_identity = s_identity++;
#endif
        }

        //--//

        //
        // Helper Methods
        //

        public abstract Annotation Clone( CloningContext context );

        protected Annotation RegisterAndCloneState( CloningContext context ,
                                                    Annotation     clone   )
        {
            context.Register( this, clone );

            CloneState( context, clone );

            return clone;
        }

        protected virtual void CloneState( CloningContext context ,
                                           Annotation     clone   )
        {
        }

        //--//

        public virtual void ApplyTransformation( TransformationContextForIR context )
        {
        }

        public virtual Operator.OperatorLevel GetLevel( Operator.IOperatorLevelHelper helper )
        {
            return Operator.OperatorLevel.Lowest;
        }

        //--//

        protected static Annotation MakeUnique( TypeSystemForIR ts ,
                                                Annotation      an )
        {
            return ts != null ? ts.CreateUniqueAnnotation( an ) : an;
        }

        //--//

        //
        // Access Methods
        //

        //--//

        //
        // Debug Methods
        //

        public abstract string FormatOutput( IIntermediateRepresentationDumper dumper );
    }
}