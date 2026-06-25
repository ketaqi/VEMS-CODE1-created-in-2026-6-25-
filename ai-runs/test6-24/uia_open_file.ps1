Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes
Add-Type -AssemblyName System.Windows.Forms

# Find WorkBench window
$root = [System.Windows.Automation.AutomationElement]::RootElement
$cond = New-Object System.Windows.Automation.PropertyCondition(
    [System.Windows.Automation.AutomationElement]::ProcessIdProperty, 52636)
$window = $root.FindFirst([System.Windows.Automation.TreeScope]::Children, $cond)
if (-not $window) {
    # Try by name
    $proc = Get-Process -Name "VEMS*" -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($proc) {
        $cond = New-Object System.Windows.Automation.PropertyCondition(
            [System.Windows.Automation.AutomationElement]::ProcessIdProperty, $proc.Id)
        $window = $root.FindFirst([System.Windows.Automation.TreeScope]::Children, $cond)
    }
}
if (-not $window) { Write-Error "WorkBench not found"; exit 1 }
Write-Host "Found: $($window.Current.Name)"

# Bring to front
$window.SetFocus()
Start-Sleep -Milliseconds 400

# Helper to find button by name
function Find-Button($name) {
    $btnCond = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
        [System.Windows.Automation.ControlType]::Button)
    $btns = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants, $btnCond)
    foreach ($b in $btns) {
        if ($b.Current.Name -eq $name) { return $b }
    }
    return $null
}

# Step 1: Click "Open file" button
$openBtn = Find-Button "Open file"
if (-not $openBtn) { Write-Error "Open file button not found"; exit 1 }
Write-Host "Clicking 'Open file'..."
$openBtn.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke()
Start-Sleep -Milliseconds 1000

# Step 2: Find the file dialog
$dialogCond = New-Object System.Windows.Automation.PropertyCondition(
    [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
    [System.Windows.Automation.ControlType]::Window)
$allWindows = $root.FindAll([System.Windows.Automation.TreeScope]::Children, $dialogCond)

$dialog = $null
foreach ($w in $allWindows) {
    $name = $w.Current.Name
    if ($name -match "Open|打开" -and $w.Current.ProcessId -eq 52636) {
        $dialog = $w
        Write-Host "Found file dialog: '$name'"
        break
    }
}

if (-not $dialog) {
    Write-Host "File dialog not found by name, listing all WorkBench child windows:"
    foreach ($w in $allWindows) {
        if ($w.Current.ProcessId -eq 52636) {
            Write-Host "  PID=$($w.Current.ProcessId) '$($w.Current.Name)' class=$($w.Current.ClassName)"
        }
    }
}

# Step 3: Navigate file dialog with keyboard
$scriptPath = "c:\Users\Weilun Xu\Desktop\VSCODE test\VEMS\VEMS.WorkBench\_sample\ai_generated\SPW_Propagation_1D.cs"
Set-Clipboard -Value $scriptPath
Write-Host "Path in clipboard"

# Send Alt+D to focus address bar, then paste
[System.Windows.Forms.SendKeys]::SendWait("%d")
Start-Sleep -Milliseconds 400
[System.Windows.Forms.SendKeys]::SendWait("^v")
Start-Sleep -Milliseconds 400
[System.Windows.Forms.SendKeys]::SendWait("{ENTER}")
Start-Sleep -Milliseconds 1500

# Step 4: Find and click "Run code" button
Write-Host "Looking for Run code button..."
$runBtn = Find-Button "Run code"
if ($runBtn) {
    Write-Host "Clicking 'Run code'..."
    $runBtn.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke()
    Write-Host "Done! Check for VFrame windows."
} else {
    Write-Host "Run code not found, trying Ctrl+F5..."
    [System.Windows.Forms.SendKeys]::SendWait("^{F5}")
    Write-Host "Done."
}
