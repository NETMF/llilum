//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TypeSystem
{
    using System;
    using System.Collections.Generic;


    public sealed class EnumerationTypeRepresentation : ScalarTypeRepresentation
    {
        //
        // State
        //

        private TypeRepresentation  m_underlyingType;
        private string[]            m_literalsName;
        private ulong []            m_literalsValue;

        //
        // Constructor Methods
        //

        public EnumerationTypeRepresentation( AssemblyRepresentation owner          ,
                                              BuiltInTypes           builtinType    ,
                                              Attributes             flags          ,
                                              GenericContext         genericContext ,
                                              uint                   size           ) : base( owner, builtinType, flags, genericContext, size )
        {
        }

        //--//

        //
        // Helper Methods
        //

        protected override void PerformInnerDelayedTypeAnalysis(     TypeSystem        typeSystem ,
                                                                 ref ConversionContext context    )
        {
            base.PerformInnerDelayedTypeAnalysis( typeSystem, ref context );

            m_underlyingType = m_fields[0].FieldType;
        }

        public override void ApplyTransformation( TransformationContext context )
        {
            context.Push( this );

            base.ApplyTransformation( context );

            context.Transform( ref m_underlyingType );
            context.Transform( ref m_literalsName   );
            context.Transform( ref m_literalsValue  );

            context.Pop();
        }

        //--//

        public void AddLiteral( string name  ,
                                object value )
        {
            ulong intVal;
            
            DataConversion.GetAsRawUlong( value, out intVal );

            int len = m_literalsValue != null ? m_literalsValue.Length : 0;
            int pos = len;

            while(pos > 0)
            {
                if(m_literalsValue[pos-1] < intVal)
                {
                    break;
                }

                pos--;
            }

            m_literalsName  = ArrayUtility.InsertAtPositionOfArray( m_literalsName , pos, name   );
            m_literalsValue = ArrayUtility.InsertAtPositionOfArray( m_literalsValue, pos, intVal );
        }

        public string FormatValue( object val )
        {
            ulong intVal;
            
            if(DataConversion.GetAsRawUlong( val, out intVal ))
            {
                if(intVal != 0 && this.HasBuildTimeFlag( BuildTimeAttributes.FlagsAttribute ))
                {
                    var sb = new System.Text.StringBuilder();

                    for(int i = m_literalsValue.Length; i-- > 0;)
                    {
                        ulong mask = m_literalsValue[i];

                        if((mask & intVal) == mask)
                        {
                            intVal &= ~mask;

                            if(sb.Length != 0)
                            {
                                sb.Append( " | " );
                            }

                            sb.Append( m_literalsName[i] );
                        }
                    }

                    if(intVal != 0)
                    {
                        if(sb.Length != 0)
                        {
                            sb.Append( " | " );
                        }

                        sb.AppendFormat( "0x{0}", intVal );
                    }

                    if(sb.Length != 0)
                    {
                        return sb.ToString();
                    }
                }

                for(int i = 0; i < m_literalsValue.Length; i++)
                {
                    if(m_literalsValue[i] == intVal)
                    {
                        return m_literalsName[i];
                    }
                }
            }

            return val.ToString();
        }

        //
        // Access Methods
        //

        public override StackEquivalentType StackEquivalentType
        {
            get
            {
                return m_underlyingType.StackEquivalentType;
            }
        }

        public override TypeRepresentation UnderlyingType
        {
            get
            {
                return m_underlyingType;
            }
        }

        //--//

        public override bool IsSigned
        {
            get
            {
                return m_underlyingType.IsSigned;
            }
        }

        public override bool IsInteger
        {
            get
            {
                return m_underlyingType.IsInteger;
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

            if(m_underlyingType.CanBeAssignedFrom( rvalue, set ))
            {
                return true;
            }

            return false;
        }

        //--//

        //
        // Debug Methods
        //

        public override String ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder( "EnumerationTypeRepresentation(" );

            PrettyToString( sb, true, false );

            sb.Append( ")" );

            return sb.ToString();
        }
    }
}
