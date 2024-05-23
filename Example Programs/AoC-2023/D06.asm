; Don't define MAX_LINE_LEN if it already exists (i.e. it was defined through command line)
%IF NDEF, MAX_LINE_LEN
    %DEFINE MAX_LINE_LEN, 32  ; Enough for all input tested, increase if necessary (note there needs to be room for a null terminator)
%ENDIF

; Stop assembly if MAX_LINE_LEN is an unsupported value
%IF LT, @MAX_LINE_LEN, 1
    %STOP "Maximum line length must be 1 or more"
%ENDIF

; Read contents of input file, putting both lines into separate buffers, cutting off line headers
; Trailing newline is required, as file end flag isn't checked due to there always being 2 lines
OFL :FILE_PATH
; rg0 - read character
; rg1 - line 1 buffer
; rg2 - line 2 buffer
; rg3 - current write address
; rg4 - non-zero if on second line
HEAP_ALC rg1, @MAX_LINE_LEN
HEAP_ALC rg2, @MAX_LINE_LEN
MVQ rg3, rg1
:FILE_READ_LOOP
RFC rg0
CMP rg0, '\n'
JEQ :FILE_NEWLINE
; Anything greater than the numbers in ASCII isn't needed (cuts of the "Time:" and "Distance:" headers)
CMP rg0, '9'
JGT :FILE_READ_LOOP
MVB *rg3, rg0
ICR rg3
JMP :FILE_READ_LOOP
:FILE_NEWLINE
TST rg4, rg4
JNZ :END_FILE_READ
NOT rg4
MVQ rg3, rg2  ; Move onto next buffer
JMP :FILE_READ_LOOP
:END_FILE_READ
CFL

; Push 0 to stack to mark end of entries
PSH 0
PSH 0

; rg3 - current line 1 position
; rg4 - current line 2 position
; rg5 - current position
; rg6 - non-zero if on second line
; rg7 - current number
; rg8 - part 2 time (all numbers concatenated)
; rg9 - part 2 distance (all numbers concatenated)
MVQ rg3, rg1
MVQ rg4, rg2
MVQ rg5, rg3
:SKIP_LEADING_SPACES_LOOP
MVB rg0, B*rg5
ICR rg5
CMP rg0, ' '
JEQ :SKIP_LEADING_SPACES_LOOP
DCR rg5
:NUMBER_PARSE_LOOP
MVB rg0, B*rg5
ICR rg5
CMP rg0, ' '
JEQ :SWITCH_NEXT_NUMBER
TST rg0, rg0
JZO :SWITCH_NEXT_NUMBER  ; Reached end of line
SUB rg0, '0'
MUL rg7, 10
ADD rg7, rg0
TST rg6, rg6
JNZ :SECOND_LINE_PART_2_NUM
MUL rg8, 10
ADD rg8, rg0
JMP :NUMBER_PARSE_LOOP
:SECOND_LINE_PART_2_NUM
MUL rg9, 10
ADD rg9, rg0
JMP :NUMBER_PARSE_LOOP

:SWITCH_NEXT_NUMBER
; Push parsed number to stack
PSH rg7
XOR rg7, rg7
NOT rg6
; Alternate between lines
JZO :SWITCH_TO_LINE_1
MVQ rg3, rg5
MVQ rg5, rg4
JMP :SKIP_LEADING_SPACES_LOOP
:SWITCH_TO_LINE_1
TST rg0, rg0
JZO :END_NUMBER_PARSE  ; End of both lines reached
MVQ rg4, rg5
MVQ rg5, rg3
JMP :SKIP_LEADING_SPACES_LOOP

:END_NUMBER_PARSE
HEAP_FRE rg1
HEAP_FRE rg2

; $0 - calculated sqrt component register
; $1 - race time
; $2 - record distance
; $3 - temp register
; $4 - lower hold time register
; $5 - upper hold time register
%MACRO CalculateHoldTime
    ; (-b +- sqrt(b^2 - 4c)) / -2
    ; Calculate sqrt component
    MVQ $0!, $1!
    FLPT_POW $0!, 2.0
    MVQ $3!, $2!
    FLPT_MUL $3!, 4.0
    FLPT_SUB $0!, $3!
    FLPT_POW $0!, 0.5
    ; Calculate lowest hold time
    MVQ $4!, $1!
    FLPT_NEG $4!
    FLPT_ADD $4!, $0!
    FLPT_DIV $4!, -2.0
    FLPT_FCS $4!
    ; Calculate highest hold time
    MVQ $5!, $1!
    FLPT_NEG $5!
    FLPT_SUB $5!, $0!
    FLPT_DIV $5!, -2.0
    FLPT_FFS $5!
%ENDMACRO

; rg0 - temp value holding
; rg1 - race time (b)
; rg2 - record distance (+ 1 = c)
; rg3 - calculated sqrt component
; rg4 - lower hold time
; rg5 - upper hold time
; rg6 - final product
MVQ rg6, 1
:PART_1_SOLVE_LOOP
POP rg2
POP rg1
TST rg1, rg1
JZO :OUTPUT_PART_1
FLPT_UTF rg1
ICR rg2
FLPT_UTF rg2
CalculateHoldTime(rg3, rg1, rg2, rg0, rg4, rg5)
; Calculate difference and add to total
SUB rg5, rg4
ICR rg5
MUL rg6, rg5
JMP :PART_1_SOLVE_LOOP

:OUTPUT_PART_1
WCN rg6
WCC '\n'

; Solve part 2
FLPT_UTF rg8
ICR rg9
FLPT_UTF rg9
CalculateHoldTime(rg3, rg8, rg9, rg0, rg4, rg5)
; Calculate difference and add to total
SUB rg5, rg4
ICR rg5
WCN rg5
WCC '\n'

HLT

:FILE_PATH
%DAT "input06.txt\0"
