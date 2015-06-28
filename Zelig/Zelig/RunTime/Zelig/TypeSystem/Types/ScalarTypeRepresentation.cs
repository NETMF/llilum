//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TypeSystem
{
    using System;
    using System.Collections.Generic;


    public class ScalarTypeRepresentation : ValueTypeRepresentation
    {
        //
        // Constructor Methods
        //

        public ScalarTypeRepresentation( AssemblyRepresentation owner          ,
                                         BuiltInTypes           builtinType    ,
                                         Attributes             flags          ,
                                         GenericContext         genericContext ,
                                         uint                   size           ) : base( owner, builtinType, flags, genericContext )
        {
            this.Size = size;
        }

        //--//

        //
        // Helper Methods
        //

        public Type ConvertToRuntimeType()
        {
            switch(m_builtinType)
            {
                case TypeRepresentation.BuiltInTypes.VOID   : return typeof(void          );
                case TypeRepresentation.BuiltInTypes.BOOLEAN: return typeof(System.Boolean);
                case TypeRepresentation.BuiltInTypes.CHAR   : return typeof(System.Char   );
                case TypeRepresentation.BuiltInTypes.I1     : return typeof(System.SByte  );
                case TypeRepresentation.BuiltInTypes.U1     : return typeof(System.Byte   );
                case TypeRepresentation.BuiltInTypes.I2     : return typeof(System.Int16  );
                case TypeRepresentation.BuiltInTypes.U2     : return typeof(System.UInt16 );
                case TypeRepresentation.BuiltInTypes.I4     : return typeof(System.Int32  );
                case TypeRepresentation.BuiltInTypes.U4     : return typeof(System.UInt32 );
                case TypeRepresentation.BuiltInTypes.I8     : return typeof(System.Int64  );
                case TypeRepresentation.BuiltInTypes.U8     : return typeof(System.UInt64 );
                case TypeRepresentation.BuiltInTypes.R4     : return typeof(System.Single );
                case TypeRepresentation.BuiltInTypes.R8     : return typeof(System.Double );
                case TypeRepresentation.BuiltInTypes.I      : return typeof(System.IntPtr );
                case TypeRepresentation.BuiltInTypes.U      : return typeof(System.UIntPtr);
            }

            return null;
        }

        //--//

        public override GCInfo.Kind ClassifyAsPointer()
        {
            switch(m_builtinType)
            {
                case BuiltInTypes.I:
                case BuiltInTypes.U:
                    return GCInfo.Kind.Potential;

                default:
                    return GCInfo.Kind.NotAPointer;
            }
        }

        //--//
        
        protected override void SetShapeCategory( TypeSystem typeSystem )
        {
            m_vTable.ShapeCategory = VTable.Shape.Scalar;
        }

        //--//

        protected override TypeRepresentation AllocateInstantiation( InstantiationContext ic )
        {
            return this;
        }

        //--//

        internal override void InvalidateLayout()
        {
            // A scalar has always a valid layout.
        }

        //--//

        //
        // Access Methods
        //

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
                switch(m_builtinType)
                {
                    case BuiltInTypes.I:
                    case BuiltInTypes.U:
                        return true;
                }

                return false;
            }
        }

        public override bool IsNumeric
        {
            get
            {
                return true;
            }
        }

        public override bool IsSigned
        {
            get
            {
                switch(m_builtinType)
                {
                    case BuiltInTypes.I1:
                    case BuiltInTypes.I2:
                    case BuiltInTypes.I4:
                    case BuiltInTypes.I8:
                    case BuiltInTypes.R4:
                    case BuiltInTypes.R8:
                    case BuiltInTypes.I :
                        return true;
                }

                return false;
            }
        }

        public override bool IsInteger
        {
            get
            {
                switch(m_builtinType)
                {
                    case BuiltInTypes.BOOLEAN:
                    case BuiltInTypes.CHAR   :
                    case BuiltInTypes.I1     :
                    case BuiltInTypes.U1     :
                    case BuiltInTypes.I2     :
                    case BuiltInTypes.U2     :
                    case BuiltInTypes.I4     :
                    case BuiltInTypes.U4     :
                    case BuiltInTypes.I8     :
                    case BuiltInTypes.U8     :
                    case BuiltInTypes.I      :
                    case BuiltInTypes.U      :
                        return true;
                }

                return false;
            }
        }

        public override bool IsFloatingPoint
        {
            get
            {
                switch(m_builtinType)
                {
                    case BuiltInTypes.R4:
                    case BuiltInTypes.R8:
                        return true;
                }

                return false;
            }
        }

        //--//

        //
        // Debug Methods
        //

        public override String ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder( "ScalarTypeRepresentation(" );

            PrettyToString( sb, true, false );

            sb.Append( ")" );

            return sb.ToString();
        }
    }
}
