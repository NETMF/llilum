// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*=============================================================================
**
** Class: Console
**
**
** Purpose: This class provides access to the standard input, standard output
**          and standard error streams.
**
**
=============================================================================*/
namespace System
{
    using System.Security.Permissions;
    ////using Microsoft.Win32;
    using System.Runtime.CompilerServices;
    ////using Microsoft.Win32.SafeHandles;
    ////using System.Runtime.Versioning;

    // Provides static fields for console input & output.  Use 
    // Console.In for input from the standard input stream (stdin),
    // Console.Out for output to stdout, and Console.Error
    // for output to stderr.  If any of those console streams are 
    // redirected from the command line, these streams will be redirected.
    // A program can also redirect its own output or input with the 
    // SetIn, SetOut, and SetError methods.
    // 
    // The distinction between Console.Out & Console.Error is useful
    // for programs that redirect output to a file or a pipe.  Note that
    // stdout & stderr can be output to different files at the same
    // time from the DOS command line:
    // 
    // someProgram 1> out 2> err
    // 
    //Contains only static data.  Serializable attribute not required.
    public static class Console
    {
////    private const int _DefaultConsoleBufferSize = 256;
////    private const short AltVKCode = 0x12;
////
////    private static TextReader _in;
////    private static TextWriter _out;
////    private static TextWriter _error;
////
////    private static ConsoleCancelEventHandler _cancelCallbacks;
////    private static ControlCHooker _hooker;
////
////    private static bool _wasOutRedirected;
////    private static bool _wasErrorRedirected;
////
////    // Private object for locking instead of locking on a public type for SQL reliability work.
////    private static Object s_InternalSyncObject;
////    private static Object InternalSyncObject
////    {
////        get
////        {
////            if(s_InternalSyncObject == null)
////            {
////                Object o = new Object();
////                Interlocked.CompareExchange( ref s_InternalSyncObject, o, null );
////            }
////            return s_InternalSyncObject;
////        }
////    }
////
////    // About reliability: I'm not using SafeHandle here.  We don't 
////    // need to close these handles, and we don't allow the user to close
////    // them so we don't have many of the security problems inherent in
////    // something like file handles.  Additionally, in a host like SQL 
////    // Server, we won't have a console.
////    private static IntPtr _consoleInputHandle;
////    private static IntPtr _consoleOutputHandle;
////
////    private static IntPtr ConsoleInputHandle
////    {
////        [ResourceExposure( ResourceScope.Process )]
////        [ResourceConsumption( ResourceScope.Process )]
////        get
////        {
////            if(_consoleInputHandle == IntPtr.Zero)
////            {
////                _consoleInputHandle = Win32Native.GetStdHandle( Win32Native.STD_INPUT_HANDLE );
////            }
////            return _consoleInputHandle;
////        }
////    }
////
////    private static IntPtr ConsoleOutputHandle
////    {
////        [ResourceExposure( ResourceScope.Process )]
////        [ResourceConsumption( ResourceScope.Process )]
////        get
////        {
////            if(_consoleOutputHandle == IntPtr.Zero)
////            {
////                _consoleOutputHandle = Win32Native.GetStdHandle( Win32Native.STD_OUTPUT_HANDLE );
////            }
////            return _consoleOutputHandle;
////        }
////    }
////
////    public static TextWriter Error
////    {
////        [HostProtection( UI = true )]
////        get
////        {
////            // Hopefully this is inlineable.
////            if(_error == null)
////                InitializeStdOutError( false );
////            return _error;
////        }
////    }
////
////    public static TextReader In
////    {
////        [HostProtection( UI = true )]
////        [ResourceExposure( ResourceScope.None )]
////        [ResourceConsumption( ResourceScope.Process, ResourceScope.Process )]
////        get
////        {
////            // Because most applications don't use stdin, we can delay 
////            // initialize it slightly better startup performance.
////            if(_in == null)
////            {
////                lock(InternalSyncObject)
////                {
////                    if(_in == null)
////                    {
////                        // Set up Console.In
////                        Stream s = OpenStandardInput( _DefaultConsoleBufferSize );
////                        TextReader tr;
////                        if(s == Stream.Null)
////                            tr = StreamReader.Null;
////                        else
////                        {
////                            // Hopefully Encoding.GetEncoding doesn't load as many classes now.
////                            Encoding enc = Encoding.GetEncoding( (int)Win32Native.GetConsoleCP() );
////                            tr = TextReader.Synchronized( new StreamReader( s, enc, false, _DefaultConsoleBufferSize, false ) );
////                        }
////                        System.Threading.Thread.MemoryBarrier();
////                        _in = tr;
////                    }
////                }
////            }
////            return _in;
////        }
////    }
////
////    public static TextWriter Out
////    {
////        [HostProtection( UI = true )]
////        [ResourceExposure( ResourceScope.None )]
////        [ResourceConsumption( ResourceScope.Process, ResourceScope.Process )]
////        get
////        {
////            // Hopefully this is inlineable.
////            if(_out == null)
////                InitializeStdOutError( true );
////            return _out;
////        }
////    }
////
////    // For console apps, the console handles are set to values like 3, 7, 
////    // and 11 OR if you've been created via CreateProcess, possibly -1
////    // or 0.  -1 is definitely invalid, while 0 is probably invalid.
////    // Also note each handle can independently be invalid or good.
////    // For Windows apps, the console handles are set to values like 3, 7, 
////    // and 11 but are invalid handles - you may not write to them.  However,
////    // you can still spawn a Windows app via CreateProcess and read stdout
////    // and stderr.
////    // So, we always need to check each handle independently for validity
////    // by trying to write or read to it, unless it is -1.
////
////    // We do not do a security check here, under the assumption that this
////    // cannot create a security hole, but only waste a user's time or 
////    // cause a possible denial of service attack.
////    [ResourceExposure( ResourceScope.None )]
////    [ResourceConsumption( ResourceScope.Process, ResourceScope.Process )]
////    private static void InitializeStdOutError( bool stdout )
////    {
////        // Set up Console.Out or Console.Error.
////        lock(InternalSyncObject)
////        {
////            if(stdout && _out != null)
////                return;
////            else if(!stdout && _error != null)
////                return;
////
////            TextWriter writer = null;
////            Stream s;
////            if(stdout)
////                s = OpenStandardOutput( _DefaultConsoleBufferSize );
////            else
////                s = OpenStandardError( _DefaultConsoleBufferSize );
////
////            if(s == Stream.Null)
////            {
////                writer = TextWriter.Synchronized( StreamWriter.Null );
////            }
////            else
////            {
////                int codePage = (int)Win32Native.GetConsoleOutputCP();
////                Encoding encoding = Encoding.GetEncoding( codePage );
////                StreamWriter stdxxx = new StreamWriter( s, encoding, _DefaultConsoleBufferSize, false );
////                stdxxx.HaveWrittenPreamble = true;
////                stdxxx.AutoFlush = true;
////                writer = TextWriter.Synchronized( stdxxx );
////            }
////            if(stdout)
////                _out = writer;
////            else
////                _error = writer;
////            BCLDebug.Assert( (stdout && _out != null) || (!stdout && _error != null), "Didn't set Console::_out or _error appropriately!" );
////        }
////    }
////
////    // This method is only exposed via methods to get at the console.
////    // We won't use any security checks here.
////    [ResourceExposure( ResourceScope.Process )]
////    [ResourceConsumption( ResourceScope.Process )]
////    private static Stream GetStandardFile( int stdHandleName, FileAccess access, int bufferSize )
////    {
////        // We shouldn't close the handle for stdout, etc, or we'll break
////        // unmanaged code in the process that will print to console.
////        // We should have a better way of marking this on SafeHandle.
////        IntPtr handle = Win32Native.GetStdHandle( stdHandleName );
////        SafeFileHandle sh = new SafeFileHandle( handle, false );
////
////        // If someone launches a managed process via CreateProcess, stdout
////        // stderr, & stdin could independently be set to INVALID_HANDLE_VALUE.
////        // Additionally they might use 0 as an invalid handle.
////        if(sh.IsInvalid)
////        {
////            // Minor perf optimization - get it out of the finalizer queue.
////            sh.SetHandleAsInvalid();
////            return Stream.Null;
////        }
////
////        // Check whether we can read or write to this handle.
////        if(stdHandleName != Win32Native.STD_INPUT_HANDLE && !ConsoleHandleIsValid( sh ))
////        {
////            //BCLDebug.ConsoleError("Console::ConsoleHandleIsValid for std handle "+stdHandleName+" failed, setting it to a null stream");
////            return Stream.Null;
////        }
////
////        //BCLDebug.ConsoleError("Console::GetStandardFile for std handle "+stdHandleName+" succeeded, returning handle number "+handle.ToString());
////        Stream console = new __ConsoleStream( sh, access );
////        // Do not buffer console streams, or we can get into situations where
////        // we end up blocking waiting for you to hit enter twice.  It was
////        // redundant.  
////        return console;
////    }
////
////    public static Encoding InputEncoding
////    {
////        get
////        {
////            uint cp = Win32Native.GetConsoleCP();
////            return Encoding.GetEncoding( (int)cp );
////        }
////        [ResourceExposure( ResourceScope.Process )]
////        [ResourceConsumption( ResourceScope.Process )]
////        set
////        {
////            if(value == null)
////                throw new ArgumentNullException( "value" );
////
////            new UIPermission( UIPermissionWindow.SafeTopLevelWindows ).Demand();
////
////            uint cp = (uint)value.CodePage;
////
////            lock(InternalSyncObject)
////            {
////                bool r = Win32Native.SetConsoleCP( cp );
////                if(!r)
////                    __Error.WinIOError();
////
////                // We need to reinitialize Console.In in the next call to _in
////                // This will discard the current StreamReader, potentially 
////                // losing buffered data
////                _in = null;
////            }
////        }
////    }
////
////    public static Encoding OutputEncoding
////    {
////        get
////        {
////            uint cp = Win32Native.GetConsoleOutputCP();
////            return Encoding.GetEncoding( (int)cp );
////        }
////        [ResourceExposure( ResourceScope.Process )]
////        [ResourceConsumption( ResourceScope.Process )]
////        set
////        {
////            if(value == null)
////                throw new ArgumentNullException( "value" );
////
////            new UIPermission( UIPermissionWindow.SafeTopLevelWindows ).Demand();
////
////            lock(InternalSyncObject)
////            {
////                // Before changing the code page we need to flush the data 
////                // if Out hasn't been redirected. Also, have the next call to  
////                // _out reinitialize the console code page.
////
////                if(_out != null && !_wasOutRedirected)
////                {
////                    _out.Flush();
////                    _out = null;
////                }
////                if(_error != null && !_wasErrorRedirected)
////                {
////                    _error.Flush();
////                    _error = null;
////                }
////
////                uint cp = (uint)value.CodePage;
////                bool r = Win32Native.SetConsoleOutputCP( cp );
////                if(!r)
////                    __Error.WinIOError();
////            }
////        }
////    }
////
////    [HostProtection( UI = true )]
////    public static void Beep()
////    {
////        Beep( 800, 200 );
////    }
////
////    [HostProtection( UI = true )]
////    public static void Beep( int frequency, int duration )
////    {
////        if(frequency < MinBeepFrequency || frequency > MaxBeepFrequency)
////            throw new ArgumentOutOfRangeException( "frequency", frequency, Environment.GetResourceString( "ArgumentOutOfRange_BeepFrequency", MinBeepFrequency, MaxBeepFrequency ) );
////        if(duration <= 0)
////            throw new ArgumentOutOfRangeException( "duration", duration, Environment.GetResourceString( "ArgumentOutOfRange_NeedPosNum" ) );
////
////        // Note that Beep over Remote Desktop connections does not currently
////        // work.  Ignore any failures here.
////        Win32Native.Beep( frequency, duration );
////    }
////
////    [ResourceExposure( ResourceScope.Process )]
////    [ResourceConsumption( ResourceScope.Process )]
////    public static void Clear()
////    {
////        Win32Native.COORD coordScreen = new Win32Native.COORD();
////        Win32Native.CONSOLE_SCREEN_BUFFER_INFO csbi;
////        bool success;
////        int conSize;
////
////        IntPtr hConsole = ConsoleOutputHandle;
////        if(hConsole == Win32Native.INVALID_HANDLE_VALUE)
////            throw new IOException( Environment.GetResourceString( "IO.IO_NoConsole" ) );
////
////        // get the number of character cells in the current buffer
////        // Go through my helper method for fetching a screen buffer info
////        // to correctly handle default console colors.
////        csbi = GetBufferInfo();
////        conSize = csbi.dwSize.X * csbi.dwSize.Y;
////
////        // fill the entire screen with blanks
////
////        int numCellsWritten = 0;
////        success = Win32Native.FillConsoleOutputCharacter( hConsole, ' ',
////            conSize, coordScreen, out numCellsWritten );
////        if(!success)
////            __Error.WinIOError();
////
////        // now set the buffer's attributes accordingly
////
////        numCellsWritten = 0;
////        success = Win32Native.FillConsoleOutputAttribute( hConsole, csbi.wAttributes,
////            conSize, coordScreen, out numCellsWritten );
////        if(!success)
////            __Error.WinIOError();
////
////        // put the cursor at (0, 0)
////
////        success = Win32Native.SetConsoleCursorPosition( hConsole, coordScreen );
////        if(!success)
////            __Error.WinIOError();
////    }

        public static extern ConsoleColor BackgroundColor
        {
////        [ResourceExposure( ResourceScope.None )]
////        [ResourceConsumption( ResourceScope.Process, ResourceScope.Process )]
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;

////        [ResourceExposure( ResourceScope.Process )]
////        [ResourceConsumption( ResourceScope.Process )]
            [MethodImpl( MethodImplOptions.InternalCall )]
            set;
        }

        public static extern ConsoleColor ForegroundColor
        {
////        [ResourceExposure( ResourceScope.None )]
////        [ResourceConsumption( ResourceScope.Process, ResourceScope.Process )]
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;

////        [ResourceExposure( ResourceScope.Process )]
////        [ResourceConsumption( ResourceScope.Process )]
            [MethodImpl( MethodImplOptions.InternalCall )]
            set;
        }

////    [ResourceExposure( ResourceScope.Process )]
////    [ResourceConsumption( ResourceScope.Process )]
////    public static void ResetColor()
////    {
////        new UIPermission( UIPermissionWindow.SafeTopLevelWindows ).Demand();
////
////        bool succeeded;
////        Win32Native.CONSOLE_SCREEN_BUFFER_INFO csbi = GetBufferInfo( false, out succeeded );
////        // For code that may be used from Windows app w/ no console
////        if(!succeeded)
////            return;
////
////        BCLDebug.Assert( _haveReadDefaultColors, "Setting the foreground color before we've read the default foreground color!" );
////
////        short defaultAttrs = (short)(ushort)_defaultColors;
////        // Ignore errors here - there are some scenarios for running code that wants
////        // to print in colors to the console in a Windows application.
////        Win32Native.SetConsoleTextAttribute( ConsoleOutputHandle, defaultAttrs );
////    }
////
////    [ResourceExposure( ResourceScope.Process )]
////    [ResourceConsumption( ResourceScope.Process )]
////    public static void MoveBufferArea( int sourceLeft, int sourceTop,
////        int sourceWidth, int sourceHeight, int targetLeft, int targetTop )
////    {
////        MoveBufferArea( sourceLeft, sourceTop, sourceWidth, sourceHeight, targetLeft, targetTop, ' ', ConsoleColor.Black, BackgroundColor );
////    }
////
////    [ResourceExposure( ResourceScope.Process )]
////    [ResourceConsumption( ResourceScope.Process )]
////    public unsafe static void MoveBufferArea( int sourceLeft, int sourceTop,
////        int sourceWidth, int sourceHeight, int targetLeft, int targetTop,
////        char sourceChar, ConsoleColor sourceForeColor,
////        ConsoleColor sourceBackColor )
////    {
////        if(sourceForeColor < ConsoleColor.Black || sourceForeColor > ConsoleColor.White)
////            throw new ArgumentException( Environment.GetResourceString( "Arg_InvalidConsoleColor" ), "sourceForeColor" );
////        if(sourceBackColor < ConsoleColor.Black || sourceBackColor > ConsoleColor.White)
////            throw new ArgumentException( Environment.GetResourceString( "Arg_InvalidConsoleColor" ), "sourceBackColor" );
////
////        Win32Native.CONSOLE_SCREEN_BUFFER_INFO csbi = GetBufferInfo();
////        Win32Native.COORD bufferSize = csbi.dwSize;
////        if(sourceLeft < 0 || sourceLeft > bufferSize.X)
////            throw new ArgumentOutOfRangeException( "sourceLeft", sourceLeft, Environment.GetResourceString( "ArgumentOutOfRange_ConsoleBufferBoundaries" ) );
////        if(sourceTop < 0 || sourceTop > bufferSize.Y)
////            throw new ArgumentOutOfRangeException( "sourceTop", sourceTop, Environment.GetResourceString( "ArgumentOutOfRange_ConsoleBufferBoundaries" ) );
////        if(sourceWidth < 0 || sourceWidth > bufferSize.X - sourceLeft)
////            throw new ArgumentOutOfRangeException( "sourceWidth", sourceWidth, Environment.GetResourceString( "ArgumentOutOfRange_ConsoleBufferBoundaries" ) );
////        if(sourceHeight < 0 || sourceTop > bufferSize.Y - sourceHeight)
////            throw new ArgumentOutOfRangeException( "sourceHeight", sourceHeight, Environment.GetResourceString( "ArgumentOutOfRange_ConsoleBufferBoundaries" ) );
////
////        // Note: if the target range is partially in and partially out
////        // of the buffer, then we let the OS clip it for us.
////        if(targetLeft < 0 || targetLeft > bufferSize.X)
////            throw new ArgumentOutOfRangeException( "targetLeft", targetLeft, Environment.GetResourceString( "ArgumentOutOfRange_ConsoleBufferBoundaries" ) );
////        if(targetTop < 0 || targetTop > bufferSize.Y)
////            throw new ArgumentOutOfRangeException( "targetTop", targetTop, Environment.GetResourceString( "ArgumentOutOfRange_ConsoleBufferBoundaries" ) );
////
////        // If we're not doing any work, bail out now (Windows will return
////        // an error otherwise)
////        if(sourceWidth == 0 || sourceHeight == 0)
////            return;
////
////        new UIPermission( UIPermissionWindow.SafeTopLevelWindows ).Demand();
////
////        // Read data from the original location, blank it out, then write
////        // it to the new location.  This will handle overlapping source and
////        // destination regions correctly.
////
////        // See the "Reading and Writing Blocks of Characters and Attributes" 
////        // sample for help
////
////        // Read the old data
////        Win32Native.CHAR_INFO[] data = new Win32Native.CHAR_INFO[sourceWidth * sourceHeight];
////        bufferSize.X = (short)sourceWidth;
////        bufferSize.Y = (short)sourceHeight;
////        Win32Native.COORD bufferCoord = new Win32Native.COORD();
////        Win32Native.SMALL_RECT readRegion = new Win32Native.SMALL_RECT();
////        readRegion.Left = (short)sourceLeft;
////        readRegion.Right = (short)(sourceLeft + sourceWidth - 1);
////        readRegion.Top = (short)sourceTop;
////        readRegion.Bottom = (short)(sourceTop + sourceHeight - 1);
////
////        bool r;
////        fixed(Win32Native.CHAR_INFO* pCharInfo = data)
////            r = Win32Native.ReadConsoleOutput( ConsoleOutputHandle, pCharInfo, bufferSize, bufferCoord, ref readRegion );
////        if(!r)
////            __Error.WinIOError();
////
////        // Overwrite old section
////        // I don't have a good function to blank out a rectangle.
////        Win32Native.COORD writeCoord = new Win32Native.COORD();
////        writeCoord.X = (short)sourceLeft;
////        Win32Native.Color c = ConsoleColorToColorAttribute( sourceBackColor, true );
////        c |= ConsoleColorToColorAttribute( sourceForeColor, false );
////        short attr = (short)c;
////        int numWritten;
////        for(int i = sourceTop; i < sourceTop + sourceHeight; i++)
////        {
////            writeCoord.Y = (short)i;
////            r = Win32Native.FillConsoleOutputCharacter( ConsoleOutputHandle, sourceChar, sourceWidth, writeCoord, out numWritten );
////            BCLDebug.Assert( numWritten == sourceWidth, "FillConsoleOutputCharacter wrote the wrong number of chars!" );
////            if(!r)
////                __Error.WinIOError();
////
////            r = Win32Native.FillConsoleOutputAttribute( ConsoleOutputHandle, attr, sourceWidth, writeCoord, out numWritten );
////            if(!r)
////                __Error.WinIOError();
////        }
////
////        // Write text to new location
////        Win32Native.SMALL_RECT writeRegion = new Win32Native.SMALL_RECT();
////        writeRegion.Left = (short)targetLeft;
////        writeRegion.Right = (short)(targetLeft + sourceWidth);
////        writeRegion.Top = (short)targetTop;
////        writeRegion.Bottom = (short)(targetTop + sourceHeight);
////
////        fixed(Win32Native.CHAR_INFO* pCharInfo = data)
////            r = Win32Native.WriteConsoleOutput( ConsoleOutputHandle, pCharInfo, bufferSize, bufferCoord, ref writeRegion );
////    }
////
////    public static int BufferHeight
////    {
////        [ResourceExposure( ResourceScope.Process )]
////        [ResourceConsumption( ResourceScope.Process )]
////        get
////        {
////            Win32Native.CONSOLE_SCREEN_BUFFER_INFO csbi = GetBufferInfo();
////            return csbi.dwSize.Y;
////        }
////        [ResourceExposure( ResourceScope.Process )]
////        [ResourceConsumption( ResourceScope.Process )]
////        set
////        {
////            SetBufferSize( BufferWidth, value );
////        }
////    }
////
////    public static int BufferWidth
////    {
////        [ResourceExposure( ResourceScope.Process )]
////        [ResourceConsumption( ResourceScope.Process )]
////        get
////        {
////            Win32Native.CONSOLE_SCREEN_BUFFER_INFO csbi = GetBufferInfo();
////            return csbi.dwSize.X;
////        }
////        [ResourceExposure( ResourceScope.Process )]
////        [ResourceConsumption( ResourceScope.Process )]
////        set
////        {
////            SetBufferSize( value, BufferHeight );
////        }
////    }
////
////    [ResourceExposure( ResourceScope.Process )]
////    [ResourceConsumption( ResourceScope.Process )]
////    public static void SetBufferSize( int width, int height )
////    {
////        new UIPermission( UIPermissionWindow.SafeTopLevelWindows ).Demand();
////
////        // Ensure the new size is not smaller than the console window
////        Win32Native.CONSOLE_SCREEN_BUFFER_INFO csbi = GetBufferInfo();
////        Win32Native.SMALL_RECT srWindow = csbi.srWindow;
////        if(width < srWindow.Right + 1 || width >= Int16.MaxValue)
////            throw new ArgumentOutOfRangeException( "width", width, Environment.GetResourceString( "ArgumentOutOfRange_ConsoleBufferLessThanWindowSize" ) );
////        if(height < srWindow.Bottom + 1 || height >= Int16.MaxValue)
////            throw new ArgumentOutOfRangeException( "height", height, Environment.GetResourceString( "ArgumentOutOfRange_ConsoleBufferLessThanWindowSize" ) );
////
////        Win32Native.COORD size = new Win32Native.COORD();
////        size.X = (short)width;
////        size.Y = (short)height;
////        bool r = Win32Native.SetConsoleScreenBufferSize( ConsoleOutputHandle, size );
////        if(!r)
////            __Error.WinIOError();
////    }
////
////
////    public static int WindowHeight
////    {
////        [ResourceExposure( ResourceScope.None )]
////        [ResourceConsumption( ResourceScope.Process, ResourceScope.Process )]
////        get
////        {
////            Win32Native.CONSOLE_SCREEN_BUFFER_INFO csbi = GetBufferInfo();
////            return csbi.srWindow.Bottom - csbi.srWindow.Top + 1;
////        }
////        [ResourceExposure( ResourceScope.Process )]
////        [ResourceConsumption( ResourceScope.Process )]
////        set
////        {
////            SetWindowSize( WindowWidth, value );
////        }
////    }
////
////    public static int WindowWidth
////    {
////        [ResourceExposure( ResourceScope.None )]
////        [ResourceConsumption( ResourceScope.Process, ResourceScope.Process )]
////        get
////        {
////            Win32Native.CONSOLE_SCREEN_BUFFER_INFO csbi = GetBufferInfo();
////            return csbi.srWindow.Right - csbi.srWindow.Left + 1;
////        }
////        [ResourceExposure( ResourceScope.Process )]
////        [ResourceConsumption( ResourceScope.Process )]
////        set
////        {
////            SetWindowSize( value, WindowHeight );
////        }
////    }
////
////    [ResourceExposure( ResourceScope.Process )]
////    [ResourceConsumption( ResourceScope.Process )]
////    public static unsafe void SetWindowSize( int width, int height )
////    {
////        new UIPermission( UIPermissionWindow.SafeTopLevelWindows ).Demand();
////
////        // Get the position of the current console window
////        Win32Native.CONSOLE_SCREEN_BUFFER_INFO csbi = GetBufferInfo();
////
////        if(width <= 0)
////            throw new ArgumentOutOfRangeException( "width", width, Environment.GetResourceString( "ArgumentOutOfRange_NeedPosNum" ) );
////        if(height <= 0)
////            throw new ArgumentOutOfRangeException( "height", height, Environment.GetResourceString( "ArgumentOutOfRange_NeedPosNum" ) );
////
////        bool r;
////
////        // If the buffer is smaller than this new window size, resize the
////        // buffer to be large enough.  Include window position.
////        bool resizeBuffer = false;
////        Win32Native.COORD size = new Win32Native.COORD();
////        size.X = csbi.dwSize.X;
////        size.Y = csbi.dwSize.Y;
////        if(csbi.dwSize.X < csbi.srWindow.Left + width)
////        {
////            if(csbi.srWindow.Left >= Int16.MaxValue - width)
////                throw new ArgumentOutOfRangeException( "width", Environment.GetResourceString( "ArgumentOutOfRange_ConsoleWindowBufferSize" ) );
////            size.X = (short)(csbi.srWindow.Left + width);
////            resizeBuffer = true;
////        }
////        if(csbi.dwSize.Y < csbi.srWindow.Top + height)
////        {
////            if(csbi.srWindow.Top >= Int16.MaxValue - height)
////                throw new ArgumentOutOfRangeException( "height", Environment.GetResourceString( "ArgumentOutOfRange_ConsoleWindowBufferSize" ) );
////            size.Y = (short)(csbi.srWindow.Top + height);
////            resizeBuffer = true;
////        }
////        if(resizeBuffer)
////        {
////            r = Win32Native.SetConsoleScreenBufferSize( ConsoleOutputHandle, size );
////            if(!r)
////                __Error.WinIOError();
////        }
////
////        Win32Native.SMALL_RECT srWindow = csbi.srWindow;
////        // Preserve the position, but change the size.
////        srWindow.Bottom = (short)(srWindow.Top + height - 1);
////        srWindow.Right = (short)(srWindow.Left + width - 1);
////
////        r = Win32Native.SetConsoleWindowInfo( ConsoleOutputHandle, true, &srWindow );
////        if(!r)
////        {
////            int errorCode = Marshal.GetLastWin32Error();
////
////            // If we resized the buffer, un-resize it.
////            if(resizeBuffer)
////            {
////                Win32Native.SetConsoleScreenBufferSize( ConsoleOutputHandle, csbi.dwSize );
////            }
////
////            // Try to give a better error message here
////            Win32Native.COORD bounds = Win32Native.GetLargestConsoleWindowSize( ConsoleOutputHandle );
////            if(width > bounds.X)
////                throw new ArgumentOutOfRangeException( "width", width, Environment.GetResourceString( "ArgumentOutOfRange_ConsoleWindowSize_Size", bounds.X ) );
////            if(height > bounds.Y)
////                throw new ArgumentOutOfRangeException( "height", height, Environment.GetResourceString( "ArgumentOutOfRange_ConsoleWindowSize_Size", bounds.Y ) );
////
////            __Error.WinIOError( errorCode, String.Empty );
////        }
////    }
////
////    public static int LargestWindowWidth
////    {
////        [ResourceExposure( ResourceScope.None )]
////        [ResourceConsumption( ResourceScope.Process, ResourceScope.Process )]
////        get
////        {
////            // Note this varies based on current screen resolution and 
////            // current console font.  Do not cache this value.
////            Win32Native.COORD bounds = Win32Native.GetLargestConsoleWindowSize( ConsoleOutputHandle );
////            return bounds.X;
////        }
////    }
////
////    public static int LargestWindowHeight
////    {
////        [ResourceExposure( ResourceScope.None )]
////        [ResourceConsumption( ResourceScope.Process, ResourceScope.Process )]
////        get
////        {
////            // Note this varies based on current screen resolution and 
////            // current console font.  Do not cache this value.
////            Win32Native.COORD bounds = Win32Native.GetLargestConsoleWindowSize( ConsoleOutputHandle );
////            return bounds.Y;
////        }
////    }
////
////    public static int WindowLeft
////    {
////        [ResourceExposure( ResourceScope.None )]
////        [ResourceConsumption( ResourceScope.Process, ResourceScope.Process )]
////        get
////        {
////            Win32Native.CONSOLE_SCREEN_BUFFER_INFO csbi = GetBufferInfo();
////            return csbi.srWindow.Left;
////        }
////        [ResourceExposure( ResourceScope.Process )]
////        [ResourceConsumption( ResourceScope.Process )]
////        set
////        {
////            SetWindowPosition( value, WindowTop );
////        }
////    }
////
////    public static int WindowTop
////    {
////        [ResourceExposure( ResourceScope.None )]
////        [ResourceConsumption( ResourceScope.Process, ResourceScope.Process )]
////        get
////        {
////            Win32Native.CONSOLE_SCREEN_BUFFER_INFO csbi = GetBufferInfo();
////            return csbi.srWindow.Top;
////        }
////        [ResourceExposure( ResourceScope.Process )]
////        [ResourceConsumption( ResourceScope.Process )]
////        set
////        {
////            SetWindowPosition( WindowLeft, value );
////        }
////    }
////
////    [ResourceExposure( ResourceScope.Process )]
////    [ResourceConsumption( ResourceScope.Process )]
////    public static unsafe void SetWindowPosition( int left, int top )
////    {
////        new UIPermission( UIPermissionWindow.SafeTopLevelWindows ).Demand();
////
////        // Get the size of the current console window
////        Win32Native.CONSOLE_SCREEN_BUFFER_INFO csbi = GetBufferInfo();
////
////        Win32Native.SMALL_RECT srWindow = csbi.srWindow;
////
////        // Check for arithmetic underflows & overflows.
////        int newRight = left + srWindow.Right - srWindow.Left + 1;
////        if(left < 0 || newRight > csbi.dwSize.X || newRight < 0)
////            throw new ArgumentOutOfRangeException( "left", left, Environment.GetResourceString( "ArgumentOutOfRange_ConsoleWindowPos" ) );
////        int newBottom = top + srWindow.Bottom - srWindow.Top + 1;
////        if(top < 0 || newBottom > csbi.dwSize.Y || newBottom < 0)
////            throw new ArgumentOutOfRangeException( "top", top, Environment.GetResourceString( "ArgumentOutOfRange_ConsoleWindowPos" ) );
////
////        // Preserve the size, but move the position.
////        srWindow.Bottom -= (short)(srWindow.Top - top);
////        srWindow.Right -= (short)(srWindow.Left - left);
////        srWindow.Left = (short)left;
////        srWindow.Top = (short)top;
////
////        bool r = Win32Native.SetConsoleWindowInfo( ConsoleOutputHandle, true, &srWindow );
////        if(!r)
////            __Error.WinIOError();
////    }
////
////    public static int CursorLeft
////    {
////        [ResourceExposure( ResourceScope.None )]
////        [ResourceConsumption( ResourceScope.Process, ResourceScope.Process )]
////        get
////        {
////            Win32Native.CONSOLE_SCREEN_BUFFER_INFO csbi = GetBufferInfo();
////            return csbi.dwCursorPosition.X;
////        }
////        [ResourceExposure( ResourceScope.Process )]
////        [ResourceConsumption( ResourceScope.Process )]
////        set
////        {
////            SetCursorPosition( value, CursorTop );
////        }
////    }
////
////    public static int CursorTop
////    {
////        [ResourceExposure( ResourceScope.None )]
////        [ResourceConsumption( ResourceScope.Process, ResourceScope.Process )]
////        get
////        {
////            Win32Native.CONSOLE_SCREEN_BUFFER_INFO csbi = GetBufferInfo();
////            return csbi.dwCursorPosition.Y;
////        }
////        [ResourceExposure( ResourceScope.Process )]
////        [ResourceConsumption( ResourceScope.Process )]
////        set
////        {
////            SetCursorPosition( CursorLeft, value );
////        }
////    }
////
////    [ResourceExposure( ResourceScope.Process )]
////    [ResourceConsumption( ResourceScope.Process )]
////    public static void SetCursorPosition( int left, int top )
////    {
////        // Note on argument checking - the upper bounds are NOT correct 
////        // here!  But it looks slightly expensive to compute them.  Let
////        // Windows calculate them, then we'll give a nice error message.
////        if(left < 0 || left >= Int16.MaxValue)
////            throw new ArgumentOutOfRangeException( "left", left, Environment.GetResourceString( "ArgumentOutOfRange_ConsoleBufferBoundaries" ) );
////        if(top < 0 || top >= Int16.MaxValue)
////            throw new ArgumentOutOfRangeException( "top", top, Environment.GetResourceString( "ArgumentOutOfRange_ConsoleBufferBoundaries" ) );
////
////        new UIPermission( UIPermissionWindow.SafeTopLevelWindows ).Demand();
////
////        IntPtr hConsole = ConsoleOutputHandle;
////        Win32Native.COORD coords = new Win32Native.COORD();
////        coords.X = (short)left;
////        coords.Y = (short)top;
////        bool r = Win32Native.SetConsoleCursorPosition( hConsole, coords );
////        if(!r)
////        {
////            // Give a nice error message for out of range sizes
////            int errorCode = Marshal.GetLastWin32Error();
////            Win32Native.CONSOLE_SCREEN_BUFFER_INFO csbi = GetBufferInfo();
////            if(left < 0 || left >= csbi.dwSize.X)
////                throw new ArgumentOutOfRangeException( "left", left, Environment.GetResourceString( "ArgumentOutOfRange_ConsoleBufferBoundaries" ) );
////            if(top < 0 || top >= csbi.dwSize.Y)
////                throw new ArgumentOutOfRangeException( "top", top, Environment.GetResourceString( "ArgumentOutOfRange_ConsoleBufferBoundaries" ) );
////
////            __Error.WinIOError( errorCode, String.Empty );
////        }
////    }
////
////    public static int CursorSize
////    {
////        [ResourceExposure( ResourceScope.None )]
////        [ResourceConsumption( ResourceScope.Process, ResourceScope.Process )]
////        get
////        {
////            Win32Native.CONSOLE_CURSOR_INFO cci;
////            IntPtr hConsole = ConsoleOutputHandle;
////            bool r = Win32Native.GetConsoleCursorInfo( hConsole, out cci );
////            if(!r)
////                __Error.WinIOError();
////
////            return cci.dwSize;
////        }
////        [ResourceExposure( ResourceScope.Process )]
////        [ResourceConsumption( ResourceScope.Process )]
////        set
////        {
////            // Value should be a percentage from [1, 100].
////            if(value < 1 || value > 100)
////                throw new ArgumentOutOfRangeException( "value", value, Environment.GetResourceString( "ArgumentOutOfRange_CursorSize" ) );
////
////            new UIPermission( UIPermissionWindow.SafeTopLevelWindows ).Demand();
////
////            Win32Native.CONSOLE_CURSOR_INFO cci;
////            IntPtr hConsole = ConsoleOutputHandle;
////            bool r = Win32Native.GetConsoleCursorInfo( hConsole, out cci );
////            if(!r)
////                __Error.WinIOError();
////
////            cci.dwSize = value;
////            r = Win32Native.SetConsoleCursorInfo( hConsole, ref cci );
////            if(!r)
////                __Error.WinIOError();
////        }
////    }
////
////    public static bool CursorVisible
////    {
////        [ResourceExposure( ResourceScope.None )]
////        [ResourceConsumption( ResourceScope.Process, ResourceScope.Process )]
////        get
////        {
////            Win32Native.CONSOLE_CURSOR_INFO cci;
////            IntPtr hConsole = ConsoleOutputHandle;
////            bool r = Win32Native.GetConsoleCursorInfo( hConsole, out cci );
////            if(!r)
////                __Error.WinIOError();
////
////            return cci.bVisible;
////        }
////        [ResourceExposure( ResourceScope.Process )]
////        [ResourceConsumption( ResourceScope.Process )]
////        set
////        {
////            new UIPermission( UIPermissionWindow.SafeTopLevelWindows ).Demand();
////
////            Win32Native.CONSOLE_CURSOR_INFO cci;
////            IntPtr hConsole = ConsoleOutputHandle;
////            bool r = Win32Native.GetConsoleCursorInfo( hConsole, out cci );
////            if(!r)
////                __Error.WinIOError();
////
////            cci.bVisible = value;
////            r = Win32Native.SetConsoleCursorInfo( hConsole, ref cci );
////            if(!r)
////                __Error.WinIOError();
////        }
////    }
////
////    public static String Title
////    {
////        [ResourceExposure( ResourceScope.None )]
////        [ResourceConsumption( ResourceScope.Process, ResourceScope.Process )]
////        get
////        {
////            StringBuilder sb = new StringBuilder( MaxConsoleTitleLength + 1 );
////            // As of Windows Server 2003 (and probably all prior 
////            // releases), GetConsoleTitle doesn't do anything useful on
////            // empty strings, such as calling SetLastError so callers can
////            // determine whether that 0 was an error or was a success.
////            Win32Native.SetLastError( 0 );
////            int r = Win32Native.GetConsoleTitle( sb, sb.Capacity );
////            if(r == 0)
////            {
////                // Allow a 0-length title
////                int errorCode = Marshal.GetLastWin32Error();
////                if(errorCode == 0)
////                    sb.Length = 0;
////                else
////                    __Error.WinIOError( errorCode, String.Empty );
////            }
////            else if(r > MaxConsoleTitleLength)
////                throw new InvalidOperationException( Environment.GetResourceString( "ArgumentOutOfRange_ConsoleTitleTooLong" ) );
////            return sb.ToString();
////        }
////        [ResourceExposure( ResourceScope.None )]
////        [ResourceConsumption( ResourceScope.Process, ResourceScope.Process )]
////        set
////        {
////            new UIPermission( UIPermissionWindow.SafeTopLevelWindows ).Demand();
////
////            if(value == null)
////                throw new ArgumentNullException( "value" );
////            if(value.Length > MaxConsoleTitleLength)
////                throw new ArgumentOutOfRangeException( "value", Environment.GetResourceString( "ArgumentOutOfRange_ConsoleTitleTooLong" ) );
////
////            if(!Win32Native.SetConsoleTitle( value ))
////                __Error.WinIOError();
////        }
////    }
////
////    [Flags]
////    internal enum ControlKeyState
////    {
////        RightAltPressed = 0x0001,
////        LeftAltPressed = 0x0002,
////        RightCtrlPressed = 0x0004,
////        LeftCtrlPressed = 0x0008,
////        ShiftPressed = 0x0010,
////        NumLockOn = 0x0020,
////        ScrollLockOn = 0x0040,
////        CapsLockOn = 0x0080,
////        EnhancedKey = 0x0100
////    }
////
////    [HostProtection( UI = true )]
////    [ResourceExposure( ResourceScope.Process )]
////    [ResourceConsumption( ResourceScope.Process )]
////    public static ConsoleKeyInfo ReadKey()
////    {
////        return ReadKey( false );
////    }
////
////    // For tracking Alt+NumPad unicode key sequence. When you presss Alt key down 
////    // and press a numpad unicode decimal sequence and then release Alt key, the
////    // desired effect is to translate the sequence into one Unicode KeyPress. 
////    // We need to keep track of the Alt+NumPad sequence and surface the final
////    // unicode char alone when the Alt key is released. 
////    private static bool IsAltKeyDown( Win32Native.InputRecord ir )
////    {
////        return (((ControlKeyState)ir.keyEvent.controlKeyState)
////                          & (ControlKeyState.LeftAltPressed | ControlKeyState.RightAltPressed)) != 0;
////    }
////
////    // Skip non key events. Generally we want to surface only KeyDown event 
////    // and suppress KeyUp event from the same Key press but there are cases
////    // where the assumption of KeyDown-KeyUp pairing for a given key press 
////    // is invalid. For example in IME Unicode keyboard input, we often see
////    // only KeyUp until the key is released.  
////    private static bool IsKeyDownEvent( Win32Native.InputRecord ir )
////    {
////        return (ir.eventType == Win32Native.KEY_EVENT && ir.keyEvent.keyDown);
////    }
////
////    private static bool IsModKey( Win32Native.InputRecord ir )
////    {
////        // We should also skip over Shift, Control, and Alt, as well as caps lock.
////        // Apparently we don't need to check for 0xA0 through 0xA5, which are keys like 
////        // Left Control & Right Control. See the ConsoleKey enum for these values.
////        short keyCode = ir.keyEvent.virtualKeyCode;
////        return ((keyCode >= 0x10 && keyCode <= 0x12)
////                || keyCode == 0x14 || keyCode == 0x90 || keyCode == 0x91);
////    }
////
////    [HostProtection( UI = true )]
////    [ResourceExposure( ResourceScope.Process )]
////    [ResourceConsumption( ResourceScope.Process )]
////    public static ConsoleKeyInfo ReadKey( bool intercept )
////    {
////        Win32Native.InputRecord ir;
////        int numEventsRead = -1;
////        bool r;
////
////        if(_cachedInputRecord.eventType == Win32Native.KEY_EVENT)
////        {
////            // We had a previous keystroke with repeated characters.
////            ir = _cachedInputRecord;
////            if(_cachedInputRecord.keyEvent.repeatCount == 0)
////                _cachedInputRecord.eventType = -1;
////            else
////            {
////                _cachedInputRecord.keyEvent.repeatCount--;
////            }
////            // We will return one key from this method, so we decrement the
////            // repeatCount here, leaving the cachedInputRecord in the "queue".
////        }
////        else
////        {
////            while(true)
////            {
////                r = Win32Native.ReadConsoleInput( ConsoleInputHandle, out ir, 1, out numEventsRead );
////                if(!r || numEventsRead == 0)
////                {
////                    // This will fail when stdin is redirected from a file or pipe. 
////                    // We could theoretically call Console.Read here, but I 
////                    // think we might do some things incorrectly then.
////                    throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_ConsoleReadKeyOnFile" ) );
////                }
////
////                short keyCode = ir.keyEvent.virtualKeyCode;
////
////                // First check for non-keyboard events & discard them. Generally we tap into only KeyDown events and ignore the KeyUp events
////                // but it is possible that we are dealing with a Alt+NumPad unicode key sequence, the final unicode char is revealed only when 
////                // the Alt key is released (i.e when the sequence is complete). To avoid noise, when the Alt key is down, we should eat up 
////                // any intermediate key strokes (from NumPad) that collectively forms the Unicode character.  
////
////                if(!IsKeyDownEvent( ir ))
////                {
////                    // REVIEW: Unicode IME input comes through as KeyUp event with no accompanying KeyDown.
////                    if(keyCode != AltVKCode)
////                        continue;
////                }
////
////                char ch = (char)ir.keyEvent.uChar;
////
////                // In a Alt+NumPad unicode sequence, when the alt key is released uChar will represent the final unicode character, we need to 
////                // surface this. VirtualKeyCode for this event will be Alt from the Alt-Up key event. This is probably not the right code, 
////                // especially when we don't expose ConsoleKey.Alt, so this will end up being the hex value (0x12). VK_PACKET comes very 
////                // close to being useful and something that we could look into using for this purpose... 
////
////                if(ch == 0)
////                {
////                    // Skip mod keys.
////                    if(IsModKey( ir ))
////                        continue;
////                }
////
////                // When Alt is down, it is possible that we are in the middle of a Alt+NumPad unicode sequence.
////                // Escape any intermediate NumPad keys whether NumLock is on or not (notepad behavior)
////                ConsoleKey key = (ConsoleKey)keyCode;
////                if(IsAltKeyDown( ir ) && ((key >= ConsoleKey.NumPad0 && key <= ConsoleKey.NumPad9)
////                                     || (key == ConsoleKey.Clear) || (key == ConsoleKey.Insert)
////                                     || (key >= ConsoleKey.PageUp && key <= ConsoleKey.DownArrow)))
////                {
////                    continue;
////                }
////
////                if(ir.keyEvent.repeatCount > 1)
////                {
////                    ir.keyEvent.repeatCount--;
////                    _cachedInputRecord = ir;
////                }
////                break;
////            }
////        }
////
////        ControlKeyState state = (ControlKeyState)ir.keyEvent.controlKeyState;
////        bool shift = (state & ControlKeyState.ShiftPressed) != 0;
////        bool alt = (state & (ControlKeyState.LeftAltPressed | ControlKeyState.RightAltPressed)) != 0;
////        bool control = (state & (ControlKeyState.LeftCtrlPressed | ControlKeyState.RightCtrlPressed)) != 0;
////
////        ConsoleKeyInfo info = new ConsoleKeyInfo( (char)ir.keyEvent.uChar, (ConsoleKey)ir.keyEvent.virtualKeyCode, shift, alt, control );
////
////        if(!intercept)
////            Console.Write( ir.keyEvent.uChar );
////        return info;
////    }
////
////    public static bool KeyAvailable
////    {
////        [ResourceExposure( ResourceScope.Process )]
////        [ResourceConsumption( ResourceScope.Process )]
////        [HostProtection( UI = true )]
////        get
////        {
////            if(_cachedInputRecord.eventType == Win32Native.KEY_EVENT)
////                return true;
////
////            Win32Native.InputRecord ir = new Win32Native.InputRecord();
////            int numEventsRead = 0;
////            while(true)
////            {
////                bool r = Win32Native.PeekConsoleInput( ConsoleInputHandle, out ir, 1, out numEventsRead );
////                if(!r)
////                {
////                    int errorCode = Marshal.GetLastWin32Error();
////                    if(errorCode == Win32Native.ERROR_INVALID_HANDLE)
////                        throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_ConsoleKeyAvailableOnFile" ) );
////                    __Error.WinIOError( errorCode, "stdin" );
////                }
////
////                if(numEventsRead == 0)
////                    return false;
////
////                // Skip non key-down && mod key events. 
////                if(!IsKeyDownEvent( ir ) || IsModKey( ir ))
////                {
////                    // REVIEW: Unicode IME input comes through as KeyUp event with no accompanying KeyDown
////
////                    // Exempt Alt keyUp for possible Alt+NumPad unicode sequence.
////                    //short keyCode = ir.keyEvent.virtualKeyCode;
////                    //if (!IsKeyDownEvent(ir) && (keyCode == AltVKCode))
////                    //    return true;
////
////                    r = Win32Native.ReadConsoleInput( ConsoleInputHandle, out ir, 1, out numEventsRead );
////
////                    if(!r)
////                        __Error.WinIOError();
////                }
////                else
////                {
////                    return true;
////                }
////            }
////        }
////    }
////
////    public static bool NumberLock
////    {
////        [ResourceExposure( ResourceScope.Process )]
////        [ResourceConsumption( ResourceScope.Process )]
////        get
////        {
////            short s = Win32Native.GetKeyState( NumberLockVKCode );
////            return (s & 1) == 1;
////        }
////    }
////
////    public static bool CapsLock
////    {
////        [ResourceExposure( ResourceScope.Process )]
////        [ResourceConsumption( ResourceScope.Process )]
////        get
////        {
////            short s = Win32Native.GetKeyState( CapsLockVKCode );
////            return (s & 1) == 1;
////        }
////    }
////
////    public static bool TreatControlCAsInput
////    {
////        [ResourceExposure( ResourceScope.None )]
////        [ResourceConsumption( ResourceScope.Process, ResourceScope.Process )]
////        get
////        {
////            IntPtr handle = ConsoleInputHandle;
////            if(handle == Win32Native.INVALID_HANDLE_VALUE)
////                throw new IOException( Environment.GetResourceString( "IO.IO_NoConsole" ) );
////            int mode = 0;
////            bool r = Win32Native.GetConsoleMode( handle, out mode );
////            if(!r)
////                __Error.WinIOError();
////            return (mode & Win32Native.ENABLE_PROCESSED_INPUT) == 0;
////        }
////        [ResourceExposure( ResourceScope.Process )]
////        [ResourceConsumption( ResourceScope.Process )]
////        set
////        {
////            new UIPermission( UIPermissionWindow.SafeTopLevelWindows ).Demand();
////
////            IntPtr handle = ConsoleInputHandle;
////            if(handle == Win32Native.INVALID_HANDLE_VALUE)
////                throw new IOException( Environment.GetResourceString( "IO.IO_NoConsole" ) );
////
////            int mode = 0;
////            bool r = Win32Native.GetConsoleMode( handle, out mode );
////            if(value)
////                mode &= ~Win32Native.ENABLE_PROCESSED_INPUT;
////            else
////                mode |= Win32Native.ENABLE_PROCESSED_INPUT;
////            r = Win32Native.SetConsoleMode( handle, mode );
////
////            if(!r)
////                __Error.WinIOError();
////        }
////    }
////
////    // During an appdomain unload, we must call into the OS and remove
////    // our delegate from the OS's list of console control handlers.  If
////    // we don't do this, the OS will call back on a delegate that no
////    // longer exists.  So, subclass CriticalFinalizableObject.
////    // This problem would theoretically exist during process exit for a
////    // single appdomain too, so using a critical finalizer is probably
////    // better than the appdomain unload event (I'm not sure we call that
////    // in the default appdomain during process exit).
////    internal sealed class ControlCHooker : CriticalFinalizerObject
////    {
////        private bool _hooked;
////        private Win32Native.ConsoleCtrlHandlerRoutine _handler;
////
////        internal ControlCHooker()
////        {
////            _handler = new Win32Native.ConsoleCtrlHandlerRoutine( BreakEvent );
////        }
////
////        [ResourceExposure( ResourceScope.None )]
////        [ResourceConsumption( ResourceScope.Machine, ResourceScope.Machine )]
////        ~ControlCHooker()
////        {
////            Unhook();
////        }
////
////        [ResourceExposure( ResourceScope.Process )]
////        [ResourceConsumption( ResourceScope.Process )]
////        internal void Hook()
////        {
////            if(!_hooked)
////            {
////                bool r = Win32Native.SetConsoleCtrlHandler( _handler, true );
////                if(!r)
////                    __Error.WinIOError();
////                _hooked = true;
////            }
////        }
////
////        [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
////        [ResourceExposure( ResourceScope.Process )]
////        [ResourceConsumption( ResourceScope.Process )]
////        internal void Unhook()
////        {
////            if(_hooked)
////            {
////                bool r = Win32Native.SetConsoleCtrlHandler( _handler, false );
////                if(!r)
////                    __Error.WinIOError();
////                _hooked = false;
////            }
////        }
////    }
////
////    // A class with data so ControlC handlers can be called on a Threadpool thread.
////    private sealed class ControlCDelegateData
////    {
////        internal ConsoleSpecialKey ControlKey;
////        internal bool Cancel;
////        internal bool DelegateStarted;
////        internal ManualResetEvent CompletionEvent;
////        internal ConsoleCancelEventHandler CancelCallbacks;
////        internal ControlCDelegateData( ConsoleSpecialKey controlKey, ConsoleCancelEventHandler cancelCallbacks )
////        {
////            this.ControlKey = controlKey;
////            this.CancelCallbacks = cancelCallbacks;
////            this.CompletionEvent = new ManualResetEvent( false );
////            // this.Cancel defaults to false
////            // this.DelegateStarted defaults to false
////        }
////    }
////
////    // Returns true if we've "handled" the break request, false if
////    // we want to terminate the process (or at least let the next
////    // control handler function have a chance).
////    private static bool BreakEvent( int controlType )
////    {
////
////        // The thread that this gets called back on has a very small stack on 64 bit systems. There is
////        // not enough space to handle a managed exception being caught and thrown. So, queue up a work
////        // item on another thread for the actual event callback.
////
////        if(controlType == Win32Native.CTRL_C_EVENT ||
////            controlType == Win32Native.CTRL_BREAK_EVENT)
////        {
////
////            // To avoid race between remove handler and raising the event
////            ConsoleCancelEventHandler cancelCallbacks = Console._cancelCallbacks;
////            if(cancelCallbacks == null)
////            {
////                return false;
////            }
////
////            // Create the delegate
////            ConsoleSpecialKey controlKey = (controlType == 0) ? ConsoleSpecialKey.ControlC : ConsoleSpecialKey.ControlBreak;
////            ControlCDelegateData delegateData = new ControlCDelegateData( controlKey, cancelCallbacks );
////            WaitCallback controlCCallback = new WaitCallback( ControlCDelegate );
////
////            // Queue the delegate
////            if(!ThreadPool.QueueUserWorkItem( controlCCallback, delegateData ))
////            {
////                BCLDebug.Assert( false, "ThreadPool.QueueUserWorkItem returned false without throwing. Unable to execute ControlC handler" );
////                return false;
////            }
////            // Block until the delegate is done. We need to be robust in the face of the work item not executing
////            // but we also want to get control back immediately after it is done and we don't want to give the
////            // handler a fixed time limit in case it needs to display UI. Wait on the event twice, once with a
////            // timout and a second time without if we are sure that the handler actually started.
////            TimeSpan controlCWaitTime = new TimeSpan( 0, 0, 30 ); // 30 seconds
////            delegateData.CompletionEvent.WaitOne( controlCWaitTime, false );
////            if(!delegateData.DelegateStarted)
////            {
////                BCLDebug.Assert( false, "ThreadPool.QueueUserWorkItem did not execute the handler within 30 seconds." );
////                return false;
////            }
////            delegateData.CompletionEvent.WaitOne();
////            delegateData.CompletionEvent.Close();
////            return delegateData.Cancel;
////
////        }
////        return false;
////    }
////
////    // This is the worker delegate that is called on the Threadpool thread to fire the actual events. It must guarentee that it
////    // signals the caller on the ControlC thread so that it does not block indefinitely.
////    private static void ControlCDelegate( object data )
////    {
////        ControlCDelegateData controlCData = (ControlCDelegateData)data;
////        try
////        {
////            controlCData.DelegateStarted = true;
////            ConsoleCancelEventArgs args = new ConsoleCancelEventArgs( controlCData.ControlKey );
////            controlCData.CancelCallbacks( null, args );
////            controlCData.Cancel = args.Cancel;
////        }
////        finally
////        {
////            controlCData.CompletionEvent.Set();
////        }
////    }
////
////    // Note: hooking this event allows you to prevent Control-C from 
////    // killing a console app, which is somewhat surprising for users.
////    // Some permission seems appropriate.  We chose UI permission for lack
////    // of a better one.  However, we also applied host protection 
////    // permission here as well, for self-affecting process management.
////    // This allows hosts to prevent people from adding a handler for
////    // this event.
////    public static event ConsoleCancelEventHandler CancelKeyPress
////    {
////        [ResourceExposure( ResourceScope.Process )]
////        [ResourceConsumption( ResourceScope.Process )]
////        add
////        {
////            new UIPermission( UIPermissionWindow.SafeTopLevelWindows ).Demand();
////
////            lock(InternalSyncObject)
////            {
////                // Add this delegate to the pile.
////                _cancelCallbacks += value;
////
////                // If we haven't registered our control-C handler, do it.
////                if(_hooker == null)
////                {
////                    _hooker = new ControlCHooker();
////                    _hooker.Hook();
////                }
////            }
////        }
////        [ResourceExposure( ResourceScope.Process )]
////        [ResourceConsumption( ResourceScope.Process )]
////        remove
////        {
////            new UIPermission( UIPermissionWindow.SafeTopLevelWindows ).Demand();
////
////            lock(InternalSyncObject)
////            {
////                // If count was 0, call SetConsoleCtrlEvent to remove cb.
////                _cancelCallbacks -= value;
////                BCLDebug.Assert( _cancelCallbacks == null || _cancelCallbacks.GetInvocationList().Length > 0, "Teach Console::CancelKeyPress to handle a non-null but empty list of callbacks" );
////                if(_hooker != null && _cancelCallbacks == null)
////                    _hooker.Unhook();
////            }
////        }
////    }
////
////    [HostProtection( UI = true )]
////    [ResourceExposure( ResourceScope.Process )]
////    [ResourceConsumption( ResourceScope.Process )]
////    public static Stream OpenStandardError()
////    {
////        return OpenStandardError( _DefaultConsoleBufferSize );
////    }
////
////    [HostProtection( UI = true )]
////    [ResourceExposure( ResourceScope.Process )]
////    [ResourceConsumption( ResourceScope.Process )]
////    public static Stream OpenStandardError( int bufferSize )
////    {
////        if(bufferSize < 0)
////            throw new ArgumentOutOfRangeException( "bufferSize", Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegNum" ) );
////        return GetStandardFile( Win32Native.STD_ERROR_HANDLE,
////                               FileAccess.Write, bufferSize );
////    }
////
////    [HostProtection( UI = true )]
////    [ResourceExposure( ResourceScope.Process )]
////    [ResourceConsumption( ResourceScope.Process )]
////    public static Stream OpenStandardInput()
////    {
////        return OpenStandardInput( _DefaultConsoleBufferSize );
////    }
////
////    [HostProtection( UI = true )]
////    [ResourceExposure( ResourceScope.Process )]
////    [ResourceConsumption( ResourceScope.Process )]
////    public static Stream OpenStandardInput( int bufferSize )
////    {
////        if(bufferSize < 0)
////            throw new ArgumentOutOfRangeException( "bufferSize", Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegNum" ) );
////        return GetStandardFile( Win32Native.STD_INPUT_HANDLE,
////                               FileAccess.Read, bufferSize );
////    }
////
////    [HostProtection( UI = true )]
////    [ResourceExposure( ResourceScope.Process )]
////    [ResourceConsumption( ResourceScope.Process )]
////    public static Stream OpenStandardOutput()
////    {
////        return OpenStandardOutput( _DefaultConsoleBufferSize );
////    }
////
////    [HostProtection( UI = true )]
////    [ResourceExposure( ResourceScope.Process )]
////    [ResourceConsumption( ResourceScope.Process )]
////    public static Stream OpenStandardOutput( int bufferSize )
////    {
////        if(bufferSize < 0)
////            throw new ArgumentOutOfRangeException( "bufferSize", Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegNum" ) );
////        return GetStandardFile( Win32Native.STD_OUTPUT_HANDLE,
////                               FileAccess.Write, bufferSize );
////    }
////
////    [HostProtection( UI = true )]
////    [ResourceExposure( ResourceScope.AppDomain )]
////    public static void SetIn( TextReader newIn )
////    {
////        if(newIn == null)
////            throw new ArgumentNullException( "newIn" );
////        new SecurityPermission( SecurityPermissionFlag.UnmanagedCode ).Demand();
////
////        newIn = TextReader.Synchronized( newIn );
////        lock(InternalSyncObject)
////        {
////            _in = newIn;
////        }
////    }
////
////    [HostProtection( UI = true )]
////    [ResourceExposure( ResourceScope.AppDomain )]
////    public static void SetOut( TextWriter newOut )
////    {
////        if(newOut == null)
////            throw new ArgumentNullException( "newOut" );
////        new SecurityPermission( SecurityPermissionFlag.UnmanagedCode ).Demand();
////
////        _wasOutRedirected = true;
////        newOut = TextWriter.Synchronized( newOut );
////        lock(InternalSyncObject)
////        {
////            _out = newOut;
////        }
////    }
////
////    [HostProtection( UI = true )]
////    [ResourceExposure( ResourceScope.AppDomain )]
////    public static void SetError( TextWriter newError )
////    {
////        if(newError == null)
////            throw new ArgumentNullException( "newError" );
////        new SecurityPermission( SecurityPermissionFlag.UnmanagedCode ).Demand();
////
////        _wasErrorRedirected = true;
////        newError = TextWriter.Synchronized( newError );
////        lock(InternalSyncObject)
////        {
////            _error = newError;
////        }
////    }
////
////    [HostProtection( UI = true )]
////    public static int Read()
////    {
////        return In.Read();
////    }
////
////    [HostProtection( UI = true )]
////    public static String ReadLine()
////    {
////        return In.ReadLine();
////    }

        [HostProtection( UI = true )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        public static extern void WriteLine();
////    {
////        Out.WriteLine();
////    }
    
////    [HostProtection( UI = true )]
////    public static void WriteLine( bool value )
////    {
////        Out.WriteLine( value );
////    }
////
////    [HostProtection( UI = true )]
////    public static void WriteLine( char value )
////    {
////        Out.WriteLine( value );
////    }
////
////    [HostProtection( UI = true )]
////    public static void WriteLine( char[] buffer )
////    {
////        Out.WriteLine( buffer );
////    }
////
////    [HostProtection( UI = true )]
////    public static void WriteLine( char[] buffer, int index, int count )
////    {
////        Out.WriteLine( buffer, index, count );
////    }
////
////    [HostProtection( UI = true )]
////    public static void WriteLine( decimal value )
////    {
////        Out.WriteLine( value );
////    }

        [HostProtection( UI = true )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        public extern static void WriteLine( double value );
////    {
////        Out.WriteLine( value );
////    }
////
////    [HostProtection( UI = true )]
////    public static void WriteLine( float value )
////    {
////        Out.WriteLine( value );
////    }

        [HostProtection( UI = true )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        public extern static void WriteLine( int value );
////    {
////        Out.WriteLine( value );
////    }
////
////    [HostProtection( UI = true )]
////    [CLSCompliant( false )]
////    public static void WriteLine( uint value )
////    {
////        Out.WriteLine( value );
////    }
////
////    [HostProtection( UI = true )]
////    public static void WriteLine( long value )
////    {
////        Out.WriteLine( value );
////    }
////
////    [HostProtection( UI = true )]
////    [CLSCompliant( false )]
////    public static void WriteLine( ulong value )
////    {
////        Out.WriteLine( value );
////    }

        [HostProtection( UI = true )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        public extern static void WriteLine( Object value );
////    {
////        Out.WriteLine( value );
////    }

        [HostProtection( UI = true )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        public static extern void WriteLine( String value );
////    {
////        Out.WriteLine( value );
////    }

        [HostProtection( UI = true )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        public static extern void WriteLine( String format, Object arg0 );
////    {
////        Out.WriteLine( format, arg0 );
////    }

        [HostProtection( UI = true )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        public static extern void WriteLine( String format, Object arg0, Object arg1 );
////    {
////        Out.WriteLine( format, arg0, arg1 );
////    }

        [HostProtection( UI = true )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        public static extern void WriteLine( String format, Object arg0, Object arg1, Object arg2 );
////    {
////        Out.WriteLine( format, arg0, arg1, arg2 );
////    }
    
////    [HostProtection( UI = true )]
////    [CLSCompliant( false )]
////    public static void WriteLine( String format, Object arg0, Object arg1, Object arg2, Object arg3, __arglist )
////    {
////        Object[] objArgs;
////        int argCount;
////
////        ArgIterator args = new ArgIterator( __arglist );
////
////        //+4 to account for the 4 hard-coded arguments at the beginning of the list.
////        argCount = args.GetRemainingCount() + 4;
////
////        objArgs = new Object[argCount];
////
////        //Handle the hard-coded arguments
////        objArgs[0] = arg0;
////        objArgs[1] = arg1;
////        objArgs[2] = arg2;
////        objArgs[3] = arg3;
////
////        //Walk all of the args in the variable part of the argument list.
////        for(int i = 4; i < argCount; i++)
////        {
////            objArgs[i] = TypedReference.ToObject( args.GetNextArg() );
////        }
////
////        Out.WriteLine( format, objArgs );
////    }

        [HostProtection( UI = true )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        public static extern void WriteLine( String format, params Object[] arg );
////    {
////        Out.WriteLine( format, arg );
////    }
    
////    [HostProtection( UI = true )]
////    public static void Write( String format, Object arg0 )
////    {
////        Out.Write( format, arg0 );
////    }
////
////    [HostProtection( UI = true )]
////    public static void Write( String format, Object arg0, Object arg1 )
////    {
////        Out.Write( format, arg0, arg1 );
////    }
////
////    [HostProtection( UI = true )]
////    public static void Write( String format, Object arg0, Object arg1, Object arg2 )
////    {
////        Out.Write( format, arg0, arg1, arg2 );
////    }
////
////    [HostProtection( UI = true )]
////    [CLSCompliant( false )]
////    public static void Write( String format, Object arg0, Object arg1, Object arg2, Object arg3, __arglist )
////    {
////        Object[] objArgs;
////        int argCount;
////
////        ArgIterator args = new ArgIterator( __arglist );
////
////        //+4 to account for the 4 hard-coded arguments at the beginning of the list.
////        argCount = args.GetRemainingCount() + 4;
////
////        objArgs = new Object[argCount];
////
////        //Handle the hard-coded arguments
////        objArgs[0] = arg0;
////        objArgs[1] = arg1;
////        objArgs[2] = arg2;
////        objArgs[3] = arg3;
////
////        //Walk all of the args in the variable part of the argument list.
////        for(int i = 4; i < argCount; i++)
////        {
////            objArgs[i] = TypedReference.ToObject( args.GetNextArg() );
////        }
////
////        Out.Write( format, objArgs );
////    }
////
////
////    [HostProtection( UI = true )]
////    public static void Write( String format, params Object[] arg )
////    {
////        Out.Write( format, arg );
////    }
////
////    [HostProtection( UI = true )]
////    public static void Write( bool value )
////    {
////        Out.Write( value );
////    }
////
////    [HostProtection( UI = true )]
////    public static void Write( char value )
////    {
////        Out.Write( value );
////    }
////
////    [HostProtection( UI = true )]
////    public static void Write( char[] buffer )
////    {
////        Out.Write( buffer );
////    }
////
////    [HostProtection( UI = true )]
////    public static void Write( char[] buffer, int index, int count )
////    {
////        Out.Write( buffer, index, count );
////    }
////
////    [HostProtection( UI = true )]
////    public static void Write( double value )
////    {
////        Out.Write( value );
////    }
////
////    [HostProtection( UI = true )]
////    public static void Write( decimal value )
////    {
////        Out.Write( value );
////    }
////
////    [HostProtection( UI = true )]
////    public static void Write( float value )
////    {
////        Out.Write( value );
////    }
////
////    [HostProtection( UI = true )]
////    public static void Write( int value )
////    {
////        Out.Write( value );
////    }
////
////    [HostProtection( UI = true )]
////    [CLSCompliant( false )]
////    public static void Write( uint value )
////    {
////        Out.Write( value );
////    }
////
////    [HostProtection( UI = true )]
////    public static void Write( long value )
////    {
////        Out.Write( value );
////    }
////
////    [HostProtection( UI = true )]
////    [CLSCompliant( false )]
////    public static void Write( ulong value )
////    {
////        Out.Write( value );
////    }
////
////    [HostProtection( UI = true )]
////    public static void Write( Object value )
////    {
////        Out.Write( value );
////    }
////
////    [HostProtection( UI = true )]
////    public static void Write( String value )
////    {
////        Out.Write( value );
////    }
////
////    // Checks whether stdout or stderr are writable.  Do NOT pass
////    // stdin here.
////    [ResourceExposure( ResourceScope.Process )]
////    [ResourceConsumption( ResourceScope.Process )]
////    private static unsafe bool ConsoleHandleIsValid( SafeFileHandle handle )
////    {
////        // Do NOT call this method on stdin!
////
////        // Windows apps may have non-null valid looking handle values for 
////        // stdin, stdout and stderr, but they may not be readable or 
////        // writable.  Verify this by calling ReadFile & WriteFile in the 
////        // appropriate modes.
////        // This must handle console-less Windows apps.
////        if(handle.IsInvalid)
////            return false;  // WinCE would return here, if we ran on CE.
////
////        int bytesWritten;
////        byte junkByte = 0x41;
////        int r;
////        r = __ConsoleStream.WriteFile( handle, &junkByte, 0, out bytesWritten, IntPtr.Zero );
////        // In Win32 apps w/ no console, bResult should be 0 for failure.
////        return r != 0;
////    }
    }
}
