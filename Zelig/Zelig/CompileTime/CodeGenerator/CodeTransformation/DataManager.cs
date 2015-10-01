//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

#if DEBUG
#define TRACK_DATADESCRIPTOR_IDENTITY
#else
//#define TRACK_DATADESCRIPTOR_IDENTITY
#endif

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public class DataManager
    {
        [Flags]
        public enum Attributes
        {
            Constant                       = 0x00000000,
            Mutable                        = 0x00000001,
            SuitableForConstantPropagation = 0x00000002,
        }

        public abstract class DataDescriptor
        {
            //
            // State
            //

#if TRACK_DATADESCRIPTOR_IDENTITY
            protected static int                         s_identity;
#endif
            public           int                         m_identity;

            //--//
                                                 
            protected DataManager                        m_owner;
            protected TypeRepresentation                 m_context;
            protected Attributes                         m_flags;
            protected Abstractions.PlacementRequirements m_placementRequirements;
                                                 
            protected DataDescriptor                     m_nestingDd;
            protected InstanceFieldRepresentation        m_nestingFd;
            protected int                                m_nestingPos;


            //
            // Constructor Methods
            //

            protected DataDescriptor() // Default constructor required by TypeSystemSerializer.
            {
            }

            protected DataDescriptor( DataManager                        owner   ,
                                      TypeRepresentation                 context ,
                                      Attributes                         flags   ,
                                      Abstractions.PlacementRequirements pr      )
            {
#if TRACK_DATADESCRIPTOR_IDENTITY
                m_identity              = s_identity++;
#endif

                m_owner                 = owner;
                m_context               = context;
                m_flags                 = flags;
                m_placementRequirements = pr;
            }

            //
            // Helper Methods
            //

            internal void SetNesting( DataDescriptor              nestingDd  ,
                                      InstanceFieldRepresentation nestingFd  ,
                                      int                         nestingPos )
            {
                m_nestingDd  = nestingDd;
                m_nestingFd  = nestingFd;
                m_nestingPos = nestingPos;
            }

            protected void ApplyTransformationInner( TransformationContextForCodeTransformation context )
            {
                context.Transform       ( ref m_owner                 );
                context.Transform       ( ref m_context               );
                context.Transform       ( ref m_flags                 );
                context.Transform       ( ref m_placementRequirements );
                                                               
                context.TransformGeneric( ref m_nestingDd             );
                context.Transform       ( ref m_nestingFd             );
                context.Transform       ( ref m_nestingPos            );
            }

            //--//

            internal virtual void IncludeExtraTypes( TypeSystem.Reachability      reachability ,
                                                     CompilationSteps.PhaseDriver phase        )
            {
                RefreshValues( phase );

                reachability.ExpandPending( m_context );

                if(m_context is ReferenceTypeRepresentation)
                {
                    object valVt = m_owner.GetObjectDescriptor( m_context.VirtualTable );

                    reachability.ExpandPending( valVt );
                }
            }

            internal abstract void Reduce( GrowOnlySet< DataDescriptor > visited      ,
                                           TypeSystem.Reachability       reachability ,
                                           bool                          fApply       );

            internal abstract void RefreshValues( CompilationSteps.PhaseDriver phase );

            internal abstract void Write( ImageBuilders.SequentialRegion region );

            public abstract object GetDataAtOffset( FieldRepresentation[] accessPath      ,
                                                    int                   accessPathIndex ,
                                                    int                   offset          );

            //--//

            protected void WriteHeader( ImageBuilders.SequentialRegion region  ,
                                        TypeRepresentation             context )
            {
                if(context is ValueTypeRepresentation)
                {
                    //
                    // Value types don't have headers.
                    //
                    return;
                }

                WellKnownTypes     wkt      = m_owner.m_typeSystem.WellKnownTypes;
                TypeRepresentation tdHeader = wkt.Microsoft_Zelig_Runtime_ObjectHeader;
                if(tdHeader != null)
                {
                    WellKnownFields                        wkf = m_owner.m_typeSystem.WellKnownFields;
                    ImageBuilders.SequentialRegion.Section sec = region.GetSectionOfFixedSize( tdHeader.Size );
                    FieldRepresentation                    fd;

                    region.PointerOffset = region.Position;

                    fd = wkf.ObjectHeader_VirtualTable;
                    if(fd != null)
                    {
                        DataManager.DataDescriptor vTable = (DataManager.DataDescriptor)m_owner.GetObjectDescriptor( context.VirtualTable );

                        sec.Offset = (uint)fd.Offset;

                        sec.AddImageAnnotation( fd.FieldType.SizeOfHoldingVariable, fd );

                        sec.WritePointerToDataDescriptor( vTable );
                    }

                    fd = wkf.ObjectHeader_MultiUseWord;
                    if(fd != null)
                    {
                        sec.Offset = (uint)fd.Offset;

                        sec.AddImageAnnotation( fd.FieldType.SizeOfHoldingVariable, fd );

                        Runtime.ObjectHeader.GarbageCollectorFlags flags;

                        if(this.IsMutable)
                        {
                            flags = Runtime.ObjectHeader.GarbageCollectorFlags.UnreclaimableObject;
                        }
                        else
                        {
                            flags = Runtime.ObjectHeader.GarbageCollectorFlags.ReadOnlyObject;
                        }

                        sec.Write( (uint)flags );
                    }
                }
            }

            //
            // Access Methods
            //

            public TypeRepresentation Context
            {
                get
                {
                    return m_context;
                }
            }

            public DataManager Owner
            {
                get
                {
                    return m_owner;
                }
            }
            
            public bool IsMutable
            {
                get
                {
                    return (m_flags & Attributes.Mutable) != 0;
                }
            }

            public bool CanPropagate
            {
                get
                {
                    return (m_flags & Attributes.SuitableForConstantPropagation) != 0;
                }
            }

            public DataDescriptor Nesting
            {
                get
                {
                    return m_nestingDd;
                }
            }

            public InstanceFieldRepresentation NestingField
            {
                get
                {
                    return m_nestingFd;
                }
            }

            public int NestingIndex
            {
                get
                {
                    return m_nestingPos;
                }
            }

            public Abstractions.PlacementRequirements PlacementRequirements 
            {
                get
                {
                    return m_placementRequirements;
                }

                set
                {
                    m_placementRequirements = value;
                }
            }

            //
            // Debug Methods
            //

            public override string ToString()
            {
                return ToString( false );
            }

            internal string ToStringVerbose()
            {
                return ToString( true );
            }

            protected abstract string ToString( bool fVerbose );
        }

        //--//--//--//--//--//--//--//--//--//

        public class ObjectDescriptor : DataDescriptor
        {
            //
            // State
            //

            private object                                                   m_source;
            private GrowOnlyHashTable< InstanceFieldRepresentation, object > m_values;

            //
            // Constructor Methods
            //

            private ObjectDescriptor() // Default constructor required by TypeSystemSerializer.
            {
                m_values = HashTableFactory.New< InstanceFieldRepresentation, object >();
            }
            
            internal ObjectDescriptor( DataManager                        owner   ,
                                       TypeRepresentation                 context ,
                                       Attributes                         flags   ,
                                       Abstractions.PlacementRequirements pr      ,
                                       object                             source  ) : base( owner, context, flags, pr )
            {
                m_source = source;
                m_values = HashTableFactory.New< InstanceFieldRepresentation, object >();
            }

            //
            // Helper Methods
            //

            public void ApplyTransformation( TransformationContextForCodeTransformation context )
            {
                context.Push( this );

                ApplyTransformationInner( context );

                context.Transform( ref m_source );
                context.Transform( ref m_values );

                context.Pop();
            }

            //--//

            internal override void IncludeExtraTypes( TypeSystem.Reachability      reachability ,
                                                      CompilationSteps.PhaseDriver phase        )
            {
                base.IncludeExtraTypes( reachability, phase );

                if(m_source != null)
                {
                    reachability.ExpandPending( m_source );
                }

                foreach(InstanceFieldRepresentation fd in m_values.Keys)
                {
                    object val = m_values[fd];

                    if(val != null && reachability.Contains( fd ))
                    {
                        if(val is DataDescriptor)
                        {
                            reachability.ExpandPending( val );
                        }
                    }
                }
            }

            internal override void Reduce( GrowOnlySet< DataDescriptor > visited      ,
                                           TypeSystem.Reachability       reachability ,
                                           bool                          fApply       )
            {
                if(visited.Insert( this ) == false)
                {
                    CHECKS.ASSERT( reachability.Contains( m_context ), "The type of {0} is not included in the globalReachabilitySet: {1}", this, m_context );

                    GrowOnlyHashTable< InstanceFieldRepresentation, object > valuesNew = m_values.CloneSettings();

                    foreach(InstanceFieldRepresentation fd in m_values.Keys)
                    {
                        if(reachability.Contains( fd ))
                        {
                            object val = m_values[fd];

                            if(val == null || reachability.IsProhibited( val ) == false)
                            {
                                valuesNew[fd] = val;

                                if(val is DataDescriptor)
                                {
                                    DataDescriptor dd = (DataDescriptor)val;

                                    dd.Reduce( visited, reachability, fApply );
                                }
                            }
                        }
                    }

                    if(fApply)
                    {
                        m_values = valuesNew;
                    }
                }
            }

            //--//

            internal void UpdateSource( object                       source ,
                                        CompilationSteps.PhaseDriver phase  )
            {
                m_source = source;

                RefreshValues( phase );
            }

            internal override void RefreshValues( CompilationSteps.PhaseDriver phase )
            {
                if(m_source != null)
                {
                    const System.Reflection.BindingFlags bindingFlags = System.Reflection.BindingFlags.DeclaredOnly |
                                                                        System.Reflection.BindingFlags.Public       |
                                                                        System.Reflection.BindingFlags.NonPublic    |
                                                                        System.Reflection.BindingFlags.Instance;

                    Type                            typeSrc                = m_source.GetType();
                    TypeRepresentation              typeDst                = m_context;
                    TypeSystemForCodeTransformation typeSystem             = m_owner.m_typeSystem;
                    WellKnownTypes                  wkt                    = typeSystem.WellKnownTypes;
                    WellKnownFields                 wkf                    = typeSystem.WellKnownFields;
                    FieldRepresentation             fdStringImpl_FirstChar = wkf.StringImpl_FirstChar;
                    FieldRepresentation             fdVTable_Type          = wkf.VTable_Type;


                    GrowOnlyHashTable< InstanceFieldRepresentation, object > values = m_values;

                    m_values = values.CloneSettingsAndSize();

                    while(typeDst != null)
                    {
                        foreach(FieldRepresentation fdIn in typeDst.Fields)
                        {
                            InstanceFieldRepresentation fd = fdIn as InstanceFieldRepresentation;

                            if(fd != null)
                            {
                                object valOld;

                                values.TryGetValue( fd, out valOld );

                                if(fd == fdStringImpl_FirstChar)
                                {
                                    //
                                    // Special case for strings: store the whole content as an array of chars.
                                    //
                                    Set( fd, ((string)m_source).ToCharArray() );
                                }
                                else if(fd == wkf.StringImpl_ArrayLength)
                                {
                                    Set(fd, ((string)m_source).ToCharArray().Length);
                                }
                                else if(fd == wkf.StringImpl_StringLength)
                                {
                                    Set(fd, ((string)m_source).Length);
                                }
                                else if(fd == fdVTable_Type)
                                {
                                    //
                                    // Special case for virtual tables: create a TypeImpl on the fly.
                                    //
                                    if(valOld == null)
                                    {
                                        TypeRepresentation valTd = wkt.Microsoft_Zelig_Runtime_RuntimeTypeImpl;
                                        if(valTd != null)
                                        {
                                            ObjectDescriptor od = m_owner.BuildObjectDescriptor( valTd, Attributes.Constant | Attributes.SuitableForConstantPropagation, null );

                                            InstanceFieldRepresentation fdTarget = (InstanceFieldRepresentation)wkf.RuntimeTypeImpl_m_handle;
                                            VTable                      vTable   = (VTable)m_source;
                                            ObjectDescriptor            odSub    = typeSystem.CreateDescriptorForRuntimeHandle( vTable.TypeInfo );

                                            odSub.SetNesting( od, fdTarget, -1 );

                                            odSub.RefreshValues( phase );

                                            od.Set( fdTarget, odSub );

                                            valOld = od;
                                        }
                                    }

                                    Set( fd, valOld );
                                }
                                else
                                {
                                    System.Reflection.FieldInfo fi = typeSrc.GetField( fd.Name, bindingFlags );
                                    if(fi != null)
                                    {
                                        object           val   = fi.GetValue( m_source );
                                        ObjectDescriptor oldOD = valOld as ObjectDescriptor;

                                        if(fi.FieldType.IsValueType && oldOD != null)
                                        {
                                            oldOD.UpdateSource( val, phase );

                                            Set( fd, oldOD );
                                        }
                                        else
                                        {
                                            Set( fd, m_owner.ConvertToObjectDescriptor( fd.FieldType, m_flags, m_placementRequirements, val, this, fd, -1, phase ) );
                                        }
                                    }
                                    else
                                    {
                                        throw TypeConsistencyErrorException.Create( "Cannot create ObjectDescriptor, field '{0}' is missing in source object", fd );
                                    }
                                }
                            }
                        }

                        typeSrc = typeSrc.BaseType;
                        typeDst = typeDst.Extends;
                    }
                }
            }

            //--//

            internal override void Write( ImageBuilders.SequentialRegion region )
            {
                WriteHeader( region, m_context );

                string                                 text        = m_source as string;
                int                                    arrayLength = text != null ? (text.Length+1) : 0;
                VTable                                 vtable      = m_context.VirtualTable;
                ImageBuilders.SequentialRegion.Section sec         = region.GetSectionOfFixedSize( vtable.BaseSize + vtable.ElementSize * (uint)arrayLength );

                WriteFields( sec, vtable );
            }

            internal void WriteFields( ImageBuilders.SequentialRegion.Section sec    ,
                                       VTable                                 vtable )
            {
                WellKnownFields wkf = m_owner.m_typeSystem.WellKnownFields;

                FieldRepresentation fdFirstChar          = wkf.StringImpl_FirstChar;
                FieldRepresentation fdCodePointer_Target = wkf.CodePointer_Target;

                foreach(InstanceFieldRepresentation fd in m_values.Keys)
                {
                    object val = m_values[fd];

                    sec.Offset = (uint)fd.Offset;

                    sec.AddImageAnnotation( fd.FieldType.SizeOfHoldingVariable, fd );

                    if(fd == fdFirstChar)
                    {
                        //
                        // Special case for strings: store the whole content as an array of chars.
                        //
                        sec.Write( (char[])val );
                    }
                    else if(fd == fdCodePointer_Target)
                    {
                        //
                        // Special case for code pointers: substitute with actual code pointers.
                        //
                        IntPtr id = (IntPtr)val;

                        object ptr = m_owner.GetCodePointerFromUniqueID( id );

                        if(ptr is MethodRepresentation)
                        {
                            MethodRepresentation                       md  = (MethodRepresentation)ptr;
                            ControlFlowGraphStateForCodeTransformation cfg = TypeSystemForCodeTransformation.GetCodeForMethod( md );

                            if(cfg == null)
                            {
                                sec.WriteNullPointer();
                            }
                            else
                            {
                                sec.WritePointerToBasicBlock( cfg.EntryBasicBlock );
                            }
                        }
                        else if(ptr is ExceptionHandlerBasicBlock)
                        {
                            ExceptionHandlerBasicBlock ehBB = (ExceptionHandlerBasicBlock)ptr;

                            sec.WritePointerToBasicBlock( ehBB );
                        }
                        else
                        {
                            sec.Write( id );
                        }
                    }
                    else if(val == null)
                    {
                        sec.WriteNullPointer();
                    }
                    else if(val is DataDescriptor)
                    {
                        DataDescriptor dd = (DataDescriptor)val;

                        if(dd.Nesting != null)
                        {
                            ObjectDescriptor od = (ObjectDescriptor)dd;

                            od.WriteFields( sec.GetSubSection( dd.Context.Size ), dd.Context.VirtualTable );
                        }
                        else
                        {
                            sec.WritePointerToDataDescriptor( dd );
                        }
                    }
                    else if(fd.FieldType is ScalarTypeRepresentation)
                    {
                        if(sec.WriteGeneric( val ) == false)
                        {
                            throw TypeConsistencyErrorException.Create( "Can't write scalar value {0}", val );
                        }
                    }
                    else
                    {
                        throw TypeConsistencyErrorException.Create( "Don't know how to write {0}", val );
                    }
                }
            }

            //--//

            public override object GetDataAtOffset( FieldRepresentation[] accessPath      ,
                                                    int                   accessPathIndex ,
                                                    int                   offset          )
            {
                if(!this.CanPropagate)
                {
                    return null;
                }

                lock(TypeSystemForCodeTransformation.Lock)
                {
                    if(accessPath != null && accessPathIndex < accessPath.Length)
                    {
                        var    fd  = (InstanceFieldRepresentation)accessPath[accessPathIndex++];
                        object val;

                        if(m_values.TryGetValue( fd, out val ) == false)
                        {
                            return null;
                        }

                        offset -= fd.Offset;

                        if(accessPathIndex == accessPath.Length)
                        {
                            return val;
                        }

                        var dd = val as DataDescriptor;
                        if(dd == null)
                        {
                            return null;
                        }

                        return dd.GetDataAtOffset( accessPath, accessPathIndex, offset );
                    }

                    foreach(InstanceFieldRepresentation fd in m_values.Keys)
                    {
                        if(fd.Offset == offset)
                        {
                            return m_values[fd];
                        }
                    }

                    return null;
                }
            }

            //--//

            public void ConvertAndSet( InstanceFieldRepresentation        fd    ,
                                       Attributes                         flags ,
                                       Abstractions.PlacementRequirements pr    ,
                                       object                             val   )
            {
                lock(TypeSystemForCodeTransformation.Lock)
                {
                    Set( fd, m_owner.ConvertToObjectDescriptor( fd.FieldType, flags, pr, val ) );
                }
            }

            public void Set( InstanceFieldRepresentation fd  ,
                             object                      val )
            {
                lock(TypeSystemForCodeTransformation.Lock)
                {
                    m_values[fd] = val;
                }
            }

            public object Get( InstanceFieldRepresentation fd )
            {
                lock(TypeSystemForCodeTransformation.Lock)
                {
                    object res;

                    m_values.TryGetValue( fd, out res );

                    return res;
                }
            }

            public bool Has( InstanceFieldRepresentation fd )
            {
                lock(TypeSystemForCodeTransformation.Lock)
                {
                    return m_values.ContainsKey( fd );
                }
            }

            //--//

            //
            // Access Methods
            //

            public object Source
            {
                get
                {
                    return m_source;
                }
            }

            public GrowOnlyHashTable< InstanceFieldRepresentation, object > Values
            {
                get
                {
                    return m_values;
                }
            }

            //
            // Debug Methods
            //

            protected override string ToString( bool fVerbose )
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                sb.AppendFormat( "$Object({0}", m_context.FullName );

                if(m_source != null)
                {
                    sb.AppendFormat( " => {0}", m_source );
                }

                if(fVerbose)
                {
                    bool fFirst = true;

                    foreach(InstanceFieldRepresentation fd in m_values.Keys)
                    {
                        object val = m_values[fd];

                        if(val != null)
                        {
                            if(fFirst)
                            {
                                sb.Append( " -> " );
                                fFirst = false;
                            }
                            else
                            {
                                sb.Append( ", " );
                            }

                            if(val is DataDescriptor)
                            {
                                DataDescriptor dd = (DataDescriptor)val;

                                val = dd.Context;
                            }

                            sb.AppendFormat( "{0}::{1} = {2}", fd.OwnerType.FullNameWithAbbreviation, fd.Name, val );
                        }
                    }
                }

                sb.Append( ")" );

                if(m_nestingDd != null)
                {
                    sb.AppendFormat( " => {0}", fVerbose ? m_nestingDd.ToStringVerbose() : m_nestingDd.ToString() );
                }

                return sb.ToString();
            }
        }

        //--//--//--//--//--//--//--//--//--//

        public class ArrayDescriptor : DataDescriptor
        {
            //
            // State
            //

            private Array    m_source;
            private int      m_length;
            private object[] m_values;

            //
            // Constructor Methods
            //

            internal ArrayDescriptor( DataManager                        owner   ,
                                      ArrayReferenceTypeRepresentation   context ,
                                      Attributes                         flags   ,
                                      Abstractions.PlacementRequirements pr      ,
                                      Array                              source  ,
                                      int                                len     ) : base( owner, context, flags, pr )
            {
                m_source = source;
                m_length = len;

                if(source == null || source.GetType().GetElementType().IsPrimitive == false)
                {
                    m_values = new object[len];
                }
            }

            //
            // Helper Methods
            //

            public void ApplyTransformation( TransformationContextForCodeTransformation context )
            {
                context.Push( this );

                ApplyTransformationInner( context );

                context.TransformGeneric( ref m_source );
                context.Transform       ( ref m_values );

                context.Pop();
            }

            //--//

            internal override void IncludeExtraTypes( TypeSystem.Reachability      reachability ,
                                                      CompilationSteps.PhaseDriver phase        )
            {
                base.IncludeExtraTypes( reachability, phase );

                if(m_values != null)
                {
                    foreach(object obj in m_values)
                    {
                        if(obj is DataDescriptor)
                        {
                            DataDescriptor dd = (DataDescriptor)obj;

                            if(reachability.Contains( dd.Context ))
                            {
                                reachability.ExpandPending( obj );
                            }
                        }
                    }
                }
            }

            internal override void Reduce( GrowOnlySet< DataDescriptor > visited      ,
                                           TypeSystem.Reachability       reachability ,
                                           bool                          fApply       )
            {
                if(visited.Insert( this ) == false)
                {
                    CHECKS.ASSERT( reachability.Contains( m_context ), "The type of {0} is not included in the globalReachabilitySet: {1}", this, m_context );

                    if(m_values != null)
                    {
                        foreach(object val in m_values)
                        {
                            if(val is DataDescriptor)
                            {
                                DataDescriptor dd = (DataDescriptor)val;

                                dd.Reduce( visited, reachability, fApply );
                            }
                        }
                    }
                }
            }

            //--//

            internal override void RefreshValues( CompilationSteps.PhaseDriver phase )
            {
                if(m_source != null && m_values != null)
                {
                    TypeRepresentation typeDst = m_context.UnderlyingType.ContainedType;

                    for(int i = 0; i < m_length; i++)
                    {
                        object           val   = m_source.GetValue( i );
                        ObjectDescriptor oldOD = Get( i ) as ObjectDescriptor;

                        if(typeDst is ValueTypeRepresentation && oldOD != null)
                        {
                            oldOD.UpdateSource( val, phase );
                        }
                        else
                        {
                            Set( i, m_owner.ConvertToObjectDescriptor( typeDst, m_flags, m_placementRequirements, val, this, null, i, phase ) );
                        }
                    }
                }
            }

            //--//

            internal override void Write( ImageBuilders.SequentialRegion region )
            {
                WriteHeader( region, m_context );

                uint                                   arrayLength = (uint)m_length;
                VTable                                 vtable      = m_context.VirtualTable;
                ImageBuilders.SequentialRegion.Section sec         = region.GetSectionOfFixedSize( vtable.BaseSize + vtable.ElementSize * arrayLength );

                FieldRepresentation fd = m_owner.m_typeSystem.WellKnownFields.ArrayImpl_m_numElements;

                if(fd != null)
                {
                    sec.Offset = (uint)fd.Offset;

                    sec.AddImageAnnotation( fd.FieldType.SizeOfHoldingVariable, fd );

                    sec.Write( arrayLength );
                }

                if(region.PlacementRequirements.ContentsUninitialized)
                {
                    region.PayloadCutoff = sec.Position;
                }
                else
                {
                    //
                    // Special case for common scalar arrays.
                    //
                    if(m_values == null)
                    {
                        if(m_source is byte[])
                        {
                            sec.Write( (byte[])m_source );
                            return;
                        }

                        if(m_source is char[])
                        {
                            sec.Write( (char[])m_source );
                            return;
                        }

                        if(m_source is int[])
                        {
                            sec.Write( (int[])m_source );
                            return;
                        }

                        if(m_source is uint[])
                        {
                            sec.Write( (uint[])m_source );
                            return;
                        }
                    }

                    for(uint i = 0; i < arrayLength; i++)
                    {
                        object val = GetDirect( (int)i );

                        sec.Offset = vtable.BaseSize + vtable.ElementSize * i;

                        if(val == null)
                        {
                            sec.WriteNullPointer();
                        }
                        else if(val is DataDescriptor)
                        {
                            DataDescriptor dd = (DataDescriptor)val;

                            if(dd.Nesting != null)
                            {
                                ObjectDescriptor od = (ObjectDescriptor)dd;

                                od.WriteFields( sec.GetSubSection( vtable.ElementSize ), dd.Context.VirtualTable );
                            }
                            else
                            {
                                sec.WritePointerToDataDescriptor( dd );
                            }
                        }
                        else if(m_context.ContainedType is ScalarTypeRepresentation)
                        {
                            if(sec.WriteGeneric( val ) == false)
                            {
                                throw TypeConsistencyErrorException.Create( "Can't write scalar value {0}", val );
                            }
                        }
                        else
                        {
                            throw TypeConsistencyErrorException.Create( "Don't know how to write {0}", val );
                        }
                    }
                }
            }

            //--//

            public override object GetDataAtOffset( FieldRepresentation[] accessPath      ,
                                                    int                   accessPathIndex ,
                                                    int                   offset          )
            {
                if(!this.CanPropagate)
                {
                    return null;
                }

                lock(TypeSystemForCodeTransformation.Lock)
                {
                    uint arrayLength = (uint)m_length;

                    if(offset == 0)
                    {
                        return arrayLength;
                    }
                    else
                    {
                        VTable vtable = m_context.VirtualTable;

                        offset -= (int)vtable.BaseSize;
                        offset /= (int)vtable.ElementSize;

                        if(offset >= 0 && offset < m_length)
                        {
                            return GetDirect( offset );
                        }

                        return null;
                    }
                }
            }

            //--//

            public void Set( int    pos ,
                             object val )
            {
                lock(TypeSystemForCodeTransformation.Lock)
                {
                    m_values[pos] = val;
                }
            }

            public object Get( int pos )
            {
                lock(TypeSystemForCodeTransformation.Lock)
                {
                    return GetDirect( pos );
                }
            }

            private object GetDirect( int pos )
            {
                return m_values != null ? m_values[pos] : m_source.GetValue( pos );
            }

            //--//

            //
            // Access Methods
            //

            public Array Source
            {
                get
                {
                    return m_source;
                }
            }

            //ZELIG2LLVM: Added Length public getter
            public int Length
            {
                get
                {
                    return m_length;
                }
            }

            public object[] Values
            {
                get
                {
                    return m_values;
                }
            }

            //--//

            //
            // Debug Methods
            //

            protected override string ToString( bool fVerbose )
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                sb.AppendFormat( "$Array({0}", m_context.FullName );

                if(m_source != null)
                {
                    sb.AppendFormat( " => {0}", m_source );
                }

                if(fVerbose)
                {
                    sb.Append( " -> [" );

                    for(int i = 0; i < m_length; i++)
                    {
                        object val = GetDirect( i );

                        if(val != null)
                        {
                            if(i != 0)
                            {
                                sb.Append( ", " );
                            }

                            if(val is DataDescriptor)
                            {
                                DataDescriptor dd = (DataDescriptor)val;

                                val = dd.Context;
                            }

                            sb.AppendFormat( "{0} = {1}", i, val );
                        }
                    }

                    sb.Append( "]" );
                }

                sb.Append( ")" );

                if(m_nestingDd != null)
                {
                    sb.AppendFormat( " => {0}", fVerbose ? m_nestingDd.ToStringVerbose() : m_nestingDd.ToString() );
                }

                return sb.ToString();
            }
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        //
        // State
        //

        TypeSystemForCodeTransformation             m_typeSystem;
        GrowOnlyHashTable< object, DataDescriptor > m_data;
        GrowOnlyHashTable< object, int            > m_codePointers;
        GrowOnlyHashTable< int   , object         > m_codePointersReverse; // Not persisted, rebuilt if needed.
        int                                         m_nextCodePointerId;

        //
        // Constructor Methods
        //

        private DataManager() // Default constructor required by TypeSystemSerializer.
        {
            m_data         = HashTableFactory.NewWithWeakEquality     < object, DataDescriptor >();
            m_codePointers = HashTableFactory.NewWithReferenceEquality< object, int            >();
        }

        internal DataManager( TypeSystemForCodeTransformation typeSystem ) : this()
        {
            m_typeSystem = typeSystem;
        }

        //
        // Helper Methods
        //

        public void ApplyTransformation( TransformationContextForCodeTransformation context )
        {
            context.Push( this );

            context.Transform( ref m_typeSystem        );
            context.Transform( ref m_data              );
            context.Transform( ref m_codePointers      );
            context.Transform( ref m_nextCodePointerId );

            context.Pop();
        }

        //--//

        internal void Reduce( TypeSystem.Reachability reachability ,
                              bool                    fApply       )
        {
            GrowOnlyHashTable< object, DataDescriptor > dataNew = m_data.CloneSettings();
            GrowOnlySet< DataDescriptor >               visited = SetFactory.NewWithReferenceEquality< DataDescriptor >();

            foreach(object obj in m_data.Keys)
            {
                DataDescriptor dd = m_data[obj];

                if(reachability.IsProhibited( obj ) == false)
                {
                    if(reachability.Contains( dd ))
                    {
                        dd.Reduce( visited, reachability, fApply );

                        dataNew[obj] = dd;

                        dd = null;
                    }
                }

                if(dd != null)
                {
                    CHECKS.ASSERT( reachability.Contains( dd ) == false, "{0} cannot belong both to globalReachabilitySet and useProhibited", dd );

                    reachability.ExpandProhibition( dd );
                }
            }

            if(fApply)
            {
                m_data = dataNew;
            }

            //--//

            if(fApply)
            {
                GrowOnlyHashTable< object, int > codePointers = m_codePointers.CloneSettings();

                foreach(object obj in m_codePointers.Keys)
                {
                    if(reachability.IsProhibited( obj ) == false)
                    {
                        codePointers[obj] = m_codePointers[obj];
                    }
                }

                m_codePointers        = codePointers;
                m_codePointersReverse = null;
            }
        }

        //--//

        public object GetObjectDescriptor( object obj )
        {
            if(obj == null)
            {
                return null;
            }
            else if(obj is DataDescriptor)
            {
                return (DataDescriptor)obj;
            }
            else if(obj.GetType().IsPrimitive)
            {
                return obj;
            }
            else if(obj is Enum)
            {
                return obj;
            }
            else
            {
                lock(TypeSystemForCodeTransformation.Lock) // It's called from multiple threads during parallel phase executions.
                {
                    DataDescriptor res;

                    if(m_data.TryGetValue( obj, out res ))
                    {
                        return res;
                    }
                }

                return null;
            }
        }

        internal object ConvertToObjectDescriptor(     object             value    ,
                                                   out TypeRepresentation tdTarget )
        {
            tdTarget = m_typeSystem.GetTypeRepresentationFromObject( value );

            return ConvertToObjectDescriptor( tdTarget, DataManager.Attributes.Constant | DataManager.Attributes.SuitableForConstantPropagation, null, value );
        }

        internal object ConvertToObjectDescriptor( TypeRepresentation                 td    ,
                                                   Attributes                         flags ,
                                                   Abstractions.PlacementRequirements pr    ,
                                                   object                             obj   )
        {
            return ConvertToObjectDescriptor( td, flags, pr, obj, null, null, 0, null );
        }

        private object ConvertToObjectDescriptor( TypeRepresentation                 td         ,
                                                  Attributes                         flags      ,
                                                  Abstractions.PlacementRequirements pr         ,
                                                  object                             obj        ,
                                                  DataDescriptor                     nestingDd  ,
                                                  InstanceFieldRepresentation        nestingFd  ,
                                                  int                                nestingPos ,
                                                  CompilationSteps.PhaseDriver       phase      )
        {
            if(obj == null)
            {
                return null;
            }
            else if(obj is DataDescriptor)
            {
                return (DataDescriptor)obj;
            }
            else if(obj.GetType().IsPrimitive)
            {
                return obj;
            }
            else if(obj is Enum)
            {
                return obj;
            }
            else
            {
                TypeRepresentation tdObj = m_typeSystem.GetTypeRepresentationFromType( obj.GetType() );

                if(td.CanBeAssignedFrom( tdObj, null ) == false)
                {
                    throw TypeConsistencyErrorException.Create( "Cannot create a DataDescriptor of type {0} with a value of type {1}", td, tdObj );
                }

                lock(TypeSystemForCodeTransformation.Lock) // It's called from multiple threads during parallel phase executions.
                {
                    DataDescriptor res;

                    if(obj is ValueType || m_data.TryGetValue( obj, out res ) == false)
                    {
                        if(nestingFd != null)
                        {
                            bool            fSkip = false;
                            WellKnownFields wkf   = m_typeSystem.WellKnownFields;

                            if(CompilationSteps.PhaseDriver.CompareOrder( phase, typeof(CompilationSteps.Phases.ReduceTypeSystem) ) <= 0)
                            {
                                if(nestingFd == wkf.TypeRepresentation_MethodTable           || 
                                   nestingFd == wkf.TypeRepresentation_InterfaceMethodTables || 
                                   nestingFd == wkf.VTable_MethodPointers                    ||
                                   nestingFd == wkf.VTable_InterfaceMap                       )
                                {
                                    fSkip = true;
                                }
                            }

                            // Note that we only want to skip GCInfo before the LayoutTypes phase, but not in LayoutType phase,
                            // so GCInfo will be corrected during the call to RefreshValues as part of the LayoutTypes phase.
                            if (CompilationSteps.PhaseDriver.CompareOrder( phase, typeof(CompilationSteps.Phases.LayoutTypes) ) < 0)
                            {
                                if(nestingFd == wkf.VTable_GCInfo)
                                {
                                    fSkip = true;
                                }
                            }

                            if(fSkip)
                            {
                                //
                                // Don't generate a DataDescriptor for these items, they will be regenerated later.
                                //
                                return null;
                            }
                        }

                        if(obj is Array)
                        {
                            Array array = (Array)obj;

                            res = new ArrayDescriptor( this, (ArrayReferenceTypeRepresentation)tdObj, flags, pr, array, array.Length );
                        }
                        else
                        {
                            res = new ObjectDescriptor( this, tdObj, flags, pr, obj );
                        }

                        if(obj is ValueType)
                        {
                            res.SetNesting( nestingDd, nestingFd, nestingPos );

                            res.RefreshValues( phase );
                        }
                        else
                        {
                            m_data[obj] = res;
                        }

                        InstantiateVTable( res );
                    }

                    return res;
                }
            }
        }

        //--//

        private object ConvertVirtualTable( VTable vTable )
        {
            TypeRepresentation tdVT = m_typeSystem.GetTypeRepresentationFromObject( vTable );

            return ConvertToObjectDescriptor( tdVT, DataManager.Attributes.Constant | DataManager.Attributes.SuitableForConstantPropagation, null, vTable );
        }

        private void InstantiateVTable( DataDescriptor dd )
        {
            TypeRepresentation td = dd.Context;

            if(td is ReferenceTypeRepresentation)
            {
                VTable vt = td.VirtualTable;

                if(vt != null)
                {
                    ConvertVirtualTable( vt );
                }
            }
        }

        internal ObjectDescriptor BuildObjectDescriptor( TypeRepresentation                 td    ,
                                                         DataManager.Attributes             flags ,
                                                         Abstractions.PlacementRequirements pr    )
        {
            CHECKS.ASSERT( !(td is ArrayReferenceTypeRepresentation   ), "Expecting an object, got an array: {0}", td );
            CHECKS.ASSERT( !(td is AbstractReferenceTypeRepresentation), "Cannot instantiate abstract type: {0}", td );

            lock(TypeSystemForCodeTransformation.Lock) // It's called from multiple threads during parallel phase executions.
            {
                ObjectDescriptor od = new ObjectDescriptor( this, td, flags, pr, null );

                m_data[od] = od;

                InstantiateVTable( od );

                return od;
            }
        }

        internal ArrayDescriptor BuildArrayDescriptor( ArrayReferenceTypeRepresentation   td    ,
                                                       DataManager.Attributes             flags ,
                                                       Abstractions.PlacementRequirements pr    ,
                                                       Array                              array ,
                                                       int                                len   )
        {
            CHECKS.ASSERT( (td is ArrayReferenceTypeRepresentation), "Expecting an array, got an object: {0}", td );

            ArrayDescriptor ad;

            lock(TypeSystemForCodeTransformation.Lock) // It's called from multiple threads during parallel phase executions.
            {
                ad = new ArrayDescriptor( this, td, flags, pr, array, len );

                m_data[ad] = ad;

                InstantiateVTable( ad );
            }

            if(array == null)
            {
                var tdElement = td.ContainedType;

                if(tdElement is ValueTypeRepresentation)
                {
                    if(!(tdElement is ScalarTypeRepresentation))
                    {
                        for(int pos = 0; pos < len; pos++)
                        {
                            var od = BuildObjectDescriptor( tdElement, flags, pr );

                            od.SetNesting( ad, null, pos );

                            ad.Set( pos, od );
                        }
                    }
                }
            }

            return ad;
        }

        //--//

        internal CodePointer CreateCodePointer( object obj )
        {
            CodePointer res;
            int         id;

            lock(TypeSystemForCodeTransformation.Lock) // It's called from multiple threads during parallel phase executions.
            {
                if(m_codePointers.TryGetValue( obj, out id ) == false)
                {
                    id = m_nextCodePointerId++;

                    //
                    // To distinguish between real method pointers and placeholders,
                    // we use a non-word-aligned value, which is illegal for real methods.
                    //
                    id = (id * 2 + 1);

                    m_codePointers[obj] = id;

                    if(m_codePointersReverse != null)
                    {
                        m_codePointersReverse[id] = obj;
                    }
                }
            }

            //
            // To distinguish between real method pointers and placeholders,
            // we use a non-word-aligned value, which is illegal for real methods.
            //
            res.Target = new IntPtr( id );

            return res;
        }

        internal object GetCodePointerFromUniqueID( IntPtr val )
        {
            int id = val.ToInt32();

            if((id & 1) != 0)
            {
                lock(TypeSystemForCodeTransformation.Lock) // It's called from multiple threads during parallel phase executions.
                {
                    if(m_codePointersReverse == null)
                    {
                        m_codePointersReverse = HashTableFactory.New< int, object >();

                        object[] keys   = m_codePointers.KeysToArray  ();
                        int[]    values = m_codePointers.ValuesToArray();

                        for(int i = 0; i < keys.Length; i++)
                        {
                            m_codePointersReverse[ values[i] ] = keys[i];
                        }
                    }

                    object obj;

                    if(m_codePointersReverse.TryGetValue( id, out obj ))
                    {
                        return obj;
                    }
                }
            }

            return null;
        }

        internal void RefreshValues( CompilationSteps.PhaseDriver phase )
        {
            GrowOnlyHashTable< object, DataDescriptor > dataIncr = m_data.CloneSettings();

            //
            // Because the process of population can create new DataDescriptors, we need to keep iterating.
            //
            while(true)
            {
                object[] keys  = m_data.KeysToArray();
                bool     fDone = true;

                foreach(object obj in keys)
                {
                    DataDescriptor dd = m_data[obj];

                    if(dataIncr.Update( obj, dd ) == false)
                    {
                        dd.RefreshValues( phase );

                        fDone = false;
                    }
                }

                if(fDone) break;
            }
        }
    }
}
