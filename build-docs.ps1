# ReferenceManual.md is the source of the documentation,
# edits to the other files (txt, html, docx, pdf) will be overwritten.
# Requires pandoc to be installed and on PATH
Write-Output "    Building .txt..."
pandoc -t plain -s .\Documentation\ReferenceManual\ReferenceManual.md -o .\Documentation\ReferenceManual\ReferenceManual.txt
Write-Output "    Building .html..."
pandoc -s .\Documentation\ReferenceManual\ReferenceManual.md --highlight-style breezedark  -o .\Documentation\ReferenceManual\ReferenceManual.html --metadata pagetitle="AssEmbly Reference Manual"
Write-Output "    Building .docx..."
pandoc -s .\Documentation\ReferenceManual\ReferenceManual.md --highlight-style breezedark -o .\Documentation\ReferenceManual\ReferenceManual.docx

Write-Output "    Building .pdf..."
$word_app = New-Object -ComObject Word.Application
$document = $word_app.Documents.Open([IO.Path]::GetFullPath(".\Documentation\ReferenceManual\ReferenceManual.docx"))
$pdf_filename = [IO.Path]::GetFullPath(".\Documentation\ReferenceManual\ReferenceManual.pdf")
# 17 = wdFormatPDF
$document.SaveAs([ref] $pdf_filename, [ref] 17)
$document.Close()
$word_app.Quit()
[System.Runtime.Interopservices.Marshal]::ReleaseComObject($word_app)
Remove-Variable word_app
