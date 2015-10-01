// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

// Note: If you add a new ctor overloads you need to update ParameterInfo.RawDefaultValue

namespace System.Runtime.CompilerServices
{
    [Serializable, AttributeUsage( AttributeTargets.Field | AttributeTargets.Parameter, Inherited = false )]
    public sealed class DecimalConstantAttribute : Attribute
    {
        [CLSCompliant( false )]
        public DecimalConstantAttribute( byte scale ,
                                         byte sign  ,
                                         uint hi    ,
                                         uint mid   ,
                                         uint low   )
        {
            dec = new System.Decimal( (int)low, (int)mid, (int)hi, (sign != 0), scale );
        }

        public DecimalConstantAttribute( byte scale ,
                                         byte sign  ,
                                         int  hi    ,
                                         int  mid   ,
                                         int  low   )
        {
            dec = new System.Decimal( low, mid, hi, (sign != 0), scale );
        }


        public System.Decimal Value
        {
            get
            {
                return dec;
            }
        }

        private System.Decimal dec;
    }
}

