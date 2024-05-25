; This file should not throw any errors or return any assembler messages
; when assembled.

; Its contents are not supposed to form a meaningful program, it is solely for
; testing the assembler.

; The exact binary output that this program should produce
; is located in KitchenSink.bin

:ENTRY
MVQ rg0, 69

!>
%MACRO Macro-d!
    ADD rg9, rG8
    #**nes ted& &

    MUL $0, $1
%ENDMACRO
<!

%MACRO #**nes ted& &
    SUB rg6, rg7
%ENDMACRO

%MACRO start disable block
    !>
%ENDMACRO

:LOOP  ; comment :)
; comment
ADD rg0, :&LOOP
CMP Rg0, 420  ; comment
JLT :LOOP
; JLE :LOOP

            Macro-d!(rg4, 0b110001101010001110101110011)

HLT

%ANALYZER suggestion, 0004, 0

%NUM @!CURRENT_ADDRESS

%DAT "Hello, world!\nEscape test complete\"Still string\0",
%DAT                     0x42

:LINK_VAL1
%LABEL_OVERRIDE 9990000000000000
:LINK_VAL2
%LABEL_OVERRIDE 8810000000000
:LINK_VAL3
%LABEL_OVERRIDE 1180000000000000000

:VAL1
%LABEL_OVERRIDE :&LINK_VAL1[:&LINK_VAL2[:&LINK_VAL3]]
:VAL2
%LABEL_OVERRIDE 9991190000
:VAL3
%LABEL_OVERRIDE 7253

%NUM :&VAL1[:&VAL2[:&VAL3]]

%ANALYZER suggestion, 0004, r

%MACRO this@LineIsNowBlank,$1
this@LineIsNowBlank()

:NOR_DOES_THIS
%LABEL_OVERRIDE   :&THIS_DOESNT_POINT_HERE[    3   7  6  ]

     :THIS_DOESNT_POINT_HERE     
%LABEL_OVERRIDE   1234    ,

:NOR_THIS
%LABEL_OVERRIDE :&NOR_DOES_THIS

%ANALYZER suggestion, 0004, 0

%num :&THIS_DOESNT_POINT_HERE
%nUm :&NOR_DOES_THIS[69]
%NuM :&NOR_THIS[ -  4      2        0     ]

%ANALYZER suGGeStIoN, 4, R

%MACRO CFL, ASMX_CLF
%ANALYZER suGGeStIoN, 18, 0
:thisLabelRemovesTheWarning
%ANALYZER suggestion, 18, r
CFL
!CFL
%DELMACRO CFL
CFL

%ANALYZER suggestion, 0001, 0
%MACRO single-in-multi
    %MACRO NOP, NOT rg0
%ENDMACRO
NOP
single-in-multi
NOP
start disable block
NOP
NOP
NOP
<!
NOP
%ANALYZER suggestion, 0001, r

%MACRO my_mac, MVQ rg0, 9
%MACRO my_macro, MVW rg1, 10

my_macro

%DELMACRO my_mac
%DELMACRO my_macro

%MACRO NEST_TEST, %STOP "Multi-line macro should not coincide with single-line macro"
%MACRO NEST_TEST
PSH rg0
POP rg1
%ENDMACRO

%MACRO first,ST
%MACRO second,ES

NEfirst_TsecondT

%DELMACRO NEST_TEST

%MACRO NEST_TEST, PSH rg9

%MACRO first,ST
%MACRO second,ES

NEfirst_TsecondT

%MACRO 45,9
%MACRO 56,8

DVR rg6, rg7, 456

%DELMACRO 45
%DELMACRO 56

%analyzer SUGGESTION, 0018, 0
:labels_are_case_sensitive
:LABELS_ARE_CASE_SENSITIVE
:LABELS_ARE_CASE_SENSITIVE_AND_CAN_OVERLAP
%ANALYZER suggestion, 0018, r
hlt

%ANALYZER suggestion, 0004, 0
%DAT "\@THIS_ASSEMBLER_VARIABLE_DOESNT_EXIST\0"
%DEFINE THIS_ONE_DOES, 0x0123456789ABCDEF
%DAT "ABCD@THIS_ONE_DOES'efg"
%DAT 0

%UNDEFINE THIS_ONE_DOES
%IF DEF, THIS_ONE_DOES
    %STOP "Oops! %UNDEFINE didn't work"
%ELSE
    %PAD 12
%ENDIF



%MACRO EmbeddedIf
    %IF GT, $0!, 5
        %NUM 5674390555444
    %ENDIF
%ENDMACRO

%MACRO EmbeddedElse
    %ELSE
        %NUM $0!
    %ENDIF
%ENDMACRO

%IF NDEF, THIS_VARIABLE_DOES_NOT_EXIST
    %IF DEF, THIS_VARIABLE_DEFINITELY_DOES_NOT_EXIST
        %STOP "THIS_VARIABLE_DEFINITELY_DOES_NOT_EXIST"
    %ELSE
        %NUM 0x420
    %ENDIF
    %DAT 0x69
%ELSE
    %STOP "THIS_VARIABLE_DOES_NOT_EXIST"
%ENDIF

%IF DEF, THIS_VARIABLE_DOES_NOT_EXIST
    %IF NDEF, THIS_VARIABLE_DEFINITELY_DOES_NOT_EXIST
        %NUM 0x420
    %ELSE
        %STOP "THIS_VARIABLE_DEFINITELY_DOES_NOT_EXIST"
    %ENDIF
    %STOP "THIS_VARIABLE_DOES_NOT_EXIST"
%ELSE_IF EQ, @!V1_CALL_STACK, 0
    %DAT 0x69
%ELSE_IF EQ, @!V1_FORMAT, 0
    %DAT 0x76
%ELSE
    %DAT 0x96
%ENDIF

%DEFINE GREATER_THAN_5, 6
%IF NDEF, QWERTYUIOP
    EmbeddedIf(@GREATER_THAN_5)

    %IF NDEF, GREATER_THAN_5
    EmbeddedElse(46165465156468461)

                                %ELSE
    EmbeddedIf(@GREATER_THAN_5)

%ENDIF

%ANALYZER suggestion, 0004, r

; ASSEMBLER_VERSION_MAJOR < 3 || (ASSEMBLER_VERSION_MAJOR == 3 && ASSEMBLER_VERSION_MINOR < 2)
%DEFINE _major_lt, @!ASSEMBLER_VERSION_MAJOR
%VAROP CMP_LT, _major_lt, 3

%DEFINE _major_eq, @!ASSEMBLER_VERSION_MAJOR
%VAROP CMP_EQ, _major_eq, 3
%DEFINE _minor_lt, @!ASSEMBLER_VERSION_MINOR
%VAROP CMP_LT, _minor_lt, 2

%VAROP AND, _minor_lt, @_major_eq
%VAROP OR, _major_lt, @_minor_lt
%IF NEQ, @_major_lt, 0
    %STOP "Assembler version must be >=3.2.0 to support conditional assembly"
%ENDIF

%ANALYZER suggestion, 0018, 0
:MORE_CODE
%ANALYZER suggestion, 0018, r
mvQ  RG0, :0x123456789[0  x  9 7]
MVq :0X123456789, 6969.69420
HLT

%ANALYZER suggestion, 0003, 0
%ANALYZER suggestion, 0004, 0

%NUM '🍭'
%NUM '\U0001F36D'

%NUM 'ト'
%NUM '\u30C8'

%DAT "\a\b\f⛩\t\v\\0\0"

%MACRO CONCAT, $0!$1!

CONCAT(CONCAT(%,DAT), CONCAT("\,\,, \,\,\)\\0"))

%MACRO STRING, "$$0!"
%DAT STRING
%PAD 1

%IMP "KitchenSink.1.asm"
%IMP "KitchenSink.1.asm"

%DAT 0x____________4__2___
%DAT 0xFF

%IBF "test-invalid.dll"

%REPEAT 0Xf
    %DAT 1
    %REPEAT 3
        %DAT 3
        %REPEAT 1
            %DAT 4
        %ENDREPEAT
    %ENDREPEAT
    %DAT 2
%ENDREPEAT

%NUM 123.4
%NUM .5
%NUM 5.
%NUM -.567
%NUM -567.
%NUM -45

; Insert a value with only the number of bytes required to represent the whole value
%MACRO InsertNoZeroPadding
    %DEFINE _InsertNoZeroPadding_Value, $0!
!   %WHILE GT, @_InsertNoZeroPadding_Value, 0
        !>
        %DEFINE _InsertNoZeroPadding_Value_Part, @_InsertNoZeroPadding_Value
        %VAROP REM, _InsertNoZeroPadding_Value_Part, 256
        %VAROP DIV, _InsertNoZeroPadding_Value, 256
        %DAT @_InsertNoZeroPadding_Value_Part
        <!
    %ENDWHILE
    %UNDEFINE _InsertNoZeroPadding_Value
    %IF DEF, _InsertNoZeroPadding_Value
        %UNDEFINE _InsertNoZeroPadding_Value
    %ENDIF
%ENDMACRO

InsertNoZeroPadding(42)
InsertNoZeroPadding(0xF987654)
InsertNoZeroPadding(0b1101101101001000101100111010111011011)

%WHILE DEF, this_doesnt_exist
    %STOP "%WHILE loop ran when condition wasn't satisfied"
%ENDWHILE

%DEFINE _counter, 0
%WHILE NDEF, while_var_test
    %DAT 0xFE
    %IF GT, @_counter, 10
        %DEFINE while_var_test, 0
    %ELSE
        %DEFINE _counter_2, 0
        %WHILE LT, @_counter_2, @_counter
            %DAT 0xFD
            %VAROP ADD, _counter_2, 1
        %ENDWHILE
    %ENDIF
    %VAROP ADD, _counter, 1
%ENDWHILE

%ANALYZER suggestion, 0003, r
%ANALYZER suggestion, 0004, r

HLT

:DISPLACEMENT_LABEL_1
%LABEL_OVERRIDE 1
:DISPLACEMENT_LABEL_2
%LABEL_OVERRIDE 2

%ANALYZER suggestion, 0018, 0
:DISPLACEMENTS
%ANALYZER suggestion, 0018, r
%ANALYZER warning, 0032, 0
%ANALYZER warning, 0035, 0
%ANALYZER suggestion, 0021, 0
MVQ rg0, *rg1
MVQ rg1, Q*rg2
MVQ rg2, D*rg3
MVQ rg3, W*rg4
MVQ rg4, B*rg5

MVQ *rg1[1 ]   , rg0
MVQ q*rg2[  -2], rg1
MVQ D*rg3[  0  b  1  1 ],rg2
MVQ W*rg4[   -   4    ], rg3
MVQ b*rg5[  : &  DISPLAC EMENT_LAB  EL_  1   [    : &DIS  PLACEMENT_LAB EL_2 [  2 ] ] ], rg4

MVQ rg0, *rg1[rg9]
MVQ rg1, Q*rg2[-rg8 * 0b1000]
MVQ rg2, d*rg3[    r    g       7      *        12     8       ]
MVQ rg3, W*rg4[   -      r   g   6  ]
MVQ rg4, B*rg5[  r   g      5*4]

MVQ *rg1[rg9-1 ]   , rg0
MVQ Q*rg2[-rg8 * 8  +2], rg1
MVQ D*rg3[    r    g       7      *        12     8       -   0 x  3    ],rg2
%ANALYZER suggestion, 0022, 0
MVQ w*rg4[   -      r   g   6*1+:&DISPLACEMENT_LABEL_1[:&DISPLACEMENT_LABEL_2[1]]    ], rg3
%ANALYZER suggestion, 0022, r
MVQ B*rg5[  r   g      5*4 -5], rg4
%ANALYZER warning, 0032, r
%ANALYZER warning, 0035, r
%ANALYZER suggestion, 0021, r

HLT

; Don't close this
!>

; This MUST stay at the end for a disassembler test!
%DAT 0xFF
%DAT 0x00
%DAT 0x01

%DAT 0xFF
