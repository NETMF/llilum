//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//#define DEBUG_BRICKTABLE


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

    using EncDef                 = Microsoft.Zelig.TargetModel.ArmProcessor.EncodingDefinition;
    using EncDef_VFP             = Microsoft.Zelig.TargetModel.ArmProcessor.EncodingDefinition_VFP;
    using InstructionSet         = Microsoft.Zelig.TargetModel.ArmProcessor.InstructionSet;
    using IR                     = Microsoft.Zelig.CodeGeneration.IR;
    using RT                     = Microsoft.Zelig.Runtime;
    using TS                     = Microsoft.Zelig.Runtime.TypeSystem;
    using Cfg                    = Microsoft.Zelig.Configuration.Environment;


    public class DebugGarbageColllection
    {
        class ProhibitedRange
        {
            //
            // State
            //

            internal uint m_addressStart;
            internal uint m_addressEnd;

            //
            // Constructor Methods
            //

            internal ProhibitedRange( uint addressStart ,
                                      uint addressEnd   )
            {
                m_addressStart = addressStart;
                m_addressEnd   = addressEnd;
            }

            //
            // Helper Methods
            //

            internal int Compare( uint addressStart ,
                                  uint addressEnd   )
            {
                if(m_addressStart >= addressEnd)
                {
                    return 1;
                }
                else if(m_addressEnd <= addressStart)
                {
                    return -1;
                }
                else
                {
                    return 0;
                }
            }

            //
            // Debug Methods
            //

            public override string ToString()
            {
                return string.Format( "[{0:X8}-{1:X8}]", m_addressStart, m_addressEnd );
            }
        }

        //
        // State
        //

        private MemoryDelta                      m_memDelta;
        private bool                             m_fVerbose;

        private InteropHelper                    m_ih;
        private Emulation.ArmProcessor.Simulator m_simulator;

        private List< ProhibitedRange >          m_freeRanges = new List< ProhibitedRange >();
        private int                              m_suspendCount;

        //
        // Constructor Methods
        //

        public DebugGarbageColllection( MemoryDelta memDelta ,
                                        bool        fVerbose )
        {
            m_memDelta = memDelta;
            m_fVerbose = fVerbose;

            m_ih = new InteropHelper( memDelta.ImageInformation, memDelta.Host );

            memDelta.Host.GetHostingService( out this.m_simulator );

            if(memDelta.ImageInformation.TypeSystem != null && this.m_simulator != null)
            {
                RegisterInterops();
            }
        }

        //
        // Helper Methods
        //

        void RegisterInterops()
        {
            this.m_simulator.NotifyOnStore += delegate( uint address, uint value, TargetAdapterAbstractionLayer.MemoryAccessType kind )
            {
                CheckAccess( address, value == 0xDEADBEEF );
            };

            m_ih.SetInteropOnWellKnownMethod( "DebugGC_MemorySegment_Initialize", false, delegate()
            {
                m_suspendCount++;

                m_ih.SetTemporaryInteropOnReturn( delegate()
                {
                    m_suspendCount--;

                    return Emulation.Hosting.Interop.CallbackResponse.DoNothing;
                } );

                return Emulation.Hosting.Interop.CallbackResponse.DoNothing;
            } );

            m_ih.SetInteropOnWellKnownMethod( "DebugGC_MemorySegment_LinkNewFreeBlock", false, delegate()
            {
                m_suspendCount++;

                m_ih.SetTemporaryInteropOnReturn( delegate()
                {
                    m_suspendCount--;

                    return Emulation.Hosting.Interop.CallbackResponse.DoNothing;
                } );

                return Emulation.Hosting.Interop.CallbackResponse.DoNothing;
            } );

            m_ih.SetInteropOnWellKnownMethod( "DebugGC_MemorySegment_RemoveFreeBlock", false, delegate()
            {
                uint ptr = m_ih.GetRegisterUInt32( EncDef.c_register_r1 );

                int pos = FindRange( m_freeRanges, ptr );

                if(pos < 0)
                {
                    ReportProblem( "Expecting a free block at 0x{0:X8}", ptr );
                }
                else
                {
                    m_freeRanges.RemoveAt( pos );
                }

                return Emulation.Hosting.Interop.CallbackResponse.DoNothing;
            } );

            m_ih.SetInteropOnWellKnownMethod( "DebugGC_MemoryFreeBlock_ZeroFreeMemory", false, delegate()
            {
                m_suspendCount++;

                m_ih.SetTemporaryInteropOnReturn( delegate()
                {
                    m_suspendCount--;

                    return Emulation.Hosting.Interop.CallbackResponse.DoNothing;
                } );

                return Emulation.Hosting.Interop.CallbackResponse.DoNothing;
            } );

            m_ih.SetInteropOnWellKnownMethod( "DebugGC_MemoryFreeBlock_DirtyFreeMemory", false, delegate()
            {
                m_suspendCount++;

                m_ih.SetTemporaryInteropOnReturn( delegate()
                {
                    m_suspendCount--;

                    return Emulation.Hosting.Interop.CallbackResponse.DoNothing;
                } );

                return Emulation.Hosting.Interop.CallbackResponse.DoNothing;
            } );

            m_ih.SetInteropOnWellKnownMethod( "DebugGC_MemoryFreeBlock_InitializeFromRawMemory", false, delegate()
            {
                uint baseAddress = m_ih.GetRegisterUInt32( EncDef.c_register_r1 );
                uint sizeInBytes = m_ih.GetRegisterUInt32( EncDef.c_register_r2 );

                m_ih.SetTemporaryInteropOnReturn( delegate()
                {
                    AddRange( baseAddress, baseAddress + sizeInBytes );

                    return Emulation.Hosting.Interop.CallbackResponse.DoNothing;
                } );

                return Emulation.Hosting.Interop.CallbackResponse.DoNothing;
            } );

            m_ih.SetInteropOnWellKnownMethod( "DebugGC_ObjectHeader_InsertPlug", false, delegate()
            {
                uint pThis = m_ih.GetRegisterUInt32( EncDef.c_register_r0 );
                uint size  = m_ih.GetRegisterUInt32( EncDef.c_register_r1 );

                m_suspendCount++;

                m_ih.SetTemporaryInteropOnReturn( delegate()
                {
                    m_suspendCount--;

                    if(FindRange( m_freeRanges, pThis ) < 0)
                    {
                        AddRange( pThis, pThis + size );
                    }

                    return Emulation.Hosting.Interop.CallbackResponse.DoNothing;
                } );

                return Emulation.Hosting.Interop.CallbackResponse.DoNothing;
            } );

            if(m_fVerbose)
            { 
                m_ih.SetInteropOnWellKnownMethod( "DebugGC_DefaultTypeSystemManager_AllocateInner", false, delegate()
                {
                    uint vTable = m_ih.GetRegisterUInt32( EncDef.c_register_r1 );
                    uint size   = m_ih.GetRegisterUInt32( EncDef.c_register_r2 );

                    m_ih.SetTemporaryInteropOnReturn( delegate()
                    {
                        uint ptr = m_ih.GetRegisterUInt32( EncDef.c_register_r0 );

                        if(ptr != 0)
                        {
                            var td = m_memDelta.ImageInformation.GetTypeFromVirtualTable( vTable );

                            if(td == null)
                            {
                                ReportProblem( "Cannot decode vTable at {0:X8}", vTable );
                            }
                            else
                            {
                                ReportInfo( "NewObject: {0:X8} {1} bytes, {2}", ptr, size, td.FullNameWithAbbreviation );
                            }
                        }

                        return Emulation.Hosting.Interop.CallbackResponse.DoNothing;
                    } );

                    return Emulation.Hosting.Interop.CallbackResponse.DoNothing;
                } );
            }

            m_ih.SetInteropOnWellKnownMethod( "DebugGC_MemoryFreeBlock_Allocate", false, delegate()
            {
                uint size = m_ih.GetRegisterUInt32( EncDef.c_register_r2 );

                m_suspendCount++;

                var freeRangesOld = new List< ProhibitedRange >( m_freeRanges.ToArray() );

                m_ih.SetTemporaryInteropOnReturn( delegate()
                {
                    m_suspendCount--;

                    uint ptr = m_ih.GetRegisterUInt32( EncDef.c_register_r0 );

                    if(ptr != 0)
                    {
                        int pos;

                        pos = FindRange( freeRangesOld, ptr );
                        if(pos < 0)
                        {
                            ReportProblem( "Allocated memory from a non-free range: 0x{0:X8}, {1} bytes", ptr, size );
                        }

                        pos = FindRange( m_freeRanges, ptr );
                        if(pos >= 0)
                        {
                            ProhibitedRange rng = m_freeRanges[pos];

                            if(rng.m_addressEnd == ptr + size)
                            {
                                rng.m_addressEnd = ptr;
                            }
                            else
                            {
                                ReportProblem( "Memory still free after allocation: 0x{0:X8}, {1} bytes", ptr, size );
                            }
                        }
                    }

                    return Emulation.Hosting.Interop.CallbackResponse.DoNothing;
                } );

                return Emulation.Hosting.Interop.CallbackResponse.DoNothing;
            } );

#if DEBUG_BRICKTABLE
            m_ih.SetInteropOnWellKnownMethod( "DebugBrickTable_VerifyBrickTable", false, delegate()
            {
                ReportInfo( "Verify" );

                m_ih.SetTemporaryInteropOnReturn( delegate()
                {
                    ReportInfo( "Verify Done" );

                    return Emulation.Hosting.Interop.CallbackResponse.DoNothing;
                } );

                return Emulation.Hosting.Interop.CallbackResponse.DoNothing;
            } );

            m_ih.SetInteropOnWellKnownMethod( "DebugBrickTable_Reset", false, delegate()
            {
                ReportInfo( "Reset" );

                return Emulation.Hosting.Interop.CallbackResponse.DoNothing;
            } );

            m_ih.SetInteropOnWellKnownMethod( "DebugBrickTable_MarkObject", false, delegate()
            {
                uint objectPtr  = m_ih.GetRegisterUInt32( EncodingDefinition.c_register_r1 );
                uint objectSize = m_ih.GetRegisterUInt32( EncodingDefinition.c_register_r2 );

                ReportInfo( "Mark   {0:X8}[{2:X8}] {1}", objectPtr, objectSize, objectPtr - 0x080008AC );

                return Emulation.Hosting.Interop.CallbackResponse.DoNothing;
            } );

            m_ih.SetInteropOnWellKnownMethod( "DebugBrickTable_FindLowerBoundForObjectPointer", false, delegate()
            {
                uint interiorPtr = m_ih.GetRegisterUInt32( EncodingDefinition.c_register_r1 );

                m_ih.SetTemporaryInteropOnReturn( delegate()
                {
                    uint res = m_ih.GetRegisterUInt32( EncodingDefinition.c_register_r0 );

                    ReportInfo( "Find   {0:X8}[{2:X8}] {1:X8}", interiorPtr, res, interiorPtr - 0x080008AC );

                    return Emulation.Hosting.Interop.CallbackResponse.DoNothing;
                } );

                return Emulation.Hosting.Interop.CallbackResponse.DoNothing;
            } );
#endif
        }

        //--//

        private void CheckAccess( uint address ,
                                  bool fDirty  )
        {
            if(m_suspendCount == 0 && this.m_simulator.AreTimingUpdatesEnabled)
            {
                int pos = FindRange( m_freeRanges, address );

                if(pos >= 0)
                {
                    if(fDirty)
                    {
                        // It's OK to overwrite a free block with dirty patterns.
                    }
                    else
                    {
                        ProhibitedRange rng = m_freeRanges[pos];

////                    if(CheckStackTrace( "HeapInitialization" ))
////                    {
////                        return;
////                    }

                        ReportProblem( "Bad access: 0x{0:X8}", address );
                    }
                }
            }
        }

        private void RemoveRange( uint addressStart ,
                                  uint addressEnd   )
        {
            while(true)
            {
                int pos = FindRange( m_freeRanges, addressStart, addressEnd );
                if(pos < 0)
                {
                    return;
                }

                ProhibitedRange rng           = m_freeRanges[pos];
                bool            fChunkOnLeft  = rng.m_addressStart < addressStart;
                bool            fChunkOnRight = rng.m_addressEnd   > addressEnd;

                if(fChunkOnLeft)
                {
                    if(fChunkOnRight)
                    {
                        var rng2 = new ProhibitedRange( addressEnd, rng.m_addressEnd );

                        m_freeRanges.Insert( pos + 1, rng2 );

                        rng.m_addressEnd = addressStart;
                    }
                    else
                    {
                        rng.m_addressEnd = addressStart;
                    }
                }
                else
                {
                    if(fChunkOnRight)
                    {
                        rng.m_addressStart = addressStart;
                    }
                    else
                    {
                        m_freeRanges.RemoveAt( pos );
                    }
                }
            }
        }

        private void AddRange( uint addressStart ,
                               uint addressEnd   )
        {
            RemoveRange( addressStart, addressEnd );

            int pos = FindRange( m_freeRanges, addressStart, addressEnd );

            if(pos < 0)
            {
                pos = ~pos;

                ProhibitedRange rng = new ProhibitedRange( addressStart, addressEnd );

                m_freeRanges.Insert( pos, rng );

                if(pos > 0)
                {
                    ProhibitedRange rngPre = m_freeRanges[pos-1];
 
                    if(rngPre.m_addressEnd == rng.m_addressStart)
                    {
                        rngPre.m_addressEnd = rng.m_addressEnd;
                        m_freeRanges.RemoveAt( pos );

                        rng = rngPre;
                        pos--;
                    }
                }

                while(pos < m_freeRanges.Count - 1)
                {
                    ProhibitedRange rngPost = m_freeRanges[pos+1];

                    if(rngPost.m_addressStart > rng.m_addressEnd)
                    {
                        break;
                    }

                    rng.m_addressEnd = rngPost.m_addressEnd;
                    m_freeRanges.RemoveAt( pos+1 );
                }
            }
            else
            {
                ReportProblem( "Already a free range: 0x{0:X8}-0x{1:X8}", addressStart, addressEnd );
            }
        }

        private bool CheckStackTrace( params string[] methods )
        {
            m_memDelta.FlushCache();

            ThreadStatus ts = ThreadStatus.GetCurrent( m_memDelta );

            foreach(StackFrame sf in ts.StackTrace)
            {
                var methodName = sf.Method.ToShortString();

                foreach(var method in methods)
                {
                    if(methodName.Contains( method ))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void ReportProblem(        string   fmt   ,
                                    params object[] parms )
        {
            string issue = string.Format( fmt, parms );

            m_memDelta.FlushCache();

            ThreadStatus ts = ThreadStatus.GetCurrent( m_memDelta );

            Emulation.Hosting.OutputSink sink;
           
            if(m_memDelta.Host.GetHostingService( out sink ))
            {
                sink.OutputLine( issue );

                foreach(StackFrame sf in ts.StackTrace)
                {
                    sink.OutputLine( "#### {0}", sf );
                }
            }

            Emulation.Hosting.ProcessorControl svcPC; m_memDelta.Host.GetHostingService( out svcPC );

            svcPC.StopExecution = true;
        }

        private void ReportInfo(        string   fmt   ,
                                 params object[] parms )
        {
            string issue = string.Format( fmt, parms );

            Emulation.Hosting.OutputSink sink;
           
            if(m_memDelta.Host.GetHostingService( out sink ))
            {
                sink.OutputLine( issue );
            }
        }

        private static int FindRange( List< ProhibitedRange > freeRanges ,
                                      uint                    address    )
        {
            return FindRange( freeRanges, address, address + 1 );
        }

        private static int FindRange( List< ProhibitedRange > freeRanges   ,
                                      uint                    addressStart ,
                                      uint                    addressEnd   )
        {
            int lo = 0;
            int hi = freeRanges.Count - 1;

            while(lo <= hi)
            {
                int mid   = (lo + hi) / 2;
                int order = freeRanges[mid].Compare( addressStart, addressEnd );

                if(order < 0)
                {
                    lo = mid + 1;
                }
                else if(order > 0)
                {
                    hi = mid - 1;
                }
                else
                {
                    return mid;
                }
            }

            return ~lo;
        }
    }
}