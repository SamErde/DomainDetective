Describe 'Test-DDDnsForwardReverse cmdlet' {
    It 'executes and returns data' {
        Import-Module "$PSScriptRoot/../DomainDetective.psd1" -Force
        $result = Test-DDDnsForwardReverse -DomainName 'example.com' -DnsEndpoint System
        $result | Should -Not -BeNullOrEmpty
    }
    It 'throws if DomainName is empty' {
        Import-Module "$PSScriptRoot/../DomainDetective.psd1" -Force
        { Test-DDDnsForwardReverse -DomainName '' } | Should -Throw
    }
}
