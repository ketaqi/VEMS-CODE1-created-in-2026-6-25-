Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes

$root = [System.Windows.Automation.AutomationElement]::RootElement
$proc = Get-Process -Name "VEMS*" -ErrorAction SilentlyContinue | Select-Object -First 1
if (-not $proc) { Write-Host "WorkBench not running"; exit 0 }

$cond = New-Object System.Windows.Automation.PropertyCondition(
    [System.Windows.Automation.AutomationElement]::ProcessIdProperty, $proc.Id)
$main = $root.FindFirst([System.Windows.Automation.TreeScope]::Children, $cond)
if (-not $main) { Write-Host "Main window not found"; exit 0 }
$mainHwnd = $main.Current.NativeWindowHandle

# Find VFrame child windows
$wc = New-Object System.Windows.Automation.PropertyCondition(
    [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
    [System.Windows.Automation.ControlType]::Window)
$all = $root.FindAll([System.Windows.Automation.TreeScope]::Children, $wc)

$closed = 0
foreach ($w in $all) {
    if ($w.Current.ProcessId -eq $proc.Id -and
        $w.Current.NativeWindowHandle -ne $mainHwnd -and
        $w.Current.IsEnabled) {

        $name = $w.Current.Name
        $hwnd = $w.Current.NativeWindowHandle
        Write-Host "Closing: $name [hwnd=$hwnd]"

        # Try to close via WindowPattern
        $wp = $null
        try { $wp = $w.GetCurrentPattern([System.Windows.Automation.WindowPattern]::Pattern) } catch {}
        if ($wp) {
            $wp.Close()
            $closed++
            Write-Host "  Closed via WindowPattern"
            Start-Sleep -Milliseconds 300
        }
    }
}

if ($closed -eq 0) {
    Write-Host "No VFrame windows found"
} else {
    Write-Host "Closed $closed VFrame window(s)"
}
