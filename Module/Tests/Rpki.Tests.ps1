Describe 'Test-DDRpki cmdlet' {
    It 'executes without error' {
        Import-Module "$PSScriptRoot/../DomainDetective.psd1" -Force
        { Test-DDRpki -DomainName 'example.com' -DnsEndpoint System } | Should -Not -Throw
    }
    It 'throws if DomainName is empty' {
        Import-Module "$PSScriptRoot/../DomainDetective.psd1" -Force
        { Test-DDRpki -DomainName '' } | Should -Throw
}
}
