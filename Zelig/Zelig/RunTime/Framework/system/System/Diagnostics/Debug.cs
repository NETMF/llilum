//------------------------------------------------------------------------------
// <copyright file="Debug.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
//#define DEBUG
namespace System.Diagnostics
{
    using System;
    using System.Collections;
    using System.Security.Permissions;
    using System.Globalization;

    /// <devdoc>
    ///    <para>Provides a set of properties and
    ///       methods
    ///       for debugging code.</para>
    /// </devdoc>
    public static class Debug
    {

        /// <devdoc>
        ///    <para>Gets
        ///       the collection of listeners that is monitoring the debug
        ///       output.</para>
        /// </devdoc>
        public static TraceListenerCollection Listeners
        {
            [SecurityPermission( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
            [HostProtection( SharedState = true )]
            get
            {
                return TraceInternal.Listeners;
            }
        }

        /// <devdoc>
        /// <para>Gets or sets a value indicating whether <see cref='System.Diagnostics.Debug.Flush'/> should be called on the
        /// <see cref='System.Diagnostics.Debug.Listeners'/>
        /// after every write.</para>
        /// </devdoc>
        public static bool AutoFlush
        {
            [SecurityPermission( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
            get
            {
                return TraceInternal.AutoFlush;
            }

            [SecurityPermission( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
            set
            {
                TraceInternal.AutoFlush = value;
            }
        }

        /// <devdoc>
        ///    <para>Gets or sets
        ///       the indent level.</para>
        /// </devdoc>
        public static int IndentLevel
        {
            get
            {
                return TraceInternal.IndentLevel;
            }

            set
            {
                TraceInternal.IndentLevel = value;
            }
        }

        /// <devdoc>
        ///    <para>Gets or sets the number of spaces in an indent.</para>
        /// </devdoc>
        public static int IndentSize
        {
            get
            {
                return TraceInternal.IndentSize;
            }

            set
            {
                TraceInternal.IndentSize = value;
            }
        }

        /// <devdoc>
        ///    <para>Clears the output buffer, and causes buffered data to
        ///       be written to the <see cref='System.Diagnostics.Debug.Listeners'/>.</para>
        /// </devdoc>
        [System.Diagnostics.Conditional( "DEBUG" )]
        public static void Flush( )
        {
            TraceInternal.Flush( );
        }

        /// <devdoc>
        ///    <para>Clears the output buffer, and then closes the <see cref='System.Diagnostics.Debug.Listeners'/> so that they no longer receive
        ///       debugging output.</para>
        /// </devdoc>
        [System.Diagnostics.Conditional( "DEBUG" )]
        [SecurityPermission( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
        public static void Close( )
        {
            TraceInternal.Close( );
        }

        /// <devdoc>
        /// <para>Checks for a condition, and outputs the callstack if the condition is <see langword='false'/>.</para>
        /// </devdoc>
        [System.Diagnostics.Conditional( "DEBUG" )]
        public static void Assert( bool condition )
        {
            if(condition == false)
            {
                throw new ArgumentException();
            }
            TraceInternal.Assert( condition );
        }

        /// <devdoc>
        ///    <para>Checks for a condition, and displays a message if the condition is
        ///    <see langword='false'/>. </para>
        /// </devdoc>
        [System.Diagnostics.Conditional( "DEBUG" )]
        public static void Assert( bool   condition ,
                                   string message   )
        {
            if(condition == false)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentException( message );
#else
                throw new ArgumentException();
#endif
            }
            TraceInternal.Assert( condition, message );
        }

        /// <devdoc>
        ///    <para>Checks for a condition, and displays both the specified messages if the condition
        ///       is <see langword='false'/>. </para>
        /// </devdoc>
        [System.Diagnostics.Conditional( "DEBUG" )]
        public static void Assert( bool   condition     ,
                                   string message       ,
                                   string detailMessage )
        {
            if(condition == false)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentException( message );
#else
                throw new ArgumentException();
#endif
            }
            TraceInternal.Assert( condition, message, detailMessage );
        }

        /// <devdoc>
        ///    <para>Emits or displays a message for an assertion that always fails.</para>
        /// </devdoc>
        [System.Diagnostics.Conditional( "DEBUG" )]
        public static void Fail( string message )
        {
            TraceInternal.Fail( message );
        }

        /// <devdoc>
        ///    <para>Emits or displays both messages for an assertion that always fails.</para>
        /// </devdoc>
        [System.Diagnostics.Conditional( "DEBUG" )]
        public static void Fail( string message       ,
                                 string detailMessage )
        {
            TraceInternal.Fail( message, detailMessage );
        }

        [System.Diagnostics.Conditional( "DEBUG" )]
        public static void Print( string message )
        {
            TraceInternal.WriteLine( message );
        }
    
        [System.Diagnostics.Conditional( "DEBUG" )]
        public static void Print(        string   format ,
                                  params object[] args   )
        {
            TraceInternal.WriteLine( String.Format( CultureInfo.InvariantCulture, format, args ) );
        }

        /// <devdoc>
        ///    <para>Writes a message to the trace listeners in the <see cref='System.Diagnostics.Debug.Listeners'/> collection.</para>
        /// </devdoc>
        [System.Diagnostics.Conditional( "DEBUG" )]
        public static void Write( string message )
        {
            TraceInternal.Write( message );
        }

        /// <devdoc>
        ///    <para>Writes the name of the value 
        ///       parameter to the trace listeners in the <see cref='System.Diagnostics.Debug.Listeners'/> collection.</para>
        /// </devdoc>
        [System.Diagnostics.Conditional( "DEBUG" )]
        public static void Write( object value )
        {
            TraceInternal.Write( value );
        }

        /// <devdoc>
        ///    <para>Writes a category name and message 
        ///       to the trace listeners in the <see cref='System.Diagnostics.Debug.Listeners'/> collection.</para>
        /// </devdoc>
        [System.Diagnostics.Conditional( "DEBUG" )]
        public static void Write( string message  ,
                                  string category )
        {
            TraceInternal.Write( message, category );
        }
    
        /// <devdoc>
        ///    <para>Writes a category name and the name of the value parameter to the trace
        ///       listeners in the <see cref='System.Diagnostics.Debug.Listeners'/> collection.</para>
        /// </devdoc>
        [System.Diagnostics.Conditional( "DEBUG" )]
        public static void Write( object value    ,
                                  string category )
        {
            TraceInternal.Write( value, category );
        }
    
        /// <devdoc>
        ///    <para>Writes a message followed by a line terminator to the trace listeners in the
        ///    <see cref='System.Diagnostics.Debug.Listeners'/> collection. The default line terminator 
        ///       is a carriage return followed by a line feed (\r\n).</para>
        /// </devdoc>
        [System.Diagnostics.Conditional( "DEBUG" )]
        public static void WriteLine( string message )
        {
            TraceInternal.WriteLine( message );
        }
    
        /// <devdoc>
        ///    <para>Writes the name of the value 
        ///       parameter followed by a line terminator to the
        ///       trace listeners in the <see cref='System.Diagnostics.Debug.Listeners'/> collection. The default line
        ///       terminator is a carriage return followed by a line feed (\r\n).</para>
        /// </devdoc>
        [System.Diagnostics.Conditional( "DEBUG" )]
        public static void WriteLine( object value )
        {
            TraceInternal.WriteLine( value );
        }
    
        /// <devdoc>
        ///    <para>Writes a category name and message followed by a line terminator to the trace
        ///       listeners in the <see cref='System.Diagnostics.Debug.Listeners'/> collection. The default line
        ///       terminator is a carriage return followed by a line feed (\r\n).</para>
        /// </devdoc>
        [System.Diagnostics.Conditional( "DEBUG" )]
        public static void WriteLine( string message  ,
                                      string category )
        {
            TraceInternal.WriteLine( message, category );
        }
    
        /// <devdoc>
        ///    <para>Writes a category name and the name of the value 
        ///       parameter followed by a line
        ///       terminator to the trace listeners in the <see cref='System.Diagnostics.Debug.Listeners'/> collection. The
        ///       default line terminator is a carriage return followed by a line feed (\r\n).</para>
        /// </devdoc>
        [System.Diagnostics.Conditional( "DEBUG" )]
        public static void WriteLine( object value    ,
                                      string category )
        {
            TraceInternal.WriteLine( value, category );
        }
    
        /// <devdoc>
        /// <para>Writes a message to the trace listeners in the <see cref='System.Diagnostics.Debug.Listeners'/> collection 
        ///    if a condition is
        /// <see langword='true'/>. </para>
        /// </devdoc>
        [System.Diagnostics.Conditional( "DEBUG" )]
        public static void WriteIf( bool   condition ,
                                    string message   )
        {
            TraceInternal.WriteIf( condition, message );
        }
    
        /// <devdoc>
        ///    <para>Writes the name of the value 
        ///       parameter to the trace listeners in the <see cref='System.Diagnostics.Debug.Listeners'/>
        ///       collection if a condition is
        ///    <see langword='true'/>. </para>
        /// </devdoc>
        [System.Diagnostics.Conditional( "DEBUG" )]
        public static void WriteIf( bool   condition ,
                                    object value     )
        {
            TraceInternal.WriteIf( condition, value );
        }
    
        /// <devdoc>
        ///    <para>Writes a category name and message 
        ///       to the trace listeners in the <see cref='System.Diagnostics.Debug.Listeners'/>
        ///       collection if a condition is
        ///    <see langword='true'/>. </para>
        /// </devdoc>
        [System.Diagnostics.Conditional( "DEBUG" )]
        public static void WriteIf( bool   condition ,
                                    string message   ,
                                    string category  )
        {
            TraceInternal.WriteIf( condition, message, category );
        }
    
        /// <devdoc>
        ///    <para>Writes a category name and the name of the value 
        ///       parameter to the trace
        ///       listeners in the <see cref='System.Diagnostics.Debug.Listeners'/> collection if a condition is
        ///    <see langword='true'/>. </para>
        /// </devdoc>
        [System.Diagnostics.Conditional( "DEBUG" )]
        public static void WriteIf( bool   condition ,
                                    object value     ,
                                    string category  )
        {
            TraceInternal.WriteIf( condition, value, category );
        }
    
        /// <devdoc>
        ///    <para>Writes a message followed by a line terminator to the trace listeners in the
        ///    <see cref='System.Diagnostics.Debug.Listeners'/> collection if a condition is 
        ///    <see langword='true'/>. The default line terminator is a carriage return followed 
        ///       by a line feed (\r\n).</para>
        /// </devdoc>
        [System.Diagnostics.Conditional( "DEBUG" )]
        public static void WriteLineIf( bool   condition ,
                                        string message   )
        {
            TraceInternal.WriteLineIf( condition, message );
        }
    
        /// <devdoc>
        ///    <para>Writes the name of the value 
        ///       parameter followed by a line terminator to the
        ///       trace listeners in the <see cref='System.Diagnostics.Debug.Listeners'/> collection if a condition is
        ///    <see langword='true'/>. The default line terminator is a carriage return followed 
        ///       by a line feed (\r\n).</para>
        /// </devdoc>
        [System.Diagnostics.Conditional( "DEBUG" )]
        public static void WriteLineIf( bool   condition ,
                                        object value     )
        {
            TraceInternal.WriteLineIf( condition, value );
        }
    
        /// <devdoc>
        ///    <para>Writes a category name and message
        ///       followed by a line terminator to the trace
        ///       listeners in the <see cref='System.Diagnostics.Debug.Listeners'/> collection if a condition is
        ///    <see langword='true'/>. The default line terminator is a carriage return followed 
        ///       by a line feed (\r\n).</para>
        /// </devdoc>
        [System.Diagnostics.Conditional( "DEBUG" )]
        public static void WriteLineIf( bool   condition ,
                                        string message   ,
                                        string category  )
        {
            TraceInternal.WriteLineIf( condition, message, category );
        }
    
        /// <devdoc>
        ///    <para>Writes a category name and the name of the value parameter followed by a line
        ///       terminator to the trace listeners in the <see cref='System.Diagnostics.Debug.Listeners'/> collection
        ///       if a condition is <see langword='true'/>. The default line terminator is a carriage
        ///       return followed by a line feed (\r\n).</para>
        /// </devdoc>
        [System.Diagnostics.Conditional( "DEBUG" )]
        public static void WriteLineIf( bool   condition ,
                                        object value     ,
                                        string category  )
        {
            TraceInternal.WriteLineIf( condition, value, category );
        }
    
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [System.Diagnostics.Conditional( "DEBUG" )]
        public static void Indent()
        {
            TraceInternal.Indent();
        }
    
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [System.Diagnostics.Conditional( "DEBUG" )]
        public static void Unindent()
        {
            TraceInternal.Unindent();
        }
    }
}
