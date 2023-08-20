MAC _ffe, 0b100  ; Create a macro for the file end flag

; Print input prompt
CAL :FUNC_PRINT, :&PROMPT_STRING

:END_WRITE
CAL :FUNC_INPUT, :&FILE_PATH  ; Call input function to get file path from user
FEX rg1, :FILE_PATH  ; Check if file exists, storing 1 in rg1 if it does, 0 otherwise
CMP rg1, 1
JEQ :FILE_EXISTS
HLT  ; End program printing nothing if file doesn't exist
:FILE_EXISTS
OFL :FILE_PATH  ; Open the file
:READ_PRINT_CHAR
RFC rg0  ; Read a single character from the file
WCC rg0  ; Write the character to the console
TST rsf, _ffe  ; Check if the "file end" status bit is set
JZO :READ_PRINT_CHAR  ; If bit isn't set, continue looping
:END
CFL  ; Close the file
HLT  ; Stop execution

:FILE_PATH
PAD 256  ; Create a continuous string of 0s, 256 bytes long - will be used to store file path

:PROMPT_STRING
DAT "Enter file path > \0"  ; Store string after program data.

IMP "input.ext.asm"  ; Import input function
IMP "print.ext.asm"  ; Import print function
