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


    public class StackFrame
    {
        //
        // State
        //

        public ThreadStatus                      Thread;
        public int                               Depth;
        public TS.CodeMap                        CodeMapOfTarget;
        public RegisterContext                   RegisterContext;

        public IR.ImageBuilders.SequentialRegion Region;
        public uint                              RegionOffset;
        public List< Debugging.DebugInfo >       DebugInfos;
        public int                               DebugInfoOffset;

        //
        // Helper Methods
        //

        public List< ImageInformation.DisassemblyLine > DisassembleBlock( MemoryDelta memDelta                   ,
                                                                          uint        pastOpcodesToDisassembly   ,
                                                                          uint        futureOpcodesToDisassembly )
        {
            var  disasm           = new List< ImageInformation.DisassemblyLine >();
            var  reg              = this.Region;
            uint offset           = this.RegionOffset;
            var  imageInformation = memDelta.ImageInformation;

            var bb = reg.Context as IR.BasicBlock;
            if(bb != null)
            {
                uint address      = reg.BaseAddress.ToUInt32() + offset;
                uint addressStart = FindBoundary( imageInformation, bb.Owner.Method, address, pastOpcodesToDisassembly  , -sizeof(uint) );
                uint addressEnd   = FindBoundary( imageInformation, bb.Owner.Method, address, futureOpcodesToDisassembly,  sizeof(uint) ) + sizeof(uint);

                disasm.Add( new ImageInformation.DisassemblyLine( uint.MaxValue, "{0} [BasicBlock #{1}]", bb.Owner.Method.ToShortString(), bb.SpanningTreeIndex ) );

                imageInformation.DisassembleBlock( memDelta, disasm, addressStart, address, addressEnd );
            }

            return disasm;
        }

        private static uint FindBoundary( ImageInformation        imageInformation ,
                                          TS.MethodRepresentation md               ,
                                          uint                    address          ,
                                          uint                    steps            ,
                                          int                     stepAmount       )
        {
            while(steps-- > 0)
            {
                uint nextAddress = (uint)(address + stepAmount);

                TS.CodeMap cm = imageInformation.ResolveAddressToCodeMap( nextAddress );
                if(cm == null || cm.Target != md)
                {
                    break;
                }

                address = nextAddress;
            }

            return address;
        }

        public bool AdvanceToNextDebugInfo()
        {
            if(this.DebugInfos != null && (this.DebugInfoOffset + 1) < this.DebugInfos.Count)
            {
                this.DebugInfoOffset++;

                return true;
            }

            return false;
        }

        public bool MoveForwardToDebugInfo( Debugging.DebugInfo debugInfo )
        {
            if(debugInfo != null && this.DebugInfos != null)
            {
                for(int i = this.DebugInfoOffset + 1; i < this.DebugInfos.Count; i++)
                {
                    if(this.DebugInfos[i] == debugInfo)
                    {
                        this.DebugInfoOffset = i;
                        return true;
                    }
                }
            }

            return false;
        }

        public void MoveToLastDebugInfo()
        {
            if(this.DebugInfos != null && this.DebugInfos.Count > 0)
            {
                this.DebugInfoOffset = this.DebugInfos.Count - 1;
            }
        }

        //
        // Access Methods
        //

        public uint ProgramCounter
        {
            get
            {
                return this.RegisterContext.ProgramCounter;
            }
        }

        public uint StackPointer
        {
            get
            {
                return this.RegisterContext.StackPointer;
            }
        }

        public TS.MethodRepresentation Method
        {
            get
            {
                var reg = this.Region;

                if(reg != null)
                {
                    var bb = reg.Context as IR.BasicBlock;
                    if(bb != null)
                    {
                        return bb.Owner.Method;
                    }
                }

                return null;
            }
        }

        public Debugging.DebugInfo DebugInfo
        {
            get
            {
                if(this.DebugInfos != null && this.DebugInfoOffset < this.DebugInfos.Count)
                {
                    return this.DebugInfos[ this.DebugInfoOffset ];
                }

                return null;
            }
        }

        //
        // Debug Methods
        //

        public override string ToString()
        {
            var md = this.Method;
            if(md != null)
            {
                return md.ToShortString();
            }

            var reg = this.Region;
            if(reg != null)
            {
                return string.Format( "0x{0:X8}", reg.ExternalAddress );
            }
            
            return string.Format( "0x{0:X8}", this.ProgramCounter );
        }
    }
}