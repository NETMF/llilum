// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==

namespace System.Reflection
{
    using System;
    using System.Reflection;
////using System.Reflection.Cache;
////using System.Reflection.Emit;
    using System.Globalization;
    using System.Threading;
////using System.Diagnostics;
////using System.Security.Permissions;
////using System.Collections;
////using System.Security;
////using System.Text;
    using System.Runtime.ConstrainedExecution;
////using System.Runtime.CompilerServices;
////using System.Runtime.InteropServices;
////using System.Runtime.Serialization;
////using System.Runtime.Versioning;
////using RuntimeTypeCache = System.RuntimeType.RuntimeTypeCache;
////using CorElementType   = System.Reflection.CorElementType;
////using MdToken          = System.Reflection.MetadataToken;

    internal enum MemberListType
    {
        All            ,
        CaseSensitive  ,
        CaseInsensitive,
        HandleToInfo   ,
    }

    [Serializable]
////[PermissionSetAttribute( SecurityAction.InheritanceDemand, Name = "FullTrust" )]
    public abstract class MemberInfo : ICustomAttributeProvider
    {
        #region Static Members
        public static bool operator ==( MemberInfo left,
                                        MemberInfo right )
        {
            if ((object)left == null)
            {
                return (object)right == null;
            }

            if ((object)right == null)
            {
                return false;
            }

            return left.Equals( right );
        }

        public static bool operator !=( MemberInfo left,
                                        MemberInfo right )
        {
            return !(left == right);
        }
        #endregion

        #region Consts
        //
        // Invocation cached flags. Those are used in unmanaged code as well
        // so be careful if you change them
        //
        internal const uint INVOCATION_FLAGS_UNKNOWN                 = 0x00000000;
        internal const uint INVOCATION_FLAGS_INITIALIZED             = 0x00000001;
        // it's used for both method and field to signify that no access is allowed
        internal const uint INVOCATION_FLAGS_NO_INVOKE               = 0x00000002;
        internal const uint INVOCATION_FLAGS_NEED_SECURITY           = 0x00000004;
        internal const uint INVOCATION_FLAGS_NO_CTOR_INVOKE          = 0x00000008;
        // because field and method are different we can reuse the same bits method
        internal const uint INVOCATION_FLAGS_IS_CTOR                 = 0x00000010;
        internal const uint INVOCATION_FLAGS_RISKY_METHOD            = 0x00000020;
        internal const uint INVOCATION_FLAGS_SECURITY_IMPOSED        = 0x00000040;
        internal const uint INVOCATION_FLAGS_IS_DELEGATE_CTOR        = 0x00000080;
        internal const uint INVOCATION_FLAGS_CONTAINS_STACK_POINTERS = 0x00000100;
        // field
        internal const uint INVOCATION_FLAGS_SPECIAL_FIELD           = 0x00000010;
        internal const uint INVOCATION_FLAGS_FIELD_SPECIAL_CAST      = 0x00000020;

        // temporary flag used for flagging invocation of method vs ctor
        // this flag never appears on the instance m_invocationFlag and is simply
        // passed down from within ConstructorInfo.Invoke()
        internal const uint INVOCATION_FLAGS_CONSTRUCTOR_INVOKE      = 0x10000000;
        #endregion

        #region Constructor
        protected MemberInfo()
        {
        }
        #endregion

        #region MemberInfo Overrides
        public override bool Equals( object obj )
        {
            if (obj == null)
            {
                return false;
            }
            if ((obj is MemberInfo) == false)
            {
                return false;
            }
            return this.GetHashCode() == obj.GetHashCode();
        }

        public override int GetHashCode()
        {
            return MetadataTokenInternal;
        }
        #endregion

        #region Internal Methods
////    internal virtual bool CacheEquals( object o )
////    {
////        throw new NotImplementedException();
////    }
        #endregion

        #region Legacy Remoting Cache
////    // The size of CachedData is accounted for by BaseObjectWithCachedData in object.h.
////    // This member is currently being used by Remoting for caching remoting data. If you
////    // need to cache data here, talk to the Remoting team to work out a mechanism, so that
////    // both caching systems can happily work together.
////    private InternalCache m_cachedData;
////
////    internal InternalCache Cache
////    {
////        get
////        {
////            // This grabs an internal copy of m_cachedData and uses
////            // that instead of looking at m_cachedData directly because
////            // the cache may get cleared asynchronously.  This prevents
////            // us from having to take a lock.
////            InternalCache cache = m_cachedData;
////            if(cache == null)
////            {
////                cache = new InternalCache( "MemberInfo" );
////
////                InternalCache ret = Interlocked.CompareExchange( ref m_cachedData, cache, null );
////                if(ret != null)
////                {
////                    cache = ret;
////                }
////
////                GC.ClearCache += new ClearCacheHandler( OnCacheClear );
////            }
////
////            return cache;
////        }
////    }
////
////
////    internal void OnCacheClear( Object sender, ClearCacheEventArgs cacheEventArgs )
////    {
////        m_cachedData = null;
////    }
        #endregion

        #region Public Abstract\Virtual Members
        public abstract MemberTypes MemberType
        {
            get;
        }

        public abstract String Name
        {
            get;
        }
    
        public abstract Type DeclaringType
        {
            get;
        }
    
////    public abstract Type ReflectedType
////    {
////        get;
////    }

        #region ICustomAttributeProvider
    
        public abstract Object[] GetCustomAttributes( bool inherit );
    
        public abstract Object[] GetCustomAttributes( Type attributeType, bool inherit );
    
        public abstract bool IsDefined( Type attributeType, bool inherit );
    
        #endregion

        public virtual int MetadataToken
        {
            get
            {
                throw new InvalidOperationException();
            }
        }

        internal virtual int MetadataTokenInternal
        {
            get
            {
                return MetadataToken;
            }
        }

        public virtual Module Module
        {
            get
            {
                if(this is Type)
                {
                    return ((Type)this).Module;
                }

                throw new NotImplementedException();
            }
        }

        #endregion
    }
}
