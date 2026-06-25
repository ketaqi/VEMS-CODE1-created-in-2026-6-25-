Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes

$root = [System.Windows.Automation.AutomationElement]::RootElement

# Dynamic PID
$proc = Get-Process -Name "VEMS*" -ErrorAction SilentlyContinue | Select-Object -First 1
if (-not $proc) { Write-Error "Not running"; exit 1 }
$wpid = $proc.Id

$cond = New-Object System.Windows.Automation.PropertyCondition(
    [System.Windows.Automation.AutomationElement]::ProcessIdProperty, $wpid)
$window = $root.FindFirst([System.Windows.Automation.TreeScope]::Children, $cond)

if (-not $window) {
    # Try by name pattern
    $nameCond = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::NameProperty, "VEMS WorkBench*")
    $window = $root.FindFirst([System.Windows.Automation.TreeScope]::Children, $nameCond)
}

if (-not $window) {
    Write-Error "WorkBench window not found via PID or name"
    exit 1
}
Write-Host "Window: $($window.Current.Name) PID=$wpid"
$window.SetFocus()
Start-Sleep -Milliseconds 300

# Find ALL controls with size > 100x100 (likely editor panels)
Write-Host ""
Write-Host "=== Large controls (>100x100) ==="
$allCond = [System.Windows.Automation.Condition]::TrueCondition
$all = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants, $allCond)
foreach ($c in $all) {
    $b = $c.Current.BoundingRectangle
    $w = $b.Width; $h = $b.Height
    if ($w -gt 100 -and $h -gt 100) {
        $name = $c.Current.Name
        $class = $c.Current.ClassName
        $ctrl = $c.Current.ControlType.ProgrammaticName
        $autoId = $c.Current.AutomationId
        $isEnabled = $c.Current.IsEnabled

        # Check supported patterns
        $patterns = @()
        try { if ($c.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern)) { $patterns += "Value" } } catch {}
        try { if ($c.GetCurrentPattern([System.Windows.Automation.TextPattern]::Pattern)) { $patterns += "Text" } } catch {}
        try { if ($c.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern)) { $patterns += "Invoke" } } catch {}
        try { if ($c.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern)) { $patterns += "Select" } } catch {}

        Write-Host "  [$($w)x$h] class='$class' name='$name' ctrl=$ctrl autoId='$autoId' enabled=$isEnabled patterns=$($patterns -join ',')"
    }
}

# Specifically look for AvalonEdit / RoslynPad controls
Write-Host ""
Write-Host "=== Looking for RoslynPad/AvalonEdit ==="
$textEdits = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants,
    (New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::IsTextPatternAvailableProperty, $true)))
Write-Host "TextPattern-capable controls: $($textEdits.Count)"
foreach ($te in $textEdits) {
    $b = $te.Current.BoundingRectangle
    Write-Host "  [$($b.Width)x$($b.Height)] class='$($te.Current.ClassName)' name='$($te.Current.Name)' autoId='$($te.Current.AutomationId)'"
}
