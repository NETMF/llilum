#include "gpio_api.h"
#include "core_cmFunc.h"
#include <stdlib.h>

void tmp_gpio_write(gpio_t *obj, int value)
{
    gpio_write(obj, value);
}

int tmp_gpio_read(gpio_t *obj)
{
    return gpio_read(obj);
}

void tmp_gpio_alloc(gpio_t **obj)
{
    *obj = calloc(sizeof(gpio_t), 1);
}

void tmp_gpio_free(gpio_t *obj)
{
    free(obj);
}

unsigned char *callocWrapper( unsigned num, unsigned size )
{
    return (unsigned char *) calloc( num, size );
}

void SetPriMaskRegister( unsigned primask )
{
    __set_PRIMASK(primask);
}

unsigned GetPriMaskRegister( )
{
    return __get_PRIMASK();
}

void SetFaultMaskRegister( unsigned primask )
{
    __set_FAULTMASK(primask);
}

unsigned GetFaultMaskRegister( )
{
    return __get_FAULTMASK( );
}

int GetANumber() 
{ 
    // 
    // we will get max a 24 bit numer, never smaller than 42 
    // 
    const int max = ( 1 << 24 ) - 1; 
 
    return (rand() % max) + 42; 
} 

void BreakWithTrap( ) 
{ 
	__builtin_trap(); 
} 

void Break() 
{ 
	asm("bkpt"); 
} 

void Breakpoint( unsigned n ) 
{ 
	volatile unsigned int valueToWatch = n; 
	
	Break( ); 
} 

void Nop() 
{ 
	asm("nop"); 
} 

#include <stdio.h>

void mbedPrint(const char *str)
{
  printf("%s", str);
}


//
//
//

void* CreateNativeContext( void* entryPoint, void* stack, int stackSize )
{
    return NULL;
}

void Yield( void* nativeContext )
{
}

void Retire( void* nativeContext )
{
}

void SwitchToContext( void* nativeContext )
{
}

void* GetPriority( void* nativeContext )
{
    return 0;
}

void SetPriority( void* nativeContext, void* priority )
{
}
		
//
//
//
