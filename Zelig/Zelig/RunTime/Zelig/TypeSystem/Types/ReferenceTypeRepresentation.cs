//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TypeSystem
{
    using System;
    using System.Collections.Generic;


    public abstract class ReferenceTypeRepresentation : TypeRepresentation
    {
        //
        // Constructor Methods
        //

        protected ReferenceTypeRepresentation( AssemblyRepresentation owner       ,
                                               BuiltInTypes           builtinType ,
                                               Attributes             flags       ) : base( owner, builtinType, flags )
        {
        }

        protected ReferenceTypeRepresentation( AssemblyRepresentation owner          ,
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
            m_vTable.ShapeCategory = (this == typeSystem.WellKnownTypes.System_Array) ? VTable.Shape.ArrayRoot : VTable.Shape.Class;
        }

        //--//

        public override GCInfo.Kind ClassifyAsPointer()
        {
            for(TypeRepresentation td = this; td != null; td = td.Extends)
            {
                if(td.HasBuildTimeFlag( BuildTimeAttributes.NoVTable ))
                {
                    return GCInfo.Kind.Internal;
                }
            }

            return GCInfo.Kind.Heap;
        }

        //--//

        //
        // Access Methods
        //

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
    }
}
