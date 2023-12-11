; Program needs to run with more than the default memory size for full input file!
; Include the parameter --mem-size=5000 with the execute command!
MAC _ffe, 0b100  ; Create a macro for the file end flag

OFL :FILE_PATH
; rg0 - character
; rg1 - intermediate number
; rg3 - elf final sum
; rg4 - elf memory offset
; rg5 - number of elves
MVQ rg4, :&TOTALS
:CHAR_READ_LOOP
RFC rg0
TST rsf, 0b100  ; End of file?
JNZ :RESET_INTERS
CMP rg0, '\n'  ; Newline?
JNE :CHAR_PROCESS
TST rg1, rg1  ; Did we already have newline?
JNE :RESET_INTERS
:NEW_ELF
; Start new elf - store current total
MVQ *rg4, rg3
ICR rg5
ADD rg4, 8
XOR rg1, rg1
XOR rg3, rg3
TST rsf, _ffe  ; End of file?
JNZ :MAX
JMP :CHAR_READ_LOOP
:RESET_INTERS
; Next number for same elf
ADD rg3, rg1
XOR rg1, rg1
TST rsf, _ffe  ; End of file?
JNZ :NEW_ELF
JMP :CHAR_READ_LOOP
:CHAR_PROCESS
SUB rg0, '0'  ; Convert ASCII digit to number
MUL rg1, 10
ADD rg1, rg0
JMP :CHAR_READ_LOOP

:MAX
CFL
; rg1 - max number
; rg2 - current elf
; rg4 - elf memory offset
; rg5 - number of elves
MVQ rg4, :&TOTALS
XOR rg2, rg2
XOR rg1, rg1
; Lower values as they'll be raised again
SUB rg4, 8
DCR rg2
:MAX_LOOP
ADD rg4, 8  ; Move onto next elf
ICR rg2
CMP rg2, rg5  ; Have we reached end of elf list?
JGE :NEXT_PART
CMP rg1, *rg4  ; Is the value we're checking greater than the current maximum?
JLT :UPDATE_MAX  ; (i.e. the current maximum is less than)
JMP :MAX_LOOP
:UPDATE_MAX
MVQ rg1, *rg4
JMP :MAX_LOOP
:NEXT_PART
WCN rg1
WCC '\n'  ; Newline

; rg6 - Second highest
MVQ rg4, :&TOTALS
XOR rg2, rg2
; Lower values as they'll be raised again
SUB rg4, 8
DCR rg2
:MAX_LOOP_2
ADD rg4, 8  ; Move onto next elf
ICR rg2
CMP rg2, rg5  ; Have we reached end of elf list?
JGE :THIRD_HIGHEST
CMP rg6, *rg4  ; Is the value we're checking greater than the current maximum?
JLT :UPDATE_MAX_2  ; (i.e. the current maximum is less than)
JMP :MAX_LOOP_2
:UPDATE_MAX_2
CMP rg1, *rg4  ; If we've found the highest - don't update - we want the second highest
JEQ :MAX_LOOP_2
MVQ rg6, *rg4
JMP :MAX_LOOP_2

:THIRD_HIGHEST
; rg7 - Third highest
MVQ rg4, :&TOTALS
XOR rg2, rg2
; Lower values as they'll be raised again
SUB rg4, 8
DCR rg2
:MAX_LOOP_3
ADD rg4, 8  ; Move onto next elf
ICR rg2
CMP rg2, rg5  ; Have we reached end of elf list?
JGE :END
CMP rg7, *rg4  ; Is the value we're checking greater than the current maximum?
JLT :UPDATE_MAX_3  ; (i.e. the current maximum is less than)
JMP :MAX_LOOP_3
:UPDATE_MAX_3
CMP rg1, *rg4  ; If we've found the highest - don't update - we want the third highest
JEQ :MAX_LOOP_3
CMP rg6, *rg4  ; If we've found the second highest - don't update - we want the third highest
JEQ :MAX_LOOP_3
MVQ rg7, *rg4
JMP :MAX_LOOP_3

:END
ADD rg1, rg6
ADD rg1, rg7
WCN rg1
WCC '\n'  ; Newline
HLT

:FILE_PATH
DAT "input01.txt\0"


:TOTALS  ; Use all of remaining memory for elf storage
