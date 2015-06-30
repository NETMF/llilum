#
# Project-specific settings
#
VERBOSE=1
TARGET = LPC1768
PROJECT = mbed_simple
OBJECTS = .\Microsoft.Zelig.Test.mbed.Simple_opt.o $(TARGET)\temporary_helper.o

#
# Paths and includes
# TODO: Make this algorithmic based on TARGET.
#

ifndef GCC_BIN
    $(error GCC_BIN is undefined. Define GCC_BIN to point to arm-none-eabi-xxx tools)
endif

MBED_ROOT = $(ZELIG_ROOT)external\targets\mbed
SYS_OBJECTS = $(MBED_ROOT)\TARGET_LPC1768\TOOLCHAIN_GCC_ARM\board.o $(MBED_ROOT)\TARGET_LPC1768\TOOLCHAIN_GCC_ARM\startup_LPC17xx.o $(MBED_ROOT)\TARGET_LPC1768\TOOLCHAIN_GCC_ARM\cmsis_nvic.o $(MBED_ROOT)\TARGET_LPC1768\TOOLCHAIN_GCC_ARM\retarget.o $(MBED_ROOT)\TARGET_LPC1768\TOOLCHAIN_GCC_ARM\system_LPC17xx.o
INCLUDE_PATHS = -I. -I$(MBED_ROOT) -I$(MBED_ROOT)\TARGET_LPC1768 -I$(MBED_ROOT)\TARGET_LPC1768\TOOLCHAIN_GCC_ARM -I$(MBED_ROOT)\TARGET_LPC1768\TARGET_NXP -I$(MBED_ROOT)\TARGET_LPC1768\TARGET_NXP\TARGET_LPC176X -I$(MBED_ROOT)\TARGET_LPC1768\TARGET_NXP\TARGET_LPC176X\TARGET_MBED_LPC1768
LIBRARY_PATHS = -L$(MBED_ROOT)\TARGET_LPC1768\TOOLCHAIN_GCC_ARM
LIBRARIES = -lmbed
LINKER_SCRIPT = $(MBED_ROOT)\TARGET_LPC1768\TOOLCHAIN_GCC_ARM\LPC1768.ld
DEPS = $(OBJECTS:.o=.d)

#
# Build tools
#

AS      = "$(GCC_BIN)arm-none-eabi-as"
CC      = "$(GCC_BIN)arm-none-eabi-gcc"
CPP     = "$(GCC_BIN)arm-none-eabi-g++"
LD      = "$(GCC_BIN)arm-none-eabi-gcc"
OBJCOPY = "$(GCC_BIN)arm-none-eabi-objcopy"
OBJDUMP = "$(GCC_BIN)arm-none-eabi-objdump"
SIZE    = "$(GCC_BIN)arm-none-eabi-size"

#
# Build flags
#

CPU = -mcpu=cortex-m3 -mthumb
CC_FLAGS = $(CPU) -c -g -fno-common -fmessage-length=0 -Wall -fno-exceptions -ffunction-sections -fdata-sections -fomit-frame-pointer -MMD -MP
CC_SYMBOLS = -DTARGET_LPC1768 -DTARGET_M3 -DTARGET_CORTEX_M -DTARGET_NXP -DTARGET_LPC176X -DTARGET_MBED_LPC1768 -DTOOLCHAIN_GCC_ARM -DTOOLCHAIN_GCC -D__CORTEX_M3 -DARM_MATH_CM3 -DMBED_BUILD_TIMESTAMP=1435185689.48 -D__MBED__=1
LD_FLAGS = $(CPU) -Wl,--gc-sections --specs=nano.specs -u _printf_float -u _scanf_float -Wl,--wrap,main -Wl,-Map=$(TARGET)\$(PROJECT).map,--cref
LD_SYS_LIBS = -lstdc++ -lsupc++ -lm -lc -lgcc -lnosys

ifeq ($(DEBUG), 1)
    CC_FLAGS += -DDEBUG -O0
else
    CC_FLAGS += -DNDEBUG -Os
endif

#
# Recipes
#

.PHONY: all clean lst size

all: ${TARGET} $(TARGET)\$(PROJECT).bin $(TARGET)\$(PROJECT).hex $(TARGET)\$(PROJECT).lst $(TARGET)\$(PROJECT).disasm size

# Conditionals (ifeq/ifneq) are bugged on Windows. Instead, we'll just ensure the directory exists and remove it unconditionally.
clean: ${TARGET}
	@rmdir /q /s $(TARGET)

# Ensures the target directory has been created.
${TARGET}:
	@mkdir $@

$(TARGET)\\%.o: %.s
	$(AS) $(CPU) -o $@ $<

$(TARGET)\\%.o: %.c
	$(CC)  $(CC_FLAGS) $(CC_SYMBOLS) -std=gnu99   $(INCLUDE_PATHS) -o $@ $<

$(TARGET)\\%.o: %.cpp
	$(CPP) $(CC_FLAGS) $(CC_SYMBOLS) -std=gnu++98 -fno-rtti $(INCLUDE_PATHS) -o $@ $<

$(TARGET)\$(PROJECT).elf: $(OBJECTS) $(SYS_OBJECTS)
	$(LD) $(LD_FLAGS) -T$(LINKER_SCRIPT) $(LIBRARY_PATHS) -o $@ $^ $(LIBRARIES) $(LD_SYS_LIBS) $(LIBRARIES) $(LD_SYS_LIBS)

$(TARGET)\$(PROJECT).bin: $(TARGET)\$(PROJECT).elf
	$(OBJCOPY) -O binary $< $@

$(TARGET)\$(PROJECT).hex: $(TARGET)\$(PROJECT).elf
	@$(OBJCOPY) -O ihex $< $@

$(TARGET)\$(PROJECT).lst: $(TARGET)\$(PROJECT).elf
	@$(OBJDUMP) -Sdh $< > $@

$(TARGET)\$(PROJECT).disasm: $(TARGET)\$(PROJECT).elf
	@$(OBJDUMP) -D $< > $@

size: $(TARGET)\$(PROJECT).elf
	@echo Size report...
	$(SIZE) $<
