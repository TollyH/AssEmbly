; Only solves part one
; Program needs to run with more than the default memory size for full input file!
; Include the parameter --mem-size=100000 with the execute command!
%MACRO _ffe, 0b100  ; Create a macro for the file end flag

OFL :FILE_PATH
; rg0 - read character
; rg1 - trees buffer
; rg2 - length of single row
; rg8 - number of rows
; rg9 - tree index
MVQ rg1, :&TREES
:READ_LOOP
RFC rg0
CMP rg0, '\n'  ; Newline?
JEQ :NEWLINE
SUB rg0, '0'  ; Convert ASCII digit to number
MVQ *rg1[rg9 * 8], rg0
ICR rg9
JMP :READ_LOOP
:NEWLINE
TST rg2, rg2
JNZ :SKIP_ROW_LENGTH_SET
MVQ rg2, rg9
:SKIP_ROW_LENGTH_SET
TST rsf, _ffe  ; End of file?
JZO :READ_LOOP
MVQ rg8, rg9
DIV rg8, rg2
CFL

; rg0 - tree row address / new scenic score
; rg3 - max in row / max in col
; rg4 - x
; rg5 - y
; rg6 - first tree flag
:Y_LOOP
XOR rg3, rg3
XOR rg4, rg4
DCR rg4
MVQ rg6, 1
:Y_INNER_LOOP_INC
ICR rg4
CMP rg4, rg2
JGE :Y_INNER_LOOP_INC_END
MVQ rg0, rg5
MUL rg0, rg2
ADD rg0, rg4
EXTD_MPA rg0, *rg1[rg0 * 8]
TST rg6, rg6
JNZ :Y_INC_SKIP_MAX_CHECK
CMP rg3, *rg0
JGE :Y_INNER_LOOP_INC
:Y_INC_SKIP_MAX_CHECK
XOR rg6, rg6
CAL :FUNC_VALUE_IN_STACK, rg0
TST rrv, rrv
JNZ :Y_INC_SKIP_STACK_PUSH
PSH rg0
:Y_INC_SKIP_STACK_PUSH
MVQ rg3, *rg0
JMP :Y_INNER_LOOP_INC
:Y_INNER_LOOP_INC_END
XOR rg3, rg3
MVQ rg4, rg2
MVQ rg6, 1

:Y_INNER_LOOP_DEC
DCR rg4
JCA :Y_INNER_LOOP_DEC_END
MVQ rg0, rg5
MUL rg0, rg2
ADD rg0, rg4
EXTD_MPA rg0, *rg1[rg0 * 8]
TST rg6, rg6
JNZ :Y_DEC_SKIP_MAX_CHECK
CMP rg3, *rg0
JGE :Y_INNER_LOOP_DEC
:Y_DEC_SKIP_MAX_CHECK
XOR rg6, rg6
CAL :FUNC_VALUE_IN_STACK, rg0
TST rrv, rrv
JNZ :Y_DEC_SKIP_STACK_PUSH
PSH rg0
:Y_DEC_SKIP_STACK_PUSH
MVQ rg3, *rg0
JMP :Y_INNER_LOOP_DEC
:Y_INNER_LOOP_DEC_END
ICR rg5
CMP rg5, rg8
JLT :Y_LOOP

XOR rg4, rg4
:X_LOOP
XOR rg3, rg3
XOR rg5, rg5
DCR rg5
MVQ rg6, 1
:X_INNER_LOOP_INC
ICR rg5
CMP rg5, rg8
JGE :X_INNER_LOOP_INC_END
MVQ rg0, rg5
MUL rg0, rg2
ADD rg0, rg4
EXTD_MPA rg0, *rg1[rg0 * 8]
TST rg6, rg6
JNZ :X_INC_SKIP_MAX_CHECK
CMP rg3, *rg0
JGE :X_INNER_LOOP_INC
:X_INC_SKIP_MAX_CHECK
XOR rg6, rg6
CAL :FUNC_VALUE_IN_STACK, rg0
TST rrv, rrv
JNZ :X_INC_SKIP_STACK_PUSH
PSH rg0
:X_INC_SKIP_STACK_PUSH
MVQ rg3, *rg0
JMP :X_INNER_LOOP_INC
:X_INNER_LOOP_INC_END
XOR rg3, rg3
MVQ rg5, rg8
MVQ rg6, 1

:X_INNER_LOOP_DEC
DCR rg5
JCA :X_INNER_LOOP_DEC_END
MVQ rg0, rg5
MUL rg0, rg2
ADD rg0, rg4
EXTD_MPA rg0, *rg1[rg0 * 8]
TST rg6, rg6
JNZ :X_DEC_SKIP_MAX_CHECK
CMP rg3, *rg0
JGE :X_INNER_LOOP_DEC
:X_DEC_SKIP_MAX_CHECK
XOR rg6, rg6
CAL :FUNC_VALUE_IN_STACK, rg0
TST rrv, rrv
JNZ :X_DEC_SKIP_STACK_PUSH
PSH rg0
:X_DEC_SKIP_STACK_PUSH
MVQ rg3, *rg0
JMP :X_INNER_LOOP_DEC
:X_INNER_LOOP_DEC_END
ICR rg4
CMP rg4, rg2
JLT :X_LOOP

MVQ rg0, rsb
SUB rg0, rso
DIV rg0, 8
WCN rg0
WCC '\n'  ; Newline
HLT

:FUNC_VALUE_IN_STACK
; Value to check for is given as fast pass parameter
; rg8 - calculated address
; rg9 - current index
PSH rg8
PSH rg9
XOR rg9, rg9
:FUNC_VALUE_IN_STACK_LOOP
EXTD_MPA rg8, *rsb[rg9 * 8 + 16]
CMP rg8, *rsb
JGE :FUNC_VALUE_IN_STACK_LOOP_END
CMP rfp, *rg8
JEQ :FUNC_VALUE_IN_STACK_FOUND
ICR rg9
JMP :FUNC_VALUE_IN_STACK_LOOP
:FUNC_VALUE_IN_STACK_LOOP_END
POP rg9
POP rg8
RET 0
:FUNC_VALUE_IN_STACK_FOUND
POP rg9
POP rg8
RET 1

:FILE_PATH
%DAT "input08.txt\0"

:TREES  ; Use remaining memory to store trees
