MAC _ffe, 0b100  ; Create a macro for the file end flag

OFL :FILE_PATH
; rg0 - read character
; rg1 - x value
; rg2 - cycle value
; rg3 - signal strength sum
; rg4 - part two result pointer
; rg5 - int parse store
; rg6 - negative flag
; rg9 - character discard
MVQ rg1, 1
MVQ rg4, :&PART_TWO_RESULT
:READ_LOOP
RFC rg0
RFC rg9  ; Skip 3 characters (only first character in mnemonic matters)
RFC rg9
RFC rg9
CMP rg0, 'n'  ; (noop)
JEQ :NOOP
CAL :FUNC_INCREMENT_CYCLE, 2
XOR rg5, rg5
XOR rg6, rg6
RFC rg9  ; Discard space
:INT_PARSE_LOOP
RFC rg0
CMP rg0, '-'
JEQ :SET_NEGATIVE
CMP rg0, '\n'  ; Newline?
JEQ :INT_PARSE_END
SUB rg0, '0'  ; Convert ASCII digit to number
MUL rg5, 10
ADD rg5, rg0
JMP :INT_PARSE_LOOP
:SET_NEGATIVE
MVQ rg6, 1
JMP :INT_PARSE_LOOP
:INT_PARSE_END
TST rg6, rg6
JNZ :SUBTRACT
ADD rg1, rg5
JMP :READ_LOOP_CONDITION
:SUBTRACT
SUB rg1, rg5
JMP :READ_LOOP_CONDITION
:NOOP
CAL :FUNC_INCREMENT_CYCLE, 1
RFC rg9  ; Discard newline
:READ_LOOP_CONDITION
TST rsf, _ffe  ; End of file?
JZO :READ_LOOP
CFL
WCN rg3
WCC '\n'  ; Newline
; rg7 - part two print pointer
MVQ rg7, :&PART_TWO_RESULT
:PART_TWO_PRINT_LOOP
CMP rg7, rg4
JGE :END
WCC *rg7
ICR rg7
JMP :PART_TWO_PRINT_LOOP
:END
HLT

:FUNC_INCREMENT_CYCLE
; Takes increment amount as fast pass parameter
; rg0 - loop counter
; rg7 - difference between x and screen x
; rg8 - screen x value
; rg9 - cycle check value / new signal strength
PSH rg0
PSH rg7
PSH rg8
PSH rg9
XOR rg0, rg0
:FUNC_INCREMENT_CYCLE_LOOP
CMP rg0, rfp
JGE :FUNC_INCREMENT_CYCLE_END
ICR rg0
MVQ rg8, rg2
REM rg8, 40
MVQ rg7, rg8
SUB rg7, rg1
CMP rg7, 2
SIGN_JGE :PIXEL_OFF
CMP rg7, -2
SIGN_JLE :PIXEL_OFF
MVB *rg4, '#'
ADD rg4, 3
JMP :NEWLINE_INSERTION_CHECK
:PIXEL_OFF
MVB *rg4, ' '
ICR rg4
:NEWLINE_INSERTION_CHECK
CMP rg8, 39
JNE :SKIP_NEWLINE_INSERTION
MVB *rg4, '\n'  ; Newline
ICR rg4
:SKIP_NEWLINE_INSERTION
ICR rg2
MVQ rg9, rg2
ADD rg9, 20
REM rg9, 40
JNZ :FUNC_INCREMENT_CYCLE_LOOP
MVQ rg9, rg1
MUL rg9, rg2
ADD rg3, rg9
JMP :FUNC_INCREMENT_CYCLE_LOOP
:FUNC_INCREMENT_CYCLE_END
POP rg9
POP rg8
POP rg7
POP rg0
RET

:FILE_PATH
DAT "input10.txt\0"

:PART_TWO_RESULT  ; Use remaining memory for storing part two result
