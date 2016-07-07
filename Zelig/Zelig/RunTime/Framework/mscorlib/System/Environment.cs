// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*============================================================
**
** Class: Environment
**
**
** Purpose: Provides some basic access to some environment
** functionality.
**
**
============================================================*/
namespace System
{
////using System.IO;
////using System.Security;
////using System.Resources;
    using System.Globalization;
    using System.Collections;
////using System.Security.Permissions;
////using System.Text;
    using System.Configuration.Assemblies;
    using System.Runtime.InteropServices;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Reflection;
    using System.Diagnostics;
    using System.Threading;
////using System.Runtime.Versioning;
////using Microsoft.Win32;

////public enum EnvironmentVariableTarget
////{
////    Process = 0,
////    User    = 1,
////    Machine = 2,
////}

    public static class Environment
    {
////    // Assume the following constants include the terminating '\0' - use <, not <=
////    const int MaxEnvVariableValueLength = 32767;  // maximum length for environment variable name and value
////
////    // System environment variables are stored in the registry, and have
////    // a size restriction that is separate from both normal environment
////    // variables and registry value name lengths, according to MSDN.
////    // MSDN doesn't detail whether the name is limited to 1024, or whether
////    // that includes the contents of the environment variable.
////    const int MaxSystemEnvVariableLength = 1024;
////
////    internal sealed class ResourceHelper
////    {
////        private ResourceManager SystemResMgr;
////
////        // To avoid infinite loops when calling GetResourceString.  See comments
////        // in GetResourceString for this field.
////        private Stack           currentlyLoading;
////
////        // process-wide state (since this is only used in one domain),
////        // used to avoid the TypeInitialization infinite recusion
////        // in GetResourceStringCode
////        internal bool           resourceManagerInited = false;
////
////        internal class GetResourceStringUserData
////        {
////            public ResourceHelper m_resourceHelper;
////            public String         m_key;
////            public String         m_retVal;
////            public bool           m_lockWasTaken;
////
////            public GetResourceStringUserData( ResourceHelper resourceHelper, String key )
////            {
////                m_resourceHelper = resourceHelper;
////                m_key            = key;
////            }
////        }
////
////        [ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
////        internal String GetResourceString( String key )
////        {
////            if(key == null || key.Length == 0)
////            {
////                BCLDebug.Assert( false, "Environment::GetResourceString with null or empty key.  Bug in caller, or weird recursive loading problem?" );
////                return "[Resource lookup failed - null or empty resource name]";
////            }
////
////            // We have a somewhat common potential for infinite
////            // loops with mscorlib's ResourceManager.  If "potentially dangerous"
////            // code throws an exception, we will get into an infinite loop
////            // inside the ResourceManager and this "potentially dangerous" code.
////            // Potentially dangerous code includes the IO package, CultureInfo,
////            // parts of the loader, some parts of Reflection, Security (including
////            // custom user-written permissions that may parse an XML file at
////            // class load time), assembly load event handlers, etc.  Essentially,
////            // this is not a bounded set of code, and we need to fix the problem.
////            // Fortunately, this is limited to mscorlib's error lookups and is NOT
////            // a general problem for all user code using the ResourceManager.
////
////            // The solution is to make sure only one thread at a time can call
////            // GetResourceString.  Also, since resource lookups can be
////            // reentrant, if the same thread comes into GetResourceString
////            // twice looking for the exact same resource name before
////            // returning, we're going into an infinite loop and we should
////            // return a bogus string.
////
////            GetResourceStringUserData userData = new GetResourceStringUserData( this, key );
////
////            RuntimeHelpers.TryCode     tryCode     = new RuntimeHelpers.TryCode    ( GetResourceStringCode        );
////            RuntimeHelpers.CleanupCode cleanupCode = new RuntimeHelpers.CleanupCode( GetResourceStringBackoutCode );
////
////            RuntimeHelpers.ExecuteCodeWithGuaranteedCleanup( tryCode, cleanupCode, userData );
////
////            return userData.m_retVal;
////        }
////
////        private void GetResourceStringCode( Object userDataIn )
////        {
////            GetResourceStringUserData userData = (GetResourceStringUserData)userDataIn;
////            ResourceHelper            rh       = userData.m_resourceHelper;
////            String                    key      = userData.m_key;
////
////            Monitor.ReliableEnter( rh, ref userData.m_lockWasTaken );
////
////            // Are we recursively looking up the same resource?
////            if(rh.currentlyLoading != null && rh.currentlyLoading.Count > 0 && rh.currentlyLoading.Contains( key ))
////            {
////                // This is often a bug in the BCL, security, NLS+ code,
////                // or the loader somewhere.  However, this could also
////                // be a setup problem - check whether mscorlib &
////                // mscorwks are both of the same build flavor.
////                String stackTrace = "[Couldn't get a stack trace]";
////
////                try
////                {
////                    StackTrace st = new StackTrace( true );
////
////                    // Don't attempt to localize strings in this stack trace, otherwise it could cause
////                    // infinite recursion. This stack trace is used for an Assert message only, and
////                    // so the lack of localization should not be an issue.
////                    stackTrace = st.ToString( System.Diagnostics.StackTrace.TraceFormat.NoResourceLookup );
////                }
////                catch(StackOverflowException)
////                {
////                }
////                catch(NullReferenceException)
////                {
////                }
////                catch(OutOfMemoryException)
////                {
////                }
////
////                BCLDebug.Assert( false, "Infinite recursion during resource lookup.  Resource name: " + key + "\r\n" + stackTrace );
////
////                // Note: can't append the key name, since that may require
////                // an extra allocation...
////                userData.m_retVal = "[Resource lookup failed - infinite recursion or critical failure detected.]";
////                return;
////            }
////
////            if(rh.currentlyLoading == null)
////            {
////                rh.currentlyLoading = new Stack( 4 );
////            }
////
////            // Call class constructors preemptively, so that we cannot get into an infinite
////            // loop constructing a TypeInitializationException.  If this were omitted,
////            // we could get the Infinite recursion assert above by failing type initialization
////            // between the Push and Pop calls below.
////
////            if(!rh.resourceManagerInited)
////            {
////                // process-critical code here.  No ThreadAbortExceptions
////                // can be thrown here.  Other exceptions percolate as normal.
////                RuntimeHelpers.PrepareConstrainedRegions();
////                try
////                {
////                }
////                finally
////                {
////                    RuntimeHelpers.RunClassConstructor( typeof( ResourceManager    ).TypeHandle );
////                    RuntimeHelpers.RunClassConstructor( typeof( ResourceReader     ).TypeHandle );
////                    RuntimeHelpers.RunClassConstructor( typeof( RuntimeResourceSet ).TypeHandle );
////                    RuntimeHelpers.RunClassConstructor( typeof( BinaryReader       ).TypeHandle );
////
////                    rh.resourceManagerInited = true;
////                }
////
////            }
////
////            rh.currentlyLoading.Push( key );
////
////            if(rh.SystemResMgr == null)
////            {
////                rh.SystemResMgr = new ResourceManager( "mscorlib", typeof( Object ).Assembly );
////            }
////
////            String s = rh.SystemResMgr.GetString( key, null );
////
////            rh.currentlyLoading.Pop();
////
////            BCLDebug.Assert( s != null, "Managed resource string lookup failed.  Was your resource name misspelled?  Did you rebuild mscorlib after adding a resource to resources.txt?  Debug this w/ cordbg and bug whoever owns the code that called rhironment.GetResourceString.  Resource name was: \"" + key + "\"" );
////
////            userData.m_retVal = s;
////        }
////
////        [PrePrepareMethod]
////        private void GetResourceStringBackoutCode( Object userDataIn, bool exceptionThrown )
////        {
////            GetResourceStringUserData userData = (GetResourceStringUserData)userDataIn;
////            ResourceHelper            rh       = userData.m_resourceHelper;
////
////            if(exceptionThrown)
////            {
////                if(userData.m_lockWasTaken)
////                {
////                    // Backout code - throw away potentially corrupt state
////                    rh.SystemResMgr     = null;
////                    rh.currentlyLoading = null;
////                }
////            }
////
////            // Release the lock, if we took it.
////            if(userData.m_lockWasTaken)
////            {
////                Monitor.Exit( rh );
////            }
////        }
////    }
////
////
////    private static ResourceHelper m_resHelper;  // Doesn't need to be initialized as they're zero-init.
////
////    private const int MaxMachineNameLength = 256;
////
////    // Private object for locking instead of locking on a public type for SQL reliability work.
////    private static Object s_InternalSyncObject;
////    private static Object InternalSyncObject
////    {
////        [ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
////        get
////        {
////            if(s_InternalSyncObject == null)
////            {
////                Object o = new Object();
////
////                Interlocked.CompareExchange( ref s_InternalSyncObject, o, null );
////            }
////
////            return s_InternalSyncObject;
////        }
////    }
////
////
////    private static OperatingSystem m_os;  // Cached OperatingSystem value
////    private static OSName          m_osname;
    
        /*==================================TickCount===================================
        **Action: Gets the number of ticks since the system was started.
        **Returns: The number of ticks since the system was started.
        **Arguments: None
        **Exceptions: None
        ==============================================================================*/
        public extern static int TickCount
        {
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
////        {
////            return nativeGetTickCount();
////        }
        }
    
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private static extern int nativeGetTickCount();
////
////    // Terminates this process with the given exit code.
////    [ResourceExposure( ResourceScope.Process )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private static extern void ExitNative( int exitCode );
////
////    [ResourceExposure( ResourceScope.Process )]
////    [ResourceConsumption( ResourceScope.Process )]
////    [SecurityPermissionAttribute( SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static void Exit( int exitCode )
////    {
////        ExitNative( exitCode );
////    }
////
////
////    public static int ExitCode
////    {
////        get
////        {
////            return nativeGetExitCode();
////        }
////
////        set
////        {
////            nativeSetExitCode( value );
////        }
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private static extern void nativeSetExitCode( int exitCode );
////
////    // Gets the exit code of the process.
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private static extern int nativeGetExitCode();
////
////
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    [ResourceExposure( ResourceScope.Process )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    public static extern void FailFast( String message );
////
////
////    public static String CommandLine
////    {
////        get
////        {
////            new EnvironmentPermission( EnvironmentPermissionAccess.Read, "Path" ).Demand();
////
////            return GetCommandLineNative();
////        }
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private static extern String GetCommandLineNative();
////
////    /*===============================CurrentDirectory===============================
////    **Action:  Provides a getter and setter for the current directory.  The original
////    **         current directory is the one from which the process was started.
////    **Returns: The current directory (from the getter).  Void from the setter.
////    **Arguments: The current directory to which to switch to the setter.
////    **Exceptions:
////    ==============================================================================*/
////    public static String CurrentDirectory
////    {
////        [ResourceExposure( ResourceScope.Machine )]
////        [ResourceConsumption( ResourceScope.Machine )]
////        get
////        {
////            return Directory.GetCurrentDirectory();
////        }
////
////        [ResourceExposure( ResourceScope.Machine )]
////        [ResourceConsumption( ResourceScope.Machine )]
////        set
////        {
////            Directory.SetCurrentDirectory( value );
////        }
////    }
////
////    // Returns the system directory (ie, C:\WinNT\System32).
////    public static String SystemDirectory
////    {
////        [ResourceExposure( ResourceScope.Machine )]
////        [ResourceConsumption( ResourceScope.Machine )]
////        get
////        {
////            StringBuilder sb = new StringBuilder( Path.MAX_PATH );
////            int r = Win32Native.GetSystemDirectory( sb, Path.MAX_PATH );
////            BCLDebug.Assert( r < Path.MAX_PATH, "r < Path.MAX_PATH" );
////            if(r == 0)
////            {
////                __Error.WinIOError();
////            }
////
////            String path = sb.ToString();
////
////            // Do security check
////            new FileIOPermission( FileIOPermissionAccess.PathDiscovery, path ).Demand();
////
////            return path;
////        }
////    }
////
////    // Returns the windows directory (ie, C:\WinNT).
////    // Used by NLS+ custom culures only at the moment.
////    internal static String InternalWindowsDirectory
////    {
////        [ResourceExposure( ResourceScope.Machine )]
////        [ResourceConsumption( ResourceScope.Machine )]
////        get
////        {
////            StringBuilder sb = new StringBuilder( Path.MAX_PATH );
////
////            int r = Win32Native.GetWindowsDirectory( sb, Path.MAX_PATH );
////            BCLDebug.Assert( r < Path.MAX_PATH, "r < Path.MAX_PATH" );
////
////            if(r == 0)
////            {
////                __Error.WinIOError();
////            }
////
////            String path = sb.ToString();
////
////            return path;
////        }
////    }
////
////    public static String ExpandEnvironmentVariables( String name )
////    {
////        if(name == null)
////        {
////            throw new ArgumentNullException( "name" );
////        }
////
////        if(name.Length == 0)
////        {
////            return name;
////        }
////
////        // Do a security check to guarantee we can read each of the
////        // individual environment variables requested here.
////        String[]      varArray      = name.Split( new char[] { '%' } );
////        StringBuilder vars          = new StringBuilder();
////
////        int           currentSize   = 100;
////        StringBuilder blob          = new StringBuilder( currentSize ); // A somewhat reasonable default size
////        int           size;
////        bool          fJustExpanded = false; // to accommodate expansion alg.
////
////        for(int i = 1; i < varArray.Length - 1; i++)
////        { // Skip first and last tokens
////            // ExpandEnvironmentStrings' greedy algorithm expands every
////            // non-boundary %-delimited substring, provided the previous
////            // has not been expanded.
////            // if "foo" is not expandable, and "PATH" is, then both
////            // %foo%PATH% and %foo%foo%PATH% will expand PATH, but
////            // %PATH%PATH% will expand only once.
////            // Therefore, if we've just expanded, skip this substring.
////            if(varArray[i].Length == 0 || fJustExpanded == true)
////            {
////                fJustExpanded = false;
////                continue; // Nothing to expand
////            }
////
////            // Guess a somewhat reasonable initial size, call the method, then if
////            // it fails (ie, the return value is larger than our buffer size),
////            // make a new buffer & try again.
////            blob.Length = 0;
////
////            String envVar = "%" + varArray[i] + "%";
////            size = Win32Native.ExpandEnvironmentStrings( envVar, blob, currentSize );
////            if(size == 0)
////            {
////                Marshal.ThrowExceptionForHR( Marshal.GetHRForLastWin32Error() );
////            }
////
////            // some environment variable might be changed while this function is called
////            while(size > currentSize)
////            {
////                currentSize = size;
////                blob.Capacity = currentSize;
////                blob.Length = 0;
////                size = Win32Native.ExpandEnvironmentStrings( envVar, blob, currentSize );
////                if(size == 0)
////                {
////                    Marshal.ThrowExceptionForHR( Marshal.GetHRForLastWin32Error() );
////                }
////            }
////
////            String temp = blob.ToString();
////            fJustExpanded = (temp != envVar);
////            if(fJustExpanded)
////            { // We expanded successfully, we need to do String comparision here
////                // since %FOO% can become %FOOD
////                vars.Append( varArray[i] );
////                vars.Append( ';' );
////            }
////        }
////
////        new EnvironmentPermission( EnvironmentPermissionAccess.Read, vars.ToString() ).Demand();
////
////        blob.Length = 0;
////        size = Win32Native.ExpandEnvironmentStrings( name, blob, currentSize );
////        if(size == 0)
////        {
////            Marshal.ThrowExceptionForHR( Marshal.GetHRForLastWin32Error() );
////        }
////
////        while(size > currentSize)
////        {
////            currentSize = size;
////
////            blob.Capacity = currentSize;
////            blob.Length   = 0;
////
////            size = Win32Native.ExpandEnvironmentStrings( name, blob, currentSize );
////            if(size == 0)
////            {
////                Marshal.ThrowExceptionForHR( Marshal.GetHRForLastWin32Error() );
////            }
////        }
////
////        return blob.ToString();
////    }
////
////    public static String MachineName
////    {
////        get
////        {
////            // In future release of operating systems, you might be able to rename a machine without
////            // rebooting.  Therefore, don't cache this machine name.
////            new EnvironmentPermission( EnvironmentPermissionAccess.Read, "COMPUTERNAME" ).Demand();
////
////            StringBuilder buf = new StringBuilder( MaxMachineNameLength );
////            int           len =                    MaxMachineNameLength;
////            if(Win32Native.GetComputerName( buf, ref len ) == 0)
////            {
////                throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_ComputerName" ) );
////            }
////
////            return buf.ToString();
////        }
////    }
////
////    public static int ProcessorCount
////    {
////        get
////        {
////            new EnvironmentPermission( EnvironmentPermissionAccess.Read, "NUMBER_OF_PROCESSORS" ).Demand();
////
////            Win32Native.SYSTEM_INFO info = new Win32Native.SYSTEM_INFO();
////
////            Win32Native.GetSystemInfo( ref info );
////
////            return info.dwNumberOfProcessors;
////        }
////    }
////
////    /*==============================GetCommandLineArgs==============================
////    **Action: Gets the command line and splits it appropriately to deal with whitespace,
////    **        quotes, and escape characters.
////    **Returns: A string array containing your command line arguments.
////    **Arguments: None
////    **Exceptions: None.
////    ==============================================================================*/
////    public static String[] GetCommandLineArgs()
////    {
////        new EnvironmentPermission( EnvironmentPermissionAccess.Read, "Path" ).Demand();
////
////        return GetCommandLineArgsNative();
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private static extern String[] GetCommandLineArgsNative();
////
////    // We need to keep this Fcall since it is used in AppDomain.cs.
////    // If we call GetEnvironmentVariable from AppDomain.cs, we will use StringBuilder class.
////    // That has side effect to change the ApartmentState of the calling Thread to MTA.
////    // So runtime can't change the ApartmentState of calling thread any more.
////    [ResourceExposure( ResourceScope.Process )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal static extern String nativeGetEnvironmentVariable( String variable );
////
////    /*============================GetEnvironmentVariable============================
////    **Action:
////    **Returns:
////    **Arguments:
////    **Exceptions:
////    ==============================================================================*/
////    [ResourceExposure( ResourceScope.Machine )]
////    [ResourceConsumption( ResourceScope.Machine )]
////    public static String GetEnvironmentVariable( String variable )
////    {
////        if(variable == null)
////        {
////            throw new ArgumentNullException( "variable" );
////        }
////
////        (new EnvironmentPermission( EnvironmentPermissionAccess.Read, variable )).Demand();
////
////        StringBuilder blob = new StringBuilder( 128 ); // A somewhat reasonable default size
////        int requiredSize = Win32Native.GetEnvironmentVariable( variable, blob, blob.Capacity );
////        if(requiredSize == 0)
////        {  //  GetEnvironmentVariable failed
////            if(Marshal.GetLastWin32Error() == Win32Native.ERROR_ENVVAR_NOT_FOUND)
////            {
////                return null;
////            }
////        }
////
////        while(requiredSize > blob.Capacity)
////        { // need to retry since the environment variable might be changed
////            blob.Capacity = requiredSize;
////            blob.Length   = 0;
////
////            requiredSize = Win32Native.GetEnvironmentVariable( variable, blob, blob.Capacity );
////        }
////
////        return blob.ToString();
////    }
////
////    [ResourceExposure( ResourceScope.Machine )]
////    [ResourceConsumption( ResourceScope.Machine )]
////    public static string GetEnvironmentVariable( string variable, EnvironmentVariableTarget target )
////    {
////        if(target == EnvironmentVariableTarget.Process)
////        {
////            return GetEnvironmentVariable( variable );
////        }
////
////        if(variable == null)
////        {
////            throw new ArgumentNullException( "variable" );
////        }
////
////        (new EnvironmentPermission( PermissionState.Unrestricted )).Demand();
////
////        if(target == EnvironmentVariableTarget.Machine)
////        {
////            using(RegistryKey environmentKey = Registry.LocalMachine.OpenSubKey( @"System\CurrentControlSet\Control\Session Manager\Environment", false ))
////            {
////                string value = environmentKey.GetValue( variable ) as string;
////                return value;
////            }
////        }
////        else if(target == EnvironmentVariableTarget.User)
////        {
////            using(RegistryKey environmentKey = Registry.CurrentUser.OpenSubKey( "Environment", false ))
////            {
////                string value = environmentKey.GetValue( variable ) as string;
////                return value;
////            }
////        }
////        else
////        {
////            throw new ArgumentException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "Arg_EnumIllegalVal" ), (int)target ) );
////        }
////    }
////
////    /*===========================GetEnvironmentVariables============================
////    **Action: Returns an IDictionary containing all enviroment variables and their values.
////    **Returns: An IDictionary containing all environment variables and their values.
////    **Arguments: None.
////    **Exceptions: None.
////    ==============================================================================*/
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    [ResourceExposure( ResourceScope.Machine )]
////    private static extern char[] nativeGetEnvironmentCharArray();
////
////    [ResourceExposure( ResourceScope.Machine )]
////    [ResourceConsumption( ResourceScope.Machine )]
////    public static IDictionary GetEnvironmentVariables()
////    {
////        // Using an FCall is better than using PInvoke here since
////        // Interop Marshaler can't handle string which contains '\0', we need
////        // to allocate an unmanaged buffer, do the conversion and free the buffer
////        // if we do use PInvoke.
////        char[] block = nativeGetEnvironmentCharArray();
////        if(block == null)
////        {
////            throw new OutOfMemoryException();
////        }
////
////        Hashtable     table = new Hashtable( 20 );
////        StringBuilder vars  = new StringBuilder();
////        // Copy strings out, parsing into pairs and inserting into the table.
////        // The first few environment variable entries start with an '='!
////        // The current working directory of every drive (except for those drives
////        // you haven't cd'ed into in your DOS window) are stored in the
////        // environment block (as =C:=pwd) and the program's exit code is
////        // as well (=ExitCode=00000000)  Skip all that start with =.
////        // Read docs about Environment Blocks on MSDN's CreateProcess page.
////
////        // Format for GetEnvironmentStrings is:
////        // (=HiddenVar=value\0 | Variable=value\0)* \0
////        // See the description of Environment Blocks in MSDN's
////        // CreateProcess page (null-terminated array of null-terminated strings).
////        // Note the =HiddenVar's aren't always at the beginning.
////
////        // GetEnvironmentCharArray will not return the trailing 0 to terminate
////        // the array - we have the array length instead.
////        bool first = true;
////        for(int i = 0; i < block.Length; i++)
////        {
////            int startKey = i;
////            // Skip to key
////            // On some old OS, the environment block can be corrupted.
////            // Someline will not have '=', so we need to check for '\0'.
////            while(block[i] != '=' && block[i] != '\0')
////            {
////                i++;
////            }
////
////            if(block[i] == '\0')
////            {
////                continue;
////            }
////
////            // Skip over environment variables starting with '='
////            if(i - startKey == 0)
////            {
////                while(block[i] != 0)
////                {
////                    i++;
////                }
////                continue;
////            }
////
////            String key = new String( block, startKey, i - startKey );
////            i++;  // skip over '='
////            int startValue = i;
////            while(block[i] != 0)
////            {
////                // Read to end of this entry
////                i++;
////            }
////
////            String value = new String( block, startValue, i - startValue );
////            // skip over 0 handled by for loop's i++
////            table[key] = value;
////
////            if(first)
////            {
////                first = false;
////            }
////            else
////            {
////                vars.Append( ';' );
////            }
////
////            vars.Append( key );
////        }
////
////        new EnvironmentPermission( EnvironmentPermissionAccess.Read, vars.ToString() ).Demand();
////        return table;
////    }
////
////
////    internal static IDictionary GetRegistryKeyNameValuePairs( RegistryKey registryKey )
////    {
////        Hashtable table = new Hashtable( 20 );
////        string[]  names = registryKey.GetValueNames();
////        foreach(string name in names)
////        {
////            string value = registryKey.GetValue( name, "" ).ToString();
////
////            table.Add( name, value );
////        }
////
////        return table;
////    }
////
////    [ResourceExposure( ResourceScope.Machine )]
////    [ResourceConsumption( ResourceScope.Machine )]
////    public static IDictionary GetEnvironmentVariables( EnvironmentVariableTarget target )
////    {
////        if(target == EnvironmentVariableTarget.Process)
////        {
////            return GetEnvironmentVariables();
////        }
////
////        (new EnvironmentPermission( PermissionState.Unrestricted )).Demand();
////
////        if(target == EnvironmentVariableTarget.Machine)
////        {
////            using(RegistryKey environmentKey = Registry.LocalMachine.OpenSubKey( @"System\CurrentControlSet\Control\Session Manager\Environment", false ))
////            {
////
////                return GetRegistryKeyNameValuePairs( environmentKey );
////            }
////        }
////        else if(target == EnvironmentVariableTarget.User)
////        {
////            using(RegistryKey environmentKey = Registry.CurrentUser.OpenSubKey( "Environment", false ))
////            {
////                return GetRegistryKeyNameValuePairs( environmentKey );
////            }
////        }
////        else
////        {
////            throw new ArgumentException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "Arg_EnumIllegalVal" ), (int)target ) );
////        }
////    }
////
////    [ResourceExposure( ResourceScope.Machine )]
////    [ResourceConsumption( ResourceScope.Machine )]
////    public static void SetEnvironmentVariable( string variable, string value )
////    {
////        CheckEnvironmentVariableName( variable );
////
////        new EnvironmentPermission( PermissionState.Unrestricted ).Demand();
////
////        // explicitly null out value if is the empty string.
////        if(String.IsNullOrEmpty( value ) || value[0] == '\0')
////        {
////            value = null;
////        }
////        else
////        {
////            if(value.Length >= MaxEnvVariableValueLength)
////            {
////                throw new ArgumentException( Environment.GetResourceString( "Argument_LongEnvVarValue" ) );
////            }
////        }
////
////        if(!Win32Native.SetEnvironmentVariable( variable, value ))
////        {
////            int errorCode = Marshal.GetLastWin32Error();
////
////            // Allow user to try to clear a environment variable
////            if(errorCode == Win32Native.ERROR_ENVVAR_NOT_FOUND)
////            {
////                return;
////            }
////
////            // The error message from Win32 is "The filename or extension is too long",
////            // which is not accurate.
////            if(errorCode == Win32Native.ERROR_FILENAME_EXCED_RANGE)
////            {
////                throw new ArgumentException( Environment.GetResourceString( "Argument_LongEnvVarValue" ) );
////            }
////
////            throw new ArgumentException( Win32Native.GetMessage( errorCode ) );
////        }
////    }
////
////    private static void CheckEnvironmentVariableName( string variable )
////    {
////        if(variable == null)
////        {
////            throw new ArgumentNullException( "variable" );
////        }
////
////        if(variable.Length == 0)
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Argument_StringZeroLength" ), "variable" );
////        }
////
////        if(variable[0] == '\0')
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Argument_StringFirstCharIsZero" ), "variable" );
////        }
////
////        // Make sure the environment variable name isn't longer than the
////        // max limit on environment variable values.  (MSDN is ambiguous
////        // on whether this check is necessary.)
////        if(variable.Length >= MaxEnvVariableValueLength)
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Argument_LongEnvVarValue" ) );
////        }
////
////        if(variable.IndexOf( '=' ) != -1)
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Argument_IllegalEnvVarName" ) );
////        }
////    }
////
////    [ResourceExposure( ResourceScope.Machine )]
////    [ResourceConsumption( ResourceScope.Machine )]
////    public static void SetEnvironmentVariable( string variable, string value, EnvironmentVariableTarget target )
////    {
////        if(target == EnvironmentVariableTarget.Process)
////        {
////            SetEnvironmentVariable( variable, value );
////            return;
////        }
////
////        CheckEnvironmentVariableName( variable );
////
////        // System-wide environment variables stored in the registry are
////        // limited to 1024 chars for the environment variable name.
////        if(variable.Length >= MaxSystemEnvVariableLength)
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Argument_LongEnvVarName" ) );
////        }
////
////        new EnvironmentPermission( PermissionState.Unrestricted ).Demand();
////
////        // explicitly null out value if is the empty string.
////        if(String.IsNullOrEmpty( value ) || value[0] == '\0')
////        {
////            value = null;
////        }
////
////        if(target == EnvironmentVariableTarget.Machine)
////        {
////            using(RegistryKey environmentKey = Registry.LocalMachine.OpenSubKey( @"System\CurrentControlSet\Control\Session Manager\Environment", true ))
////            {
////                if(value == null)
////                {
////                    environmentKey.DeleteValue( variable, false );
////                }
////                else
////                {
////                    environmentKey.SetValue( variable, value );
////                }
////            }
////        }
////        else if(target == EnvironmentVariableTarget.User)
////        {
////            using(RegistryKey environmentKey = Registry.CurrentUser.OpenSubKey( "Environment", true ))
////            {
////                if(value == null)
////                {
////                    environmentKey.DeleteValue( variable, false );
////                }
////                else
////                {
////                    environmentKey.SetValue( variable, value );
////                }
////            }
////        }
////        else
////        {
////            throw new ArgumentException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "Arg_EnumIllegalVal" ), (int)target ) );
////        }
////
////        // send a WM_SETTINGCHANGE message to all windows
////        IntPtr r = Win32Native.SendMessageTimeout( new IntPtr( Win32Native.HWND_BROADCAST ), Win32Native.WM_SETTINGCHANGE, IntPtr.Zero, "Environment", 0, 1000, IntPtr.Zero );
////
////        if(r == IntPtr.Zero)
////        {
////            BCLDebug.Assert( false, "SetEnvironmentVariable failed: " + Marshal.GetLastWin32Error() );
////        }
////    }
////
////
////    /*===============================GetLogicalDrives===============================
////    **Action: Retrieves the names of the logical drives on this machine in the  form "C:\".
////    **Arguments:   None.
////    **Exceptions:  IOException.
////    **Permissions: SystemInfo Permission.
////    ==============================================================================*/
////    public static String[] GetLogicalDrives()
////    {
////        new EnvironmentPermission( PermissionState.Unrestricted ).Demand();
////
////        int drives = Win32Native.GetLogicalDrives();
////        if(drives == 0)
////        {
////            __Error.WinIOError();
////        }
////
////        uint d     = (uint)drives;
////        int  count = 0;
////        while(d != 0)
////        {
////            if(((int)d & 1) != 0) count++;
////            d >>= 1;
////        }
////
////        String[] result = new String[count];
////        char[]   root   = new char[] { 'A', ':', '\\' };
////
////        d     = (uint)drives;
////        count = 0;
////        while(d != 0)
////        {
////            if(((int)d & 1) != 0)
////            {
////                result[count++] = new String( root );
////            }
////            d >>= 1;
////            root[0]++;
////        }
////
////        return result;
////    }

        /*===================================NewLine====================================
        **Action: A property which returns the appropriate newline string for the given
        **        platform.
        **Returns: \r\n on Win32.
        **Arguments: None.
        **Exceptions: None.
        ==============================================================================*/
        public static String NewLine
        {
            get
            {
#if !PLATFORM_UNIX
                return "\r\n";
#else
                return "\n";
#endif // !PLATFORM_UNIX
            }
        }
        ////
        ////
        ////    /*===================================Version====================================
        ////    **Action: Returns the COM+ version struct, describing the build number.
        ////    **Returns:
        ////    **Arguments:
        ////    **Exceptions:
        ////    ==============================================================================*/
        ////    public static Version Version
        ////    {
        ////        get
        ////        {
        ////            return new Version( ThisAssembly.InformationalVersion );
        ////        }
        ////    }
        ////
        ////
        ////    /*==================================WorkingSet==================================
        ////    **Action:
        ////    **Returns:
        ////    **Arguments:
        ////    **Exceptions:
        ////    ==============================================================================*/
        ////    [ResourceExposure( ResourceScope.None )]
        ////    [MethodImpl( MethodImplOptions.InternalCall )]
        ////    internal static extern long nativeGetWorkingSet();
        ////
        ////    public static long WorkingSet
        ////    {
        ////        get
        ////        {
        ////            new EnvironmentPermission( PermissionState.Unrestricted ).Demand();
        ////
        ////            return (long)nativeGetWorkingSet();
        ////        }
        ////    }
        ////
        ////    /*==================================OSVersion===================================
        ////    **Action:
        ////    **Returns:
        ////    **Arguments:
        ////    **Exceptions:
        ////    ==============================================================================*/
        ////    public static OperatingSystem OSVersion
        ////    {
        ////        get
        ////        {
        ////            if(m_os == null)
        ////            {
        ////                // We avoid the lock since we don't care if two threads will set this at the same time.
        ////                Microsoft.Win32.Win32Native.OSVERSIONINFO osvi = new Microsoft.Win32.Win32Native.OSVERSIONINFO();
        ////                if(!Win32Native.GetVersionEx( osvi ))
        ////                {
        ////                    throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_GetVersion" ) );
        ////                }
        ////
        ////                // Get Service pack information
        ////                Microsoft.Win32.Win32Native.OSVERSIONINFOEX osviEx = new Microsoft.Win32.Win32Native.OSVERSIONINFOEX();
        ////                if(!Win32Native.GetVersionEx( osviEx ))
        ////                {
        ////                    throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_GetVersion" ) );
        ////                }
        ////
        ////                PlatformID id;
        ////                switch(osvi.PlatformId)
        ////                {
        ////                    case Win32Native.VER_PLATFORM_WIN32_NT:
        ////                        id = PlatformID.Win32NT;
        ////                        break;
        ////
        ////                    default:
        ////                        BCLDebug.Assert( false, "Unsupported platform!" );
        ////                        throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_InvalidPlatformID" ) );
        ////                }
        ////
        ////                Version v = new Version( osvi.MajorVersion, osvi.MinorVersion, osvi.BuildNumber, (osviEx.ServicePackMajor << 16) | osviEx.ServicePackMinor );
        ////
        ////                m_os = new OperatingSystem( id, v, osvi.CSDVersion );
        ////            }
        ////
        ////            BCLDebug.Assert( m_os != null, "m_os != null" );
        ////            return m_os;
        ////        }
        ////    }
        ////
        ////    [Serializable]
        ////    internal enum OSName
        ////    {
        ////        Invalid = 0,
        ////        Unknown = 1,
        ////        WinNT   = 0x80,
        ////        Win2k   = 2 | WinNT
        ////    }
        ////
        ////    internal static OSName OSInfo
        ////    {
        ////        get
        ////        {
        ////            if(m_osname == OSName.Invalid)
        ////            {
        ////                lock(InternalSyncObject)
        ////                {
        ////                    if(m_osname == OSName.Invalid)
        ////                    {
        ////                        Microsoft.Win32.Win32Native.OSVERSIONINFO osvi = new Microsoft.Win32.Win32Native.OSVERSIONINFO();
        ////
        ////                        bool r = Win32Native.GetVersionEx( osvi );
        ////                        if(!r)
        ////                        {
        ////                            BCLDebug.Assert( r, "OSVersion native call failed." );
        ////                            throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_GetVersion" ) );
        ////                        }
        ////
        ////                        switch(osvi.PlatformId)
        ////                        {
        ////                            case Win32Native.VER_PLATFORM_WIN32_NT:
        ////                                switch(osvi.MajorVersion)
        ////                                {
        ////                                    case 5:
        ////                                        m_osname = OSName.Win2k;
        ////                                        break;
        ////
        ////                                    case 4:
        ////                                        BCLDebug.Assert( false, "NT4 is no longer a supported platform!" );
        ////                                        m_osname = OSName.Unknown; // Unknown OS
        ////                                        break;
        ////
        ////                                    default:
        ////                                        m_osname = OSName.WinNT;
        ////                                        break;
        ////                                }
        ////                                break;
        ////
        ////                            case Win32Native.VER_PLATFORM_WIN32_WINDOWS:
        ////                                BCLDebug.Assert( false, "Win9x is no longer a supported platform!" );
        ////                                m_osname = OSName.Unknown; // Unknown OS
        ////                                break;
        ////
        ////                            default:
        ////                                m_osname = OSName.Unknown; // Unknown OS
        ////                                break;
        ////
        ////                        }
        ////                    }
        ////                }
        ////            }
        ////            return m_osname;
        ////        }
        ////    }

        /*==================================StackTrace==================================
        **Action:
        **Returns:
        **Arguments:
        **Exceptions:
        ==============================================================================*/
        //////public static String StackTrace
        //////{
        //////    get
        //////    {
        //////        //////new EnvironmentPermission( PermissionState.Unrestricted ).Demand( );

        //////        return GetStackTrace( null, true );
        //////    }
        //////}

        //////internal static String GetStackTrace( Exception e, bool needFileInfo )
        //////{
        //////    // Note: Setting needFileInfo to true will start up COM and set our
        //////    // apartment state.  Try to not call this when passing "true"
        //////    // before the EE's ExecuteMainMethod has had a chance to set up the
        //////    // apartment state.  --
        //////    StackTrace st;
        //////    if(e == null)
        //////    {
        //////        st = new StackTrace( needFileInfo );
        //////    }
        //////    else
        //////    {
        //////        st = new StackTrace( e, needFileInfo );
        //////    }

        //////    // Do no include a trailing newline for backwards compatibility
        //////    return st.ToString( System.Diagnostics.StackTrace.TraceFormat.Normal );
        //////}

        ////    private static void InitResourceHelper()
        ////    {
        ////        // Only the default AppDomain should have a ResourceHelper.  All calls to
        ////        // GetResourceString from any AppDomain delegate to GetResourceStringLocal
        ////        // in the default AppDomain via the fcall GetResourceFromDefault.
        ////
        ////        // Use Thread.BeginCriticalRegion to tell the CLR all managed
        ////        // allocations within this block are appdomain-critical.
        ////        // Use a CER to ensure we always exit this region.
        ////        bool enteredRegion = false;
        ////        bool tookLock      = false;
        ////
        ////        RuntimeHelpers.PrepareConstrainedRegions();
        ////        try
        ////        {
        ////            RuntimeHelpers.PrepareConstrainedRegions();
        ////            try
        ////            {
        ////            }
        ////            finally
        ////            {
        ////                Thread.BeginCriticalRegion();
        ////                enteredRegion = true;
        ////
        ////                Monitor.Enter( Environment.InternalSyncObject );
        ////                tookLock = true;
        ////            }
        ////
        ////            if(m_resHelper == null)
        ////            {
        ////                ResourceHelper rh = new ResourceHelper();
        ////
        ////                System.Threading.Thread.MemoryBarrier();
        ////                m_resHelper = rh;
        ////            }
        ////        }
        ////        finally
        ////        {
        ////            if(tookLock)
        ////            {
        ////                Monitor.Exit( Environment.InternalSyncObject );
        ////            }
        ////
        ////            if(enteredRegion)
        ////            {
        ////                Thread.EndCriticalRegion();
        ////            }
        ////        }
        ////    }

        ////    [ResourceExposure( ResourceScope.None )]
        ////    [MethodImpl( MethodImplOptions.InternalCall )]
        internal static String GetResourceFromDefault( String key )
        {
            return key;
        }

////    // Looks up the resource string value for key.
////    //
////    // if you change this method's signature then you must change the code that calls it
////    // in excep.cpp and probably you will have to visit mscorlib.h to add the new signature
////    // as well as metasig.h to create the new signature type
////    internal static String GetResourceStringLocal( String key )
////    {
////        if(m_resHelper == null)
////        {
////            InitResourceHelper();
////        }
////
////        return m_resHelper.GetResourceString( key );
////    }

////    [ResourceExposure( ResourceScope.None )]
        internal static String GetResourceString( String key )
        {
            return GetResourceFromDefault( key );
        }

////    [ResourceExposure( ResourceScope.None )]
        internal static String GetResourceString( String key, params Object[] values )
        {
            String s = GetResourceFromDefault( key );

            return String.Format( CultureInfo.CurrentCulture, s, values );
        }

////    public static bool HasShutdownStarted
////    {
////        get
////        {
////            return nativeHasShutdownStarted();
////        }
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private static extern bool nativeHasShutdownStarted();
////
////    // This is the temporary Whidbey stub for compatibility flags
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal static extern bool GetCompatibilityFlag( CompatibilityFlag flag );
////
////    public static string UserName
////    {
////        get
////        {
////            new EnvironmentPermission( EnvironmentPermissionAccess.Read, "UserName" ).Demand();
////
////            StringBuilder sb   = new StringBuilder( 256 );
////            int           size = sb.Capacity;
////
////            Win32Native.GetUserName( sb, ref size );
////
////            return sb.ToString();
////        }
////    }
////
////    // Note that this is a handle to a process window station, but it does
////    // not need to be closed.  CloseWindowStation would ignore this handle.
////    // We also do handle equality checking as well.  This isn't a great fit
////    // for SafeHandle.  We don't gain anything by using SafeHandle here.
////    private static IntPtr processWinStation;        // Doesn't need to be initialized as they're zero-init.
////    private static bool   isUserNonInteractive;
////
////    public static bool UserInteractive
////    {
////        [ResourceExposure( ResourceScope.None )]
////        [ResourceConsumption( ResourceScope.Machine, ResourceScope.Machine )]
////        get
////        {
////            if((OSInfo & OSName.WinNT) == OSName.WinNT)
////            { // On WinNT
////                IntPtr hwinsta = Win32Native.GetProcessWindowStation();
////                if(hwinsta != IntPtr.Zero && processWinStation != hwinsta)
////                {
////                    int                         lengthNeeded = 0;
////                    Win32Native.USEROBJECTFLAGS flags        = new Win32Native.USEROBJECTFLAGS();
////
////                    if(Win32Native.GetUserObjectInformation( hwinsta, Win32Native.UOI_FLAGS, flags, Marshal.SizeOf( flags ), ref lengthNeeded ))
////                    {
////                        if((flags.dwFlags & Win32Native.WSF_VISIBLE) == 0)
////                        {
////                            isUserNonInteractive = true;
////                        }
////                    }
////                    processWinStation = hwinsta;
////                }
////            }
////            // The logic is reversed to avoid static initialization to true
////            return !isUserNonInteractive;
////        }
////    }
////
////    [ResourceExposure( ResourceScope.Machine )]
////    [ResourceConsumption( ResourceScope.Machine )]
////    public static string GetFolderPath( SpecialFolder folder )
////    {
////        if(!Enum.IsDefined( typeof( SpecialFolder ), folder ))
////        {
////            throw new ArgumentException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "Arg_EnumIllegalVal" ), (int)folder ) );
////        }
////
////        StringBuilder sb = new StringBuilder( Path.MAX_PATH );
////
////        Win32Native.SHGetFolderPath( IntPtr.Zero, (int)folder, IntPtr.Zero, Win32Native.SHGFP_TYPE_CURRENT, sb );
////
////        String s = sb.ToString();
////
////        new FileIOPermission( FileIOPermissionAccess.PathDiscovery, s ).Demand();
////
////        return s;
////    }
////
////    public static string UserDomainName
////    {
////        get
////        {
////            new EnvironmentPermission( EnvironmentPermissionAccess.Read, "UserDomain" ).Demand();
////
////            byte[] sid    = new byte[1024];
////            int    sidLen = sid.Length;
////
////            StringBuilder domainName    = new StringBuilder( 1024 );
////            int           domainNameLen = domainName.Capacity;
////            int           peUse;
////
////            byte ret = Win32Native.GetUserNameEx( Win32Native.NameSamCompatible, domainName, ref domainNameLen );
////            if(ret == 1)
////            {
////                string samName = domainName.ToString();
////                int index = samName.IndexOf( '\\' );
////                if(index != -1)
////                {
////                    return samName.Substring( 0, index );
////                }
////            }
////
////            domainNameLen = domainName.Capacity;
////
////            bool success = Win32Native.LookupAccountName( null, UserName, sid, ref sidLen, domainName, ref domainNameLen, out peUse );
////            if(!success)
////            {
////                int errorCode = Marshal.GetLastWin32Error();
////                throw new InvalidOperationException( Win32Native.GetMessage( errorCode ) );
////            }
////
////            return domainName.ToString();
////        }
////    }
////
////    public enum SpecialFolder
////    {
////        //
////        //      Represents the file system directory that serves as a common repository for
////        //       application-specific data for the current, roaming user.
////        //     A roaming user works on more than one computer on a network. A roaming user's
////        //       profile is kept on a server on the network and is loaded onto a system when the
////        //       user logs on.
////        //
////        ApplicationData       = Win32Native.CSIDL_APPDATA,
////        //
////        //      Represents the file system directory that serves as a common repository for application-specific data that
////        //       is used by all users.
////        //
////        CommonApplicationData = Win32Native.CSIDL_COMMON_APPDATA,
////        //
////        //     Represents the file system directory that serves as a common repository for application specific data that
////        //       is used by the current, non-roaming user.
////        //
////        LocalApplicationData  = Win32Native.CSIDL_LOCAL_APPDATA,
////        //
////        //     Represents the file system directory that serves as a common repository for Internet
////        //       cookies.
////        //
////        Cookies               = Win32Native.CSIDL_COOKIES,
////        Desktop               = Win32Native.CSIDL_DESKTOP,
////        //
////        //     Represents the file system directory that serves as a common repository for the user's
////        //       favorite items.
////        //
////        Favorites             = Win32Native.CSIDL_FAVORITES,
////        //
////        //     Represents the file system directory that serves as a common repository for Internet
////        //       history items.
////        //
////        History               = Win32Native.CSIDL_HISTORY,
////        //
////        //     Represents the file system directory that serves as a common repository for temporary
////        //       Internet files.
////        //
////        InternetCache         = Win32Native.CSIDL_INTERNET_CACHE,
////        //
////        //      Represents the file system directory that contains
////        //       the user's program groups.
////        //
////        Programs              = Win32Native.CSIDL_PROGRAMS,
////        MyComputer            = Win32Native.CSIDL_DRIVES,
////        MyMusic               = Win32Native.CSIDL_MYMUSIC,
////        MyPictures            = Win32Native.CSIDL_MYPICTURES,
////        //
////        //     Represents the file system directory that contains the user's most recently used
////        //       documents.
////        //
////        Recent                = Win32Native.CSIDL_RECENT,
////        //
////        //     Represents the file system directory that contains Send To menu items.
////        //
////        SendTo                = Win32Native.CSIDL_SENDTO,
////        //
////        //     Represents the file system directory that contains the Start menu items.
////        //
////        StartMenu             = Win32Native.CSIDL_STARTMENU,
////        //
////        //     Represents the file system directory that corresponds to the user's Startup program group. The system
////        //       starts these programs whenever any user logs on to Windows NT, or
////        //       starts Windows 95 or Windows 98.
////        //
////        Startup               = Win32Native.CSIDL_STARTUP,
////        //
////        //     System directory.
////        //
////        System                = Win32Native.CSIDL_SYSTEM,
////        //
////        //     Represents the file system directory that serves as a common repository for document
////        //       templates.
////        //
////        Templates             = Win32Native.CSIDL_TEMPLATES,
////        //
////        //     Represents the file system directory used to physically store file objects on the desktop.
////        //       This should not be confused with the desktop folder itself, which is
////        //       a virtual folder.
////        //
////        DesktopDirectory      = Win32Native.CSIDL_DESKTOPDIRECTORY,
////        //
////        //     Represents the file system directory that serves as a common repository for documents.
////        //
////        Personal              = Win32Native.CSIDL_PERSONAL,
////
////        // "MyDocuments" is a better name than "Personal"
////        MyDocuments           = Win32Native.CSIDL_PERSONAL,
////        //
////        //     Represents the program files folder.
////        //
////        ProgramFiles          = Win32Native.CSIDL_PROGRAM_FILES,
////        //
////        //     Represents the folder for components that are shared across applications.
////        //
////        CommonProgramFiles    = Win32Native.CSIDL_PROGRAM_FILES_COMMON,
////    }
    }
}
