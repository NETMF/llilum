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

using System;
using System.ComponentModel.Design;
using System.Globalization;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.MIEngine.CoreRegisters.ARM
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class CoreRegistersWindowCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid( "8d5e2453-09b4-41bf-aa8d-7b6e1602a1c7" );

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        private IVsMonitorSelection SelectionService;
        private uint DebuggingContextCookie;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreRegistersWindowCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        [SuppressMessage( "Warning","VSSDK002", Justification = "Checked via ThreadHelper.ThrowIfNotOnUIThread()")]
        private CoreRegistersWindowCommand( Package package )
        {
            if( package == null )
            {
                throw new ArgumentNullException( nameof( package ) );
            }

            this.package = package;

            ThreadHelper.ThrowIfNotOnUIThread( );
            SelectionService = ( IVsMonitorSelection )Package.GetGlobalService( typeof( SVsShellMonitorSelection ) );
            if( SelectionService == null )
                return;

            OleMenuCommandService commandService = ServiceProvider.GetService( typeof( IMenuCommandService ) ) as OleMenuCommandService;
            if( commandService == null )
                return;

            var menuCommandID = new CommandID( CommandSet, CommandId );
            var menuItem = new OleMenuCommand( ShowToolWindow, menuCommandID );
            commandService.AddCommand( menuItem );

            menuItem.BeforeQueryStatus += MenuItem_BeforeQueryStatus;
            Guid tempGuid = VSConstants.UICONTEXT.Debugging_guid;
            ErrorHandler.ThrowOnFailure( SelectionService.GetCmdUIContextCookie( ref tempGuid, out DebuggingContextCookie ) );
        }

        [SuppressMessage( "Warning", "VSSDK002", Justification = "UI thread callback, always on UI" )]
        private void MenuItem_BeforeQueryStatus( object sender, EventArgs e )
        {
            var cmd = ( OleMenuCommand )sender;
            int active = 0;
            ErrorHandler.ThrowOnFailure( SelectionService.IsCmdUIContextActive( DebuggingContextCookie, out active ) );
            cmd.Visible = active != 0;
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static CoreRegistersWindowCommand Instance { get; private set; }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider => package;

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize( Package package )
        {
            Instance = new CoreRegistersWindowCommand( package );
        }

        /// <summary>
        /// Shows the tool window when the menu item is clicked.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        [SuppressMessage( "Warning", "VSSDK002", Justification = "UI thread callback, always on UI" )]
        private void ShowToolWindow( object sender, EventArgs e )
        {
            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            ToolWindowPane window = package.FindToolWindow( typeof( CoreRegistersWindow ), 0, true );
            if( ( null == window ) || ( null == window.Frame ) )
            {
                throw new NotSupportedException( "Cannot create tool window" );
            }

            IVsWindowFrame windowFrame = ( IVsWindowFrame )window.Frame;
            ErrorHandler.ThrowOnFailure( windowFrame.Show( ) );
        }
    }
}
