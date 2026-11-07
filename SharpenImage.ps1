param(
    [Parameter(Mandatory=$true)][string]$InputPath,
    [Parameter(Mandatory=$true)][string]$OutputPath
)

Add-Type -AssemblyName System.Drawing

if (-not (Test-Path $InputPath)) {
    Write-Error "Input file not found: $InputPath"
    exit 1
}

$bitmap = [System.Drawing.Bitmap]::FromFile($InputPath)

# Define sharpening kernel (3x3)
$kernel = @(
    @(0, -1, 0),
    @(-1, 5, -1),
    @(0, -1, 0)
)
$kernelWeight = 1 # sum is 1

$width = $bitmap.Width
$height = $bitmap.Height
$newBmp = New-Object System.Drawing.Bitmap $width, $height

for ($y = 0; $y -lt $height; $y++) {
    for ($x = 0; $x -lt $width; $x++) {
        $r = $g = $b = 0
        for ($ky = -1; $ky -le 1; $ky++) {
            $ny = $y + $ky
            if ($ny -lt 0 -or $ny -ge $height) { continue }
            for ($kx = -1; $kx -le 1; $kx++) {
                $nx = $x + $kx
                if ($nx -lt 0 -or $nx -ge $width) { continue }
                $color = $bitmap.GetPixel($nx, $ny)
                $factor = $kernel[$ky + 1][$kx + 1]
                $r += $color.R * $factor
                $g += $color.G * $factor
                $b += $color.B * $factor
            }
        }
        # Clamp values
        $r = [math]::Min(255, [math]::Max(0, [int]$r))
        $g = [math]::Min(255, [math]::Max(0, [int]$g))
        $b = [math]::Min(255, [math]::Max(0, [int]$b))
        $newBmp.SetPixel($x, $y, [System.Drawing.Color]::FromArgb($r, $g, $b))
    }
}

$newBmp.Save($OutputPath, [System.Drawing.Imaging.ImageFormat]::Png)
$bitmap.Dispose()
$newBmp.Dispose()
Write-Output "Sharpened image saved to $OutputPath"
