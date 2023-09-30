# Does NOT build the grammar diagrams.
# ReferenceManual.md is the source of the documentation,
# edits to the other files (txt, html, docx) will be overwritten.
# Requires pandoc to be installed and on PATH
pandoc -t plain -s .\Documentation\ReferenceManual\ReferenceManual.md -o .\Documentation\ReferenceManual\ReferenceManual.txt
pandoc -s .\Documentation\ReferenceManual\ReferenceManual.md -o .\Documentation\ReferenceManual\ReferenceManual.html --metadata pagetitle="AssEmbly Reference Manual"
pandoc -s .\Documentation\ReferenceManual\ReferenceManual.md -o .\Documentation\ReferenceManual\ReferenceManual.docx
