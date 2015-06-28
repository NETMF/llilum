//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;


    [WellKnownType( "Microsoft_Zelig_Runtime_TypeSystem_VTable" )]
    public sealed class VTable
    {
        [AllowCompileTimeIntrospection]
        [Flags]
        public enum Shape : byte
        {
            Invalid    =                     0x00,
            Scalar     = ValueType |         0x01,
            Struct     = ValueType |         0x02,
            Interface  = Reference |         0x03,
            Class      = Reference |         0x04,
            ArrayRoot  = Reference | Array | 0x05,
            SzArray    = Reference | Array | 0x06,
            MultiArray = Reference | Array | 0x07,

            ValueType = 0x20,
            Reference = 0x40,
            Array     = 0x80,
        }

        public struct InterfaceMap
        {
            public static readonly InterfaceMap[] SharedEmptyArray = new InterfaceMap[0];

            //
            // State
            //

            public VTable        Interface;
            public CodePointer[] MethodPointers;
        }

        //
        // State
        //

        //
        // The two size fields, BaseSize and ElementSize, are only copy of the corresponding values from TypeInfo.
        // They are here for performance reasons (copying the values here removes one indirection).
        // During heap walking, we need to compute the size of a block, to skip over it.
        //
        // This is done treating each object as if it were an array and computing the quantity:
        //
        //          <size> = Vt.BaseSize + Vt.ElementSize * <array length>
        //
        // This works because arrays have an extra uint field at the start, holding the number of elements in the array.
        // However, instances of non-array types will have random data at the offset of the Length field.
        // This is not a problem, as long as we ensure that ElementSize is set to zero.
        // Whatever the value of array length, the multiplication by zero will make it irrelevant.
        //
        [WellKnownField( "VTable_BaseSize"       )] public uint               BaseSize;
        [WellKnownField( "VTable_ElementSize"    )] public uint               ElementSize;
        [WellKnownField( "VTable_TypeInfo"       )] public TypeRepresentation TypeInfo;
        [WellKnownField( "VTable_GCInfo"         )] public GCInfo             GCInfo;
        [WellKnownField( "VTable_Type"           )] public Type               Type;
        [WellKnownField( "VTable_ShapeCategory"  )] public Shape              ShapeCategory;

        //
        // TODO: We need to embed these pointers in the VTable object itself.
        // TODO: This way we can assume a fixed offset between MethodPointers and VTable and hardcode it in the method lookup code.
        //
        [WellKnownField( "VTable_MethodPointers" )] public CodePointer[]      MethodPointers;

        [WellKnownField( "VTable_InterfaceMap"   )] public InterfaceMap[]     InterfaceMethodPointers;

        //
        // Constructor Methods
        //

        public VTable( TypeRepresentation owner )
        {
            this.TypeInfo                = owner;
            this.InterfaceMethodPointers = InterfaceMap.SharedEmptyArray;
        }

        //--//

        //
        // Helper Methods
        //

        public static bool SameType( object a ,
                                     object b )
        {
            return Get( a ) == Get( b );
        }

        //--//

        public void ApplyTransformation( TransformationContext context )
        {
            context.Push( this );

            context.Transform( ref this.BaseSize       );
            context.Transform( ref this.ElementSize    );
            context.Transform( ref this.TypeInfo       );
            context.Transform( ref this.GCInfo         );
            context.Transform( ref this.MethodPointers );

            context.Pop();
        }

        //--//

        //
        // Access Methods
        //

        [WellKnownMethod( "VTable_Get" )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        public extern static VTable Get( object a );

        [WellKnownMethod( "VTable_GetInterface" )]
        public static CodePointer[] GetInterface( object a             ,
                                                  VTable vtblInterface )
        {
            VTable         vtbl  = Get( a );
            InterfaceMap[] array = vtbl.InterfaceMethodPointers;

            for(int i = 0; i < array.Length; i++)
            {
                if(Object.ReferenceEquals( array[i].Interface, vtblInterface ))
                {
                    return array[i].MethodPointers;
                }
            }

            return null;
        }


        [Inline]
        public static VTable GetFromType( Type t )
        {
            return GetFromTypeHandle( t.TypeHandle );
        }

        [MethodImpl( MethodImplOptions.InternalCall )]
        public extern static VTable GetFromTypeHandle( RuntimeTypeHandle hnd );

        //--//



        [Inline]
        public bool CanBeAssignedFrom( VTable target )
        {
            if(this == target)
            {
                return true;
            }

            return CanBeAssignedFrom_Slow( target );
        }

        [NoInline]
        private bool CanBeAssignedFrom_Slow( VTable source )
        {
            if(source.IsSubclassOf( this ))
            {
                return true;
            }

            if(this.IsArray && source.IsArray)
            {
                CHECKS.ASSERT( this.ShapeCategory == Shape.SzArray || this.ShapeCategory == Shape.MultiArray, "Found array that does not inherit from System.Array" );

                if(this.ShapeCategory == source.ShapeCategory)
                {
                    ArrayReferenceTypeRepresentation tdThis   = (ArrayReferenceTypeRepresentation)this  .TypeInfo;
                    ArrayReferenceTypeRepresentation tdSource = (ArrayReferenceTypeRepresentation)source.TypeInfo;

                    if(tdThis.SameShape( tdSource ))
                    {
                        TypeRepresentation subThis   = tdThis  .ContainedType.UnderlyingType;
                        TypeRepresentation subSource = tdSource.ContainedType.UnderlyingType;

                        VTable subVTableThis   = subThis  .VirtualTable;
                        VTable subVTableSource = subSource.VirtualTable;

                        if(subVTableThis == subVTableSource)
                        {
                            return true;
                        }

                        if(subVTableSource.IsValueType)
                        {
                            //
                            // We require exact matching for value types.
                            //
                            return false;
                        }

                        if(subVTableThis.IsInterface)
                        {
                            return subVTableSource.ImplementsInterface( subVTableThis );
                        }

                        return subVTableThis.CanBeAssignedFrom_Slow( subVTableSource );
                    }
                }
            }

            return false;
        }

        [NoInline]
        public bool IsSubclassOf( VTable target )
        {
            TypeRepresentation td = this.TypeInfo;
            while(td != null)
            {
                if(target == td.VirtualTable)
                {
                    return true;
                }

                td = td.Extends;
            }

            return false;
        }

        [NoInline]
        public bool ImplementsInterface( VTable expectedItf )
        {
            VTable.InterfaceMap[] itfs = this.InterfaceMethodPointers;

            for(int i = itfs.Length; --i >= 0; )
            {
                if(Object.ReferenceEquals( itfs[i].Interface, expectedItf ))
                {
                    return true;
                }
            }

            return false;
        }

        //
        // Access Methods
        //

        public bool IsArray
        {
            [Inline]
            get
            {
                return (this.ShapeCategory & Shape.Array) != 0;
            }
        }

        public bool IsValueType
        {
            [Inline]
            get
            {
                return (this.ShapeCategory & Shape.ValueType) != 0;
            }
        }

        public bool IsInterface
        {
            [Inline]
            get
            {
                return this.ShapeCategory == Shape.Interface;
            }
        }

        //
        // Debug Methods
        //

        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.AppendFormat( "VTable({0})", this.TypeInfo );

            return sb.ToString();
        }
    }
}
