param(
    [string]$Executable = "$env:LOCALAPPDATA\Sunroom\Sunroom.exe",
    [string]$Result = "$env:TEMP\Sunroom-package-result.json",
    [switch]$QuitOnly
)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName UIAutomationClient, UIAutomationTypes
$checks = New-Object 'System.Collections.Generic.List[string]'
function Check($condition, $label) {
    if (!$condition) { throw $label }
    $checks.Add($label)
}
function Window($process) {
    $condition = New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::ProcessIdProperty, [int]$process.Id)
    $windows = [System.Windows.Automation.AutomationElement]::RootElement.FindAll([System.Windows.Automation.TreeScope]::Children, $condition)
    foreach ($window in $windows) { if ($window.Current.Name -eq 'Sunroom') { return $window } }
    throw 'Settings window not found'
}
function Control($window, $id) {
    $condition = New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::AutomationIdProperty, $id)
    $control = $window.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $condition)
    if (!$control) { throw "Control not found: $id" }
    return $control
}
function Invoke($window, $id) { (Control $window $id).GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke() }
function Settings { Get-Content "$env:LOCALAPPDATA\Sunroom\settings.json" -Raw | ConvertFrom-Json }
try {
    if ($QuitOnly) {
        $running = Get-Process Sunroom -ErrorAction SilentlyContinue | Where-Object SessionId -eq ([System.Diagnostics.Process]::GetCurrentProcess().SessionId)
        if ($running) {
            Start-Process $Executable
            Start-Sleep -Milliseconds 500
            Invoke (Window $running) 'Quit'
            Check ($running.WaitForExit(10000)) 'Running app quits for update'
        }
        return
    }
    if (Get-Process Sunroom -ErrorAction SilentlyContinue) { throw 'Quit Sunroom before running the package smoke test.' }
    $process = Start-Process $Executable -PassThru
    Check ($process.WaitForInputIdle(15000)) 'Packaged executable starts its UI'
    Start-Sleep -Milliseconds 700
    $window = Window $process
    Check ($process.SessionId -gt 0) 'Packaged app runs in the interactive desktop session'
    $show = (Control $window 'ShowTint').GetCurrentPattern([System.Windows.Automation.TogglePattern]::Pattern)
    if ($show.Current.ToggleState -ne [System.Windows.Automation.ToggleState]::Off) { $show.Toggle() }
    Start-Sleep -Milliseconds 500
    Check (!(Settings).Enabled) 'Show/hide control saves hidden state in the installed app'
    Invoke $window 'Blue'
    Start-Sleep -Milliseconds 500
    Check ((Settings).Blue -eq 51 -and (Settings).Opacity -eq 25) 'Packaged preset controls update persisted color and strength'
    Invoke $window 'Amber'
    Invoke $window 'LockTint'
    Start-Sleep -Milliseconds 300
    $second = Start-Process $Executable -PassThru
    Check ($second.WaitForExit(10000)) 'Second launch exits instead of creating another instance'
    Start-Sleep -Milliseconds 500
    $window = Window $process
    Check (!$window.Current.IsOffscreen) 'Second launch reopens the existing settings panel'
    if (!(Settings).Locked) { Invoke $window 'LockTint' }
    Invoke $window 'Quit'
    Check ($process.WaitForExit(10000)) 'Quit exits the packaged app cleanly'
    $process = Start-Process $Executable -PassThru
    Check ($process.WaitForInputIdle(15000)) 'Packaged app restarts successfully'
    Start-Sleep -Milliseconds 500
    Check (!(Settings).Enabled -and (Settings).Locked -and (Settings).Red -eq 51) 'Packaged app preserves hidden, locked, and tint state on restart'
    $window = Window $process
    $window.GetCurrentPattern([System.Windows.Automation.WindowPattern]::Pattern).Close()
    Start-Sleep -Milliseconds 300
    $process.Refresh()
    Check (!$process.HasExited) 'Closing settings leaves the packaged tray app running'
    @{ Success = $true; Checks = $checks.ToArray(); ProcessId = $process.Id; Executable = $Executable } | ConvertTo-Json -Depth 5 | Set-Content $Result
} catch {
    @{ Success = $false; Checks = $checks.ToArray(); Error = $_.ToString() } | ConvertTo-Json -Depth 5 | Set-Content $Result
    exit 1
}
