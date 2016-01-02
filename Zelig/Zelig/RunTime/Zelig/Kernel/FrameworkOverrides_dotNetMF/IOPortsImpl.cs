using System;
using System.Runtime.CompilerServices;

using NETMF = Microsoft.SPOT.Hardware;
using LLILUM = Microsoft.Llilum.Devices;
using Microsoft.Llilum.Devices.Gpio;

namespace Microsoft.Zelig.Runtime
{
    [ExtendClass(typeof(NETMF.NativeEventDispatcher))]
    public class NativeEventDispatcher
    {
        [DiscardTargetImplementation]
        public NativeEventDispatcher( string strDriverName, ulong drvData )
        {

        }
    }

    //--//

    [ExtendClass(typeof(NETMF.Port))]
    public class Port
    {
        protected int                       m_portId;
        protected NETMF.Port.ResistorMode   m_resistorMode;
        protected bool                      m_glitchFilterEnable;
        protected bool                      m_initialState;
        protected NETMF.Port.InterruptMode  m_interruptMode;
        //--//
        protected LLILUM.Gpio.GpioPin       m_pin;

        //--//
        
        protected LLILUM.Gpio.GpioPin Pin
        {
            get
            {
                return m_pin;
            }
        }
        
        //--//

        [DiscardTargetImplementation]
        protected Port( NETMF.Cpu.Pin portId,
                        bool glitchFilter, 
                        NETMF.Port.ResistorMode resistor, 
                        NETMF.Port.InterruptMode interruptMode)
        {
            m_portId                = (int)portId;
            m_resistorMode          = resistor;
            m_glitchFilterEnable    = glitchFilter;
            m_interruptMode         = interruptMode;
            m_initialState          = InitialState;

            m_pin = GpioProvider.Instance.CreateGpioPin( (int)portId );

            m_pin.Direction = Llilum.Devices.Gpio.PinDirection.Input;

            m_pin.Mode = ConvertResistorModeValue( resistor ); 

            m_pin.ActivePinEdge = ConvertInterruptModeValue( interruptMode );

            m_pin.ValueChanged += OnValueChanged;
        }
        
        [DiscardTargetImplementation]
        protected Port(NETMF.Cpu.Pin portId, bool initialState) 
        {
            m_portId = (int)portId;

            m_pin = GpioProvider.Instance.CreateGpioPin( (int)portId );

            m_pin.Direction = Llilum.Devices.Gpio.PinDirection.Output;

            m_pin.Write( initialState ? 1 : 0 ); 
        }

        public NETMF.Cpu.Pin Id
        {
            get
            {
                return (NETMF.Cpu.Pin)m_portId;
            }
        }

        public NETMF.Port.ResistorMode Resistor
        {
            get
            {
                return m_resistorMode;
            }

            set
            {
                m_resistorMode = value;
            }
        }

        public bool GlitchFilter
        {
            get
            {
                return m_glitchFilterEnable;
            }
            
            set
            {
                m_glitchFilterEnable = value;
                throw new NotSupportedException( ); 
            }
        }

        public NETMF.Port.InterruptMode Interrupt
        {
            get
            {
                return m_interruptMode;
            }

            set
            {
                m_interruptMode = value;
            }
        }
        
        public bool InitialState
        {
            get
            {
                return m_initialState;
            }
        }

        static public bool ReservePin( NETMF.Cpu.Pin pin, bool fReserve )
        {
            if(fReserve)
            {
                if(HardwareProvider.Instance.TryReservePins( (int)pin ))
                {
                    return true;
                }

                throw new InvalidOperationException( );
            }

            HardwareProvider.Instance.ReleasePins( (int)pin );

            return true;
        }

        //--//

        private void OnValueChanged( object sender, LLILUM.Gpio.PinEdge args )
        {

        }

        //--//

        private static PinMode ConvertResistorModeValue( NETMF.Port.ResistorMode resistor )
        {
            return resistor == NETMF.Port.ResistorMode.Disabled ? LLILUM.Gpio.PinMode.OpenDrain :
                   resistor == NETMF.Port.ResistorMode.PullUp   ? LLILUM.Gpio.PinMode.PullUp    : LLILUM.Gpio.PinMode.PullDown;
            
        }

        private static PinEdge ConvertInterruptModeValue( NETMF.Port.InterruptMode interruptMode )
        {
            Llilum.Devices.Gpio.PinEdge edge = PinEdge.None;

            switch(interruptMode)
            {
                case NETMF.Port.InterruptMode.InterruptNone:
                    edge = Llilum.Devices.Gpio.PinEdge.None;
                    break;
                case NETMF.Port.InterruptMode.InterruptEdgeHigh:
                    edge = Llilum.Devices.Gpio.PinEdge.RisingEdge;
                    break;
                case NETMF.Port.InterruptMode.InterruptEdgeLow:
                    edge = Llilum.Devices.Gpio.PinEdge.FallingEdge;
                    break;
                case NETMF.Port.InterruptMode.InterruptEdgeBoth:
                    edge = Llilum.Devices.Gpio.PinEdge.BothEdges;
                    break;
                case NETMF.Port.InterruptMode.InterruptEdgeLevelHigh:
                    edge = Llilum.Devices.Gpio.PinEdge.LevelHigh;
                    break;
                case NETMF.Port.InterruptMode.InterruptEdgeLevelLow:
                    edge = Llilum.Devices.Gpio.PinEdge.LevelLow;
                    break;
                default:
                    throw new ArgumentException( );
            }

            return edge;
        }
    }
}


