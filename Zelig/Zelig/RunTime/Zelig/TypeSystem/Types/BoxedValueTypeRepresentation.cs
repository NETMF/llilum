//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TypeSystem
{
    using System;
    using System.Collections.Generic;


    public sealed class BoxedValueTypeRepresentation : ReferenceTypeRepresentation
    {
        //
        // State
        //

        private TypeRepresentation m_valueType; // We need to use TypeRepresentation instead of ValueTypeRepresentation because we might need to box a delayed type.

        //
        // Constructor Methods
        //

        public BoxedValueTypeRepresentation( TypeRepresentation valueType ) : base( valueType.Owner, valueType.BuiltInType, valueType.Flags )
        {
            m_valueType = valueType;
        }

        //
        // MetaDataEquality Methods
        //

        public override bool EqualsThroughEquivalence( object         obj ,
                                                       EquivalenceSet set )
        {
            if(obj is BoxedValueTypeRepresentation)
            {
                BoxedValueTypeRepresentation other = (BoxedValueTypeRepresentation)obj;

                return EqualsThroughEquivalence( m_valueType, other.m_valueType, set );
            }

            return false;
        }

        public override bool Equals( object obj )
        {
            return this.EqualsThroughEquivalence( obj, null );
        }

        public override int GetHashCode()
        {
            return m_valueType.GetHashCode();
        }

        //--//

        //
        // Helper Methods
        //

        protected override void PerformInnerDelayedTypeAnalysis(     TypeSystem        typeSystem ,
                                                                 ref ConversionContext context    )
        {
            m_extends = m_valueType.Extends;
        }

        //--//

        public override void ApplyTransformation( TransformationContext context )
        {
            context.Push( this );

            //
            // Load before calling the base method, because we might get a call to GetHashCode().
            //
            context.Transform( ref m_valueType );

            base.ApplyTransformation( context );

            context.Pop();
        }

        //--//

        protected override TypeRepresentation AllocateInstantiation( InstantiationContext ic )
        {
            TypeRepresentation valueType = ic.Instantiate( m_valueType );

            //
            // When expanding box or unbox in the context of a generic method or type, we might need to box a delayed type.
            // At instantiation time, the delayed type might be resolved to a reference type.
            //
            if(valueType is ValueTypeRepresentation)
            {
                BoxedValueTypeRepresentation tdRes = new BoxedValueTypeRepresentation( valueType );

                tdRes.PopulateInstantiation( this, ic );

                return tdRes;
            }
            else
            {
                return valueType;
            }
        }

        //--//

        public override GCInfo.Kind ClassifyAsPointer()
        {
            return GCInfo.Kind.Internal;
        }

        //
        // Access Methods
        //

        public override TypeRepresentation ContainedType
        {
            get
            {
                return m_valueType;
            }
        }

        public override TypeRepresentation UnderlyingType
        {
            get
            {
                return m_valueType;
            }
        }

        public override bool IsOpenType
        {
            get
            {
                return m_valueType.IsOpenType;
            }
        }

        public override bool IsDelayedType
        {
            get
            {
                return m_valueType.IsDelayedType;
            }
        }

        public override StackEquivalentType StackEquivalentType
        {
            get
            {
                return StackEquivalentType.Object;
            }
        }

        //--//

        public override bool CanBeAssignedFrom( TypeRepresentation rvalue ,
                                                EquivalenceSet     set    )
        {
            if(this.EqualsThroughEquivalence( rvalue, set ))
            {
                return true;
            }

            if(rvalue is PointerTypeRepresentation)
            {
                //
                // Going from a byref to a boxed valuetype is OK.
                //
                return m_valueType.EqualsThroughEquivalence( rvalue.UnderlyingType, set );
            }

            return m_valueType.CanBeAssignedFrom( rvalue, set );
        }

        //--//

        //
        // Debug Methods
        //

        public override String ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder( "BoxedValueTypeRepresentation(" );

            PrettyToString( sb, true, false );

            sb.Append( ")" );

            return sb.ToString();
        }

        internal override void PrettyToString( System.Text.StringBuilder sb                 ,
                                               bool                      fPrefix            ,
                                               bool                      fWithAbbreviations )
        {
            sb.Append( "boxed " );

            m_valueType.PrettyToString( sb, fPrefix, fWithAbbreviations );
        }
    }
}
