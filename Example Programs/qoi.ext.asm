; This file should not be assembled, it should only be imported into other AssEmbly files

MAC MagicBytes, 0x66696F71

; Pixel {
;   uint8 Red
;   uint8 Green
;   uint8 Blue
;   uint8 Alpha
; }
;
; QOIImage {
;   uint32    Width
;   uint32    Height
;   uint8     Channels (3 - RGB, 4 - RGBA)
;   uint8     Colorspace (0 - sRGB, 1 - Linear)
;   Pixel  [] Pixels
;   unknown[] TrailingData
;   uint64    TrailingData Length
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
PSH rg0
PSH rg1
PSH rg2
PSH rg3
XOR rg3, rg3

; Get stack parameters
MVQ rg1, rsb
ADD rg1, 24
MVQ rg2, *rg1
ADD rg1, 8
MVQ rg1, *rg1

; Check that magic bytes are present and correct
MVD rg0, *rfp
ADD rfp, 4
CMP rg0, MagicBytes
JNE :QOI_DECODE_EXIT

; Width
MVD rg0, *rfp
MVD *rg1, rg0
ADD rfp, 4
ADD rg1, 4

; Height
MVD rg0, *rfp
MVD *rg1, rg0
ADD rfp, 4
ADD rg1, 4

; Channels
MVB rg0, *rfp
CMP rg0, 3
JEQ :QOI_DECODE_VALID_CHANNELS
CMP rg0, 4
JNE :QOI_DECODE_EXIT
:QOI_DECODE_VALID_CHANNELS
MVB *rg1, rg0
ICR rfp
ICR rg1

; Colorspace
MVB rg0, *rfp
CMP rg0, 3
JEQ :QOI_DECODE_VALID_COLORSPACE
CMP rg0, 4
JNE :QOI_DECODE_EXIT
:QOI_DECODE_VALID_COLORSPACE
MVB *rg1, rg0
ICR rfp
ICR rg1

; Pixels

; Trailing data

ICR rg3
:QOI_DECODE_EXIT
MVQ rrv, rg3
POP rg3
POP rg2
POP rg1
POP rg0
RET
