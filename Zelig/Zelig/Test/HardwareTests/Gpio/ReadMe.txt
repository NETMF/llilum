The GpioTest project contains hardware tests related to GPIOs.

GPIO tests:
  1. GPIO Interrupt Handling Latency
     This test enables the user to measuring GPIO interrupt handling latency.  The test uses 4 GPIO pins (including one LED).  
     The pins are labeled in the test class GpioInterruptTestData as PinLED, PinIn, PinOut, PinMon.  PinIn and PinOut must be
     physically connected with a jumper.  PinOut will drive the interrupt for PinIn.  PinMon is another output pin that is used
     to monitor the time between setting PinOut and receiving the interrupt for PinIn.  An oscilloscope can be connected to
     PinMon to roughly determine the time it takes to service a GPIO interrupt.  The PinLED pin is just used as a toggle for 
     each iteration of the GPIO interrupt test. The following shows the pin map for both the LPC1768 and the FRDM K64F boards:

     a. LPC1768
        PinLED = LPC1768.PinName.LED1
        PinIn  = LPC1768.PinName.p9
        PinOut = LPC1768.PinName.p10
        PinMon = LPC1768.PinName.p11

     b. FRDM K64F
        PinLED = K64F.PinName.LED1
        PinIn  = K64F.PinName.PTC7 (J1 Pin15)
        PinOut = K64F.PinName.PTC5 (J1 Pin13)
        PinMon = K64F.PinName.PTC0 (J1 Pin11)
        