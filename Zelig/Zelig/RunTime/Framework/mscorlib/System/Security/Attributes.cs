// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

namespace System.Security
{
    using System.Runtime.InteropServices;

////// DynamicSecurityMethodAttribute:
//////  Indicates that calling the target method requires space for a security
//////  object to be allocated on the callers stack. This attribute is only ever
//////  set on certain security methods defined within mscorlib.
////[AttributeUsage( AttributeTargets.Method, AllowMultiple = true, Inherited = false )]
////sealed internal class DynamicSecurityMethodAttribute : System.Attribute
////{
////}
////
////// SuppressUnmanagedCodeSecurityAttribute:
//////  Indicates that the target P/Invoke method(s) should skip the per-call
//////  security checked for unmanaged code permission.
////[AttributeUsage( AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Delegate, AllowMultiple = true, Inherited = false )]
////sealed public class SuppressUnmanagedCodeSecurityAttribute : System.Attribute
////{
////}

    // UnverifiableCodeAttribute:
    //  Indicates that the target module contains unverifiable code.
    [AttributeUsage( AttributeTargets.Module, AllowMultiple = true, Inherited = false )]
    sealed public class UnverifiableCodeAttribute : System.Attribute
    {
    }

////// AllowPartiallyTrustedCallersAttribute:
//////  Indicates that the Assembly is secure and can be used by untrusted
//////  and semitrusted clients
//////  For v.1, this is valid only on Assemblies, but could be expanded to 
//////  include Module, Method, class
////[AttributeUsage( AttributeTargets.Assembly, AllowMultiple = false, Inherited = false )]
////sealed public class AllowPartiallyTrustedCallersAttribute : System.Attribute
////{
////    public AllowPartiallyTrustedCallersAttribute() { }
////}

    public enum SecurityCriticalScope
    {
        Explicit   = 0,
        Everything = 0x1
    }

    // SecurityCriticalAttribute
    //  Indicates that the decorated code or assembly performs security critical operations (e.g. Assert, "unsafe", LinkDemand, etc.)
    //  The attribute can be placed on most targets, except on arguments/return values.
    //  The attribute applies only to the specific target and not to everything underneath it (similar to 'public' qualifier)
    //   i.e. marking an assembly SecurityCritical doesn't imply all types within the assembly are critical, 
    //      and similarly marking a type critical doesn't imply all of its members are critical
    //  For code to perform security critical actions, both the code (e.g. method, field, etc.) and the assembly must be decorated
    //      with the SecurityCriticalAttribute.

    [AttributeUsage( AttributeTargets.Assembly |
                    AttributeTargets.Module |
                    AttributeTargets.Class |
                    AttributeTargets.Struct |
                    AttributeTargets.Enum |
                    AttributeTargets.Constructor |
                    AttributeTargets.Method |
                    AttributeTargets.Property |
                    AttributeTargets.Field |
                    AttributeTargets.Event |
                    AttributeTargets.Interface |
                    AttributeTargets.Delegate,
        AllowMultiple = false,
        Inherited = false )]
    sealed public class SecurityCriticalAttribute : System.Attribute
    {
        internal SecurityCriticalScope _val;
    
        public SecurityCriticalAttribute() { }
    
        public SecurityCriticalAttribute( SecurityCriticalScope scope )
        {
            _val = scope;
        }
    
        public SecurityCriticalScope Scope
        {
            get
            {
                return _val;
            }
        }
    }

////// SecurityTreatAsSafeAttribute:
////// Indicates that the code may contain violations to the security critical rules (e.g. transitions from
//////      critical to non-public transparent, transparent to non-public critical, etc.), has been audited for
//////      security concerns and is considered security clean.
////// At assembly-scope, all rule checks will be suppressed within the assembly and for calls made against the assembly.
////// At type-scope, all rule checks will be suppressed for members within the type and for calls made against the type.
////// At member level (e.g. field and method) the code will be treated as public - i.e. no rule checks for the members.
////
////[AttributeUsage( AttributeTargets.All,
////    AllowMultiple = false,
////    Inherited = false )]
////sealed public class SecurityTreatAsSafeAttribute : System.Attribute
////{
////    public SecurityTreatAsSafeAttribute() { }
////}

    // SecuritySafeCriticalAttribute: 
    // Indicates that the code may contain violations to the security critical rules (e.g. transitions from
    //      critical to non-public transparent, transparent to non-public critical, etc.), has been audited for
    //      security concerns and is considered security clean. Also indicates that the code is considered SecurityCritical.
    // The effect of this attribute is as if the code was marked [SecurityCritical][SecurityTreatAsSafe].
    // At assembly-scope, all rule checks will be suppressed within the assembly and for calls made against the assembly.
    // At type-scope, all rule checks will be suppressed for members within the type and for calls made against the type.
    // At member level (e.g. field and method) the code will be treated as public - i.e. no rule checks for the members.

    [AttributeUsage(AttributeTargets.Class |
                    AttributeTargets.Struct |
                    AttributeTargets.Enum |
                    AttributeTargets.Constructor |
                    AttributeTargets.Method |
                    AttributeTargets.Field |
                    AttributeTargets.Interface |
                    AttributeTargets.Delegate,
        AllowMultiple = false,
        Inherited = false )]
    sealed public class SecuritySafeCriticalAttribute : System.Attribute
    {
        public SecuritySafeCriticalAttribute () { }
    }

////// SecurityTransparentAttribute:
////// Indicates the assembly contains only transparent code.
////// Security critical actions will be restricted or converted into less critical actions. For example,
////// Assert will be restricted, SuppressUnmanagedCode, LinkDemand, unsafe, and unverifiable code will be converted
////// into Full-Demands.
////
////[AttributeUsage( AttributeTargets.Assembly,
////    AllowMultiple = false,
////    Inherited = false )]
////sealed public class SecurityTransparentAttribute : System.Attribute
////{
////    public SecurityTransparentAttribute() { }
////}
}
