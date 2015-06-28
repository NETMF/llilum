//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

using System.Collections.Generic;

namespace Microsoft.Zelig.Test.mbed
{
    namespace Simple
    {
        internal abstract class LedToggler
        {
            public LedToggler( )
            {
                dOuts = new DigitalOut[ 4 ];
                dOuts[ 0 ] = new DigitalOut( PinName.LED1 );
                dOuts[ 1 ] = new DigitalOut( PinName.LED2 );
                dOuts[ 2 ] = new DigitalOut( PinName.LED3 );
                dOuts[ 3 ] = new DigitalOut( PinName.LED4 );
            }

            public abstract void run( float t );

            public int this[ int key ]
            {
                get
                {
                    return dOuts[ key ].value;
                }
                set
                {
                    dOuts[ key ].value = value;
                }
            }

            private DigitalOut[] dOuts;
        };

        internal class LedTogglerSimultaneous : LedToggler
        {
            public override void run( float t )
            {
                for( int i=0; i < 4; i++ )
                {
                    this[ i ] = t < 0.5f ? 0 : 1;
                }
            }
        }

        internal class LedTogglerSequential : LedToggler
        {
            public override void run( float t )
            {
                for( int i=0; i < 4; i++ )
                {
                    this[ i ] = ( int )( t * 4 ) == i ? 1 : 0;
                }
            }
        }

        internal class LedTogglerAlternate : LedToggler
        {
            public override void run( float t )
            {
                for( int i=0; i < 4; i++ )
                {
                    this[ i ] = ( i % 2 ) == ( int )( t * 2 ) ? 1 : 0;
                }
            }
        }


        class Program
        {
            const float period = 0.75f;
            const float timePerMode = 4.0f;

            static void Main( )
            {
                LedToggler[] blinkingModes = new LedToggler[ 3 ];

                blinkingModes[ 0 ] = new LedTogglerSimultaneous( );
                blinkingModes[ 1 ] = new LedTogglerSequential( );
                blinkingModes[ 2 ] = new LedTogglerAlternate( );

                var blinkingTimer           = new Timer( );
                var blinkingModeSwitchTimer = new Timer( );

                blinkingTimer.start( );
                blinkingModeSwitchTimer.start( );

                int currentMode=0;

                while( true )
                {
                    if( blinkingTimer.read( ) >= period ) blinkingTimer.reset( );
                    if( blinkingModeSwitchTimer.read( ) >= timePerMode )
                    {
                        currentMode = ( currentMode + 1 ) % blinkingModes.Length;
                        blinkingModeSwitchTimer.reset( );
                    }

                    blinkingModes[ currentMode ].run( blinkingTimer.read( ) / period );

                }

            }


        }
    }
}
