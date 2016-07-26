//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

// Enable this macro to shift object pointers from the beginning of the header to the beginning of the payload.
#define CANONICAL_OBJECT_POINTERS

namespace Microsoft.Zelig.LLVM
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.IO;

    using Importer = Microsoft.Zelig.MetaData.Importer;
    using Normalized = Microsoft.Zelig.MetaData.Normalized;
    using IR = Microsoft.Zelig.CodeGeneration.IR;
    using TS = Microsoft.Zelig.Runtime.TypeSystem;
    using Llvm.NET.Values;
    using System.Diagnostics;
    using Llvm.NET.DebugInfo;

    public partial class LLVMModuleManager
    {
        private          ISectionNameProvider               m_SectionNameProvider;
        private readonly Debugging.DebugInfo                m_dummyDebugInfo;
        private _Module                                     m_module;
        private readonly string                             m_imageName;
        private readonly IR.TypeSystemForCodeTransformation m_typeSystem;

        private GrowOnlyHashTable <TS.TypeRepresentation, _Type>                     m_typeRepresentationsToType;
        private GrowOnlyHashTable <TS.MethodRepresentation, Debugging.DebugInfo>     m_debugInfoForMethods;
        private GrowOnlyHashTable <IR.DataManager.DataDescriptor, Constant>          m_globalInitializedValues;
        private bool                                                                 m_typeSystemAlreadyConverted;
        private bool                                                                 m_turnOffCompilationAndValidation;

        //TypeSystem might not be completely initialized at this point.
        public LLVMModuleManager( IR.TypeSystemForCodeTransformation typeSystem, string imageName )
        {
            m_imageName = imageName;
            m_typeSystem = typeSystem;

            m_debugInfoForMethods = HashTableFactory.New<TS.MethodRepresentation, Debugging.DebugInfo>( );
            m_typeRepresentationsToType = HashTableFactory.New<TS.TypeRepresentation, _Type>( );
            m_globalInitializedValues = HashTableFactory.New<IR.DataManager.DataDescriptor, Constant>();

            m_typeSystemAlreadyConverted = false;
            m_turnOffCompilationAndValidation = false;

            m_module = new _Module( m_imageName, m_typeSystem );
            m_dummyDebugInfo = new Debugging.DebugInfo( m_imageName, 0, 0, 0, 0 );

            // Initialize to safe default value, will be changed later before the manager is used
            m_SectionNameProvider = new EmptySectionNameProvider( m_typeSystem );
        }

        public ISectionNameProvider SectionNameProvider
        {
            get
            {
                return m_SectionNameProvider;
            }
            set
            {
                m_SectionNameProvider = value;
            }
        }

        public void Compile()
        {
            CompleteMissingDataDescriptors();

            //
            // Synthesize the code for all exported methods as a simple C-style function using the name
            // of the method without full qualification
            //
            foreach(TS.MethodRepresentation md in m_typeSystem.ExportedMethods)
            {
                _Function handler = GetOrInsertFunction( md );

                m_module.CreateAlias(handler.LlvmFunction, md.Name);
            }

            if(!m_turnOffCompilationAndValidation)
            {
                TS.MethodRepresentation bootstrapResetMR = m_typeSystem.GetWellKnownMethod( "Bootstrap_Initialization" );
                _Function bootstrapReset = GetOrInsertFunction( bootstrapResetMR );

                if(m_typeSystem.PlatformAbstraction.PlatformFamily == TargetModel.Win32.InstructionSetVersion.Platform_Family__Win32 )
                {
                    m_module.CreateAlias(bootstrapReset.LlvmFunction, "LLILUM_main");
                }
                else
                {
                    m_module.CreateAlias(bootstrapReset.LlvmFunction, "main");
                }

                m_module.Compile();
            }
        }

        public void DumpToFile( string filename, OutputFormat format )
        {
            m_module.DumpToFile( filename, format );
        }

        public void TurnOffCompilationAndValidation( )
        {
            m_turnOffCompilationAndValidation = true;
        }

        public DISubProgram GetScopeFor( TS.MethodRepresentation md )
        {
            return GetOrInsertFunction(md)?.LlvmFunction?.DISubProgram;
        }

        public _Function GetOrInsertFunction( TS.MethodRepresentation md )
        {
            _Function function = m_module.GetOrInsertFunction( md );
            function.LlvmFunction.Section = m_SectionNameProvider.GetSectionNameFor( md );
            return function;
        }

        public static string GetFullMethodName( TS.MethodRepresentation method )
        {
            if( method.Flags.HasFlag( TS.MethodRepresentation.Attributes.PinvokeImpl ) )
            {
                return method.Name;
            }
            return $"{method.OwnerType.FullName}::{method.Name}#{method.Identity}";
        }

        private void CompleteMissingDataDescriptors( )
        {
            bool stillMissing = true;

            while( stillMissing )
            {
                stillMissing = false;

                foreach( var dd in m_globalInitializedValues.Keys )
                {
                    // Force creation of any missing global data descriptor by accessing its value.
                    if( m_globalInitializedValues[ dd ].IsAnUninitializedGlobal( ) )
                    {
                        stillMissing = true;
                        GlobalValueFromDataDescriptor( dd, setInitializer: true );
                        break;
                    }
                }
            }

            m_module.FinalizeGlobals();
        }

        private Constant GetUCVStruct( _Type type, bool anon, params Constant[ ] vals )
        {
            var fields = new List<Constant>( );
            foreach( var val in vals )
            {
                fields.Add( val );
            }

            return m_module.GetUCVStruct( type, fields, anon );
        }

        private object GetDirectFromArrayDescriptor( IR.DataManager.ArrayDescriptor ad, uint pos )
        {
            return ad.Values != null ? ad.Values[ pos ] : ad.Source.GetValue( pos );
        }

        private Constant GetUCVArray( IR.DataManager.ArrayDescriptor ad )
        {
            uint arrayLength = ( uint )( ( ad.Values == null ) ? ad.Source.Length : ad.Values.Length );

            var fields = new List<Constant>( );
            TS.TypeRepresentation elTR = ( ( TS.ArrayReferenceTypeRepresentation )ad.Context ).ContainedType;

            //
            // Special case for common scalar arrays.
            //
            if( ad.Values == null )
            {
                for( uint i = 0; i < arrayLength; i++ )
                {
                    object fdVal = GetDirectFromArrayDescriptor( ad, i );
                    fields.Add( GetScalarTypeUCV( elTR, fdVal ) );
                }
            }
            else
            {
                for( uint i = 0; i < arrayLength; i++ )
                {
                    object val = GetDirectFromArrayDescriptor( ad, i );

                    if( val == null )
                    {
                        fields.Add(m_module.GetNullValue(GetOrInsertType(elTR)));
                    }
                    else if( val is IR.DataManager.DataDescriptor )
                    {
                        IR.DataManager.DataDescriptor valDD = ( IR.DataManager.DataDescriptor )val;

                        if( valDD.Nesting != null )
                        {
                            fields.Add( UCVFromDataDescriptor( valDD ) );
                        }
                        else
                        {
                            fields.Add(GlobalValueFromDataDescriptor(valDD, false));
                        }
                    }
                    else if( ad.Context.ContainedType is TS.ScalarTypeRepresentation )
                    {
                        fields.Add( GetScalarTypeUCV( elTR, val ) );
                    }
                    else
                    {
                        throw TypeConsistencyErrorException.Create( "Don't know how to write {0}", val );
                    }
                }
            }

            return m_module.GetUCVArray( GetOrInsertType( elTR ), fields );
        }

        private Constant GetScalarTypeUCV( TS.TypeRepresentation tr, object value )
        {
            _Type type = GetOrInsertType( tr );
            return m_module.GetUCVScalar( type, value );
        }

        private bool TypeChangesAfterCreation( IR.DataManager.DataDescriptor dd )
        {
            return dd is IR.DataManager.ArrayDescriptor || dd.Context == m_typeSystem.WellKnownTypes.System_String;
        }

        public Constant GlobalValueFromDataDescriptor(IR.DataManager.DataDescriptor dd, bool setInitializer)
        {
            // If the type changes after creation it's a string or array type, so force initialization.
            if (TypeChangesAfterCreation(dd))
            {
                setInitializer = true;
            }

            Constant global;
            if (m_globalInitializedValues.TryGetValue(dd, out global))
            {
                // If we already have an initialized global, or an uninitialized one that we're still not initializing,
                // return the cached one. Uninitialized globals will be replaced below.
                if (!global.IsAnUninitializedGlobal() || !setInitializer)
                {
                    return global;
                }
            }

            if (setInitializer)
            {
                // Get the object header if this type needs one.
                Constant header = null;
#if CANONICAL_OBJECT_POINTERS
                if (!(dd.Context is TS.ValueTypeRepresentation))
                {
                    header = GetUCVObjectHeader(dd);
                }
#endif // CANONICAL_OBJECT_POINTERS

                _Type type = GetOrInsertInlineType(dd.Context);
                Constant value = UCVFromDataDescriptor(dd);
                string name = type.Name;
                string sectionName = m_SectionNameProvider.GetSectionNameFor(dd);

                // Rename vtable constants so they can be easily read in the output.
                // FUTURE: This should probably be set more generally at a higher level, likely in DataManager.
                if (dd.Context == m_typeSystem.WellKnownTypes.Microsoft_Zelig_Runtime_TypeSystem_VTable)
                {
                    var objDescriptor = (IR.DataManager.ObjectDescriptor)dd;
                    var vtable = objDescriptor.Source as TS.VTable;
                    string sourceName = vtable?.TypeInfo?.FullName;
                    if (sourceName != null)
                    {
                        name = sourceName + ".VTable";
                    }
                }

                global = m_module.GetGlobalFromUCV(type, header, value, !dd.IsMutable, name, sectionName);
            }
            else
            {
                global = m_module.GetUninitializedGlobal(GetOrInsertInlineType(dd.Context));
            }

            // If we had an uninitialized placeholder, replace it with the new copy.
            Constant cachedGlobal;
            if (m_globalInitializedValues.TryGetValue(dd, out cachedGlobal))
            {
                cachedGlobal.MergeToAndRemove(global);
            }

            m_globalInitializedValues[dd] = global;
            return global;
        }

        private Constant GetUCVObjectHeader( IR.DataManager.DataDescriptor dd )
        {
            TS.WellKnownFields wkf = m_typeSystem.WellKnownFields;
            TS.WellKnownTypes wkt = m_typeSystem.WellKnownTypes;

            var fields = new List<Constant>( );

            Runtime.ObjectHeader.GarbageCollectorFlags flags;

            if( dd.IsMutable )
            {
                flags = Runtime.ObjectHeader.GarbageCollectorFlags.UnreclaimableObject;
            }
            else
            {
                flags = Runtime.ObjectHeader.GarbageCollectorFlags.ReadOnlyObject;
            }

            var descriptor = (IR.DataManager.DataDescriptor)dd.Owner.GetObjectDescriptor(dd.Context.VirtualTable);
            Constant virtualTable = GlobalValueFromDataDescriptor(descriptor, false);
            fields.Add(GetScalarTypeUCV(wkf.ObjectHeader_MultiUseWord.FieldType, (ulong)flags));
            fields.Add(virtualTable);

            _Type headerType = GetOrInsertType( wkt.Microsoft_Zelig_Runtime_ObjectHeader );
            return m_module.GetUCVStruct( headerType.UnderlyingType, fields, false );
        }

        private Constant UCVFromDataDescriptor(IR.DataManager.DataDescriptor dd)
        {
            return UCVFromDataDescriptor(dd, dd.Context);
        }

        private Constant UCVFromDataDescriptor(
            IR.DataManager.DataDescriptor dd,
            TS.TypeRepresentation currentType)
        {
            TS.WellKnownFields wkf = m_typeSystem.WellKnownFields;
            TS.WellKnownTypes wkt = m_typeSystem.WellKnownTypes;
            TS.WellKnownMethods wkm = m_typeSystem.WellKnownMethods;

            if( dd is IR.DataManager.ObjectDescriptor )
            {
                IR.DataManager.ObjectDescriptor od = ( IR.DataManager.ObjectDescriptor )dd;

                var fields = new List<Constant>( );

#if CANONICAL_OBJECT_POINTERS
                // Recursively add parent class fields for reference types.
                if( !( currentType is TS.ValueTypeRepresentation ) && ( currentType != wkt.System_Object ) )
#else // CANONICAL_OBJECT_POINTERS
                // Special case: System.Object always gets an object header.
                if( currentType == wkt.System_Object )
                {
                    Constant ucv = GetUCVObjectHeader( dd );
                    _Type objectType = GetOrInsertType( wkt.System_Object );
                    return GetUCVStruct( objectType.UnderlyingType, false, ucv );
                }

                if( !( currentType is TS.ValueTypeRepresentation ) )
#endif // CANONICAL_OBJECT_POINTERS
                {
                    fields.Add( UCVFromDataDescriptor( dd, currentType.Extends ) );
                }

                for( uint i = 0; i < currentType.Size - ( currentType.Extends == null ? 0 : currentType.Extends.Size ); i++ )
                {
                    var fd = currentType.FindFieldAtOffset( ( int )( i + ( currentType.Extends == null ? 0 : currentType.Extends.Size ) ) );

                    if( fd != null )
                    {
                        i += fd.FieldType.SizeOfHoldingVariable - 1;

                        if( !od.Values.ContainsKey( fd ) )
                        {
                            fields.Add(m_module.GetNullValue(GetOrInsertType(fd.FieldType)));
                            continue;
                        }

                        var fdVal = od.Values[ fd ];

                        if( fd == wkf.CodePointer_Target )
                        {
                            //
                            // Special case for code pointers: substitute with actual code pointers.
                            //
                            IntPtr id = ( IntPtr )fdVal;

                            object ptr = od.Owner.GetCodePointerFromUniqueID( id );

                            if( ptr is TS.MethodRepresentation )
                            {
                                TS.MethodRepresentation md = ( TS.MethodRepresentation )ptr;
                                Constant ucv = GetOrInsertFunction(md).LlvmFunction;
                                ucv = GetUCVStruct( GetOrInsertType( fd.FieldType ), false, ucv );
                                fields.Add( ucv );
                            }
                            else if( ptr is IR.ExceptionHandlerBasicBlock )
                            {
                                IR.ExceptionHandlerBasicBlock ehBB = ( IR.ExceptionHandlerBasicBlock )ptr;

                                //
                                // temporary place-holder
                                //
                                Constant ucv = GetOrInsertFunction(wkm.TypeSystemManager_Throw).LlvmFunction;
                                ucv = GetUCVStruct( GetOrInsertType( fd.FieldType ), false, ucv );
                                fields.Add( ucv );

                            }
                            else
                            {
                                throw new Exception( "I'm not sure what kind of code pointer is this:" + ptr );
                            }
                        }
                        else if( fdVal == null )
                        {
                            fields.Add(m_module.GetNullValue(GetOrInsertType(fd.FieldType)));
                        }
                        else if( fdVal is IR.DataManager.DataDescriptor )
                        {
                            IR.DataManager.DataDescriptor valDD = ( IR.DataManager.DataDescriptor )fdVal;

                            if( valDD.Nesting != null )
                            {
                                fields.Add( UCVFromDataDescriptor( valDD ) );
                            }
                            else
                            {
                                fields.Add(GlobalValueFromDataDescriptor(valDD, false));
                            }
                        }
                        else if( fd.FieldType is TS.ScalarTypeRepresentation )
                        {
                            fields.Add( GetScalarTypeUCV( fd.FieldType, fdVal ) );
                        }
                        else
                        {
                            throw TypeConsistencyErrorException.Create( "Don't know how to write {0}", fdVal );
                        }
                    }
                    else
                    {
                        fields.Add( GetScalarTypeUCV( wkt.System_Byte, 0xAB ) );
                    }
                }

                if( currentType == wkt.System_String )
                {
                    var chars = new List<Constant>( );
                    foreach( char c in ( ( string )od.Source ).ToCharArray( ) )
                    {
                        chars.Add( GetScalarTypeUCV( wkf.StringImpl_FirstChar.FieldType, c ) );
                    }

                    fields.Add( m_module.GetUCVArray( GetOrInsertType( wkf.StringImpl_FirstChar.FieldType ), chars ) );
                }

                return m_module.GetUCVStruct( GetOrInsertInlineType( currentType ), fields, currentType == wkt.System_String );
            }
            else if( dd is IR.DataManager.ArrayDescriptor )
            {
                IR.DataManager.ArrayDescriptor ad = ( IR.DataManager.ArrayDescriptor )dd;
                int length = ad.Source?.Length ?? ad.Length;

#if CANONICAL_OBJECT_POINTERS
                Constant obj = GetUCVStruct( GetOrInsertInlineType( wkt.System_Object ), false );
#else // CANONICAL_OBJECT_POINTERS
                Constant header = GetUCVObjectHeader( dd );
                Constant obj = GetUCVStruct( GetOrInsertInlineType( wkt.System_Object ), false, header );
#endif // CANONICAL_OBJECT_POINTERS
                Constant arrayLength = GetScalarTypeUCV( wkf.ArrayImpl_m_numElements.FieldType, length );
                Constant arrayFields = GetUCVStruct( GetOrInsertInlineType( wkt.System_Array ), false, obj, arrayLength );
                return GetUCVStruct( GetOrInsertInlineType( dd.Context ), true, arrayFields, GetUCVArray( ad ) );
            }
            else
            {
                throw new System.InvalidOperationException( "DataDescriptor type not supported:" + dd );
            }
        }

        public _Module Module
        {
            get
            {
                return m_module;
            }
        }

        public GrowOnlyHashTable<TS.MethodRepresentation, Debugging.DebugInfo> DebugInfoForMethods
        {
            get
            {
                return m_debugInfoForMethods;
            }
        }
    }
}
