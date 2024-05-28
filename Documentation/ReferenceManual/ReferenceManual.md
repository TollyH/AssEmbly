# AssEmbly Reference Manual

Applies to versions: `4.0.0`

Last revised: 2024-05-27

## Introduction

AssEmbly is a custom processor architecture and assembly language implemented in .NET. It is designed to simplify the process of learning and writing in assembly language, while still following the same basic concepts and constraints seen in mainstream architectures such as x86.

AssEmbly was designed and implemented in its entirety by [Tolly Hill](https://github.com/TollyH).

<!-- Insert table of contents for Word documents -->
```{=openxml}
<w:sdt>
    <w:sdtPr>
        <w:docPartObj>
            <w:docPartGallery w:val="Table of Contents" />
            <w:docPartUnique />
        </w:docPartObj>
    </w:sdtPr>
    <w:sdtContent>
        <w:p>
            <w:pPr>
                <w:pStyle w:val="TOCHeading" />
            </w:pPr>
            <w:r>
                <w:t>Table of Contents</w:t>
            </w:r>
        </w:p>
        <w:p>
            <w:r>
                <w:fldChar w:fldCharType="begin" w:dirty="true" />
                <w:instrText xml:space="preserve"> TOC \h \u </w:instrText>
                <w:fldChar w:fldCharType="separate" />
                <w:fldChar w:fldCharType="end" />
            </w:r>
        </w:p>
    </w:sdtContent>
</w:sdt>
<w:p><w:r><w:br w:type="page"/></w:r></w:p>
```

## Technical Information

|                              |                                                                            |
|------------------------------|----------------------------------------------------------------------------|
| Bits                         | 64 (registers, operands & addresses)                                       |
| Word Size                    | 8 bytes (64-bits – called a Quad Word for consistency with x86)            |
| Minimum Addressable Unit     | Byte (8-bits)                                                              |
| Register Count               | 16 (10 general purpose)                                                    |
| Architecture Type            | Register–memory                                                            |
| Endianness                   | Little                                                                     |
| Signed Number Representation | Two's Complement                                                           |
| Branching                    | Condition code (status register)                                           |
| Opcode Size                  | 1 byte (base instruction set) / 3 bytes (extension sets)                   |
| Operand Size                 | 1 byte (registers) / 8 bytes (literals, addresses) / 1–10 bytes (pointers) |
| Instruction Size             | 1 byte – 23 bytes (current) / unlimited (theoretical)                      |
| Instruction Count            | 410 opcodes (153 unique operations)                                        |
| Text Encoding                | UTF-8                                                                      |

## Basic Syntax

### Mnemonics and Operands

All AssEmbly instructions are written on a separate line, starting with a **mnemonic** — a human-readable code that tells the **assembler** exactly what operation needs to be performed — followed by any **operands** for the instruction. The assembler is the program that takes human-readable assembly programs and turns them into raw numbers — bytes — that can be read by the processor. This process is called **assembly** or **assembling**. An operand can be thought of like a parameter to a function in a high-level language — data that is given to the processor to read and/or operate on. Mnemonics are separated from operands with spaces, and operands are separated with commas.

A simple example:

```text
MVQ rg0, 10
```

```text
  MVQ        rg0,      10
  ↑          ↑         ↑
  Mnemonic   Operand   Operand
|----------Instruction----------|
```

You can have as many spaces as you like between commas and mnemonics/operands. There do not need to be any around commas, but there must be at least one between mnemonics and operands. Mnemonics and operands **cannot** be separated with commas.

Some instructions, like `CFL`, don't need any operands. In these cases, simply have the mnemonic alone on the line.

A line may end in a trailing comma as long as there is at least one operand on the line. Mnemonics taking no operands cannot be followed by a trailing comma.

Mnemonics correspond to and are assembled down to **opcodes**, numbers (in the case of AssEmbly either 1 or 3 bytes) that the processor reads to know what instruction to perform and what types of operands it needs to read. If an opcode starts with a `0xFF` byte, the opcode will be 3 bytes long, with the second byte corresponding to an *extension set* number, and the third byte corresponding to an *instruction code*. If an opcode starts with any other byte, that single byte will be the entire opcode, with the byte corresponding to an *instruction code* in the base instruction set (extension set number `0x00`). This means that opcodes in the form `0xFF, 0x00, 0x??` and opcodes in the form `0x??` refer to the same instruction, though this **only** works when the extension set is `0x00`. A full list of extension sets and instruction codes can be found toward the end of the document.

The processor will begin executing from the **first line** in the file downwards, unless a label with the name `ENTRY` is defined, in which case the processor will start there (more in the following section on labels). Programs should *always* end in a `HLT` instruction (with no operands) to stop the processor.

For the most part, if an instruction modifies or stores a value somewhere, the **first** operand will be used as the **destination**.

### Comments

If you wish to insert text into a program without it being considered by the assembler as part of the program, you can use a semicolon (`;`). Any character after a semicolon will be ignored by the assembler until the end of the line. You can have a line be entirely a comment without any instruction if you wish.

For example:

```text
MVQ rg0, 10  ; This text will be ignored
; As will this text
DCR rg0  ; "DCR rg0" will assemble as normal
; Another Comment ; HLT - This is still a comment and will not insert an HLT instruction!
```

### Labels

Labels mark a position in the file for the program to move (**jump**) to or reference from elsewhere. They can be given any name you like (names are **case-sensitive**), but they must be unique per program and can only contain letters, numbers, and underscores. Label names **may not** begin with a number, however. A definition for a label is marked by beginning a line with a colon — the entire rest of the line will then be read as the new label name (excluding comments).

For example:

```text
:AREA_1  ; This comment is valid and will not be read as part of the label
MVQ rg0, 10  ; :AREA_1 now points here

:Area2
DCR rg0  ; :Area2 now points here
HLT
```

Labels store the **address** of whatever is next assembled after they are defined.

For example:

```text
:NOT_COMMENT  ; Comment 1
; Comment 2
; Comment 3
WCC '\n'
```

> Here `:NOT_COMMENT` will point to `WCC`, as it is the first thing that will be assembled after the definition was written (comments are ignored by the assembler and do not contribute to the final assembled program).

Labels can also be placed at the very end of a file to point to the first byte in memory that is not part of the program.

For example, in the small file:

```text
MVQ rg0, 5
MVQ rg1, 10
:END
```

> `:END` here will have a value of `20` when referenced, as each instruction prior will take up `10` bytes (more on this later).

The label name `:ENTRY` (case insensitive) has a special meaning. If it is present in a file, execution will start from wherever the entry label points to. If it is not present, execution will start from the first line.

For example, in this small file:

```text
MVQ rg0, 5
:ENTRY
MVQ rg1, 10
HLT
```

> When this program is executed, only the `MVQ rg1, 10` line will run. `MVQ rg0, 5` will never be executed.

## Operand Types

There are four different types of operand that an instruction may be able to take. If an instruction supports multiple different possible combinations of operands, the assembler will automatically determine their types, you do not need to change the mnemonic at all.

### Register

Registers are named, single-number stores separate from the processor's main memory. Most operations must be performed on them, instead of in locations in memory. They are referenced by using their name (currently always 3 letters — the first one being `r`, for example `rg0`). They always occupy a single byte of memory after being assembled.

The first operand in this instruction is a register:

```text
MVQ rg0, 10
```

### Literal

Literals are numeric values that are directly written in an assembly file and **do not change**. Their value is read literally instead of being subject to special consideration, hence the name. They always occupy 8 bytes (64-bits) of memory after assembly and can be written in base 10 (denary/decimal), base 2 (binary), or base 16 (hexadecimal). To write in binary, place the characters `0b` before the number, or to write in hexadecimal, place `0x` before the number.

The second operand in each of these instructions is a literal that will each represent the same number (ten) after assembly:

```text
MVQ rg0, 10  ; Base 10
MVQ rg0, 0b1010  ; Base 2
MVQ rg0, 0xA  ; Base 16
```

When writing literals, you can place an underscore anywhere within the number value to separate the digits. Underscores cannot be the first character of the number.

For example:

```text
MVQ rg0, 1_000_000  ; This is valid, will be assembled as 1000000 (0xF4240)
MVQ rg0, 0x_10_0__000_0  ; This is still valid, underscores don't have to be uniform

MVQ rg0, _1_000_000  ; This is not valid
MVQ rg0, 0_x1_000_000  ; This is also not valid
MVQ rg0, _0x1_000_000  ; Nor is this
```

Literals can be made negative by putting a `-` sign directly before them (e.g. `-42`), or be made floating point by putting a `.` anywhere in them (e.g. `2.3`). Floating point literals can also be made negative (e.g. `-2.3`). This is explained in more detail in the relevant sections on negative and floating point values.

#### Character Literal

In addition to numeric literals, literal values can also be written in the form of **character literals**. A character literal is a single character, surrounded by single quotes (`'`), that is assembled into the numeric representation of the contained character in UTF-8.

For example:

```text
MVQ rg0, 'a'  ; Move the value 97 to rg0
MVQ rg0, '*'  ; Move the value 42 to rg0
MVQ rg0, 'ト'  ; Move the value 8946659 to rg0
; 8946659 is the numeric value of the UTF-8 bytes 0xE3, 0x83, 0x88 that represent 'ト' when interpreted as little endian

MVQ rg0, 'aa'  ; Results in an error (character literals can only contain a single character)
MVQ rg0, ''  ; Results in an error (character literals cannot be empty)
```

Character literals can also contain escape sequences, assuming the escape sequence is the only thing in the literal and there is only one.

For example:

```text
MVQ rg0, '\''  ; Move the value 39 to rg0
MVQ rg0, '\\'  ; Move the value 92 to rg0
MVQ rg0, '\n'  ; Move the value 10 to rg0
MVQ rg0, '\uABCD'  ; Move the value 9285610 to rg0
; 9285610 is the numeric value of the UTF-8 bytes 0xEA, 0xAF, 0x8D that represent the unicode codepoint U+ABCD when interpreted as little endian

MVQ rg0, '\r\n'  ; Results in an error (character literals can only contain a single character)
MVQ rg0, '\'  ; Results in an error (the only closing quote of the literal has been escaped)
```

Escape sequences are explained in more detail and listed in full in a dedicated section toward the end of the document.

### Address

An address is a value that is interpreted as a location to be read from, written to, or jumped to in a processor's main memory. In AssEmbly, an address is usually specified by using a **label**. After defining a label as seen earlier, they can be referenced from anywhere within the program by prefixing their name with a colon (`:`), similarly to how they are defined — only now it will be in the place of an operand. Like literals, they always occupy 8 bytes (64-bits) of memory after assembly.

Consider the following example:

```text
:AREA_1
WCC '\n'
MVB rg0, :AREA_1  ; Move the byte stored at :AREA_1 in memory to rg0
```

> Here `:AREA_1` will point to the **first byte** (i.e. the start of the **opcode**) of the **directly subsequent assemble-able line** — in this case `WCC`. The second operand to `MVQ` will become the address that `WCC` is stored at in memory, `0` if it is the first instruction in the file. As `MVQ` is the instruction to move to a destination from a source, `rg0` will contain `0xCD` after the instruction executes (`0xCD` being the opcode for `WCC <Literal>`).

Another example, assuming these are the very first lines in a file:

```text
WCC '\n'
:AREA_1
WCX :AREA_1  ; Will write "CA" to the console
```

> `:AREA_1` will store the memory address `9`, as `WCC '\n'` occupies `9` bytes. Note that `CA` (the opcode for `WCX <Address>`) will be written to the console, *not* `9`, as the processor is accessing the byte in memory *at* the address — *not* the address itself.

If, when referencing a label, you want to utilise the address of the label *itself*, rather than the value in memory at that address, insert an ampersand (`&`) after the colon, and before the label name. This is called a **label literal**.

For example:

```text
WCC '\n'
:AREA_1
MVQ rg0, :&AREA_1  ; Move 9 (the address itself) to rg0
WCX :&AREA_1  ; Will write "9" to the console
```

#### Address Literals

While it is usually recommended to use a label to reference an address, it is also possible to directly reference a numerical address without necessarily having a label that points there. To do so, simply put the numerical value of the address after a colon instead of a label name. Hexadecimal and binary values can also used by prefixing the number with `0x` or `0b` respectively.

For example:

```text
WCC '\n'
WCX :9  ; Will write "CA" to the console
WCX :0x09  ; Will write "CA" to the console
WCX :0b1001  ; Will write "CA" to the console
```

> The `WCX` instructions here are accessing the data in memory at address `9` (the opcode for `WCX <Address>` itself - `0xCA`), despite there not being any label that points there.

### Pointer

Memory can also be accessed by using the current value of a register as a memory address. In AssEmbly, this is called a **pointer**, though it is often also called **indirect addressing**. Simply prefix a register name with an asterisk (`*`) to treat the contents of the register as a location to store to, read from, or jump to — instead of a number to operate on. Just like registers, pointers by default occupy a single byte in memory after assembly - unless they are displaced as explained in the following section on displacement.

For example:

```text
:AREA_1
WCC '\n'
MVQ rg0, :&AREA_1  ; Move 0 (the address itself) to rg0
MVQ rg1, *rg0  ; Move the item in memory (0xCD) at the address (0) in rg0 to rg1
```

> `rg1` will contain `0xCD` after the third instruction finishes.

#### Read Sizes

By default, all instructions that use a pointer to read a number from memory will read 8 bytes (64 bits) starting at the pointer's address. You can change the number of bytes that will be read by prefixing the pointer with a **size specifier**: `D` for 4 bytes (32 bits), `W` for 2 bytes (16 bits), and `B` for 1 byte (8 bits). There is also a `Q` size specifier for 8 bytes, however it is never necessary to use this, as pointers already read 64 bits by default. These size specifiers are case-insensitive, so `q`, `d`, `w`, and `b` will also be recognised.

There should be no space between the size specifier and the pointer, for example: `D*rg0`, `W*rsb`, and `B*rg6`.

Using read size specifiers on pointers that are used as a write destination, string address, or that otherwise do not result in memory being read will have no effect and as such read specifiers should not be used in these contexts.

#### Displacement

Pointer operands can optionally be **displaced**, which allows you to access memory at a different address relative to the pointer without modifying the value stored in the register itself. This is achieved by putting the value to displace by in square brackets (`[]`) directly after the pointer - this value will then be added or subtracted from the value stored in the pointer register to get the address in memory to access. Displacement can be done by a constant amount, by the value of a register, or by a combination of both. Both register and numeric constant displacements can be subtracted instead of added by prefixing them with a negative (`-`) sign.

The value of a register displacement component can optionally be multiplied by any power of 2 between 1 and 128 before it is added to the pointer register. To do this, follow the register with a multiplication sign (`*`) and the number to multiply by, which can be any one of: `1`, `2`, `4`, `8`, `16`, `32`, `64`, or `128`, in denary, `0x` prefixed hexadecimal, or `0b` prefixed binary. Multiplications by `1` have no effect and are the default, so shouldn't be explicitly written. Constants cannot be multiplied.

If displacing by both a register and a constant, the register (including any multiplication) must come first, separated from the constant with either the plus (`+`) or minus (`-`) operator depending on whether the constant is to be added or subtracted from the value of the register. No more than one register and constant can be provided.

Displacement constants can be either a numeric literal or a label literal (a label prefixed with `:&`). Numeric literals can be any supported base: denary, `0x` prefixed hexadecimal, or `0b` prefixed binary. Label literals behave nearly identically to numeric literals, with the exception that they cannot be prefixed with a `-` sign to negate or subtract them. The address stored in the label is used as the literal value to add to the pointer register. This value can itself also be displaced by following the label literal immediately with another set of square brackets. Label literals can only be displaced by constant values (either label or numeric literals), not by registers, as the displacement value is calculated at the time of assembly, unlike pointers which are displaced by the processor at runtime. If a label literal is displaced by another label literal, that nested label literal can *also* be displaced. The displacement of label literals can be nested to a theoretically infinite depth.

To get the displaced address of a pointer without actually accessing the memory at that address, the `EXTD_MPA` instruction can be used. This instruction calculates the address of a displaced pointer given as the second operand, and moves the result into the first operand. It does not read any data from memory, therefore the calculated address value does not actually have to be a valid address.

All whitespace inside square brackets is ignored by the assembler, so you can separate the different displacement components with any amount of spaces that you wish. Displacement can be performed on pointers both with and without explicit read size specifiers.

Some examples of displacement:

```text
; Assume a label named :LABEL is defined at address 8 for these examples
MVQ rg0, 10  ; Set rg0 to 10
MVQ rg1, 6  ; Set rg1 to 6

EXTD_MPA rg2, *rg0[rg1]  ; rg1 is added to rg0 to produce 16
; rg2 is now 16 - rg0 and rg1 remain unchanged
EXTD_MPA rg2, *rg0[-rg1]  ; rg1 is subtracted from rg0 to produce 4
; rg2 is now 4 - rg0 and rg1 remain unchanged
EXTD_MPA rg2, *rg0[22]  ; 22 is added to rg0 to produce 32
; rg2 is now 32 - rg0 and rg1 remain unchanged
EXTD_MPA rg2, *rg0[-0xA]  ; 10 is subtracted from rg0 to produce 0
; rg2 is now 0 - rg0 and rg1 remain unchanged

EXTD_MPA rg2, *rg0[rg1 + 22]  ; rg1 and 22 are added to rg0 to produce 38
; rg2 is now 38 - rg0 and rg1 remain unchanged
EXTD_MPA rg2, *rg0[-rg1 * 4 + 22]  ; rg1 is multiplied by 4 then subtracted from rg0 and 22 is then added to rg0 to produce 8
; rg2 is now 8 - rg0 and rg1 remain unchanged
EXTD_MPA rg2, *rg0[rg1 * 16 - 0x5A]  ; rg1 is multiplied by 16 then added to rg0 and 90 is then subtracted from rg0 to produce 16
; rg2 is now 16 - rg0 and rg1 remain unchanged
EXTD_MPA rg2, *rg0[-rg1 - 3]  ; rg1 and 3 are subtracted from rg0 to produce 1
; rg2 is now 1 - rg0 and rg1 remain unchanged

EXTD_MPA rg2, *rg0[:&LABEL]  ; 8 is added to rg0 to produce 18
; rg2 is now 18 - rg0 and rg1 remain unchanged
EXTD_MPA rg2, *rg0[rg1 + :&LABEL]  ; rg1 and 8 are added to rg0 to produce 24
; rg2 is now 24 - rg0 and rg1 remain unchanged
EXTD_MPA rg2, *rg0[:&LABEL[5]]  ; 8 and 5 are added to rg0 to produce 23
; rg2 is now 23 - rg0 and rg1 remain unchanged
EXTD_MPA rg2, *rg0[rg1 + :&LABEL[5]]  ; rg1, 8, and 5 are added to rg0 to produce 29
; rg2 is now 29 - rg0 and rg1 remain unchanged
EXTD_MPA rg2, *rg0[rg1 + :&LABEL[:&LABEL[5]]]  ; rg1, 8, 8, and 5 are added to rg0 to produce 37
; rg2 is now 37 - rg0 and rg1 remain unchanged
EXTD_MPA rg2, *rg0[rg1 * 8 + :&LABEL[:&LABEL[5]]]  ; rg1 is multiplied by 8 then added to rg0, and 8, 8, and 5 are added to rg0 to produce 79
; rg2 is now 79 - rg0 and rg1 remain unchanged
```

Label addresses, label literals, and address literals can also be displaced outside of pointer displacements if desired. These displacements are still performed at assemble-time, so cannot involve register values.

For example:

```text
; Continue to assume a label named :LABEL is defined at address 8

MVQ rg2, :&LABEL[10]  ; Adds 10 to 8 and moves the resulting address into rg2
; rg2 is now 18
MVQ rg2, :&LABEL[:&LABEL]  ; Adds 8 to 8 and moves the resulting address into rg2
; rg2 is now 16
MVQ rg2, :&LABEL[:&LABEL[10]]  ; Adds 10 to 8, then 8 again and moves the resulting address into rg2
; rg2 is now 26

MVQ rg2, :LABEL[10]  ; Adds 10 to 8 and moves the value in memory at the resulting address into rg2
; rg2 is now the value in memory at address 18
MVQ rg2, :LABEL[:&LABEL]  ; Adds 8 to 8 and moves the value in memory at the resulting address into rg2
; rg2 is now the value in memory at address 16
MVQ rg2, :LABEL[:&LABEL[10]]  ; Adds 10 to 8, then 8 again and moves the value in memory at the resulting address into rg2
; rg2 is now the value in memory at address 26

MVQ rg2, :10[10]  ; Adds 10 to 8 and moves the value in memory at the resulting address into rg2
; rg2 is now the value in memory at address 18
MVQ rg2, :0xA[:&LABEL]  ; Adds 8 to 8 and moves the value in memory at the resulting address into rg2
; rg2 is now the value in memory at address 16
MVQ rg2, :0b1010[:&LABEL[10]]  ; Adds 10 to 8, then 8 again and moves the value in memory at the resulting address into rg2
; rg2 is now the value in memory at address 26
```

#### Pointer Encoding

Pointers are assembled into `1` to `10` bytes depending on if and how they are displaced. The first, always present byte of a pointer stores the base register in the lower 4 bits, the read size in the next 2 bits, and the displacement mode in the upper 2 bits. See the following section on registers for the bits that each register encodes to.

The mode and read sizes are encoded like so:

```text
Pointer Byte 1: MMSSRRRR
M = Mode
S = Read Size
R = Register

Modes
=============
00 = No Displacement (Pointer encoded in 1 byte)
01 = Constant (Pointer encoded in 9 bytes)
10 = Register (Pointer encoded in 2 bytes)
11 = Constant and Register (Pointer encoded in 10 bytes)

Read Sizes
=============
00 = 8 bytes (64 bits)
01 = 4 bytes (32 bits)
10 = 2 bytes (16 bits)
11 = 1 byte (8 bits)
```

If the pointer has no displacement component, then the pointer encoding stops after the first byte. Otherwise, the remaining bytes encode the pointer's displacement component.

Constant displacements are encoded as a single 8 byte (64 bit), little endian, two's complement signed integer number (signed numbers and little endian are explained in later sections).

Register displacements are encoded as a single byte with the register itself in the lower 4 bits, the multiplier in the next 3 bits, and a subtraction flag in the highest bit, like so:

```text
Pointer Register Displacement Byte: SMMMRRRR
S = Subtraction Flag (0 = Add, 1 = Subtract)
M = Multiplier
R = Register

Multipliers
=============
000 = x1
001 = x2
010 = x4
011 = x8
100 = x16
101 = x32
110 = x64
111 = x128
```

If a pointer has both a constant and a register displacement, the constant displacement is encoded first, followed immediately by the encoded register displacement.

Some examples of pointer encoding (result bytes are in hexadecimal):

```text
*rg0 = 06 (00|00|0110)
           ^  ^  ^
           |  |  rg0
           |  8 bytes
           No Displacement

W*rg1 = 27 (00|10|0111)
            ^  ^  ^
            |  |  rg1
            |  2 bytes
            No Displacement

D*rg1[rg3] = 97 09 (10|01|0111, 0|000|1001)
                    ^  ^  ^     ^ ^   ^
                    |  |  rg1   | x1  rg3
                    |  4 bytes  Addition
                    Register Displacement

D*rso[-rg8 * 8] = 91 BE (10|01|0001, 1|011|1110)
                         ^  ^  ^     ^ ^   ^
                         |  |  rso   | x8  rg8
                         |  4 bytes  Subtraction
                         Register Displacement

B*rso[66] = 71 42 00 00 00 00 00 00 00 (01|11|0001, ...)
                                        ^  ^  ^   
                                        |  |  rso 
                                        |  1 byte
                                        Constant Displacement

*rsb[-rg6 * 64 - 66] = C2 BE FF FF FF FF FF FF FF EC (11|00|0010, ..., 1|110|1100)
                                                      ^  ^  ^          ^ ^   ^
                                                      |  |  rsb        | x64 rg6
                                                      |  8 bytes       Subtraction
                                                      Constant and Register Displacement
```

## Registers

As with most modern architectures, operations in AssEmbly are almost always performed on **registers**. Each register contains a 64-bit number and has a unique, pre-assigned name. They are stored separately from the processor's memory, therefore cannot be referenced by an address, only by name. There are 16 of them in AssEmbly, 10 of which are *general purpose*, meaning they are free to be used for whatever you wish. All general purpose registers start with a value of `0`. The remaining six have special purposes within the architecture, so should be used with care.

Please be aware that to understand the full operation and purpose for some registers, knowledge explained later on in the manual may be required.

### Register Table

| Byte | Bits | Symbol | Writeable | Full Name           | Purpose                                                                    |
|------|------|--------|-----------|---------------------|----------------------------------------------------------------------------|
| 0x00 | 0000 | rpo    | No        | Program Offset      | Stores the memory address of the current location in memory being executed |
| 0x01 | 0001 | rso    | Yes       | Stack Offset        | Stores the memory address of the highest non-popped item on the stack      |
| 0x02 | 0010 | rsb    | Yes       | Stack Base          | Stores the memory address of the bottom of the current stack frame         |
| 0x03 | 0011 | rsf    | Yes       | Status Flags        | Stores bits representing the status of certain instructions                |
| 0x04 | 0100 | rrv    | Yes       | Return Value        | Stores the return value of the last executed subroutine                    |
| 0x05 | 0101 | rfp    | Yes       | Fast Pass Parameter | Stores a single parameter passed to a subroutine                           |
| 0x06 | 0110 | rg0    | Yes       | General 0           | *General purpose*                                                          |
| 0x07 | 0111 | rg1    | Yes       | General 1           | *General purpose*                                                          |
| 0x08 | 1000 | rg2    | Yes       | General 2           | *General purpose*                                                          |
| 0x09 | 1001 | rg3    | Yes       | General 3           | *General purpose*                                                          |
| 0x0A | 1010 | rg4    | Yes       | General 4           | *General purpose*                                                          |
| 0x0B | 1011 | rg5    | Yes       | General 5           | *General purpose*                                                          |
| 0x0C | 1100 | rg6    | Yes       | General 6           | *General purpose*                                                          |
| 0x0D | 1101 | rg7    | Yes       | General 7           | *General purpose*                                                          |
| 0x0E | 1110 | rg8    | Yes       | General 8           | *General purpose*                                                          |
| 0x0F | 1111 | rg9    | Yes       | General 9           | *General purpose*                                                          |

### rpo - Program Offset

Stores the memory address of the current location in memory being executed. For safety, it cannot be directly written to. To change where you are in a program, use a **jump instruction** (explained later on).

For example, in the short program (assuming the first instruction is the first in a file):

```text
MVQ rg0, 10
DCR rg0
```

> When the program starts, `rpo` will have a value of `0` — the address of the first item in memory. After the first instruction has finished executing, `rpo` will have a value of `10`: its previous value `0`, plus `1` byte for the mnemonic's opcode, `1` byte for the register operand, and `8` bytes for the literal operand. `rpo` is now pointing to the opcode of the next instruction (`DCR`).

**Note:** `rpo` is incremented past the opcode ***before*** an instruction begins execution, therefore when used as an operand in an instruction, it will point to the address of the **first operand**, **not to the address of the opcode**. It will not be incremented again until *after* the instruction has completed.

For example, in the instruction:

```text
MVQ rg0, rpo
```

> Before execution of the instruction begins, `rpo` will point to the opcode corresponding to `MVQ` with a register and literal. Once the processor reads this, it increments `rpo` by `1`. `rpo` now points to the first operand: `rg0`. This value will be retained until after the instruction has completed, when `rpo` will be increased by `2` (`1` for each register operand). This means there was an increase of `3` overall when including the initial increment by `1` for the opcode.

### rsf - Status Flags

The status flags register is used to mark some information about previously executed instructions. While it stores a 64-bit number just like every other register, its value should instead be treated bit-by-bit rather than as one number.

Currently, the **lowest 5** bits of the 64-bit value have a special use — the remaining 59 will not be automatically modified as of current, though it is recommended that you do not use them for anything else in case this changes in the future.

The 5 bits currently in use are:

```text
0b00...00000OSFCZ

... = 52 omitted bits
Z = Zero flag
C = Carry flag
F = File end flag
S = Sign flag
O = Overflow flag
```

Each bit of this number can be considered as a `true` (`1`) or `false` (`0`) value as to whether the flag is "set" or not.

More information on using these flags can be found in the section on comparison and testing.

A full table of how each instruction modifies the status flag register can be found toward the end of the document.

### rrv - Return Value

Stores the return value of the last executed subroutine. Note that if a subroutine doesn't return a value, `rrv` will remain unaffected.

For example:

```text
:SUBROUTINE_ONE
...
...
...
RET 4  ; Return, setting rrv to the literal 4

:SUBROUTINE_TWO
...
...
...
RET  ; Return, leaving rrv unaffected

CAL :SUBROUTINE_ONE
; rrv is now 4
CAL :SUBROUTINE_TWO
; rrv is still 4
```

More information can be found in the section on subroutines.

### rfp - Fast Pass Parameter

Stores a single parameter passed to a subroutine. If such a parameter is not provided, `rfp` remains unaffected.

For example:

```text
:SUBROUTINE_ONE
ADD rfp, 1
RET rfp

:SUBROUTINE_TWO
ADD rfp, 2
RET rfp

CAL :SUBROUTINE_ONE, 4  ; This will implicitly set rfp to 4
; rrv is now 5
CAL :SUBROUTINE_TWO, 6  ; This will implicitly set rfp to 6
; rrv is now 8
CAL :SUBROUTINE_TWO  ; rfp will remain 6 here
; rrv is now 10
```

Implicitly setting `rfp` like this with the `CAL` instruction is called **fast passing** or **fast calling**, hence the name fast pass parameter.

Note that in practice, if a subroutine is designed to take a fast pass parameter, you should **always** explicitly provide it, even if you think `rfp` will already have the value you want. Similarly, you should not use `rfp` in a subroutine if it has not been explicitly set in its calls.

More information can be found in the section on subroutines.

### rso - Stack Offset

Stores the memory address of the highest non-popped item on the stack (note that the stack fills from the end of memory backwards). If nothing is left on the stack in the current subroutine, it will be equal to `rsb`, and if nothing is left on the stack at all, it will still be equal to `rsb`, with both being equal to one over the highest possible address in memory (so will result in an error if that address is read from).

More information can be found in the dedicated sections on the stack and subroutines.

A simple example, assuming memory is 8192 bytes in size (making 8191 the highest address):

```text
WCN rso  ; Outputs "8192"
PSH 5  ; Push the literal 5 to the stack
WCN rso  ; Outputs "8184" (stack values are 8 bytes)
POP rg0  ; Pop the just-pushed 5 into rg0
WCN rso  ; Outputs "8192"
```

### rsb - Stack Base

Stores the memory address of the bottom of the current stack frame. `rsb` will only ever change when subroutines are being utilised — see the dedicated sections on the stack and subroutines for more info.

Note that `rsb` does not contain the address of the first item pushed to the stack, rather the address that all pushed items will be on top of.

### rg0 - rg9 - General Purpose

These 10 registers have no special purpose. They will never be changed unless you explicitly change them with either a move operation, or another operation that stores to registers. These will be used most of the time to store and operate on values, as using memory or the stack to do so is inefficient (and in many cases impossible without copying to a register first), so should only be done when you run out of free registers.

## Moving Data

There are four different instructions that are used to move data around without altering it in AssEmbly, each one moving a different number of bytes. `MVB` moves a single byte, `MVW` moves two (a.k.a. a word, 16-bits), `MVD` moves four (a.k.a. a double word, 32-bits), and `MVQ` moves eight (a.k.a. a quad word, 64-bits, a full number in AssEmbly).

Data can either be moved between two registers, from a register to a memory location, or from a memory location to a register. You cannot move data between two memory locations, you must use a register as a midpoint instead. To move data to or from a memory location, you can use either a label, address literal, or a pointer.

The move instructions are also how the value of a register or memory location is set to a literal value. In a sense, they can be considered the equivalent of the `=` assignment operator in higher-level languages.

When using move instructions, the destination always comes first. The destination cannot be a literal.

### Moving with Literals

An example of setting registers to the maximum literal values for each instruction:

```text
MVQ rg0, 18446744073709551615  ; 64-bit integer limit
MVD rg1, 4294967295  ; 32-bit integer limit
MVW rg2, 65535  ; 16-bit integer limit
MVB rg3, 255  ; 8-bit integer limit
```

Or labels and pointers:

```text
MVQ *rg0, 18446744073709551615  ; 64-bit integer limit
MVD *rg1, 4294967295  ; 32-bit integer limit
MVW :AREA_1, 65535  ; 16-bit integer limit
MVB :AREA_2, 255  ; 8-bit integer limit
```

Note that providing a literal over the limit for a given instruction will not result in an error. Instead, the **upper** bits that do not fit in the specified size will be truncated. All 64-bits will still be assembled into the binary (literals are **always** assembled to 8 bytes).

For example:

```text
MVB rg0, 9874
```

> `MVB` can only take a single byte, or 8 bits, but in binary `9874` is `10011010010010`, requiring 14 bits at minimum to store. The lower 8 bits will be kept: `10010010` — the remaining 6 (`100110`) will be discarded. After this instruction has been executed, `rg0` will have a value of `146`.

### Moving with Registers

When moving to and from a register, `MVQ` will update or read all of its bits (remember that registers are 64-bit). If any of the smaller move instructions are used, the **lower** bits of the register will be used, with the remaining upper bits of a destination register all being set to `0`.

For example, assume that before the `MVD` instruction, `rg1` has a value of `14,879,176,506,051,693,048`:

```text
MVW rg1, 65535
```

> `14,879,176,506,051,693,048` in binary is `1100111001111101011101000011001011110001100011001000100111111000`, a full 64-bits, and `65535` is `1111111111111111`, requiring only 16 bits. `MVW` will only consider these 16 bits (if there were more they would have been truncated, see above section). Instead of altering only the lowest 16 bits of `rg1`, `MVW` will instead set all the remaining 48 bits to `0`, resulting in a final value of `0000000000000000000000000000000000000000000000001111111111111111` — `65535` perfectly.

Similarly to literals, if a source register contains a number greater than what a move instruction can handle, the upper bits will be disregarded.

### Moving with Memory

Unlike with registers, using different sizes of move instruction *will* affect how any bytes are read from memory. Bytes are read from or written to **starting** at the given address, and only the required number for the given instruction are read or written (1 for `MVB`, 2 for `MVW`, 4 for `MVD`, 8 for `MVQ`). The instructions will *always* write these numbers of bytes, if a number to be moved takes up less, it will be padded with `0`s.

Numbers are stored in memory in little endian encoding, meaning that the smallest byte is stored first, up to the largest. For example, the 32-bit number `2,356,895,874` is represented in hexadecimal as `0x8C7B6082`, which can be broken down into 4 bytes: `8C`, `7B`, `60`, and `82`. When stored in memory, this order will be *reversed*, as follows:

```text
| Address | 00 | 01 | 02 | 03 |
|  Value  | 82 | 60 | 7B | 8C |
```

This allows you to read a number with a smaller move instruction than what it was written with, whilst maintaining the same upper-bit truncating behaviour seen with literals and registers.

An example with a 64-bit number, `35,312,134,238,538,232` (`0x007D7432F18C89F8`):

```text
| Address | 00 | 01 | 02 | 03 | 04 | 05 | 06 | 07 |
|  Value  | F8 | 89 | 8C | F1 | 32 | 74 | 7D | 00 |
```

Be aware that moving directly between two memory locations is not allowed. To move from one location in memory to another, use a register as a midpoint, like so:

```text
MVQ rg0, :MEMORY_SOURCE
MVQ :MEMORY_DESTINATION, rg0
```

This also applies to pointers as well as labels (`rg1` contains the source address, `rg2` the destination):

```text
MVQ rg0, *rg1
MVQ *rg2, rg0
```

When using any move instruction larger than `MVB`, be careful to ensure that not only the starting point is within the bounds of available memory, but also all of the subsequent bytes. For example, if you have `8192` bytes of available memory (making `8191` the maximum address), you cannot use `MVQ` on the starting address `8189`, as that requires at least 8 bytes.

## Maths and Bitwise Operations

Math and bitwise instructions operate **in-place** in AssEmbly, meaning the first operand for the operation is also used as the destination for the resulting value to be stored to. Destinations, and thus the first operand, must always be a **register**.

Mathematical and bitwise operations are always done with 64-bits, therefore if a memory location (e.g. a label or pointer) is used as the second operand, 8 bytes will be read starting at that address for the operation in little endian encoding (see the "moving with memory" section above for more info on little endian).

### Addition and Multiplication

Examples of addition and multiplication:

```text
MVQ rg0, 55  ; Set the value of rg0 to 55
ADD rg0, 45  ; Add 45 to the value of rg0, storing in rg0
; rg0 is now 100
MUL rg0, 3  ; Multiply the value of rg0 by 3, storing in rg0
; rg0 is now 300
MVQ rg1, rg0
MUL rg1, rg0  ; Multiply the value of rg1 by the value of rg0, storing in rg1
; rg1 is now 90000
```

Be aware that because there is a limit of 64-bits for mathematical operations, if an addition or multiplication operation results in this limit (`18446744073709551615`) being exceeded, the carry status flag will be set to `1`, and the result will be wrapped around back to `0`, plus however much the limit was exceeded by.

For example:

```text
MVQ rg0, 18446744073709551615  ; Set rg0 to the 64-bit limit
ADD rg0, 10  ; Add 10 to rg0
; rg0 is now 10

MVQ rg0, 18446744073709551590  ; Set rg0 to the 64-bit limit take 25
ADD rg0, 50  ; Add 50 to rg0
; rg0 is now 24
```

In the specific case of adding `1` to a register, the `ICR` (increment) operation can be used instead.

```text
MVQ rg0, 5
ICR rg0
; rg0 is now 6
```

### Subtraction

An example of subtraction:

```text
MVQ rg0, 55  ; Set the value of rg0 to 55
SUB rg0, 45  ; Subtract 45 from the value of rg0, storing in rg0
; rg0 is now 10
MVQ rg1, rg0
SUB rg1, rg0  ; Subtract the value of rg0 from rg1, storing in rg1
; rg1 is now 0
```

If a subtraction causes the result to go below 0, the carry status flag will be set to `1`, and the result will be wrapped around up to the upper limit `18446744073709551615`, minus however much the limit was exceeded by.

For example:

```text
MVQ rg0, 0  ; Set rg0 to 0
SUB rg0, 1  ; Subtract 1 from rg0
; rg0 is now 18446744073709551615 (-1)

MVQ rg0, 25  ; Set rg0 to 25
SUB rg0, 50  ; Subtract 50 from rg0
; rg0 is now 18446744073709551591 (-25)
```

This overflowed value can also be interpreted as a negative number using two's complement if desired, which is explained further in the section on negative numbers.

In the specific case of subtracting `1` from a register, the `DCR` (decrement) operation can be used instead.

```text
MVQ rg0, 5
DCR rg0
; rg0 is now 4
```

### Division

There are three types of division in AssEmbly: integer division (`DIV`), division with remainder (`DVR`), and remainder only (`REM`).

Integer division divides the first operand by the second, discards the remainder, then stores the result in the first operand. For example:

```text
MVQ rg0, 12  ; Set rg0 to 12
DIV rg0, 4  ; Divide the value in rg0 by 4, storing the result in rg0
; rg0 is now 3

MVQ rg1, 23  ; Set rg1 to 23
DIV rg1, 3  ; Divide the value in rg1 by 3, storing the result in rg1
; rg1 is now 7 (the remainder of 2 is discarded)
```

Division with remainder, unlike most other operations, takes three operands, the first two being destination registers, and the third being the divisor. Like with the other operations, the first operand is used as the dividend and the result for the integer part of the division. The value of the second operand is not considered, the second operand simply being the register to store the remainder of the division.

For example:

```text
MVQ rg0, 12  ; Set rg0 to 12
DVR rg0, rg1, 4  ; Divide the value in rg0 by 4, storing the integer result in rg0, and remainder in rg1
; rg0 is now 3, rg1 is now 0

MVQ rg2, 23  ; Set rg2 to 23
DVR rg2, rg3, 3  ; Divide the value in rg2 by 3, storing the integer result in rg2, and remainder in rg3
; rg2 is now 7, rg3 is now 2
```

Remainder only division is similar to integer division in that it only keeps one of the results, but this time the dividend (first operand) is overwritten by the remainder, and the integer result is discarded:

```text
MVQ rg0, 12  ; Set rg0 to 12
REM rg0, 4  ; Divide the value in rg0 by 4, storing the remainder in rg0
; rg0 is now 0

MVQ rg1, 23  ; Set rg1 to 23
REM rg1, 3  ; Divide the value in rg1 by 3, storing the remainder in rg1
; rg1 is now 2 (the integer result of 7 is discarded)
```

### Shifting

Shifting is the process of moving the bits in a binary number either up (left — `SHL`) or down (right — `SHR`) a certain number of places.

For example:

```text
MVQ rg0, 0b11010
; rg0:
; |  Bit  | ... | 64 | 32 | 16 | 8  | 4  | 2  | 1  |
; | Value | ... | 0  | 0  | 1  | 1  | 0  | 1  | 0  |

SHL rg0, 2
; rg0:
; |  Bit  | ... | 64 | 32 | 16 | 8  | 4  | 2  | 1  |
; | Value | ... | 1  | 1  | 0  | 1  | 0  | 0  | 0  |
```

> The bits were shifted 2 places to the left, and new bits on the right were set to 0.

Here's one for shifting right:

```text
MVQ rg0, 0b11010
; rg0:
; |  Bit  | ... | 64 | 32 | 16 | 8  | 4  | 2  | 1  |
; | Value | ... | 0  | 0  | 1  | 1  | 0  | 1  | 0  |

SHR rg0, 2
; rg0:
; |  Bit  | ... | 64 | 32 | 16 | 8  | 4  | 2  | 1  |
; | Value | ... | 0  | 0  | 0  | 0  | 1  | 1  | 0  |
```

> The bits were shifted 2 places to the right, and new bits on the left were set to 0.

If, like with the right shift example above, a shift causes at least one `1` bit to go off the edge (either below the first bit or above the 64th), the carry flag will be set to `1`, otherwise it will be set to `0`.

### Bitwise

Bitwise operations consider each bit of the operands individually instead of as a whole number. There are three operations that take two operands (`AND`, `ORR`, and `XOR`), and one that takes only one (`NOT`).

Here are tables of how each two-operand operation will affect each bit

Bitwise AND (`AND`):

```text
    +---+---+
    | 0 | 1 |
+---+---+---+
| 0 | 0 | 0 |
+---+---+---+
| 1 | 0 | 1 |
+---+---+---+
```

The `AND` operation will only set a bit to `1` if the bit in both operands is `1`. For example:

```text
MVQ rg0, 0b00101
AND rg0, 0b10100
; rg0 now has a value of 0b00100
```

Bitwise OR (`ORR`):

```text
    +---+---+
    | 0 | 1 |
+---+---+---+
| 0 | 0 | 1 |
+---+---+---+
| 1 | 1 | 1 |
+---+---+---+
```

The `ORR` operation will set a bit to `1` if the bit in either operand is `1`. For example:

```text
MVQ rg0, 0b00101
ORR rg0, 0b10100
; rg0 now has a value of 0b10101
```

Bitwise XOR (Exclusive OR - `XOR`):

```text
    +---+---+
    | 0 | 1 |
+---+---+---+
| 0 | 0 | 1 |
+---+---+---+
| 1 | 1 | 0 |
+---+---+---+
```

The `XOR` operation will set a bit to `1` if the bit in one, but not both, operands is `1`. For example:

```text
MVQ rg0, 0b00101
XOR rg0, 0b10100
; rg0 now has a value of 0b10001
```

The `NOT` operation only takes a single operand, which must be a register. It simply "flips" the value of each bit (i.e. `1` becomes `0`, `0` becomes `1`).

For example:

```text
MVQ rg0, 0b00101
NOT rg0
; rg0 now has a value of 0b11010
```

### Random Number Generation

The random number instruction (`RNG`) takes a single operand: the register to store the result in. The instruction always randomises all 64-bits of a register, meaning the result could be anywhere between 0 and 18446744073709551615.

Remainder only division (`REM`) by a value one higher than the desired maximum can be used to limit the random number to a maximum value, like so:

```text
RNG rg0  ; rg0 could now be any value between 0 and 18446744073709551615
REM rg0, 5  ; rg0 is now constrained between 0 and 4 depending on its initial value
```

To set a minimum value also, simply add a constant value to the result of the `REM` operation:

```text
RNG rg0  ; rg0 could now be any value between 0 and 18446744073709551615
REM rg0, 5  ; rg0 is now constrained between 0 and 4 depending on its initial value
ADD rg0, 5  ; rg0 is now constrained between 5 and 9
```

## Negative Numbers

Negative numbers are stored using two's complement in AssEmbly, which means that negative values are stored as their positive counterpart with a bitwise NOT performed, then incremented by `1`.

For example:

```text
MVQ rg0, 9547
; rg0 is 0b0000000000000000000000000000000000000000000000000010010101001011 in binary
MVQ rg0, -9547  ; You can use a '-' sign anywhere a regular literal would be accepted
; rg0 is 0b1111111111111111111111111111111111111111111111111101101010110101 in binary
```

To switch between the positive and negative form of a number, use the `SIGN_NEG` instruction:

```text
MVQ rg0, 9547
SIGN_NEG rg0  ; Performs the equivalent of "NOT rg0" then "ICR rg0" in one instruction
; rg0 is now -9547 (or 18446744073709542069 when interpreted as unsigned)
```

Stored values can be interpreted as either **unsigned** or **signed**. Unsigned values are always positive and use all 64 bits to store their value, giving a range of `0` to `18,446,744,073,709,551,615`. Signed values can be either positive *or* negative and, while still stored using 64-bits, the highest bit is instead to store the sign. This gives a range of `-9,223,372,036,854,775,808` to `9,223,372,036,854,775,807` for signed operations. The number of distinct values is the same as unsigned values, but now half of the values are negative.

To check if the limits of a signed number have been exceeded after an operation instead of the limits of an unsigned number, the **overflow flag** should be used instead of the carry flag. This is explained in detail in the dedicated section on the overflow flag vs the carry flag.

Numeric literals can be made negative by prepending the `-` sign onto them. Much of the base instruction set can take negative numbers as operands and work exactly as expected, though there are some exceptions. A full table of which instructions work as expected with negative values and which ones do not can be found toward the end of the document, though as a general rule, if an instruction has an equivalent that begins with `SIGN_`, you should use the signed one instead if negative values are expected.

Some instructions that work normally with negative values include `ADD`, `SUB`, and `MUL`. Some that do not include `DIV` and `WCN`, where the distinction between unsigned and signed values becomes important, as it will affect the result. The `SIGN_DIV` and `SIGN_WCN` instructions for example should be used instead when negative numbers are possible and desired. It is worth noting that instructions in the base instruction set (instructions not beginning with an extension like `SIGN_`) always interpret numbers as unsigned; the reason some operations do not need a signed counterpart to counteract this is that the usage of two's complement allows overflowed unsigned results and signed results to have the same bit representation with these compatible operations.

For example:

```text
MVQ rg0, 12
ADD rg0, -5
; rg0 is now 7, ADD works as expected with negative values

MVQ rg0, 12
SUB rg0, -5
; rg0 is now 17, SUB works as expected with negative values

MVQ rg0, 12
DIV rg0, -6
; rg0 is NOT -2, the SIGN_DIV instruction needs to be used instead

MVQ rg0, 12
SIGN_DIV rg0, -6
; rg0 is now -2, as expected

WCN rg0
; 18446744073709551614 has been printed to the console, as WCN always assumes that the value is unsigned
SIGN_WCN rg0
; -2 has now been printed to the console, as expected
```

There are other instructions that have signed equivalents, these are simply used as an example. The signed operations also work on positive values, so the signed equivalent of relevant instructions should always be used wherever negative values are *possible* and desired, not just where they are guaranteed.

### Arithmetic Right Shifting

When shifting bits to the right, there are two options: logical shifting (as explained in the previous shifting section), or arithmetic shifting. Arithmetic shifting should be used when you wish to shift a value whilst retaining its sign.

Arithmetic right shifts can be performed with the `SIGN_SHR` instruction, which takes the same operands as `SHR`, but behaves slightly differently when the sign bit of the initial value is set.

For example:

```text
MVQ rg0, 0b11010
; rg0:
; |  Bit  | ... | 64 | 32 | 16 | 8  | 4  | 2  | 1  |
; | Value | ... | 0  | 0  | 1  | 1  | 0  | 1  | 0  |
; All omitted bits are 0

SIGN_SHR rg0, 2
; rg0:
; |  Bit  | ... | 64 | 32 | 16 | 8  | 4  | 2  | 1  |
; | Value | ... | 0  | 0  | 0  | 0  | 1  | 1  | 0  |
; All omitted bits are 0
```

> This behaviour is identical to `SHR`, as the value is not signed.

Here's an example with a negative value:

```text
MVQ rg0, -26
; rg0:
; |  Bit  | ... | 64 | 32 | 16 | 8  | 4  | 2  | 1  |
; | Value | ... | 1  | 1  | 0  | 0  | 1  | 1  | 0  |
; All omitted bits are 1

SIGN_SHR rg0, 2
; rg0:
; |  Bit  | ... | 64 | 32 | 16 | 8  | 4  | 2  | 1  |
; | Value | ... | 1  | 1  | 1  | 1  | 0  | 0  | 1  |
; All omitted bits are 1
```

> Because the sign bit was set in the original value, all new bits shifted into the most significant bit were set to `1` instead of `0`, keeping the sign of the result the same as the initial value.

The behaviour of the carry flag is also altered when performing an arithmetic shift. Where `SHR` sets the carry flag if any `1` bit is shifted past the least significant bit and discarded, `SIGN_SHR` instead sets the carry flag if any bits **not equal to the sign bit** are discarded. This means that for negative initial values, any `0` bit being discarded will set the carry bit, and for positive initial values, any `1` bit being discarded will set the carry bit.

Using an 8-bit number for demonstration, the behaviour of a **logical shift** (`SHR`) looks like this:

```text
-26 >> 1
| 1   | 1   | 1   | 0   | 0   | 1   | 1   | 0   |-> discarded bit not 1, UNSET carry flag
     \     \     \     \     \     \     \
      \     \     \     \     \     \     \
| 0   | 1   | 1   | 1   | 0   | 0   | 1   | 1   |
= 115
```

Whereas the behaviour of an **arithmetic shift** (`SIGN_SHR`) looks like this:

```text
-26 >> 1
| 1   | 1   | 1   | 0   | 0   | 1   | 1   | 0   |-> discard not equal to sign, SET carry flag
  |  \     \     \     \     \     \     \
  |   \     \     \     \     \     \     \
| 1   | 1   | 1   | 1   | 0   | 0   | 1   | 1   |
= -13
```

### Extending Smaller Signed Values

Operations on signed numbers will always expect them to be 64-bits in size, with the 64th bit as the sign bit. If you have a signed value stored in a smaller format, using the 8th (byte), 16th (word), or 32nd (double word) bits as the sign bit, you can use one of the extension instructions (`SIGN_EXB`, `SIGN_EXW`, and `SIGN_EXD` respectively) to convert the number to its equivalent value in 64 bits.

For example:

```text
MVW rg0, 0b1111111101011011
; rg0 is 0b0000000000000000000000000000000000000000000000001111111101011011 in binary
; This is -165 when considering only the lower 16 bits as a signed number,
; however we need the value to occupy all 64-bits to be interpreted properly.
; As of current, even the signed instructions will read rg0 as 65371

SIGN_EXW rg0  ; SIGN_EXW is for extending 16->64, use SIGN_EXB for 8->64 or SIGN_EXD for 32->64
; rg0 is now 0b1111111111111111111111111111111111111111111111111111111101011011 in binary
; This occupies all 64-bits, so rg0 will now work correctly as -165
```

Using the extending instructions with a positive value will not affect the value of the register up to the specified size of bits, though any bits higher than the number supported by the used extend instruction will be set to `0` instead of `1`.

For example:

```text
MVB rg0, 12
; rg0 is 0b0000000000000000000000000000000000000000000000000000000000001100 in binary

SIGN_EXB rg0
; rg0 is unchanged

MVW rg0, 569
; rg0 is 0b0000000000000000000000000000000000000000000000000000001000111001 in binary
; rg0 doesn't fit in a single byte!

SIGN_EXB rg0
; rg0 is now 0b0000000000000000000000000000000000000000000000000000000000111001
; Any bits higher than the 8th bit have been unset, making rg0 equal to only 57
```

The second example here caused part of the number to be lost as `SIGN_EXB` was used when the value was larger than 8-bits. A similar scenario will occur if a negative value requires more bits than the used extend instruction can handle, though the upper bits will all be set to `1` instead of `0` in this case.

Converting from a larger size of signed integer to a smaller one is as simple as taking only the desired number of lower bits. Assuming the value can fit within the target signed integer size's limits, no specific operation needs to be used.

### The Overflow Flag vs the Carry Flag

As explained earlier, during most mathematical operations the carry flag is set whenever a subtraction goes below `0`, or an addition goes above `18446744073709551615`. This is useful in unsigned arithmetic, as it will inform you when the result of an operation is not mathematically correct, however in signed arithmetic, it cannot be used for this purpose. To overcome this, the status flag register also contains an **overflow flag**. This flag is set specifically when the result of an operation is incorrect when interpreted as a *signed* value. It has no useful meaning during unsigned arithmetic.

Some examples:

```text
MVQ rg0, 10
SUB rg0, 5
; As unsigned, rg0 is now 5. As signed it is also 5.
; Carry flag has been UNSET, answer is correct as unsigned.
; Overflow flag has been UNSET, answer is correct as signed.

MVQ rg0, 0
SUB rg0, 5
; As unsigned, rg0 is now 18446744073709551611. As signed it is -5.
; Carry flag has been SET, answer is incorrect as unsigned.
; Overflow flag has been UNSET, answer is correct as signed.

MVQ rg0, 0x7FFFFFFFFFFFFFFF  ; (hexadecimal for 9223372036854775807 as both signed and unsigned)
ADD rg0, 5
; As unsigned, rg0 is now 9223372036854775812. As signed it is -9223372036854775804.
; Carry flag has been UNSET, answer is correct as unsigned.
; Overflow flag has been SET, answer is incorrect as signed.

MVQ rg0, 0x7FFFFFFFFFFFFFFF
SUB rg0, 0xFFFFFFFFFFFFFFFF
; As unsigned, rg0 is now 9223372036854775808. As signed it is -9223372036854775808.
; Carry flag has been SET, answer is incorrect as unsigned.
; Overflow flag has been SET, answer is incorrect as signed.
```

## Floating Point Numbers

AssEmbly has instructions to perform operations on floating point values. These instructions work with the IEEE 754 double-precision floating point format (also known as `float64` or `double`). In this format, values, including whole numbers, are stored using an entirely different format from regular integer values, which means that, unlike with signed values, very little of the base instruction set can work with floating point values. Instead, instructions in the floating point instruction set (mnemonics starting with `FLPT_`) must be used. There is a full table towards the end of the document that details which instructions accept which formats of data.

To make an integer literal into a floating point literal, it must contain a decimal point (`.`). Any numeric literal containing a decimal point will be assembled into a 64-bit float.

For example:

```text
MVQ rg0, 5
; rg0 is 0x0000000000000005, which cannot be used in floating point operations

MVQ rg0, 5.0  ; The trailing 0 can be omitted to just have "5." if desired
; rg0 is 0x4014000000000000, or 5.0 in double floating point format,
; and can now be used in floating point operations
```

### Floating Point Math

There are floating point equivalents of all the math operations in the base instruction set, as well as some additional mathematical operations exclusive to floating point values. Integers and floating point values *cannot* be mixed when performing floating point operations; any integer values must be converted to a float first, as explained in the following section.

Some examples of basic floating point math:

```text
MVQ rg0, 5.7
FLPT_ADD rg0, 3.2
FLPT_WCN rg0
; "8.9" is printed to the console

MVQ rg1, -12.3
FLPT_MUL rg0, rg1
FLPT_WCN rg0
; "-109.47000000000001" is printed to the console (note the floating point inaccuracy)

MVQ rg0, 1.0
FLPT_DIV rg0, 3.0
FLPT_WCN rg0
; "0.3333333333333333" is printed to the console
```

> As can be seen with the second operation, floating point values cannot always represent decimal numbers with 100% accuracy, and may sometimes be off by a tiny fractional amount when converted to and from base 10.

Operations exclusive to floating point include trigonometric functions (i.e. Sine, Cosine, and Tangent and their inverses), single-instruction exponentiation, and logarithms. The trigonometric functions all operate on **radians** (a full circle is `2 * PI` radians). You can convert degrees to radians by multiplying the degrees by `0.017453292519943295` (`PI / 180`), and you can convert radians to degrees by multiplying the radians by `57.295779513082323` (`180 / PI`).

Some examples:

```text
MVQ rg0, 5.0
FLPT_POW rg0, 2.0
FLPT_WCN rg0
; "25" is printed to the console

FLPT_LOG rg0, 5.0
FLPT_WCN rg0
; "2" is printed to the console

FLPT_SIN rg0
FLPT_WCN rg0
; "0.9092974268256817" is printed to the console
```

### Converting Between Integers and Floats

Because integers and floating point values are stored in separate formats and are not implicitly compatible, you must explicitly convert between them to have data in the format expected by each instruction being used.

There are two instructions for converting integers to floats: `FLPT_UTF` and `FLPT_STF`. These interpret the integer value of a register as either unsigned or signed respectively, and convert it to its closest equivalent in floating point format. Be aware that integers that require more than 53 bits to represent as an integer may not be converted to an identical value as a float, due to precision limitations with large numbers in the double-precision floating point format.

Examples of integer to float conversion:

```text
MVQ rg0, 5
; rg0 is 0x0000000000000005, which cannot be used in floating point operations

FLPT_UTF rg0  ; FLPT_STF would produce the same result in this case
; rg0 is 0x4014000000000000, or 5.0 in double floating point format,
; and can now be used in floating point operations

MVQ rg0, -8
; rg0 is 0xFFFFFFFFFFFFFFF8
FLPT_STF rg0
; rg0 is 0xC020000000000000 (-8.0)

MVQ rg0, -8
; rg0 is 0xFFFFFFFFFFFFFFF8
FLPT_UTF rg0
; rg0 is 0x43F0000000000000 (18446744073709552000.0)
```

There are four instructions for converting floats to integers: `FLPT_FTS`, `FLPT_FCS`, `FLPT_FFS`, and `FLPT_FNS`. These convert a floating point value to an integer which can be interpreted as signed, using one of four rounding methods respectively: truncation (rounding toward zero), ceiling (rounding to the greater adjacent integer), floor (rounding to the lesser adjacent integer), and nearest (rounding to the closest integer, with exact midpoints being rounded to the adjacent integer that is even).

Examples of float to integer conversion:

```text
MVQ rg0, 5.7
FLPT_FTS rg0
SIGN_WCN rg0
; "5" is printed to console

MVQ rg0, 5.7
FLPT_FCS rg0
SIGN_WCN rg0
; "6" is printed to console

MVQ rg0, 5.7
FLPT_FFS rg0
SIGN_WCN rg0
; "5" is printed to console

MVQ rg0, 5.7
FLPT_FNS rg0
SIGN_WCN rg0
; "6" is printed to console

MVQ rg0, -5.7
FLPT_FTS rg0
SIGN_WCN rg0
; "-5" is printed to console

MVQ rg0, -5.7
FLPT_FCS rg0
SIGN_WCN rg0
; "-5" is printed to console

MVQ rg0, -5.7
FLPT_FFS rg0
SIGN_WCN rg0
; "-6" is printed to console

MVQ rg0, -5.7
FLPT_FNS rg0
SIGN_WCN rg0
; "-6" is printed to console
```

Some further examples of `FLPT_FNS` with midpoint and lower values:

```text
MVQ rg0, 5.5
FLPT_FNS rg0
SIGN_WCN rg0
; "6" is printed to console

MVQ rg0, 6.5
FLPT_FNS rg0
SIGN_WCN rg0
; "6" is printed to console

MVQ rg0, 2.5
FLPT_FNS rg0
SIGN_WCN rg0
; "2" is printed to console

MVQ rg0, 3.5
FLPT_FNS rg0
SIGN_WCN rg0
; "4" is printed to console

MVQ rg0, 12.4
FLPT_FNS rg0
SIGN_WCN rg0
; "12" is printed to console

MVQ rg0, 3.2
FLPT_FNS rg0
SIGN_WCN rg0
; "3" is printed to console
```

### Converting Between Floating Point Sizes

Floating point operations work solely on 64-bit floating point values, however there are other common sizes of floating point value which you may wish to convert between. There are instructions to convert to and from the half-precision (16-bit) and single-precision (32-bit) IEEE 754 floating point formats. To convert **to** a double-precision float, the `FLPT_EXH` and `FLPT_EXS` instructions are used to convert from half-precision and single-precision floats respectively. To convert **from** a double-precision float, the `FLPT_SHH` and `FLPT_SHS` instructions are used to convert to half-precision and single-precision floats respectively. You cannot convert directly between half- and single-precision floats without converting to a double-precision float first.

Here are some examples of direct conversion:

```text
MVQ rg0, 0x4248  ; 3.141 as a half-precision float
; rg0 cannot currently be used with floating point operations
FLPT_EXH rg0
; rg0 is now 0x4009200000000000 (3.140625) and can be used in floating point operations

MVQ rg0, 0x40490FDB  ; 3.1415927 as a single-precision float
; rg0 cannot currently be used with floating point operations
FLPT_EXS rg0
; rg0 is now 0x400921FB60000000 (3.14159274101257) and can be used in floating point operations

MVQ rg0, 3.141592653589793
; rg0 is 0x400921FB54442D18
FLPT_SHH rg0
; rg0 is now 0x4248 (3.141 as a half-precision float)

MVQ rg0, 3.141592653589793
; rg0 is 0x400921FB54442D18
FLPT_SHS rg0
; rg0 is now 0x40490FDB (3.1415927 as a single-precision float)
```

And one for converting a single-precision to a half-precision float:

```text
MVQ rg0, 0x40490FDB  ; 3.1415927 as a single-precision float
FLPT_EXS rg0
; rg0 is now 0x400921FB60000000 (3.14159274101257)
FLPT_SHH rg0
; rg0 is now 0x4248 (3.141 as a half-precision float)
```

## Jumping

Jumping is the processes of changing where the processor is currently executing in a program (represented with the `rpo` register). Jumps can be used to make loops, execute code if only a certain condition is met, or to reuse code, such as with subroutines. After a jump, the processor will continue to execute instructions from the new location, it will not automatically return to where it was before.

Jumps are usually made to labels, like so:

```text
MVQ rg0, 0  ; Set rg0 to 0
:ADD_LOOP  ; Create a label to the following instruction (ADD)
ADD rg0, 5  ; Add 5 to the current value of rg0
JMP :ADD_LOOP  ; Go back to ADD_LOOP and continue executing from there
```

> This program will set rg0 to 0, then infinitely keep adding 5 to the register by jumping back to the `ADD_LOOP` label. To only jump some of the time, for example to create a conditional loop, see the following section on branching.

Here is another example of a jump:

```text
MVQ rg0, 0
ADD rg0, 5
JMP :SKIP
ADD rg0, 5  ; This won't be executed
ADD rg0, 5  ; This won't be executed
:SKIP
; rg0 is 5 here
```

> `rg0` only ends up being 5 at the end of this example, as jumping to the `SKIP` label prevented the two other `ADD` instructions from being reached.

Jumps can also be made to pointers and address literals, though you must be sure that the address is that of a valid opcode before jumping there.

For example:

```text
MVQ rg0, :&MY_CODE  ; Move the literal address of MY_CODE to rg0
JMP *rg0  ; Jump to that address
MVQ rg0, 5  ; This won't be executed
:MY_CODE
MVQ rg0, 17
; rg0 will be 17, not 5
```

## Comparing, Testing, and Branching

Branching is similar to jumping in that it changes where in the program execution is currently taking place, however, when branching, a condition is checked first before performing the jump. If the condition is not met, the program will continue execution as normal without jumping anywhere.

The conditional jump instructions are as follows:

```text
+----------+----------------------------------+
| Mnemonic | Meaning                          |
+----------+----------------------------------+
| JEQ      | Jump if Equal                    |
| JNE      | Jump if not Equal                |
| JLT      | Jump if Less Than                |
| JLE      | Jump if Less Than or Equal To    |
| JGT      | Jump if Greater Than             |
| JGE      | Jump if Greater Than or Equal To |
+----------+----------------------------------+
| JZO      | Jump if Zero (=JEQ)              |
| JNZ      | Jump if not Zero (=JNE)          |
| JCA      | Jump if Carry (=JLT)             |
| JNC      | Jump if no Carry (=JGE)          |
+----------+----------------------------------+
| SIGN_JLT | Jump if Less Than                |
| SIGN_JLE | Jump if Less Than or Equal To    |
| SIGN_JGT | Jump if Greater Than             |
| SIGN_JGE | Jump if Greater Than or Equal To |
+----------+----------------------------------+
| SIGN_JSI | Jump if Sign                     |
| SIGN_JNS | Jump if not Sign                 |
| SIGN_JOV | Jump if Overflow                 |
| SIGN_JNO | Jump if not Overflow             |
+----------+----------------------------------+
```

The top section of instructions should be performed following a `CMP` operation on unsigned values, or a `FLPT_CMP` operation on floating point values. The instructions in the second section are aliases of four of the mnemonics in the top section (i.e. they share the same opcode) designed for use after mathematical operations or for bit testing (explained more in the relevant sections).

The bottom two sections are part of the signed extension set, with the higher of the two being designed for use following a `CMP` instruction on signed values, and the bottom section being for use specifically to branch based on the state of the sign or overflow flags.

### Comparing Unsigned Numbers

To branch based on how two unsigned (always positive) numbers relate to each other, the `CMP` instruction can be utilised. It takes two operands (the first of which must be a register — it won't be modified), and compares them for use with a conditional jump instruction immediately afterwards.

For example:

```text
RNG rg0  ; Set rg0 to a random number
CMP rg0, 1000  ; Compare rg0 to 1000
JGT :GREATER  ; Jump straight to GREATER if rg0 is greater than 1000
ADD rg0, 1000  ; This will execute only if rg0 is less than or equal to 1000
:GREATER
SUB rg0, 1000  ; This will execute in either situation
```

> Be aware that the `GREATER` label will still be reached if `rg0` is less than or equal to `1000` here, the `ADD` instruction will just be executed first.
> To have the contents of the `GREATER` label execute **only** if `rg0` is greater than `1000`, include an unconditional jump like so:

```text
RNG rg0  ; Set rg0 to a random number
CMP rg0, 1000  ; Compare rg0 to 1000
JGT :GREATER  ; Jump straight to GREATER if rg0 is greater than 1000
ADD rg0, 1000  ; This will execute only if rg0 is less than or equal to 1000
JMP :END  ; Jump straight to END to prevent GREATER section being executed
:GREATER
SUB rg0, 1000  ; This will execute only if rg0 is greater than 1000
:END
```

The `CMP` instruction works by subtracting the second operand from the first, but not storing the result anywhere. This operation still updates the status flags (`rsf`) however, and these can be used to check how the numbers relate. For example, if the second operand is greater than the first, you can guarantee that the operation will set the carry flag, as it would cause the result to be negative. This means to check if the first is greater than or equal to the second, you can simply check if the carry flag was unset. To check if the values were equal, the zero flag can be checked, as if the two operands of a subtraction are equal, the result will always be zero.

A full list of what each conditional jump instruction is checking for in terms of the status flags can be found in the full instruction reference.

### Comparing Signed Numbers

The `CMP` instruction can also be used to compare signed (negative and positive) values, with its usage and behaviour remaining unchanged. After using the `CMP` instruction, however, you should use the signed version of the base conditional jump instructions, e.g. `SIGN_JLT` instead of `JLT`. The only exception to this is `JEQ` and `JNE`, which do not have signed versions, as they work with both signed and unsigned values.

For example:

```text
MVQ rg0, 25
MVQ rg1, -6
CMP rg0, rg1
SIGN_JGT :GREATER
WCN 10  ; This will not execute, 25 is greater than -6
:GREATER
WCN 20  ; This will execute
; Only "20" is output to the console
```

And what would happen if the regular `JGT` instruction was used:

```text
MVQ rg0, 25
MVQ rg1, -6
CMP rg0, rg1
JGT :GREATER
WCN 10  ; This will execute, even though 25 is greater than -6
:GREATER
WCN 20  ; This will execute
; "1020" is output to the console, -6 was interpreted instead as 18446744073709551610
```

> Here the comparison doesn't work as expected because the conditional jump used (`JGT`) only works assuming the comparison was intended to be unsigned. The signed versions of these instructions (like `SIGN_JGT`) use the state of the sign, overflow, and zero status flags so that they work as expected when used after signed comparisons. A full list of what each conditional jump instruction is checking for in terms of the status flags can be found in the full instruction reference.

### Comparing Floating Point Numbers

To compare two floating point values, the `FLPT_CMP` instruction needs to be used instead of the `CMP` instruction. After using `FLPT_CMP`, the **unsigned** version of the desired conditional jump should be used, **even if one or both of the floating point values were negative**. There are no dedicated conditional jump instructions for floating point values.

For example:

```text
MVQ rg0, 25.4
MVQ rg1, -6.3
FLPT_CMP rg0, rg1
JGT :GREATER
WCN 10  ; This will not execute, 25.4 is greater than -6.3
:GREATER
WCN 20  ; This will execute
; Only "20" is output to the console
```

`FLPT_CMP` updates the status flags with the unsigned conditional jumps in mind. If the first operand is less than the second, the carry flag is set. If they are equal, the zero flag is set. The overflow flag is always `0` after using `FLPT_CMP`.

### Testing Bits

To test if a single bit of a number is set or not, the `TST` instruction can be used. Just like `CMP`, it takes two operands, the first of which being a register. The second should usually be a binary literal with only a single bit (the one to check) set as 1. It should then be followed by either `JZO` (jump if zero), or `JNZ` (jump if not zero). An example of where this may be used is checking if the third bit of `rsf` is set (the file end flag), as there isn't a built-in conditional jump that checks this flag.

This would be done like so:

```text
:READ
RFC rg0  ; Read the next byte from the open file to rg0
TST rsf, 0b100  ; Check if the third bit is set
JZO :READ  ; If it isn't set (i.e. it is equal to 0), jump back to READ
```

> This program will keep looping until the third bit of `rsf` becomes `1`. meaning that the end of the file has been reached.

Similarly to `CMP`, `TST` works by performing a bitwise AND on the two operands, discarding the result, but still updating the status flags. A bitwise AND will ensure that only the bit you want to check remains as `1`, but only if it started as `1`. If a bit is not one that you are checking, or it wasn't `1` to start with, it will end up as `0`. If the resulting number isn't zero, leaving the zero flag unset, the bit must've been `1`, and vice versa.

### Checking the Carry, Overflow, Zero, and Sign Flags

The carry, overflow, zero, and sign flags also have specific jump operations that can check if they are currently set or unset.

For example:

```text
MVQ rg0, 5
SUB rg0, 10
JCA :CARRY  ; Jump to label if carry flag is set
WCN 10  ; This will not execute, as 5 SUB 10 will cause the carry flag to be set
:CARRY
WCN 20
; Only "20" will be written to the console
```

> `JCA` here is checking if the carry flag is set or not following the subtraction. The jump will only occur if the carry flag is `1` (set), otherwise, as with the other jump types, execution will continue as normal. `JNC` can be used to perform the inverse, jump only if the carry flag is unset.

The zero flag checks can also be used following a mathematical operation like so:

```text
SUB rg0, 7  ; Subtract 7 from rg0
JNZ :NOT_ZERO  ; Jump straight to NOT_ZERO if rg0 didn't become 0
ADD rg0, 1  ; Only execute this if rg0 became 0 because of the SUB operation
:NOT_ZERO
```

> The `ADD` instruction here will only execute if the subtraction by 7 caused `rg0` to become exactly equal to `0`.

The `SIGN_JOV`, `SIGN_JNO`, `SIGN_JSI`, and `SIGN_JNS` instructions can be used to check if the overflow and sign flags are set and unset respectively in the same way:

```text
SUB rg0, 7  ; Subtract 7 from rg0
SIGN_JNS :NOT_NEGATIVE  ; Jump straight to NOT_NEGATIVE if rg0 didn't become negative
SIGN_NEG rg0  ; Only execute this if rg0 became negative because of the SUB operation
:NOT_NEGATIVE
; rg0 is now the absolute result
```

An equivalent of the first example, but for the overflow flag instead of the carry flag, as should be used for signed operations:

```text
MVQ rg0, 5
SUB rg0, 10
JOV :OVERFLOW  ; Jump to label if overflow flag is set
WCN 10  ; This will execute, as 5 SUB 10 will not cause the overflow flag to be set
:OVERFLOW
WCN 20
; "1020" will be written to the console
```

## Assembler Directives

Assembler directives follow the same format as standard instructions, however, instead of being assembled to an opcode for the processor to execute, they instead instruct the assembler itself to do something. They may insert data into the final program file, affect the lines of source code being assembled, or change the state of the assembler itself.

Directives are all prefixed with a percent sign (`%`) to distinguish them from regular instructions.

### %PAD - Byte Padding

The `%PAD` directive tells the assembler to insert a certain number of `0` bytes wherever the directive is placed in the file. This is most often used just after a label definition to allocate a certain amount of guaranteed free and available memory to store data.

For example, consider the following program:

```text
MVQ rg0, :&PADDING  ; Store the address of the padding in rg0
JMP :PROGRAM  ; Jump to the next part of the program, skipping over the padding

:PADDING
%PAD 16  ; Insert 16 empty bytes

:PROGRAM
MVQ *rg0, 765  ; Set the first 8 bytes of the padding to represent 765
ADD rg0, 8  ; Add 8 to rg0, it now points to the next number
```

> This program would assemble to the following bytes:

```text
99 06 13 00 00 00 00 00 00 00 02 23 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 9F 06 FD 02 00 00 00 00 00 00 11 06 08 00 00 00 00 00 00 00
```

> Which can be broken down to:

```text
Address | Bytes
--------+----------------------------------------------------
 0x00   | 99             | 06  | 13 00 00 00 00 00 00 00
        | MVQ (reg, lit) | rg0 | :PADDING (address 0x13)
--------+----------------------------------------------------
 0x0A   | 02  | 23 00 00 00 00 00 00 00
        | JMP | :PROGRAM (address 0x23)
--------+----------------------------------------------------
 0x13   | 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
        | %PAD 16
--------+----------------------------------------------------
 0x23   | 9F             | 06   | FD 02 00 00 00 00 00 00
        | MVQ (ptr, lit) | *rg0 | 765 (0x2FD)
--------+----------------------------------------------------
 0x2D   | 11  | 06  | 08 00 00 00 00 00 00 00
        | ADD | rg0 | 8
```

Note that usually, to reduce the number of jumps required, `%PAD`s would be placed after all program instructions. It was put in the middle of the program here for demonstration purposes.

### %DAT - Byte Insertion

The `%DAT` directive inserts either a single byte, or a string of UTF-8 character bytes, into a program wherever the directive is located. As with `%PAD`, it can be directly preceded by a label definition to point to the byte or string of bytes. If not being used with a string, `%DAT` can only insert single bytes at once, meaning the maximum value is 255. It is also not suitable for inserting numbers to be used in 64-bit expecting operations (such as maths and bitwise), see the following section on the `%NUM` directive for inserting 64-bit numbers.

An example of single byte insertion:

```text
MVB rg0, :BYTE  ; MVB must be used, as %DAT will not insert a full 64-bit number
; rg0 is now 54
HLT  ; Stop the program executing into the %DAT insertion (important!)

:BYTE
%DAT 54  ; Insert a single 54 byte (0x36)
```

> This program assembles into the following bytes:

```text
82 06 0B 00 00 00 00 00 00 00 00 36
```

> Which can be broken down to:

```text
Address | Bytes
--------+----------------------------------------------------
 0x00   | 82             | 06  | 0B 00 00 00 00 00 00 00
        | MVB (reg, adr) | rg0 | :BYTE (address 0x0B)
--------+----------------------------------------------------
 0x0A   | 00
        | HLT
--------+----------------------------------------------------
 0x0B   | 36
        | %DAT 54
```

To insert a string using `%DAT`, the desired characters must be surrounded by double quote marks (`"`) and be given as the sole operand to the directive. For example:

```text
MVQ rg0, :&STRING  ; Move literal address of string to rg0
:STRING_LOOP
MVB rg1, *rg0  ; Move contents of address stored in rg0 to rg1
CMP rg1, 0  ; Check if rg1 is 0
JEQ :END  ; If it is, stop program
ICR rg0  ; Otherwise, increment source address by 1
WCC rg1  ; Write the read character to the console
JMP :STRING_LOOP  ; Loop back to print next character

:END
HLT  ; End execution to stop processor running into string data

:STRING
%DAT "Hello!\0"  ; Store a string of character bytes after program data.
; Note that the string ends with '\0' (a 0 or "null" byte)
```

> This program will loop through the string, placing the byte value of each character in `rg0` and writing it to the console, until it reaches the 0 byte, when it will then stop to avoid looping infinitely.

While not a strict requirement, terminating a string with a 0 byte like this should always be done to give an easy way of knowing when the end of a string has been reached. Placing a `%DAT 0` directive on the line after the string insertion will also achieve this 0 termination, and will result in the exact same bytes being assembled, however using the `\0` escape sequence is more compact. Escape sequences are explained toward the end of the document along with a table listing all of the possible sequences.

The example program assembles down to the following bytes:

```text
99 06 2E 00 00 00 00 00 00 00 83 07 06 75 07 00 00 00 00 00 00 00 00 04 2D 00 00 00 00 00 00 00 14 06 CC 07 02 0A 00 00 00 00 00 00 00 00 48 65 6C 6C 6F 21 00
```

> Which can be broken down to:

```text
Address | Bytes
--------+----------------------------------------------------
 0x00   | 99             | 06  | 2E 00 00 00 00 00 00 00
        | MVQ (reg, lit) | rg0 | :STRING (address 0x2E)
--------+----------------------------------------------------
 0x0A   | 83             | 07  | 06
        | MVB (reg, ptr) | rg1 | *rg0
--------+----------------------------------------------------
 0x0D   | 75             | 07  | 00 00 00 00 00 00 00 00
        | CMP (reg, lit) | rg1 | 0
--------+----------------------------------------------------
 0x17   | 04        | 2D 00 00 00 00 00 00 00
        | JEQ (adr) | :END (address 0x2D)
--------+----------------------------------------------------
 0x20   | 14        | 06
        | ICR (reg) | rg0
--------+----------------------------------------------------
 0x22   | CC        | 07
        | WCC (reg) | rg1
--------+----------------------------------------------------
 0x24   | 02        | 0A 00 00 00 00 00 00 00
        | JMP (adr) | :STRING_LOOP (address 0x0A)
--------+----------------------------------------------------
 0x2D   | 00
        | HLT
--------+----------------------------------------------------
 0x2E   | 48 65 6C 6C 6F 21 00
        | %DAT "Hello!\0"
```

### %NUM - Number Insertion

The `%NUM` directive is similar to `%DAT`, except it always inserts 8 bytes exactly, so can be used to represent 64-bit numbers for use in instructions which always work on 64-bit values, like maths and bitwise operations. `%NUM` cannot be used to insert strings, only single 64-bit numerical values (including unsigned, signed, and floating point).

An example:

```text
MVQ rg0, 115  ; Initialise rg0 to 15
ADD rg0, :NUMBER  ; Add the number stored in memory to rg0
; rg0 is now 100130
HLT  ; End execution to stop processor running into number data

:NUMBER
%NUM 100_015  ; Insert the number 100015 with 8 bytes
```

> Which will produce the following bytes:

```text
99 06 73 00 00 00 00 00 00 00 12 06 15 00 00 00 00 00 00 00 00 AF 86 01 00 00 00 00 00
```

> Breaking down into:

```text
Address | Bytes
--------+----------------------------------------------------
 0x00   | 99             | 06  | 73 00 00 00 00 00 00 00
        | MVQ (reg, lit) | rg0 | 115 (0x73)
--------+----------------------------------------------------
 0x0A   | 12             | 06  | 15 00 00 00 00 00 00 00
        | ADD (reg, adr) | rg0 | :NUMBER (address 0x15)
--------+----------------------------------------------------
 0x14   | 00
        | HLT
--------+----------------------------------------------------
 0x15   | AF 86 01 00 00 00 00 00
        | %NUM 100_015 (0x186AF)
```

As with other operations in AssEmbly, `%NUM` stores numbers in memory using little endian encoding. See the section on moving with memory for more info on how this encoding works. You can also use `%NUM` to insert the resolved address of a label as an 8-byte value in memory. The label must use the ampersand prefix syntax (i.e. `:&LABEL_NAME`).

### %IMP - Import AssEmbly Source File

The `%IMP` directive inserts lines of AssEmbly source code from another file into wherever the directive is placed. It allows a program to be split across multiple files, as well as allowing code to be reused across multiple source files without having to copy the code into each file. The directive takes a single string operand (which must be enclosed in quotes), which can either be a full path (i.e. `Drive:/Folder/Folder/file.asm`) or a path relative to the directory of the source file being assembled (i.e. `file.asm`, `Folder/file.asm`, or `../Folder/file.asm`).

For example, suppose you had two files in the same folder, one called `program.asm`, and one called `numbers.asm`.

> Contents of `program.asm`:

```text
MVQ rg0, :NUMBER_ONE
MVQ rg1, :NUMBER_TWO
HLT  ; Prevent program executing into number data

%IMP "numbers.asm"
```

> Contents of `numbers.asm`:

```text
:NUMBER_ONE
%NUM 123

:NUMBER_TWO
%NUM 456
```

> When `program.asm` is assembled, the assembler will open and include the lines in `numbers.asm` once it reaches the `%IMP` directive, resulting in the file looking like so:

```text
MVQ rg0, :NUMBER_ONE
MVQ rg1, :NUMBER_TWO
HLT  ; Prevent program executing into number data

%IMP "numbers.asm"
:NUMBER_ONE
%NUM 123

:NUMBER_TWO
%NUM 456
```

> Meaning that `rg0` will finish with a value of `123`, and `rg1` will finish with a value of `456`.

The `%IMP` directive simply inserts the text contents of a file into the current file for assembly. This means that any label names in files being imported will be usable in the main file, though imposes the added restriction that label names must be unique across the main file and all its imported files.

Files given to the `%IMP` directive are assembled as AssEmbly source code, so **must** be AssEmbly source files, not already assembled binaries. To insert the raw binary contents of a file into the assembled program, use the `%IBF` directive. It is recommended, though not a strict requirement, that import statements are placed at the end of a file, as that will make it easier to ensure that the imported contents of a file aren't executed by mistake as part of the main program.

Care should be taken to ensure that a file does not end up depending on itself, even if it is through other files, as this will result in an infinite loop of imports (also known as a circular dependency). The AssEmbly assembler will detect these and throw an error should one occur.

An example of a circular dependency:

> `file_one.asm`:

```text
%IMP "file_two.asm"
```

> `file_two.asm`:

```text
%IMP "file_three.asm"
```

> `file_three.asm`:

```text
%IMP "file_one.asm"
```

> Attempting to assemble any of these three files would result in the assembler throwing an error, as each file ends up depending on itself as it resolves its import.

### %IBF - Import Binary File Contents

The `%IBF` directive inserts the raw binary contents of a file into a program wherever the directive is located. It differs from the `%IMP` directive in that the contents of the file is neither assembled nor otherwise manipulated in any way by the assembler, it is simply inserted as-is into the final assembled program. The directive takes a single string operand (which must be enclosed in quotes), which can either be a full path (i.e. `Drive:/Folder/Folder/file.bin`) or a path relative to the directory of the source file being assembled (i.e. `file.bin`, `Folder/file.bin`, or `../Folder/file.bin`).

For example, suppose you had two files in the same folder, one called `program.asm`, and one called `string.txt`.

> Contents of `program.asm`:

```text
MVQ rg0, :&STRING
:LOOP
MVQ rg1, *rg0
TST rg1, rg1
JZO :END
WCC rg1
ICR rg0
JMP :LOOP
:END
HLT  ; Prevent program executing into string data

:STRING
%IBF "string.txt"
%DAT 0
```

> Contents of `string.txt`:

```text
Hello, world!
```

> This program will print `Hello, world!` to the console when executed, with the string "Hello, world!" now being contained within the program itself.
> Assembling the program produces the following bytes:

```text
99 06 27 00 00 00 00 00 00 00 9B 07 06 70 07 07 04 26 00 00 00 00 00 00 00 CC 07 14 06 02 0A 00 00 00 00 00 00 00 00 48 65 6C 6C 6F 2C 20 77 6F 72 6C 64 21 00
```

> Breaking down into:

```text
Address | Bytes
--------+----------------------------------------------------
 0x00   | 99             | 06  | 27 00 00 00 00 00 00 00
        | MVQ (reg, lit) | rg0 | 39 (0x27)
--------+----------------------------------------------------
 0x0A   | 9B             | 07  | 06
        | MVQ (reg, ptr) | rg1 | rg0
--------+----------------------------------------------------
 0x0D   | 70             | 07  | 07
        | TST (reg, reg) | rg1 | rg1
--------+----------------------------------------------------
 0x10   | 04             | 26 00 00 00 00 00 00 00
        | JZO (adr)      | 38 (0x26)
--------+----------------------------------------------------
 0x19   | CC             | 07
        | WCC (reg)      | rg1
--------+----------------------------------------------------
 0x1B   | 14             | 06
        | ICR (reg)      | rg0
--------+----------------------------------------------------
 0x1D   | 04             | 0A 00 00 00 00 00 00 00 00
        | JMP (adr)      | 10 (0x0A)
--------+----------------------------------------------------
 0x26   | 00
        | HLT
--------+----------------------------------------------------
 0x27   | 48 65 6C 6C 6F 2C 20 77 6F 72 6C 64 21
        | H  e  l  l  o  ,     W  o  r  l  d  !
--------+----------------------------------------------------
 0x34   | 00
        | %DAT 0
```

Note that the file given to the `%IBF` directive does not need to contain plain text; any data can be inserted.

### %ANALYZER - Toggling Assembler Warnings

The AssEmbly assembler checks for common issues with your source code when you assemble it in order to alert you of potential issues and improvements that can be made. There may be some situations, however, where you want to suppress these issues from being detected. This can be done within the source code using the `%ANALYZER` directive. The directive takes three operands: the severity of the warning (either `error`, `warning`, or `suggestion`); the numerical code for the warning (this is a 4-digit number printed alongside the message); and whether to enable (`1`), disable (`0`) or restore the warning to its state as it was at the beginning of assembly (`r`).

After using the directive, its effect remains active until assembly ends, or the same warning is toggled again with the directive further on in the code.

For example:

```text
CMP rg0, 0  ; generates suggestion 0005

%ANALYZER suggestion, 0005, 0
CMP rg0, 0  ; generates no suggestion
CMP rg0, 0  ; still generates no suggestion
%ANALYZER suggestion, 0005, 1  ; 'r' would also work if the suggestion isn't disabled via a CLI argument

CMP rg0, 0  ; generates suggestion 0005 again
```

Be aware that some analyzers do not run until the end of the assembly process and so cannot be re-enabled without inadvertently causing the warning to re-appear. This can be overcome by placing the disabling `%ANALYZER` directive at the end of the base file for any analyzers where this behaviour is an issue, or by simply not re-enabling the analyzer.

### %MESSAGE - Manually Emit Assembler Warning

The `%MESSAGE` directive can be used to cause a custom assembler message (i.e. an error, warning, or suggestion) to be given for the line that the directive is used on. One operand is required: the severity of the message to raise (either `error`, `warning`, or `suggestion`). A second, optional operand can also be given, which must be a quoted string literal to use as the content of the custom message.

Two examples of the directive being used:

```text
%MESSAGE suggestion
%MESSAGE warning, "This needs changing"
```

Manually emitted messages always have the code `0000`, regardless of severity. Messages, even with the error severity, will not cause the assembly process to fail and have no effect on the final program output.

### %LABEL_OVERRIDE - Manual Label Address Assignment

By default, labels store the address of the instruction that they are directly followed by. It is, however, possible to store a custom address within a label. To do so, define one or more labels as normal, then follow them directly with a single `%LABEL_OVERRIDE` directive. The directive takes one operand, the address to assign to all the directly preceding labels.

For example:

```text
:MY_LABEL
:MY_OTHER_LABEL
%LABEL_OVERRIDE 1234
```

> This program defines two labels (`MY_LABEL` and `MY_OTHER_LABEL`) that will point to the address `1234` (`0x4D2`) when used, despite the labels not being defined at a line with that address.

The address given can also be a reference to another label. The label should be in ampersand-prefixed form. For example:

```text
:POINTS_TO_SOME_CODE
%LABEL_OVERRIDE :&SOME_CODE

MVQ rg2, rg4
MVQ rg5, rg3

:SOME_CODE
MVQ rg0, rg3
```

> Both `SOME_CODE` and `POINTS_TO_SOME_CODE` will store the same address here, despite `POINTS_TO_SOME_CODE` being defined before `SOME_CODE` and not being directly above the target code. It is possible to reference labels that are not yet defined, as long as they are defined by the time assembly finishes.

### %REPEAT - Repeat Lines of Source Code

The `%REPEAT` directive is used to assemble a block of lines multiple times, without needing to duplicate the lines within the source code. The directive takes a single operand, the number of times to repeat the lines. This number cannot be zero. The lines to repeat start immediately after the `%REPEAT` directive, and are ended by using the **`%ENDREPEAT`** directive. Each repetition is assembled immediately after the last.

For example:

```text
SUB rfp, 8
%REPEAT 3
    ICR rfp
    MVB rg0, *rfp
    MVB *rg1, rg0
    ICR rg1
%ENDREPEAT
ADD rg1, 16
```

> The above example is equivalent to the following program and will produce the same program bytes when assembled:

```text
SUB rfp, 8
ICR rfp
MVB rg0, *rfp
MVB *rg1, rg0
ICR rg1
ICR rfp
MVB rg0, *rfp
MVB *rg1, rg0
ICR rg1
ICR rfp
MVB rg0, *rfp
MVB *rg1, rg0
ICR rg1
ADD rg1, 16
```

> The lines within the `%REPEAT` block are now present 3 times within the program (the number of repeats given to the directive includes the initial instance).
> Keep in mind that the leading whitespace on each line inside the repeat block (the **indentation**) is completely optional, and can be omitted or made a different number of spaces if you wish. Its purpose is merely to improve the readability of the program.

Assembler directives can also be present within `%REPEAT` blocks and will be processed multiple times by the assembler, just like regular instructions.

`%REPEAT` blocks can be nested within each other, with nested `%REPEAT` blocks themselves being repeated along with any surrounding lines.

For example:

```text
SUB rfp, 8

%REPEAT 4
    ICR rfp
    MVB rg0, *rfp
    %REPEAT 3
        DCR rg2
    %ENDREPEAT
    MVB *rg1, rg0
    ICR rg1
%ENDREPEAT

ADD rg1, 16
```

> The above example is equivalent to the following program and will produce the same program bytes when assembled:

```text
SUB rfp, 8

ICR rfp
MVB rg0, *rfp
DCR rg2
DCR rg2
DCR rg2
MVB *rg1, rg0
ICR rg1
ICR rfp
MVB rg0, *rfp
DCR rg2
DCR rg2
DCR rg2
MVB *rg1, rg0
ICR rg1
ICR rfp
MVB rg0, *rfp
DCR rg2
DCR rg2
DCR rg2
MVB *rg1, rg0
ICR rg1
ICR rfp
MVB rg0, *rfp
DCR rg2
DCR rg2
DCR rg2
MVB *rg1, rg0
ICR rg1

ADD rg1, 16
```

> Lines within the first `%REPEAT` block are all repeated 4 times. The line within the second `%REPEAT` block is repeated 3 times *for every repeat of the outer `%REPEAT` block*, meaning the line within the second block is repeated a total of 12 (`4 * 3`) times.

### %ASM_ONCE - Guard a File from Being Assembled Multiple Times

Due to the restriction that all label definitions must be unique, it is usually not possible to import a file that defines any labels multiple times. This can pose an issue, especially for files belonging to re-usable libraries, which may end up being depended on by multiple different files in the same project. Importing the same file many times also takes up unnecessary program memory, and may result in the inadvertent multiple execution of certain directives. These issues can be mitigated by using the `%ASM_ONCE` directive. When the assembler encounters an instance of this directive, it checks if it has already assembled the current imported file before. If it has, it immediately skips to the end of the file, thereby resuming assembly of the parent file. Any instructions located before the `%ASM_ONCE` directive in the file will still be assembled multiple times. The directive doesn't take any operands.

In the specific case that the `%ASM_ONCE` directive is the first instruction in a file, it will prevent a circular import error being thrown if that file ends up importing itself. The assembler will simply skip over the entire contents of the file instead, preventing any infinite loops in the process.

It is not possible to use the `%ASM_ONCE` directive in the initial (base) file, and doing so will result in the assembler throwing an error, immediately stopping the assembly process.

Whether or not a file has been assembled before is determined by a case-sensitive check of the file's path. The `%ASM_ONCE` directive will **not** prevent a file with identical contents but located within a different file from being assembled. Furthermore, symbolic links will **not** be considered to be the same file as the file that they link to.

### %MACRO - Macro Definition

The `%MACRO` directive defines a **macro**, a piece of text that the assembler will replace (**expand**) on every line where the text is present. The text that a macro replaces can also be referred to as the macro's **name**. There are two distinct types of macro: **single-line** macros and **multi-line** macros. Both are defined with the `%MACRO` directive. Single-line macros replace portions of text *within* a line, whereas multi-line macros replace an entire line with multiple new lines of AssEmbly code. Macros only take effect on lines after the one where they are defined, and they can be overwritten to change the replacement text by defining a new macro with the same name as a previous one. Single-line and multi-line macros with the same name cannot exist simultaneously, and defining a new single-line macro with the same name as an existing multi-line macro or vice versa will result in the existing macro being replaced.

Macro names can contain almost any character, with the exception of opening and closing brackets (`(` and `)`). They are case sensitive, and macros with the same name but different capitalisations can exist simultaneously. Almost all lines are subject to macro expansion, with the automatic exception of lines that start with `%MACRO` or `%DELMACRO` before macro expansion begins. You can also manually exclude any line from being processed for macros, as explained in the section on disabling macro expansion.

#### Single-line Macros

To define a single-line macro, the `%MACRO` directive should be given two operands. The first is the text to replace, and the second is the text for it to be replaced with. Unlike other instructions, the operands to the `%MACRO` directive don't have to be a standard valid format of operand, both will automatically be interpreted as literal text.

During the assembly process, for every line in the source code (with some exceptions as explained later), the assembler will search to see if the name of an existing single-line macro is present on the line. If one is, the macro name is removed from the line and replaced with the macro's replacement text in the same position. The surrounding text is unaffected. This then becomes the new content of the line, and the search process restarts, now including the text that was just inserted. The assembler will continue replacing the names of single-line macros on the line until there are no more matches. This means that the *replacement* content of a macro, or even the combination of the replacement text and the text already on the line, can itself also contain the name of a macro that will be replaced.

In the event that there are multiple single-line macros present on the line at the same time, the macro closest to the start of the line (the left) will be expanded first. If multiple macros match starting at the same character (i.e. there are multiple macros with names that start with the same character/characters), the macro with the **longest** name takes priority.

For example:

```text
MVQ rg0, Number  ; Results in an error

%MACRO Number, 345
MVQ rg0, Number
; rg0 is now 345

%MACRO Num, 123
%MACRO Number, 678  ; (lines starting with %MACRO are exempt from macro expansion
                    ;  - so we can define "Number" here without the "Num" part being replaced)
MVQ rg1, Number
; rg1 is now 678 (the longer macro name takes priority)

%MACRO Inst, ICR rg1
Inst
; rg1 is now 679

%MACRO Inst, OtherMacro
%MACRO OtherMacro, ADD rg1, 6
Inst
; rg1 is now 685 ("Inst" was replaced with "OtherMacro", which was then replaced with "ADD rg1, 6")
```

> The first line here results in an error, as a macro with a name of `Number` hasn't been defined yet (macros don't apply retroactively). `MVQ rg0, Number` gets replaced with `MVQ rg0, 345`, setting `rg0` to `345`. `MVQ rg1, Number` gets replaced with `MVQ rg1, 678`, as the `Number` macro was redefined on the line before, setting `rg1` to `678`. `Inst` gets replaced with `ICR rg1`, incrementing `rg1` by `1`, therefore setting it to `679` (macros can contain spaces and can be used to give another name to mnemonics, or even entire instructions, as seen in the last example). Because macro contents isn't processed until the macro is used, `Inst` was able to contain `OtherMacro` even though `OtherMacro` didn't exist when it was defined.

Note that single-line macro definitions ignore many standard syntax rules due to each operand being interpreted as literal text. Both operands can contain whitespace, and the second operand may contain commas. Be aware that aside from a **single** space character separating the `%MACRO` mnemonic from its operands, leading and trailing whitespace in the first operand, and leading whitespace in the second operand, will not be removed. Macros can also contain quotation marks (`"`), which will not be immediately parsed as a string within the macro. If the quotation marks are placed into a line as replacement text, they will be parsed normally as a part of the line.

#### Multi-line Macros

Multi-line macros are defined by only giving a single operand to `%MACRO` - the text to replace. Every line after the directive is then treated as the contents of the macro, up until the next detected instance of the **`%ENDMACRO`** directive. As with single-line macros, the name of a multi-line macro does not need to be in any standard format, and is automatically interpreted as literal text. The contents of the macro itself will not contain the opening `%MACRO` directive or the closing `%ENDMACRO` directive. None of the instructions or directives inside the macro contents will be assembled until the macro is used.

To use a multi-line macro, the name of the macro must be the sole contents of the line, not including surrounding whitespace. The entire multi-line contents of the macro will then be inserted, and assembly will continue from the first line in the macro. Once the end of the macro's contents is reached, assembly will then carry on assembling the lines after the macro's use.

For example:

```text
%MACRO multi-line
    ICR rg0
    DCR rg1
%ENDMACRO

MVQ rg0, 5
MVQ rg1, 5
multi-line
multi-line
multi-line
WCN rg0
WCC '\n'  ; Newline
WCN rg1
```

> When executed, this program will print `8` (the value of `rg0`) and `2` (the value of `rg1`) to the console, as each instance of `multi-line` was replaced with both the `ICR` *and* `DCR` instructions. The fully expanded program is equivalent to the following:

```text
MVQ rg0, 5
MVQ rg1, 5
ICR rg0
DCR rg1
ICR rg0
DCR rg1
ICR rg0
DCR rg1
WCN rg0
WCC '\n'  ; Newline
WCN rg1
```

> Both of the above examples result in the same program bytes after being assembled, as macros are expanded by the assembler, not the processor. The *definition* of a macro does not itself insert any bytes into the program - the macro must be used for its contents to be inserted.
> Keep in mind that the leading whitespace on each line inside the macro definition (the **indentation**) is completely optional, and can be omitted or made a different number of spaces if you wish. Its purpose is merely to improve the readability of the program.

The assembler does not attempt to search for multi-line macros until every possible single-line macro replacement has been completed on the line. This means that a single-line macro, or a combination of single-line macros, can contain the name of a multi-line macro to expand, as long as the multi-line macro name ends up as the only thing on the line.

Because the contents of a multi-line macro is terminated as soon as the first instance of `%ENDMACRO` is found, it is not possible to nest a multi-line macro definition within another multi-line macro definition. It is, however, possible to define *single-line* macros inside a multi-line macro.

Both single-line and multi-line macros can be used inside multi-line macros, however they will not be expanded until the macro is used. This means that the `%ENDMACRO` directive cannot itself be contained within a single-line or multi-line macro. It also means that macros that aren't defined until *after* the definition of the multi-line macro can still be referenced from within it, as long as they are defined before each usage of the macro. Macro expansions that occur within the inserted contents of a multi-line macro do not affect the original content of the macro, so the contents of referenced macros can change between usages of the same multi-line macro.

Multi-line macros cannot reference themselves, even indirectly through another macro. If at any point during the assembly of a multi-line macro's contents the assembler detects any multi-line macro it is already currently in the process of expanding, it will stop and throw an error. This detection does not occur until the *usage* of the macro. If the macro is defined but never used, the error will not be thrown.

For example, this program is invalid and will throw an error:

```text
%MACRO my_macro1
    MVQ rg0, 123
    ADD rg0, 456
    my_macro2
%ENDMACRO

%MACRO my_macro2
    MVQ rg1, 654
    SUB rg1, 321
    my_macro1
%ENDMACRO

my_macro1  ; Throws error
```

> Even though neither `my_macro1` nor `my_macro2` directly contain themselves, they still end up cyclically referring to themselves through each other. The assembler will detect this, and stop assembly.
> This program, however, is valid, and will assemble without failure:

```text
%MACRO my_macro1
    MVQ rg0, 123
    ADD rg0, 456
    my_macro2
    my_macro2
    my_macro3
    my_macro3
%ENDMACRO

%MACRO my_macro2
    MVQ rg1, 654
    SUB rg1, 321
    my_macro3
%ENDMACRO

%MACRO my_macro3
    MVQ rg2, 246
    MUL rg2, 810
%ENDMACRO

my_macro1  ; No error thrown
```

> Throughout the expansion of `my_macro1`, `my_macro3` gets expanded multiple times, both directly and indirectly. However, this never occurs while another instance of `my_macro3` is still in the process of being expanded, so it is valid. As long as the expansion of a multi-line macro has finished by the time it is referenced again, it is perfectly valid to reference it multiple times from within the same macro.

It is not valid to have an `%ENDMACRO` directive in the source code without a paired multi-line `%MACRO` directive, and all multi-line `%MACRO` directives *must* end with an `%ENDMACRO` directive. It is not valid to have a multi-line `%MACRO` directive reach the end of the program without an `%ENDMACRO` directive.

#### Disabling Macro Expansion

It is possible to disable the expansion of both single-line and multi-line macros on a line. This can be done for individual lines by prefixing them with a single exclamation mark (`!`), or for an entire block of lines by surrounding them with the `!>` and `<!` characters respectively.

For example:

```text
%MACRO rg0, rg1  ; Would replace all instances of "rg0" with "rg1" if matched

!MVQ rg0, rg2  ; Macro does not match - instruction stays as "MVQ rg0, rg2"
MVQ rg0, rg2  ; Macro matches - instruction becomes "MVQ rg1, rg2"
!>
MVQ rg0, rg2  ; Macro does not match - instruction stays as "MVQ rg0, rg2"
MVQ rg0, rg2  ; Macro does not match - instruction stays as "MVQ rg0, rg2"
<!
MVQ rg0, rg2  ; Macro matches - instruction becomes "MVQ rg1, rg2"
```

The beginning and ending markers for a macro disabling block **must** be the only thing on their respective lines.

For example, this is **not** valid and will result in an error:

```text
!>MVQ rg0, rg1
MVQ rg2, rg3<!
```

Unlike multi-line macro definitions, macro disabling blocks *can* run to the end of the program without being closed (i.e. you can have `!>` in a program without ever having `<!`). This will effectively disable macro expansion for the entire rest of the program after the block is opened, so care should be taken to ensure you don't accidentally forget a `<!` closing marker. It is not valid to use a `<!` closing marker when a macro disabling block is not currently open. Every `<!` closing marker must be paired with a corresponding `!>` opening marker. It is also not valid to use a `!>` opening marker when a macro disabling block is already open, meaning you cannot nest macro disabling blocks within each other.

Neither the `!` line prefix nor the `!>` and `<!` markers can be inserted as the result of a single-line macro expansion, however the `!>` and `<!` markers *can* be used within the contents of a multi-line macro, as long as they are still the sole thing on their respective lines. Lines within a multi-line macro can also use the `!` prefix. Macro disabling blocks inside a multi-line macro will take effect on each *usage* of a multi-line macro, not on the macro's definition, and whether or not a macro disabling block is currently open or not will persist in to and out of the lines inserted by the multi-line macro. Surrounding the *definition* of a multi-line macro in a macro disabling block has no effect, and will **not** disable macro expansion within the macro's content when the macro is used.

Some examples:

```text
%MACRO start disable block
    !>
    ; This line WILL BE inside a macro disabling block when it is inserted by the macro
%ENDMACRO

; This line is NOT inside a macro disabling block

start disable block

; This line IS inside a macro disabling block

<!

; This line is NOT inside a macro disabling block
```

```text
%MACRO my_macro
    !>
    ; This line WILL BE inside a macro disabling block when it is inserted by the macro
    <!
    ; This line WILL NOT be inside a macro disabling block when it is inserted by the macro
%ENDMACRO
```

```text
%MACRO another_macro
    ...
%ENDMACRO

!>
another_macro
; The open macro disabling block will also affect the lines inserted by the above macro
<!

another_macro
; Despite being the same macro - the lines inserted by the above macro are now not part of a macro disabling block
```

#### Macro Parameters

Both single-line and multi-line macros are also capable of inserting text that varies between usages of the macro. This is achieved through the use of **parameters**. Macros can take multiple distinct parameters, which can be accessed many times and in any order.

To use parameters within a macro definition, type a dollar sign (`$`) followed by the desired 0-based index of the parameter within the list of parameters. Each time the macro is expanded, these characters will then be removed and replaced with the given parameter text in the same position. The parameter indices can be more than one digit long, though they must not be separated by any characters.

To give parameters while using either a single-line or multi-line macro, type an opening bracket (`(`) directly after the macro name. There must be no extra whitespace between the macro name and the opening bracket. After the bracket, give all the desired parameters separated by commas (`,`), then type a closing bracket (`)`). The commas themselves will be removed and will not become a part of the parameter, however any surrounding whitespace will not be removed. All the text within the brackets, as well as the brackets themselves, will be removed when the macro is replaced with its replacement text. For single-line macros, text after the closing bracket will continue to be assembled as part of the line as normal. For multi-line macros, there must be no further text after the closing bracket - the macro name and its parameters must be the only thing on the respective line.

For example:

```text
%MACRO register, rg$0

DVR register(0), register(1), rg2

%MACRO instruction
    ADD $0, $1
    SUB $1, 5
%ENDMACRO

instruction(rg2,rg3)
```

> Here, the line `DVR register(0), register(1), rg2` gets expanded to `DVR rg0, rg1, rg2`, as the `$0` is removed and replaced with the first (and in this case only) parameter given to the `register` macro. `instruction(rg2,rg3)` gets expanded to the following lines:

```text
    ADD rg2, rg3
    SUB rg3, 5
```

It is possible to give parameters to a macro even if it never references them, and it is even possible to give parameters to a macro that doesn't reference any parameters at all. In both cases the extraneous parameters will simply be ignored. For example, if instead of `instruction(rg2,rg3)`, `instruction(rg2,rg3,rg4)` were written in the above example, it would make no difference to the final output, as the parameter `$2` is never referenced.

Macro parameters and macro names do not need to be separated by any characters, for example, the following is perfectly valid:

```text
%MACRO surround,$0$1$0

MVQ rg0, surround(1,2)surround(3,4)
```

> This gets expanded to `MVQ rg0, 121343`. Note that the whitespace after the comma in the macro definition was omitted, otherwise it would have been included as a part of the replacement text. Leading whitespace in macro replacement text is *never* trimmed, so it should be removed if it is not desired.

##### Nesting Macros Inside Parameters

The individual separated parameters of both single-line and multi-line macros are, themselves, also subjected to single-line macro expansion. This makes it possible to nest single-line macro usages within parameters. These nested macros can even be given parameters themselves, to a theoretically infinite depth.

For example:

```text
%MACRO register, rg$0

%MACRO instruction
    ADD $0, $1
%ENDMACRO

instruction(register(2), register(3))
```

> Here `instruction(register(2), register(3))` expands to `ADD rg2, rg3`, as the `register(2)` macro was expanded, followed by the `register(3)` macro, which then became parameters to the final expansion of `instruction`.

A single parameter can also contain multiple single-line macros if desired, as they follow the same single-line macro expansion logic as regular lines.

##### Making Parameters Required

By default, if a parameter is referenced by a macro but it is not given when the macro is used, the parameter reference (i.e. the `$` sign and the following index) will still be removed, however no text will be inserted in its place. If this is not desired, you can mark a parameter as *required* by appending an exclamation mark (`!`) directly after the parameter index. The assembler will stop and throw an error if a required parameter is not provided when a macro is used.

For example:

```text
%MACRO my_macro, 12$0

ADD rg0, my_macro  ; No error thrown
```

> In this example `ADD rg0, my_macro` expands to `ADD rg0, 12`, as the `$0` is replaced with empty text.
> To instead throw an error, the example should be written like this:

```text
%MACRO my_macro, 12$0!

ADD rg0, my_macro  ; Throws error
```

> The assembler detects that `my_macro` is attempting to access a parameter at index `0`, but finds that one doesn't exist. Because the parameter has been suffixed with an `!`, the assembler now stops assembly and throws an error instead of replacing it with empty text.

If the same parameter is used multiple times within a macro, only one instance of the parameter needs to be suffixed with an `!` for that parameter to become required. Be aware that a required parameter **can** still be empty without an error being thrown if it is explicitly made so when the macro is used.

For example, all of the parameters in the following examples are empty text but will not throw an error, even if the parameter was marked as required:

```text
uses-1-param()  ; $0 is given but empty, all other parameters are not given
uses-2-params(,)  ; $0 and $1 are given but empty, all other parameters are not given
uses-3-params(,,)  ; $0, $1, and $2 are given but empty, all other parameters are not given
; etc.
```

As seen above, an opening bracket followed immediately by a closing bracket (`()`) is interpreted by the assembler as passing a **single, empty** parameter. You should **not** add these opening and closing brackets after a macro name if your intention is to pass zero parameters.

It is **not** possible for a macro nested inside another macro to access the parent macro's parameters. They must be explicitly passed on to the nested macro by the parent macro if this access is desired.

For example:

```text
%MACRO my_macro1
    MVQ rg0, $0!
    ADD rg0, $1!
%ENDMACRO

%MACRO my_macro2
    MVQ rg1, $0!
    SUB rg1, $1!
    my_macro1
%ENDMACRO

my_macro2(123,456)  ; Throws error
```

> Here, an error is thrown by the assembler, as `my_macro1` has been used by `my_macro2` without giving it its required parameters. `my_macro1` does not automatically get access to `my_macro2`'s parameters.

The program must instead be written like so:

```text
%MACRO my_macro1
    MVQ rg0, $0!
    ADD rg0, $1!
%ENDMACRO

%MACRO my_macro2
    MVQ rg1, $0!
    SUB rg1, $1!
    my_macro1($0,$1)
%ENDMACRO

my_macro2(123,456)  ; No error thrown
```

> Here `my_macro2` is now explicitly sharing its parameters with `my_macro1`, so this program is valid.

Parameter references can be contained within the parameters of a nested macro, and they will be replaced just as they would be anywhere else within the containing macro. The replacement happens before the parameter text is inserted into the contents of the macro being nested.

##### Escaping Macro-Specific Special Characters

Because commas and brackets have special meanings within macro parameters, if you want to provide a comma or bracket character as a literal part of a parameter's content, you must precede each instance of one with a backslash. These backslashes will not be included as a part of the inserted parameter text. Backslashes themselves must be doubled up (i.e. having one backslash preceded by another: `\\`) in order to treat them as literal text.

For example:

```text
%MACRO instruction, $0 $1

instruction(MVQ,rg0\,rg1)
```

> Here `instruction(MVQ,rg0\,rg1)` expands to `MVQ rg0,rg1`. The first comma was treated as normal, separating the first and second parameters and being removed from the final text, however the second comma became part of the second parameter's contents itself, with the backslash being removed instead.

These backslashes are distinct from those found in string escape sequences, as they are processed as part of the macro expansion, before strings even begin being parsed. As a result of this, backslashes being used as string escape sequences must also be doubled up.

For example:

```text
%MACRO insert_string, %DAT "$0"

insert_string(a\\nb\\nc)
```

> `insert_string(a\\nb\\nc)` expands to `%DAT "a\nb\nc"`, as each backslash has another backslash to escape it so that it becomes a part of the final string. The final string inserted into the program will be the bytes `61 0A 62 0A 63`.

Similarly, if you wish to have a literal `$` character within the contents of a macro definition, you must instead use two directly adjacent to each other (`$$`) to prevent the dollar sign being interpreted as the start of a parameter.

For example:

```text
%MACRO balance, %DAT "Your balance is $$$0"

balance(1.23)
```

> The final string inserted by `balance(1.23)` will be `"Your balance is $1.23"`, as the first two `$` signs became a literal dollar sign character, and the third was simply interpreted as the start of a parameter reference as normal.

Keep in mind that this is only required within a macro *definition*. You should only use a single `$` character if you wish to have a literal dollar sign as a part of a parameter to a macro being used outside a macro definition.

#### Deleting Macros

Macros can be deleted at any point after being defined by using the **`%DELMACRO`** directive. It takes a single operand, the name of the macro to delete. Both single-line and multi-line macros are deleted the same way.

Deletion of a macro takes effect immediately. Any subsequent uses of the macro after the `%DELMACRO` directive is used will not be replaced. This includes macros nested within other macros, even if the now deleted macro existed when the containing macros were defined. Deleted macros can be defined again by using the `%MACRO` directive as normal, however you do not *need* to delete a macro in order to redefine it.

The `%DELMACRO` directive can be used from within a macro, however, as with everything else inside macros, it will not be executed until the macro is used, and will be executed *every* time the macro is used.

Using the `%DELMACRO` directive with a macro name that does not exist will immediately stop assembly and cause an error to be thrown. This will happen even if the macro *used* to exist, meaning you cannot delete the same macro twice (unless it has been redefined in the meantime).

### %DEFINE - Assembler Variable Definition

The `%DEFINE` directive creates an **assembler variable**, a named storage location that exists for the duration of the assembly process. The directive takes two operands: the name of the variable to define, and the value to assign to it. You can also assign a new value to an existing variable by giving the existing name as the first operand. Assembler variables are always 64-bit integers, and can be inserted into a program as a literal number by writing their name prefixed with an at sign (`@`) at the desired location in the program. Variable names follow the same restrictions as label names: they are case sensitive, and can only contain letters, numbers, and underscores. Unlike labels, the first character in a variable name *can* be a number.

For example:

```text
%DEFINE MY_VARIABLE, 123

MVQ rg0, @MY_VARIABLE
```

> When assembled `MVQ rg0, @MY_VARIABLE` becomes `MVQ rg0, 123`, meaning `rg0` will be given a value of `123` when the program is executed.

Assembler variables can be inserted anywhere in the program, including inside strings. When encountered by the assembler, the name and `@` sign are replaced with the literal base-10 value of the variable at the time it was encountered. This substitution occurs after all macros on the line have been expanded. The name of the assembler variable to insert is determined by scanning every character after the `@` sign until the first character that is not a valid name character is found, or the end of the line is reached. All of the valid characters are then treated as part of the variable name. Attempting to insert a variable that does not exist will result in the assembler throwing an error, stopping the assembly process. Scanned variable names will **never** be truncated, therefore *all* valid label name characters directly after an `@` sign *must* be a part of the desired variable name. For example, if only a variable with the name `VAR` exists, then typing `@VARIABLE` would result in an error, as a variable with the name `VARIABLE` does not exist, even though a variable with the name `VAR` does.

To use a literal `@` sign within a string and not have it interpreted as the start of an assembler variable, prefix it with a backslash (`\`) like any other escape sequence.

For example:

```text
%DEFINE MY_VARIABLE, 0xFF_FF

:STRING
%DAT "This is the value of \@MY_VARIABLE: @MY_VARIABLE"

%DEFINE MY_VARIABLE, 123
```

> The string in this example will result in the literal text `This is the value of @MY_VARIABLE: 65535` being assembled. It does not matter what formatting was used to define the variable, the inserted value will always be in plain base-10. It also does not matter that `MY_VARIABLE` was redefined later on, as only the current value of the variable is taken into account when inserting it.

Assembler variables are not replaced on lines that begin with `%MACRO` or `%DELMACRO` to allow `@` signs to be used as a part of macro names. Variables *are* still replaced on lines prefixed with `!` and within macro disabling blocks, however.

#### Assembler Constants

Assembler constants are special, predefined variables that have values set by the assembler itself. You cannot manipulate these variables in any way, only read from them. Their names are all prefixed with an exclamation mark to distinguish them from regular variables.

The following table is a list of all assembler constants and their purpose:

| Constant                    | Static? | Description                                                                                                                                                               |
|-----------------------------|---------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `@!ASSEMBLER_VERSION_MAJOR` | Yes     | The major version of AssEmbly being used to assemble the program (the first number in a version formatted as `x.x.x`)                                                     |
| `@!ASSEMBLER_VERSION_MINOR` | Yes     | The minor version of AssEmbly being used to assemble the program (the second number in a version formatted as `x.x.x`)                                                    |
| `@!ASSEMBLER_VERSION_PATCH` | Yes     | The patch version of AssEmbly being used to assemble the program (the third number in a version formatted as `x.x.x`)                                                     |
| `@!V1_FORMAT`               | Yes     | Has a value of `1` if the program is being assembled into the header-less AAP format used by AssEmbly version 1, otherwise has a value of `0`                             |
| `@!V1_CALL_STACK`           | Yes     | Has a value of `1` if the program will be executed using the "3 registers pushed" calling convention used by AssEmbly version 1, otherwise has a value of `0`             |
| `@!IMPORT_DEPTH`            | No      | The current size of the import stack. Starts at `0` when used within the base file, then increments for every import statement, and decrements when an imported file ends |
| `@!CURRENT_ADDRESS`         | No      | The current number of bytes that have been inserted into the program so far. Equivalent to the address that the current instruction will start at when assembled          |
| `@!OBSOLETE_DIRECTIVES`     | Yes     | Has a value of `1` if the use of pre-3.2 directives is enabled, otherwise has a value of `0`                                                                              |
| `@!ESCAPE_SEQUENCES`        | Yes     | Has a value of `1` if post-1.1 string escape sequences are enabled, otherwise has a value of `0`                                                                          |

"Static" assembler constants will retain the same value throughout the entire assembly process. Other assembler constants may change throughout assembly depending on where they are used.

#### Deleting Assembler Variables

To delete an assembler variable after it has been defined, use the **`%UNDEFINE`** directive. It takes a single operand, the name of the assembler variable to delete. For example, `%UNDEFINE MY_VARIABLE` will delete an assembler variable with the name `MY_VARIABLE`. Assembler constants cannot be deleted.

Deletion of a variable takes effect immediately and the variable will no longer be usable until and unless it is re-defined. To re-define a deleted variable, simply use the `%DEFINE` directive as normal. You do not *need* to delete a variable in order to redefine it, however.

The name of the variable given to the `%UNDEFINE` directive must already exist. It is not valid to give a non-existent variable to the directive, even if the variable *used* to exist but was deleted.

### %VAROP - Assembler Variable Operation

The `%VAROP` directive is used to perform mathematical and logical operations on assembler variables. These operations are performed by the assembler at assembly-time, not assembled into the program (recall the processor has no concept of "variables", only memory and registers). The directive takes three operands: the operation to perform, the name of the assembler variable to operate on, and the numeric value to operate with. The variable to operate on cannot be an assembler constant.

The first operand can be any one of the following:

| Operation | Name                          | Purpose                                                                                     |
|-----------|-------------------------------|---------------------------------------------------------------------------------------------|
| `ADD`     | Addition                      | Add a value to the variable                                                                 |
| `SUB`     | Subtraction                   | Subtract a value from the variable                                                          |
| `MUL`     | Multiplication                | Multiply the variable by a value                                                            |
| `DIV`     | Division                      | Divide the variable by a value                                                              |
| `REM`     | Remainder                     | Divide the variable by a value, then store the remainder in the variable                    |
| `BIT_AND` | Bitwise AND                   | Store the bitwise AND of a variable and a value in the variable                             |
| `BIT_OR`  | Bitwise OR                    | Store the bitwise OR of a variable and a value in the variable                              |
| `BIT_XOR` | Bitwise XOR                   | Store the bitwise XOR of a variable and a value in the variable                             |
| `BIT_NOT` | Bitwise NOT                   | Store the bitwise NOT of a value in a variable                                              |
| `AND`     | Logical AND                   | Store `1` in a variable if both the variable and a value are non-zero, else store `0`       |
| `OR`      | Logical OR                    | Store `1` in a variable if either the variable or a value are non-zero, else store `0`      |
| `XOR`     | Logical XOR                   | Store `1` in a variable if only one of the variable or a value are non-zero, else store `0` |
| `NOT`     | Logical NOT                   | Store `0` in a variable if a value is non-zero, else store `1`                              |
| `SHL`     | Bit Shift Left                | Shift the bits in a variable left by a given number of places                               |
| `SHR`     | Bit Shift Right               | Shift the bits in a variable right by a given number of places                              |
| `CMP_EQ`  | Compare Equal                 | Store `1` in a variable if both the variable and a value are equal, else store `0`          |
| `CMP_NEQ` | Compare Not Equal             | Store `1` in a variable if both the variable and a value are not equal, else store `0`      |
| `CMP_GT`  | Compare Greater Than          | Store `1` in a variable the variable is greater than a value, else store `0`                |
| `CMP_GTE` | Compare Greater Than or Equal | Store `1` in a variable the variable is greater than or equal to a value, else store `0`    |
| `CMP_LT`  | Compare Less Than             | Store `1` in a variable the variable is less than a value, else store `0`                   |
| `CMP_LTE` | Compare Less Than or Equal    | Store `1` in a variable the variable is less than or equal to a value, else store `0`       |

For example:

```text
%DEFINE MY_VARIABLE, 5

%VAROP ADD, MY_VARIABLE, 6
; MY_VARIABLE is now 11

%VAROP MUL, MY_VARIABLE, 4
; MY_VARIABLE is now 44

%VAROP CMP_GT, MY_VARIABLE, 55
; MY_VARIABLE is now 0
```

As with anywhere else, the numeric literal for the value to operate with can also be a reference to an assembler variable or assembler constant.

For example:

```text
%DEFINE MY_VARIABLE, 5
%DEFINE MY_OTHER_VARIABLE, 10

%VAROP ADD, MY_VARIABLE, @MY_OTHER_VARIABLE
; MY_VARIABLE is now 15

%VAROP DIV, MY_VARIABLE, @MY_VARIABLE
; MY_VARIABLE is now 1
```

Note that if an assembler variable is being used as the third operand, it must be prefixed with an `@` sign so that it is replaced with a numeric literal by the assembler.

### %IF - Conditional Assembly

The `%IF` directive is used to start a **conditional assembly** block, a block of code that will only be assembled if a given condition is met. When the assembler encounters an `%IF` directive, it will check if the condition given in the directive is met. If it is, the assembler will continue to assemble the following lines as normal. If it is not, the assembler will skip ahead to the end of the conditional block (marked by the **`%ENDIF`** directive) and continue from there, resulting in none of the lines within the block being processed. All conditional blocks **must** end with a single `%ENDIF` directive. Having a conditional block run to the end of the program without one will result in an error and the assembly process failing. An error will also be thrown if the `%ENDIF` directive is used outside of an existing conditional block.

There are two types of check an `%IF` condition can perform: assembler variable existence and literal value comparison. For both, the first operand to the `%IF` directive is always the conditional operation to perform.

#### Checking Variable Existence

To check for the existence of an assembler variable, either the `DEF` (defined) or `NDEF` (not defined) operation should be given as the first operand to the `%IF` directive. A second operand then must be given, the name of the variable to check for. For the `DEF` operation, the condition passes if the variable *does* exist. The opposite is true for `NDEF`.

For example:

```text
%DEFINE MY_VARIABLE, 0

%IF DEF, MY_VARIABLE
    ; Code here WILL be assembled
%ENDIF

%IF NDEF, MY_VARIABLE
    ; Code here will NOT be assembled
%ENDIF

%IF DEF, SOME_OTHER_VARIABLE
    ; Code here will NOT be assembled
%ENDIF

%IF NDEF, SOME_OTHER_VARIABLE
    ; Code here WILL be assembled
%ENDIF
```

#### Comparing Values

There are six literal value comparisons available for use with the `%IF` directive: `EQ` (equal), `NEQ` (not equal), `GT` (greater than), `GTE` (greater than or equal), `LT` (less than), and `LTE` (less than or equal). After the comparison operation, two additional operands are required, the literal values to compare. Usually one or both of these will be a reference to an assembler variable, which must be prefixed with an `@` sign to replace it with its literal value.

For example:

```text
%DEFINE MY_VARIABLE, 0
%DEFINE MY_OTHER_VARIABLE, 5

%IF EQ, @MY_VARIABLE, 0
    ; Code here WILL be assembled
%ENDIF

%IF NEQ, @MY_VARIABLE, 0
    ; Code here will NOT be assembled
%ENDIF

%IF GT, @MY_VARIABLE, 5
    ; Code here will NOT be assembled
%ENDIF

%IF LTE, @MY_VARIABLE, @MY_OTHER_VARIABLE
    ; Code here WILL be assembled
%ENDIF
```

#### %ELSE / %ELSE_IF

The `%ELSE` and `%ELSE_IF` directives can be used to assemble a block of code in the case that the condition for an `%IF` block *doesn't* succeed. `%ELSE_IF` directives behave almost identically to `%IF` blocks and take the same operands, however are only evaluated if the condition for the prior conditional block failed. The `%ELSE` directive takes no operands, with `%ELSE` blocks always being assembled if the condition of the conditional directive they follow failed. Any `%ELSE` and `%ELSE_IF` directives must be located *before* the `%ENDIF` directive that closes the conditional block.

An `%IF` block can be followed by any number of `%ELSE_IF` blocks, with each `%ELSE_IF` directive only being evaluated if the last one failed. Once a successful `%ELSE_IF` condition is reached, or if the initial `%IF` condition was successful, none of the following `%ELSE_IF` conditions will be evaluated. `%ELSE_IF` blocks can also be followed by a single `%ELSE` block which will only be assembled if *all* of the above `%ELSE_IF` conditions failed.

For example:

```text
%DEFINE MY_VARIABLE, 0
%DEFINE MY_OTHER_VARIABLE, 5

%IF EQ, @MY_VARIABLE, 0
    ; Code here WILL be assembled
%ELSE
    ; Code here will NOT be assembled
%ENDIF

%IF GT, @MY_VARIABLE, @MY_OTHER_VARIABLE
    ; Code here will NOT be assembled
%ELSE_IF LT, @MY_VARIABLE, 10
    ; Code here WILL be assembled
%ELSE_IF LT, @MY_VARIABLE, 20
    ; Code here will NOT be assembled
%ELSE
    ; Code here will NOT be assembled
%ENDIF

%IF DEF, MY_VARIABLE
    ; Code here WILL be assembled
%ELSE_IF DEF, MY_OTHER_VARIABLE
    ; Code here will NOT be assembled
%ELSE
    ; Code here will NOT be assembled
%ENDIF

%IF NDEF, MY_VARIABLE
    ; Code here will NOT be assembled
%ELSE_IF NDEF, NON_EXISTENT_VARIABLE
    ; Code here WILL be assembled
%ENDIF
```

Note that `%ELSE` and `%ELSE_IF` blocks automatically close the conditional block above them, they do not warrant the use of an additional `%ENDIF` directive.

#### Nesting Conditional Blocks

Conditional blocks can be located within other conditional blocks. Inner blocks will only be evaluated if all the outer blocks that they are contained within pass their condition.

For example:

```text
%DEFINE MY_VARIABLE, 0
%DEFINE MY_OTHER_VARIABLE, 5

%IF EQ, @MY_VARIABLE, 0
    ; Code here WILL be assembled

    %IF NEQ, @MY_VARIABLE, 0
        ; Code here will NOT be assembled
    %ELSE
        ; Code here WILL be assembled
    %ENDIF
%ENDIF

%IF GT, @MY_OTHER_VARIABLE, 20
    ; Code here will NOT be assembled

    %IF GT, @MY_OTHER_VARIABLE, 1
        ; Code here will NOT be assembled
    %ELSE
        ; Code here will NOT be assembled
    %ENDIF
%ENDIF
```

> Keep in mind that the leading whitespace on each line inside the conditional blocks (the **indentation**) is completely optional, and can be omitted or made a different number of spaces if you wish. Its purpose is merely to improve the readability of the program.

### %WHILE - Conditional Source Code Repetition

The `%WHILE` directive continually repeats a block of source code as long as a condition remains true, making it effectively a combination of the `%REPEAT` and `%IF` directives. The available condition operations and operands to the directive are the same as those to the `%IF` directive. Similarly to the repeat directive, the repeated lines are terminated with the **`%ENDWHILE`** directive, and `%WHILE` blocks can be nested within each other.

The condition for the block is re-evaluated before each insertion. If it fails, the repetition immediately stops and the assembler skips straight to the `%ENDWHILE` directive. If the condition for the `%WHILE` block is not met when the block is first encountered, no instructions from within the block will be inserted.

For example:

```text
%DEFINE MY_VARIABLE, 0

%WHILE LT, @MY_VARIABLE, 30
    MVQ rg0, @MY_VARIABLE
    %VAROP ADD, MY_VARIABLE, 5
%ENDWHILE

%WHILE LT, @MY_VARIABLE, 60
    MVQ rg3, @MY_VARIABLE
    %DEFINE MY_OTHER_VARIABLE, 4
    %WHILE GT, @MY_OTHER_VARIABLE, 0
        MVQ rg1, @MY_OTHER_VARIABLE
        %VAROP SUB, MY_OTHER_VARIABLE, 1
    %ENDWHILE
    MVQ rg2, @MY_VARIABLE
    %VAROP ADD, MY_VARIABLE, 10
%ENDWHILE

%WHILE EQ, @MY_VARIABLE, 0
    ADD rg0, rg1
%ENDWHILE
```

> This program is effectively equivalent to the following example and will assemble to the same bytes:

```text
MVQ rg0, @MY_VARIABLE
MVQ rg0, @MY_VARIABLE
MVQ rg0, @MY_VARIABLE
MVQ rg0, @MY_VARIABLE
MVQ rg0, @MY_VARIABLE
MVQ rg0, @MY_VARIABLE

MVQ rg3, @MY_VARIABLE
MVQ rg1, @MY_OTHER_VARIABLE
MVQ rg1, @MY_OTHER_VARIABLE
MVQ rg1, @MY_OTHER_VARIABLE
MVQ rg1, @MY_OTHER_VARIABLE
MVQ rg2, @MY_VARIABLE

MVQ rg3, @MY_VARIABLE
MVQ rg1, @MY_OTHER_VARIABLE
MVQ rg1, @MY_OTHER_VARIABLE
MVQ rg1, @MY_OTHER_VARIABLE
MVQ rg1, @MY_OTHER_VARIABLE
MVQ rg2, @MY_VARIABLE

MVQ rg3, @MY_VARIABLE
MVQ rg1, @MY_OTHER_VARIABLE
MVQ rg1, @MY_OTHER_VARIABLE
MVQ rg1, @MY_OTHER_VARIABLE
MVQ rg1, @MY_OTHER_VARIABLE
MVQ rg2, @MY_VARIABLE
```

> The first `%WHILE` block is repeated six times, as `5` can only be added to `0` six times before it becomes greater than or equal to `30`. The second `%WHILE` block is repeated three times, as `10` can only be added to `30` three times before it becomes greater than or equal to `60`. The nested `%WHILE` block is repeated four times **every time the outer `%WHILE` block repeats**, as `MY_OTHER_VARIABLE` is reset for every outer repetition, and `1` can only be subtracted from `4` four times before it becomes less than or equal to `0`. The final `%WHILE` block is never assembled as its condition is never met.

### %STOP - End Assembly

The `%STOP` directive is used to immediately end the assembly process. As soon as the assembler reaches the directive, it stops and throws a guaranteed error. The assembly will not be considered successful, and no executable file will be produced. The `%STOP` directive optionally takes a single string operand to use as the message to display when assembly stops.

`%STOP` directives are most useful when combined with conditional assembly to guarantee that a particular condition is met before continuing with assembly.

For example:

```text
%IF NDEF, MY_VARIABLE
    %STOP "@MY_VARIABLE is a required variable. Please define it."
%ENDIF
```

> This example immediately ends assembly if a variable with the name `MY_VARIABLE` does not exist, thereby ensuring that `MY_VARIABLE` *will* be defined for anything after this `%IF` block.

## Console Input and Output

AssEmbly has native support for reading and writing from the console. There are four types of write that can be performed: 64-bit number in decimal; byte in decimal; byte in hexadecimal; and a raw byte (character). There is only a single type of read: a single raw byte. There is no native support for reading numbers in any base, nor is there support for reading or writing multiple numbers/bytes at once.

Writing can be done from registers, literals, and memory locations; reading must be done to a register. As with the move instructions, if a byte write instruction is used on a register or literal, only the lowest byte will be considered. If one is used on a memory address (e.g. a label or a pointer), only a single byte of memory will be read, as an opposed to the 8 bytes that are read when writing a 64-bit number.

An example of each type of write:

```text
MVQ rg0, 0xFF0062

WCN rg0  ; Write a 64-bit number to the console in decimal
; "16711778" (0xFF0062) is written to the console

WCC '\n'  ; Write a newline character

WCB rg0  ; Write a single byte to the console in decimal
; "98" (0x62) is written to the console

WCC '\n'  ; Write a newline character

WCX rg0  ; Write a single byte to the console in hexadecimal
; "62" is written to the console

WCC '\n'  ; Write a newline character

WCC rg0  ; Write a single byte to the console as a character
; "b" (0x62) is written to the console

WCC '\n'  ; Write a newline character
```

Keep in mind that newlines are not automatically written after each write instruction, you will need to manually write the raw byte `10`/`0xA` (a newline character - can be represented with the escape sequence `\n`) to start writing on a new line. See the ASCII table at the end of the document for other common character codes.

An example of reading a byte:

```text
RCC rg0  ; Read a byte from the console and save the byte code to rg0
```

When an `RCC` instruction is reached, the program will pause execution and wait for the user to input a character to the console. Once a character has been inputted, the corresponding byte value of the character will be copied to the given register. In this example, if the user types a lowercase "b", `0x62` would be copied to `rg0`.

Be aware that if the user types a character that requires multiple bytes to represent in UTF-8, `RCC` will still only retrieve a single byte. You will have to use `RCC` multiple times to get all of the bytes needed to represent the character. `WCC` will also only write a single byte at a time, though as long as the console has UTF-8 support, simply writing each UTF-8 byte one after the other will result in the correct character being displayed.

Note that the user does not need to press enter after inputting a character, execution will resume immediately after a single character is typed. If you wish to wait for the user to press enter, compare the inputted character to the newline character. The example program `input.ext.asm` contains a subroutine which does this. The user pressing the enter key will always give a single `10`/`0xA` newline byte, regardless of platform.

## File Handling

As well as interfacing with the console, AssEmbly also has native support for handling files.

### Opening and Closing

Files must be explicitly opened with the `OFL` instruction before they can read or written to, and only one file can be open at a time. You should close the currently open file with the `CFL` instruction when you have finished operating on it. It is important that you ensure that any open file is closed with `CFL` before the processor halts, as if the program ends while there is a file still open, any data written to the open file may be incorrectly or only partially saved.

Filepaths given to `OFL` to be opened should be strings of UTF-8 character bytes in memory, ending with at least one `0` byte. An example static filepath definition is as follows:

```text
:FILE_PATH
%DAT "file.txt\0"
```

> This would normally be placed after all program code and a `HLT` instruction to prevent it accidentally being executed as if it were part of the program.
> The file can be opened with the following line anywhere in the program:

```text
OFL :FILE_PATH
...
CFL
```

> You could also use a pointer if you wish:

```text
MVQ rg0, :&FILE_PATH
OFL *rg0
...
CFL
```

`CFL` will close whatever file is currently open, so does not require any operands. If a file at the specified path does not exist when it is opened, an empty one will be created.

### Reading and Writing

Reading and writing from files is almost identical to how it is done from the console. Registers, literals, and data in memory can all be written, and reading must be done to a register. When using byte writing instructions, only the lower byte of registers and literals is considered, and only a single byte of memory is read for memory locations. An open file can be both read from and written to while it is open, though changes written to the file will not be reflected in either the current AssEmbly program or other applications until the file is closed. If a file already has data in it when it is written to, the new data will start overwriting from the first byte in the file. Any remaining data that does not get overwritten will remain unchanged, and the size of the file will not change unless more bytes are written than were originally in the file. To clear a file before writing it, use the `DFL` instruction to delete the file beforehand.

An example of writing to a file:

```text
MVQ rg0, 0xFF0062
OFL :FILE_PATH  ; Open file with the 0-terminated string at :FILE_PATH

WFN rg0  ; Write a 64-bit number to the file in decimal
; "16711778" (0xFF0062) is appended to the file

WFC '\n'  ; Write a newline character

WFB rg0  ; Write a single byte to the file in decimal
; "98" (0x62) is appended to the file

WFC '\n'  ; Write a newline character

WFX rg0  ; Write a single byte to the file in hexadecimal
; "62" is appended to the file

WFC '\n'  ; Write a newline character

WFC rg0  ; Write a single byte to the file as a character
; "b" (0x62) is appended to the file

WFC '\n'  ; Write a newline character
CFL  ; Close the file, saving newly written contents

HLT  ; Prevent executing into string data

:FILE_PATH
%DAT "file.txt\0"
```

> Executing this program will create a file called `file.txt` with the following contents:

```text
16711778
98
62
b

```

File contents can be read with the `RFC` instruction, taking a single register as an operand. The next unread byte from the file will be stored in the specified register. Text files are not treated specially, `RFC` will simply retrieve the characters 1 byte at a time as they are encoded in the file. If the end of the file has been reached after reading, the file end flag will be set to `1`. The only way to reset the current reading position in a file is to close and reopen the file.

To read all bytes until the end of a file, you will need to continually read single bytes from the file, testing the file end flag after every read, stopping as soon as it becomes set. The example program `read_file.asm` has an example of this, as well as this example from the bit testing section:

```text
:READ
RFC rg0  ; Read the next byte from the open file to rg0
TST rsf, 0b100  ; Check if the third bit is set
JZO :READ  ; If it isn't set (i.e. it is equal to 0), jump back to READ
```

### Other Operations

As well as reading and writing, there are also instructions for checking whether a file exists (`FEX`), getting the size of a file (`FSZ`), and deleting a file (`DFL`). They all take a path in the same way `OFL` does. `DFL` has no effect other than deleting the file. `FEX` and `FSZ` first take a register operand to store their result in, then the path to the file as the second operand. `FEX` stores `1` in the register if the file exists, `0` if not. `FSZ` stores the total size of the file in bytes.

## The Stack

The stack is a section of memory most often used in conjunction with subroutines, explained in the subsequent section. It starts at the very end of available memory, and dynamically grows backwards as more items are added (**pushed**) to it. The stack contains exclusively 64-bit (8 byte) values. Registers, literals, addresses, and pointers can all be given as operands to the push (`PSH`) instruction.

Once items have been pushed to the stack, they can be removed (**popped**), starting with the most recently pushed item. As with most other instructions with a destination, items from the stack must be popped into registers with the `POP` instruction. Once an item is removed from the stack, the effective size of the stack shrinks back down, and the popped item will no longer be considered part of the stack until and unless it is pushed again.

The `rso` register contains the address of the first byte of the top item in the stack. Its value will get **lower** as items are **pushed**, and **greater** as items are **popped**. More info on the `rso` register's behaviour can be found in the registers section.

Take this visual example, assuming memory is 8192 bytes in size (making 8191 the maximum address):

```text
; rso = 8192
; | Addresses |    8168..8175    |    8176..8183    |    8184..8191    ||
; |   Value   | ???????????????? | ???????????????? | ???????????????? ||

PSH 0xDEADBEEF  ; Push 0xDEADBEEF (3735928559) to the stack

; rso = 8184
; | Addresses |    8168..8175    |    8176..8183    ||    8184..8191    |
; |   Value   | ???????????????? | ???????????????? || 00000000EFBEADDE |

PSH 0xCAFEB0BA  ; Push 0xCAFEB0BA (3405689018) to the stack

; rso = 8176
; | Addresses |    8168..8175    ||    8176..8183    |    8184..8191    |
; |   Value   | ???????????????? || 00000000BAB0FECA | 00000000EFBEADDE |

PSH 0xD00D2BAD  ; Push 0xD00D2BAD (3490524077) to the stack

; rso = 8168
; | Addresses ||    8168..8175    |    8176..8183    |    8184..8191    |
; |   Value   || 00000000AD2B0DD0 | 00000000BAB0FECA | 00000000EFBEADDE |

POP rg0  ; Pop the most recent non-popped item from the stack into rg0

; rso = 8176
; | Addresses |    8168..8175    ||    8176..8183    |    8184..8191    |
; |   Value   | ???????????????? || 00000000BAB0FECA | 00000000EFBEADDE |
; rg0 = 0xD00D2BAD

POP rg0  ; Pop the most recent non-popped item from the stack into rg0

; rso = 8184
; | Addresses |    8168..8175    |    8176..8183    ||    8184..8191    |
; |   Value   | ???????????????? | ???????????????? || 00000000EFBEADDE |
; rg0 = 0xCAFEB0BA
```

### Using the Stack to Preserve Registers

A common use of the stack is to store the value of a register, use the register for a purpose that differs from its original one, then restore the register to the stored value. This is particularly useful in sections of reusable code (such as subroutines) where you cannot guarantee whether a register will be in use or not.

An example of this is as follows:

```text
MVQ rg0, 45
ADD rg0, 20
; rg0 is 65

PSH rg0  ; Push the current value of rg0 to the stack
MVQ rg0, 200
MUL rg0, 10
; rg0 is 2000

POP rg0  ; Pop the old rg0 back into rg0
; rg0 is back to 65
```

## Subroutines

A subroutine is a section of a program that can be specially jumped to (**called**) from multiple different points in a program. They differ from a standard jump in that the position in the program that a subroutine is called from is stored automatically, so can be **returned** to at any point with ease. This makes reusing the same section of code across different parts of a program, or even across different programs, much easier.

Subroutines are defined with a label as with any other form of jump destination — to call one, use the `CAL` instruction with either the label or a pointer to that label. Once you are within a subroutine, you can return to the calling location with the `RET` instruction, no operands required.

An example of a simple subroutine:

```text
MVQ rg0, 5
CAL :ADD_TO_RG0
; rg0 is now 15

MVQ rg1, :&ADD_TO_RG0
MVQ rg0, 46
CAL *rg1
; rg0 is now 56

HLT

:ADD_TO_RG0
ADD rg0, 10
RET
```

Specifically, `RET` will cause `rpo` to be updated to the address storing the opcode directly after the `CAL` instruction that was used to call the subroutine. Unless they are halting the program, subroutines should always exit with a `RET` instruction and nothing else.

### Fast Calling

The `CAL` instruction can also take an optional second operand: a value to pass to the subroutine. This is called **fast calling** or **fast passing**; the passed value gets stored in `rfp` and can be any one of a register, literal, address, or pointer. More info on the behaviour of the register itself and how it should be used can be found in its part of the registers section. Parameters are always 64-bit values, so when passing a memory location, 8 bytes of memory will always be read.

An example of subroutines utilising fast calling:

```text
:SUBROUTINE_ONE
ADD rfp, 1
MVQ rg0, rfp
RET

:SUBROUTINE_TWO
ADD rfp, 2
MVQ rg0, rfp
RET

CAL :SUBROUTINE_ONE, 4  ; This will implicitly set rfp to 4
; rg0 is now 5
CAL :SUBROUTINE_TWO, 6  ; This will implicitly set rfp to 6
; rg0 is now 8
```

### Return Values

The `RET` instruction can also take an optional operand to return a value. Return values can be registers, literals, or data in memory, and are stored in `rrv`. As with fast pass parameters, return values are always 64-bits/8 bytes. The exact behaviour and usage of the register can be found in its part of the registers section.

Here is the above example for fast calling adapted to use return values:

```text
:SUBROUTINE_ONE
ADD rfp, 1
RET rfp  ; Return, setting rrv to the value of rfp

:SUBROUTINE_TWO
ADD rfp, 2
RET rfp  ; Return, setting rrv to the value of rfp

CAL :SUBROUTINE_ONE, 4
; rrv is now 5
CAL :SUBROUTINE_TWO, 6
; rrv is now 8
```

### Subroutines and the Stack

In order to store the address to return to when using subroutines, the stack is utilised. Every time the `CAL` instruction is used, the address of the next opcode, and the current value of `rsb`, are pushed to the stack in that order. `rsb` and `rso` will then be updated to the new address of the top of the stack (the address where `rsb` was pushed to). `rsb` will continue to point here (the **base**) until another subroutine is called or the subroutine is returned from. `rso` will continue to update as normal as items are popped to and pushed from the stack, always pointing to the top of it. The area from the current **base** (`rsb`) to the top of the stack (`rso`) is called the current **stack frame**. Multiple stack frames can be stacked on top of each other if a subroutine is called from another subroutine.

When returning from a subroutine, the opposite is performed. `rsb` and `rpo` are popped off the top of the stack, thereby continuing execution as it was before the subroutine was called. All values apart from these two must be popped off the stack before using the `RET` instruction (you can ensure this by moving the value of `rsb` into `rso`). After returning, `rso` will point to the same address as when the function was called.

If you utilise registers in a subroutine, you should use the stack to ensure that the value of each modified register is returned to its initial value before returning from the subroutine. See the above section on using the stack to preserve registers for info on how to do this.

### Passing Multiple Parameters

The `CAL` instruction can only take a single data parameter, however, there may be situations where multiple values need to be passed to a subroutine; it is best to use the stack in situations such as these. Before calling the subroutine, push any values you want to act as parameters to the subroutine, to the stack. Once the subroutine has been called, you can use `rsb` to calculate the address that each parameter will be stored at. To access the first parameter (the last one pushed before calling), you need to account for the two automatically pushed values first. These, along with every other value in the stack, are both 8 bytes long, so adding `16` (`8 * 2`) to `rsb` will get you the address of this parameter (you should do this in another register, `rsb` should be left unmodified). To access any subsequent parameters, simply add another `8` on top of this.

For example:

```text
PSH 4  ; Parameter D
PSH 3  ; Parameter C
PSH 2  ; Parameter B
CAL :SUBROUTINE, 1  ; Parameter A (rfp)
; rrv is now 10

:SUBROUTINE
PSH rg0  ; Preserve the value of rg0

MVQ rg0, rsb
ADD rg0, 16  ; Parameter B
ADD rfp, *rg0
; rfp is now 3
ADD rg0, 8  ; Parameter C
ADD rfp, *rg0
; rfp is now 6
ADD rg0, 8  ; Parameter D
ADD rfp, *rg0
; rfp is now 10

POP rg0  ; Restore rg0 to its original value
RET rfp
```

## Allocating Memory Regions

AssEmbly has support for dynamically allocating regions of memory with a given size. This is optional, as memory does not have to be allocated for you to be able to read and write to it. Utilising dynamic memory allocation, however, can help you ensure that you have enough unused memory for the operation you wish to perform, and that you have a region of memory separated from any other. It also allows you to have many different memory regions without having to calculate the start addresses and region placement yourself. Instructions related to memory allocation are all found in the Memory Allocation Extension Set, and have mnemonics prefixed with `HEAP_`.

Memory regions can be allocated, re-allocated, and freed. All allocated regions should be freed once you are finished working with them to prevent memory leaks, which can lead to a situation where you may run out of memory by continually allocating memory without freeing it, as memory regions **cannot** overlap.

The regions of memory occupied by the loaded program bytes and the stack are also considered allocated regions, and will never be overlapped by user allocated regions. The size of the stack region will dynamically update as the stack is pushed to and popped from.

### Allocating a New Region

There are two instructions that can be used to allocate a new region of memory: `HEAP_ALC` and `HEAP_TRY`. Both allocate an exact number of bytes given in the second operand as either a value in a register, a literal value, or a value in memory. After allocating, the instructions store the memory address of the first byte in the newly allocated block in a register given as the first operand. If an error occurs while allocating memory (for example if there is not enough free memory remaining), `HEAP_ALC` will throw an error, stopping execution of the program immediately, whereas `HEAP_TRY` will set the value of the destination register to `-1` (`0xFFFFFFFFFFFFFFFF`) and execution will continue.

For example, assuming memory is 8192 bytes in size:

```text
HEAP_TRY rg0, 20
; rg0 now stores the memory address to the first byte in a 20 byte long region

MVQ rg1, 10_000  ; The value of a register can also be used as the amount of bytes to allocate
HEAP_TRY rg2, rg1
; rg2 is now -1, as the allocation failed. No further memory has been allocated

HEAP_ALC rg3, 10_000
; An error is thrown, execution stops
```

Allocated blocks of memory are always **contiguous**, meaning each byte of a region will follow one after the other - a region will never be split into multiple parts. The first address of a memory region is the only address that can be used to identify it with re-allocation/free instructions, so it is important you keep track of it until the region has been freed.

Allocated regions also do **not** automatically have their contents set to `0` or any other value. The contents of memory in the region will remain unchanged from before it was allocated.

### Re-allocating an Existing Region

You can change the size of a memory region after it has been allocated by *re-allocating* it - there are specific re-allocation instructions to do this. They take a register as the first operand, which is used both as the source for the starting address of the memory region to re-allocate, as well as the destination to store the starting address of the re-allocated region. The second operand is the number of bytes to use as the new size for the region, the same as with the allocation instructions.

Regions can either be expanded or shrunk. As with allocation, neither will modify any values in memory. When a region shrinks, or when a region is expanded and has enough free contiguous memory following it to do so without being moved, the starting address of the region will remain unchanged. If there is not enough free contiguous memory following a region to expand it without moving it, then the starting address of the region will change, and all of the bytes in the old region will be copied to the new region. Bytes beyond the length of the old region but still within the new region will remain unchanged. The new region may overlap the old region. If the start address of a region does change after re-allocation, the old start address will no longer be a valid pointer corresponding to the region. You do not need to free the old address, only the newly allocated region needs freeing.

Similarly to allocation, there are two instructions for performing a re-allocation: `HEAP_REA` and `HEAP_TRE`. `HEAP_REA` will throw an error if the re-allocation fails, stopping execution, whereas `HEAP_TRE` will set the value of the destination register to either `-1` (`0xFFFFFFFFFFFFFFFF`) or `-2` (`0xFFFFFFFFFFFFFFFE`) if the re-allocation fails. `-1` means that there was not enough free memory to perform the re-allocation, `-2` means that the address in the first operand did not correspond to the start of an already mapped memory region. If a re-allocation does fail after using the `HEAP_TRE` instruction, the old region **will still be allocated** with its original size. The register holding the address will have been overwritten with the error code, however, so it is important to have the original address stored elsewhere as a backup when using the `HEAP_TRE` instruction.

### Freeing an Allocated Region

Once you have finished working with a region of memory, you must explicitly free it. Failing to do so will result in the region remaining allocated, leaving its bytes unavailable for any future allocations. This is called a **memory leak** (or **leaking memory**), and if it is done repeatedly, you may end up in a situation where you completely run out of available memory and are unable to make any more allocations.

To free a region, give the starting address of the region to free in a register as the first and only operand to the `HEAP_FRE` instruction. The region will be immediately freed for use in future allocations and the first address of the region will no longer be considered a valid region pointer. Freeing a region does not in and of itself affect the contents of memory in said region, however it does erase the guarantee that no other regions will be present there, so you should not rely on memory values staying the same at any point after a region has been freed.

Attempting to free a region with an invalid region pointer will result in an error, stopping execution. There is no instruction to "try" freeing a pointer like there is with re-allocation. You cannot free the memory regions used by the loaded program or the stack.

### Memory Fragmentation

As a consequence of memory regions being contiguous, the maximum number of bytes you can allocate at once may be less than the total number of unallocated bytes in memory.

Consider the following situation, assuming we're starting with 32 bytes of free memory:

```text
HEAP_ALC rg0, 4  ; Region "A"
HEAP_ALC rg1, 4  ; Region "B"
HEAP_ALC rg2, 4  ; Region "C"
HEAP_ALC rg3, 4  ; Region "D"
```

> Our mapped memory currently looks like this (`.` corresponds to free memory):

```text
AAAABBBBCCCCDDDD................
```

> Now what if we free Region B?

```text
HEAP_FRE rg1
```

> Our memory now looks like this:

```text
AAAA....CCCCDDDD................
```

> Freeing a region does not cause the other regions to move, so even though we now have 20 free bytes in memory, we cannot allocate any more than 16 into a single region, as it would require the region to be split across multiple ranges, which is not valid. This ultimately means that the most memory you can allocate in a single region is the number of bytes in the **largest contiguous region of unallocated memory**. Attempting to allocate 17+ bytes in this situation would produce the same result as attempting to allocate without enough free total memory.

## Interoperating with C# Code

It is possible to execute external code from .NET assembly files in AssEmbly. These external methods have the ability to both read from and write to the AssEmbly processor's memory and registers. An optional value can also be passed to the external method upon calling to prevent needing to go through registers or memory for a single parameter.

### Writing and Compiling a Compatible C# Program

In order for AssEmbly to detect an external method within a .NET DLL, it must be located immediately within a class named `AssEmblyInterop` that is not located within any defined namespace (i.e. it is in the `global` namespace alias). The method itself *must* be `public` and `static`, and *must* have three parameters with the following types **in order**: `byte[]`, `ulong[]`, and `ulong?`. These correspond to memory, registers, and the passed value respectively, though the parameters' names, along with the name of the method itself, can be anything you wish. The method's return type should be `void`, as any returned value will be ignored by AssEmbly. The passed value parameter will be `null` if no value is given from AssEmbly.

An example C# program may look like this:

```csharp
using System;

// Note the class is not in a namespace
public static class AssEmblyInterop
{
    public static void YourMethod(byte[] memory, ulong[] registers, ulong? passedValue)
    {
        // Methods can read memory...
        byte value = memory[0xFF];
        // Or write to it...
        memory[0xFF] = 12;

        // They can read registers...
        ulong rg0 = registers[6];  // 6 = rg0, see the registers table for the byte values of each register
        // Or write to them...
        registers[6] = 9000;

        // Or do anything else that a normal C# program can do
        if (passedValue == null)
        {
            Console.WriteLine("You need to pass a value!");
            return;
        }

        Console.WriteLine($"Your value: {passedValue}");
    }

    public static void YourOtherMethod(byte[] memory, ulong[] registers, ulong? passedValue)
    {
        // Your code here...
    }
}
```

In order to compile a single C# source file into a .NET DLL, you can use the `csc` tool included with Visual Studio. The command `csc /t:library <file_name>.cs` will compile the given C# script into a .NET Framework assembly with the name `<file_name>.dll`. While AssEmbly is capable of loading .NET Core DLLs, .NET Framework is recommended to prevent potential dependency issues. More complex C# projects using a `.csproj` file can also be used, as long as there is a resulting assembly with the `AssEmblyInterop` class in its global namespace.

### Accessing Methods from an AssEmbly Program

For AssEmbly to load a DLL and methods within it, their names need to be defined as null-terminated strings in memory, similarly to when performing file operations. DLL paths can either be relative (i.e. `MyAsm.dll` or `Folder/MyAsm.dll`), or absolute (i.e. `Drive:/Folder/Folder/MyAsm.dll`). Method names should not include the `AssEmblyInterop` class name. Only a single assembly can be loaded at once, and only a single method from that assembly can be loaded at a time.

To load an assembly, use the `ASMX_LDA` instruction with the memory address of the null-terminated file path. After loading an assembly, you can load a function from it with `ASMX_LDF`, with the memory address of the null-terminated method name. Once an assembly is loaded, you can load and unload methods from it as many times as you like. The current function must be closed with `ASMX_CLF` before you can load another, and the current assembly must be closed with `ASMX_CLA` before another can be opened. `ASMX_CLA` will automatically close the open function as well if one is still loaded when it is used.

Once both an assembly and function are loaded, you can use the `ASMX_CAL` instruction with an optional operand to use as the passed value in order to call it.

Here is an example program that utilises a method from the C# example above:

```text
ASMX_LDA :DLL_PATH  ; Load the assembly
ASMX_LDF :FUNC_PATH  ; Load the function from the assembly

ASMX_CAL  ; Call the loaded function with null as the passed value
ASMX_CAL 20  ; Call the loaded function with the literal value of 20 as the passed value
ASMX_CAL rg0  ; Call the loaded function with the value in rg0 as the passed value

ASMX_CLF  ; Close the function
ASMX_CLA  ; Close the assembly

HLT  ; Halt the processor before it reaches data

:DLL_PATH
%DAT "MyAsm.dll\0"

:FUNC_PATH
%DAT "YourMethod\0"
```

> Executing this program results in the following console output:

```text
You need to pass a value!
Your value: 20
Your value: 9000
```

> `9000` is printed on the final line as `rg0` was set to `9000` in both of the prior external function calls.

### Testing if an Assembly or Function Exists

The `ASMX_LDA` and `ASMX_LDF` instructions will throw an error, stopping execution, if the path/name they are given does not correspond to a valid target to load. If you wish to test whether or not this will happen without crashing the program, you can use the `ASMX_AEX` and `ASMX_FEX` instructions. They both take a register as their first operand, then the memory address of the null-terminated target string as their second. If the target assembly/function exists and is valid, the value of the first operand register will be set to `1`, otherwise it will be set to `0`. An assembly must already be loaded in order to check the validity of a function, as only the currently open assembly will be searched.

## Text Encoding

All text in AssEmbly (input from/output to the console; strings inserted by `%DAT`; strings given to `OFL`, `DFL`, `FEX`, etc.) is encoded in UTF-8. This means that all characters that are a part of the ASCII character set only take up a single byte, though some characters may take as many as 4 bytes to store fully.

Be aware that when working with characters that require multiple bytes, instructions like `RCC`, `RFC`, `WCC`, and `WFC` still only work on single bytes at a time. As long as you read/write all of the UTF-8 bytes in the correct order, they should be stored and displayed correctly.

Text bytes read from files **will not** be automatically converted to UTF-8 if the file was saved with another encoding.

## Escape Sequences

There are some sequences of characters that have special meanings when found inside a string or character literal. Each of these begins with a backslash (`\`) character and are used to insert characters that couldn't be included normally. Every supported sequence is as follows:

| Escape sequence | Character name             | Notes                                                                                                                                                                 |
|-----------------|----------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `\"`            | Double quote               | Used to insert a double quote into a string without causing the string to end. Not required in single character literals.                                             |
| `\'`            | Single quote               | Used to insert a single quote into a single character literal without causing the literal to end. Not required in string literals.                                    |
| `\\`            | Backslash                  | For a string to contain a backslash, you must escape it so it isn't treated as the start of an escape sequence.                                                       |
| `\@`            | At sign                    | For a string to contain an at sign, you must escape it so it isn't treated as an assembler variable/constant.                                                         |
| `\0`            | Null                       | ASCII 0x00. Should be used to terminate every string.                                                                                                                 |
| `\a`            | Alert                      | ASCII 0x07.                                                                                                                                                           |
| `\b`            | Backspace                  | ASCII 0x08.                                                                                                                                                           |
| `\f`            | Form feed                  | ASCII 0x0C.                                                                                                                                                           |
| `\n`            | Newline                    | ASCII 0x0A. Will cause the string to move onto a new console/file line when printed. Should be preceded by `\r` on Windows.                                           |
| `\r`            | Carriage return            | ASCII 0x0D.                                                                                                                                                           |
| `\t`            | Horizontal tab             | ASCII 0x09.                                                                                                                                                           |
| `\v`            | Vertical tab               | ASCII 0x0B.                                                                                                                                                           |
| `\u....`        | Unicode codepoint (16-bit) | Inserts the unicode character with a codepoint represented by 4 hexadecimal digits in the range `0x0000` to `0xFFFF`.                                                 |
| `\U........`    | Unicode codepoint (32-bit) | Inserts the unicode character with a codepoint represented by 8 hexadecimal digits in the range `0x00000000` to `0x0010FFFF`, excluding `0x0000d800` to `0x0000dfff`. |

## Instruction Data Type Acceptance

The following is a table of which types of numeric data can be given to each instruction and have them function as expected. AssEmbly **does not** keep track of data types, it is your responsibility to do so. If you use the wrong instruction for the type of data you have, it is unlikely you will receive an error — you will most likely simply get an unexpected answer, as the processor is interpreting the data as a valid, but different, numeric value in a different format.

If an instruction supports signed integers but not unsigned integers, the instruction *will* still accept positive values, but those positive values must be below the signed limit (`9,223,372,036,854,775,807`), or they will be erroneously interpreted as negative.

- `O` = Instruction accepts the data type
- `X` = Instruction does not accept the data type
- `(...)` = Instruction accepts the data type, but see the numbered footnote below the table for additional information to keep in mind

Instructions that don't take any data or are otherwise not applicable have been omitted.

| Instruction   | Unsigned Integer | Signed Integer | Floating Point |
|---------------|------------------|----------------|----------------|
| `ADD`         | O                | O              | X              |
| `ICR`         | O                | O              | X              |
| `SUB`         | O                | O              | X              |
| `DCR`         | O                | O              | X              |
| `MUL`         | O                | O              | X              |
| `DIV`         | O                | X              | X              |
| `DVR`         | O                | X              | X              |
| `REM`         | O                | X              | X              |
| `SHL`         | O                | (5)            | X              |
| `SHR`         | O                | (1) (5)        | X              |
| `AND`         | O                | (2)            | X              |
| `ORR`         | O                | (2)            | X              |
| `XOR`         | O                | (2)            | X              |
| `NOT`         | O                | (2)            | X              |
| `TST`         | O                | (2)            | X              |
| `CMP`         | O                | X              | X              |
| `MVB`         | O                | (3)            | X              |
| `MVW`         | O                | (3)            | X              |
| `MVD`         | O                | (3)            | X              |
| `MVQ`         | O                | O              | O              |
| `PSH`         | O                | O              | O              |
| `CAL`         | O                | O              | O              |
| `RET`         | O                | O              | O              |
| `WCN`         | O                | X              | X              |
| `WCB`         | O                | X              | X              |
| `WCX`         | O                | X              | X              |
| `WCC`         | O                | X              | X              |
| `WFN`         | O                | X              | X              |
| `WFB`         | O                | X              | X              |
| `WFX`         | O                | X              | X              |
| `WFC`         | O                | X              | X              |
| `SIGN_DIV`    | X                | O              | X              |
| `SIGN_DVR`    | X                | O              | X              |
| `SIGN_REM`    | X                | O              | X              |
| `SIGN_SHR`    | X                | (5)            | X              |
| `SIGN_MVB`    | X                | O              | X              |
| `SIGN_MVW`    | X                | O              | X              |
| `SIGN_MVD`    | X                | O              | X              |
| `SIGN_WCN`    | X                | O              | X              |
| `SIGN_WCB`    | X                | O              | X              |
| `SIGN_WFN`    | X                | O              | X              |
| `SIGN_WFB`    | X                | O              | X              |
| `SIGN_EXB`    | X                | O              | X              |
| `SIGN_EXW`    | X                | O              | X              |
| `SIGN_EXD`    | X                | O              | X              |
| `SIGN_NEG`    | X                | O              | X              |
| `FLPT_ADD`    | X                | X              | O              |
| `FLPT_SUB`    | X                | X              | O              |
| `FLPT_MUL`    | X                | X              | O              |
| `FLPT_DIV`    | X                | X              | O              |
| `FLPT_DVR`    | X                | X              | O              |
| `FLPT_REM`    | X                | X              | O              |
| `FLPT_SIN`    | X                | X              | O              |
| `FLPT_ASN`    | X                | X              | O              |
| `FLPT_COS`    | X                | X              | O              |
| `FLPT_ACS`    | X                | X              | O              |
| `FLPT_TAN`    | X                | X              | O              |
| `FLPT_ATN`    | X                | X              | O              |
| `FLPT_PTN`    | X                | X              | O              |
| `FLPT_POW`    | X                | X              | O              |
| `FLPT_LOG`    | X                | X              | O              |
| `FLPT_WCN`    | X                | X              | O              |
| `FLPT_WFN`    | X                | X              | O              |
| `FLPT_EXH`    | X                | X              | O              |
| `FLPT_EXS`    | X                | X              | O              |
| `FLPT_SHS`    | X                | X              | O              |
| `FLPT_SHH`    | X                | X              | O              |
| `FLPT_NEG`    | X                | X              | O              |
| `FLPT_UTF`    | O                | X              | X              |
| `FLPT_STF`    | X                | O              | X              |
| `FLPT_FTS`    | X                | X              | O              |
| `FLPT_FCS`    | X                | X              | O              |
| `FLPT_FFS`    | X                | X              | O              |
| `FLPT_FNS`    | X                | X              | O              |
| `FLPT_CMP`    | X                | X              | O              |
| `EXTD_BSW`    | (4)              | (4)            | (4)            |
| `HEAP_ALC`    | O                | X              | X              |
| `HEAP_TRY`    | O                | X              | X              |
| `HEAP_REA`    | O                | X              | X              |
| `HEAP_TRE`    | O                | X              | X              |
| `FSYS_SCT`    | X                | O              | X              |
| `FSYS_SMT`    | X                | O              | X              |
| `FSYS_SAT`    | X                | O              | X              |
| `TERM_SCY`    | O                | X              | X              |
| `TERM_SCX`    | O                | X              | X              |
| `TERM_SFC`    | O                | X              | X              |
| `TERM_SBC`    | O                | X              | X              |

1. Signed integers *can* still be used with `SHR`, though it will perform a logical shift, not an arithmetic one, which may or may not be what you desire. See the section on Arithmetic Right Shifting for the difference.
2. Bitwise operations on signed integers will treat the sign bit like any other, there is no special logic involving it.
3. Using smaller-than-64-bit move instructions on signed integers if the target is a memory location will work as expected, truncating the upper bits. If the target is a register, however, you may wish to use the signed versions to automatically extend the smaller integer to a signed 64-bit one so it is correctly interpreted by other instructions.
4. Reversing the byte order of a register can work on any data type, however, registers **must** be in little endian order *after* reversing to have their value correctly interpreted by other instructions (this does not apply to instructions where the format of the register's value is unimportant, such as with `MVQ`).
5. Only the first operand to shift instructions (the value to shift) can be signed. The second operand (the number of bits to shift by) must be unsigned. Negative values will have the same effect as using a value of `64` or higher, simply setting the destination register to `0`.

## Status Flag Behaviour

- `0` = Instruction always unsets flag
- `1` = Instruction always sets flag
- `(...)` = Instruction sets flag if the given condition is satisfied, otherwise it unsets it
- `[...]` = Instruction sets flag if the given condition is satisfied, otherwise it maintains its current value
- `{...}` = Instruction unsets flag if the given condition is satisfied, otherwise it maintains its current value
- `X` = Instruction does not affect flag
- `STD` = Instruction uses standard behaviour for flag according to result, unaffected by operands. They are as follows:
  - For zero flag, set if the result is equal to 0, otherwise unset (for floating point operations, `-0` is considered equal to `0` and will set the zero flag)
  - For sign flag, set if the most significant bit of the result is set, otherwise unset

| Instruction   | Zero                                            | Carry                                                    | File End                       | Sign | Overflow                              | Auto Echo |
|---------------|-------------------------------------------------|----------------------------------------------------------|--------------------------------|------|---------------------------------------|-----------|
| `HLT`         | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `NOP`         | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `JMP`         | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `JEQ` / `JZO` | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `JNE` / `JNZ` | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `JLT` / `JCA` | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `JLE`         | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `JGT`         | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `JGE` / `JNC` | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `ADD`         | STD                                             | (Result is unrepresentable as unsigned)                  | X                              | STD  | (Result is unrepresentable as signed) | X         |
| `ICR`         | STD                                             | (Result is unrepresentable as unsigned)                  | X                              | STD  | (Result is unrepresentable as signed) | X         |
| `SUB`         | STD                                             | (Result is unrepresentable as unsigned)                  | X                              | STD  | (Result is unrepresentable as signed) | X         |
| `DCR`         | STD                                             | (Result is unrepresentable as unsigned)                  | X                              | STD  | (Result is unrepresentable as signed) | X         |
| `MUL`         | STD                                             | (Result is unrepresentable as both unsigned and signed)  | X                              | STD  | 0                                     | X         |
| `DIV`         | STD                                             | 0                                                        | X                              | STD  | 0                                     | X         |
| `DVR`         | STD                                             | 0                                                        | X                              | STD  | 0                                     | X         |
| `REM`         | STD                                             | 0                                                        | X                              | STD  | 0                                     | X         |
| `SHL`         | STD                                             | (Any `1` bit was shifted past MSB)                       | X                              | STD  | 0                                     | X         |
| `SHR`         | STD                                             | (Any `1` bit was shifted past LSB)                       | X                              | STD  | 0                                     | X         |
| `AND`         | STD                                             | 0                                                        | X                              | STD  | 0                                     | X         |
| `ORR`         | STD                                             | 0                                                        | X                              | STD  | 0                                     | X         |
| `XOR`         | STD                                             | 0                                                        | X                              | STD  | 0                                     | X         |
| `NOT`         | STD                                             | 0                                                        | X                              | STD  | 0                                     | X         |
| `RNG`         | STD                                             | 0                                                        | X                              | STD  | 0                                     | X         |
| `TST`         | STD                                             | X                                                        | X                              | STD  | X                                     | X         |
| `CMP`         | STD                                             | (Result is unrepresentable as unsigned)                  | X                              | STD  | (Result is unrepresentable as signed) | X         |
| `MVB`         | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `MVW`         | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `MVD`         | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `MVQ`         | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `PSH`         | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `POP`         | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `CAL`         | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `RET`         | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `WCN`         | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `WCB`         | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `WCX`         | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `WCC`         | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `WFN`         | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `WFB`         | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `WFX`         | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `WFC`         | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `OFL`         | X                                               | X                                                        | (File is empty)                | X    | X                                     | X         |
| `CFL`         | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `DFL`         | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `FEX`         | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `FSZ`         | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `RCC`         | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `RFC`         | X                                               | X                                                        | [No more unread bytes in file] | X    | X                                     | X         |
| `SIGN_JLT`    | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `SIGN_JLE`    | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `SIGN_JGT`    | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `SIGN_JGE`    | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `SIGN_JSI`    | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `SIGN_JNS`    | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `SIGN_JOV`    | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `SIGN_JNO`    | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `SIGN_DIV`    | STD                                             | 0                                                        | X                              | STD  | 0                                     | X         |
| `SIGN_DVR`    | STD                                             | 0                                                        | X                              | STD  | 0                                     | X         |
| `SIGN_REM`    | STD                                             | 0                                                        | X                              | STD  | 0                                     | X         |
| `SIGN_SHR`    | STD                                             | (Any bit not equal to the sign bit was shifted past LSB) | X                              | STD  | 0                                     | X         |
| `SIGN_MVB`    | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `SIGN_MVW`    | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `SIGN_MVD`    | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `SIGN_WCN`    | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `SIGN_WCB`    | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `SIGN_WFN`    | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `SIGN_WFB`    | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `SIGN_EXB`    | STD                                             | 0                                                        | X                              | STD  | 0                                     | X         |
| `SIGN_EXW`    | STD                                             | 0                                                        | X                              | STD  | 0                                     | X         |
| `SIGN_EXD`    | STD                                             | 0                                                        | X                              | STD  | 0                                     | X         |
| `SIGN_NEG`    | STD                                             | 0                                                        | X                              | STD  | 0                                     | X         |
| `FLPT_ADD`    | STD                                             | (Result is less than the initial value)                  | X                              | STD  | 0                                     | X         |
| `FLPT_SUB`    | STD                                             | (Result is greater than the initial value)               | X                              | STD  | 0                                     | X         |
| `FLPT_MUL`    | STD                                             | (Result is less than the initial value)                  | X                              | STD  | 0                                     | X         |
| `FLPT_DIV`    | STD                                             | 0                                                        | X                              | STD  | 0                                     | X         |
| `FLPT_DVR`    | STD                                             | 0                                                        | X                              | STD  | 0                                     | X         |
| `FLPT_REM`    | STD                                             | 0                                                        | X                              | STD  | 0                                     | X         |
| `FLPT_SIN`    | STD                                             | 0                                                        | X                              | STD  | 0                                     | X         |
| `FLPT_ASN`    | STD                                             | 0                                                        | X                              | STD  | 0                                     | X         |
| `FLPT_COS`    | STD                                             | 0                                                        | X                              | STD  | 0                                     | X         |
| `FLPT_ACS`    | STD                                             | 0                                                        | X                              | STD  | 0                                     | X         |
| `FLPT_TAN`    | STD                                             | 0                                                        | X                              | STD  | 0                                     | X         |
| `FLPT_ATN`    | STD                                             | 0                                                        | X                              | STD  | 0                                     | X         |
| `FLPT_PTN`    | STD                                             | 0                                                        | X                              | STD  | 0                                     | X         |
| `FLPT_POW`    | STD                                             | (Result is less than the initial value)                  | X                              | STD  | 0                                     | X         |
| `FLPT_LOG`    | STD                                             | (Result is greater than the initial value)               | X                              | STD  | 0                                     | X         |
| `FLPT_WCN`    | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `FLPT_WFN`    | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `FLPT_EXH`    | STD                                             | 0                                                        | X                              | STD  | 0                                     | X         |
| `FLPT_EXS`    | STD                                             | 0                                                        | X                              | STD  | 0                                     | X         |
| `FLPT_SHS`    | STD                                             | 0                                                        | X                              | STD  | 0                                     | X         |
| `FLPT_SHH`    | STD                                             | 0                                                        | X                              | STD  | 0                                     | X         |
| `FLPT_NEG`    | STD                                             | 0                                                        | X                              | STD  | 0                                     | X         |
| `FLPT_UTF`    | STD                                             | 0                                                        | X                              | STD  | 0                                     | X         |
| `FLPT_STF`    | STD                                             | 0                                                        | X                              | STD  | 0                                     | X         |
| `FLPT_FTS`    | STD                                             | 0                                                        | X                              | STD  | 0                                     | X         |
| `FLPT_FCS`    | STD                                             | 0                                                        | X                              | STD  | 0                                     | X         |
| `FLPT_FFS`    | STD                                             | 0                                                        | X                              | STD  | 0                                     | X         |
| `FLPT_FNS`    | STD                                             | 0                                                        | X                              | STD  | 0                                     | X         |
| `FLPT_CMP`    | STD                                             | (Value of first operand is less than second)             | X                              | STD  | 0                                     | X         |
| `EXTD_BSW`    | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `EXTD_QPF`    | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `EXTD_QPV`    | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `EXTD_CSS`    | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `EXTD_HLT`    | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `EXTD_MPA`    | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `ASMX_LDA`    | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `ASMX_LDF`    | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `ASMX_CLA`    | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `ASMX_CLF`    | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `ASMX_AEX`    | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `ASMX_FEX`    | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `ASMX_CAL`    | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `HEAP_ALC`    | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `HEAP_TRY`    | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `HEAP_REA`    | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `HEAP_TRE`    | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `HEAP_FRE`    | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `FSYS_CWD`    | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `FSYS_GWD`    | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `FSYS_CDR`    | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `FSYS_DDR`    | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `FSYS_DDE`    | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `FSYS_DEX`    | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `FSYS_CPY`    | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `FSYS_MOV`    | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `FSYS_BDL`    | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `FSYS_GNF`    | (There were no more files in the listing)       | X                                                        | X                              | X    | X                                     | X         |
| `FSYS_GND`    | (There were no more directories in the listing) | X                                                        | X                              | X    | X                                     | X         |
| `FSYS_GCT`    | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `FSYS_GMT`    | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `FSYS_GAT`    | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `FSYS_SCT`    | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `FSYS_SMT`    | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `FSYS_SAT`    | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `TERM_CLS`    | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `TERM_AEE`    | X                                               | X                                                        | X                              | X    | X                                     | 1         |
| `TERM_AED`    | X                                               | X                                                        | X                              | X    | X                                     | 0         |
| `TERM_SCY`    | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `TERM_SCX`    | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `TERM_GCY`    | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `TERM_GCX`    | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `TERM_GSY`    | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `TERM_GSX`    | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `TERM_BEP`    | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `TERM_SFC`    | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `TERM_SBC`    | X                                               | X                                                        | X                              | X    | X                                     | X         |
| `TERM_RSC`    | X                                               | X                                                        | X                              | X    | X                                     | X         |

## Full Instruction Reference

### Base Instruction Set

Extension set number `0x00`, opcodes start with `0xFF, 0x00`. Contains the core features of the architecture, remaining mostly unchanged by updates.

Note that for the base instruction set (number `0x00`) *only*, the leading `0xFF, 0x00` to specify the extension set can be omitted, as the processor will automatically treat opcodes not starting with `0xFF` as base instruction set opcodes.

| Mnemonic | Full Name | Operands | Function | Instruction Code | Minimum Major Version |
|----------|-----------|----------|----------|------------------|-----------------------|
| **Control** ||||||
| `HLT` | Halt | - | Stops the processor from executing the program | `0x00` | `pre-v1` |
| `NOP` | No Operation | - | Do nothing | `0x01` | `pre-v1` |
| **Jumping** ||||||
| `JMP` | Jump | Address | Jump unconditionally to an address | `0x02` | `pre-v1` |
| `JMP` | Jump | Pointer | Jump unconditionally to an address in a register | `0x03` | `pre-v1` |
| `JEQ` / `JZO` | Jump if Equal / Jump if Zero | Address | Jump to an address only if the zero status flag is set | `0x04` | `pre-v1` |
| `JEQ` / `JZO` | Jump if Equal / Jump if Zero | Pointer | Jump to an address in a register only if the zero status flag is set | `0x05` | `pre-v1` |
| `JNE` / `JNZ` | Jump if not Equal / Jump if not Zero | Address | Jump to an address only if the zero status flag is unset | `0x06` | `pre-v1` |
| `JNE` / `JNZ` | Jump if not Equal / Jump if not Zero | Pointer | Jump to an address in a register only if the zero status flag is unset | `0x07` | `pre-v1` |
| `JLT` / `JCA` | Jump if Less Than / Jump if Carry | Address | Jump to an address only if the carry status flag is set | `0x08` | `pre-v1` |
| `JLT` / `JCA` | Jump if Less Than / Jump if Carry | Pointer | Jump to an address in a register only if the carry status flag is set | `0x09` | `pre-v1` |
| `JLE` | Jump if Less Than or Equal To | Address | Jump to an address only if either the carry or zero flags are set | `0x0A` | `pre-v1` |
| `JLE` | Jump if Less Than or Equal To | Pointer | Jump to an address in a register only if either the carry or zero flags are set | `0x0B` | `pre-v1` |
| `JGT` | Jump if Greater Than | Address | Jump to an address only if both the carry and zero flags are unset | `0x0C` | `pre-v1` |
| `JGT` | Jump if Greater Than | Pointer | Jump to an address in a register only if both the carry and zero flags are unset | `0x0D` | `pre-v1` |
| `JGE` / `JNC` | Jump if Greater Than or Equal To / Jump if no Carry | Address | Jump to an address only if the carry status flag is unset | `0x0E` | `pre-v1` |
| `JGE` / `JNC` | Jump if Greater Than or Equal To / Jump if no Carry | Pointer | Jump to an address in a register only if the carry status flag is unset | `0x0F` | `pre-v1` |
| **Math** ||||||
| `ADD` | Add | Register, Register | Add the contents of one register to another | `0x10` | `pre-v1` |
| `ADD` | Add | Register, Literal | Add a literal value to the contents of a register | `0x11` | `pre-v1` |
| `ADD` | Add | Register, Address | Add the contents of memory at an address to a register | `0x12` | `pre-v1` |
| `ADD` | Add | Register, Pointer | Add the contents of memory at an address in a register to a register | `0x13` | `pre-v1` |
| `ICR` | Increment | Register | Increment the contents of a register by 1 | `0x14` | `pre-v1` |
| `SUB` | Subtract | Register, Register | Subtract the contents of one register from another | `0x20` | `pre-v1` |
| `SUB` | Subtract | Register, Literal | Subtract a literal value from the contents of a register | `0x21` | `pre-v1` |
| `SUB` | Subtract | Register, Address | Subtract the contents of memory at an address from a register | `0x22` | `pre-v1` |
| `SUB` | Subtract | Register, Pointer | Subtract the contents of memory at an address in a register from a register | `0x23` | `pre-v1` |
| `DCR` | Decrement | Register | Decrement the contents of a register by 1 | `0x24` | `pre-v1` |
| `MUL` | Multiply | Register, Register | Multiply the contents of one register by another | `0x30` | `pre-v1` |
| `MUL` | Multiply | Register, Literal | Multiply the contents of a register by a literal value | `0x31` | `pre-v1` |
| `MUL` | Multiply | Register, Address | Multiply a register by the contents of memory at an address | `0x32` | `pre-v1` |
| `MUL` | Multiply | Register, Pointer | Multiply a register by the contents of memory at an address in a register | `0x33` | `pre-v1` |
| `DIV` | Integer Divide | Register, Register | Divide the contents of one register by another, discarding the remainder | `0x40` | `pre-v1` |
| `DIV` | Integer Divide | Register, Literal | Divide the contents of a register by a literal value, discarding the remainder | `0x41` | `pre-v1` |
| `DIV` | Integer Divide | Register, Address | Divide a register by the contents of memory at an address, discarding the remainder | `0x42` | `pre-v1` |
| `DIV` | Integer Divide | Register, Pointer | Divide a register by the contents of memory at an address in a register, discarding the remainder | `0x43` | `pre-v1` |
| `DVR` | Divide With Remainder | Register, Register, Register | Divide the contents of one register by another, storing the remainder | `0x44` | `pre-v1` |
| `DVR` | Divide With Remainder | Register, Register, Literal | Divide the contents of a register by a literal value, storing the remainder | `0x45` | `pre-v1` |
| `DVR` | Divide With Remainder | Register, Register, Address | Divide a register by the contents of memory at an address, storing the remainder | `0x46` | `pre-v1` |
| `DVR` | Divide With Remainder | Register, Register, Pointer | Divide a register by the contents of memory at an address in a register, storing the remainder | `0x47` | `pre-v1` |
| `REM` | Remainder Only | Register, Register | Divide the contents of one register by another, storing only the remainder | `0x48` | `pre-v1` |
| `REM` | Remainder Only | Register, Literal | Divide the contents of a register by a literal value, storing only the remainder | `0x49` | `pre-v1` |
| `REM` | Remainder Only | Register, Address | Divide a register by the contents of memory at an address, storing only the remainder | `0x4A` | `pre-v1` |
| `REM` | Remainder Only | Register, Pointer | Divide a register by the contents of memory at an address in a register, storing only the remainder | `0x4B` | `pre-v1` |
| `SHL` | Shift Left | Register, Register | Shift the bits of one register left by another register | `0x50` | `pre-v1` |
| `SHL` | Shift Left | Register, Literal | Shift the bits of a register left by a literal value | `0x51` | `pre-v1` |
| `SHL` | Shift Left | Register, Address | Shift the bits of a register left by the contents of memory at an address | `0x52` | `pre-v1` |
| `SHL` | Shift Left | Register, Pointer | Shift the bits of a register left by the contents of memory at an address in a register | `0x53` | `pre-v1` |
| `SHR` | Shift Right | Register, Register | Shift the bits of one register right by another register | `0x54` | `pre-v1` |
| `SHR` | Shift Right | Register, Literal | Shift the bits of a register right by a literal value | `0x55` | `pre-v1` |
| `SHR` | Shift Right | Register, Address | Shift the bits of a register right by the contents of memory at an address | `0x56` | `pre-v1` |
| `SHR` | Shift Right | Register, Pointer | Shift the bits of a register right by the contents of memory at an address in a register | `0x57` | `pre-v1` |
| **Bitwise** ||||||
| `AND` | Bitwise AND | Register, Register | Bitwise AND one register by another | `0x60` | `pre-v1` |
| `AND` | Bitwise AND | Register, Literal | Bitwise AND a register by a literal value | `0x61` | `pre-v1` |
| `AND` | Bitwise AND | Register, Address | Bitwise AND a register by the contents of memory at an address | `0x62` | `pre-v1` |
| `AND` | Bitwise AND | Register, Pointer | Bitwise AND a register by the contents of memory at an address in a register | `0x63` | `pre-v1` |
| `ORR` | Bitwise OR | Register, Register | Bitwise OR one register by another | `0x64` | `pre-v1` |
| `ORR` | Bitwise OR | Register, Literal | Bitwise OR a register by a literal value | `0x65` | `pre-v1` |
| `ORR` | Bitwise OR | Register, Address | Bitwise OR a register by the contents of memory at an address | `0x66` | `pre-v1` |
| `ORR` | Bitwise OR | Register, Pointer | Bitwise OR a register by the contents of memory at an address in a register | `0x67` | `pre-v1` |
| `XOR` | Bitwise XOR | Register, Register | Bitwise XOR one register by another | `0x68` | `pre-v1` |
| `XOR` | Bitwise XOR | Register, Literal | Bitwise XOR a register by a literal value | `0x69` | `pre-v1` |
| `XOR` | Bitwise XOR | Register, Address | Bitwise XOR a register by the contents of memory at an address | `0x6A` | `pre-v1` |
| `XOR` | Bitwise XOR | Register, Pointer | Bitwise XOR a register by the contents of memory at an address in a register | `0x6B` | `pre-v1` |
| `NOT` | Bitwise NOT | Register | Invert each bit of a register | `0x6C` | `pre-v1` |
| `RNG` | Random Number Generator | Register | Randomise each bit of a register | `0x6D` | `pre-v1` |
| **Comparison** ||||||
| `TST` | Test | Register, Register | Bitwise AND two registers, discarding the result whilst still updating status flags | `0x70` | `pre-v1` |
| `TST` | Test | Register, Literal | Bitwise AND a register and a literal value, discarding the result whilst still updating status flags | `0x71` | `pre-v1` |
| `TST` | Test | Register, Address | Bitwise AND a register and the contents of memory at an address, discarding the result whilst still updating status flags | `0x72` | `pre-v1` |
| `TST` | Test | Register, Pointer | Bitwise AND a register and the contents of memory at an address in a register, discarding the result whilst still updating status flags | `0x73` | `pre-v1` |
| `CMP` | Compare | Register, Register | Subtract a register from another, discarding the result whilst still updating status flags | `0x74` | `pre-v1` |
| `CMP` | Compare | Register, Literal | Subtract a literal value from a register, discarding the result whilst still updating status flags | `0x75` | `pre-v1` |
| `CMP` | Compare | Register, Address | Subtract the contents of memory at an address from a register, discarding the result whilst still updating status flags | `0x76` | `pre-v1` |
| `CMP` | Compare | Register, Pointer | Subtract the contents of memory at an address in a register from a register, discarding the result whilst still updating status flags | `0x77` | `pre-v1` |
| **Data Moving** ||||||
| `MVB` | Move Byte | Register, Register | Move the lower 8-bits of one register to another | `0x80` | `pre-v1` |
| `MVB` | Move Byte | Register, Literal | Move the lower 8-bits of a literal value to a register | `0x81` | `pre-v1` |
| `MVB` | Move Byte | Register, Address | Move 8-bits of the contents of memory starting at an address to a register | `0x82` | `pre-v1` |
| `MVB` | Move Byte | Register, Pointer | Move 8-bits of the contents of memory starting at an address in a register to a register | `0x83` | `pre-v1` |
| `MVB` | Move Byte | Address, Register | Move the lower 8-bits of a register to the contents of memory at an address | `0x84` | `pre-v1` |
| `MVB` | Move Byte | Address, Literal | Move the lower 8-bits of a literal to the contents of memory at an address | `0x85` | `pre-v1` |
| `MVB` | Move Byte | Pointer, Register | Move the lower 8-bits of a register to the contents of memory at an address in a register | `0x86` | `pre-v1` |
| `MVB` | Move Byte | Pointer, Literal | Move the lower 8-bits of a literal to the contents of memory at an address in a register | `0x87` | `pre-v1` |
| `MVW` | Move Word | Register, Register | Move the lower 16-bits (2 bytes) of one register to another | `0x88` | `pre-v1` |
| `MVW` | Move Word | Register, Literal | Move the lower 16-bits (2 bytes) of a literal value to a register | `0x89` | `pre-v1` |
| `MVW` | Move Word | Register, Address | Move 16-bits (2 bytes) of the contents of memory starting at an address to a register | `0x8A` | `pre-v1` |
| `MVW` | Move Word | Register, Pointer | Move 16-bits (2 bytes) of the contents of memory starting at an address in a register to a register | `0x8B` | `pre-v1` |
| `MVW` | Move Word | Address, Register | Move the lower 16-bits (2 bytes) of a register to the contents of memory at an address | `0x8C` | `pre-v1` |
| `MVW` | Move Word | Address, Literal | Move the lower 16-bits (2 bytes) of a literal to the contents of memory at an address | `0x8D` | `pre-v1` |
| `MVW` | Move Word | Pointer, Register | Move the lower 16-bits (2 bytes) of a register to the contents of memory at an address in a register | `0x8E` | `pre-v1` |
| `MVW` | Move Word | Pointer, Literal | Move the lower 16-bits (2 bytes) of a literal to the contents of memory at an address in a register | `0x8F` | `pre-v1` |
| `MVD` | Move Double Word | Register, Register | Move the lower 32-bits (4 bytes) of one register to another | `0x90` | `pre-v1` |
| `MVD` | Move Double Word | Register, Literal | Move the lower 32-bits (4 bytes) of a literal value to a register | `0x91` | `pre-v1` |
| `MVD` | Move Double Word | Register, Address | Move 32-bits (4 bytes) of the contents of memory starting at an address to a register | `0x92` | `pre-v1` |
| `MVD` | Move Double Word | Register, Pointer | Move 32-bits (4 bytes) of the contents of memory starting at an address in a register to a register | `0x93` | `pre-v1` |
| `MVD` | Move Double Word | Address, Register | Move the lower 32-bits (4 bytes) of a register to the contents of memory at an address | `0x94` | `pre-v1` |
| `MVD` | Move Double Word | Address, Literal | Move the lower 32-bits (4 bytes) of a literal to the contents of memory at an address | `0x95` | `pre-v1` |
| `MVD` | Move Double Word | Pointer, Register | Move the lower 32-bits (4 bytes) of a register to the contents of memory at an address in a register | `0x96` | `pre-v1` |
| `MVD` | Move Double Word | Pointer, Literal | Move the lower 32-bits (4 bytes) of a literal to the contents of memory at an address in a register | `0x97` | `pre-v1` |
| `MVQ` | Move Quad Word | Register, Register | Move all 64-bits (8 bytes) of one register to another | `0x98` | `pre-v1` |
| `MVQ` | Move Quad Word | Register, Literal | Move all 64-bits (8 bytes) of a literal value to a register | `0x99` | `pre-v1` |
| `MVQ` | Move Quad Word | Register, Address | Move 64-bits (8 bytes) of the contents of memory starting at an address to a register | `0x9A` | `pre-v1` |
| `MVQ` | Move Quad Word | Register, Pointer | Move 64-bits (8 bytes) of the contents of memory starting at an address in a register to a register | `0x9B` | `pre-v1` |
| `MVQ` | Move Quad Word | Address, Register | Move all 64-bits (8 bytes) of a register to the contents of memory at an address | `0x9C` | `pre-v1` |
| `MVQ` | Move Quad Word | Address, Literal | Move all 64-bits (8 bytes) of a literal to the contents of memory at an address | `0x9D` | `pre-v1` |
| `MVQ` | Move Quad Word | Pointer, Register | Move all 64-bits (8 bytes) of a register to the contents of memory at an address in a register | `0x9E` | `pre-v1` |
| `MVQ` | Move Quad Word | Pointer, Literal | Move all 64-bits (8 bytes) of a literal to the contents of memory at an address in a register | `0x9F` | `pre-v1` |
| **Stack** ||||||
| `PSH` | Push to Stack | Register | Insert the value in a register to the top of the stack | `0xA0` | `pre-v1` |
| `PSH` | Push to Stack | Literal | Insert a literal value to the top of the stack | `0xA1` | `pre-v1` |
| `PSH` | Push to Stack | Address | Insert the contents of memory at an address to the top of the stack | `0xA2` | `pre-v1` |
| `PSH` | Push to Stack | Pointer | Insert the contents of memory at an address in a register to the top of the stack | `0xA3` | `pre-v1` |
| `POP` | Pop from Stack | Register | Remove the value from the top of the stack and store it in a register | `0xA4` | `pre-v1` |
| **Subroutines** ||||||
| `CAL` | Call Subroutine | Address | Call the subroutine at an address, pushing `rpo` and `rsb` to the stack | `0xB0` | `pre-v1` |
| `CAL` | Call Subroutine | Pointer | Call the subroutine at an address in a register, pushing `rpo` and `rsb` to the stack | `0xB1` | `pre-v1` |
| `CAL` | Call Subroutine | Address, Register | Call the subroutine at an address, moving the value in a register to `rfp` | `0xB2` | `pre-v1` |
| `CAL` | Call Subroutine | Address, Literal | Call the subroutine at an address, moving a literal value to `rfp` | `0xB3` | `pre-v1` |
| `CAL` | Call Subroutine | Address, Address | Call the subroutine at an address, moving the contents of memory at an address to `rfp` | `0xB4` | `pre-v1` |
| `CAL` | Call Subroutine | Address, Pointer | Call the subroutine at an address, moving the contents of memory at an address in a register to `rfp` | `0xB5` | `pre-v1` |
| `CAL` | Call Subroutine | Pointer, Register | Call the subroutine at an address in a register, moving the value in a register to `rfp` | `0xB6` | `pre-v1` |
| `CAL` | Call Subroutine | Pointer, Literal | Call the subroutine at an address in a register, moving a literal value to `rfp` | `0xB7` | `pre-v1` |
| `CAL` | Call Subroutine | Pointer, Address | Call the subroutine at an address in a register, moving the contents of memory at an address to `rfp` | `0xB8` | `pre-v1` |
| `CAL` | Call Subroutine | Pointer, Pointer | Call the subroutine at an address in a register, moving the contents of memory at an address in a register to `rfp` | `0xB9` | `pre-v1` |
| `RET` | Return from Subroutine | - | Pop the previous states of `rsb` and `rpo` off the stack | `0xBA` | `pre-v1` |
| `RET` | Return from Subroutine | Register | Pop the previous states of `rsb` and `rpo` off the stack, moving the value in a register to `rrv` | `0xBB` | `pre-v1` |
| `RET` | Return from Subroutine | Literal | Pop the previous states of `rsb` and `rpo` off the stack, moving a literal value to `rrv` | `0xBC` | `pre-v1` |
| `RET` | Return from Subroutine | Address | Pop the previous states off the stack, moving the contents of memory at an address to `rrv` | `0xBD` | `pre-v1` |
| `RET` | Return from Subroutine | Pointer | Pop the previous states off the stack, moving the contents of memory at an address in a register to `rrv` | `0xBE` | `pre-v1` |
| **Console Writing** ||||||
| `WCN` | Write Number to Console | Register | Write a register value as a decimal number to the console | `0xC0` | `pre-v1` |
| `WCN` | Write Number to Console | Literal | Write a literal value as a decimal number to the console | `0xC1` | `pre-v1` |
| `WCN` | Write Number to Console | Address | Write 64-bits (4 bytes) of memory starting at the address as a decimal number to the console | `0xC2` | `pre-v1` |
| `WCN` | Write Number to Console | Pointer | Write 64-bits (4 bytes) of memory starting at the address in a register as a decimal number to the console | `0xC3` | `pre-v1` |
| `WCB` | Write Numeric Byte to Console | Register | Write the lower 8-bits of a register value as a decimal number to the console | `0xC4` | `pre-v1` |
| `WCB` | Write Numeric Byte to Console | Literal | Write the lower 8-bits of a literal value as a decimal number to the console | `0xC5` | `pre-v1` |
| `WCB` | Write Numeric Byte to Console | Address | Write contents of memory at the address as a decimal number to the console | `0xC6` | `pre-v1` |
| `WCB` | Write Numeric Byte to Console | Pointer | Write contents of memory at the address in a register as a decimal number to the console | `0xC7` | `pre-v1` |
| `WCX` | Write Hexadecimal to Console | Register | Write the lower 8-bits of a register value as a hexadecimal number to the console | `0xC8` | `pre-v1` |
| `WCX` | Write Hexadecimal to Console | Literal | Write the lower 8-bits of a literal value as a hexadecimal number to the console | `0xC9` | `pre-v1` |
| `WCX` | Write Hexadecimal to Console | Address | Write contents of memory at the address as a hexadecimal number to the console | `0xCA` | `pre-v1` |
| `WCX` | Write Hexadecimal to Console | Pointer | Write contents of memory at the address in a register as a hexadecimal number to the console | `0xCB` | `pre-v1` |
| `WCC` | Write Raw Byte to Console | Register | Write the lower 8-bits of a register value as a raw byte to the console | `0xCC` | `pre-v1` |
| `WCC` | Write Raw Byte to Console | Literal | Write the lower 8-bits of a literal value as a raw byte to the console | `0xCD` | `pre-v1` |
| `WCC` | Write Raw Byte to Console | Address | Write contents of memory at the address as a raw byte to the console | `0xCE` | `pre-v1` |
| `WCC` | Write Raw Byte to Console | Pointer | Write contents of memory at the address in a register as a raw byte to the console | `0xCF` | `pre-v1` |
| **File Writing** ||||||
| `WFN` | Write Number to File | Register | Write a register value as a decimal number to the opened file | `0xD0` | `pre-v1` |
| `WFN` | Write Number to File | Literal | Write a literal value as a decimal number to the opened file | `0xD1` | `pre-v1` |
| `WFN` | Write Number to File | Address | Write 64-bits (4 bytes) of memory starting at the address as a decimal number to the opened file | `0xD2` | `pre-v1` |
| `WFN` | Write Number to File | Pointer | Write 64-bits (4 bytes) of memory starting at the address in a register as a decimal number to the opened file | `0xD3` | `pre-v1` |
| `WFB` | Write Numeric Byte to File | Register | Write the lower 8-bits of a register value as a decimal number to the opened file | `0xD4` | `pre-v1` |
| `WFB` | Write Numeric Byte to File | Literal | Write the lower 8-bits of a literal value as a decimal number to the opened file | `0xD5` | `pre-v1` |
| `WFB` | Write Numeric Byte to File | Address | Write contents of memory at the address as a decimal number to the opened file | `0xD6` | `pre-v1` |
| `WFB` | Write Numeric Byte to File | Pointer | Write contents of memory at the address in a register as a decimal number to the opened file | `0xD7` | `pre-v1` |
| `WFX` | Write Hexadecimal to File | Register | Write the lower 8-bits of a register value as a hexadecimal number to the opened file | `0xD8` | `pre-v1` |
| `WFX` | Write Hexadecimal to File | Literal | Write the lower 8-bits of a literal value as a hexadecimal number to the opened file | `0xD9` | `pre-v1` |
| `WFX` | Write Hexadecimal to File | Address | Write contents of memory at the address as a hexadecimal number to the opened file | `0xDA` | `pre-v1` |
| `WFX` | Write Hexadecimal to File | Pointer | Write contents of memory at the address in a register as a hexadecimal number to the opened file | `0xDB` | `pre-v1` |
| `WFC` | Write Raw Byte to File | Register | Write the lower 8-bits of a register value as a raw byte to the opened file | `0xDC` | `pre-v1` |
| `WFC` | Write Raw Byte to File | Literal | Write the lower 8-bits of a literal value as a raw byte to the opened file | `0xDD` | `pre-v1` |
| `WFC` | Write Raw Byte to File | Address | Write contents of memory at the address as a raw byte to the opened file | `0xDE` | `pre-v1` |
| `WFC` | Write Raw Byte to File | Pointer | Write contents of memory at the address in a register as a raw byte to the opened file | `0xDF` | `pre-v1` |
| **File Operations** ||||||
| `OFL` | Open File | Address | Open the file at the path specified by a `0x00` terminated string in memory starting at an address | `0xE0` | `pre-v1` |
| `OFL` | Open File | Pointer | Open the file at the path specified by a `0x00` terminated string in memory starting at an address in a register | `0xE1` | `pre-v1` |
| `CFL` | Close File | - | Close the currently open file | `0xE2` | `pre-v1` |
| `DFL` | Delete File | Address | Delete the file at the path specified by a `0x00` terminated string in memory starting at an address | `0xE3` | `pre-v1` |
| `DFL` | Delete File | Pointer | Delete the file at the path specified by a `0x00` terminated string in memory starting at an address in a register | `0xE4` | `pre-v1` |
| `FEX` | File Exists? | Register, Address | Store `1` in a register if the filepath specified in memory starting at an address exists, else `0` | `0xE5` | `pre-v1` |
| `FEX` | File Exists? | Register, Pointer | Store `1` in a register if the filepath specified in memory starting at an address in a register exists, else `0` | `0xE6` | `pre-v1` |
| `FSZ` | Get File Size | Register, Address | In a register, store the byte size of the file at the path specified in memory starting at an address | `0xE7` | `pre-v1` |
| `FSZ` | Get File Size | Register, Pointer | In a register, store the byte size of the file at the path specified in memory starting at an address in a register | `0xE8` | `pre-v1` |
| **Reading** ||||||
| `RCC` | Read Raw Byte from Console | Register | Read a raw byte from the console, storing it in a register | `0xF0` | `pre-v1` |
| `RFC` | Read Raw Byte from File | Register | Read the next byte from the currently open file, storing it in a register | `0xF1` | `pre-v1` |

### Signed Extension Set

Extension set number `0x01`, opcodes start with `0xFF, 0x01`. Contains instructions required for interacting with two's complement signed/negative values.

| Mnemonic | Full Name | Operands | Function | Instruction Code | Minimum Major Version |
|----------|-----------|----------|----------|------------------|-----------------------|
| **Signed Conditional Jumps** ||||||
| `SIGN_JLT` | Jump if Less Than | Address | Jump to an address only if the sign and overflow status flags are different | `0x00` | `v2` |
| `SIGN_JLT` | Jump if Less Than | Pointer | Jump to an address in a register only if the sign and overflow status flags are different | `0x01` | `v2` |
| `SIGN_JLE` | Jump if Less Than or Equal To | Address | Jump to an address only if the sign and overflow status flags are different or the zero status flag is set | `0x02` | `v2` |
| `SIGN_JLE` | Jump if Less Than or Equal To | Pointer | Jump to an address in a register only if the sign and overflow status flags are different or the zero status flag is set | `0x03` | `v2` |
| `SIGN_JGT` | Jump if Greater Than | Address | Jump to an address only if the sign and overflow status flags are the same and the zero status flag is unset | `0x04` | `v2` |
| `SIGN_JGT` | Jump if Greater Than | Pointer | Jump to an address in a register only if the sign and overflow status flags are the same and the zero status flag is unset | `0x05` | `v2` |
| `SIGN_JGE` | Jump if Greater Than or Equal To | Address | Jump to an address only if the sign and overflow status flags are the same | `0x06` | `v2` |
| `SIGN_JGE` | Jump if Greater Than or Equal To | Pointer | Jump to an address in a register only if the sign and overflow status flags are the same | `0x07` | `v2` |
| `SIGN_JSI` | Jump if Signed | Address | Jump to an address only if the sign status flag is set | `0x08` | `v2` |
| `SIGN_JSI` | Jump if Signed | Pointer | Jump to an address in a register only if the sign status flag is set | `0x09` | `v2` |
| `SIGN_JNS` | Jump if not Sign | Address | Jump to an address only if the sign status flag is unset | `0x0A` | `v2` |
| `SIGN_JNS` | Jump if not Sign | Pointer | Jump to an address in a register only the sign status flag is unset | `0x0B` | `v2` |
| `SIGN_JOV` | Jump if Overflow | Address | Jump to an address only if the overflow status flag is set | `0x0C` | `v2` |
| `SIGN_JOV` | Jump if Overflow | Pointer | Jump to an address in a register only if the overflow status flag is set | `0x0D` | `v2` |
| `SIGN_JNO` | Jump if not Overflow | Address | Jump to an address only if the overflow status flag is unset | `0x0E` | `v2` |
| `SIGN_JNO` | Jump if not Overflow | Pointer | Jump to an address in a register only if the overflow status flag is unset | `0x0F` | `v2` |
| **Math** ||||||
| `SIGN_DIV` | Integer Divide | Register, Register | Divide the contents of one register by another, discarding the remainder | `0x10` | `v2` |
| `SIGN_DIV` | Integer Divide | Register, Literal | Divide the contents of a register by a literal value, discarding the remainder | `0x11` | `v2` |
| `SIGN_DIV` | Integer Divide | Register, Address | Divide a register by the contents of memory at an address, discarding the remainder | `0x12` | `v2` |
| `SIGN_DIV` | Integer Divide | Register, Pointer | Divide a register by the contents of memory at an address in a register, discarding the remainder | `0x13` | `v2` |
| `SIGN_DVR` | Divide With Remainder | Register, Register, Register | Divide the contents of one register by another, storing the remainder | `0x14` | `v2` |
| `SIGN_DVR` | Divide With Remainder | Register, Register, Literal | Divide the contents of a register by a literal value, storing the remainder | `0x15` | `v2` |
| `SIGN_DVR` | Divide With Remainder | Register, Register, Address | Divide a register by the contents of memory at an address, storing the remainder | `0x16` | `v2` |
| `SIGN_DVR` | Divide With Remainder | Register, Register, Pointer | Divide a register by the contents of memory at an address in a register, storing the remainder | `0x17` | `v2` |
| `SIGN_REM` | Remainder Only | Register, Register | Divide the contents of one register by another, storing only the remainder | `0x18` | `v2` |
| `SIGN_REM` | Remainder Only | Register, Literal | Divide the contents of a register by a literal value, storing only the remainder | `0x19` | `v2` |
| `SIGN_REM` | Remainder Only | Register, Address | Divide a register by the contents of memory at an address, storing only the remainder | `0x1A` | `v2` |
| `SIGN_REM` | Remainder Only | Register, Pointer | Divide a register by the contents of memory at an address in a register, storing only the remainder | `0x1B` | `v2` |
| `SIGN_SHR` | Arithmetic Shift Right | Register, Register | Shift the bits of one register right by another register, preserving the sign of the original value | `0x20` | `v2` |
| `SIGN_SHR` | Arithmetic Shift Right | Register, Literal | Shift the bits of a register right by a literal value, preserving the sign of the original value | `0x21` | `v2` |
| `SIGN_SHR` | Arithmetic Shift Right | Register, Address | Shift the bits of a register right by the contents of memory at an address, preserving the sign of the original value | `0x22` | `v2` |
| `SIGN_SHR` | Arithmetic Shift Right | Register, Pointer | Shift the bits of a register right by the contents of memory at an address in a register, preserving the sign of the original value | `0x23` | `v2` |
| **Sign-Extending Data Moves** ||||||
| `SIGN_MVB` | Move Byte, Extend to Quad Word | Register, Register | Move the lower 8-bits of one register to another, extending the resulting value to a signed 64-bit value | `0x30` | `v2` |
| `SIGN_MVB` | Move Byte, Extend to Quad Word | Register, Literal | Move the lower 8-bits of a literal value to a register, extending the resulting value to a signed 64-bit value | `0x31` | `v2` |
| `SIGN_MVB` | Move Byte, Extend to Quad Word | Register, Address | Move 8-bits of the contents of memory starting at an address to a register, extending the resulting value to a signed 64-bit value | `0x32` | `v2` |
| `SIGN_MVB` | Move Byte, Extend to Quad Word | Register, Pointer | Move 8-bits of the contents of memory starting at an address in a register to a register, extending the resulting value to a signed 64-bit value | `0x33` | `v2` |
| `SIGN_MVW` | Move Word, Extend to Quad Word | Register, Register | Move the lower 16-bits (2 bytes) of one register to another, extending the resulting value to a signed 64-bit value | `0x34` | `v2` |
| `SIGN_MVW` | Move Word, Extend to Quad Word | Register, Literal | Move the lower 16-bits (2 bytes) of a literal value to a register, extending the resulting value to a signed 64-bit value | `0x35` | `v2` |
| `SIGN_MVW` | Move Word, Extend to Quad Word | Register, Address | Move 16-bits (2 bytes) of the contents of memory starting at an address to a register, extending the resulting value to a signed 64-bit value | `0x36` | `v2` |
| `SIGN_MVW` | Move Word, Extend to Quad Word | Register, Pointer | Move 16-bits (2 bytes) of the contents of memory starting at an address in a register to a register, extending the resulting value to a signed 64-bit value | `0x37` | `v2` |
| `SIGN_MVD` | Move Double Word, Extend to Quad Word | Register, Register | Move the lower 32-bits (4 bytes) of one register to another, extending the resulting value to a signed 64-bit value | `0x40` | `v2` |
| `SIGN_MVD` | Move Double Word, Extend to Quad Word | Register, Literal | Move the lower 32-bits (4 bytes) of a literal value to a register, extending the resulting value to a signed 64-bit value | `0x41` | `v2` |
| `SIGN_MVD` | Move Double Word, Extend to Quad Word | Register, Address | Move 32-bits (4 bytes) of the contents of memory starting at an address to a register, extending the resulting value to a signed 64-bit value | `0x42` | `v2` |
| `SIGN_MVD` | Move Double Word, Extend to Quad Word | Register, Pointer | Move 32-bits (4 bytes) of the contents of memory starting at an address in a register to a register, extending the resulting value to a signed 64-bit value | `0x43` | `v2` |
| **Console Writing** ||||||
| `SIGN_WCN` | Write Number to Console | Register | Write a register value as a signed decimal number to the console | `0x50` | `v2` |
| `SIGN_WCN` | Write Number to Console | Literal | Write a literal value as a signed decimal number to the console | `0x51` | `v2` |
| `SIGN_WCN` | Write Number to Console | Address | Write 64-bits (4 bytes) of memory starting at the address as a signed decimal number to the console | `0x52` | `v2` |
| `SIGN_WCN` | Write Number to Console | Pointer | Write 64-bits (4 bytes) of memory starting at the address in a register as a signed decimal number to the console | `0x53` | `v2` |
| `SIGN_WCB` | Write Numeric Byte to Console | Register | Write the lower 8-bits of a register value as a signed decimal number to the console | `0x54` | `v2` |
| `SIGN_WCB` | Write Numeric Byte to Console | Literal | Write the lower 8-bits of a literal value as a signed decimal number to the console | `0x55` | `v2` |
| `SIGN_WCB` | Write Numeric Byte to Console | Address | Write contents of memory at the address as a signed decimal number to the console | `0x56` | `v2` |
| `SIGN_WCB` | Write Numeric Byte to Console | Pointer | Write contents of memory at the address in a register as a signed decimal number to the console | `0x57` | `v2` |
| **File Writing** ||||||
| `SIGN_WFN` | Write Number to File | Register | Write a register value as a signed decimal number to the opened file | `0x60` | `v2` |
| `SIGN_WFN` | Write Number to File | Literal | Write a literal value as a signed decimal number to the opened file | `0x61` | `v2` |
| `SIGN_WFN` | Write Number to File | Address | Write 64-bits (4 bytes) of memory starting at the address as a signed decimal number to the opened file | `0x62` | `v2` |
| `SIGN_WFN` | Write Number to File | Pointer | Write 64-bits (4 bytes) of memory starting at the address in a register as a signed decimal number to the opened file | `0x63` | `v2` |
| `SIGN_WFB` | Write Numeric Byte to File | Register | Write the lower 8-bits of a register value as a signed decimal number to the opened file | `0x64` | `v2` |
| `SIGN_WFB` | Write Numeric Byte to File | Literal | Write the lower 8-bits of a literal value as a signed decimal number to the opened file | `0x65` | `v2` |
| `SIGN_WFB` | Write Numeric Byte to File | Address | Write contents of memory at the address as a signed decimal number to the opened file | `0x66` | `v2` |
| `SIGN_WFB` | Write Numeric Byte to File | Pointer | Write contents of memory at the address in a register as a signed decimal number to the opened file | `0x67` | `v2` |
| **Sign Extension** ||||||
| `SIGN_EXB` | Extend Signed Byte to Signed Quad Word | Register | Convert the signed value in the lower 8-bits of a register to its equivalent representation as a signed 64-bit number  | `0x70` | `v2` |
| `SIGN_EXW` | Extend Signed Word to Signed Quad Word | Register | Convert the signed value in the lower 16-bits of a register to its equivalent representation as a signed 64-bit number | `0x71` | `v2` |
| `SIGN_EXD` | Extend Signed Double Word to Signed Quad Word | Register | Convert the signed value in the lower 32-bits of a register to its equivalent representation as a signed 64-bit number | `0x72` | `v2` |
| **Negation** ||||||
| `SIGN_NEG` | Two's Complement Negation | Register | Replace the value in a register with its two's complement, thereby flipping the sign of the value. | `0x80` | `v2` |

### Floating Point Extension Set

Extension set number `0x02`, opcodes start with `0xFF, 0x02`. Contains instructions required for interacting with IEEE 754 floating point values.

| Mnemonic | Full Name | Operands | Function | Instruction Code | Minimum Major Version |
|----------|-----------|----------|----------|------------------|-----------------------|
| **Math** ||||||
| `FLPT_ADD` | Add | Register, Register | Add the contents of one register to another | `0x00` | `v2` |
| `FLPT_ADD` | Add | Register, Literal | Add a literal value to the contents of a register | `0x01` | `v2` |
| `FLPT_ADD` | Add | Register, Address | Add the contents of memory at an address to a register | `0x02` | `v2` |
| `FLPT_ADD` | Add | Register, Pointer | Add the contents of memory at an address in a register to a register | `0x03` | `v2` |
| `FLPT_SUB` | Subtract | Register, Register | Subtract the contents of one register from another | `0x10` | `v2` |
| `FLPT_SUB` | Subtract | Register, Literal | Subtract a literal value from the contents of a register | `0x11` | `v2` |
| `FLPT_SUB` | Subtract | Register, Address | Subtract the contents of memory at an address from a register | `0x12` | `v2` |
| `FLPT_SUB` | Subtract | Register, Pointer | Subtract the contents of memory at an address in a register from a register | `0x13` | `v2` |
| `FLPT_MUL` | Multiply | Register, Register | Multiply the contents of one register by another | `0x20` | `v2` |
| `FLPT_MUL` | Multiply | Register, Literal | Multiply the contents of a register by a literal value | `0x21` | `v2` |
| `FLPT_MUL` | Multiply | Register, Address | Multiply a register by the contents of memory at an address | `0x22` | `v2` |
| `FLPT_MUL` | Multiply | Register, Pointer | Multiply a register by the contents of memory at an address in a register | `0x23` | `v2` |
| `FLPT_DIV` | Integer Divide | Register, Register | Divide the contents of one register by another, discarding the remainder | `0x30` | `v2` |
| `FLPT_DIV` | Integer Divide | Register, Literal | Divide the contents of a register by a literal value, discarding the remainder | `0x31` | `v2` |
| `FLPT_DIV` | Integer Divide | Register, Address | Divide a register by the contents of memory at an address, discarding the remainder | `0x32` | `v2` |
| `FLPT_DIV` | Integer Divide | Register, Pointer | Divide a register by the contents of memory at an address in a register, discarding the remainder | `0x33` | `v2` |
| `FLPT_DVR` | Divide With Remainder | Register, Register, Register | Divide the contents of one register by another, storing the remainder | `0x34` | `v2` |
| `FLPT_DVR` | Divide With Remainder | Register, Register, Literal | Divide the contents of a register by a literal value, storing the remainder | `0x35` | `v2` |
| `FLPT_DVR` | Divide With Remainder | Register, Register, Address | Divide a register by the contents of memory at an address, storing the remainder | `0x36` | `v2` |
| `FLPT_DVR` | Divide With Remainder | Register, Register, Pointer | Divide a register by the contents of memory at an address in a register, storing the remainder | `0x37` | `v2` |
| `FLPT_REM` | Remainder Only | Register, Register | Divide the contents of one register by another, storing only the remainder | `0x38` | `v2` |
| `FLPT_REM` | Remainder Only | Register, Literal | Divide the contents of a register by a literal value, storing only the remainder | `0x39` | `v2` |
| `FLPT_REM` | Remainder Only | Register, Address | Divide a register by the contents of memory at an address, storing only the remainder | `0x3A` | `v2` |
| `FLPT_REM` | Remainder Only | Register, Pointer | Divide a register by the contents of memory at an address in a register, storing only the remainder | `0x3B` | `v2` |
| `FLPT_SIN` | Sine | Register | Calculate the sine of the value in a register in radians | `0x40` | `v2` |
| `FLPT_ASN` | Inverse Sine | Register | Calculate the inverse sine of the value in a register in radians | `0x41` | `v2` |
| `FLPT_COS` | Cosine | Register | Calculate the cosine of the value in a register in radians | `0x42` | `v2` |
| `FLPT_ACS` | Inverse Cosine | Register | Calculate the inverse cosine of the value in a register in radians | `0x43` | `v2` |
| `FLPT_TAN` | Tangent | Register | Calculate the tangent of the value in a register in radians | `0x44` | `v2` |
| `FLPT_ATN` | Inverse Tangent | Register | Calculate the inverse tangent of the value in a register in radians | `0x45` | `v2` |
| `FLPT_PTN` | 2 Argument Inverse Tangent | Register, Register | Calculate the 2 argument inverse tangent between 2 registers in the order y, x | `0x46` | `v2` |
| `FLPT_PTN` | 2 Argument Inverse Tangent | Register, Literal | Calculate the 2 argument inverse tangent between a register and a literal in the order y, x | `0x47` | `v2` |
| `FLPT_PTN` | 2 Argument Inverse Tangent | Register, Address | Calculate the 2 argument inverse tangent between a register and the contents of memory at an address in the order y, x | `0x48` | `v2` |
| `FLPT_PTN` | 2 Argument Inverse Tangent | Register, Pointer | Calculate the 2 argument inverse tangent between a register and the contents of memory at an address in a register in the order y, x | `0x49` | `v2` |
| `FLPT_POW` | Exponentiation | Register, Register | Calculate the value of a register raised to the power of another register | `0x50` | `v2` |
| `FLPT_POW` | Exponentiation | Register, Literal | Calculate the value of a register raised to the power of a literal | `0x51` | `v2` |
| `FLPT_POW` | Exponentiation | Register, Address | Calculate the value of a register raised to the power of the contents of memory at an address | `0x52` | `v2` |
| `FLPT_POW` | Exponentiation | Register, Pointer | Calculate the value of a register raised to the power of the contents of memory at an address in a register | `0x53` | `v2` |
| `FLPT_LOG` | Logarithm | Register, Register | Calculate the logarithm of a register with the base from another register | `0x60` | `v2` |
| `FLPT_LOG` | Logarithm | Register, Literal | Calculate the logarithm of a register with the base from a literal | `0x61` | `v2` |
| `FLPT_LOG` | Logarithm | Register, Address | Calculate the logarithm of a register with the base from the contents of memory at an address | `0x62` | `v2` |
| `FLPT_LOG` | Logarithm | Register, Pointer | Calculate the logarithm of a register with the base from the contents of memory at an address in a register | `0x63` | `v2` |
| **Console Writing** ||||||
| `FLPT_WCN` | Write Number to Console | Register | Write a register value as a signed decimal number to the console | `0x70` | `v2` |
| `FLPT_WCN` | Write Number to Console | Literal | Write a literal value as a signed decimal number to the console | `0x71` | `v2` |
| `FLPT_WCN` | Write Number to Console | Address | Write 64-bits (4 bytes) of memory starting at the address as a signed decimal number to the console | `0x72` | `v2` |
| `FLPT_WCN` | Write Number to Console | Pointer | Write 64-bits (4 bytes) of memory starting at the address in a register as a signed decimal number to the console | `0x73` | `v2` |
| **File Writing** ||||||
| `FLPT_WFN` | Write Number to File | Register | Write a register value as a floating point decimal number to the opened file | `0x80` | `v2` |
| `FLPT_WFN` | Write Number to File | Literal | Write a literal value as a floating point decimal number to the opened file | `0x81` | `v2` |
| `FLPT_WFN` | Write Number to File | Address | Write 64-bits (4 bytes) of memory starting at the address as a floating point decimal number to the opened file | `0x82` | `v2` |
| `FLPT_WFN` | Write Number to File | Pointer | Write 64-bits (4 bytes) of memory starting at the address in a register as a floating point decimal number to the opened file | `0x83` | `v2` |
| **Conversions** ||||||
| `FLPT_EXH` | Extend Half Precision Float to Double Precision Float | Register | Convert the value in a register from a half-precision float (16-bits) to a double-precision float (64-bits) | `0x90` | `v2` |
| `FLPT_EXS` | Extend Single Precision Float to Double Precision Float | Register | Convert the value in a register from a single-precision float (32-bits) to a double-precision float (64-bits) | `0x91` | `v2` |
| `FLPT_SHS` | Shrink Double Precision Float to Single Precision Float | Register | Convert the value in a register from a double-precision float (64-bits) to a single-precision float (32-bits) | `0x92` | `v2` |
| `FLPT_SHH` | Shrink Double Precision Float to Half Precision Float | Register | Convert the value in a register from a double-precision float (64-bits) to a half-precision float (16-bits) | `0x93` | `v2` |
| `FLPT_NEG` | Negation | Register | Reverse the sign of the floating point number in a register, equivalent to flipping the sign bit. | `0xA0` | `v2` |
| `FLPT_UTF` | Convert Unsigned Quad Word to Double Precision Float | Register | Convert the unsigned value in a register to a double-precision float (64-bits) | `0xB0` | `v2` |
| `FLPT_STF` | Convert Signed Quad Word to Double Precision Float | Register | Convert the signed value in a register to a double-precision float (64-bits) | `0xB1` | `v2` |
| `FLPT_FTS` | Convert Double Precision Float to Signed Quad Word through Truncation | Register | Convert the double-precision float (64-bits) value in a register to a signed 64-bit integer by rounding toward 0 | `0xC0` | `v2` |
| `FLPT_FCS` | Convert Double Precision Float to Signed Quad Word through Ceiling Rounding | Register | Convert the double-precision float (64-bits) value in a register to a signed 64-bit integer by rounding to the greater integer | `0xC1` | `v2` |
| `FLPT_FFS` | Convert Double Precision Float to Signed Quad Word through Floor Rounding | Register | Convert the double-precision float (64-bits) value in a register to a signed 64-bit integer by rounding to the lesser integer | `0xC2` | `v2` |
| `FLPT_FNS` | Convert Double Precision Float to Signed Quad Word through Nearest Rounding | Register | Convert the double-precision float (64-bits) value in a register to the nearest signed 64-bit integer, rounding midpoints to the nearest even number | `0xC3` | `v2` |
| **Comparison** ||||||
| `FLPT_CMP` | Compare | Register, Register | Subtract a register from another, discarding the result whilst still updating status flags | `0xD0` | `v2` |
| `FLPT_CMP` | Compare | Register, Literal | Subtract a literal value from a register, discarding the result whilst still updating status flags | `0xD1` | `v2` |
| `FLPT_CMP` | Compare | Register, Address | Subtract the contents of memory at an address from a register, discarding the result whilst still updating status flags | `0xD2` | `v2` |
| `FLPT_CMP` | Compare | Register, Pointer | Subtract the contents of memory at an address in a register from a register, discarding the result whilst still updating status flags | `0xD3` | `v2` |

### Extended Base Set

Extension set number `0x03`, opcodes start with `0xFF, 0x03`. Contains additional instructions that complement the base instruction set, but do not provide any major additional functionality.

| Mnemonic | Full Name | Operands | Function | Instruction Code | Minimum Major Version |
|----------|-----------|----------|----------|------------------|-----------------------|
| **Byte Operations** ||||||
| `EXTD_BSW` | Reverse Byte Order | Register | Reverse the byte order of a register, thereby converting little endian to big endian and vice versa | `0x00` | `v2` |
| **Processor Queries** ||||||
| `EXTD_QPF` | Query Present Features | Register | Set the value of a register to a bit-field representing the features implemented in the executing processor | `0x10` | `v4` |
| `EXTD_QPV` | Query Present Version | Register | Set the value of a register to the major version the executing processor | `0x11` | `v4` |
| `EXTD_QPV` | Query Present Version | Register, Register | Set the value of two registers to the major and minor version the executing processor respectively | `0x12` | `v4` |
| `EXTD_CSS` | Call Stack Size | Register | Set the value of a register to the number of bytes pushed to the stack when the `CAL` instruction is used | `0x13` | `v4` |
| **Halt** ||||||
| `EXTD_HLT` | Halt With Exit Code | Register | Stops the processor from executing the program, setting the exit code to the value of a register | `0x20` | `v4` |
| `EXTD_HLT` | Halt With Exit Code | Literal | Stops the processor from executing the program, setting the exit code to a literal value | `0x21` | `v4` |
| `EXTD_HLT` | Halt With Exit Code | Address | Stops the processor from executing the program, setting the exit code to the contents of memory at an address | `0x22` | `v4` |
| `EXTD_HLT` | Halt With Exit Code | Pointer | Stops the processor from executing the program, setting the exit code to the contents of memory at an address in a register | `0x23` | `v4` |
| **Pointers** ||||||
| `EXTD_MPA` | Move Pointer Address | Register, Pointer | Set the value of a register to the calculated address of a pointer | `0x30` | `v4` |
| `EXTD_MPA` | Move Pointer Address | Address, Pointer | Set the value of memory at an address to the calculated address of a pointer | `0x31` | `v4` |
| `EXTD_MPA` | Move Pointer Address | Pointer, Pointer | Set the value of memory at an address in a register to the calculated address of a pointer | `0x32` | `v4` |

### External Assembly Extension Set

Extension set number `0x04`, opcodes start with `0xFF, 0x04`. Contains instructions that enable interoperation with external C#/.NET programs.

| Mnemonic | Full Name | Operands | Function | Instruction Code | Minimum Major Version |
|----------|-----------|----------|----------|------------------|-----------------------|
| **Loading** ||||||
| `ASMX_LDA` | Load Assembly | Address | Open the .NET Assembly at the path specified by a `0x00` terminated string in memory starting at an address | `0x00` | `v3` |
| `ASMX_LDA` | Load Assembly | Pointer | Open the .NET Assembly at the path specified by a `0x00` terminated string in memory starting at an address in a register | `0x01` | `v3` |
| `ASMX_LDF` | Load Function | Address | Open the function in the open .NET assembly with the name specified by a `0x00` terminated string in memory starting at an address | `0x02` | `v3` |
| `ASMX_LDF` | Load Function | Pointer | Open the function in the open .NET assembly with the name specified by a `0x00` terminated string in memory starting at an address in a register | `0x03` | `v3` |
| **Closing** ||||||
| `ASMX_CLA` | Close Assembly | - | Close the currently open .NET Assembly, as well as any open function | `0x10` | `v3` |
| `ASMX_CLF` | Close Function | - | Close the currently open function, the assembly stays open | `0x11` | `v3` |
| **Validity Check** ||||||
| `ASMX_AEX` | Assembly Valid? | Address | Store `1` in a register if the .NET Assembly at the path specified in memory starting at an address exists and is valid, else `0` | `0x20` | `v3` |
| `ASMX_AEX` | Assembly Valid? | Pointer | Store `1` in a register if the .NET Assembly at the path specified in memory starting at an address in a register exists and is valid, else `0` | `0x21` | `v3` |
| `ASMX_FEX` | Function Valid? | Address | Store `1` in a register if the function with the name specified in memory starting at an address exists in the open .NET Assembly and is valid, else `0` | `0x22` | `v3` |
| `ASMX_FEX` | Function Valid? | Pointer | Store `1` in a register if the function with the name specified in memory starting at an address in a register exists in the open .NET Assembly and is valid, else `0` | `0x23` | `v3` |
| **Calling** ||||||
| `ASMX_CAL` | Call External Function | - | Call the loaded external function, giving `null` as the passed value | `0x30` | `v3` |
| `ASMX_CAL` | Call External Function | Register | Call the loaded external function, giving the value of a register as the passed value | `0x31` | `v3` |
| `ASMX_CAL` | Call External Function | Literal | Call the loaded external function, giving a literal value as the passed value | `0x32` | `v3` |
| `ASMX_CAL` | Call External Function | Address | Call the loaded external function, giving the contents of memory at an address as the passed value | `0x33` | `v3` |
| `ASMX_CAL` | Call External Function | Pointer | Call the loaded external function, giving the contents of memory at an address in a register as the passed value | `0x34` | `v3` |

### Memory Allocation Extension Set

Extension set number `0x05`, opcodes start with `0xFF, 0x05`. Contains instructions that provide runtime memory management, ensuring that memory regions are non-overlapping and that there is enough free memory available.

| Mnemonic | Full Name | Operands | Function | Instruction Code | Minimum Major Version |
|----------|-----------|----------|----------|------------------|-----------------------|
| **Allocation** ||||||
| `HEAP_ALC` | Allocate Memory | Register, Register | Allocate a block of memory with the value of a register as its size, storing the first address of the allocated block in a register, throwing an error if the operation fails | `0x00` | `v3` |
| `HEAP_ALC` | Allocate Memory | Register, Literal | Allocate a block of memory with a literal value as its size, storing the first address of the allocated block in a register, throwing an error if the operation fails | `0x01` | `v3` |
| `HEAP_ALC` | Allocate Memory | Register, Address | Allocate a block of memory with the contents of memory at an address as its size, storing the first address of the allocated block in a register, throwing an error if the operation fails | `0x02` | `v3` |
| `HEAP_ALC` | Allocate Memory | Register, Pointer | Allocate a block of memory with the contents of memory at an address in a register as its size, storing the first address of the allocated block in a register, throwing an error if the operation fails | `0x03` | `v3` |
| `HEAP_TRY` | Try Allocate Memory | Register, Register | Allocate a block of memory with the value of a register as its size, storing the first address of the allocated block in a register, or storing `-1` if the operation fails  | `0x04` | `v3` |
| `HEAP_TRY` | Try Allocate Memory | Register, Literal | Allocate a block of memory with a literal value as its size, storing the first address of the allocated block in a register, or storing `-1` if the operation fails | `0x05` | `v3` |
| `HEAP_TRY` | Try Allocate Memory | Register, Address | Allocate a block of memory with the contents of memory at an address as its size, storing the first address of the allocated block in a register, or storing `-1` if the operation fails | `0x06` | `v3` |
| `HEAP_TRY` | Try Allocate Memory | Register, Pointer | Allocate a block of memory with the contents of memory at an address in a register as its size, storing the first address of the allocated block in a register, or storing `-1` if the operation fails | `0x07` | `v3` |
| **Re-allocation** ||||||
| `HEAP_REA` | Re-allocate Memory | Register, Register | Re-allocate a block of memory starting at the address in a register with the value of a register as its size, storing the first address of the allocated block in a register, throwing an error if the operation fails | `0x10` | `v3` |
| `HEAP_REA` | Re-allocate Memory | Register, Literal | Re-allocate a block of memory starting at the address in a register with a literal value as its size, storing the first address of the allocated block in a register, throwing an error if the operation fails | `0x11` | `v3` |
| `HEAP_REA` | Re-allocate Memory | Register, Address | Re-allocate a block of memory starting at the address in a register with the contents of memory at an address as its size, storing the first address of the allocated block in a register, throwing an error if the operation fails | `0x12` | `v3` |
| `HEAP_REA` | Re-allocate Memory | Register, Pointer | Re-allocate a block of memory starting at the address in a register with the contents of memory at an address in a register as its size, storing the first address of the allocated block in a register, throwing an error if the operation fails | `0x13` | `v3` |
| `HEAP_TRE` | Try Re-allocate Memory | Register, Register | Re-allocate a block of memory starting at the address in a register with the value of a register as its size, storing the first address of the allocated block in a register, or storing `-1` if the operation fails  | `0x14` | `v3` |
| `HEAP_TRE` | Try Re-allocate Memory | Register, Literal | Re-allocate a block of memory starting at the address in a register with a literal value as its size, storing the first address of the allocated block in a register, or storing `-1` if the operation fails | `0x15` | `v3` |
| `HEAP_TRE` | Try Re-allocate Memory | Register, Address | Re-allocate a block of memory starting at the address in a register with the contents of memory at an address as its size, storing the first address of the allocated block in a register, or storing `-1` if the operation fails | `0x16` | `v3` |
| `HEAP_TRE` | Try Re-allocate Memory | Register, Pointer | Re-allocate a block of memory starting at the address in a register with the contents of memory at an address in a register as its size, storing the first address of the allocated block in a register, or storing `-1` if the operation fails | `0x17` | `v3` |
| **Freeing** ||||||
| `HEAP_FRE` | Free Memory | Register | Free a block of memory starting at the address in a register | `0x20` | `v3` |

### File System Extension Set

Extension set number `0x06`, opcodes start with `0xFF, 0x06`. Contains instructions that provide additional file and directory operations not part of the base instruction set.

| Mnemonic | Full Name | Operands | Function | Instruction Code | Minimum Major Version |
|----------|-----------|----------|----------|------------------|-----------------------|
| **Working Directory** ||||||
| `FSYS_CWD` | Change Working Directory | Address | Change the process working directory to the directory at the path specified by a `0x00` terminated string in memory starting at an address | `0x00` | `v4` |
| `FSYS_CWD` | Change Working Directory | Pointer | Change the process working directory to the directory at the path specified by a `0x00` terminated string in memory starting at an address in a register | `0x01` | `v4` |
| `FSYS_GWD` | Get Working Directory | Address | Store the `0x00` terminated path of the process working directory in memory starting at an address | `0x02` | `v4` |
| `FSYS_GWD` | Get Working Directory | Pointer | Store the `0x00` terminated path of the process working directory in memory starting at an address in a register | `0x03` | `v4` |
| **Directory Operations** ||||||
| `FSYS_CDR` | Create Directory | Address | Create a directory at the path specified by a `0x00` terminated string in memory starting at an address | `0x10` | `v4` |
| `FSYS_CDR` | Create Directory | Pointer | Create a directory at the path specified by a `0x00` terminated string in memory starting at an address in a register | `0x11` | `v4` |
| `FSYS_DDR` | Recursively Delete Directory | Address | Delete the directory and all its contents at the path specified by a `0x00` terminated string in memory starting at an address | `0x20` | `v4` |
| `FSYS_DDR` | Recursively Delete Directory | Pointer | Delete the directory and all its contents at the path specified by a `0x00` terminated string in memory starting at an address in a register | `0x21` | `v4` |
| `FSYS_DDE` | Delete Empty Directory | Address | Delete the empty directory at the path specified by a `0x00` terminated string in memory starting at an address | `0x22` | `v4` |
| `FSYS_DDE` | Delete Empty Directory | Pointer | Delete the empty directory at the path specified by a `0x00` terminated string in memory starting at an address in a register | `0x23` | `v4` |
| `FSYS_DEX` | Directory Exists? | Register, Address | Store `1` in a register if the directory path specified in memory starting at an address exists, else `0` | `0x30` | `v4` |
| `FSYS_DEX` | Directory Exists? | Register, Pointer | Store `1` in a register if the directory path specified in memory starting at an address in a register exists, else `0` | `0x31` | `v4` |
| **File Movement** ||||||
| `FSYS_CPY` | Copy File | Address, Address | Copy between paths specified in memory, from the second operand to the first | `0x40` | `v4` |
| `FSYS_CPY` | Copy File | Address, Pointer | Copy between paths specified in memory, from the second operand to the first | `0x41` | `v4` |
| `FSYS_CPY` | Copy File | Pointer, Address | Copy between paths specified in memory, from the second operand to the first | `0x42` | `v4` |
| `FSYS_CPY` | Copy File | Pointer, Pointer | Copy between paths specified in memory, from the second operand to the first | `0x43` | `v4` |
| `FSYS_MOV` | Move File | Address, Address | Move between paths specified in memory, from the second operand to the first, deleting the source file | `0x44` | `v4` |
| `FSYS_MOV` | Move File | Address, Pointer | Move between paths specified in memory, from the second operand to the first, deleting the source file | `0x45` | `v4` |
| `FSYS_MOV` | Move File | Pointer, Address | Move between paths specified in memory, from the second operand to the first, deleting the source file | `0x46` | `v4` |
| `FSYS_MOV` | Move File | Pointer, Pointer | Move between paths specified in memory, from the second operand to the first, deleting the source file | `0x47` | `v4` |
| **Directory Listing** ||||||
| `FSYS_BDL` | Begin Directory Listing | - | Start listing the contents of the process's current directory | `0x50` | `v4` |
| `FSYS_BDL` | Begin Directory Listing | Address | Start listing the contents of the directory path specified in memory starting at an address | `0x51` | `v4` |
| `FSYS_BDL` | Begin Directory Listing | Pointer | Start listing the contents of the directory path specified in memory starting at an address in a register | `0x52` | `v4` |
| `FSYS_GNF` | Get Next File | Address | Store the `0x00` terminated path of the next file in the current directory listing in memory starting at an address | `0x60` | `v4` |
| `FSYS_GNF` | Get Next File | Pointer | Store the `0x00` terminated path of the next file in the current directory listing in memory starting at an address in a register | `0x61` | `v4` |
| `FSYS_GND` | Get Next Directory | Address | Store the `0x00` terminated path of the next directory in the current directory listing in memory starting at an address | `0x62` | `v4` |
| `FSYS_GND` | Get Next Directory | Pointer | Store the `0x00` terminated path of the next directory in the current directory listing in memory starting at an address in a register | `0x63` | `v4` |
| **File Times** ||||||
| `FSYS_GCT` | Get Creation Time | Register, Address | Store in a register the creation time of a file specified by a `0x00` terminated string in memory starting at an address | `0x70` | `v4` |
| `FSYS_GCT` | Get Creation Time | Register, Pointer | Store in a register the creation time of a file specified by a `0x00` terminated string in memory starting at an address in a register | `0x71` | `v4` |
| `FSYS_GMT` | Get Modified Time | Register, Address | Store in a register the modified time of a file specified by a `0x00` terminated string in memory starting at an address | `0x72` | `v4` |
| `FSYS_GMT` | Get Modified Time | Register, Pointer | Store in a register the modified time of a file specified by a `0x00` terminated string in memory starting at an address in a register | `0x73` | `v4` |
| `FSYS_GAT` | Get Access Time | Register, Address | Store in a register the access time of a file specified by a `0x00` terminated string in memory starting at an address | `0x74` | `v4` |
| `FSYS_GAT` | Get Access Time | Register, Pointer | Store in a register the access time of a file specified by a `0x00` terminated string in memory starting at an address in a register | `0x75` | `v4` |
| `FSYS_SCT` | Set Creation Time | Address, Register | Set the creation time of a file specified by a `0x00` terminated string in memory starting at an address to a value stored in a register | `0x80` | `v4` |
| `FSYS_SCT` | Set Creation Time | Address, Literal | Set the creation time of a file specified by a `0x00` terminated string in memory starting at an address to a literal value | `0x81` | `v4` |
| `FSYS_SCT` | Set Creation Time | Pointer, Register | Set the creation time of a file specified by a `0x00` terminated string in memory starting at an address in a register to a value stored in a register | `0x82` | `v4` |
| `FSYS_SCT` | Set Creation Time | Pointer, Literal | Set the creation time of a file specified by a `0x00` terminated string in memory starting at an address in a register to a literal value | `0x83` | `v4` |
| `FSYS_SMT` | Set Modified Time | Address, Register | Set the modified time of a file specified by a `0x00` terminated string in memory starting at an address to a value stored in a register | `0x84` | `v4` |
| `FSYS_SMT` | Set Modified Time | Address, Literal | Set the modified time of a file specified by a `0x00` terminated string in memory starting at an address to a literal value | `0x85` | `v4` |
| `FSYS_SMT` | Set Modified Time | Pointer, Register | Set the modified time of a file specified by a `0x00` terminated string in memory starting at an address in a register to a value stored in a register | `0x86` | `v4` |
| `FSYS_SMT` | Set Modified Time | Pointer, Literal | Set the modified time of a file specified by a `0x00` terminated string in memory starting at an address in a register to a literal value | `0x87` | `v4` |
| `FSYS_SAT` | Set Access Time | Address, Register | Set the access time of a file specified by a `0x00` terminated string in memory starting at an address to a value stored in a register | `0x88` | `v4` |
| `FSYS_SAT` | Set Access Time | Address, Literal | Set the access time of a file specified by a `0x00` terminated string in memory starting at an address to a literal value | `0x89` | `v4` |
| `FSYS_SAT` | Set Access Time | Pointer, Register | Set the access time of a file specified by a `0x00` terminated string in memory starting at an address in a register to a value stored in a register | `0x8A` | `v4` |
| `FSYS_SAT` | Set Access Time | Pointer, Literal | Set the access time of a file specified by a `0x00` terminated string in memory starting at an address in a register to a literal value | `0x8B` | `v4` |

### Terminal Extension Set

Extension set number `0x07`, opcodes start with `0xFF, 0x07`. Contains instructions that provide functionality to interact with and control the process's console window.

| Mnemonic | Full Name | Operands | Function | Instruction Code | Minimum Major Version |
|----------|-----------|----------|----------|------------------|-----------------------|
| **Clear** ||||||
| `TERM_CLS` | Clear Screen | - | Remove all characters from the console window and return the cursor to its initial position | `0x00` | `v4` |
| **Auto Echo** ||||||
| `TERM_AEE` | Auto Echo Enable | - | Set the Auto Echo status flag | `0x10` | `v4` |
| `TERM_AED` | Auto Echo Disable | - | Unset the Auto Echo status flag | `0x11` | `v4` |
| **Cursor Position** ||||||
| `TERM_SCY` | Set Vertical Cursor Position | Register | Set the vertical position of the console cursor to the value of a register | `0x20` | `v4` |
| `TERM_SCY` | Set Vertical Cursor Position | Literal | Set the vertical position of the console cursor to a literal value | `0x21` | `v4` |
| `TERM_SCY` | Set Vertical Cursor Position | Address | Set the vertical position of the console cursor to the value of memory at an address | `0x22` | `v4` |
| `TERM_SCY` | Set Vertical Cursor Position | Pointer | Set the vertical position of the console cursor to the value of memory at an address in a register | `0x23` | `v4` |
| `TERM_SCX` | Set Horizontal Cursor Position | Register | Set the horizontal position of the console cursor to the value of a register | `0x24` | `v4` |
| `TERM_SCX` | Set Horizontal Cursor Position | Literal | Set the horizontal position of the console cursor to a literal value | `0x25` | `v4` |
| `TERM_SCX` | Set Horizontal Cursor Position | Address | Set the horizontal position of the console cursor to the value of memory at an address | `0x26` | `v4` |
| `TERM_SCX` | Set Horizontal Cursor Position | Pointer | Set the horizontal position of the console cursor to the value of memory at an address in a register | `0x27` | `v4` |
| `TERM_GCY` | Get Vertical Cursor Position | Register | Set the value of a register to the current vertical position of the console cursor | `0x30` | `v4` |
| `TERM_GCX` | Get Horizontal Cursor Position | Register | Set the value of a register to the current horizontal position of the console cursor | `0x31` | `v4` |
| `TERM_GSY` | Get Vertical Console Size | Register | Set the value of a register to the current vertical size of the console | `0x32` | `v4` |
| `TERM_GSX` | Get Horizontal Console Size | Register | Set the value of a register to the current horizontal size of the console | `0x33` | `v4` |
| **Beep** ||||||
| `TERM_BEP` | Beep | - | Play a beep/bell sound | `0x40` | `v4` |
| **Colours** ||||||
| `TERM_SFC` | Set Foreground Colour | Register | Set the console foreground (text) colour to the value of a register | `0x50` | `v4` |
| `TERM_SFC` | Set Foreground Colour | Literal | Set the console foreground (text) colour to a literal value | `0x51` | `v4` |
| `TERM_SFC` | Set Foreground Colour | Address | Set the console foreground (text) colour to a value in memory at an address | `0x52` | `v4` |
| `TERM_SFC` | Set Foreground Colour | Address | Set the console foreground (text) colour to a value in memory at an address in a register | `0x53` | `v4` |
| `TERM_SBC` | Set Background Colour | Register | Set the console background (text) colour to the value of a register | `0x54` | `v4` |
| `TERM_SBC` | Set Background Colour | Literal | Set the console background (text) colour to a literal value | `0x55` | `v4` |
| `TERM_SBC` | Set Background Colour | Address | Set the console background (text) colour to a value in memory at an address | `0x56` | `v4` |
| `TERM_SBC` | Set Background Colour | Address | Set the console background (text) colour to a value in memory at an address in a register | `0x57` | `v4` |
| `TERM_RSC` | Reset Colours | - | Reset both the console foreground and background colours to their defaults | `0x58` | `v4` |

## ASCII Table

The following is a list of common characters and their corresponding byte value in decimal.

| Code (Dec) | Code (Hex) | Character                 |
|------------|------------|---------------------------|
| 10         | 0A         | LF  (line feed, new line) |
| 13         | 0D         | CR  (carriage return)     |
| 32         | 20         | SPACE                     |
| 33         | 21         | !                         |
| 34         | 22         | "                         |
| 35         | 23         | #                         |
| 36         | 24         | $                         |
| 37         | 25         | %                         |
| 38         | 26         | &                         |
| 39         | 27         | '                         |
| 40         | 28         | (                         |
| 41         | 29         | )                         |
| 42         | 2A         | *                         |
| 43         | 2B         | +                         |
| 44         | 2C         | ,                         |
| 45         | 2D         | -                         |
| 46         | 2E         | .                         |
| 47         | 2F         | /                         |
| 48         | 30         | 0                         |
| 49         | 31         | 1                         |
| 50         | 32         | 2                         |
| 51         | 33         | 3                         |
| 52         | 34         | 4                         |
| 53         | 35         | 5                         |
| 54         | 36         | 6                         |
| 55         | 37         | 7                         |
| 56         | 38         | 8                         |
| 57         | 39         | 9                         |
| 58         | 3A         | :                         |
| 59         | 3B         | ;                         |
| 60         | 3C         | <                         |
| 61         | 3D         | =                         |
| 62         | 3E         | >                         |
| 63         | 3F         | ?                         |
| 64         | 40         | @                         |
| 65         | 41         | A                         |
| 66         | 42         | B                         |
| 67         | 43         | C                         |
| 68         | 44         | D                         |
| 69         | 45         | E                         |
| 70         | 46         | F                         |
| 71         | 47         | G                         |
| 72         | 48         | H                         |
| 73         | 49         | I                         |
| 74         | 4A         | J                         |
| 75         | 4B         | K                         |
| 76         | 4C         | L                         |
| 77         | 4D         | M                         |
| 78         | 4E         | N                         |
| 79         | 4F         | O                         |
| 80         | 50         | P                         |
| 81         | 51         | Q                         |
| 82         | 52         | R                         |
| 83         | 53         | S                         |
| 84         | 54         | T                         |
| 85         | 55         | U                         |
| 86         | 56         | V                         |
| 87         | 57         | W                         |
| 88         | 58         | X                         |
| 89         | 59         | Y                         |
| 90         | 5A         | Z                         |
| 91         | 5B         | [                         |
| 92         | 5C         | \                         |
| 93         | 5D         | ]                         |
| 94         | 5E         | ^                         |
| 95         | 5F         | _                         |
| 96         | 60         | `                         |
| 97         | 61         | a                         |
| 98         | 62         | b                         |
| 99         | 63         | c                         |
| 100        | 64         | d                         |
| 101        | 65         | e                         |
| 102        | 66         | f                         |
| 103        | 67         | g                         |
| 104        | 68         | h                         |
| 105        | 69         | i                         |
| 106        | 6A         | j                         |
| 107        | 6B         | k                         |
| 108        | 6C         | l                         |
| 109        | 6D         | m                         |
| 110        | 6E         | n                         |
| 111        | 6F         | o                         |
| 112        | 70         | p                         |
| 113        | 71         | q                         |
| 114        | 72         | r                         |
| 115        | 73         | s                         |
| 116        | 74         | t                         |
| 117        | 75         | u                         |
| 118        | 76         | v                         |
| 119        | 77         | w                         |
| 120        | 78         | x                         |
| 121        | 79         | y                         |
| 122        | 7A         | z                         |
| 123        | 7B         | {                         |
| 124        | 7C         | \|                        |
| 125        | 7D         | }                         |
| 126        | 7E         | ~                         |

---

**Copyright © 2022–2024  Ptolemy Hill**

**Licensed under CC BY-SA 4.0. To view a copy of this license, visit <http://creativecommons.org/licenses/by-sa/4.0/>**
