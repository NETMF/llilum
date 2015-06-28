//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Debugger.ArmProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.Text;
    using System.IO;
    using System.Windows.Forms;
    using System.Threading;

    using EncDef             = Microsoft.Zelig.TargetModel.ArmProcessor.EncodingDefinition;
    using InstructionSet     = Microsoft.Zelig.TargetModel.ArmProcessor.InstructionSet;
    using IR                 = Microsoft.Zelig.CodeGeneration.IR;
    using RT                 = Microsoft.Zelig.Runtime;
    using TS                 = Microsoft.Zelig.Runtime.TypeSystem;


    public struct PerformanceCounter
    {
        //
        // State
        //

        public uint  Hits;
        public ulong TotalTime;

        //
        // Helper Methods
        //

        public void Fetch( ImageInformation.PointerContext pc )
        {
            if(pc != null)
            {
                ImageInformation.PointerContext pcHits = pc.AccessField( "m_hits" );
                if(pcHits != null)
                {
                    this.Hits = (uint)pcHits.Value;
                }

                ImageInformation.PointerContext pcTime = pc.AccessField( "m_total" );
                if(pcTime != null)
                {
                    this.TotalTime = (ulong)pcTime.Value;
                }
            }
        }
    }
}