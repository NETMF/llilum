
==================================
 CMSIS-RTOS API compliant library
==================================

Please read about CMSIS-RTOS specification here: https://www.keil.com/pack/doc/CMSIS/RTOS/html/index.html
Also please see documentaiton for CMSIS-RTOS API here: https://www.keil.com/pack/doc/CMSIS/RTOS/html/group___c_m_s_i_s___r_t_o_s.html


A CMSIS-RTOS compliant port is a library and a header file that matches the following template: https://www.keil.com/pack/doc/CMSIS/RTOS/html/cmsis__os_8h.html. 

You can read here as to how use a CMSIS-RTOS implementation: https://www.keil.com/pack/doc/CMSIS/RTOS/html/_using_o_s.html 

LLILUM CMSIS-RTOS implementation is an assembly that exports static methods from the Microsoft.Zelig.LlilumOSAbstraction.API.CmsisRTOS class. The function names match the corresponding names in CMSIS-RTOS/ 
A C-wrapper could easily be provided to make it a formally compliant port.