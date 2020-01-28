#requires -Module InvokeBuild -Version 6.2.3
[CmdletBinding()]
param(
    [Parameter()]
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Debug',

    [Parameter()]
    [ValidateSet('netstandard2.0', 'netcoreapp2.1', 'netcoreapp3.1')]
    [string[]] $Framework = 'netstandard2.0',

    [Parameter()]
    [switch] $Force
)

$ModuleName     = 'PSValueWildcard'
$TargetSdkVersion = '3.1.100'
$ToolsPath        = "$PSScriptRoot/tools"
$IsUnix           = $PSEdition -eq 'Core' -and -not $IsWindows

$FailOnError = @{
    ErrorAction = [System.Management.Automation.ActionPreference]::Stop
}

$Silent = @{
    ErrorAction = [System.Management.Automation.ActionPreference]::Ignore
    WarningAction = [System.Management.Automation.ActionPreference]::Ignore
}

task AssertDependencies AssertRequiredModules, AssertDotNet -Before Build, Test, Clean, Pack

task AssertRequiredModules -If { $false } {
    # Currently only InvokeBuild is required.
    # $assertRequiredModule = Get-Command $ToolsPath/AssertRequiredModule.ps1 @FailOnError
}

task Clean {
    exec { & $dotnet clean --nologo --verbosity minimal }
}

task AssertDotNet -If { -not $script:dotnet -or $script:dotnet -isnot [System.Management.Automation.CommandInfo] } {
    $script:dotnet = & $ToolsPath/GetDotNet.ps1 -Unix:$IsUnix -Version $TargetSdkVersion
}

task Build {
    exec { & $dotnet publish --framework $Framework --configuration $Configuration --verbosity minimal --nologo }
}

task Pack {
    if ($Configuration -ne 'Release') {
        throw 'Configuration must be Release to pack.'
    }

    exec { & $dotnet pack --configuration Release --verbosity minimal --nologo }
}

task Test {
    Push-Location "$PSScriptRoot\tests\$ModuleName.Tests" @FailOnError
    try {
        exec { & $dotnet test --verbosity minimal --nologo --configuration $Configuration --framework $Framework }
    } finally {
        Pop-Location @Silent
    }
}

task . Build

task Release { $script:Configuration = 'Release'; $script:Framework = 'netcoreapp3.1' }, Clean, Test, Pack
