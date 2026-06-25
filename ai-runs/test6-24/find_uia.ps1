Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes

$root = [System.Windows.Automation.AutomationElement]::RootElement

# List ALL top-level windows
Write-Host "=== All top-level windows ==="
$all = $root.FindAll([System.Windows.Automation.TreeScope]::Children, [System.Windows.Automation.Condition]::TrueCondition)
foreach ($w in $all) {
    $name = $w.Current.Name
    $ctrl = $w.Current.ControlType.ProgrammaticName
    $pid = $w.Current.ProcessId
    $class = $w.Current.ClassName
    if ($name.Length -gt 0 -or $ctrl -match "Window") {
        Write-Host "PID=$pid Class='$class' Name='$name' Type=$ctrl"
    }
}

# Also try by process
Write-Host ""
Write-Host "=== Find by Process ID 52456 ==="
$procCond = New-Object System.Windows.Automation.PropertyCondition(
    [System.Windows.Automation.AutomationElement]::ProcessIdProperty, 52456)
$procWindows = $root.FindAll([System.Windows.Automation.TreeScope]::Children, $procCond)
foreach ($w in $procWindows) {
    Write-Host "Name='$($w.Current.Name)' Class='$($w.Current.ClassName)' Type=$($w.Current.ControlType.ProgrammaticName)"
}
