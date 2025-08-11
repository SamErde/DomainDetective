# Clear-Host

Import-Module $PSScriptRoot\..\DomainDetective.psd1 -Force

$Soa = Test-DnsSoa -DomainName 'evotec.pl' -Verbose
$Soa | Format-Table
$Soa | Format-List

$Example = Test-DnsSoa -DomainName 'example.com'
$Example | Format-Table
