//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;
    using System.Runtime.CompilerServices;


    [ImplicitInstance]
    [ForceDevirtualization]
    public abstract class SerialPortsManager
    {
        class EmptyManager : SerialPortsManager
        {
            //
            // Helper Methods
            //

            public override void Initialize()
            {
            }

            public override string[] GetPortNames()
            {
                return null;
            }

            public override System.IO.Ports.SerialStream Open( ref BaseSerialStream.Configuration cfg )
            {
                return null;
            }
        }

        //
        // Helper Methods
        //

        public abstract void Initialize();

        public abstract string[] GetPortNames();

        public abstract System.IO.Ports.SerialStream Open( ref BaseSerialStream.Configuration cfg );

        //
        // Access Methods
        //

        public static extern SerialPortsManager Instance
        {
            [SingletonFactory(Fallback=typeof(EmptyManager))]
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
        }
    }
}
