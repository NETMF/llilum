// The MIT License( MIT)
// 
// Copyright( c) 2015 Microsoft
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using Microsoft.MIDebugEngine;
using Microsoft.MIEngine.Extensions;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace Microsoft.MIEngine.CoreRegisters.ARM
{
    internal sealed class CoreRegistersViewModel
        : INotifyPropertyChanged
        , IVsDebuggerEvents
        , IDisposable
    {
        [SuppressMessage("Warning", "VSSDK002", Justification = "Asserted via ThreadHelper.ThrowIfNotOnUIThread( )")]
        internal CoreRegistersViewModel( IVsDebugger debugger )
        {
            if( debugger == null )
                throw new ArgumentNullException( nameof( debugger ) );

            Registers = new List<CoreRegisterViewModel>
                { new CoreRegisterViewModel("R0",        "Core",   0)
                , new CoreRegisterViewModel("R1",        "Core",   1)
                , new CoreRegisterViewModel("R2",        "Core",   2)
                , new CoreRegisterViewModel("R3",        "Core",   3)
                , new CoreRegisterViewModel("R4",        "Core",   4)
                , new CoreRegisterViewModel("R5",        "Core",   5)
                , new CoreRegisterViewModel("R6",        "Core",   6)
                , new CoreRegisterViewModel("R7",        "Core",   7)
                , new CoreRegisterViewModel("R8",        "Core",   8)
                , new CoreRegisterViewModel("R9",        "Core",   9)
                , new CoreRegisterViewModel("R10",       "Core",   10)
                , new CoreRegisterViewModel("R11",       "Core",   11)
                , new CoreRegisterViewModel("R12",       "Core",   12)
                , new CoreRegisterViewModel("R13(SP)",   "Core",   13)
                , new CoreRegisterViewModel("R14(LR)",   "Core",   14)
                , new CoreRegisterViewModel("R15(PC)",   "Core",   15)
                , new CoreRegisterViewModel("xpsr",      "Core",   0x19, (r)=>new XpsrRegisterDetailsViewModel(r) )
                , new CoreRegisterViewModel("msp",       "Banked", 0x5B)
                , new CoreRegisterViewModel("psp",       "Banked", 0x5C)
                , new CoreRegisterViewModel("primask",   "System", 0x5D)
                , new CoreRegisterViewModel("control",   "System", 0x5E)
                , new CoreRegisterViewModel("basepri",   "System", 0x5F)
                , new CoreRegisterViewModel("faultmask", "System", 0x60)
                };

            RegIdToViewModelMap = Registers.ToDictionary( vm => vm.Id );
            ThreadHelper.ThrowIfNotOnUIThread( );
            Debugger = debugger;
            Debugger.AdviseDebuggerEvents( this, out DebuggerEventsSinkCookie );
        }

        public string DebugMode { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged = delegate {};

        public List<CoreRegisterViewModel> Registers { get; }

        public void Dispose( )
        {
            ThreadHelper.ThrowIfNotOnUIThread( );
            Debugger.UnadviseDebuggerEvents( DebuggerEventsSinkCookie );
        }

        int IVsDebuggerEvents.OnModeChange( DBGMODE dbgmodeNew )
        {
            // Push actual handler activity to the UI thread, if not already there
            // Run Async is needed since this is an interface defined method that
            // isn't async aware. 
            ThreadHelper.JoinableTaskFactory.RunAsync( async ()=>
                {
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync( );
                    DebugMode = dbgmodeNew.ToString( );
                    PropertyChanged( this, DebugModeChangedEventArgs );
                    if( dbgmodeNew != DBGMODE.DBGMODE_Break )
                        return;
                    
                    foreach( var knownReg in RegIdToViewModelMap.Values )
                        knownReg.IsChanged = false;

                    var registers = await GetUpdatedRegistersAsync( );
                    foreach( var reg in registers )
                        RegIdToViewModelMap[ reg.Id ].Value = reg.Value;
                });

            return VSConstants.S_OK;
        }

        internal async Task< IReadOnlyList<RegisterIdValuePair> > GetUpdatedRegistersAsync( )
        {
#if DEBUG
            await VerifyRegisterIds( );
#endif
            string cmdResult = await MIDebugCommandDispatcher.ExecuteCommand( ChangedRegistersResult.Command );
            var regChangedResult = await ChangedRegistersResult.ParseAsync( cmdResult );
            // if( regChangedResult.Status != ResultStatus.Done )
            //     TODO: How should errors be reported?

            // send a request for the values of the changed registers
            var cmd = RegisterValuesResult.GetCommand( regChangedResult.Registers );
            cmdResult = await MIDebugCommandDispatcher.ExecuteCommand( cmd );
            var regValues = await RegisterValuesResult.ParseAsync( cmdResult );
            //if( regValues.Status != ResultStatus.Done )
            //    TODO: How should errors be reported?

            return regValues.Registers;
        }

        private readonly Dictionary<int, CoreRegisterViewModel> RegIdToViewModelMap;
        private readonly IVsDebugger Debugger;
        private readonly uint DebuggerEventsSinkCookie;
        private static readonly PropertyChangedEventArgs DebugModeChangedEventArgs = new PropertyChangedEventArgs( nameof( DebugMode ) );

#if DEBUG
        // request all register names and ids, validate that all core registers
        // have a matching view model
        private async Task VerifyRegisterIds( )
        {
            if( VerifiedRegisterIds )
                return;

            string cmdResult = await MIDebugCommandDispatcher.ExecuteCommand( RegisterNamesResult.Command );
            var result = await RegisterNamesResult.ParseAsync( cmdResult );
            if( result.Status != ResultStatus.Done )
                return;

            foreach( var regNameId in result.Names )
            {
                Debug.Assert( RegIdToViewModelMap.ContainsKey( regNameId.Id ) );
            }
            VerifiedRegisterIds = true;
        }

        bool VerifiedRegisterIds;
#endif
    }
}
