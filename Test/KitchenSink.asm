; This file should not throw any errors or return any assembler messages
; when assembled.

; Its contents are not supposed to form a meaningful program, it is solely for
; testing the assembler.

; The exact binary output that this program should produce
; is located in KitchenSink.bin

MVQ rg0, 69

%MACRO Macro-d!
    ADD rg9, rg8
    #**nes ted& &

    MUL $0, $1
%ENDMACRO

%MACRO #**nes ted& &
    SUB rg6, rg7
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

%NUM @!CURRENT_ADDRESS

:MORE_CODE
MVQ rg0, :0x123456789
MVQ :0x123456789, 696969420
HLT

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
