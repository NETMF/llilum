//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class ConstantExpression : Expression
    {
        public abstract class DelayedValue
        {
            //
            // Access Methods
            //

            public abstract bool CanEvaluate
            {
                get;
            }

            public abstract object Value
            {
                get;
            }
        }

        //--//

        public static new readonly ConstantExpression[] SharedEmptyArray = new ConstantExpression[0];

        //
        // State
        //

        private object m_value;

        //
        // Constructor Methods
        //

        public ConstantExpression( TypeRepresentation type  ,
                                   object             value ) : base( type )
        {
            m_value = value;

            //
            // Coming from byte code, we might see loads for long/ulong that take a 32 bit value as input.
            //
            if(this.IsValueInteger)
            {
                ulong val; this.GetAsRawUlong( out val );

                m_value = ConvertToType( type, val );
            }
        }

        //--//

        //
        // Helper Methods
        //

        public override Expression Clone( CloningContext context )
        {
            TypeRepresentation td = context.ConvertType( m_type );

            if(Object.ReferenceEquals( td, m_type ))
            {
                return this; // Constants don't need to be cloned.
            }
            else
            {
                return new ConstantExpression( td, m_value );
            }
        }

        //--//

        public override void ApplyTransformation( TransformationContextForIR context )
        {
            context.Push( this );

            base.ApplyTransformation( context );

            context.Transform( ref m_value );

            context.Pop();
        }

        //--//

        public override Operator.OperatorLevel GetLevel( Operator.IOperatorLevelHelper helper )
        {
            if(m_type.ValidLayout == false)
            {
                return Operator.OperatorLevel.ConcreteTypes;
            }

            if(helper.FitsInPhysicalRegister( m_type ))
            {
                //
                // If they can fit in a register, we consider it good enough to be at the lowest level.
                //
                return Operator.OperatorLevel.Lowest;
            }

            return Operator.OperatorLevel.ConcreteTypes_NoExceptions;
        }

        //--//

        public bool GetAsRawUlong( out ulong value )
        {
            return DataConversion.GetAsRawUlong( this.Value, out value );
        }

        public bool GetAsUnsignedInteger( out ulong value )
        {
            return DataConversion.GetAsUnsignedInteger( this.Value, out value );
        }

        public bool GetAsSignedInteger( out long value )
        {
            return DataConversion.GetAsSignedInteger( this.Value, out value );
        }

        public bool GetFloatingPoint( out double value )
        {
            return DataConversion.GetFloatingPoint( this.Value, out value );
        }

        //--//

        public static object ConvertToType( TypeRepresentation type  ,
                                            ulong              value )
        {
            //
            // Special case for memory-mapped peripherals.
            //
            if(type is ReferenceTypeRepresentation)
            {
                return new UIntPtr( (uint)value );
            }

            //
            // Special case for pointers.
            //
            if(type is PointerTypeRepresentation)
            {
                return new UIntPtr( (uint)value );
            }

            if(type is EnumerationTypeRepresentation)
            {
                type = type.UnderlyingType;
            }

            if(type is ScalarTypeRepresentation)
            {
                switch(type.BuiltInType)
                {
                    case TypeRepresentation.BuiltInTypes.BOOLEAN: return                                            value != 0;
                    case TypeRepresentation.BuiltInTypes.CHAR   : return                                    (char  )value;
                    case TypeRepresentation.BuiltInTypes.I1     : return                                    (sbyte )value;
                    case TypeRepresentation.BuiltInTypes.U1     : return                                    (byte  )value;
                    case TypeRepresentation.BuiltInTypes.I2     : return                                    (short )value;
                    case TypeRepresentation.BuiltInTypes.U2     : return                                    (ushort)value;
                    case TypeRepresentation.BuiltInTypes.I4     : return                                    (int   )value;
                    case TypeRepresentation.BuiltInTypes.U4     : return                                    (uint  )value;
                    case TypeRepresentation.BuiltInTypes.I8     : return                                    (long  )value;
                    case TypeRepresentation.BuiltInTypes.U8     : return                                    (ulong )value;
                    case TypeRepresentation.BuiltInTypes.I      : return new IntPtr                       ( (int   )value );
                    case TypeRepresentation.BuiltInTypes.U      : return new UIntPtr                      ( (uint  )value );
                    case TypeRepresentation.BuiltInTypes.R4     : return DataConversion.GetFloatFromBytes ( (uint  )value );
                    case TypeRepresentation.BuiltInTypes.R8     : return DataConversion.GetDoubleFromBytes(         value );
                }
            }

            throw TypeConsistencyErrorException.Create( "Cannot convert {0} to {1}", value, type );
        }

        //--//

        //
        // Access Methods
        //

        public object Value
        {
            get
            {
                DelayedValue delayedValue = m_value as DelayedValue;

                if(delayedValue != null && delayedValue.CanEvaluate)
                {
                    return delayedValue.Value;
                }

                return m_value;
            }
        }

        public override CanBeNull CanBeNull
        {
            get
            {
                object val = this.Value;

                return (val == null) ? CanBeNull.Yes : CanBeNull.No;
            }
        }

        public override bool CanTakeAddress
        {
            get
            {
                return false;
            }
        }

        public int SizeOfValue
        {
            get
            {
                switch(this.TypeCode)
                {
                    case TypeCode.Boolean: return 1;
                    case TypeCode.Char   : return 2;
                    case TypeCode.SByte  : return 1;
                    case TypeCode.Byte   : return 1;
                    case TypeCode.Int16  : return 2;
                    case TypeCode.UInt16 : return 2;
                    case TypeCode.Int32  : return 4;
                    case TypeCode.UInt32 : return 4;
                    case TypeCode.Int64  : return 8;
                    case TypeCode.UInt64 : return 8;
                    case TypeCode.Single : return 4;
                    case TypeCode.Double : return 8;
////                case TypeCode.Decimal:
                }
                
                return 0;
            }
        }

        public bool IsValueSigned
        {
            get
            {
                switch(this.TypeCode)
                {
                    case TypeCode.SByte  : return true;
                    case TypeCode.Int16  : return true;
                    case TypeCode.Int32  : return true;
                    case TypeCode.Int64  : return true;
                    case TypeCode.Single : return true;
                    case TypeCode.Double : return true;
////                case TypeCode.Decimal:
                }

                return false;
            }
        }

        public bool IsValueInteger
        {
            get
            {
                switch(this.TypeCode)
                {
                    case TypeCode.Boolean:
                    case TypeCode.Char   :
                    case TypeCode.SByte  :
                    case TypeCode.Byte   :
                    case TypeCode.Int16  :
                    case TypeCode.UInt16 :
                    case TypeCode.Int32  :
                    case TypeCode.UInt32 :
                    case TypeCode.Int64  :
                    case TypeCode.UInt64 :
                         return true;
                }
                
                return false;
            }
        }

        public TypeCode TypeCode
        {
            get
            {
                return DataConversion.GetTypeCode( this.Value );
            }
        }

        public bool IsValueFloatingPoint
        {
            get
            {
                object val = this.Value;

                if(val is float ) return true;
                if(val is double) return true;

                return false;
            }
        }

        //--//

        //
        // Debug Methods
        //

        public override void InnerToString( System.Text.StringBuilder sb )
        {
            object val = this.Value;

            if(val != null)
            {
                sb.AppendFormat( "$Const({0} {1})", m_type.FullNameWithAbbreviation, val );
            }
            else
            {
                sb.Append( "<null value>" );
            }
        }
    }
}