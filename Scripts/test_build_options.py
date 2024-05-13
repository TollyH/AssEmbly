"""
This script tests each possible combination of AssEmbly compilation options
to ensure that none of them result in build errors.
"""
import itertools
import os
import subprocess
import sys

import tqdm

BUILD_ARGS = [
    "dotnet", "build", "AssEmbly.csproj", "-c", "Release",
    "-p:TreatWarningsAsErrors=true", "-warnaserror"
]

EXTENSION_SET_OPTIONS = [
    "V1_CALL_STACK_COMPAT",
    "EXTENSION_SET_SIGNED",
    "EXTENSION_SET_FLOATING_POINT",
    "EXTENSION_SET_EXTENDED_BASE",
    "GZIP_COMPRESSION",
    "EXTENSION_SET_EXTERNAL_ASM",
    "EXTENSION_SET_HEAP_ALLOCATE",
    "EXTENSION_SET_FILE_SYSTEM",
    "EXTENSION_SET_TERMINAL",
]

OPERATION_OPTIONS: dict[str, set[str]] = {
    "ASSEMBLER": set(),
    "ASSEMBLER_WARNINGS": {"ASSEMBLER"},
    "PROCESSOR": set(),
    "DEBUGGER": {"ASSEMBLER", "PROCESSOR", "DISASSEMBLER", "CLI"},
    "DISASSEMBLER": set(),
    "CLI": set(),
}

ALL_EXTENSION_SETS = ';' + ';'.join(EXTENSION_SET_OPTIONS) + ';'
ALL_OPERATIONS = ';' + ';'.join(OPERATION_OPTIONS) + ';'

# Change directory to AssEmbly repository root
os.chdir(os.path.dirname(os.path.dirname(__file__)))

print("Testing extension set combinations...")

combinations: list[tuple[str, ...]] = []
# Extension sets are tested alone and in pairs
for length in range(3):
    for combination in itertools.combinations(EXTENSION_SET_OPTIONS, length):
        combinations.append(combination)

failed_extension_set_combinations: list[str] = []
for combination in tqdm.tqdm(combinations):
    combination_str = ';'.join(combination)
    build_process = subprocess.Popen(
        BUILD_ARGS
        + [f"-p:DefineConstants=\"{ALL_OPERATIONS}{combination_str};\""],
        stdout=subprocess.DEVNULL
    )
    if build_process.wait() != 0:
        failed_extension_set_combinations.append(combination_str)

print("Testing operation combinations...")

combinations: list[tuple[str, ...]] = []
# Every possible combination of operations is tested
for length in range(len(OPERATION_OPTIONS) + 1):
    for combination in itertools.combinations(OPERATION_OPTIONS, length):
        valid = True
        for operation in combination:
            # Skip combinations of operations where one or more operations
            # are missing any of their required operations.
            if not OPERATION_OPTIONS[operation].issubset(combination):
                valid = False
                break
        if valid:
            combinations.append(combination)

failed_operation_combinations: list[str] = []
for combination in tqdm.tqdm(combinations):
    combination_str = ';'.join(combination)
    build_process = subprocess.Popen(
        BUILD_ARGS
        + [f"-p:DefineConstants=\"{ALL_EXTENSION_SETS}{combination_str};\""],
        stdout=subprocess.DEVNULL
    )
    if build_process.wait() != 0:
        failed_operation_combinations.append(combination_str)

# Output results
exit_code = 0

print()

if len(failed_extension_set_combinations) > 0:
    exit_code += 1
    print("Failed extension set combinations:")
    for combination_str in failed_extension_set_combinations:
        print(f"    {combination_str}")
else:
    print("All extension set combinations passed")

print()

if len(failed_operation_combinations) > 0:
    exit_code += 1
    print("Failed operation combinations:")
    for combination_str in failed_operation_combinations:
        print(f"    {combination_str}")
else:
    print("All operation combinations passed")

sys.exit(exit_code)
