describe 'Get-DDSearchEngineInfo cmdlet' {
    It 'exposes Engine parameter' {
        Import-Module "$PSScriptRoot/../DomainDetective.psd1" -Force
        $command = Get-Command Get-DDSearchEngineInfo
        $command.Parameters.Keys | Should -Contain 'Engine'
        [DomainDetective.PowerShell.CmdletGetSearchEngineInfo]::new().Engine | Should -Be 'google'
    }
    It 'throws when Query is empty' {
        Import-Module "$PSScriptRoot/../DomainDetective.psd1" -Force
        { Get-DDSearchEngineInfo -Query '' } | Should -Throw
    }

    It 'registers alias mapping to primary cmdlet' {
        Import-Module "$PSScriptRoot/../DomainDetective.psd1" -Force
        (Get-Alias Get-SearchEngineInfo).Definition | Should -Be 'Get-DDSearchEngineInfo'
    }
}
