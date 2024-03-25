# ADI File Format Version 1.0

This document describes version `1.0` of the ADI file format, used by all versions of AssEmbly from `3.2.0` onwards.

## Table of Contents

- [ADI File Format Version 1.0](#adi-file-format-version-10)
  - [Table of Contents](#table-of-contents)
  - [File Layout](#file-layout)
    - [Section 1 - Assembled Instructions](#section-1---assembled-instructions)
    - [Section 2 - Address Labels](#section-2---address-labels)
    - [Section 3 - Resolved Imports](#section-3---resolved-imports)
    - [Section 4 - File and Line Mapping](#section-4---file-and-line-mapping)
  - [ADI Format Version History](#adi-format-version-history)

## File Layout

ADI files are UTF-8 encoded and follow a strict standard layout. Newlines should be represented with a single `LF` (`0x0A`) byte on all platforms.

The general layout of the file is as follows:

```text
AssEmbly Debug Information File
Format Version: 1.0
Date: <YYYY-MM-DD> <HH:MM:SS>
Command Line: <command line - for reference only and can be any text>
Total Program Size: <size excluding AAP header> bytes
===============================================================================

[1]: Assembled Instructions
===============================================================================
<...>
===============================================================================

[2]: Address Labels
===============================================================================
<...>
===============================================================================

[3]: Resolved Imports
===============================================================================
<...>
===============================================================================

[4]: File and Line Mapping
===============================================================================
<...>
===============================================================================
```

Anything not surrounded by `<` and `>` must be copied verbatim. Ellipsis (`<...>`) designate where the contents of each section should be located. Sections can be empty, and if they are, there should be **no** blank line between the start and end of the section. Lines within the sections **may** exceed the length of the equals sign (`=`) bars. The exact format of each section is described in the below sections of the document.

> [!IMPORTANT]
> There must be no leading or trailing whitespace on any line, and there should **not** be a newline character at the end of the final line. There is no leeway for additional lines between sections, each section must be separated by a **single** blank line.

An ADI file for a blank program would look like this - with the date and command line being the only things that could change:

```text
AssEmbly Debug Information File
Format Version: 1.0
Date: 2024-03-24 19:57:26
Command Line: C:\path\to\AssEmbly.exe assemble "C:\path\to\program.asm"
Total Program Size: 0 bytes
===============================================================================

[1]: Assembled Instructions
===============================================================================
===============================================================================

[2]: Address Labels
===============================================================================
===============================================================================

[3]: Resolved Imports
===============================================================================
===============================================================================

[4]: File and Line Mapping
===============================================================================
===============================================================================
```

### Section 1 - Assembled Instructions

Section 1 maps program addresses to the text of the source code line that they were assembled from. The address is encoded as a 0-indexed 16-digit uppercase hexadecimal number with no prefix, and the source line can be any raw text (as long as it does not contain any newline characters). The address and the source line are separated by an `@` sign with a single space on either side. Each entry is stored on its own line, and there should be no blank line between the final entry and the end of the section. Each address must be unique.

For example:

```text
[1]: Assembled Instructions
===============================================================================
0000000000000000 @ HEAP_ALC rg0, @MAX_PATH
000000000000000C @ HEAP_ALC rg1, @MAX_PATH
0000000000000018 @ CAL :FUNC_PRINT, :&STR_QOI_PATH_PROMPT
0000000000000029 @ CAL :FUNC_INPUT, rg0
0000000000000033 @ CAL :FUNC_PRINT, :&STR_BMP_PATH_PROMPT
0000000000000044 @ CAL :FUNC_INPUT, rg1
000000000000004E @ CAL :FUNC_QOI_DECODE_FILE, rg0
===============================================================================
```

### Section 2 - Address Labels

Section 2 maps addresses to the names of all labels that point to that address. The address is encoded as a 0-indexed 16-digit uppercase hexadecimal number with no prefix, and the label names are separated by commas (`,`) with no surrounding whitespace. The address and the label names are separated by an `@` sign with a single space on either side. Each entry is stored on its own line, and there should be no blank line between the final entry and the end of the section. Each address must be unique.

For example:

```text
[2]: Address Labels
===============================================================================
0000000000000000 @ FUNC_INPUT
000000000000000C @ FUNC_INPUT_READ
000000000000002F @ FUNC_INPUT_RETURN,FUNC_INPUT_END
===============================================================================
```

### Section 3 - Resolved Imports

Section 3 maps addresses to the names of imported files that started assembly at that address, along with the fully qualified path to that file. The address is encoded as a 0-indexed 16-digit uppercase hexadecimal number with no prefix. The file names are literal text strings, optionally surrounded by double quotes (`"`), separated with the `->` characters. The separating characters are surrounded by a single space on either side. The address and the file names are separated by an `@` sign with a single space on either side. Each entry is stored on its own line, and there should be no blank line between the final entry and the end of the section. Each address must be unique, so if multiple imports began on a single address, only one should be inserted. There is no requirement for which import is chosen.

For example:

```text
[3]: Resolved Imports
===============================================================================
000000000000019A @ "input.ext.asm" -> "C:\path\to\input.ext.asm"
00000000000001CC @ "print.ext.asm" -> "C:\path\to\print.ext.asm"
===============================================================================
```

### Section 4 - File and Line Mapping

Section 4 maps addresses to the line number and file path that they were assembled from. The address is encoded as a 0-indexed 16-digit uppercase hexadecimal number with no prefix. The line number is encoded as a 1-indexed denary number with an arbitrary number of digits, always followed immediately by a colon `:`. The file path is then omitted if the line was from the initial (base) file, or inserted as the fully qualified literal text of the path if it was from an imported file. The address and the line number are separated by an `@` sign with a single space on either side. Each entry is stored on its own line, and there should be no blank line between the final entry and the end of the section. Each address must be unique.

For example:

```text
[4]: File and Line Mapping
===============================================================================
0000000000000000 @ 4:
0000000000000011 @ 7:
0000000000000022 @ 8:
000000000000002C @ 9:
000000000000002F @ 10:
0000000000000038 @ 11:
0000000000000049 @ 12:
000000000000019A @ 7:C:\path\to\input.ext.asm
000000000000019C @ 9:C:\path\to\input.ext.asm
000000000000019E @ 10:C:\path\to\input.ext.asm
00000000000001A8 @ 11:C:\path\to\input.ext.asm
00000000000001B1 @ 12:C:\path\to\input.ext.asm
00000000000001CC @ 8:C:\path\to\print.ext.asm
00000000000001CE @ 10:C:\path\to\print.ext.asm
00000000000001D1 @ 11:C:\path\to\print.ext.asm
00000000000001D4 @ 12:C:\path\to\print.ext.asm
00000000000001DD @ 13:C:\path\to\print.ext.asm
===============================================================================
```

## ADI Format Version History

The format version of ADI files is contained in plain-text in the file header.

| Format version | First AssEmbly Version | Last AssEmbly Version | Changes                                                                    |
|----------------|------------------------|-----------------------|----------------------------------------------------------------------------|
| `0.1`          | `pre-1.0.0`            | `pre-1.0.0`           | -                                                                          |
| `0.2`          | `pre-1.0.0`            | `3.1.0`               | Added the address that each import starts on in section 3                  |
| `1.0`          | `3.2.0`                | *current*             | Added a fourth section mapping addresses to a line number in a source file |

---

**Copyright © 2022–2024  Ptolemy Hill**

**Licensed under CC BY-SA 4.0. To view a copy of this license, visit <http://creativecommons.org/licenses/by-sa/4.0/>**
