# AAP File Format Version 1

This document describes version `1` of the AAP file format, used by all versions of AssEmbly from `2.0.0` onwards.

## Table of Contents

- [AAP File Format Version 1](#aap-file-format-version-1)
  - [Table of Contents](#table-of-contents)
  - [File Layout](#file-layout)
  - [Feature Flags](#feature-flags)
    - [Feature Availability Table](#feature-availability-table)
  - [AAP Format Version History](#aap-format-version-history)

## File Layout

All numerical values are encoded in little endian. The header of an AAP file is currently always `36` bytes long.

| Offset | Size        | Type      | Name           | Description                                                                                                                       |
|--------|-------------|-----------|----------------|-----------------------------------------------------------------------------------------------------------------------------------|
| `0x00` | `0x08`      | `uint8[]` | File signature | Identifies the version of the AAP file. Will be the bytes `41 73 73 45 6D 62 6C 79` (`AssEmbly` encoded in ASCII) for Version 1   |
| `0x08` | `0x04`      | `int32`   | Major version  | The major version of AssEmbly that was used to assemble the file (the first digit in a version formatted as `x.x.x`)              |
| `0x0C` | `0x04`      | `int32`   | Minor version  | The minor version of AssEmbly that was used to assemble the file (the second digit in a version formatted as `x.x.x`)             |
| `0x10` | `0x04`      | `int32`   | Patch version  | The patch version of AssEmbly that was used to assemble the file (the third digit in a version formatted as `x.x.x`)              |
| `0x14` | `0x08`      | `uint64`  | Feature flags  | A 64-bit long bit field that identifies which features the program uses. The meaning of each bit is explained in the next section |
| `0x1C` | `0x08`      | `uint64`  | Entry point    | The address in memory that the processor should start execution from (i.e. the initial value of the `rpo` register)               |
| `0x24` | Until `EOF` | `uint8[]` | Program bytes  | The (optionally compressed) bytes to be copied to processor memory                                                                |

## Feature Flags

Every AAP file contains 64-bits of feature flags that identify which features a given file uses. A `1` indicates that the feature is used, a `0` indicates that it is not. From least-to-most significant bit, they correspond to the following:

| Bit | Meaning                                                                                         |
|-----|-------------------------------------------------------------------------------------------------|
| 0   | Program requests the use of the "3 registers pushed" calling convention from AssEmbly version 1 |
| 1   | Program uses instructions from the Signed Extension Set                                         |
| 2   | Program uses instructions from the Floating Point Extension Set                                 |
| 3   | Program uses instructions from the Extended Base Set                                            |
| 4   | The program bytes are compressed with GZip compression                                          |
| 5   | Program uses instructions from the External Assembly Extension Set                              |
| 6   | Program uses instructions from the Memory Allocation Extension Set                              |

The remaining bits are currently unused, and should always be set to `0`.

### Feature Availability Table

This table charts the availability of each feature flag on each major version of AssEmbly. Minor and patch versions never introduce new AAP feature flags, so have been omitted.

> [!WARNING]
> Additional instructions may be added to existing extension sets by new versions. A version having a tick below only indicates that the extension set existed in some form within that version, not that it necessarily supports *all* instructions currently in the set. See the reference manual for a full list of instructions and what AssEmbly version they were added in.

| Feature                                      | v1 | v2 | v3 |
|----------------------------------------------|----|----|----|
| Choice between v1 and v2+ calling convention | ❌ | ✔️ | ✔️ |
| Signed Extension Set                         | ❌ | ✔️ | ✔️ |
| Floating Point Extension Set                 | ❌ | ✔️ | ✔️ |
| Extended Base Set                            | ❌ | ✔️ | ✔️ |
| GZip Compression                             | ❌ | ❌ | ✔️ |
| External Assembly Extension Set              | ❌ | ❌ | ✔️ |
| Memory Allocation Extension Set              | ❌ | ❌ | ✔️ |

## AAP Format Version History

The format version of AAP files can be determined by the initial file signature, which will change between versions.

> [!IMPORTANT]
> AAP format versions are separate from AssEmbly versions. Additional feature flags can be added by new versions of AssEmbly without a change in the AAP format version. For instance, program compression is only supported in version 3 of AssEmbly and onwards, however it can still be used in version 1 of the AAP format, as it does not affect the file header.

| Format version | File Signature                         | Header size (bytes) | Minimum AssEmbly Version | Changes                                                                        |
|----------------|----------------------------------------|---------------------|--------------------------|--------------------------------------------------------------------------------|
| `0`            | *(none)*                               | `0`                 | `pre-1.0.0`              | -                                                                              |
| `1`            | `41 73 73 45 6D 62 6C 79` (`AssEmbly`) | `36`                | `2.0.0`                  | Introduced file header with signature, version, feature flags, and entry point |

> [!NOTE]
> Format version `0` is undocumented, as it was simply a raw container of the assembled program bytes with no additional header.

---

**Copyright © 2022–2024  Ptolemy Hill**

**Licensed under CC BY-SA 4.0. To view a copy of this license, visit <http://creativecommons.org/licenses/by-sa/4.0/>**
