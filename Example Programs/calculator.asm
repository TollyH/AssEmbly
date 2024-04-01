%MACRO INPUT_BUFFER_SIZE, 24

; rg0 - first number
; rg1 - second number
; rg2 - loop address
; rg3 - working loop value
; rg4 - operator
; rg5 - remainder (if dividing)
; rg6 - non-zero if negative
; rg7 - input buffer address
HEAP_ALC rg7, INPUT_BUFFER_SIZE  ; Allocate a free region in memory to store input

CAL :FUNC_PRINT, :&STR_NUM_1_PROMPT
CAL :FUNC_INPUT, rg7

MVQ rg2, rg7
:NUM_1_READ_LOOP
; Parse the first input from ASCII to decimal in rg0
MVB rg3, *rg2
TST rg3, rg3  ; Check for 0-byte terminator
JZO :NUM_1_READ_LOOP_END
CMP rg3, '-'  ; Check for negative sign
JNE :NUM_1_PARSE
NOT rg6
ICR rg2
JMP :NUM_1_READ_LOOP
:NUM_1_PARSE
SUB rg3, '0'  ; Convert ASCII digit to number
MUL rg0, 10
ADD rg0, rg3
ICR rg2
JMP :NUM_1_READ_LOOP

:NUM_1_READ_LOOP_END
TST rg6, rg6
JZO :NUM_1_NO_NEGATE
SIGN_NEG rg0
:NUM_1_NO_NEGATE
CAL :FUNC_PRINT, :&STR_NUM_2_PROMPT
CAL :FUNC_INPUT, rg7

XOR rg6, rg6
MVQ rg2, rg7
:NUM_2_READ_LOOP
; Parse the first input from ASCII to decimal in rg0
MVB rg3, *rg2
TST rg3, rg3  ; Check for 0-byte terminator
JZO :NUM_2_READ_LOOP_END
CMP rg3, '-'  ; Check for negative sign
JNE :NUM_2_PARSE
NOT rg6
ICR rg2
JMP :NUM_2_READ_LOOP
:NUM_2_PARSE
SUB rg3, '0'  ; Convert ASCII digit to number
MUL rg1, 10
ADD rg1, rg3
ICR rg2
JMP :NUM_2_READ_LOOP

:NUM_2_READ_LOOP_END
TST rg6, rg6
JZO :NUM_2_NO_NEGATE
SIGN_NEG rg1
:NUM_2_NO_NEGATE
CAL :FUNC_PRINT, :&STR_OPERATOR_PROMPT
RCC rg4
WCC rg4  ; Echo typed character
WCC '\n'  ; Write newline after input

CMP rg4, '+'
JEQ :ADDITION
CMP rg4, '-'
JEQ :SUBTRACTION
CMP rg4, '*'
JEQ :MULTIPLICATION
CMP rg4, '/'
JEQ :DIVISION
JMP :INVALID_OPERATOR

:ADDITION
ADD rg0, rg1
JMP :PRINTOUT

:SUBTRACTION
SUB rg0, rg1
JMP :PRINTOUT

:MULTIPLICATION
MUL rg0, rg1
JMP :PRINTOUT

:DIVISION
SIGN_DVR rg0, rg5, rg1
JMP :PRINTOUT

:INVALID_OPERATOR
CAL :FUNC_PRINT, :&STR_INVALID_OPERATOR
JMP :NUM_2_READ_LOOP_END

:PRINTOUT
SIGN_WCN rg0
TST rg5, rg5
JZO :END
; If remainder in not 0, print it (will only happen when dividing)
CAL :FUNC_PRINT, :&STR_REMAINDER
SIGN_WCN rg5
:END
HEAP_FRE rg7  ; Free the memory allocated for input before the program halts
HLT  ; Stop program to prevent execution of data as executable code

:STR_NUM_1_PROMPT
%DAT "Enter first number > \0"

:STR_NUM_2_PROMPT
%DAT "Enter second number > \0"

:STR_OPERATOR_PROMPT
%DAT "Enter operator (+, -, *, /) > \0"

:STR_INVALID_OPERATOR
%DAT "The entered operator was invalid\n\0"

:STR_REMAINDER
%DAT " remainder \0"

%IMP "input.ext.asm"  ; Import input function
%IMP "print.ext.asm"  ; Import print function
