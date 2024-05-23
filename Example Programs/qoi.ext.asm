; An AssEmbly library for decoding (TODO: encoding) QOI image files
; https://qoiformat.org/ (Website)
; https://qoiformat.org/qoi-specification.pdf (Specification)
; This file should not be assembled, it should only be imported into other AssEmbly files

%ASM_ONCE

%MACRO MagicBytes, 0x66696F71
%MACRO _ffe, 0b100  ; Create a macro for the file end flag

; Pixel {
;   uint8 Red
;   uint8 Green
;   uint8 Blue
;   uint8 Alpha
; }
;
; QOIImage {
;   uint32  Width
;   uint32  Height
;   uint8   Channels (3 - RGB, 4 - RGBA)
;   uint8   Colorspace (0 - sRGB, 1 - Linear)
;   Pixel[] Pixels
; }

; +==============FUNCTION==============+
; |   Decode a QOI image byte stream.  |
; +-------------PARAMETERS-------------+
; | rfp      - Address to source data  |
; | stack[0] - Length of source data   |
; | stack[1] - Address to destination  |
; +--------------RETURNS---------------+
; | rrv - 0 if failed, 1+ if succeeded |
; +====================================+
:FUNC_QOI_DECODE
; rg0 - temp storage
; rg1 - data pointer
; rg2 - length of source data
; rg3 - Exit status
; rg4 - pixel count
PSH rg0
PSH rg1
PSH rg2
PSH rg3
XOR rg3, rg3

; Get stack parameters
MVQ rg1, rsb
ADD rg1, 16
MVQ rg2, *rg1
ADD rg1, 8
MVQ rg1, *rg1

; Check that magic bytes are present and correct
MVD rg0, D*rfp
ADD rfp, 4
CMP rg0, MagicBytes
JNE :QOI_DECODE_EXIT

; Width (stored in big endian)
MVD rg4, D*rfp
SHL rg4, 32
EXTD_BSW rg4
ADD rfp, 4

MVD *rg1, rg4
ADD rg1, 4
; Store width in stack to multiply after height is read
PSH rg4

; Height (stored in big endian)
MVD rg4, D*rfp
SHL rg4, 32
EXTD_BSW rg4
ADD rfp, 4

MVD *rg1, rg4
ADD rg1, 4
POP rg0
MUL rg4, rg0

; Channels
MVB rg0, B*rfp
CMP rg0, 3
JEQ :QOI_DECODE_VALID_CHANNELS
CMP rg0, 4
JNE :QOI_DECODE_EXIT
:QOI_DECODE_VALID_CHANNELS
MVB *rg1, rg0
ICR rfp
ICR rg1

; Colorspace
MVB rg0, B*rfp
TST rg0, rg0
JZO :QOI_DECODE_VALID_COLORSPACE
CMP rg0, 1
JNE :QOI_DECODE_EXIT
:QOI_DECODE_VALID_COLORSPACE
MVB *rg1, rg0
ICR rfp
ICR rg1

; Pixels
PSH rg4
PSH rg1
MVQ rg0, rg2
SUB rg0, 14
PSH rg0
CAL :FUNC_QOI_DECODE_PIXELS, rfp
ADD rso, 24  ; Remove parameters from stack

ICR rg3
:QOI_DECODE_EXIT
MVQ rrv, rg3
POP rg3
POP rg2
POP rg1
POP rg0
RET


; +===========================FUNCTION===========================+
; | Decode a QOI image data stream into an array of RGBA pixels. |
; +--------------------------PARAMETERS--------------------------+
; | rfp      - Address to source data (file header not included) |
; | stack[0] - Length of source data                             |
; | stack[1] - Address to destination for pixel data             |
; | stack[2] - Number of pixels                                  |
; +==============================================================+
:FUNC_QOI_DECODE_PIXELS
; rg0 - temp storage
; rg1 - pixel data pointer
; rg2 - length of source data
; rg3 - previous pixel
; rg4 - number of pixels
; rg5 - current chunk
; rg6 - temp storage
; rg7 - temp storage
PSH rg0
PSH rg1
PSH rg2
PSH rg3
PSH rg4
PSH rg5
PSH rg6
PSH rg7
MVD rg3, 0xFF000000

; Get stack parameters
MVQ rg4, rsb
ADD rg4, 16
MVQ rg2, *rg4
ADD rg2, rfp  ; Convert length to the end index + 1
ADD rg4, 8
MVQ rg1, *rg4
ADD rg4, 8
MVQ rg4, *rg4
; Convert length to the end index + 1
MUL rg4, 4
ADD rg4, rg1

; Initialise 256 byte array in stack (64 pixels) for color index
MVQ rg0, rso
SUB rg0, 256
:QOI_DECODE_PIXELS_ZERO_ARRAY_LOOP
PSH 0
CMP rso, rg0
JGT :QOI_DECODE_PIXELS_ZERO_ARRAY_LOOP

:QOI_DECODE_PIXELS_DECODER_LOOP
; while pixel pointer < pixel count && data pointer 
CMP rg1, rg4
JGE :QOI_DECODE_PIXELS_DECODER_LOOP_END
CMP rfp, rg2
JGE :QOI_DECODE_PIXELS_DECODER_LOOP_END
; process chunk
MVB rg5, B*rfp
CMP rg5, 0b11111110
JEQ :QOI_DECODE_PIXELS_QOI_OP_RGB
CMP rg5, 0b11111111
JEQ :QOI_DECODE_PIXELS_QOI_OP_RGBA
MVQ rg0, rg5
AND rg0, 0b11000000
JZO :QOI_DECODE_PIXELS_QOI_OP_INDEX
CMP rg0, 0b01000000
JEQ :QOI_DECODE_PIXELS_QOI_OP_DIFF
CMP rg0, 0b10000000
JEQ :QOI_DECODE_PIXELS_QOI_OP_LUMA
CMP rg0, 0b11000000
JEQ :QOI_DECODE_PIXELS_QOI_OP_RUN

:QOI_DECODE_PIXELS_QOI_OP_RGB
; Red, green, blue
%REPEAT 3
    ICR rfp
    MVB rg0, B*rfp
    MVB *rg1, rg0
    ICR rg1
%ENDREPEAT
; Alpha
MVQ rg0, rg3
AND rg0, 0xFF000000
SHR rg0, 24
MVB *rg1, rg0
ICR rg1
JMP :QOI_DECODE_PIXELS_OP_JUMP_END

:QOI_DECODE_PIXELS_QOI_OP_RGBA
; Red, green, blue, alpha
%REPEAT 4
    ICR rfp
    MVB rg0, B*rfp
    MVB *rg1, rg0
    ICR rg1
%ENDREPEAT
JMP :QOI_DECODE_PIXELS_OP_JUMP_END

:QOI_DECODE_PIXELS_QOI_OP_INDEX
MVB rg0, B*rfp
MUL rg0, 4 ; Multiply hash index by 4 as pixels are 4 bytes wide
ADD rg0, rso
MVD rg0, D*rg0
MVD *rg1, rg0
ADD rg1, 4
JMP :QOI_DECODE_PIXELS_OP_JUMP_END

:QOI_DECODE_PIXELS_QOI_OP_DIFF
; Red
MVQ rg0, *rfp
AND rg0, 0b00110000
SHR rg0, 4
SUB rg0, 2
MVB rg6, rg3
ADD rg0, rg6
MVB *rg1, rg0
ICR rg1
; Green
MVQ rg0, *rfp
AND rg0, 0b00001100
SHR rg0, 2
SUB rg0, 2
MVQ rg6, rg3
AND rg6, 0x0000FF00
SHR rg6, 8
ADD rg0, rg6
MVB *rg1, rg0
ICR rg1
; Blue
MVQ rg0, *rfp
AND rg0, 0b00000011
SUB rg0, 2
MVQ rg6, rg3
AND rg6, 0x00FF0000
SHR rg6, 16
ADD rg0, rg6
MVB *rg1, rg0
ICR rg1
; Alpha
MVQ rg0, rg3
AND rg0, 0xFF000000
SHR rg0, 24
MVB *rg1, rg0
ICR rg1
JMP :QOI_DECODE_PIXELS_OP_JUMP_END

:QOI_DECODE_PIXELS_QOI_OP_LUMA
; rg6 - green diff
MVQ rg6, *rfp
AND rg6, 0b00111111
SUB rg6, 32
ICR rfp
; Red
MVQ rg0, *rfp
AND rg0, 0b11110000
SHR rg0, 4
SUB rg0, 8
MVB rg7, rg3
ADD rg0, rg7
ADD rg0, rg6
MVB *rg1, rg0
ICR rg1
; Green
MVQ rg0, rg3
AND rg0, 0x0000FF00
SHR rg0, 8
ADD rg0, rg6
MVB *rg1, rg0
ICR rg1
; Blue
MVQ rg0, *rfp
AND rg0, 0b00001111
SUB rg0, 8
MVQ rg7, rg3
AND rg7, 0x00FF0000
SHR rg7, 16
ADD rg0, rg7
ADD rg0, rg6
MVB *rg1, rg0
ICR rg1
; Alpha
MVQ rg0, rg3
AND rg0, 0xFF000000
SHR rg0, 24
MVB *rg1, rg0
ICR rg1
JMP :QOI_DECODE_PIXELS_OP_JUMP_END

:QOI_DECODE_PIXELS_QOI_OP_RUN
MVQ rg0, *rfp
AND rg0, 0b00111111
ICR rg0
:QOI_DECODE_PIXELS_QOI_OP_RUN_LOOP
TST rg0, rg0
JZO :QOI_DECODE_PIXELS_QOI_OP_RUN_LOOP_END
MVD *rg1, rg3
ADD rg1, 4
DCR rg0
JMP :QOI_DECODE_PIXELS_QOI_OP_RUN_LOOP
:QOI_DECODE_PIXELS_QOI_OP_RUN_LOOP_END
JMP :QOI_DECODE_PIXELS_OP_JUMP_END

:QOI_DECODE_PIXELS_OP_JUMP_END
SUB rg1, 4
MVD rg3, D*rg1
ADD rg1, 4
PSH rfp
CAL :FUNC_QOI_HASH, rg3
POP rfp
MVQ rg0, rrv
MUL rg0, 4  ; Multiply hash index by 4 as pixels are 4 bytes wide
ADD rg0, rso
MVD *rg0, rg3
ICR rfp
JMP :QOI_DECODE_PIXELS_DECODER_LOOP
:QOI_DECODE_PIXELS_DECODER_LOOP_END

ADD rso, 256  ; Quickly remove entire color index array from stack
POP rg7
POP rg6
POP rg5
POP rg4
POP rg3
POP rg2
POP rg1
POP rg0
RET


; +=============================FUNCTION============================+
; |                     Decode a QOI image file.                    |
; +----------------------------PARAMETERS---------------------------+
; | rfp - Address of zero terminated path                           |
; +-----------------------------RETURNS-----------------------------+
; | rrv - The newly allocated decoded data, must be freed by caller |
; | rg9 - The length of the decoded data                            |
; +=================================================================+
:FUNC_QOI_DECODE_FILE
; rg0 - temp storage
; rg1 - data pointer
; rg2 - length of source data
; rg3 - start of file data in memory
; rg4 - file data pointer
PSH rg0
PSH rg1
PSH rg2
PSH rg3
PSH rg4

; Allocate space in memory for file to be read to
FSZ rg2, *rfp
HEAP_ALC rg3, rg2

; Read entire input file
OFL *rfp
MVQ rg4, rg3
:FUNC_QOI_DECODE_FILE_READ_LOOP
TST rsf, _ffe
JNZ :FUNC_QOI_DECODE_FILE_READ_LOOP_END
RFC rg0
MVB *rg4, rg0
ICR rg4
JMP :FUNC_QOI_DECODE_FILE_READ_LOOP
:FUNC_QOI_DECODE_FILE_READ_LOOP_END
CFL

; Calculate the size of and allocate memory for decoded image
MVQ rg0, rg3
ADD rg0, 4
; Width
MVD rg1, D*rg0
; Convert big endian to little endian
SHL rg1, 32
EXTD_BSW rg1
; Height
ADD rg0, 4
MVD rg0, D*rg0
; Convert big endian to little endian
SHL rg0, 32
EXTD_BSW rg0
MUL rg1, rg0  ; Total pixels
MUL rg1, 4  ; Pixels are 4 bytes (RGBA)
ADD rg1, 10  ; Header size
MVQ rg9, rg1
HEAP_ALC rg1, rg1

PSH rg1
PSH rg2
CAL :FUNC_QOI_DECODE, rg3
ADD rso, 16  ; Remove pushed parameters from stack

MVQ rrv, rg1

HEAP_FRE rg3
POP rg4
POP rg3
POP rg2
POP rg1
POP rg0

RET


; +==============FUNCTION==============+
; |  Generate a hash for a QOI pixel.  |
; +-------------PARAMETERS-------------+
; | rfp - Pixel                        |
; +--------------RETURNS---------------+
; | rrv - Hash value                   |
; +====================================+
:FUNC_QOI_HASH
PSH rg0
PSH rg1

MVD rg0, rfp
MVB rg0, rg0  ; Shorter version of: AND rg0, 0x000000FF
MUL rg0, 3

MVD rg1, rfp
AND rg1, 0x0000FF00
SHR rg1, 8
MUL rg1, 5
ADD rg0, rg1

MVD rg1, rfp
AND rg1, 0x00FF0000
SHR rg1, 16
MUL rg1, 7
ADD rg0, rg1

MVD rg1, rfp
AND rg1, 0xFF000000
SHR rg1, 24
MUL rg1, 11
ADD rg0, rg1

REM rg0, 64
MVQ rrv, rg0
POP rg1
POP rg0
RET
