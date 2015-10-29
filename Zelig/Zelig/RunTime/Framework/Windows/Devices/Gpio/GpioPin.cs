//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

using System;
using Windows.Devices.Gpio.Provider;
using Windows.Foundation;

namespace Windows.Devices.Gpio
{
    /// <summary>
    /// Represents a general-purpose I/O (GPIO) pin.
    /// </summary>
    public sealed class GpioPin : IDisposable
    {
        private IGpioPinProvider _provider;

        private GpioPinDriveMode _driveMode = GpioPinDriveMode.Input;
        private GpioPinValue _lastOutputValue = GpioPinValue.Low;
        private TypedEventHandler<GpioPin, GpioPinValueChangedEventArgs> _callbacks = null;

        internal GpioPin()
        {
        }

        ~GpioPin()
        {
            Dispose(false);
        }

        /// <summary>
        /// Occurs when the value of the general-purpose I/O (GPIO) pin changes, either because of an external stimulus
        /// when the pin is configured as an input, or when a value is written to the pin when the pin is configured as
        /// an output.
        /// </summary>
        public event TypedEventHandler<GpioPin, GpioPinValueChangedEventArgs> ValueChanged
        {
            add
            {
                ThrowIfDisposed();

                var callbacksOld = _callbacks;
                _callbacks += value;

                if (callbacksOld == null)
                {
                    _provider.ValueChanged += HandlePinChangedEvent;
                }
            }

            remove
            {
                ThrowIfDisposed();

                _callbacks -= value;

                if( _callbacks == null )
                {
                    _provider.ValueChanged -= HandlePinChangedEvent;
                }
            }
        }

        /// <summary>
        /// Gets or sets the debounce timeout for the general-purpose I/O (GPIO) pin, which is an interval during which
        /// changes to the value of the pin are filtered out and do not generate <c>ValueChanged</c> events.
        /// </summary>
        /// <value> The debounce timeout for the GPIO pin, which is an interval during which changes to the value of the
        ///     pin are filtered out and do not generate <c>ValueChanged</c> events. If the length of this interval is
        ///     0, all changes to the value of the pin generate <c>ValueChanged</c> events.</value>
        public TimeSpan DebounceTimeout
        {
            get
            {
                ThrowIfDisposed();

                return _provider.DebounceTimeout;
            }

            set
            {
                ThrowIfDisposed();

                _provider.DebounceTimeout = value;
            }
        }

        /// <summary>
        /// Gets the pin number of the general-purpose I/O (GPIO) pin.
        /// </summary>
        /// <value>The pin number of the GPIO pin.</value>
        public int PinNumber
        {
            get
            {
                ThrowIfDisposed();

                return _provider.PinNumber;
            }
        }

        /// <summary>
        /// Gets the sharing mode in which the general-purpose I/O (GPIO) pin is open.
        /// </summary>
        /// <value>The sharing mode in which the GPIO pin is open.</value>
        public GpioSharingMode SharingMode => GpioSharingMode.Exclusive;

        internal IGpioPinProvider PinProvider
        {
            set
            {
                _provider = value;
            }
        }

        /// <summary>
        /// Reads the current value of the general-purpose I/O (GPIO) pin.
        /// </summary>
        /// <returns>The current value of the GPIO pin. If the pin is configured as an output, this value is the last
        ///     value written to the pin.</returns>
        public GpioPinValue Read()
        {
            ThrowIfDisposed();

            return (GpioPinValue)_provider.Read();
        }

        /// <summary>
        /// Drives the specified value onto the general purpose I/O (GPIO) pin according to the current drive mode for
        /// the pin if the pin is configured as an output, or updates the latched output value for the pin if the pin is
        /// configured as an input.
        /// </summary>
        /// <param name="value">The enumeration value to write to the GPIO pin.
        ///     <para>If the GPIO pin is configured as an output, the method drives the specified value onto the pin
        ///         according to the current drive mode for the pin.</para>
        ///     <para>If the GPIO pin is configured as an input, the method updates the latched output value for the pin.
        ///         The latched output value is driven onto the pin when the configuration for the pin changes to
        ///         output.</para></param>
        /// <remarks>If the pin drive mode is not currently set to output, this will latch <paramref name="value"/>
        ///     and drive the signal the when the mode is set.</remarks>
        public void Write(GpioPinValue value)
        {
            ThrowIfDisposed();

            // Remember this value in case we switch drive mode
            _lastOutputValue = value;

            if (_driveMode == GpioPinDriveMode.Output)
            {
                _provider.Write(value);
            }
        }

        /// <summary>
        /// Gets whether the general-purpose I/O (GPIO) pin supports the specified drive mode.
        /// </summary>
        /// <param name="driveMode">The drive mode to check for support.</param>
        /// <returns>True if the GPIO pin supports the drive mode that driveMode specifies; otherwise false. If you
        ///     specify a drive mode for which this method returns false when you call SetDriveMode, SetDriveMode
        ///     generates an exception.</returns>
        public bool IsDriveModeSupported(GpioPinDriveMode driveMode)
        {
            switch (driveMode)
            {
                case GpioPinDriveMode.Input:
                case GpioPinDriveMode.Output:
                case GpioPinDriveMode.InputPullUp:
                case GpioPinDriveMode.InputPullDown:
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the current drive mode for the general-purpose I/O (GPIO) pin. The drive mode specifies whether the pin
        /// is configured as an input or an output, and determines how values are driven onto the pin.
        /// </summary>
        /// <returns>An enumeration value that indicates the current drive mode for the GPIO pin. The drive mode
        ///     specifies whether the pin is configured as an input or an output, and determines how values are driven
        ///     onto the pin.</returns>
        public GpioPinDriveMode GetDriveMode()
        {
            ThrowIfDisposed();

            return _driveMode;
        }

        /// <summary>
        /// Sets the drive mode of the general-purpose I/O (GPIO) pin. The drive mode specifies whether the pin is
        /// configured as an input or an output, and determines how values are driven onto the pin.
        /// </summary>
        /// <param name="driveMode">An enumeration value that specifies drive mode to use for the GPIO pin. The drive
        ///     mode specifies whether the pin is configured as an input or an output, and determines how values are
        ///     driven onto the pin.</param>
        public void SetDriveMode(GpioPinDriveMode driveMode)
        {
            ThrowIfDisposed();

            if (driveMode != _driveMode)
            {
                _provider.SetPinDriveMode((GpioDriveMode)driveMode);
                if (driveMode == GpioPinDriveMode.Output)
                {
                    _provider.Write(_lastOutputValue);
                }

                _driveMode = driveMode;
            }
        }

        /// <summary>
        /// Closes the general-purpose I/O (GPIO) pin and releases the resources associated with it.
        /// </summary>
        public void Dispose()
        {
            if (_provider != null)
            {
                Dispose(true);
                _provider = null;
                GC.SuppressFinalize(this);
            }
        }

        private void HandlePinChangedEvent(GpioPin sender, GpioPinValueChangedEventArgs eventArgs)
        {
            if (_provider != null)
            {
                _callbacks?.Invoke(this, eventArgs);
            }
        }

        /// <summary>
        /// Releases internal resources held by the GPIO pin.
        /// </summary>
        /// <param name="disposing">True if called from Dispose, false if called from the finalizer.</param>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_provider != null)
                {
                    _provider.Dispose();
                }
            }
        }

        private void ThrowIfDisposed()
        {
            if (_provider == null)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
        }
    }
}
