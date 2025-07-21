Describe 'Test-EmailArc cmdlet' {
    It 'supports pipeline input' {
        Import-Module "$PSScriptRoot/../DomainDetective.psd1" -Force
        $source = Join-Path $PSScriptRoot '../../DomainDetective.Tests/Data/arc-valid.txt'
        $path = Join-Path $TestDrive 'arc-valid.txt'
        Copy-Item -Path $source -Destination $path
        $headers = Get-Content -Path $path -Raw
        $result = $headers | Test-EmailArc
        $result | Should -Not -BeNullOrEmpty
    }
}

