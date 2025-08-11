Describe 'Test-EmailAutoDiscover alias' {
    It 'executes and returns data' {
        Import-Module "$PSScriptRoot/../DomainDetective.psd1" -Force
        $result = Test-EmailAutoDiscover -DomainName 'example.com' -DnsEndpoint System -IncludeEndpoints -ErrorAction SilentlyContinue
        $result | Should -Not -BeNullOrEmpty
        ($result | Measure-Object).Count | Should -BeGreaterThan 1
    }
    It 'throws if DomainName is empty' {
        Import-Module "$PSScriptRoot/../DomainDetective.psd1" -Force
        { Test-EmailAutoDiscover -DomainName '' } | Should -Throw
}
}
