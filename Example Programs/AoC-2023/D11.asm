%MACRO _ffe, 0b100

%MACRO EXPANSION_FACTOR_PART_1, 2
%MACRO EXPANSION_FACTOR_PART_2, 1000000

; Don't define variables if they already exist (i.e. they were defined through command line)
%IF NDEF, MAX_EXPANDED
    %DEFINE MAX_EXPANDED, 20  ; Enough for all input tested, increase if necessary (note there needs to be room for a single-byte terminator)
%ENDIF
; Stop assembly if variables are an unsupported value
%IF LT, @MAX_EXPANDED, 1
    %STOP "MAX_EXPANDED must be 1 or more"
%ENDIF

; Don't define variables if they already exist (i.e. they were defined through command line)
%IF NDEF, MAX_INPUT_LEN
    %DEFINE MAX_INPUT_LEN, 20000  ; Enough for all input tested, increase if necessary (note there needs to be room for a single-byte terminator)
%ENDIF
; Stop assembly if variables are an unsupported value
%IF LT, @MAX_INPUT_LEN, 1
    %STOP "MAX_INPUT_LEN must be 1 or more"
%ENDIF

; Don't define variables if they already exist (i.e. they were defined through command line)
%IF NDEF, MAX_GALAXIES
    %DEFINE MAX_GALAXIES, 500  ; Enough for all input tested, increase if necessary (note there needs to be room for a single-byte terminator)
%ENDIF
; Stop assembly if variables are an unsupported value
%IF LT, @MAX_GALAXIES, 1
    %STOP "MAX_GALAXIES must be 1 or more"
%ENDIF

; rg0 - read character
; rg1 - image pointer
; rg2 - current write pointer
; rg3 - read characters count
; rg7 - characters per line
HEAP_ALC rg1, @MAX_INPUT_LEN
MVQ rg2, rg1
OFL :FILE_PATH
:READ_LOOP
RFC rg0
TST rsf, _ffe
JNZ :READ_END  ; End of file reached
MVB *rg2, rg0
ICR rg2
ICR rg3
TST rg7, rg7  ; Only update length of one line if it isn't set yet
JNZ :READ_LOOP
CMP rg0, '\n'
JEQ :WRITE_PER_LINE_CHARS
JMP :READ_LOOP
:WRITE_PER_LINE_CHARS
MVQ rg7, rg3
JMP :READ_LOOP
:READ_END
CFL

; rg2 - expanded rows pointer
; rg3 - expanded columns pointer
; Terminated with FF
HEAP_ALC rg2, @MAX_EXPANDED
HEAP_ALC rg3, @MAX_EXPANDED

; rg4 - current y
; rg5 - current x
; rg6 - current character pointer
; rg9 - current row pointer
MVQ rg9, rg2
:FIND_EXPANDED_ROWS_LOOP
; Convert coordinates to index
MVQ rg6, rg4
MUL rg6, rg7
ADD rg6, rg5
ADD rg6, rg1  ; Convert index to image pointer
MVB rg0, *rg6
TST rg0, rg0
JZO :FIND_EXPANDED_ROWS_END  ; Reached end of file
CMP rg0, '#'
JEQ :FIND_EXPANDED_ROWS_NEXT  ; Line not empty of galaxies
CMP rg0, '\n'
JEQ :FIND_EXPANDED_ROWS_ADD_ROW  ; Reached end of line
ICR rg5
JMP :FIND_EXPANDED_ROWS_LOOP
:FIND_EXPANDED_ROWS_ADD_ROW
MVB *rg9, rg4
ICR rg9
:FIND_EXPANDED_ROWS_NEXT
XOR rg5, rg5
ICR rg4
JMP :FIND_EXPANDED_ROWS_LOOP
:FIND_EXPANDED_ROWS_END
MVB *rg9, 0xFF  ; Write terminator

XOR rg4, rg4
XOR rg5, rg5
MVQ rg9, rg3
:FIND_EXPANDED_COLUMNS_LOOP
; Convert coordinates to index
MVQ rg6, rg4
MUL rg6, rg7
ADD rg6, rg5
ADD rg6, rg1  ; Convert index to image pointer
MVB rg0, *rg6
CMP rg0, '\n'
JEQ :FIND_EXPANDED_COLUMNS_END  ; Reached end of file
CMP rg0, '#'
JEQ :FIND_EXPANDED_COLUMNS_NEXT  ; Line not empty of galaxies
TST rg0, rg0
JZO :FIND_EXPANDED_COLUMNS_ADD_COLUMN  ; Reached end of line
ICR rg4
JMP :FIND_EXPANDED_COLUMNS_LOOP
:FIND_EXPANDED_COLUMNS_ADD_COLUMN
MVB *rg9, rg5
ICR rg9
:FIND_EXPANDED_COLUMNS_NEXT
XOR rg4, rg4
ICR rg5
JMP :FIND_EXPANDED_COLUMNS_LOOP
:FIND_EXPANDED_COLUMNS_END
MVB *rg9, 0xFF  ; Write terminator

; rg9 - current galaxy pointer
; galaxies are in the format (part_1_x, part_2_x, part_1_y, part_2_y)
MVQ rg0, @MAX_GALAXIES
ICR rg0
MUL rg0, 32
HEAP_ALC rg9, rg0
PSH rg9

; rg6 - passed rows
; rg8 - passed columns
XOR rg4, rg4
XOR rg5, rg5
XOR rg6, rg6
XOR rg8, rg8
:FIND_GALAXIES_Y_LOOP
; Check if passed any expanded rows
MVQ rg0, rg2
ADD rg0, rg6
MVB rg0, *rg0
CMP rg0, 0xFF  ; Check for terminator
JEQ :FIND_GALAXIES_X_LOOP
CMP rg4, rg0
JLT :FIND_GALAXIES_X_LOOP
ICR rg6
:FIND_GALAXIES_X_LOOP
; Check if passed any expanded columns
MVQ rg0, rg3
ADD rg0, rg8
MVB rg0, *rg0
CMP rg0, 0xFF  ; Check for terminator
JEQ :SKIP_ICR_PASSED_COLUMNS
CMP rg5, rg0
JLT :SKIP_ICR_PASSED_COLUMNS
ICR rg8
:SKIP_ICR_PASSED_COLUMNS
; Convert coordinates to index
MVQ rg0, rg4
MUL rg0, rg7
ADD rg0, rg5
ADD rg0, rg1  ; Convert index to image pointer
MVB rg0, *rg0
TST rg0, rg0
JZO :FIND_GALAXIES_END  ; Reached end of file
CMP rg0, '\n'
JEQ :FIND_GALAXIES_NEXT  ; Reached end of line
CMP rg0, '#'
JNE :FIND_GALAXIES_INCREMENT
; part 1 x
MVQ rg0, EXPANSION_FACTOR_PART_1
DCR rg0
MUL rg0, rg8
ADD rg0, rg5
MVQ *rg9, rg0
ADD rg9, 8
; part 2 x
MVQ rg0, EXPANSION_FACTOR_PART_2
DCR rg0
MUL rg0, rg8
ADD rg0, rg5
MVQ *rg9, rg0
ADD rg9, 8
; part 1 y
MVQ rg0, EXPANSION_FACTOR_PART_1
DCR rg0
MUL rg0, rg6
ADD rg0, rg4
MVQ *rg9, rg0
ADD rg9, 8
; part 2 y
MVQ rg0, EXPANSION_FACTOR_PART_2
DCR rg0
MUL rg0, rg6
ADD rg0, rg4
MVQ *rg9, rg0
ADD rg9, 8
:FIND_GALAXIES_INCREMENT
ICR rg5
JMP :FIND_GALAXIES_X_LOOP
:FIND_GALAXIES_NEXT
XOR rg5, rg5
XOR rg8, rg8
ICR rg4
JMP :FIND_GALAXIES_Y_LOOP
:FIND_GALAXIES_END
MVQ *rg9, 0xFFFFFFFFFFFFFFFF  ; Write terminator
POP rg9

HEAP_FRE rg1
HEAP_FRE rg2
HEAP_FRE rg3

; rg1 - first galaxy index
; rg2 - second galaxy index
; rg3 - sum (pt 1)
; rg4 - sum (pt 2)
; rg5 - first galaxy address
; rg6 - second galaxy address
; rg7 - temp value
XOR rg1, rg1
MVQ rg2, 1
XOR rg3, rg3
XOR rg4, rg4
:GALAXY_PAIRS_LOOP
; Calculate first galaxy index and read it
MVQ rg5, rg1
MUL rg5, 32
ADD rg5, rg9
MVQ rg0, *rg5
CMP rg0, 0xFFFFFFFFFFFFFFFF  ; Reached end of galaxy list
JEQ :GALAXY_PAIRS_END
; Calculate second galaxy index and read it
MVQ rg6, rg2
MUL rg6, 32
ADD rg6, rg9
MVQ rg0, *rg6
CMP rg0, 0xFFFFFFFFFFFFFFFF  ; Reached end of galaxy list
JEQ :GALAXY_PAIRS_NEXT
; abs(first.part_one_x - second.part_one_x)
MVQ rg0, *rg5
MVQ rg7, *rg6
SUB rg0, rg7
CAL :FUNC_ABS, rg0
ADD rg3, rrv
; abs(first.part_one_y - second.part_one_y)
MVQ rg0, rg5
ADD rg0, 16  ; 2 values
MVQ rg0, *rg0
MVQ rg7, rg6
ADD rg7, 16  ; 2 values
MVQ rg7, *rg7
SUB rg0, rg7
CAL :FUNC_ABS, rg0
ADD rg3, rrv
; abs(first.part_two_x - second.part_two_x)
MVQ rg0, rg5
ADD rg0, 8  ; 1 value
MVQ rg0, *rg0
MVQ rg7, rg6
ADD rg7, 8  ; 1 value
MVQ rg7, *rg7
SUB rg0, rg7
CAL :FUNC_ABS, rg0
ADD rg4, rrv
; abs(first.part_two_y - second.part_two_y)
MVQ rg0, rg5
ADD rg0, 24  ; 3 values
MVQ rg0, *rg0
MVQ rg7, rg6
ADD rg7, 24  ; 3 values
MVQ rg7, *rg7
SUB rg0, rg7
CAL :FUNC_ABS, rg0
ADD rg4, rrv
; Move to next pair
ICR rg2
JMP :GALAXY_PAIRS_LOOP
:GALAXY_PAIRS_NEXT
ICR rg1
MVQ rg2, rg1
ICR rg2
JMP :GALAXY_PAIRS_LOOP
:GALAXY_PAIRS_END

HEAP_FRE rg9

WCN rg3
WCC '\n'
WCN rg4

HLT

:FUNC_ABS
TST rfp, rfp
SIGN_JNS :FUNC_ABS_RET
SIGN_NEG rfp
:FUNC_ABS_RET
RET rfp

:FILE_PATH
%DAT "input11.txt\0"
