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

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Microsoft.MIEngine.CoreRegisters.ARM
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid( "43a63d28-8723-4b55-9671-93597c2e78e5" )]
    public class CoreRegistersWindow 
        : ToolWindowPane
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreRegistersWindow"/> class.
        /// </summary>
        [SuppressMessage( "Warning", "VSSDK002", Justification = "Asserted via ThreadHelper.ThrowIfNotOnUIThread( )" )]
        public CoreRegistersWindow( IServiceProvider svcProvider )
            : base( svcProvider )
        {
            Caption = "CoreRegistersWindow";

            ThreadHelper.ThrowIfNotOnUIThread( );
            var debugger = ( IVsDebugger )GetService( typeof( SVsShellDebugger ) );
            Debug.Assert( debugger != null );

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property. Setting it on a private field and overriding the 
            // Content property avoids the problem of calling a virtual in the constructor.
            ContentControl = new CoreRegistersWindowControl( new CoreRegistersViewModel( debugger ) );
        }

        public override object Content
        {
            get
            {
                if( base.Content == null )
                    base.Content = ContentControl;

                return base.Content;
            }

            set
            {
                base.Content = value;
            }
        }

        private readonly CoreRegistersWindowControl ContentControl;
    }
}
