//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Configuration.Environment
{
    using System;
    using System.Collections.Generic;


    public abstract class JtagLoaderCategory : AbstractCategory
    {
        //
        // State
        //

        [DisplayName("Driver Name of the Target")]
        public string DriverName;

        [DisplayName("Clock Speed to Use When Running")]
        public int Speed;

        [DisplayName("Can Set Breakpoints During Reset")]
        public bool CanSetBreakpointsDuringReset;

        //--//

        protected GrowOnlyHashTable< uint, byte[] > m_loaderData;
        protected uint                              m_entryPoint;

        //
        // Constructor Methods
        //

        protected JtagLoaderCategory()
        {
            m_loaderData = HashTableFactory.New< uint, byte[] >();
        }

        //
        // Access Methods
        //

        public GrowOnlyHashTable< uint, byte[] > LoaderData
        {
            get
            {
                return m_loaderData;
            }
        }

        public uint EntryPoint
        {
            get
            {
                return m_entryPoint;
            }
        }
    }
}
