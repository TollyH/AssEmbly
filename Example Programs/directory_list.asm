; rg0 - path buffer
; rg1 - temp storage
CAL :FUNC_PRINT, :&PROMPT_STRING

; Allocate 256 bytes to store entered/listed directory paths
HEAP_ALC rg0, 256
CAL :FUNC_INPUT, rg0

; Print error message if entered directory doesn't exist
FSYS_DEX rg1, *rg0
TST rg1, rg1
JNZ :START_LISTING
TERM_SFC 12  ; Red
CAL :FUNC_PRINT, :&ERROR_STRING
TERM_RSC
JMP :END

:START_LISTING
FSYS_BDL *rg0
CAL :FUNC_PRINT_CONTAINED_ITEMS

:END
HEAP_FRE rg0
HLT

:FUNC_PRINT_CONTAINED_ITEMS
:FUNC_PRINT_CONTAINED_ITEMS_DIRECTORY_LOOP
FSYS_GND *rg0
JZO :FUNC_PRINT_CONTAINED_ITEMS_FILE_LOOP
TERM_SFC 11  ; Cyan
CAL :FUNC_PRINT, rg0
TERM_RSC
WCC '\n'
JMP :FUNC_PRINT_CONTAINED_ITEMS_DIRECTORY_LOOP
:FUNC_PRINT_CONTAINED_ITEMS_FILE_LOOP
FSYS_GNF *rg0
JZO :FUNC_PRINT_CONTAINED_ITEMS_RETURN
CAL :FUNC_PRINT, rg0
WCC '\n'
JMP :FUNC_PRINT_CONTAINED_ITEMS_FILE_LOOP
:FUNC_PRINT_CONTAINED_ITEMS_RETURN
RET

:PROMPT_STRING
%DAT "Enter path to directory > \0"

:ERROR_STRING
%DAT "Specified directory does not exist.\0"

%IMP "input.ext.asm"
%IMP "print.ext.asm"
