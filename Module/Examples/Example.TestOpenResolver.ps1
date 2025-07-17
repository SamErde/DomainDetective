# Clear-Host

Import-Module $PSScriptRoot\..\DomainDetective.psd1 -Force

$Google = Test-OpenResolver -Server '8.8.8.8' -Port 53 -Verbose
$Google | Format-Table

$Example = Test-OpenResolver -Server '1.1.1.1'
$Example | Format-Table
