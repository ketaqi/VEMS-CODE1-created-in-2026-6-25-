Add-Type -AssemblyName UIAutomationClient; Add-Type -AssemblyName UIAutomationTypes; Add-Type -AssemblyName System.Windows.Forms
$root = [System.Windows.Automation.AutomationElement]::RootElement
$proc = Get-Process -Name "VEMS*" -ErrorAction SilentlyContinue | Select-Object -First 1
if (-not $proc) { Write-Error "Not running"; exit 1 }
$window = $root.FindFirst([System.Windows.Automation.TreeScope]::Children,
    (New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::ProcessIdProperty, $proc.Id)))
$mainHwnd = $window.Current.NativeWindowHandle; $window.SetFocus(); Start-Sleep -Milliseconds 300

$path = "c:\Users\Weilun Xu\Desktop\VSCODE test\ai-runs\test6-25\code\RCWA2D_Simple_20260625_130522.cs"
$code = [System.IO.File]::ReadAllText($path)
Write-Host "Script: $($code.Length) chars"

[System.Windows.Forms.SendKeys]::SendWait("^n"); Start-Sleep -Milliseconds 600

$dc = New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::ControlTypeProperty, [System.Windows.Automation.ControlType]::Document)
$editor = $null
foreach ($d in $window.FindAll([System.Windows.Automation.TreeScope]::Descendants, $dc)) {
    $b = $d.Current.BoundingRectangle
    try { $d.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern) | Out-Null; if ($b.Width -gt 500) { $editor = $d; break } } catch {}
}
if (-not $editor) { Write-Error "No editor"; exit 1 }
$editor.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern).SetValue($code)
Write-Host "Loaded"

Start-Sleep -Milliseconds 300
$tc = New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::ControlTypeProperty, [System.Windows.Automation.ControlType]::TabItem)
foreach ($t in $window.FindAll([System.Windows.Automation.TreeScope]::Descendants, $tc)) {
    if ($t.Current.Name -eq "Code") { try { $t.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern).Select(); break } catch {} }
}
Start-Sleep -Milliseconds 300

$bc = New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::ControlTypeProperty, [System.Windows.Automation.ControlType]::Button)
$runBtn = $null
foreach ($b in $window.FindAll([System.Windows.Automation.TreeScope]::Descendants, $bc)) {
    if ($b.Current.Name -eq "Run code") { $runBtn = $b; break }
}
if (-not $runBtn) { Write-Error "No run"; exit 1 }
Write-Host "Running..."
$runBtn.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke()
Start-Sleep -Milliseconds 15000

# Close VFrames
$wc = New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::ControlTypeProperty, [System.Windows.Automation.ControlType]::Window)
$closed = 0
foreach ($w in $root.FindAll([System.Windows.Automation.TreeScope]::Children, $wc)) {
    if ($w.Current.ProcessId -eq $proc.Id -and $w.Current.NativeWindowHandle -ne $mainHwnd) {
        try { $w.GetCurrentPattern([System.Windows.Automation.WindowPattern]::Pattern).Close(); $closed++; Start-Sleep -Milliseconds 200 } catch {}
    }
}
Write-Host "Closed $closed VFrames"
