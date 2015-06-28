//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    //
    // Debug classes
    //
    public interface IIntermediateRepresentationDumper
    {
        string FormatOutput( string s    ,
                             object arg1 );

        string FormatOutput( string s    ,
                             object arg1 ,
                             object arg2 );

        string FormatOutput( string s    ,
                             object arg1 ,
                             object arg2 ,
                             object arg3 );

        string FormatOutput(        string   s    ,
                             params object[] args );


        void DumpGraph( ControlFlowGraphState cfg );

        string CreateName( Expression ex );

        string CreateLabel( BasicBlock bb );
    }
}