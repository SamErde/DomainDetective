# Clear-Host

Import-Module $PSScriptRoot\..\DomainDetective.psd1 -Force

$result = Test-EmailSpf -DomainName 'github.com' -Verbose
$analysis = $result.FlattenedIpAnalysis
$analysis
