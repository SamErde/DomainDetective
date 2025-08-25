Describe 'Add-DDDnsblProvider cmdlet' {
    It 'executes and returns analysis' {
        Import-Module "$PSScriptRoot/../DomainDetective.psd1" -Force
        $result = Add-DDDnsblProvider -Domain 'dnsbl.example.com' -Comment 'test'
        $result | Should -Not -BeNullOrEmpty
    }
    It 'throws if Domain is empty' {
        Import-Module "$PSScriptRoot/../DomainDetective.psd1" -Force
        { Add-DDDnsblProvider -Domain '' } | Should -Throw
    }
}

Describe 'Remove-DDDnsblProvider cmdlet' {
    It 'executes and returns analysis' {
        Import-Module "$PSScriptRoot/../DomainDetective.psd1" -Force
        $analysis = Add-DDDnsblProvider -Domain 'remove.example.com'
        $result = $analysis | Remove-DDDnsblProvider -Domain 'remove.example.com'
        $result | Should -Not -BeNullOrEmpty
    }
    It 'throws if Domain is empty' {
        Import-Module "$PSScriptRoot/../DomainDetective.psd1" -Force
        { Remove-DDDnsblProvider -Domain '' } | Should -Throw
    }
}

Describe 'Clear-DDDnsblProviderList cmdlet' {
    It 'executes and returns analysis' {
        Import-Module "$PSScriptRoot/../DomainDetective.psd1" -Force
        Add-DDDnsblProvider -Domain 'clear.example.com' | Out-Null
        $result = Clear-DDDnsblProviderList
        $result | Should -Not -BeNullOrEmpty
    }
}

Describe 'Import-DDDnsblConfig cmdlet' {
    It 'executes and returns analysis' {
        Import-Module "$PSScriptRoot/../DomainDetective.psd1" -Force
        $source = Join-Path $PSScriptRoot '../../DnsblProviders.sample.json'
        $path = Join-Path $TestDrive 'DnsblProviders.sample.json'
        Copy-Item -Path $source -Destination $path
        $result = Import-DDDnsblConfig -Path $path -OverwriteExisting
        $result | Should -Not -BeNullOrEmpty
    }
    It 'throws if Path is empty' {
        Import-Module "$PSScriptRoot/../DomainDetective.psd1" -Force
        { Import-DDDnsblConfig -Path '' } | Should -Throw
    }

    It 'skips duplicate domains' {
        Import-Module "$PSScriptRoot/../DomainDetective.psd1" -Force
        $json = '{"providers":[{"domain":"dup.test"},{"domain":"DUP.test"}]}'
        $path = Join-Path $TestDrive (([guid]::NewGuid()).ToString() + '.json')
        $json | Set-Content -Path $path
        try {
            $result = Import-DDDnsblConfig -Path $path -ClearExisting
            ($result.GetDNSBL() | Where-Object { $_.Domain -ieq 'dup.test' }).Count | Should -Be 1
        } finally {
            Remove-Item $path -ErrorAction SilentlyContinue
        }
    }
}
