<#
.SYNOPSIS
    Regenerate the explicit source item lists in the MSBuild projects.

.DESCRIPTION
    The Visual C++ project system does not support wildcards in project items
    (it warns: "VC projects do not support wildcards ..."). MSBuild itself does
    expand them, so the build works but the IDE can misbehave.

    This script keeps the convenience of deriving the list from the file system
    while writing explicit <ClCompile>/<ClInclude> items, which both the IDE and
    MSBuild are happy with.

    Run it after adding, moving or deleting a source file.

.PARAMETER Check
    Do not write anything. Exit with code 1 when a project is out of sync, which
    makes it usable as a pre-build or CI guard.

.EXAMPLE
    tools\sync-project.ps1

.EXAMPLE
    tools\sync-project.ps1 -Check
#>
[CmdletBinding()]
param([switch]$Check)

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$nsUri = 'http://schemas.microsoft.com/developer/msbuild/2003'

function Get-Files {
    param(
        [string]   $Directory,
        [string[]] $Extensions,
        [string[]] $ExcludeRegex
    )
    if (-not (Test-Path -LiteralPath $Directory)) { return @() }

    Get-ChildItem -LiteralPath $Directory -Recurse -File |
        Where-Object { $Extensions -contains $_.Extension.ToLowerInvariant() } |
        Where-Object {
            $rel = $_.FullName.Substring($root.Length + 1)
            foreach ($pattern in $ExcludeRegex) {
                if ($rel -match $pattern) { return $false }
            }
            return $true
        } |
        ForEach-Object { $_.FullName.Substring($root.Length + 1) } |
        Sort-Object
}

function Set-ProjectItems {
    param(
        [string]   $ProjectPath,
        [string[]] $Compile,
        [string[]] $Include
    )
    if (-not (Test-Path -LiteralPath $ProjectPath)) {
        throw "project not found: $ProjectPath"
    }

    $doc = New-Object System.Xml.XmlDocument
    $doc.PreserveWhitespace = $false
    $doc.Load($ProjectPath)
    $ns = New-Object System.Xml.XmlNamespaceManager($doc.NameTable)
    $ns.AddNamespace('m', $nsUri)

    foreach ($group in @($doc.SelectNodes('//m:ItemGroup[m:ClCompile or m:ClInclude]', $ns))) {
        $group.ParentNode.RemoveChild($group) | Out-Null
    }

    $itemGroup = $doc.CreateElement('ItemGroup', $nsUri)
    foreach ($file in $Compile) {
        $element = $doc.CreateElement('ClCompile', $nsUri)
        $element.SetAttribute('Include', $file)
        $itemGroup.AppendChild($element) | Out-Null
    }
    foreach ($file in $Include) {
        $element = $doc.CreateElement('ClInclude', $nsUri)
        $element.SetAttribute('Include', $file)
        $itemGroup.AppendChild($element) | Out-Null
    }

    $import = $doc.SelectSingleNode("m:Import[contains(@Project,'Microsoft.Cpp.targets')]", $ns)
    if ($import) { $doc.DocumentElement.InsertBefore($itemGroup, $import) | Out-Null }
    else { $doc.DocumentElement.AppendChild($itemGroup) | Out-Null }

    $doc.Save($ProjectPath)
}

# ---------------------------------------------------------------- library project
$libProject = (Get-ChildItem -LiteralPath $root -Filter '*.vcxproj' |
    Where-Object { $_.Name -notlike '*.user' } | Select-Object -First 1).FullName

$libCompile = Get-Files -Directory (Join-Path $root 'src') -Extensions '.cpp' -ExcludeRegex @('\\legacy\\')
$libInclude = @()
$libInclude += Get-Files -Directory (Join-Path $root 'include') -Extensions @('.h', '.hpp')
$libInclude += Get-Files -Directory (Join-Path $root 'src') -Extensions @('.h', '.hpp') -ExcludeRegex @('\\legacy\\')
$libInclude = $libInclude | Sort-Object

# ---------------------------------------------------------------- test project
$testProject = (Get-ChildItem -LiteralPath (Join-Path $root 'Tests') -Filter '*.vcxproj' |
    Where-Object { $_.Name -notlike '*.user' } | Select-Object -First 1).FullName

# tests compile every active source except dllmain.cpp, plus the legacy sources they exercise
$testCompile = @()
$testCompile += Get-Files -Directory (Join-Path $root 'tests') -Extensions '.cpp'
$testCompile += Get-Files -Directory (Join-Path $root 'src') -Extensions '.cpp' -ExcludeRegex @('\\legacy\\', '\\src\\dllmain\.cpp$')
$testCompile += @(
    'src\legacy\algorithm\Math.cpp',
    'src\legacy\algorithm\Sort.cpp',
    'src\legacy\media\Wave.cpp',
    'src\legacy\nstd\string.cpp'
)
$testCompile = $testCompile | Sort-Object
# items are resolved relative to the project file, and the test project lives in Tests\
$testCompile = $testCompile | ForEach-Object { '..\' + $_ }

Write-Host "library : $($libCompile.Count) sources, $($libInclude.Count) headers"
Write-Host "tests   : $($testCompile.Count) sources"

if ($Check) {
    # crude but sufficient: compare the recorded file names with the expected ones
    $ok = $true
    foreach ($pair in @(@($libProject, $libCompile, $libInclude), @($testProject, $testCompile, @()))) {
        $doc = New-Object System.Xml.XmlDocument
        $doc.Load($pair[0])
        $ns = New-Object System.Xml.XmlNamespaceManager($doc.NameTable)
        $ns.AddNamespace('m', $nsUri)
        $actual = @($doc.SelectNodes('//m:ItemGroup/m:ClCompile', $ns) | ForEach-Object { $_.Include })
        if (($actual -join "`n") -ne (($pair[1]) -join "`n")) {
            Write-Host "OUT OF SYNC: $([System.IO.Path]::GetFileName($pair[0]))"
            $ok = $false
        }
    }
    if ($ok) { Write-Host 'in sync' }
    exit $(if ($ok) { 0 } else { 1 })
}

Set-ProjectItems -ProjectPath $libProject -Compile $libCompile -Include $libInclude
Set-ProjectItems -ProjectPath $testProject -Compile $testCompile -Include @()
Write-Host 'projects updated'
exit 0
