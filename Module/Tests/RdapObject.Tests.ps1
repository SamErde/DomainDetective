Describe 'Get-RdapObject cmdlet' {
    It 'retrieves domain data' {
        Import-Module "$PSScriptRoot/../DomainDetective.psd1" -Force
        $data = Join-Path $PSScriptRoot 'Data'
        $result = Get-RdapObject -Domain 'example.com' -ServiceEndpoint $data
        $result | Should -Not -BeNullOrEmpty
    }
    It 'retrieves IP data' {
        Import-Module "$PSScriptRoot/../DomainDetective.psd1" -Force
        $data = Join-Path $PSScriptRoot 'Data'
        $result = Get-RdapObject -Ip '192.0.2.1' -ServiceEndpoint $data
        $result | Should -Not -BeNullOrEmpty
    }
}

