# Clear-Host

Import-Module $PSScriptRoot\..\DomainDetective.psd1 -Force

$Gmail = Test-EmailStartTls -DomainName 'gmail.com' -Verbose
$Gmail | Format-Table

$Evotec = Test-EmailStartTls -DomainName 'evotec.pl' -Port 25
$Evotec | Format-Table

$Example = Test-EmailStartTls -DomainName 'example.com' -DnsEndpoint System -Port 587
$Example | Format-Table

# Test a single host on a custom port
$HostTls = Test-EmailStartTls -DomainName 'example.com' -Port 2525
$HostTls | Format-Table
