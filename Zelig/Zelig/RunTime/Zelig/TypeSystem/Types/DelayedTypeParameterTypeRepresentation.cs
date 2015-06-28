//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TypeSystem
{
    using System;
    using System.Collections.Generic;


    public sealed class DelayedTypeParameterTypeRepresentation : TypeRepresentation
    {
        //
        // State
        //

        private TypeRepresentation m_context;
        private int                m_parameterNumber;

        //
        // Constructor Methods
        //

        public DelayedTypeParameterTypeRepresentation( AssemblyRepresentation owner           ,
                                                       TypeRepresentation     context         ,
                                                       int                    parameterNumber ) : base( owner, BuiltInTypes.VAR, Attributes.None )
        {
            m_context         = context;
            m_parameterNumber = parameterNumber;
        }

        //
        // MetaDataEquality Methods
        //

        public override bool EqualsThroughEquivalence( object         obj ,
                                                       EquivalenceSet set )
        {
            if(obj is DelayedTypeParameterTypeRepresentation)
            {
                DelayedTypeParameterTypeRepresentation other = (DelayedTypeParameterTypeRepresentation)obj;

                if(m_parameterNumber == other.m_parameterNumber)
                {
                    //
                    // Don't compare the context with Equals, it would lead to an infinite recursion!
                    //
                    if(Object.ReferenceEquals( m_context, other.m_context ))
                    {
                        return true;
                    }

                    if(set != null)
                    {
                        if(set.AreEquivalent( this, this.m_context, other, other.m_context ))
                        {
                            return true;
                        }
                    }
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
            return (int)m_parameterNumber ^
                        m_context.GetHashCode();
        }

        //--//

        //
        // Helper Methods
        //

        protected override void PerformInnerDelayedTypeAnalysis(     TypeSystem        typeSystem ,
                                                                 ref ConversionContext context    )
        {
            m_extends = (ReferenceTypeRepresentation)typeSystem.WellKnownTypes.System_Object;

            foreach(TypeRepresentation td in m_context.GenericParametersDefinition[m_parameterNumber].Constraints)
            {
                if(td is InterfaceTypeRepresentation)
                {
                    continue;
                }

                m_extends = td;
                break;
            }
        }

        //--//

        public override void ApplyTransformation( TransformationContext context )
        {
            context.Push( this );

            //
            // Load before calling the base method, because we might get a call to GetHashCode().
            //
            context.Transform( ref m_context         );
            context.Transform( ref m_parameterNumber );

            base.ApplyTransformation( context );

            context.Pop();
        }

        //--//

        protected override void SetShapeCategory( TypeSystem typeSystem )
        {
            m_vTable.ShapeCategory = VTable.Shape.Invalid;
        }

        //--//

        protected override TypeRepresentation AllocateInstantiation( InstantiationContext ic )
        {
            return ic.TypeParameters[m_parameterNumber];
        }

        //--//

        public override GCInfo.Kind ClassifyAsPointer()
        {
            return GCInfo.Kind.Invalid;
        }

        //--//

        public override InstantiationFlavor GetInstantiationFlavor( TypeSystem typeSystem )
        {
            if(m_extends.IsObject)
            {
                return InstantiationFlavor.Delayed;
            }

            if(m_extends == typeSystem.WellKnownTypes.System_ValueType)
            {
                return InstantiationFlavor.ValueType;
            }

            return m_extends.GetInstantiationFlavor( typeSystem );
        }

        //--//

        //
        // Access Methods
        //

        public override bool IsOpenType
        {
            get
            {
                return true;
            }
        }

        public override bool IsDelayedType
        {
            get
            {
                return true;
            }
        }

        public override uint SizeOfHoldingVariable
        {
            get
            {
                return 0;
            }
        }

        public override bool CanPointToMemory
        {
            get
            {
                return false; // We don't know at this stage.
            }
        }

        public TypeRepresentation Context
        {
            get
            {
                return m_context;
            }
        }

        public int ParameterNumber
        {
            get
            {
                return m_parameterNumber;
            }
        }

        //--//

        //
        // Debug Methods
        //

        public override String ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder( "DelayedTypeParameterTypeRepresentation(" );

            PrettyToString( sb, true, false );

            sb.Append( "," );
            sb.Append( m_context );

            sb.Append( ")" );

            return sb.ToString();
        }

        internal override void PrettyToString( System.Text.StringBuilder sb                 ,
                                               bool                      fPrefix            ,
                                               bool                      fWithAbbreviations )
        {
            GenericContext gc = m_context.Generic;

            if(gc != null)
            {
                TypeRepresentation[] parameters = gc.Parameters;
                if(m_parameterNumber < parameters.Length)
                {
                    parameters[m_parameterNumber].PrettyToString( sb, fPrefix, fWithAbbreviations );
                    return;
                }

                GenericParameterDefinition[] defs = gc.ParametersDefinition;
                if(defs != null && m_parameterNumber < defs.Length)
                {
                    sb.Append( defs[m_parameterNumber].Name );
                    return;
                }
            }

            sb.Append( "{VAR,"           );
            sb.Append( m_parameterNumber );
            sb.Append( "}"               );
        }
    }
}
