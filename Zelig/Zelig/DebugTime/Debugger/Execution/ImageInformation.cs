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
    
    using                          Microsoft.Zelig.TargetModel.ArmProcessor;
    using EncDef                 = Microsoft.Zelig.TargetModel.ArmProcessor.EncodingDefinition_ARM;
    using EncDef_VFP             = Microsoft.Zelig.TargetModel.ArmProcessor.EncodingDefinition_VFP_ARM;
    using InstructionSet         = Microsoft.Zelig.TargetModel.ArmProcessor.InstructionSet;
    using IR                     = Microsoft.Zelig.CodeGeneration.IR;
    using RT                     = Microsoft.Zelig.Runtime;
    using TS                     = Microsoft.Zelig.Runtime.TypeSystem;
    using Cfg                    = Microsoft.Zelig.Configuration.Environment;
    using Microsoft.Zelig.CodeGeneration.IR;


    public class ImageInformation
    {
        private static EncDef s_Encoding = (EncodingDefinition_ARM)CurrentInstructionSetEncoding.GetEncoding();

        //--//

        private class TypeSystemForExecution : IR.TypeSystemForCodeTransformation
        {
            internal TypeSystemForExecution() : base( null )
            {
            }
        }

        public class PointerContext
        {
            //
            // State
            //

            private readonly ImageInformation         m_owner;
            private readonly MemoryDelta              m_mem;
            private readonly uint                     m_baseAddress;
            private readonly TS.TypeRepresentation    m_baseType;
            private readonly TS.FieldRepresentation[] m_path;

            //
            // Constructor Methods
            //

            private PointerContext( ImageInformation         owner       ,
                                    MemoryDelta              mem         ,
                                    uint                     baseAddress ,
                                    TS.TypeRepresentation    baseType    ,
                                    TS.FieldRepresentation[] path        )

            {
                if(path == null)
                {
                    path = TS.FieldRepresentation.SharedEmptyArray;
                }

                m_owner       = owner;
                m_mem         = mem;
                m_baseAddress = baseAddress;
                m_baseType    = baseType;
                m_path        = path;
            }

            //
            // Helper Methods
            //

            public static PointerContext Create( ImageInformation                  owner ,
                                                 MemoryDelta                       mem   ,
                                                 IR.ImageBuilders.SequentialRegion reg   )
            {
                IR.DataManager.DataDescriptor dd = (IR.DataManager.DataDescriptor)reg.Context;

                return new PointerContext( owner, mem, reg.ExternalAddress, dd.Context, null );
            }

            public PointerContext AccessField( TS.FieldRepresentation fd )
            {
                if(fd.FieldType is TS.ValueTypeRepresentation)
                {
                    return new PointerContext( m_owner, m_mem, m_baseAddress, m_baseType, ArrayUtility.AppendToNotNullArray( m_path, fd ) );
                }
                else
                {
                    uint address = this.Address + (uint)fd.Offset;
                    uint ptr;

                    if(m_mem.GetUInt32( address, out ptr ))
                    {
                        TS.TypeRepresentation td = m_owner.VerifyPresenceOfVirtualTable( m_mem, ptr );
                        if(td != null)
                        {
                            return new PointerContext( m_owner, m_mem, ptr, td, null );
                        }
                    }
                }

                return null;
            }

            public PointerContext AccessField( string name )
            {
                TS.TypeRepresentation td = this.Type;

                while(td != null)
                {
                    foreach(TS.FieldRepresentation fd in td.Fields)
                    {
                        if(fd.Name == name)
                        {
                            return AccessField( fd );
                        }
                    }

                    td = td.Extends;
                }

                return null;
            }

            public PointerContext AccessField( params string[] path )
            {
                PointerContext ptr = this;

                foreach(string name in path)
                {
                    ptr = ptr.AccessField( name );
                    if(ptr == null)
                    {
                        break;
                    }
                }

                return ptr;
            }

            public object AccessFieldValue( params string[] path )
            {
                PointerContext ptr = this.AccessField( path );

                return (ptr != null) ? ptr.Value : null;
            }

            //--//

            public override bool Equals( Object obj )
            {
                if(obj is PointerContext)
                {
                    PointerContext other = (PointerContext)obj;

                    return this.Address == other.Address;
                }

                return false;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public static bool operator ==( PointerContext a ,
                                            PointerContext b )
            {
                if((object)a == null && (object)b == null)
                {
                    return true;
                }

                if((object)a != null && (object)b != null)
                {
                    return a.Address == b.Address;
                }

                return false;
            }

            public static bool operator !=( PointerContext a ,
                                            PointerContext b )
            {
                return !(a == b);
            }

            //
            // Access Methods
            //

            public MemoryDelta MemoryDelta
            {
                get
                {
                    return this.m_mem;
                }
            }

            public TS.TypeRepresentation Type
            {
                get
                {
                    if(m_path.Length > 0)
                    {
                        return m_path[m_path.Length - 1].FieldType;
                    }

                    return m_baseType;
                }
            }

            public uint Address
            {
                get
                {
                    uint address = m_baseAddress;

                    foreach(TS.FieldRepresentation fd in m_path)
                    {
                        address += (uint)fd.Offset;
                    }

                    return address;
                }
            }

            public object Value
            {
                get
                {
                    TS.TypeRepresentation td = this.Type;

                    if(td is TS.ScalarTypeRepresentation)
                    {
                        uint size = td.SizeOfHoldingVariable;

                        switch(size)
                        {
                            case 1:
                                {
                                    byte val;

                                    if(m_mem.GetUInt8( this.Address, out val ))
                                    {
                                        if(td.IsSigned)
                                        {
                                            return (sbyte)val;
                                        }
                                        else
                                        {
                                            return val;
                                        }
                                    }
                                }
                                break;

                            case 2:
                                {
                                    ushort val;

                                    if(m_mem.GetUInt16( this.Address, out val ))
                                    {
                                        if(td.IsSigned)
                                        {
                                            return (short)val;
                                        }
                                        else
                                        {
                                            return val;
                                        }
                                    }
                                }
                                break;

                            case 4:
                                {
                                    uint val;

                                    if(m_mem.GetUInt32( this.Address, out val ))
                                    {
                                        if(td.IsInteger)
                                        {
                                            if(td.IsSigned)
                                            {
                                                return (int)val;
                                            }
                                            else
                                            {
                                                return val;
                                            }
                                        }
                                        else
                                        {
                                            return DataConversion.GetFloatFromBytes( val );
                                        }
                                    }
                                }
                                break;

                            case 8:
                                {
                                    ulong val;

                                    if(m_mem.GetUInt64( this.Address, out val ))
                                    {
                                        if(td.IsInteger)
                                        {
                                            if(td.IsSigned)
                                            {
                                                return (long)val;
                                            }
                                            else
                                            {
                                                return val;
                                            }
                                        }
                                        else
                                        {
                                            return DataConversion.GetDoubleFromBytes( val );
                                        }
                                    }
                                }
                                break;
                        }
                    }

                    return null;
                }
            }
        }

        public class RangeToSourceCode
        {
            public class CodeCoverage
            {
                //
                // State
                //

                public ulong Hits;
                public ulong Cycles;
                public ulong WaitStates;
            }

            public class DebugInfoPlusBasicBlock
            {
                public Debugging.DebugInfo Info;
                public IR.BasicBlock       Owner;

                public DebugInfoPlusBasicBlock( Debugging.DebugInfo info ,
                                                IR.BasicBlock       bb   )
                {
                    this.Info  = info;
                    this.Owner = bb;
                }
            }

            //
            // State
            //

            public uint                              BaseAddress;
            public uint                              EndAddress;
            public IR.ImageBuilders.SequentialRegion Region;
            public List< DebugInfoPlusBasicBlock >   DebugInfos = new List< DebugInfoPlusBasicBlock >();
            public CodeCoverage                      Profile;

            //
            // Constructor Methods
            //

            public RangeToSourceCode( IR.ImageBuilders.SequentialRegion region      ,
                                      uint                              baseAddress )
            {
                this.BaseAddress = baseAddress;
                this.EndAddress  = baseAddress;
                this.Region      = region;
            }

            //
            // Helper Methods
            //

            public bool Contains( uint address )
            {
                return (this.BaseAddress <= address && address < this.EndAddress);
            }

            public int CompareTo( RangeToSourceCode other )
            {
                int res = this.BaseAddress.CompareTo( other.BaseAddress );

                if(res == 0)
                {
                    res = this.EndAddress.CompareTo( other.EndAddress );
                }

                return res;
            }

            public static int Compare( RangeToSourceCode left, RangeToSourceCode right )
            {
                return left.CompareTo( right );
            }

            public void AddDebugInfo( Debugging.DebugInfo di ,
                                      IR.BasicBlock       bb )
            {
                if(di.Equals( this.LastDebugInfo ) == false)
                {
                    this.DebugInfos.Add( new DebugInfoPlusBasicBlock( di, bb ) );
                }
            }

            //
            // Access Methods
            //

            public bool IsEmpty
            {
                get
                {
                    return this.BaseAddress == this.EndAddress;
                }
            }

            public Debugging.DebugInfo FirstDebugInfo
            {
                get
                {
                    return this.DebugInfos.Count > 0 ? this.DebugInfos[0].Info : null;
                }
            }

            public Debugging.DebugInfo LastDebugInfo
            {
                get
                {
                    return this.DebugInfos.Count > 0 ? this.DebugInfos[this.DebugInfos.Count-1].Info : null;
                }
            }

            //
            // Debug Methods
            //

            public override string ToString()
            {
                string fmt;

                if(this.FirstDebugInfo != null)
                {
                    fmt = "0x{0:X8}-0x{1:X8} for {2} - {3}";
                }
                else
                {
                    fmt = "0x{0:X8}-0x{1:X8} for {2}";
                }

                return string.Format( fmt, this.BaseAddress, this.EndAddress, this.Region, this.FirstDebugInfo );
            }
        }

        public class DisassemblyLine
        {
            //
            // State
            //

            public readonly uint   Address;
            public readonly string Text;

            //
            // Constructor Methods
            //

            public DisassemblyLine(        uint     address ,
                                           string   format  ,
                                    params object[] args    )
            {
                this.Address = address;
                this.Text    = string.Format( format, args );
            }
        }

        class TypeSystemImpl : Emulation.Hosting.TypeSystem
        {
            //
            // State
            //

            ImageInformation               m_owner;
            Emulation.Hosting.AbstractHost m_host;

            //
            // Constructor Methods
            //

            internal TypeSystemImpl( ImageInformation               owner ,
                                     Emulation.Hosting.AbstractHost host  )
            {
                m_owner = owner;
                m_host  = host;

                m_host.RegisterService( typeof(Emulation.Hosting.TypeSystem), this );
            }

            //
            // Helper Methods
            //

            public override string FetchString( uint address )
            {
                ProcessorHost processorHost; m_host.GetHostingService( out processorHost );

                MemoryDelta memDelta = new MemoryDelta( m_owner, processorHost );

                return m_owner.GetStringFromMemory( memDelta, address );
            }
        }

        //
        // State
        //

        public  string                                                                                          ImageFile;
        public  Emulation.ArmProcessor.SymDef.SymbolToAddressMap                                                SymbolToAddressMap;
        public  Emulation.ArmProcessor.SymDef.AddressToSymbolMap                                                AddressToSymbolMap;
        public  List< Configuration.Environment.ImageSection >                                                  PhysicalImage;
        public  uint                                                                                            ResetVector;
        public  Cfg.ProductCategory                                                                             Configuration;
              
        public  IR.TypeSystemForCodeTransformation                                                              TypeSystem;
        public  IR.ImageBuilders.Core                                                                           ImageBuilder;
        public  TS.CodeMap.ReverseIndex[]                                                                       ReverseCodeMapIndex;
              
        public  GrowOnlyHashTable< Debugging.DebugInfo, List< RangeToSourceCode                             > > SourceCodeToCodeLookup;
        public  GrowOnlyHashTable< string             , List< RangeToSourceCode                             > > FileToCodeLookup;
        public  GrowOnlySet      < string                                                                     > SourceCodeFiles;
        public  GrowOnlyHashTable< string             , List< IR.ControlFlowGraphStateForCodeTransformation > > NameToCfg;
        public  GrowOnlyHashTable< uint               ,       TS.VTable                                       > AddressToVirtualTable;
        public  GrowOnlyHashTable< IR.BasicBlock      ,       IR.BasicBlock                                   > ResolveMethodWrappers;
        public  RangeToSourceCode[]                                                                             SortedRangesToSourceCode;

        public  bool                                                                                            DisplayWrapper;

        private TypeSystemImpl                                                                                  m_implTypeSystem;

        //
        // Constructor Methods
        //

        public ImageInformation( string file )
        {
            this.ImageFile              = file;

            this.SymbolToAddressMap     = new Emulation.ArmProcessor.SymDef.SymbolToAddressMap();
            this.AddressToSymbolMap     = new Emulation.ArmProcessor.SymDef.AddressToSymbolMap();

            this.SourceCodeToCodeLookup = HashTableFactory.New                     < Debugging.DebugInfo, List< RangeToSourceCode                             > >();
            this.FileToCodeLookup       = HashTableFactory.New                     < string             , List< RangeToSourceCode                             > >();
            this.SourceCodeFiles        = SetFactory      .New                     < string                                                                     >();
            this.NameToCfg              = HashTableFactory.New                     < string             , List< IR.ControlFlowGraphStateForCodeTransformation > >();
            this.AddressToVirtualTable  = HashTableFactory.New                     < uint               ,       TS.VTable                                       >();
            this.ResolveMethodWrappers  = HashTableFactory.NewWithReferenceEquality< IR.BasicBlock      ,       IR.BasicBlock                                   >();
        }

        //
        // Helper Methods
        //

        public void ApplyToProcessorHost( Emulation.Hosting.AbstractHost host          ,
                                          Cfg.ProductCategory            configuration )
        {
            this.Configuration = configuration;

            m_implTypeSystem = new TypeSystemImpl( this, host );

            //--//

            Emulation.Hosting.ProcessorControl svcPC; host.GetHostingService( out svcPC );

            svcPC.ResetState( configuration );

            {
                Emulation.Hosting.CodeCoverage svcCC;

                if(host.GetHostingService( out svcCC ))
                {
                    svcCC.SetSymbols( this.SymbolToAddressMap, this.AddressToSymbolMap );
                }
            }

            svcPC.PrepareHardwareModels( configuration );

            {
                Emulation.Hosting.ProcessorStatus svcPS; host.GetHostingService( out svcPS );

                svcPS.ProgramCounter = this.ResetVector;
            }
        }

        public void DeployImage( Emulation.Hosting.AbstractHost                      host     ,
                                 Emulation.Hosting.ProcessorControl.ProgressCallback callback )
        {
            Emulation.Hosting.ProcessorControl svcPC; host.GetHostingService( out svcPC );

            svcPC.DeployImage( this.PhysicalImage, callback );
        }

        public void InitializePlugIns( Emulation.Hosting.AbstractHost host          ,
                                       List< Type >                   extraHandlers )
        {
            Emulation.Hosting.ProcessorControl svcPC; host.GetHostingService( out svcPC );

            svcPC.StartPlugIns( extraHandlers );
        }

        //--//

        public bool TryReadUInt32FromPhysicalImage( uint address, out uint value )
        {
            foreach(var section in this.PhysicalImage)
            {
                if(section.InRange( address ))
                {
                    value = BitConverter.ToUInt32( section.Payload, (int)(address - section.Address) );
                    return true;
                }
            }

            value = 0;
            return false;
        }

        public IR.ImageBuilders.SequentialRegion ResolveMethodToRegion( TS.MethodRepresentation md )
        {
            if(md != null)
            {
                IR.ControlFlowGraphStateForCodeTransformation cfg = IR.TypeSystemForCodeTransformation.GetCodeForMethod( md );

                if(cfg != null)
                {
                    IR.ImageBuilders.SequentialRegion reg = this.TypeSystem.ImageBuilder.GetAssociatedRegion( cfg );

                    if(reg != null)
                    {
                        return reg;
                    }
                }
            }

            return null;
        }

        public TS.CodeMap ResolveAddressToCodeMap( uint address )
        {
            return TS.CodeMap.ResolveAddressToCodeMap( new UIntPtr( address ), this.ReverseCodeMapIndex );
        }

        //--//

        public void CollectCodeCoverage( Emulation.Hosting.AbstractHost host )
        {
            RangeToSourceCode lastRange = null;

            foreach(RangeToSourceCode rng in this.SortedRangesToSourceCode)
            {
                rng.Profile = null;
            }

            Emulation.Hosting.CodeCoverage svc;

            if(host.GetHostingService( out svc ))
            {
                svc.Enumerate( delegate( uint address, uint hits, uint cycles, uint waitStates )
                {
                    if(lastRange == null || !(lastRange.BaseAddress <= address && address < lastRange.EndAddress))
                    {
                        uint offset;

                        lastRange = FindSourceCode( address, out offset );

                        if(lastRange == null)
                        {
                            return;
                        }
                    }

                    RangeToSourceCode.CodeCoverage profile = lastRange.Profile;
                    if(profile == null)
                    {
                        profile = new RangeToSourceCode.CodeCoverage();

                        lastRange.Profile = profile;
                    }

                    profile.Hits       += hits;
                    profile.Cycles     += cycles;
                    profile.WaitStates += waitStates;
                } );
            }
        }

        public RangeToSourceCode FindSourceCode(     uint address ,
                                                 out uint offset  )
        {
            RangeToSourceCode[] array = this.SortedRangesToSourceCode;
            int                 low   = 0;
            int                 high  = array.Length - 1;

            while(low <= high)
            {
                int mid = (high + low) / 2;

                RangeToSourceCode rng = array[mid];

                if(address < rng.BaseAddress)
                {
                    high = mid - 1;
                }
                else if(address >= rng.EndAddress)
                {
                    low = mid + 1;
                }
                else
                {
                    offset = address - rng.BaseAddress;

                    return rng;
                }
            }

            offset = 0;

            return null;
        }

        public IR.ImageBuilders.SequentialRegion FindRegion(     uint address ,
                                                             out uint offset  )
        {
            RangeToSourceCode rng = FindSourceCode( address, out offset );
            
            return rng != null ? rng.Region : null;
        }

        //--//

        private void SetTypeSystem( IR.TypeSystemForCodeTransformation ts )
        {
            ts.BuildHierarchyTables();

            IR.ImageBuilders.Core ib = ts.ImageBuilder;

            this.TypeSystem          = ts;
            this.ImageBuilder        = ib;
            this.ReverseCodeMapIndex = ib.ReverseCodeMapIndex;

            this.PhysicalImage       = ts.Image;

            //--//

            ts.EnumerateFlowGraphs( delegate( IR.ControlFlowGraphStateForCodeTransformation cfg )
            {
                var bbEnter             = cfg.NormalizedEntryBasicBlock;
                var bbExit              = cfg.NormalizedExitBasicBlock;
                var dominance           = cfg.DataFlow_Dominance;
                var immediateDominators = cfg.DataFlow_ImmediateDominators;

                foreach(var bb in cfg.DataFlow_SpanningTree_BasicBlocks)
                {
                    IR.BasicBlock bbMapsTo;

                    if(bb == bbEnter)
                    {
                        bbMapsTo = bb;
                    }
                    else if(bbEnter.IsDominatedBy( bb, dominance ))
                    {
                        bbMapsTo = bbEnter;
                    }
                    else if(bb == bbExit)
                    {
                        bbMapsTo = bbExit;
                    }
                    else if(bbExit != null && bb.IsDominatedBy( bbExit, dominance ))
                    {
                        bbMapsTo = immediateDominators[bbExit.SpanningTreeIndex];
                    }
                    else
                    {
                        bbMapsTo = bb;
                    }

                    this.ResolveMethodWrappers[bb] = bbMapsTo;
                }
            } );

            ComputeRangeToSourceCode( ib );

            this.ResetVector = ib.Bootstrap.ExternalAddress;
        }

        private void ComputeRangeToSourceCode( IR.ImageBuilders.Core ib )
        {
            List< RangeToSourceCode > ranges    = new List< RangeToSourceCode >();
            RangeToSourceCode         lastRange = null;

            foreach(IR.ImageBuilders.SequentialRegion reg in ib.SortedRegions)
            {
                object regCtx = reg.Context;

                if(regCtx is IR.BasicBlock)
                {
                    IR.BasicBlock                                 bb  = (IR.BasicBlock)regCtx;
                    IR.ControlFlowGraphStateForCodeTransformation cfg = (IR.ControlFlowGraphStateForCodeTransformation)bb.Owner;

                    string txt     = cfg.Method.ToShortString();
                    uint   address = reg.BaseAddress.ToUInt32();

                    if(bb is IR.EntryBasicBlock)
                    {
                        this.SymbolToAddressMap[txt] = address;

                        HashTableWithListFactory.AddUnique( this.NameToCfg, txt, cfg );
                    }

                    this.AddressToSymbolMap[address] = txt;

                    //--//

                    if(lastRange != null)
                    {
                        if(lastRange.EndAddress == address && lastRange.IsEmpty)
                        {
                            lastRange.Region = reg;
                        }
                        else
                        {
                            lastRange = null;
                        }
                    }

                    if(lastRange == null)
                    {
                        lastRange = new RangeToSourceCode( reg, address );

                        ranges.Add( lastRange );
                    }

                    foreach(IR.ImageBuilders.ImageAnnotation an in reg.AnnotationList)
                    {
                        IR.Operator op = an.Target as IR.Operator;

                        if(op != null)
                        {
                            Debugging.DebugInfo di = op.DebugInfo;

                            if(di != null)
                            {
                                string fileName    = di.SrcFileName;
                                uint   baseAddress = an.InsertionAddress;
                                bool   fNewRange;
                                bool   fUpdateDI;

                                this.SourceCodeFiles.Insert( fileName );

                                if(lastRange.IsEmpty && lastRange.EndAddress == baseAddress)
                                {
                                    fNewRange = false;
                                    fUpdateDI = true;
                                }
                                else if(lastRange.LastDebugInfo == null)
                                {
                                    fNewRange = false;
                                    fUpdateDI = true;
                                }
                                else if(di.Equals( lastRange.LastDebugInfo ))
                                {
                                    fNewRange = false;
                                    fUpdateDI = false;
                                }
                                else
                                {
                                    fNewRange = true;
                                    fUpdateDI = true;
                                }

                                if(fNewRange)
                                {
                                    if(lastRange != null)
                                    {
                                        lastRange.EndAddress = baseAddress;
                                    }

                                    lastRange = new RangeToSourceCode( reg, baseAddress );

                                    ranges.Add( lastRange );
                                }

                                if(fUpdateDI)
                                {
                                    lastRange.AddDebugInfo( di, bb );

                                    HashTableWithListFactory.AddUnique( this.SourceCodeToCodeLookup, di                , lastRange );
                                    HashTableWithListFactory.AddUnique( this.FileToCodeLookup      , fileName.ToUpper(), lastRange );
                                }

                                lastRange.EndAddress = baseAddress;
                            }
                        }
                    }

                    lastRange.EndAddress = reg.EndAddress.ToUInt32();
                }
                else if(regCtx is IR.DataManager.ObjectDescriptor)
                {
                    IR.DataManager.ObjectDescriptor od = regCtx as IR.DataManager.ObjectDescriptor;

                    if(od != null)
                    {
                        TS.VTable vTable = od.Source as TS.VTable;

                        if(vTable != null)
                        {
                            this.AddressToVirtualTable[ reg.ExternalAddress ] = vTable;
                        }
                    }

                    lastRange = null;
                }
            }

            this.SortedRangesToSourceCode = ranges.ToArray();

            Array.Sort( this.SortedRangesToSourceCode, RangeToSourceCode.Compare );
        }

        //--//--//--//

        public bool LocateFirstSourceCode(     uint                              pc     ,
                                           out IR.ImageBuilders.SequentialRegion reg    ,
                                           out uint                              offset ,
                                           out Debugging.DebugInfo               di     )
        {
            List< Debugging.DebugInfo > diLst;
            uint rgnOffset;

            if(LocateSourceCode( pc, out reg, out offset, out rgnOffset, out diLst ) && diLst.Count > 0)
            {
                di = diLst[0];

                return true;
            }
            else
            {
                di = null;
                return false;
            }
        }

        public bool LocateFirstSourceCode(     IR.ImageBuilders.SequentialRegion reg    ,
                                               uint                              offset ,
                                           out Debugging.DebugInfo               di     )
        {
            List< Debugging.DebugInfo > diLst;

            if(LocateSourceCode( reg, offset, out diLst ) && diLst.Count > 0)
            {
                di = diLst[0];

                return true;
            }
            else
            {
                di = null;
                return false;
            }
        }

        public bool LocateSourceCode(     IR.ImageBuilders.SequentialRegion reg    ,
                                          uint                              offset ,
                                      out List< Debugging.DebugInfo >       di     )
        {
            uint rangeOffset;
            return LocateSourceCode( reg.ExternalAddress + offset, out reg, out offset, out rangeOffset, out di );
        }

        public bool LocateSourceCode(     uint                              pc       ,
                                      out IR.ImageBuilders.SequentialRegion reg      ,
                                      out uint                              offset   ,
                                      out uint                              rngOffset,
                                      out List< Debugging.DebugInfo >       di       )
        {
            di = new List< Debugging.DebugInfo >();

            RangeToSourceCode rng = FindSourceCode( pc, out offset );

            rngOffset = 0;

            if(rng != null)
            {
                reg = rng.Region;

                var           bb     = reg.Context as IR.BasicBlock;
                IR.BasicBlock bbReal = null;

                if(bb != null && this.DisplayWrapper == false)
                {
                    this.ResolveMethodWrappers.TryGetValue( bb, out bbReal );
                }

                if(bbReal != null && bbReal != bb)
                {
                    var reg2 = this.ImageBuilder.GetAssociatedRegion( bbReal );
                    if(reg2 != null)
                    {
                        uint offset2;

                        RangeToSourceCode rng2 = FindSourceCode( reg2.BaseAddress.ToUInt32(), out offset2 );
                        if(rng2 != null)
                        {
                            //
                            // We are currently in a method wrapper. We just need to look for the first DebugInfo in the real code.
                            //
                            foreach(var pair in rng2.DebugInfos)
                            {
                                IR.BasicBlock bb3;
                    
                                this.ResolveMethodWrappers.TryGetValue( pair.Owner, out bb3 );

                                if(bb3 == pair.Owner)
                                {
                                    di.Add( pair.Info );
                                    return true;
                                }
                            }
                        }
                    }
                }

                if(rng.BaseAddress == pc)
                {
                    //
                    // If PC points to the beginning of a range, let's add all the DebugInfo for the range.
                    //
                    foreach(var pair in rng.DebugInfos)
                    {
                        IR.BasicBlock bb3;
            
                        this.ResolveMethodWrappers.TryGetValue( pair.Owner, out bb3 );

                        if(bb3 == pair.Owner || this.DisplayWrapper)
                        {
                            di.Add( pair.Info );
                        }
                    }
                }
                else if(rng.LastDebugInfo != null)
                {
                    //
                    // Otherwise only add the last one.
                    //
                    di.Add( rng.LastDebugInfo );
                }

                rngOffset  = rng.BaseAddress - reg.BaseAddress.ToUInt32();
                offset    += rngOffset;

                if(di.Count == 0)
                {
                    di.Add( FindAnyDebugInfo( reg ) );
                }

                return di.Count > 0;
            }
            else
            {
                reg = null;
            }

            return di.Count != 0;
        }

        //--//--//--//

        public void DisassembleBlock( MemoryDelta             memDelta       ,
                                      List< DisassemblyLine > disasm         ,
                                      uint                    addressStart   ,
                                      uint                    addressCurrent ,
                                      uint                    addressEnd     )
        {
            InstructionSet encoder = (InstructionSet)this.TypeSystem.PlatformAbstraction.GetInstructionSetProvider();

            for(uint diff = addressEnd - addressStart; diff > 0; addressStart += sizeof(uint), diff -= sizeof(uint))
            {
                uint opcode;

                if(memDelta.GetUInt32( addressStart, out opcode ))
                {
                    uint   target;
                    bool   targetIsCode;
                    string res = encoder.DecodeAndPrint( addressStart, opcode, out target, out targetIsCode );

                    disasm.Add( new DisassemblyLine( addressStart, "0x{0:X8}:{1} {2:X8}  {3}", addressStart, addressCurrent == addressStart ? "*" : " ", opcode, res ) );
                }
                else
                {
                    disasm.Add( new DisassemblyLine( addressStart, "0x{0:X8}:{1} ????????", addressStart, addressCurrent == addressStart ? "*" : " " ) );
                }
            }
        }

        //--//--//--//

        public void FetchCurrentStackTrace( List< StackFrame >             lst         ,
                                            MemoryDelta                    memDelta    ,
                                            Emulation.Hosting.Breakpoint[] breakpoints ,
                                            RegisterContext                regs        )
        {
            Emulation.Hosting.DeviceClockTicksTracking svc; memDelta.Host.GetHostingService( out svc );

            using(var hnd = svc.SuspendTiming())
            {
                lst.Clear();

                int depth = 0;

                while(true)
                {
                    uint pc = regs.ProgramCounter;
                    uint rngOffset = 0;

                    TS.CodeMap codeMap = ResolveAddressToCodeMap( pc );

                    var sf = new StackFrame();

                    sf.Depth           = depth++;
                    sf.CodeMapOfTarget = codeMap;
                    sf.RegisterContext = regs;

                    lst.Add( sf );

                    if(codeMap == null)
                    {
                        break;
                    }
                    
                    regs = new RegisterContext( regs );

                    //
                    // Instead of the return address, let's use the call site address.
                    //
                    if(sf.Depth > 0)
                    {
                        pc -= sizeof(uint);
                    }

                    LocateSourceCode( pc, out sf.Region, out sf.RegionOffset, out rngOffset, out sf.DebugInfos );

                    if(sf.Depth > 0)
                    {
                        sf.MoveToLastDebugInfo();
                    }
                    else if(breakpoints != null)
                    {
                        foreach(var bp in breakpoints)
                        {
                            if(bp.WasHit && bp.Address == pc)
                            {
                                sf.MoveForwardToDebugInfo( bp.DebugInfo );
                            }
                        }
                    }

                    if(Unwind( memDelta, codeMap, regs, rngOffset) == false)
                    {
                        break;
                    }
                }
            }
        }

        private bool Unwind( MemoryDelta     memDelta ,
                             TS.CodeMap      codeMap  ,
                             RegisterContext regs     ,
                             uint            rngOffset)
        {
            const uint c_STMFD_Mask     = 0xFFFF0000;
            const uint c_STMFD_Opcode   = 0xE92D0000;

            const uint c_FSTMFDD_Mask   = 0xFFFF0F00;
            const uint c_FSTMFDD_Opcode = 0xED2D0B00;

            const uint c_SUBSP_Mask     = 0xFFFFF000;
            const uint c_SUBSP_Opcode   = 0xE24DD000;

            //--//

            const uint c_FLDMFDD_Mask   = 0xFFFF0F00;
            const uint c_FLDMFDD_Opcode = 0xECBD0B00;

            const uint c_LDMFD_Mask     = 0xFFFF0000;
            const uint c_LDMFD_Opcode   = 0xE8BD0000;

            //--//

            uint pc = regs.ProgramCounter;

            for(int i = 0; i < codeMap.Ranges.Length; i++)
            {
                TS.CodeMap.Range rng = codeMap.Ranges[i];

                if((rng.Flags & TS.CodeMap.Flags.EntryPoint) != 0)
                {
                    if((rng.Flags & TS.CodeMap.Flags.BottomOfCallStack) != 0)
                    {
                        return false;
                    }

                    uint address            = rng.Start.ToUInt32();
                    uint regRestoreMap      = 0;
                    uint regFpRestore       = 0;
                    uint regFpRestoreCount  = 0;
                    uint stackAdjustment    = 0;
                    bool fReturnAddressinLR = false;
                    bool fDone              = false;
                    uint sp                 = regs.StackPointer;
                    bool fIsImport          = (0 != ( codeMap.Target.BuildTimeFlags & TS.MethodRepresentation.BuildTimeAttributes.Imported ));

                    if(fIsImport)
                    {
                        address += rngOffset;
                    }

                    if(pc == address)
                    {
                        //
                        // We are at the beginning of a method, the return address is in LR for sure.
                        //
                        fReturnAddressinLR = true;

                        //
                        // The PC has not executed the next prologue instruction, stop processing.
                        //
                        fDone = true;
                    }

                    if(fDone == false)
                    {
                        if((rng.Flags & TS.CodeMap.Flags.HasIntRegisterSave) != 0)
                        {
                            uint opcode_STMFD;

                            if(memDelta.GetUInt32( address, out opcode_STMFD ))
                            {
                                if((opcode_STMFD & c_STMFD_Mask) == c_STMFD_Opcode)
                                {
                                    regRestoreMap = opcode_STMFD & 0xFFFF;
                                }
                                else
                                {
                                    //CHECKS.ASSERT( false, "Expecting a STMFD opcode, got 0x{0:X8}", opcode_STMFD );
                                    return false;
                                }
                            }
                            else
                            {
                                //CHECKS.ASSERT( false, "Cannot access method entrypoint at 0x{0:X8}", address );
                                return false;
                            }

                            address += sizeof(uint);
                        }
                        else if(fIsImport)
                        {
                            uint opcode_STMFD;

                            if(memDelta.GetUInt32( address, out opcode_STMFD ))
                            {
                                if(( opcode_STMFD & c_STMFD_Mask ) == c_STMFD_Opcode)
                                {
                                    regRestoreMap = opcode_STMFD & 0xFFFF;
                                    address += sizeof( uint );
                                }
                                else
                                {
                                    fReturnAddressinLR = true;
                                }
                            }
                        }
                        else
                        {
                            //
                            // No register push, the return address is in LR for sure.
                            //
                            fReturnAddressinLR = true;
                        }
                    }

                    if(pc == address)
                    {
                        //
                        // The PC has not executed the next prologue instruction, stop processing.
                        //
                        fDone = true;
                    }

                    if(fDone == false)
                    {
                        if((rng.Flags & TS.CodeMap.Flags.HasFpRegisterSave) != 0)
                        {
                            uint opcode_FSTMFDD;

                            if(memDelta.GetUInt32( address, out opcode_FSTMFDD ))
                            {
                                if((opcode_FSTMFDD & c_FSTMFDD_Mask) == c_FSTMFDD_Opcode)
                                {
                                    regFpRestore      = ((opcode_FSTMFDD & 0x0000F000) >> 12) * 2;
                                    regFpRestoreCount =  (opcode_FSTMFDD & 0x000000FF);
                                }
                                else
                                {
                                    //CHECKS.ASSERT( false, "Expecting a FSTMFDD opcode, got 0x{0:X8}", opcode_FSTMFDD );
                                    return false;
                                }
                            }
                            else
                            {
                                //CHECKS.ASSERT( false, "Cannot access method entrypoint at 0x{0:X8}", address );
                                return false;
                            }

                            address += sizeof(uint);
                        }
                        else if(fIsImport)
                        {
                            uint opcode_FSTMFDD;

                            if(memDelta.GetUInt32( address, out opcode_FSTMFDD ))
                            {
                                if(( opcode_FSTMFDD & c_FSTMFDD_Mask ) == c_FSTMFDD_Opcode)
                                {
                                    regFpRestore = ( ( opcode_FSTMFDD & 0x0000F000 ) >> 12 ) * 2;
                                    regFpRestoreCount = ( opcode_FSTMFDD & 0x000000FF );

                                    address += sizeof( uint );
                                }
                            }
                        }
                    }

                    if(pc == address)
                    {
                        //
                        // The PC has not executed the next prologue instruction, stop processing.
                        //
                        fDone = true;
                    }

                    if(fDone == false)
                    {
                        if((rng.Flags & TS.CodeMap.Flags.HasStackAdjustment) != 0)
                        {
                            uint opcode_SUBSP;

                            if(memDelta.GetUInt32( address, out opcode_SUBSP ))
                            {
                                if((opcode_SUBSP & c_SUBSP_Mask) == c_SUBSP_Opcode)
                                {
                                    stackAdjustment = s_Encoding.get_DataProcessing_ImmediateValue( opcode_SUBSP );
                                }
                                else
                                {
                                    //CHECKS.ASSERT( false, "Expecting a SUBSP opcode, got 0x{0:X8}", opcode_SUBSP );
                                    return false;
                                }
                            }
                            else
                            {
                                //CHECKS.ASSERT( false, "Cannot access method entrypoint at 0x{0:X8}", address );
                                return false;
                            }
                        }
                        else if(fIsImport)
                        {
                            uint opcode_SUBSP;

                            if(memDelta.GetUInt32( address, out opcode_SUBSP ))
                            {
                                if(( opcode_SUBSP & c_SUBSP_Mask ) == c_SUBSP_Opcode)
                                {
                                    stackAdjustment = s_Encoding.get_DataProcessing_ImmediateValue( opcode_SUBSP );
                                }
                            }
                        }
                    }

                    //--//

                    //
                    // Deal with method epilogue: if we are on one of the return instructions, we need to restore less state.
                    //
                    uint opcode;

                    if(memDelta.GetUInt32( pc, out opcode ) == false)
                    {
                        return false;
                    }

                    if((opcode & c_FLDMFDD_Mask) == c_FLDMFDD_Opcode)
                    {
                        stackAdjustment = 0;
                    }

                    if((opcode & c_LDMFD_Mask) == c_LDMFD_Opcode)
                    {
                        stackAdjustment   = 0;
                        regFpRestoreCount = 0;
                    }

                    //--//

                    sp += stackAdjustment;

                    if(fReturnAddressinLR)
                    {
                        //TODO: Why does the condition happen where we need to do this?
                        //fmegen: sanity check
                        uint newpc = regs.GetValueAsUInt( EncDef.c_register_lr );
                        if (pc == newpc)
                            return false;

                        pc = newpc;
                    }
                    else
                    {
                        while(regFpRestoreCount > 0)
                        {
                            regs.SetLocationInMemory( EncDef_VFP.c_register_d0 + regFpRestore / 2, memDelta, sp                );
                            regs.SetLocationInMemory( EncDef_VFP.c_register_s0 + regFpRestore    , memDelta, sp                );
                            regs.SetLocationInMemory( EncDef_VFP.c_register_s1 + regFpRestore    , memDelta, sp + sizeof(uint) );

                            sp += 2 * sizeof(uint);

                            regFpRestore      += 2;
                            regFpRestoreCount -= 2;
                        }

                        for(uint regIdx = 0; regIdx < 16; regIdx++)
                        {
                            if((regRestoreMap & (1u << (int)regIdx)) != 0)
                            {
                                if(regIdx == EncDef.c_register_lr)
                                {
                                    memDelta.GetUInt32( sp, out pc );
                                }
                                else
                                {
                                    regs.SetLocationInMemory( regIdx, memDelta, sp );
                                }

                                sp += sizeof(uint);
                            }
                        }
                    }

                    regs.UpdateStackFrame( pc, sp );

                    return true;
                }
            }

            //CHECKS.ASSERT( false, "Cannot unwind {0}", codeMap.Target );

            return false;
        }

        //--//

        public Debugging.DebugInfo FindAnyDebugInfo( IR.ImageBuilders.SequentialRegion reg )
        {
            IR.BasicBlock bb = reg.Context as IR.BasicBlock;
            if(bb != null)
            {
                GrowOnlySet< IR.BasicBlock > visited = SetFactory.NewWithReferenceEquality< IR.BasicBlock >();

                Debugging.DebugInfo di = FindAnyDebugInfo( reg.Owner, bb, visited, false );

                if(di == null)
                {
                    visited.Clear();

                    di = FindAnyDebugInfo( reg.Owner, bb, visited, true );
                }

                return di;
            }

            return null;
        }

        private Debugging.DebugInfo FindAnyDebugInfo( IR.ImageBuilders.Core        ib       ,
                                                      IR.BasicBlock                bb       ,
                                                      GrowOnlySet< IR.BasicBlock > visited  ,
                                                      bool                         fForward )
        {
            if(visited.Insert( bb ) == false)
            {
                Debugging.DebugInfo di;
                IR.BasicBlockEdge[] array = (IR.BasicBlockEdge[])(fForward ? bb.Successors : bb.Predecessors);

                for(int pass = 0; pass < 2; pass++)
                {
                    foreach(IR.BasicBlockEdge edge in array)
                    {
                        IR.BasicBlock bbNext = (IR.BasicBlock)(fForward ? edge.Successor : edge.Predecessor);

                        if(pass == 0)
                        {
                            IR.ImageBuilders.SequentialRegion reg = ib.GetAssociatedRegion( bbNext );
                            if(reg != null)
                            {
                                List< IR.ImageBuilders.ImageAnnotation > lst = reg.AnnotationList;

                                for(int pos = 0; pos < lst.Count; pos++)
                                {
                                    IR.ImageBuilders.ImageAnnotation an = lst[fForward ? pos : (lst.Count-1) - pos];

                                    IR.Operator op = an.Target as IR.Operator;
                                    if(op != null)
                                    {
                                        di = op.DebugInfo;
                                        if(di != null)
                                        {
                                            return di;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            di = FindAnyDebugInfo( ib, bbNext, visited, fForward );
                            if(di != null)
                            {
                                return di;
                            }
                        }
                    }
                }
            }

            return null;
        }

        //--//

        public IR.LowLevelVariableExpression[] AliveVariables( IR.ImageBuilders.SequentialRegion reg    ,
                                                               uint                              offset )
        {
            if(reg == null)
            {
                return new IR.LowLevelVariableExpression[0];
            }

            GrowOnlyHashTable< IR.LowLevelVariableExpression, bool > liveness = HashTableFactory.NewWithReferenceEquality< IR.LowLevelVariableExpression, bool >();

            foreach(IR.ImageBuilders.ImageAnnotation an in reg.AnnotationList)
            {
                if(an.Offset > offset)
                {
                    break;
                }

                IR.ImageBuilders.TrackVariableLifetime tvl = an as IR.ImageBuilders.TrackVariableLifetime;
                if(tvl != null)
                {
                    IR.LowLevelVariableExpression var = (IR.LowLevelVariableExpression)tvl.Target;

                    liveness[var] = tvl.IsAlive;
                }
            }

            List< IR.LowLevelVariableExpression > livenessLst = new List< IR.LowLevelVariableExpression >();

            foreach(IR.LowLevelVariableExpression var in liveness.Keys)
            {
                if(liveness[var])
                {
                    livenessLst.Add( var );
                }
            }

            IR.LowLevelVariableExpression[] livenessArray = livenessLst.ToArray();

            Array.Sort( livenessArray, delegate( IR.LowLevelVariableExpression x ,
                                                 IR.LowLevelVariableExpression y )
            {
                int xKind = x.GetVariableKind();
                int yKind = y.GetVariableKind();

                if(xKind < yKind) return -1;
                if(xKind > yKind) return  1;

                return x.Number - y.Number;
            } );

            return livenessArray;
        }

        //--//

        public PointerContext ResolveCostantExpression( MemoryDelta           mem ,
                                                        IR.ConstantExpression ex  )
        {
            IR.ImageBuilders.SequentialRegion reg = this.ImageBuilder.GetAssociatedRegion( ex );
            if(reg != null)
            {
                return PointerContext.Create( this, mem, reg );
            }

            return null;
        }

        public PointerContext ResolveSingleton( MemoryDelta           mem ,
                                                TS.TypeRepresentation td  )
        {
            td = this.TypeSystem.FindActualSingleton( td );

            IR.DataManager.ObjectDescriptor od = this.TypeSystem.GetSingleton( td );
            if(od != null)
            {
                IR.ImageBuilders.SequentialRegion reg = this.ImageBuilder.GetAssociatedRegion( od );
                if(reg != null)
                {
                    if(VerifyPresenceOfVirtualTable( mem, reg.ExternalAddress ) != null)
                    {
                        return PointerContext.Create( this, mem, reg );
                    }
                }
            }

            return null;
        }

        public TS.TypeRepresentation VerifyPresenceOfVirtualTable( MemoryDelta mem     ,
                                                                   uint        address )
        {
            if(address != 0)
            {
                address = GetVirtualTablePointerAddress( address );

                if(address != 0)
                {
                    uint vTableAddress;

                    if(mem.GetUInt32( address, out vTableAddress ))
                    {
                        return GetTypeFromVirtualTable( vTableAddress );
                    }
                }
            }

            return null;
        }

        //--//

        public uint GetVirtualTablePointerAddress( uint address )
        {
            IR.TypeSystemForCodeTransformation ts   = this.TypeSystem;
            TS.TypeRepresentation              ohTd = ts.WellKnownTypes .Microsoft_Zelig_Runtime_ObjectHeader;
            TS.FieldRepresentation             vtFd = ts.WellKnownFields.ObjectHeader_VirtualTable;

            if(ohTd == null || vtFd == null)
            {
                return 0;
            }
            
            address -=       ohTd.VirtualTable.BaseSize;
            address += (uint)vtFd.Offset;

            return address;
        }

        public TS.TypeRepresentation GetTypeFromVirtualTable( uint vTableAddress )
        {
            TS.VTable vTable;

            if(this.AddressToVirtualTable.TryGetValue( vTableAddress, out vTable ))
            {
                return vTable.TypeInfo;
            }

            return null;
        }

        public string GetStringFromMemory( MemoryDelta memDelta ,
                                           uint        address  )
        {
            if(address != 0)
            {
                IR.TypeSystemForCodeTransformation ts = this.TypeSystem;
                uint                               len;

                if(memDelta.GetUInt32( (uint)(address + ts.WellKnownFields.StringImpl_StringLength.Offset), out len ) && len < 8192)
                {
                    char[] data = new char[len];

                    for(int pos = 0; pos < len; pos++)
                    {
                        ushort val;
                        char   ch;

                        if(memDelta.GetUInt16( (uint)(address + ts.WellKnownFields.StringImpl_FirstChar.Offset + pos * sizeof(char)), out val ) == false)
                        {
                            ch = '?';
                        }
                        else
                        {
                            ch = (char)val;

                            switch(Char.GetUnicodeCategory( ch ))
                            {
                                case System.Globalization.UnicodeCategory.Surrogate       :
                                case System.Globalization.UnicodeCategory.OtherNotAssigned:
                                    ch = '?';
                                    break;
                            }
                        }

                        data[pos] = ch;
                    }

                    return new string( data );
                }
            }

            return null;
        }

        //--//

        public static ImageInformation LoadZeligImage( string                                              file     ,
                                                       Emulation.Hosting.ProcessorControl.ProgressCallback callback )
        {
            var imageInformation = new ImageInformation( file );

            using(System.IO.Stream serializedStream = new System.IO.FileStream( file, System.IO.FileMode.Open ))
            {
                IR.TypeSystemSerializer.CreateInstance dlgCreateInstance = delegate( Type t )
                {
                    if(t == typeof(IR.TypeSystemForCodeTransformation))
                    {
                        return new TypeSystemForExecution();
                    }

                    return null;
                };

                IR.TypeSystemSerializer.ProgressCallback dlgProgress = delegate( long pos, long total )
                {
                    callback( "Loading {0}%", (uint)(pos * 100.0 / total + 0.5) );
                };

                var ts = IR.TypeSystemSerializer.Deserialize( serializedStream, dlgCreateInstance, dlgProgress, 10000 );

                imageInformation.SetTypeSystem( ts );
            }

            return imageInformation;
        }

        public static ImageInformation LoadHexImage( string file )
        {
            ImageInformation imageInformation = new ImageInformation( file );

            imageInformation.ResetVector = 0x10300000;

            var blocks = new List< Emulation.ArmProcessor.SRecordParser.Block >();

            Emulation.ArmProcessor.SRecordParser.Parse( file, blocks );

            imageInformation.ConvertFromBlocksToImage( blocks ); 

            return imageInformation;
        }

        public static ImageInformation LoadAdsImage( string file )
        {
            ImageInformation imageInformation = new ImageInformation( file );

            string prefix = Path.GetDirectoryName( file ) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension( file );

////        root = @"S:\enlistments\local\main\client_BUILD\arm\FLASH\release\P3B_STUB\bin"; // Live depoth
////        root = Environment.ExpandEnvironmentVariables( @"%DEPOTROOT%\Zelig\Test\ArmEmulator\P3B_STUB\bin" );
////        root = Environment.ExpandEnvironmentVariables( @"%DEPOTROOT%\Zelig\Test\ArmEmulator\ZeligVsZenith" );

            Emulation.ArmProcessor.SymDef.Parse( prefix + ".axfdump", imageInformation.SymbolToAddressMap, imageInformation.AddressToSymbolMap );
            Emulation.ArmProcessor.SymDef.Parse( prefix + ".symdefs", imageInformation.SymbolToAddressMap, imageInformation.AddressToSymbolMap );

            uint resetVector;

            if(imageInformation.SymbolToAddressMap.TryGetValue( "EntryPoint", out resetVector ) == false)
            {
                imageInformation.SymbolToAddressMap.TryGetValue( "EntryPoint__Fv", out resetVector );
            }

            imageInformation.ResetVector = resetVector;

            var blocks = new List< Emulation.ArmProcessor.SRecordParser.Block >();

            Emulation.ArmProcessor.SRecordParser.Parse( prefix + @".hex\ER_FLASH", blocks );
            Emulation.ArmProcessor.SRecordParser.Parse( prefix + @".hex\ER_DAT"  , blocks );

            imageInformation.ConvertFromBlocksToImage( blocks ); 

            return imageInformation;
        }

        private void ConvertFromBlocksToImage( List< Emulation.ArmProcessor.SRecordParser.Block > blocks )
        {
            this.PhysicalImage = new List< Configuration.Environment.ImageSection >();

            foreach(Emulation.ArmProcessor.SRecordParser.Block block in blocks)
            {
                this.PhysicalImage.Add( new Configuration.Environment.ImageSection( block.address, block.data.ToArray(), null, 0, 0 ) );
            }
        }
    }
}