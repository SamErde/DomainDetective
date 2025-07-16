describe 'Test-DomainHealth BrandKeyword parameter' {
    It 'exposes BrandKeyword parameter' {
        Import-Module "$PSScriptRoot/../DomainDetective.psd1" -Force
        $command = Get-Command Test-DomainHealth
        $command.Parameters.Keys | Should -Contain 'BrandKeyword'
    }
    It 'has empty BrandKeyword by default' {
        Import-Module "$PSScriptRoot/../DomainDetective.psd1" -Force
        [DomainDetective.PowerShell.CmdletTestDomainHealth]::new().BrandKeyword |
            Should -BeNullOrEmpty
    }
}
