Describe 'Get-RdapObject cmdlet' {
    It 'retrieves domain data' {
        Import-Module "$PSScriptRoot/../DomainDetective.psd1" -Force
        $source = Join-Path $PSScriptRoot 'Data'
        $data = Join-Path $TestDrive 'Data'
        Copy-Item -Path $source -Destination $data -Recurse
        $result = Get-RdapObject -Domain 'example.com' -ServiceEndpoint $data
        Remove-Item -Path $data -Recurse -Force
        $result | Should -Not -BeNullOrEmpty
    }
    It 'retrieves IP data' {
        Import-Module "$PSScriptRoot/../DomainDetective.psd1" -Force
        $source = Join-Path $PSScriptRoot 'Data'
        $data = Join-Path $TestDrive 'Data'
        Copy-Item -Path $source -Destination $data -Recurse
        $result = Get-RdapObject -Ip '192.0.2.1' -ServiceEndpoint $data
        Remove-Item -Path $data -Recurse -Force
        $result | Should -Not -BeNullOrEmpty
    }
}

