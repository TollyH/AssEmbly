MAC _ffe, 0b100  ; Create a macro for the file end flag

; Print input prompt
MVQ rg0, :&PROMPT_STRING  ; Move literal address of prompt string to rg0
:WRITE_PROMPT
MVB rg1, *rg0  ; Move contents of address stored in rg0 to rg1
CMP rg1, 0  ; Check if rg1 is 0
JEQ :END_WRITE  ; If it is, skip ahead and begin next section of program
ICR rg0  ; Otherwise, increment source address by 1
WCC rg1  ; Write the character in rg1 to the console
JMP :WRITE_PROMPT  ; Loop back to print next character

:END_WRITE
CAL :FUNC_INPUT, :&FILE_PATH  ; Call input function to get file path from user
FEX rg1, :FILE_PATH  ; Check if file exists, storing 1 in rg1 if it does, 0 otherwise
CMP rg1, 1
JEQ :FILE_EXISTS
HLT  ; End program printing nothing if file doesn't exist
:FILE_EXISTS
OFL :FILE_PATH  ; Open the file
:READ_PRINT_CHAR
TST rsf, _ffe  ; Check if the "file end" status bit is set
JNZ :END  ; If bit is set, stop
RFC rg0  ; Read a single character from the file
WCC rg0  ; Write the character to the console
JMP :READ_PRINT_CHAR  ; Loop back around
:END
CFL  ; Close the file
HLT  ; Stop execution

:FUNC_INPUT
IMP "input.ext.asm"  ; Import input function

:FILE_PATH
PAD 256  ; Create a continuous string of 0s, 256 bytes long - will be used to store file path

:PROMPT_STRING
DAT "Enter file path > "  ; Store string after program data.
