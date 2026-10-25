param([string]$Source = (Join-Path $PSScriptRoot 'artifacts\win-x64\Sunroom.exe'), [switch]$NoLaunch)
$ErrorActionPreference = 'Stop'
if (!(Test-Path $Source)) { throw "Build Sunroom first: .\build.ps1" }
$destination = Join-Path $env:LOCALAPPDATA 'Sunroom'
$executable = Join-Path $destination 'Sunroom.exe'
if (Get-Process Sunroom -ErrorAction SilentlyContinue) { throw 'Quit Sunroom from its tray icon before updating.' }
New-Item -ItemType Directory -Force $destination | Out-Null
Copy-Item $Source $executable -Force
$shell = New-Object -ComObject WScript.Shell
$shortcut = $shell.CreateShortcut((Join-Path ([Environment]::GetFolderPath('Programs')) 'Sunroom.lnk'))
$shortcut.TargetPath = $executable
$shortcut.WorkingDirectory = $destination
$shortcut.Description = 'A movable tint for bright screens'
$shortcut.IconLocation = "$executable,0"
$shortcut.Save()
Write-Host "Installed $executable (Start menu: Sunroom)"
if (!$NoLaunch) { Start-Process $executable }
