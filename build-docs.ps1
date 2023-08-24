# Language.md is the source of the documentation,
# edits to the other files (txt, html, docx) will be overwritten.
# Requires pandoc to be installed and on PATH
pandoc -t plain -s .\Documentation\Language.md -o .\Documentation\Language.txt
pandoc -s .\Documentation\Language.md -o .\Documentation\Language.html
pandoc -s .\Documentation\Language.md -o .\Documentation\Language.docx
