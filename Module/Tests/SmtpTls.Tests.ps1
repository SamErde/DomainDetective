Describe 'Test-EmailSmtpTls cmdlet' {
    It 'returns SMTPTLSAnalysis object' {
        Import-Module "$PSScriptRoot/../DomainDetective.psd1" -Force
        $result = Test-EmailSmtpTls -HostName 'localhost' -Port 25
        $result | Should -BeOfType 'DomainDetective.SMTPTLSAnalysis'
    }
}
