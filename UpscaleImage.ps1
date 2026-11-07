param(
    [Parameter(Mandatory=$true)][string]$InputPath,
    [Parameter(Mandatory=$true)][string]$OutputPath,
    [float]$ScaleFactor = 2.5
)

# Ensure .NET System.Drawing is available
Add-Type -AssemblyName System.Drawing

if (-not (Test-Path $InputPath)) {
    Write-Error "Input file not found: $InputPath"
    exit 1
}

try {
    $src = [System.Drawing.Bitmap]::FromFile($InputPath)
    $newWidth = [int]($src.Width * $ScaleFactor)
    $newHeight = [int]($src.Height * $ScaleFactor)
    $scaled = New-Object System.Drawing.Bitmap $newWidth, $newHeight
    $g = [System.Drawing.Graphics]::FromImage($scaled)
    $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $g.DrawImage($src, 0, 0, $newWidth, $newHeight)
    $g.Dispose()
    $src.Dispose()
    $scaled.Save($OutputPath, [System.Drawing.Imaging.ImageFormat]::Png)
    $scaled.Dispose()
    Write-Output "Enhanced image saved to $OutputPath"
} catch {
    Write-Error "Error during processing: $_"
    exit 1
}
