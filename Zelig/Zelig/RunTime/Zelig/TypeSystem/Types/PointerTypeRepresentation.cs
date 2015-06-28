//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TypeSystem
{
    using System;
    using System.Collections.Generic;


    public abstract class PointerTypeRepresentation : TypeRepresentation
    {
        //
        // State
        //

        protected TypeRepresentation m_pointerType;

        //
        // Constructor Methods
        //

        protected PointerTypeRepresentation( AssemblyRepresentation owner       ,
                                             BuiltInTypes           builtinType ,
                                             TypeRepresentation     pointerType ) : base( owner, builtinType, Attributes.None )
        {
            m_pointerType = pointerType;

            this.Size = sizeof(uint);
        }

        //
        // MetaDataEquality Methods
        //

        public override bool EqualsThroughEquivalence( object         obj ,
                                                       EquivalenceSet set )
        {
            if(obj is PointerTypeRepresentation)
            {
                PointerTypeRepresentation other = (PointerTypeRepresentation)obj;

                if(EqualsThroughEquivalence( m_pointerType, other.m_pointerType, set ))
                {
                    if(this.GetType() == other.GetType())
                    {
                        return true;
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
            return m_pointerType.GetHashCode();
        }

        //--//

        //
        // Helper Methods
        //

        protected override void PerformInnerDelayedTypeAnalysis(     TypeSystem        typeSystem ,
                                                                 ref ConversionContext context    )
        {
            //m_extends = (ReferenceTypeRepresentation)typeSystem.WellKnownTypes.System_Object;
        }

        //--//

        public override void ApplyTransformation( TransformationContext context )
        {
            context.Push( this );

            //
            // Load before calling the base method, because we might get a call to GetHashCode().
            //
            context.Transform( ref m_pointerType );

            base.ApplyTransformation( context );

            context.Pop();
        }

        //--//

        protected override void SetShapeCategory( TypeSystem typeSystem )
        {
            m_vTable.ShapeCategory = VTable.Shape.Invalid;
        }

        //--//

        public override GCInfo.Kind ClassifyAsPointer()
        {
            return GCInfo.Kind.Internal;
        }

        //--//

        internal override void InvalidateLayout()
        {
            // A pointer has always a valid layout.
        }

        //--//
        
        //
        // Access Methods
        //

        public override TypeRepresentation ContainedType
        {
            get
            {
                return m_pointerType;
            }
        }

        public override TypeRepresentation UnderlyingType
        {
            get
            {
                return m_pointerType;
            }
        }

        public override bool IsOpenType
        {
            get
            {
                return m_pointerType.IsOpenType;
            }
        }

        public override bool IsDelayedType
        {
            get
            {
                return m_pointerType.IsDelayedType;
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

            if(rvalue is UnmanagedPointerTypeRepresentation)
            {
                //
                // Always allow an unmanaged pointer to be cast to another pointer.
                //
                return true;
            }

            StackEquivalentType seRValue = rvalue.StackEquivalentType;
            if(seRValue == StackEquivalentType.NativeInt)
            {
                return true; // Always allow a cast from a native int to a pointer.
            }

            return false;
        }

        public override StackEquivalentType StackEquivalentType
        {
            get
            {
                return StackEquivalentType.Pointer;
            }
        }

        public override uint SizeOfHoldingVariable
        {
            get
            {
                return sizeof(uint);
            }
        }

        public override bool CanPointToMemory
        {
            get
            {
                return true;
            }
        }

        //--//

        //
        // Debug Methods
        //

        internal override void PrettyToString( System.Text.StringBuilder sb                 ,
                                               bool                      fPrefix            ,
                                               bool                      fWithAbbreviations )
        {
            m_pointerType.PrettyToString( sb, fPrefix, fWithAbbreviations );
        }
    }
}
