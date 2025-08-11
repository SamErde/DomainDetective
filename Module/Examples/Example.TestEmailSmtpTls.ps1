# Clear-Host

Import-Module $PSScriptRoot\..\DomainDetective.psd1 -Force

$Gmail = Test-EmailSmtpTls -HostName 'gmail.com' -Port 25 -Verbose -ErrorAction SilentlyContinue
$Gmail | Format-Table

$Example = Test-EmailSmtpTls -HostName 'mail.example.com' -ShowChain -Verbose -ErrorAction SilentlyContinue
$Example | Format-List
