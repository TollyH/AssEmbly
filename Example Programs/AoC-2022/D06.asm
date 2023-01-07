; Program needs to run with more than the default memory size for full input file!
; Include the parameter --mem-size=5000 with the execute command!
MAC _ffe, 0b100  ; Create a macro for the file end flag

OFL :FILE_PATH
; rg0 - read character
; rg1 - pointer to message location
MVQ rg1, :&MESSAGE
:READ_LOOP
RFC rg0
CMP rg0, 10  ; Newline?
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
CMP rg6, 0
JEQ :SKIP_COMPLETE_CHECK
CMP rg7, 0
JNE :END
:SKIP_COMPLETE_CHECK
POP rg2
ICR rg2
PSH rg2
MVQ rg5, rg2
MVQ rg4, 0
:INNER_LOOP
MVQ rg2, *rso
ICR rg5
ICR rg4
CMP rg5, rg1
JGE :CHARACTER_LOOP
MVB rg8, *rg5
:INNER_LOOP_2
MVB rg3, *rg2
CMP rg3, rg8
JEQ :CHARACTER_LOOP
ICR rg2
CMP rg2, rg5
JLT :INNER_LOOP_2
CMP rg4, 3
JNE :PART_TWO_CHECK
CMP rg6, 0
JNE :PART_TWO_CHECK
MVQ rg6, rg5
SUB rg6, :&MESSAGE
ICR rg6
:PART_TWO_CHECK
CMP rg4, 13
JNE :INNER_LOOP
CMP rg7, 0
JNE :INNER_LOOP
MVQ rg7, rg5
SUB rg7, :&MESSAGE
ICR rg7
JMP :INNER_LOOP

:END
WCN rg6
WCC 10  ; Newline
WCN rg7
WCC 10  ; Newline
HLT

:FILE_PATH
DAT "input06.txt"
DAT 0

:MESSAGE  ; Store message in remaining memory
