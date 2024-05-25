; An AssEmbly library for encoding (TODO: decoding) BMP image files
; https://en.wikipedia.org/wiki/BMP_file_format
; This file should not be assembled, it should only be imported into other AssEmbly files

%ASM_ONCE

%MACRO HeaderField, 0x4D42  ; "BM"

%DEFINE InfoHeaderSize, 40
%DEFINE TotalHeaderSize, 14
%VAROP ADD, TotalHeaderSize, @InfoHeaderSize

; Pixel {
;   uint8 Red
;   uint8 Green
;   uint8 Blue
;   uint8 Alpha [Ignored]
; }
;
; InputImage {
;   uint32  Width [Limited to 65535]
;   uint32  Height [Limited to 65535]
;   uint8   Channels (3 - RGB, 4 - RGBA) [Ignored - alpha channel not supported]
;   uint8   Colorspace (0 - sRGB, 1 - Linear) [Ignored]
;   Pixel[] Pixels
; }

; +==============FUNCTION==============+
; |   Encode a BMP image byte stream.  |
; +-------------PARAMETERS-------------+
; | rfp      - Address to source data  |
; | stack[0] - Length of source data   |
; | stack[1] - Address to destination  |
; +====================================+
:FUNC_BITMAP_ENCODE
; rg0 - temp storage
; rg1 - destination data pointer
; rg2 - source data length
; rg3 - height
; rg4 - number of line end padding bytes
; rg5 - byte width
PSH rg0
PSH rg1
PSH rg2
PSH rg3
PSH rg4
PSH rg5

; Get stack parameters
MVQ rg1, *rsb[24]
MVQ rg2, *rsb[16]

MVW *rg1, HeaderField
ADD rg1, 2

; Width
MVD rg5, D*rfp
MUL rg5, 3
; Calculate padding for end of each line (bytes of each line must be divisible by 4)
MVQ rg4, rg5
MVQ rg0, rg5
REM rg0, 4
JZO :FUNC_BITMAP_ENCODE_NO_ROUND
MVQ rg4, 4
SUB rg4, rg0
JMP :FUNC_BITMAP_ENCODE_ROUNDED
:FUNC_BITMAP_ENCODE_NO_ROUND
XOR rg4, rg4
:FUNC_BITMAP_ENCODE_ROUNDED

; Height
MVD rg3, D*rfp[4]

; Byte size of file
; Row bytes
MVQ rg0, rg5
ADD rg0, rg4
; Total pixel bytes
MUL rg0, rg3
ADD rg0, @TotalHeaderSize
MVD *rg1, rg0

; Offset of pixel data
MVD *rg1[8], @TotalHeaderSize

MVD *rg1[12], @InfoHeaderSize

; Width
DIV rg5, 3
MVD *rg1[16], rg5
MUL rg5, 3

; Height
SIGN_NEG rg3  ; Height is negative to store from top-to-bottom
MVD *rg1[20], rg3

; Color planes
MVW *rg1[24], 1

; Bits per pixel
MVW *rg1[26], 24

; No compression
MVD *rg1[28], 0

; Dummy image size
MVD *rg1[32], 0

; Horizontal and vertical pixels per metre
MVD *rg1[36], 2834
MVD *rg1[40], 2834

; Dummy palette/important colours size
MVQ *rg1[44], 0

; Skip to input pixel data
ADD rfp, 10
ADD rg1, 52

; rg3 - source data end pointer
MVQ rg3, rfp
ADD rg3, rg2
; rg2 - current line byte
XOR rg2, rg2
SUB rg3, 10
; Copy pixel data
:FUNC_BITMAP_ENCODE_PIXELS_LOOP
CMP rfp, rg3
JGE :FUNC_BITMAP_ENCODE_PIXELS_LOOP_END
; Red, green, and blue need to be reversed
ADD rfp, 2
%REPEAT 3
    MVB rg0, B*rfp
    MVB *rg1, rg0
    ICR rg1
    DCR rfp
%ENDREPEAT
; Skip alpha
ADD rfp, 5
; We can skip padding logic if we don't need to insert any
TST rg4, rg4
JZO :FUNC_BITMAP_ENCODE_PIXELS_LOOP
ADD rg2, 3
CMP rg2, rg5
JLT :FUNC_BITMAP_ENCODE_PIXELS_LOOP
XOR rg2, rg2
; We have reached the end of the line, so write the amount of needed line-end padding
PSH rg4
:FUNC_BITMAP_ENCODE_PIXELS_PADDING_LOOP
MVB *rg1, 0
ICR rg1
DCR rg4
JNZ :FUNC_BITMAP_ENCODE_PIXELS_PADDING_LOOP
POP rg4
JMP :FUNC_BITMAP_ENCODE_PIXELS_LOOP
:FUNC_BITMAP_ENCODE_PIXELS_LOOP_END

POP rg5
POP rg4
POP rg3
POP rg2
POP rg1
POP rg0
RET


; +=====================FUNCTION=====================+
; |             Encode a BMP image file.             |
; +--------------------PARAMETERS--------------------+
; | rfp      - Address of zero terminated path       |
; | stack[0] - Address to source for image data      |
; | stack[1] - Length of source data                 |
; +==================================================+
:FUNC_BITMAP_ENCODE_FILE
; rg0 - temp storage
; rg1 - source data pointer
; rg2 - source data length
; rg3 - destination data pointer
; rg4 - zero terminated file path/current destination pointer
PSH rg0
PSH rg1
PSH rg2
PSH rg3
PSH rg4

MVQ rg4, rfp

; Get stack parameters
MVQ rg1, *rsb[16]
MVQ rg2, *rsb[24]

; Calculate size of and allocate memory for file data
; Width
MVD rg0, D*rg1
MUL rg0, 3
MVQ rg3, rg0
REM rg3, 4  ; Bitmap rows must be divisible by 4 bytes
JZO :FUNC_BITMAP_ENCODE_FILE_NO_ROUND
ADD rg0, 4
SUB rg0, rg3
:FUNC_BITMAP_ENCODE_FILE_NO_ROUND
; Total pixel bytes
MUL rg0, D*rg1[4]  ; Height
ADD rg0, @TotalHeaderSize
HEAP_ALC rg3, rg0

PSH rg3
PSH rg2
CAL :FUNC_BITMAP_ENCODE, rg1
ADD rso, 16  ; Remove pushed parameters from stack

; rg0 - data end pointer
ADD rg0, rg3
DFL *rg4
OFL *rg4
MVQ rg4, rg3
:FUNC_BITMAP_ENCODE_FILE_WRITE_LOOP
CMP rg4, rg0
JGE :FUNC_BITMAP_ENCODE_FILE_WRITE_LOOP_END
WFC *rg4
ICR rg4
JMP :FUNC_BITMAP_ENCODE_FILE_WRITE_LOOP
:FUNC_BITMAP_ENCODE_FILE_WRITE_LOOP_END
CFL

HEAP_FRE rg3

POP rg4
POP rg3
POP rg2
POP rg1
POP rg0
RET
