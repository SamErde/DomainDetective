describe 'Get-SearchEngineInfo cmdlet' {
    It 'exposes Engine parameter' {
        Import-Module "$PSScriptRoot/../DomainDetective.psd1" -Force
        $command = Get-Command Get-SearchEngineInfo
        $command.Parameters.Keys | Should -Contain 'Engine'
        [DomainDetective.PowerShell.CmdletGetSearchEngineInfo]::new().Engine | Should -Be 'google'
    }
    It 'throws when Query is empty' {
        Import-Module "$PSScriptRoot/../DomainDetective.psd1" -Force
        { Get-SearchEngineInfo -Query '' } | Should -Throw
    }

    It 'registers alias' {
        Import-Module "$PSScriptRoot/../DomainDetective.psd1" -Force
        (Get-Alias Get-SearchEngineInfo).Name | Should -Be 'Get-SearchEngineInfo'
    }
}
