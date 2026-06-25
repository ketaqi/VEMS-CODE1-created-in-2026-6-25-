Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes

# Script content to run
$code = @'
// AI-Generated: FreeSpace SPW 1D
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

# Find the WorkBench window element
$root = [System.Windows.Automation.AutomationElement]::RootElement
$cond = New-Object System.Windows.Automation.PropertyCondition(
    [System.Windows.Automation.AutomationElement]::NameProperty, "VEMS WorkBench [2026624]")
$window = $root.FindFirst([System.Windows.Automation.TreeScope]::Children, $cond)

if (-not $window) {
    # Try partial match
    $cond2 = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::NameProperty, "VEMS WorkBench*")
    $window = $root.FindFirst([System.Windows.Automation.TreeScope]::Children, $cond2)
}

if (-not $window) {
    Write-Error "WorkBench window not found via UIA"
    exit 1
}

Write-Host "Found window: $($window.Current.Name)"
Write-Host "Control type: $($window.Current.ControlType.ProgrammaticName)"
Write-Host ""

# Set focus
$window.SetFocus()
Start-Sleep -Milliseconds 500

# Find all editable text areas (RoslynPad editor)
$editCond = New-Object System.Windows.Automation.PropertyCondition(
    [System.Windows.Automation.AutomationElement]::IsTextPatternAvailableProperty, $true)
$edits = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants, $editCond)

Write-Host "Found $($edits.Count) text-capable elements"

# Find the main editor (largest Document/Edit control)
$best = $null
foreach ($e in $edits) {
    $ctrl = $e.Current.ControlType.ProgrammaticName
    $name = $e.Current.Name
    $b = $e.Current.BoundingRectangle
    $area = $b.Width * $b.Height
    if ($area -gt 10000) {
        Write-Host "  '$name' type=$ctrl area=$area rect=($($b.X),$($b.Y),$($b.Width),$($b.Height))"
        if ($area -gt 50000 -and $ctrl -match "Document|Edit|Window") {
            if ($best -eq $null -or $area -gt $best_area) {
                $best = $e
                $best_area = $area
            }
        }
    }
}

if ($best) {
    Write-Host ""
    Write-Host "Target editor: $($best.Current.Name) area=$best_area"
    $best.SetFocus()
    Start-Sleep -Milliseconds 300

    # Use TextPattern to select all and paste
    $textPattern = $best.GetCurrentPattern([System.Windows.Automation.TextPattern]::Pattern)
    if ($textPattern) {
        $docRange = $textPattern.DocumentRange
        $docRange.Select()
        Start-Sleep -Milliseconds 100

        # Paste via clipboard
        Set-Clipboard -Value $code
        Write-Host "Script copied to clipboard, pasting..."

        # Use ValuePattern to set text directly
        $valuePattern = $best.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern)
        if ($valuePattern) {
            Write-Host "Using ValuePattern to set text..."
            $valuePattern.SetValue($code)
            Write-Host "Script loaded via ValuePattern!"
        }
    } else {
        Write-Host "No TextPattern available"
    }
} else {
    Write-Host "No suitable editor found. Listing all controls:"
    $allCond = [System.Windows.Automation.Condition]::TrueCondition
    $all = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants, $allCond)
    foreach ($e in $all) {
        $b = $e.Current.BoundingRectangle
        if ($b.Width -gt 50 -and $b.Height -gt 20) {
            Write-Host "  '$($e.Current.Name)' type=$($e.Current.ControlType.ProgrammaticName) rect=($($b.X),$($b.Y),$($b.Width)x$($b.Height))"
        }
    }
}
