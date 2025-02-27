

# Specify the kernel version so the build config file knows the kernel addresses have been defined.
.set KRNL_VER,              17559

# Kernel function addresses:
.set DbgPrint,                          0x80085EE8
.set DbgBreakPoint,                     0x80085EA0
.set HalSendSMCMessage,                 0x80067F48
.set KeFlushCacheRange,                 0x80073850
.set KeLockL2,                          0x80071E00
.set KeStallExecutionProcessor,         0x80073484
.set MmFreePhysicalMemory,				0x800807B8
.set MmGetPhysicalAddress,              0x80080048
.set NtAllocateVirtualMemory,           0x80083AA8
.set NtClose,                           0x80089EB0
.set ObCreateSymbolicLink,              0x8008AEF0
.set RtlInitAnsiString,                 0x80086110
.set VdDisplayFatalError,               0x800BDD40
.set XexLoadImage,                      0x8007D7C0
.set XexUnloadImage,                    0x8007D0E8

.set memcmp,                            0x80117200  # Can be substituted for XeCryptMemDiff if memcmp is not available (or anything with same signature and behavior) (do NOT use RtlCompareMemory, it doesn't return 0 on matching data)

# System call functions:
.set HvxKeysExGetKey,                   0x80108580
.set HvxKeysExSetKey,                   0x80108570
.set HvxEncryptedReserveAllocation,     0x80082CD0
.set HvxEncryptedReleaseAllocation,     0x80082D00
.set HvxEncryptedEncryptAllocation,     0x80082CE0
.set HvxFlushDCacheRange,               0x8007F968

# System call ordinals:
.set sc_HvxPostOutputExploit,           0x0D
.set sc_HvxFlushUserModeTb,             0x21
.set sc_HvxKeysExecute,                 0x42
.set sc_HvxEncryptedReserveAllocation,  0x49
.set sc_HvxEncryptedEncryptAllocation,  0x4A
.set sc_HvxEncryptedReleaseAllocation,  0x4C
.set sc_HvxRevokeUpdate,                0x65

.set sc_HvxArbWriteSyscall,             sc_HvxFlushUserModeTb

# Boot animation addresses:
.set BootAnimCodePageAddress,           0x98030000

# Xam function addresses:
.set CreateFileA,                       0x8171BF40  # Export 1095
.set GetFileSize,                       0x8171C7C0  # Export 1063
.set ReadFile,                          0x8171D2C0  # Export 1052
.set WriteFile,                         0x81722268  # Export 1054
.set CloseHandle,                       0x8171B9A8  # Export 1044

.set CreateThread,                      0x8171C1B0  # Export 1084
.set ResumeThread,                      0x8171D498  # Export 1085
.set GetLastError,                      0x81721AC0  # Export 1006

.set memcpy,                            0x8172D590
.set memset,                            0x8172D4F0

.set XamLoaderLaunchTitle,              0x816A1820  # Export 420
.set XamLoaderTerminateTitle,           0x816A1458  # Export 425
.set XLaunchNewImage,                   XamLoaderLaunchTitle


###########################################################
# Kernel gadget address.

#   addi    r1, r1, 0xA0
#   b       __restgprlr_24
.set    __restgprlr_24,                 0x800631A0      # .fill 0x58, 1, 0x00

#   addi    r1, r1, 0x90
#   b       __restgprlr_26
.set    __restgprlr_26,                 0x80062578      # .fill 0x58, 1, 0x00

#   addi    r1, r1, 0x80
#   b       __restgprlr_27
.set    __restgprlr_27,                 0x80061D50      # .fill 0x50, 1, 0x00

#   addi    r1, r1, 0x80
#   b       __restgprlr_28
.set    __restgprlr_28,                 0x8006148C      # .fill 0x58, 1, 0x00

#   addi    r1, r1, 0x70
#   b       __restgprlr_29
.set    __restgprlr_29,                 0x800619B4      # .fill 0x50, 1, 0x00

#   addi    r1, r1, 0x70
#   lwz     r12, -0x8(r1)
#   mtlr    r12
#   ld      r30, -0x18(r1)
#   ld      r31, -0x10(r1)
#   blr
.set    __restgprlr_30,                 0x80061538      # .fill 0x58, 1, 0x00

#   addi    r1, r1, 0x60
#   lwz     r12, -0x8(r1)
#   mtlr    r12
#   ld      r31, -0x10(r1)
#   blr
.set    __restgprlr_31,                 0x800664B0      # .fill 0x50, 1, 0x00

#   stw     r3, 0(r31)
#   addi    r1, r1, 0x60
#   lwz     r12, -0x8(r1)
#   mtlr    r12
#   ld      r31, -0x10(r1)
#   blr
.set    stw_r3,                         0x800D986C      # .fill 0x50, 1, 0x00

#   mr      r3, r31
#   addi    r1, r1, 0x70
#   lwz     r12, -8(r1)
#   mtlr    r12
#   ld      r31, -0x10(r1)
#   blr
.set    mr_r31_to_r3,                   0x800661E4      # .fill 0x60, 1, 0x00

#   mr      r11, r31
#   mr      r3, r11
#   addi    r1, r1, 0x70
#   lwz     r12, -8(r1)
#   mtlr    r12
#   ld      r30, -0x18(r1)
#   ld      r31, -0x10(r1)
#   blr
.set    mr_r31_to_r11,                  0x800C8748      # .fill 0x58, 1, 0x00

#   mtctr   r31
#   bctrl
#   addi    r1, r1, 0x60
#   lwz     r12, -8(r1)
#   mtlr    r12
#   ld      r31, -0x10(r1)
#   blr
.set    call_func_dispatch,             0x8007B0AC      # .fill 0x50, 1, 0x00


###########################################################
# Xam gadget address.

#   lwz     r1, 0(r1)
#   lwz     r12, -8(r1)
#   mtlr    r12
#   blr
.set    stack_pivot,                    0x81725378

#   lwz     r3, 0(r31)
#   addi    r1, r1, 0x60
#   lwz     r12, var_8(r1)
#   mtlr    r12
#   ld      r31, var_10(r1)
#   blr
.set    lwz_r3,                         0x816ABA5C      # .fill 0x50, 1, 0x00

#   lwz     r11, 0(r3)
#   stw     r11, 8(r4)
#   li      r3, 0
#   blr
.set    lwz_r3_stw_r4,                  0x817A27B0
.set    lwz_r3_stw_r4__r3_disp,         0               # Displacement for r3 load
.set    lwz_r3_stw_r4__r4_disp,         8               # Displacement for r4 store

#   lwz     r10, 0(r3)
#   slwi    r11, r11, 2
#   add     r3, r11, r10
#   blr
.set    lwz_r10,                        0x8196C574

#   lwz     r11, 8(r31)
#   addi    r3, r11, -1
#   addi    r1, r1, 0x70
#   lwz     r12, -8(r1)
#   mtlr    r12
#   ld      r30, -0x18(r1)
#   ld      r31, -0x10(r1)
#   blr
.set    lwz_r11_off_r31,                0x816A8D94      # .fill 0x58, 1, 0x00

#   stw     r30, 0(r31)
#   addi    r1, r1, 0x70
#   lwz     r12, -8(r1)
#   mtlr    r12
#   ld      r30, -0x18(r1)
#   ld      r31, -0x10(r1)
#   blr
.set    stw_r30_on_r31,                 0x816FCAAC      # .fill 0x58, 1, 0x00

#   lwz     r11, 4(r31)
#   stw     r3, 0(r11)
#   li      r3, 0
#   addi    r1, r1, 0x60
#   lwz     r12, -8(r1)
#   mtlr    r12
#   ld      r31, -0x10(r1)
#   blr
.set    stw_r3_onto_pointer,            0x81922120      # .fill 0x50, 1, 0x00

#   lwz     r10, 8(r11)
#   add     r10, r5, r10
#   stw     r10, 8(r11)
#   blr
.set    load_add_store_r10_r5_on_r11,   0x819D88A8

#   mr      r7, r25
#   mtctr   r30
#   mr      r6, r26
#   mr      r5, r27
#   mr      r4, r28
#   mr      r3, r29
#   bctrl
.set    call_func_preload,              0x8169CDDC

# Default register values for unused parameters to call_func_preload:
.set    cf_r3_def,                      0x29292929
.set    cf_r4_def,                      0x28282828
.set    cf_r5_def,                      0x27272727
.set    cf_r6_def,                      0x26262626
.set    cf_r7_def,                      0x25252525

# Offsets for low half of argument registers in CALL_FUNC_LABEL macro:
.set    cf_r3_offset,                   0x2C
.set    cf_r4_offset,                   0x24
.set    cf_r5_offset,                   0x1C
.set    cf_r6_offset,                   0x14
.set    cf_r7_offset,                   0x0C

#   mr      r3, r1
#   blr
.set    mr_r1_to_r3,                    0x817F4EC4

#   blr
.set    blr_nop,                        0x817F4EC8

#   cmplwi  r3, 0
#   li      r3, 0
#   beq     loc_817F031C
#       li      r3, 1
#
#   addi    r1, r1, 0x60
#   lwz     r12, -8(r1)
#   mtlr    r12
#   ld      r31, -0x10(r1)
#   blr
.set    clamp_r3,                       0x817F030C      # .fill 0x50, 1, 0x00

#   slwi    r10, r3, 2
#   addi    r11, r11, 0x3D64
#   lwzx    r3, r10, r11
#   blr
.set    mul_r3_4_lwzx_r11,              0x816D8864
.set    mul_r3_4_lwzx_r11__disp,        0x3D64

#   lwz     r11, 0x18(r31)
#   add     r11, r30, r11
#   stw     r11, 0x18(r31)
#   addi    r1, r1, 0x70
#   lwz     r12, -8(r1)
#   mtlr    r12
#   ld      r30, -0x18(r1)
#   ld      r31, -0x10(r1)
#   blr
.set    load_add_store_r11_r30_on_r31,          0x817F7DC8      # .fill 0x58, 1, 0x00
.set    load_add_store_r11_r30_on_r31__disp,    0x18

#   lwz     r11, 0(r31)
#   mtctr   r11
#   bctrl
.set    call_ptr_off_r31,                       0x81699DC8
