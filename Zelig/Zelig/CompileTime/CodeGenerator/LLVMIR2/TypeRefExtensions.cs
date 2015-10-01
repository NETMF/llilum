using Llvm.NET;
using Llvm.NET.Types;

namespace Microsoft.Zelig.LLVM
{
    public static class TypeRefExtensions
    {
        public static bool IsInteger( this TypeRef typeRef ) => typeRef.Kind == TypeKind.Integer;

        // Return true if value is 'float', a 32-bit IEEE fp type.
        public static bool IsFloat( this TypeRef typeRef ) => typeRef.Kind == TypeKind.Float32;

        // Return true if this is 'double', a 64-bit IEEE fp type
        public static bool IsDouble( this TypeRef typeRef ) => typeRef.Kind == TypeKind.Float64;

        public static bool IsVoid( this TypeRef typeRef ) => typeRef.Kind == TypeKind.Void;

        public static bool IsFloatingPoint( this TypeRef typeRef )
        {   
            switch( typeRef.Kind )
            {
            case TypeKind.Float16:
            case TypeKind.Float32:
            case TypeKind.Float64:
            case TypeKind.X86Float80:
            case TypeKind.Float128m112:
            case TypeKind.Float128:
                return true;

            default:
                return false;
            }
        }

        public static bool IsPointer( this TypeRef typeRef ) => typeRef.Kind == TypeKind.Pointer;
        public static bool IsPointerPointer( this TypeRef typeRef )
        {
            var ptrType = typeRef as PointerType;
            return ptrType !=null && ptrType.ElementType.Kind == TypeKind.Pointer;
        }

        public static bool IsStruct( this TypeRef typeRef ) => typeRef.Kind == TypeKind.Struct;
    }
}
