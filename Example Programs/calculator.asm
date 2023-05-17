; rg0 - first number
; rg1 - second number
; rg2 - loop address
; rg3 - working loop value
; rg4 - operator
; rg5 - remainder (if dividing)
CAL :FUNC_PRINT, :&STR_NUM_1_PROMPT
CAL :FUNC_INPUT, :&INPUT_BUFFER

MVQ rg2, :&INPUT_BUFFER
:NUM_1_READ_LOOP
; Parse the first input from ASCII to decimal in rg0
MVB rg3, *rg2
CMP rg3, 0  ; Check for 0-byte terminator
JEQ :NUM_1_READ_LOOP_END
SUB rg3, 48  ; Convert ASCII digit to number
MUL rg0, 10
ADD rg0, rg3
ICR rg2
JMP :NUM_1_READ_LOOP

:NUM_1_READ_LOOP_END
CAL :FUNC_PRINT, :&STR_NUM_2_PROMPT
CAL :FUNC_INPUT, :&INPUT_BUFFER

MVQ rg2, :&INPUT_BUFFER
:NUM_2_READ_LOOP
; Parse the first input from ASCII to decimal in rg0
MVB rg3, *rg2
CMP rg3, 0  ; Check for 0-byte terminator
JEQ :NUM_2_READ_LOOP_END
SUB rg3, 48  ; Convert ASCII digit to number
MUL rg1, 10
ADD rg1, rg3
ICR rg2
JMP :NUM_2_READ_LOOP

:NUM_2_READ_LOOP_END
CAL :FUNC_PRINT, :&STR_OPERATOR_PROMPT
RCC rg4
WCC 10  ; Write newline after input

CMP rg4, 43  ; '+'
JEQ :ADDITION
CMP rg4, 45  ; '-'
JEQ :SUBTRACTION
CMP rg4, 42  ; '*'
JEQ :MULTIPLICATION
CMP rg4, 47  ; '/'
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
DVR rg0, rg5, rg1
JMP :PRINTOUT

:INVALID_OPERATOR
CAL :FUNC_PRINT, :&STR_INVALID_OPERATOR
JMP :NUM_2_READ_LOOP_END

:PRINTOUT
WCN rg0
CMP rg5, 0
JEQ :END
; If remainder in not 0, print it (will only happen when dividing)
CAL :FUNC_PRINT, :&STR_REMAINDER
WCN rg5
:END
HLT  ; Stop program to prevent execution of data as executable code

:INPUT_BUFFER
PAD 256

:STR_NUM_1_PROMPT
DAT "Enter first number > "
DAT 0  ; Terminate string with 0 byte

:STR_NUM_2_PROMPT
DAT "Enter second number > "
DAT 0  ; Terminate string with 0 byte

:STR_OPERATOR_PROMPT
DAT "Enter operator (+, -, *, /) > "
DAT 0  ; Terminate string with 0 byte

:STR_INVALID_OPERATOR
DAT "The entered operator was invalid"
DAT 10  ; Newline
DAT 0  ; Terminate string with 0 byte

:STR_REMAINDER
DAT " remainder "
DAT 0  ; Terminate string with 0 byte

IMP "input.ext.asm"  ; Import input function
IMP "print.ext.asm"  ; Import print function
