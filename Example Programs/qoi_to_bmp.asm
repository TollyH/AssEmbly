%DEFINE MAX_PATH, 256

; rg0 - QOI source file path input
; rg1 - BMP target file path input
; rg2 - Image data pointer

; Allocate buffers to store inputted paths
HEAP_ALC rg0, @MAX_PATH
HEAP_ALC rg1, @MAX_PATH

CAL :FUNC_PRINT, :&STR_QOI_PATH_PROMPT
CAL :FUNC_INPUT, rg0

CAL :FUNC_PRINT, :&STR_BMP_PATH_PROMPT
CAL :FUNC_INPUT, rg1

; This call allocates a new region and returns it in rrv
; - we need to free it manually when we're done with it
CAL :FUNC_QOI_DECODE_FILE, rg0
MVQ rg2, rrv

PSH rg9
PSH rg2
CAL :FUNC_BITMAP_ENCODE_FILE, rg1
ADD rso, 16  ; Remove pushed parameters from stack

HEAP_FRE rg0
HEAP_FRE rg1
HEAP_FRE rg2
HLT

:STR_QOI_PATH_PROMPT
%DAT "Enter the path to a QOI file > \0"

:STR_BMP_PATH_PROMPT
%DAT "Enter the path for the BMP file > \0"

; I/O
%IMP "print.ext.asm"
%IMP "input.ext.asm"
; Imaging
%IMP "qoi.ext.asm"
%IMP "bitmap.ext.asm"
