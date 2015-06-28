//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.MetaData.Normalized
{
    using System;

    public abstract class MarshalSpec
    {
        public override abstract String ToString();
    }

    public class MarshalSpecNative : MarshalSpec
    {
        //
        // State
        //

        private readonly NativeTypes m_kind;

        //
        // Constructor Methods
        //

        public MarshalSpecNative( NativeTypes kind )
        {
            m_kind = kind;
        }

        //
        // Access Methods
        //

        public NativeTypes Kind
        {
            get
            {
                return m_kind;
            }
        }

        public override String ToString()
        {
            return "MarshalSpecNative(" + m_kind + ")";
        }
    }

    public class MarshalSpecSafeArray : MarshalSpec
    {
        //
        // State
        //

        private readonly VariantTypes m_elemType;

        //
        // Constructor Methods
        //

        public MarshalSpecSafeArray( VariantTypes elemType )
        {
            m_elemType = elemType;
        }

        //
        // Access Methods
        //

        public VariantTypes ElementType
        {
            get
            {
                return m_elemType;
            }
        }

        public override String ToString()
        {
            return "MarshalSpecSafeArray(" + m_elemType + ")";
        }
    }

    public class MarshalSpecFixedString : MarshalSpec
    {
        //
        // State
        //

        private readonly uint m_elemCount;

        //
        // Constructor Methods
        //

        public MarshalSpecFixedString( uint elemCount )
        {
            m_elemCount = elemCount;
        }

        //
        // Access Methods
        //

        public uint ElementCount
        {
            get
            {
                return m_elemCount;
            }
        }

        public override String ToString()
        {
            return "MarshalSpecFixedString(" + m_elemCount + ")";
        }
    }

    public class MarshalSpecFixedArray : MarshalSpec
    {
        //
        // State
        //

        private readonly uint m_elemCount;

        //
        // Constructor Methods
        //

        public MarshalSpecFixedArray( uint elemCount )
        {
            m_elemCount = elemCount;
        }

        //
        // Access Methods
        //

        public uint ElementCount
        {
            get
            {
                return m_elemCount;
            }
        }

        public override String ToString()
        {
            return "MarshalSpecFixedArray(" + m_elemCount + ")";
        }
    }

    public class MarshalSpecCustom : MarshalSpec
    {
        //
        // State
        //

        private readonly String m_guid;
        private readonly String m_unmanagedType;
        private readonly String m_managedType;
        private readonly String m_cookie;

        //
        // Constructor Methods
        //

        public MarshalSpecCustom( String guid          ,
                                  String unmanagedType ,
                                  String managedType   ,
                                  String cookie        )
        {
            m_guid          = guid;
            m_unmanagedType = unmanagedType;
            m_managedType   = managedType;
            m_cookie        = cookie;
        }

        //
        // Access Methods
        //

        public String Guid
        {
            get
            {
                return m_guid;
            }
        }

        public String UnmanagedType
        {
            get
            {
                return m_unmanagedType;
            }
        }

        public String ManagedType
        {
            get
            {
                return m_managedType;
            }
        }

        public String Cookie
        {
            get
            {
                return m_cookie;
            }
        }

        public override String ToString()
        {
            return "MarshalSpecCustom(" + m_guid + "," + m_managedType + "->" + m_unmanagedType + "," + m_cookie + ")";
        }
    }

    public class MarshalSpecArray : MarshalSpec
    {
        //
        // State
        //

        private readonly NativeTypes m_elemType;
        private readonly uint        m_paramNumber;
        private readonly uint        m_extras;

        //
        // Constructor Methods
        //

        public MarshalSpecArray( NativeTypes elemType    ,
                                 uint        paramNumber ,
                                 uint        extras      )
        {
            m_elemType    = elemType;
            m_paramNumber = paramNumber;
            m_extras      = extras;
        }

        //
        // Access Methods
        //

        public NativeTypes ElementType
        {
            get
            {
                return m_elemType;
            }
        }

        public uint ParameterNumber
        {
            get
            {
                return m_paramNumber;
            }
        }

        public uint ExtraCount
        {
            get
            {
                return m_extras;
            }
        }

        public override String ToString()
        {
            return "MarshalSpecArray(" + m_elemType + "," + m_paramNumber + "," + m_extras + ")";
        }
    }
}
