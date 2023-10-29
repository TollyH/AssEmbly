MAC _ffe, 0b100  ; Create a macro for the file end flag

; rg0 - range 1, 1st value
; rg1 - range 1, 2nd value
; rg2 - range 2, 1st value
; rg3 - range 2, 2nd value
; rg4 - total overlaps (part 1)
; rg5 - total overlaps (part 2)
; rg6 - read character
OFL :FILE_PATH
:RANGE_1_1ST
RFC rg6
CMP rg6, '-'
JEQ :RANGE_1_2ND
MUL rg0, 10
SUB rg6, '0'  ; Convert ASCII digit to number
ADD rg0, rg6
JMP :RANGE_1_1ST
:RANGE_1_2ND
RFC rg6
CMP rg6, ','
JEQ :RANGE_2_1ST
MUL rg1, 10
SUB rg6, '0'  ; Convert ASCII digit to number
ADD rg1, rg6
JMP :RANGE_1_2ND
:RANGE_2_1ST
RFC rg6
CMP rg6, '-'
JEQ :RANGE_2_2ND
MUL rg2, 10
SUB rg6, '0'  ; Convert ASCII digit to number
ADD rg2, rg6
JMP :RANGE_2_1ST
:RANGE_2_2ND
RFC rg6
CMP rg6, '\n'  ; Newline?
JEQ :TEST_COMPLETE_OVERLAP_1
MUL rg3, 10
SUB rg6, '0'  ; Convert ASCII digit to number
ADD rg3, rg6
JMP :RANGE_2_2ND

:TEST_COMPLETE_OVERLAP_1
CMP rg0, rg2
JGT :TEST_COMPLETE_OVERLAP_2
CMP rg1, rg3
JLT :TEST_COMPLETE_OVERLAP_2
ICR rg4
JMP :TEST_ANY_OVERLAP
:TEST_COMPLETE_OVERLAP_2
CMP rg0, rg2
JLT :TEST_ANY_OVERLAP
CMP rg1, rg3
JGT :TEST_ANY_OVERLAP
ICR rg4
:TEST_ANY_OVERLAP
CMP rg0, rg3
JGT :TEST_END
CMP rg2, rg1
JGT :TEST_END
ICR rg5
:TEST_END
TST rsf, _ffe
JNZ :END
XOR rg0, rg0
XOR rg1, rg1
XOR rg2, rg2
XOR rg3, rg3
JMP :RANGE_1_1ST

:END
CFL
WCN rg4
WCC '\n'  ; Newline
WCN rg5
WCC '\n'  ; Newline
HLT

:FILE_PATH
DAT "input04.txt\0"
