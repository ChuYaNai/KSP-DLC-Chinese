# Build the KSP DLC Chinese patch installer.
# Requires Windows + csc.exe from .NET Framework 4.x (bundled with Win10/11).
# Usage:  .\src\build.ps1

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$csc = @(
    'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe',
    'C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe'
) | Where-Object { Test-Path $_ } | Select-Object -First 1

if (-not $csc) { throw 'csc.exe not found. Please enable .NET Framework 4.x.' }

$dist = Join-Path $root 'dist'
New-Item -ItemType Directory -Force $dist | Out-Null

$exe = Join-Path $dist 'KSP_DLC_Chinese_Patch_v1.0.exe'
$serenity = Join-Path $root 'GameData\SquadExpansion\Serenity\Localization\zh-cn.cfg'
$makingHistory = Join-Path $root 'GameData\SquadExpansion\MakingHistory\Localization\zh-cn.cfg'

foreach ($f in @($serenity, $makingHistory)) {
    if (-not (Test-Path $f)) { throw "Missing dictionary file: $f" }
}

& $csc /nologo /target:winexe /platform:anycpu /optimize+ /warn:2 "/out:$exe" `
    "/resource:$serenity,KspDlcCn.serenity.cfg" `
    "/resource:$makingHistory,KspDlcCn.makinghistory.cfg" `
    /r:System.Windows.Forms.dll /r:System.Drawing.dll `
    (Join-Path $PSScriptRoot 'Installer.cs')

if ($LASTEXITCODE -ne 0) { throw "Build failed, exit=$LASTEXITCODE" }

Get-Item $exe | Select-Object Name, Length, LastWriteTime | Format-List
Write-Host "Build OK -> $exe" -ForegroundColor Green
