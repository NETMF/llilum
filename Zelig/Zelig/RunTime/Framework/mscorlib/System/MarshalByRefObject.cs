// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*============================================================
**
** File:    MarshalByRefObject.cs
**
**
**
** Purpose: Defines the root type for all marshal by reference aka
**          AppDomain bound types
**
**
**
===========================================================*/
namespace System
{
    using System;
////using System.Security;
////using System.Security.Permissions;
    using System.Threading;
////using System.Runtime.Remoting;
////using System.Runtime.Remoting.Lifetime;
////using System.Runtime.Remoting.Services;
    using System.Runtime.InteropServices;
    using System.Reflection;
    using System.Runtime.CompilerServices;
////using System.Runtime.Versioning;
    using CultureInfo = System.Globalization.CultureInfo;

    [Serializable]
    public abstract class MarshalByRefObject
    {
////    private Object m_identity;
////
////    private Object Identity
////    {
////        get
////        {
////            return m_identity;
////        }
////
////        set
////        {
////            m_identity = value;
////        }
////    }
////
////    // (1) for remote COM objects IsInstance of can't be executed on
////    // the proxies, so we need this method to be executed on the
////    // actual object.
////    // (2) for remote objects that do not have the complete type information
////    // we intercept calls to check the type and execute it on the actual
////    // object
////    internal bool IsInstanceOfType( Type T )
////    {
////        return T.IsInstanceOfType( this );
////    }
////
////    // Returns a new cloned MBR instance that is a memberwise copy of this
////    // with the identity nulled out, so there are no identity conflicts
////    // when the cloned object is marshalled
////    protected MarshalByRefObject MemberwiseClone( bool cloneIdentity )
////    {
////        MarshalByRefObject mbr = (MarshalByRefObject)base.MemberwiseClone();
////        // set the identity on the cloned object to null
////        if(!cloneIdentity)
////        {
////            mbr.Identity = null;
////        }
////
////        return mbr;
////    }
////
////    // A helper routine to extract the identity either from the marshalbyrefobject base
////    // class if it is not a proxy, otherwise from the real proxy.
////    // A flag is set to indicate whether the object passed in is a server or a proxy
////    internal static Identity GetIdentity( MarshalByRefObject obj, out bool fServer )
////    {
////        fServer = true;
////
////        Identity id = null;
////
////        if(obj != null)
////        {
////            if(!RemotingServices.IsTransparentProxy( obj ))
////            {
////                id = (Identity)obj.Identity;
////            }
////            else
////            {
////                // Toggle flag to indicate that we have a proxy
////                fServer = false;
////
////                id = RemotingServices.GetRealProxy( obj ).IdentityObject;
////            }
////        }
////
////        return id;
////    }
////
////    // Another helper that delegates to the helper above
////    internal static Identity GetIdentity( MarshalByRefObject obj )
////    {
////        BCLDebug.Assert( !RemotingServices.IsTransparentProxy( obj ), "Use this method for server objects only" );
////
////        bool fServer;
////
////        return GetIdentity( obj, out fServer );
////    }
////
////    internal ServerIdentity __RaceSetServerIdentity( ServerIdentity id )
////    {
////        if(m_identity == null)
////        {
////            // For strictly MBR types, the TP field in the identity
////            // holds the real server
////            if(!id.IsContextBound)
////            {
////                id.RaceSetTransparentProxy( this );
////            }
////
////            Interlocked.CompareExchange( ref m_identity, id, null );
////        }
////
////        return (ServerIdentity)m_identity;
////    }
////
////
////    internal void __ResetServerIdentity()
////    {
////        m_identity = null;
////    }
////
////    // This method is used return a lifetime service object which
////    // is used to control the lifetime policy to the object.
////    // For the default Lifetime service this will be an object of typoe ILease.
////    //
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure )]
////    public Object GetLifetimeService()
////    {
////        return LifetimeServices.GetLease( this );
////    }
////
////    // This method is used return lifetime service object. This method
////    // can be overridden to return a LifetimeService object with properties unique to
////    // this object.
////    // For the default Lifetime service this will be an object of type ILease.
////    //
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure )]
////    public virtual Object InitializeLifetimeService()
////    {
////        return LifetimeServices.GetLeaseInitial( this );
////    }
////
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure )]
////    public virtual ObjRef CreateObjRef( Type requestedType )
////    {
////        if(m_identity == null)
////        {
////            throw new RemotingException( Environment.GetResourceString( "Remoting_NoIdentityEntry" ) );
////        }
////
////        return new ObjRef( this, requestedType );
////    }
////
////
////    // This is for casting interop ObjRefLite's.
////    // ObjRefLite's have been deprecated. These methods are not exposed
////    // through any user APIs and would be removed in the future
////    internal bool CanCastToXmlType( String xmlTypeName, String xmlTypeNamespace )
////    {
////        Type castType = SoapServices.GetInteropTypeFromXmlType( xmlTypeName, xmlTypeNamespace );
////
////        if(castType == null)
////        {
////            String typeNamespace;
////            String assemblyName;
////
////            if(!SoapServices.DecodeXmlNamespaceForClrTypeNamespace( xmlTypeNamespace, out typeNamespace, out assemblyName ))
////            {
////                return false;
////            }
////
////            String typeName;
////
////            if((typeNamespace != null) && (typeNamespace.Length > 0))
////            {
////                typeName = typeNamespace + "." + xmlTypeName;
////            }
////            else
////            {
////                typeName = xmlTypeName;
////            }
////
////            try
////            {
////                Assembly asm = Assembly.Load( assemblyName );
////
////                castType = asm.GetType( typeName, false, false );
////            }
////            catch
////            {
////                return false;
////            }
////        }
////
////        if(castType != null)
////        {
////            return castType.IsAssignableFrom( this.GetType() );
////        }
////
////        return false;
////    } // CanCastToXmlType
////
////    // helper method for calling CanCastToXmlType
////    // ObjRefLite's have been deprecated. These methods are not exposed
////    // through any user APIs and would be removed in the future
////    internal static bool CanCastToXmlTypeHelper( Type castType, MarshalByRefObject o )
////    {
////        if(castType == null)
////        {
////            throw new ArgumentNullException( "castType" );
////        }
////
////        // MarshalByRefObject's can only be casted to MarshalByRefObject's or interfaces.
////        if(!castType.IsInterface && !castType.IsMarshalByRef)
////        {
////            return false;
////        }
////
////        // figure out xml type name
////        String xmlTypeName      = null;
////        String xmlTypeNamespace = null;
////
////        if(!SoapServices.GetXmlTypeForInteropType( castType, out xmlTypeName, out xmlTypeNamespace ))
////        {
////            // There's no registered interop type name, so just use the default.
////            xmlTypeName      = castType.Name;
////            xmlTypeNamespace = SoapServices.CodeXmlNamespaceForClrTypeNamespace( castType.Namespace, castType.Module.Assembly.GetSimpleName() );
////        }
////
////        return o.CanCastToXmlType( xmlTypeName, xmlTypeNamespace );
////    } // CanCastToXmlType
    }
} // namespace
