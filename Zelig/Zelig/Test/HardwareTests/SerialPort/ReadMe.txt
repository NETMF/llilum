The SerialPortTest project contains hardware tests related to the serial port.

GPIO tests:
  1. SerialPort Loop-back Test
     This test validates the interrupt driven serial port implementation by sending and receiving data in a loop-back scenario.
     In order for this test to work properly, the TX/RX pins of the selected UART must be physically connected to create a hardware
     loop-back.  The test currently sends/receives data in a loop indefinitely with a 2 second delay in between iterations.  The
     following is the pin loop-back pin map for the LPC1768 and the FRDM K64F boards:

     a. LPC1768 (UART1)
        RX = LPC1768.PinName.p14
        TX = LPC1768.PinName.p13

     b. FDRM K64F (UART3)
        RX = K64F.PinName.PTC16 (J1 Pin2)
        TX = K64F.PinName.PTC17 (J1 Pin4)
        