// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==

namespace System
{
    using System;
    using System.Reflection;
    using System.Runtime.ConstrainedExecution;
    using System.Globalization;
    using System.Threading;
    using System.Diagnostics;
////using System.Security.Permissions;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Runtime.CompilerServices;
////using System.Security;
////using System.Text;
////using System.Reflection.Emit;
////using System.Runtime.Remoting;
////using System.Runtime.Remoting.Proxies;
////using System.Runtime.Remoting.Messaging;
////using System.Runtime.Remoting.Activation;
    using System.Runtime.InteropServices;
////using System.Runtime.Versioning;
////using MdSigCallingConvention       = System.Signature.MdSigCallingConvention;
////using RuntimeTypeCache             = System.RuntimeType.RuntimeTypeCache;
////using StackCrawlMark               = System.Threading.StackCrawlMark;
////using DebuggerStepThroughAttribute = System.Diagnostics.DebuggerStepThroughAttribute;
////using MdToken                      = System.Reflection.MetadataToken;

    // this is a work around to get the concept of a calli. It's not as fast but it would be interesting to
    // see how it compares to the current implementation.
    // This delegate will disappear at some point in favor of calli

////internal delegate void CtorDelegate( Object instance );

    [Microsoft.Zelig.Internals.WellKnownType( "System_RuntimeType" )]
    [Serializable]
    internal class RuntimeType : Type/*, ISerializable, ICloneable*/
    {
        #region Definitions

////    [Serializable]
////    internal class RuntimeTypeCache
////    {
////        #region Definitions
////        internal enum WhatsCached
////        {
////            Nothing = 0x0,
////            EnclosingType = 0x1,
////        }
////
////        internal enum CacheType
////        {
////            Method,
////            Constructor,
////            Field,
////            Property,
////            Event,
////            Interface,
////            NestedType
////        }
////
////        // This method is purely an aid for NGen to statically deduce which
////        // instantiations to save in the ngen image.
////        // Otherwise, the JIT-compiler gets used, which is bad for working-set.
////        // Note that IBC can provide this information too.
////        // However, this helps in keeping the JIT-compiler out even for
////        // test scenarios which do not use IBC.
////        // This can be removed after V2, when we implement other schemes
////        // of keeping the JIT-compiler out for generic instantiations.
////        internal static void Prejitinit_HACK()
////        {
////            new MemberInfoCache<RuntimeMethodInfo>( null );
////            new MemberInfoCache<RuntimeConstructorInfo>( null );
////            new MemberInfoCache<RuntimeFieldInfo>( null );
////            new MemberInfoCache<RuntimeType>( null );
////            new MemberInfoCache<RuntimePropertyInfo>( null );
////            new MemberInfoCache<RuntimeEventInfo>( null );
////        }
////
////        private struct Filter
////        {
////            private Utf8String m_name;
////            private MemberListType m_listType;
////
////            public unsafe Filter( byte* pUtf8Name, int cUtf8Name, MemberListType listType )
////            {
////                this.m_name = new Utf8String( (void*)pUtf8Name, cUtf8Name );
////                this.m_listType = listType;
////            }
////
////            public bool Match( Utf8String name )
////            {
////                if(m_listType == MemberListType.CaseSensitive)
////                    return m_name.Equals( name );
////                else if(m_listType == MemberListType.CaseInsensitive)
////                    return m_name.EqualsCaseInsensitive( name );
////                else
////                    return true;
////            }
////        }
////
////        [Serializable]
////        private class MemberInfoCache<T> where T : MemberInfo
////        {
////            #region Static Members
////            static MemberInfoCache()
////            {
////                // We need to prepare some code in this class for reliable
////                // execution on a per-instantiation basis. A static class
////                // constructor is ideal for this since we only need to do
////                // this once per-instantiation. We can't go through the
////                // normal approach using RuntimeHelpers.PrepareMethod since
////                // that would involve using reflection and we'd wind back up
////                // here recursively. So we call through an fcall helper
////                // instead. The fcall is on RuntimeType to avoid having an
////                // fcall entry for a nested class (a generic one at that).
////                // I've no idea if that would work, but I'm pretty sure it
////                // wouldn't. Also we pass in our own base type since using
////                // the mscorlib binder doesn't work for nested types (I've
////                // tried that one).
////                PrepareMemberInfoCache( typeof( MemberInfoCache<T> ).TypeHandle );
////            }
////            #endregion
////
////            #region Private Data Members
////            // MemberInfo caches
////            private CerHashtable<string, CerArrayList<T>> m_csMemberInfos;
////            private CerHashtable<string, CerArrayList<T>> m_cisMemberInfos;
////            private CerArrayList<T> m_root;
////            private bool m_cacheComplete;
////
////            // This is the strong reference back to the cache
////            private RuntimeTypeCache m_runtimeTypeCache;
////            #endregion
////
////            #region Constructor
////            internal MemberInfoCache( RuntimeTypeCache runtimeTypeCache )
////            {
////                m_runtimeTypeCache = runtimeTypeCache;
////                m_cacheComplete = false;
////            }
////
////            internal MethodBase AddMethod( RuntimeTypeHandle declaringType, RuntimeMethodHandle method, CacheType cacheType )
////            {
////                Object list = null;
////                MethodAttributes methodAttributes = method.GetAttributes();
////                bool isPublic = (methodAttributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Public;
////                bool isStatic = (methodAttributes & MethodAttributes.Static) != 0;
////                bool isInherited = declaringType.Value != ReflectedTypeHandle.Value;
////                BindingFlags bindingFlags = RuntimeType.FilterPreCalculate( isPublic, isInherited, isStatic );
////                switch(cacheType)
////                {
////                    case CacheType.Method:
////                        List<RuntimeMethodInfo> mlist = new List<RuntimeMethodInfo>( 1 );
////                        mlist.Add( new RuntimeMethodInfo( method, declaringType, m_runtimeTypeCache, methodAttributes, bindingFlags ) );
////                        list = mlist;
////                        break;
////                    case CacheType.Constructor:
////                        List<RuntimeConstructorInfo> clist = new List<RuntimeConstructorInfo>( 1 );
////                        clist.Add( new RuntimeConstructorInfo( method, declaringType, m_runtimeTypeCache, methodAttributes, bindingFlags ) );
////                        list = clist;
////                        break;
////                }
////
////                CerArrayList<T> cerList = new CerArrayList<T>( (List<T>)list );
////
////                Insert( ref cerList, null, MemberListType.HandleToInfo );
////
////                return (MethodBase)(object)cerList[0];
////            }
////
////            internal FieldInfo AddField( RuntimeFieldHandle field )
////            {
////                // create the runtime field info
////                List<RuntimeFieldInfo> list = new List<RuntimeFieldInfo>( 1 );
////                FieldAttributes fieldAttributes = field.GetAttributes();
////                bool isPublic = (fieldAttributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Public;
////                bool isStatic = (fieldAttributes & FieldAttributes.Static) != 0;
////                bool isInherited = field.GetApproxDeclaringType().Value != ReflectedTypeHandle.Value;
////                BindingFlags bindingFlags = RuntimeType.FilterPreCalculate( isPublic, isInherited, isStatic );
////                list.Add( new RtFieldInfo( field, ReflectedType, m_runtimeTypeCache, bindingFlags ) );
////
////                CerArrayList<T> cerList = new CerArrayList<T>( (List<T>)(object)list );
////                Insert( ref cerList, null, MemberListType.HandleToInfo );
////
////                return (FieldInfo)(object)cerList[0];
////            }
////
////            private unsafe CerArrayList<T> Populate( string name, MemberListType listType, CacheType cacheType )
////            {
////                if(name == null || name.Length == 0 ||
////                    (cacheType == CacheType.Constructor && name.FirstChar != '.' && name.FirstChar != '*'))
////                {
////                    Filter filter = new Filter( null, 0, listType );
////                    List<T> list = null;
////
////                    switch(cacheType)
////                    {
////                        case CacheType.Method:
////                            list = PopulateMethods( filter ) as List<T>;
////                            break;
////                        case CacheType.Field:
////                            list = PopulateFields( filter ) as List<T>;
////                            break;
////                        case CacheType.Constructor:
////                            list = PopulateConstructors( filter ) as List<T>;
////                            break;
////                        case CacheType.Property:
////                            list = PopulateProperties( filter ) as List<T>;
////                            break;
////                        case CacheType.Event:
////                            list = PopulateEvents( filter ) as List<T>;
////                            break;
////                        case CacheType.NestedType:
////                            list = PopulateNestedClasses( filter ) as List<T>;
////                            break;
////                        case CacheType.Interface:
////                            list = PopulateInterfaces( filter ) as List<T>;
////                            break;
////                    }
////
////                    CerArrayList<T> cerList = new CerArrayList<T>( list );
////
////                    Insert( ref cerList, name, listType );
////
////                    return cerList;
////                }
////                else
////                {
////                    fixed(char* pName = name)
////                    {
////                        int cUtf8Name = Encoding.UTF8.GetByteCount( pName, name.Length );
////                        byte* pUtf8Name = stackalloc byte[cUtf8Name];
////                        Encoding.UTF8.GetBytes( pName, name.Length, pUtf8Name, cUtf8Name );
////
////                        Filter filter = new Filter( pUtf8Name, cUtf8Name, listType );
////                        List<T> list = null;
////
////                        switch(cacheType)
////                        {
////                            case CacheType.Method:
////                                list = PopulateMethods( filter ) as List<T>;
////                                break;
////                            case CacheType.Field:
////                                list = PopulateFields( filter ) as List<T>;
////                                break;
////                            case CacheType.Constructor:
////                                list = PopulateConstructors( filter ) as List<T>;
////                                break;
////                            case CacheType.Property:
////                                list = PopulateProperties( filter ) as List<T>;
////                                break;
////                            case CacheType.Event:
////                                list = PopulateEvents( filter ) as List<T>;
////                                break;
////                            case CacheType.NestedType:
////                                list = PopulateNestedClasses( filter ) as List<T>;
////                                break;
////                            case CacheType.Interface:
////                                list = PopulateInterfaces( filter ) as List<T>;
////                                break;
////                        }
////
////                        CerArrayList<T> cerList = new CerArrayList<T>( list );
////
////                        Insert( ref cerList, name, listType );
////
////                        return cerList;
////                    }
////                }
////            }
////
////            // May replace the list with a new one if certain cache
////            // lookups succeed.  Also, may modify the contents of the list
////            // after merging these new data structures with cached ones.
////            private void Insert( ref CerArrayList<T> list, string name, MemberListType listType )
////            {
////                bool lockTaken = false;
////                bool preallocationComplete = false;
////
////                RuntimeHelpers.PrepareConstrainedRegions();
////                try
////                {
////                    Monitor.ReliableEnter( this, ref lockTaken );
////
////                    if(listType == MemberListType.CaseSensitive)
////                    {
////                        if(m_csMemberInfos == null)
////                            m_csMemberInfos = new CerHashtable<string, CerArrayList<T>>();
////                        else
////                            m_csMemberInfos.Preallocate( 1 );
////                    }
////                    else if(listType == MemberListType.CaseInsensitive)
////                    {
////                        if(m_cisMemberInfos == null)
////                            m_cisMemberInfos = new CerHashtable<string, CerArrayList<T>>();
////                        else
////                            m_cisMemberInfos.Preallocate( 1 );
////                    }
////
////                    if(m_root == null)
////                        m_root = new CerArrayList<T>( list.Count );
////                    else
////                        m_root.Preallocate( list.Count );
////
////                    preallocationComplete = true;
////                }
////                finally
////                {
////                    try
////                    {
////                        if(preallocationComplete)
////                        {
////                            if(listType == MemberListType.CaseSensitive)
////                            {
////                                // Ensure we always return a list that has
////                                // been merged with the global list.
////                                CerArrayList<T> cachedList = m_csMemberInfos[name];
////                                if(cachedList == null)
////                                {
////                                    MergeWithGlobalList( list );
////                                    m_csMemberInfos[name] = list;
////                                }
////                                else
////                                    list = cachedList;
////                            }
////                            else if(listType == MemberListType.CaseInsensitive)
////                            {
////                                // Ensure we always return a list that has
////                                // been merged with the global list.
////                                CerArrayList<T> cachedList = m_cisMemberInfos[name];
////                                if(cachedList == null)
////                                {
////                                    MergeWithGlobalList( list );
////                                    m_cisMemberInfos[name] = list;
////                                }
////                                else
////                                    list = cachedList;
////                            }
////                            else
////                            {
////                                MergeWithGlobalList( list );
////                            }
////
////                            if(listType == MemberListType.All)
////                            {
////                                m_cacheComplete = true;
////                            }
////                        }
////                    }
////                    finally
////                    {
////                        if(lockTaken)
////                        {
////                            Monitor.Exit( this );
////                        }
////                    }
////                }
////            }
////
////            // Modifies the existing list.
////            [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
////            private void MergeWithGlobalList( CerArrayList<T> list )
////            {
////                int cachedCount = m_root.Count;
////
////                for(int i = 0; i < list.Count; i++)
////                {
////                    T newMemberInfo = list[i];
////                    T cachedMemberInfo = null;
////
////                    for(int j = 0; j < cachedCount; j++)
////                    {
////                        cachedMemberInfo = m_root[j];
////
////                        if(newMemberInfo.CacheEquals( cachedMemberInfo ))
////                        {
////                            list.Replace( i, cachedMemberInfo );
////                            break;
////                        }
////                    }
////
////                    if(list[i] != cachedMemberInfo)
////                        m_root.Add( newMemberInfo );
////                }
////            }
////            #endregion
////
////            #region Population Logic
////            private unsafe List<RuntimeMethodInfo> PopulateMethods( Filter filter )
////            {
////                List<RuntimeMethodInfo> list = new List<RuntimeMethodInfo>();
////
////                bool isInterface =
////                    (ReflectedTypeHandle.GetAttributes() & TypeAttributes.ClassSemanticsMask)
////                    == TypeAttributes.Interface;
////
////                if(isInterface)
////                {
////                    #region IsInterface
////                    RuntimeTypeHandle declaringInterfaceHandle = ReflectedTypeHandle;
////                    bool mayNeedInstantiatingStub = declaringInterfaceHandle.HasInstantiation() && !declaringInterfaceHandle.IsGenericTypeDefinition();
////                    MethodDescChunkHandle chunkHandle = declaringInterfaceHandle.GetMethodDescChunk();
////
////                    while(!chunkHandle.IsNullHandle())
////                    {
////                        int methodCount = chunkHandle.GetMethodCount();
////
////                        for(int i = 0; i < methodCount; i++)
////                        {
////                            RuntimeMethodHandle methodHandle = chunkHandle.GetMethodAt( i );
////
////                            if(!filter.Match( methodHandle.GetUtf8Name() ))
////                                continue;
////
////                            #region Loop through all methods on the interface
////                            ASSERT.CONSISTENCY_CHECK( !methodHandle.IsNullHandle() );
////                            ASSERT.CONSISTENCY_CHECK( LOGIC.IMPLIES(
////                                (methodHandle.GetAttributes() & MethodAttributes.RTSpecialName) != 0,
////                                methodHandle.GetName().Equals( ".ctor" ) ||
////                                methodHandle.GetName().Equals( ".cctor" ) ||
////                                methodHandle.GetName().Equals( "IL_STUB" ) ) );
////                            ASSERT.CONSISTENCY_CHECK( (methodHandle.GetAttributes() & MethodAttributes.Abstract) != 0 );
////                            ASSERT.CONSISTENCY_CHECK( (methodHandle.GetAttributes() & MethodAttributes.Virtual) != 0 );
////
////                            #region Calculate Binding Flags
////                            MethodAttributes methodAttributes = methodHandle.GetAttributes();
////                            bool isPublic = (methodAttributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Public;
////                            bool isStatic = (methodAttributes & MethodAttributes.Static) != 0;
////                            bool isInherited = false;
////                            BindingFlags bindingFlags = RuntimeType.FilterPreCalculate( isPublic, isInherited, isStatic );
////                            #endregion
////
////                            if((methodAttributes & MethodAttributes.RTSpecialName) != 0 || methodHandle.IsILStub())
////                                continue;
////
////                            // if it is an instantiated type get the InstantiatedMethodDesc if needed
////                            if(mayNeedInstantiatingStub)
////                                methodHandle = methodHandle.GetInstantiatingStubIfNeeded( declaringInterfaceHandle );
////
////                            RuntimeMethodInfo runtimeMethodInfo = new RuntimeMethodInfo(
////                                methodHandle, declaringInterfaceHandle, m_runtimeTypeCache, methodAttributes, bindingFlags );
////
////                            list.Add( runtimeMethodInfo );
////                            #endregion
////                        }
////
////                        chunkHandle = chunkHandle.GetNextMethodDescChunk();
////                    }
////                    #endregion
////                }
////                else
////                {
////                    #region IsClass or GenericParameter
////                    RuntimeTypeHandle declaringTypeHandle = ReflectedTypeHandle;
////
////                    while(declaringTypeHandle.IsGenericVariable())
////                        declaringTypeHandle = declaringTypeHandle.GetRuntimeType().BaseType.GetTypeHandleInternal();
////
////                    bool* overrides = stackalloc bool[declaringTypeHandle.GetNumVtableSlots()];
////                    bool isValueType = declaringTypeHandle.GetRuntimeType().IsValueType;
////
////                    while(!declaringTypeHandle.IsNullHandle())
////                    {
////                        bool mayNeedInstantiatingStub = declaringTypeHandle.HasInstantiation() &&
////                            !declaringTypeHandle.IsGenericTypeDefinition();
////
////                        int vtableSlots = declaringTypeHandle.GetNumVtableSlots();
////                        MethodDescChunkHandle chunkHandle = declaringTypeHandle.GetMethodDescChunk();
////
////                        while(!chunkHandle.IsNullHandle())
////                        {
////                            int methodCount = chunkHandle.GetMethodCount();
////
////                            for(int i = 0; i < methodCount; i++)
////                            {
////                                RuntimeMethodHandle methodHandle = chunkHandle.GetMethodAt( i );
////
////                                if(!filter.Match( methodHandle.GetUtf8Name() ))
////                                    continue;
////
////                                #region Loop through all methods on the current type
////                                ASSERT.CONSISTENCY_CHECK( !methodHandle.IsNullHandle() );
////
////                                MethodAttributes methodAttributes = methodHandle.GetAttributes();
////                                MethodAttributes methodAccess = methodAttributes & MethodAttributes.MemberAccessMask;
////
////                                #region Continue if this is a constructor
////                                ASSERT.CONSISTENCY_CHECK(
////                                    LOGIC.IMPLIES( (methodHandle.GetAttributes() & MethodAttributes.RTSpecialName) != 0,
////                                    methodHandle.GetName().Equals( ".ctor" ) ||
////                                    methodHandle.GetName().Equals( ".cctor" ) ||
////                                    methodHandle.GetName().Equals( "IL_STUB" ) ) );
////
////                                if((methodAttributes & MethodAttributes.RTSpecialName) != 0 || methodHandle.IsILStub())
////                                    continue;
////                                #endregion
////
////                                #region Continue if this is a private declared on a base type
////                                bool isVirtual = false;
////                                int methodSlot = 0;
////                                if((methodAttributes & MethodAttributes.Virtual) != 0)
////                                {
////                                    // only virtual if actually in the vtableslot range, but GetSlot will
////                                    // assert if an EnC method, which can't be virtual, so narrow down first
////                                    // before calling GetSlot
////                                    methodSlot = methodHandle.GetSlot();
////                                    isVirtual = (methodSlot < vtableSlots);
////                                }
////                                bool isPrivate = methodAccess == MethodAttributes.Private;
////                                bool isPrivateVirtual = isVirtual & isPrivate;
////                                bool isInherited = declaringTypeHandle.Value != ReflectedTypeHandle.Value;
////                                if(isInherited && isPrivate && !isPrivateVirtual)
////                                    continue;
////                                #endregion
////
////                                #region Continue if this is a virtual and is already overridden
////                                if(isVirtual)
////                                {
////                                    ASSERT.CONSISTENCY_CHECK(
////                                        (methodAttributes & MethodAttributes.Abstract) != 0 ||
////                                        (methodAttributes & MethodAttributes.Virtual) != 0 ||
////                                        methodHandle.GetDeclaringType().Value != declaringTypeHandle.Value );
////
////                                    if(overrides[methodSlot] == true)
////                                        continue;
////
////                                    overrides[methodSlot] = true;
////                                }
////                                else if(isValueType)
////                                {
////                                    if((methodAttributes & (MethodAttributes.Virtual | MethodAttributes.Abstract)) != 0)
////                                        continue;
////                                }
////                                else
////                                {
////                                    ASSERT.CONSISTENCY_CHECK( (methodAttributes & (MethodAttributes.Virtual | MethodAttributes.Abstract)) == 0 );
////                                }
////                                #endregion
////
////                                #region Calculate Binding Flags
////                                bool isPublic = methodAccess == MethodAttributes.Public;
////                                bool isStatic = (methodAttributes & MethodAttributes.Static) != 0;
////                                BindingFlags bindingFlags = RuntimeType.FilterPreCalculate( isPublic, isInherited, isStatic );
////                                #endregion
////
////                                // if it is an instantiated type get the InstantiatedMethodDesc if needed
////                                if(mayNeedInstantiatingStub)
////                                    methodHandle = methodHandle.GetInstantiatingStubIfNeeded( declaringTypeHandle );
////
////                                RuntimeMethodInfo runtimeMethodInfo = new RuntimeMethodInfo(
////                                    methodHandle, declaringTypeHandle, m_runtimeTypeCache, methodAttributes, bindingFlags );
////
////                                list.Add( runtimeMethodInfo );
////                                #endregion
////                            }
////
////                            chunkHandle = chunkHandle.GetNextMethodDescChunk();
////                        }
////
////                        declaringTypeHandle = declaringTypeHandle.GetBaseTypeHandle();
////                    }
////                    #endregion
////                }
////
////                return list;
////            }
////
////            private List<RuntimeConstructorInfo> PopulateConstructors( Filter filter )
////            {
////                List<RuntimeConstructorInfo> list = new List<RuntimeConstructorInfo>();
////
////                if(ReflectedType.IsGenericParameter)
////                {
////                    return list;
////                }
////
////                bool mayNeedInstantiatingStub = ReflectedTypeHandle.HasInstantiation() &&
////                    !ReflectedTypeHandle.IsGenericTypeDefinition();
////
////                MethodDescChunkHandle chunkHandle = ReflectedTypeHandle.GetMethodDescChunk();
////
////                while(!chunkHandle.IsNullHandle())
////                {
////                    int methodCount = chunkHandle.GetMethodCount();
////
////                    for(int i = 0; i < methodCount; i++)
////                    {
////                        RuntimeMethodHandle methodHandle = chunkHandle.GetMethodAt( i );
////
////                        if(!filter.Match( methodHandle.GetUtf8Name() ))
////                            continue;
////
////                        MethodAttributes methodAttributes = methodHandle.GetAttributes();
////
////                        ASSERT.CONSISTENCY_CHECK( !methodHandle.IsNullHandle() );
////                        /*
////                                                    ASSERT.CONSISTENCY_CHECK(
////                                                        LOGIC.IMPLIES((methodAttributes & MethodAttributes.RTSpecialName) != 0,
////                                                        methodHandle.GetName().Equals(".ctor") ||
////                                                        methodHandle.GetName().Equals(".cctor") ||
////                                                        methodHandle.GetName().Equals("IL_STUB")));
////                        */
////
////                        if((methodAttributes & MethodAttributes.RTSpecialName) == 0)
////                            continue;
////
////                        if(methodHandle.GetName().Equals( "IL_STUB" ))
////                            continue;
////
////                        // Constructors should not be virtual or abstract
////                        ASSERT.CONSISTENCY_CHECK(
////                            (methodAttributes & MethodAttributes.Abstract) == 0 &&
////                            (methodAttributes & MethodAttributes.Virtual) == 0 );
////
////                        #region Calculate Binding Flags
////                        bool isPublic = (methodAttributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Public;
////                        bool isStatic = (methodAttributes & MethodAttributes.Static) != 0;
////                        bool isInherited = false;
////                        BindingFlags bindingFlags = RuntimeType.FilterPreCalculate( isPublic, isInherited, isStatic );
////                        #endregion
////
////                        // if it is an instantiated type get the InstantiatedMethodDesc if needed
////                        if(mayNeedInstantiatingStub)
////                            methodHandle = methodHandle.GetInstantiatingStubIfNeeded( ReflectedTypeHandle );
////
////                        RuntimeConstructorInfo runtimeConstructorInfo =
////                            new RuntimeConstructorInfo( methodHandle, ReflectedTypeHandle, m_runtimeTypeCache, methodAttributes, bindingFlags );
////
////                        list.Add( runtimeConstructorInfo );
////                    }
////
////                    chunkHandle = chunkHandle.GetNextMethodDescChunk();
////                }
////
////                return list;
////            }
////
////            private unsafe List<RuntimeFieldInfo> PopulateFields( Filter filter )
////            {
////                List<RuntimeFieldInfo> list = new List<RuntimeFieldInfo>();
////
////                RuntimeTypeHandle declaringTypeHandle = ReflectedTypeHandle;
////
////                #region Populate all static, instance and literal fields
////                while(declaringTypeHandle.IsGenericVariable())
////                    declaringTypeHandle = declaringTypeHandle.GetRuntimeType().BaseType.GetTypeHandleInternal();
////
////                while(!declaringTypeHandle.IsNullHandle())
////                {
////                    PopulateRtFields( filter, declaringTypeHandle, list );
////
////                    PopulateLiteralFields( filter, declaringTypeHandle, list );
////
////                    declaringTypeHandle = declaringTypeHandle.GetBaseTypeHandle();
////                }
////                #endregion
////
////                #region Populate Literal Fields on Interfaces
////                if(ReflectedType.IsGenericParameter)
////                {
////                    Type[] interfaces = ReflectedTypeHandle.GetRuntimeType().BaseType.GetInterfaces();
////
////                    for(int i = 0; i < interfaces.Length; i++)
////                    {
////                        // Populate literal fields defined on any of the interfaces implemented by the declaring type
////                        PopulateLiteralFields( filter, interfaces[i].GetTypeHandleInternal(), list );
////                        PopulateRtFields( filter, interfaces[i].GetTypeHandleInternal(), list );
////                    }
////                }
////                else
////                {
////                    RuntimeTypeHandle[] interfaces = ReflectedTypeHandle.GetInterfaces();
////
////                    if(interfaces != null)
////                    {
////                        for(int i = 0; i < interfaces.Length; i++)
////                        {
////                            // Populate literal fields defined on any of the interfaces implemented by the declaring type
////                            PopulateLiteralFields( filter, interfaces[i], list );
////                            PopulateRtFields( filter, interfaces[i], list );
////                        }
////                    }
////                }
////                #endregion
////
////                return list;
////            }
////
////            private unsafe void PopulateRtFields( Filter filter, RuntimeTypeHandle declaringTypeHandle, List<RuntimeFieldInfo> list )
////            {
////                int** pResult = stackalloc int*[64];
////                int count = 64;
////
////                if(!declaringTypeHandle.GetFields( pResult, &count ))
////                {
////                    fixed(int** pBigResult = new int*[count])
////                    {
////                        declaringTypeHandle.GetFields( pBigResult, &count );
////                        PopulateRtFields( filter, pBigResult, count, declaringTypeHandle, list );
////                    }
////                }
////                else if(count > 0)
////                {
////                    PopulateRtFields( filter, pResult, count, declaringTypeHandle, list );
////                }
////            }
////
////            private unsafe void PopulateRtFields( Filter filter,
////                int** ppFieldHandles, int count, RuntimeTypeHandle declaringTypeHandle, List<RuntimeFieldInfo> list )
////            {
////                ASSERT.PRECONDITION( !declaringTypeHandle.IsNullHandle() );
////                ASSERT.PRECONDITION( !ReflectedTypeHandle.IsNullHandle() );
////
////                bool needsStaticFieldForGeneric = declaringTypeHandle.HasInstantiation() && !declaringTypeHandle.ContainsGenericVariables();
////                bool isInherited = !declaringTypeHandle.Equals( ReflectedTypeHandle );
////
////                for(int i = 0; i < count; i++)
////                {
////                    RuntimeFieldHandle runtimeFieldHandle = new RuntimeFieldHandle( ppFieldHandles[i] );
////
////                    if(!filter.Match( runtimeFieldHandle.GetUtf8Name() ))
////                        continue;
////
////                    ASSERT.CONSISTENCY_CHECK( !runtimeFieldHandle.IsNullHandle() );
////
////                    FieldAttributes fieldAttributes = runtimeFieldHandle.GetAttributes();
////                    FieldAttributes fieldAccess = fieldAttributes & FieldAttributes.FieldAccessMask;
////
////                    if(isInherited)
////                    {
////                        if(fieldAccess == FieldAttributes.Private)
////                            continue;
////                    }
////
////                    #region Calculate Binding Flags
////                    bool isPublic = fieldAccess == FieldAttributes.Public;
////                    bool isStatic = (fieldAttributes & FieldAttributes.Static) != 0;
////                    BindingFlags bindingFlags = RuntimeType.FilterPreCalculate( isPublic, isInherited, isStatic );
////                    #endregion
////
////                    // correct the FieldDesc if needed
////                    if(needsStaticFieldForGeneric && isStatic)
////                        runtimeFieldHandle = runtimeFieldHandle.GetStaticFieldForGenericType( declaringTypeHandle );
////
////                    RuntimeFieldInfo runtimeFieldInfo =
////                        new RtFieldInfo( runtimeFieldHandle, declaringTypeHandle.GetRuntimeType(), m_runtimeTypeCache, bindingFlags );
////
////                    list.Add( runtimeFieldInfo );
////                }
////            }
////
////            private unsafe void PopulateLiteralFields( Filter filter, RuntimeTypeHandle declaringTypeHandle, List<RuntimeFieldInfo> list )
////            {
////                ASSERT.PRECONDITION( !declaringTypeHandle.IsNullHandle() );
////                ASSERT.PRECONDITION( !ReflectedTypeHandle.IsNullHandle() );
////
////                int tkDeclaringType = declaringTypeHandle.GetToken();
////
////                // Our policy is that TypeDescs do not have metadata tokens
////                if(MdToken.IsNullToken( tkDeclaringType ))
////                    return;
////
////                MetadataImport scope = declaringTypeHandle.GetModuleHandle().GetMetadataImport();
////                int cFields = scope.EnumFieldsCount( tkDeclaringType );
////                int* tkFields = stackalloc int[cFields];
////                scope.EnumFields( tkDeclaringType, tkFields, cFields );
////
////                for(int i = 0; i < cFields; i++)
////                {
////                    int tkField = tkFields[i];
////                    ASSERT.PRECONDITION( MdToken.IsTokenOfType( tkField, MetadataTokenType.FieldDef ) );
////                    ASSERT.PRECONDITION( !MdToken.IsNullToken( tkField ) );
////
////                    Utf8String name;
////                    name = scope.GetName( tkField );
////
////                    if(!filter.Match( name ))
////                        continue;
////
////                    FieldAttributes fieldAttributes;
////                    scope.GetFieldDefProps( tkField, out fieldAttributes );
////
////                    FieldAttributes fieldAccess = fieldAttributes & FieldAttributes.FieldAccessMask;
////
////                    if((fieldAttributes & FieldAttributes.Literal) != 0)
////                    {
////                        bool isInherited = !declaringTypeHandle.Equals( ReflectedTypeHandle );
////                        if(isInherited)
////                        {
////                            bool isPrivate = fieldAccess == FieldAttributes.Private;
////                            if(isPrivate)
////                                continue;
////                        }
////
////                        #region Calculate Binding Flags
////                        bool isPublic = fieldAccess == FieldAttributes.Public;
////                        bool isStatic = (fieldAttributes & FieldAttributes.Static) != 0;
////                        BindingFlags bindingFlags = RuntimeType.FilterPreCalculate( isPublic, isInherited, isStatic );
////                        #endregion
////
////                        RuntimeFieldInfo runtimeFieldInfo =
////                        new MdFieldInfo( tkField, fieldAttributes, declaringTypeHandle, m_runtimeTypeCache, bindingFlags );
////
////                        list.Add( runtimeFieldInfo );
////                    }
////                }
////            }
////
////            private static void AddElementTypes( Type template, IList<Type> types )
////            {
////                if(!template.HasElementType)
////                    return;
////
////                AddElementTypes( template.GetElementType(), types );
////
////                for(int i = 0; i < types.Count; i++)
////                {
////                    if(template.IsArray)
////                    {
////                        if(template.IsSzArray)
////                            types[i] = types[i].MakeArrayType();
////                        else
////                            types[i] = types[i].MakeArrayType( template.GetArrayRank() );
////                    }
////                    else if(template.IsPointer)
////                    {
////                        types[i] = types[i].MakePointerType();
////                    }
////                }
////            }
////
////            private List<RuntimeType> PopulateInterfaces( Filter filter )
////            {
////                List<RuntimeType> list = new List<RuntimeType>();
////
////                RuntimeTypeHandle declaringTypeHandle = ReflectedTypeHandle;
////
////                if(!declaringTypeHandle.IsGenericVariable())
////                {
////                    RuntimeTypeHandle[] ifaces = ReflectedTypeHandle.GetInterfaces();
////
////                    if(ifaces != null)
////                    {
////                        for(int i = 0; i < ifaces.Length; i++)
////                        {
////                            RuntimeType interfaceType = ifaces[i].GetRuntimeType();
////
////                            if(!filter.Match( interfaceType.GetTypeHandleInternal().GetUtf8Name() ))
////                                continue;
////
////                            ASSERT.CONSISTENCY_CHECK( interfaceType.IsInterface );
////                            list.Add( interfaceType );
////                        }
////                    }
////
////                    if(ReflectedType.IsSzArray)
////                    {
////                        Type arrayType = ReflectedType.GetElementType();
////
////                        if(!arrayType.IsPointer)
////                        {
////                            Type iList = typeof( IList<> ).MakeGenericType( arrayType );
////
////                            if(iList.IsAssignableFrom( ReflectedType ))
////                            {
////                                if(filter.Match( iList.GetTypeHandleInternal().GetUtf8Name() ))
////                                    list.Add( iList as RuntimeType );
////
////                                Type[] iFaces = iList.GetInterfaces();
////                                for(int j = 0; j < iFaces.Length; j++)
////                                {
////                                    Type iFace = iFaces[j];
////                                    if(iFace.IsGenericType && filter.Match( iFace.GetTypeHandleInternal().GetUtf8Name() ))
////                                        list.Add( iFaces[j] as RuntimeType );
////                                }
////                            }
////                        }
////                    }
////                }
////                else
////                {
////                    List<RuntimeType> al = new List<RuntimeType>();
////
////                    // Get all constraints
////                    Type[] constraints = declaringTypeHandle.GetRuntimeType().GetGenericParameterConstraints();
////
////                    // Populate transitive closure of all interfaces in constraint set
////                    for(int i = 0; i < constraints.Length; i++)
////                    {
////                        Type constraint = constraints[i];
////                        if(constraint.IsInterface)
////                            al.Add( constraint as RuntimeType );
////
////                        Type[] temp = constraint.GetInterfaces();
////                        for(int j = 0; j < temp.Length; j++)
////                            al.Add( temp[j] as RuntimeType );
////                    }
////
////                    // Remove duplicates
////                    Hashtable ht = new Hashtable();
////                    for(int i = 0; i < al.Count; i++)
////                    {
////                        Type constraint = al[i] as Type;
////                        if(!ht.Contains( constraint ))
////                            ht[constraint] = constraint;
////                    }
////
////                    Type[] interfaces = new Type[ht.Values.Count];
////                    ht.Values.CopyTo( interfaces, 0 );
////
////                    // Populate link-list
////                    for(int i = 0; i < interfaces.Length; i++)
////                    {
////                        if(!filter.Match( interfaces[i].GetTypeHandleInternal().GetUtf8Name() ))
////                            continue;
////
////                        list.Add( interfaces[i] as RuntimeType );
////                    }
////                }
////
////                return list;
////            }
////
////            private unsafe List<RuntimeType> PopulateNestedClasses( Filter filter )
////            {
////                List<RuntimeType> list = new List<RuntimeType>();
////
////                RuntimeTypeHandle declaringTypeHandle = ReflectedTypeHandle;
////
////                if(declaringTypeHandle.IsGenericVariable())
////                {
////                    while(declaringTypeHandle.IsGenericVariable())
////                        declaringTypeHandle = declaringTypeHandle.GetRuntimeType().BaseType.GetTypeHandleInternal();
////                }
////
////                int tkEnclosingType = declaringTypeHandle.GetToken();
////
////                // For example, TypeDescs do not have metadata tokens
////                if(MdToken.IsNullToken( tkEnclosingType ))
////                    return list;
////
////                ModuleHandle moduleHandle = declaringTypeHandle.GetModuleHandle();
////                MetadataImport scope = moduleHandle.GetMetadataImport();
////
////                int cNestedClasses = scope.EnumNestedTypesCount( tkEnclosingType );
////                int* tkNestedClasses = stackalloc int[cNestedClasses];
////                scope.EnumNestedTypes( tkEnclosingType, tkNestedClasses, cNestedClasses );
////
////                for(int i = 0; i < cNestedClasses; i++)
////                {
////                    RuntimeTypeHandle nestedTypeHandle = new RuntimeTypeHandle();
////
////                    try
////                    {
////                        nestedTypeHandle = moduleHandle.ResolveTypeHandle( tkNestedClasses[i] );
////                    }
////                    catch(System.TypeLoadException)
////                    {
////                        // In a reflection emit scenario, we may have a token for a class which
////                        // has not been baked and hence cannot be loaded.
////                        continue;
////                    }
////
////                    if(!filter.Match( nestedTypeHandle.GetRuntimeType().GetTypeHandleInternal().GetUtf8Name() ))
////                        continue;
////
////                    list.Add( nestedTypeHandle.GetRuntimeType() );
////                }
////
////                return list;
////            }
////
////            private unsafe List<RuntimeEventInfo> PopulateEvents( Filter filter )
////            {
////                ASSERT.PRECONDITION( !ReflectedTypeHandle.IsNullHandle() );
////
////                Hashtable csEventInfos = new Hashtable();
////
////                RuntimeTypeHandle declaringTypeHandle = ReflectedTypeHandle;
////                List<RuntimeEventInfo> list = new List<RuntimeEventInfo>();
////
////                bool isInterface = (declaringTypeHandle.GetAttributes() & TypeAttributes.ClassSemanticsMask) == TypeAttributes.Interface;
////
////                if(!isInterface)
////                {
////                    while(declaringTypeHandle.IsGenericVariable())
////                        declaringTypeHandle = declaringTypeHandle.GetRuntimeType().BaseType.GetTypeHandleInternal();
////
////                    // Populate associates off of the class hierarchy
////                    while(!declaringTypeHandle.IsNullHandle())
////                    {
////                        PopulateEvents( filter, declaringTypeHandle, csEventInfos, list );
////                        declaringTypeHandle = declaringTypeHandle.GetBaseTypeHandle();
////                    }
////                }
////                else
////                {
////                    // Populate associates for this interface
////                    PopulateEvents( filter, declaringTypeHandle, csEventInfos, list );
////                }
////
////                return list;
////            }
////
////            private unsafe void PopulateEvents(
////                Filter filter, RuntimeTypeHandle declaringTypeHandle, Hashtable csEventInfos, List<RuntimeEventInfo> list )
////            {
////                int tkDeclaringType = declaringTypeHandle.GetToken();
////
////                // Arrays, Pointers, ByRef types and others generated only the fly by the RT do not have tokens.
////                if(MdToken.IsNullToken( tkDeclaringType ))
////                    return;
////
////                MetadataImport scope = declaringTypeHandle.GetModuleHandle().GetMetadataImport();
////                int cEvents = scope.EnumEventsCount( tkDeclaringType );
////                int* tkEvents = stackalloc int[cEvents];
////                scope.EnumEvents( tkDeclaringType, tkEvents, cEvents );
////                PopulateEvents( filter, declaringTypeHandle, scope, tkEvents, cEvents, csEventInfos, list );
////            }
////
////            private unsafe void PopulateEvents( Filter filter,
////                RuntimeTypeHandle declaringTypeHandle, MetadataImport scope, int* tkAssociates, int cAssociates, Hashtable csEventInfos, List<RuntimeEventInfo> list )
////            {
////                for(int i = 0; i < cAssociates; i++)
////                {
////                    int tkAssociate = tkAssociates[i];
////                    bool isPrivate;
////
////                    ASSERT.PRECONDITION( !MdToken.IsNullToken( tkAssociate ) );
////                    ASSERT.PRECONDITION( MdToken.IsTokenOfType( tkAssociate, MetadataTokenType.Event ) );
////
////                    Utf8String name;
////                    name = scope.GetName( tkAssociate );
////
////                    if(!filter.Match( name ))
////                        continue;
////
////                    RuntimeEventInfo eventInfo = new RuntimeEventInfo(
////                        tkAssociate, declaringTypeHandle.GetRuntimeType() as RuntimeType, m_runtimeTypeCache, out isPrivate );
////
////                    #region Remove Inherited Privates
////                    if(!declaringTypeHandle.Equals( m_runtimeTypeCache.RuntimeTypeHandle ) && isPrivate)
////                        continue;
////                    #endregion
////
////                    #region Remove Duplicates
////                    if(csEventInfos[eventInfo.Name] != null)
////                        continue;
////
////                    csEventInfos[eventInfo.Name] = eventInfo;
////                    #endregion
////
////                    list.Add( eventInfo );
////                }
////            }
////
////            private unsafe List<RuntimePropertyInfo> PopulateProperties( Filter filter )
////            {
////                ASSERT.PRECONDITION( !ReflectedTypeHandle.IsNullHandle() );
////                ASSERT.CONSISTENCY_CHECK( m_csMemberInfos == null );
////
////                Hashtable csPropertyInfos = new Hashtable();
////
////                RuntimeTypeHandle declaringTypeHandle = ReflectedTypeHandle;
////                List<RuntimePropertyInfo> list = new List<RuntimePropertyInfo>();
////
////                bool isInterface = (declaringTypeHandle.GetAttributes() & TypeAttributes.ClassSemanticsMask) == TypeAttributes.Interface;
////
////                if(!isInterface)
////                {
////                    while(declaringTypeHandle.IsGenericVariable())
////                        declaringTypeHandle = declaringTypeHandle.GetRuntimeType().BaseType.GetTypeHandleInternal();
////
////                    // Populate associates off of the class hierarchy
////                    while(!declaringTypeHandle.IsNullHandle())
////                    {
////                        PopulateProperties( filter, declaringTypeHandle, csPropertyInfos, list );
////                        declaringTypeHandle = declaringTypeHandle.GetBaseTypeHandle();
////                    }
////                }
////                else
////                {
////                    // Populate associates for this interface
////                    PopulateProperties( filter, declaringTypeHandle, csPropertyInfos, list );
////                }
////
////                return list;
////            }
////
////            private unsafe void PopulateProperties( Filter filter,
////                RuntimeTypeHandle declaringTypeHandle, Hashtable csPropertyInfos, List<RuntimePropertyInfo> list )
////            {
////                int tkDeclaringType = declaringTypeHandle.GetToken();
////
////                // Arrays, Pointers, ByRef types and others generated only the fly by the RT do not have tokens.
////                if(MdToken.IsNullToken( tkDeclaringType ))
////                    return;
////
////                MetadataImport scope = declaringTypeHandle.GetModuleHandle().GetMetadataImport();
////                int cProperties = scope.EnumPropertiesCount( tkDeclaringType );
////                int* tkProperties = stackalloc int[cProperties];
////                scope.EnumProperties( tkDeclaringType, tkProperties, cProperties );
////                PopulateProperties( filter, declaringTypeHandle, tkProperties, cProperties, csPropertyInfos, list );
////            }
////
////            private unsafe void PopulateProperties( Filter filter,
////                RuntimeTypeHandle declaringTypeHandle, int* tkAssociates, int cProperties, Hashtable csPropertyInfos, List<RuntimePropertyInfo> list )
////            {
////                for(int i = 0; i < cProperties; i++)
////                {
////                    int tkAssociate = tkAssociates[i];
////                    bool isPrivate;
////
////                    ASSERT.PRECONDITION( !MdToken.IsNullToken( tkAssociate ) );
////                    ASSERT.PRECONDITION( MdToken.IsTokenOfType( tkAssociate, MetadataTokenType.Property ) );
////
////                    Utf8String name;
////                    name = declaringTypeHandle.GetRuntimeType().Module.MetadataImport.GetName( tkAssociate );
////
////                    if(!filter.Match( name ))
////                        continue;
////
////                    RuntimePropertyInfo propertyInfo =
////                        new RuntimePropertyInfo(
////                        tkAssociate, declaringTypeHandle.GetRuntimeType() as RuntimeType, m_runtimeTypeCache, out isPrivate );
////
////                    #region Remove Privates
////                    if(!declaringTypeHandle.Equals( m_runtimeTypeCache.RuntimeTypeHandle ) && isPrivate)
////                        continue;
////                    #endregion
////
////                    #region Remove Duplicates
////                    List<RuntimePropertyInfo> cache = csPropertyInfos[propertyInfo.Name] as List<RuntimePropertyInfo>;
////
////                    if(cache == null)
////                    {
////                        cache = new List<RuntimePropertyInfo>();
////                        csPropertyInfos[propertyInfo.Name] = cache;
////                    }
////                    else
////                    {
////                        for(int j = 0; j < cache.Count; j++)
////                        {
////                            if(propertyInfo.EqualsSig( cache[j] ))
////                            {
////                                cache = null;
////                                break;
////                            }
////                        }
////                    }
////
////                    if(cache == null)
////                        continue;
////
////                    cache.Add( propertyInfo );
////                    #endregion
////
////                    list.Add( propertyInfo );
////                }
////            }
////            #endregion
////
////            #region NonPrivate Members
////            internal CerArrayList<T> GetMemberList( MemberListType listType, string name, CacheType cacheType )
////            {
////                CerArrayList<T> list = null;
////
////                switch(listType)
////                {
////                    case MemberListType.CaseSensitive:
////                        if(m_csMemberInfos == null)
////                        {
////                            return Populate( name, listType, cacheType );
////                        }
////                        else
////                        {
////                            list = m_csMemberInfos[name];
////
////                            if(list == null)
////                                return Populate( name, listType, cacheType );
////
////                            return list;
////                        }
////
////                    case MemberListType.All:
////                        if(m_cacheComplete)
////                            return m_root;
////
////                        return Populate( null, listType, cacheType );
////
////                    default:
////                        if(m_cisMemberInfos == null)
////                        {
////                            return Populate( name, listType, cacheType );
////                        }
////                        else
////                        {
////                            list = m_cisMemberInfos[name];
////
////                            if(list == null)
////                                return Populate( name, listType, cacheType );
////
////                            return list;
////                        }
////                }
////            }
////
////            internal RuntimeTypeHandle ReflectedTypeHandle
////            {
////                get
////                {
////                    return m_runtimeTypeCache.RuntimeTypeHandle;
////                }
////            }
////            internal RuntimeType ReflectedType
////            {
////                get
////                {
////                    return ReflectedTypeHandle.GetRuntimeType();
////                }
////            }
////            #endregion
////        }
////        #endregion
////
////        #region Private Data Members
////        private WhatsCached m_whatsCached;
////        private RuntimeTypeHandle m_runtimeTypeHandle;
////        private RuntimeType m_runtimeType;
////        private RuntimeType m_enclosingType;
////        private TypeCode m_typeCode;
////        private string m_name, m_fullname, m_toString, m_namespace;
////        private bool m_isGlobal;
////        private bool m_bIsDomainInitialized;
////        private MemberInfoCache<RuntimeMethodInfo> m_methodInfoCache;
////        private MemberInfoCache<RuntimeConstructorInfo> m_constructorInfoCache;
////        private MemberInfoCache<RuntimeFieldInfo> m_fieldInfoCache;
////        private MemberInfoCache<RuntimeType> m_interfaceCache;
////        private MemberInfoCache<RuntimeType> m_nestedClassesCache;
////        private MemberInfoCache<RuntimePropertyInfo> m_propertyInfoCache;
////        private MemberInfoCache<RuntimeEventInfo> m_eventInfoCache;
////        private CerHashtable<RuntimeMethodInfo, RuntimeMethodInfo> m_methodInstantiations;
////        #endregion
////
////        #region Constructor
////        internal RuntimeTypeCache( RuntimeType runtimeType )
////        {
////            m_typeCode = TypeCode.Empty;
////            m_runtimeType = runtimeType;
////            m_runtimeTypeHandle = runtimeType.GetTypeHandleInternal();
////            m_isGlobal = m_runtimeTypeHandle.GetModuleHandle().GetModuleTypeHandle().Equals( m_runtimeTypeHandle );
////        }
////        #endregion
////
////        #region Private Members
////        private string ConstructName( ref string name, bool nameSpace, bool fullinst, bool assembly )
////        {
////            if(name == null)
////            {
////                name = RuntimeTypeHandle.ConstructName( nameSpace, fullinst, assembly );
////            }
////            return name;
////        }
////
////        private CerArrayList<T> GetMemberList<T>( ref MemberInfoCache<T> m_cache, MemberListType listType, string name, CacheType cacheType )
////            where T : MemberInfo
////        {
////            MemberInfoCache<T> existingCache = GetMemberCache<T>( ref m_cache );
////            return existingCache.GetMemberList( listType, name, cacheType );
////        }
////
////        private MemberInfoCache<T> GetMemberCache<T>( ref MemberInfoCache<T> m_cache )
////            where T : MemberInfo
////        {
////            MemberInfoCache<T> existingCache = m_cache;
////
////            if(existingCache == null)
////            {
////                MemberInfoCache<T> newCache = new MemberInfoCache<T>( this );
////                existingCache = Interlocked.CompareExchange( ref m_cache, newCache, null );
////                if(existingCache == null)
////                    existingCache = newCache;
////            }
////
////            return existingCache;
////        }
////        #endregion
////
////        #region Internal Members
////        internal bool DomainInitialized
////        {
////            get
////            {
////                return m_bIsDomainInitialized;
////            }
////            
////            set
////            {
////                m_bIsDomainInitialized = value;
////            }
////        }
////
////        internal string GetName()
////        {
////            return ConstructName( ref m_name, false, false, false );
////        }
////
////        internal unsafe string GetNameSpace()
////        {
////            // @Optimization - Use ConstructName to populate m_namespace
////            if(m_namespace == null)
////            {
////                Type type = m_runtimeType;
////                type = type.GetRootElementType();
////
////                while(type.IsNested)
////                    type = type.DeclaringType;
////
////                m_namespace = type.GetTypeHandleInternal().GetModuleHandle(
////                    ).GetMetadataImport().GetNamespace( type.MetadataToken ).ToString();
////            }
////
////            return m_namespace;
////        }
////
////        internal string GetToString()
////        {
////            return ConstructName( ref m_toString, true, false, false );
////        }
////
////        internal string GetFullName()
////        {
////            if(!m_runtimeType.IsGenericTypeDefinition && m_runtimeType.ContainsGenericParameters)
////                return null;
////
////            return ConstructName( ref m_fullname, true, true, false );
////        }
////
////        internal TypeCode TypeCode
////        {
////            get
////            {
////                return m_typeCode;
////            }
////            
////            set
////            {
////                m_typeCode = value;
////            }
////        }
////
////        internal unsafe RuntimeType GetEnclosingType()
////        {
////            if((m_whatsCached & WhatsCached.EnclosingType) == 0)
////            {
////                m_enclosingType = RuntimeTypeHandle.GetDeclaringType().GetRuntimeType();
////
////                m_whatsCached |= WhatsCached.EnclosingType;
////            }
////
////            return m_enclosingType;
////        }
////
////        internal bool IsGlobal
////        {
////            [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
////            get
////            {
////                return m_isGlobal;
////            }
////        }
////
////        internal RuntimeType RuntimeType
////        {
////            get
////            {
////                return m_runtimeType;
////            }
////        }
////
////        internal RuntimeTypeHandle RuntimeTypeHandle
////        {
////            get
////            {
////                return m_runtimeTypeHandle;
////            }
////        }
////        internal void InvalidateCachedNestedType()
////        {
////            m_nestedClassesCache = null;
////        }
////        #endregion
////
////        #region Caches Accessors
////        internal MethodInfo GetGenericMethodInfo( RuntimeMethodHandle genericMethod )
////        {
////            if(m_methodInstantiations == null)
////                Interlocked.CompareExchange( ref m_methodInstantiations, new CerHashtable<RuntimeMethodInfo, RuntimeMethodInfo>(), null );
////
////            RuntimeMethodInfo rmi = new RuntimeMethodInfo(
////                genericMethod, genericMethod.GetDeclaringType(), this,
////                genericMethod.GetAttributes(), (BindingFlags)(-1) );
////
////            RuntimeMethodInfo crmi = null;
////
////            crmi = m_methodInstantiations[rmi];
////            if(crmi != null)
////                return crmi;
////
////            bool lockTaken = false;
////            bool preallocationComplete = false;
////            RuntimeHelpers.PrepareConstrainedRegions();
////            try
////            {
////                Monitor.ReliableEnter( this, ref lockTaken );
////
////                crmi = m_methodInstantiations[rmi];
////                if(crmi != null)
////                    return crmi;
////
////                m_methodInstantiations.Preallocate( 1 );
////
////                preallocationComplete = true;
////            }
////            finally
////            {
////                if(preallocationComplete)
////                {
////                    m_methodInstantiations[rmi] = rmi;
////                }
////
////                if(lockTaken)
////                {
////                    Monitor.Exit( this );
////                }
////            }
////
////            return rmi;
////        }
////
////        internal CerArrayList<RuntimeMethodInfo> GetMethodList( MemberListType listType, string name )
////        {
////            return GetMemberList<RuntimeMethodInfo>( ref m_methodInfoCache, listType, name, CacheType.Method );
////        }
////
////        internal CerArrayList<RuntimeConstructorInfo> GetConstructorList( MemberListType listType, string name )
////        {
////            return GetMemberList<RuntimeConstructorInfo>( ref m_constructorInfoCache, listType, name, CacheType.Constructor );
////        }
////
////        internal CerArrayList<RuntimePropertyInfo> GetPropertyList( MemberListType listType, string name )
////        {
////            return GetMemberList<RuntimePropertyInfo>( ref m_propertyInfoCache, listType, name, CacheType.Property );
////        }
////
////        internal CerArrayList<RuntimeEventInfo> GetEventList( MemberListType listType, string name )
////        {
////            return GetMemberList<RuntimeEventInfo>( ref m_eventInfoCache, listType, name, CacheType.Event );
////        }
////
////        internal CerArrayList<RuntimeFieldInfo> GetFieldList( MemberListType listType, string name )
////        {
////            return GetMemberList<RuntimeFieldInfo>( ref m_fieldInfoCache, listType, name, CacheType.Field );
////        }
////
////        internal CerArrayList<RuntimeType> GetInterfaceList( MemberListType listType, string name )
////        {
////            return GetMemberList<RuntimeType>( ref m_interfaceCache, listType, name, CacheType.Interface );
////        }
////
////        internal CerArrayList<RuntimeType> GetNestedTypeList( MemberListType listType, string name )
////        {
////            return GetMemberList<RuntimeType>( ref m_nestedClassesCache, listType, name, CacheType.NestedType );
////        }
////
////        internal MethodBase GetMethod( RuntimeTypeHandle declaringType, RuntimeMethodHandle method )
////        {
////            GetMemberCache<RuntimeMethodInfo>( ref m_methodInfoCache );
////            return m_methodInfoCache.AddMethod( declaringType, method, CacheType.Method );
////        }
////
////        internal MethodBase GetConstructor( RuntimeTypeHandle declaringType, RuntimeMethodHandle constructor )
////        {
////            GetMemberCache<RuntimeConstructorInfo>( ref m_constructorInfoCache );
////            return m_constructorInfoCache.AddMethod( declaringType, constructor, CacheType.Constructor );
////        }
////
////        internal FieldInfo GetField( RuntimeFieldHandle field )
////        {
////            GetMemberCache<RuntimeFieldInfo>( ref m_fieldInfoCache );
////            return m_fieldInfoCache.AddField( field );
////        }
////
////        #endregion
////    }
        #endregion

        #region Static Members

        #region Internal
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    static internal extern void PrepareMemberInfoCache( RuntimeTypeHandle rt );
////
////    internal static MethodBase GetMethodBase( ModuleHandle scope, int typeMetadataToken )
////    {
////        return GetMethodBase( scope.ResolveMethodHandle( typeMetadataToken ) );
////    }
////
////    internal static MethodBase GetMethodBase( Module scope, int typeMetadataToken )
////    {
////        return GetMethodBase( scope.GetModuleHandle(), typeMetadataToken );
////    }
////
////    internal static MethodBase GetMethodBase( RuntimeMethodHandle methodHandle )
////    {
////        return GetMethodBase( RuntimeTypeHandle.EmptyHandle, methodHandle );
////    }
////
////    internal unsafe static MethodBase GetMethodBase( RuntimeTypeHandle reflectedTypeHandle, RuntimeMethodHandle methodHandle )
////    {
////        ASSERT.CONSISTENCY_CHECK( !reflectedTypeHandle.IsNullHandle() );
////        ASSERT.CONSISTENCY_CHECK( !methodHandle.IsNullHandle() );
////
////        if(methodHandle.IsDynamicMethod())
////        {
////            Resolver resolver = methodHandle.GetResolver();
////
////            if(resolver != null)
////            {
////                return resolver.GetDynamicMethod();
////            }
////
////            return null;
////        }
////
////        // verify the type/method relationship
////        Type        declaredType  = methodHandle.GetDeclaringType().GetRuntimeType();
////        RuntimeType reflectedType = reflectedTypeHandle.GetRuntimeType();
////
////        if(reflectedType == null)
////        {
////            reflectedType = declaredType as RuntimeType;
////        }
////
////        if(reflectedType.IsArray)
////        {
////            MethodBase[] methodBases = reflectedType.GetMember( methodHandle.GetName(), MemberTypes.Constructor | MemberTypes.Method,
////                                                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance ) as MethodBase[];
////
////            bool loaderAssuredCompatible = false;
////            for(int i = 0; i < methodBases.Length; i++)
////            {
////                if(methodBases[0].GetMethodHandle() == methodHandle)
////                {
////                    loaderAssuredCompatible = true;
////                }
////            }
////
////            if(!loaderAssuredCompatible)
////            {
////                throw new ArgumentException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "Argument_ResolveMethodHandle" ), reflectedType.ToString(), declaredType.ToString() ) );
////            }
////
////            declaredType = reflectedType;
////        }
////        else if(!declaredType.IsAssignableFrom( reflectedType ))
////        {
////            if(!declaredType.IsGenericType)
////            {
////                throw new ArgumentException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "Argument_ResolveMethodHandle" ), reflectedType.ToString(), declaredType.ToString() ) );
////            }
////
////            // ignoring instantiation is the ReflectedType a subtype of the DeclaringType
////            Type declaringDefinition = declaredType.GetGenericTypeDefinition();
////
////            Type baseType = reflectedType;
////
////            while(baseType != null)
////            {
////                Type baseDefinition = baseType;
////
////                if(baseDefinition.IsGenericType && !baseType.IsGenericTypeDefinition)
////                {
////                    baseDefinition = baseDefinition.GetGenericTypeDefinition();
////                }
////
////                if(baseDefinition.Equals( declaringDefinition ))
////                {
////                    break;
////                }
////
////                baseType = baseType.BaseType;
////            }
////
////            if(baseType == null)
////            {
////                // ignoring instantiation is the ReflectedType is not a subtype of the DeclaringType
////                throw new ArgumentException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "Argument_ResolveMethodHandle" ), reflectedType.ToString(), declaredType.ToString() ) );
////            }
////
////            // remap the method to same method on the subclass ReflectedType
////            declaredType = baseType;
////
////            RuntimeTypeHandle[] methodInstantiation = methodHandle.GetMethodInstantiation();
////
////            bool bIsGenericMethodDefinition = methodHandle.IsGenericMethodDefinition();
////
////            // lookup via v-table slot the RuntimeMethodHandle on the new declaring type
////            methodHandle = methodHandle.GetMethodFromCanonical( declaredType.GetTypeHandleInternal() );
////
////            // if the origional methodHandle was the definition than we don't need to rebind generic method arguments
////            // because all RuntimeMethodHandles retrieved off of the cannonical method table are definitions. That's
////            // why for everything else we need to rebind the generic method arguments.
////            if(!bIsGenericMethodDefinition)
////            {
////                // rebind any generic method arguments
////                if(methodInstantiation != null)
////                {
////                    methodHandle = methodHandle.GetInstantiatingStub( declaredType.GetTypeHandleInternal(), methodInstantiation );
////                }
////                else
////                {
////                    methodHandle = methodHandle.GetInstantiatingStubIfNeeded( declaredType.GetTypeHandleInternal() );
////                }
////            }
////        }
////
////        if(methodHandle.IsConstructor())
////        {
////            return reflectedType.Cache.GetConstructor( declaredType.GetTypeHandleInternal(), methodHandle );
////        }
////
////        if(methodHandle.HasMethodInstantiation() && !methodHandle.IsGenericMethodDefinition())
////        {
////            return reflectedType.Cache.GetGenericMethodInfo( methodHandle );
////        }
////
////        return reflectedType.Cache.GetMethod( declaredType.GetTypeHandleInternal(), methodHandle );
////    }
////
////    internal bool DomainInitialized
////    {
////        get
////        {
////            return Cache.DomainInitialized;
////        }
////
////        set
////        {
////            Cache.DomainInitialized = value;
////        }
////    }
////
////    internal unsafe static FieldInfo GetFieldInfo( RuntimeFieldHandle fieldHandle )
////    {
////        return GetFieldInfo( fieldHandle.GetApproxDeclaringType(), fieldHandle );
////    }
////
////    internal unsafe static FieldInfo GetFieldInfo( RuntimeTypeHandle reflectedTypeHandle, RuntimeFieldHandle fieldHandle )
////    {
////        // verify the type/method relationship
////        if(reflectedTypeHandle.IsNullHandle())
////        {
////            reflectedTypeHandle = fieldHandle.GetApproxDeclaringType();
////        }
////        else
////        {
////            RuntimeTypeHandle declaredTypeHandle = fieldHandle.GetApproxDeclaringType();
////            if(!reflectedTypeHandle.Equals( declaredTypeHandle ))
////            {
////                if(!fieldHandle.AcquiresContextFromThis() || !declaredTypeHandle.GetCanonicalHandle().Equals( reflectedTypeHandle.GetCanonicalHandle() ))
////                {
////                    throw new ArgumentException( String.Format(
////                        CultureInfo.CurrentCulture, Environment.GetResourceString( "Argument_ResolveFieldHandle" ),
////                        reflectedTypeHandle.GetRuntimeType().ToString(),
////                        declaredTypeHandle.GetRuntimeType().ToString() ) );
////                }
////            }
////        }
////
////        return reflectedTypeHandle.GetRuntimeType().Cache.GetField( fieldHandle );
////    }
////
////    // Called internally
////    internal unsafe static PropertyInfo GetPropertyInfo( RuntimeTypeHandle reflectedTypeHandle, int tkProperty )
////    {
////        RuntimePropertyInfo               property   = null;
////        CerArrayList<RuntimePropertyInfo> candidates = reflectedTypeHandle.GetRuntimeType().Cache.GetPropertyList( MemberListType.All, null );
////
////        for(int i = 0; i < candidates.Count; i++)
////        {
////            property = candidates[i];
////            if(property.MetadataToken == tkProperty)
////            {
////                return property;
////            }
////        }
////
////        ASSERT.UNREACHABLE();
////        throw new SystemException();
////    }
////
////    private static void ThrowIfTypeNeverValidGenericArgument( Type type )
////    {
////        if(type.IsPointer || type.IsByRef || type == typeof( void ))
////        {
////            throw new ArgumentException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "Argument_NeverValidGenericArgument" ), type.ToString() ) );
////        }
////    }
////
////
////    internal static void SanityCheckGenericArguments( Type[] genericArguments, Type[] genericParamters )
////    {
////        if(genericArguments == null)
////        {
////            throw new ArgumentNullException();
////        }
////
////        for(int i = 0; i < genericArguments.Length; i++)
////        {
////            if(genericArguments[i] == null)
////            {
////                throw new ArgumentNullException();
////            }
////
////            ThrowIfTypeNeverValidGenericArgument( genericArguments[i] );
////        }
////
////        if(genericArguments.Length != genericParamters.Length)
////        {
////            throw new ArgumentException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "Argument_NotEnoughGenArguments", genericArguments.Length, genericParamters.Length ) ) );
////        }
////    }
////
////    internal static void ValidateGenericArguments( MemberInfo definition, Type[] genericArguments, Exception e )
////    {
////        RuntimeTypeHandle[] typeContextHandle   = null;
////        RuntimeTypeHandle[] methodContextHandle = null;
////        Type[]              genericParamters    = null;
////
////        if(definition is Type)
////        {
////            Type genericTypeDefinition = (Type)definition;
////
////            genericParamters  = genericTypeDefinition.GetGenericArguments();
////            typeContextHandle = new RuntimeTypeHandle[genericArguments.Length];
////            for(int i = 0; i < genericArguments.Length; i++)
////            {
////                typeContextHandle[i] = genericArguments[i].GetTypeHandleInternal();
////            }
////        }
////        else
////        {
////            MethodInfo genericMethodDefinition = (MethodInfo)definition;
////
////            genericParamters    = genericMethodDefinition.GetGenericArguments();
////            methodContextHandle = new RuntimeTypeHandle[genericArguments.Length];
////            for(int i = 0; i < genericArguments.Length; i++)
////            {
////                methodContextHandle[i] = genericArguments[i].GetTypeHandleInternal();
////            }
////
////            Type declaringType = genericMethodDefinition.DeclaringType;
////            if(declaringType != null)
////            {
////                typeContextHandle = declaringType.GetTypeHandleInternal().GetInstantiation();
////            }
////        }
////
////        for(int i = 0; i < genericArguments.Length; i++)
////        {
////            Type genericArgument  = genericArguments[i];
////            Type genericParameter = genericParamters[i];
////
////            if(!genericParameter.GetTypeHandleInternal().SatisfiesConstraints( typeContextHandle, methodContextHandle, genericArgument.GetTypeHandleInternal() ))
////            {
////                throw new ArgumentException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "Argument_GenConstraintViolation" ),
////                    i.ToString( CultureInfo.CurrentCulture ), genericArgument.ToString(), definition.ToString(), genericParameter.ToString() ), e );
////            }
////        }
////    }
////
////    private static void SplitName( string fullname, out string name, out string ns )
////    {
////        name = null;
////        ns   = null;
////
////        if(fullname == null)
////        {
////            return;
////        }
////
////        // Get namespace
////        int nsDelimiter = fullname.LastIndexOf( ".", StringComparison.Ordinal );
////        if(nsDelimiter != -1)
////        {
////            ns = fullname.Substring( 0, nsDelimiter );
////            int nameLength = fullname.Length - ns.Length - 1;
////            if(nameLength != 0)
////            {
////                name = fullname.Substring( nsDelimiter + 1, nameLength );
////            }
////            else
////            {
////                name = "";
////            }
////
////            ASSERT.CONSISTENCY_CHECK( fullname.Equals( ns + "." + name ) );
////        }
////        else
////        {
////            name = fullname;
////        }
////
////    }
        #endregion

        #region Filters
////    internal static BindingFlags FilterPreCalculate( bool isPublic, bool isInherited, bool isStatic )
////    {
////        BindingFlags bindingFlags = isPublic ? BindingFlags.Public : BindingFlags.NonPublic;
////
////        if(isInherited)
////        {
////            // We arrange things so the DeclaredOnly flag means "include inherited members"
////            bindingFlags |= BindingFlags.DeclaredOnly;
////
////            if(isStatic)
////            {
////                bindingFlags |= BindingFlags.Static | BindingFlags.FlattenHierarchy;
////            }
////            else
////            {
////                bindingFlags |= BindingFlags.Instance;
////            }
////        }
////        else
////        {
////            if(isStatic)
////            {
////                bindingFlags |= BindingFlags.Static;
////            }
////            else
////            {
////                bindingFlags |= BindingFlags.Instance;
////            }
////        }
////
////        return bindingFlags;
////    }
////
////    private static void FilterHelper(     BindingFlags   bindingFlags     ,
////                                      ref string         name             ,
////                                          bool           allowPrefixLookup,
////                                      out bool           prefixLookup     ,
////                                      out bool           ignoreCase       ,
////                                      out MemberListType listType         )
////    {
////        prefixLookup = false;
////        ignoreCase   = false;
////
////        if(name != null)
////        {
////            if((bindingFlags & BindingFlags.IgnoreCase) != 0)
////            {
////                name       = name.ToLower( CultureInfo.InvariantCulture );
////                ignoreCase = true;
////                listType   = MemberListType.CaseInsensitive;
////            }
////            else
////            {
////                listType = MemberListType.CaseSensitive;
////            }
////
////            if(allowPrefixLookup && name.EndsWith( "*", StringComparison.Ordinal ))
////            {
////                name         = name.Substring( 0, name.Length - 1 );
////                prefixLookup = true;
////                listType     = MemberListType.All;
////            }
////        }
////        else
////        {
////            listType = MemberListType.All;
////        }
////    }
////
////    private static void FilterHelper( BindingFlags bindingFlags, ref string name, out bool ignoreCase, out MemberListType listType )
////    {
////        bool prefixLookup;
////
////        FilterHelper( bindingFlags, ref name, false, out prefixLookup, out ignoreCase, out listType );
////    }
////
////    private static bool FilterApplyPrefixLookup( MemberInfo memberInfo, string name, bool ignoreCase )
////    {
////        ASSERT.CONSISTENCY_CHECK( name != null );
////
////        if(ignoreCase)
////        {
////            if(!memberInfo.Name.ToLower( CultureInfo.InvariantCulture ).StartsWith( name, StringComparison.Ordinal ))
////            {
////                return false;
////            }
////        }
////        else
////        {
////            if(!memberInfo.Name.StartsWith( name, StringComparison.Ordinal ))
////            {
////                return false;
////            }
////        }
////
////        return true;
////    }
////
////
////    private static bool FilterApplyBase( MemberInfo   memberInfo            ,
////                                         BindingFlags bindingFlags          ,
////                                         bool         isPublic              ,
////                                         bool         isNonProtectedInternal,
////                                         bool         isStatic              ,
////                                         string       name                  ,
////                                         bool         prefixLookup          )
////    {
////        #region Preconditions
////        ASSERT.PRECONDITION( memberInfo != null );
////        ASSERT.PRECONDITION( name == null || (bindingFlags & BindingFlags.IgnoreCase) == 0 || (name.ToLower( CultureInfo.InvariantCulture ).Equals( name )) );
////        #endregion
////
////        #region Filter by Public & Private
////        if(isPublic)
////        {
////            if((bindingFlags & BindingFlags.Public) == 0)
////            {
////                return false;
////            }
////        }
////        else
////        {
////            if((bindingFlags & BindingFlags.NonPublic) == 0)
////            {
////                return false;
////            }
////        }
////        #endregion
////
////        bool isInherited = memberInfo.DeclaringType != memberInfo.ReflectedType;
////
////        #region Filter by DeclaredOnly
////        if((bindingFlags & BindingFlags.DeclaredOnly) != 0 && isInherited)
////        {
////            return false;
////        }
////        #endregion
////
////        #region Filter by Static & Instance
////        if(memberInfo.MemberType != MemberTypes.TypeInfo   &&
////           memberInfo.MemberType != MemberTypes.NestedType  )
////        {
////            if(isStatic)
////            {
////                if((bindingFlags & BindingFlags.FlattenHierarchy) == 0 && isInherited)
////                {
////                    return false;
////                }
////
////                if((bindingFlags & BindingFlags.Static) == 0)
////                {
////                    return false;
////                }
////            }
////            else
////            {
////                if((bindingFlags & BindingFlags.Instance) == 0)
////                {
////                    return false;
////                }
////            }
////        }
////        #endregion
////
////        #region Filter by name wrt prefixLookup and implicitly by case sensitivity
////        if(prefixLookup == true)
////        {
////            if(!FilterApplyPrefixLookup( memberInfo, name, (bindingFlags & BindingFlags.IgnoreCase) != 0 ))
////            {
////                return false;
////            }
////        }
////        #endregion
////
////        #region Asymmetries
////        // @Asymmetry - Internal, inherited, instance, non-protected, non-virtual, non-abstract members returned
////        //              iff BindingFlags !DeclaredOnly, Instance and Public are present except for fields
////        if(((bindingFlags & BindingFlags.DeclaredOnly) == 0) &&        // DeclaredOnly not present
////             isInherited &&                                            // Is inherited Member
////
////            (isNonProtectedInternal) &&                                 // Is non-protected internal member
////            ((bindingFlags & BindingFlags.NonPublic) != 0) &&           // BindingFlag.NonPublic present
////
////            (!isStatic) &&                                              // Is instance member
////            ((bindingFlags & BindingFlags.Instance) != 0))              // BindingFlag.Instance present
////        {
////            MethodInfo methodInfo = memberInfo as MethodInfo;
////
////            if(methodInfo == null)
////            {
////                return false;
////            }
////
////            if(!methodInfo.IsVirtual && !methodInfo.IsAbstract)
////            {
////                return false;
////            }
////        }
////        #endregion
////
////        return true;
////    }
////
////
////    private static bool FilterApplyType( Type type, BindingFlags bindingFlags, string name, bool prefixLookup, string ns )
////    {
////        ASSERT.PRECONDITION( type != null );
////        ASSERT.PRECONDITION( type is RuntimeType );
////
////        bool isPublic = type.IsNestedPublic || type.IsPublic;
////        bool isStatic = false;
////
////        if(!RuntimeType.FilterApplyBase( type, bindingFlags, isPublic, type.IsNestedAssembly, isStatic, name, prefixLookup ))
////        {
////            return false;
////        }
////
////        if(ns != null && !type.Namespace.Equals( ns ))
////        {
////            return false;
////        }
////
////        return true;
////    }
////
////
////    private static bool FilterApplyMethodBaseInfo( MethodBase         methodBase   ,
////                                                   BindingFlags       bindingFlags ,
////                                                   string             name         ,
////                                                   CallingConventions callConv     ,
////                                                   Type[]             argumentTypes,
////                                                   bool               prefixLookup )
////    {
////        ASSERT.PRECONDITION( methodBase != null );
////
////        #region Apply Base Filter
////        bindingFlags ^= BindingFlags.DeclaredOnly;
////        BindingFlags matchFlags;
////
////        RuntimeMethodInfo methodInfo = methodBase as RuntimeMethodInfo;
////        if(methodInfo == null)
////        {
////            RuntimeConstructorInfo constructorInfo = methodBase as RuntimeConstructorInfo;
////            matchFlags = constructorInfo.BindingFlags;
////        }
////        else
////        {
////            matchFlags = methodInfo.BindingFlags;
////        }
////
////        if((bindingFlags & matchFlags) != matchFlags ||
////            (prefixLookup && !FilterApplyPrefixLookup( methodBase, name, (bindingFlags & BindingFlags.IgnoreCase) != 0 )))
////        {
////            return false;
////        }
////        #endregion
////
////        return FilterApplyMethodBaseInfo( methodBase, bindingFlags, callConv, argumentTypes );
////    }
////
////
////    private static bool FilterApplyMethodBaseInfo( MethodBase         methodBase    ,
////                                                   BindingFlags       bindingFlags  ,
////                                                   CallingConventions callConv      ,
////                                                   Type[]             argumentTypes )
////    {
////        #region Check CallingConvention
////        if((callConv & CallingConventions.Any) == 0)
////        {
////            if((callConv & CallingConventions.VarArgs) != 0 && (methodBase.CallingConvention & CallingConventions.VarArgs) == 0)
////            {
////                return false;
////            }
////
////            if((callConv & CallingConventions.Standard) != 0 && (methodBase.CallingConvention & CallingConventions.Standard) == 0)
////            {
////                return false;
////            }
////        }
////        #endregion
////
////        #region If argumentTypes supplied
////        if(argumentTypes != null)
////        {
////            ParameterInfo[] parameterInfos = methodBase.GetParametersNoCopy();
////
////            if(argumentTypes.Length != parameterInfos.Length)
////            {
////                #region Invoke Member, Get\Set & Create Instance specific case
////                // If the number of supplied arguments differs than the number in the signature AND
////                // we are not filtering for a dynamic call -- InvokeMethod or CreateInstance -- filter out the method.
////                if((bindingFlags & (BindingFlags.InvokeMethod | BindingFlags.CreateInstance | BindingFlags.GetProperty | BindingFlags.SetProperty)) == 0)
////                {
////                    return false;
////                }
////
////                bool testForParamArray       = false;
////                bool excessSuppliedArguments = argumentTypes.Length > parameterInfos.Length;
////
////                if(excessSuppliedArguments)
////                { // more supplied arguments than parameters, additional arguments could be vararg
////                    #region Varargs
////                    // If method is not vararg, additional arguments can not be passed as vararg
////                    if((methodBase.CallingConvention & CallingConventions.VarArgs) == 0)
////                    {
////                        testForParamArray = true;
////                    }
////                    else
////                    {
////                        // If Binding flags did not include varargs we would have filtered this vararg method.
////                        // This Invariant established during callConv check.
////                        ASSERT.CONSISTENCY_CHECK( (callConv & CallingConventions.VarArgs) != 0 );
////                    }
////                    #endregion
////                }
////                else
////                {// fewer supplied arguments than parameters, missing arguments could be optional
////                    #region OptionalParamBinding
////                    if((bindingFlags & BindingFlags.OptionalParamBinding) == 0)
////                    {
////                        testForParamArray = true;
////                    }
////                    else
////                    {
////                        // From our existing code, our policy here is that if a parameterInfo
////                        // is optional then all subsequent parameterInfos shall be optional.
////
////                        // Thus, iff the first parameterInfo is not optional then this MethodInfo is no longer a canidate.
////                        if(!parameterInfos[argumentTypes.Length].IsOptional)
////                        {
////                            testForParamArray = true;
////                        }
////                    }
////                    #endregion
////                }
////
////                #region ParamArray
////                if(testForParamArray)
////                {
////                    if(parameterInfos.Length == 0)
////                    {
////                        return false;
////                    }
////
////                    // The last argument of the signature could be a param array.
////                    bool shortByMoreThanOneSuppliedArgument = argumentTypes.Length < parameterInfos.Length - 1;
////
////                    if(shortByMoreThanOneSuppliedArgument)
////                    {
////                        return false;
////                    }
////
////                    ParameterInfo lastParameter = parameterInfos[parameterInfos.Length - 1];
////
////                    if(!lastParameter.ParameterType.IsArray)
////                    {
////                        return false;
////                    }
////
////                    if(!lastParameter.IsDefined( typeof( ParamArrayAttribute ), false ))
////                    {
////                        return false;
////                    }
////                }
////                #endregion
////
////                #endregion
////            }
////            else
////            {
////                #region Exact Binding
////                if((bindingFlags & BindingFlags.ExactBinding) != 0)
////                {
////                    // Legacy behavior is to ignore ExactBinding when InvokeMember is specified.
////                    // Why filter by InvokeMember? If the answer is we leave this to the binder then why not leave
////                    // all the rest of this  to the binder too? Further, what other semanitc would the binder
////                    // use for BindingFlags.ExactBinding besides this one? Further, why not include CreateInstance
////                    // in this if statement? That's just InvokeMethod with a constructor, right?
////                    if((bindingFlags & (BindingFlags.InvokeMethod)) == 0)
////                    {
////                        for(int i = 0; i < parameterInfos.Length; i++)
////                        {
////                            // a null argument type implies a null arg which is always a perfect match
////                            if(argumentTypes[i] != null && parameterInfos[i].ParameterType != argumentTypes[i])
////                            {
////                                return false;
////                            }
////                        }
////                    }
////                }
////                #endregion
////            }
////        }
////        #endregion
////
////        return true;
////    }

        #endregion

        #endregion

        #region Private Data Members
        private RuntimeTypeHandle m_handle;
////    private IntPtr            m_cache;

////    private class TypeCacheQueue
////    {
////        // must be a power of 2 for this to work
////        const int QUEUE_SIZE = 4;
////
////        Object[] liveCache;
////
////        internal TypeCacheQueue()
////        {
////            liveCache = new Object[QUEUE_SIZE];
////        }
////    }
////    private static TypeCacheQueue s_typeCache = null;
        #endregion

        #region Constructor
        private RuntimeType( RuntimeTypeHandle typeHandle )
        {
            m_handle = typeHandle;
        }

        internal RuntimeType() { }
        #endregion

        #region Private\Internal Members
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
////    internal override bool CacheEquals( object o )
////    {
////        RuntimeType m = o as RuntimeType;
////
////        if(m == null)
////        {
////            return false;
////        }
////
////        return m.m_handle.Equals( m_handle );
////    }
////
////    private new RuntimeTypeCache Cache
////    {
////        [ResourceExposure( ResourceScope.None )]
////        [ResourceConsumption( ResourceScope.AppDomain, ResourceScope.AppDomain )]
////        get
////        {
////            if(m_cache.IsNull())
////            {
////                IntPtr newgcHandle = m_handle.GetGCHandle( GCHandleType.WeakTrackResurrection );
////                IntPtr gcHandle = Interlocked.CompareExchange( ref m_cache, newgcHandle, (IntPtr)0 );
////                if(!gcHandle.IsNull())
////                {
////                    m_handle.FreeGCHandle( newgcHandle );
////                }
////            }
////
////            RuntimeTypeCache cache = GCHandle.InternalGet( m_cache ) as RuntimeTypeCache;
////            if(cache == null)
////            {
////                cache = new RuntimeTypeCache( this );
////
////                RuntimeTypeCache existingCache = GCHandle.InternalCompareExchange( m_cache, cache, null, false ) as RuntimeTypeCache;
////                if(existingCache != null)
////                {
////                    cache = existingCache;
////                }
////                if(s_typeCache == null)
////                {
////                    s_typeCache = new TypeCacheQueue();
////                }
////                //s_typeCache.Add(cache);
////            }
////            /*
////                            RuntimeTypeCache cache = m_cache as RuntimeTypeCache;
////                            if (cache == null)
////                            {
////                                cache = new RuntimeTypeCache(TypeHandle);
////                                RuntimeTypeCache existingCache = Interlocked.CompareExchange(ref m_cache, cache, null) as RuntimeTypeCache;
////                                if (existingCache != null)
////                                    cache = existingCache;
////                            }
////            */
////            ASSERT.CONSISTENCY_CHECK( cache != null );
////            return cache;
////        }
////    }
        #endregion

        #region Type Overrides

        #region Get XXXInfo Candidates
////    private MethodInfo[] GetMethodCandidates( String             name              ,
////                                              BindingFlags       bindingAttr       ,
////                                              CallingConventions callConv          ,
////                                              Type[]             types             ,
////                                              bool               allowPrefixLookup )
////    {
////        bool           prefixLookup;
////        bool           ignoreCase;
////        MemberListType listType;
////
////        RuntimeType.FilterHelper( bindingAttr, ref name, allowPrefixLookup, out prefixLookup, out ignoreCase, out listType );
////
////        List<MethodInfo>                candidates = new List<MethodInfo>();
////        CerArrayList<RuntimeMethodInfo> cache      = Cache.GetMethodList( listType, name );
////
////        bindingAttr ^= BindingFlags.DeclaredOnly;
////
////        for(int i = 0; i < cache.Count; i++)
////        {
////            RuntimeMethodInfo methodInfo = cache[i];
////
////            if((bindingAttr & methodInfo.BindingFlags) == methodInfo.BindingFlags &&
////                FilterApplyMethodBaseInfo( methodInfo, bindingAttr, callConv, types ) &&
////                (!prefixLookup || RuntimeType.FilterApplyPrefixLookup( methodInfo, name, ignoreCase )))
////            {
////                candidates.Add( methodInfo );
////            }
////        }
////
////        return candidates.ToArray();
////    }
////
////
////    private ConstructorInfo[] GetConstructorCandidates( string             name              ,
////                                                        BindingFlags       bindingAttr       ,
////                                                        CallingConventions callConv          ,
////                                                        Type[]             types             ,
////                                                        bool               allowPrefixLookup )
////    {
////        bool           prefixLookup;
////        bool           ignoreCase;
////        MemberListType listType;
////
////        RuntimeType.FilterHelper( bindingAttr, ref name, allowPrefixLookup, out prefixLookup, out ignoreCase, out listType );
////
////        List<ConstructorInfo>                candidates = new List<ConstructorInfo>();
////        CerArrayList<RuntimeConstructorInfo> cache      = Cache.GetConstructorList( listType, name );
////
////        bindingAttr ^= BindingFlags.DeclaredOnly;
////
////        for(int i = 0; i < cache.Count; i++)
////        {
////            RuntimeConstructorInfo constructorInfo = cache[i];
////
////            if((bindingAttr & constructorInfo.BindingFlags) == constructorInfo.BindingFlags &&
////                FilterApplyMethodBaseInfo( constructorInfo, bindingAttr, callConv, types ) &&
////                (!prefixLookup || RuntimeType.FilterApplyPrefixLookup( constructorInfo, name, ignoreCase )))
////            {
////                candidates.Add( constructorInfo );
////            }
////        }
////
////        return candidates.ToArray();
////    }
////
////
////    private PropertyInfo[] GetPropertyCandidates( String       name              ,
////                                                  BindingFlags bindingAttr       ,
////                                                  Type[]       types             ,
////                                                  bool         allowPrefixLookup )
////    {
////        bool           prefixLookup;
////        bool           ignoreCase;
////        MemberListType listType;
////
////        RuntimeType.FilterHelper( bindingAttr, ref name, allowPrefixLookup, out prefixLookup, out ignoreCase, out listType );
////
////        List<PropertyInfo>                candidates = new List<PropertyInfo>();
////        CerArrayList<RuntimePropertyInfo> cache      = Cache.GetPropertyList( listType, name );
////
////        bindingAttr ^= BindingFlags.DeclaredOnly;
////
////        for(int i = 0; i < cache.Count; i++)
////        {
////            RuntimePropertyInfo propertyInfo = cache[i];
////
////            if((bindingAttr & propertyInfo.BindingFlags) == propertyInfo.BindingFlags &&
////                (!prefixLookup || RuntimeType.FilterApplyPrefixLookup( propertyInfo, name, ignoreCase )) &&
////                (types == null || (propertyInfo.GetIndexParameters().Length == types.Length)))
////            {
////                candidates.Add( propertyInfo );
////            }
////        }
////
////        return candidates.ToArray();
////    }
////
////
////    private EventInfo[] GetEventCandidates( String       name              ,
////                                            BindingFlags bindingAttr       ,
////                                            bool         allowPrefixLookup )
////    {
////        bool           prefixLookup;
////        bool           ignoreCase;
////        MemberListType listType;
////
////        RuntimeType.FilterHelper( bindingAttr, ref name, allowPrefixLookup, out prefixLookup, out ignoreCase, out listType );
////
////        List<EventInfo>                candidates = new List<EventInfo>();
////        CerArrayList<RuntimeEventInfo> cache      = Cache.GetEventList( listType, name );
////
////        bindingAttr ^= BindingFlags.DeclaredOnly;
////
////        for(int i = 0; i < cache.Count; i++)
////        {
////            RuntimeEventInfo eventInfo = cache[i];
////
////            if((bindingAttr & eventInfo.BindingFlags) == eventInfo.BindingFlags &&
////                (!prefixLookup || RuntimeType.FilterApplyPrefixLookup( eventInfo, name, ignoreCase )))
////            {
////                candidates.Add( eventInfo );
////            }
////        }
////
////        return candidates.ToArray();
////    }
////
////    private FieldInfo[] GetFieldCandidates( String       name              ,
////                                            BindingFlags bindingAttr       ,
////                                            bool         allowPrefixLookup )
////    {
////        bool           prefixLookup;
////        bool           ignoreCase;
////        MemberListType listType;
////
////        RuntimeType.FilterHelper( bindingAttr, ref name, allowPrefixLookup, out prefixLookup, out ignoreCase, out listType );
////
////        List<FieldInfo>                candidates = new List<FieldInfo>();
////        CerArrayList<RuntimeFieldInfo> cache      = Cache.GetFieldList( listType, name );
////
////        bindingAttr ^= BindingFlags.DeclaredOnly;
////
////        for(int i = 0; i < cache.Count; i++)
////        {
////            RuntimeFieldInfo fieldInfo = cache[i];
////
////            if((bindingAttr & fieldInfo.BindingFlags) == fieldInfo.BindingFlags &&
////                (!prefixLookup || FilterApplyPrefixLookup( fieldInfo, name, ignoreCase )))
////            {
////                candidates.Add( fieldInfo );
////            }
////        }
////
////        return candidates.ToArray();
////    }
////
////    private Type[] GetNestedTypeCandidates( String       fullname          ,
////                                            BindingFlags bindingAttr       ,
////                                            bool         allowPrefixLookup )
////    {
////        string name;
////        string ns;
////
////        SplitName( fullname, out name, out ns );
////
////        bool           prefixLookup;
////        bool           ignoreCase;
////        MemberListType listType;
////
////        bindingAttr &= ~BindingFlags.Static;
////
////        RuntimeType.FilterHelper( bindingAttr, ref name, allowPrefixLookup, out prefixLookup, out ignoreCase, out listType );
////
////        List<Type>                candidates = new List<Type>();
////        CerArrayList<RuntimeType> cache      = Cache.GetNestedTypeList( listType, name );
////
////        for(int i = 0; i < cache.Count; i++)
////        {
////            RuntimeType nestedClass = cache[i];
////
////            if(RuntimeType.FilterApplyType( nestedClass, bindingAttr, name, prefixLookup, ns ))
////            {
////                candidates.Add( nestedClass );
////            }
////        }
////
////        return candidates.ToArray();
////    }
        #endregion

        #region Get All XXXInfos
        [MethodImpl( MethodImplOptions.InternalCall )]
        public override extern MethodInfo[] GetMethods( BindingFlags bindingAttr );
////    {
////        return GetMethodCandidates( null, bindingAttr, CallingConventions.Any, null, false );
////    }

////    public override ConstructorInfo[] GetConstructors( BindingFlags bindingAttr )
////    {
////        return GetConstructorCandidates( null, bindingAttr, CallingConventions.Any, null, false );
////    }

        [MethodImpl( MethodImplOptions.InternalCall )]
        public override extern PropertyInfo[] GetProperties( BindingFlags bindingAttr );
////    {
////        return GetPropertyCandidates( null, bindingAttr, null, false );
////    }

////    public override EventInfo[] GetEvents( BindingFlags bindingAttr )
////    {
////        return GetEventCandidates( null, bindingAttr, false );
////    }

        [MethodImpl( MethodImplOptions.InternalCall )]
        public override extern FieldInfo[] GetFields( BindingFlags bindingAttr );
////    {
////        return GetFieldCandidates( null, bindingAttr, false );
////    }

////    public override Type[] GetInterfaces()
////    {
////        CerArrayList<RuntimeType> candidates = this.Cache.GetInterfaceList( MemberListType.All, null );
////
////        Type[] interfaces = new Type[candidates.Count];
////        for(int i = 0; i < candidates.Count; i++)
////        {
////            FastArrayHandle.SetValueAt( interfaces, i, candidates[i] );
////        }
////
////        return interfaces;
////    }
////
////    public override Type[] GetNestedTypes( BindingFlags bindingAttr )
////    {
////        return GetNestedTypeCandidates( null, bindingAttr, false );
////    }

        [MethodImpl( MethodImplOptions.InternalCall )]
        public override extern MemberInfo[] GetMembers( BindingFlags bindingAttr );
////    {
////        MethodInfo[]      methods      = GetMethodCandidates     ( null, bindingAttr, CallingConventions.Any, null, false );
////        ConstructorInfo[] constructors = GetConstructorCandidates( null, bindingAttr, CallingConventions.Any, null, false );
////        PropertyInfo[]    properties   = GetPropertyCandidates   ( null, bindingAttr,                         null, false );
////        EventInfo[]       events       = GetEventCandidates      ( null, bindingAttr,                               false );
////        FieldInfo[]       fields       = GetFieldCandidates      ( null, bindingAttr,                               false );
////        Type[]            nestedTypes  = GetNestedTypeCandidates ( null, bindingAttr,                               false );
////
////        // Interfaces are excluded from the result of GetMembers
////
////        MemberInfo[] members = new MemberInfo[ methods     .Length +
////                                               constructors.Length +
////                                               properties  .Length +
////                                               events      .Length +
////                                               fields      .Length +
////                                               nestedTypes .Length ];
////
////        int i = 0;
////
////        Array.Copy( methods     , 0, members, i, methods     .Length ); i += methods     .Length;
////        Array.Copy( constructors, 0, members, i, constructors.Length ); i += constructors.Length;
////        Array.Copy( properties  , 0, members, i, properties  .Length ); i += properties  .Length;
////        Array.Copy( events      , 0, members, i, events      .Length ); i += events      .Length;
////        Array.Copy( fields      , 0, members, i, fields      .Length ); i += fields      .Length;
////        Array.Copy( nestedTypes , 0, members, i, nestedTypes .Length ); i += nestedTypes .Length;
////
////        ASSERT.POSTCONDITION( i == members.Length );
////        return members;
////    }
    
////    public override InterfaceMapping GetInterfaceMap( Type ifaceType )
////    {
////        if(IsGenericParameter)
////        {
////            throw new InvalidOperationException( Environment.GetResourceString( "Arg_GenericParameter" ) );
////        }
////
////        if(ifaceType == null)
////        {
////            throw new ArgumentNullException( "ifaceType" );
////        }
////
////        if(!(ifaceType is RuntimeType))
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Argument_MustBeRuntimeType" ), "ifaceType" );
////        }
////
////        RuntimeType       ifaceRtType       = ifaceType as RuntimeType;
////        RuntimeTypeHandle ifaceRtTypeHandle = ifaceRtType.GetTypeHandleInternal();
////
////        int firstIfaceSlot = GetTypeHandleInternal().GetFirstSlotForInterface( ifaceRtType.GetTypeHandleInternal() );
////        ASSERT.CONSISTENCY_CHECK( ifaceType.IsInterface );  // GetFirstSlotForInterface enforces this invariant
////        ASSERT.CONSISTENCY_CHECK( !IsInterface ); // GetFirstSlotForInterface enforces this invariant
////
////        int ifaceSlotCount         = ifaceRtTypeHandle.GetInterfaceMethodSlots();
////        int ifaceStaticMethodCount = 0;
////
////        // @Optimization - Most interface have the same number of static members.
////
////        // Filter out static methods
////        for(int i = 0; i < ifaceSlotCount; i++)
////        {
////            if((ifaceRtTypeHandle.GetMethodAt( i ).GetAttributes() & MethodAttributes.Static) != 0)
////            {
////                ifaceStaticMethodCount++;
////            }
////        }
////
////        int ifaceInstanceMethodCount = ifaceSlotCount - ifaceStaticMethodCount;
////
////        InterfaceMapping im;
////        im.InterfaceType    = ifaceType;
////        im.TargetType       = this;
////        im.InterfaceMethods = new MethodInfo[ifaceInstanceMethodCount];
////        im.TargetMethods    = new MethodInfo[ifaceInstanceMethodCount];
////
////        for(int i = 0; i < ifaceSlotCount; i++)
////        {
////            RuntimeMethodHandle ifaceRtMethodHandle = ifaceRtTypeHandle.GetMethodAt( i );
////
////            if((ifaceRtTypeHandle.GetMethodAt( i ).GetAttributes() & MethodAttributes.Static) != 0)
////            {
////                continue;
////            }
////
////            bool mayNeedInstantiatingStub = ifaceRtTypeHandle.HasInstantiation() && !ifaceRtTypeHandle.IsGenericTypeDefinition();
////
////            // if it is an instantiated type get the InstantiatedMethodDesc if needed
////            if(mayNeedInstantiatingStub)
////            {
////                ifaceRtMethodHandle = ifaceRtMethodHandle.GetInstantiatingStubIfNeeded( ifaceRtTypeHandle );
////            }
////
////            MethodBase ifaceMethodBase = RuntimeType.GetMethodBase( ifaceRtTypeHandle, ifaceRtMethodHandle );
////            ASSERT.CONSISTENCY_CHECK( ifaceMethodBase is RuntimeMethodInfo );
////            im.InterfaceMethods[i] = (MethodInfo)ifaceMethodBase;
////
////            // If the slot is -1, then virtual stub dispatch is active.
////            // Should remove old "firstIfaceSlot + i" code when old behaviour disappears.
////            int slot;
////            if(firstIfaceSlot == -1)
////            {
////                slot = GetTypeHandleInternal().GetInterfaceMethodImplementationSlot( ifaceRtTypeHandle, ifaceRtMethodHandle );
////            }
////            else
////            {
////                slot = firstIfaceSlot + i;
////            }
////
////            if(slot == -1) continue;
////
////
////            RuntimeTypeHandle   classRtTypeHandle   = GetTypeHandleInternal();
////            RuntimeMethodHandle classRtMethodHandle = classRtTypeHandle.GetMethodAt( slot );
////
////            mayNeedInstantiatingStub = classRtTypeHandle.HasInstantiation() && !classRtTypeHandle.IsGenericTypeDefinition();
////
////            if(mayNeedInstantiatingStub)
////            {
////                classRtMethodHandle = classRtMethodHandle.GetInstantiatingStubIfNeeded( classRtTypeHandle );
////            }
////
////            MethodBase rtTypeMethodBase = RuntimeType.GetMethodBase( classRtTypeHandle, classRtMethodHandle );
////            // a class may not implement all the methods of an interface (abstract class) so null is a valid value
////            ASSERT.CONSISTENCY_CHECK( rtTypeMethodBase == null || rtTypeMethodBase is RuntimeMethodInfo );
////            im.TargetMethods[i] = (MethodInfo)rtTypeMethodBase;
////        }
////
////        return im;
////    }
        #endregion

        #region Find XXXInfo
        [MethodImpl( MethodImplOptions.InternalCall )]
        protected override extern MethodInfo GetMethodImpl( String              name        ,
                                                            BindingFlags        bindingAttr ,
                                                            Binder              binder      ,
                                                            CallingConventions  callConv    ,
                                                            Type[]              types       ,
                                                            ParameterModifier[] modifiers   );
////    {
////        MethodInfo[] candidates = GetMethodCandidates( name, bindingAttr, callConv, types, false );
////        if(candidates.Length == 0)
////        {
////            return null;
////        }
////
////        if(types == null || types.Length == 0)
////        {
////            if(candidates.Length == 1)
////            {
////                return candidates[0];
////            }
////            else if(types == null)
////            {
////                for(int j = 1; j < candidates.Length; j++)
////                {
////                    MethodInfo methodInfo = candidates[j];
////                    if(!System.DefaultBinder.CompareMethodSigAndName( methodInfo, candidates[0] ))
////                    {
////                        throw new AmbiguousMatchException( Environment.GetResourceString( "Arg_AmbiguousMatchException" ) );
////                    }
////                }
////
////                // All the methods have the exact same name and sig so return the most derived one.
////                return System.DefaultBinder.FindMostDerivedNewSlotMeth( candidates, candidates.Length ) as MethodInfo;
////            }
////        }
////
////        if(binder == null)
////        {
////            binder = DefaultBinder;
////        }
////
////        return binder.SelectMethod( bindingAttr, candidates, types, modifiers ) as MethodInfo;
////    }

////    protected override ConstructorInfo GetConstructorImpl( BindingFlags        bindingAttr   ,
////                                                           Binder              binder        ,
////                                                           CallingConventions  callConvention,
////                                                           Type[]              types         ,
////                                                           ParameterModifier[] modifiers     )
////    {
////        ConstructorInfo[] candidates = GetConstructorCandidates( null, bindingAttr, CallingConventions.Any, types, false );
////
////        if(binder == null)
////        {
////            binder = DefaultBinder;
////        }
////
////        if(candidates.Length == 0)
////        {
////            return null;
////        }
////
////        if(types.Length == 0 && candidates.Length == 1)
////        {
////            ParameterInfo[] parameters = (candidates[0]).GetParametersNoCopy();
////            if(parameters == null || parameters.Length == 0)
////            {
////                return candidates[0];
////            }
////        }
////
////        if((bindingAttr & BindingFlags.ExactBinding) != 0)
////        {
////            return System.DefaultBinder.ExactBinding( candidates, types, modifiers ) as ConstructorInfo;
////        }
////
////        return binder.SelectMethod( bindingAttr, candidates, types, modifiers ) as ConstructorInfo;
////    }


        [MethodImpl( MethodImplOptions.InternalCall )]
        protected override extern PropertyInfo GetPropertyImpl( String              name        ,
                                                                BindingFlags        bindingAttr ,
                                                                Binder              binder      ,
                                                                Type                returnType  ,
                                                                Type[]              types       ,
                                                                ParameterModifier[] modifiers   );
////    {
////        if(name == null) throw new ArgumentNullException();
////
////        PropertyInfo[] candidates = GetPropertyCandidates( name, bindingAttr, types, false );
////
////        if(binder == null)
////        {
////            binder = DefaultBinder;
////        }
////
////        if(candidates.Length == 0)
////        {
////            return null;
////        }
////
////        if(types == null || types.Length == 0)
////        {
////            // no arguments
////            if(candidates.Length == 1)
////            {
////                if(returnType != null && returnType != candidates[0].PropertyType)
////                {
////                    return null;
////                }
////
////                return candidates[0];
////            }
////            else
////            {
////                if(returnType == null)
////                {
////                    // if we are here we have no args or property type to select over and we have more than one property with that name
////                    throw new AmbiguousMatchException( Environment.GetResourceString( "Arg_AmbiguousMatchException" ) );
////                }
////            }
////        }
////
////        if((bindingAttr & BindingFlags.ExactBinding) != 0)
////        {
////            return System.DefaultBinder.ExactPropertyBinding( candidates, returnType, types, modifiers );
////        }
////
////        return binder.SelectProperty( bindingAttr, candidates, returnType, types, modifiers );
////    }


////    public override EventInfo GetEvent( String name, BindingFlags bindingAttr )
////    {
////        if(name == null) throw new ArgumentNullException();
////
////        bool           ignoreCase;
////        MemberListType listType;
////
////        RuntimeType.FilterHelper( bindingAttr, ref name, out ignoreCase, out listType );
////
////        CerArrayList<RuntimeEventInfo> cache = Cache.GetEventList( listType, name );
////        EventInfo                      match = null;
////
////        bindingAttr ^= BindingFlags.DeclaredOnly;
////
////        for(int i = 0; i < cache.Count; i++)
////        {
////            RuntimeEventInfo eventInfo = cache[i];
////
////            if((bindingAttr & eventInfo.BindingFlags) == eventInfo.BindingFlags)
////            {
////                if(match != null)
////                {
////                    throw new AmbiguousMatchException( Environment.GetResourceString( "Arg_AmbiguousMatchException" ) );
////                }
////
////                match = eventInfo;
////            }
////        }
////
////        return match;
////    }

        [MethodImpl( MethodImplOptions.InternalCall )]
        public override extern FieldInfo GetField( String name, BindingFlags bindingAttr );
////    {
////        if(name == null) throw new ArgumentNullException();
////
////        bool           ignoreCase;
////        MemberListType listType;
////
////        RuntimeType.FilterHelper( bindingAttr, ref name, out ignoreCase, out listType );
////
////        CerArrayList<RuntimeFieldInfo> cache = Cache.GetFieldList( listType, name );
////        FieldInfo                      match = null;
////
////        bindingAttr ^= BindingFlags.DeclaredOnly;
////
////        bool multipleStaticFieldMatches = false;
////
////        for(int i = 0; i < cache.Count; i++)
////        {
////            RuntimeFieldInfo fieldInfo = cache[i];
////
////            if((bindingAttr & fieldInfo.BindingFlags) == fieldInfo.BindingFlags)
////            {
////                if(match != null)
////                {
////                    if(fieldInfo.DeclaringType == match.DeclaringType)
////                    {
////                        throw new AmbiguousMatchException( Environment.GetResourceString( "Arg_AmbiguousMatchException" ) );
////                    }
////
////                    if((match.DeclaringType.IsInterface == true) && (fieldInfo.DeclaringType.IsInterface == true))
////                    {
////                        multipleStaticFieldMatches = true;
////                    }
////                }
////
////                if(match == null || fieldInfo.DeclaringType.IsSubclassOf( match.DeclaringType ) || match.DeclaringType.IsInterface)
////                {
////                    match = fieldInfo;
////                }
////            }
////        }
////
////        if(multipleStaticFieldMatches && match.DeclaringType.IsInterface)
////        {
////            throw new AmbiguousMatchException( Environment.GetResourceString( "Arg_AmbiguousMatchException" ) );
////        }
////
////        return match;
////    }

////    public override Type GetInterface( String fullname, bool ignoreCase )
////    {
////        if(fullname == null) throw new ArgumentNullException();
////
////        BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.NonPublic;
////
////        bindingAttr &= ~BindingFlags.Static;
////
////        if(ignoreCase)
////        {
////            bindingAttr |= BindingFlags.IgnoreCase;
////        }
////
////        string name;
////        string ns;
////
////        SplitName( fullname, out name, out ns );
////
////        MemberListType listType;
////
////        RuntimeType.FilterHelper( bindingAttr, ref name, out ignoreCase, out listType );
////
////        CerArrayList<RuntimeType> cache = Cache.GetInterfaceList( listType, name );
////        RuntimeType               match = null;
////
////        for(int i = 0; i < cache.Count; i++)
////        {
////            RuntimeType iface = cache[i];
////
////            if(RuntimeType.FilterApplyType( iface, bindingAttr, name, false, ns ))
////            {
////                if(match != null)
////                {
////                    throw new AmbiguousMatchException( Environment.GetResourceString( "Arg_AmbiguousMatchException" ) );
////                }
////
////                match = iface;
////            }
////        }
////
////        return match;
////    }
////
////    public override Type GetNestedType( String fullname, BindingFlags bindingAttr )
////    {
////        if(fullname == null) throw new ArgumentNullException();
////
////        bindingAttr &= ~BindingFlags.Static;
////
////        string name;
////        string ns;
////
////        SplitName( fullname, out name, out ns );
////
////        bool           ignoreCase;
////        MemberListType listType;
////
////        RuntimeType.FilterHelper( bindingAttr, ref name, out ignoreCase, out listType );
////
////        CerArrayList<RuntimeType> cache = Cache.GetNestedTypeList( listType, name );
////        RuntimeType               match = null;
////
////        for(int i = 0; i < cache.Count; i++)
////        {
////            RuntimeType nestedType = cache[i];
////
////            if(RuntimeType.FilterApplyType( nestedType, bindingAttr, name, false, ns ))
////            {
////                if(match != null)
////                {
////                    throw new AmbiguousMatchException( Environment.GetResourceString( "Arg_AmbiguousMatchException" ) );
////                }
////
////                match = nestedType;
////            }
////        }
////
////        return match;
////    }
////
////    public override MemberInfo[] GetMember( String name, MemberTypes type, BindingFlags bindingAttr )
////    {
////        if(name == null) throw new ArgumentNullException();
////
////        MethodInfo[]      methods      = new MethodInfo     [0];
////        ConstructorInfo[] constructors = new ConstructorInfo[0];
////        PropertyInfo[]    properties   = new PropertyInfo   [0];
////        EventInfo[]       events       = new EventInfo      [0];
////        FieldInfo[]       fields       = new FieldInfo      [0];
////        Type[]            nestedTypes  = new Type           [0];
////
////        // Methods
////        if((type & MemberTypes.Method) != 0)
////        {
////            methods = GetMethodCandidates( name, bindingAttr, CallingConventions.Any, null, true );
////        }
////
////        // Constructors
////        if((type & MemberTypes.Constructor) != 0)
////        {
////            constructors = GetConstructorCandidates( name, bindingAttr, CallingConventions.Any, null, true );
////        }
////
////        // Properties
////        if((type & MemberTypes.Property) != 0)
////        {
////            properties = GetPropertyCandidates( name, bindingAttr, null, true );
////        }
////
////        // Events
////        if((type & MemberTypes.Event) != 0)
////        {
////            events = GetEventCandidates( name, bindingAttr, true );
////        }
////
////        // Fields
////        if((type & MemberTypes.Field) != 0)
////        {
////            fields = GetFieldCandidates( name, bindingAttr, true );
////        }
////
////        // NestedTypes
////        if((type & (MemberTypes.NestedType | MemberTypes.TypeInfo)) != 0)
////        {
////            nestedTypes = GetNestedTypeCandidates( name, bindingAttr, true );
////        }
////
////        switch(type)
////        {
////            case MemberTypes.Method | MemberTypes.Constructor:
////                MethodBase[] compressBaseses = new MethodBase[methods.Length + constructors.Length];
////
////                Array.Copy( methods     ,    compressBaseses, methods.Length                      );
////                Array.Copy( constructors, 0, compressBaseses, methods.Length, constructors.Length );
////                return compressBaseses;
////
////            case MemberTypes.Method:
////                return methods;
////
////            case MemberTypes.Constructor:
////                return constructors;
////
////            case MemberTypes.Field:
////                return fields;
////
////            case MemberTypes.Property:
////                return properties;
////
////            case MemberTypes.Event:
////                return events;
////
////            case MemberTypes.NestedType:
////                return nestedTypes;
////
////            case MemberTypes.TypeInfo:
////                return nestedTypes;
////        }
////
////        MemberInfo[] compressMembers = new MemberInfo[ methods     .Length +
////                                                       constructors.Length +
////                                                       properties  .Length +
////                                                       events      .Length +
////                                                       fields      .Length +
////                                                       nestedTypes .Length ];
////
////        int i = 0;
////        if(methods     .Length > 0) Array.Copy( methods     , 0, compressMembers, i, methods     .Length ); i += methods     .Length;
////        if(constructors.Length > 0) Array.Copy( constructors, 0, compressMembers, i, constructors.Length ); i += constructors.Length;
////        if(properties  .Length > 0) Array.Copy( properties  , 0, compressMembers, i, properties  .Length ); i += properties  .Length;
////        if(events      .Length > 0) Array.Copy( events      , 0, compressMembers, i, events      .Length ); i += events      .Length;
////        if(fields      .Length > 0) Array.Copy( fields      , 0, compressMembers, i, fields      .Length ); i += fields      .Length;
////        if(nestedTypes .Length > 0) Array.Copy( nestedTypes , 0, compressMembers, i, nestedTypes .Length ); i += nestedTypes .Length;
////
////        ASSERT.POSTCONDITION( i == compressMembers.Length );
////        return compressMembers;
////    }
        #endregion

        #region Identity
////    public override Module Module
////    {
////        get
////        {
////            return GetTypeHandleInternal().GetModuleHandle().GetModule();
////        }
////    }

        public override extern Assembly Assembly
        {
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
////        {
////            return GetTypeHandleInternal().GetAssemblyHandle().GetAssembly();
////        }
        }

        public override RuntimeTypeHandle TypeHandle
        {
            get
            {
                return m_handle;
            }
        }

////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
////    internal override RuntimeTypeHandle GetTypeHandleInternal()
////    {
////        return m_handle;
////    }

////    internal override TypeCode GetTypeCodeInternal()
////    {
////        TypeCode typeCode = Cache.TypeCode;
////
////        if(typeCode != TypeCode.Empty)
////        {
////            return typeCode;
////        }
////
////        CorElementType corElementType = GetTypeHandleInternal().GetCorElementType();
////        switch(corElementType)
////        {
////            case CorElementType.Boolean:
////                typeCode = TypeCode.Boolean; break;
////
////            case CorElementType.Char:
////                typeCode = TypeCode.Char; break;
////
////            case CorElementType.I1:
////                typeCode = TypeCode.SByte; break;
////
////            case CorElementType.U1:
////                typeCode = TypeCode.Byte; break;
////
////            case CorElementType.I2:
////                typeCode = TypeCode.Int16; break;
////
////            case CorElementType.U2:
////                typeCode = TypeCode.UInt16; break;
////
////            case CorElementType.I4:
////                typeCode = TypeCode.Int32; break;
////
////            case CorElementType.U4:
////                typeCode = TypeCode.UInt32; break;
////
////            case CorElementType.I8:
////                typeCode = TypeCode.Int64; break;
////
////            case CorElementType.U8:
////                typeCode = TypeCode.UInt64; break;
////
////            case CorElementType.R4:
////                typeCode = TypeCode.Single; break;
////
////            case CorElementType.R8:
////                typeCode = TypeCode.Double; break;
////
////            case CorElementType.String:
////                typeCode = TypeCode.String; break;
////
////            case CorElementType.ValueType:
////                if(this == Convert.ConvertTypes[(int)TypeCode.Decimal])
////                {
////                    typeCode = TypeCode.Decimal;
////                }
////                else if(this == Convert.ConvertTypes[(int)TypeCode.DateTime])
////                {
////                    typeCode = TypeCode.DateTime;
////                }
////                else if(this.IsEnum)
////                {
////                    typeCode = Type.GetTypeCode( Enum.GetUnderlyingType( this ) );
////                }
////                else
////                {
////                    typeCode = TypeCode.Object;
////                }
////                break;
////
////            default:
////                if(this == Convert.ConvertTypes[(int)TypeCode.DBNull])
////                {
////                    typeCode = TypeCode.DBNull;
////                }
////                else if(this == Convert.ConvertTypes[(int)TypeCode.String])
////                {
////                    typeCode = TypeCode.String;
////                }
////                else
////                {
////                    typeCode = TypeCode.Object;
////                }
////                break;
////        }
////
////        Cache.TypeCode = typeCode;
////
////        return typeCode;
////    }
////
////    public override MethodBase DeclaringMethod
////    {
////        get
////        {
////            if(!IsGenericParameter)
////            {
////                throw new InvalidOperationException( Environment.GetResourceString( "Arg_NotGenericParameter" ) );
////            }
////
////            RuntimeMethodHandle declaringMethod = GetTypeHandleInternal().GetDeclaringMethod();
////
////            if(declaringMethod.IsNullHandle())
////            {
////                return null;
////            }
////
////            return GetMethodBase( declaringMethod.GetDeclaringType(), declaringMethod );
////        }
////    }
        #endregion

        #region Hierarchy
////    public override bool IsInstanceOfType( Object o )
////    {
////        return GetTypeHandleInternal().IsInstanceOfType( o );
////    }

        public override bool IsSubclassOf( Type type )
        {
            if(type == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "type" );
#else
                throw new ArgumentNullException();
#endif
            }
            ASSERT.PRECONDITION( type is RuntimeType );

            Type baseType = BaseType;

            while(baseType != null)
            {
                if(baseType == type)
                {
                    return true;
                }

                baseType = baseType.BaseType;
            }

            // pretty much everything is a subclass of object, even interfaces
            // notice that interfaces are really odd because they do not have a BaseType
            // yet IsSubclassOf(typeof(object)) returns true
            if(type == typeof( Object ) && type != this)
            {
                return true;
            }

            return false;
        }

        public override extern Type BaseType
        {
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
////        {
////            if(IsInterface)
////            {
////                return null;
////            }
////
////            if(m_handle.IsGenericVariable())
////            {
////                Type[] constraints = GetGenericParameterConstraints();
////
////                Type baseType = typeof( object );
////
////                for(int i = 0; i < constraints.Length; i++)
////                {
////                    Type constraint = constraints[i];
////
////                    if(constraint.IsInterface)
////                    {
////                        continue;
////                    }
////
////                    if(constraint.IsGenericParameter)
////                    {
////                        GenericParameterAttributes special;
////                        special = constraint.GenericParameterAttributes & GenericParameterAttributes.SpecialConstraintMask;
////
////                        if((special & GenericParameterAttributes.ReferenceTypeConstraint       ) == 0 &&
////                           (special & GenericParameterAttributes.NotNullableValueTypeConstraint) == 0  )
////                        {
////                            continue;
////                        }
////                    }
////
////                    baseType = constraint;
////                }
////
////                if(baseType == typeof( object ))
////                {
////                    GenericParameterAttributes special;
////
////                    special = GenericParameterAttributes & GenericParameterAttributes.SpecialConstraintMask;
////
////                    if((special & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0)
////                    {
////                        baseType = typeof( ValueType );
////                    }
////                }
////
////                return baseType;
////            }
////
////            return m_handle.GetBaseTypeHandle().GetRuntimeType();
////        }
        }

        public override Type UnderlyingSystemType
        {
            get
            {
                // Origional Comment: Return the underlying Type that represents the IReflect Object.
                // For expando object, this is the (Object) IReflectInstance.GetType().  For Type object it is this.
                return this;
            }
        }
        #endregion

        #region Name
        public override extern String FullName
        {
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
////        {
////            return Cache.GetFullName();
////        }
        }
    
        public override extern String AssemblyQualifiedName
        {
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
////        {
////            if(!IsGenericTypeDefinition && ContainsGenericParameters)
////            {
////                return null;
////            }
////
////            return Assembly.CreateQualifiedName( this.Assembly.FullName, this.FullName );
////        }
        }
    
        public override extern String Namespace
        {
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
////        {
////            string ns = Cache.GetNameSpace();
////
////            if(ns == null || ns.Length == 0)
////            {
////                return null;
////            }
////
////            return ns;
////        }
        }
        #endregion

        #region Attributes
////    protected override TypeAttributes GetAttributeFlagsImpl()
////    {
////        return m_handle.GetAttributes();
////    }
////
////    public override Guid GUID
////    {
////        get
////        {
////            Guid result = new Guid();
////
////            GetGUID( ref result );
////
////            return result;
////        }
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private extern void GetGUID( ref Guid result );
////
////    protected override bool IsContextfulImpl()
////    {
////        return GetTypeHandleInternal().IsContextful();
////    }
////
////    /*
////    protected override bool IsMarshalByRefImpl()
////    {
////        return GetTypeHandleInternal().IsMarshalByRef();
////    }
////    */
////
////    protected override bool IsByRefImpl()
////    {
////        CorElementType corElemType = GetTypeHandleInternal().GetCorElementType();
////
////        return (corElemType == CorElementType.ByRef);
////    }
////
////    protected override bool IsPrimitiveImpl()
////    {
////        CorElementType corElemType = GetTypeHandleInternal().GetCorElementType();
////
////        return (corElemType >= CorElementType.Boolean && corElemType <= CorElementType.R8) ||
////                corElemType == CorElementType.I ||
////                corElemType == CorElementType.U;
////    }
////
////    protected override bool IsPointerImpl()
////    {
////        CorElementType corElemType = GetTypeHandleInternal().GetCorElementType();
////
////        return (corElemType == CorElementType.Ptr);
////    }
////
////    protected override bool IsCOMObjectImpl()
////    {
////        return GetTypeHandleInternal().IsComObject( false );
////    }
////
////    internal override bool HasProxyAttributeImpl()
////    {
////        return GetTypeHandleInternal().HasProxyAttribute();
////    }
////
////    protected override bool HasElementTypeImpl()
////    {
////        return (IsArray || IsPointer || IsByRef);
////    }
////
////    public override GenericParameterAttributes GenericParameterAttributes
////    {
////        get
////        {
////            if(!IsGenericParameter)
////            {
////                throw new InvalidOperationException( Environment.GetResourceString( "Arg_NotGenericParameter" ) );
////            }
////
////            GenericParameterAttributes attributes;
////
////            GetTypeHandleInternal().GetModuleHandle().GetMetadataImport().GetGenericParamProps( MetadataToken, out attributes );
////
////            return attributes;
////        }
////    }
        #endregion

        #region Arrays
////    internal override bool IsSzArray
////    {
////        get
////        {
////            CorElementType corElemType = GetTypeHandleInternal().GetCorElementType();
////
////            return (corElemType == CorElementType.SzArray);
////        }
////    }
////
////    protected override bool IsArrayImpl()
////    {
////        CorElementType corElemType = GetTypeHandleInternal().GetCorElementType();
////
////        return (corElemType == CorElementType.Array || corElemType == CorElementType.SzArray);
////    }
////
////    public override int GetArrayRank()
////    {
////        if(!IsArrayImpl())
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Argument_HasToBeArrayClass" ) );
////        }
////
////        return GetTypeHandleInternal().GetArrayRank();
////    }

        [MethodImpl( MethodImplOptions.InternalCall )]
        public override extern Type GetElementType();
////    {
////        return GetTypeHandleInternal().GetElementType().GetRuntimeType();
////    }
        #endregion

        #region Generics
////    public override Type[] GetGenericArguments()
////    {
////        Type[]              rtypes = null;
////        RuntimeTypeHandle[] types  = GetRootElementType().GetTypeHandleInternal().GetInstantiation();
////
////        if(types != null)
////        {
////            rtypes = new Type[types.Length];
////
////            for(int i = 0; i < types.Length; i++)
////            {
////                rtypes[i] = types[i].GetRuntimeType();
////            }
////        }
////        else
////        {
////            rtypes = new Type[0];
////        }
////
////        return rtypes;
////    }
////
////    public override Type MakeGenericType( Type[] instantiation )
////    {
////        if(instantiation == null)
////        {
////            throw new ArgumentNullException( "instantiation" );
////        }
////
////        Type[] instantiationCopy = new Type[instantiation.Length];
////
////        for(int i = 0; i < instantiation.Length; i++)
////        {
////            instantiationCopy[i] = instantiation[i];
////        }
////
////        instantiation = instantiationCopy;
////
////        if(!IsGenericTypeDefinition)
////        {
////            throw new InvalidOperationException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "Arg_NotGenericTypeDefinition" ), this ) );
////        }
////
////        for(int i = 0; i < instantiation.Length; i++)
////        {
////            if(instantiation[i] == null)
////            {
////                throw new ArgumentNullException();
////            }
////
////            if(!(instantiation[i] is RuntimeType))
////            {
////                return new TypeBuilderInstantiation( this, instantiation );
////            }
////        }
////
////        Type[] genericParameters = GetGenericArguments();
////
////        SanityCheckGenericArguments( instantiation, genericParameters );
////
////        RuntimeTypeHandle[] typeHandles = new RuntimeTypeHandle[instantiation.Length];
////
////        for(int i = 0; i < instantiation.Length; i++)
////        {
////            typeHandles[i] = instantiation[i].GetTypeHandleInternal();
////        }
////
////        Type ret = null;
////        try
////        {
////            ret = m_handle.Instantiate( typeHandles ).GetRuntimeType();
////        }
////        catch(TypeLoadException e)
////        {
////            ValidateGenericArguments( this, instantiation, e );
////            throw e;
////        }
////
////        return ret;
////    }
////
////    public override bool IsGenericTypeDefinition
////    {
////        get
////        {
////            return m_handle.IsGenericTypeDefinition();
////        }
////    }
////
////    public override bool IsGenericParameter
////    {
////        get
////        {
////            return m_handle.IsGenericVariable();
////        }
////    }
////
////    public override int GenericParameterPosition
////    {
////        get
////        {
////            if(!IsGenericParameter)
////            {
////                throw new InvalidOperationException( Environment.GetResourceString( "Arg_NotGenericParameter" ) );
////            }
////
////            return m_handle.GetGenericVariableIndex();
////        }
////    }
////
////    public override Type GetGenericTypeDefinition()
////    {
////        if(!IsGenericType)
////        {
////            throw new InvalidOperationException();
////        }
////
////        return m_handle.GetGenericTypeDefinition().GetRuntimeType();
////    }
////
////    public override bool IsGenericType
////    {
////        get
////        {
////            return HasElementType ? false : GetTypeHandleInternal().HasInstantiation();
////        }
////    }
////
////    public override bool ContainsGenericParameters
////    {
////        get
////        {
////            return GetRootElementType().GetTypeHandleInternal().ContainsGenericVariables();
////        }
////    }
////
////    public override Type[] GetGenericParameterConstraints()
////    {
////        if(!IsGenericParameter)
////        {
////            throw new InvalidOperationException( Environment.GetResourceString( "Arg_NotGenericParameter" ) );
////        }
////
////        RuntimeTypeHandle[] constraintHandles = m_handle.GetConstraints();
////        Type[]              constraints       = new Type[constraintHandles.Length];
////
////        for(int i = 0; i < constraints.Length; i++)
////        {
////            constraints[i] = constraintHandles[i].GetRuntimeType();
////        }
////
////        return constraints;
////    }
        #endregion

        #region Misc
////    public override Type MakePointerType()
////    {
////        return m_handle.MakePointer().GetRuntimeType();
////    }
////
////    public override Type MakeByRefType()
////    {
////        return m_handle.MakeByRef().GetRuntimeType();
////    }
////
////    public override Type MakeArrayType()
////    {
////        return m_handle.MakeSZArray().GetRuntimeType();
////    }
////
////    public override Type MakeArrayType( int rank )
////    {
////        if(rank <= 0)
////        {
////            throw new IndexOutOfRangeException();
////        }
////
////        return m_handle.MakeArray( rank ).GetRuntimeType();
////    }
////    public override StructLayoutAttribute StructLayoutAttribute
////    {
////        get
////        {
////            return (StructLayoutAttribute)StructLayoutAttribute.GetCustomAttribute( this );
////        }
////    }
        #endregion

        #region Invoke Member
////    private const BindingFlags MemberBindingMask       = (BindingFlags)0x000000FF;
////    private const BindingFlags InvocationMask          = (BindingFlags)0x0000FF00;
////    private const BindingFlags BinderNonCreateInstance = BindingFlags.InvokeMethod | BinderGetSetField | BinderGetSetProperty;
////    private const BindingFlags BinderGetSetProperty    = BindingFlags.GetProperty | BindingFlags.SetProperty;
////    private const BindingFlags BinderSetInvokeProperty = BindingFlags.InvokeMethod | BindingFlags.SetProperty;
////    private const BindingFlags BinderGetSetField       = BindingFlags.GetField | BindingFlags.SetField;
////    private const BindingFlags BinderSetInvokeField    = BindingFlags.SetField | BindingFlags.InvokeMethod;
////    private const BindingFlags BinderNonFieldGetSet    = (BindingFlags)0x00FFF300;
////    private const BindingFlags ClassicBindingMask      = BindingFlags.InvokeMethod | BindingFlags.GetProperty | BindingFlags.SetProperty |
////                                                         BindingFlags.PutDispProperty | BindingFlags.PutRefDispProperty;
////
////    private static Type s_typedRef = typeof( TypedReference );
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    static private extern bool CanValueSpecialCast( IntPtr valueType, IntPtr targetType );
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    static private extern Object AllocateObjectForByRef( RuntimeTypeHandle type, object value );
////
////    internal unsafe Object CheckValue( Object value, Binder binder, CultureInfo culture, BindingFlags invokeAttr )
////    {
////        // this method is used by invocation in reflection to check whether a value can be assigned to type.
////        if(IsInstanceOfType( value ))
////        {
////            return value;
////        }
////
////        // if this is a ByRef get the element type and check if it's compatible
////        bool isByRef = IsByRef;
////        if(isByRef)
////        {
////            Type elementType = GetElementType();
////            if(elementType.IsInstanceOfType( value ) || value == null)
////            {
////                // need to create an instance of the ByRef if null was provided, but only if primitive, enum or value type
////                return AllocateObjectForByRef( elementType.TypeHandle, value );
////            }
////        }
////        else if(value == null)
////        {
////            return value;
////        }
////        else if(this == s_typedRef)
////        {
////            // everything works for a typedref
////            return value;
////        }
////
////        // check the strange ones courtesy of reflection:
////        // - implicit cast between primitives
////        // - enum treated as underlying type
////        // - IntPtr and System.Reflection.Pointer to pointer types
////        bool needsSpecialCast = IsPointer || IsEnum || IsPrimitive;
////        if(needsSpecialCast)
////        {
////            Pointer pointer = value as Pointer;
////            Type    valueType;
////
////            if(pointer != null)
////            {
////                valueType = pointer.GetPointerType();
////            }
////            else
////            {
////                valueType = value.GetType();
////            }
////
////            if(CanValueSpecialCast( valueType.TypeHandle.Value, TypeHandle.Value ))
////            {
////                if(pointer != null)
////                {
////                    return pointer.GetPointerValue();
////                }
////                else
////                {
////                    return value;
////                }
////            }
////        }
////
////        if((invokeAttr & BindingFlags.ExactBinding) == BindingFlags.ExactBinding)
////        {
////            throw new ArgumentException( String.Format( CultureInfo.CurrentUICulture, Environment.GetResourceString( "Arg_ObjObjEx" ), value.GetType(), this ) );
////        }
////
////        if(binder != null && binder != Type.DefaultBinder)
////        {
////            value = binder.ChangeType( value, this, culture );
////            if(IsInstanceOfType( value ))
////            {
////                return value;
////            }
////
////            // if this is a ByRef get the element type and check if it's compatible
////            if(isByRef)
////            {
////                Type elementType = GetElementType();
////                if(elementType.IsInstanceOfType( value ) || value == null)
////                {
////                    return AllocateObjectForByRef( elementType.TypeHandle, value );
////                }
////            }
////            else if(value == null)
////            {
////                return value;
////            }
////
////            if(needsSpecialCast)
////            {
////                Pointer pointer = value as Pointer;
////                Type    valueType;
////
////                if(pointer != null)
////                {
////                    valueType = pointer.GetPointerType();
////                }
////                else
////                {
////                    valueType = value.GetType();
////                }
////
////                if(CanValueSpecialCast( valueType.TypeHandle.Value, TypeHandle.Value ))
////                {
////                    if(pointer != null)
////                    {
////                        return pointer.GetPointerValue();
////                    }
////                    else
////                    {
////                        return value;
////                    }
////                }
////            }
////        }
////
////        throw new ArgumentException( String.Format( CultureInfo.CurrentUICulture, Environment.GetResourceString( "Arg_ObjObjEx" ), value.GetType(), this ) );
////    }

        [DebuggerStepThroughAttribute]
        [Diagnostics.DebuggerHidden]
        [MethodImpl( MethodImplOptions.InternalCall )]
        public override extern Object InvokeMember( String              name         ,
                                                    BindingFlags        bindingFlags ,
                                                    Binder              binder       ,
                                                    Object              target       ,
                                                    Object[]            providedArgs ,
                                                    ParameterModifier[] modifiers    ,
                                                    CultureInfo         culture      ,
                                                    String[]            namedParams  );
////    {
////        if(IsGenericParameter)
////        {
////            throw new InvalidOperationException( Environment.GetResourceString( "Arg_GenericParameter" ) );
////        }
////
////        #region Preconditions
////        if((bindingFlags & InvocationMask) == 0)
////        {
////            // "Must specify binding flags describing the invoke operation required."
////            throw new ArgumentException( Environment.GetResourceString( "Arg_NoAccessSpec" ), "bindingFlags" );
////        }
////
////        // Provide a default binding mask if none is provided
////        if((bindingFlags & MemberBindingMask) == 0)
////        {
////            bindingFlags |= BindingFlags.Instance | BindingFlags.Public;
////
////            if((bindingFlags & BindingFlags.CreateInstance) == 0)
////            {
////                bindingFlags |= BindingFlags.Static;
////            }
////        }
////
////        // There must not be more named parameters than provided arguments
////        if(namedParams != null)
////        {
////            if(providedArgs != null)
////            {
////                if(namedParams.Length > providedArgs.Length)
////                {
////                    // "Named parameter array can not be bigger than argument array."
////                    throw new ArgumentException( Environment.GetResourceString( "Arg_NamedParamTooBig" ), "namedParams" );
////                }
////            }
////            else
////            {
////                if(namedParams.Length != 0)
////                {
////                    // "Named parameter array can not be bigger than argument array."
////                    throw new ArgumentException( Environment.GetResourceString( "Arg_NamedParamTooBig" ), "namedParams" );
////                }
////            }
////        }
////        #endregion
////
////        #region Check that any named paramters are not null
////        if(namedParams != null && Array.IndexOf( namedParams, null ) != -1)
////        {
////            // "Named parameter value must not be null."
////            throw new ArgumentException( Environment.GetResourceString( "Arg_NamedParamNull" ), "namedParams" );
////        }
////        #endregion
////
////        int argCnt = (providedArgs != null) ? providedArgs.Length : 0;
////
////        #region Get a Binder
////        if(binder == null)
////        {
////            binder = DefaultBinder;
////        }
////
////        bool bDefaultBinder = (binder == DefaultBinder);
////        #endregion
////
////        #region Delegate to Activator.CreateInstance
////        if((bindingFlags & BindingFlags.CreateInstance) != 0)
////        {
////            if((bindingFlags & BindingFlags.CreateInstance) != 0 && (bindingFlags & BinderNonCreateInstance) != 0)
////            {
////                // "Can not specify both CreateInstance and another access type."
////                throw new ArgumentException( Environment.GetResourceString( "Arg_CreatInstAccess" ), "bindingFlags" );
////            }
////
////            return Activator.CreateInstance( this, bindingFlags, binder, providedArgs, culture );
////        }
////        #endregion
////
////        // PutDispProperty and\or PutRefDispProperty ==> SetProperty.
////        if((bindingFlags & (BindingFlags.PutDispProperty | BindingFlags.PutRefDispProperty)) != 0)
////        {
////            bindingFlags |= BindingFlags.SetProperty;
////        }
////
////        #region Name
////        if(name == null)
////        {
////            throw new ArgumentNullException( "name" );
////        }
////
////        if(name.Length == 0 || name.Equals( @"[DISPID=0]" ))
////        {
////            name = GetDefaultMemberName();
////
////            if(name == null)
////            {
////                // in InvokeMember we always pretend there is a default member if none is provided and we make it ToString
////                name = "ToString";
////            }
////        }
////        #endregion
////
////        #region GetField or SetField
////        bool IsGetField = (bindingFlags & BindingFlags.GetField) != 0;
////        bool IsSetField = (bindingFlags & BindingFlags.SetField) != 0;
////
////        if(IsGetField || IsSetField)
////        {
////            #region Preconditions
////            if(IsGetField)
////            {
////                if(IsSetField)
////                {
////                    // "Can not specify both Get and Set on a field."
////                    throw new ArgumentException( Environment.GetResourceString( "Arg_FldSetGet" ), "bindingFlags" );
////                }
////
////                if((bindingFlags & BindingFlags.SetProperty) != 0)
////                {
////                    // "Can not specify both GetField and SetProperty."
////                    throw new ArgumentException( Environment.GetResourceString( "Arg_FldGetPropSet" ), "bindingFlags" );
////                }
////            }
////            else
////            {
////                ASSERT.CONSISTENCY_CHECK( IsSetField );
////
////                if(providedArgs == null)
////                {
////                    throw new ArgumentNullException( "providedArgs" );
////                }
////
////                if((bindingFlags & BindingFlags.GetProperty) != 0)
////                {
////                    // "Can not specify both SetField and GetProperty."
////                    throw new ArgumentException( Environment.GetResourceString( "Arg_FldSetPropGet" ), "bindingFlags" );
////                }
////
////                if((bindingFlags & BindingFlags.InvokeMethod) != 0)
////                {
////                    // "Can not specify Set on a Field and Invoke on a method."
////                    throw new ArgumentException( Environment.GetResourceString( "Arg_FldSetInvoke" ), "bindingFlags" );
////                }
////            }
////            #endregion
////
////            #region Lookup Field
////            FieldInfo   selFld = null;
////            FieldInfo[] flds   = GetMember( name, MemberTypes.Field, bindingFlags ) as FieldInfo[];
////
////            ASSERT.CONSISTENCY_CHECK( flds != null );
////
////            if(flds.Length == 1)
////            {
////                selFld = flds[0];
////            }
////            else if(flds.Length > 0)
////            {
////                selFld = binder.BindToField( bindingFlags, flds, IsGetField ? Empty.Value : providedArgs[0], culture );
////            }
////            #endregion
////
////            if(selFld != null)
////            {
////                #region Invocation on a field
////                if(selFld.FieldType.IsArray || selFld.FieldType == typeof( System.Array ))
////                {
////                    #region Invocation of an array Field
////                    int idxCnt;
////
////                    if((bindingFlags & BindingFlags.GetField) != 0)
////                    {
////                        idxCnt = argCnt;
////                    }
////                    else
////                    {
////                        idxCnt = argCnt - 1;
////                    }
////
////                    if(idxCnt > 0)
////                    {
////                        // Verify that all of the index values are ints
////                        int[] idx = new int[idxCnt];
////                        for(int i = 0; i < idxCnt; i++)
////                        {
////                            try
////                            {
////                                idx[i] = ((IConvertible)providedArgs[i]).ToInt32( null );
////                            }
////                            catch(InvalidCastException)
////                            {
////                                throw new ArgumentException( Environment.GetResourceString( "Arg_IndexMustBeInt" ) );
////                            }
////                        }
////
////                        // Set or get the value...
////                        Array a = (Array)selFld.GetValue( target );
////
////                        // Set or get the value in the array
////                        if((bindingFlags & BindingFlags.GetField) != 0)
////                        {
////                            return a.GetValue( idx );
////                        }
////                        else
////                        {
////                            a.SetValue( providedArgs[idxCnt], idx );
////                            return null;
////                        }
////                    }
////                    #endregion
////                }
////
////                if(IsGetField)
////                {
////                    #region Get the field value
////                    if(argCnt != 0)
////                    {
////                        throw new ArgumentException( Environment.GetResourceString( "Arg_FldGetArgErr" ), "bindingFlags" );
////                    }
////
////                    return selFld.GetValue( target );
////                    #endregion
////                }
////                else
////                {
////                    #region Set the field Value
////                    if(argCnt != 1)
////                    {
////                        throw new ArgumentException( Environment.GetResourceString( "Arg_FldSetArgErr" ), "bindingFlags" );
////                    }
////
////                    selFld.SetValue( target, providedArgs[0], bindingFlags, binder, culture );
////
////                    return null;
////                    #endregion
////                }
////                #endregion
////            }
////
////            if((bindingFlags & BinderNonFieldGetSet) == 0)
////            {
////                throw new MissingFieldException( FullName, name );
////            }
////        }
////        #endregion
////
////        #region Caching Logic
////        /*
////        bool useCache = false;
////
////        // Note that when we add something to the cache, we are careful to ensure
////        // that the actual providedArgs matches the parameters of the method.  Otherwise,
////        // some default argument processing has occurred.  We don't want anyone
////        // else with the same (insufficient) number of actual arguments to get a
////        // cache hit because then they would bypass the default argument processing
////        // and the invocation would fail.
////        if (bDefaultBinder && namedParams == null && argCnt < 6)
////            useCache = true;
////
////        if (useCache)
////        {
////            MethodBase invokeMethod = GetMethodFromCache (name, bindingFlags, argCnt, providedArgs);
////
////            if (invokeMethod != null)
////                return ((MethodInfo) invokeMethod).Invoke(target, bindingFlags, binder, providedArgs, culture);
////        }
////        */
////        #endregion
////
////        #region Property PreConditions
////        // @Legacy - This is RTM behavior
////        bool isGetProperty = (bindingFlags & BindingFlags.GetProperty) != 0;
////        bool isSetProperty = (bindingFlags & BindingFlags.SetProperty) != 0;
////
////        if(isGetProperty || isSetProperty)
////        {
////            #region Preconditions
////            if(isGetProperty)
////            {
////                ASSERT.CONSISTENCY_CHECK( !IsSetField );
////
////                if(isSetProperty)
////                {
////                    throw new ArgumentException( Environment.GetResourceString( "Arg_PropSetGet" ), "bindingFlags" );
////                }
////            }
////            else
////            {
////                ASSERT.CONSISTENCY_CHECK( isSetProperty );
////
////                ASSERT.CONSISTENCY_CHECK( !IsGetField );
////
////                if((bindingFlags & BindingFlags.InvokeMethod) != 0)
////                {
////                    throw new ArgumentException( Environment.GetResourceString( "Arg_PropSetInvoke" ), "bindingFlags" );
////                }
////            }
////            #endregion
////        }
////        #endregion
////
////        MethodInfo[] finalists = null;
////        MethodInfo   finalist  = null;
////
////        #region BindingFlags.InvokeMethod
////        if((bindingFlags & BindingFlags.InvokeMethod) != 0)
////        {
////            #region Lookup Methods
////            MethodInfo[] semiFinalists = GetMember( name, MemberTypes.Method, bindingFlags ) as MethodInfo[];
////            ArrayList    results       = null;
////
////            for(int i = 0; i < semiFinalists.Length; i++)
////            {
////                MethodInfo semiFinalist = semiFinalists[i];
////                ASSERT.CONSISTENCY_CHECK( semiFinalist != null );
////
////                if(!FilterApplyMethodBaseInfo( semiFinalist, bindingFlags, null, CallingConventions.Any, new Type[argCnt], false ))
////                {
////                    continue;
////                }
////
////                if(finalist == null)
////                {
////                    finalist = semiFinalist;
////                }
////                else
////                {
////                    if(results == null)
////                    {
////                        results = new ArrayList( semiFinalists.Length );
////                        results.Add( finalist );
////                    }
////
////                    results.Add( semiFinalist );
////                }
////            }
////
////            if(results != null)
////            {
////                ASSERT.CONSISTENCY_CHECK( results.Count > 1 );
////                finalists = new MethodInfo[results.Count];
////                results.CopyTo( finalists );
////            }
////            #endregion
////        }
////        #endregion
////
////        ASSERT.CONSISTENCY_CHECK( LOGIC.IMPLIES( finalists != null, finalist != null ) );
////
////        #region BindingFlags.GetProperty or BindingFlags.SetProperty
////        if(finalist == null && isGetProperty || isSetProperty)
////        {
////            #region Lookup Property
////            PropertyInfo[] semiFinalists = GetMember( name, MemberTypes.Property, bindingFlags ) as PropertyInfo[];
////            ArrayList      results       = null;
////
////            for(int i = 0; i < semiFinalists.Length; i++)
////            {
////                MethodInfo semiFinalist = null;
////
////                if(isSetProperty)
////                {
////                    semiFinalist = semiFinalists[i].GetSetMethod( true );
////                }
////                else
////                {
////                    semiFinalist = semiFinalists[i].GetGetMethod( true );
////                }
////
////                if(semiFinalist == null)
////                {
////                    continue;
////                }
////
////                if(!FilterApplyMethodBaseInfo( semiFinalist, bindingFlags, null, CallingConventions.Any, new Type[argCnt], false ))
////                {
////                    continue;
////                }
////
////                if(finalist == null)
////                {
////                    finalist = semiFinalist;
////                }
////                else
////                {
////                    if(results == null)
////                    {
////                        results = new ArrayList( semiFinalists.Length );
////                        results.Add( finalist );
////                    }
////
////                    results.Add( semiFinalist );
////                }
////            }
////
////            if(results != null)
////            {
////                ASSERT.CONSISTENCY_CHECK( results.Count > 1 );
////                finalists = new MethodInfo[results.Count];
////                results.CopyTo( finalists );
////            }
////            #endregion
////        }
////        #endregion
////
////        if(finalist != null)
////        {
////            #region Invoke
////            if(finalists == null && argCnt == 0 && finalist.GetParametersNoCopy().Length == 0 && (bindingFlags & BindingFlags.OptionalParamBinding) == 0)
////            {
////                //if (useCache && argCnt == props[0].GetParameters().Length)
////                //    AddMethodToCache(name, bindingFlags, argCnt, providedArgs, props[0]);
////
////                return finalist.Invoke( target, bindingFlags, binder, providedArgs, culture );
////            }
////
////            if(finalists == null)
////            {
////                finalists = new MethodInfo[] { finalist };
////            }
////
////            if(providedArgs == null)
////            {
////                providedArgs = new Object[0];
////            }
////
////            Object state = null;
////
////
////            MethodBase invokeMethod = null;
////
////            try
////            {
////                invokeMethod = binder.BindToMethod( bindingFlags, finalists, ref providedArgs, modifiers, culture, namedParams, out state );
////            }
////            catch(MissingMethodException)
////            {
////            }
////
////            if(invokeMethod == null)
////            {
////                throw new MissingMethodException( FullName, name );
////            }
////
////            //if (useCache && argCnt == invokeMethod.GetParameters().Length)
////            //    AddMethodToCache(name, bindingFlags, argCnt, providedArgs, invokeMethod);
////
////            Object result = ((MethodInfo)invokeMethod).Invoke( target, bindingFlags, binder, providedArgs, culture );
////
////            if(state != null)
////            {
////                binder.ReorderArgumentArray( ref providedArgs, state );
////            }
////
////            return result;
////            #endregion
////        }
////
////        throw new MissingMethodException( FullName, name );
////    }
        #endregion

        #endregion

        #region Object Overrides
////    public override bool Equals( object obj )
////    {
////        // ComObjects are identified by the instance of the Type object and not the TypeHandle.
////        return obj == this;
////    }
////
////    public override int GetHashCode()
////    {
#if WIN32
////        return (int)GetTypeHandleInternal().Value;
#else
////        long l = (long)GetTypeHandleInternal().Value;
////        return unchecked( (int)l );
#endif
////    }
    
        public override String ToString()
        {
            return this.Name;
////        return Cache.GetToString();
        }
        #endregion

        #region ICloneable
////    public Object Clone()
////    {
////        return this;
////    }
        #endregion

        #region ISerializable
////    public void GetObjectData( SerializationInfo info, StreamingContext context )
////    {
////        if(info == null)
////        {
////            throw new ArgumentNullException( "info" );
////        }
////
////        UnitySerializationHolder.GetUnitySerializationInfo( info, this );
////    }
        #endregion

        #region ICustomAttributeProvider
        [MethodImpl( MethodImplOptions.InternalCall )]
        public override extern Object[] GetCustomAttributes( bool inherit );
////    {
////        return CustomAttribute.GetCustomAttributes( this, typeof( object ) as RuntimeType, inherit );
////    }
    
        [MethodImpl( MethodImplOptions.InternalCall )]
        public override extern Object[] GetCustomAttributes( Type attributeType, bool inherit );
////    {
////        if(attributeType == null)
////        {
////            throw new ArgumentNullException( "attributeType" );
////        }
////
////        RuntimeType attributeRuntimeType = attributeType.UnderlyingSystemType as RuntimeType;
////
////        if(attributeRuntimeType == null)
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Arg_MustBeType" ), "attributeType" );
////        }
////
////        return CustomAttribute.GetCustomAttributes( this, attributeRuntimeType, inherit );
////    }
    
        [MethodImpl( MethodImplOptions.InternalCall )]
        public override extern bool IsDefined( Type attributeType, bool inherit );
////    {
////        if(attributeType == null)
////        {
////            throw new ArgumentNullException( "attributeType" );
////        }
////
////        RuntimeType attributeRuntimeType = attributeType.UnderlyingSystemType as RuntimeType;
////
////        if(attributeRuntimeType == null)
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Arg_MustBeType" ), "attributeType" );
////        }
////
////        return CustomAttribute.IsDefined( this, attributeRuntimeType, inherit );
////    }
        #endregion

        #region MemberInfo Overrides
        public override extern String Name
        {
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
////        {
////            return Cache.GetName();
////        }
        }
    
////    public override MemberTypes MemberType
////    {
////        get
////        {
////            if(this.IsPublic || this.IsNotPublic)
////            {
////                return MemberTypes.TypeInfo;
////            }
////            else
////            {
////                return MemberTypes.NestedType;
////            }
////        }
////    }
    
        public override extern Type DeclaringType
        {
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
////        {
////            return Cache.GetEnclosingType();
////        }
        }
    
////    public override Type ReflectedType
////    {
////        get
////        {
////            return DeclaringType;
////        }
////    }
////
////    public override int MetadataToken
////    {
////        get
////        {
////            return m_handle.GetToken();
////        }
////    }
        #endregion

        #region Legacy Internal
////    internal void CreateInstanceCheckThis()
////    {
////        if(this is ReflectionOnlyType)
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Arg_ReflectionOnlyInvoke" ) );
////        }
////
////        if(ContainsGenericParameters)
////        {
////            throw new ArgumentException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "Acc_CreateGenericEx" ), this ) );
////        }
////
////        Type elementType = this.GetRootElementType();
////
////        if(elementType == typeof( ArgIterator ))
////        {
////            throw new NotSupportedException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "Acc_CreateArgIterator" ) ) );
////        }
////
////        if(elementType == typeof( void ))
////        {
////            throw new NotSupportedException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "Acc_CreateVoid" ) ) );
////        }
////    }
////
////    internal Object CreateInstanceImpl( BindingFlags bindingAttr, Binder binder, Object[] args, CultureInfo culture, Object[] activationAttributes )
////    {
////        CreateInstanceCheckThis();
////
////        Object server = null;
////
////        try
////        {
////            try
////            {
////                // Store the activation attributes in thread local storage.
////                // These attributes are later picked up by specialized
////                // activation services like remote activation services to
////                // influence the activation.
////                if(null != activationAttributes)
////                {
////                    ActivationServices.PushActivationAttributes( this, activationAttributes );
////                }
////
////                if(args == null)
////                {
////                    args = new Object[0];
////                }
////
////                int argCnt = args.Length;
////
////                // Without a binder we need to do use the default binder...
////                if(binder == null)
////                {
////                    binder = DefaultBinder;
////                }
////
////                // deal with the __COMObject case first. It is very special because from a reflection point of view it has no ctors
////                // so a call to GetMemberCons would fail
////                if(argCnt == 0 && (bindingAttr & BindingFlags.Public) != 0 && (bindingAttr & BindingFlags.Instance) != 0
////                    && (IsGenericCOMObjectImpl() || IsSubclassOf( typeof( ValueType ) )))
////                {
////                    server = CreateInstanceImpl( ((bindingAttr & BindingFlags.NonPublic) != 0) ? false : true );
////                }
////                else
////                {
////                    MethodBase[] candidates = GetConstructors( bindingAttr );
////                    ArrayList    matches    = new ArrayList( candidates.Length );
////                    Type[]       argsType   = new Type[argCnt];
////                    for(int i = 0; i < argCnt; i++)
////                    {
////                        if(args[i] != null)
////                        {
////                            argsType[i] = args[i].GetType();
////                        }
////                    }
////
////
////                    for(int i = 0; i < candidates.Length; i++)
////                    {
////                        MethodBase canidate = candidates[i];
////
////                        if(FilterApplyMethodBaseInfo( candidates[i], bindingAttr, null, CallingConventions.Any, argsType, false ))
////                        {
////                            matches.Add( candidates[i] );
////                        }
////                    }
////
////                    MethodBase[] cons = new MethodBase[matches.Count];
////                    matches.CopyTo( cons );
////                    if(cons != null && cons.Length == 0)
////                    {
////                        cons = null;
////                    }
////
////                    if(cons == null)
////                    {
////                        // Null out activation attributes before throwing exception
////                        if(null != activationAttributes)
////                        {
////                            ActivationServices.PopActivationAttributes( this );
////                            activationAttributes = null;
////                        }
////
////                        throw new MissingMethodException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "MissingConstructor_Name" ), FullName ) );
////                    }
////
////                    // It would be strange to have an argCnt of 0 and more than
////                    //  one constructor.
////                    if(argCnt == 0 && cons.Length == 1 && (bindingAttr & BindingFlags.OptionalParamBinding) == 0)
////                    {
////                        server = Activator.CreateInstance( this, true );
////                    }
////                    else
////                    {
////                        //      MethodBase invokeMethod = binder.BindToMethod(cons,args,null,null,culture);
////                        MethodBase invokeMethod;
////                        Object     state = null;
////
////                        try
////                        {
////                            invokeMethod = binder.BindToMethod( bindingAttr, cons, ref args, null, culture, null, out state );
////                        }
////                        catch(MissingMethodException)
////                        {
////                            invokeMethod = null;
////                        }
////
////                        if(invokeMethod == null)
////                        {
////                            // Null out activation attributes before throwing exception
////                            if(null != activationAttributes)
////                            {
////                                ActivationServices.PopActivationAttributes( this );
////                                activationAttributes = null;
////                            }
////                            throw new MissingMethodException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "MissingConstructor_Name" ), FullName ) );
////                        }
////
////                        // If we're creating a delegate, we're about to call a
////                        // constructor taking an integer to represent a target
////                        // method. Since this is very difficult (and expensive)
////                        // to verify, we're just going to demand UnmanagedCode
////                        // permission before allowing this. Partially trusted
////                        // clients can instead use Delegate.CreateDelegate,
////                        // which allows specification of the target method via
////                        // name or MethodInfo.
////                        //if (isDelegate)
////                        if(typeof( Delegate ).IsAssignableFrom( invokeMethod.DeclaringType ))
////                        {
////                            new SecurityPermission( SecurityPermissionFlag.UnmanagedCode ).Demand();
////                        }
////
////                        server = ((ConstructorInfo)invokeMethod).Invoke( bindingAttr, binder, args, culture );
////                        if(state != null)
////                        {
////                            binder.ReorderArgumentArray( ref args, state );
////                        }
////                    }
////                }
////            }
////            finally
////            {
////                // Reset the TLS to null
////                if(null != activationAttributes)
////                {
////                    ActivationServices.PopActivationAttributes( this );
////                    activationAttributes = null;
////                }
////            }
////        }
////        catch(Exception)
////        {
////            throw;
////        }
////
////        //Console.WriteLine(server);
////        return server;
////    }
////
////    // the cache entry
////    class ActivatorCacheEntry
////    {
////        // the type to cache
////        internal Type                m_type;
////        // the delegate containing the call to the ctor, will be replaced by an IntPtr to feed a calli with
////        internal CtorDelegate        m_ctor;
////        internal RuntimeMethodHandle m_hCtorMethodHandle;
////        // Is a security check needed before this constructor is invoked?
////        internal bool                m_bNeedSecurityCheck;
////        // Lazy initialization was performed
////        internal bool                m_bFullyInitialized;
////
////        internal ActivatorCacheEntry( Type t, RuntimeMethodHandle rmh, bool bNeedSecurityCheck )
////        {
////            m_type               = t;
////            m_bNeedSecurityCheck = bNeedSecurityCheck;
////            m_hCtorMethodHandle  = rmh;
////        }
////    }
////
////    //ActivatorCache
////    class ActivatorCache
////    {
////        const int CACHE_SIZE = 16;
////
////        int                   hash_counter; //Counter for wrap around
////        ActivatorCacheEntry[] cache = new ActivatorCacheEntry[CACHE_SIZE];
////
////        ConstructorInfo       delegateCtorInfo;
////        PermissionSet         delegateCreatePermissions;
////
////        private void InitializeDelegateCreator()
////        {
////            // No synchronization needed here. In the worst case we create extra garbage
////            PermissionSet ps = new PermissionSet( PermissionState.None );
////
////            ps.AddPermission( new ReflectionPermission( ReflectionPermissionFlag.MemberAccess  ) );
////            ps.AddPermission( new SecurityPermission  ( SecurityPermissionFlag  .UnmanagedCode ) );
////
////            System.Threading.Thread.MemoryBarrier();
////
////            delegateCreatePermissions = ps;
////
////            ConstructorInfo ctorInfo = typeof( CtorDelegate ).GetConstructor( new Type[] { typeof( Object ), typeof( IntPtr ) } );
////
////            System.Threading.Thread.MemoryBarrier();
////            delegateCtorInfo = ctorInfo; // this assignment should be last
////        }
////
////        private void InitializeCacheEntry( ActivatorCacheEntry ace )
////        {
////            if(!ace.m_type.IsValueType)
////            {
////                BCLDebug.Assert( !ace.m_hCtorMethodHandle.Equals( RuntimeMethodHandle.EmptyHandle ), "Expected the default ctor method handle for a reference type." );
////
////                if(delegateCtorInfo == null)
////                {
////                    InitializeDelegateCreator();
////                }
////
////                delegateCreatePermissions.Assert();
////
////                // No synchronization needed here. In the worst case we create extra garbage
////                CtorDelegate ctor = (CtorDelegate)delegateCtorInfo.Invoke( new Object[] { null, ace.m_hCtorMethodHandle.GetFunctionPointer() } );
////
////                System.Threading.Thread.MemoryBarrier();
////                ace.m_ctor = ctor;
////            }
////
////            ace.m_bFullyInitialized = true;
////        }
////
////        internal ActivatorCacheEntry GetEntry( Type t )
////        {
////            int index = hash_counter;
////            for(int i = 0; i < CACHE_SIZE; i++)
////            {
////                ActivatorCacheEntry ace = cache[index];
////                if(ace != null && (Object)ace.m_type == (Object)t) //check for type match..
////                {
////                    if(!ace.m_bFullyInitDialized)
////                    {
////                        InitializeCacheEntry( ace );
////                    }
////                    return ace;
////                }
////                index = (index + 1) & (ActivatorCache.CACHE_SIZE - 1);
////            }
////            return null;
////        }
////
////        internal void SetEntry( ActivatorCacheEntry ace )
////        {
////            // fill the the array backwards to hit the most recently filled entries first in GetEntry
////            int index = (hash_counter - 1) & (ActivatorCache.CACHE_SIZE - 1);
////
////            hash_counter = index;
////            cache[index] = ace;
////        }
////    }
////
////    static ActivatorCache s_ActivatorCache;
////
////    // the slow path of CreateInstanceImpl
////    private Object CreateInstanceSlow( bool publicOnly, bool fillCache )
////    {
////        RuntimeMethodHandle runtime_ctor       = RuntimeMethodHandle.EmptyHandle;
////        bool                bNeedSecurityCheck = true;
////        bool                bCanBeCached       = false;
////        bool                bSecurityCheckOff  = false;
////
////        CreateInstanceCheckThis();
////
////        if(!fillCache)
////        {
////            bSecurityCheckOff = true;
////        }
////
////        Object instance = RuntimeTypeHandle.CreateInstance( this, publicOnly, bSecurityCheckOff, ref bCanBeCached, ref runtime_ctor, ref bNeedSecurityCheck );
////
////        if(bCanBeCached && fillCache)
////        {
////            ActivatorCache activatorCache = s_ActivatorCache;
////            if(activatorCache == null)
////            {
////                // No synchronization needed here. In the worst case we create extra garbage
////                activatorCache = new ActivatorCache();
////
////                System.Threading.Thread.MemoryBarrier();
////                s_ActivatorCache = activatorCache;
////            }
////
////            // cache the ctor
////            ActivatorCacheEntry ace = new ActivatorCacheEntry( this, runtime_ctor, bNeedSecurityCheck );
////
////            System.Threading.Thread.MemoryBarrier();
////            activatorCache.SetEntry( ace );
////        }
////        return instance;
////    }
////
////    [DebuggerStepThroughAttribute]
////    [Diagnostics.DebuggerHidden]
////    internal Object CreateInstanceImpl( bool publicOnly )
////    {
////        return CreateInstanceImpl( publicOnly, false, true );
////    }
////
////    [DebuggerStepThroughAttribute]
////    [Diagnostics.DebuggerHidden]
////    internal Object CreateInstanceImpl( bool publicOnly, bool skipVisibilityChecks, bool fillCache )
////    {
////        // next line will throw for ReflectionOnly types
////        RuntimeTypeHandle typeHandle = TypeHandle;
////
////        ActivatorCache activatorCache = s_ActivatorCache;
////        if(activatorCache != null)
////        {
////            ActivatorCacheEntry ace = activatorCache.GetEntry( this );
////            if(ace != null)
////            {
////                if(publicOnly)
////                {
////                    if(ace.m_ctor != null && (ace.m_hCtorMethodHandle.GetAttributes() & MethodAttributes.MemberAccessMask) != MethodAttributes.Public)
////                    {
////                        throw new MissingMethodException( Environment.GetResourceString( "Arg_NoDefCTor" ) );
////                    }
////                }
////
////                // Allocate empty object
////                Object instance = typeHandle.Allocate();
////                if(ace.m_ctor != null)
////                {
////                    // Perform security checks if needed
////                    if(!skipVisibilityChecks && ace.m_bNeedSecurityCheck)
////                    {
////                        MethodBase.PerformSecurityCheck( instance, ace.m_hCtorMethodHandle, TypeHandle.Value, INVOCATION_FLAGS_CONSTRUCTOR_INVOKE );
////                    }
////
////                    // Call ctor (value types wont have any)
////                    try
////                    {
////                        ace.m_ctor( instance );
////                    }
////                    catch(Exception e)
////                    {
////                        throw new TargetInvocationException( e );
////                    }
////                }
////
////                return instance;
////            }
////        }
////
////        return CreateInstanceSlow( publicOnly, fillCache );
////    }
////
////    //End
////
////    internal bool SupportsInterface( Object o )
////    {
////        return TypeHandle.SupportsInterface( o );
////    }
////
////    internal void InvalidateCachedNestedType()
////    {
////        Cache.InvalidateCachedNestedType();
////    }
////
////    internal bool IsGenericCOMObjectImpl()
////    {
////        return m_handle.IsComObject( true );
////    }
        #endregion

        #region Legacy Static Internal
////    internal static bool CanCastTo( RuntimeType fromType, RuntimeType toType )
////    {
////        return fromType.GetTypeHandleInternal().CanCastTo( toType.GetTypeHandleInternal() );
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private static extern Object _CreateEnum( IntPtr enumType, long value );
////
////    internal static Object CreateEnum( RuntimeTypeHandle enumType, long value )
////    {
////        return _CreateEnum( enumType.Value, value );
////    }
////
////    internal static Type PrivateGetType( String typeName, bool throwOnError, bool ignoreCase, ref StackCrawlMark stackMark )
////    {
////        return PrivateGetType( typeName, throwOnError, ignoreCase, false, ref stackMark );
////    }
////
////    internal static Type PrivateGetType( String typeName, bool throwOnError, bool ignoreCase, bool reflectionOnly, ref StackCrawlMark stackMark )
////    {
////        unsafe
////        {
////            if(typeName == null)
////            {
////                throw new ArgumentNullException( "TypeName" );
////            }
////
////            return RuntimeTypeHandle.GetTypeByName( typeName, throwOnError, ignoreCase, reflectionOnly, ref stackMark ).GetRuntimeType();
////        }
////    }

        #endregion
    }
}
