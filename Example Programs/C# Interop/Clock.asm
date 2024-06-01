; $0 = Address of function name string
; $1 = Parameter (optional)
%MACRO CallExternalFunction
    ASMX_LDF $0!
    ASMX_CAL $1
    ASMX_CLF
%ENDMACRO

TERM_SFC 11  ; Cyan
CAL :FUNC_PRINT, :&PROGRAM_HEADER
TERM_RSC

ASMX_LDA :ASSEMBLY_NAME

:CLOCK_LOOP
CallExternalFunction(:CLOCK_METHOD_NAME, :&DATETIME_FORMAT)
EXTD_SLP 500  ; 500ms/0.5s

WCC '\r'  ; Return to start of line

JMP :CLOCK_LOOP

:ASSEMBLY_NAME
%DAT "Clock.dll\0"

:CLOCK_METHOD_NAME
%DAT "PrintFormattedDateTime\0"

:DATETIME_FORMAT
%DAT "yyyy-MM-dd HH:mm:ss\0"

:PROGRAM_HEADER
%DAT "AssEmbly Clock Example\nPress CTRL+C to exit\n\n\0"

%IF DEF, RUNNING_UNIT_TESTS
    ; Unit tests run relative to the parent Example Programs directory
    %IMP "print.ext.asm"
%ELSE
    %IMP "../print.ext.asm"
%ENDIF
