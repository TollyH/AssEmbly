; This file should not be assembled, it should only be imported into other AssEmbly files

%ASM_ONCE

:FUNC_INPUT
; Destination address given as parameter is in rfp
PSH rg0  ; Store value of rg0 so it isn't overwritten permanently
:FUNC_INPUT_READ
RCC rg0  ; Read a single character from the console into rg0
CMP rg0, '\n'  ; Check if rg0 is a newline
JEQ :FUNC_INPUT_RETURN  ; If it is, return from subroutine
MVB *rfp, rg0  ; Otherwise, move the inputted character (rg0), to the destination address (*rfp)
ICR rfp  ; Increment the destination address by 1
JMP :FUNC_INPUT_READ  ; Loop asking for character
:FUNC_INPUT_RETURN
MVB *rfp, 0 ; Terminate the inputted string with a null character
POP rg0  ; Restore value of rg0 to what it was prior to function call
RET  ; Return from function with no value, it is already in the provided destination address
