; Only solves part one
OFL :FILE_PATH
; rg0 - read character
; rg1 - current value
; rg2 - sum
:READ_LOOP
RFC rg0
CMP rg0, ','
JEQ :NEXT
CMP rg0, '\n'
JEQ :NEXT
ADD rg1, rg0
MUL rg1, 17
JMP :READ_LOOP
:NEXT
MVB rg1, rg1  ; Get only the lowest byte of rg1
ADD rg2, rg1
CMP rg0, '\n'
JEQ :END
XOR rg1, rg1
JMP :READ_LOOP

:END
CFL
WCN rg2
HLT

:FILE_PATH
DAT "input15.txt\0"
