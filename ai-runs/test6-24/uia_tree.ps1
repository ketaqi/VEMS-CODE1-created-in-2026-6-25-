Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes

$root = [System.Windows.Automation.AutomationElement]::RootElement

# Find WorkBench window by process ID
$procCond = New-Object System.Windows.Automation.PropertyCondition(
    [System.Windows.Automation.AutomationElement]::ProcessIdProperty, 52636)
$window = $root.FindFirst([System.Windows.Automation.TreeScope]::Children, $procCond)

if (-not $window) {
    # Try all top-level windows
    Write-Host "Searching all windows for WorkBench..."
    $all = $root.FindAll([System.Windows.Automation.TreeScope]::Children, [System.Windows.Automation.Condition]::TrueCondition)
    foreach ($w in $all) {
        if ($w.Current.Name -match "VEMS") {
            $window = $w
            Write-Host "Found: $($w.Current.Name)"
            break
        }
    }
}

if (-not $window) { Write-Error "Window not found"; exit 1 }

Write-Host "Window: $($window.Current.Name)"
Write-Host "PID: $($window.Current.ProcessId)"
Write-Host ""

# Find ALL TreeViewItems (the file tree)
$treeCond = New-Object System.Windows.Automation.PropertyCondition(
    [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
    [System.Windows.Automation.ControlType]::TreeItem)
$treeItems = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants, $treeCond)

Write-Host "Found $($treeItems.Count) TreeItems:"
foreach ($ti in $treeItems) {
    $name = $ti.Current.Name
    if ($name.Length -gt 0) {
        Write-Host "  '$name'"
        if ($name -match "SPW_Propagation|ai_generated") {
            Write-Host "  *** TARGET FOUND ***"
            $target = $ti
        }
    }
}

# Find TabItems (editor tabs)
Write-Host ""
$tabCond = New-Object System.Windows.Automation.PropertyCondition(
    [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
    [System.Windows.Automation.ControlType]::TabItem)
$tabs = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants, $tabCond)
Write-Host "Found $($tabs.Count) TabItems:"
foreach ($t in $tabs) { Write-Host "  '$($t.Current.Name)'" }

# Find Buttons (menu bar, toolbar)
Write-Host ""
$btnCond = New-Object System.Windows.Automation.PropertyCondition(
    [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
    [System.Windows.Automation.ControlType]::Button)
$btns = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants, $btnCond)
Write-Host "Found $($btns.Count) Buttons:"
foreach ($b in $btns) {
    $name = $b.Current.Name
    if ($name.Length -gt 0) { Write-Host "  '$name'" }
}

# Find MenuItems
Write-Host ""
$menuCond = New-Object System.Windows.Automation.PropertyCondition(
    [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
    [System.Windows.Automation.ControlType]::MenuItem)
$menus = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants, $menuCond)
Write-Host "Found $($menus.Count) MenuItems:"
foreach ($m in $menus) {
    $name = $m.Current.Name
    if ($name.Length -gt 0) { Write-Host "  '$name'" }
}
