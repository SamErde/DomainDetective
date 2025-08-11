# Clear-Host

Import-Module $PSScriptRoot\..\DomainDetective.psd1 -Force

$Results = Test-TlsDane -DomainName 'evotec.pl' -Verbose
$Results | Format-List

$Results = Test-TlsDane -DomainName 'ietf.org' -Verbose
$Results | Format-List

