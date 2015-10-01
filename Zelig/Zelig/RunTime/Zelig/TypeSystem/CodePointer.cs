//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TypeSystem
{
    using System;
    using System.Collections.Generic;


    [WellKnownType( "Microsoft_Zelig_Runtime_TypeSystem_CodePointer" )]
    public struct CodePointer
    {
        //
        // State
        //

        [WellKnownField( "CodePointer_Target" )] public IntPtr Target;

        //
        // Equality Methods
        //

        public bool SameContents( ref CodePointer other )
        {
            return (this.Target == other.Target);
        }

        //
        // Access Methods
        //

        public bool IsValid
        {
            get
            {
                return this.Target.ToInt32() != 0;
            }
        }
    }
}
