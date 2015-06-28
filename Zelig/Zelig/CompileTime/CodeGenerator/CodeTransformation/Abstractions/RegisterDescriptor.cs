//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR.Abstractions
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.MetaData;
    using Microsoft.Zelig.MetaData.Normalized;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public class RegisterDescriptor
    {
        public static readonly RegisterDescriptor[] SharedEmptyArray = new RegisterDescriptor[0];

        public enum Pair
        {
            Int_Int = 0,
            Int_FP  = 1,
            Int_Sys = 2,
            FP_Int  = 3,
            FP_FP   = 4,
            FP_Sys  = 5,
            Sys_Int = 6,
            Sys_FP  = 7,
            Sys_Sys = 8,
        }

        //
        // State
        //

        int                  m_index;
        string               m_mnemonic;
        uint                 m_encoding;
        uint                 m_physicalStorageOffset;
        uint                 m_physicalStorageSize;
        RegisterDescriptor[] m_interfersWith;
        RegisterClass        m_physicalClass;
        RegisterClass        m_storageCapabilities;
        RegisterClass        m_computeCapabilities;

        //
        // Constructor Methods
        //

        private RegisterDescriptor( List< RegisterDescriptor > lst                   ,
                                    string                     mnemonic              ,
                                    uint                       encoding              ,
                                    uint                       physicalStorageOffset ,
                                    uint                       physicalStorageSize   )
        {
            m_index                 = lst.Count;
            m_mnemonic              = mnemonic;
            m_physicalStorageOffset = physicalStorageOffset;
            m_physicalStorageSize   = physicalStorageSize;
            m_encoding              = encoding;
            m_interfersWith         = SharedEmptyArray;

            lst.Add( this );
        }
        
        //
        // Helper Methods
        //

        public void ApplyTransformation( TransformationContextForCodeTransformation context )
        {
            context.Push( this );

            context.Transform( ref m_index                 );
            context.Transform( ref m_mnemonic              );
            context.Transform( ref m_encoding              );
            context.Transform( ref m_physicalStorageOffset );
            context.Transform( ref m_physicalStorageSize   );
            context.Transform( ref m_interfersWith         );
            context.Transform( ref m_physicalClass         );
            context.Transform( ref m_storageCapabilities   );
            context.Transform( ref m_computeCapabilities   );

            context.Pop();
        }

        //--//

        public static RegisterDescriptor ExtractFromExpression( Expression ex )
        {
            var var = ex as VariableExpression;
            if(var != null)
            {
                var reg = var.AliasedVariable as PhysicalRegisterExpression;

                if(reg != null)
                {
                    return reg.RegisterDescriptor;
                }
            }
            
            return null;
        }

        public static Pair GetPairState( RegisterDescriptor lhs ,
                                         RegisterDescriptor rhs )
        {
            if(lhs.InIntegerRegisterFile)
            {
                if(rhs.InIntegerRegisterFile)
                {
                    return Pair.Int_Int;
                }

                if(rhs.IsSystemRegister)
                {
                    return Pair.Int_Sys;
                }

                return Pair.Int_FP;
            }
            else if(lhs.InFloatingPointRegisterFile)
            {
                if(rhs.InIntegerRegisterFile)
                {
                    return Pair.FP_Int;
                }

                if(rhs.IsSystemRegister)
                {
                    return Pair.FP_Sys;
                }

                return Pair.FP_FP;
            }
            else
            {
                if(rhs.InIntegerRegisterFile)
                {
                    return Pair.Sys_Int;
                }

                if(rhs.IsSystemRegister)
                {
                    return Pair.Sys_Sys;
                }

                return Pair.Sys_FP;
            }
        }

        public static RegisterDescriptor CreateTemplateFor32bitSystemRegister( List< RegisterDescriptor > lst                   ,
                                                                               string                     mnemonic              ,
                                                                               uint                       encoding              ,
                                                                               uint                       physicalStorageOffset ,
                                                                               Abstractions.RegisterClass physicalClass         )
        {
            RegisterDescriptor reg = new RegisterDescriptor( lst, mnemonic, encoding, physicalStorageOffset, 1 );

            physicalClass |= RegisterClass.System;

            reg.m_physicalClass       = physicalClass;
            reg.m_storageCapabilities = RegisterClass.None;
            reg.m_computeCapabilities = RegisterClass.None;

            return reg;
        }

        public static RegisterDescriptor CreateTemplateFor32bitIntegerRegister( List< RegisterDescriptor > lst                   ,
                                                                                string                     mnemonic              ,
                                                                                uint                       encoding              ,
                                                                                uint                       physicalStorageOffset ,
                                                                                Abstractions.RegisterClass physicalClass         )
        {
            RegisterDescriptor reg = new RegisterDescriptor( lst, mnemonic, encoding, physicalStorageOffset, 1 );

            physicalClass |= RegisterClass.Integer;

            reg.m_physicalClass       = physicalClass;
            reg.m_storageCapabilities = RegisterClass.Integer              |
                                        RegisterClass.Address              |
                                        RegisterClass.SinglePrecision      |
                                        RegisterClass.DoublePrecision_Low  |
                                        RegisterClass.DoublePrecision_High ;
            reg.m_computeCapabilities = RegisterClass.Integer              |
                                        RegisterClass.Address              ;

            return reg;
        }

        public static RegisterDescriptor CreateTemplateFor32bitFloatingPointRegister( List< RegisterDescriptor > lst                   ,
                                                                                      string                     mnemonic              ,
                                                                                      uint                       encoding              ,
                                                                                      uint                       physicalStorageOffset ,
                                                                                      Abstractions.RegisterClass physicalClass         ,
                                                                                      bool                       lowPart               )
        {
            RegisterDescriptor reg = new RegisterDescriptor( lst, mnemonic, encoding, physicalStorageOffset, 1 );

            physicalClass |= RegisterClass.SinglePrecision;

            reg.m_physicalClass       = physicalClass;
            reg.m_storageCapabilities = RegisterClass.Integer         |
                                        RegisterClass.SinglePrecision | (lowPart ? RegisterClass.DoublePrecision_Low : RegisterClass.DoublePrecision_High);
            reg.m_computeCapabilities = RegisterClass.SinglePrecision;

            return reg;
        }

        public static RegisterDescriptor CreateTemplateFor64bitFloatingPointRegister( List< RegisterDescriptor > lst                   ,
                                                                                      string                     mnemonic              ,
                                                                                      uint                       encoding              ,
                                                                                      uint                       physicalStorageOffset ,
                                                                                      Abstractions.RegisterClass physicalClass         )
        {
            RegisterDescriptor reg = new RegisterDescriptor( lst, mnemonic, encoding, physicalStorageOffset, 2 );

            physicalClass |= RegisterClass.DoublePrecision;

            reg.m_physicalClass       = physicalClass;
            reg.m_storageCapabilities = RegisterClass.DoublePrecision;
            reg.m_computeCapabilities = RegisterClass.DoublePrecision;
            
            return reg;
        }

        public void AddInterference( RegisterDescriptor reg )
        {
            m_interfersWith = ArrayUtility.AddUniqueToNotNullArray( m_interfersWith, reg );
        }

        public object ValueFromBytes( uint[] data )
        {
            if(this.InIntegerRegisterFile)
            {
                switch(m_physicalStorageSize)
                {
                    case 1:
                        return data[0];

                    case 2:
                        return ((ulong)data[1] << 32) | (ulong)data[0];

                    default:
                        return null;
                }
            }
            else if(this.InFloatingPointRegisterFile)
            {
                if(this.IsDoublePrecision)
                {
                    return DataConversion.GetDoubleFromBytes( ((ulong)data[1] << 32) | (ulong)data[0] );
                }
                else
                {
                    return DataConversion.GetFloatFromBytes( data[0] );
                }
            }
            else
            {
                return data[0];
            }
        }

        public uint[] BytesFromValue( object value )
        {
            uint[] res = new uint[m_physicalStorageSize];

            if(this.InIntegerRegisterFile)
            {
                switch(m_physicalStorageSize)
                {
                    case 1:
                        res[0] = (uint)value;
                        break;

                    case 2:
                        ulong val = (ulong)value;

                        res[0] = (uint) val       ;
                        res[1] = (uint)(val >> 32);
                        break;

                    default:
                        return null;
                }
            }
            else if(this.InFloatingPointRegisterFile)
            {
                if(this.IsDoublePrecision)
                {
                    ulong val = DataConversion.GetDoubleAsBytes( (double)value );

                    res[0] = (uint) val       ;
                    res[1] = (uint)(val >> 32);
                }
                else
                {
                    uint val = DataConversion.GetFloatAsBytes( (float)value );

                    res[0] = val;
                }
            }
            else
            {
                res[0] = (uint)value;
            }

            return res;
        }

        //
        // Access Methods
        //

        public int Index
        {
            get
            {
                return m_index;
            }
        }

        public string Mnemonic
        {
            get
            {
                return m_mnemonic;
            }
        }

        public RegisterClass PhysicalClass
        {
            get
            {
                return m_physicalClass;
            }
        }

        public uint Encoding
        {
            get
            {
                return m_encoding;
            }
        }

        public uint PhysicalStorageOffset
        {
            get
            {
                return m_physicalStorageOffset;
            }
        }

        public uint PhysicalStorageSize
        {
            get
            {
                return m_physicalStorageSize;
            }
        }

        public RegisterDescriptor[] InterfersWith
        {
            get
            {
                return m_interfersWith;
            }
        }

        public RegisterClass StorageCapabilities
        {
            get
            {
                return m_storageCapabilities;
            }
        }

        public RegisterClass ComputeCapabilities
        {
            get
            {
                return m_computeCapabilities;
            }
        }

        public bool InIntegerRegisterFile
        {
            get
            {
                return (this.PhysicalClass & RegisterClass.Integer) != 0;
            }
        }

        public bool InFloatingPointRegisterFile
        {
            get
            {
                return (this.PhysicalClass & (RegisterClass.SinglePrecision | RegisterClass.DoublePrecision)) != 0;
            }
        }

        public bool IsDoublePrecision
        {
            get
            {
                return (this.PhysicalClass & RegisterClass.DoublePrecision) != 0;
            }
        }

        public bool IsSystemRegister
        {
            get
            {
                return (this.PhysicalClass & RegisterClass.System) != 0;
            }
        }

        public bool IsSpecial
        {
            get
            {
                return (this.PhysicalClass & RegisterClass.Special) != 0;
            }
        }

        public bool IsLinkAddress
        {
            get
            {
                return (this.PhysicalClass & RegisterClass.LinkAddress) != 0;
            }
        }

        public bool CanAllocate
        {
            get
            {
                return (this.PhysicalClass & RegisterClass.AvailableForAllocation) != 0;
            }
        }

        //
        // Debug Methods
        //

        public override String ToString()
        {
            return string.Format( "RegisterDescriptor( {0} )", this.Mnemonic );
        }
    }
}
