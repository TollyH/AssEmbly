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
TST rsf, 0b100  ; Check if the "file end" status bit is set
JNZ :END  ; If bit is set, stop
RFC rg0  ; Read a single character from the file
WCC rg0  ; Write the character to the console
JMP :READ_PRINT_CHAR  ; Loop back around
:END
CFL  ; Close the file
HLT  ; Stop execution

:FUNC_INPUT
; Destination address given as parameter is in rfp
PSH rg0  ; Store value of rg0 so it isn't overwritten permanently
:FUNC_INPUT_READ
RCC rg0  ; Read a single character from the console into rg0
CMP rg0, 0x0A  ; Check if rg0 is a newline
JEQ :FUNC_INPUT_RETURN  ; If it is, return from subroutine
MVB *rfp, rg0  ; Otherwise, move the inputted character (rg0), to the destination address (*rfp)
ICR rfp  ; Increment the destination address by 1
JMP :FUNC_INPUT_READ  ; Loop asking for character
:FUNC_INPUT_RETURN
POP rg0  ; Restore value of rg0 to what it was prior to function call
RET  ; Return from function with no value, it is already in the provided destination address

:FILE_PATH
PAD 256  ; Create a continuous string of 0s, 256 bytes long - will be used to store file path

:PROMPT_STRING
DAT "Enter file path > "  ; Store string after program data.
