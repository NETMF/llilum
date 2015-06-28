//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

#define READER__USE_EMIT
//#define DEBUG_PERSISTENCE
//#define DEBUG_PERSISTENCE__COUNT_INSTANCES


namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

#if READER__USE_EMIT
    using System.Reflection.Emit;
#endif

    using Microsoft.Zelig.Runtime.TypeSystem;
    using Microsoft.binutils.elflib;
    using Microsoft.Zelig.CodeGeneration.IR.ExternalMethodImporters;


    public static class TypeSystemSerializer
    {
        public delegate object CreateInstance( Type t );

        public delegate void ProgressCallback( long pos, long total );

        enum RecordType : byte
        {
            Null          ,
            Index         ,
            Class         ,
            ClassNoDef    ,
            Array         ,
            TypeUse       ,
            TypeDefinition,
        }

        const string VersionId = "v1.0.0.0, 20081009";

        //--//

        public static TransformationContextForCodeTransformation GetSerializationContext( System.IO.Stream stream )
        {
            return new Writer( stream );
        }

        public static TransformationContextForCodeTransformation GetDeserializationContext( System.IO.Stream stream          ,
                                                                                            CreateInstance   callback        ,
                                                                                            ProgressCallback feedback        ,
                                                                                            int              feedbackQuantum )
        {
            return new Reader( stream, callback, feedback, feedbackQuantum );
        }

        //--//

        public static void Serialize( System.IO.Stream                stream     ,
                                      TypeSystemForCodeTransformation typeSystem )
        {
            using(TransformationContextForCodeTransformation ctx = GetSerializationContext( stream ))
            {
                ctx.Transform( ref typeSystem );
            }
        }

        public static TypeSystemForCodeTransformation Deserialize( System.IO.Stream stream          ,
                                                                   CreateInstance   callback        ,
                                                                   ProgressCallback feedback        ,
                                                                   int              feedbackQuantum )
        {
            TransformationContextForCodeTransformation ctx        = GetDeserializationContext( stream, callback, feedback, feedbackQuantum );
            TypeSystemForCodeTransformation            typeSystem = null;

            ctx.Transform( ref typeSystem );

            return typeSystem;
        }

        //--//

        //
        // PersistenceContextForSerialization
        //

        internal class Writer : TransformationContextForCodeTransformation
        {
            //
            // State
            //

            static GrowOnlyHashTable< Type, System.Reflection.MethodInfo > s_handlers;
            static DynamicTransform                                        s_dynamicTransform;

            System.IO.Stream                                               m_payload;
            System.IO.BinaryWriter                                         m_writer;
            object                                                         m_pending; // Transform( ref object ) has to go through Visit twice. Keep track of it.
                                            
            GrowOnlyHashTable< object, int >                               m_objectToIndex = HashTableFactory.NewWithReferenceEquality< object, int >();
            GrowOnlyHashTable< Type  , int >                               m_typeToIndex   = HashTableFactory.NewWithReferenceEquality< Type  , int >();
            GrowOnlyHashTable< string, int >                               m_stringToIndex = HashTableFactory.New                     < string, int >();

#if DEBUG_PERSISTENCE__COUNT_INSTANCES
            GrowOnlyHashTable< Type  , int >                               m_countTypes    = HashTableFactory.NewWithReferenceEquality< Type  , int >();
#endif

            //
            // Constructor Methods
            //

            internal Writer( System.IO.Stream payload )
            {
                m_payload = payload;
                m_writer  = new System.IO.BinaryWriter( m_payload, System.Text.Encoding.UTF8 );

                m_writer.Write( VersionId );
            }

            //
            // Helper Methods
            //

            public override void Dispose()
            {
                Flush();
            }

            internal void Flush()
            {
#if DEBUG_PERSISTENCE__COUNT_INSTANCES
                Type[] keys = m_countTypes.KeysToArray();

                Array.Sort( keys, delegate( Type t1, Type t2 )
                {
                    return - m_countTypes[t1].CompareTo( m_countTypes[t2] );
                } );

                foreach(Type t in keys)
                {
                    Console.WriteLine( "{0,-100} : {1}", t, m_countTypes[t] );
                }
#endif

                m_writer.Flush();
            }

            //--//

            private bool FindObject(     object obj ,
                                     out int    idx )
            {
                if(m_objectToIndex.TryGetValue( obj, out idx ))
                {
                    return true;
                }

                if(obj is string)
                {
                    if(m_stringToIndex.TryGetValue( (string)obj, out idx ))
                    {
                        return true;
                    }
                }

                return false;
            }

            private void NewObject( object obj )
            {
                if(obj is ValueType)
                {
                    return;
                }

#if DEBUG_PERSISTENCE
                Console.WriteLine( "NewObject {0} {1}", obj.GetType(), m_objectToIndex.Count, obj );
#endif

////            if(m_objectToIndex.Count == )
////            {
////            }

#if DEBUG_PERSISTENCE__COUNT_INSTANCES
                {
                    int count;

                    if(m_countTypes.TryGetValue( obj.GetType(), out count ) == false)
                    {
                        count = 0;
                    }

                    m_countTypes[ obj.GetType() ] = count + 1;
                }
#endif

                int idx = m_objectToIndex.Count;

                m_objectToIndex[obj] = idx;

                if(obj is string)
                {
                    m_stringToIndex[(string)obj] = idx;
                }
            }

            private void NewType( Type type )
            {
                m_typeToIndex[type] = m_typeToIndex.Count;
            }

            //--//

            private void EmitRecordType( RecordType rt )
            {
                m_writer.Write( (byte)rt );
            }

            private void EmitRecordType( RecordType rt  ,
                                         int        idx )
            {
                m_writer.Write( (byte)rt  );
                m_writer.Write(       idx );
            }

            private bool HasPending( object target )
            {
                if(m_pending != null)
                {
                    CHECKS.ASSERT( m_pending == target, "Internal error, ScanTypeSystem was expecting {0}, got {1}", m_pending, target );

                    m_pending = null;
                    return true;
                }

                return false;
            }

            protected override void ClearPending()
            {
                m_pending = null;
            }

            //--//

            private bool EmitArraySignature<T>( T[] array )
            {
                return EmitSignature( array, typeof(T[]) );
            }

            private bool EmitSignature<T>( T obj ) where T : class
            {
                return EmitSignature( obj, typeof(T) );
            }

            private bool EmitSignature( object obj          ,
                                        Type   typeExpected )
            {
                return EmitSignature( obj, obj != null ? obj.GetType() : null, typeExpected );
            }

            private bool EmitSignature( object obj                  ,
                                        Type   typeForSerialization ,
                                        Type   typeExpected         )
            {
                if(HasPending( obj ))
                {
                    return true;
                }

                if(obj == null)
                {
#if DEBUG_PERSISTENCE
                    Console.WriteLine( "RecordType.Null" );
#endif
                    EmitRecordType( RecordType.Null );
                    return false;
                }

                int idx;

                if(FindObject( obj, out idx ))
                {
                    EmitRecordType( RecordType.Index, idx );
#if DEBUG_PERSISTENCE
                    Console.WriteLine( "RecordType.Index {0} {1}", idx, m_writer.BaseStream.Position );
#endif
                    return false;
                }

                EncodeTypeDefinition( typeForSerialization, typeExpected );

                NewObject( obj );

                if(     obj is string) { m_writer.Write( (string)obj ); }
                else if(obj is bool  ) { m_writer.Write( (bool  )obj ); }
                else if(obj is byte  ) { m_writer.Write( (byte  )obj ); }
                else if(obj is sbyte ) { m_writer.Write( (sbyte )obj ); }
                else if(obj is char  ) { m_writer.Write( (char  )obj ); }
                else if(obj is short ) { m_writer.Write( (short )obj ); }
                else if(obj is ushort) { m_writer.Write( (ushort)obj ); }
                else if(obj is int   ) { m_writer.Write( (int   )obj ); }
                else if(obj is uint  ) { m_writer.Write( (uint  )obj ); }
                else if(obj is long  ) { m_writer.Write( (long  )obj ); }
                else if(obj is ulong ) { m_writer.Write( (ulong )obj ); }
                else if(obj is float ) { m_writer.Write( (float )obj ); }
                else if(obj is double) { m_writer.Write( (double)obj ); }
                else if(obj is Type)
                {
                    EncodeTypeDefinition( (Type)obj, null );
                }
                else if(obj is Array)
                {
                    Array array = (Array)obj;

                    m_writer.Write( (int)array.Length );

#if DEBUG_PERSISTENCE
                    Console.WriteLine( "array.Length {0}", array.Length ); 
#endif

                    return array.Length > 0;
                }
                else
                {
                    return true;
                }

                return false;
            }

            //--//

            private int EmitListSize<T>( List<T> lst )
            {
                bool fContinue = EmitSignature( lst );

                if(fContinue)
                {
                    int count = lst.Count;

                    m_writer.Write( count );

#if DEBUG_PERSISTENCE
                    Console.WriteLine( "list.Count {0}", count ); 
#endif

                    return count;
                }

                return -1;
            }

            //--//

            private void EncodeTypeDefinition( Type type       ,
                                               Type typeTarget )
            {
                int idx;

                if(type == typeTarget)
                {
#if DEBUG_PERSISTENCE
                    Console.WriteLine( "RecordType.ClassNoDef {0}", type );
#endif
                    EmitRecordType( RecordType.ClassNoDef );
                }
                else if(m_typeToIndex.TryGetValue( type, out idx ))
                {
#if DEBUG_PERSISTENCE
                    Console.WriteLine( "RecordType.TypeUse {0}", idx );
#endif
                    EmitRecordType( RecordType.TypeUse, idx );
                }
                else
                {
#if DEBUG_PERSISTENCE
                    Console.WriteLine( "RecordType.TypeDefinition {0}", m_typeToIndex.Count );
#endif
                    EmitRecordType( RecordType.TypeDefinition );

                    if(type.IsArray)
                    {
#if DEBUG_PERSISTENCE
                        Console.WriteLine( "RecordType.Array" );
#endif
                        EmitRecordType( RecordType.Array );

                        EncodeTypeDefinition( type.GetElementType(), null );
                    }
                    else
                    {
#if DEBUG_PERSISTENCE
                        Console.WriteLine( "RecordType.Class" );
#endif
                        EmitRecordType( RecordType.Class );

                        if(type.IsGenericType)
                        {
                            Type   templateType = type.GetGenericTypeDefinition();
                            Type[] argsType     = type.GetGenericArguments();

                            m_writer.Write( templateType.AssemblyQualifiedName );
                            m_writer.Write( argsType.Length                    );

                            foreach(Type argType in argsType)
                            {
                                EncodeTypeDefinition( argType, null );
                            }

#if DEBUG_PERSISTENCE
                            Console.WriteLine( "TypeName '{0}' : {1}", type.AssemblyQualifiedName, argsType.Length ); 
#endif
                        }
                        else
                        {
#if DEBUG_PERSISTENCE
                            Console.WriteLine( "TypeName '{0}'", type.AssemblyQualifiedName ); 
#endif
                            m_writer.Write( type.AssemblyQualifiedName );
                        }
                    }

                    NewType( type );
                }
            }


            //--//

            //
            // PersistenceContext
            //

            protected override bool ShouldTransform( object target )
            {
                return true;
            }

            public override void MarkAsVisited( object obj )
            {
            }

            public override void Push( object ctx )
            {
            }

            public override void Pop()
            {
            }

            public override object TopContext()
            {
                return null;
            }

            public override object FindContext( Type ctx )
            {
                return null;
            }

            public override object GetTransformInitiator()
            {
                return null;
            }

            public override TypeSystem GetTypeSystem()
            {
                return null;
            }

            //--//

            public override void Transform( ref ITransformationContextTarget itf )
            {
                if(EmitSignature( itf ))
                {
                    itf.ApplyTransformation( this );
                }
            }

            //--//

            public override void Transform( ref bool val )
            {
                ClearPending();

                m_writer.Write( val );
            }

            public override void Transform( ref char val )
            {
                ClearPending();

                m_writer.Write( val );
            }

            public override void Transform( ref sbyte val )
            {
                ClearPending();

                m_writer.Write( val );
            }

            public override void Transform( ref byte val )
            {
                ClearPending();

                m_writer.Write( val );
            }

            public override void Transform( ref short val )
            {
                ClearPending();

                m_writer.Write( val );
            }

            public override void Transform( ref ushort val )
            {
                ClearPending();

                m_writer.Write( val );
            }

            public override void Transform( ref int val )
            {
                ClearPending();

                m_writer.Write( val );
            }

            public override void Transform( ref uint val )
            {
                ClearPending();

                m_writer.Write( val );
            }

            public override void Transform( ref long val )
            {
                ClearPending();

                m_writer.Write( val );
            }

            public override void Transform( ref ulong val )
            {
                ClearPending();

                m_writer.Write( val );
            }

            public override void Transform( ref float val )
            {
                ClearPending();

                m_writer.Write( val );
            }

            public override void Transform( ref double val )
            {
                ClearPending();

                m_writer.Write( val );
            }

            public override void Transform( ref IntPtr val )
            {
                ClearPending();

                m_writer.Write( val.ToInt32() );
            }

            public override void Transform( ref UIntPtr val )
            {
                ClearPending();

                m_writer.Write( val.ToUInt32() );
            }

            public override void Transform( ref string val )
            {
                if(EmitSignature( val ))
                {
                    ; // Nothing else to do.
                }
            }

            public override void Transform( ref object val )
            {
                if(EmitSignature( val, null ))
                {
                    if(val is ExternalDataDescriptor)
                    {
                    }
                    else if(val is ArmElfExternalDataContext)
                    {
                    }
                    else
                    {
                        m_pending = val;

                        val = TransformGenericReference( val );
                    }
                }
            }

            public override void Transform( ref Type val )
            {
                if(EmitSignature( val, null ))
                {
                    ; // Nothing else to do.
                }
            }

            //--//

            public override void Transform( ref bool[] valArray )
            {
                if(EmitArraySignature( valArray ))
                {
                    for(int i = 0; i < valArray.Length; i++)
                    {
                        m_writer.Write( valArray[i] );
                    }
                }
            }

            public override void Transform( ref char[] valArray )
            {
                if(EmitArraySignature( valArray ))
                {
                    for(int i = 0; i < valArray.Length; i++)
                    {
                        m_writer.Write( valArray[i] );
                    }
                }
            }

            public override void Transform( ref sbyte[] valArray )
            {
                if(EmitArraySignature( valArray ))
                {
                    for(int i = 0; i < valArray.Length; i++)
                    {
                        m_writer.Write( valArray[i] );
                    }
                }
            }

            public override void Transform( ref byte[] valArray )
            {
                if(EmitArraySignature( valArray ))
                {
                    m_writer.Write( valArray, 0, valArray.Length );
                }
            }

            public override void Transform( ref short[] valArray )
            {
                if(EmitArraySignature( valArray ))
                {
                    for(int i = 0; i < valArray.Length; i++)
                    {
                        m_writer.Write( valArray[i] );
                    }
                }
            }

            public override void Transform( ref ushort[] valArray )
            {
                if(EmitArraySignature( valArray ))
                {
                    for(int i = 0; i < valArray.Length; i++)
                    {
                        m_writer.Write( valArray[i] );
                    }
                }
            }

            public override void Transform( ref int[] valArray )
            {
                if(EmitArraySignature( valArray ))
                {
                    for(int i = 0; i < valArray.Length; i++)
                    {
                        m_writer.Write( valArray[i] );
                    }
                }
            }

            public override void Transform( ref uint[] valArray )
            {
                if(EmitArraySignature( valArray ))
                {
                    for(int i = 0; i < valArray.Length; i++)
                    {
                        m_writer.Write( valArray[i] );
                    }
                }
            }

            public override void Transform( ref long[] valArray )
            {
                if(EmitArraySignature( valArray ))
                {
                    for(int i = 0; i < valArray.Length; i++)
                    {
                        m_writer.Write( valArray[i] );
                    }
                }
            }

            public override void Transform( ref ulong[] valArray )
            {
                if(EmitArraySignature( valArray ))
                {
                    for(int i = 0; i < valArray.Length; i++)
                    {
                        m_writer.Write( valArray[i] );
                    }
                }
            }

            public override void Transform( ref float[] valArray )
            {
                if(EmitArraySignature( valArray ))
                {
                    for(int i = 0; i < valArray.Length; i++)
                    {
                        m_writer.Write( valArray[i] );
                    }
                }
            }

            public override void Transform( ref double[] valArray )
            {
                if(EmitArraySignature( valArray ))
                {
                    for(int i = 0; i < valArray.Length; i++)
                    {
                        m_writer.Write( valArray[i] );
                    }
                }
            }

            public override void Transform( ref IntPtr[] valArray )
            {
                if(EmitArraySignature( valArray ))
                {
                    for(int i = 0; i < valArray.Length; i++)
                    {
                        m_writer.Write( valArray[i].ToInt32() );
                    }
                }
            }

            public override void Transform( ref UIntPtr[] valArray )
            {
                if(EmitArraySignature( valArray ))
                {
                    for(int i = 0; i < valArray.Length; i++)
                    {
                        m_writer.Write( valArray[i].ToUInt32() );
                    }
                }
            }

            public override void Transform( ref string[] valArray )
            {
                if(EmitArraySignature( valArray ))
                {
                    var array = valArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                    for(int i = 0; i < array.Length; i++)
                    {
                        Transform( ref array[i] );
                    }
                }
            }

            public override void Transform( ref object[] objArray )
            {
                if(EmitArraySignature( objArray ))
                {
                    var array = objArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                    for(int i = 0; i < array.Length; i++)
                    {
                        Transform( ref array[i] );
                    }
                }
            }

            //--//

            public override void Transform( ref List< string > strLst )
            {
                int count = EmitListSize( strLst );

                var lst = strLst; // Get a local copy, because the original could change, due to the 'ref' argument.

                for(int i = 0; i < count; i++)
                {
                    string str = lst[i];

                    Transform( ref str );
                }
            }

            //--//

            public override void Transform( ref Debugging.DebugInfo debugInfo )
            {
                if(EmitSignature( debugInfo ))
                {
                    Transform( ref debugInfo.SrcFileName     );
                    Transform( ref debugInfo.MethodName      );
                    Transform( ref debugInfo.BeginLineNumber );
                    Transform( ref debugInfo.BeginColumn     );
                    Transform( ref debugInfo.EndLineNumber   );
                    Transform( ref debugInfo.EndColumn       );
                }
            }

            //--//

            public override void Transform( ref WellKnownTypes wkt )
            {
                if(EmitSignature( wkt ))
                {
                    wkt.ApplyTransformation( this );
                }
            }

            public override void Transform( ref WellKnownMethods wkm )
            {
                if(EmitSignature( wkm ))
                {
                    wkm.ApplyTransformation( this );
                }
            }

            public override void Transform( ref WellKnownFields wkf )
            {
                if(EmitSignature( wkf ))
                {
                    wkf.ApplyTransformation( this );
                }
            }

            //--//


            public override void Transform( ref AssemblyRepresentation asml )
            {
                if(EmitSignature( asml ))
                {
                    asml.ApplyTransformation( this );
                }
            }

            public override void Transform( ref List< AssemblyRepresentation > asmlLst )
            {
                int count = EmitListSize( asmlLst );

                var lst = asmlLst; // Get a local copy, because the original could change, due to the 'ref' argument.

                for(int i = 0; i < count; i++)
                {
                    AssemblyRepresentation asml = lst[i];

                    Transform( ref asml );
                }
            }

            public override void Transform( ref AssemblyRepresentation.VersionData ver )
            {
                ClearPending();

                ver.ApplyTransformation( this );
            }

            public override void Transform( ref AssemblyRepresentation.VersionData.AssemblyFlags val )
            {
                ClearPending();

                m_writer.Write( (int)val );
            }

            //--//

            public override void Transform( ref BaseRepresentation bd )
            {
                if(EmitSignature( bd ))
                {
                    bd.ApplyTransformation( this );
                }
            }

            //--//

            public override void Transform( ref TypeRepresentation td )
            {
                if(EmitSignature( td ))
                {
                    td.ApplyTransformation( this );
                }
            }

            public override void Transform( ref ValueTypeRepresentation td )
            {
                if(EmitSignature( td ))
                {
                    td.ApplyTransformation( this );
                }
            }

            public override void Transform( ref ArrayReferenceTypeRepresentation td )
            {
                if(EmitSignature( td ))
                {
                    td.ApplyTransformation( this );
                }
            }

            public override void Transform( ref InterfaceTypeRepresentation itf )
            {
                if(EmitSignature( itf ))
                {
                    itf.ApplyTransformation( this );
                }
            }

            public override void Transform( ref TypeRepresentation[] tdArray )
            {
                if(EmitArraySignature( tdArray ))
                {
                    var array = tdArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                    for(int i = 0; i < array.Length; i++)
                    {
                        Transform( ref array[i] );
                    }
                }
            }

            public override void Transform( ref InterfaceTypeRepresentation[] itfArray )
            {
                if(EmitArraySignature( itfArray ))
                {
                    var array = itfArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                    for(int i = 0; i < array.Length; i++)
                    {
                        Transform( ref array[i] );
                    }
                }
            }

            public override void Transform( ref List< TypeRepresentation > tdLst )
            {
                int count = EmitListSize( tdLst );

                var lst = tdLst; // Get a local copy, because the original could change, due to the 'ref' argument.

                for(int i = 0; i < count; i++)
                {
                    TypeRepresentation td = lst[i];

                    Transform( ref td );
                }
            }

            public override void Transform( ref FieldRepresentation fd )
            {
                if(EmitSignature( fd ))
                {
                    fd.ApplyTransformation( this );
                }
            }

            public override void Transform( ref InstanceFieldRepresentation fd )
            {
                if(EmitSignature( fd ))
                {
                    fd.ApplyTransformation( this );
                }
            }

            public override void Transform( ref StaticFieldRepresentation fd )
            {
                if(EmitSignature( fd ))
                {
                    fd.ApplyTransformation( this );
                }
            }

            public override void Transform( ref FieldRepresentation[] fdArray )
            {
                if(EmitArraySignature( fdArray ))
                {
                    var array = fdArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                    for(int i = 0; i < array.Length; i++)
                    {
                        Transform( ref array[i] );
                    }
                }
            }

            public override void Transform( ref InstanceFieldRepresentation[] fdArray )
            {
                if(EmitArraySignature( fdArray ))
                {
                    var array = fdArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                    for(int i = 0; i < array.Length; i++)
                    {
                        Transform( ref array[i] );
                    }
                }
            }

            public override void Transform( ref MethodRepresentation md )
            {
                if(EmitSignature( md ))
                {
                    md.ApplyTransformation( this );
                }
            }

            public override void Transform( ref MethodRepresentation[] mdArray )
            {
                if(EmitArraySignature( mdArray ))
                {
                    var array = mdArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                    for(int i = 0; i < array.Length; i++)
                    {
                        Transform( ref array[i] );
                    }
                }
            }

            public override void Transform( ref List<MethodRepresentation> resLst )
            {
                int count = EmitListSize( resLst );

                var lst = resLst; // Get a local copy, because the original could change, due to the 'ref' argument.

                for(int i = 0; i < count; i++)
                {
                    MethodRepresentation res = lst[i];

                    Transform( ref res );
                }
            }


            public override void Transform( ref MethodImplRepresentation mi )
            {
                if(EmitSignature( mi ))
                {
                    mi.ApplyTransformation( this );
                }
            }

            public override void Transform( ref MethodImplRepresentation[] miArray )
            {
                if(EmitArraySignature( miArray ))
                {
                    var array = miArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                    for(int i = 0; i < array.Length; i++)
                    {
                        Transform( ref array[i] );
                    }
                }
            }

            public override void Transform( ref GenericParameterDefinition param )
            {
                ClearPending();

                param.ApplyTransformation( this );
            }

            public override void Transform( ref GenericParameterDefinition[] paramArray )
            {
                if(EmitArraySignature( paramArray ))
                {
                    var array = paramArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                    for(int i = 0; i < array.Length; i++)
                    {
                        Transform( ref array[i] );
                    }
                }
            }

            //--//

            public override void Transform( ref CustomAttributeRepresentation ca )
            {
                if(EmitSignature( ca ))
                {
                    ca.ApplyTransformation( this );
                }
            }

            public override void Transform( ref CustomAttributeRepresentation[] caArray )
            {
                if(EmitArraySignature( caArray ))
                {
                    var array = caArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                    for(int i = 0; i < array.Length; i++)
                    {
                        Transform( ref array[i] );
                    }
                }
            }

            //--//

            public override void Transform( ref CustomAttributeAssociationRepresentation caa )
            {
                if(EmitSignature( caa ))
                {
                    caa.ApplyTransformation( this );
                }
            }

            public override void Transform( ref CustomAttributeAssociationRepresentation[] caaArray )
            {
                if(EmitArraySignature( caaArray ))
                {
                    var array = caaArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                    for(int i = 0; i < array.Length; i++)
                    {
                        Transform( ref array[i] );
                    }
                }
            }

            //--//

            public override void Transform( ref ResourceRepresentation res )
            {
                if(EmitSignature( res ))
                {
                    res.ApplyTransformation( this );
                }
            }

            public override void Transform( ref List< ResourceRepresentation > resLst )
            {
                int count = EmitListSize( resLst );

                var lst = resLst; // Get a local copy, because the original could change, due to the 'ref' argument.

                for(int i = 0; i < count; i++)
                {
                    ResourceRepresentation res = lst[i];

                    Transform( ref res );
                }
            }

            public override void Transform( ref ResourceRepresentation.Attributes val )
            {
                ClearPending();

                m_writer.Write( (int)val );
            }

            public override void Transform( ref ResourceRepresentation.Pair[] pairArray )
            {
                if(EmitArraySignature( pairArray ))
                {
                    var array = pairArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                    for(int i = 0; i < array.Length; i++)
                    {
                        TransformGeneric( ref array[i] );
                    }
                }
            }

            //--//

            public override void Transform( ref VTable vTable )
            {
                if(EmitSignature( vTable ))
                {
                    vTable.ApplyTransformation( this );
                }
            }

            public override void Transform( ref VTable.InterfaceMap iMap )
            {
                ClearPending();

                Transform( ref iMap.Interface      );
                Transform( ref iMap.MethodPointers );
            }

            public override void Transform( ref GCInfo gi )
            {
                ClearPending();

                Transform( ref gi.Pointers );
            }

            public override void Transform( ref GCInfo.Kind giKind )
            {
                ClearPending();

                m_writer.Write( (short)giKind );
            }

            public override void Transform( ref GCInfo.Pointer giPtr )
            {
                ClearPending();

                Transform( ref giPtr.Kind   );
                Transform( ref giPtr.OffsetInWords );
            }

            public override void Transform( ref GCInfo.Pointer[] giPtrArray )
            {
                if(EmitArraySignature( giPtrArray ))
                {
                    var array = giPtrArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                    for(int i = 0; i < array.Length; i++)
                    {
                        Transform( ref array[i] );
                    }
                }
            }

            public override void Transform( ref CodePointer cp )
            {
                ClearPending();

                Transform( ref cp.Target );
            }

            public override void Transform( ref CodePointer[] cpArray )
            {
                if(EmitArraySignature( cpArray ))
                {
                    var array = cpArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                    for(int i = 0; i < array.Length; i++)
                    {
                        Transform( ref array[i].Target );
                    }
                }
            }

            //--//

            public override void Transform( ref TypeRepresentation.BuiltInTypes val )
            {
                ClearPending();

                m_writer.Write( (byte)val );
            }

            public override void Transform( ref TypeRepresentation.Attributes val )
            {
                ClearPending();

                m_writer.Write( (int)val );
            }

            public override void Transform( ref TypeRepresentation.BuildTimeAttributes val )
            {
                ClearPending();

                m_writer.Write( (int)val );
            }

            public override void Transform( ref TypeRepresentation.GenericContext gc )
            {
                if(EmitSignature( gc ))
                {
                    gc.ApplyTransformation( this );
                }
            }

            public override void Transform( ref TypeRepresentation.InterfaceMap map )
            {
                ClearPending();

                Transform( ref map.Interface );
                Transform( ref map.Methods   );
            }

            //--//

            public override void Transform( ref FieldRepresentation.Attributes val )
            {
                ClearPending();

                m_writer.Write( (int)val );
            }

            public override void Transform( ref GenericParameterDefinition.Attributes val )
            {
                ClearPending();

                m_writer.Write( (ushort)val );
            }

            public override void Transform( ref MethodRepresentation.Attributes val )
            {
                ClearPending();

                m_writer.Write( (ushort)val );
            }

            public override void Transform( ref MethodRepresentation.BuildTimeAttributes val )
            {
                ClearPending();

                m_writer.Write( (uint)val );
            }

            public override void Transform( ref MethodRepresentation.GenericContext gc )
            {
                if(EmitSignature( gc ))
                {
                    gc.ApplyTransformation( this );
                }
            }

            public override void Transform( ref MultiArrayReferenceTypeRepresentation.Dimension dim )
            {
                ClearPending();

                m_writer.Write( dim.m_lowerBound );
                m_writer.Write( dim.m_upperBound );
            }

            public override void Transform( ref MultiArrayReferenceTypeRepresentation.Dimension[] dimArray )
            {
                if(EmitArraySignature( dimArray ))
                {
                    var array = dimArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                    for(int i = 0; i < array.Length; i++)
                    {
                        Transform( ref array[i] );
                    }
                }
            }

            public override void Transform( ref Runtime.ActivationRecordEvents val )
            {
                ClearPending();

                m_writer.Write( (int)val );
            }

            //
            // PersistenceContextForIR
            //

            public override void Transform( ref ControlFlowGraphState cfg )
            {
                if(EmitSignature( cfg ))
                {
                    ControlFlowGraphStateForCodeTransformation cfg2 = (ControlFlowGraphStateForCodeTransformation)cfg;

                    cfg2.ApplyTransformation( this );
                }
            }

            //--//

            public override void Transform( ref Operator op )
            {
                if(EmitSignature( op ))
                {
                    op.ApplyTransformation( this );
                }
            }

            public override void Transform( ref Operator[] opArray )
            {
                if(EmitArraySignature( opArray ))
                {
                    var array = opArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                    for(int i = 0; i < array.Length; i++)
                    {
                        Transform( ref array[i] );
                    }
                }
            }

            public override void Transform( ref Annotation an )
            {
                if(EmitSignature( an ))
                {
                    an.ApplyTransformation( this );
                }
            }

            public override void Transform( ref Annotation[] anArray )
            {
                if(EmitArraySignature( anArray ))
                {
                    var array = anArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                    for(int i = 0; i < array.Length; i++)
                    {
                        Transform( ref array[i] );
                    }
                }
            }

            //--//

            public override void Transform( ref Expression ex )
            {
                if(EmitSignature( ex ))
                {
                    ex.ApplyTransformation( this );
                }
            }

            public override void Transform( ref ConstantExpression ex )
            {
                if(EmitSignature( ex ))
                {
                    ex.ApplyTransformation( this );
                }
            }

            public override void Transform( ref VariableExpression ex )
            {
                if(EmitSignature( ex ))
                {
                    ex.ApplyTransformation( this );
                }
            }

            public override void Transform( ref VariableExpression.DebugInfo val )
            {
                if(EmitSignature( val ))
                {
                    val.ApplyTransformation( this );
                }
            }

            //--//

            public override void Transform( ref Expression[] exArray )
            {
                if(EmitArraySignature( exArray ))
                {
                    var array = exArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                    for(int i = 0; i < array.Length; i++)
                    {
                        Transform( ref array[i] );
                    }
                }
            }

            public override void Transform( ref VariableExpression[] exArray )
            {
                if(EmitArraySignature( exArray ))
                {
                    var array = exArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                    for(int i = 0; i < array.Length; i++)
                    {
                        Transform( ref array[i] );
                    }
                }
            }

            public override void Transform( ref List< ConstantExpression > exLst )
            {
                int count = EmitListSize( exLst );

                var lst = exLst; // Get a local copy, because the original could change, due to the 'ref' argument.

                for(int i = 0; i < count; i++)
                {
                    ConstantExpression ex = lst[i];

                    Transform( ref ex );
                }
            }

            //--//

            public override void Transform( ref BasicBlock bb )
            {
                if(EmitSignature( bb ))
                {
                    bb.ApplyTransformation( this );
                }
            }

            public override void Transform( ref EntryBasicBlock bb )
            {
                if(EmitSignature( bb ))
                {
                    bb.ApplyTransformation( this );
                }
            }

            public override void Transform( ref ExitBasicBlock bb )
            {
                if(EmitSignature( bb ))
                {
                    bb.ApplyTransformation( this );
                }
            }

            public override void Transform( ref ExceptionHandlerBasicBlock bb )
            {
                if(EmitSignature( bb ))
                {
                    bb.ApplyTransformation( this );
                }
            }

            public override void Transform( ref BasicBlock[] bbArray )
            {
                if(EmitArraySignature( bbArray ))
                {
                    var array = bbArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                    for(int i = 0; i < array.Length; i++)
                    {
                        Transform( ref array[i] );
                    }
                }
            }

            public override void Transform( ref ExceptionHandlerBasicBlock[] bbArray )
            {
                if(EmitArraySignature( bbArray ))
                {
                    var array = bbArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                    for(int i = 0; i < array.Length; i++)
                    {
                        Transform( ref array[i] );
                    }
                }
            }

            public override void Transform( ref BasicBlock.Qualifier val )
            {
                ClearPending();

                m_writer.Write( (int)val );
            }

            //--//

            public override void Transform( ref ExceptionClause ec )
            {
                if(EmitSignature( ec ))
                {
                    ec.ApplyTransformation( this );
                }
            }

            public override void Transform( ref ExceptionClause[] ecArray )
            {
                if(EmitArraySignature( ecArray ))
                {
                    var array = ecArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                    for(int i = 0; i < array.Length; i++)
                    {
                        Transform( ref array[i] );
                    }
                }
            }

            public override void Transform( ref ExceptionClause.ExceptionFlag val )
            {
                ClearPending();

                m_writer.Write( (int)val );
            }

            //--//

            public override void Transform( ref CompilationConstraints val )
            {
                ClearPending();

                m_writer.Write( (int)val );
            }

            public override void Transform( ref CompilationConstraints[] ccArray )
            {
                if(EmitArraySignature( ccArray ))
                {
                    var array = ccArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                    for(int i = 0; i < array.Length; i++)
                    {
                        Transform( ref array[i] );
                    }
                }
            }

            public override void Transform( ref Operator.OperatorCapabilities val )
            {
                ClearPending();

                m_writer.Write( (int)val );
            }

            public override void Transform( ref Operator.OperatorLevel val )
            {
                ClearPending();

                m_writer.Write( (int)val );
            }

            public override void Transform( ref BinaryOperator.ALU val )
            {
                ClearPending();

                m_writer.Write( (int)val );
            }

            public override void Transform( ref UnaryOperator.ALU val )
            {
                ClearPending();

                m_writer.Write( (int)val );
            }

            public override void Transform( ref CallOperator.CallKind val )
            {
                ClearPending();

                m_writer.Write( (int)val );
            }

            public override void Transform( ref CompareAndSetOperator.ActionCondition val )
            {
                ClearPending();

                m_writer.Write( (int)val );
            }

            //
            // TransformationContextForCodeTransformation
            //

            public override void Transform( ref TypeSystemForCodeTransformation typeSystem )
            {
                //
                // Special handling for the root of the type system:
                // don't use the actual type for the signature,
                // just emit the signature for the abstract type.
                //
                if(EmitSignature( typeSystem, typeof(TypeSystemForCodeTransformation), typeof(TypeSystemForCodeTransformation) ))
                {
                    typeSystem.ApplyTransformation( this );
                }
            }

            //--//

            public override void Transform( ref StackLocationExpression.Placement val )
            {
                ClearPending();

                m_writer.Write( (int)val );
            }

            public override void Transform( ref ConditionCodeExpression.Comparison val )
            {
                ClearPending();

                m_writer.Write( (byte)val );
            }

            public override void Transform( ref PiOperator.Relation val )
            {
                ClearPending();

                m_writer.Write( (int)val );
            }

            //--//

            public override void Transform( ref DataManager dataManager )
            {
                if(EmitSignature( dataManager ))
                {
                    dataManager.ApplyTransformation( this );
                }
            }

            public override void Transform( ref DataManager.Attributes val )
            {
                ClearPending();

                m_writer.Write( (int)val );
            }

            public override void Transform( ref DataManager.ObjectDescriptor od )
            {
                if(EmitSignature( od ))
                {
                    od.ApplyTransformation( this );
                }
            }

            public override void Transform( ref DataManager.ArrayDescriptor ad )
            {
                if(EmitSignature( ad ))
                {
                    ad.ApplyTransformation( this );
                }
            }

            //--//

            public override void Transform( ref ImageBuilders.Core imageBuilder )
            {
                if(EmitSignature( imageBuilder ))
                {
                    imageBuilder.ApplyTransformation( this );
                }
            }

            public override void Transform( ref ImageBuilders.CompilationState cs )
            {
                if(EmitSignature( cs ))
                {
                    cs.ApplyTransformation( this );
                }
            }

            public override void Transform( ref ImageBuilders.SequentialRegion reg )
            {
                if(EmitSignature( reg ))
                {
                    reg.ApplyTransformation( this );
                }
            }

            public override void Transform( ref ImageBuilders.ImageAnnotation an )
            {
                if(EmitSignature( an ))
                {
                    an.ApplyTransformation( this );
                }
            }

            public override void Transform( ref ImageBuilders.CodeConstant cc )
            {
                if(EmitSignature( cc ))
                {
                    cc.ApplyTransformation( this );
                }
            }

            public override void Transform( ref ImageBuilders.SequentialRegion[] regArray )
            {
                if(EmitArraySignature( regArray ))
                {
                    var array = regArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                    for(int i = 0; i < array.Length; i++)
                    {
                        Transform( ref array[i] );
                    }
                }
            }

            public override void Transform( ref List< ImageBuilders.SequentialRegion > regLst )
            {
                int count = EmitListSize( regLst );

                var lst = regLst; // Get a local copy, because the original could change, due to the 'ref' argument.

                for(int i = 0; i < count; i++)
                {
                    ImageBuilders.SequentialRegion reg = lst[i];

                    Transform( ref reg );
                }
            }

            public override void Transform( ref List< ImageBuilders.ImageAnnotation > anLst )
            {
                int count = EmitListSize( anLst );

                var lst = anLst; // Get a local copy, because the original could change, due to the 'ref' argument.

                for(int i = 0; i < count; i++)
                {
                    ImageBuilders.ImageAnnotation an = lst[i];

                    Transform( ref an );
                }
            }

            public override void Transform( ref List< ImageBuilders.CodeConstant > ccLst )
            {
                int count = EmitListSize( ccLst );

                var lst = ccLst; // Get a local copy, because the original could change, due to the 'ref' argument.

                for(int i = 0; i < count; i++)
                {
                    ImageBuilders.CodeConstant cc = lst[i];

                    Transform( ref cc );
                }
            }

            public override void Transform( ref List< Runtime.Memory.Range > mrLst )
            {
                int count = EmitListSize( mrLst );

                var lst = mrLst; // Get a local copy, because the original could change, due to the 'ref' argument.

                for(int i = 0; i < count; i++)
                {
                    Runtime.Memory.Range mr = lst[i];

                    TransformGeneric( ref mr );
                }
            }

            public override void Transform( ref Runtime.MemoryAttributes val )
            {
                ClearPending();

                m_writer.Write( (uint)val );
            }

            public override void Transform( ref Runtime.MemoryAttributes[] maArray )
            {
                if(EmitArraySignature( maArray ))
                {
                    var array = maArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                    for(int i = 0; i < array.Length; i++)
                    {
                        Transform( ref array[i] );
                    }
                }
            }

            public override void Transform( ref Runtime.MemoryUsage val )
            {
                ClearPending();

                m_writer.Write( (uint)val );
            }

            public override void Transform( ref Runtime.MemoryUsage[] muArray )
            {
                if(EmitArraySignature( muArray ))
                {
                    var array = muArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                    for(int i = 0; i < array.Length; i++)
                    {
                        Transform( ref array[i] );
                    }
                }
            }

            public override void Transform( ref Abstractions.PlacementRequirements pr )
            {
                if(EmitSignature( pr ))
                {
                    pr.ApplyTransformation( this );
                }
            }

            public override void Transform( ref Abstractions.RegisterDescriptor regDesc )
            {
                if(EmitSignature( regDesc ))
                {
                    regDesc.ApplyTransformation( this );
                }
            }

            public override void Transform( ref Abstractions.RegisterDescriptor[] regDescArray )
            {
                if(EmitArraySignature( regDescArray ))
                {
                    var array = regDescArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                    for(int i = 0; i < array.Length; i++)
                    {
                        Transform( ref array[i] );
                    }
                }
            }

            public override void Transform( ref Abstractions.RegisterClass val )
            {
                ClearPending();

                m_writer.Write( (uint)val );
            }

            public override void Transform( ref Abstractions.CallingConvention.Direction val )
            {
                ClearPending();

                m_writer.Write( (uint)val );
            }

            //--//

            protected override void TransformArray( ref Array arrayIn )
            {
                if(EmitSignature( arrayIn ))
                {
                    var array = arrayIn; // Get a local copy, because the original could change, due to the 'ref' argument.

                    for(int i = 0; i < array.Length; i++)
                    {
                        object obj = array.GetValue( i );

                        Transform( ref obj );
                    }
                }
            }

            //--//

            protected override GrowOnlyHashTable< Type, System.Reflection.MethodInfo > GetMethodInfoTable()
            {
                if(s_handlers == null)
                {
                    s_handlers = BuildMethodInfoTable();
                }

                return s_handlers;
            }

            protected override DynamicTransform GetDynamicTransform()
            {
                if(s_dynamicTransform == null)
                {
                    s_dynamicTransform = BuildDynamicTransform();
                }

                return s_dynamicTransform;
            }

            protected override object TransformThroughReflection( object obj )
            {
                if(EmitSignature( obj, null ))
                {
                    TransformFields( obj, obj.GetType() );
                }

                return obj;
            }

#if TRANSFORMATIONCONTEXT__USE_EMIT
            public override bool IsReader
            {
                get { return false; }
            }
#endif
        }

        //--//

        //
        // PersistenceContextForDeserialization
        //

        internal class Reader : TransformationContextForCodeTransformation
        {
            delegate object DynamicNewObj();

            class SparseList
            {
                //
                // State
                //

                const int c_ChunkSize = 1024;

                List< object[] > m_list;
                object[]         m_current;
                int              m_pos;

                //
                // Constructor Methods
                //

                internal SparseList()
                {
                    m_list = new List< object[] >();
                }

                //
                // Helper Methods
                //

                internal void Add( object obj )
                {
                    if(m_current == null)
                    {
                        m_current = new object[c_ChunkSize];

                        m_list.Add( m_current );
                    }

                    m_current[m_pos++] = obj;

                    if(m_pos == c_ChunkSize)
                    {
                        m_current = null;
                        m_pos     = 0;
                    }
                }

                //
                // Access Methods
                //

                internal int Count
                {
                    get
                    {
                        int res = m_list.Count * c_ChunkSize;

                        if(m_current != null)
                        {
                            res -= c_ChunkSize;
                            res += m_pos;
                        }

                        return res;
                    }
                }

                internal object this[int index]
                {
                    get
                    {
                        object[] chunk = m_list[ index / c_ChunkSize ];

                        return chunk[index % c_ChunkSize];
                    }
                }
            }

            //
            // State
            //

            static GrowOnlyHashTable< Type, System.Reflection.MethodInfo > s_handlers;
            static DynamicTransform                                        s_dynamicTransform;

            System.IO.BinaryReader                                         m_reader;
            object                                                         m_pending; // Transform( ref object ) has to go through Visit twice. Keep track of it.
                                  
            SparseList                                                     m_indexToObject = new SparseList  ();
            List< Type   >                                                 m_indexToType   = new List< Type >();
            CreateInstance                                                 m_callback;
            ProgressCallback                                               m_feedback;
            int                                                            m_feedbackQuantum;
            int                                                            m_feedbackCount;
            int                                                            m_contextStackSize;

            GrowOnlyHashTable< Type, DynamicNewObj >                       m_constructors;

            //
            // Constructor Methods
            //

            internal Reader( System.IO.Stream stream          ,
                             CreateInstance   callback        ,
                             ProgressCallback feedback        ,
                             int              feedbackQuantum )
            {
                m_reader          = new System.IO.BinaryReader( stream, System.Text.Encoding.UTF8 );
                m_callback        = callback;
                m_feedback        = feedback;
                m_feedbackQuantum = feedbackQuantum;

                string version = m_reader.ReadString();

                if(version != VersionId)
                {
                    throw new NotSupportedException( string.Format( "Expecting file version {0}, got {1}", VersionId, version ) );
                }
            }

            //--//

            private void NewObject( object obj )
            {
                if(obj is ValueType)
                {
                    return;
                }

#if DEBUG_PERSISTENCE
                Console.WriteLine( "NewObject {0} {1}", obj.GetType(), m_indexToObject.Count );
#endif

////            if(m_indexToObject.Count == )
////            {
////            }

                m_indexToObject.Add( obj );
            }

            private void NewType( Type type )
            {
                m_indexToType.Add( type );
            }

            //--//

            private RecordType ReadRecordType()
            {
                return (RecordType)m_reader.ReadByte();
            }

            protected override void ClearPending()
            {
                m_pending = null;
            }

            //--//

            private bool GetSignature<T>( ref T obj ) where T : class
            {
                object res  = null;
                bool   fGot = GetSignature( ref res, typeof(T) );

                obj = (T)res;

                return fGot;
            }

            private bool GetSignature( ref object obj          ,
                                           Type   typeExpected )
            {
                if(m_pending != null)
                {
                    obj = m_pending;
                    
                    m_pending = null;
                    return true;
                }

                if(m_feedback != null)
                {
                    m_feedbackCount++;

                    if(m_feedbackCount >= m_feedbackQuantum)
                    {
                        m_feedbackCount = 0;

                        m_feedback( m_reader.BaseStream.Position, m_reader.BaseStream.Length );
                    }
                }

                RecordType rt = ReadRecordType();

                if(rt == RecordType.Null)
                {
#if DEBUG_PERSISTENCE
                    Console.WriteLine( "RecordType.Null" );
#endif
                    obj = null;
                    return false;
                }
                else if(rt == RecordType.Index)
                {
                    int idx = m_reader.ReadInt32();

#if DEBUG_PERSISTENCE
                    Console.WriteLine( "RecordType.Index {0} {1}", idx, m_reader.BaseStream.Position );
#endif

                    obj = m_indexToObject[ idx ];
                    return false;
                }

                Type t = DecodeTypeDefinition( rt, typeExpected );

                if(t.IsArray)
                {
                    if(t.GetArrayRank() != 1)
                    {
                        throw TypeConsistencyErrorException.Create( "Multi-dimension arrays not supported during type system serialization: {0}", t );
                    }
    
                    Type elementType = t.GetElementType();
                    int  len         = m_reader.ReadInt32();
    
                    Array array = Array.CreateInstance( elementType, len );

                    NewObject( array );

#if DEBUG_PERSISTENCE
                    Console.WriteLine( "array.Length {0}", len ); 
#endif

                    obj = array;

                    return array.Length > 0;
                }

                if(     t == typeof(string)) { obj = m_reader.ReadString (); }
                else if(t == typeof(bool  )) { obj = m_reader.ReadBoolean(); }
                else if(t == typeof(byte  )) { obj = m_reader.ReadByte   (); }
                else if(t == typeof(sbyte )) { obj = m_reader.ReadSByte  (); }
                else if(t == typeof(char  )) { obj = m_reader.ReadChar   (); }
                else if(t == typeof(short )) { obj = m_reader.ReadInt16  (); }
                else if(t == typeof(ushort)) { obj = m_reader.ReadUInt16 (); }
                else if(t == typeof(int   )) { obj = m_reader.ReadInt32  (); }
                else if(t == typeof(uint  )) { obj = m_reader.ReadUInt32 (); }
                else if(t == typeof(long  )) { obj = m_reader.ReadInt64  (); }
                else if(t == typeof(ulong )) { obj = m_reader.ReadUInt64 (); }
                else if(t == typeof(float )) { obj = m_reader.ReadSingle (); }
                else if(t == typeof(double)) { obj = m_reader.ReadDouble (); }
                else if(t.IsSubclassOf( typeof(Type) ))
                {
                    obj = DecodeTypeDefinition( ReadRecordType(), null );
                }
                else
                {
                    obj = m_callback != null ? m_callback( t ) : null;
                    if(obj == null)
                    {
                        if(m_constructors == null)
                        {
                            m_constructors = HashTableFactory.NewWithReferenceEquality< Type, DynamicNewObj >();
                        }

                        DynamicNewObj dlg;

                        if(m_constructors.TryGetValue( t, out dlg ) == false)
                        {
                            dlg = BuildDynamicNewObj( t );

                            m_constructors[t] = dlg;
                        }

                        obj = dlg();
                    }

                    NewObject( obj );

                    return true;
                }

                NewObject( obj );
                return false;
            }

#if READER__USE_EMIT
            delegate object GetUninitializedObjectDelegate( Type type );
            delegate Type   GetTypeFromHandleDelegate( RuntimeTypeHandle handle );
#endif

            private DynamicNewObj BuildDynamicNewObj( Type t )
            {
                const BindingFlags flags = BindingFlags.Instance  |
                                           BindingFlags.NonPublic |
                                           BindingFlags.Public;


                Type            tObj   = t;
                Type[]          tParam = new Type[0];
                ConstructorInfo ci     = null;

                while(tObj != null)
                {
                    ci = tObj.GetConstructor( flags, null, tParam, null );
                    if(ci != null)
                    {
                        break;
                    }

                    tObj = tObj.BaseType;
                }

#if READER__USE_EMIT

                DynamicMethod methodBuilder = new DynamicMethod( "", typeof(object), null, true );

                ILGenerator il = methodBuilder.GetILGenerator( 256 );

                if(t.IsValueType)
                {
                    LocalBuilder objectRef = il.DeclareLocal( t );

                    il.Emit( OpCodes.Ldloca , objectRef   );
                    il.Emit( OpCodes.Initobj, t           );
                    il.Emit( OpCodes.Ldloc  , objectRef   );
                    il.Emit( OpCodes.Box    , t           );
                }
                else
                {
                    if(ci != null)
                    {
                        if(ci.DeclaringType == t)
                        {
                            il.Emit( OpCodes.Newobj, ci );
                        }
                        else
                        {
                            return delegate()
                            {
                                object obj = System.Runtime.Serialization.FormatterServices.GetUninitializedObject( t );

                                ci.Invoke( obj, null );

                                return obj;
                            };
                        }
////
////                    GetUninitializedObjectDelegate dlgGetUninitializedObject = System.Runtime.Serialization.FormatterServices.GetUninitializedObject;
////                    GetTypeFromHandleDelegate      dlgGetTypeFromHandle      = Type.GetTypeFromHandle;
////
////                    LocalBuilder objectRef = il.DeclareLocal( t );
////
////                    il.Emit( OpCodes.Ldtoken  , t                                );
////                    il.Emit( OpCodes.Call     , dlgGetTypeFromHandle     .Method );
////                    il.Emit( OpCodes.Call     , dlgGetUninitializedObject.Method );
////                    il.Emit( OpCodes.Castclass, t                                );
////
////                    il.Emit( OpCodes.Stloc, objectRef );
////    
////                    if(ci != null)
////                    {
////                        il.Emit( OpCodes.Ldloc   , objectRef );
////                        il.Emit( OpCodes.Callvirt, ci        ); << This causes a verification exception.
////                    }
////    
////                    il.Emit( OpCodes.Ldloc, objectRef );
                    }
                    else
                    {
                        GetUninitializedObjectDelegate dlgGetUninitializedObject = System.Runtime.Serialization.FormatterServices.GetUninitializedObject;
                        GetTypeFromHandleDelegate      dlgGetTypeFromHandle      = Type.GetTypeFromHandle;

                        il.Emit( OpCodes.Ldtoken, t                                );
                        il.Emit( OpCodes.Call   , dlgGetTypeFromHandle     .Method );
                        il.Emit( OpCodes.Call   , dlgGetUninitializedObject.Method );
                    }
                }

                il.Emit( OpCodes.Ret );

                return (DynamicNewObj)methodBuilder.CreateDelegate( typeof(DynamicNewObj) );

#else

                if(ci == null)
                {
                    return delegate()
                    {
                        return System.Runtime.Serialization.FormatterServices.GetUninitializedObject( t );
                    };
                }
                else
                {
                    return delegate()
                    {
                        object obj = System.Runtime.Serialization.FormatterServices.GetUninitializedObject( t );

                        ci.Invoke( obj, null );

                        return obj;
                    };
                }
#endif
            }

            //--//

            private int GetListSize<T>( ref List<T> lst )
            {
                object res       = null;
                bool   fContinue = GetSignature( ref res, typeof(List< T >) );

                lst = (List< T >)res;

                if(fContinue)
                {
                    int count = m_reader.ReadInt32();

#if DEBUG_PERSISTENCE
                    Console.WriteLine( "list.Count {0}", count ); 
#endif

                    return count;

                }
                else
                {
                    return -1;
                }
            }

            //--//

            private Type DecodeTypeDefinition( RecordType rt         ,
                                               Type       targetType )
            {
                if(rt == RecordType.ClassNoDef)
                {
#if DEBUG_PERSISTENCE
                    Console.WriteLine( "RecordType.ClassNoDef {0}", targetType );
#endif
                    return targetType;
                }
                else if(rt == RecordType.TypeUse)
                {
                    int idx = m_reader.ReadInt32();

#if DEBUG_PERSISTENCE
                    Console.WriteLine( "RecordType.TypeUse {0}", idx );
#endif
                    return m_indexToType[ idx ];
                }
                else if(rt == RecordType.TypeDefinition)
                {
#if DEBUG_PERSISTENCE
                    Console.WriteLine( "RecordType.TypeDefinition {0}", m_indexToType.Count );
#endif
                    Type t;

                    rt = ReadRecordType();

                    if(rt == RecordType.Array)
                    {
#if DEBUG_PERSISTENCE
                        Console.WriteLine( "RecordType.Array" ); 
#endif
                        Type elementType = DecodeTypeDefinition( ReadRecordType(), null );

                        t = elementType.MakeArrayType();
                    }
                    else if(rt == RecordType.Class)
                    {
#if DEBUG_PERSISTENCE
                        Console.WriteLine( "RecordType.Class" );
#endif
                        string assemblyQualifiedName = m_reader.ReadString();

                        t = Type.GetType( assemblyQualifiedName );

                        if(t.IsGenericType)
                        {
                            int    argNum   = m_reader.ReadInt32();
                            Type[] argsType = new Type[argNum];

                            for(int i = 0; i < argNum; i++)
                            {
                                argsType[i] = DecodeTypeDefinition( ReadRecordType(), null );
                            }

                            t = t.MakeGenericType( argsType );

#if DEBUG_PERSISTENCE
                            Console.WriteLine( "TypeName '{0}' : {1}", t.AssemblyQualifiedName, argNum ); 
#endif
                        }
                        else
                        {
#if DEBUG_PERSISTENCE
                            Console.WriteLine( "TypeName '{0}'", assemblyQualifiedName ); 
#endif
                        }
                    }
                    else
                    {
                        throw TypeConsistencyErrorException.Create( "Unrecognized token '{0}' received during type system deserialization", rt );
                    }

                    NewType( t );

                    return t;
                }
                else
                {
                    throw TypeConsistencyErrorException.Create( "Unrecognized token '{0}' received during type system deserialization", rt );
                }
            }

            //--//

            //
            // TransformationContext
            //

            protected override bool ShouldTransform( object target )
            {
                return true;
            }

            public override void MarkAsVisited( object obj )
            {
            }

            public override void Push( object ctx )
            {
                m_contextStackSize++;
            }

            public override void Pop()
            {
                CHECKS.ASSERT( m_contextStackSize > 0, "Unbalanced push/pop pair" );

                if(--m_contextStackSize == 0)
                {
                    RunDelayedUpdates();
                }
            }

            public override object TopContext()
            {
                return null;
            }

            public override object FindContext( Type ctx )
            {
                return null;
            }

            public override object GetTransformInitiator()
            {
                return null;
            }

            public override TypeSystem GetTypeSystem()
            {
                return null;
            }

            //--//

            public override void Transform( ref ITransformationContextTarget itf )
            {
                if(GetSignature( ref itf ))
                {
                    itf.ApplyTransformation( this );
                }
            }

            //--//

            public override void Transform( ref bool val )
            {
                ClearPending();

                val = m_reader.ReadBoolean();
            }

            public override void Transform( ref char val )
            {
                ClearPending();

                val = m_reader.ReadChar();
            }


            public override void Transform( ref sbyte val )
            {
                ClearPending();

                val = m_reader.ReadSByte();
            }

            public override void Transform( ref byte val )
            {
                ClearPending();

                val = m_reader.ReadByte();
            }

            public override void Transform( ref short val )
            {
                ClearPending();

                val = m_reader.ReadInt16();
            }

            public override void Transform( ref ushort val )
            {
                ClearPending();

                val = m_reader.ReadUInt16();
            }

            public override void Transform( ref int val )
            {
                ClearPending();

                val = m_reader.ReadInt32();
            }

            public override void Transform( ref uint val )
            {
                ClearPending();

                val = m_reader.ReadUInt32();
            }

            public override void Transform( ref long val )
            {
                ClearPending();

                val = m_reader.ReadInt64();
            }

            public override void Transform( ref ulong val )
            {
                ClearPending();

                val = m_reader.ReadUInt64();
            }

            public override void Transform( ref float val )
            {
                ClearPending();

                val = m_reader.ReadSingle();
            }

            public override void Transform( ref double val )
            {
                ClearPending();

                val = m_reader.ReadDouble();
            }

            public override void Transform( ref IntPtr val )
            {
                ClearPending();

                val = new IntPtr( m_reader.ReadInt32() );
            }

            public override void Transform( ref UIntPtr val )
            {
                ClearPending();

                val = new UIntPtr( m_reader.ReadUInt32() );
            }

            public override void Transform( ref string val )
            {
                if(GetSignature( ref val ))
                {
                    ; // Nothing else to do.
                }
            }

            public override void Transform( ref object val )
            {
                if(GetSignature( ref val, null ))
                {
                    if(val is ExternalDataDescriptor)
                    {
                        val = new ExternalDataDescriptor();
                    }
                    else if(val is ArmElfExternalDataContext)
                    {
                        val = new ArmElfExternalDataContext(null, null);
                    }
                    else
                    {
                        m_pending = val;

                        val = TransformGenericReference( val );
                    }
                }
            }

            public override void Transform( ref Type val )
            {
                if(GetSignature( ref val ))
                {
                    ; // Nothing else to do.
                }
            }

            //--//

            public override void Transform( ref bool[] valArray )
            {
                if(GetSignature( ref valArray ))
                {
                    for(int i = 0; i < valArray.Length; i++)
                    {
                        valArray[i] = m_reader.ReadBoolean();
                    }
                }
            }

            public override void Transform( ref char[] valArray )
            {
                if(GetSignature( ref valArray ))
                {
                    for(int i = 0; i < valArray.Length; i++)
                    {
                        valArray[i] = m_reader.ReadChar();
                    }
                }
            }

            public override void Transform( ref sbyte[] valArray )
            {
                if(GetSignature( ref valArray ))
                {
                    for(int i = 0; i < valArray.Length; i++)
                    {
                        valArray[i] = m_reader.ReadSByte();
                    }
                }
            }

            public override void Transform( ref byte[] valArray )
            {
                if(GetSignature( ref valArray ))
                {
                    m_reader.Read( valArray, 0, valArray.Length );
                }
            }

            public override void Transform( ref short[] valArray )
            {
                if(GetSignature( ref valArray ))
                {
                    for(int i = 0; i < valArray.Length; i++)
                    {
                        valArray[i] = m_reader.ReadInt16();
                    }
                }
            }

            public override void Transform( ref ushort[] valArray )
            {
                if(GetSignature( ref valArray ))
                {
                    for(int i = 0; i < valArray.Length; i++)
                    {
                        valArray[i] = m_reader.ReadUInt16();
                    }
                }
            }

            public override void Transform( ref int[] valArray )
            {
                if(GetSignature( ref valArray ))
                {
                    for(int i = 0; i < valArray.Length; i++)
                    {
                        valArray[i] = m_reader.ReadInt32();
                    }
                }
            }

            public override void Transform( ref uint[] valArray )
            {
                if(GetSignature( ref valArray ))
                {
                    for(int i = 0; i < valArray.Length; i++)
                    {
                        valArray[i] = m_reader.ReadUInt32();
                    }
                }
            }

            public override void Transform( ref long[] valArray )
            {
                if(GetSignature( ref valArray ))
                {
                    for(int i = 0; i < valArray.Length; i++)
                    {
                        valArray[i] = m_reader.ReadInt64();
                    }
                }
            }

            public override void Transform( ref ulong[] valArray )
            {
                if(GetSignature( ref valArray ))
                {
                    for(int i = 0; i < valArray.Length; i++)
                    {
                        valArray[i] = m_reader.ReadUInt64();
                    }
                }
            }

            public override void Transform( ref float[] valArray )
            {
                if(GetSignature( ref valArray ))
                {
                    for(int i = 0; i < valArray.Length; i++)
                    {
                        valArray[i] = m_reader.ReadSingle();
                    }
                }
            }

            public override void Transform( ref double[] valArray )
            {
                if(GetSignature( ref valArray ))
                {
                    for(int i = 0; i < valArray.Length; i++)
                    {
                        valArray[i] = m_reader.ReadDouble();
                    }
                }
            }

            public override void Transform( ref IntPtr[] valArray )
            {
                if(GetSignature( ref valArray ))
                {
                    for(int i = 0; i < valArray.Length; i++)
                    {
                        valArray[i] = new IntPtr( m_reader.ReadInt32() );
                    }
                }
            }

            public override void Transform( ref UIntPtr[] valArray )
            {
                if(GetSignature( ref valArray ))
                {
                    for(int i = 0; i < valArray.Length; i++)
                    {
                        valArray[i] = new UIntPtr( m_reader.ReadUInt32() );
                    }
                }
            }

            public override void Transform( ref string[] valArray )
            {
                if(GetSignature( ref valArray ))
                {
                    var array = valArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                    for(int i = 0; i < array.Length; i++)
                    {
                        Transform( ref array[i] );
                    }
                }
            }

            public override void Transform( ref object[] objArray )
            {
                if(GetSignature( ref objArray ))
                {
                    var array = objArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                    for(int i = 0; i < array.Length; i++)
                    {
                        Transform( ref array[i] );
                    }
                }
            }

            //--//

            public override void Transform( ref List< string > strLst )
            {
                int count = GetListSize( ref strLst );

                var lst = strLst; // Get a local copy, because the original could change, due to the 'ref' argument.

                for(int i = 0; i < count; i++)
                {
                    string str = null;

                    Transform( ref str );

                    lst.Add( str );
                }
            }

            //--//

            public override void Transform( ref Debugging.DebugInfo debugInfo )
            {
                if(GetSignature( ref debugInfo ))
                {
                    Transform( ref debugInfo.SrcFileName     );
                    Transform( ref debugInfo.MethodName      );
                    Transform( ref debugInfo.BeginLineNumber );
                    Transform( ref debugInfo.BeginColumn     );
                    Transform( ref debugInfo.EndLineNumber   );
                    Transform( ref debugInfo.EndColumn       );
                }
            }

            //--//

            public override void Transform( ref WellKnownTypes wkt )
            {
                if(GetSignature( ref wkt ))
                {
                    wkt.ApplyTransformation( this );
                }
            }

            public override void Transform( ref WellKnownMethods wkm )
            {
                if(GetSignature( ref wkm ))
                {
                    wkm.ApplyTransformation( this );
                }
            }

            public override void Transform( ref WellKnownFields wkf )
            {
                if(GetSignature( ref wkf ))
                {
                    wkf.ApplyTransformation( this );
                }
            }

            //--//

            public override void Transform( ref AssemblyRepresentation asml )
            {
                if(GetSignature( ref asml ))
                {
                    asml.ApplyTransformation( this );
                }
            }

            public override void Transform( ref List< AssemblyRepresentation > asmlLst )
            {
                int count = GetListSize( ref asmlLst );

                var lst = asmlLst; // Get a local copy, because the original could change, due to the 'ref' argument.

                for(int i = 0; i < count; i++)
                {
                    AssemblyRepresentation asml = null;

                    Transform( ref asml );

                    lst.Add( asml );
                }
            }

            public override void Transform( ref AssemblyRepresentation.VersionData ver )
            {
                ClearPending();

                ver.ApplyTransformation( this );
            }

            public override void Transform( ref AssemblyRepresentation.VersionData.AssemblyFlags val )
            {
                ClearPending();

                val = (AssemblyRepresentation.VersionData.AssemblyFlags)m_reader.ReadInt32();
            }

            //--//

            public override void Transform( ref BaseRepresentation bd )
            {
                if(GetSignature( ref bd ))
                {
                    bd.ApplyTransformation( this );
                }
            }

            //--//

            public override void Transform( ref TypeRepresentation td )
            {
                if(GetSignature( ref td ))
                {
                    td.ApplyTransformation( this );
                }
            }

            public override void Transform( ref ValueTypeRepresentation td )
            {
                if(GetSignature( ref td ))
                {
                    td.ApplyTransformation( this );
                }
            }

            public override void Transform( ref ArrayReferenceTypeRepresentation td )
            {
                if(GetSignature( ref td ))
                {
                    td.ApplyTransformation( this );
                }
            }

            public override void Transform( ref InterfaceTypeRepresentation itf )
            {
                if(GetSignature( ref itf ))
                {
                    itf.ApplyTransformation( this );
                }
            }

            public override void Transform( ref TypeRepresentation[] tdArray )
            {
                if(GetSignature( ref tdArray ))
                {
                    var array = tdArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                    for(int i = 0; i < array.Length; i++)
                    {
                        Transform( ref array[i] );
                    }
                }
            }

            public override void Transform( ref InterfaceTypeRepresentation[] itfArray )
            {
                if(GetSignature( ref itfArray ))
                {
                    var array = itfArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                    for(int i = 0; i < array.Length; i++)
                    {
                        Transform( ref array[i] );
                    }
                }
            }

            public override void Transform( ref List< TypeRepresentation > tdLst )
            {
                int count = GetListSize( ref tdLst );

                var lst = tdLst; // Get a local copy, because the original could change, due to the 'ref' argument.

                for(int i = 0; i < count; i++)
                {
                    TypeRepresentation td = null;

                    Transform( ref td );

                    lst.Add( td );
                }
            }

            public override void Transform( ref FieldRepresentation fd )
            {
                if(GetSignature( ref fd ))
                {
                    fd.ApplyTransformation( this );
                }
            }

            public override void Transform( ref InstanceFieldRepresentation fd )
            {
                if(GetSignature( ref fd ))
                {
                    fd.ApplyTransformation( this );
                }
            }

            public override void Transform( ref StaticFieldRepresentation fd )
            {
                if(GetSignature( ref fd ))
                {
                    fd.ApplyTransformation( this );
                }
            }

            public override void Transform( ref FieldRepresentation[] fdArray )
            {
                if(GetSignature( ref fdArray ))
                {
                    var array = fdArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                    for(int i = 0; i < array.Length; i++)
                    {
                        Transform( ref array[i] );
                    }
                }
            }

            public override void Transform( ref InstanceFieldRepresentation[] fdArray )
            {
                if(GetSignature( ref fdArray ))
                {
                    var array = fdArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                    for(int i = 0; i < array.Length; i++)
                    {
                        Transform( ref array[i] );
                    }
                }
            }

            public override void Transform( ref MethodRepresentation md )
            {
                if(GetSignature( ref md ))
                {
                    md.ApplyTransformation( this );
                }
            }

            public override void Transform( ref MethodRepresentation[] mdArray )
            {
                if(GetSignature( ref mdArray ))
                {
                    var array = mdArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                    for(int i = 0; i < array.Length; i++)
                    {
                        Transform( ref array[i] );
                    }
                }
            }

            public override void Transform( ref List<MethodRepresentation> resLst )
            {
                int count = GetListSize( ref resLst );

                var lst = resLst; // Get a local copy, because the original could change, due to the 'ref' argument.

                for(int i = 0; i < count; i++)
                {
                    MethodRepresentation res = null;

                    Transform( ref res );

                    lst.Add( res );
                }
            }

            public override void Transform( ref MethodImplRepresentation mi )
            {
                if(GetSignature( ref mi ))
                {
                    mi.ApplyTransformation( this );
                }
            }

            public override void Transform( ref MethodImplRepresentation[] miArray )
            {
                if(GetSignature( ref miArray ))
                {
                    var array = miArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                    for(int i = 0; i < array.Length; i++)
                    {
                        Transform( ref array[i] );
                    }
                }
            }

            public override void Transform( ref GenericParameterDefinition param )
            {
                ClearPending();

                param.ApplyTransformation( this );
            }

            public override void Transform( ref GenericParameterDefinition[] paramArray )
            {
                if(GetSignature( ref paramArray ))
                {
                    var array = paramArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                    for(int i = 0; i < array.Length; i++)
                    {
                        Transform( ref array[i] );
                    }
                }
            }

            //--//

            public override void Transform( ref CustomAttributeRepresentation ca )
            {
                if(GetSignature( ref ca ))
                {
                    ca.ApplyTransformation( this );
                }
            }

            public override void Transform( ref CustomAttributeRepresentation[] caArray )
            {
                if(GetSignature( ref caArray ))
                {
                    var array = caArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                    for(int i = 0; i < array.Length; i++)
                    {
                        Transform( ref array[i] );
                    }
                }
            }

            //--//

            public override void Transform( ref CustomAttributeAssociationRepresentation caa )
            {
                if(GetSignature( ref caa ))
                {
                    caa.ApplyTransformation( this );
                }
            }

            public override void Transform( ref CustomAttributeAssociationRepresentation[] caaArray )
            {
                if(GetSignature( ref caaArray ))
                {
                    var array = caaArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                    for(int i = 0; i < array.Length; i++)
                    {
                        Transform( ref array[i] );
                    }
                }
            }

            //--//

            public override void Transform( ref ResourceRepresentation res )
            {
                if(GetSignature( ref res ))
                {
                    res.ApplyTransformation( this );
                }
            }

            public override void Transform( ref List< ResourceRepresentation > resLst )
            {
                int count = GetListSize( ref resLst );

                var lst = resLst; // Get a local copy, because the original could change, due to the 'ref' argument.

                for(int i = 0; i < count; i++)
                {
                    ResourceRepresentation res = null;

                    Transform( ref res );

                    lst.Add( res );
                }
            }

            public override void Transform( ref ResourceRepresentation.Attributes val )
            {
                ClearPending();

                val = (ResourceRepresentation.Attributes)m_reader.ReadInt32();
            }

            public override void Transform( ref ResourceRepresentation.Pair[] pairArray )
            {
                if(GetSignature( ref pairArray ))
                {
                    var array = pairArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                    for(int i = 0; i < array.Length; i++)
                    {
                        TransformGeneric( ref array[i] );
                    }
                }
            }

            //--//

            public override void Transform( ref VTable vTable )
            {
                if(GetSignature( ref vTable ))
                {
                    vTable.ApplyTransformation( this );
                }
            }

            public override void Transform( ref VTable.InterfaceMap iMap )
            {
                ClearPending();

                Transform( ref iMap.Interface      );
                Transform( ref iMap.MethodPointers );
            }

            public override void Transform( ref GCInfo gi )
            {
                ClearPending();

                Transform( ref gi.Pointers );
            }

            public override void Transform( ref GCInfo.Kind giKind )
            {
                ClearPending();

                giKind = (GCInfo.Kind)m_reader.ReadInt16();
            }

            public override void Transform( ref GCInfo.Pointer giPtr )
            {
                ClearPending();

                Transform( ref giPtr.Kind          );
                Transform( ref giPtr.OffsetInWords );
            }

            public override void Transform( ref GCInfo.Pointer[] giPtrArray )
            {
                if(GetSignature( ref giPtrArray ))
                {
                    var array = giPtrArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                    for(int i = 0; i < array.Length; i++)
                    {
                        Transform( ref array[i] );
                    }
                }
            }

            public override void Transform( ref CodePointer cp )
            {
                ClearPending();

                Transform( ref cp.Target );
            }

            public override void Transform( ref CodePointer[] cpArray )
            {
                if(GetSignature( ref cpArray ))
                {
                    var array = cpArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                    for(int i = 0; i < array.Length; i++)
                    {
                        Transform( ref array[i].Target );
                    }
                }
            }

            //--//

            public override void Transform( ref TypeRepresentation.BuiltInTypes val )
            {
                ClearPending();

                val = (TypeRepresentation.BuiltInTypes)m_reader.ReadByte();
            }

            public override void Transform( ref TypeRepresentation.Attributes val )
            {
                ClearPending();

                val = (TypeRepresentation.Attributes)m_reader.ReadInt32();
            }

            public override void Transform( ref TypeRepresentation.BuildTimeAttributes val )
            {
                ClearPending();

                val = (TypeRepresentation.BuildTimeAttributes)m_reader.ReadInt32();
            }

            public override void Transform( ref TypeRepresentation.GenericContext gc )
            {
                if(GetSignature( ref gc ))
                {
                    gc.ApplyTransformation( this );
                }
            }

            public override void Transform( ref TypeRepresentation.InterfaceMap map )
            {
                ClearPending();

                Transform( ref map.Interface );
                Transform( ref map.Methods   );
            }

            //--//

            public override void Transform( ref FieldRepresentation.Attributes val )
            {
                ClearPending();

                val = (FieldRepresentation.Attributes)m_reader.ReadInt32();
            }

            public override void Transform( ref GenericParameterDefinition.Attributes val )
            {
                ClearPending();

                val = (GenericParameterDefinition.Attributes)m_reader.ReadUInt16();
            }

            public override void Transform( ref MethodRepresentation.Attributes val )
            {
                ClearPending();

                val = (MethodRepresentation.Attributes)m_reader.ReadUInt16();
            }

            public override void Transform( ref MethodRepresentation.BuildTimeAttributes val )
            {
                ClearPending();

                val = (MethodRepresentation.BuildTimeAttributes)m_reader.ReadUInt32();
            }

            public override void Transform( ref MethodRepresentation.GenericContext gc )
            {
                if(GetSignature( ref gc ))
                {
                    gc.ApplyTransformation( this );
                }
            }

            public override void Transform( ref MultiArrayReferenceTypeRepresentation.Dimension dim )
            {
                ClearPending();

                dim.m_lowerBound = m_reader.ReadUInt32();
                dim.m_upperBound = m_reader.ReadUInt32();
            }

            public override void Transform( ref MultiArrayReferenceTypeRepresentation.Dimension[] dimArray )
            {
                if(GetSignature( ref dimArray ))
                {
                    var array = dimArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                    for(int i = 0; i < array.Length; i++)
                    {
                        Transform( ref array[i] );
                    }
                }
            }

            public override void Transform( ref Runtime.ActivationRecordEvents val )
            {
                ClearPending();

                val = (Runtime.ActivationRecordEvents)m_reader.ReadUInt32();
            }

            //
            // TransformationContextForIR
            //

            public override void Transform( ref ControlFlowGraphState cfg )
            {
                if(GetSignature( ref cfg ))
                {
                    ControlFlowGraphStateForCodeTransformation cfg2 = (ControlFlowGraphStateForCodeTransformation)cfg;

                    cfg2.ApplyTransformation( this );
                }
            }

            //--//

            public override void Transform( ref Operator op )
            {
                if(GetSignature( ref op ))
                {
                    op.ApplyTransformation( this );
                }
            }

            public override void Transform( ref Operator[] opArray )
            {
                if(GetSignature( ref opArray ))
                {
                    var array = opArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                    for(int i = 0; i < array.Length; i++)
                    {
                        Transform( ref array[i] );
                    }
                }
            }

            public override void Transform( ref Annotation an )
            {
                if(GetSignature( ref an ))
                {
                    an.ApplyTransformation( this );
                }
            }

            public override void Transform( ref Annotation[] anArray )
            {
                if(GetSignature( ref anArray ))
                {
                    var array = anArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                    for(int i = 0; i < array.Length; i++)
                    {
                        Transform( ref array[i] );
                    }
                }
            }

            //--//

            public override void Transform( ref Expression ex )
            {
                if(GetSignature( ref ex ))
                {
                    ex.ApplyTransformation( this );
                }
            }

            public override void Transform( ref ConstantExpression ex )
            {
                if(GetSignature( ref ex ))
                {
                    ex.ApplyTransformation( this );
                }
            }

            public override void Transform( ref VariableExpression ex )
            {
                if(GetSignature( ref ex ))
                {
                    ex.ApplyTransformation( this );
                }
            }

            public override void Transform( ref VariableExpression.DebugInfo val )
            {
                if(GetSignature( ref val ))
                {
                    val.ApplyTransformation( this );
                }
            }

            //--//

            public override void Transform( ref Expression[] exArray )
            {
                if(GetSignature( ref exArray ))
                {
                    var array = exArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                    for(int i = 0; i < array.Length; i++)
                    {
                        Transform( ref array[i] );
                    }
                }
            }

            public override void Transform( ref VariableExpression[] exArray )
            {
                if(GetSignature( ref exArray ))
                {
                    var array = exArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                    for(int i = 0; i < array.Length; i++)
                    {
                        Transform( ref array[i] );
                    }
                }
            }

            public override void Transform( ref List< ConstantExpression > exLst )
            {
                int count = GetListSize( ref exLst );

                var lst = exLst; // Get a local copy, because the original could change, due to the 'ref' argument.

                for(int i = 0; i < count; i++)
                {
                    ConstantExpression ex = null;

                    Transform( ref ex );

                    lst.Add( ex );
                }
            }

            //--//

            public override void Transform( ref BasicBlock bb )
            {
                if(GetSignature( ref bb ))
                {
                    bb.ApplyTransformation( this );
                }
            }

            public override void Transform( ref EntryBasicBlock bb )
            {
                if(GetSignature( ref bb ))
                {
                    bb.ApplyTransformation( this );
                }
            }

            public override void Transform( ref ExitBasicBlock bb )
            {
                if(GetSignature( ref bb ))
                {
                    bb.ApplyTransformation( this );
                }
            }
            
            public override void Transform( ref ExceptionHandlerBasicBlock bb )
            {
                if(GetSignature( ref bb ))
                {
                    bb.ApplyTransformation( this );
                }
            }

            public override void Transform( ref BasicBlock[] bbArray )
            {
                if(GetSignature( ref bbArray ))
                {
                    var array = bbArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                    for(int i = 0; i < array.Length; i++)
                    {
                        Transform( ref array[i] );
                    }
                }
            }

            public override void Transform( ref ExceptionHandlerBasicBlock[] bbArray )
            {
                if(GetSignature( ref bbArray ))
                {
                    var array = bbArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                    for(int i = 0; i < array.Length; i++)
                    {
                        Transform( ref array[i] );
                    }
                }
            }

            public override void Transform( ref BasicBlock.Qualifier val )
            {
                ClearPending();

                val = (BasicBlock.Qualifier)m_reader.ReadInt32();
            }

            //--//

            public override void Transform( ref ExceptionClause ec )
            {
                if(GetSignature( ref ec ))
                {
                    ec.ApplyTransformation( this );
                }
            }

            public override void Transform( ref ExceptionClause[] ecArray )
            {
                if(GetSignature( ref ecArray ))
                {
                    var array = ecArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                    for(int i = 0; i < array.Length; i++)
                    {
                        Transform( ref array[i] );
                    }
                }
            }

            public override void Transform( ref ExceptionClause.ExceptionFlag val )
            {
                ClearPending();

                val = (ExceptionClause.ExceptionFlag)m_reader.ReadInt32();
            }

            //--//

            public override void Transform( ref CompilationConstraints val )
            {
                ClearPending();

                val = (CompilationConstraints)m_reader.ReadInt32();
            }

            public override void Transform( ref CompilationConstraints[] ccArray )
            {
                if(GetSignature( ref ccArray ))
                {
                    var array = ccArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                    for(int i = 0; i < array.Length; i++)
                    {
                        Transform( ref array[i] );
                    }
                }
            }

            public override void Transform( ref Operator.OperatorCapabilities val )
            {
                ClearPending();

                val = (Operator.OperatorCapabilities)m_reader.ReadInt32();
            }

            public override void Transform( ref Operator.OperatorLevel val )
            {
                ClearPending();

                val = (Operator.OperatorLevel)m_reader.ReadInt32();
            }

            public override void Transform( ref BinaryOperator.ALU val )
            {
                ClearPending();

                val = (BinaryOperator.ALU)m_reader.ReadInt32();
            }

            public override void Transform( ref UnaryOperator.ALU val )
            {
                ClearPending();

                val = (UnaryOperator.ALU)m_reader.ReadInt32();
            }

            public override void Transform( ref CallOperator.CallKind val )
            {
                ClearPending();

                val = (CallOperator.CallKind)m_reader.ReadInt32();
            }

            public override void Transform( ref CompareAndSetOperator.ActionCondition val )
            {
                ClearPending();

                val = (CompareAndSetOperator.ActionCondition)m_reader.ReadInt32();
            }

            //
            // TransformationContextForCodeGeneration
            //

            public override void Transform( ref TypeSystemForCodeTransformation typeSystem )
            {
                if(GetSignature( ref typeSystem ))
                {
                    typeSystem.ApplyTransformation( this );
                }
            }

            //--//

            public override void Transform( ref StackLocationExpression.Placement val )
            {
                ClearPending();

                val = (StackLocationExpression.Placement)m_reader.ReadInt32();
            }

            public override void Transform( ref ConditionCodeExpression.Comparison val )
            {
                ClearPending();

                val = (ConditionCodeExpression.Comparison)m_reader.ReadByte();
            }

            public override void Transform( ref PiOperator.Relation val )
            {
                ClearPending();

                val = (PiOperator.Relation)m_reader.ReadInt32();
            }

            //--//

            public override void Transform( ref DataManager dataManager )
            {
                if(GetSignature( ref dataManager ))
                {
                    dataManager.ApplyTransformation( this );
                }
            }

            public override void Transform( ref DataManager.Attributes val )
            {
                ClearPending();

                val = (DataManager.Attributes)m_reader.ReadInt32();
            }

            public override void Transform( ref DataManager.ObjectDescriptor od )
            {
                if(GetSignature( ref od ))
                {
                    od.ApplyTransformation( this );
                }
            }

            public override void Transform( ref DataManager.ArrayDescriptor ad )
            {
                if(GetSignature( ref ad ))
                {
                    ad.ApplyTransformation( this );
                }
            }

            //--//

            public override void Transform( ref ImageBuilders.Core imageBuilder )
            {
                if(GetSignature( ref imageBuilder ))
                {
                    imageBuilder.ApplyTransformation( this );
                }
            }

            public override void Transform( ref ImageBuilders.CompilationState cs )
            {
                if(GetSignature( ref cs ))
                {
                    cs.ApplyTransformation( this );
                }
            }

            public override void Transform( ref ImageBuilders.SequentialRegion reg )
            {
                if(GetSignature( ref reg ))
                {
                    reg.ApplyTransformation( this );
                }
            }

            public override void Transform( ref ImageBuilders.ImageAnnotation an )
            {
                if(GetSignature( ref an ))
                {
                    an.ApplyTransformation( this );
                }
            }

            public override void Transform( ref ImageBuilders.CodeConstant cc )
            {
                if(GetSignature( ref cc ))
                {
                    cc.ApplyTransformation( this );
                }
            }

            public override void Transform( ref ImageBuilders.SequentialRegion[] regArray )
            {
                if(GetSignature( ref regArray ))
                {
                    var array = regArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                    for(int i = 0; i < array.Length; i++)
                    {
                        Transform( ref array[i] );
                    }
                }
            }

            public override void Transform( ref List< ImageBuilders.SequentialRegion > regLst )
            {
                int count = GetListSize( ref regLst );

                var lst = regLst; // Get a local copy, because the original could change, due to the 'ref' argument.

                for(int i = 0; i < count; i++)
                {
                    ImageBuilders.SequentialRegion reg = null;

                    Transform( ref reg );

                    lst.Add( reg );
                }
            }

            public override void Transform( ref List< ImageBuilders.ImageAnnotation > anLst )
            {
                int count = GetListSize( ref anLst );

                var lst = anLst; // Get a local copy, because the original could change, due to the 'ref' argument.

                for(int i = 0; i < count; i++)
                {
                    ImageBuilders.ImageAnnotation an = null;

                    Transform( ref an );

                    lst.Add( an );
                }
            }

            public override void Transform( ref List< ImageBuilders.CodeConstant > ccLst )
            {
                int count = GetListSize( ref ccLst );

                var lst = ccLst; // Get a local copy, because the original could change, due to the 'ref' argument.

                for(int i = 0; i < count; i++)
                {
                    ImageBuilders.CodeConstant cc = null;

                    Transform( ref cc );

                    lst.Add( cc );
                }
            }

            public override void Transform( ref List< Runtime.Memory.Range > mrLst )
            {
                int count = GetListSize( ref mrLst );

                var lst = mrLst; // Get a local copy, because the original could change, due to the 'ref' argument.

                for(int i = 0; i < count; i++)
                {
                    Runtime.Memory.Range mr = null;

                    TransformGeneric( ref mr );

                    lst.Add( mr );
                }
            }

            public override void Transform( ref Runtime.MemoryAttributes val )
            {
                ClearPending();

                val = (Runtime.MemoryAttributes)m_reader.ReadUInt32();
            }

            public override void Transform( ref Runtime.MemoryAttributes[] maArray )
            {
                if(GetSignature( ref maArray ))
                {
                    var array = maArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                    for(int i = 0; i < array.Length; i++)
                    {
                        Transform( ref array[i] );
                    }
                }
            }

            public override void Transform( ref Runtime.MemoryUsage val )
            {
                ClearPending();

                val = (Runtime.MemoryUsage)m_reader.ReadUInt32();
            }

            public override void Transform( ref Runtime.MemoryUsage[] muArray )
            {
                if(GetSignature( ref muArray ))
                {
                    var array = muArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                    for(int i = 0; i < array.Length; i++)
                    {
                        Transform( ref array[i] );
                    }
                }
            }

            public override void Transform( ref Abstractions.PlacementRequirements pr )
            {
                if(GetSignature( ref pr ))
                {
                    pr.ApplyTransformation( this );
                }
            }

            public override void Transform( ref Abstractions.RegisterDescriptor regDesc )
            {
                if(GetSignature( ref regDesc ))
                {
                    regDesc.ApplyTransformation( this );
                }
            }

            public override void Transform( ref Abstractions.RegisterDescriptor[] regDescArray )
            {
                if(GetSignature( ref regDescArray ))
                {
                    var array = regDescArray; // Get a local copy, because the original could change, due to the 'ref' argument.

                    for(int i = 0; i < array.Length; i++)
                    {
                        Transform( ref array[i] );
                    }
                }
            }

            public override void Transform( ref Abstractions.RegisterClass val )
            {
                ClearPending();

                val = (Abstractions.RegisterClass)m_reader.ReadUInt32();
            }

            public override void Transform( ref Abstractions.CallingConvention.Direction val )
            {
                ClearPending();

                val = (Abstractions.CallingConvention.Direction)m_reader.ReadUInt32();
            }

            //--//

            protected override void TransformArray( ref Array arrayIn )
            {
                if(GetSignature( ref arrayIn ))
                {
                    var array = arrayIn; // Get a local copy, because the original could change, due to the 'ref' argument.

                    for(int i = 0; i < array.Length; i++)
                    {
                        object obj = null;

                        Transform( ref obj );

                        array.SetValue( obj, i );
                    }
                }
            }

            //--//

            protected override GrowOnlyHashTable< Type, System.Reflection.MethodInfo > GetMethodInfoTable()
            {
                if(s_handlers == null)
                {
                    s_handlers = BuildMethodInfoTable();
                }

                return s_handlers;
            }

            protected override DynamicTransform GetDynamicTransform()
            {
                if(s_dynamicTransform == null)
                {
                    s_dynamicTransform = BuildDynamicTransform();
                }

                return s_dynamicTransform;
            }

            protected override object TransformThroughReflection( object obj )
            {
                if(GetSignature( ref obj, null ))
                {
                    TransformFields( obj, obj.GetType() );
                }

                return obj;
            }

#if TRANSFORMATIONCONTEXT__USE_EMIT
            public override bool IsReader
            {
                get { return true; }
            }
#endif
        }
    }
}
