Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes
Add-Type -AssemblyName System.Windows.Forms

$root = [System.Windows.Automation.AutomationElement]::RootElement
$proc = Get-Process -Name "VEMS*" -ErrorAction SilentlyContinue | Select-Object -First 1
if (-not $proc) { Write-Error "WorkBench not running"; exit 1 }
$window = $root.FindFirst([System.Windows.Automation.TreeScope]::Children,
    (New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::ProcessIdProperty, $proc.Id)))
$mainHwnd = $window.Current.NativeWindowHandle
Write-Host "Window: $($window.Current.Name)"
$window.SetFocus()
Start-Sleep -Milliseconds 300

$ts = "20260625_124328"
$wls = @(193, 248, 365, 436, 532)
$codeDir = "c:\Users\Weilun Xu\Desktop\VSCODE test\ai-runs\test6-25\code"
$imgDir = "c:\Users\Weilun Xu\Desktop\VSCODE test\ai-runs\test6-25\output\images"

# Helper: find editor Document control
function Find-Editor {
    $docCond = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
        [System.Windows.Automation.ControlType]::Document)
    foreach ($d in $window.FindAll([System.Windows.Automation.TreeScope]::Descendants, $docCond)) {
        $b = $d.Current.BoundingRectangle
        try { $d.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern) | Out-Null
              if ($b.Width -gt 500) { return $d } } catch {}
    }
    return $null
}

# Helper: find button
function Find-Button($name) {
    $c = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
        [System.Windows.Automation.ControlType]::Button)
    foreach ($b in $window.FindAll([System.Windows.Automation.TreeScope]::Descendants, $c)) {
        if ($b.Current.Name -eq $name) { return $b }
    }
    return $null
}

# Helper: select Code tab
function Select-CodeTab {
    $c = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
        [System.Windows.Automation.ControlType]::TabItem)
    foreach ($t in $window.FindAll([System.Windows.Automation.TreeScope]::Descendants, $c)) {
        if ($t.Current.Name -eq "Code") {
            try { $t.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern).Select(); return } catch {}
        }
    }
}

foreach ($wl in $wls) {
    $fileName = "RCWA1D_${wl}nm_${ts}.cs"
    $filePath = Join-Path $codeDir $fileName
    Write-Host ""
    Write-Host "=== [$wl nm] ==="

    # Load
    $editor = Find-Editor
    if (-not $editor) {
        [System.Windows.Forms.SendKeys]::SendWait("^n")
        Start-Sleep -Milliseconds 800
        $editor = Find-Editor
    }
    if (-not $editor) { Write-Host "  SKIP: no editor"; continue }

    $scriptContent = [System.IO.File]::ReadAllText($filePath)
    $editor.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern).SetValue($scriptContent)
    Write-Host "  Loaded ($($scriptContent.Length) chars)"

    # Run
    Start-Sleep -Milliseconds 300
    Select-CodeTab
    Start-Sleep -Milliseconds 300

    $runBtn = Find-Button "Run code"
    if (-not $runBtn) { Write-Host "  SKIP: no run button"; continue }
    $runBtn.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke()
    Write-Host "  Running..."

    $waitMs = if ($wl -lt 250) { 8000 } elseif ($wl -lt 400) { 4000 } else { 2000 }
    Start-Sleep -Milliseconds $waitMs

    # Close VFrame windows
    $wc = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
        [System.Windows.Automation.ControlType]::Window)
    $all = $root.FindAll([System.Windows.Automation.TreeScope]::Children, $wc)
    $closed = 0
    foreach ($w in $all) {
        if ($w.Current.ProcessId -eq $proc.Id -and
            $w.Current.NativeWindowHandle -ne $mainHwnd -and
            $w.Current.IsEnabled) {
            $wp = $null
            try { $wp = $w.GetCurrentPattern([System.Windows.Automation.WindowPattern]::Pattern) } catch {}
            if ($wp) { $wp.Close(); $closed++; Start-Sleep -Milliseconds 200 }
        }
    }
    Write-Host "  Closed $closed VFrame(s)"
}

Write-Host ""
Write-Host "=== All 5 processed ==="
