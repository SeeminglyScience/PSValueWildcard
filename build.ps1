#!/usr/bin/env pwsh
#requires -Version 5.1
[CmdletBinding()]
param(
    [Parameter()]
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Release',

    [Parameter()]
    [ValidateSet('netstandard2.0', 'netcoreapp2.1', 'netcoreapp3.1')]
    [string[]] $Framework,

    [Parameter()]
    [string] $Task = 'Release',

    [Parameter()]
    [switch] $Force
)
end {
    & "$PSScriptRoot\tools\AssertRequiredModule.ps1" InvokeBuild 5.5.6 -Force:$Force.IsPresent
    $invokeBuildSplat = @{
        Task = $Task
        File = Join-Path $PSScriptRoot -ChildPath PSValueWildcard.build.ps1
        Force = $Force.IsPresent
        Configuration = $Configuration
    }

    if ($PSBoundParameters.ContainsKey('Framework')) {
        $invokeBuildSplat['Framework'] = $Framework
    }

    Invoke-Build @invokeBuildSplat -ErrorAction Stop
}
