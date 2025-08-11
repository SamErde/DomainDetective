# Clear-Host

Import-Module $PSScriptRoot\..\DomainDetective.psd1 -Force

$Results = Test-DnsCaa -DomainName 'evotec.pl'
$Results | Format-List
$Results.AnalysisResults | Format-Table

$Google = Test-DnsCaa -DomainName 'google.com'
$Google | Format-List
$Google.AnalysisResults | Format-Table

$VerboseExample = Test-DnsCaa -DomainName 'example.com' -Verbose
$VerboseExample | Format-List
$VerboseExample.AnalysisResults | Format-Table
