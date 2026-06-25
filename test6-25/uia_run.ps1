Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes
Add-Type -AssemblyName System.Windows.Forms

$root = [System.Windows.Automation.AutomationElement]::RootElement
$proc = Get-Process -Name "VEMS*" -ErrorAction SilentlyContinue | Select-Object -First 1
if (-not $proc) { Write-Error "WorkBench not running"; exit 1 }
$wpid = $proc.Id

$cond = New-Object System.Windows.Automation.PropertyCondition(
    [System.Windows.Automation.AutomationElement]::ProcessIdProperty, $wpid)
$window = $root.FindFirst([System.Windows.Automation.TreeScope]::Children, $cond)
Write-Host "Window: $($window.Current.Name)"
$window.SetFocus()
Start-Sleep -Milliseconds 400

# === Config ===
$fileName = "RCWA1D_5Wavelengths_20260625_124035.cs"
$filePath = "c:\Users\Weilun Xu\Desktop\VSCODE test\ai-runs\test6-25\code\$fileName"
Write-Host "=== $fileName ==="

# === Step 1: Load into editor ===
Write-Host "1. Loading script..."
[System.Windows.Forms.SendKeys]::SendWait("^n")
Start-Sleep -Milliseconds 800

$docCond = New-Object System.Windows.Automation.PropertyCondition(
    [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
    [System.Windows.Automation.ControlType]::Document)
$docs = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants, $docCond)
$editor = $null
foreach ($d in $docs) {
    $b = $d.Current.BoundingRectangle
    $hasValue = $false
    try { $d.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern) | Out-Null; $hasValue = $true } catch {}
    if ($hasValue -and $b.Width -gt 500) { $editor = $d; break }
}
if (-not $editor) {
    Start-Sleep -Milliseconds 1000
    $docs = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants, $docCond)
    foreach ($d in $docs) {
        $b = $d.Current.BoundingRectangle
        $hasValue = $false
        try { $d.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern) | Out-Null; $hasValue = $true } catch {}
        if ($hasValue -and $b.Width -gt 500) { $editor = $d; break }
    }
}
if (-not $editor) { Write-Error "Editor not found"; exit 1 }

$scriptContent = [System.IO.File]::ReadAllText($filePath)
$vp = $editor.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern)
$vp.SetValue($scriptContent)
Write-Host "   Loaded ($($scriptContent.Length) chars)"

# === Step 2: Run ===
Start-Sleep -Milliseconds 500
$tabCond = New-Object System.Windows.Automation.PropertyCondition(
    [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
    [System.Windows.Automation.ControlType]::TabItem)
$tabs = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants, $tabCond)
foreach ($t in $tabs) {
    if ($t.Current.Name -eq "Code") {
        try { $t.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern).Select(); break } catch {}
    }
}
Start-Sleep -Milliseconds 400

$btnCond = New-Object System.Windows.Automation.PropertyCondition(
    [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
    [System.Windows.Automation.ControlType]::Button)
$btns = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants, $btnCond)
$runBtn = $null
foreach ($b in $btns) { if ($b.Current.Name -eq "Run code") { $runBtn = $b; break } }
if (-not $runBtn) { Write-Error "Run code not found"; exit 1 }

Write-Host "2. Running (this will take ~30s for 5 wavelengths)..."
$runBtn.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke()
Start-Sleep -Milliseconds 35000
Write-Host "   Done"

Write-Host "=== Complete ==="
Write-Host "Output: ai-runs/test6-25/output/images/"
