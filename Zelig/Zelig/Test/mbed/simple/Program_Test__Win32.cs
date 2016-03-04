//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

#define WIN32

namespace Microsoft.Zelig.Test.mbed.Simple
{
    using System;
    using System.Threading;
    

    internal partial class Program
    {
        static int s_Counter1 = 0;
        static int s_Counter2 = 0;
        static int s_Counter3 = 0;

        static void TestWin32Threading()
        {
#if WIN32
            Thread th1 = new Thread( ( ) =>
            {
                int i=0;

                while(true)
                {
                    System.Diagnostics.Debug.WriteLine( "Thread1: " + i++);
                    Thread.Sleep(500);
                }
            } );

            Thread th2 = new Thread( ( ) =>
            {
                int i=0;

                while(true)
                {
                    System.Diagnostics.Debug.WriteLine( "Thread2: " + i++);
                    Thread.Sleep(500);
                }
            } );

            Timer t1 = new Timer( (object arg) =>
            {
                System.Diagnostics.Debug.WriteLine( "Timer1: " + s_Counter1++ );
            }, null, 1000, 1000 );

            Timer t2 = new Timer( (object arg) =>
            {
                System.Diagnostics.Debug.WriteLine( "Timer2: " + s_Counter2++ );
            }, null, 2000, 2000 );

            Timer t3 = new Timer( (object arg) =>
            {
                System.Diagnostics.Debug.WriteLine( "Timer3: " + s_Counter3++ );
            }, null, 4000, 4000 );

            th1.Start( );
            th2.Start( );

            while(true)
            {
                Thread.Sleep(-1);
            }
#endif // WIN32
        }

    }
}
