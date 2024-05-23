OFL :FILE_PATH
; rg0 - read character
; rg1 - pointer to message location
MVQ rg1, :&MESSAGE
:READ_LOOP
RFC rg0
CMP rg0, '\n'  ; Newline?
JEQ :PROCESS
MVB *rg1, rg0
ICR rg1
JMP :READ_LOOP

:PROCESS
CFL
; rg1 - pointer to message end
; rg2 - pointer to current character
; rg3 - current character
; rg4 - offset from current character
; rg5 - pointer to current character comparison
; rg6 - part one answer
; rg7 - part two answer
; rg8 - comparison character
MVQ rg2, :&MESSAGE
DCR rg2
PSH rg2
:CHARACTER_LOOP
TST rg6, rg6
JZO :SKIP_COMPLETE_CHECK
TST rg7, rg7
JNZ :END
:SKIP_COMPLETE_CHECK
POP rg2
ICR rg2
PSH rg2
MVQ rg5, rg2
XOR rg4, rg4
:INNER_LOOP
MVQ rg2, *rso
ICR rg5
ICR rg4
CMP rg5, rg1
JGE :CHARACTER_LOOP
MVB rg8, B*rg5
:INNER_LOOP_2
MVB rg3, B*rg2
CMP rg3, rg8
JEQ :CHARACTER_LOOP
ICR rg2
CMP rg2, rg5
JLT :INNER_LOOP_2
CMP rg4, 3
JNE :PART_TWO_CHECK
TST rg6, rg6
JNZ :PART_TWO_CHECK
MVQ rg6, rg5
SUB rg6, :&MESSAGE
ICR rg6
:PART_TWO_CHECK
CMP rg4, 13
JNE :INNER_LOOP
TST rg7, rg7
JNZ :INNER_LOOP
MVQ rg7, rg5
SUB rg7, :&MESSAGE
ICR rg7
JMP :INNER_LOOP

:END
WCN rg6
WCC '\n'  ; Newline
WCN rg7
WCC '\n'  ; Newline
HLT

:FILE_PATH
%DAT "input06.txt\0"

:MESSAGE  ; Store message in remaining memory
