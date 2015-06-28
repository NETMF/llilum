//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class InliningPathAnnotation : Annotation
    {
        //
        // State
        //

        private MethodRepresentation[] m_path;

        //
        // Constructor Methods
        //

        private InliningPathAnnotation( MethodRepresentation[] path )
        {
            m_path = path;
        }

        public static InliningPathAnnotation Create( TypeSystemForIR        ts      ,
                                                     InliningPathAnnotation anOuter ,
                                                     MethodRepresentation   md      ,
                                                     InliningPathAnnotation anInner )
        {
            var pathOuter = anOuter != null ? anOuter.m_path : MethodRepresentation.SharedEmptyArray;
            var pathInner = anInner != null ? anInner.m_path : MethodRepresentation.SharedEmptyArray;

            var path = ArrayUtility.AppendToNotNullArray( pathOuter, md );

            path = ArrayUtility.AppendNotNullArrayToNotNullArray( path, pathInner );

            return (InliningPathAnnotation)MakeUnique( ts, new InliningPathAnnotation( path ) );
        }

        //
        // Equality Methods
        //

        public override bool Equals( Object obj )
        {
            if(obj is InliningPathAnnotation)
            {
                InliningPathAnnotation other = (InliningPathAnnotation)obj;

                return ArrayUtility.ArrayEqualsNotNull( this.m_path, other.m_path, 0 );
            }

            return false;
        }

        public override int GetHashCode()
        {
            if(m_path.Length > 0)
            {
                return m_path[0].GetHashCode();
            }

            return 0;
        }

        //
        // Helper Methods
        //

        public override Annotation Clone( CloningContext context )
        {
            MethodRepresentation[] path = context.ConvertMethods( m_path );

            if(Object.ReferenceEquals( path, m_path ))
            {
                return this; // Nothing to change.
            }

            return RegisterAndCloneState( context, MakeUnique( context.TypeSystem, new InliningPathAnnotation( path ) ) );
        }

        //--//

        public override void ApplyTransformation( TransformationContextForIR context )
        {
            context.Push( this );
            
            base.ApplyTransformation( context );

            object origin = context.GetTransformInitiator();

            if(origin is CompilationSteps.ComputeCallsClosure.Context)
            {
                //
                // Don't propagate the path, it might include methods that don't exist anymore.
                //
            }
            else if(context is TypeSystemForCodeTransformation.FlagProhibitedUses)
            {
                TypeSystemForCodeTransformation ts = (TypeSystemForCodeTransformation)context.GetTypeSystem();

                for(int i = m_path.Length; --i >= 0; )
                {
                    MethodRepresentation md = m_path[i];

                    if(ts.ReachabilitySet.IsProhibited( md ))
                    {
                        m_path = ArrayUtility.RemoveAtPositionFromNotNullArray( m_path, i );
                    }
                }
            }
            else
            {
                context.Transform( ref m_path );
            }

            context.Pop();
        }

        //--//

        //
        // Access Methods
        //

        public MethodRepresentation[] Path
        {
            get
            {
                return m_path;
            }
        }

        //--//

        //
        // Debug Methods
        //

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.Append( "<Inlining Path:" );

            bool fFirst = true;

            foreach(MethodRepresentation md in m_path)
            {
                if(!fFirst)
                {
                    sb.AppendLine();
                }
                else
                {
                    fFirst = false;
                }

                sb.AppendFormat( " {0}", md.ToShortString() );
            }

            sb.Append( ">" );

            return sb.ToString();
        }
    }
}