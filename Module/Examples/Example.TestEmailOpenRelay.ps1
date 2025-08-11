# Clear-Host

Import-Module $PSScriptRoot\..\DomainDetective.psd1 -Force

$Gmail = Test-EmailOpenRelay -HostName 'gmail-smtp-in.l.google.com' -Port 25 -Verbose -ErrorAction SilentlyContinue
$Gmail | Format-Table

$Example = Test-EmailOpenRelay -HostName 'gmail-smtp-in.l.google.com' -Port 25 -Verbose -ErrorAction SilentlyContinue
$Example | Format-Table

$Example = Test-EmailOpenRelay -HostName 'gmail-smtp-in.l.google.com' -ErrorAction SilentlyContinue
$Example | Format-Table
