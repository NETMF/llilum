//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Debugger.ArmProcessor
{
    using System;
    using System.Collections.Generic;

    using IR = Microsoft.Zelig.CodeGeneration.IR;
    using RT = Microsoft.Zelig.Runtime;
    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    public abstract class AbstractValueHandle
    {
        //
        // State
        //

        public readonly TS.TypeRepresentation            Type;
        public readonly TS.CustomAttributeRepresentation MemoryMappedPeripheral;
        public readonly TS.CustomAttributeRepresentation MemoryMappedRegister;
        public readonly bool                             AsHoldingVariable;

        //
        // Contructor Methods
        //

        protected AbstractValueHandle( TS.TypeRepresentation            type                     ,
                                       TS.CustomAttributeRepresentation caMemoryMappedPeripheral ,
                                       TS.CustomAttributeRepresentation caMemoryMappedRegister   ,
                                       bool                             fAsHoldingVariable       )
        {
            this.Type                   = AdjustTypeForScalars( type );
            this.MemoryMappedPeripheral = caMemoryMappedPeripheral;
            this.MemoryMappedRegister   = caMemoryMappedRegister;
            this.AsHoldingVariable      = fAsHoldingVariable;
        }

        //
        // Helper Methods
        //

        public static TS.TypeRepresentation AdjustTypeForScalars( TS.TypeRepresentation td )
        {
            var tdS = td as TS.ScalarTypeRepresentation;
            if(tdS != null)
            {
                return tdS;
            }

            if(td is TS.ValueTypeRepresentation)
            {
                //
                // Is this a value type with only one instance, scalar field? If so, display the value directly.
                //
                foreach(TS.FieldRepresentation fd in td.Fields)
                {
                    if(fd is TS.InstanceFieldRepresentation)
                    {
                        if(tdS != null)
                        {
                            //
                            // More than one field, fail.
                            //
                            return td;
                        }

                        tdS = fd.FieldType as TS.ScalarTypeRepresentation;

                        if(tdS == null)
                        {
                            //
                            // Not a scalar, fail.
                            //
                            return td;
                        }

                        if(fd.Offset != 0)
                        {
                            //
                            // Not aligned, fail.
                            //
                            return td;
                        }
                    }
                }

                if(tdS != null)
                {
                    return tdS;
                }
            }

            return td;
        }

        //--//

        public abstract bool IsEquivalent( AbstractValueHandle abstractValueHandle );

        //--//

        public Emulation.Hosting.BinaryBlob Read( out bool fChanged )
        {
            return Read( 0, this.Size, out fChanged );
        }

        public abstract Emulation.Hosting.BinaryBlob Read(     int  offset   ,
                                                               int  count    ,
                                                           out bool fChanged );

        //--//

        public bool Write( Emulation.Hosting.BinaryBlob bb )
        {
            return Write( bb, 0, this.Size );
        }

        public abstract bool Write( Emulation.Hosting.BinaryBlob bb     ,
                                    int                          offset ,
                                    int                          count  );

        //--//

        public virtual AbstractValueHandle AccessField( TS.InstanceFieldRepresentation   fd                       ,
                                                        TS.CustomAttributeRepresentation caMemoryMappedPeripheral ,
                                                        TS.CustomAttributeRepresentation caMemoryMappedRegister   )
        {
            bool fAsHoldingVariable = (caMemoryMappedRegister == null);
            var  fdType             = fd.FieldType;
            uint size;

            if(fAsHoldingVariable)
            {
                size = fdType.SizeOfHoldingVariable;
            }
            else
            {
                if(fdType is TS.ArrayReferenceTypeRepresentation)
                {
                    int len = caMemoryMappedRegister.GetNamedArg< int >( "Instances" );

                    size = fdType.ContainedType.Size * (uint)len;
                }
                else
                {
                    size = fdType.Size;
                }
            }

            return new SlicedValueHandle( fd.FieldType, caMemoryMappedPeripheral, caMemoryMappedRegister, fAsHoldingVariable, this, fd.Offset, (int)size );
        }

        public AbstractValueHandle AccessBitField( TS.InstanceFieldRepresentation   fd                       ,
                                                   TS.CustomAttributeRepresentation caMemoryMappedPeripheral ,
                                                   TS.CustomAttributeRepresentation caMemoryMappedRegister   ,
                                                   IR.BitFieldDefinition            bfDef                    )
        {
            return new BitFieldValueHandle( fd.FieldType, caMemoryMappedPeripheral, caMemoryMappedRegister, this, fd.OwnerType, bfDef );
        }

        //
        // Access Methods
        //

        public virtual int Size
        {
            get
            {
                return (int)(this.AsHoldingVariable ? this.Type.SizeOfHoldingVariable: this.Type.Size);
            }
        }

        public abstract bool CanUpdate
        {
            get;
        }

        public abstract bool HasAddress
        {
            get;
        }

        public abstract uint Address
        {
            get;
        }
    }
}