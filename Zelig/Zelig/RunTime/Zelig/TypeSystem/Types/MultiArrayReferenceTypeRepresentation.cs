//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TypeSystem
{
    using System;
    using System.Collections.Generic;


    public sealed class MultiArrayReferenceTypeRepresentation : ArrayReferenceTypeRepresentation
    {
        public struct Dimension
        {
            //
            // State
            //

            public uint m_lowerBound;
            public uint m_upperBound;
        }

        //
        // State
        //

        private uint        m_rank;
        private Dimension[] m_dimensions;

        //
        // Constructor Methods
        //

        public MultiArrayReferenceTypeRepresentation( AssemblyRepresentation owner       ,
                                                      TypeRepresentation     elementType ,
                                                      uint                   rank        ,
                                                      Dimension[]            dimensions  ) : base( owner, BuiltInTypes.ARRAY, Attributes.None, elementType )
        {
            m_rank       = rank;
            m_dimensions = dimensions;
        }

        //
        // MetaDataEquality Methods
        //

        public override bool EqualsThroughEquivalence( object         obj ,
                                                       EquivalenceSet set )
        {
            if(obj is MultiArrayReferenceTypeRepresentation)
            {
                MultiArrayReferenceTypeRepresentation other = (MultiArrayReferenceTypeRepresentation)obj;

                if(m_rank == other.m_rank)
                {
                    if(ArrayUtility.ArrayEquals( m_dimensions, other.m_dimensions ))
                    {
                        return base.EqualsThroughEquivalence( obj, set );
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
            return base.GetHashCode();
        }

        //--//

        //
        // Helper Methods
        //

        public override void ApplyTransformation( TransformationContext context )
        {
            context.Push( this );

            //
            // Load before calling the base method, because we might get a call to GetHashCode().
            //
            context.Transform( ref m_rank       );
            context.Transform( ref m_dimensions );

            base.ApplyTransformation( context );

            context.Pop();
        }

        //--//

        protected override void SetShapeCategory( TypeSystem typeSystem )
        {
            m_vTable.ShapeCategory = VTable.Shape.MultiArray;
        }

        //--//

        protected override TypeRepresentation AllocateInstantiation( InstantiationContext ic )
        {
            MultiArrayReferenceTypeRepresentation tdRes = new MultiArrayReferenceTypeRepresentation( m_owner, ic.Instantiate( m_elementType ), m_rank, m_dimensions );

            tdRes.PopulateInstantiation( this, ic );

            return tdRes;
        }

        //--//

        public override bool SameShape( ArrayReferenceTypeRepresentation other )
        {
            MultiArrayReferenceTypeRepresentation other2 = other as MultiArrayReferenceTypeRepresentation;

            if(other2 != null)
            {
                if(m_rank == other2.m_rank)
                {
                    if(ArrayUtility.ArrayEquals( m_dimensions, other2.m_dimensions ))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        //--//

        //
        // Access Methods
        //

        public uint Rank
        {
            get
            {
                return m_rank;
            }
        }

        public Dimension[] Dimensions
        {
            get
            {
                return m_dimensions;
            }
        }

        //--//

        //
        // Debug Methods
        //

        public override String ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder( "MultiArrayReferenceTypeRepresentation(" );

            PrettyToString( sb, true, false );

            sb.Append( ")" );

            return sb.ToString();
        }

        internal override void PrettyToString( System.Text.StringBuilder sb                 ,
                                               bool                      fPrefix            ,
                                               bool                      fWithAbbreviations )
        {
            m_elementType.PrettyToString( sb, fPrefix, fWithAbbreviations );

            sb.Append( "[" );
            for(int i = 0; i < m_rank; i++)
            {
                if(i != 0)
                {
                    sb.Append( "," );
                }

                if(i < m_dimensions.Length)
                {
                    Dimension dm = m_dimensions[i];

                    if(dm.m_lowerBound != 0 || dm.m_upperBound != 0)
                    {
                        sb.Append( dm.m_lowerBound );
                        sb.Append( ".." );
                        sb.Append( dm.m_upperBound );
                    }
                }
            }
            sb.Append( "]" );
        }
    }
}
