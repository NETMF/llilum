//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.ImageBuilders
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;

    using Microsoft.Zelig.TargetModel.ArmProcessor;


    public sealed class TrackVariableLifetime : ImageAnnotation
    {
        //
        // State
        //

        bool m_fAlive;

        //
        // Constructor Methods
        //

        private TrackVariableLifetime() // Default constructor required by TypeSystemSerializer.
        {
        }

        public TrackVariableLifetime( SequentialRegion   region ,
                                      uint               offset ,
                                      VariableExpression target ,
                                      bool               fAlive ) : base( region, offset, sizeof(uint), target )
        {
            m_fAlive = fAlive;
        }

        //
        // Helper Methods
        //

        public override void ApplyTransformation( TransformationContextForCodeTransformation context )
        {
            context.Push( this );

            base.ApplyTransformation( context );

            context.Transform( ref m_fAlive );

            context.Pop();
        }

        //--//

        public override bool IsCompileTimeAnnotation
        {
            get { return false; }
        }

        public override void GetAllowedRelocationRange( out int lowerBound ,
                                                        out int upperBound )
        {
            lowerBound = int.MinValue;
            upperBound = int.MaxValue;
        }

        public override bool ApplyRelocation()
        {
            return true;
        }

        //--//

        public static bool ShouldTrackAsAPointer( VariableExpression var )
        {
            if(var is PhysicalRegisterExpression ||
               var is StackLocationExpression     )
            {
                TypeRepresentation td = var.Type;

                if(td is ReferenceTypeRepresentation ||
                   td is PointerTypeRepresentation    )
                {
                    return true;
                }

                if(td is ScalarTypeRepresentation)
                {
                    ScalarTypeRepresentation tdS = (ScalarTypeRepresentation)td;

                    switch(tdS.BuiltInType)
                    {
                        case TypeRepresentation.BuiltInTypes.I:
                        case TypeRepresentation.BuiltInTypes.U:
                            return true; // We track IntPtr and UIntPtr quantities as potentially pinned pointers.
                    }
                }
            }
        
            return false;
        }

        //
        // Access Methods
        //

        public bool IsAlive
        {
            get
            {
                return m_fAlive;
            }
        }

        //
        // Debug Methods
        //

        internal override void Dump( System.IO.TextWriter textWriter )
        {
            VariableExpression var = (VariableExpression)this.Target;

            textWriter.Write( "{0} {1}", m_fAlive ? "ALIVE" : "DEAD ", var );
        }
    }
}
