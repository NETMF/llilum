////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Microsoft.SPOT.Hardware
{
    public static class SystemInfo
    {
        //////[MethodImplAttribute(MethodImplOptions.InternalCall)]
        //////extern private static void GetSystemVersion(out int major, out int minor, out int build, out int revision);
        private static void GetSystemVersion(out int major, out int minor, out int build, out int revision)
        {
            // TODO TODO TODO: implement 
            throw new NotImplementedException( ); 
        }

        public static Version Version
        {
            get
            {
                int major, minor, build, revision;

                GetSystemVersion(out major, out minor, out build, out revision);

                return new Version(major, minor, build, revision);
            }
        }
        
        //////extern public static String OEMString
        public static String OEMString
        {
            //////[MethodImplAttribute(MethodImplOptions.InternalCall)]
            //////get;
            get
            {
                // TODO TODO TODO: implement 
                throw new NotImplementedException( ); 
            }
        }

        public static class SystemID
        {
            //////extern static public byte OEM
            static public byte OEM
            {
                //////[MethodImplAttribute(MethodImplOptions.InternalCall)]
                //////get;
                get
                {
                    // TODO TODO TODO: implement 
                    throw new NotImplementedException( ); 
                }
            }
            
            //////extern static public byte Model
            static public byte Model
            {
                //////[MethodImplAttribute(MethodImplOptions.InternalCall)]
                //////get;
                get
                {
                    // TODO TODO TODO: implement 
                    throw new NotImplementedException( ); 
                }
            }
            
            //////extern static public ushort SKU
            static public ushort SKU
            {
                //////[MethodImplAttribute(MethodImplOptions.InternalCall)]
                //////get;
                get
                {
                    return 4; 
                }
            }
        }
        
        //////extern public static bool IsBigEndian
        public static bool IsBigEndian
        {
            //////[MethodImplAttribute(MethodImplOptions.InternalCall)]
            //////get;
            get
            {
                return false; 
            }
        }

        public static readonly bool IsEmulator = (SystemID.SKU == 3);

        public static readonly bool IsLlilum = (SystemID.SKU == 4);
    }
}


