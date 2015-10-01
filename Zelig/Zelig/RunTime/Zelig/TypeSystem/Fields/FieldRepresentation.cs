//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TypeSystem
{
    using System;
    using System.Collections.Generic;

    public abstract class FieldRepresentation : BaseRepresentation
    {
        //
        // This is just a copy of Microsoft.Zelig.MetaData.FieldAttributes, needed to break the dependency of TypeSystem from MetaData.
        //
        // Also, it's extended to 32bits, to allow extra values.
        //
        [Flags]
        public enum Attributes : uint
        {
            FieldAccessMask     = 0x00000007, // member access mask - Use this mask to retrieve accessibility information.
            PrivateScope        = 0x00000000, // Member not referenceable.
            Private             = 0x00000001, // Accessible only by the parent type.
            FamANDAssem         = 0x00000002, // Accessible by sub-types only in this Assembly.
            Assembly            = 0x00000003, // Accessibly by anyone in the Assembly.
            Family              = 0x00000004, // Accessible only by type and sub-types.
            FamORAssem          = 0x00000005, // Accessibly by sub-types anywhere, plus anyone in assembly.
            Public              = 0x00000006, // Accessibly by anyone who has visibility to this scope.
                                              // end member access mask
                                         
                                              // field contract attributes.
            Static              = 0x00000010, // Defined on type, else per instance.
            InitOnly            = 0x00000020, // Field may only be initialized, not written to after init.
            Literal             = 0x00000040, // Value is compile time constant.
            NotSerialized       = 0x00000080, // Field does not have to be serialized when type is remoted.
            SpecialName         = 0x00000200, // field is special.  Name describes how.
                                    
                                              // interop attributes
            PinvokeImpl         = 0x00002000, // Implementation is forwarded through pinvoke.
                           
                                              // Reserved flags for runtime use only.
            ReservedMask        = 0x00009500,
            RTSpecialName       = 0x00000400, // Runtime(metadata internal APIs) should check name encoding.
            HasFieldMarshal     = 0x00001000, // Field has marshalling information.
            HasDefault          = 0x00008000, // Field has default.
            HasFieldRVA         = 0x00000100, // Field has RVA.

            //
            // Values exclusive to Zelig.
            //
            IsVolatile          = 0x00010000,
            HasSingleAssignment = 0x00020000,
            NeverNull           = 0x00040000,
            HasFixedSize        = 0x00080000,
        }

        public static readonly FieldRepresentation[] SharedEmptyArray = new FieldRepresentation[0];

        //
        // State
        //

        protected TypeRepresentation m_ownerType;
        protected Attributes         m_flags;
        protected string             m_name;
        protected TypeRepresentation m_fieldType;

        //
        // This is populated by the DetectConstants analysis.
        //
        protected int                m_fixedSize;

        //
        // This field is synthesized from the analysis of the type hierarchy and the type characteristics.
        //
        private   int                m_offset;

        //
        // Constructor Methods
        //

        protected FieldRepresentation( TypeRepresentation ownerType ,
                                       string             name      )
        {
            CHECKS.ASSERT( ownerType != null, "Cannot create a FieldRepresentation without an owner" );
            CHECKS.ASSERT( name      != null, "Cannot create a FieldRepresentation without a name"   );

            m_ownerType = ownerType;
            m_name      = name;
            m_offset    = ownerType is ScalarTypeRepresentation ? 0 : int.MinValue;
        }

        //
        // MetaDataEquality Methods
        //

        public override bool EqualsThroughEquivalence( object         obj ,
                                                       EquivalenceSet set )
        {
            if(obj is FieldRepresentation)
            {
                FieldRepresentation other = (FieldRepresentation)obj;

                if(                          m_name      == other.m_name             &&
                   EqualsThroughEquivalence( m_ownerType ,  other.m_ownerType, set ) &&
                   EqualsThroughEquivalence( m_fieldType ,  other.m_fieldType, set )  )
                {
                    CHECKS.ASSERT( this.GetType() == other.GetType(), "Found two inconsistent FieldRepresentation" );

                    return true;
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
            return m_name     .GetHashCode() ^
                   m_ownerType.GetHashCode();
        }

        public static bool operator ==( FieldRepresentation left  ,
                                        FieldRepresentation right )
        {
            return Object.Equals( left, right );
        }

        public static bool operator !=( FieldRepresentation left  ,
                                        FieldRepresentation right )
        {
            return !(left == right);
        }

        //--//

        //
        // Helper Methods
        //

        protected void Done( TypeSystem typeSystem )
        {
            typeSystem.NotifyNewFieldComplete( this );
        }

        internal void PerformFieldAnalysis(     TypeSystem        typeSystem ,
                                            ref ConversionContext context    )
        {
            MetaData.Normalized.MetaDataField metadata = typeSystem.GetAssociatedMetaData( this );

            MetaData.Normalized.SignatureType sig = metadata.FieldSignature.TypeSignature;

            m_flags     = (Attributes)metadata.Flags;
            m_fieldType = typeSystem.ConvertToIR( sig, context );

            if(sig.Modifiers != null)
            {
                TypeRepresentation[] modifiers    = new TypeRepresentation[sig.Modifiers.Length];
                TypeRepresentation   tdIsVolatile = typeSystem.WellKnownTypes.System_Runtime_CompilerServices_IsVolatile;

                for(int i = 0; i < modifiers.Length; i++)
                {
                    modifiers[i] = typeSystem.ConvertToIR( sig.Modifiers[i], context );

                    if(modifiers[i] == tdIsVolatile)
                    {
                        m_flags |= Attributes.IsVolatile;
                    }
                }
            }

            Done( typeSystem );
        }

        internal void ProcessCustomAttributes(     TypeSystem        typeSystem ,
                                               ref ConversionContext context    )
        {
            MetaData.Normalized.MetaDataField metadata = typeSystem.GetAssociatedMetaData( this );

            typeSystem.ConvertToIR( metadata.CustomAttributes, context, this, -1 );
        }

        //--//

        public override void ApplyTransformation( TransformationContext context )
        {
            context.Push( this );

            base.ApplyTransformation( context );

            context.Transform( ref m_ownerType );
            context.Transform( ref m_flags     );
            context.Transform( ref m_name      );
            context.Transform( ref m_fieldType );

            context.Transform( ref m_fixedSize );

            context.Transform( ref m_offset    );

            context.Pop();
        }

        //--//

        //
        // Access Methods
        //

        public TypeRepresentation OwnerType
        {
            get
            {
                return m_ownerType;
            }
        }

        public Attributes Flags
        {
            get
            {
                return m_flags;
            }

            set
            {
                m_flags = value;
            }
        }

        public string Name
        {
            get
            {
                return m_name;
            }
        }

        public TypeRepresentation FieldType
        {
            get
            {
                return m_fieldType;
            }
        }

        public int FixedSize
        {
            get
            {
                return m_fixedSize;
            }

            set
            {
                m_fixedSize = value;

                m_flags |= Attributes.HasFixedSize;
            }
        }

        public bool IsOpenField
        {
            get
            {
                return m_fieldType.IsOpenType;
            }
        }

        //--//

        public bool ValidLayout
        {
            get
            {
                return m_offset >= 0;
            }
        }

        public int Offset
        {
            get
            {
                CHECKS.ASSERT( this.ValidLayout, "Cannot access 'Offset' property on field '{0}' before the field has been laid out", this );

                return m_offset;
            }

            set
            {
                m_offset = value;
            }
        }

        //--//

        //
        // Debug Methods
        //

        public string ToShortString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            PrettyToString( sb, false );

            return sb.ToString();
        }

        protected void PrettyToString( System.Text.StringBuilder sb      ,
                                       bool                      fPrefix )
        {
            m_fieldType.PrettyToString( sb, fPrefix, true );
            sb.Append( " " );

            if(m_ownerType != null)
            {
                m_ownerType.PrettyToString( sb, fPrefix, false );
                sb.Append( "::" );
            }

            sb.Append( m_name );
        }
    }
}
