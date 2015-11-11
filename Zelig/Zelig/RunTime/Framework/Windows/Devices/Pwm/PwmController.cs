//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Windows.Devices.Pwm.Provider;

using Llilum = Microsoft.Llilum.Devices.Pwm;

namespace Windows.Devices.Pwm
{
    public sealed class PwmController
    {
        internal IPwmControllerProvider m_pwmControllerProvider;

        private PwmController(IPwmControllerProvider provider)
        {
            m_pwmControllerProvider = provider;
        }

        public double ActualFrequency
        {
            get
            {
                return m_pwmControllerProvider.ActualFrequency;
            }
        }

        public double MaxFrequency
        {
            get
            {
                return m_pwmControllerProvider.MaxFrequency;
            }
        }

        public double MinFrequency
        {
            get
            {
                return m_pwmControllerProvider.MinFrequency;
            }
        }

        public int PinCount
        {
            get
            {
                return m_pwmControllerProvider.PinCount;
            }
        }

        public double SetDesiredFrequency(double desiredFrequency)
        {
            return m_pwmControllerProvider.SetDesiredFrequency(desiredFrequency);
        }

        public PwmPin OpenPin(int pinNumber)
        {
            // If the channel cannot be acquired, this will throw
            m_pwmControllerProvider.AcquirePin(pinNumber);

            return new PwmPin(this, pinNumber);
        }
        
        // TODO: Implement as IAsyncOperation
        public static /*IAsyncOperation<PwmController>*/PwmController GetDefaultAsync()
        {
            return new PwmController(new DefaultPwmControllerProvider(GetPwmProviderInfo()));
        }

        //TODO: public static IAsyncOperation<IVectorView<PwmController>> GetControllersAsync(IPwmProvider provider)
        public static IList<PwmController> GetControllersAsync(IPwmProvider provider)
        {
            List<PwmController> controllers = new List<PwmController>();
            foreach(var controller in provider.GetControllers())
            {
                controllers.Add(new PwmController(controller));
            }
            return controllers;
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern Llilum.IPwmChannelInfoUwp GetPwmProviderInfo();
    }
}
