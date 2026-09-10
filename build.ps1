param([string]$PreviewPath)

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$ico = Join-Path $root 'app.ico'
$exe = Join-Path $root 'Curfew.exe'

Add-Type -AssemblyName System.Drawing

$accent = [Drawing.Color]::FromArgb(124, 108, 255)

function Get-RoundRect([single]$x, [single]$y, [single]$w, [single]$h, [single]$rad) {
    $d = $rad * 2
    $p = New-Object Drawing.Drawing2D.GraphicsPath
    $p.AddArc($x, $y, $d, $d, 180, 90)
    $p.AddArc($x + $w - $d, $y, $d, $d, 270, 90)
    $p.AddArc($x + $w - $d, $y + $h - $d, $d, $d, 0, 90)
    $p.AddArc($x, $y + $h - $d, $d, $d, 90, 90)
    $p.CloseFigure()
    return $p
}

function New-Frame([int]$S) {
    $bmp = New-Object Drawing.Bitmap $S, $S
    $g = [Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = 'AntiAlias'
    $g.Clear([Drawing.Color]::Transparent)

    $m = [single][Math]::Round($S * 0.03)
    $side = [single]($S - 2 * $m)
    $path = Get-RoundRect $m $m $side $side ([single]($S * 0.22))
    $grad = New-Object Drawing.Drawing2D.LinearGradientBrush `
        (New-Object Drawing.PointF $m, $m), `
        (New-Object Drawing.PointF ($m + $side), ($m + $side)), `
        ([Drawing.Color]::FromArgb(140, 124, 255)), `
        ([Drawing.Color]::FromArgb(74, 56, 204))
    $g.FillPath($grad, $path)
    $grad.Dispose()
    $path.Dispose()

    if ($S -le 24) {
        $R = [single]($S * 0.40)
        $arcStart = [single]78.5
        $arcSweep = [single]203
        $ir = [single]($R * 1.24)
        $dx = [single]($R * 0.96)
        $inStart = [single]232.2
        $inSweep = [single](-104.4)
    }
    else {
        $R = [single]($S * 0.34)
        $arcStart = [single]85
        $arcSweep = [single]190
        $ir = [single]($R * 1.262)
        $dx = [single]($R * 0.862)
        $inStart = [single]232.1
        $inSweep = [single](-104.2)
    }
    $cx = [single]($S * 0.5)
    $cy = [single]($S * 0.5)
    $moon = New-Object Drawing.Drawing2D.GraphicsPath
    $moon.AddArc([single]($cx - $R), [single]($cy - $R), [single](2 * $R), [single](2 * $R), $arcStart, $arcSweep)
    $moon.AddArc([single]($cx + $dx - $ir), [single]($cy - $ir), [single](2 * $ir), [single](2 * $ir), $inStart, $inSweep)
    $moon.CloseFigure()
    $rot = New-Object Drawing.Drawing2D.Matrix
    $rot.RotateAt(-35, (New-Object Drawing.PointF $cx, $cy))
    $moon.Transform($rot)
    $rot.Dispose()
    $white = New-Object Drawing.SolidBrush ([Drawing.Color]::White)
    $g.FillPath($white, $moon)
    $white.Dispose()
    $moon.Dispose()

    $g.Dispose()
    return $bmp
}

$sizes = 16, 20, 24, 32, 48, 64, 128, 256
$pngs = @()
foreach ($s in $sizes) {
    $bmp = New-Frame $s
    $ms = New-Object IO.MemoryStream
    $bmp.Save($ms, [Drawing.Imaging.ImageFormat]::Png)
    $pngs += , $ms.ToArray()
    $bmp.Dispose()
}

$out = New-Object IO.MemoryStream
$out.Write([byte[]]@(0, 0, 1, 0), 0, 4)
$out.Write([BitConverter]::GetBytes([int16]$sizes.Count), 0, 2)
$offset = 6 + 16 * $sizes.Count
for ($i = 0; $i -lt $sizes.Count; $i++) {
    $d = if ($sizes[$i] -ge 256) { 0 } else { $sizes[$i] }
    $out.Write([byte[]]@($d, $d, 0, 0, 1, 0, 32, 0), 0, 8)
    $out.Write([BitConverter]::GetBytes([int]$pngs[$i].Length), 0, 4)
    $out.Write([BitConverter]::GetBytes([int]$offset), 0, 4)
    $offset += $pngs[$i].Length
}
foreach ($p in $pngs) { $out.Write($p, 0, $p.Length) }
[IO.File]::WriteAllBytes($ico, $out.ToArray())

if ($PreviewPath) {
    $show = 16, 20, 24, 32, 48
    $zoom = 6
    $sheet = New-Object Drawing.Bitmap 520, 400
    $sg = [Drawing.Graphics]::FromImage($sheet)
    $sg.InterpolationMode = 'NearestNeighbor'
    $sg.PixelOffsetMode = 'Half'
    $sg.Clear([Drawing.Color]::FromArgb(32, 32, 36))
    $sg.FillRectangle([Drawing.Brushes]::Gainsboro, 0, 200, 520, 200)
    $x = 12
    foreach ($s in $show) {
        $f = New-Frame $s
        $sg.DrawImage($f, $x, 40, $s * $zoom, $s * $zoom)
        $sg.DrawImage($f, $x, 240, $s * $zoom, $s * $zoom)
        $f.Dispose()
        $x += $s * $zoom + 14
    }
    $sg.Dispose()
    $sheet.Save($PreviewPath, [Drawing.Imaging.ImageFormat]::Png)
    $sheet.Dispose()
    "Preview $PreviewPath"
}

$csc = "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
$sources = Get-ChildItem -Path (Join-Path $root 'src') -Filter *.cs | Sort-Object Name | ForEach-Object { $_.FullName }
& $csc /nologo /target:winexe /optimize+ /win32icon:"$ico" `
    /r:System.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll `
    /out:"$exe" $sources
if ($LASTEXITCODE -eq 0) { "Built $exe" } else { "Build failed ($LASTEXITCODE)" }
