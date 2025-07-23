Describe 'Test-Rdap cmdlet' {
    It 'exposes CacheDuration parameter' {
        Import-Module "$PSScriptRoot/../DomainDetective.psd1" -Force
        $command = Get-Command Test-Rdap
        $command.Parameters.Keys | Should -Contain 'CacheDuration'
        [DomainDetective.PowerShell.CmdletTestRdap]::new().CacheDuration |
            Should -Be ([TimeSpan]::FromHours(1))
    }
}
