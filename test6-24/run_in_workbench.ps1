Add-Type -TypeDefinition @"
using System;
using System.Runtime.InteropServices;
using System.Text;
public class KA {
    [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")] public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);
    [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] public static extern bool BringWindowToTop(IntPtr hWnd);
    [DllImport("user32.dll")] public static extern int MapVirtualKey(uint uCode, uint uMapType);
}
"@

$VK_CONTROL = 0x11
$VK_V = 0x56
$VK_A = 0x41
$VK_N = 0x4E
$VK_F5 = 0x74
$KEYEVENTF_KEYUP = 0x0002

function KDown($vk) { [KA]::keybd_event($vk, 0, 0, [UIntPtr]::Zero); Start-Sleep -Milliseconds 20 }
function KUp($vk) { [KA]::keybd_event($vk, 0, $KEYEVENTF_KEYUP, [UIntPtr]::Zero); Start-Sleep -Milliseconds 20 }
function Combo($mod, $key) {
    KDown $mod; Start-Sleep -Milliseconds 40
    KDown $key; Start-Sleep -Milliseconds 50
    KUp $key; Start-Sleep -Milliseconds 30
    KUp $mod; Start-Sleep -Milliseconds 200
}
function KTap($vk) { KDown $vk; Start-Sleep -Milliseconds 50; KUp $vk; Start-Sleep -Milliseconds 100 }

# Script content
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

// Input field
var vIn = det.Sample(v: v, quantity: DetectQuantity.Magnitude);
VFrame.CreateShow(values: vIn,
    title: "Input |E| Field",
    xLabel: "x [um]", yLabel: "|E| [A.U.]",
    grid: det.GridInfo * 1E3);

// Propagate 675 um
var sw = System.Diagnostics.Stopwatch.StartNew();
v.Propagate(d: 675E-6, targetDomain: ModelingDomain.Spatial, loopMode: LoopMode.Parallel);
sw.Stop();

// Output field
var vOut = det.Sample(v: v, quantity: DetectQuantity.Magnitude);
VFrame.CreateShow(values: vOut,
    title: $"Output |E| Field (z=675um, {sw.Elapsed.TotalMilliseconds:F1}ms)",
    xLabel: "x [um]", yLabel: "|E| [A.U.]",
    grid: det.GridInfo * 1E3);

// Argument
var vArg = det.Sample(v: v, quantity: DetectQuantity.Argument);
VFrame.CreateShow(values: vArg,
    title: "Output Argument",
    xLabel: "x [um]", yLabel: "Arg [rad]",
    grid: det.GridInfo * 1E3);
'@

Set-Clipboard -Value $code
Write-Host "Script copied to clipboard"

# Find WorkBench
$proc = Get-Process -Name "VEMS*" -ErrorAction SilentlyContinue | Select-Object -First 1
$hwnd = $proc.MainWindowHandle
Write-Host "WorkBench hwnd=$hwnd"

# Activate
[KA]::ShowWindow($hwnd, 9) | Out-Null
[KA]::BringWindowToTop($hwnd) | Out-Null
Start-Sleep -Milliseconds 300
[KA]::SetForegroundWindow($hwnd) | Out-Null
Start-Sleep -Milliseconds 600

# Verify foreground
$fg = [KA]::GetForegroundWindow()
Write-Host "Foreground: $fg (expect $hwnd)"

# Step 1: Ctrl+N → New empty document
Write-Host "1. New doc (Ctrl+N)..."
Combo $VK_CONTROL $VK_N
Start-Sleep -Milliseconds 600

# Step 2: Ctrl+A → Select all (clear any default template)
Write-Host "2. Select all (Ctrl+A)..."
Combo $VK_CONTROL $VK_A
Start-Sleep -Milliseconds 400

# Step 3: Ctrl+V → Paste script
Write-Host "3. Paste script (Ctrl+V)..."
Combo $VK_CONTROL $VK_V
Start-Sleep -Milliseconds 800

# Step 4: Ctrl+F5 → RUN
Write-Host "4. Run (Ctrl+F5)..."
Combo $VK_CONTROL $VK_F5

Write-Host "Done!"
