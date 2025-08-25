Describe 'Get-DomainFlattenedSpfIp cmdlet' {
    It 'executes and returns analysis data' {
        Import-Module "$PSScriptRoot/../DomainDetective.psd1" -Force
        $result = Get-DomainFlattenedSpfIp -DomainName 'example.com' -DnsEndpoint System -TestSpfRecord 'v=spf1 ip4:192.0.2.10 ip4:192.0.2.10 -all'
        $result.UniqueIps | Should -Contain '192.0.2.10'
        $result.DuplicateIps | Should -Contain '192.0.2.10'
        $result.TokenIpMap.'ip4:192.0.2.10' | Should -Contain '192.0.2.10'
    }
    It 'throws if DomainName is empty' {
        Import-Module "$PSScriptRoot/../DomainDetective.psd1" -Force
        { Get-DomainFlattenedSpfIp -DomainName '' } | Should -Throw
}
}
