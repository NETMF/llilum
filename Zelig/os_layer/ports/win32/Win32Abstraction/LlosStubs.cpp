#include "LlosWin32.h"

extern "C"
{
    uint32_t CMSIS_STUB_SCB__get_CONTROL()
    {
        return 0;
    }

    uint32_t CMSIS_STUB_SCB__get_BASEPRI()
    {
        return 0;
    }

    uint32_t CMSIS_STUB_SCB__set_BASEPRI(uint32_t basePri)
    {
        return 0;
    }

    void CUSTOM_STUB_SCB_SCR_SetSystemControlRegister(uint32_t scr)
    {
    }

    void CUSTOM_STUB_SCB_set_CCR(uint32_t value)
    {
    }

    void CUSTOM_STUB_SCB_SHCRS_EnableSystemHandler(uint32_t ex)
    {
    }

    void CUSTOM_STUB_RaiseSupervisorCallForLongJump()
    {
    }

    uint32_t CUSTOM_STUB_SCB__get_FPCCR()
    {
        return 0;
    }

    void CUSTOM_STUB_SCB__set_FPCCR(uint32_t fpscr)
    {
    }

    void Breakpoint()
    {
    }

    uint32_t CUSTOM_STUB_DebuggerConnected()
    {
        return 0;
    }

    uint32_t CUSTOM_STUB_SCB__get_CFSR()
    {
        return 0;
    }

    uint32_t CUSTOM_STUB_SCB__get_HFSR()
    {
        return 0;
    }

    uint32_t CUSTOM_STUB_SCB__get_MMFAR()
    {
        return 0;
    }

    uint32_t CUSTOM_STUB_SCB__get_BFAR()
    {
        return 0;
    }

    uint32_t* CUSTOM_STUB_FetchSoftwareFrameSnapshot()
    {
        return nullptr;
    }

    void CMSIS_STUB_SCB__set_PSP(uint32_t topOfProcStack)
    {
    }

    void CUSTOM_STUB_SetExcReturn(uint32_t ret)
    {
    }

    void CUSTOM_STUB_RaiseSupervisorCallForStartThreads()
    {
    }

    void CUSTOM_STUB_RaiseSupervisorCallForRetireThread()
    {
    }

    void CUSTOM_STUB_RaiseSupervisorCallForSnapshotProcessModeRegisters()
    {
    }

    uint32_t CMSIS_STUB_SCB__get_FAULTMASK()
    {
        return 0;
    }

    uint32_t CMSIS_STUB_SCB__get_PRIMASK()
    {
        return 0;
    }

    uint32_t CMSIS_STUB_SCB__set_PRIMASK(uint32_t priMask)
    {
        return 0;
    }
    
    uint32_t CUSTOM_STUB_SCB_IPSR_GetCurrentISRNumber()
    {
        return 0;
    }

    uint32_t us_ticker_read()
    {
        // This function will only be included and called if the test application was
        // compiledfor a board other than Win32. You should verify that program.cs in
        // the directory: zelig\zelig\test\mbed\simple\ defines WIN32 at the top of the
        // file.
        DebugBreak();
        return 0;
    }
}