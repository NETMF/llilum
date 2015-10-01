// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==

namespace System.Reflection
{
    using System;

    [Serializable]
    public struct ParameterModifier
    {
        #region Private Data Members
        private bool[] m_byRef;
        #endregion

        #region Constructor
        public ParameterModifier( int parameterCount )
        {
            if(parameterCount <= 0)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentException( Environment.GetResourceString( "Arg_ParmArraySize" ) );
#else
                throw new ArgumentException();
#endif
            }

            m_byRef = new bool[parameterCount];
        }
        #endregion

        #region Internal Members
        internal bool[] IsByRefArray
        {
            get
            {
                return m_byRef;
            }
        }
        #endregion

        #region Public Members
        public bool this[int index]
        {
            get
            {
                return m_byRef[index];
            }

            set
            {
                m_byRef[index] = value;
            }
        }
        #endregion
    }
}
