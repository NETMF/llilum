//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Debugger.ArmProcessor
{
    using System;
    using System.Collections.Generic;

    using IR = Microsoft.Zelig.CodeGeneration.IR;
    using RT = Microsoft.Zelig.Runtime;
    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class RegisterValueHandle : AbstractValueHandle
    {
        //
        // State
        //

        public readonly RegisterContext.AbstractValue RegisterValue;

        //
        // Contructor Methods
        //

        public RegisterValueHandle( RegisterContext.AbstractValue registerValue      ,
                                    TS.TypeRepresentation         type               ,
                                    bool                          fAsHoldingVariable ) : base( type, null, null, fAsHoldingVariable )
        {
            this.RegisterValue = registerValue;
        }

        //
        // Helper Methods
        //

        public override bool IsEquivalent( AbstractValueHandle abstractValueHandle )
        {
            var other = abstractValueHandle as RegisterValueHandle;

            if(other != null)
            {
                if(this.RegisterValue.Register == other.RegisterValue.Register)
                {
                    return true;
                }
            }

            return false;
        }

        public override Emulation.Hosting.BinaryBlob Read(     int  offset   ,
                                                               int  count    ,
                                                           out bool fChanged )
        {
            //
            // TODO: Move this to register context
            //
            fChanged = false;

            if(this.RegisterValue.IsAvailable == false)
            {
                return null;
            }

            var bb = this.RegisterValue.GetValue();

            return bb.Extract( offset, count ); 
        }

        public override bool Write( Emulation.Hosting.BinaryBlob bb     ,
                                    int                          offset ,
                                    int                          count  )
        {
            var bbReg = this.RegisterValue.GetValue();

            bbReg.Insert( bb, offset, count );

            return this.RegisterValue.SetValue( bbReg );
        }

        //
        // Access Methods
        //

        public override bool CanUpdate
        {
            get
            {
                return this.RegisterValue.CanUpdate;
            }
        }

        public override bool HasAddress
        {
            get
            {
                return false;
            }
        }

        public override uint Address
        {
            get
            {
                return 0;
            }
        }
    }
}