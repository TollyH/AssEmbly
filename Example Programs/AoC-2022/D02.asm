OFL :FILE_PATH
; rg0 - their index
; rg1 - our index
; rg2 - total score (part 1)
; rg3 - total score (part 2)
:READ_LOOP
RFC rg0
RFC rg1  ; Will be a space
RFC rg1
RFC rg9  ; Will be a newline (rg9 is unused)
; Convert letters to indices for calculation
SUB rg0, 65
SUB rg1, 88
; rg4 - temp storage for their index manipulation
; rg5 - temp storage for our index manipulation
; rg6 - temp storage for part two (ourIndex * 3)
MVQ rg4, rg0
MVQ rg5, rg1
; totalScoreP1 += (Mod(ourIndex - (theirIndex - 1), 3) * 3) + ourIndex + 1
SUB rg4, 1
SUB rg5, rg4
; Add 3 to prevent negatives while keeping MOD result the same
ADD rg4, 3
ADD rg5, 3
REM rg5, 3
MUL rg5, 3
ADD rg5, rg1
ADD rg5, 1
ADD rg2, rg5
; Reset temp storage for part 2
MVQ rg4, rg0
MVQ rg5, rg1
; totalScoreP2 += Mod(ourIndex + (theirIndex - 1), 3) + (ourIndex * 3) + 1
SUB rg4, 1
ADD rg5, rg4
; Add 3 to prevent negatives while keeping MOD result the same
ADD rg4, 3
ADD rg5, 3
REM rg5, 3
MVQ rg6, rg1
MUL rg6, 3
ADD rg5, rg6
ADD rg5, 1
ADD rg3, rg5
; End of file?
TST rsf, 0b100
JZO :READ_LOOP
CFL
WCN rg2
WCC 10  ; Newline
WCN rg3
WCC 10  ; Newline
HLT

:FILE_PATH
DAT "input02.txt"
PAD 1
