using System;
using System.Diagnostics.CodeAnalysis;
using Llvm.NET.Types;
using Llvm.NET.Values;

namespace Llvm.NET.DebugInfo
{
    /// <summary>Provides pairing of a <see cref="ITypeRef"/> with a <see cref="DIType"/> for function signatures</summary>
    /// <remarks>
    /// <para>Primitve types and function signature types are all uniqued in LLVM, thus there won't be a
    /// strict one to one relationship between an LLVM type and corresponding language specific debug
    /// type. (e.g. unsigned char, char, byte and signed byte might all be 8 bit integer values as far
    /// as LLVM is concerened. Also, when using the pointer+alloca+memcpy pattern to pass by value the
    /// actual source debug info type is different than the LLVM function signature. This class is used
    /// to construct native type andd ebug info pairing to allow applications to maintain a link from 
    /// their AST or IR types into the LLVM native type and debug information.
    /// </para>
    /// <note type="note">
    /// It is important to note that the realtionship between the <see cref="DIType"/> to it's <see cref="NativeType"/>
    /// properties is strictly one way. That is, there is no way to take an arbitrary <see cref="ITypeRef"/> and re-associate
    /// it with the DIType or an inplementation of this interface as there may be many such mappings to choose from. 
    /// </note>
    /// </remarks>
    public interface IDebugType<out NativeT, out DebugT>
        : ITypeRef
        where NativeT : ITypeRef
        where DebugT : DIType
    {
        /// <summary>LLVM NativeType this interface is associating with debug info in <see cref="DIType"/></summary>
        NativeT NativeType { get; }

        /// <summary>Debug information type this interface is associating with <see cref="NativeType"/></summary>
        DebugT DIType { get; }
    }

    public class DebugType< NativeT, DebugT>
        : IDebugType< NativeT, DebugT>
        , ITypeRef
        where NativeT : ITypeRef
        where DebugT : DIType
    {
        internal DebugType( NativeT llvmType, DebugT diType )
        {
            NativeType = llvmType;
            DIType = diType;
        }

        [SuppressMessage( "Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "DIType" )]
        public DebugT DIType
        {
            get { return DIType_; }
            set
            {
                if( DIType_ == null )
                {
                    DIType_ = value;
                }
                else if( DIType_.IsTemporary )
                {
                    if( value.IsTemporary )
                        throw new InvalidOperationException( "Cannot replace a temporary with another temporary" );

                    DIType_.ReplaceAllUsesWith( value );
                    DIType_ = value;
                }
                else
                    throw new InvalidOperationException( "Cannot replace non temporary DIType with a new Type" );
            }
        }
        private DebugT DIType_;

        public NativeT NativeType { get; }

        public IntPtr TypeHandle => NativeType.TypeHandle;

        public bool IsSized => NativeType.IsSized;

        public TypeKind Kind => NativeType.Kind;

        public Context Context => NativeType.Context;

        public uint IntegerBitWidth => NativeType.IntegerBitWidth;

        public bool IsInteger => NativeType.IsInteger;

        public bool IsFloat => NativeType.IsFloat;

        public bool IsDouble => NativeType.IsDouble;

        public bool IsVoid => NativeType.IsVoid;

        public bool IsStruct => NativeType.IsStruct;

        public bool IsPointer => NativeType.IsPointer;

        public bool IsSequence => NativeType.IsSequence;

        public bool IsFloatingPoint => NativeType.IsFloatingPoint;

        public bool IsPointerPointer => NativeType.IsPointerPointer;

        public Constant GetNullValue( ) => NativeType.GetNullValue( );

        public IArrayType CreateArrayType( uint count ) => NativeType.CreateArrayType( count );

        public IPointerType CreatePointerType( ) => NativeType.CreatePointerType( );

        public IPointerType CreatePointerType( uint addressSpace ) => NativeType.CreatePointerType( addressSpace );

        public DebugPointerType CreatePointerType( NativeModule module, uint addressSpace )
        {
            if( DIType == null )
                throw new ArgumentException( "Type does not have associated Debug type from which to construct a pointer type" );

            var nativePointer = NativeType.CreatePointerType( addressSpace );
            return new DebugPointerType( nativePointer, module, DIType, string.Empty );
        }

        public DebugArrayType CreateArrayType( NativeModule module, uint lowerBound, uint count )
        {
            if( DIType == null )
                throw new ArgumentException( "Type does not have associated Debug type from which to construct an array type" );

            var llvmArray = NativeType.CreateArrayType( count );
            return new DebugArrayType( llvmArray, module, DIType, count, lowerBound );
        }

        public bool TryGetExtendedPropertyValue<PropT>( string id, out PropT value )
        {
            if( PropertyContainer.TryGetExtendedPropertyValue( id, out value ) )
                return true;

            return NativeType.TryGetExtendedPropertyValue( id, out value );
        }

        public void AddExtendedPropertyValue( string id, object value )
        {
            PropertyContainer.AddExtendedPropertyValue( id, value );
        }

        [SuppressMessage( "Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates", Justification = "Available as a property, this is for convenience" )]
        public static implicit operator DebugT( DebugType<NativeT, DebugT> self ) => self.DIType;

        private readonly ExtensiblePropertyContainer PropertyContainer = new ExtensiblePropertyContainer( );
    }

    public static class DebugType
    {
        /// <summary>Creates a new <see cref="DebugType"/>instance inferring the generic arguments from the parameters</summary>
        /// <typeparam name="NativeT">Type of the Native LLVM type for the association</typeparam>
        /// <typeparam name="DebugT">Type of the debug information type for the assocaion</typeparam>
        /// <param name="nativeType"><typeparamref name="NativeT"/> type instance for this association</param>
        /// <param name="debugType"><typeparamref name="DebugT"/> type instance for this association</param>
        /// <returns><see cref="IDebugType{NativeT, DebugT}"/> implementation for the specified association</returns>
        public static IDebugType<NativeT, DebugT> Create<NativeT, DebugT>( NativeT nativeType
                                                                         , DebugT debugType
                                                                         )
            where NativeT : ITypeRef
            where DebugT : DIType
        {
            return new DebugType<NativeT, DebugT>( nativeType, debugType );
        }

        /// <summary>Convenience extensions for determining if the <see cref="DIType"/> property is valid</summary>
        /// <param name="debugType"></param>
        /// <remarks>In LLVM Debug information a <see langword="null"/> <see cref="Llvm.NET.DebugInfo.DIType"/> is
        /// used to represent the void type. Thus, looking only at the <see cref="DIType"/> property is
        /// insufficient to distinguish between a type with no debug information and one representing the void 
        /// type. This property is used to disambiguate the two possibilities. 
        /// </remarks>
        public static bool HasDebugInfo( this IDebugType<ITypeRef,DIType> debugType )
        {
            return debugType.DIType != null || debugType.NativeType.IsVoid;
        }
    }
}
