# Clear-Host

Import-Module $PSScriptRoot\..\DomainDetective.psd1 -Force

$Result = Get-SearchEngineInfo -Query 'Domain Detective' -GoogleApiKey 'YOUR_KEY' -GoogleCx 'ENGINE_ID'
$Result
