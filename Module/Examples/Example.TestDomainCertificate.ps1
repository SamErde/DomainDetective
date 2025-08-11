# Clear-Host

Import-Module $PSScriptRoot\..\DomainDetective.psd1 -Force

$Cert = Test-DomainCertificate -Url 'https://evotec.pl' -Verbose
$Cert | Format-List

$Example = Test-DomainCertificate -Url 'https://example.com' -ShowChain
$Example | Format-List
