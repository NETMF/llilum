// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
namespace System.Security.Permissions
{
////using System.Security.Util;
////using System.IO;
////using System.Security.Policy;
////using System.Security.AccessControl;
////using System.Runtime.Remoting.Activation;
////using System.Text;
////using System.Runtime.Serialization.Formatters;
////using System.Threading;
////using System.Runtime.InteropServices;
////using System.Runtime.Remoting;
////using System.Runtime.Serialization;
////using System.Security.Cryptography.X509Certificates;
////using System.Runtime.Versioning;

    [Serializable]
    public enum SecurityAction
    {
        Demand            =  2, // Demand permission of all caller
        Assert            =  3, // Assert permission so callers don't need
        Deny              =  4, // Deny permissions so checks will fail
        PermitOnly        =  5, // Reduce permissions so check will fail
        LinkDemand        =  6, // Demand permission of caller
        InheritanceDemand =  7, // Demand permission of a subclass
        RequestMinimum    =  8, // Request minimum permissions to run
        RequestOptional   =  9, // Request optional additional permissions
        RequestRefuse     = 10, // Refuse to be granted these permissions
    }


    [Serializable]
    [AttributeUsage( AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Assembly, AllowMultiple = true, Inherited = false )]
    public abstract class SecurityAttribute : System.Attribute
    {
        /// <internalonly/>
        internal SecurityAction m_action;
        /// <internalonly/>
        internal bool           m_unrestricted;

        protected SecurityAttribute( SecurityAction action )
        {
            m_action = action;
        }

        public SecurityAction Action
        {
            get
            {
                return m_action;
            }

            set
            {
                m_action = value;
            }
        }

        public bool Unrestricted
        {
            get
            {
                return m_unrestricted;
            }

            set
            {
                m_unrestricted = value;
            }
        }

////    abstract public IPermission CreatePermission();
////
////    internal static unsafe IntPtr FindSecurityAttributeTypeHandle( String typeName )
////    {
////        PermissionSet.s_fullTrust.Assert();
////        Type t = Type.GetType( typeName, false, false );
////        if(t == null)
////            return IntPtr.Zero;
////        IntPtr typeHandle = t.TypeHandle.Value;
////        return typeHandle;
////    }
    }

    [Serializable]
    [AttributeUsage( AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Assembly, AllowMultiple = true, Inherited = false )]
    public abstract class CodeAccessSecurityAttribute : SecurityAttribute
    {
        protected CodeAccessSecurityAttribute( SecurityAction action ) : base( action )
        {
        }
    }

    [Serializable]
    [AttributeUsage( AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Assembly, AllowMultiple = true, Inherited = false )]
    sealed public class EnvironmentPermissionAttribute : CodeAccessSecurityAttribute
    {
        private String m_read  = null;
        private String m_write = null;

        public EnvironmentPermissionAttribute( SecurityAction action ) : base( action )
        {
        }

        public String Read
        {
            get { return m_read; }
            set { m_read = value; }
        }

        public String Write
        {
            get { return m_write; }
            set { m_write = value; }
        }

        public String All
        {
            get
            {
#if EXCEPTION_STRINGS
                throw new NotSupportedException( Environment.GetResourceString( "NotSupported_GetMethod" ) );
#else
                throw new NotSupportedException();
#endif
            }
            set { m_write = value; m_read = value; }
        }

////    public override IPermission CreatePermission()
////    {
////        if(m_unrestricted)
////        {
////            return new EnvironmentPermission( PermissionState.Unrestricted );
////        }
////        else
////        {
////            EnvironmentPermission perm = new EnvironmentPermission( PermissionState.None );
////            if(m_read != null)
////                perm.SetPathList( EnvironmentPermissionAccess.Read, m_read );
////            if(m_write != null)
////                perm.SetPathList( EnvironmentPermissionAccess.Write, m_write );
////            return perm;
////        }
////    }
    }

////[Serializable]
////[AttributeUsage( AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Assembly, AllowMultiple = true, Inherited = false )]
////sealed public class FileDialogPermissionAttribute : CodeAccessSecurityAttribute
////{
////    private FileDialogPermissionAccess m_access;
////
////    public FileDialogPermissionAttribute( SecurityAction action ) : base( action )
////    {
////    }
////
////    public bool Open
////    {
////        get { return (m_access & FileDialogPermissionAccess.Open) != 0; }
////        set { m_access = value ? m_access | FileDialogPermissionAccess.Open : m_access & ~FileDialogPermissionAccess.Open; }
////    }
////
////    public bool Save
////    {
////        get { return (m_access & FileDialogPermissionAccess.Save) != 0; }
////        set { m_access = value ? m_access | FileDialogPermissionAccess.Save : m_access & ~FileDialogPermissionAccess.Save; }
////    }
////
////    public override IPermission CreatePermission()
////    {
////        if(m_unrestricted)
////        {
////            return new FileDialogPermission( PermissionState.Unrestricted );
////        }
////        else
////        {
////            return new FileDialogPermission( m_access );
////        }
////    }
////}
////
////
////[Serializable]
////[AttributeUsage( AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Assembly, AllowMultiple = true, Inherited = false )]
////sealed public class FileIOPermissionAttribute : CodeAccessSecurityAttribute
////{
////    private String m_read          = null;
////    private String m_write         = null;
////    private String m_append        = null;
////    private String m_pathDiscovery = null;
////    private String m_viewAccess    = null;
////    private String m_changeAccess  = null;
////
////    [OptionalField( VersionAdded = 2 )]
////    private FileIOPermissionAccess m_allLocalFiles = FileIOPermissionAccess.NoAccess;
////
////    [OptionalField( VersionAdded = 2 )]
////    private FileIOPermissionAccess m_allFiles      = FileIOPermissionAccess.NoAccess;
////
////    public FileIOPermissionAttribute( SecurityAction action ) : base( action )
////    {
////    }
////
////    public String Read
////    {
////        get { return m_read; }
////        set { m_read = value; }
////    }
////
////    public String Write
////    {
////        get { return m_write; }
////        set { m_write = value; }
////    }
////
////    public String Append
////    {
////        get { return m_append; }
////        set { m_append = value; }
////    }
////
////    public String PathDiscovery
////    {
////        get { return m_pathDiscovery; }
////        set { m_pathDiscovery = value; }
////    }
////
////    public String ViewAccessControl
////    {
////        get { return m_viewAccess; }
////        set { m_viewAccess = value; }
////    }
////
////    public String ChangeAccessControl
////    {
////        get { return m_changeAccess; }
////        set { m_changeAccess = value; }
////    }
////
////    [Obsolete( "Please use the ViewAndModify property instead." )]
////    public String All
////    {
////        set { m_read = value; m_write = value; m_append = value; m_pathDiscovery = value; }
////        get { throw new NotSupportedException( Environment.GetResourceString( "NotSupported_GetMethod" ) ); }
////    }
////
////    // Read, Write, Append, PathDiscovery, but no ACL-related permissions
////    public String ViewAndModify
////    {
////        get { throw new NotSupportedException( Environment.GetResourceString( "NotSupported_GetMethod" ) ); }
////        set { m_read = value; m_write = value; m_append = value; m_pathDiscovery = value; }
////    }
////
////    public FileIOPermissionAccess AllFiles
////    {
////        get { return m_allFiles; }
////        set { m_allFiles = value; }
////    }
////
////    public FileIOPermissionAccess AllLocalFiles
////    {
////        get { return m_allLocalFiles; }
////        set { m_allLocalFiles = value; }
////    }
////
////    public override IPermission CreatePermission()
////    {
////        if(m_unrestricted)
////        {
////            return new FileIOPermission( PermissionState.Unrestricted );
////        }
////        else
////        {
////            FileIOPermission perm = new FileIOPermission( PermissionState.None );
////            if(m_read != null)
////                perm.SetPathList( FileIOPermissionAccess.Read, m_read );
////            if(m_write != null)
////                perm.SetPathList( FileIOPermissionAccess.Write, m_write );
////            if(m_append != null)
////                perm.SetPathList( FileIOPermissionAccess.Append, m_append );
////            if(m_pathDiscovery != null)
////                perm.SetPathList( FileIOPermissionAccess.PathDiscovery, m_pathDiscovery );
////            if(m_viewAccess != null)
////                perm.SetPathList( FileIOPermissionAccess.NoAccess, AccessControlActions.View, new String[] { m_viewAccess }, false );
////            if(m_changeAccess != null)
////                perm.SetPathList( FileIOPermissionAccess.NoAccess, AccessControlActions.Change, new String[] { m_changeAccess }, false );
////
////            perm.AllFiles = m_allFiles;
////            perm.AllLocalFiles = m_allLocalFiles;
////            return perm;
////        }
////    }
////}
////
////[Serializable]
////[AttributeUsage( AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Assembly, AllowMultiple = true, Inherited = false )]
////public sealed class KeyContainerPermissionAttribute : CodeAccessSecurityAttribute
////{
////    KeyContainerPermissionFlags m_flags = KeyContainerPermissionFlags.NoFlags;
////    private string              m_keyStore;
////    private string              m_providerName;
////    private int                 m_providerType = -1;
////    private string              m_keyContainerName;
////    private int                 m_keySpec = -1;
////
////    public KeyContainerPermissionAttribute( SecurityAction action ) : base( action )
////    {
////    }
////
////    public string KeyStore
////    {
////        get { return m_keyStore; }
////        set { m_keyStore = value; }
////    }
////
////    public string ProviderName
////    {
////        get { return m_providerName; }
////        set { m_providerName = value; }
////    }
////
////    public int ProviderType
////    {
////        get { return m_providerType; }
////        set { m_providerType = value; }
////    }
////
////    public string KeyContainerName
////    {
////        get { return m_keyContainerName; }
////        set { m_keyContainerName = value; }
////    }
////
////    public int KeySpec
////    {
////        get { return m_keySpec; }
////        set { m_keySpec = value; }
////    }
////
////    public KeyContainerPermissionFlags Flags
////    {
////        get { return m_flags; }
////        set { m_flags = value; }
////    }
////
////    public override IPermission CreatePermission()
////    {
////        if(m_unrestricted)
////        {
////            return new KeyContainerPermission( PermissionState.Unrestricted );
////        }
////        else
////        {
////            if(KeyContainerPermissionAccessEntry.IsUnrestrictedEntry( m_keyStore, m_providerName, m_providerType, m_keyContainerName, m_keySpec ))
////                return new KeyContainerPermission( m_flags );
////
////            // create a KeyContainerPermission with a single access entry.
////            KeyContainerPermission cp = new KeyContainerPermission( KeyContainerPermissionFlags.NoFlags );
////            KeyContainerPermissionAccessEntry accessEntry = new KeyContainerPermissionAccessEntry( m_keyStore, m_providerName, m_providerType, m_keyContainerName, m_keySpec, m_flags );
////            cp.AccessEntries.Add( accessEntry );
////            return cp;
////        }
////    }
////}
////
////// PrincipalPermissionAttribute currently derives from
////// CodeAccessSecurityAttribute, even though it's not related to code access
////// security. This is because compilers are currently looking for
////// CodeAccessSecurityAttribute as a direct parent class rather than
////// SecurityAttribute as the root class.
////[Serializable]
////[AttributeUsage( AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true, Inherited = false )]
////sealed public class PrincipalPermissionAttribute : CodeAccessSecurityAttribute
////{
////    private String m_name          = null;
////    private String m_role          = null;
////    private bool   m_authenticated = true;
////
////    public PrincipalPermissionAttribute( SecurityAction action ) : base( action )
////    {
////    }
////
////    public String Name
////    {
////        get { return m_name; }
////        set { m_name = value; }
////    }
////
////    public String Role
////    {
////        get { return m_role; }
////        set { m_role = value; }
////    }
////
////    public bool Authenticated
////    {
////        get { return m_authenticated; }
////        set { m_authenticated = value; }
////    }
////
////
////    public override IPermission CreatePermission()
////    {
////        if(m_unrestricted)
////        {
////            return new PrincipalPermission( PermissionState.Unrestricted );
////        }
////        else
////        {
////            return new PrincipalPermission( m_name, m_role, m_authenticated );
////        }
////    }
////}
////
////
////
////[Serializable]
////[AttributeUsage( AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Assembly, AllowMultiple = true, Inherited = false )]
////sealed public class ReflectionPermissionAttribute : CodeAccessSecurityAttribute
////{
////    private ReflectionPermissionFlag m_flag = ReflectionPermissionFlag.NoFlags;
////
////    public ReflectionPermissionAttribute( SecurityAction action ) : base( action )
////    {
////    }
////
////    public ReflectionPermissionFlag Flags
////    {
////        get { return m_flag; }
////        set { m_flag = value; }
////    }
////
////    [Obsolete( "This API has been deprecated. http://go.microsoft.com/fwlink/?linkid=14202" )]
////    public bool TypeInformation
////    {
////    #pragma warning disable 618
////        get { return (m_flag & ReflectionPermissionFlag.TypeInformation) != 0; }
////        set { m_flag = value ? m_flag | ReflectionPermissionFlag.TypeInformation : m_flag & ~ReflectionPermissionFlag.TypeInformation; }
////    #pragma warning restore 618
////    }
////
////    public bool MemberAccess
////    {
////        get { return (m_flag & ReflectionPermissionFlag.MemberAccess) != 0; }
////        set { m_flag = value ? m_flag | ReflectionPermissionFlag.MemberAccess : m_flag & ~ReflectionPermissionFlag.MemberAccess; }
////    }
////
////    public bool ReflectionEmit
////    {
////        get { return (m_flag & ReflectionPermissionFlag.ReflectionEmit) != 0; }
////        set { m_flag = value ? m_flag | ReflectionPermissionFlag.ReflectionEmit : m_flag & ~ReflectionPermissionFlag.ReflectionEmit; }
////    }
////
////    public override IPermission CreatePermission()
////    {
////        if(m_unrestricted)
////        {
////            return new ReflectionPermission( PermissionState.Unrestricted );
////        }
////        else
////        {
////            return new ReflectionPermission( m_flag );
////        }
////    }
////}
////
////[Serializable]
////[AttributeUsage( AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Assembly, AllowMultiple = true, Inherited = false )]
////sealed public class RegistryPermissionAttribute : CodeAccessSecurityAttribute
////{
////    private String m_read      = null;
////    private String m_write     = null;
////    private String m_create    = null;
////    private String m_viewAcl   = null;
////    private String m_changeAcl = null;
////
////    public RegistryPermissionAttribute( SecurityAction action ) : base( action )
////    {
////    }
////
////    public String Read
////    {
////        get { return m_read; }
////        set { m_read = value; }
////    }
////
////    public String Write
////    {
////        get { return m_write; }
////        set { m_write = value; }
////    }
////
////    public String Create
////    {
////        get { return m_create; }
////        set { m_create = value; }
////    }
////
////    public String ViewAccessControl
////    {
////        get { return m_viewAcl; }
////        set { m_viewAcl = value; }
////    }
////
////    public String ChangeAccessControl
////    {
////        get { return m_changeAcl; }
////        set { m_changeAcl = value; }
////    }
////
////    // Read, Write, & Create, but no ACL's
////    public String ViewAndModify
////    {
////        get { throw new NotSupportedException( Environment.GetResourceString( "NotSupported_GetMethod" ) ); }
////        set { m_read = value; m_write = value; m_create = value; }
////    }
////
////    [Obsolete( "Please use the ViewAndModify property instead." )]
////    public String All
////    {
////        get { throw new NotSupportedException( Environment.GetResourceString( "NotSupported_GetMethod" ) ); }
////        set { m_read = value; m_write = value; m_create = value; }
////    }
////
////    public override IPermission CreatePermission()
////    {
////        if(m_unrestricted)
////        {
////            return new RegistryPermission( PermissionState.Unrestricted );
////        }
////        else
////        {
////            RegistryPermission perm = new RegistryPermission( PermissionState.None );
////            if(m_read != null)
////                perm.SetPathList( RegistryPermissionAccess.Read, m_read );
////            if(m_write != null)
////                perm.SetPathList( RegistryPermissionAccess.Write, m_write );
////            if(m_create != null)
////                perm.SetPathList( RegistryPermissionAccess.Create, m_create );
////            if(m_viewAcl != null)
////                perm.SetPathList( AccessControlActions.View, m_viewAcl );
////            if(m_changeAcl != null)
////                perm.SetPathList( AccessControlActions.Change, m_changeAcl );
////
////            return perm;
////        }
////    }
////}

    [Serializable]
    [AttributeUsage( AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Assembly, AllowMultiple = true, Inherited = false )]
    sealed public class SecurityPermissionAttribute : CodeAccessSecurityAttribute
    {
        private SecurityPermissionFlag m_flag = SecurityPermissionFlag.NoFlags;

        public SecurityPermissionAttribute( SecurityAction action ) : base( action )
        {
        }

        public SecurityPermissionFlag Flags
        {
            get { return m_flag; }
            set { m_flag = value; }
        }

        public bool Assertion
        {
            get { return (m_flag & SecurityPermissionFlag.Assertion) != 0; }
            set { m_flag = value ? m_flag | SecurityPermissionFlag.Assertion : m_flag & ~SecurityPermissionFlag.Assertion; }
        }

        public bool UnmanagedCode
        {
            get { return (m_flag & SecurityPermissionFlag.UnmanagedCode) != 0; }
            set { m_flag = value ? m_flag | SecurityPermissionFlag.UnmanagedCode : m_flag & ~SecurityPermissionFlag.UnmanagedCode; }
        }

        public bool SkipVerification
        {
            get { return (m_flag & SecurityPermissionFlag.SkipVerification) != 0; }
            set { m_flag = value ? m_flag | SecurityPermissionFlag.SkipVerification : m_flag & ~SecurityPermissionFlag.SkipVerification; }
        }

        public bool Execution
        {
            get { return (m_flag & SecurityPermissionFlag.Execution) != 0; }
            set { m_flag = value ? m_flag | SecurityPermissionFlag.Execution : m_flag & ~SecurityPermissionFlag.Execution; }
        }

        public bool ControlThread
        {
            get { return (m_flag & SecurityPermissionFlag.ControlThread) != 0; }
            set { m_flag = value ? m_flag | SecurityPermissionFlag.ControlThread : m_flag & ~SecurityPermissionFlag.ControlThread; }
        }

        public bool ControlEvidence
        {
            get { return (m_flag & SecurityPermissionFlag.ControlEvidence) != 0; }
            set { m_flag = value ? m_flag | SecurityPermissionFlag.ControlEvidence : m_flag & ~SecurityPermissionFlag.ControlEvidence; }
        }

        public bool ControlPolicy
        {
            get { return (m_flag & SecurityPermissionFlag.ControlPolicy) != 0; }
            set { m_flag = value ? m_flag | SecurityPermissionFlag.ControlPolicy : m_flag & ~SecurityPermissionFlag.ControlPolicy; }
        }

        public bool SerializationFormatter
        {
            get { return (m_flag & SecurityPermissionFlag.SerializationFormatter) != 0; }
            set { m_flag = value ? m_flag | SecurityPermissionFlag.SerializationFormatter : m_flag & ~SecurityPermissionFlag.SerializationFormatter; }
        }

        public bool ControlDomainPolicy
        {
            get { return (m_flag & SecurityPermissionFlag.ControlDomainPolicy) != 0; }
            set { m_flag = value ? m_flag | SecurityPermissionFlag.ControlDomainPolicy : m_flag & ~SecurityPermissionFlag.ControlDomainPolicy; }
        }

        public bool ControlPrincipal
        {
            get { return (m_flag & SecurityPermissionFlag.ControlPrincipal) != 0; }
            set { m_flag = value ? m_flag | SecurityPermissionFlag.ControlPrincipal : m_flag & ~SecurityPermissionFlag.ControlPrincipal; }
        }

        public bool ControlAppDomain
        {
            get { return (m_flag & SecurityPermissionFlag.ControlAppDomain) != 0; }
            set { m_flag = value ? m_flag | SecurityPermissionFlag.ControlAppDomain : m_flag & ~SecurityPermissionFlag.ControlAppDomain; }
        }

        public bool RemotingConfiguration
        {
            get { return (m_flag & SecurityPermissionFlag.RemotingConfiguration) != 0; }
            set { m_flag = value ? m_flag | SecurityPermissionFlag.RemotingConfiguration : m_flag & ~SecurityPermissionFlag.RemotingConfiguration; }
        }

        public bool Infrastructure
        {
            get { return (m_flag & SecurityPermissionFlag.Infrastructure) != 0; }
            set { m_flag = value ? m_flag | SecurityPermissionFlag.Infrastructure : m_flag & ~SecurityPermissionFlag.Infrastructure; }
        }

        public bool BindingRedirects
        {
            get { return (m_flag & SecurityPermissionFlag.BindingRedirects) != 0; }
            set { m_flag = value ? m_flag | SecurityPermissionFlag.BindingRedirects : m_flag & ~SecurityPermissionFlag.BindingRedirects; }
        }

////    public override IPermission CreatePermission()
////    {
////        if(m_unrestricted)
////        {
////            return new SecurityPermission( PermissionState.Unrestricted );
////        }
////        else
////        {
////            return new SecurityPermission( m_flag );
////        }
////    }
    }

////[Serializable]
////[AttributeUsage( AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Assembly, AllowMultiple = true, Inherited = false )]
////sealed public class UIPermissionAttribute : CodeAccessSecurityAttribute
////{
////    private UIPermissionWindow    m_windowFlag    = UIPermissionWindow.NoWindows;
////    private UIPermissionClipboard m_clipboardFlag = UIPermissionClipboard.NoClipboard;
////
////    public UIPermissionAttribute( SecurityAction action ) : base( action )
////    {
////    }
////
////    public UIPermissionWindow Window
////    {
////        get { return m_windowFlag; }
////        set { m_windowFlag = value; }
////    }
////
////    public UIPermissionClipboard Clipboard
////    {
////        get { return m_clipboardFlag; }
////        set { m_clipboardFlag = value; }
////    }
////
////    public override IPermission CreatePermission()
////    {
////        if(m_unrestricted)
////        {
////            return new UIPermission( PermissionState.Unrestricted );
////        }
////        else
////        {
////            return new UIPermission( m_windowFlag, m_clipboardFlag );
////        }
////    }
////}
////
////[Serializable]
////[AttributeUsage( AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Assembly, AllowMultiple = true, Inherited = false )]
////sealed public class ZoneIdentityPermissionAttribute : CodeAccessSecurityAttribute
////{
////    private SecurityZone m_flag = SecurityZone.NoZone;
////
////    public ZoneIdentityPermissionAttribute( SecurityAction action ) : base( action )
////    {
////    }
////
////    public SecurityZone Zone
////    {
////        get { return m_flag; }
////        set { m_flag = value; }
////    }
////
////    public override IPermission CreatePermission()
////    {
////        if(m_unrestricted)
////        {
////            return new ZoneIdentityPermission( PermissionState.Unrestricted );
////        }
////        else
////        {
////            return new ZoneIdentityPermission( m_flag );
////        }
////    }
////}
////
////[Serializable]
////[AttributeUsage( AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Assembly, AllowMultiple = true, Inherited = false )]
////sealed public class StrongNameIdentityPermissionAttribute : CodeAccessSecurityAttribute
////{
////    private String m_name    = null;
////    private String m_version = null;
////    private String m_blob    = null;
////
////    public StrongNameIdentityPermissionAttribute( SecurityAction action ) : base( action )
////    {
////    }
////
////    public String Name
////    {
////        get { return m_name; }
////        set { m_name = value; }
////    }
////
////    public String Version
////    {
////        get { return m_version; }
////        set { m_version = value; }
////    }
////
////    public String PublicKey
////    {
////        get { return m_blob; }
////        set { m_blob = value; }
////    }
////
////    public override IPermission CreatePermission()
////    {
////        if(m_unrestricted)
////        {
////            return new StrongNameIdentityPermission( PermissionState.Unrestricted );
////        }
////        else
////        {
////            if(m_blob == null && m_name == null && m_version == null)
////                return new StrongNameIdentityPermission( PermissionState.None );
////
////            if(m_blob == null)
////                throw new ArgumentException( Environment.GetResourceString( "ArgumentNull_Key" ) );
////
////            StrongNamePublicKeyBlob blob = new StrongNamePublicKeyBlob( m_blob );
////
////            if(m_version == null || m_version.Equals( String.Empty ))
////                return new StrongNameIdentityPermission( blob, m_name, null );
////            else
////                return new StrongNameIdentityPermission( blob, m_name, new Version( m_version ) );
////        }
////    }
////}
////
////
////[Serializable]
////[AttributeUsage( AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Assembly, AllowMultiple = true, Inherited = false )]
////sealed public class SiteIdentityPermissionAttribute : CodeAccessSecurityAttribute
////{
////    private String m_site = null;
////
////    public SiteIdentityPermissionAttribute( SecurityAction action ) : base( action )
////    {
////    }
////
////    public String Site
////    {
////        get { return m_site; }
////        set { m_site = value; }
////    }
////
////    public override IPermission CreatePermission()
////    {
////        if(m_unrestricted)
////        {
////            return new SiteIdentityPermission( PermissionState.Unrestricted );
////        }
////        else
////        {
////            if(m_site == null)
////                return new SiteIdentityPermission( PermissionState.None );
////
////            return new SiteIdentityPermission( m_site );
////        }
////    }
////}
////
////[Serializable]
////[AttributeUsage( AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Assembly, AllowMultiple = true, Inherited = false )]
////sealed public class UrlIdentityPermissionAttribute : CodeAccessSecurityAttribute
////{
////    private String m_url = null;
////
////    public UrlIdentityPermissionAttribute( SecurityAction action ) : base( action )
////    {
////    }
////
////    public String Url
////    {
////        get { return m_url; }
////        set { m_url = value; }
////    }
////
////    public override IPermission CreatePermission()
////    {
////        if(m_unrestricted)
////        {
////            return new UrlIdentityPermission( PermissionState.Unrestricted );
////        }
////        else
////        {
////            if(m_url == null)
////                return new UrlIdentityPermission( PermissionState.None );
////
////            return new UrlIdentityPermission( m_url );
////        }
////    }
////}
////
////[Serializable]
////[AttributeUsage( AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Assembly, AllowMultiple = true, Inherited = false )]
////sealed public class PublisherIdentityPermissionAttribute : CodeAccessSecurityAttribute
////{
////    private String m_x509cert   = null;
////    private String m_certFile   = null;
////    private String m_signedFile = null;
////
////    public PublisherIdentityPermissionAttribute( SecurityAction action ) : base( action )
////    {
////        m_x509cert = null;
////        m_certFile = null;
////        m_signedFile = null;
////    }
////
////    public String X509Certificate
////    {
////        get { return m_x509cert; }
////        set { m_x509cert = value; }
////    }
////
////    public String CertFile
////    {
////        get { return m_certFile; }
////        [ResourceExposure( ResourceScope.Machine )]
////        [ResourceConsumption( ResourceScope.Machine )]
////        set { m_certFile = value; }
////    }
////
////    public String SignedFile
////    {
////        get { return m_signedFile; }
////        [ResourceExposure( ResourceScope.Machine )]
////        [ResourceConsumption( ResourceScope.Machine )]
////        set { m_signedFile = value; }
////    }
////
////    [ResourceExposure( ResourceScope.Machine )]
////    [ResourceConsumption( ResourceScope.Machine )]
////    public override IPermission CreatePermission()
////    {
////        if(m_unrestricted)
////        {
////            return new PublisherIdentityPermission( PermissionState.Unrestricted );
////        }
////        else
////        {
////            if(m_x509cert != null)
////            {
////                return new PublisherIdentityPermission( new X509Certificate( System.Security.Util.Hex.DecodeHexString( m_x509cert ) ) );
////            }
////            else if(m_certFile != null)
////            {
////                return new PublisherIdentityPermission( System.Security.Cryptography.X509Certificates.X509Certificate.CreateFromCertFile( m_certFile ) );
////            }
////            else if(m_signedFile != null)
////            {
////                return new PublisherIdentityPermission( System.Security.Cryptography.X509Certificates.X509Certificate.CreateFromSignedFile( m_signedFile ) );
////            }
////            else
////            {
////                return new PublisherIdentityPermission( PermissionState.None );
////            }
////        }
////    }
////}
////
////[Serializable]
////[AttributeUsage( AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Assembly, AllowMultiple = true, Inherited = false )]
////public abstract class IsolatedStoragePermissionAttribute : CodeAccessSecurityAttribute
////{
////    /// <internalonly/>
////    internal long                       m_userQuota;
////    /// <internalonly/>
////    internal IsolatedStorageContainment m_allowed;
////
////    protected IsolatedStoragePermissionAttribute( SecurityAction action ) : base( action )
////    {
////    }
////
////    // properties
////    public long UserQuota
////    {
////        set
////        {
////            m_userQuota = value;
////        }
////        get
////        {
////            return m_userQuota;
////        }
////    }
////
////    public IsolatedStorageContainment UsageAllowed
////    {
////        set
////        {
////            m_allowed = value;
////        }
////        get
////        {
////            return m_allowed;
////        }
////    }
////}
////
////[Serializable]
////[AttributeUsage( AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Assembly, AllowMultiple = true, Inherited = false )]
////sealed public class IsolatedStorageFilePermissionAttribute : IsolatedStoragePermissionAttribute
////{
////    public IsolatedStorageFilePermissionAttribute( SecurityAction action ) : base( action )
////    {
////
////    }
////    public override IPermission CreatePermission()
////    {
////        IsolatedStorageFilePermission p;
////        if(m_unrestricted)
////        {
////            p = new IsolatedStorageFilePermission
////                    ( PermissionState.Unrestricted );
////        }
////        else
////        {
////            p = new IsolatedStorageFilePermission( PermissionState.None );
////            p.UserQuota = m_userQuota;
////            p.UsageAllowed = m_allowed;
////        }
////        return p;
////    }
////}
////
////[Serializable]
////[AttributeUsage( AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Assembly, AllowMultiple = true, Inherited = false )]
////sealed public class PermissionSetAttribute : CodeAccessSecurityAttribute
////{
////    private String m_file;
////    private String m_name;
////    private bool   m_unicode;
////    private String m_xml;
////    private String m_hex;
////
////    public PermissionSetAttribute( SecurityAction action ) : base( action )
////    {
////        m_unicode = false;
////    }
////
////    public String File
////    {
////        get { return m_file; }
////        [ResourceExposure( ResourceScope.Machine )]
////        [ResourceConsumption( ResourceScope.Machine )]
////        set { m_file = value; }
////    }
////
////    public bool UnicodeEncoded
////    {
////        get { return m_unicode; }
////        set { m_unicode = value; }
////    }
////
////    public String Name
////    {
////        get { return m_name; }
////        set { m_name = value; }
////    }
////
////    public String XML
////    {
////        get { return m_xml; }
////        set { m_xml = value; }
////    }
////
////    public String Hex
////    {
////        get { return m_hex; }
////        set { m_hex = value; }
////    }
////
////    public override IPermission CreatePermission()
////    {
////        return null;
////    }
////
////    private PermissionSet BruteForceParseStream( Stream stream )
////    {
////        Encoding[] encodings = new Encoding[] { Encoding.UTF8, Encoding.ASCII, Encoding.Unicode };
////
////        StreamReader reader = null;
////        Exception exception = null;
////
////        for(int i = 0; reader == null && i < encodings.Length; ++i)
////        {
////            try
////            {
////                stream.Position = 0;
////                reader = new StreamReader( stream, encodings[i] );
////
////                return ParsePermissionSet( new Parser( reader ) );
////            }
////            catch(Exception e1)
////            {
////                if(exception == null)
////                    exception = e1;
////            }
////        }
////
////        throw exception;
////    }
////
////    private PermissionSet ParsePermissionSet( Parser parser )
////    {
////        SecurityElement e = parser.GetTopElement();
////        PermissionSet permSet = new PermissionSet( PermissionState.None );
////        permSet.FromXml( e );
////
////        return permSet;
////    }
////
////    [ResourceExposure( ResourceScope.Machine )]
////    [ResourceConsumption( ResourceScope.Machine )]
////    public PermissionSet CreatePermissionSet()
////    {
////        if(m_unrestricted)
////            return new PermissionSet( PermissionState.Unrestricted );
////        else if(m_name != null)
////            return PolicyLevel.GetBuiltInSet( m_name );
////        else if(m_xml != null)
////            return ParsePermissionSet( new Parser( m_xml.ToCharArray() ) );
////        else if(m_hex != null)
////            return BruteForceParseStream( new MemoryStream( Util.Hex.DecodeHexString( m_hex ) ) );
////        else if(m_file != null)
////            return BruteForceParseStream( new FileStream( m_file, FileMode.Open, FileAccess.Read ) );
////        else
////            return new PermissionSet( PermissionState.None );
////    }
////}
}
