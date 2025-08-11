# Clear-Host

Import-Module $PSScriptRoot\..\DomainDetective.psd1 -Force

$Results = Test-DnsNs -DomainName 'google.com' -Verbose
$Results | Format-Table
$Results | Format-List

$Cloudflare = Test-DnsNs -DomainName 'example.com' -DnsEndpoint System
$Cloudflare | Format-Table

$Evotec = Test-DnsNs -DomainName 'evotec.pl' -Verbose
$Evotec | Format-Table
