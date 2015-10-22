//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TypeSystem
{
    using System;
    using System.Collections.Generic;


    public class ValueTypeRepresentation : TypeRepresentation
    {
        //
        // Constructor Methods
        //

        public ValueTypeRepresentation( AssemblyRepresentation owner          ,
                                        BuiltInTypes           builtinType    ,
                                        Attributes             flags          ,
                                        GenericContext         genericContext ) : base( owner, builtinType, flags, genericContext )
        {
        }

        //--//

        //
        // Helper Methods
        //

        protected override void PerformInnerDelayedTypeAnalysis(     TypeSystem        typeSystem ,
                                                                 ref ConversionContext context    )
        {
            MetaData.Normalized.MetaDataTypeDefinitionBase metadata = (MetaData.Normalized.MetaDataTypeDefinitionBase)typeSystem.GetAssociatedMetaData( this );

            ConversionContext localContext = context; localContext.SetContextAsType( this );

            m_extends = typeSystem.ConvertToIR( metadata.Extends, localContext );

            if(this.IsDelayedType == false)
            {
                AnalyzeEntries( typeSystem, ref localContext, metadata.Interfaces, metadata.Fields, metadata.Methods, metadata.MethodImpls );
            }
        }

        //--//

        protected override void SetShapeCategory( TypeSystem typeSystem )
        {
            m_vTable.ShapeCategory = VTable.Shape.Struct;
        }

        //--//

        protected override TypeRepresentation AllocateInstantiation( InstantiationContext ic )
        {
            ValueTypeRepresentation tdRes = new ValueTypeRepresentation( m_owner, m_builtinType, m_flags, new GenericContext( this, ic.TypeParameters ) );

            tdRes.PopulateInstantiation( this, ic );

            return tdRes;
        }

        //--//

        public override GCInfo.Kind ClassifyAsPointer()
        {
            return GCInfo.Kind.AggregateType;
        }

        //--//

        public override InstantiationFlavor GetInstantiationFlavor( TypeSystem typeSystem )
        {
            return InstantiationFlavor.ValueType;
        }

        //--//

        //
        // Access Methods
        //

        public override bool CanBeAssignedFrom( TypeRepresentation rvalue ,
                                                EquivalenceSet     set    )
        {
            if(this.EqualsThroughEquivalence( rvalue, set ))
            {
                return true;
            }

            if(rvalue is BoxedValueTypeRepresentation)
            {
                if(this == rvalue.UnderlyingType)
                {
                    // We can always assign a boxed value type to the same unboxed type. At most, this can cause a null
                    // reference expception, but we should handle that later with injected null checks.
                    return true;
                }
            }

            if(this  .GetType() == typeof(ScalarTypeRepresentation) &&
               rvalue.GetType() == typeof(ScalarTypeRepresentation)  )
            {
                ValueTypeRepresentation rvalue2 = (ValueTypeRepresentation)rvalue;

                uint thisSize   = this   .Size;
                uint rvalueSize = rvalue2.Size;

                // REVIEW: This check still allows the following operations; we should revisit each case:
                // - Assign float to int/uint/long/ulong (lossy).
                // - Assign double to long/ulong (lossy).
                // - Sign changes for same bit-width (can overflow).
                return ((thisSize >= rvalueSize) && (rvalueSize != 0)); // 'void' yields a zero size.
            }

            return base.CanBeAssignedFrom( rvalue, set );
        }

        public override uint SizeOfHoldingVariable
        {
            get
            {
                return this.Size;
            }
        }

        public override bool CanPointToMemory
        {
            get
            {
                return false;
            }
        }

        //--//

        //
        // Debug Methods
        //

        public override String ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder( "ValueTypeRepresentation(" );

            PrettyToString( sb, true, false );

            sb.Append( ")" );

            return sb.ToString();
        }
    }
}
