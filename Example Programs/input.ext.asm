; This file should not be assembled, it should only be imported into other AssEmbly files

%ASM_ONCE

:FUNC_INPUT
; Destination address given as parameter is in rfp
PSH rg0  ; Store value of rg0 so it isn't overwritten permanently
PSH rg1  ; Do the same for rg1

MVQ rg1, rfp  ; Store the destination address in rg1

:FUNC_INPUT_READ
RCC rg0  ; Read a single character from the console into rg0

CMP rg0, '\b'  ; Check if rg0 is a backspace
JNE :FUNC_INPUT_STORE  ; If it isn't, jump straight to storing character
CMP rg1, rfp  ; Check that we're not already at the beginning of the buffer
JLE :FUNC_INPUT_READ  ; If we are, jump straight back to reading next character
DCR rg1  ; Otherwise, move back a character in the input buffer so that it's overwritten
WCC '\b'  ; Erase the character in the console by overwriting with a space
WCC ' '
WCC '\b'
JMP :FUNC_INPUT_READ  ; Jump back to reading next character

:FUNC_INPUT_STORE
WCC rg0  ; Write the character back to the console so the user can see what they've typed

CMP rg0, '\n'  ; Check if rg0 is a newline
JEQ :FUNC_INPUT_RETURN  ; If it is, return from subroutine

MVB *rg1, rg0  ; Otherwise, move the inputted character (rg0), to the destination address (*rg1)
ICR rg1  ; Increment the destination address by 1
JMP :FUNC_INPUT_READ  ; Loop asking for character

:FUNC_INPUT_RETURN
MVB *rg1, 0 ; Terminate the inputted string with a null character

POP rg1  ; Restore value of rg1 to what it was prior to function call
POP rg0  ; Do the same for rg0
RET  ; Return from function with no value, it is already in the provided destination address
