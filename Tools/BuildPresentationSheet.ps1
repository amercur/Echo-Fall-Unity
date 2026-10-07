# Contact sheet of unaltered Unity review captures; labels are review metadata.
Add-Type -AssemblyName System.Drawing
$captureDirectory = Join-Path $PSScriptRoot '../Docs/Validation/player-presentation'
$review = Get-Content (Join-Path $PSScriptRoot '../Docs/Validation/player-presentation-review.json') -Raw | ConvertFrom-Json
$names = @($review.data.result.result.captures | Where-Object { $_.name -notin @('movement-lab','wake','belfry','cistern','archive','procession') })
$sheet = New-Object System.Drawing.Bitmap(1440, (270 * [Math]::Ceiling($names.Count / 6)))
$graphics = [System.Drawing.Graphics]::FromImage($sheet)
$font = New-Object System.Drawing.Font('Arial', 11)
$graphics.Clear([System.Drawing.Color]::FromArgb(15, 23, 31))
try {
    for ($index = 0; $index -lt $names.Count; $index++) {
        $capture = $names[$index]
        $source = [System.Drawing.Image]::FromFile((Join-Path $captureDirectory ($capture.name + '.png')))
        try {
            $left = ($index % 6) * 240
            $top = [Math]::Floor($index / 6) * 270
            $graphics.DrawImage($source, [int]$left, [int]$top, 240, 240)
            $graphics.DrawString(($capture.name + ' / ' + $capture.frame), $font, [System.Drawing.Brushes]::White, [single]($left + 5), [single]($top + 246))
        } finally { $source.Dispose() }
    }
    $sheet.Save((Join-Path $captureDirectory 'pose-sheet.png'), [System.Drawing.Imaging.ImageFormat]::Png)
} finally { $graphics.Dispose(); $font.Dispose(); $sheet.Dispose() }
