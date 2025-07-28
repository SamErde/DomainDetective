Describe 'Test-DaneRecord cmdlet' {
    It 'cancels on Ctrl+C' {
        Import-Module "$PSScriptRoot/../DomainDetective.psd1" -Force
        $job = Start-Job -ScriptBlock {
            Import-Module "$using:PSScriptRoot/../DomainDetective.psd1" -Force
            Test-TlsDane -DomainName 'does-not-exist.invalid' -DnsEndpoint System -Verbose
        }
        Start-Sleep -Milliseconds 20
        Stop-Job $job
        Wait-Job $job
        $job.ChildJobs[0].State | Should -Be 'Stopped'
    }

    It 'cancels using PowerShell.Stop()' {
        Import-Module "$PSScriptRoot/../DomainDetective.psd1" -Force
        $ps = [powershell]::Create()
        $ps.AddScript("Import-Module '$PSScriptRoot/../DomainDetective.psd1' -Force; Test-TlsDane -DomainName 'does-not-exist.invalid' -DnsEndpoint System -Verbose") | Out-Null
        $handle = $ps.BeginInvoke()
        Start-Sleep -Milliseconds 20
        $ps.Stop()
        $null = $handle.AsyncWaitHandle.WaitOne()
        $ps.InvocationStateInfo.State | Should -Be 'Stopped'
        $ps.Dispose()
    }
}
