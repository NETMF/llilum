//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Emulation.Hosting
{
    using System;
    using System.Collections.Generic;


    public abstract class OutputSink
    {
        public abstract void SetOutput( string file );

        public abstract void StartOutput();

        public abstract void OutputLine(        string   format ,
                                         params object[] args   );

        public abstract void OutputChar( char c );
    }
}
