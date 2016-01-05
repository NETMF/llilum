    .text
    .thumb
    .thumb_func
    .align 2
    .globl   LLOS_CLOCK_DelayCycles
    .type    LLOS_CLOCK_DelayCycles, %function

LLOS_CLOCK_DelayCycles:
    MOV     R2, #4      // Per loop cycles
    SUB     R0, R0, #8  // Startup cycles (including inter-op instructions)

START_LOOP:
    SUB     R0, R0, R2
    CMP     R0, #0
    BGT     START_LOOP
    MOV     R0, #0
    BX      LR

    .end

