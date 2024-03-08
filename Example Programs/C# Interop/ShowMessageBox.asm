; $0 = Address of function name string
; $1 = Parameter (optional)
%MACRO CallExternalFunction
    ASMX_LDF $0!
    ASMX_CAL $1
    ASMX_CLF
%ENDMACRO

; rg0 - Title buffer
; rg1 - Content buffer
HEAP_ALC rg0, 64
HEAP_ALC rg1, 256

; Get message box title from user
CAL :FUNC_PRINT, :&STR_TITLE_PROMPT
CAL :FUNC_INPUT, rg0
; Get message box content from user
CAL :FUNC_PRINT, :&STR_CONTENT_PROMPT
CAL :FUNC_INPUT, rg1

; Load the assembly containing the functions needed to show a message box
ASMX_LDA :EXT_ASSEMBLY_MSG_BOX

; Call the external set title method with the corresponding user input buffer
CallExternalFunction(:EXT_FUNC_MSG_BOX_SET_TITLE, rg0)

; Call the external set content method with the corresponding user input buffer
CallExternalFunction(:EXT_FUNC_MSG_BOX_SET_CONTENT, rg1)

; Call the external set flags method with a preset value
CallExternalFunction(:EXT_FUNC_MSG_BOX_SET_FLAGS, 0x00000044)  ; MB_YESNO | MB_ICONINFORMATION

; Call the show method, which puts the clicked button value in rrv
CallExternalFunction(:EXT_FUNC_MSG_BOX_SHOW)

; If the ID of the clicked button is 6 (Yes), print that the Yes button was clicked,
; otherwise print that the No button was clicked.
CMP rrv, 6
JEQ :YES_CLICK
CAL :FUNC_PRINT, :&STR_NO_CLICK
JMP :CLEAN_UP
:YES_CLICK
CAL :FUNC_PRINT, :&STR_YES_CLICK

:CLEAN_UP
; Final clean up
ASMX_CLA
HEAP_FRE rg0
HEAP_FRE rg1

HLT

:EXT_ASSEMBLY_MSG_BOX
%DAT "ShowMessageBox.dll\0"

:EXT_FUNC_MSG_BOX_SET_TITLE
%DAT "SetMessageBoxTitle\0"

:EXT_FUNC_MSG_BOX_SET_CONTENT
%DAT "SetMessageBoxContent\0"

:EXT_FUNC_MSG_BOX_SET_FLAGS
%DAT "SetMessageBoxFlags\0"

:EXT_FUNC_MSG_BOX_SHOW
%DAT "ShowMessageBox\0"

:STR_TITLE_PROMPT
%DAT "Enter the desired message box title > \0"

:STR_CONTENT_PROMPT
%DAT "Enter the desired message box content > \0"

:STR_YES_CLICK
%DAT "You clicked Yes\0"

:STR_NO_CLICK
%DAT "You clicked No\0"

%IF DEF, RUNNING_UNIT_TESTS
    ; Unit tests run relative to the parent Example Programs directory
    %IMP "input.ext.asm"
    %IMP "print.ext.asm"
%ELSE
    %IMP "../input.ext.asm"
    %IMP "../print.ext.asm"
%ENDIF
