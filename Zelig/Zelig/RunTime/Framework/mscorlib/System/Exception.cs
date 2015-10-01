// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*=============================================================================
**
** Class: Exception
**
**
** Purpose: The base class for all exceptional conditions.
**
**
=============================================================================*/

namespace System
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
////using System.Runtime.Versioning;
////using System.Diagnostics;
////using System.Security.Permissions;
////using System.Security;
////using System.IO;
////using System.Text;
    using System.Reflection;
    using System.Collections;
    using System.Globalization;
////using MethodInfo = System.Reflection.MethodInfo;
////using MethodBase = System.Reflection.MethodBase;


    [Serializable]
    public class Exception /*: ISerializable*/
    {
        internal String      m_message;
        private  Exception   m_innerException;
////    private  Object      m_stackTrace;
////    private  IDictionary m_data;
////    private  String      m_helpURL;
////    private  String      m_className;             // Needed for serialization.
////    private  MethodBase  m_exceptionMethod;       // Needed for serialization.
////    private  String      m_exceptionMethodString; // Needed for serialization.
////    private  String      m_stackTraceString;      // Needed for serialization.
////    private  String      m_remoteStackTraceString;
////    private  int         m_remoteStackIndex;
////    private  String      m_source;         // Mainly used by VB.
////
////////#pragma warning disable 414  // Field is not used from managed.
////////    // m_dynamicMethods is an array of System.Resolver objects, used to keep
////////    // DynamicMethodDescs alive for the lifetime of the exception. We do this because
////////    // the m_stackTrace field holds MethodDescs, and a DynamicMethodDesc can be destroyed
////////    // unless a System.Resolver object roots it.
////////    private  Object      m_dynamicMethods;
////////#pragma warning restore 414


        public Exception()
        {
            m_message        = null;
////        m_stackTrace     = null;
////        m_dynamicMethods = null;
        }

        public Exception( String message )
        {
            m_message        = message;
////        m_stackTrace     = null;
////        m_dynamicMethods = null;
        }

        // Creates a new Exception.  All derived classes should
        // provide this constructor.
        // Note: the stack trace is not started until the exception
        // is thrown
        //
        public Exception( String message, Exception innerException )
        {
            m_message        = message;
            m_innerException = innerException;
////        m_stackTrace     = null;
////        m_dynamicMethods = null;
        }

////    protected Exception( SerializationInfo info, StreamingContext context )
////    {
////        if(info == null)
////        {
////            throw new ArgumentNullException( "info" );
////        }
////
////        m_message                =               info.GetString      ( "Message"                               );
////        m_innerException         = (Exception  )(info.GetValue       ( "InnerException", typeof( Exception   ) ));
////        m_data                   = (IDictionary)(info.GetValueNoThrow( "Data"          , typeof( IDictionary ) ));
////        m_helpURL                =               info.GetString      ( "HelpURL"                               );
////        m_className              =               info.GetString      ( "ClassName"                             );
////        m_exceptionMethodString  =               info.GetString      ( "ExceptionMethod"                       );
////        m_stackTraceString       =               info.GetString      ( "StackTraceString"                      );
////        m_remoteStackTraceString =               info.GetString      ( "RemoteStackTraceString"                );
////        m_remoteStackIndex       =               info.GetInt32       ( "RemoteStackIndex"                      );
////        m_source                 =               info.GetString      ( "Source"                                );
////
////        if(m_className == null)
////        {
////            throw new SerializationException( Environment.GetResourceString( "Serialization_InsufficientState" ) );
////        }
////
////        // If we are constructing a new exception after a cross-appdomain call...
////        if(context.State == StreamingContextStates.CrossAppDomain)
////        {
////            // ...this new exception may get thrown.  It is logically a re-throw, but
////            //  physically a brand-new exception.  Since the stack trace is cleared
////            //  on a new exception, the "_remoteStackTraceString" is provided to
////            //  effectively import a stack trace from a "remote" exception.  So,
////            //  move the _stackTraceString into the _remoteStackTraceString.  Note
////            //  that if there is an existing _remoteStackTraceString, it will be
////            //  preserved at the head of the new string, so everything works as
////            //  expected.
////            // Even if this exception is NOT thrown, things will still work as expected
////            //  because the StackTrace property returns the concatenation of the
////            //  _remoteStackTraceString and the _stackTraceString.
////            m_remoteStackTraceString = m_remoteStackTraceString + m_stackTraceString;
////            m_stackTraceString       = null;
////        }
////    }


        public virtual String Message
        {
            get
            {
////            if(m_message == null)
////            {
////                if(m_className == null)
////                {
////                    m_className = GetClassName();
////                }
////
////                return String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "Exception_WasThrown" ), m_className );
////            }
////            else
////            {
                    return m_message;
////            }
            }
        }

////    public virtual IDictionary Data
////    {
////        get
////        {
////            return GetDataInternal();
////        }
////    }
////
////    // This method is internal so that callers can't override which function they call.
////    internal IDictionary GetDataInternal()
////    {
////        if(m_data == null)
////        {
////            if(IsImmutableAgileException( this ))
////            {
////                m_data = new EmptyReadOnlyDictionaryInternal();
////            }
////            else
////            {
////                m_data = new ListDictionaryInternal();
////            }
////        }
////
////        return m_data;
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private static extern bool IsImmutableAgileException( Exception e );
////
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private extern String GetClassName();

        // Retrieves the lowest exception (inner most) for the given Exception.
        // This will traverse exceptions using the innerException property.
        //
        public virtual Exception GetBaseException()
        {
            Exception inner = InnerException;
            Exception back  = this;

            while(inner != null)
            {
                back  = inner;
                inner = inner.InnerException;
            }

            return back;
        }

        // Returns the inner exception contained in this exception
        //
        public Exception InnerException
        {
            get
            {
                return m_innerException;
            }
        }


////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    static extern unsafe private void* _InternalGetMethod( Object stackTrace );
////
////    static unsafe private RuntimeMethodHandle InternalGetMethod( Object stackTrace )
////    {
////        return new RuntimeMethodHandle( _InternalGetMethod( stackTrace ) );
////    }
////
////
////    public MethodBase TargetSite
////    {
////        get
////        {
////            return GetTargetSiteInternal();
////        }
////    }
////
////
////    // this function is provided as a private helper to avoid the security demand
////    private MethodBase GetTargetSiteInternal()
////    {
////        if(m_exceptionMethod != null)
////        {
////            return m_exceptionMethod;
////        }
////
////        if(m_stackTrace == null)
////        {
////            return null;
////        }
////
////        if(m_exceptionMethodString != null)
////        {
////            m_exceptionMethod = GetExceptionMethodFromString();
////        }
////        else
////        {
////            RuntimeMethodHandle method = InternalGetMethod( m_stackTrace ).GetTypicalMethodDefinition();
////
////            m_exceptionMethod = RuntimeType.GetMethodBase( method );
////        }
////
////        return m_exceptionMethod;
////    }
////
////    // Returns the stack trace as a string.  If no stack trace is
////    // available, null is returned.
////    public virtual String StackTrace
////    {
////        get
////        {
////            // if no stack trace, try to get one
////            if(m_stackTraceString != null)
////            {
////                return m_remoteStackTraceString + m_stackTraceString;
////            }
////
////            if(m_stackTrace == null)
////            {
////                return m_remoteStackTraceString;
////            }
////
////            // Obtain the stack trace string. Note that since Environment.GetStackTrace
////            // will add the path to the source file if the PDB is present and a demand
////            // for PathDiscoveryPermission succeeds, we need to make sure we don't store
////            // the stack trace string in the _stackTraceString member variable.
////            String tempStackTraceString = Environment.GetStackTrace( this, true );
////
////            return m_remoteStackTraceString + tempStackTraceString;
////        }
////    }
////
////    // Sets the help link for this exception.
////    // This should be in a URL/URN form, such as:
////    // "file:///C:/Applications/Bazzal/help.html#ErrorNum42"
////    // Changed to be a read-write String and not return an exception
////    public virtual String HelpLink
////    {
////        get
////        {
////            return m_helpURL;
////        }
////
////        set
////        {
////            m_helpURL = value;
////        }
////    }
////
////    public virtual String Source
////    {
////        get
////        {
////            if(m_source == null)
////            {
////                StackTrace st = new StackTrace( this, true );
////
////                if(st.FrameCount > 0)
////                {
////                    StackFrame sf     = st.GetFrame( 0 );
////                    MethodBase method = sf.GetMethod();
////
////                    m_source = method.Module.Assembly.GetSimpleName();
////                }
////            }
////
////            return m_source;
////        }
////
////        set
////        {
////            m_source = value;
////        }
////    }

    public override String ToString()
    {
        return Message;
////        String message = Message;
////        String s;
////
////        if(m_className == null)
////        {
////            m_className = GetClassName();
////        }
////
////        if(message == null || message.Length <= 0)
////        {
////            s = m_className;
////        }
////        else
////        {
////            s = m_className + ": " + message;
////        }
////
////        if(m_innerException != null)
////        {
////            s = s + " ---> " + m_innerException.ToString() + Environment.NewLine + "   " + Environment.GetResourceString( "Exception_EndOfInnerExceptionStack" );
////        }
////
////
////        if(StackTrace != null)
////        {
////            s += Environment.NewLine + StackTrace;
////        }
////
////        return s;
        }

////    private String GetExceptionMethodString()
////    {
////        MethodBase methBase = GetTargetSiteInternal();
////        if(methBase == null)
////        {
////            return null;
////        }
////
////        // Note that the newline separator is only a separator, chosen such that
////        //  it won't (generally) occur in a method name.  This string is used
////        //  only for serialization of the Exception Method.
////        char separator = '\n';
////
////        StringBuilder result = new StringBuilder();
////
////        if(methBase is ConstructorInfo)
////        {
////            RuntimeConstructorInfo rci = (RuntimeConstructorInfo)methBase;
////            Type                   t   = rci.ReflectedType;
////
////            result.Append( (int)MemberTypes.Constructor );
////            result.Append(      separator               );
////            result.Append(      rci.Name                );
////
////            if(t != null)
////            {
////                result.Append( separator           );
////                result.Append( t.Assembly.FullName );
////                result.Append( separator           );
////                result.Append( t.FullName          );
////            }
////
////            result.Append( separator      );
////            result.Append( rci.ToString() );
////        }
////        else
////        {
////            BCLDebug.Assert( methBase is MethodInfo, "[Exception.GetExceptionMethodString]methBase is MethodInfo" );
////
////            RuntimeMethodInfo rmi = (RuntimeMethodInfo)methBase;
////            Type              t   = rmi.DeclaringType;
////
////            result.Append( (int)MemberTypes.Method           );
////            result.Append(      separator                    );
////            result.Append(      rmi.Name                     );
////            result.Append(      separator                    );
////            result.Append(      rmi.Module.Assembly.FullName );
////            result.Append(      separator                    );
////            if(t != null)
////            {
////                result.Append( t.FullName );
////                result.Append( separator  );
////            }
////            result.Append( rmi.ToString() );
////        }
////
////        return result.ToString();
////    }
////
////    private MethodBase GetExceptionMethodFromString()
////    {
////        BCLDebug.Assert( m_exceptionMethodString != null, "Method string cannot be NULL!" );
////
////        String[] args = m_exceptionMethodString.Split( new char[] { '\0', '\n' } );
////        if(args.Length != 5)
////        {
////            throw new SerializationException();
////        }
////
////        SerializationInfo si = new SerializationInfo( typeof( MemberInfoSerializationHolder ), new FormatterConverter() );
////
////        si.AddValue( "MemberType"  , (int)Int32.Parse( args[0], CultureInfo.InvariantCulture ), typeof( Int32  ) );
////        si.AddValue( "Name"        ,                   args[1],                                 typeof( String ) );
////        si.AddValue( "AssemblyName",                   args[2],                                 typeof( String ) );
////        si.AddValue( "ClassName"   ,                   args[3]                                                   );
////        si.AddValue( "Signature"   ,                   args[4]                                                   );
////
////        MethodBase result;
////
////        StreamingContext sc = new StreamingContext( StreamingContextStates.All );
////        try
////        {
////            result = (MethodBase)new MemberInfoSerializationHolder( si, sc ).GetRealObject( sc );
////        }
////        catch(SerializationException)
////        {
////            result = null;
////        }
////        return result;
////    }
////
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter )]
////    public virtual void GetObjectData( SerializationInfo info, StreamingContext context )
////    {
////        String tempStackTraceString = m_stackTraceString;
////
////        if(info == null)
////        {
////            throw new ArgumentNullException( "info" );
////        }
////
////        if(m_className == null)
////        {
////            m_className = GetClassName();
////        }
////
////        if(m_stackTrace != null)
////        {
////            if(tempStackTraceString == null)
////            {
////                tempStackTraceString = Environment.GetStackTrace( this, true );
////            }
////
////            if(m_exceptionMethod == null)
////            {
////                RuntimeMethodHandle method = InternalGetMethod( m_stackTrace ).GetTypicalMethodDefinition();
////
////                m_exceptionMethod = RuntimeType.GetMethodBase( method );
////            }
////        }
////
////        if(m_source == null)
////        {
////            m_source = Source; // Set the Source information correctly before serialization
////        }
////
////        info.AddValue( "ClassName"             , m_className               , typeof( String      ) );
////        info.AddValue( "Message"               , m_message                 , typeof( String      ) );
////        info.AddValue( "Data"                  , m_data                    , typeof( IDictionary ) );
////        info.AddValue( "InnerException"        , m_innerException          , typeof( Exception   ) );
////        info.AddValue( "HelpURL"               , m_helpURL                 , typeof( String      ) );
////        info.AddValue( "StackTraceString"      , tempStackTraceString      , typeof( String      ) );
////        info.AddValue( "RemoteStackTraceString", m_remoteStackTraceString  , typeof( String      ) );
////        info.AddValue( "RemoteStackIndex"      , m_remoteStackIndex        , typeof( Int32       ) );
////        info.AddValue( "ExceptionMethod"       , GetExceptionMethodString(), typeof( String      ) );
////        info.AddValue( "Source"                , m_source                  , typeof( String      ) );
////    }
////
////    // This is used by remoting to preserve the server side stack trace
////    // by appending it to the message ... before the exception is rethrown
////    // at the client call site.
////    internal Exception PrepForRemoting()
////    {
////        String tmp = null;
////
////        if(m_remoteStackIndex == 0)
////        {
////            tmp = Environment.NewLine + "Server stack trace: " + Environment.NewLine
////                + StackTrace
////                + Environment.NewLine + Environment.NewLine
////                + "Exception rethrown at [" + m_remoteStackIndex + "]: " + Environment.NewLine;
////        }
////        else
////        {
////            tmp = StackTrace
////                + Environment.NewLine + Environment.NewLine
////                + "Exception rethrown at [" + m_remoteStackIndex + "]: " + Environment.NewLine;
////        }
////
////        m_remoteStackTraceString = tmp;
////        m_remoteStackIndex++;
////        return this;
////    }
////
////    // This is used by the runtime when re-throwing a managed exception.  It will
////    //  copy the stack trace to _remoteStackTraceString.
////    internal void InternalPreserveStackTrace()
////    {
////        string tmpStackTraceString = StackTrace;
////
////        if(tmpStackTraceString != null && tmpStackTraceString.Length > 0)
////        {
////            m_remoteStackTraceString = tmpStackTraceString + Environment.NewLine;
////        }
////
////        m_stackTrace       = null;
////        m_stackTraceString = null;
////    }
////
////    internal virtual String InternalToString()
////    {
////        try
////        {
////            SecurityPermission sp = new SecurityPermission( SecurityPermissionFlag.ControlEvidence | SecurityPermissionFlag.ControlPolicy );
////            sp.Assert();
////        }
////        catch
////        {
////            //under normal conditions there should be no exceptions
////            //however if something wrong happens we still can call the usual ToString
////        }
////        return ToString();
////    }

        // this method is required so Object.GetType is not made virtual by the compiler
        public new Type GetType()
        {
            return base.GetType();
        }

////    internal bool IsTransient
////    {
////        get
////        {
////            return nIsTransient( m_HResult );
////        }
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private extern static bool nIsTransient( int hr );


        // This piece of infrastructure exists to help avoid deadlocks
        // between parts of mscorlib that might throw an exception while
        // holding a lock that are also used by mscorlib's ResourceManager
        // instance.  As a special case of code that may throw while holding
        // a lock, we also need to fix our asynchronous exceptions to use
        // Win32 resources as well (assuming we ever call a managed
        // constructor on instances of them).  We should grow this set of
        // exception messages as we discover problems, then move the resources
        // involved to native code.
        internal enum ExceptionMessageKind
        {
            ThreadAbort       = 1,
            ThreadInterrupted = 2,
            OutOfMemory       = 3,
        }

        // See comment on ExceptionMessageKind
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal static extern String GetMessageFromNativeResources( ExceptionMessageKind kind );
        internal static String GetMessageFromNativeResources( ExceptionMessageKind kind )
        {
            //
            // BUGBUG: This needs to be implemented as an internal call.
            // 
            return kind.ToString();
        }
    }
}
