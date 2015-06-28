using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Microsoft.Zelig.Test.mbed
{
    public class Timer
    {
        private bool _running;     // whether the timer is running
        private uint _start;      // the start time of the latest slice
        private int _time;        // any accumulated time from previous slices

        public static void wait_ms(int ms)
        {
            uint timestamp = UsTicker.us_ticker_read();
            ms *= 1000;
            while ((int)(UsTicker.us_ticker_read() - timestamp)<ms) { }
        }

        public static void wait_us(int us)
        {
            uint timestamp = UsTicker.us_ticker_read();
            while ((int)(UsTicker.us_ticker_read() - timestamp) < us) { }
        }

        public Timer()
        {
            reset();
        }

        /** Start the timer
         */
        public void start()
        {
            if (!_running)
            {
                reset( );
                _running = true;
            }
        }

        /** Stop the timer
         */
        public void stop()
        {
            _time += slicetime();
            _running = false;
        }

        /** Reset the timer to 0.
         *
         * If it was already counting, it will continue
         */
        public void reset()
        {
            _start = UsTicker.us_ticker_read();
            _time = 0;
        }

        /** Get the time passed in seconds
         */
        public float read()
        {
            return (float)read_us() / 1000000.0f;
        }

        /** Get the time passed in mili-seconds
         */
        public int read_ms()
        {
            return read_us() / 1000;
        }

        /** Get the time passed in micro-seconds
         */

        public int read_us()
        {
            return _time + slicetime();
        }

        private int slicetime()
        {
            if (_running)
            {
                return (int)(UsTicker.us_ticker_read() - _start);
            }
            else
            {
                return 0;
            }
        }

    }
}
