# Simple approach: open the .cs file directly with WorkBench exe as argument
# Many WPF apps support opening files via command line

$scriptPath = "c:\Users\Weilun Xu\Desktop\VSCODE test\VEMS\VEMS.WorkBench\_sample\ai_generated\SPW_Propagation_1D.cs"
$exePath = "c:\Users\Weilun Xu\Desktop\VSCODE test\VEMS\VEMS.WorkBench\bin\x64\Debug\net8.0-windows7.0\VEMS.WorkBench.exe"

# Try opening file with WorkBench
Write-Host "Trying to open: $scriptPath"
Start-Process -FilePath $exePath -ArgumentList "`"$scriptPath`"" -Wait:$false
Write-Host "Done. If WorkBench supports file args, the script should open."
