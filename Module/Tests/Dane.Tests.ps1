Describe 'Test-DDTlsDaneRecord cmdlet' {
    It 'cancels on Ctrl+C' {
        Import-Module "$PSScriptRoot/../DomainDetective.psd1" -Force
        $job = Start-Job -ScriptBlock {
            Import-Module "$using:PSScriptRoot/../DomainDetective.psd1" -Force
            Test-DDTlsDaneRecord -DomainName 'does-not-exist.invalid' -DnsEndpoint System -Verbose
        }
        $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
        while ($job.State -eq 'NotStarted' -and $stopwatch.ElapsedMilliseconds -lt 1000) {
            Start-Sleep -Milliseconds 10
        }
        Stop-Job $job
        Wait-Job $job
        $job.ChildJobs[0].State | Should -Be 'Stopped'
    }

    It 'cancels using PowerShell.Stop()' {
        Import-Module "$PSScriptRoot/../DomainDetective.psd1" -Force
        $ps = [powershell]::Create()
        $ps.AddScript("Import-Module '$PSScriptRoot/../DomainDetective.psd1' -Force; Test-DDTlsDaneRecord -DomainName 'does-not-exist.invalid' -DnsEndpoint System -Verbose") | Out-Null
        $handle = $ps.BeginInvoke()
        $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
        while ($ps.InvocationStateInfo.State -eq 'NotStarted' -and $stopwatch.ElapsedMilliseconds -lt 1000) {
            Start-Sleep -Milliseconds 10
        }
        $ps.Stop()
        $null = $handle.AsyncWaitHandle.WaitOne()
        $ps.InvocationStateInfo.State | Should -Be 'Stopped'
        $ps.Dispose()
    }
}
