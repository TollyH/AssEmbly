; This file should not throw any errors or return any assembler messages
; when assembled.

; Its contents are not supposed to form a meaningful program, it is solely for
; testing the assembler.

; The exact binary output that this program should produce
; is located in KitchenSink.bin

MVQ rg0, 69

%MACRO Macro-d!
    ADD rg9, rg8
    SUB rg6, rg7
    MUL $0, $1
%ENDMACRO

:LOOP  ; comment :)
; comment
ICR rg0
CMP rg0, 420  ; comment
JLT :LOOP
; JLE :LOOP

Macro-d!(rg4, 0b110001101010001110101110011)

HLT

%ANALYZER suggestion, 0004, 0

%DAT "Hello, world!\nEscape test complete\"Still string\0",
%DAT                     0x42
%NUM 1189998819991197253

%MACRO thisLineIsNowBlank,$1
thisLineIsNowBlank()

     :THIS_DOESNT_POINT_HERE     
%LABEL_OVERRIDE   1234    ,

:NOR_DOES_THIS
%LABEL_OVERRIDE   :&THIS_DOESNT_POINT_HERE

:NOR_THIS
%LABEL_OVERRIDE :&NOR_DOES_THIS

%NUM :&THIS_DOESNT_POINT_HERE
%NUM :&NOR_DOES_THIS
%NUM :&NOR_THIS

; Final analyzers currently can't be re-enabled without them showing again
; This should hopefully be changed in a future version
; %ANALYZER suggestion, 0004, r

%MACRO CFL, ASMX_CLF
:thisLabelRemovesTheWarning
CFL
!CFL
%DELMACRO CFL
CFL

%ANALYZER suggestion, 0001, 0
%MACRO NOP, NOT rg0
!>
NOP
NOP
NOP
<!
NOP
%ANALYZER suggestion, 0001, r

:labels_are_case_sensitive
:LABELS_ARE_CASE_SENSITIVE
:LABELS_ARE_CASE_SENSITIVE_AND_CAN_OVERLAP
HLT

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
