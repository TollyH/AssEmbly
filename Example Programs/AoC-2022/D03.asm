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
JNZ :PART_ONE_END
MVQ rg1, :&LINE_STORE
JMP :READ_LOOP

:PART_ONE_END
CFL
WCN rg5
WCC 10  ; Newline

; Part Two
OFL :FILE_PATH
; rg1 - pointer to current counter position
; rg2 - pointer to counter end
; rg3 - total priorities
; rg4 - line number in group (1-3)
; rg5 - store for character
; rg6 - store for character line last occurred
MVQ rg2, :&LINE_STORE
DCR rg2
XOR rg3, rg3
MVQ rg4, 1
:READ_LOOP_TWO
MVQ rg1, :&LINE_STORE
RFC rg0
CMP rg0, 10  ; Newline?
JEQ :NEWLINE
:FIND_LOOP
; Search for character in counter table
CMP rg1, rg2
JGE :NOT_FOUND
MVB rg5, *rg1
ICR rg1
MVB rg6, *rg1
ICR rg1
CMP rg5, rg0
JNE :FIND_LOOP
; Character was found in counter table, set its value to current line
; If on last line (line 3), and character wasn't on line 2, skip
CMP rg4, 3
JNE :SKIP_NOT_ONE_CHECK
CMP rg6, 1
JEQ :READ_LOOP_TWO
:SKIP_NOT_ONE_CHECK
DCR rg1
MVB *rg1, rg4
ICR rg1
JMP :FIND_LOOP
:NOT_FOUND
; Character wasn't found, add to table if on first line
CMP rg4, 1
JNE :READ_LOOP_TWO
MVB *rg1, rg0
ICR rg1
MVB *rg1, rg4
MVQ rg2, rg1
ICR rg1
JMP :READ_LOOP_TWO
:NEWLINE
ICR rg4
CMP rg4, 4
JNE :READ_LOOP_TWO
MVQ rg1, :&LINE_STORE
:FIND_THREE_LOOP
; Find the character that was present on all three lines, adding its value to the total
CMP rg1, rg2
JGE :RESET  ; No characters appeared three times (shouldn't happen)
MVB rg5, *rg1
ICR rg1
MVB rg6, *rg1
ICR rg1
CMP rg6, 3
JLT :FIND_THREE_LOOP
CMP rg5, 97  ; Is letter lowercase?
JGE :LETTER_LOWERCASE_TWO
SUB rg5, 38
ADD rg3, rg5
JMP :RESET
:LETTER_LOWERCASE_TWO
SUB rg5, 96
ADD rg3, rg5
:RESET
TST rsf, _ffe  ; End of file?
JNZ :END
MVQ rg2, :&LINE_STORE
DCR rg2
MVQ rg4, 1
JMP :READ_LOOP_TWO

:END
CFL
WCN rg3
WCC 10  ; Newline
HLT

:FILE_PATH
DAT "input03.txt\0"

:LINE_STORE  ; Use all of remaining memory for storing each line
