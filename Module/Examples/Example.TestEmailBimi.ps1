# Clear-Host

Import-Module $PSScriptRoot\..\DomainDetective.psd1 -Force

$Results = Test-EmailBimi -DomainName 'evotec.pl' -Verbose
$Results | Format-Table

$Example = Test-EmailBimi -DomainName 'example.com'
$Example | Format-Table
