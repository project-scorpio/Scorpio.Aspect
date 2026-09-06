[CmdletBinding()]
param(
    [ValidateSet('InstallSdk','Restore','Build','Test','Pack','Publish')]
    [string[]]$Target = @(),
    [ValidateSet('Debug','Release')]
    [string]$Configuration = 'Release',
    [switch]$TestAllFrameworks
)

$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'
$RepoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path

try {
    [Net.ServicePointManager]::SecurityProtocol = [Net.ServicePointManager]::SecurityProtocol -bor [Net.SecurityProtocolType]::Tls12
} catch {
}

function Read-RequiredSdk {
    $defaultVersion = '10.0.100'
    $configPath = Join-Path $RepoRoot 'build.config'
    if (-not (Test-Path $configPath)) { return $defaultVersion }
    $match = Select-String -Path $configPath -Pattern '^\s*DOTNET_VERSION=(.+?)\s*$' | Select-Object -First 1
    if ($match -and $match.Matches[0].Groups[1].Value) { return $match.Matches[0].Groups[1].Value.Trim() }
    return $defaultVersion
}

function Get-DotNetVersion {
    try { return [System.Version](& dotnet --version) } catch { return $null }
}

function Ensure-Sdk {
    param([string]$RequiredVersion)
    $required = [System.Version]$RequiredVersion
    $current = Get-DotNetVersion
    if ($current -and ($current.CompareTo($required) -ge 0)) { return }

    $installDir = Join-Path $RepoRoot '.dotnet'
    New-Item -ItemType Directory -Force -Path $installDir | Out-Null
    $installer = Join-Path $installDir 'dotnet-install.ps1'
    if (-not (Test-Path $installer)) {
        Invoke-WebRequest -Uri 'https://dot.net/v1/dotnet-install.ps1' -OutFile $installer
    }
    & $installer -Version $RequiredVersion -InstallDir $installDir -NoPath
    if ($LASTEXITCODE -ne 0) { throw "Failed to install .NET SDK $RequiredVersion" }
    $env:DOTNET_ROOT = $installDir
    $env:PATH = "$installDir$([IO.Path]::PathSeparator)$env:PATH"
}

function Invoke-DotNet {
    param([string[]]$Cmd)
    & dotnet @Cmd
    if ($LASTEXITCODE -ne 0) { throw "Command failed: dotnet $($Cmd -join ' ')" }
}

function Get-PackageVersion {
    [xml]$versionProps = Get-Content -Raw (Join-Path $RepoRoot 'versions.props')
    $major = $versionProps.Project.PropertyGroup.VersionMajor
    $minor = $versionProps.Project.PropertyGroup.VersionMinor
    $patch = $versionProps.Project.PropertyGroup.VersionPatch
    $suffix = $versionProps.Project.PropertyGroup.VersionSuffix
    $prefix = "$major.$minor.$patch"
    if ($suffix) { return "$prefix-$suffix" }
    return $prefix
}

$solution = Get-ChildItem -Path $RepoRoot -Filter '*.sln' -File | Select-Object -First 1
if (-not $solution) { throw "No solution file found in $RepoRoot" }

$isTag = ($env:APPVEYOR_REPO_TAG -eq 'true')
if ($Target.Count -eq 0) {
    $Target = @('Restore', 'Build', 'Test')
    if ($isTag) { $Target = @('Restore', 'Build', 'Test', 'Pack', 'Publish') }
}

Ensure-Sdk -RequiredVersion (Read-RequiredSdk)

foreach ($item in $Target) {
    switch ($item) {
        'InstallSdk' {
            Ensure-Sdk -RequiredVersion (Read-RequiredSdk)
        }
        'Restore' {
            $cmd = @('restore', $solution.FullName)
            if ($env:NUGET_CONFIG_FILE) { $cmd += @('--configfile', $env:NUGET_CONFIG_FILE) }
            Invoke-DotNet $cmd
        }
        'Build' {
            Invoke-DotNet @('build', $solution.FullName, '-c', $Configuration)
        }
        'Test' {
            $cmd = @('test', $solution.FullName, '-c', $Configuration, '--no-build')
            if (-not $TestAllFrameworks) { $cmd += @('-f', 'net10.0') }
            Invoke-DotNet $cmd
        }
        'Pack' {
            $artifacts = Join-Path $RepoRoot 'artifacts'
            New-Item -ItemType Directory -Force -Path $artifacts | Out-Null
            $version = Get-PackageVersion
            Invoke-DotNet @('pack', $solution.FullName, '-c', $Configuration, '--no-build', '-o', $artifacts, "-p:Version=$version", "-p:PackageVersion=$version", '-p:IncludeSymbols=true', '-p:SymbolPackageFormat=snupkg')
        }
        'Publish' {
            if (-not $isTag) { throw 'Publish is only enabled for tag builds.' }
            $source = if ($env:NUGET_SOURCE) { $env:NUGET_SOURCE } else { 'https://api.nuget.org/v3/index.json' }
            $apiKey = $env:NUGET_API_KEY
            if (-not $apiKey) { throw 'NUGET_API_KEY environment variable is required to publish.' }
            $artifacts = Join-Path $RepoRoot 'artifacts'
            Get-ChildItem -Path $artifacts -Filter '*.nupkg' -File |
                Where-Object { $_.Name -notlike '*.symbols.nupkg' } |
                ForEach-Object {
                    Invoke-DotNet @('nuget', 'push', $_.FullName, '--source', $source, '--api-key', $apiKey, '--skip-duplicate')
                }
        }
    }
}
