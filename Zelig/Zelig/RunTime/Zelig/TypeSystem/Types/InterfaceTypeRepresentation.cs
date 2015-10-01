//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TypeSystem
{
    using System;
    using System.Collections.Generic;


    public sealed class InterfaceTypeRepresentation : AbstractReferenceTypeRepresentation
    {
        public static new readonly InterfaceTypeRepresentation[] SharedEmptyArray = new InterfaceTypeRepresentation[0];

        //
        // Constructor Methods
        //

        public InterfaceTypeRepresentation( AssemblyRepresentation owner          ,
                                            BuiltInTypes           builtinType    ,
                                            Attributes             flags          ,
                                            GenericContext         genericContext ) : base( owner, builtinType, flags, genericContext )
        {
        }

        //--//

        //
        // Helper Methods
        //

        protected override void SetShapeCategory( TypeSystem typeSystem )
        {
            m_vTable.ShapeCategory = VTable.Shape.Interface;
        }

        //--//

        protected override TypeRepresentation AllocateInstantiation( InstantiationContext ic )
        {
            InterfaceTypeRepresentation tdRes = new InterfaceTypeRepresentation( m_owner, m_builtinType, m_flags, new GenericContext( this, ic.TypeParameters ) );

            tdRes.PopulateInstantiation( this, ic );

            return tdRes;
        }

        //--//

        public override bool CanBeAssignedFrom( TypeRepresentation rvalue ,
                                                EquivalenceSet     set    )
        {
            rvalue = rvalue.UnderlyingType;

            while(rvalue != null)
            {
                if(this.EqualsThroughEquivalence( rvalue, set ))
                {
                    return true;
                }

                foreach(InterfaceTypeRepresentation itf in rvalue.Interfaces)
                {
                    if(this.CanBeAssignedFrom( itf, set ))
                    {
                        return true;
                    }
                }

                rvalue = rvalue.Extends;
            }

            return false;
        }

        //--//

        //
        // Debug Methods
        //

        public override String ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder( "InterfaceTypeRepresentation(" );

            PrettyToString( sb, true, false );

            sb.Append( ")" );

            return sb.ToString();
        }
    }
}
