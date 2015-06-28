//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;
    using System.Runtime.CompilerServices;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    [ExtendClass(typeof(System.Threading.Interlocked), NoConstructors=true)]
    public static class InterlockedImpl
    {
        //
        // Helper Methods
        //

        public static int Increment( ref int location )
        {
            using(SmartHandles.InterruptState.DisableAll())
            {
                return ++location;
            }
        }

        public static long Increment( ref long location )
        {
            using(SmartHandles.InterruptState.DisableAll())
            {
                return ++location;
            }
        }

        public static int Decrement( ref int location )
        {
            using(SmartHandles.InterruptState.DisableAll())
            {
                return --location;
            }
        }

        public static long Decrement( ref long location )
        {
            using(SmartHandles.InterruptState.DisableAll())
            {
                return --location;
            }
        }

        public static int Exchange( ref int location1 ,
                                        int value     )
        {
            using(SmartHandles.InterruptState.DisableAll())
            {
                int oldValue = location1;

                location1 = value;

                return oldValue;
            }
        }

        public static long Exchange( ref long location1 ,
                                         long value     )
        {
            using(SmartHandles.InterruptState.DisableAll())
            {
                long oldValue = location1;

                location1 = value;

                return oldValue;
            }
        }

        public static float Exchange( ref float location1 ,
                                          float value     )
        {
            using(SmartHandles.InterruptState.DisableAll())
            {
                float oldValue = location1;

                location1 = value;

                return oldValue;
            }
        }

        public static double Exchange( ref double location1 ,
                                           double value     )
        {
            using(SmartHandles.InterruptState.DisableAll())
            {
                double oldValue = location1;

                location1 = value;

                return oldValue;
            }
        }

        public static Object Exchange( ref Object location1 ,
                                           Object value     )
        {
            using(SmartHandles.InterruptState.DisableAll())
            {
                object oldValue = location1;

                location1 = value;

                return oldValue;
            }
        }

        public static IntPtr Exchange( ref IntPtr location1, IntPtr value )
        {
            using(SmartHandles.InterruptState.DisableAll())
            {
                IntPtr oldValue = location1;

                location1 = value;

                return oldValue;
            }
        }

        public static T Exchange<T>( ref T location1 ,
                                         T value     ) where T : class
        {
            using(SmartHandles.InterruptState.DisableAll())
            {
                T oldValue = location1;

                location1 = value;

                return oldValue;
            }
        }

        //--//

        public static int CompareExchange( ref int location1 ,
                                               int value     ,
                                               int comparand )
        {
            using(SmartHandles.InterruptState.DisableAll())
            {
                int oldValue = location1;

                if(oldValue == comparand)
                {
                    location1 = value;
                }

                return oldValue;
            }
        }

        public static long CompareExchange( ref long location1 ,
                                                long value     ,
                                                long comparand )
        {
            using(SmartHandles.InterruptState.DisableAll())
            {
                long oldValue = location1;

                if(oldValue == comparand)
                {
                    location1 = value;
                }

                return oldValue;
            }
        }

        public static float CompareExchange( ref float location1 ,
                                                 float value     ,
                                                 float comparand )
        {
            using(SmartHandles.InterruptState.DisableAll())
            {
                float oldValue = location1;

                if(oldValue == comparand)
                {
                    location1 = value;
                }

                return oldValue;
            }
        }

        public static double CompareExchange( ref double location1 ,
                                                  double value     ,
                                                  double comparand )
        {
            using(SmartHandles.InterruptState.DisableAll())
            {
                double oldValue = location1;

                if(oldValue == comparand)
                {
                    location1 = value;
                }

                return oldValue;
            }
        }

        public static Object CompareExchange( ref Object location1 ,
                                                  Object value     ,
                                                  Object comparand )
        {
            using(SmartHandles.InterruptState.DisableAll())
            {
                object oldValue = location1;

                if(oldValue == comparand)
                {
                    location1 = value;
                }

                return oldValue;
            }
        }

        public static IntPtr CompareExchange( ref IntPtr location1 ,
                                                  IntPtr value     ,
                                                  IntPtr comparand )
        {
            using(SmartHandles.InterruptState.DisableAll())
            {
                IntPtr oldValue = location1;

                if(oldValue == comparand)
                {
                    location1 = value;
                }

                return oldValue;
            }
        }

        public static T CompareExchange<T>( ref T location1 ,
                                                T value     ,
                                                T comparand ) where T : class
        {
            using(SmartHandles.InterruptState.DisableAll())
            {
                T oldValue = location1;

                if(Object.ReferenceEquals( oldValue, comparand ))
                {
                    location1 = value;
                }

                return oldValue;
            }
        }

        public static int Add( ref int location1 ,
                                   int value     )
        {
            using(SmartHandles.InterruptState.DisableAll())
            {
                int res = location1 + value;

                location1 = res;

                return res;
            }
        }

        public static long Add( ref long location1 ,
                                    long value     )
        {
            using(SmartHandles.InterruptState.DisableAll())
            {
                long res = location1 + value;

                location1 = res;

                return res;
            }
        }
    }
}

