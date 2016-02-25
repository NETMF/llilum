//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

// Enable this macro to shift object pointers from the beginning of the header to the beginning of the payload.
#define CANONICAL_OBJECT_POINTERS

using System.Collections.ObjectModel;
using Microsoft.Zelig.Debugging;

namespace Microsoft.Zelig.LLVM
{
    using System;
    using System.Diagnostics;
    using System.Collections.Generic;
    using IR = Microsoft.Zelig.CodeGeneration.IR;
    using TS = Microsoft.Zelig.Runtime.TypeSystem;
    using System.Linq;

    public partial class LLVMModuleManager
    {
        public void ConvertTypeLayoutsToLLVM( )
        {
            // LT72NOTE: are we thread safe??
            if( !m_typeSystemAlreadyConverted )
            {
                foreach( TS.TypeRepresentation type in m_typeSystem.Types )
                {
                    GetOrInsertType( type );
                }

                m_typeSystemAlreadyConverted = true;
            }
        }

        public LLVM._Type GetOrInsertInlineType( TS.TypeRepresentation tr )
        {
            _Type type = GetOrInsertType( tr );
            if( type.IsPointer &&
                ( type.UnderlyingType != null ) &&
                !type.UnderlyingType.IsValueType )
            {
                type = type.UnderlyingType;
            }

            return type;
        }

        public LLVM._Type GetOrInsertType( TS.TypeRepresentation tr )
        {
            TS.WellKnownFields wkf = m_typeSystem.WellKnownFields;
            TS.WellKnownTypes wkt = m_typeSystem.WellKnownTypes;
            
            //
            // Open and Delayed types do not participate in layout
            //
            if( tr.IsOpenType || tr.IsDelayedType )
            {
                return null;
            }

            //
            // Delayed types do not participate in layout
            //
            if( tr is TS.DelayedMethodParameterTypeRepresentation ||
                tr is TS.DelayedTypeParameterTypeRepresentation )
            {
                return null;
            }

            if( tr is TS.PointerTypeRepresentation )
            {
                TS.TypeRepresentation underlying = tr.UnderlyingType;

                if( underlying is TS.DelayedMethodParameterTypeRepresentation ||
                    underlying is TS.DelayedTypeParameterTypeRepresentation )
                {
                    return null;
                }
            }

            if( tr is TS.ArrayReferenceTypeRepresentation )
            {
                TS.TypeRepresentation contained = tr.ContainedType;

                if( contained is TS.DelayedMethodParameterTypeRepresentation ||
                    contained is TS.DelayedTypeParameterTypeRepresentation )
                {
                    return null;
                }
            }

            if( !tr.ValidLayout )
            {
                // only unresolved generics get here
                return null;
            }

            if( tr is TS.PinnedPointerTypeRepresentation || tr is TS.EnumerationTypeRepresentation )
            {
                tr = tr.UnderlyingType;
            }

            // Try to get the cached type before we create a new one.
            _Type cachedType;
            if( m_typeRepresentationsToType.TryGetValue( tr, out cachedType ) )
            {
                return cachedType;
            }

            // Remap scalar basic types to native types.
            if( tr == wkt.System_Boolean ||
                tr == wkt.System_Char    ||
                tr == wkt.System_SByte   ||
                tr == wkt.System_Byte    ||
                tr == wkt.System_Int16   ||
                tr == wkt.System_UInt16  ||
                tr == wkt.System_Int32   ||
                tr == wkt.System_UInt32  ||
                tr == wkt.System_Int64   ||
                tr == wkt.System_UInt64  ||
                tr == wkt.System_Single  ||
                tr == wkt.System_Double  ||
                tr == wkt.System_IntPtr  ||
                tr == wkt.System_UIntPtr ||
                tr == wkt.System_Void )
            {
                _Type type = m_module.GetOrInsertType( tr );
                m_typeRepresentationsToType[ tr ] = type;
                return type;
            }

            string typeName = tr.FullName;

            if( tr is TS.PointerTypeRepresentation )
            {
                if( tr.UnderlyingType == wkt.System_Void )
                {
                    // Special case: we remap void* to an IntPtr to allow LLVM to function.
                    return GetOrInsertType( wkt.System_IntPtr );
                }

                _Type underlyingType = GetOrInsertType( tr.UnderlyingType );
                m_typeRepresentationsToType[ tr ] = m_module.GetOrInsertPointerType( ( TS.PointerTypeRepresentation )tr, underlyingType );
                return m_typeRepresentationsToType[ tr ];
            }

            // All other types
            _Type llvmType = m_module.GetOrInsertType( tr );

            // Ensure that we always return the correct type for storage. If this is a value type, then we're done.
            // Otherwise, return the type as a pointer.
            if( llvmType.IsValueType )
            {
                m_typeRepresentationsToType[ tr ] = llvmType;
            }
            else
            {
                m_typeRepresentationsToType[ tr ] = m_module.GetOrInsertPointerType( llvmType );
            }

#if !CANONICAL_OBJECT_POINTERS
            // Special case: System.Object always gets an ObjectHeader.
            if( tr == wkt.System_Object )
            {
                _Type headerType = GetOrInsertType( wkt.Microsoft_Zelig_Runtime_ObjectHeader );
                llvmType.AddField( 0, headerType.UnderlyingType, ".header" );
            }
#endif // !CANONICAL_OBJECT_POINTERS

            // Inline the parent class for object types. We will represent unboxed Value types as flat, c++ style, types
            // because they are 'inlined' in the object layout and their fields offset are based on such layout. We also
            // exempt ObjectHeader as a special case, since it can't inherit anything.
            if( ( tr.Extends != null ) &&
                ( tr.Extends != wkt.System_ValueType ) &&
                ( tr.Extends != wkt.System_Enum ) &&
                ( tr != wkt.Microsoft_Zelig_Runtime_ObjectHeader ) )
            {
                _Type parentType = GetOrInsertType( tr.Extends );
                llvmType.AddField( 0, parentType.UnderlyingType, ".extends" );
            }

            // Boxed types extend ValueType or Enum (ECMA-335 I.8.2.4), but should be treated as reference types.
            if( tr is TS.BoxedValueTypeRepresentation )
            {
                _Type objectType = GetOrInsertType( wkt.System_Object );
                _Type underlyingType = GetOrInsertType( tr.UnderlyingType );
                llvmType.UnderlyingType = underlyingType;
                llvmType.IsBoxed = true;

                llvmType.AddField( 0, objectType.UnderlyingType, ".extends" );
            }

            foreach( var field in tr.Fields )
            {
                // static fields are not part of object layout
                if( field is TS.StaticFieldRepresentation )
                {
                    continue;
                }

                if( !field.ValidLayout )
                {
                    continue;
                }

                // Strings are implemented inside the type
                if( field == wkf.StringImpl_FirstChar )
                {
                    _Type arrayType = m_module.GetOrInsertZeroSizedArray( GetOrInsertType( wkt.System_Char ) );
                    llvmType.AddField( ( uint )field.Offset, arrayType, field.Name );

                    continue;
                }

                llvmType.AddField( ( uint )field.Offset, GetOrInsertType( field.FieldType ), field.Name );
            }

            if( tr is TS.ArrayReferenceTypeRepresentation )
            {
                _Type arrayType = m_module.GetOrInsertZeroSizedArray( GetOrInsertType( tr.ContainedType ) );
                llvmType.AddField( wkt.System_Array.Size, arrayType, "array_data" );
            }

            llvmType.SetupFields( );

            return m_typeRepresentationsToType[ tr ];
        }

        public _Type GetOrInsertType( TS.MethodRepresentation mr )
        {
            var args = new List<_Type>( );

            foreach( var param in mr.ThisPlusArguments )
            {
                args.Add( GetOrInsertType( param ) );
            }

            if( mr is TS.StaticMethodRepresentation )
                args.RemoveAt( 0 );

            _Type retType = GetOrInsertType( mr.ReturnType );

            if( retType == null )
            {
                retType = m_module.GetVoidType( );
            }

            string sigName;
            if( mr.Flags.HasFlag( TS.MethodRepresentation.Attributes.PinvokeImpl ) )
                sigName = mr.Name;
            else
                sigName = mr.ToSignatureString( );

            return m_module.GetOrInsertFunctionType( sigName, retType, args );
        }

        public DebugInfo GetDebugInfoFor( TS.MethodRepresentation method )
        {
            if( method.DebugInfo == null )
            {
                var cfg = IR.TypeSystemForCodeTransformation.GetCodeForMethod( method );
                if( cfg == null )
                    return m_dummyDebugInfo;

                method.DebugInfo = ( from op in cfg.DataFlow_SpanningTree_Operators
                                      where op.DebugInfo != null
                                      select op.DebugInfo
                                   ).FirstOrDefault( );

                if( method.DebugInfo == null )
                    method.DebugInfo = m_dummyDebugInfo;
            }

            return method.DebugInfo;
        }
    }
}