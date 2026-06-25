Get-Process -Name "VEMS*" -ErrorAction SilentlyContinue | ForEach-Object {
    Write-Host "PID: $($_.Id)"
    Write-Host "Title: '$($_.MainWindowTitle)'"
    Write-Host "Handle: $($_.MainWindowHandle)"
}
