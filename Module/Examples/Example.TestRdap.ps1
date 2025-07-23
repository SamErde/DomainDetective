# Clear-Host

Import-Module $PSScriptRoot\..\DomainDetective.psd1 -Force

$rdap = Test-Rdap -DomainName 'example.com' -CacheDuration '00:10:00'
$rdap | Format-List
