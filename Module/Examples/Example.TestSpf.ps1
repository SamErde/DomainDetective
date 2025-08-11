# Clear-Host

Import-Module $PSScriptRoot\..\DomainDetective.psd1 -Force

$Results = Test-EmailSpf -DomainName 'google.com' -Verbose
$Results | Format-Table

$ResultsMicrosoft = Test-EmailSpf -DomainName 'microsoft.com' -Verbose
$ResultsMicrosoft | Format-Table
$ResultsMicrosoft | Format-List

$ResultsEvotec = Test-EmailSpf -DomainName 'evotec.pl' -Verbose
$ResultsEvotec | Format-Table
$ResultsEvotec | Format-List

$ResultsIdn = Test-EmailSpf -DomainName 'xn--bcher-kva.ch' -Verbose
$ResultsIdn | Format-Table
$ResultsIdn | Format-List
