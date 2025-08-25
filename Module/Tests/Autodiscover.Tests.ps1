Describe 'Test-DDEmailAutoDiscover cmdlet' {
    It 'executes and returns data' {
        Import-Module "$PSScriptRoot/../DomainDetective.psd1" -Force
        $result = Test-DDEmailAutoDiscover -DomainName 'example.com' -DnsEndpoint System -IncludeEndpoints -ErrorAction SilentlyContinue
        $result | Should -Not -BeNullOrEmpty
        ($result | Measure-Object).Count | Should -BeGreaterThan 1
    }
    It 'throws if DomainName is empty' {
        Import-Module "$PSScriptRoot/../DomainDetective.psd1" -Force
        { Test-DDEmailAutoDiscover -DomainName '' } | Should -Throw
}
}
