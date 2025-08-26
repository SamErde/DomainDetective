Describe 'DomainDetective module' {
    It 'imports successfully' {
        { Import-Module "$PSScriptRoot/../DomainDetective.psd1" -Force } | Should -Not -Throw
    }

    It 'exposes Test-DDEmailSpfRecord cmdlet' {
        Import-Module "$PSScriptRoot/../DomainDetective.psd1" -Force
        Get-Command Test-DDEmailSpfRecord -ErrorAction Stop | Should -Not -BeNullOrEmpty
    }

    It 'exposes Add-DDDnsblProvider cmdlet' {
        Import-Module "$PSScriptRoot/../DomainDetective.psd1" -Force
        Get-Command Add-DDDnsblProvider -ErrorAction Stop | Should -Not -BeNullOrEmpty
    }

    It 'exposes Import-DDDmarcForensic cmdlet' {
        Import-Module "$PSScriptRoot/../DomainDetective.psd1" -Force
        Get-Command Import-DDDmarcForensic -ErrorAction Stop | Should -Not -BeNullOrEmpty
    }
}
