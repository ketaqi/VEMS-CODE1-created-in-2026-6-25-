Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes

$root = [System.Windows.Automation.AutomationElement]::RootElement
$proc = Get-Process -Name "VEMS*" -ErrorAction SilentlyContinue | Select-Object -First 1
if (-not $proc) { Write-Error "WorkBench not running"; exit 1 }
$wpid = $proc.Id

$cond = New-Object System.Windows.Automation.PropertyCondition(
    [System.Windows.Automation.AutomationElement]::ProcessIdProperty, $wpid)
$window = $root.FindFirst([System.Windows.Automation.TreeScope]::Children, $cond)
Write-Host "Window: $($window.Current.Name) PID=$wpid"

# Bring to front
$window.SetFocus()
Start-Sleep -Milliseconds 500

# Find the RoslynPad Document control (the one with Value+Text pattern, ~1121x428)
$docCond = New-Object System.Windows.Automation.PropertyCondition(
    [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
    [System.Windows.Automation.ControlType]::Document)
$docs = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants, $docCond)

$editor = $null
foreach ($d in $docs) {
    $b = $d.Current.BoundingRectangle
    $hasValue = $false
    try { $d.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern) | Out-Null; $hasValue = $true } catch {}
    if ($hasValue -and $b.Width -gt 500) {
        $editor = $d
        Write-Host "Found editor: [$($b.Width)x$($b.Height)]"
        break
    }
}

if (-not $editor) { Write-Error "Editor not found"; exit 1 }

# Set script content directly via ValuePattern
$script = @'
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

Write-Host "Setting script text via ValuePattern..."
$vp = $editor.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern)
$vp.SetValue($script)
Write-Host "Script loaded! ($($script.Length) chars)"
Start-Sleep -Milliseconds 500

# Select the "Code" ribbon tab first (buttons are on this tab)
$tabCond = New-Object System.Windows.Automation.PropertyCondition(
    [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
    [System.Windows.Automation.ControlType]::TabItem)
$tabs = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants, $tabCond)
foreach ($t in $tabs) {
    if ($t.Current.Name -eq "Code") {
        Write-Host "Selecting Code tab..."
        try {
            $sel = $t.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern)
            $sel.Select()
            Write-Host "Code tab selected!"
        } catch { Write-Host "Could not select Code tab: $_" }
        break
    }
}
Start-Sleep -Milliseconds 500

# First, let's list all available buttons for debugging
Write-Host "Available buttons:"
$btnCond = New-Object System.Windows.Automation.PropertyCondition(
    [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
    [System.Windows.Automation.ControlType]::Button)
$btns = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants, $btnCond)
$runBtn = $null
foreach ($b in $btns) {
    $name = $b.Current.Name
    $bnd = $b.Current.BoundingRectangle
    if ($name.Length -gt 0 -and $bnd.Width -gt 5) {
        Write-Host "  '$name' [$($bnd.Width)x$($bnd.Height)]"
        if ($name -eq "Run code") { $runBtn = $b }
        # Also check for Debug code (right next to Run code)
        if ($name -eq "Debug code") { $debugBtn = $b }
    }
}

# If "Run code" not directly found, try alternative approach
if (-not $runBtn) {
    Write-Host "Run code button not in list, trying Ctrl+F5..."
    Start-Sleep -Milliseconds 300
    Add-Type -AssemblyName System.Windows.Forms
    [System.Windows.Forms.SendKeys]::SendWait("^{F5}")
} else {
    Write-Host "Clicking 'Run code' button..."
    $runBtn.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke()
}
