; A bubble sort algorithm implemented as an AssEmbly function

; %ASM_ONCE can only be used if file is imported
%IF NEQ, @!IMPORT_DEPTH, 0
    %ASM_ONCE
%ENDIF

; Sorts an array of 64-bit numbers in-place
; rfp      - Address to start of array
; stack[0] - Count of numbers in the array
:FUNC_BUBBLE_SORT_64
; rg0 - current position
; rg1 - iteration number
; rg2 - current number
; rg3 - change made flag
; rg4 - array count
; rg5 - next number
PSH rg0
PSH rg1
PSH rg2
PSH rg3
PSH rg4
PSH rg5

XOR rg1, rg1
MVQ rg4, *rsb[16]

:FUNC_BUBBLE_SORT_OUTER_LOOP
XOR rg0, rg0
XOR rg3, rg3

:FUNC_BUBBLE_SORT_INNER_LOOP
MVQ rg2, *rfp[rg0 * 8]
MVQ rg5, *rfp[rg0 * 8 + 8]
CMP rg2, rg5
JLT :FUNC_BUBBLE_SORT_SKIP_SWAP
MVQ *rfp[rg0 * 8 + 8], rg2
MVQ *rfp[rg0 * 8], rg5
ICR rg3
:FUNC_BUBBLE_SORT_SKIP_SWAP
; Stop once we reach (last index - number of passes),
; as each pass guarantees the top n items are sorted
MVQ rg2, rg4
SUB rg2, rg1
DCR rg2
ICR rg0
CMP rg0, rg2
JLT :FUNC_BUBBLE_SORT_INNER_LOOP

; If no moves were made in the entire pass - the array is sorted
TST rg3, rg3
JZO :FUNC_BUBBLE_SORT_END
ICR rg1
JMP :FUNC_BUBBLE_SORT_OUTER_LOOP

:FUNC_BUBBLE_SORT_END
POP rg5
POP rg4
POP rg3
POP rg2
POP rg1
POP rg0
RET

; Test the bubble sort - not assembled if this file is imported
%IF EQ, @!IMPORT_DEPTH, 0
    :ENTRY
    PSH 14
    CAL :FUNC_BUBBLE_SORT_64, :&TEST_ARRAY
    ADD rso, 8  ; Remove pushed parameter from stack
    MVQ rg0, :&TEST_ARRAY
    :PRINT_LOOP
    WCN *rg0
    WCC '\n'
    ADD rg0, 8
    CMP rg0, :&TEST_ARRAY_END
    JLT :PRINT_LOOP
    HLT
    :TEST_ARRAY
    %NUM 4
    %NUM 500
    %NUM 7
    %NUM 8
    %NUM 1
    %NUM 42
    %NUM 0
    %NUM 458
    %NUM 987
    %NUM 365
    %NUM 69
    %NUM 369
    %NUM 420
    %NUM 1
    %NUM 2  ; Not part of the array given to function - should stay in place
    :TEST_ARRAY_END
%ENDIF
