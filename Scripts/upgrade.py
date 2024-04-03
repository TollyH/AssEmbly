"""
This script upgrades an AssEmbly program written for an older version of the
language to one that is valid for a newer version of the language.

Usage: upgrade.py <src_file_path> <dst_file_path> <src_version> <dst_version>
       Versions are in the format major.minor (patch version not included)
"""
import re
import sys


def escape_backslashes(program: str) -> str:
    """
    Escape backslashes in strings so they are not interpreted as
    escape sequences.
    """
    lines = program.splitlines(True)
    for i, line in enumerate(lines):
        chars = list(line)
        index = 0
        while (index := line.find('\\', index + 1)) != -1:
            if line.find(';', 0, index) != -1:
                # This is a comment, don't replace
                break
            if len(re.findall(r'(?<!\\)"', line[:index])) != 1:
                # This isn't a string, don't replace
                continue
            if index + 1 < len(line) and line[index + 1] == '"':
                # This is an escaped quote, don't replace
                continue
            chars[index] = "\\\\"
        lines[i] = "".join(chars)
    return "".join(lines)


def escape_at_signs(program: str) -> str:
    """
    Escape @ signs in strings so they are not interpreted as variable names.
    """
    lines = program.splitlines(True)
    for i, line in enumerate(lines):
        chars = list(line)
        index = 0
        while (index := line.find('@', index + 1)) != -1:
            if line.find(';', 0, index) != -1:
                # This is a comment, don't replace
                break
            if line.count('"', 0, index) != 1:
                # This isn't a string, don't replace
                continue
            chars[index] = "\\@"
        lines[i] = "".join(chars)
    return ''.join(lines)


def replace_directives(program: str) -> str:
    """
    Replace instances of pre-3.2.0 directives (e.g. DAT, MAC) with their
    updated equivalents (%DAT, %MAC, etc).
    """
    program = re.sub(
        r"^( *)(DAT|PAD|NUM|IBF|IMP|MAC|ANALYZER|MESSAGE|DEBUG)",
        r"\1%\2",
        program,
        flags=re.IGNORECASE | re.MULTILINE
    )
    return re.sub(
        r"^( *)%MAC(?= |$)",
        r"\1%MACRO",
        program,
        flags=re.IGNORECASE | re.MULTILINE
    )


UPGRADES = {
    (1, 1): [escape_backslashes],
    (3, 2): [escape_at_signs, replace_directives]
}


def upgrade_program(program: str,
                    old_version: tuple[int, int],
                    new_version: tuple[int, int]
                    ) -> tuple[str, list[tuple[str, tuple[int, int]]]]:
    """
    Apply upgrades to a program based on a given source and target version.
    Returns the updated program and the list of all applied upgrades in the
    form (upgrade name, upgrade version).
    """
    applied_upgrades: list[tuple[str, tuple[int, int]]] = []
    for version, upgrade_list in UPGRADES.items():
        if old_version < version <= new_version:
            for upgrade_func in upgrade_list:
                program = upgrade_func(program)
                applied_upgrades.append((upgrade_func.__name__, version))
    return program, applied_upgrades


def main():
    if len(sys.argv) != 5:
        print(
            "Usage: upgrade.py <src_file_path> <dst_file_path>"
            + " <src_version> <dst_version>"
        )
        sys.exit(1)

    with open(sys.argv[1], encoding="utf8") as file:
        src_program = file.read()

    src_split = sys.argv[3].split('.', 1)
    src_version = (int(src_split[0]), int(src_split[1]))
    dst_split = sys.argv[4].split('.', 1)
    dst_version = (int(dst_split[0]), int(dst_split[1]))

    new_program, applied_upgrades = upgrade_program(
        src_program, src_version, dst_version)

    if len(applied_upgrades) == 0:
        print(
            "There are no upgrades required between version"
            + f" {sys.argv[3]} and {sys.argv[4]}"
        )
        return

    with open(sys.argv[2], 'w', encoding="utf8") as file:
        file.write(new_program)

    print(f"Applied {len(applied_upgrades)} upgrade(s):")
    for upgrade_name, upgrade_version in applied_upgrades:
        print(
            f"    {upgrade_name}"
            + f" (for v{upgrade_version[0]}.{upgrade_version[1]})"
        )


if __name__ == "__main__":
    main()
