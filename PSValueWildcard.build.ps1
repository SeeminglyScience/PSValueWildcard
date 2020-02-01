#requires -Module InvokeBuild -Version 5.1
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

$ModuleName       = 'PSValueWildcard'
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

task TestImpl {
    $resultsPath = Join-Path $PSScriptRoot\tests\$ModuleName.Tests\TestResults\ -ChildPath *
    if (Test-Path $resultsPath) {
        Remove-Item $resultsPath -Recurse -Force @FailOnError
    }

    Push-Location "$PSScriptRoot\tests\$ModuleName.Tests" @FailOnError
    try {
        exec {
            & $dotnet test `
                --verbosity minimal `
                --nologo `
                --configuration $Configuration `
                --framework $Framework `
                --collect "XPlat Code Coverage" `
                --settings "$PWD\coverlet.runsettings"
        }

        $artifacts = Join-Path $PSScriptRoot -ChildPath artifacts
        if (Test-Path $artifacts) {
            Remove-Item $artifacts\* -Force @Silent
        } else {
            New-Item $artifacts -ItemType Directory | Out-Null
        }

        $packagePath = Join-Path $PSScriptRoot -ChildPath src/$ModuleName/bin/Release/$ModuleName.*.nupkg
        $package = Get-ChildItem $packagePath @FailOnError |
            Sort-Object LastWriteTime -Descending |
            Select-Object -First 1

        Copy-Item $package.FullName -Destination $artifacts @FailOnError
    } finally {
        Pop-Location @Silent
    }
}

task Test { $script:Framework = 'netcoreapp3.1' }, Build, TestImpl

task . Build

task Release { $script:Configuration = 'Release'; $script:Framework = 'netcoreapp3.1' }, Clean, TestImpl, Pack
