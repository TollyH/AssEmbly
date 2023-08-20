; rg0 - first number
; rg1 - second number
; rg2 - loop address
; rg3 - working loop value
; rg4 - operator
; rg5 - remainder (if dividing)
CAL :FUNC_PRINT, :&STR_NUM_1_PROMPT
CAL :FUNC_INPUT, :&INPUT_BUFFER_1

MVQ rg2, :&INPUT_BUFFER_1
:NUM_1_READ_LOOP
; Parse the first input from ASCII to decimal in rg0
MVB rg3, *rg2
TST rg3, rg3  ; Check for 0-byte terminator
JZO :NUM_1_READ_LOOP_END
SUB rg3, 48  ; Convert ASCII digit to number
MUL rg0, 10
ADD rg0, rg3
ICR rg2
JMP :NUM_1_READ_LOOP

:NUM_1_READ_LOOP_END
CAL :FUNC_PRINT, :&STR_NUM_2_PROMPT
CAL :FUNC_INPUT, :&INPUT_BUFFER_2

MVQ rg2, :&INPUT_BUFFER_2
:NUM_2_READ_LOOP
; Parse the first input from ASCII to decimal in rg0
MVB rg3, *rg2
TST rg3, rg3  ; Check for 0-byte terminator
JZO :NUM_2_READ_LOOP_END
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
JNC :NOT_NEGATIVE
; If result is negative, print the '-' sign along with the absolute result value
WCC 45  ; '-'
NOT rg0
ICR rg0
:NOT_NEGATIVE
WCN rg0
TST rg5, rg5
JZO :END
; If remainder in not 0, print it (will only happen when dividing)
CAL :FUNC_PRINT, :&STR_REMAINDER
WCN rg5
:END
HLT  ; Stop program to prevent execution of data as executable code

:INPUT_BUFFER_1
PAD 64
:INPUT_BUFFER_2
PAD 64

:STR_NUM_1_PROMPT
DAT "Enter first number > \0"

:STR_NUM_2_PROMPT
DAT "Enter second number > \0"

:STR_OPERATOR_PROMPT
DAT "Enter operator (+, -, *, /) > \0"

:STR_INVALID_OPERATOR
DAT "The entered operator was invalid\n\0"

:STR_REMAINDER
DAT " remainder \0"

IMP "input.ext.asm"  ; Import input function
IMP "print.ext.asm"  ; Import print function
