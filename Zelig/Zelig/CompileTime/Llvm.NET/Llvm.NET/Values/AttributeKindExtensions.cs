using System;
using System.Diagnostics;

namespace Llvm.NET.Values
{
    /// <summary>Enumeration flags to indicate which attribute set index an attribute may apply to</summary>
    [Flags]
    public enum FunctionIndexKinds
    {
        /// <summary>Invalid attributes don't apply to any index</summary>
        None = 0,
        /// <summary>The attribute is applicable to a function</summary>
        Function = 1,
        /// <summary>The attribute is applicable to a function's return</summary>
        Return = 2,
        /// <summary>The attribute is applicable to a function's parameter</summary>
        Parameter = 4
    }

    public static class AttributeKindExtensions
    {
        public static bool RequiresIntValue( this AttributeKind kind )
        {
            Debug.Assert( kind >= AttributeKind.None && kind < AttributeKind.EndAttrKinds );
            switch( kind )
            {
            case AttributeKind.Alignment:
            case AttributeKind.StackAlignment:
            case AttributeKind.Dereferenceable:
            case AttributeKind.DereferenceableOrNull:
                return true;

            default:
                return false;
            }
        }

        public static bool CheckAttributeUsage( this AttributeKind kind, FunctionAttributeIndex index, Function function )
        {
            Debug.Assert( kind >= AttributeKind.None && kind < AttributeKind.EndAttrKinds );
            FunctionIndexKinds allowedindices = kind.GetAllowedIndexes( );
            switch( index )
            {
            case FunctionAttributeIndex.Function:
                if( !allowedindices.HasFlag( FunctionIndexKinds.Function ) )
                    return false;
                break;

            case FunctionAttributeIndex.ReturnType:
                if( !allowedindices.HasFlag( FunctionIndexKinds.Return ) )
                    return false;
                break;

            //case FunctionAttributeIndex.Parameter0:
            default:
                if(function == null)
                    throw new ArgumentNullException( nameof( function ) );

                if( !allowedindices.HasFlag( FunctionIndexKinds.Parameter ) )
                    return false;

                var paramIndex = index - FunctionAttributeIndex.Parameter0;
                if( paramIndex >= function.Parameters.Count )
                    return false;
                break;
            }
            return true;
        }

        public static void VerifyAttributeUsage( this AttributeKind kind, FunctionAttributeIndex index )
        {
            Debug.Assert( kind >= AttributeKind.None && kind < AttributeKind.EndAttrKinds );
            FunctionIndexKinds allowedindices = kind.GetAllowedIndexes( );
            switch( index )
            {
            case FunctionAttributeIndex.Function:
                if( !allowedindices.HasFlag( FunctionIndexKinds.Function ) )
                    throw new ArgumentException( "Attribute not allowed on functions", nameof( index ) );
                break;

            case FunctionAttributeIndex.ReturnType:
                if( !allowedindices.HasFlag( FunctionIndexKinds.Return ) )
                    throw new ArgumentException( "Attribute not allowed on function Return", nameof( index ) );
                break;

            //case FunctionAttributeIndex.Parameter0:
            default:
                if( !allowedindices.HasFlag( FunctionIndexKinds.Parameter ) )
                    throw new ArgumentException( "Attribute not allowed on function parameter", nameof( index ) );
                break;
            }
        }

        // To prevent native asserts or crashes - validates parameters before passing down to native code
        public static void VerifyIntAttributeUsage( this AttributeKind kind, FunctionAttributeIndex index, ulong value )
        {
            kind.VerifyAttributeUsage( index );
            kind.RangeCheckValue( value );
        }

        public static void RangeCheckValue( this AttributeKind kind, ulong value )
        {
            Debug.Assert( kind >= AttributeKind.None && kind < AttributeKind.EndAttrKinds );
            // To prevent native asserts or crashes - validate parameters before passing down to native code
            switch( kind )
            {
            case AttributeKind.Alignment:
                if( value > UInt32.MaxValue )
                    throw new ArgumentOutOfRangeException( nameof( value ), "Expected a 32 bit value for alignment" );

                break;

            case AttributeKind.StackAlignment:
                if( value > UInt32.MaxValue )
                    throw new ArgumentOutOfRangeException( nameof( value ), "Expected a 32 bit value for stack alignment" );
                break;

            case AttributeKind.Dereferenceable:
            case AttributeKind.DereferenceableOrNull:
                break;

            default:
                throw new ArgumentException( $"Attribute '{kind}' does not support an argument", nameof( kind ) );
            }
        }

        public static FunctionIndexKinds GetAllowedIndexes( this AttributeKind kind )
        {
            Debug.Assert( kind >= AttributeKind.None && kind < AttributeKind.EndAttrKinds );
            switch( kind )
            {
            default:
                return FunctionIndexKinds.None;

            case AttributeKind.ZExt:
            case AttributeKind.SExt:
            case AttributeKind.InReg:
            case AttributeKind.ByVal:
            case AttributeKind.InAlloca:
            case AttributeKind.StructRet:
            case AttributeKind.Alignment:
            case AttributeKind.NoAlias:
            case AttributeKind.NoCapture:
            case AttributeKind.Nest:
            case AttributeKind.Returned:
            case AttributeKind.NonNull:
            case AttributeKind.Dereferenceable:
            case AttributeKind.DereferenceableOrNull:
                return FunctionIndexKinds.Parameter | FunctionIndexKinds.Return;

            case AttributeKind.StackAlignment:
            case AttributeKind.AlwaysInline:
            case AttributeKind.Builtin:
            case AttributeKind.Cold:
            case AttributeKind.Convergent:
            case AttributeKind.InlineHint:
            case AttributeKind.JumpTable:
            case AttributeKind.MinSize:
            case AttributeKind.Naked:
            case AttributeKind.NoBuiltin:
            case AttributeKind.NoDuplicate:
            case AttributeKind.NoImplicitFloat:
            case AttributeKind.NoInline:
            case AttributeKind.NonLazyBind:
            case AttributeKind.NoRedZone:
            case AttributeKind.NoReturn:
            //case AttributeKind.NoRecurse: // ver > 3.7.0?
            case AttributeKind.NoUnwind:
            case AttributeKind.OptimizeForSize:
            case AttributeKind.OptimizeNone:
            case AttributeKind.ReadNone:
            case AttributeKind.ReadOnly:
            case AttributeKind.ArgMemOnly:
            case AttributeKind.ReturnsTwice:
            case AttributeKind.SafeStack:
            case AttributeKind.SanitizeAddress:
            case AttributeKind.SanitizeMemory:
            case AttributeKind.SanitizeThread:
            case AttributeKind.StackProtect:
            case AttributeKind.StackProtectReq:
            case AttributeKind.StackProtectStrong:
            case AttributeKind.UWTable:
                return FunctionIndexKinds.Function;
            }
        }
    }
}
