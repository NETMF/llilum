using System;
using System.Runtime.CompilerServices;

using LLILUM = Microsoft.Llilum.Devices;


namespace Microsoft.SPOT.Hardware
{

    public delegate void NativeEventHandler(uint data1, uint data2, DateTime time);

    //--//

    public class NativeEventDispatcher : IDisposable
    {
        protected NativeEventHandler m_threadSpawn = null;
        protected NativeEventHandler m_callbacks   = null;
        protected bool               m_disposed    = false;
        private object               m_NativeEventDispatcher;

        //--//

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public NativeEventDispatcher(string strDriverName, ulong drvData);

        public virtual void EnableInterrupt()
        {

        }

        public virtual void DisableInterrupt()
        {

        }

        protected virtual void Dispose(bool disposing)
        {

        }

        protected void Dispatch( object sender, LLILUM.Gpio.PinEdge args )
        {
            NativeEventHandler eh = m_threadSpawn;

            if(eh != null)
            {
                Port port = (Port)sender;

                eh( (uint)port.Id, 
                    (uint)((args == Llilum.Devices.Gpio.PinEdge.RisingEdge || args == Llilum.Devices.Gpio.PinEdge.LevelHigh) ? 1 : 0),
                    DateTime.Now ); 
            }
        }

        //--//

        ~NativeEventDispatcher()
        {
            Dispose(false);
        }

        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        public virtual void Dispose()
        {
            if (!m_disposed)
            {
                Dispose(true);

                GC.SuppressFinalize(this);

                m_disposed = true;
            }
        }

        public event NativeEventHandler OnInterrupt
        {
            [MethodImplAttribute(MethodImplOptions.Synchronized)]
            add
            {
                if (m_disposed)
                {
                    throw new ObjectDisposedException("");
                }

                NativeEventHandler callbacksOld = m_callbacks;
                NativeEventHandler callbacksNew = (NativeEventHandler)Delegate.Combine(callbacksOld, value);

                try
                {
                    m_callbacks = callbacksNew;

                    if (callbacksNew != null)
                    {
                        if (callbacksOld == null)
                        {
                            EnableInterrupt();
                        }

                        if (callbacksNew.Equals(value) == false)
                        {
                            callbacksNew = new NativeEventHandler(this.MultiCastCase);
                        }
                    }

                    m_threadSpawn = callbacksNew;
                }
                catch
                {
                    m_callbacks = callbacksOld;

                    if (callbacksOld == null)
                    {
                        DisableInterrupt();
                    }

                    throw;
                }
            }

            [MethodImplAttribute(MethodImplOptions.Synchronized)]
            remove
            {
                if (m_disposed)
                {
                    throw new ObjectDisposedException("");
                }

                NativeEventHandler callbacksOld = m_callbacks;
                NativeEventHandler callbacksNew = (NativeEventHandler)Delegate.Remove(callbacksOld, value);

                try
                {
                    m_callbacks = (NativeEventHandler)callbacksNew;

                    if (callbacksNew == null && callbacksOld != null)
                    {
                        DisableInterrupt();
                    }
                }
                catch
                {
                    m_callbacks = callbacksOld;

                    throw;
                }
            }
        }

        private void MultiCastCase(uint port, uint state, DateTime time)
        {
            NativeEventHandler callbacks = m_callbacks;

            if (callbacks != null)
            {
                callbacks(port, state, time);
            }
        }
    }

    //--//

    public class Port : NativeEventDispatcher
    {
        public enum ResistorMode
        {
            Disabled = 0,
            PullDown = 1,
            PullUp = 2,
        }

        public enum InterruptMode
        {
            InterruptNone = 0,
            InterruptEdgeLow = 1,
            InterruptEdgeHigh = 2,
            InterruptEdgeBoth = 3,
            InterruptEdgeLevelHigh = 4,
            InterruptEdgeLevelLow = 5,
        }

        //--//
        
        extern protected LLILUM.Gpio.GpioPin Pin
        {
            [MethodImplAttribute( MethodImplOptions.InternalCall )]
            get;
        }

        //--//

        [MethodImplAttribute( MethodImplOptions.InternalCall )]
        extern protected Port( Cpu.Pin portId, bool glitchFilter, ResistorMode resistor, InterruptMode interruptMode );

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern protected Port(Cpu.Pin portId, bool initialState);

        protected Port(Cpu.Pin portId, bool initialState, bool glitchFilter, ResistorMode resistor)
            : this( portId, glitchFilter, resistor, InterruptMode.InterruptNone )
        {
        }

        protected override void Dispose( bool disposing )
        {
            if(disposing)
            {
                this.Pin.Dispose( );
            }
        }

        public bool Read()
        {
            return this.Pin.Read( ) != 0; 
        }

        public Cpu.Pin Id
        {
            [MethodImplAttribute( MethodImplOptions.InternalCall )]
            get;
        }

        protected ResistorMode Resistor
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;

            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            set;
        }

        protected bool GlitchFilter
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;

            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            set;
        }

        public InterruptMode Interrupt
        {
            [MethodImplAttribute( MethodImplOptions.InternalCall )]
            get;
            
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            set;
        }

        public bool InitialState
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern static public bool ReservePin(Cpu.Pin pin, bool fReserve);
    }

    //--//

    public class InputPort : Port
    {
        public InputPort(Cpu.Pin portId, bool glitchFilter, ResistorMode resistor)
            : base(portId, glitchFilter, resistor, InterruptMode.InterruptNone)
        {
        }

        protected InputPort(Cpu.Pin portId, bool glitchFilter, ResistorMode resistor, InterruptMode interruptMode)
            : base(portId, glitchFilter, resistor, interruptMode)
        {
        }

        protected InputPort(Cpu.Pin portId, bool initialState, bool glitchFilter, ResistorMode resistor)
            : base(portId, initialState, glitchFilter, resistor)
        {
        }

        public new ResistorMode Resistor
        {
            get
            {
                return base.Resistor;
            }

            set
            {
                base.Resistor = value;
            }
        }

        public new bool GlitchFilter
        {
            get
            {
                return base.GlitchFilter;
            }

            set
            {
                base.GlitchFilter = value;
            }
        }
    }

    //--//

    public class OutputPort : Port
    {
        public OutputPort(Cpu.Pin portId, bool initialState)
            : base(portId, initialState)
        {
            this.Pin.Write( initialState ? 1 : 0 ); 
        }

        protected OutputPort(Cpu.Pin portId, bool initialState, bool glitchFilter, ResistorMode resistor)
            : base(portId, initialState, glitchFilter, resistor)
        {
            this.Pin.Write( initialState ? 1 : 0 ); 
        }

        public void Write(bool state)
        {
            this.Pin.Write( state? 1 : 0 );
        }

        new public bool InitialState
        {
            get
            {
                return base.InitialState;
            }
        }
    }

    //--//

    public sealed class TristatePort : OutputPort
    {
        public TristatePort(Cpu.Pin portId, bool initialState, bool glitchFilter, ResistorMode resistor)
            : base(portId, initialState, glitchFilter, resistor)
        {
        }

        public bool Active
        {
            get
            {
                return this.Pin.Direction == LLILUM.Gpio.PinDirection.Output;
            }

            set
            {
                this.Pin.Direction = value ? LLILUM.Gpio.PinDirection.Output : LLILUM.Gpio.PinDirection.Input;
            }
        }

        new public ResistorMode Resistor
        {
            get
            {
                return base.Resistor;
            }

            set
            {
                base.Resistor = value;
            }
        }

        new public bool GlitchFilter
        {
            get
            {
                return base.GlitchFilter;
            }
        }
    }

    //--//

    public sealed class InterruptPort : InputPort
    {
        //--//

        public InterruptPort(Cpu.Pin portId, bool glitchFilter, ResistorMode resistor, InterruptMode interrupt)
            : base(portId, glitchFilter, resistor, interrupt)
        {
            m_threadSpawn = null;
            m_callbacks   = null;
        }

        public void ClearInterrupt()
        {

        }

        new public InterruptMode Interrupt
        {
            get
            {
                return base.Interrupt;
            }
            
            set
            {
                base.Interrupt = value;
            }
        }

        public override void EnableInterrupt()
        {
            this.Pin.ValueChanged += base.Dispatch;
        }

        public override void DisableInterrupt()
        { 
            this.Pin.ValueChanged -= base.Dispatch;
        }
    }
}


