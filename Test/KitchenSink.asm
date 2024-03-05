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

%DAT "Hello, world!\nEscape test complete\"Still string\0"
%DAT 0x42
%NUM 1189998819991197253

%MACRO thisLineIsNowBlank,$1
thisLineIsNowBlank()
