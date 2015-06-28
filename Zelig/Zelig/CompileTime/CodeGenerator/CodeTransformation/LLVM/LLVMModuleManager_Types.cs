//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.LLVM
{
    using System;
    using System.Diagnostics;
    using System.Collections.Generic;
    using IR = Microsoft.Zelig.CodeGeneration.IR;
    using TS = Microsoft.Zelig.Runtime.TypeSystem;

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

        //--//

        internal LLVM._Type GetObjectHeaderType( )
        {
            return GetOrInsertType( m_typeSystem.WellKnownTypes.Microsoft_Zelig_Runtime_ObjectHeader );
        }

        public LLVM._Type GetOrInsertBasicTypeAsLLVMSingleValueType( TS.TypeRepresentation tr )
        {
            if( tr is TS.EnumerationTypeRepresentation )
            {
                tr = tr.UnderlyingType;
            }

            if( tr is TS.PointerTypeRepresentation )
            {
                tr = m_typeSystem.WellKnownTypes.System_IntPtr;
            }

            if( tr is TS.ValueTypeRepresentation && !( tr is TS.ScalarTypeRepresentation ) )
            {
                tr = m_typeSystem.WellKnownTypes.System_IntPtr;
            }

            LLVM._Type llvmType = m_module.GetOrInsertType("LLVM." + tr.FullName, ( int )tr.Size * 8 );
            llvmType.SetValueTypeFlag( tr is TS.ValueTypeRepresentation );
            return llvmType;
        }

        public LLVM._Type GetOrInsertType( TS.TypeRepresentation tr )
        {
            TS.WellKnownFields wkf=m_typeSystem.WellKnownFields;
            TS.WellKnownTypes wkt=m_typeSystem.WellKnownTypes;

            //
            // delayed types do not participate in layout
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

            //--//

            if( !tr.ValidLayout )
            {
                // only unresolved generics get here
                return null;
            }

            if( tr is TS.PinnedPointerTypeRepresentation || tr is TS.EnumerationTypeRepresentation )
            {
                tr = tr.UnderlyingType;
            }

            if( !m_typeRepresentationsToType.ContainsKey( tr ) )
            {
                string typeName = tr.FullName;

                //
                // Pointer and Boxed type representation 
                //
                if( tr is TS.PointerTypeRepresentation )
                {
                    if( tr.UnderlyingType == wkt.System_Void )
                    { 
                        //Special case, we remap void * to an IntPtr
                        //to allow LLVM to function
                        return GetOrInsertType( wkt.System_IntPtr );
                    }

                    _Type ty = GetOrInsertType( tr.UnderlyingType );

                    if( ty == null ) return null;

                    m_typeRepresentationsToType[ tr ] = m_module.GetOrInsertPointerType( typeName, ty );

                    return m_typeRepresentationsToType[ tr ];
                }
                else if( tr is TS.BoxedValueTypeRepresentation )
                {
                    _Type ty = GetOrInsertType( tr.UnderlyingType );

                    if( ty == null ) return null;

                    m_typeRepresentationsToType[ tr ] = m_module.GetOrInsertBoxedType( typeName, GetOrInsertType( wkt.System_Object ), ty );

                    return m_typeRepresentationsToType[ tr ];
                }

                //
                // All other types
                //
                LLVM._Type llvmType = m_module.GetOrInsertType( typeName, ( int )tr.Size * 8 );

                m_typeRepresentationsToType[ tr ] = llvmType;


                llvmType.SetValueTypeFlag( tr is TS.ValueTypeRepresentation );

                //
                // We will represent unboxed Value types as flat, c++ style, types because they are 'inlined' 
                // in the object layout and their fields offset are based on such layout
                // Also, Object header does not extend anything.
                if( tr.Extends != null && tr.Extends != wkt.System_ValueType
                    && tr != wkt.Microsoft_Zelig_Runtime_ObjectHeader )
                {
                    llvmType.AddField( 0, GetOrInsertType( tr.Extends ), true, ".extends" );
                }

                //
                // Fields
                //

                //Special case for native pointers bodies
                if( tr.FullName == "System.IntPtr" || tr.FullName == "System.UIntPtr" )
                {
                    llvmType.AddField( 0, GetOrInsertBasicTypeAsLLVMSingleValueType( tr ), false, "m_value" );
                }
                else
                {
                    foreach( var f in tr.Fields )
                    {
                        // static fields are not part of object layout
                        if( f is TS.StaticFieldRepresentation )
                        {
                            continue;
                        }

                        if( f.ValidLayout == false )
                        {
                            continue;
                        }

                        //
                        // Strings are implemented inside the type
                        //
                        if( f == wkf.StringImpl_FirstChar )
                        {
                            llvmType.AddField( ( uint )f.Offset, m_module.GetOrInsertZeroSizedArray( GetOrInsertType( wkt.System_Char ) ), false, f.Name );

                            continue;
                        }

                        if( ( f.FieldType is TS.ScalarTypeRepresentation && f.FieldType == tr ) )
                        {
                            //To respect zelig semantics, we are allowing it to create types with
                            //circluar references for basic types. 
                            //We must catch their fields here and switch them to LLVM basics.
                            llvmType.AddField( ( uint )f.Offset, GetOrInsertBasicTypeAsLLVMSingleValueType( f.FieldType ), false, f.Name );
                        }
                        else
                        {
                            llvmType.AddField( ( uint )f.Offset, GetOrInsertType( f.FieldType ), false, f.Name );
                        }

                    }
                }

                if( tr is TS.ArrayReferenceTypeRepresentation )
                {
                    llvmType.AddField( ( uint )wkt.System_Array.Size, m_module.GetOrInsertZeroSizedArray( GetOrInsertType( tr.ContainedType ) ), false, "array_data" );

                }

                //
                // Object is a special case and gets the ObjectHeader
                //
                if( tr == wkt.System_Object )
                {
                    llvmType.AddField( 0, GetObjectHeaderType( ), true, "object_header" );
                }

                llvmType.SetValueTypeFlag( tr is TS.ValueTypeRepresentation );
                llvmType.SetupFields( );
            }

            return m_typeRepresentationsToType[ tr ];
        }

        internal LLVM._Type GetOrInsertType( TS.MethodRepresentation mr )
        {
            var args=new List<LLVM._Type>( );

            foreach( var param in mr.ThisPlusArguments )
            {
                args.Add( GetOrInsertType( param ) );
            }

            if( mr is TS.StaticMethodRepresentation ) args.RemoveAt( 0 );

            LLVM._Type retType=GetOrInsertType( mr.ReturnType );

            if( retType == null )
            {
                retType = m_module.GetVoidType( );
            }

            return m_module.GetOrInsertFunctionType( GetFullMethodName( mr ) + "_func_signature", retType, args );
        }

    }
}