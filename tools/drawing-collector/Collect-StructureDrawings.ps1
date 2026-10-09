<#
.SYNOPSIS
  从项目 RECEIVED 文件夹中把结构 (Structure) 图纸整理出来：生成图纸清单，并按需复制最新版 / 全部版本。

.DESCRIPTION
  1. 递归扫描 -Source 下所有图纸文件 (默认 pdf/dwg/dxf/tif)。
  2. 路径 (文件夹名 + 文件名) 中出现结构关键字 (STR / STRUCT / STRUCTURAL / STEEL / 结构 …) 的文件视为结构图纸。
  3. 从文件名解析图号和版本 (Rev)，标记每张图的最新版本。
  4. 输出 Excel 可直接打开的 CSV 图纸清单；按 -Mode 复制文件。

.PARAMETER Mode
  List   只生成清单，不复制文件
  Latest 生成清单 + 复制每张图的最新版本到 <Output>\Latest  (默认)
  All    生成清单 + 按来文 (Transmittal) 文件夹复制全部结构图纸到 <Output>\By_Transmittal

.EXAMPLE
  .\Collect-StructureDrawings.ps1
  .\Collect-StructureDrawings.ps1 -Mode List
  .\Collect-StructureDrawings.ps1 -Mode All -Output 'D:\GLNG7\STR'
  .\Collect-StructureDrawings.ps1 -ExtraPattern '-S-\d{4}'   # 图号里用 -S- 表示结构专业时
#>
[CmdletBinding()]
param(
    [string]$Source = '\\192.168.100.12\Function_Engineering\ENGINEERING_TRANSMITTAL AND REGISTER\DOCUMENT TRANSMITTAL\ST7 GLNG7 (GLNG 7.1 & HRLS 4) PROJECT\RECEIVED',
    [string]$Output = (Join-Path ([Environment]::GetFolderPath('Desktop')) 'GLNG7_Structure_Drawings'),
    [string[]]$Keywords = @('ST', 'STR', 'STRU', 'STRUC', 'STRUCT', 'STRUCTURE', 'STRUCTURES', 'STRUCTURAL', 'STEEL', 'STEELWORK', '结构', '钢结构'),
    [string]$ExtraPattern = '',
    [string[]]$Extensions = @('.pdf', '.dwg', '.dxf', '.tif', '.tiff'),
    [ValidateSet('List', 'Latest', 'All')]
    [string]$Mode = 'Latest'
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path -LiteralPath $Source)) {
    Write-Host "找不到源文件夹 (请确认已连上公司网络 / 有访问权限):`n  $Source" -ForegroundColor Red
    exit 1
}

$keywordSet = @{}
foreach ($k in $Keywords) { $keywordSet[$k.ToUpperInvariant()] = $true }
$cjkKeywords = @($Keywords | Where-Object { $_ -match '[^\x00-\x7F]' })
$extSet = @{}
foreach ($e in $Extensions) { $extSet[$e.ToLowerInvariant()] = $true }

function Test-IsStructure([string]$relativePath) {
    foreach ($t in ($relativePath.ToUpperInvariant() -split '[^A-Z0-9]+')) {
        if ($t -and $keywordSet.ContainsKey($t)) { return $true }
    }
    foreach ($k in $cjkKeywords) {
        if ($relativePath.Contains($k)) { return $true }
    }
    if ($ExtraPattern -and $relativePath -match $ExtraPattern) { return $true }
    return $false
}

# 文件名 -> 图号 + 版本。认 "XXX_Rev B" / "XXX REV.0" / "XXX-REV01 Title" / 结尾 "XXX_B" "XXX_R1"
$revRegex = [regex]'(?i)(?:^|[\s_\-\.\(\[])REV(?:ISION)?[\s_\-\.]*([A-Z]{0,2}\d{0,3})(?=$|[\s_\-\.\)\]])'
$tailRevRegex = [regex]'(?i)[_\s]R?([A-Z]{1,2}\d{0,2}|\d{1,2})$'

function Get-DrawingInfo([string]$baseName) {
    $m = $revRegex.Match($baseName)
    if ($m.Success -and $m.Groups[1].Value) {
        $no = $baseName.Substring(0, $m.Index).Trim(' ', '_', '-', '.', '(', '[')
        if (-not $no) { $no = $baseName }
        return @{ No = $no; Rev = $m.Groups[1].Value.ToUpperInvariant() }
    }
    $m = $tailRevRegex.Match($baseName)
    if ($m.Success) {
        $no = $baseName.Substring(0, $m.Index).Trim(' ', '_', '-', '.')
        if ($no) { return @{ No = $no; Rev = $m.Groups[1].Value.ToUpperInvariant() } }
    }
    return @{ No = $baseName; Rev = '' }
}

# 数字版 (0,1,2 = IFC/IFR) 排在字母版 (A,B,C = 初版) 之后；P01/C02 这类按字母再按数字。
function Get-RevRank([string]$rev) {
    if (-not $rev) { return -1 }
    if ($rev -match '^\d+$') { return 1000000 + [int]$rev }
    if ($rev -match '^([A-Z]+)(\d*)$') {
        $letters = 0
        foreach ($c in $Matches[1].ToCharArray()) { $letters = $letters * 26 + ([int]$c - 64) }
        $num = 0
        if ($Matches[2]) { $num = [int]$Matches[2] }
        return $letters * 1000 + $num
    }
    return 0
}

Write-Host "扫描中: $Source" -ForegroundColor Cyan
$scanErrors = @()
$files = Get-ChildItem -LiteralPath $Source -Recurse -File -ErrorAction SilentlyContinue -ErrorVariable +scanErrors |
    Where-Object { $extSet.ContainsKey($_.Extension.ToLowerInvariant()) }

$sourceRoot = (Get-Item -LiteralPath $Source).FullName.TrimEnd('\')
$rows = foreach ($f in $files) {
    $rel = $f.FullName.Substring($sourceRoot.Length).TrimStart('\')
    if (-not (Test-IsStructure $rel)) { continue }
    $info = Get-DrawingInfo $f.BaseName
    $parts = $rel -split '\\'
    [pscustomobject]@{
        DrawingNo   = $info.No
        Rev         = $info.Rev
        Type        = $f.Extension.TrimStart('.').ToUpperInvariant()
        IsLatest    = ''
        Transmittal = $(if ($parts.Count -gt 1) { $parts[0] } else { '' })
        FileName    = $f.Name
        Modified    = $f.LastWriteTime.ToString('yyyy-MM-dd HH:mm')
        SizeKB      = [math]::Round($f.Length / 1KB)
        RelPath     = $rel
        FullPath    = $f.FullName
        _Rank       = Get-RevRank $info.Rev
        _Time       = $f.LastWriteTime
    }
}
$rows = @($rows)

if ($rows.Count -eq 0) {
    Write-Host "没有找到结构图纸。可用 -Keywords / -ExtraPattern 调整识别规则。" -ForegroundColor Yellow
    exit 0
}

# 同一图号 + 同一格式里取最新版本
$latest = @()
foreach ($g in ($rows | Group-Object { "$($_.DrawingNo.ToUpperInvariant())|$($_.Type)" })) {
    $top = $g.Group | Sort-Object _Rank, _Time -Descending | Select-Object -First 1
    $top.IsLatest = 'Y'
    $latest += $top
}

New-Item -ItemType Directory -Path $Output -Force | Out-Null
$stamp = Get-Date -Format 'yyyyMMdd_HHmm'
$csv = Join-Path $Output "Structure_Drawing_Register_$stamp.csv"
$export = $rows | Sort-Object DrawingNo, Type, _Rank, _Time |
    Select-Object DrawingNo, Rev, Type, IsLatest, Transmittal, FileName, Modified, SizeKB, RelPath, FullPath
if ($PSVersionTable.PSVersion.Major -ge 6) {
    $export | Export-Csv -LiteralPath $csv -NoTypeInformation -Encoding utf8BOM
} else {
    $export | Export-Csv -LiteralPath $csv -NoTypeInformation -Encoding UTF8
}

function Copy-IfNeeded($src, [string]$dest) {
    $dir = Split-Path -Parent $dest
    if (-not (Test-Path -LiteralPath $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }
    if (Test-Path -LiteralPath $dest) {
        $existing = Get-Item -LiteralPath $dest
        if ($existing.Length -eq $src.Length -and $existing.LastWriteTime -eq $src._Time) { return $false }
    }
    Copy-Item -LiteralPath $src.FullPath -Destination $dest -Force
    return $true
}

$copied = 0
if ($Mode -eq 'Latest') {
    $used = @{}
    foreach ($r in ($latest | Sort-Object DrawingNo, Type)) {
        $name = $r.FileName
        if ($used.ContainsKey($name.ToLowerInvariant())) {
            $name = [IO.Path]::GetFileNameWithoutExtension($name) + " ($($r.Transmittal))" + [IO.Path]::GetExtension($name)
        }
        $used[$name.ToLowerInvariant()] = $true
        if (Copy-IfNeeded $r (Join-Path (Join-Path $Output 'Latest') $name)) { $copied++ }
    }
} elseif ($Mode -eq 'All') {
    foreach ($r in $rows) {
        if (Copy-IfNeeded $r (Join-Path (Join-Path $Output 'By_Transmittal') $r.RelPath)) { $copied++ }
    }
}

Write-Host ''
Write-Host "结构图纸文件: $($rows.Count)   不同图号/格式: $($latest.Count)" -ForegroundColor Green
foreach ($t in ($latest | Group-Object Type | Sort-Object Name)) { Write-Host ("  {0,-5} {1} 张 (最新版)" -f $t.Name, $t.Count) }
Write-Host "图纸清单: $csv"
if ($Mode -ne 'List') { Write-Host "已复制 $copied 个文件到: $Output" }
if ($scanErrors.Count -gt 0) {
    Write-Host "有 $($scanErrors.Count) 个文件夹/文件无法读取 (权限或路径过长)，例如:" -ForegroundColor Yellow
    $scanErrors | Select-Object -First 5 | ForEach-Object { Write-Host "  $($_.TargetObject)" -ForegroundColor Yellow }
}
$zips = @(Get-ChildItem -LiteralPath $Source -Recurse -File -ErrorAction SilentlyContinue |
    Where-Object { @('.zip', '.rar', '.7z') -contains $_.Extension.ToLowerInvariant() })
if ($zips.Count -gt 0) {
    Write-Host "注意: RECEIVED 中有 $($zips.Count) 个压缩包，里面的图纸未被扫描，需要先解压。" -ForegroundColor Yellow
}
