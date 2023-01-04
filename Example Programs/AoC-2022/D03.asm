; Currently only solves part one
MAC _ffe, 0b100  ; Create a macro for the file end flag

OFL :FILE_PATH
; rg0 - read character
; rg1 - current line position
MVQ rg1, :&LINE_STORE
:READ_LOOP
RFC rg0
CMP rg0, 10  ; Newline?
JEQ :PROCESS_LINE
MVB *rg1, rg0
ICR rg1
JMP :READ_LOOP

:PROCESS_LINE
; rg1 - pointer to compartment two start
; rg2 - pointer to current compartment one letter
; rg3 - pointer to current compartment two letter
; rg4 - pointer to compartment two end
; rg5 - total priorities
; rg6 - store for compartment one value
; rg7 - store for compartment two value
MVQ rg4, rg1
DCR rg4
MVQ rg2, :&LINE_STORE
; Get midpoint of whole string
SUB rg1, rg2
DIV rg1, 2
ADD rg1, rg2
MVQ rg3, rg1
MVB rg6, *rg2
MVB rg7, *rg3
:COMPARE_LOOP
CMP rg6, rg7
JEQ :COMPARISON_EQUAL
CMP rg3, rg4
JEQ :NEXT_COMPARTMENT_ONE  ; Reached end of second compartment
ICR rg3
MVB rg7, *rg3
JMP :COMPARE_LOOP
:NEXT_COMPARTMENT_ONE
; Move back to beginning of second compartment
; Move onto next letter of first compartment
ICR rg2
CMP rg2, rg1
JEQ :COMPARE_END  ; End of first compartment reached, no match found
MVB rg6, *rg2
MVQ rg3, rg1
MVB rg7, *rg3
JMP :COMPARE_LOOP
:COMPARISON_EQUAL
CMP rg6, 97  ; Is letter lowercase?
JGE :LETTER_LOWERCASE
SUB rg6, 38
ADD rg5, rg6
JMP :COMPARE_END
:LETTER_LOWERCASE
SUB rg6, 96
ADD rg5, rg6
:COMPARE_END
TST rsf, _ffe  ; End of file?
JNZ :END
MVQ rg1, :&LINE_STORE
JMP :READ_LOOP

:END
CFL
WCN rg5
WCC 10  ; Newline
HLT

:FILE_PATH
DAT "input03.txt"
DAT 0

:LINE_STORE  ; Use all of remaining memory for storing each line
