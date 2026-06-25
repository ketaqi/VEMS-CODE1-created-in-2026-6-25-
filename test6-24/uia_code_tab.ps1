Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes

$root = [System.Windows.Automation.AutomationElement]::RootElement
$procCond = New-Object System.Windows.Automation.PropertyCondition(
    [System.Windows.Automation.AutomationElement]::ProcessIdProperty, 52636)
$window = $root.FindFirst([System.Windows.Automation.TreeScope]::Children, $procCond)

if (-not $window) { Write-Error "Window not found"; exit 1 }

# Find the "Code" tab and select it
$tabCond = New-Object System.Windows.Automation.PropertyCondition(
    [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
    [System.Windows.Automation.ControlType]::TabItem)
$tabs = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants, $tabCond)

foreach ($t in $tabs) {
    if ($t.Current.Name -eq "Code") {
        Write-Host "Selecting Code tab..."
        $selPattern = $t.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern)
        if ($selPattern) {
            $selPattern.Select()
            Write-Host "Code tab selected!"
        }
        Start-Sleep -Milliseconds 500
        break
    }
}

# Find ALL controls of interest within the Code tab content
# Look for Document/Edit controls (RoslynPad editor)
$docCond = New-Object System.Windows.Automation.PropertyCondition(
    [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
    [System.Windows.Automation.ControlType]::Document)
$docs = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants, $docCond)
Write-Host "Found $($docs.Count) Document controls:"
foreach ($d in $docs) {
    $b = $d.Current.BoundingRectangle
    Write-Host "  '$($d.Current.Name)' class='$($d.Current.ClassName)' autoId='$($d.Current.AutomationId)' rect=($b.Width x $b.Height)"
}

$editCond = New-Object System.Windows.Automation.PropertyCondition(
    [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
    [System.Windows.Automation.ControlType]::Edit)
$edits = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants, $editCond)
Write-Host "Found $($edits.Count) Edit controls:"
foreach ($e in $edits) {
    $b = $e.Current.BoundingRectangle
    $v = ""
    try { $vp = $e.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern); if ($vp) { $v = $vp.Current.Value.Substring(0, [Math]::Min(80, $vp.Current.Value.Length)) } } catch {}
    Write-Host "  '$($e.Current.Name)' class='$($e.Current.ClassName)' rect=($b.Width x $b.Height) value='$v'"
}

# Find ALL buttons (toolbar)
$btnCond = New-Object System.Windows.Automation.PropertyCondition(
    [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
    [System.Windows.Automation.ControlType]::Button)
$btns = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants, $btnCond)
Write-Host "Found $($btns.Count) Buttons:"
foreach ($b in $btns) {
    $name = $b.Current.Name
    $autoId = $b.Current.AutomationId
    $bnd = $b.Current.BoundingRectangle
    if ($name.Length -gt 0 -and $bnd.Width -gt 10) {
        $invokePattern = $b.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern)
        Write-Host "  '$name' autoId='$autoId' rect=($($bnd.Width)x$($bnd.Height)) invoke=$($invokePattern -ne $null)"
    }
}

# Find Ribbon tabs
$ribbonCond = New-Object System.Windows.Automation.PropertyCondition(
    [System.Windows.Automation.AutomationElement]::ClassNameProperty, "Ribbon*")
#$ribbons = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants, $ribbonCond)
#Write-Host "Ribbon controls: $($ribbons.Count)"
