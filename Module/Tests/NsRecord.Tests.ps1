Describe 'Test-DDDnsNsRecord cmdlet' {
    It 'executes and returns data' {
        Import-Module "$PSScriptRoot/../DomainDetective.psd1" -Force
        $result = Test-DDDnsNsRecord -DomainName 'example.com' -DnsEndpoint System
        $result | Should -Not -BeNullOrEmpty
    }
    It 'throws if DomainName is empty' {
        Import-Module "$PSScriptRoot/../DomainDetective.psd1" -Force
        { Test-DDDnsNsRecord -DomainName '' } | Should -Throw
    }
}

