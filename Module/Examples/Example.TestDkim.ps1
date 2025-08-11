# Clear-Host

Import-Module $PSScriptRoot\..\DomainDetective.psd1 -Force

$Results = Test-EmailDkim -DomainName 'evotec.pl' -Verbose -Selectors "selector1", "selector2"
$Results | Format-Table
$Results | Format-List
