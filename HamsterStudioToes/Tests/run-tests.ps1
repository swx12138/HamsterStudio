# Build and run the HamsterStudioToes unit tests.
#
# Usage:
#   pwsh -ExecutionPolicy Bypass -File Tests/run-tests.ps1
#   pwsh -ExecutionPolicy Bypass -File Tests/run-tests.ps1 -Configuration Debug
#
# Requires Visual Studio with the C++ toolset, and OpenCV at the path configured
# in the project file.
#
# NOTE: this script is deliberately ASCII-only. Windows PowerShell reads a script
# without a UTF-8 BOM using the local ANSI code page, which turns non-ASCII
# comments into parser errors. Keeping it ASCII avoids that entirely.

[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'

$projectDir = $PSScriptRoot
$project = Join-Path $projectDir 'HamsterStudioToes.Tests.vcxproj'

if (-not (Test-Path $project)) {
    throw "Project file not found: $project"
}

$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
if (-not (Test-Path $vswhere)) {
    throw 'vswhere.exe not found; is Visual Studio installed?'
}

$msbuild = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
if (-not $msbuild) {
    throw 'MSBuild.exe not found'
}

Write-Host "MSBuild     : $msbuild"
Write-Host "Configuration: $Configuration|x64"

& $msbuild $project /nologo /m /v:minimal /p:Configuration=$Configuration /p:Platform=x64
if ($LASTEXITCODE -ne 0) {
    throw "Build failed (exit $LASTEXITCODE)"
}

$exe = Join-Path $projectDir "bin\$Configuration\ToesTests.exe"
if (-not (Test-Path $exe)) {
    throw "Test executable not found: $exe"
}

# opencv_world4120[d].dll must be resolvable at run time
$opencvBin = 'D:\Develop\01_CMakeInstall\x64\vc17\bin'
if (Test-Path $opencvBin) {
    $env:PATH = "$opencvBin;$env:PATH"
}

# A few cases use relative paths, so run from the library root.
Push-Location (Join-Path $projectDir '..')
try {
    & $exe
    $code = $LASTEXITCODE
}
finally {
    Pop-Location
}

exit $code
