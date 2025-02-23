
#---------------------------------------------------------
# Symbol map consumed by the linker.
#---------------------------------------------------------

# Compiler flags, must be set before any code or include directives:
.include "BuildConfig.asm"

# Emit a symbol table that can be consumed by the compiler to link to known functions:
#
#   FuncName = 0xAABBCCDD;
#

.macro NIBBLE_TO_CHAR val
    .if \val > 9
        .byte 'A' + (\val - 0xA)
    .else
        .byte '0' + \val
    .endif
.endm

.macro DWORD_TO_STR val
    NIBBLE_TO_CHAR (\val >> 28) & 0xF
    NIBBLE_TO_CHAR (\val >> 24) & 0xF
    NIBBLE_TO_CHAR (\val >> 20) & 0xF
    NIBBLE_TO_CHAR (\val >> 16) & 0xF
    NIBBLE_TO_CHAR (\val >> 12) & 0xF
    NIBBLE_TO_CHAR (\val >> 8) & 0xF
    NIBBLE_TO_CHAR (\val >> 4) & 0xF
    NIBBLE_TO_CHAR (\val >> 0) & 0xF
.endm

.macro SYMBOL_ENTRY name
    .ascii "\name = 0x"
    DWORD_TO_STR \name
    .ascii ";\n"
.endm

# Note: Do NOT define symbol addresses in this file! They must be defined as part of the kernel/game configs
#   and be supported by all targets/games supported.

# Symbol table:
SYMBOL_ENTRY XSetThreadProcessor
SYMBOL_ENTRY XPhysicalAlloc
SYMBOL_ENTRY CreateFileA
SYMBOL_ENTRY GetFileSize
SYMBOL_ENTRY ReadFile
SYMBOL_ENTRY CreateThread
SYMBOL_ENTRY ResumeThread
SYMBOL_ENTRY GetLastError
SYMBOL_ENTRY XLaunchNewImage
SYMBOL_ENTRY memcpy
SYMBOL_ENTRY memset
