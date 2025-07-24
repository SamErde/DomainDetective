Describe 'Test-DaneRecord cmdlet' {
    It 'cancels on Ctrl+C' {
        Import-Module "$PSScriptRoot/../DomainDetective.psd1" -Force
        $job = Start-Job -ScriptBlock {
            Import-Module "$using:PSScriptRoot/../DomainDetective.psd1" -Force
            Test-TlsDane -DomainName 'example.com' -DnsEndpoint System -Verbose
        }
        Start-Sleep -Milliseconds 500
        Stop-Job $job
        Wait-Job $job
        $job.ChildJobs[0].State | Should -Be 'Stopped'
    }
}
