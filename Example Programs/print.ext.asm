; This file should not be assembled, it should only be imported into other AssEmbly files

%ASM_ONCE

:FUNC_PRINT
; Prints an entire 0-terminated string to the console
; Address of string given as parameter is in rfp
PSH rg0  ; Preserve value of rg0
:FUNC_PRINT_LOOP
MVB rg0, *rfp  ; Move contents of address stored in rfp to rg0
TST rg0, rg0  ; Check if rg0 is 0
JZO :FUNC_PRINT_RETURN  ; If it is, return from subroutine
ICR rfp  ; Otherwise, increment source address by 1
WCC rg0  ; Write the character in rg0 to the console
JMP :FUNC_PRINT_LOOP  ; Loop back to print next character
:FUNC_PRINT_RETURN
POP rg0  ; Restore value of rg0 to what it was prior to function call
RET  ; Otherwise, return from subroutine
