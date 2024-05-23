%MACRO _ffe, 0b100  ; Create a macro for the file end flag

OFL :FILE_PATH
; rg0 - sum of line values
; rg1 - current line length / loop counter
; rg2 - line store offset
; rg3 - current character
XOR rg1, rg1
MVQ rg2, :&LINE_STORE
:READ_LOOP
RFC rg3
CMP rg3, '\n'  ; Newline?
JEQ :LINE_END
ICR rg1
MVB *rg2, rg3
ICR rg2
JMP :READ_LOOP
:LINE_END
MVQ rg2, :&LINE_STORE
:FROM_SNAFU_LOOP
; rg4 - power counter
; rg5 - power result
; rg6 - digit base value
MVB rg3, B*rg2

; Raise 5 to the power of current digit place
MVQ rg5, 5.0
MVQ rg4, rg1
FLPT_UTF rg4
FLPT_POW rg5, rg4
FLPT_FTS rg5

; Determine digit value and add to total
CMP rg3, '2'
JNE :DIGIT_ONE_CHECK
MVQ rg6, 2
JMP :BASE_CHECK_END
:DIGIT_ONE_CHECK
CMP rg3, '1'
JNE :DIGIT_ZERO_CHECK
MVQ rg6, 1
JMP :BASE_CHECK_END
:DIGIT_ZERO_CHECK
CMP rg3, '0'
JNE :DIGIT_DASH_CHECK
XOR rg6, rg6
JMP :BASE_CHECK_END
:DIGIT_DASH_CHECK
CMP rg3, '-'
JNE :DIGIT_EQUALS_CHECK
MVQ rg6, -1
JMP :BASE_CHECK_END
:DIGIT_EQUALS_CHECK
MVQ rg6, -2
:BASE_CHECK_END
MUL rg5, rg6
ADD rg0, rg5
ICR rg2
DCR rg1
JNZ :FROM_SNAFU_LOOP
TST rsf, _ffe  ; End of file?
JNZ :TO_SNAFU
; Reset counters
XOR rg1, rg1
MVQ rg2, :&LINE_STORE
JMP :READ_LOOP

:TO_SNAFU
CFL
XOR rg1, rg1
; rg7 - remaining mod
:TO_SNAFU_LOOP
MVQ rg7, rg0
REM rg7, 5
JNZ :MOD_ONE_CHECK
PSH '0'
DIV rg0, 5
JMP :MOD_CHECK_END
:MOD_ONE_CHECK
CMP rg7, 1
JNE :MOD_TWO_CHECK
PSH '1'
DCR rg0
DIV rg0, 5
JMP :MOD_CHECK_END
:MOD_TWO_CHECK
CMP rg7, 2
JNE :MOD_THREE_CHECK
PSH '2'
SUB rg0, 2
DIV rg0, 5
JMP :MOD_CHECK_END
:MOD_THREE_CHECK
CMP rg7, 3
JNE :MOD_FOUR_CHECK
PSH '='
ADD rg0, 2
DIV rg0, 5
JMP :MOD_CHECK_END
:MOD_FOUR_CHECK
PSH '-'
ICR rg0
DIV rg0, 5
:MOD_CHECK_END
ICR rg1
TST rg0, rg0
JZO :PRINT_RESULT
JMP :TO_SNAFU_LOOP

:PRINT_RESULT
DCR rg1
JNZ :WRITE
HLT
:WRITE
POP rg8
WCC rg8
JMP :PRINT_RESULT

:FILE_PATH
%DAT "input25.txt\0"

:LINE_STORE  ; Use all of remaining memory for storing each line
