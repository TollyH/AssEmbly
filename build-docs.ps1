# Requires pandoc to be installed and on PATH

$docsFolder = ".\Documentation\ReferenceManual"
$buildFolder = "$docsFolder\Build"
$sourceFile = "$docsFolder\ReferenceManual.md"

Write-Output "`nDeleting existing documentation Build folder..."
if (Test-Path $buildFolder) {
    Remove-Item $buildFolder -Recurse -Force
}

New-Item -Path $buildFolder -ItemType Directory
Write-Output "    Building .txt..."
pandoc -t plain -s $sourceFile --tab-stop=2 -o $buildFolder\ReferenceManual.txt
Write-Output "    Building .html..."
pandoc -s $sourceFile --highlight-style=$docsFolder\pandoc.theme --syntax-definition=$docsFolder\AssEmblySyntax.xml --toc --toc-depth=6 --css=$docsFolder\ReferenceManual.css --embed-resource -H $docsFolder\ReferenceManual.HtmlHeader.html -o $buildFolder\ReferenceManual.html --metadata pagetitle="AssEmbly Reference Manual"
Write-Output "    Building .docx..."
pandoc -s $sourceFile --highlight-style=$docsFolder\pandoc.theme --syntax-definition=$docsFolder\AssEmblySyntax.xml -o $buildFolder\ReferenceManual.docx

Write-Output "    Building .pdf..."
$word_app = New-Object -ComObject Word.Application
$document = $word_app.Documents.Open([IO.Path]::GetFullPath("$buildFolder\ReferenceManual.docx"))
$pdf_filename = [IO.Path]::GetFullPath("$buildFolder\ReferenceManual.pdf")
# 17 = wdFormatPDF
$document.SaveAs([ref] $pdf_filename, [ref] 17)
$document.Close()
$word_app.Quit()
[System.Runtime.Interopservices.Marshal]::ReleaseComObject($word_app)
Remove-Variable word_app
