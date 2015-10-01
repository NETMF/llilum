//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TypeSystem
{
    using System;
    using System.Collections.Generic;


    public abstract class ArrayReferenceTypeRepresentation : ReferenceTypeRepresentation
    {
        //
        // State
        //

        protected TypeRepresentation m_elementType;

        //
        // Constructor Methods
        //

        protected ArrayReferenceTypeRepresentation( AssemblyRepresentation owner       ,
                                                    BuiltInTypes           builtinType ,
                                                    Attributes             flags       ,
                                                    TypeRepresentation     elementType ) : base( owner, builtinType, flags )
        {
            m_elementType = elementType;
        }

        //
        // MetaDataEquality Methods
        //

        public override bool EqualsThroughEquivalence( object         obj ,
                                                       EquivalenceSet set )
        {
            if(obj is ArrayReferenceTypeRepresentation)
            {
                ArrayReferenceTypeRepresentation other = (ArrayReferenceTypeRepresentation)obj;

                return EqualsThroughEquivalence( m_elementType, other.m_elementType, set );
            }

            return false;
        }

        public override bool Equals( object obj )
        {
            return this.EqualsThroughEquivalence( obj, null );
        }

        public override int GetHashCode()
        {
            return m_elementType.GetHashCode();
        }

        //--//

        //
        // Helper Methods
        //

        protected override void PerformInnerDelayedTypeAnalysis(     TypeSystem        typeSystem ,
                                                                 ref ConversionContext context    )
        {
            m_extends = (ReferenceTypeRepresentation)typeSystem.WellKnownTypes.System_Array;

            if(this.IsDelayedType == false)
            {
                MetaData.Normalized.MetaDataTypeDefinitionArray metadata = (MetaData.Normalized.MetaDataTypeDefinitionArray)typeSystem.GetAssociatedMetaData( this );
                if(metadata != null)
                {
                    AnalyzeEntries( typeSystem, ref context, metadata.Interfaces, null, metadata.Methods, null );
                }
            }
        }

        public override void ApplyTransformation( TransformationContext context )
        {
            context.Push( this );

            //
            // Load before calling the base method, because we might get a call to GetHashCode().
            //
            context.Transform( ref m_elementType );

            base.ApplyTransformation( context );

            context.Pop();
        }

        //--//

        public abstract bool SameShape( ArrayReferenceTypeRepresentation other );

        //--//

        //
        // Access Methods
        //

        public override TypeRepresentation ContainedType
        {
            get
            {
                return m_elementType;
            }
        }

        public override bool IsOpenType
        {
            get
            {
                return m_elementType.IsOpenType;
            }
        }

        public override bool IsDelayedType
        {
            get
            {
                return m_elementType.IsDelayedType;
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

            if(rvalue is ArrayReferenceTypeRepresentation)
            {
                ArrayReferenceTypeRepresentation rvalue2 = (ArrayReferenceTypeRepresentation)rvalue;

                TypeRepresentation thisElementType   = this   .ContainedType;
                TypeRepresentation rvalueElementType = rvalue2.ContainedType;

                if(thisElementType.EqualsThroughEquivalence( rvalueElementType, set ))
                {
                    return true;
                }

                if(thisElementType is ReferenceTypeRepresentation)
                {
                    return thisElementType.CanBeAssignedFrom( rvalueElementType, set );
                }
                else if(thisElementType   is EnumerationTypeRepresentation &&
                        rvalueElementType is EnumerationTypeRepresentation  )
                {
                    if(thisElementType.BuiltInType == rvalueElementType.BuiltInType)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
