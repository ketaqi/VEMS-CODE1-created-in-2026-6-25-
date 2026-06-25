Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes

$root = [System.Windows.Automation.AutomationElement]::RootElement
$proc = Get-Process -Name "VEMS*" -ErrorAction SilentlyContinue | Select-Object -First 1
if (-not $proc) { Write-Host "Not running"; exit 0 }

$cond = New-Object System.Windows.Automation.PropertyCondition(
    [System.Windows.Automation.AutomationElement]::ProcessIdProperty, $proc.Id)
$window = $root.FindFirst([System.Windows.Automation.TreeScope]::Children, $cond)

# List ALL edit/text controls with their content
$editCond = New-Object System.Windows.Automation.PropertyCondition(
    [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
    [System.Windows.Automation.ControlType]::Edit)

Write-Host "=== Edit controls ==="
foreach ($e in $window.FindAll([System.Windows.Automation.TreeScope]::Descendants, $editCond)) {
    $b = $e.Current.BoundingRectangle
    $autoId = $e.Current.AutomationId
    try {
        $vp = $e.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern)
        $val = $vp.Current.Value
        Write-Host "[${b.Width}x${b.Height}] autoId='$autoId' len=$($val.Length)"
        if ($val.Length -gt 0 -and $val.Length -lt 1000) {
            Write-Host "  CONTENT: $val"
        }
    } catch {
        Write-Host "[${b.Width}x${b.Height}] autoId='$autoId' (no ValuePattern)"
    }
}

# Also check Document controls
Write-Host ""
Write-Host "=== Document controls ==="
$docCond = New-Object System.Windows.Automation.PropertyCondition(
    [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
    [System.Windows.Automation.ControlType]::Document)
foreach ($d in $window.FindAll([System.Windows.Automation.TreeScope]::Descendants, $docCond)) {
    $b = $d.Current.BoundingRectangle
    try {
        $vp = $d.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern)
        $val = $vp.Current.Value
        Write-Host "[${b.Width}x${b.Height}] len=$($val.Length)"
        if ($val.Length -gt 0 -and $val.Length -lt 500) {
            Write-Host "  CONTENT: $val"
        }
    } catch {
        Write-Host "[${b.Width}x${b.Height}] (no ValuePattern)"
    }
}
