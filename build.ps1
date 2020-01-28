[CmdletBinding()]
param(
    [Parameter()]
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Release',

    [Parameter()]
    [switch] $Force
)
end {
    & "$PSScriptRoot\tools\AssertRequiredModule.ps1" InvokeBuild 5.5.6 -Force:$Force.IsPresent
    $invokeBuildSplat = @{
        Task = 'Release'
        File = "$PSScriptRoot/PSValueWildcard.build.ps1"
        Force = $Force.IsPresent
        Configuration = $Configuration
    }

    Invoke-Build @invokeBuildSplat
}
