Add-Type -AssemblyName System.Windows.Forms

# Script to run
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

Set-Clipboard -Value $code

# Activate WorkBench via its process
$proc = Get-Process -Name "VEMS*" -ErrorAction SilentlyContinue | Select-Object -First 1
if (-not $proc) { Write-Error "WorkBench not running"; exit 1 }

# Use .NET Process to bring window to front
Add-Type @"
using System;
using System.Runtime.InteropServices;
public class Act {
    [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] public static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);
    [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
    [DllImport("kernel32.dll")] public static extern uint GetCurrentThreadId();
}
"@

$hwnd = $proc.MainWindowHandle
Write-Host "WorkBench hwnd=$hwnd"

# Force activation via AttachThreadInput trick
$fgThread = [Act]::GetWindowThreadProcessId([Act]::GetForegroundWindow(), [ref]$null)
$ourThread = [Act]::GetCurrentThreadId()
[Act]::AttachThreadInput($ourThread, $fgThread, $true) | Out-Null
[Act]::SetForegroundWindow($hwnd) | Out-Null
[Act]::AttachThreadInput($ourThread, $fgThread, $false) | Out-Null
Start-Sleep -Milliseconds 500

Write-Host "Activated. Sending keys via SendKeys..."

# Ctrl+N → new document
[System.Windows.Forms.SendKeys]::SendWait("^n")
Start-Sleep -Milliseconds 800

# Ctrl+A → select all
[System.Windows.Forms.SendKeys]::SendWait("^a")
Start-Sleep -Milliseconds 400

# Paste the script (clipboard has it via Set-Clipboard, but let SendKeys use Ctrl+V)
[System.Windows.Forms.SendKeys]::SendWait("^v")
Start-Sleep -Milliseconds 1000

# Ctrl+F5 → Run
Write-Host "Sending Ctrl+F5 (RUN)..."
[System.Windows.Forms.SendKeys]::SendWait("^{F5}")

Write-Host "Done!"
