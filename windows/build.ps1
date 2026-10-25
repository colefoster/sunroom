param([ValidateSet('win-x64', 'win-arm64')][string]$Runtime = 'win-x64')
$ErrorActionPreference = 'Stop'
$destination = Join-Path $PSScriptRoot "artifacts\$Runtime"
$metadata = Invoke-RestMethod 'https://builds.dotnet.microsoft.com/dotnet/release-metadata/8.0/releases.json'
$runtimePatch = $metadata.'latest-runtime'
if ($runtimePatch -notmatch '^8\.0\.\d+$') { throw 'Could not resolve the current .NET 8 runtime patch.' }
dotnet publish (Join-Path $PSScriptRoot 'Sunroom\Sunroom.csproj') -c Release -r $Runtime --self-contained true "-p:RuntimeFrameworkVersion=$runtimePatch" -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -p:DebugType=None -o $destination
if ($LASTEXITCODE -ne 0) { throw "Publish failed ($LASTEXITCODE)" }
Write-Host "Built $destination\Sunroom.exe"
