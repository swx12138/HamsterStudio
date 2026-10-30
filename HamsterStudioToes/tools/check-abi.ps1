<#
.SYNOPSIS
    比对 DLL 导出表与基线，防止 ABI 静默变化。

.DESCRIPTION
    默认取 <项目根>\x64\Release 下最新的 DLL，与 docs\abi-baseline.txt 逐项比对。
    完全一致返回 0；有增删则打印差异并返回 1，可直接用于 CI 或提交前检查。

.PARAMETER Dll
    指定 DLL 路径。省略则在 x64\Release 下自动选取最新产物。

.PARAMETER Baseline
    指定基线文件。省略则用 docs\abi-baseline.txt。

.PARAMETER Update
    把当前导出表写回基线文件（仅在确认 ABI 变化是预期的时候使用）。

.EXAMPLE
    tools\check-abi.ps1

.EXAMPLE
    tools\check-abi.ps1 -Update
#>
[CmdletBinding()]
param(
    [string] $Dll,
    [string] $Baseline,
    [switch] $Update
)

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent

if (-not $Baseline) { $Baseline = Join-Path $root 'docs\abi-baseline.txt' }
if (-not $Dll) {
    $dir = Join-Path $root 'x64\Release'
    if (-not (Test-Path -LiteralPath $dir)) {
        throw "找不到输出目录：$dir，请先构建 Release x64。"
    }
    $Dll = (Get-ChildItem -LiteralPath $dir -Filter '*.dll' |
            Sort-Object LastWriteTime -Descending | Select-Object -First 1).FullName
}

function Resolve-Dumpbin {
    $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
    if (-not (Test-Path -LiteralPath $vswhere)) {
        throw '未找到 vswhere.exe，请安装 Visual Studio 或手动指定 dumpbin。'
    }
    $vs = & $vswhere -latest -products * `
        -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath
    if (-not $vs) { throw 'vswhere 未找到带 C++ 工具集的 Visual Studio。' }

    $msvc = Get-ChildItem -LiteralPath (Join-Path $vs 'VC\Tools\MSVC') -Directory |
            Sort-Object Name -Descending | Select-Object -First 1
    $exe = Join-Path $msvc.FullName 'bin\Hostx64\x64\dumpbin.exe'
    if (-not (Test-Path -LiteralPath $exe)) { throw "未找到 dumpbin：$exe" }
    return $exe
}

$dumpbin = Resolve-Dumpbin
$current = @(& $dumpbin /nologo /exports $Dll |
    ForEach-Object {
        if ($_ -match '^\s+\d+\s+[0-9A-Fa-f]+\s+[0-9A-Fa-f]+\s+(\S+)') { $matches[1] }
    } |
    Sort-Object)

if ($Update) {
    $current | Set-Content -LiteralPath $Baseline -Encoding utf8
    Write-Host "基线已更新：$Baseline（$($current.Count) 项）"
    exit 0
}

if (-not (Test-Path -LiteralPath $Baseline)) {
    throw "找不到基线：$Baseline（首次使用可加 -Update 生成）"
}

$expected = @(Get-Content -LiteralPath $Baseline | Sort-Object)
$removed  = @($expected | Where-Object { $current -notcontains $_ })
$added    = @($current  | Where-Object { $expected -notcontains $_ })

Write-Host "DLL    : $Dll"
Write-Host "基线   : $Baseline"
Write-Host "导出项 : 基线 $($expected.Count) / 当前 $($current.Count)"

if ($removed.Count -eq 0 -and $added.Count -eq 0) {
    Write-Host '结果   : 一致' -ForegroundColor Green
    exit 0
}

foreach ($n in $removed) { Write-Host "  缺失 $n" -ForegroundColor Red }
foreach ($n in $added)   { Write-Host "  新增 $n" -ForegroundColor Yellow }
exit 1
