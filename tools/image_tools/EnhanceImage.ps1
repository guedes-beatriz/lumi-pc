param(
    [Parameter(Mandatory=$true)][string]$InputPath,
    [Parameter(Mandatory=$true)][string]$OutputPath,
    [float]$ScaleFactor = 2.5,
    [int]$BlurRadius = 2,
    [float]$SharpenAmount = 1.5
)

Add-Type -AssemblyName System.Drawing

if (-not (Test-Path $InputPath)) {
    Write-Error "Input file not found: $InputPath"
    exit 1
}

$src = [System.Drawing.Bitmap]::FromFile($InputPath)
$newWidth = [int]($src.Width * $ScaleFactor)
$newHeight = [int]($src.Height * $ScaleFactor)
$scaled = New-Object System.Drawing.Bitmap $newWidth, $newHeight
$g = [System.Drawing.Graphics]::FromImage($scaled)
$g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
$g.DrawImage($src, 0, 0, $newWidth, $newHeight)
$g.Dispose()

function Gaussian-Blur([System.Drawing.Bitmap]$image, [int]$radius) {
    $blurred = New-Object System.Drawing.Bitmap $image.Width, $image.Height
    for ($y = 0; $y -lt $image.Height; $y++) {
        for ($x = 0; $x -lt $image.Width; $x++) {
            $r = $g = $b = 0
            $count = 0
            for ($ky = -$radius; $ky -le $radius; $ky++) {
                $ny = $y + $ky
                if ($ny -lt 0 -or $ny -ge $image.Height) { continue }
                for ($kx = -$radius; $kx -le $radius; $kx++) {
                    $nx = $x + $kx
                    if ($nx -lt 0 -or $nx -ge $image.Width) { continue }
                    $c = $image.GetPixel($nx, $ny)
                    $r += $c.R; $g += $c.G; $b += $c.B; $count++
                }
            }
            $r = [int]($r / $count)
            $g = [int]($g / $count)
            $b = [int]($b / $count)
            $blurred.SetPixel($x, $y, [System.Drawing.Color]::FromArgb($r, $g, $b))
        }
    }
    return $blurred
}

$blurred = Gaussian-Blur $scaled $BlurRadius
$sharpened = New-Object System.Drawing.Bitmap $scaled.Width, $scaled.Height
for ($y = 0; $y -lt $scaled.Height; $y++) {
    for ($x = 0; $x -lt $scaled.Width; $x++) {
        $orig = $scaled.GetPixel($x, $y)
        $blur = $blurred.GetPixel($x, $y)
        $r = [math]::Min(255, [math]::Max(0, [int]($orig.R + $SharpenAmount * ($orig.R - $blur.R))))
        $g = [math]::Min(255, [math]::Max(0, [int]($orig.G + $SharpenAmount * ($orig.G - $blur.G))))
        $b = [math]::Min(255, [math]::Max(0, [int]($orig.B + $SharpenAmount * ($orig.B - $blur.B))))
        $sharpened.SetPixel($x, $y, [System.Drawing.Color]::FromArgb($r, $g, $b))
    }
}

$sharpened.Save($OutputPath, [System.Drawing.Imaging.ImageFormat]::Png)
"Enhanced image saved to $OutputPath"
