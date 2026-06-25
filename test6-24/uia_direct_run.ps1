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

# === Helpers ===
function Find-Button($name) {
    $btnCond = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
        [System.Windows.Automation.ControlType]::Button)
    $btns = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants, $btnCond)
    foreach ($b in $btns) { if ($b.Current.Name -eq $name) { return $b } }
    return $null
}

function Select-CodeTab {
    $tabCond = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
        [System.Windows.Automation.ControlType]::TabItem)
    $tabs = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants, $tabCond)
    foreach ($t in $tabs) {
        if ($t.Current.Name -eq "Code") {
            try { $t.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern).Select(); return $true } catch { return $false }
        }
    }
    return $false
}

# === File paths ===
$fileName = "SPW_Propagation_1D.cs"
$filePath = "c:\Users\Weilun Xu\Desktop\VSCODE test\ai-runs\test6-24\code\$fileName"

# === Step 0: Write script file to disk first ===
$scriptContent = @'
// FreeSpace SPW 1D Propagation
var nx = 151; var dx = 1.0E-6;
var g = new GridInfo1D(n: nx, spacing: dx);
var det = new Detector1D(grid: g);
var w = 7.5E-6; var wavelength = 632.8E-9;

var v = new SCField1D.PlaneWave(
    wavelength: wavelength,
    material: new FuncMaterial(nReal: 1.0),
    diameter: 10.0 * w,
    grid: g, edge: 2.0 * w);

var vIn = det.Sample(v: v, quantity: DetectQuantity.Magnitude);
VFrame.CreateShow(values: vIn,
    title: "Input |E| Field",
    xLabel: "x [um]", yLabel: "|E| [A.U.]",
    grid: det.GridInfo * 1E3);

var sw = System.Diagnostics.Stopwatch.StartNew();
v.Propagate(d: 675E-6, targetDomain: ModelingDomain.Spatial, loopMode: LoopMode.Parallel);
sw.Stop();

var vOut = det.Sample(v: v, quantity: DetectQuantity.Magnitude);
VFrame.CreateShow(values: vOut,
    title: $"Output |E| Field (z=675um, {sw.Elapsed.TotalMilliseconds:F1}ms)",
    xLabel: "x [um]", yLabel: "|E| [A.U.]",
    grid: det.GridInfo * 1E3);

var vArg = det.Sample(v: v, quantity: DetectQuantity.Argument);
VFrame.CreateShow(values: vArg,
    title: "Output Argument",
    xLabel: "x [um]", yLabel: "Arg [rad]",
    grid: det.GridInfo * 1E3);
'@

[System.IO.File]::WriteAllText($filePath, $scriptContent)
Write-Host "0. Script written to: $filePath"

# === Step 1: Open file in WorkBench ===
# "Open file" is in the Quick Access Toolbar / File tab, NOT in the Code tab
# List all buttons to find it
Write-Host "Scanning all buttons..."
$btnCond = New-Object System.Windows.Automation.PropertyCondition(
    [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
    [System.Windows.Automation.ControlType]::Button)
$allBtns = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants, $btnCond)
$openBtn = $null
foreach ($b in $allBtns) {
    if ($b.Current.Name -eq "Open file") { $openBtn = $b; break }
}
if (-not $openBtn) {
    # Maybe it's in the File menu tab - try selecting File first
    $tabCond = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
        [System.Windows.Automation.ControlType]::TabItem)
    $tabs = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants, $tabCond)
    foreach ($t in $tabs) {
        if ($t.Current.Name -eq "File") {
            try { $t.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern).Select(); break } catch {}
        }
    }
    Start-Sleep -Milliseconds 500
    $allBtns = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants, $btnCond)
    foreach ($b in $allBtns) {
        if ($b.Current.Name -eq "Open file") { $openBtn = $b; break }
    }
}
if (-not $openBtn) { Write-Error "Open file button not found"; exit 1 }
Write-Host "1. Clicking Open file..."
$openBtn.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke()
Start-Sleep -Milliseconds 800

# Navigate file dialog: Alt+D to address bar, paste path, Enter
Set-Clipboard -Value $filePath
[System.Windows.Forms.SendKeys]::SendWait("%d")
Start-Sleep -Milliseconds 400
[System.Windows.Forms.SendKeys]::SendWait("^v")
Start-Sleep -Milliseconds 400
[System.Windows.Forms.SendKeys]::SendWait("{ENTER}")
Start-Sleep -Milliseconds 2000
Write-Host "   File opened"

# === Step 2: Run ===
Select-CodeTab | Out-Null
Start-Sleep -Milliseconds 300

$runBtn = Find-Button "Run code"
if (-not $runBtn) { Write-Error "Run code button not found"; exit 1 }
Write-Host "2. Running script..."
$runBtn.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke()
Start-Sleep -Milliseconds 3000
Write-Host "   Done"

# === Step 3: Save (just Ctrl+S since file path is already known) ===
Write-Host "3. Saving..."
$saveBtn = Find-Button "Save file"
if ($saveBtn) {
    $saveBtn.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke()
} else {
    [System.Windows.Forms.SendKeys]::SendWait("^s")
}
Start-Sleep -Milliseconds 500
Write-Host "   Saved: $filePath"

Write-Host "=== Done ==="
