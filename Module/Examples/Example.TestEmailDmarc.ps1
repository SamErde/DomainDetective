﻿# Clear-Host

Import-Module $PSScriptRoot\..\DomainDetective.psd1 -Force

$Dmarc = Test-EmailDmarc -DomainName 'evotec.pl' -Verbose
$Dmarc | Format-Table
$Dmarc | Format-List


$Example = Test-EmailDmarc -DomainName 'example.com'
$Example | Format-Table
$Example | Format-List
