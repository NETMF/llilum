#include "mbed.h"
#include "rtos.h"
#include "Thread.h"

//
// To test hardware generated interrupt handling comment out this #define and connect a function generator to the input pin
// (LPC1768: p9, K64F: PTC7).  Also place a oscilloscope probe on the monitor pin (LPC1768: p11, K64F: PTC0).
//
#define TEST_SOFTWARE_INTERRUPTS 1
 
#if TARGET_LPC1768
#if TEST_SOFTWARE_INTERRUPTS
DigitalOut  pinOut(P0_1);
#endif
InterruptIn pinIn(P0_0);
DigitalOut  pinMonitor(P0_18);
#elif TARGET_K64F
#if TEST_SOFTWARE_INTERRUPTS
DigitalOut  pinOut(PTC5);
#endif
InterruptIn pinIn(PTC7);
DigitalOut  pinMonitor(PTC0);
#else
!Error No target defined.
#endif

const int SIGNAL_MAIN_THREAD     = 0x4321;
const int SIGNAL_DISPATCH_THREAD = 0x1234;
const int SIGNAL_WAIT_FOREVER    = 0x1000;

struct Dispatcher;

Mutex          dispatcher_lock;
Mutex          gpio_lock;
Dispatcher    *s_pDispatcherInst    = NULL;
Queue<int, 16> gpio_interrupt_queue;
Thread        *p_main_thread        = NULL;
Thread        *p_dispatcher_thread  = NULL;
#if !TEST_SOFTWARE_INTERRUPTS
int            g_monitor_value = 1;
#endif

void main_thread_proc(void const *args);
void dispatcher_thread_proc(void const *args);

typedef void (*PIN_CHANGED_HANDLER)(int edge);

void handle_event(int event)
{
#if TEST_SOFTWARE_INTERRUPTS
    pinMonitor = 0;
    p_main_thread->signal_set(SIGNAL_MAIN_THREAD);
#else
    pinMonitor = g_monitor_value;
    g_monitor_value = !g_monitor_value;
#endif
}

struct Dispatcher
{
    static Mutex m_lock;
    PIN_CHANGED_HANDLER m_callback;

    void RegisterForCallback( PIN_CHANGED_HANDLER callback )
    {
        m_callback = callback;
    }
    
    PIN_CHANGED_HANDLER GetCallback()
    {
        return m_callback;
    }
    
    static Dispatcher *GetInstance()
    {
        if( s_pDispatcherInst == NULL )
        {
            dispatcher_lock.lock();
            
            if( s_pDispatcherInst == NULL )
            {
                s_pDispatcherInst = new Dispatcher();
            }
            
            dispatcher_lock.unlock();
        }
        
        return s_pDispatcherInst;
    }

    static void dispatcher_thread_proc(void const *args) 
    {
        while(p_main_thread == NULL)
        {
            wait(1.0);
        }
    
        p_main_thread->signal_set(SIGNAL_MAIN_THREAD);
        
        while (true) 
        {
            Thread::signal_wait(SIGNAL_DISPATCH_THREAD);
            gpio_lock.lock();
            osEvent evt = gpio_interrupt_queue.get();
            gpio_lock.unlock();
            
            Dispatcher *pInst = Dispatcher::GetInstance();
            PIN_CHANGED_HANDLER callback = pInst->GetCallback();
            
            if( callback != NULL)
            {
                callback((int)evt.value.p);
            }
        }
    }
};

void OnRise(void)
{
    int evt = pinIn;
        
    gpio_lock.lock();
    gpio_interrupt_queue.put((int*)evt);
    gpio_lock.unlock();
    p_dispatcher_thread->signal_set(SIGNAL_DISPATCH_THREAD);
}

void main_thread_proc(void const *args) 
{
    Thread::signal_wait(SIGNAL_MAIN_THREAD);
    
    pinMonitor = 0;
#if TEST_SOFTWARE_INTERRUPTS
    pinOut     = 0;
#else
    pinIn.fall( &OnRise );
#endif
    pinIn.rise( &OnRise );
    
    while(1) 
    {
#if TEST_SOFTWARE_INTERRUPTS
        pinMonitor = 1;
        pinOut     = 1;
#endif
        Thread::signal_wait(SIGNAL_MAIN_THREAD);
        
#if TEST_SOFTWARE_INTERRUPTS
        pinOut = 0;
        wait(2.0);
#endif
    }
}

int main()
{
    Dispatcher *pDisp = Dispatcher::GetInstance();
    pDisp->RegisterForCallback( handle_event );
    Thread dispatcher_thread(Dispatcher::dispatcher_thread_proc);
    Thread main_thread(main_thread_proc);
    
    p_dispatcher_thread = &dispatcher_thread;
    p_main_thread       = &main_thread;
    
    Thread::signal_wait(SIGNAL_WAIT_FOREVER);
}
