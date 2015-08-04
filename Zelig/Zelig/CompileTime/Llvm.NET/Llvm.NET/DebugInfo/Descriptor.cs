namespace Llvm.NET.DebugInfo
{
    // for now the DebugInfo hierarchy is mostly empty
    // classes. This is due to the "in transition" state
    // of the underlying LLVM C++ model. All of these
    // are just a wrapper around a Metadata* allocated
    // in the LLVM native libraries. The only properties
    // or methods exposed are those required by current
    // projects. This keeps the code churn to move into
    // 3.7 minimal while allowing us to achieve progress
    // on current projects.

    public class TypeArray
    {
        internal TypeArray( LLVMMetadataRef handle )
        {
            MetadataHandle = handle;
        }

        internal LLVMMetadataRef MetadataHandle { get; }
    }

    public class Array
    {
        internal Array( LLVMMetadataRef handle )
        {
            MetadataHandle = handle;
        }

        internal LLVMMetadataRef MetadataHandle { get; }
    }

    public class Descriptor
    {
        internal Descriptor( LLVMMetadataRef handle )
        {
            MetadataHandle = handle;
        }

        internal LLVMMetadataRef MetadataHandle { get; }
    }

    public class Variable : Descriptor
    {
        internal Variable( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }

    public class Subrange : Descriptor
    {
        internal Subrange( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }

    public class Scope : Descriptor
    {
        internal Scope( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }

    public class CompileUnit : Scope
    {
        internal CompileUnit( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }

    public class File : Scope
    {
        internal File( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }

    public class LexicalBlock : Scope
    {
        internal LexicalBlock( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }

    public class LexicalBlockFile : Scope
    {
        internal LexicalBlockFile( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }

    public class Namespace : Scope
    {
        internal Namespace( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }

    public class SubProgram : Scope
    {
        internal SubProgram( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }

    public class Type : Scope
    {
        internal Type( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }

    public class BasicType : Type
    {
        internal BasicType( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }

    public class DerivedType : Type
    {
        internal DerivedType( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }

    public class CompositeType : Type
    {
        internal CompositeType( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }

    // signature of a SubProgram is a type
    public class SubroutineType : CompositeType
    {
        internal SubroutineType( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }
}
