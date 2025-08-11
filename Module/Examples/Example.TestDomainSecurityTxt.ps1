# Clear-Host

Import-Module $PSScriptRoot\..\DomainDetective.psd1 -Force

$Results = Test-DomainSecurityTxt -DomainName 'google.com' -Verbose
$Results | Format-Table
$Results | Format-List

$Github = Test-DomainSecurityTxt -DomainName 'github.com'
$Github | Format-Table

$Evotec = Test-DomainSecurityTxt -DomainName 'evotec.pl'
$Evotec | Format-Table

