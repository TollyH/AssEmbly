; Only solves part one
; Program needs to run with more than the default memory size for full input file!
; Include the parameter --mem-size=100000 with the execute command!
MAC _ffe, 0b100  ; Create a macro for the file end flag

OFL :FILE_PATH
; rg0 - read character
; rg1 - pointer to tree
; rg2 - length of single row
; rg8 - number of rows
MVQ rg1, :&TREES
:READ_LOOP
RFC rg0
CMP rg0, 10  ; Newline?
JEQ :NEWLINE
SUB rg0, 48  ; Convert ASCII digit to number
MVQ *rg1, rg0
ADD rg1, 8
JMP :READ_LOOP
:NEWLINE
CMP rg2, 0
JNE :SKIP_ROW_LENGTH_SET
MVQ rg2, rg1
SUB rg2, :&TREES
DIV rg2, 8
:SKIP_ROW_LENGTH_SET
TST rsf, _ffe  ; End of file?
JZO :READ_LOOP
MVQ rg8, rg1
SUB rg8, :&TREES
DIV rg8, 8
DIV rg8, rg2
CFL

; rg0 - tree row offset / new scenic score
; rg3 - max in row / max in col
; rg4 - x
; rg5 - y
; rg6 - first tree flag
; rg7 - tree value
; rg9 - max scenic score
:Y_LOOP
MVQ rg3, 0
MVQ rg4, 0
DCR rg4
MVQ rg6, 1
:Y_INNER_LOOP_INC
ICR rg4
CMP rg4, rg2
JGE :Y_INNER_LOOP_INC_END
MVQ rg0, rg5
MUL rg0, rg2
ADD rg0, rg4
MUL rg0, 8
MVQ rg1, :&TREES
ADD rg1, rg0
MVQ rg7, *rg1
CMP rg6, 0
JNE :Y_INC_SKIP_MAX_CHECK
CMP rg7, rg3
JLE :Y_INNER_LOOP_INC
:Y_INC_SKIP_MAX_CHECK
MVQ rg6, 0
CAL :FUNC_VALUE_IN_STACK, rg1
CMP rrv, 0
JNE :Y_INC_SKIP_STACK_PUSH
PSH rg1
:Y_INC_SKIP_STACK_PUSH
MVQ rg3, rg7
JMP :Y_INNER_LOOP_INC
:Y_INNER_LOOP_INC_END
MVQ rg3, 0
MVQ rg4, rg2
MVQ rg6, 1

:Y_INNER_LOOP_DEC
DCR rg4
JCA :Y_INNER_LOOP_DEC_END
MVQ rg0, rg5
MUL rg0, rg2
ADD rg0, rg4
MUL rg0, 8
MVQ rg1, :&TREES
ADD rg1, rg0
MVQ rg7, *rg1
CMP rg6, 0
JNE :Y_DEC_SKIP_MAX_CHECK
CMP rg7, rg3
JLE :Y_INNER_LOOP_DEC
:Y_DEC_SKIP_MAX_CHECK
MVQ rg6, 0
CAL :FUNC_VALUE_IN_STACK, rg1
CMP rrv, 0
JNE :Y_DEC_SKIP_STACK_PUSH
PSH rg1
:Y_DEC_SKIP_STACK_PUSH
MVQ rg3, rg7
JMP :Y_INNER_LOOP_DEC
:Y_INNER_LOOP_DEC_END
ICR rg5
CMP rg5, rg8
JLT :Y_LOOP

MVQ rg4, 0
:X_LOOP
MVQ rg3, 0
MVQ rg5, 0
DCR rg5
MVQ rg6, 1
:X_INNER_LOOP_INC
ICR rg5
CMP rg5, rg8
JGE :X_INNER_LOOP_INC_END
MVQ rg0, rg5
MUL rg0, rg2
ADD rg0, rg4
MUL rg0, 8
MVQ rg1, :&TREES
ADD rg1, rg0
MVQ rg7, *rg1
CMP rg6, 0
JNE :X_INC_SKIP_MAX_CHECK
CMP rg7, rg3
JLE :X_INNER_LOOP_INC
:X_INC_SKIP_MAX_CHECK
MVQ rg6, 0
CAL :FUNC_VALUE_IN_STACK, rg1
CMP rrv, 0
JNE :X_INC_SKIP_STACK_PUSH
PSH rg1
:X_INC_SKIP_STACK_PUSH
MVQ rg3, rg7
JMP :X_INNER_LOOP_INC
:X_INNER_LOOP_INC_END
MVQ rg3, 0
MVQ rg5, rg8
MVQ rg6, 1

:X_INNER_LOOP_DEC
DCR rg5
JCA :X_INNER_LOOP_DEC_END
MVQ rg0, rg5
MUL rg0, rg2
ADD rg0, rg4
MUL rg0, 8
MVQ rg1, :&TREES
ADD rg1, rg0
MVQ rg7, *rg1
CMP rg6, 0
JNE :X_DEC_SKIP_MAX_CHECK
CMP rg7, rg3
JLE :X_INNER_LOOP_DEC
:X_DEC_SKIP_MAX_CHECK
MVQ rg6, 0
CAL :FUNC_VALUE_IN_STACK, rg1
CMP rrv, 0
JNE :X_DEC_SKIP_STACK_PUSH
PSH rg1
:X_DEC_SKIP_STACK_PUSH
MVQ rg3, rg7
JMP :X_INNER_LOOP_DEC
:X_INNER_LOOP_DEC_END
ICR rg4
CMP rg4, rg2
JLT :X_LOOP

MVQ rg0, rsb
SUB rg0, rso
DIV rg0, 8
WCN rg0
WCC 10  ; Newline
WCN rg9
WCC 10
HLT

:FUNC_VALUE_IN_STACK
; Value to check for is given as fast pass parameter
; rg8 - end of parent stack frame
; rg9 - current address in parent stack frame
PSH rg8
PSH rg9
MVQ rg9, rsb
ADD rg9, 8
MVQ rg8, rg9
MVQ rg9, *rg9
:FUNC_VALUE_IN_STACK_LOOP
SUB rg9, 8
CMP rfp, *rg9
JEQ :FUNC_VALUE_IN_STACK_FOUND
CMP rg9, rg8
JGE :FUNC_VALUE_IN_STACK_LOOP
POP rg9
POP rg8
RET 0
:FUNC_VALUE_IN_STACK_FOUND
POP rg9
POP rg8
RET 1

:FILE_PATH
DAT "input08.txt"
DAT 0

:TREES  ; Use remaining memory to store trees
