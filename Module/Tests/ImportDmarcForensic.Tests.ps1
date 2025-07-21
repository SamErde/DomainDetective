Describe 'Import-DmarcForensic cmdlet' {
    It 'parses forensic report' {
        Import-Module "$PSScriptRoot/../DomainDetective.psd1" -Force
        $source = Join-Path $PSScriptRoot '../../DomainDetective.Tests/Data/dmarc_forensic.b64'
        $b64 = Get-Content -Path $source -Raw
        $tmp = Join-Path $TestDrive ([IO.Path]::GetRandomFileName())
        [IO.File]::WriteAllBytes($tmp, [Convert]::FromBase64String($b64))
        try {
            $result = Import-DmarcForensic -Path $tmp
            $result.SourceIp | Should -Be '192.0.2.1'
        } finally {
            Remove-Item $tmp -Force
        }
    }
}
