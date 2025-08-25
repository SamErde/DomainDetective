# Clear-Host

Import-Module $PSScriptRoot\..\DomainDetective.psd1 -Force

$analysis = Get-DomainFlattenedSpfIp -DomainName 'github.com' -Verbose
$analysis
