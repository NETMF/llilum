//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//
// Originally based on the Bartok code base.
//

using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;

namespace Microsoft.Zelig.MetaData.Importer
{
    public class Instruction
    {
        public abstract class Operand
        {
            internal abstract object Normalize( MetaDataNormalizationContext context );
        }

        public class OperandInt : Operand
        {
            //
            // State
            //

            private readonly int m_value;

            //
            // Constructor Methods
            //

            internal OperandInt( int value )
            {
                m_value = value;
            }

            internal override object Normalize( MetaDataNormalizationContext context )
            {
                return m_value;
            }

            //
            // Access Methods
            //

            public int Value
            {
                get
                {
                    return m_value;
                }
            }

            //
            // Debug Methods
            //

            public override String ToString()
            {
                return "OperandInt(0x" + m_value.ToString( "x" ) + "/" + m_value + ")";
            }
        }

        public class OperandTarget : Operand
        {
            //
            // State
            //

            private readonly int m_target;

            //
            // Constructor Methods
            //

            internal OperandTarget( int target )
            {
                m_target = target;
            }

            internal override object Normalize( MetaDataNormalizationContext context )
            {
                return m_target;
            }

            //
            // Access Methods
            //

            public int Target
            {
                get
                {
                    return m_target;
                }
            }

            //
            // Debug Methods
            //

            public override String ToString()
            {
                return "OperandTarget(IL_" + m_target.ToString( "x4" ) + ")";
            }
        }

        public class OperandSingle : Operand
        {
            //
            // State
            //

            private readonly float m_value;

            //
            // Constructor Methods
            //

            internal OperandSingle( float value )
            {
                m_value = value;
            }

            internal override object Normalize( MetaDataNormalizationContext context )
            {
                return m_value;
            }

            //
            // Access Methods
            //

            public float Value
            {
                get
                {
                    return m_value;
                }
            }

            //
            // Debug Methods
            //

            public override String ToString()
            {
                return "OperandSingle(" + m_value + ")";
            }
        }

        public class OperandDouble : Operand
        {
            //
            // State
            //

            private readonly double m_value;

            //
            // Constructor Methods
            //

            internal OperandDouble( double value )
            {
                m_value = value;
            }

            internal override object Normalize( MetaDataNormalizationContext context )
            {
                return m_value;
            }

            //
            // Access Methods
            //

            public double Value
            {
                get
                {
                    return m_value;
                }
            }

            //
            // Debug Methods
            //

            public override String ToString()
            {
                return "OperandDouble(" + m_value + ")";
            }
        }

        public class OperandTargetArray : Operand
        {
            //
            // State
            //

            private readonly int[] m_targets;

            //
            // Constructor Methods
            //

            internal OperandTargetArray( int[] targets )
            {
                m_targets = targets;
            }

            internal override object Normalize( MetaDataNormalizationContext context )
            {
                return m_targets;
            }

            //
            // Access Methods
            //

            public int[] Targets
            {
                get
                {
                    return m_targets;
                }
            }

            //
            // Debug Methods
            //

            public override String ToString()
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder( "OperandTargetArray(" );

                if(m_targets.Length > 0)
                {
                    for(int i = 0; i < m_targets.Length; i++)
                    {
                        if(i != 0)
                        {
                            sb.Append( "," );
                        }

                        sb.Append( "IL_" );
                        sb.Append( m_targets[i].ToString( "x4" ) );
                    }
                }

                sb.Append( ")" );

                return sb.ToString();
            }
        }

        public class OperandByteArray : Operand
        {
            //
            // State
            //

            private readonly byte[] m_value;

            //
            // Constructor Methods
            //

            internal OperandByteArray( byte[] value )
            {
                m_value = value;
            }

            internal override object Normalize( MetaDataNormalizationContext context )
            {
                return m_value;
            }

            //
            // Access Methods
            //

            public byte[] Value
            {
                get
                {
                    return m_value;
                }
            }

            //
            // Debug Methods
            //

            public override String ToString()
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder( "OperandByteArray(" );

                if(m_value.Length > 0)
                {
                    for(int i = 0; i < m_value.Length; i++)
                    {
                        if(i != 0)
                        {
                            sb.Append( "," );
                        }

                        sb.Append( m_value[i] );
                    }
                }

                sb.Append( ")" );

                return sb.ToString();
            }
        }

        public class OperandLong : Operand
        {
            //
            // State
            //

            private readonly long m_value;

            //
            // Constructor Methods
            //

            internal OperandLong( long value )
            {
                m_value = value;
            }

            internal override object Normalize( MetaDataNormalizationContext context )
            {
                return m_value;
            }

            //
            // Access Methods
            //

            public long Value
            {
                get
                {
                    return m_value;
                }
            }

            //
            // Debug Methods
            //

            public override String ToString()
            {
                return "OperandLong(" + m_value + ")";
            }
        }

        public class OperandString : Operand
        {
            //
            // State
            //

            private readonly String m_value;

            //
            // Constructor Methods
            //

            internal OperandString( String value )
            {
                m_value = value;
            }

            internal override object Normalize( MetaDataNormalizationContext context )
            {
                return m_value;
            }

            //
            // Access Methods
            //

            public String Value
            {
                get
                {
                    return m_value;
                }
            }

            //
            // Debug Methods
            //

            public override String ToString()
            {
                return "OperandString(\"" + m_value + "\")";
            }
        }

        public class OperandObject : Operand
        {
            //
            // State
            //

            private MetaDataObject m_value;

            //
            // Constructor Methods
            //

            internal OperandObject( MetaDataObject value )
            {
                m_value = value;
            }

            internal override object Normalize( MetaDataNormalizationContext context )
            {
                Normalized.MetaDataObject res;

                context.GetNormalizedObject( m_value, out res, MetaDataNormalizationMode.Default );

                return res;
            }

            //
            // Access Methods
            //

            public MetaDataObject Value
            {
                get
                {
                    return m_value;
                }
            }

            //
            // Debug Methods
            //

            public override String ToString()
            {
                return "OperandObject(" + m_value + ")";
            }
        }

        //--//

        //
        // State
        //

        private readonly Normalized.Instruction.OpcodeInfo m_opcodeInfo;
        private readonly Operand                           m_operand;
        private readonly Debugging.DebugInfo               m_debugInfo;

        //
        // Constructor Methods
        //

        internal Instruction( Normalized.Instruction.OpcodeInfo opcodeInfo ,
                              Operand                           operand    ,
                              Debugging.DebugInfo               debugInfo  )
        {
            m_opcodeInfo = opcodeInfo;
            m_operand    = operand;
            m_debugInfo  = debugInfo;
        }

        internal Normalized.Instruction Normalize( MetaDataNormalizationContext context )
        {
            object operand = null;

            if(m_operand != null)
            {
                operand = m_operand.Normalize( context );
            }

            Normalized.Instruction inst = new Normalized.Instruction( m_opcodeInfo, operand, m_debugInfo );

            return inst;
        }

        //
        // Access Methods
        //

        public Normalized.Instruction.OpcodeInfo Operator
        {
            get
            {
                return m_opcodeInfo;
            }
        }

        public Operand Argument
        {
            get
            {
                return m_operand;
            }
        }

        public Debugging.DebugInfo DebugInfo
        {
            get
            {
                return m_debugInfo;
            }
        }

        //
        // Debug Methods
        //

        public override String ToString()
        {
            if(m_debugInfo != null)
            {
                return (m_opcodeInfo.Name + m_debugInfo.ToString());
            }
            else
            {
                return (m_opcodeInfo.Name);
            }
        }
    }
}
